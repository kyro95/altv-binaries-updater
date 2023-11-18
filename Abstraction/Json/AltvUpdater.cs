using System.Text.Json.Serialization;

namespace AltV.Binaries.Updater.Abstraction.Json;

public class AltvUpdater
{
    public AltvUpdater(string branch, string os, List<string> modules)
    {
        Branch = branch;
        Os = os;
        Modules = modules;
    }

    [JsonPropertyName("branch")]
    public string Branch { get; set; }
    
    [JsonPropertyName("os")]
    public string Os { get; set; }
    
    [JsonPropertyName("modules")]
    public List<string> Modules { get; set; }
}