using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Module;

public class JsModule : ICdnModule
{
    private readonly CdnInfo _cdnInfo;
    private readonly DownloaderService _downloaderService;
    
    public JsModule(IOptions<CdnInfo> cdnInfo, DownloaderService downloaderService)
    {
        _cdnInfo = cdnInfo.Value;
        _downloaderService = downloaderService;
    }

    public async Task Download(UserPreferences userPreferences, ProgressTask task)
    {
        BuildInformation? buildInformation = await _downloaderService
            .GetBuildInformation($"{_cdnInfo.BaseUri}js-module/{userPreferences.Branch}/{userPreferences.Os}/update.json");

        if (buildInformation == null)
        {
            throw new Exception("buildInformation not found");
        }
        
        var jsModulesPath = Path.Combine(_downloaderService.GetModulesPath(userPreferences), "js-module");

        if (!Directory.Exists(jsModulesPath))
            Directory.CreateDirectory(jsModulesPath);

        await _downloaderService.DownloadFiles(GetFileList(userPreferences), buildInformation, jsModulesPath, task);
    }

    public List<string> GetFileList(UserPreferences userPreferences)
    {
        var basePath = GetBasePath(userPreferences);
        
        return new List<string>
        {
            $"{basePath}/{(userPreferences.Os == "x64_win32" ? "js-module.dll" : "libjs-module.so")}",
            $"{basePath}/{(userPreferences.Os == "x64_win32" ? "libnode.dll" : "libnode.so.108")}"
        };
    }

    public string GetBasePath(UserPreferences userPreferences)
    {
        return $"{_cdnInfo.BaseUri}js-module/{userPreferences.Branch}/{userPreferences.Os}/modules/js-module";
    }
}