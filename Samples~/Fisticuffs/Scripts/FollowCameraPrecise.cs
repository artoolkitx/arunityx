using UnityEngine;
using System.Collections;

public class FollowCameraPrecise : MonoBehaviour {
		
	void LateUpdate () {
		GameObject theCamera = GameObject.Find("UI Camera");
		
		transform.position = theCamera.transform.position;
		transform.rotation = theCamera.transform.rotation;
	}
}
