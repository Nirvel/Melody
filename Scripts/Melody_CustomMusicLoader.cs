using System.Collections;
using System.Collections.Generic;
using System.IO;
using Timberborn.Modding;
using UnityEngine;
using UnityEngine.Networking;

namespace Mods.Melody.Scripts
{
    public class CustomMusicLoader : MonoBehaviour
    {
        public static List<AudioClip> LoadedTemperateOnlyTracks = new List<AudioClip>();
        public static List<AudioClip> LoadedHazardousOnlyTracks = new List<AudioClip>();
        public static List<AudioClip> LoadedBothTracks = new List<AudioClip>();
        public static bool IsLoadingAttempted = false;
        public static bool IsLoadingSuccessful = false;
        
        public void StartLoadingAllTracksFromFolder()
        {
            LoadedTemperateOnlyTracks.Clear();
            LoadedHazardousOnlyTracks.Clear();
            LoadedBothTracks.Clear();

            IsLoadingAttempted = true;
            IsLoadingSuccessful = false; 

            string baseCustomMusicPath = Path.Combine(ModInfo.ModDirectoryPath, "CustomMusic");

            if (!Directory.Exists(baseCustomMusicPath))
            {
                Debug.LogWarning($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Base CustomMusic folder not found at: {baseCustomMusicPath}. No custom tracks will be loaded.");
                return;
            }
            
            bool mp3Support = ModInfo.CurrentConfig != null && ModInfo.CurrentConfig.enableMp3Support;
            bool wavSupport = ModInfo.CurrentConfig != null && ModInfo.CurrentConfig.enableWavSupport;

            var categories = new[] {
                new { DiskSubfolderName = "ForTemperate", TargetList = LoadedTemperateOnlyTracks, LogCategoryName = "Temperate-Only" },
                new { DiskSubfolderName = "ForHazardous", TargetList = LoadedHazardousOnlyTracks, LogCategoryName = "Hazardous-Only" }, 
                new { DiskSubfolderName = "ForBoth", TargetList = LoadedBothTracks, LogCategoryName = "Both-Weather" }
            };

            int totalFilesFound = 0;

            foreach (var category in categories)
            {
                string categoryPath = Path.Combine(baseCustomMusicPath, category.DiskSubfolderName);
                if (Directory.Exists(categoryPath))
                {
                    List<string> audioFilePaths = new List<string>();
                    audioFilePaths.AddRange(Directory.GetFiles(categoryPath, "*.ogg"));
                    
                    if (mp3Support)
                    {
                        audioFilePaths.AddRange(Directory.GetFiles(categoryPath, "*.mp3"));
                    }
                    if (wavSupport)
                    {
                        audioFilePaths.AddRange(Directory.GetFiles(categoryPath, "*.wav"));
                    }

                    if (audioFilePaths.Count > 0)
                    {
                        Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Found {audioFilePaths.Count} audio file(s) in {category.DiskSubfolderName} folder (OGG always included; MP3 enabled: {mp3Support}, WAV enabled: {wavSupport}).");
                        totalFilesFound += audioFilePaths.Count;
                        
                        foreach (string fullFilePath in audioFilePaths) 
                        {
                            string extension = Path.GetExtension(fullFilePath).ToLowerInvariant();
                            AudioType audioTypeToLoad = AudioType.UNKNOWN;

                            if (extension == ".ogg")
                            {
                                audioTypeToLoad = AudioType.OGGVORBIS;
                            }
                            else if (extension == ".mp3")
                            {
                                if (mp3Support)
                                {
                                    audioTypeToLoad = AudioType.MPEG;
                                }
                            }
                            else if (extension == ".wav")
                            {
                                if (wavSupport)
                                {
                                    audioTypeToLoad = AudioType.WAV;
                                }
                            }

                            if (audioTypeToLoad != AudioType.UNKNOWN)
                            {
                                StartCoroutine(LoadTrackCoroutine(fullFilePath, audioTypeToLoad, category.TargetList, category.LogCategoryName));
                            }
                            else if (extension == ".ogg")
                            {
                                Debug.LogWarning($"[{ModInfo.Name}] Skipped file with unsupported or disabled extension: '{extension}' for {Path.GetFileName(fullFilePath)}.");
                            }
                        }
                    }
                }
            }

            if (totalFilesFound == 0)
            {
                Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] No processable .ogg" + (mp3Support ? "/.mp3" : "") + (wavSupport ? "/.wav" : "") + $" files found in any category subfolders within {baseCustomMusicPath}.");
            }
        }

        public IEnumerator LoadTrackCoroutine(string fullFilePath, AudioType audioType, List<AudioClip> targetList, string categoryName)
        {

            if (string.IsNullOrEmpty(ModInfo.ModDirectoryPath))
            {
                Debug.LogError($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Mod directory path is null or empty in CustomMusicLoader. Cannot load custom track from '{fullFilePath}'.");
                yield break; 
            }

            string trackFileNameForLogging = Path.GetFileName(fullFilePath);
            string fileUri = "file:///" + fullFilePath.Replace("\\", "/"); 

            Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Attempting to load {categoryName} track from URI: {fileUri}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || 
                    www.result == UnityWebRequest.Result.ProtocolError ||
                    www.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Error loading custom track '{trackFileNameForLogging}' ({categoryName}): {www.error}");
                }
                else
                {
                    AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(www);
                    if (loadedClip != null)
                    {
                        loadedClip.name = trackFileNameForLogging;
                        targetList.Add(loadedClip);
                        IsLoadingSuccessful = true; 
                        Debug.Log($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Successfully loaded '{trackFileNameForLogging}' for {categoryName}. Length: {loadedClip.length}s. Total in this category list: {targetList.Count}");
                    }
                    else
                    {
                         Debug.LogError($"[{ModInfo.Name}] [{System.DateTime.Now:HH:mm:ss.fff}] Loaded custom track '{trackFileNameForLogging}' for {categoryName} but GetContent returned null.");
                    }
                }
            }
        }
    }
}