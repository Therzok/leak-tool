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

    static EventPipeEventSource CreateEventSource(EventPipeSession session, StringBuilder sb)
    {
        var source = new EventPipeEventSource(session.EventStream);
        source.NeedLoadedDotNetRuntimes();

        // Process all events by default.
        source.Clr.All += (TraceEvent obj) =>
        {
            obj.ToXml(sb);
        };

        return source;
    }

    public async Task Stream(CancellationToken token)
    {
        // *shakes fist at dotnet internal diagnostics port API*
        var client = new DiagnosticsClient(_processId);

        var buffer = new StringBuilder();

        using var session = client.StartEventPipeSession(KnownEventPipeProviders.GarbageCollection);
        using var source = CreateEventSource(session, buffer);

        var processTask = Task.Run(() => source.SafeProcess(), token);
        var inputTask = Task.Run(() => Console.Console.WaitForUserInput("Running event pipe session, press Enter to exit..."), token);

        // Needed because we configured it to suspend.
        client.ResumeRuntime();

        var procs = source.Processes();

        await Task.WhenAny(processTask, inputTask);

        Console.MarkupLine("[dim]Started writing input[/]");

        File.WriteAllText("pipeoutput", buffer.ToString());

        Console.MarkupLine("[underline]Done writing output[/]");

        source.StopProcessing();
        await session.StopAsync(token);
    }
}

