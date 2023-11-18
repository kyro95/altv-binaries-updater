using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Module;

public class DataModule : ICdnModule
{
    private readonly CdnInfo _cdnInfo;
    private readonly DownloaderService _downloaderService;

    public DataModule(IOptions<CdnInfo> cdnInfo, DownloaderService downloaderService)
    {
        _downloaderService = downloaderService;
        _cdnInfo = cdnInfo.Value;
    }

    public async Task Download(UserPreferences userPreferences, ProgressTask task)
    {
        BuildInformation? buildInformation = await _downloaderService
            .GetBuildInformation($"{_cdnInfo.BaseUri}data/{userPreferences.Branch}/update.json");

        if (buildInformation == null)
        {
            throw new Exception("buildInformation not found");
        }
        
        var dataPath = Path.Combine(userPreferences.Path, "data");
        
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        
        await _downloaderService.DownloadFiles(GetFileList(userPreferences), buildInformation, dataPath, task);
    }

    public List<string> GetFileList(UserPreferences userPreferences)
    {
        string basePath = GetBasePath(userPreferences);
        
        return new List<string>
        {
            $"{basePath}/vehmodels.bin",
            $"{basePath}/vehmods.bin",
            $"{basePath}/clothes.bin",
            $"{basePath}/pedmodels.bin",
            $"{basePath}/weaponmodels.bin",
            $"{basePath}/rpfdata.bin",
        };
    }

    public string GetBasePath(UserPreferences userPreferences)
    {
        return $"{_cdnInfo.BaseUri}data/{userPreferences.Branch}/data";
    }
}