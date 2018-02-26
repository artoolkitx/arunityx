/*
 *  ARTransitionalCamera.cs
 *  ARToolKit for Unity
 *
 *  This file is part of ARToolKit for Unity.
 *
 *  ARToolKit for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ARToolKit for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with ARToolKit for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Julian Looser, Philip Lamb
 *
 */

using System;
using System.Collections;//.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Transform))]   // A Transform is required to update the position and orientation from tracking
[ExecuteInEditMode]                     // Run in the editor so we can keep the scale at 1
public class ARTransitionalCamera : ARTrackedCamera
{

    public Vector3 vrTargetPosition;    // In camera parent frame.
	public Quaternion vrTargetRotation; // In camera parent frame.

    public GameObject targetObject;
    public float transitionAmount = 0.0f;
	public float movementRate = 1.389f; // Gentle walking speed, 1.389 m/s = 5 km/hr = 3.107 miles/hr.

	// Variables for navigation in the VR environment.
    private float vrObserverAzimuth = 0.0f;
    private float vrObserverElevation = 0.0f;
	private Vector3 vrObserverOffset = Vector3.zero; // +x is right, +y is up, +z is forward.

    public bool automaticTransition = false;
    public float automaticTransitionDistance = 0.3f;

    IEnumerator DoTransition(bool flyIn)
    {
		ARController artoolkit = Component.FindObjectOfType(typeof(ARController)) as ARController;

        float transitionSpeed = flyIn ? 1.0f : -1.0f;
        bool transitioning = true;

        while (transitioning) {
            transitionAmount += transitionSpeed * Time.deltaTime;

            if (transitionAmount > 1.0f) {
                transitionAmount = 1.0f;
                transitioning = false;
            }

            if (transitionAmount < 0.0f) {
                transitionAmount = 0.0f;
                transitioning = false;
            }
       
            if (artoolkit != null) artoolkit.SetVideoAlpha(1.0f - transitionAmount);

            yield return null;
        }

        print("Transition complete");
    }

    public void transitionIn()
    {
        StopCoroutine("DoTransition");
        StartCoroutine("DoTransition", true);

        this.GetComponent<AudioSource>().Play();
    }

    public void transitionOut()
    {
        StopCoroutine("DoTransition");
        StartCoroutine("DoTransition", false);

        this.GetComponent<AudioSource>().Play();
    }

    public override void Start()
    {
		base.Start();

		Matrix4x4 targetInWorldFrame = targetObject.transform.localToWorldMatrix;
		Matrix4x4 targetInCameraFrame = this.gameObject.GetComponent<Camera>().transform.parent.worldToLocalMatrix * targetInWorldFrame;
		vrTargetPosition = ARUtilityFunctions.PositionFromMatrix(targetInCameraFrame);
		vrTargetRotation = ARUtilityFunctions.QuaternionFromMatrix(targetInCameraFrame);

		vrObserverAzimuth = vrObserverElevation = 0.0f; // VR mode starts pointing in direction specified by the axes of the target.
		vrObserverOffset = Vector3.zero;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) {
            transitionIn();
        }

        if (Input.GetKeyDown(KeyCode.O)) {
            transitionOut();
        }

        if (automaticTransition) {
            if (arVisible) {

                if (arPosition.magnitude < automaticTransitionDistance) {

                    if (transitionAmount != 1) transitionIn();

                }
            }
        }

    }

    public void OnPreRender()
    {
        GL.ClearWithSkybox(false, this.gameObject.GetComponent<Camera>());

        this.GetComponent<Skybox>().material.SetColor("_Tint", new Color(1, 1, 1, transitionAmount));
    }

    protected override void ApplyTracking()
    {
        Vector2 look = Vector2.zero;
		Vector3 move = Vector3.zero;

		if (SystemInfo.deviceType == DeviceType.Handheld) {
			if (Input.touchCount == 1) {
	            Touch touch = Input.GetTouch(0);
				look = touch.deltaPosition;           
	        } else if (Input.touchCount == 2) {
	            if (transitionAmount <= 0) transitionIn();
	            if (transitionAmount >= 1) transitionOut();
	        }
		} else {
			look.x = -Input.GetAxis("Mouse X"); // Change in azimuth.
			look.y = -Input.GetAxis("Mouse Y"); // Change in elevation.
			move.x = movementRate * Time.deltaTime * Input.GetAxis("Horizontal");
			move.y = 0.0f;
			move.z = movementRate * Time.deltaTime * Input.GetAxis("Vertical");

            if (Input.GetMouseButton(0)) transitionIn();
            if (Input.GetMouseButton(1)) transitionOut();
        }

		// Adjust azimuth and elevation, letting azimuth wrap around, and clamping elevation in range [-90, 90].
		vrObserverAzimuth -= look.x * 0.5f;
		if (vrObserverAzimuth >= 360.0f) vrObserverAzimuth -= 360.0f;
		else if (vrObserverAzimuth < 0.0f) vrObserverAzimuth += 360.0f;
		vrObserverElevation += look.y * 0.5f;
		if (vrObserverElevation > 90.0f) vrObserverElevation = 90.0f;
		else if (vrObserverElevation < -90.0f) vrObserverElevation = -90.0f;
		Quaternion vrObserverDirection = Quaternion.Euler(vrObserverElevation, vrObserverAzimuth, 0.0f);

		// Adjust offset, making forward apply in the direction the observer is facing.
		vrObserverOffset += vrObserverDirection * move;

		Vector3 vrPosition = vrTargetPosition + vrTargetRotation * vrObserverOffset;
		Quaternion vrRotation = vrTargetRotation * vrObserverDirection; 
 
        if (transitionAmount < 1) {
            if (arVisible) {
				transform.localPosition = Vector3.Lerp(arPosition, vrPosition, transitionAmount);
				transform.localRotation = Quaternion.Slerp(arRotation, vrRotation, transitionAmount);
                this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
            } else {
                this.gameObject.GetComponent<Camera>().cullingMask = 0;
            }
        } else {
            this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
			transform.localPosition = vrPosition;
			transform.localRotation = vrRotation;
		}
    }

    void OnGUI()
    {
		if (SystemInfo.deviceType == DeviceType.Handheld) {

        } else {

            /*if (GUI.Button(new Rect(100, 100, 50, 50), "Fly"))
            {
                if (transitionAmount == 0) transitionIn();
                if (transitionAmount == 1) transitionOut();
            }*/
        }
    }
       

}

