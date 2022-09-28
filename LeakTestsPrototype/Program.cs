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

var graph = new MemoryGraph(50000);
DotNetHeapInfo heapInfo = new DotNetHeapInfo();

await Task.Delay(1000);

var dumpTask = Task.Run(async () =>
{
    await Console.Console.WaitForUserInput("Running event pipe session, press Enter to run GC Dump...");

    using var log = File.Open("gcdump.log", FileMode.Create);
    using var writer = new StreamWriter(log) { AutoFlush = true, };

    // This will wait until resumeruntime is called.
    bool result = EventPipeDotNetHeapDumper.DumpFromEventPipe(CancellationToken.None, process.Id, graph, writer, Timeout.Infinite, heapInfo);
    graph.AllowReading();

    return result;
});

new DiagnosticsClient(process.Id).ResumeRuntime();

await dumpTask;

process.Kill();
process.WaitForExit();

AnsiConsole.Write(graph.ToTable());

// graph.DumpNormalized can be used to output the GC graph xml

var refGraph = new RefGraph(graph);
var spanningTree = new SpanningTree(graph, TextWriter.Null);

var typeStorage = graph.AllocTypeNodeStorage();
var nodeStorage = graph.AllocNodeStorage();

var refStorage = refGraph.AllocNodeStorage();

PrintDetails("MyObject$");
PrintDetails("CycleObject$");
PrintDetails("CycleObjectNative$");

void PrintDetails(string typeName)
{
    Console.Write(new Rule(Markup.Escape(typeName)));

    var objects = graph.NodesOfType(typeName);
    if (objects.Count == 0)
    {
        return;
    }
    refGraph.PrintRetentionGraph(graph, new HashSet<NodeIndex>(), objects[0], 0, nodeStorage, typeStorage, refStorage);

    Console.WriteLine();
    Console.Write(new Rule("Path to root"));
    Console.WriteLine();

    //spanningTree.PrintNodes(objects, nodeStorage, typeStorage);
}

static string GetOutputTraceFile()
{
    var outputPath = Environment.CurrentDirectory;
    var outputTraceFile = Path.Combine(outputPath, "perfdump.{pid}.nettrace");

    return outputTraceFile;
}

static System.Diagnostics.Process StartLeakProcess(out string traceFilePath)
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