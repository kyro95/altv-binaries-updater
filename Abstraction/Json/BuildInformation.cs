using System.Text.Json.Serialization;

namespace AltV.Binaries.Updater.Abstraction.Json;

public class BuildInformation
{
    [JsonPropertyName("latestBuildNumber")]
    public int LatestBuildNumber { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    [JsonPropertyName("sdkVersion")]
    public string SdkVersion { get; set; }
    
    [JsonPropertyName("hashList")]
    public Dictionary<string, string> HashList { get; set; }
    
    public BuildInformation(string version, string sdkVersion)
    {
        Version = version;
        SdkVersion = sdkVersion;
        HashList = new Dictionary<string, string>();
    }
}