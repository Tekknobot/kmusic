using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class DataManager
{
    // Save tile data to a file
    public static void SaveTileDataToFile(Dictionary<string, List<TileData>> tileDataGroups)
    {
        string path = Path.Combine(Application.persistentDataPath, "tileData.dat");

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, tileDataGroups);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"IOException while saving tile data: {ex.Message}");
        }
    }

    // Load tile data from a file
    public static Dictionary<string, List<TileData>> LoadTileDataFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "tileData.dat");
        Dictionary<string, List<TileData>> tileDataGroups = new Dictionary<string, List<TileData>>();

        if (File.Exists(path))
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    tileDataGroups = (Dictionary<string, List<TileData>>)formatter.Deserialize(fileStream);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"IOException while loading tile data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Tile data file does not exist.");
        }

        return tileDataGroups;
    }

    public static void EraseTileDataToFile(string spriteGroup, string currentSprite, float step)
    {
        string path = Path.Combine(Application.persistentDataPath, "tileData.dat");

        if (File.Exists(path))
        {
            try
            {
                Dictionary<string, List<TileData>> tileDataGroups;
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    tileDataGroups = (Dictionary<string, List<TileData>>)formatter.Deserialize(fileStream);
                }

                if (tileDataGroups.ContainsKey(spriteGroup))
                {
                    int initialCount = tileDataGroups[spriteGroup].Count;
                    tileDataGroups[spriteGroup].RemoveAll(tile => tile.SpriteName == currentSprite && tile.Step == step);
                    if (tileDataGroups[spriteGroup].Count < initialCount)
                    {
                        Debug.Log($"Removed {initialCount - tileDataGroups[spriteGroup].Count} tiles from group {spriteGroup}");
                    }

                    if (tileDataGroups[spriteGroup].Count == 0)
                    {
                        tileDataGroups.Remove(spriteGroup);
                        Debug.Log($"Removed sprite group {spriteGroup} as it became empty.");
                    }

                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(fileStream, tileDataGroups);
                    }

                    Debug.Log("Specified tile data erased successfully.");
                }
                else
                {
                    Debug.LogWarning($"Sprite group {spriteGroup} does not exist.");
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"IOException while erasing specific tile data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Tile data file does not exist.");
        }
    }

}
