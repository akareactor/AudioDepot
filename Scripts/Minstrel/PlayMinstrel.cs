using UnityEngine;

// 9:32 25.09.2020 общий концепт Менестреля - композиция это всегда фоновая музыка, которая играет в единственном экземпляре. Две композиции одновременно не играют.

namespace KulibinSpace.AudioDepot {

    [System.Serializable]
    public struct Piece {
        [Tooltip("Ссылка на проигрываемый клип")]
        public AudioClip audioclip;
        [Tooltip("Зацикливать воспроизведение клипа.")]
        // Если композиция состоит более чем из одного кусочка Piece, то loop имеет смысл только для последнего. Правильнее зацикливать всю композицию, но тогда нативный параметр AudioSource.loop просто не будет никогда использоваться. Разве что произвести оптимизацию и включать AudioSource.loop для одного кусочка, но это лишнее усложнение. Кроме того, если оставить зацикливание только на уровне композиции, тогда для цепочки intro-loop придётся заводить уже две композиции, а это дополнительный список композиций. Так что намеренно оставляю loop для каждого кусочка.
        public bool loop;
    }

    [System.Serializable]
    public struct Composition {
        [Header("[кусочки композиции для последовательного проигрывания]")]
        [Tooltip("кусочки композиции для последовательного проигрывания")]
        public Piece[] pieces; // последовательность групп для проигрывания
    }

    // 19:45 21.09.2020 сопровождающий для Minstrel
    // Отделение логики буферного проигрывания от списка композиций. Есть подозрение, что для послойной динамической музыки потребуется два проигрывателя.
    // 12:27 23.09.2020 убираю затухание совсем, так как громкостью можно управлять легко через снэпшоты на уровне аудиомиксера.
    // 19:17 20.07.2021 убрал автостарт Play совсем.

    public class PlayMinstrel : PlayMinstrelBase {

        public UnityEngine.Audio.AudioMixerGroup mixerGroup; // оба аудиоисточника подключаются к одной группе миксера.
        public Composition composition;
        public AudioSourceSettings settings;
        AudioSource[] asrc; // два аудиоисточника, для точного воспроизведения через PlayScheduled
        int toggle; // индекс для переключения источников, toggle = 1 - toggle;
        double nextStartTime;
        int nextClip; // счётчик клипов в composition.pieces
        bool stopping;

        AudioSource CreateAudiosource () {
            AudioSource ret = gameObject.AddComponent<AudioSource>() as AudioSource;
            ApplySettings(ret, settings);
            ret.outputAudioMixerGroup = mixerGroup;
            ret.playOnAwake = false;
            return ret;
        }

        public void ApplySettings (AudioSource source, AudioSourceSettings config) {
            if (config != null) {
                source.volume = config.volume;
                source.pitch = config.pitch;
                source.spatialBlend = config.spatialBlend;
                source.minDistance = config.minDistance;
                source.maxDistance = config.maxDistance;
                if (config.rolloffMode == AudioRolloffMode.Custom) {
                    source.rolloffMode = AudioRolloffMode.Custom;
                    source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, config.customRolloff);
                } else {
                    source.rolloffMode = config.rolloffMode;
                }
                source.loop = config.loop;
                source.playOnAwake = config.playOnAwake;
                // Дополнительно:
                source.dopplerLevel = 0f;
                source.spread = 0f;
            }
        }

        void Awake () {
            asrc = new AudioSource[2];
            asrc[0] = CreateAudiosource();
            asrc[1] = CreateAudiosource();
        }

        void PlayNext () {
            asrc[toggle].clip = composition.pieces[nextClip].audioclip;
            asrc[toggle].loop = composition.pieces[nextClip].loop;
            asrc[toggle].PlayScheduled(nextStartTime);
            if (asrc[toggle].loop) {
                nextClip = -1;
            } else {
                // расчётное время старта последующего клипа
                double duration = (double)asrc[toggle].clip.samples / asrc[toggle].clip.frequency;
                nextStartTime = nextStartTime + duration;
                // Switches the toggle to use the other Audio Source next
                toggle = 1 - toggle;
                // Increase the clip index number, reset if it runs out of clips
                // nextClip = nextClip < composition.pieces.Length - 1 ? nextClip + 1 : 0; // то же самое перепишу нормально, только добавлю охрану loop
                // Увеличим счётчик клипов на 1, и вернём в 0 если композиция зациклена или в -1 если не зациклена.
                if (nextClip < composition.pieces.Length - 1) {
                    nextClip += 1;
                } else {
                    if (loop) nextClip = 0; else nextClip = -1;
                }
            }
        }

        // запускается из родительского PlayComposition, в котором делается останов предыдущего трека
        public override void PlayImmediately () {
            toggle = 0;
            nextClip = 0;
            // точное время начала, для точного старта последующего клипа.
            // Константа 0.2 взята отсюда https://johnleonardfrench.com/articles/ultimate-guide-to-playscheduled-in-unity/
            nextStartTime = AudioSettings.dspTime + 0.2;
            playing = true; // глобальный флаг, для охраны Update
                            // первый запуск
            PlayNext();
        }

        public override void StopAudio () {
            playing = false;
            asrc[0].Stop();
            asrc[1].Stop();
        }

        void Update () {
            if (playing) {
                if (nextClip >= 0 && AudioSettings.dspTime > nextStartTime - 1) {
                    PlayNext();
                }
            }
        }

        void OnDestroy () {
            foreach (Piece piece in composition.pieces) piece.audioclip.UnloadAudioData();
        }

    }

}
