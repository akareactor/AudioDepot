using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KulibinSpace.AudioDepot {

[CreateAssetMenu(fileName = "Phased Audio Clip object", menuName = "ScriptableObjects/Phased Audio clip object")]

// 2024-07-17 13:00:04 замораживаю разработку скриптуемого клипа, т.к. неясно, как останавливать звук по времени.

    public class PhasedAudioClip : ScriptableObject {

		public bool aside = false; // выделить клип в отдельный объект, который будет звучать до конца независимо от хоста
		public float volume = 1.0f;
		public AudioClip audioclip;
		public AudioclipPhase[] phases;

		// рандомная фаза из списка или же прокси-объект фазы в случае, если фазы не указаны (упрощение работы с компонентом)
		public AudioclipPhase Phase () {
			AudioclipPhase ret;
			if (phases == null || phases.Length == 0) {
				ret = new AudioclipPhase();
			} else {
				ret = phases[Random.Range(0, phases.Length)];
			}
			// если длина клипа нулевая, то надо играть до конца. Ну так придумано. Смысл нуля может быть только такой (зачем ещё хранить фазу нулевой длины?)
			if (ret.duration <= 0) ret.duration = audioclip.length - ret.time;
		return ret;
		}

        // тут копипаста из AudioDepot
        public float Play (AudioSource src) {
            float stopTime = 0;
            if (audioclip != null) {
                src.clip = audioclip;
                AudioclipPhase phase = Phase();
                stopTime = Time.realtimeSinceStartup + phase.duration;
                src.time = phase.time;
                src.volume = volume;
                // независимый объект, при необходимости
                if (aside) {
                    GameObject clone = new GameObject(name + "_aside");
                    //AudioSource cloneSrc = Abstract.CopyComponent<AudioSource>(src, clone);
                    // поскольку CopyComponent копирует и Local File Identifier, пришлось от него отказаться.
                    AudioSource cloneSrc = clone.AddComponent<AudioSource>();
                    cloneSrc.time = phase.time;
                    cloneSrc.clip = audioclip;
                    cloneSrc.outputAudioMixerGroup = src.outputAudioMixerGroup;
                    cloneSrc.volume = volume;
                    // дополнительный компонент 
                    AudioStop astop = clone.AddComponent<AudioStop>();
                    astop.stopTime = stopTime; // глобальный момент уничтожения звука
                    astop.src = cloneSrc;
                    cloneSrc.Play();
                } else { // играть обычным образом, с прерыванием от следующего клипа
                    src.Play();
                }
            }
            return stopTime;
        }

		void OnDestroy () {
			audioclip.UnloadAudioData();
		}

    }

}
