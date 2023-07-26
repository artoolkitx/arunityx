using UnityEngine;
using System.Collections;

public class TimedSelfDeactivate : MonoBehaviour {

	private float storedTime;
	public float timeBeforeDeactivate;
	
	// Use this for initialization
	void OnEnable () {
		storedTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > storedTime + timeBeforeDeactivate) {
			gameObject.SetActive(false);
		}
	}
}
