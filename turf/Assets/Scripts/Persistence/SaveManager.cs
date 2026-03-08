using System.IO;
using UnityEngine;

// Handles reading and writing PlayerAccountProfile to disk as JSON.
// Only PlayerAccountProfile is ever persisted — run and match data live in memory only.
public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "account.json");

    public static void Save(PlayerAccountProfile profile)
    {
        string json = JsonUtility.ToJson(profile, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    public static PlayerAccountProfile Load()
    {
        if (!File.Exists(SavePath))
            return new PlayerAccountProfile();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<PlayerAccountProfile>(json);
    }

    public static bool SaveExists() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
