using System;
using Graphs;

namespace LeakTestsPrototype.Extensions;

public static class RefGraphExtensions
{
    public static void PrintRetentionGraph(this RefGraph refGraph, Graph graph, HashSet<NodeIndex> visited, NodeIndex index, int depth, Node nodeStorage, NodeType typeStorage, RefNode refStorage)
    {
        if (!visited.Add(index))
        {
            return;
        }

        var node = graph.GetNode(index, nodeStorage);
        var type = graph.GetType(node.TypeIndex, typeStorage);

        Console.Write(new string(' ', depth));
        Console.MarkupLineInterpolated($"[dim]->[/] {type.Name}");

        var refNode = refGraph.GetNode(index, refStorage);
        for (var child = refNode.GetFirstChildIndex();
            child != NodeIndex.Invalid;
            child = refNode.GetNextChildIndex())
        {
            PrintRetentionGraph(refGraph, graph, visited, child, depth + 2, nodeStorage, typeStorage, refStorage);
        }
    }
}

