using UnityEngine;
using UnityEngine.UI;
using KulibinSpace.AudioDepot;

public class AudioButton : MonoBehaviour {

	public Button audioButton;
	public UnityEngine.Audio.AudioMixer master;
	public UnityEngine.Audio.AudioMixerSnapshot mute, normal;

	private void OnEnable () {
		UpdateAudioStatus();
	}

	void UpdateAudioStatus () {
		float volume;
		if (AudioPrefs.audioOn) {
			volume = AudioPrefs.soundVolume;
			audioButton.image.color = Color.white;
			normal.TransitionTo(0.2f);
		} else {
			volume = 0;
			audioButton.image.color = Color.red;
			mute.TransitionTo(0.2f);
		}
        master.SetFloat("masterVolume", Mathf.Log10(volume) * 20);
	}

	public void Switch () {
		AudioPrefs.audioOn = !AudioPrefs.audioOn;
		UpdateAudioStatus();
		AudioPrefs.SaveSettings();
	}

}
