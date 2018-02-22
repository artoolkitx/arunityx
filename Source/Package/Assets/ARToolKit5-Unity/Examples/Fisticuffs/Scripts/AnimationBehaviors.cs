using UnityEngine;
using System.Collections;

public class AnimationBehaviors : MonoBehaviour {
	
	public AudioClip ding;
	public AudioClip beepCount;
	public AudioClip fanfare;
	private AudioSource audioSource = null;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
		if (null == audioSource) {
			audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
		}
	}

	void DeactivateSelf() {
		gameObject.SetActive(false);
	}
	
	void PlaySound(string whichSound) {
		if (whichSound == "ding") {
			audioSource.PlayOneShot(ding);
		} else if (whichSound == "beepCount") {
			audioSource.PlayOneShot(beepCount);
		} else if (whichSound == "fanfare") {
			audioSource.PlayOneShot(fanfare);
		} else {
			Debug.LogWarning("AnimationBehaviors::PlaySound - Sound \"" + whichSound + "\" does not exist!");
		}
	}
	
	void PlayMainAudioLoop() {
		audioSource.Play();
	}
	
	void StopAudio() {
		audioSource.Stop();
	}
	
	void TriggerGameStart() {
		if (null == FisticuffsController.Instance) {
			Debug.LogError("AnimationBehviors::TriggerGameStart - FisticuffsController.Instance not set. Is there one in the scene?");
			return;
		}
		FisticuffsController.Instance.GameStart();
	}
	
}
