using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using AltV.Binaries.Updater.Services;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AltV.Binaries.Updater.Module;

public class VoiceModule : ICdnModule
{
    private readonly CdnInfo _cdnInfo;
    private readonly DownloaderService _downloaderService;
    
    public VoiceModule(IOptions<CdnInfo> cdnInfo, DownloaderService downloaderService)
    {
        _downloaderService = downloaderService;
        _cdnInfo = cdnInfo.Value;
    }
    
    public async Task Download(UserPreferences userPreferences, ProgressTask task)
    {
        BuildInformation? buildInformation = await _downloaderService
            .GetBuildInformation($"{GetBasePath(userPreferences)}/update.json");
        
        if (buildInformation == null)
            throw new Exception("buildInformation not found");
        
        await _downloaderService.DownloadFiles(GetFileList(userPreferences), buildInformation, userPreferences.Path, task);
    }

    public List<string> GetFileList(UserPreferences userPreferences)
    {
        string basePath = GetBasePath(userPreferences);
        
        return new List<string>
        {
            $"{basePath}/altv-voice-server{_downloaderService.GetOsFileExtension(userPreferences.Os)}",
        };
    }

    public string GetBasePath(UserPreferences userPreferences)
    {
        return $"{_cdnInfo.BaseUri}voice-server/{userPreferences.Branch}/{userPreferences.Os}";
    }
}