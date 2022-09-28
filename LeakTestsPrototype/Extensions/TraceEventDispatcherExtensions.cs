using System;
using Graphs;
using Microsoft.Diagnostics.Tools.GCDump;
using Microsoft.Diagnostics.Tracing;

namespace LeakTestsPrototype;

#if V1
static class TraceEventDispatcherExtensions
{
    public static MemoryGraph? SafeProcess(this TraceEventDispatcher dispatcher, TextWriter log, int processId)
    {
        MemoryGraph? result = null;

        try
        {
            var graphReader = new DotNetHeapDumpGraphReader(log);
            result = graphReader.Read(dispatcher);
        }
        catch (Exception e)
        {
            Console.MarkupLine("[red]Error encountered while processing events[/]");
            Console.WriteException(e);
        }

        return result;
    }
}
#endif

