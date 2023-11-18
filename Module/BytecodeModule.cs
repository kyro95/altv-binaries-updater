using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Module;

public class BytecodeModule : ICdnModule
{
    private readonly CdnInfo _cdnInfo;
    private readonly DownloaderService _downloaderService;
    
    public BytecodeModule(IOptions<CdnInfo> cdnInfo, DownloaderService downloaderService)
    {
        _downloaderService = downloaderService;
        _cdnInfo = cdnInfo.Value;
    }

    public async Task Download(UserPreferences userPreferences, ProgressTask task)
    {
        BuildInformation? buildInformation = await _downloaderService
            .GetBuildInformation($"{_cdnInfo.BaseUri}js-bytecode-module/{userPreferences.Branch}/{userPreferences.Os}/update.json");
        
        if (buildInformation == null)
            throw new Exception("buildInformation not found");
        
        var bytecodePath = _downloaderService.GetModulesPath(userPreferences);
        await _downloaderService.DownloadFiles(GetFileList(userPreferences), buildInformation, bytecodePath, task);
    }

    public List<string> GetFileList(UserPreferences userPreferences)
    {
        var basePath = GetBasePath(userPreferences);
        
        return new List<string>()
        {
            $"{basePath}/{(userPreferences.Os == "x64_win32" ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}"
        };
    }

    public string GetBasePath(UserPreferences userPreferences)
    {
        return $"{_cdnInfo.BaseUri}js-bytecode-module/{userPreferences.Branch}/{userPreferences.Os}/modules";
    }
}
