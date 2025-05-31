using HarmonyLib;
using Timberborn.GameSound;
using Timberborn.HazardousWeatherSystem;
using UnityEngine;

namespace Mods.Melody.Scripts
{
    [HarmonyPatch(typeof(GameMusicPlayer))]
    public static class GameMusicPlayerPatches
    {
        [HarmonyPatch(nameof(GameMusicPlayer.OnHazardousWeatherStarted))]
        [HarmonyPostfix]
        public static void OnHazardousWeatherStarted_PostfixPatch()
        {
            Debug.Log($"[{ModInfo.Name}] OnHazardousWeatherStarted_PostfixPatch triggered.");
            ModState.IsHazardousWeatherActive = true;
            Debug.Log($"[{ModInfo.Name}] IsHazardousWeatherActive set to true.");
        }
        
        [HarmonyPatch(nameof(GameMusicPlayer.OnHazardousWeatherEnded))]
        [HarmonyPostfix]
        public static void OnHazardousWeatherEnded_PostfixPatch()
        {
            Debug.Log($"[{ModInfo.Name}] OnHazardousWeatherEnded_PostfixPatch triggered.");
            ModState.IsHazardousWeatherActive = false;
            Debug.Log($"[{ModInfo.Name}] IsHazardousWeatherActive set to false.");
        }
    }
}