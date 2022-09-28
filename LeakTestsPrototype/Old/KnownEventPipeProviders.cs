using System;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace LeakTestsPrototype;

static class KnownEventPipeProviders
{
    // "Microsoft-DotNETCore-SampleProfiler" - sampling cpu profiler
    // "System.Buffers.ArrayPoolEventSource" - array pool
    // "System.Net.Http" - http stack
    // "System.Threading.Tasks.TplEventSource" - threadpool

    // https://learn.microsoft.com/en-us/dotnet/fundamentals/diagnostics/runtime-garbage-collection-events#gcstart_v2-event
    const ClrTraceEventParser.Keywords gcEvents =
        ClrTraceEventParser.Keywords.GCHeapSnapshot |
        ClrTraceEventParser.Keywords.GCHeapSurvivalAndMovement;

    public static IEnumerable<EventPipeProvider> GarbageCollection { get; } = new[]
    {
        new EventPipeProvider("Microsoft-Windows-DotNETRuntime",
            EventLevel.Verbose, (long)gcEvents),
    };
}

