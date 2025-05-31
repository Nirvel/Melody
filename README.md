# Melody - Custom Music for Timberborn

**Author:** Levrin
**Version:** 1.0
**Game:** Timberborn
**Minimum Game Version:** 0.7.0.0
**Required Dependencies:** Harmony (installed from timberborn.mod.io)

---

## How to Use

### Adding Your Custom Music

1.  **Locate the Melody Mod Folder:** After installing Melody, find the mod's main folder (`\Documents\Timberborn\Mods\Melody`)
2.  **Add Your Music Files:**
    * Copy your custom `.ogg` or `.mp3` music files into the appropriate subfolders. I personally use the `.ogg` file type as this is what Timberborn uses, however either *should* be fine.
    * Each folder has a name for where the tracks will play. Ex: `ForTemperate` means music added to that folder will only play during temperate (non-drought/tide) weather. `ForHazardous` is for Droughts and Bad Tides only. `ForBoth` is obviously for both temperate and hazardous weather.
    * **IMPORTANT:** Your music files **must be in the .ogg (Ogg Vorbis) format**. Other formats (like .mp3, .wav) will not work. You can use VLC or Audacity (if you use Audacity, I personally used their github repo as I trust github more) to convert tracks for free; I tested the tracks out primarily using Audacity, but your milage may vary. These two programs are what I used them on, however I am not associated with either of those projects, so if you have another option that you're comfortable with then use that instead.

### Configuring Volume, `.mp3` support, or `.wav` support:

1.  **Config File:** After running Timberborn with Melody installed at least once, a configuration file named `MelodyConfig.json` will be automatically created in the Melody mod's main folder (the same folder where `Scripts` folder and the `CustomMusic` folder are located).
2.  **Edit `MelodyConfig.json`:** Open this file with any standard text editor (like Notepad, VS Code, Notepad++, etc.).
3.  **MP3's and WAV's**
   4. * "enableMp3Support": true (Default: true) - Set to false to _disable_ loading of .mp3 files
   5. * "enableWavSupport": false (Default: false) - Set to true to enable loading of .wav files. **"Important Note on .wav files:** While .wav file support can be enabled by setting "enableWavSupport" to true in MelodyConfig.json, please be aware that WAV files are uncompressed and can be very large. Loading many or very long WAV files may lead to increased game load times and higher RAM usage, potentially impacting performance on some systems. For a balance of quality and performance, .ogg or .mp3 files (in that order) are generally recommended if MP3 support is enabled."
6.  **Adjust Volume:** You will see a setting like this:
    ```json
    {
        "globalCustomTrackVolumeMultiplier": 1.0
    }
    ```
    * Change the value of `globalCustomTrackVolumeMultiplier`:
        * `1.0`: Default. Plays custom tracks at their normal volume (100%).
        * `0.5`: Plays custom tracks at 50% of their normal volume (quieter).
        * `0.25`: Plays custom tracks at 25% of their normal volume (even quieter).
        * `0.0`: Silences all custom tracks.
    * The value is clamped between 0.0 and 1.0 by the game's audio engine. Setting it higher than 1.0 will result in a volume of 1.0.
7.  **Save Changes:** Save the `MelodyConfig.json` file. The new volume setting will take effect the next time music is initialized in the game (e.g., on game startup or potentially when weather conditions change).

---

## Troubleshooting

* **Custom Music Not Playing?**
    * Ensure your music files are in `.ogg` or `.mp3` format.
    * Verify the folder structure is correct: `Melody/CustomMusic/ForTemperate/your_song.ogg`, etc.
    * Check the `Player.log` file for Timberborn (usually found in `C:\Users\[YourUserName]\AppData\LocalLow\Mechanistry\Timberborn\`) for any error messages from "MelodyMusic".
* **Mod Not Loading / Game Crashing?**
    * Make sure Harmony from `timberborn.mod.io` is correctly installed and is compatible versions for your game version.
    * Check the `Player.log` for errors related to mod loading.


---

## Credits & Acknowledgements

* The Harmony library developers for making runtime patching possible, including eMKa for supplying the wrapper for projects such as this mod.