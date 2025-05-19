using System;
using Microsoft.Extensions.DependencyInjection;
using JANORM.Client.utils;
using System.CommandLine;
using JANORM.Client.services;
using JANORM.Client.services.Implementation;
using JANORM.Core.services;
using JANORM.Core.services.Implementation;
using DotNetEnv;

namespace JANORM.Client;

public class Program
{   
    public static async Task<int> Main(string[] args) 
    {
        
        
        Env.Load();

        // Bloco para injetão de dependências
        ServiceCollection services = new();
        services.AddTransient<IInspectorService, InspectorService>();
        services.AddSingleton<IDBFactory, SqliteConnectionFactory>();
        ServiceProvider provider = services.BuildServiceProvider();

        RootCommand rootCommand = new("JANORM - A simple postgres ORM for .NET");

        Option<string> asmOption = new(name: "--project-assembly", description: "Path to the project assembly") {
            IsRequired = true,
            ArgumentHelpName = "path"
        };

        Command initCommand = new("init", "Creates JANORM/schema.jan file");
        Command generateCommand = new("generate", "Adds entities to the schema.jan file and generates the database"){
            asmOption
        };

        Command pushCommand = new("push", "Pushes the schema.jan file to the database");

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

        generateCommand.SetHandler(projectAsm =>
        {
            try
            {
                Janx.Generate(provider, projectAsm);
                Console.WriteLine("Entities added to schema successfully.");
            }
            catch (Exception ex) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during command execution: {ex.Message}");
                Console.ResetColor();
            }

           
        }, asmOption);


        pushCommand.SetHandler(async () => 
        {
            try
            {
                await Janx.Push();
                Console.WriteLine("Schema pushed to the database successfully.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during command execution: {ex.Message}");
                Console.ResetColor();
            }
        });

        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(generateCommand);
        rootCommand.AddCommand(pushCommand);
        return await rootCommand.InvokeAsync(args);
    }
}
