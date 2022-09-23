using System;
using Microsoft.Diagnostics.Tracing;

namespace LeakTestsPrototype;

static class TraceEventDispatcherExtensions
{
    public static bool SafeProcess(this TraceEventDispatcher dispatcher)
    {
        bool result = false;

        try
        {
            result = dispatcher.Process();
        }
        catch (Exception e)
        {
            Console.MarkupLine("[red]Error encountered while processing events[/]");
            Console.WriteException(e);
        }

        return result;
    }
}

