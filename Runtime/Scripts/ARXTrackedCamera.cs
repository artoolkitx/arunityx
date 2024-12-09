/*
 *  ARXTrackedCamera.cs
 *  artoolkitX for Unity
 *
 *  This file is part of artoolkitX for Unity.
 *
 *  artoolkitX for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artoolkitX for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artoolkitX for Unity.  If not, see <http://www.gnu.org/licenses/>.
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
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A class which directly associates an ARXTrackable with a Unity Camera object.
///
/// To get a list of foreground Camera objects, do:
///
///     List<Camera> foregroundCameras = new List<Camera>();
///     ARXCamera[] arCameras = FindObjectsOfType<ARXCamera>(); // (or FindObjectsOfType(typeof(ARXCamera)) as ARXCamera[])
///     foreach (ARXCamera arc in arCameras) {
///         foregroundCameras.Add(arc.gameObject.camera);
///     }
/// </summary>
///
[RequireComponent(typeof(Transform))]   // A Transform is required to update the position and orientation from tracking
[ExecuteInEditMode]                     // Run in the editor so we can keep the scale at 1
public class ARXTrackedCamera : ARXCamera
{
	private const string LogTag = "ARXTrackedCamera: ";

	[NonSerialized]
	protected int cullingMask = -1;					// Correct culling mask for content (set to 0 when not visible)

	[SerializeField]
	[Tooltip("Set this to the same value defined in the ARXTrackable object that defines this camera's pose.")]
	private string _trackableTag = "";                  // Unique tag for the marker to get tracking from
	public string TrackableTag
	{
		get
		{
			return _trackableTag;
		}

		set
		{
			_trackableTag = value;
			_trackable = null;
		}
	}

	private bool lastArVisible = false;
	[Tooltip("The number of seconds this object should remain visible when the associated ARXTrackable object is no longer visible.")]
	public float secondsToRemainVisible = 0.0f;     // How long to remain visible after tracking is lost (to reduce flicker)

	public ARXUnityEventUnityObject OnTrackedCameraFound = new ARXUnityEventUnityObject();
	public ARXUnityEventUnityObject OnTrackedCameraTracked = new ARXUnityEventUnityObject();
	public ARXUnityEventUnityObject OnTrackedCameraLost = new ARXUnityEventUnityObject();
	[Tooltip("Legacy event mechanism using Unity messaging. Event methods will be called on the referenced object and all children.")]
	/// </summary>
	public GameObject eventReceiver;

	// Return the marker associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public override ARXTrackable GetTrackable()
	{
		if (_trackable == null) {
			// Locate the marker identified by the tag
			ARXTrackable[] ms = FindObjectsByType<ARXTrackable>(FindObjectsSortMode.None);
			foreach (ARXTrackable m in ms) {
				if (m.Tag == _trackableTag) {
					_trackable = m;
					break;
				}
			}
		}
		return _trackable;
	}

	public virtual void Start()
	{
		// Store the camera's initial culling mask. When the marker is tracked, this mask will be used
		// so that the virtual objects are rendered. When tracking is lost, 0 will be used, so that no
		// objects are displayed.
		if (cullingMask == -1) {
			cullingMask = gameObject.GetComponent<Camera>().cullingMask;
		}
	}

	protected override void ApplyTracking()
	{
		if (arVisible || (timeLastUpdate - timeTrackingLost < secondsToRemainVisible))
		{
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
				OnTrackedCameraFound.Invoke(this);
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableFound", GetTrackable(), SendMessageOptions.DontRequireReceiver);
			}
			transform.localPosition = arPosition; // TODO: Change to transform.position = PositionFromMatrix(origin.transform.localToWorldMatrix * pose) etc;
			transform.localRotation = arRotation;
			OnTrackedCameraTracked.Invoke(this);
			if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableTracked", GetTrackable(), SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = 0;
                OnTrackedCameraLost.Invoke(this);
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableLost", GetTrackable(), SendMessageOptions.DontRequireReceiver);
			}
		}
		lastArVisible = arVisible;
	}

}
