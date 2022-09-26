using System;
using Spectre.Console;

namespace LeakTestsPrototype;

static class AnsiConsoleExtensions
{
    public static async Task WaitForUserInput(this IAnsiConsole console, string prompt)
    {
        await console.Status().StartAsync(prompt, async ctx =>
        {
            while (System.Console.ReadKey().Key != ConsoleKey.Enter)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        });
    }
}

