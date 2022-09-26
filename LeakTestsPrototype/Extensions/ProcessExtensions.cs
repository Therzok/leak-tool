using System;
using System.Diagnostics;

namespace LeakTestsPrototype;

static class ProcessExtensions
{
    public static ProcessStartInfo ConfigureSuspendRuntime(this ProcessStartInfo psi)
    {
        // Suspend the runtime, so we don't lose events
        psi.Environment.Add("DOTNET_DefaultDiagnosticPortSuspend", "1");

        return psi;
    }

    public static ProcessStartInfo ConfigureEventPipe(this ProcessStartInfo psi, string outputTraceFile)
    {
        // Enable writing event pipe to file
        psi.Environment.Add("DOTNET_EnableEventPipe", "1");

        // Set the output path.
        psi.Environment.Add("DOTNET_EventPipeOutputPath", outputTraceFile);

        // Flush the event pipe to disk immediately.
        psi.Environment.Add("DOTNET_EventPipeOutputStreaming", "1");

        return psi;
    }
}

