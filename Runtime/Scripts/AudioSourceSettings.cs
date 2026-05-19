using UnityEngine;

namespace KulibinSpace.AudioDepot {

    [CreateAssetMenu(menuName = "Kulibin Space/Scriptable Objects/Audio/AudioSource Settings")]
    public class AudioSourceSettings : ScriptableObject {
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        public float minDistance = 1f;
        public float maxDistance = 50f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        public AnimationCurve customRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        public bool loop = false;
        public bool playOnAwake = false;
    }

}
