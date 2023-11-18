using AltV.Binaries.Updater.Abstraction.Data;
using Spectre.Console;

namespace AltV.Binaries.Updater.Abstraction.Interface;

public interface ICdnModule : ISingleton
{
    public Task Download(UserPreferences userPreferences, ProgressTask task);
    public List<string> GetFileList(UserPreferences userPreferences);
    public string GetBasePath(UserPreferences userPreferences);
}