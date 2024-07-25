using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class DataManagerKeys
{
    // Save tile data to a file
    public static void SaveTileDataToFile(Dictionary<string, List<TileDataKeys>> tileDataGroups)
    {
        string path = Path.Combine(Application.persistentDataPath, "tileDataKeys.dat");

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
    public static Dictionary<string, List<TileDataKeys>> LoadTileDataFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "tileDataKeys.dat");
        Dictionary<string, List<TileDataKeys>> tileDataGroups = new Dictionary<string, List<TileDataKeys>>();

        if (File.Exists(path))
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    tileDataGroups = (Dictionary<string, List<TileDataKeys>>)formatter.Deserialize(fileStream);
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
}
