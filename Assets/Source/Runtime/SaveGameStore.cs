using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveGameStore
{
    [Serializable]
    private sealed class IntEntry
    {
        public string key;
        public int value;
    }

    [Serializable]
    private sealed class SaveFile
    {
        public int schemaVersion = 1;
        public List<IntEntry> integers = new();
    }

    private const int CurrentSchemaVersion = 1;
    private const string FileName = "ori-nabiji-save.json";

    private static SaveFile _data;
    private static bool _loaded;
    private static bool _dirty;

    public static bool IsDirty => _dirty;

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _data = null;
        _loaded = false;
        _dirty = false;
    }

    public static int GetInt(string key, int fallback = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            return fallback;

        EnsureLoaded();
        IntEntry entry = _data.integers.Find(x => x.key == key);
        return entry != null ? entry.value : fallback;
    }

    public static bool GetBool(string key, bool fallback = false)
    {
        return GetInt(key, fallback ? 1 : 0) != 0;
    }

    public static void SetInt(string key, int value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        EnsureLoaded();
        IntEntry entry = _data.integers.Find(x => x.key == key);
        if (entry == null)
        {
            entry = new IntEntry { key = key, value = value };
            _data.integers.Add(entry);
            _dirty = true;
            return;
        }

        if (entry.value == value)
            return;

        entry.value = value;
        _dirty = true;
    }

    public static void SetBool(string key, bool value)
    {
        SetInt(key, value ? 1 : 0);
    }

    public static void Flush()
    {
        EnsureLoaded();
        if (!_dirty)
            return;

        try
        {
            string directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string tempPath = SavePath + ".tmp";
            string json = JsonUtility.ToJson(_data);
            File.WriteAllText(tempPath, json, Encoding.UTF8);

            if (File.Exists(SavePath))
                File.Delete(SavePath);

            File.Move(tempPath, SavePath);
            _dirty = false;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save game data: {exception.Message}");
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
            return;

        _loaded = true;
        _data = new SaveFile { schemaVersion = CurrentSchemaVersion };

        if (!File.Exists(SavePath))
            return;

        try
        {
            string json = File.ReadAllText(SavePath, Encoding.UTF8);
            SaveFile loaded = JsonUtility.FromJson<SaveFile>(json);
            if (loaded == null)
                return;

            loaded.integers ??= new List<IntEntry>();
            loaded.schemaVersion = CurrentSchemaVersion;
            _data = loaded;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save data could not be loaded. Starting with defaults. {exception.Message}");
        }
    }
}

public static class SaveKeyUtility
{
    public static string ForComponent(Component component, string prefix, string explicitId = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitId))
            return $"{prefix}/{explicitId}";

        if (!component)
            return prefix;

        string sceneName = component.gameObject.scene.IsValid()
            ? component.gameObject.scene.name
            : "runtime";

        return $"{prefix}/{sceneName}/{GetHierarchyPath(component.transform)}";
    }

    private static string GetHierarchyPath(Transform transform)
    {
        StringBuilder builder = new StringBuilder();
        Transform current = transform;

        while (current)
        {
            if (builder.Length > 0)
                builder.Insert(0, '/');

            builder.Insert(0, $"{current.name}[{current.GetSiblingIndex()}]");
            current = current.parent;
        }

        return builder.ToString();
    }
}
