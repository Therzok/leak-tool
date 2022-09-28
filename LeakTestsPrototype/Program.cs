using System;
using System.Diagnostics;
using System.Xml.Linq;
using Graphs;
using LeakTestsPrototype;
using LeakTestsPrototype.Extensions;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tools.GCDump;
using Microsoft.Diagnostics.Tracing;
using Spectre.Console;

using var process = StartLeakProcess(out _);

#if V1
using var cts = new CancellationTokenSource();
var dumpTask = Task.Run(async () =>
{
    await Console.Console.WaitForUserInput("Running event pipe session, press Enter to stop processing...");

    cts.Cancel();
}, CancellationToken.None);

var processor = new EventProcessor(process.Id);
var graph = await processor.Stream(cts.Token);
#else
var graph = new MemoryGraph(50000);
DotNetHeapInfo heapInfo = new DotNetHeapInfo();

using var log = File.Open("gcdump.log", FileMode.Create);
using var writer = new StreamWriter(log) { AutoFlush = true, };

await Task.Delay(1000);

var dumpTask = Task.Run(async () =>
{
    await Console.Console.WaitForUserInput("Running event pipe session, press Enter to run GC Dump...");

     //This will wait until resumeruntime is called.
    bool result = EventPipeDotNetHeapDumper.DumpFromEventPipe(CancellationToken.None, process.Id, graph, writer, Timeout.Infinite, heapInfo);
    graph.AllowReading();

    return result;
});

new DiagnosticsClient(process.Id).ResumeRuntime();

Debug.Assert(graph != null);

#endif

await dumpTask;

process.Kill();
process.WaitForExit();

AnsiConsole.Write(graph.ToTable());

// graph.DumpNormalized can be used to output the GC graph xml

#if !V1
var objects = graph.NodesOfType("MyObject");

var refGraph = new RefGraph(graph);

var spanningTree = new SpanningTree(graph, TextWriter.Null);

var refStorage = refGraph.AllocNodeStorage();

var typeStorage = graph.AllocTypeNodeStorage();
var nodeStorage = graph.AllocNodeStorage();

static void PrintNode(Graph graph, RefGraph refGraph, NodeIndex index, int depth, Node nodeStorage, NodeType typeStorage, RefNode refStorage)
{
    var node = graph.GetNode(index, nodeStorage);
    var type = graph.GetType(node.TypeIndex, typeStorage);

    Console.Write(new string(' ', depth));
    Console.MarkupLineInterpolated($"[dim]->[/] {type.Name}");

    var refNode = refGraph.GetNode(index, refStorage);
    for (var child = refNode.GetFirstChildIndex();
        child != NodeIndex.Invalid;
        child = refNode.GetNextChildIndex())
    {
        PrintNode(graph, refGraph, child, depth + 2, nodeStorage, typeStorage, refStorage);
    }
}

PrintNode(graph, refGraph, objects[0], 0, nodeStorage, typeStorage, refStorage);

Console.WriteLine();
Console.Write(new Rule("Path to root"));
Console.WriteLine();

bool doneOnce = false;
spanningTree.ForEach(index =>
{
    if (doneOnce || !objects.Contains(index))
    {
        return;
    }

    while (index != NodeIndex.Invalid)
    {
        doneOnce = true;

        // This should get us a path to a root.
        var node = graph.GetNode(index, nodeStorage);
        var type = graph.GetType(node.TypeIndex, typeStorage);

        Console.WriteLine(type.Name);

        index = spanningTree.Parent(index);

        Console.Markup("[dim] -> [/]");
    }

    Console.WriteLine();
});

#endif

static string GetOutputTraceFile()
{
    var outputPath = Environment.CurrentDirectory;
    var outputTraceFile = Path.Combine(outputPath, "perfdump.{pid}.nettrace");

    return outputTraceFile;
}

System.Diagnostics.Process StartLeakProcess(out string traceFilePath)
{
    string path = Path.Combine(
        Path.GetDirectoryName(typeof(Program).Assembly.Location)!,
        "..",
        "..",
        "..",
        "..",
        "XamMacLeaks",
        "bin",
        "Debug",
        "net7.0-macos",
        "osx-x64",
        "XamMacLeaks.app",
        "Contents",
        "MacOS",
        "XamMacLeaks"
    );

    var unresolvedTraceFilePath = GetOutputTraceFile();

    var psi = new ProcessStartInfo(path)
        .ConfigureEventPipe(unresolvedTraceFilePath)
        .ConfigureSuspendRuntime();

    var process = System.Diagnostics.Process.Start(psi);

    Debug.Assert(process != null);

    traceFilePath = unresolvedTraceFilePath.Replace("{pid}", process.Id.ToString());

    // Wait for event pipe to be fully set up.
    Thread.Sleep(1000);

    return process;
}