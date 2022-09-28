# Implementation details

This implementation reuses most of dotnet gcdump for collecting the GC heap from the process
via the event pipe provider set up by gcdump.

Then a RefGraph is created for objects of a given type and traversed and printed.

TODO: Dedup

# Supporting documentation:

https://devblogs.microsoft.com/dotnet/gc-etw-events-4/
https://github.com/dotnet/diagnostics/tree/main/src/Tools/dotnet-gcdump/DotNetHeapDump

https://github.com/microsoft/perfview
https://github.com/microsoft/perfview/blob/76dc28af873e27aa8c4f9ce8efa0971a2c738165/src/MemoryGraph/graph.cs
https://github.com/microsoft/perfview/blob/main/src/TraceEvent/Samples/20_ObserveGCEvent.cs

