using HarmonyLib;
using Timberborn.SoundSystem;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Mods.Melody.Scripts
{
    [HarmonyPatch(typeof(Sounds))]
    public static class Sounds_CreateAudioSources_Patch
    {
        [HarmonyPatch(nameof(Sounds.CreateAudioSources))]
        [HarmonyPostfix]
        public static void CreateAudioSources_Postfix(
            Sounds __instance,
            string soundName,
            GameObject audioSourceRoot)
        {
            if (soundName != ModGameInfo.DroughtTrackName && soundName != ModGameInfo.StandardMusicGroupKey)
            {
                return;
            }

            Debug.Log($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Modifying sound list for '{soundName}'. Attempting to add music tracks...");

            AudioClipService audioClipService = __instance._audioClipService;
            AudioSourceFactory audioSourceFactory = __instance._audioSourceFactory;
            Dictionary<string, List<AudioSource>> allSounds = __instance._sounds;

            if (audioClipService == null || audioSourceFactory == null || allSounds == null || 
                audioSourceFactory._audioMixerGroupRetriever == null)
            {
                Debug.LogError($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Critical error: Essential component is null in Sounds instance. Cannot add music for '{soundName}'.");
                return;
            }

            List<AudioSource> existingSourcesList;
            if (!allSounds.TryGetValue(soundName, out existingSourcesList))
            {
                Debug.LogWarning($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] List for soundName '{soundName}' not found in _sounds dictionary. Creating new list.");
                existingSourcesList = new List<AudioSource>();
                allSounds[soundName] = existingSourcesList;
            }

            if (soundName == ModGameInfo.DroughtTrackName)
            {
                Debug.Log($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Modifying HAZARDOUS music list for '{soundName}'.");

                IEnumerable<string> standardMusicClipNames = audioClipService.GetAudioClipNames(ModGameInfo.StandardMusicGroupKey);
                int addedStandardCount = 0;
                if (standardMusicClipNames != null)
                {
                    foreach (string standardClipName in standardMusicClipNames)
                    {
                        if (string.IsNullOrEmpty(standardClipName)) continue;
                        AudioSource newStandardMusicSource = audioSourceFactory.Create(audioSourceRoot, standardClipName);
                        if (newStandardMusicSource != null)
                        {
                            newStandardMusicSource.loop = false;
                            newStandardMusicSource.spatialBlend = 0.0f;
                            if (newStandardMusicSource.outputAudioMixerGroup == null && audioSourceFactory._audioMixerGroupRetriever != null)
                            {
                                newStandardMusicSource.outputAudioMixerGroup = audioSourceFactory._audioMixerGroupRetriever.GetAudioMixerGroupFromSoundName(soundName);
                            }
                            existingSourcesList.Add(newStandardMusicSource);
                            addedStandardCount++;
                        }
                        else { Debug.LogWarning($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Factory returned null for in-game standard clip: {standardClipName}"); }
                    }
                }
                else { Debug.LogWarning($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] GetAudioClipNames for '{ModGameInfo.StandardMusicGroupKey}' returned null."); }
                
                if (addedStandardCount > 0) { Debug.Log($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Added {addedStandardCount} in-game standard music track(s) to '{soundName}'."); }

                if (CustomMusicLoader.IsLoadingAttempted && CustomMusicLoader.IsLoadingSuccessful)
                {
                    AddCustomTracksToPlaylist(CustomMusicLoader.LoadedHazardousOnlyTracks, existingSourcesList, audioSourceRoot, soundName, "Hazardous-Only", audioSourceFactory);
                    AddCustomTracksToPlaylist(CustomMusicLoader.LoadedBothTracks, existingSourcesList, audioSourceRoot, soundName, "Both-Weather (for Hazard)", audioSourceFactory);
                }
                else
                {
                    LogCustomMusicLoadingStatus();
                }
            }
            else if (soundName == ModGameInfo.StandardMusicGroupKey)
            {
                Debug.Log($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Modifying TEMPERATE music list for '{soundName}'.");

                if (CustomMusicLoader.IsLoadingAttempted && CustomMusicLoader.IsLoadingSuccessful)
                {
                    AddCustomTracksToPlaylist(CustomMusicLoader.LoadedTemperateOnlyTracks, existingSourcesList, audioSourceRoot, soundName, "Temperate-Only", audioSourceFactory);
                    AddCustomTracksToPlaylist(CustomMusicLoader.LoadedBothTracks, existingSourcesList, audioSourceRoot, soundName, "Both-Weather (for Temperate)", audioSourceFactory);
                }
                else
                {
                    LogCustomMusicLoadingStatus();
                }
            }
        }

        private static void AddCustomTracksToPlaylist(
            List<AudioClip> tracksToAdd,
            List<AudioSource> existingPlaylist,
            GameObject audioSourceRoot,
            string soundEventName,
            string categoryNameForLog,
            AudioSourceFactory audioSourceFactory)
        {
            if (tracksToAdd == null || tracksToAdd.Count == 0)
            {
                return;
            }

            Debug.Log($"[{ModInfo.Name}] Adding {tracksToAdd.Count} custom tracks from '{categoryNameForLog}' category to '{soundEventName}'.");
            int countAdded = 0;

            foreach (AudioClip customClip in tracksToAdd)
            {
                if (customClip == null)
                {
                    Debug.LogWarning($"[{ModInfo.Name}] Encountered a null AudioClip in '{categoryNameForLog}' list while processing for '{soundEventName}'. Skipping.");
                    continue;
                }

                AudioSource newCustomSource = audioSourceRoot.AddComponent<AudioSource>();
                newCustomSource.clip = customClip;
                newCustomSource.playOnAwake = false;
                newCustomSource.loop = false;
                newCustomSource.spatialBlend = 0.0f;

                if (ModInfo.CurrentConfig != null)
                {
                    newCustomSource.volume = Mathf.Clamp(1.0f * ModInfo.CurrentConfig.globalCustomTrackVolumeMultiplier, 0.0f, 1.0f);
                }
                else
                {
                    newCustomSource.volume = 1.0f;
                }

                if (audioSourceFactory != null && audioSourceFactory._audioMixerGroupRetriever != null)
                {
                    newCustomSource.outputAudioMixerGroup = audioSourceFactory._audioMixerGroupRetriever.GetAudioMixerGroupFromSoundName(soundEventName);
                }
                else
                {
                    Debug.LogError($"[{ModInfo.Name}] AudioSourceFactory or _audioMixerGroupRetriever is null. Cannot set outputAudioMixerGroup for {customClip.name}.");
                }

                existingPlaylist.Add(newCustomSource);
                countAdded++;
            }
            if (countAdded > 0)
            {
                Debug.Log($"[{ModInfo.Name}] Successfully added {countAdded} custom tracks from '{categoryNameForLog}' to '{soundEventName}'.");
            }
        }

        private static void LogCustomMusicLoadingStatus()
        {
            if (!CustomMusicLoader.IsLoadingAttempted)
            {
                Debug.Log($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Custom track loading not yet attempted or component not ready.");
            }
            else if (!CustomMusicLoader.IsLoadingSuccessful)
            {
                Debug.LogWarning($"[{ModInfo.Name}] [{DateTime.Now:HH:mm:ss.fff}] Custom track loading was attempted but not successful. No tracks added.");
            }
        }
    }

    [HarmonyPatch(typeof(Sounds), nameof(Sounds.GetRandomSound))]
    public static class Sounds_GetRandomSound_LogPatch 
    {
        [HarmonyPostfix]
        public static void Postfix(string soundName, AudioSource __result)
        {
            if (soundName == ModGameInfo.DroughtTrackName || soundName == ModGameInfo.StandardMusicGroupKey) {
                if (__result != null && __result.clip != null) {
                }
            }
        }
    }
}