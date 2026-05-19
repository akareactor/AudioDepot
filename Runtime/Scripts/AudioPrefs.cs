using UnityEngine;

// настройки аудио, независимая абстракция

namespace KulibinSpace.AudioDepot {

    public static class AudioPrefs {
        public static bool audioOn { get { return _audioOn; } set { _audioOn = value; } }
        public static float soundVolume { get { return _soundVolume; } set { _soundVolume = value; } }
        private static bool _audioOn = true;
        private static float _soundVolume = 1.0f;


        public static void SaveSettings () {
            Debug.Log("AudioPrefs.Save заглушка");
        }
    }
}
