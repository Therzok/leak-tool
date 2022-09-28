using System;
using Graphs;
using System.Linq;

namespace LeakTestsPrototype.Extensions;

static class SpanningTreeExtensions
{
    public static void PrintNodes(this SpanningTree spanningTree, List<NodeIndex> nodesToPrint, Node nodeStorage, NodeType typeStorage)
    {
        bool doneOnce = false;
        var graph = spanningTree.Graph;

        spanningTree.ForEach(index =>
        {
            if (doneOnce || !nodesToPrint.Contains(index))
            {
                return;
            }

            while (index != NodeIndex.Invalid)
            {
                doneOnce = true;

                // This should get us a path to a root.
                var node = graph.GetNode(index, nodeStorage);
                var type = graph.GetType(node.TypeIndex, typeStorage);

                Console.WriteLine(type.Name);

                index = spanningTree.Parent(index);

                Console.Markup("[dim] -> [/]");
            }

            Console.WriteLine();
        });
    }
}

