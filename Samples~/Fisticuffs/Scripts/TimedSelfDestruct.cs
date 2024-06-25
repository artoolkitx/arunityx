using UnityEngine;
using System.Collections;

public class TimedSelfDestruct : MonoBehaviour {
	
	private float storedTime;
	public float timeBeforeDestroy;
	
	// Use this for initialization
	void Start () {
		storedTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > storedTime + timeBeforeDestroy) {
			Destroy(gameObject);
		}
	}
}
