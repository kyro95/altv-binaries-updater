using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Module;

public class CoreclrModule : ICdnModule
{
    private readonly CdnInfo _cdnInfo;
    private readonly DownloaderService _downloaderService;

    public CoreclrModule(IOptions<CdnInfo> cdnInfo, DownloaderService downloaderService)
    {
        _cdnInfo = cdnInfo.Value;
        _downloaderService = downloaderService;
    }

    public List<string> GetFileList(UserPreferences userPreferences)
    {
        var basePath = GetBasePath(userPreferences);
        
        return new List<string>
        {
            $"{basePath}/modules/{(userPreferences.Os == "x64_win32" ? "csharp-module.dll" : "libcsharp-module.so")}",
            $"{basePath}/AltV.Net.Host.dll",
            $"{basePath}/AltV.Net.Host.runtimeconfig.json",
        };
    }

    public async Task Download(UserPreferences userPreferences, ProgressTask task)
    {
        BuildInformation? buildInformation = await _downloaderService
            .GetBuildInformation($"{GetBasePath(userPreferences)}/update.json");

        if (buildInformation == null)
        {
            throw new Exception("buildInformation not found");
        }

        var modulesPath = _downloaderService.GetModulesPath(userPreferences);
        var files = GetFileList(userPreferences);
        
        var moduleFilename = files[0];
        
        files.RemoveAt(0);

        await _downloaderService.DownloadFile(moduleFilename, modulesPath, buildInformation);
        task.Increment(100 / (double)files.Count + 1);
        
        await _downloaderService.DownloadFiles(files, buildInformation, userPreferences.Path, task);

        task.Increment(100);
    }

    public string GetBasePath(UserPreferences userPreferences)
    {
        return $"{_cdnInfo.BaseUri}coreclr-module/{userPreferences.Branch}/{userPreferences.Os}";
    }
}