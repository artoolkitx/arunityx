/*
 *  ARXTrackedObject.cs
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
 *  Author(s): Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Transform))]
[ExecuteInEditMode]
public class ARXTrackedObject : MonoBehaviour
{
	private const string LogTag = "ARXTrackedObject: ";

	private ARXOrigin _origin = null;
	private ARXCamera _camera = null;				// When no ARXOrigin is in the scene, this will be set to point to the ARXCamera to take as the pose reference. Note that for stereo ARXCameras, this will always be the left camera.
	private ARXTrackable _trackable = null;

	[SerializeField]
	[Tooltip("Set this to the same value defined in the ARXTrackable object that defines this object's pose.")]
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

	private bool _visible = false;                   // Current visibility from tracking
	private float _timeTrackingLost = 0;             // Time when tracking was last lost
	[Tooltip("The number of seconds this object should remain visible when the associated ARXTrackable object is no longer visible.")]
	public float secondsToRemainVisible = 0.0f;		// How long to remain visible after tracking is lost (to reduce flicker)
	private bool _visibleOrRemain = false;           // Whether to show the content (based on above variables)

	public ARXUnityEventUnityObject OnTrackedObjectFound;
	public ARXUnityEventUnityObject OnTrackedObjectTracked;
	public ARXUnityEventUnityObject OnTrackedObjectLost;

    [Tooltip("If set, the children of this GameObject will be activated when the trackable is found. Normally set, but you might wish to clear if you want to manually control child object activation.")]
	public bool OnTrackedObjectLostDeactivateChildren = true;
    [Tooltip("If set, the children of this GameObject will be deactivated when the trackable is lost. Normally set, but you might wish to clear if you want to manually control child object deactivation.")]
	public bool OnTrackedObjectFoundActivateChildren = true;

	[Tooltip("Legecy event mechanism using Unity messaging. Event methods will be called on the referenced object and all children.")]
	public GameObject eventReceiver;

	// Return the trackable associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual ARXTrackable GetTrackable()
	{
		if (_trackable == null) {
            // Locate the trackable identified by the tag
			ARXTrackable[] ms = FindObjectsOfType<ARXTrackable>();
			foreach (ARXTrackable m in ms) {
				if (m.Tag == _trackableTag) {
					_trackable = m;
					break;
				}
			}
		}
		return _trackable;
	}

	// Return the origin associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual ARXOrigin GetOrigin()
	{
		if (_origin == null) {
			// Locate the origin in parent.
			_origin = this.gameObject.GetComponentInParent<ARXOrigin>();
		}
		return _origin;
	}

	// Return the camera associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual ARXCamera GetCamera()
	{
		if (_camera == null)
		{
			ARXCamera[] cs = FindObjectsOfType<ARXCamera>();
			foreach (ARXCamera c in cs)
			{
				if (!c.Stereo || c.StereoEye == ARXCamera.ViewEye.Left)
				{
					_camera = c;
					break;
				}
			}
		}
		return _camera;
	}

	void Start()
	{
		//ARXController.Log(LogTag + "Start()");

		if (Application.isPlaying)
		{
			// In Player, set initial visibility to not visible.
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
		}
		else
		{
			// In Editor, set initial visibility to visible.
			for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
		}
	}

	// Note that [DefaultExecutionOrder] is set on ARXTrackable to ensure it has updated before we try and use the transformation.
	void Update()
	{
		// Update tracking if we are running in the Player.
		if (!Application.isPlaying)
        {
			return;
        }

		// Sanity check, make sure we have an ARXTrackable assigned.
		ARXTrackable trackable = GetTrackable();
		if (trackable == null)
		{
			_visible = _visibleOrRemain = false;
			return;
		}

		ARXOrigin origin = GetOrigin();
		if (origin != null)
		{
			ARXTrackable baseTrackable = origin.GetBaseTrackable();
			if (baseTrackable != null && trackable.Visible)
			{
				VisibleInternal(trackable == baseTrackable ? origin.transform.localToWorldMatrix : (origin.transform.localToWorldMatrix * baseTrackable.TransformationMatrix.inverse * trackable.TransformationMatrix));
			}
			else /* (baseTrackable == null || !trackable.Visible) */
			{
				NotVisibleInternal();
			}
		}
		else
        {
			ARXCamera c = GetCamera();
			if (!c)
            {
				ARXController.LogError("Error: no ARXOrigin and no ARXCamera in scene.", this);
				_visible = _visibleOrRemain = false;
				return;
            }

			if (trackable.Visible)
            {
				VisibleInternal(c.transform.localToWorldMatrix * trackable.TransformationMatrix);
			}
			else
            {
				NotVisibleInternal();
            }
		}
	}

	private void VisibleInternal(Matrix4x4 pose)
	{
		transform.localScale = Vector3.one; // Local scale is always 1 for now
		transform.position = ARXUtilityFunctions.PositionFromMatrix(pose);
		transform.rotation = ARXUtilityFunctions.QuaternionFromMatrix(pose);

		if (!_visible)
		{
			// Trackable was hidden but now is visible.
			_visible = _visibleOrRemain = true;
			OnTrackedObjectFound.Invoke(this);
			if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableFound", _trackable, SendMessageOptions.DontRequireReceiver);
            if (OnTrackedObjectFoundActivateChildren)
			{
			    for (int i = 0; i < this.transform.childCount; i++)
			    {
			        this.transform.GetChild(i).gameObject.SetActive(true);
			    }
			}
		}

		OnTrackedObjectTracked.Invoke(this);
		if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableTracked", _trackable, SendMessageOptions.DontRequireReceiver);
	}

	private void NotVisibleInternal()
	{
		float timeNow = _visible || _visibleOrRemain ? Time.realtimeSinceStartup : 0.0f;

		if (_visible)
		{
			// Trackable was visible but now is hidden.
			_visible = false;
			_timeTrackingLost = timeNow;
		}

		if (_visibleOrRemain && (timeNow - _timeTrackingLost >= secondsToRemainVisible))
		{
			_visibleOrRemain = false;
			OnTrackedObjectLost.Invoke(this);
			if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableLost", _trackable, SendMessageOptions.DontRequireReceiver);
			if (OnTrackedObjectLostDeactivateChildren)
			{
			    for (int i = 0; i < this.transform.childCount; i++)
			    {
			        this.transform.GetChild(i).gameObject.SetActive(false);
			    }
			}
		}
	}

}

