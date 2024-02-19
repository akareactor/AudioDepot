using UnityEngine;

// прикрепляется к отделяемому клипу в AudioDepot и автоматически прекращает проигрывание.

namespace KulibinSpace.AudioDepot {

	public class AudioStop : MonoBehaviour {

		public float stopTime;
		public AudioSource src;
		public bool autoDestroy = true;

		void Update() {
			if (Time.realtimeSinceStartup > stopTime) {
				if (src && src.isPlaying) src.Stop();
				if (autoDestroy) Destroy(gameObject);
			}
		}

	}

}
