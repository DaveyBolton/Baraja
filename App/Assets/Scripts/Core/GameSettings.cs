using UnityEngine;

namespace Baraja.Core
{
    // Player-facing settings, persisted to PlayerPrefs. No audio mixer exists
    // yet in this project - FxVolume/MusicVolume are stored now so the Options
    // UI has something real to read/write, and whichever AudioSource setup
    // lands later just needs to read these two floats.
    public static class GameSettings
    {
        private const string FxKey = "baraja_fx_volume";
        private const string MusicKey = "baraja_music_volume";
        private const string SpanishKey = "baraja_lang_spanish";

        public static float FxVolume
        {
            get => PlayerPrefs.GetFloat(FxKey, 1f);
            set => PlayerPrefs.SetFloat(FxKey, value);
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 1f);
            set => PlayerPrefs.SetFloat(MusicKey, value);
        }

        // true = Spanish (the project's default language), false = English.
        public static bool Spanish
        {
            get => PlayerPrefs.GetInt(SpanishKey, 1) != 0;
            set => PlayerPrefs.SetInt(SpanishKey, value ? 1 : 0);
        }
    }
}
