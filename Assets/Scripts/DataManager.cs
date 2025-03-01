using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    // Save tile data to a file
    public static void SaveTileDataToFile(Dictionary<string, List<TileData>> tileDataGroups)
    {
        string path = Path.Combine(Application.persistentDataPath, "tileData.dat");
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
        
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

    // Erase specific tile data from file
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

    // Save key tile data to a file
    public static void SaveKeyTileDataToFile(Dictionary<string, List<int>> keyTileData)
    {
        string path = Path.Combine(Application.persistentDataPath, "keyTileData.dat");
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, keyTileData);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"IOException while saving key tile data: {ex.Message}");
        }
    }

    // Save sample sample tile data to a file
    public static void SaveSampleTileDataToFile(Dictionary<string, List<int>> keyTileData)
    {
        string path = Path.Combine(Application.persistentDataPath, "sampleTileData.dat");
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, keyTileData);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"IOException while saving key tile data: {ex.Message}");
        }
    }

    // Load sample tile data from a file
    public static Dictionary<string, List<int>> LoadSampleTileDataFromFile()
    {
        Dictionary<string, List<int>> tileData = new Dictionary<string, List<int>>();
        string filePath = Path.Combine(Application.persistentDataPath, "sampleTileData.dat");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Key tile data file not found.");
            return tileData;
        }

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (fileStream.Length == 0)
                {
                    Debug.LogWarning("Key tile data file is empty.");
                    return tileData;
                }

                // Log the file content before deserialization
                StreamReader reader = new StreamReader(fileStream);
                string fileContent = reader.ReadToEnd();
                Debug.Log($"File content: {fileContent}");

                // Reset the stream position to the beginning before deserializing
                fileStream.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                tileData = (Dictionary<string, List<int>>)formatter.Deserialize(fileStream);

                // Debug log to check loaded data
                foreach (var entry in tileData)
                {
                    Debug.Log($"Loaded Key Tile Data: Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load key tile data: {ex.Message}");
        }

        return tileData;
    }

    // Load key tile data from a file
    public static Dictionary<string, List<int>> LoadKeyTileDataFromFile()
    {
        Dictionary<string, List<int>> tileData = new Dictionary<string, List<int>>();
        string filePath = Path.Combine(Application.persistentDataPath, "keyTileData.dat");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Key tile data file not found.");
            return tileData;
        }

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                if (fileStream.Length == 0)
                {
                    Debug.LogWarning("Key tile data file is empty.");
                    return tileData;
                }

                // Log the file content before deserialization
                StreamReader reader = new StreamReader(fileStream);
                string fileContent = reader.ReadToEnd();
                Debug.Log($"File content: {fileContent}");

                // Reset the stream position to the beginning before deserializing
                fileStream.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                tileData = (Dictionary<string, List<int>>)formatter.Deserialize(fileStream);

                // Debug log to check loaded data
                foreach (var entry in tileData)
                {
                    Debug.Log($"Loaded Key Tile Data: Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load key tile data: {ex.Message}");
        }

        return tileData;
    }

    // Erase specific key tile data from file
    public static void EraseKeyTileDataToFile(string spriteName, int step)
    {
        string path = Path.Combine(Application.persistentDataPath, "keyTileData.dat");

        if (File.Exists(path))
        {
            try
            {
                Dictionary<string, List<int>> keyTileData;
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    keyTileData = (Dictionary<string, List<int>>)formatter.Deserialize(fileStream);
                }

                if (keyTileData.ContainsKey(spriteName))
                {
                    int initialCount = keyTileData[spriteName].Count;
                    keyTileData[spriteName].Remove(step);
                    if (keyTileData[spriteName].Count < initialCount)
                    {
                        Debug.Log($"Removed step {step} from sprite {spriteName}");
                    }

                    if (keyTileData[spriteName].Count == 0)
                    {
                        keyTileData.Remove(spriteName);
                        Debug.Log($"Removed sprite {spriteName} as it became empty.");
                    }

                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(fileStream, keyTileData);
                    }

                    Debug.Log("Specified key tile data erased successfully.");
                }
                else
                {
                    Debug.LogWarning($"Sprite {spriteName} does not exist in key tile data.");
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"IOException while erasing specific key tile data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Key tile data file does not exist.");
        }
    }

    public static void EraseSampleTileDataToFile(string spriteName, int step)
    {
        string path = Path.Combine(Application.persistentDataPath, "sampleTileData.dat");

        if (File.Exists(path))
        {
            try
            {
                Dictionary<string, List<int>> keyTileData;
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    keyTileData = (Dictionary<string, List<int>>)formatter.Deserialize(fileStream);
                }

                if (keyTileData.ContainsKey(spriteName))
                {
                    int initialCount = keyTileData[spriteName].Count;
                    keyTileData[spriteName].Remove(step);
                    if (keyTileData[spriteName].Count < initialCount)
                    {
                        Debug.Log($"Removed step {step} from sprite {spriteName}");
                    }

                    if (keyTileData[spriteName].Count == 0)
                    {
                        keyTileData.Remove(spriteName);
                        Debug.Log($"Removed sprite {spriteName} as it became empty.");
                    }

                    using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(fileStream, keyTileData);
                    }

                    Debug.Log("Specified key tile data erased successfully.");
                }
                else
                {
                    Debug.LogWarning($"Sprite {spriteName} does not exist in key tile data.");
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"IOException while erasing specific key tile data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Key tile data file does not exist.");
        }
    }


    // Save patterns to a file
    public static void SavePatternsToFile(List<PatternData> patterns)
    {
        string path = Path.Combine(Application.persistentDataPath, "patterns.dat");
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, patterns);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"IOException while saving patterns: {ex.Message}");
        }
    }

    // Load patterns from a file
    public static List<PatternData> LoadPatternsFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "patterns.dat");
        List<PatternData> patterns = new List<PatternData>();

        if (File.Exists(path))
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    patterns = (List<PatternData>)formatter.Deserialize(fileStream);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError($"IOException while loading patterns: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Patterns file does not exist.");
        }

        return patterns;
    }

    // Save project data to a file
    public static void SaveProjectToFile(ProjectData projectData)
    {
        string path = Path.Combine(Application.persistentDataPath, "project.dat");
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        try
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, projectData);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"IOException while saving project data: {ex.Message}");
        }
    }

}