using System.IO;
using UnityEngine;

namespace Runtime.Meta
{
    [System.Serializable]
    public class Settings
    {
        public static string path => Path.Combine(Application.dataPath, "settings.json");
        
        private static Settings fileCache;
        public static Settings file
        {
            get
            {
                if (fileCache == null) Load();
                return fileCache;
            }
            set => fileCache = value;
        }

        public static void Load()
        {
            if (File.Exists(path))
            {
                var raw = File.ReadAllText(path);
                fileCache = JsonUtility.FromJson(raw, typeof(Settings)) as Settings;
                if (fileCache != null)
                {
                    Debug.Log($"Settings Loaded at {path}");
                    return;
                }
            }
            
            Debug.Log($"No Valid Settings file found at {path}");
            Debug.Log("Creating new Settings File");
            file = new Settings();
            Save();
        }

        public static void Save()
        {
            if (fileCache == null) return;

            var raw = JsonUtility.ToJson(fileCache);
            File.WriteAllText(path, raw);
            Debug.Log($"Settings Saved at {path}");
        }
    }
}