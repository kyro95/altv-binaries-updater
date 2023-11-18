namespace AltV.Binaries.Updater.Abstraction.Data;

public class UserPreferences
{
   public string Path { get; set; }
   public string Branch { get; set; }
   public string Os { get; set; }

   public UserPreferences(string path, string branch, string os)
   {
      Path = path;
      Branch = branch;
      Os = os;
   }
}