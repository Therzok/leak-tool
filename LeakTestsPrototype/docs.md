# Implementation details

This implementation reuses most of dotnet gcdump for collecting the GC heap from the process
via the event pipe provider set up by gcdump. The heap dump collects a Memory Graph

 MemoryGraph, which contains the in-memory view of the object references. A RefGraph
can then be constructed for a given type, which contains the inverse references (retention graph).

The CLI tool first outputs the table of live objects at a given gc dump, then prints out the retention graph
for a given object.

One possible improvement would be to create an associated Type graph for the ref graph, which would
then be used to group references with the same paths to roots.

TODO: Dedup via transforming each retention graph to an associated type retention graph and counting.

# Supporting documentation:

https://devblogs.microsoft.com/dotnet/gc-etw-events-4/
https://github.com/dotnet/diagnostics/tree/main/src/Tools/dotnet-gcdump/DotNetHeapDump

## PerfView and TraceEvent

https://github.com/Microsoft/perfview/blob/main/documentation/TraceEvent/TraceEventLibrary.md
https://github.com/microsoft/perfview/blob/76dc28af873e27aa8c4f9ce8efa0971a2c738165/src/MemoryGraph/graph.cs
https://github.com/microsoft/perfview/blob/main/src/TraceEvent/Samples/20_ObserveGCEvent.cs

