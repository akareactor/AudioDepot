using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KulibinSpace.MessageBus;

namespace KulibinSpace.AudioDepot {

	[System.Serializable]
	public class AudioclipPhase {
		public float time; // время старта
		public float duration; // продолжительность, если = 0, то играть до конца
	}

	[System.Serializable]
	public class NamedClip {

		public bool aside = false; // выделить клип в отдельный объект, который будет звучать до конца независимо от хоста
		public string name;
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

	}

	//10:00 09.02.2019 Идея объединить часто используемые звуковые клипы в одном депо и посылать сюда именованные сигналы
	// 20:06 19.07.2019 подключил к глобальной шине сообщений. К ней могут быть подключено любое количество АудиоДепо, с наборами клипов. 
	// Кто угодно может послать сигнал с именем клипа. Желательно, чтобы имена клипов из разных депо не пересекались.

	// 2023-05-02 Надо делать "отделяемый" клип, чтоб после запуска звучал независимо.

	public class AudioDepot : MonoBehaviour {

		public bool persistent = true;
		public AudioSource src; // может быть установлена заранее, а может, и соседний компонент в этом же игробе.
		public NamedClip[] clips;
		float stopTime;
		public bool debug = false;
		static AudioDepot instance; // персистентность
		public GameMessageString onPlayAudio;

		void Awake () {
			if (persistent) {
				if (instance == null) {
					DontDestroyOnLoad(gameObject);
					instance = this;
				} else if (instance != this) {
					print("AudioDepot wrong instance mark");
					Destroy(gameObject);
				}
			}
			if (!src) src = GetComponent<AudioSource>();
		}
		
		void OnEnable () {
			onPlayAudio.message += Play;
		}

		void OnDisable () {
			onPlayAudio.message -= Play;
		}
		
		NamedClip Find (string name) {
			NamedClip ret = null;
			int i = 0;
			while (ret == null && i < clips.Length) {
				if (clips[i].name == name) ret = clips[i];
				i += 1;
			}
		return ret;
		}

        // 2024-07-17 13:31:31 некий задел на проигрывание скриптуемого.
        public void Play (PhasedAudioClip phasedAudioClip) {
			if ((src && !src.isPlaying) || !src) { // охрана от слишком быстрого переключения звуков
                if (phasedAudioClip != null) {
                    stopTime = phasedAudioClip.Play(src);
                }
            }
        }
		
		// проиграть именованный клип
		// Рандомная фаза из нескольких указанных
		public void Play (string name) {
			if ((src && !src.isPlaying) || !src) { // охрана от слишком быстрого переключения звуков
				NamedClip clip = Find(name);
				//if (clip != null && clip.audioclip != null && clip.phases != null && clip.phases.Length > 0) {
				if (clip != null && clip.audioclip != null) {
					src.clip = clip.audioclip;
					AudioclipPhase phase = clip.Phase();
					stopTime = Time.realtimeSinceStartup + phase.duration;
					src.time = phase.time;
					src.volume = clip.volume;
					// независимый объект, при необходимости
					if (clip.aside) {
						GameObject clone = new GameObject(clip.name + "_aside");
						//AudioSource cloneSrc = Abstract.CopyComponent<AudioSource>(src, clone);
						// поскольку CopyComponent копирует и Local File Identifier, пришлось от него отказаться.
						AudioSource cloneSrc = clone.AddComponent<AudioSource>();
						cloneSrc.time = phase.time;
						cloneSrc.clip = clip.audioclip;
						cloneSrc.outputAudioMixerGroup = src.outputAudioMixerGroup;
						cloneSrc.volume = clip.volume;
						// дополнительный компонент 
						AudioStop astop = clone.AddComponent<AudioStop>();
						astop.stopTime = stopTime; // глобальный момент уничтожения звука
						astop.src = cloneSrc;
						cloneSrc.Play();
					} else { // играть обычным образом, с прерыванием от следующего клипа
						src.Play();
					}
				} else {
					if (debug) print("Клип " + name + " не найден");
				}
			}
		}
		
		void Update() {
		if (src && src.isPlaying && Time.realtimeSinceStartup > stopTime) src.Stop();
		}
		
		void OnDestroy () {
			//print("AudioDepot destroy");
			foreach (NamedClip clip in clips) clip.audioclip.UnloadAudioData();
		}
	}

}
