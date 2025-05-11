using System;
using JANORM.Client.utils;
using System.CommandLine;

namespace JANORM.Client;


public class Program
{   
    public static async Task<int> Main(string[] args) 
    {
        var rootCommand = new RootCommand("JANORM - A simple postgres ORM for .NET");

        var initCommand = new Command("init", "Creates JANORM/schema.jan file");

        initCommand.SetHandler(() => 
        {
            try
            {
                Janx.Init();
                Console.WriteLine("schema.jan file created successfully.");
            }
            catch (Exception ex)
            {   
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during command execution: {ex.Message}");
                Console.ResetColor();
            }
        });

        rootCommand.AddCommand(initCommand);
        return await rootCommand.InvokeAsync(args);
    }
}
