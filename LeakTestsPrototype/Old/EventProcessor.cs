#if V1
using System;
using Microsoft.Diagnostics.NETCore.Client;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;
using System.Text;

namespace LeakTestsPrototype;

sealed class EventProcessor
{
    readonly int _processId;

    public EventProcessor(int processId)
    {
        _processId = processId;
    }

    static EventPipeEventSource CreateEventSource(EventPipeSession session)
    {
        var source = new EventPipeEventSource(session.EventStream);

        source.NeedLoadedDotNetRuntimes();

        return source;
    }

    // TODO: Split this, we want some flexibility.
    public async Task<Graphs.MemoryGraph?> Stream(CancellationToken token)
    {
        // *shakes fist at dotnet internal diagnostics port API*
        var client = new DiagnosticsClient(_processId);

        var buffer = new StringBuilder();

        using var session = client.StartEventPipeSession(KnownEventPipeProviders.GarbageCollection);
        using var source = CreateEventSource(session);

        // Process all events by default.
        source.Clr.All += (TraceEvent obj) =>
        {
            obj.ToXml(buffer);
        };

        var exitTask = new TaskCompletionSource();

        using var _ = token.Register(() => {
            session.Stop();
            source.StopProcessing();

            exitTask.SetResult();
        });

        using var logFile = File.Open($"perfview.{_processId}.log", FileMode.Create);
        using var streamWriter = new StreamWriter(logFile)
        {
            AutoFlush = true,
        };

        var processTask = Task.Run(() => source.SafeProcess(streamWriter, _processId), token);

        // Needed because we configured it to suspend.
        client.ResumeRuntime();

        var procs = source.Processes();

        try
        {
            await Task.WhenAll(exitTask.Task, processTask);

            Console.MarkupLine("[dim]Started writing output[/]");

            File.WriteAllText("pipeoutput", buffer.ToString());

            Console.MarkupLine("[underline]Done writing output[/]");

            return await processTask;
        }
        finally
        {
            // Just wait for it to end.
            await session.StopAsync(CancellationToken.None);
        }
    }
}

#endif