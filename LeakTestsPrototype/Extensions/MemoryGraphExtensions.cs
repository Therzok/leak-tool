using System;
using Graphs;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LeakTestsPrototype.Extensions;

static class MemoryGraphExtensions
{
    public static IRenderable ToTable(this MemoryGraph graph)
    {
        var histogram = graph.GetHistogramByType();
        var storage = graph.AllocTypeNodeStorage();

        var table = new Table()
            .AddColumn("Module")
            .AddColumn("Name")
            .AddColumn("Count")
            .AddColumn("Size");

        table.AddRow("*", "All Objects", graph.NodeCount.ToString(), graph.TotalSize.ToString());
        table.AddEmptyRow();

        foreach (var sizeAndCount in histogram)
        {
            var nodeType = graph.GetType(sizeAndCount.TypeIdx, storage);
            table.AddRow(
                Markup.Escape(Path.GetFileName(nodeType.ModuleName) ?? ""),
                Markup.Escape(nodeType.Name),
                Markup.Escape(sizeAndCount.Count.ToString()),
                Markup.Escape(sizeAndCount.Size.ToString()));
        }

        return table;
    }
}

