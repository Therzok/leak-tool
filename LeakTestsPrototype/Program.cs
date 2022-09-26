using System.Diagnostics;
using LeakTestsPrototype;

using var process = StartLeakProcess(out _);

using var cts = new CancellationTokenSource();

var processor = new EventProcessor(process.Id);
var exitTask = Task.Run(async () =>
{
    await Console.Console.WaitForUserInput("Running event pipe session, press Enter to exit...");
    cts.Cancel();
}, CancellationToken.None);

await processor.Stream(cts.Token);


process.Kill();
process.WaitForExit();

static string GetOutputTraceFile()
{
    var outputPath = Environment.CurrentDirectory;
    var outputTraceFile = Path.Combine(outputPath, "perfdump.{pid}.nettrace");

    return outputTraceFile;
}

Process StartLeakProcess(out string traceFilePath)
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

    var process = Process.Start(psi);

    Debug.Assert(process != null);

    traceFilePath = unresolvedTraceFilePath.Replace("{pid}", process.Id.ToString());

    // Wait for event pipe to be fully set up.
    Thread.Sleep(1000);

    return process;
}