using System;
using Spectre.Console;

namespace LeakTestsPrototype;

static class AnsiConsoleExtensions
{
    public static async Task WaitForUserInput(this IAnsiConsole console, string prompt)
    {
        await console
            .Status()
            .StartAsync(prompt, ctx => WaitForKeyPressed(ctx, ConsoleKey.Enter));
    }

    static async Task WaitForKeyPressed(StatusContext ctx, ConsoleKey key)
    {
        while (System.Console.ReadKey(true).Key != key)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}

