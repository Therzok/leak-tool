using System.Diagnostics;
using LeakTestsPrototype;

using var process = StartLeakProcess();

var processor = new EventProcessor(process.Id);
await processor.Stream(CancellationToken.None);

var resolvedOutputTraceFile = GetOutputTraceFile().Replace("{pid}", process.Id.ToString());
//await processor.Process(File.OpenRead(resolvedOutputTraceFile));

process.Kill();
process.WaitForExit();

static string GetOutputTraceFile()
{
    var outputPath = Environment.CurrentDirectory;
    var outputTraceFile = Path.Combine(outputPath, "perfdump.{pid}.nettrace");

    return outputTraceFile;
}

Process StartLeakProcess()
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

    var psi = new ProcessStartInfo(path).ConfigureEventPipe(GetOutputTraceFile());
    var process = Process.Start(psi);

    Debug.Assert(process != null);

    // Wait for event pipe to be fully set up.
    Thread.Sleep(1000);

    return process;
}