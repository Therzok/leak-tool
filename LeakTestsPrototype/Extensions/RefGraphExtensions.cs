using System;
using Graphs;

namespace LeakTestsPrototype.Extensions;

public static class RefGraphExtensions
{
    public static void PrintRetentionGraph(this RefGraph refGraph, Graph graph, HashSet<NodeIndex> visited, NodeIndex index, int depth, Node nodeStorage, NodeType typeStorage, RefNode refStorage)
    {
        Console.Write(new string(' ', depth));

        if (!visited.Add(index))
        {
            Console.MarkupLineInterpolated($"[dim]->[/] [underline]Cycle[/] [dim]{index}[/]");
            return;
        }

        var node = graph.GetNode(index, nodeStorage);
        var type = graph.GetType(node.TypeIndex, typeStorage);
        Console.MarkupLineInterpolated($"[dim]->[/] {type.Name} [dim]{index}[/]");

        var refNode = refGraph.GetNode(index, refStorage);
        for (var child = refNode.GetFirstChildIndex();
            child != NodeIndex.Invalid;
            child = refNode.GetNextChildIndex())
        {
            PrintRetentionGraph(refGraph, graph, visited, child, depth + 2, nodeStorage, typeStorage, refStorage);
        }
    }
}

