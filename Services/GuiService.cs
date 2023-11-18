using System.Net;
using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Module;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Services;

public class GuiService : ISingleton
{
    private readonly CdnInfo _cdnInfo;
    private readonly IServiceProvider _serviceProvider;

    public GuiService(IOptions<CdnInfo> cdnInfo, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cdnInfo = cdnInfo.Value;
    }
    
    public async Task Start()
    {
        AnsiConsole.MarkupLine("\n[bold]Welcome to [/][green4]alt:V[/][bold] Server Updater![/]");

        if (_cdnInfo.DefaultPath.Length > 0 && !Directory.Exists(_cdnInfo.DefaultPath))
            AnsiConsole.MarkupLine(
                "[bold][yellow2]Warning: Path submitted in config.json does not exist, using default path![/][/]");

        var installationPath = await GetInstallationPath();
        var branch = GetBranch();
        
        AnsiConsole.MarkupLine("\n[bold]You have selected the branch [/]'[green4]" + branch + "[/]'");

        var os = GetOs();

        AnsiConsole.MarkupLine("[bold]You have selected [/]'[green4]" + os + "[/]'");
        
        bool connectionToCdn = await AttemptConnectionToCdn();

        if (!connectionToCdn)
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: Failed to connect to CDN[/][/]");
            Console.ReadKey(true);
            return;
        }
        
        UserPreferences userPreferences = new(installationPath, branch, os);
        var modules = GetComponentsToDownload(branch);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var taskBinaries = ctx.AddTask("[green]Downloading binaries...[/]");

                foreach (var module in modules)
                {
                    var taskModule = ctx.AddTask($"[green]Downloading {module.GetType().Name}...[/]");

                    await module.Download(userPreferences, taskModule);
                    taskBinaries.Increment(100 / (double)modules.Count);
                }

                return Task.CompletedTask;
            });
        
        AnsiConsole.MarkupLine("[bold]Finished downloading binaries![/]");
        AnsiConsole.MarkupLine("[bold]You can now start your server![/]");
        AnsiConsole.MarkupLine(
            "[bold]If you have encountered any problem, feel free to open a issue on github [link]https://github.com/kyro95/altv-server-updater[/][/]");
        AnsiConsole.Markup("[bold]Press any key to exit...[/]");
        Console.ReadKey(true);
    }

    public GuiService CheckConfigIntegrity()
    {
        if ((CdnInfo?)_cdnInfo == null)
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: Error while reading the config.json file, please check it![/][/]");
            return this;
        }

        if (_cdnInfo.BaseUri != null) 
            return this;
        
        AnsiConsole.MarkupLine("[bold][red3]Error: The CDN base uri is not set in the config.json file, please set it![/][/]");
        
        return this;
    }
    
    private async Task<string> GetInstallationPath()
    {
        var currentPath = Directory
            .Exists(_cdnInfo.DefaultPath) ? _cdnInfo.DefaultPath : Directory.GetCurrentDirectory();
        
        var installationPath = AnsiConsole.Prompt(
            new TextPrompt<string>($"[bold]Enter installation path:[/]")
                .DefaultValue(currentPath));

        if (!Directory.Exists(installationPath))
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: The installation path does not exist![/][/]");
            
            await Start();
        }
        
        return installationPath;
    }

    private string GetBranch()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Please submit the [green4]branch[/]:[/]")
                .AddChoices("release", "dev", "rc")
        );
    }
    
    private string GetOs()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Your current os is [green4]" + System.Runtime.InteropServices.RuntimeInformation
                                                               .OSDescription
                                                           + "[/], select the right binaries for your os:[/]")
                .AddChoices("windows", "linux")
        );
        
        return choice == "windows" ? "x64_win32" : "x64_linux";
    }

    private List<ICdnModule> GetComponentsToDownload(string branch)
    {
        var multiSelect = new MultiSelectionPrompt<string>()
            .Title("[bold]Select the [green4]binaries[/] you want to download:[/]")
            .PageSize(20)
            .InstructionsText(
                "[grey](Press [green4]<space>[/] to toggle a option " +
                "[green4]<enter>[/] to accept)[/]")
            .AddChoices("server", "data", "voice", "csharp-module", "js-module");

        if (branch is not "dev")
        {
            multiSelect.AddChoice("js-bytecode-module");
        }
        
        var choices = AnsiConsole.Prompt(multiSelect);
        
        AnsiConsole.MarkupLine(
            $"[bold]You have selected the following binaries:[/] [green4]{string.Join(", ", choices)}[/]");
        
        List<ICdnModule> componentsToDownload = new();
        
        foreach (var choice in choices)
        {
            ICdnModule? module = choice switch
            {
                "server" => _serviceProvider.GetRequiredService<ServerModule>(),
                "voice" => _serviceProvider.GetRequiredService<VoiceModule>(),
                "csharp-module" => _serviceProvider.GetRequiredService<CoreclrModule>(),
                "js-module" => _serviceProvider.GetRequiredService<JsModule>(),
                "js-bytecode-module" => _serviceProvider.GetRequiredService<BytecodeModule>(),
                "data" => _serviceProvider.GetRequiredService<DataModule>(),
                _ => null
            };

            if (module is null)
            {
                AnsiConsole.MarkupLine($"[bold][red3]Error: {choice} is not a valid option![/][/]");
                continue;
            }
            
            componentsToDownload.Add(module);
        }
        
        return componentsToDownload;
    }

    private async Task<bool> AttemptConnectionToCdn()
    {
        try
        {
            await AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[bold]Testing CDN connection... (base uri: " + _cdnInfo.BaseUri + ") [/]", async ctx =>
                {
                    Task.Delay(1000).Wait();
                
                    var client = new HttpClient();
                    var response = await client.GetAsync(_cdnInfo.BaseUri + "data/release/update.json");
                    
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    
                    AnsiConsole.MarkupLine($"[bold][red3]Error: Status code from CDN: {response.StatusCode}[/][/]");
                    ctx.Status("[bold][red3]Error: Status code from CDN: " + response.StatusCode + "[/][/]");
                    Console.ReadKey(true);
                        
                    return false;
                });
        } catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[bold][red3]Error: {e.Message}[/][/]");
            return false;
        }
        
        return true;
    }
}