using System.Diagnostics;
using System.Reflection;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace AltV.Binaries.Updater;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty)
            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
            .Build();

        IServiceCollection serviceCollection = new ServiceCollection()
            .Configure<CdnInfo>(config.GetSection("CdnInfo"))
            .AddSingleton(sp => sp)
            .Scan(scan => scan
                .FromAssemblyOf<Program>()
                .AddClasses(classes => classes.AssignableTo<ISingleton>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime()
            );
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        bool launchGui = ParseCommandLine(args);
        
        if(!launchGui)
            return;
        
        var guiService = serviceProvider.GetRequiredService<GuiService>();
        
        await guiService
            .CheckConfigIntegrity()
            .Start();
    }

    public static bool ParseCommandLine(string[] args)
    {
        if (args.Length == 0)
            return true;

        if (args.Length == 1 && args[0] == "--help")
        {
            AnsiConsole.MarkupLine("[bold]Commands:[/]");
            AnsiConsole.MarkupLine("[bold]--client[/] - <launch - branch - dev> - Launch the client updater");
            return false;
        }
        
        if (args.Length == 1 && args[0] == "--client")
        {
            AnsiConsole.MarkupLine("[bold]Commands:[/]");
            AnsiConsole.MarkupLine("[bold]launch[/] - Launch the client updater");
            AnsiConsole.MarkupLine("[bold]branch[/] - <release - dev - rc> - Select the branch to download");
            AnsiConsole.MarkupLine("[bold]dev[/] - <true - false> - Enable or disable dev branch");
            return false;
        }

        /*if (args.Length == 1 && args[0] == "--download")
        {
            AnsiConsole.MarkupLine("[bold]Commands:[/]");
            AnsiConsole.MarkupLine("[bold]server[/] - Download the server binaries");
            AnsiConsole.MarkupLine("[bold]voice[/] - Download the voice binaries");
            AnsiConsole.MarkupLine("[bold]csharp-module[/] - Download the csharp-module binaries");
            AnsiConsole.MarkupLine("[bold]js-module[/] - Download the js-module binaries");
            AnsiConsole.MarkupLine("[bold]js-bytecode-module[/] - Download the js-bytecode-module binaries");
            
            AnsiConsole.MarkupLine("\n[bold]Example:[/] [green4]altv-server-updater --download server, csharp-module[/]");
            return false;
        }*/

        switch (args[0])
        {
            case "--version":
                AnsiConsole.MarkupLine($"[bold]alt:V Server Updater version: [/]'[green4]{Assembly.GetExecutingAssembly().GetName().Version}[/]'");
                return false;
            case "--client":
                var altvPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "altv");
                var altvTomlPath = Path.Combine(altvPath, "altv.toml");
                
                switch (args.Length)
                {
                    case 2 when args[1] == "launch":
                    {
                        var altvExePath = Path.Combine(altvPath, "altv.exe");
                        
                        if(!File.Exists(altvExePath))
                            AnsiConsole.MarkupLine("[bold][red3]Error: altv.exe not found![/][/]");
                        else
                            Process.Start(altvExePath);
                        return false;
                    }
                    case 3 when args[1] == "branch":
                        if(!File.Exists(altvTomlPath))
                            AnsiConsole.MarkupLine("[bold][red3]Error: altv.toml not found![/][/]");
                        else
                        {
                            var lines = File.ReadAllLines(altvTomlPath);
                            var branch = args[2];
                            var dev = branch == "dev" ? "true" : "false";
                            
                            for (var i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].Contains("branch"))
                                {
                                    lines[i] = $"branch = \"{branch}\"";
                                }
                                else if (lines[i].Contains("dev"))
                                {
                                    lines[i] = $"dev = {dev}";
                                }
                            }
                            
                            File.WriteAllLines(altvTomlPath, lines);
                            AnsiConsole.MarkupLine($"[bold]Branch set to [/]'[green4]{branch}[/]'[bold] in altv.toml![/]");
                        }
                        return false;
                    case 3 when args[1] == "dev":
                        if (args[2] == "true" || args[2] == "false")
                        {
                            if(!File.Exists(altvTomlPath))
                                AnsiConsole.MarkupLine("[bold][red3]Error: altv.toml not found![/][/]");
                            else
                            {
                                var lines = File.ReadAllLines(altvTomlPath);
                                var dev = args[2];
                                
                                for (var i = 0; i < lines.Length; i++)
                                {
                                    if (lines[i].Contains("dev"))
                                    {
                                        lines[i] = $"dev = {dev}";
                                    }
                                }
                            
                                File.WriteAllLines(altvTomlPath, lines);
                                AnsiConsole.MarkupLine($"[bold]Dev set to [/]'[green4]{dev}[/]'[bold] in altv.toml![/]");
                            }
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[bold][red3]Error: Invalid command line arguments![/][/]");
                            return false;
                        }
                        return false;
                }

                break;
            case "--gui":
                return true;
        }

        if (args.Length == 1 && args[0] == "--gui")
            return true;

        AnsiConsole.MarkupLine("[bold][red3]Error: Invalid command line arguments![/][/]");
        return false;
    }
}