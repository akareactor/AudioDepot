using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

namespace KulibinSpace.AudioDepot {

	// 9:33 25.09.2020 подключается к миксеру, в котором должны быть два снимка Mute и Unmute (имена в коде)
	// 10:16 20.07.2021 убрал управление громкостью миксера, т.к. это выходит за рамки обязанностей трека.
	// 22:59 20.07.2021 треку разрешено на своём уровне управлять громкостью, это общее место всех типов треков - как обычных, так и послойных
	
	public abstract class PlayMinstrelBase : MonoBehaviour {

		public AudioMixerSnapshot snapshotUnmute;
		public AudioMixerSnapshot snapshotMute;
		public bool blocked = false; // если true, блокирование всех операций

		[Tooltip("Задержка перед проигрыванием")]
		public float delay; // на старте могут быть задержки, поэтому нужно подбирать значения и сочетать их с fadeOutSeconds
		[Tooltip("Зацикливать всю композицию. Не действует, если в piece уже есть loop")]
		public bool loop;
		//[HideInInspector]
		public bool playing = false; // возможно, из-за PlayScheduled нельзя пользоваться флагом src.isPlaying. Этот флаг ставится в реализациях родителя

		void Start () {
			//print("Проверка последовательности появления объектов в сцене - PlayMinstrelBase " + gameObject.name);
		}

		public abstract void StopAudio ();
		public abstract void PlayImmediately (); // 12:49 31.12.2020 нельзя вызывать вне контекста снэпшота!

		public void PlaySnapshot (AudioMixerSnapshot snapshot, float transitionTime) {
			if (!blocked) {
				Play(); // ничего страшного не случится, даже если он уже играет
				snapshot.TransitionTo(transitionTime);
			}
		}

		// команды проигрывания определённых слоёв (на самом деле это просто установка громкости)
		public void Mute (float transitionTime) { PlaySnapshot(snapshotMute, transitionTime); }
		public void Unmute (float transitionTime) { PlaySnapshot(snapshotUnmute, transitionTime); }


		IEnumerator DelayedTransitionStop (float transitionTime) {
			yield return new WaitForSeconds(transitionTime);
			StopAudio();
		}

		// непосредственный запуск
		// 11:51 15.01.2021 новый трек включает сначала затихание, в конце которого текущий трек делает Stop, а потом увеличивает громкость.
		// текущий трек теперь не удаляется, это ответственность верхнего уровня
		public void PlayComposition () {
			if (!blocked) {
				PlayImmediately();
				//Unmute(transitionTime); // 13:03 31.12.2020 Вне охраны, чтобы можно было использовать для включения всех слоёв в PlayMinstrelDynamic
				// 22:40 19.07.2021 автоувеличение громкости ломает логику включения слоя поверх запущенного бэкграунда
			}
		}

		IEnumerator DelayedStart () {
			yield return new WaitForSeconds(delay);
			PlayComposition();
		}

		// реализация интерфейса, безусловный отложенный запуск, без проверки сцены
		public void Play () {
			if (!blocked) {
				if (!playing) StartCoroutine(DelayedStart());
			}
		}

		// 20:41 08.01.2021 вызывается из PlayMinstrelTracks при остановке в конце сцены
		public void Stop (float transitionTime) {
			if (!blocked) {
				if (playing) StartCoroutine(DelayedTransitionStop(transitionTime));
			}
		}
		
	}
}
	