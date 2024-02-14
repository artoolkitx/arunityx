using UnityEngine;
using System.Collections;

public class LookAtCameraFisticuffs : MonoBehaviour {

	private GameObject theCamera = null;

	void OnEnable () {
		FindCamera();
	}

	void FindCamera () {
		theCamera = GameObject.FindWithTag("MainCamera");
	}
	
	void Update () {
		RotateMe();
	}
	
	void RotateMe() {
		if(theCamera == null) {
			FindCamera();
		}

		Transform lookAtMe = theCamera.transform;
		if (transform.rotation != lookAtMe.rotation) {
			Quaternion currentRotation = transform.rotation;
			Quaternion newRotation = lookAtMe.rotation;
			transform.rotation = Quaternion.Slerp(currentRotation, newRotation, Time.deltaTime * 5.0f);
		}
	}
	
	
}
