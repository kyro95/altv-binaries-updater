using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using AltV.Binaries.Updater.Abstraction.Data;
using AltV.Binaries.Updater.Abstraction.Interface;
using AltV.Binaries.Updater.Abstraction.Json;
using Spectre.Console;

namespace AltV.Binaries.Updater.Services;

public class DownloaderService : ISingleton
{
    private bool CompareChecksum(FileInfo fileInfo, string checksum)
    {
        try
        {
            using var sha1 = SHA1.Create();
            using var stream = fileInfo.OpenRead();
            var hashBytes = sha1.ComputeHash(stream);
            
            return BitConverter
                .ToString(hashBytes)
                .Replace("-", "")
                .ToLowerInvariant() == checksum;
        } catch (Exception)
        {
            return false;
        }
    }

    public async Task DownloadFiles(List<string> fileList, BuildInformation buildInformation, string path, ProgressTask task)
    {
        foreach (var url in fileList)
        {
            if (await DownloadFile(url, path, buildInformation))
            {
                task.Increment(100 / (double)fileList.Count);
                continue;
            }
            
            AnsiConsole.MarkupLine($"[bold][red3]Error: Failed to download file: {url}[/][/]");
        }
    }

    public async Task<bool> DownloadFile(string url, string path, BuildInformation buildInformation)
    {
        var routes = url.Split('/');
        var fileName = routes.Last();

        /*
         * TODO: Fix this hacky workaround
         */
        fileName = routes
            .Where(route =>
                route.Contains("data") ||
                route.Contains("modules") || 
                route.Contains("js-module")
            )
            .Aggregate(fileName, (current, route) => route + "/" + current);
        
        fileName = fileName.Replace("data/data", "data");
        fileName = fileName.Replace("js-module.dll/js-module/", "");
        fileName = fileName.Replace("js-module/modules/", "modules/");
        
        var fileInfo = new FileInfo(Path.Combine(path, routes.Last()));

        if (fileInfo.Exists && CompareChecksum(fileInfo, buildInformation.HashList[fileName]))
        {
            return true;
        }

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            AnsiConsole.MarkupLine($"[bold][red3]Error: Status code from CDN: {response.StatusCode}[/][/] url: {url}");
            return false;
        }
        
        var filePath = Path.Combine(path, Path.GetFileName(url));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        var fileBytes = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(filePath, fileBytes);

        return true;
    }

    public async Task<BuildInformation?> GetBuildInformation(string providerUrl)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(providerUrl);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return await JsonSerializer
                .DeserializeAsync<BuildInformation>(await response.Content.ReadAsStreamAsync());
        }

        AnsiConsole
            .MarkupLine($"[bold][red3]Error: Status code from CDN: {response.StatusCode}[/][/] function: GetBuildInformation");
        
        return null;
    }

    public string GetModulesPath(UserPreferences userPreferences)
    {
        var modulesPath = Path.Combine(userPreferences.Path, "modules");

        if (!Directory.Exists(modulesPath))
        {
            Directory.CreateDirectory(modulesPath);
        }
        
        return modulesPath;
    }
    
    public string GetOsFileExtension(string os)
    {
        return os == "x64_win32" ? ".exe" : "";
    }

}