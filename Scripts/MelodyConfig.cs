using System;

namespace Mods.Melody.Scripts
{
    [System.Serializable]
    public class MelodyConfig
    {
        public const int LATEST_VERSION = 1;
        
        public int configVersion;    
        public float globalCustomTrackVolumeMultiplier;
        public bool enableMp3Support;
        public bool enableWavSupport;

        public MelodyConfig()
        {
            configVersion = LATEST_VERSION;
            globalCustomTrackVolumeMultiplier = 1.0f;
            enableMp3Support = true;
            enableWavSupport = false;
            // globalCustomTrackVolumeMultiplier = 0.25f; // for testing, however this can be enabled if the custom tracks are too loud. Values up to 1.0f increase volume and values away from 1.0f decrease volume. Can either edit the value above this line OR uncomment this line while commenting out the other line
        }
    }
}