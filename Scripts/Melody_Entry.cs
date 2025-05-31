using HarmonyLib;
using UnityEngine;
using System.Reflection;
using Timberborn.ModManagerScene;
using System.IO;
using Timberborn.Modding;
using System;

namespace Mods.Melody.Scripts
{
    public static class ModInfo
    {
        public const string HarmonyID = "Melody.music";
        public const string Name = "MelodyMusic";
        public static string ModDirectoryPath { get; internal set; }
        public static MelodyConfig CurrentConfig { get; internal set; }
    }

    public class ModEntry : IModStarter
    {
        private static GameObject _modInstanceObject;

        public void StartMod(IModEnvironment modEnvironment)
        {
            ModInfo.ModDirectoryPath = modEnvironment.ModPath;

            LoadConfig();

            var harmony = new Harmony(ModInfo.HarmonyID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Mod loaded. Path: {ModInfo.ModDirectoryPath}");
            Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Harmony patches applied successfully!");

            if (string.IsNullOrEmpty(ModInfo.ModDirectoryPath))
            {
                Debug.LogError($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] ModDirectoryPath is null or empty! Custom music may not load.");
            }

            if (_modInstanceObject == null)
            {
                _modInstanceObject = new GameObject("MelodyCustomMusicLoaderObject");
                UnityEngine.Object.DontDestroyOnLoad(_modInstanceObject);
                CustomMusicLoader loader = _modInstanceObject.AddComponent<CustomMusicLoader>();

                loader.StartLoadingAllTracksFromFolder();
            }
        }
        
        private void LoadConfig()
        {
            string configFilePath = Path.Combine(ModInfo.ModDirectoryPath, "MelodyConfig.json");
            Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Attempting to load config from: {configFilePath}");

            bool needsResave = false;

            if (File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
                    ModInfo.CurrentConfig = JsonUtility.FromJson<MelodyConfig>(json);
                    Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Config loaded successfully.");

                    if (ModInfo.CurrentConfig.configVersion < MelodyConfig.LATEST_VERSION)
                    {
                        Debug.Log($"[{ModInfo.Name}] Old config version ({ModInfo.CurrentConfig.configVersion}) detected. Upgrading to version {MelodyConfig.LATEST_VERSION}.");
                        needsResave = true;

                        if (ModInfo.CurrentConfig.configVersion < 1)
                        {
                            ModInfo.CurrentConfig.enableMp3Support = true;
                            ModInfo.CurrentConfig.enableWavSupport = false;
                        }
                        ModInfo.CurrentConfig.configVersion = MelodyConfig.LATEST_VERSION;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{ModInfo.Name}] Error loading config file: {ex.Message}. Creating new default config.");
                    ModInfo.CurrentConfig = new MelodyConfig();
                    needsResave = true;
                }
            }
            else
            {
                Debug.Log($"[{ModInfo.Name}] Config file not found. Creating a default one.");
                ModInfo.CurrentConfig = new MelodyConfig();
                needsResave = true;
            }

            if (ModInfo.CurrentConfig == null)
            {
                ModInfo.CurrentConfig = new MelodyConfig();
                needsResave = true;
            }
            
            if (needsResave)
            {
                try
                {
                    string jsonToWrite = JsonUtility.ToJson(ModInfo.CurrentConfig, true);
                    File.WriteAllText(configFilePath, jsonToWrite);
                    Debug.Log($"[{ModInfo.Name}] Config file saved (version: {ModInfo.CurrentConfig.configVersion}).");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[{ModInfo.Name}] Error saving config file: {ex.Message}.");
                }
            }

            Debug.Log($"[{ModInfo.Name}] Final Config Loaded. Volume Multiplier: {ModInfo.CurrentConfig.globalCustomTrackVolumeMultiplier}, MP3 Support: {ModInfo.CurrentConfig.enableMp3Support}, WAV Support: {ModInfo.CurrentConfig.enableWavSupport}");
        }
    }

    public static class ModGameInfo
    {
        public static readonly string DroughtTrackName = "Music_Game.DroughtTrack";
        public static readonly string StandardMusicGroupKey = "Music_Game.StandardTrack";
    }

    public static class ModState
    {
        public static bool IsHazardousWeatherActive = false;
    }
}