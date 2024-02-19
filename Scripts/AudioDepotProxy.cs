using System.Collections;
using System.Collections.Generic;
using KulibinSpace.MessageBus;
using UnityEngine;

// 10:45 09.02.2019 прокси для AudioDepot, экземпляров которых может быть в сцене несколько, собранных для разных целей.
// В этот прокси идут сигналы, например, от компонента, установленного на префабе. А прокси уже транслирует в глобальную шину сообщений

namespace KulibinSpace.AudioDepot {

	public class AudioDepotProxy : MonoBehaviour {

		public GameMessageString playAudio;

		public void Play (string name) {
			if (!System.String.IsNullOrEmpty(name)) playAudio?.Invoke(name);
		}
		
	}
	
}
