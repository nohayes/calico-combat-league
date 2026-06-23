using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");
    static readonly string TempSavePath = SavePath + ".tmp";
    static readonly string BackupSavePath = SavePath + ".bak";

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
            File.WriteAllText(TempSavePath, json);

            if (File.Exists(SavePath))
            {
                File.Replace(TempSavePath, SavePath, BackupSavePath);
                TryDelete(BackupSavePath);
            }
            else
            {
                File.Move(TempSavePath, SavePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem.Save failed: {e.Message}");
            TryDelete(TempSavePath);
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
        TryDelete(TempSavePath);
        TryDelete(BackupSavePath);
    }

    static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem could not delete '{path}': {e.Message}");
        }
    }
}
