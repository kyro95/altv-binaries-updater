using System.Net;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace AltV.Binaries.Updater;

public class CDNInfo {
    public string? BaseUri { get; set; }
    public string DefaultPath { get; set; } = "";
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var cdnInfo = GetCdnInfo();
        
        if (cdnInfo == null)
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: Error while reading the config.json file, please check it![/][/]");
            return;
        }
        
        if (cdnInfo.BaseUri == null)
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: The CDN base uri is not set in the config.json file, please set it![/][/]");
            return;
        }

        AnsiConsole.MarkupLine("\n[bold]Welcome to [/][green4]alt:V[/][bold] Server Updater![/]");
        
        var currentPath = Directory.Exists(cdnInfo.DefaultPath) ? cdnInfo.DefaultPath : Directory.GetCurrentDirectory();
        
        if(cdnInfo.DefaultPath.Length > 0 && !Directory.Exists(cdnInfo.DefaultPath))
            AnsiConsole.MarkupLine("[bold][yellow2]Warning: Path submitted in config.json does not exist, using default path![/][/]");

        var installationPath = AnsiConsole.Prompt(
            new TextPrompt<string>($"[bold]Enter installation path:[/]")
                .DefaultValue(currentPath));

        if (!Directory.Exists(installationPath))
        {
            AnsiConsole.MarkupLine("[bold][red3]Error: The installation path does not exist![/][/]");
            
            await Main(args);
            return;
        }
        
        var branch = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Please submit the [green4]branch[/]:[/]")
                .AddChoices("release", "dev", "rc")
        );
        
        AnsiConsole.MarkupLine("\n[bold]You have selected the branch [/]'[green4]" + branch + "[/]'");
        
        var os = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Your current os is [green4]" + System.Runtime.InteropServices.RuntimeInformation.OSDescription 
                                                           + "[/], select the right binaries for your os:[/]")
                .AddChoices("windows", "linux")
        );
        
        AnsiConsole.MarkupLine("[bold]You have selected [/]'[green4]" + os + "[/]'");

        var multiSelect = new MultiSelectionPrompt<string>()
            .Title("[bold]Select the [green4]binaries[/] you want to download:[/]")
            .PageSize(20)
            .InstructionsText(
                "[grey](Press [green4]<space>[/] to toggle a option " +
                "[green4]<enter>[/] to accept)[/]")
            .AddChoices("server", "voice", "csharp-module", "js-module");

        if (branch is not "dev")
        {
            multiSelect.AddChoice("js-bytecode-module");
        }
        
        var binariesToDownload = AnsiConsole.Prompt(multiSelect);
        AnsiConsole.MarkupLine($"[bold]You have selected the following binaries:[/] [green4]{string.Join(", ", binariesToDownload)}[/]");

        try
        {
            await AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[bold]Testing CDN connection... (base uri: " + cdnInfo.BaseUri + ") [/]", async ctx =>
                {
                    Task.Delay(1000).Wait();
                
                    var client = new HttpClient();
                    var response = await client.GetAsync(cdnInfo.BaseUri + "data/release/update.json");

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        AnsiConsole.MarkupLine($"[bold][red3]Error: Status code from CDN: {response.StatusCode}[/][/]");
                        ctx.Status("[bold][red3]Error: Status code from CDN: " + response.StatusCode + "[/][/]");
                        Console.ReadKey(true);
                    }
                });
        } catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[bold][red3]Error: {e.Message}[/][/]");
            return;
        }

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var taskBinaries = ctx.AddTask("[green]Downloading binaries...[/]");
                var osPath = os == "windows" ? "x64_win32" : "x64_linux";
                
                List<string> serverFiles = new()
                {
                    $"{cdnInfo.BaseUri}data/{branch}/update.json",
                    $"{cdnInfo.BaseUri}server/{branch}/{osPath}/altv-server{GetOsFileExtension(os)}",
                    $"{cdnInfo.BaseUri}server/{branch}/{osPath}/altv-crash-handler{GetOsFileExtension(os)}",
                };

                List<string> dataFiles = new()
                {
                    $"{cdnInfo.BaseUri}data/{branch}/data/vehmodels.bin",
                    $"{cdnInfo.BaseUri}data/{branch}/data/vehmods.bin",
                    $"{cdnInfo.BaseUri}data/{branch}/data/clothes.bin",
                    $"{cdnInfo.BaseUri}data/{branch}/data/pedmodels.bin"
                };

                List<string> dotnetFiles = new()
                {
                    $"{cdnInfo.BaseUri}coreclr-module/{branch}/{osPath}/AltV.Net.Host.dll",
                    $"{cdnInfo.BaseUri}coreclr-module/{branch}/{osPath}/AltV.Net.Host.runtimeconfig.json",
                };

                List<string> voiceFiles = new()
                {
                    $"{cdnInfo.BaseUri}voice-server/{branch}/{osPath}/altv-voice-server{GetOsFileExtension(os)}"
                };
                
                List<string> jsFiles = new()
                {
                    $"{cdnInfo.BaseUri}js-module/{branch}/{osPath}/modules/js-module/{(os == "windows" ? "js-module.dll" : "libjs-module.so")}",
                    $"{cdnInfo.BaseUri}js-module/{branch}/{osPath}/modules/js-module/{(os == "windows" ? "libnode.dll" : "libnode.so.108")}"
                };
                
                List<string> jsBytecodeFiles = new()
                {
                    $"{cdnInfo.BaseUri}js-bytecode-module/{branch}/{osPath}/modules/{(os == "windows" ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}",
                };
                
                foreach (var binary in binariesToDownload)
                {
                    switch (binary)
                    {
                        case "server":
                            var task = ctx.AddTask($"[green]Downloading {binary}...[/]");

                            await DownloadFiles(serverFiles, installationPath, task, taskBinaries, dataFiles.Count);

                            var dataPath = Path.Combine(installationPath, "data");

                            if (!Directory.Exists(dataPath))
                                Directory.CreateDirectory(dataPath);

                            await DownloadFiles(dataFiles, dataPath, task, taskBinaries, serverFiles.Count);
                            
                            break;
                        case "voice":
                            var taskVoice = ctx.AddTask($"[green]Downloading {binary}...[/]");
                            
                            await DownloadFiles(voiceFiles, installationPath, taskVoice, taskBinaries);
                            break;
                        case "csharp-module":
                            var taskDotnet = ctx.AddTask($"[green]Downloading {binary}...[/]");
                            var modulesPath = SeedModulesDirectory(installationPath);
                            
                            var moduleFilename = $"{cdnInfo.BaseUri}coreclr-module/{branch}/{osPath}/modules/{(os == "windows" ? "csharp-module.dll" : "libcsharp-module.so")}";
                            await DownloadFileAsync(moduleFilename, modulesPath);
                            taskDotnet.Increment(100 / dotnetFiles.Count + 1);

                            await DownloadFiles(dotnetFiles, installationPath, taskDotnet, taskBinaries, 1);
                            
                            break;
                        case "js-module":
                            var taskJs = ctx.AddTask($"[green]Downloading {binary}...[/]");
                            var jsModulesPath = Path.Combine(SeedModulesDirectory(installationPath), "js-module");
                            
                            if(!Directory.Exists(jsModulesPath))
                                Directory.CreateDirectory(jsModulesPath);
                            
                            await DownloadFiles(jsFiles, jsModulesPath, taskJs, taskBinaries);
                            
                            break;
                        case "js-bytecode-module":
                            var taskJsBytecode = ctx.AddTask($"[green]Downloading {binary}...[/]");
                            var jsBytecodeModulesPath = SeedModulesDirectory(installationPath);
                            
                            await DownloadFiles(jsBytecodeFiles, jsBytecodeModulesPath, taskJsBytecode, taskBinaries);
                            break;
                    }
                    
                    taskBinaries.Increment(100 / (double)binariesToDownload.Count);
                }
            });
        
        AnsiConsole.MarkupLine("[bold]Finished downloading binaries![/]");
        AnsiConsole.MarkupLine("[bold]You can now start your server![/]");
        AnsiConsole.MarkupLine("[bold]If you have encountered any problem, feel free to open a issue on github [link]https://github.com/kyro95/altv-server-updater[/][/]");
        AnsiConsole.Markup("[bold]Press any key to exit...[/]");
        Console.ReadKey(true);
    }
    
    private static string GetOsFileExtension(string os)
    {
        return os == "windows" ? ".exe" : "";
    }

    private static async Task DownloadFiles(List<string> files, string path, ProgressTask task, ProgressTask mainTask, int countBuffer = 0) 
    {
        foreach (var file in files)
        {
            await DownloadFileAsync(file, path);
            task.Increment(100 / (double)files.Count + countBuffer);
            
            
        }
    }

    private static string SeedModulesDirectory(string installationPath)
    {
        var modulesPath = Path.Combine(installationPath, "modules");
                            
        if(!Directory.Exists(Path.Combine(installationPath, "modules")))
            Directory.CreateDirectory(Path.Combine(installationPath, "modules"));
        
        return modulesPath;
    }

    static async Task DownloadFileAsync(string fileUrl, string path)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(fileUrl);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                AnsiConsole.MarkupLine($"[bold][red3]Error: Status code from CDN: {response.StatusCode}[/][/]");
                return;
            }

            var filePath = Path.Combine(path, Path.GetFileName(fileUrl));
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
            var fileBytes = await client.GetByteArrayAsync(fileUrl);
            await File.WriteAllBytesAsync(filePath, fileBytes);
        }
    }

    public static CDNInfo? GetCdnInfo()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
            .Build();
        
        var section = config.GetSection("CDNInfo");
        
        return section.Get<CDNInfo>();
    }
}