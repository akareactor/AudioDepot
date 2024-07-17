using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KulibinSpace.AudioDepot {

	[System.Serializable]
	public class ActionPhase {
		public float time; // время старта
		public float duration; // продолжительность, если = 0, то играть до конца
	}

	// 19:56 08.02.2019 вешается на любой UI.Selectable объект
	// Поддерживает ховер и клик. 
	// Все звуки содержатся в одном клипе.
	// Если для каждого действия задано больше одной фазы, то фаза выбирается рандомно.
	// По-моему, просто и понятно.
	// 21:24 08.02.2019 отдельная поддержка состояний Toggle

	[RequireComponent(typeof(AudioSource))]
	public class AudioPlay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

		public AudioClip clip; // 19:37 08.02.2019 все варианты звуков должны быть в одном клипе.
		public ActionPhase[] pointerEnter = null;
		public ActionPhase[] pointerExit = null;
		public ActionPhase[] pointerClick = null;
		public ActionPhase[] toggleOn = null; // 21:22 08.02.2019 поддержка для переключателя
		public ActionPhase[] toggleOff = null; // 21:22 08.02.2019 поддержка для переключателя
		AudioSource source;
		float length;
		public float volume = 0.5f; // для планет на карте лучше разную громкость

		void Start() {
			source = GetComponent<AudioSource>();
			source.clip = clip;
		}

		public void PlayAction (ActionPhase[] phases) {
			if (phases != null && phases.Length > 0) {
				ActionPhase p = phases[Random.Range(0, phases.Length)];
				if (p.duration > 0)
					length = Time.realtimeSinceStartup + p.duration;
				else
					length = Time.realtimeSinceStartup + clip.length - p.time;
				source.time = p.time;
				source.Play();
			}
		}
		
		// When highlighted with mouse.
		public void OnPointerEnter(PointerEventData eventData) {
			PlayAction(pointerEnter);
		}

		public void OnPointerExit(PointerEventData eventData) {
			PlayAction(pointerExit);
		}
		
		public void OnPointerClick(PointerEventData pointerEventData) {
			Selectable selectable = pointerEventData.pointerPress.GetComponent<Selectable>();
			if (selectable && selectable.interactable) {
				PlayAction(pointerClick);
				if (selectable is Toggle) {
					Toggle t = selectable as Toggle;
					if (t.isOn) PlayAction(toggleOn); else PlayAction(toggleOff);
				}
			}
		}

		void Update() {
			if (source && source.isPlaying && Time.realtimeSinceStartup > length) source.Stop();
		}
		
	}
}