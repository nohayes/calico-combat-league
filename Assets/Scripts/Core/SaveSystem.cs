using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static bool SaveExists() => File.Exists(SavePath);

    public static void Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveSystem.Save called with null data.");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem.Save failed: {e.Message}");
        }
    }

    public static SaveData Load()
    {
        if (!SaveExists()) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem.Load failed: {e.Message}");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (SaveExists()) File.Delete(SavePath);
    }
}
