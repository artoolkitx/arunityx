/*
 *  ARTrackedObject.cs
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
public class ARTrackedObject : MonoBehaviour
{
	private const string LogTag = "ARTrackedObject: ";

	private AROrigin _origin = null;
    private ARTrackable _trackable = null;

	[SerializeField]
	[Tooltip("Set this to the same value defined in the ARTrackable object that defines this object's pose.")]
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

	private bool visible = false;                   // Current visibility from tracking
	private float timeTrackingLost = 0;             // Time when tracking was last lost
	[Tooltip("The number of seconds this object should remain visible when the associated ARTrackable object is no longer visible.")]
	public float secondsToRemainVisible = 0.0f;		// How long to remain visible after tracking is lost (to reduce flicker)
	private bool visibleOrRemain = false;           // Whether to show the content (based on above variables)

	public ARUnityEventUnityObject OnTrackedObjectFound;
	public ARUnityEventUnityObject OnTrackedObjectTracked;
	public ARUnityEventUnityObject OnTrackedObjectLost;

	[Tooltip("Legecy event mechanism using Unity messaging. Event methods will be called on the referenced object and all children.")]
	public GameObject eventReceiver;

	// Return the trackable associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual ARTrackable GetTrackable()
	{
		if (_trackable == null) {
            // Locate the trackable identified by the tag
			ARTrackable[] ms = FindObjectsOfType<ARTrackable>();
			foreach (ARTrackable m in ms) {
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
	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			// Locate the origin in parent.
			_origin = this.gameObject.GetComponentInParent<AROrigin>(); // Unity v4.5 and later.
		}
		return _origin;
	}

	void Start()
	{
		//ARController.Log(LogTag + "Start()");

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

	// Note that [DefaultExecutionOrder] is used on ARTrackable to ensure the base ARTrackable has updated before we try and use the transformation.
	void Update()
	{
		// Local scale is always 1 for now
		transform.localScale = Vector3.one;
		
		// Update tracking if we are running in the Player.
		if (Application.isPlaying)
		{
			// Sanity check, make sure we have an AROrigin in parent hierachy.
			AROrigin origin = GetOrigin();
			if (origin == null) {
				//visible = visibleOrRemain = false;
			}
			else
			{
				// Sanity check, make sure we have an ARTrackable assigned.
                ARTrackable trackable = GetTrackable();
				if (trackable == null)
				{
					//visible = visibleOrRemain = false;
				}
				else
				{
					// Note the current time
					float timeNow = Time.realtimeSinceStartup;
					
                    ARTrackable baseTrackable = origin.GetBaseTrackable();
					if (baseTrackable != null && trackable.Visible)
					{
						if (!visible)
						{
							// Trackable was hidden but now is visible.
							visible = visibleOrRemain = true;
							OnTrackedObjectFound.Invoke(this);
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableFound", trackable, SendMessageOptions.DontRequireReceiver);

							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(true);
						}

                        Matrix4x4 pose;
                        if (trackable == baseTrackable)
						{
                            // If this marker is the base, no need to take base inverse etc.
                            pose = origin.transform.localToWorldMatrix;
                        }
						else
						{
						    pose = (origin.transform.localToWorldMatrix * baseTrackable.TransformationMatrix.inverse * trackable.TransformationMatrix);
						}
						transform.position = ARUtilityFunctions.PositionFromMatrix(pose);
						transform.rotation = ARUtilityFunctions.QuaternionFromMatrix(pose);

						OnTrackedObjectTracked.Invoke(this);
						if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableTracked", trackable, SendMessageOptions.DontRequireReceiver);

					}
					else
					{
						if (visible)
						{
							// Trackable was visible but now is hidden.
							visible = false;
							timeTrackingLost = timeNow;
						}

						if (visibleOrRemain && (timeNow - timeTrackingLost >= secondsToRemainVisible))
						{
							visibleOrRemain = false;
							OnTrackedObjectLost.Invoke(this);
							if (eventReceiver != null) eventReceiver.BroadcastMessage("OnTrackableLost", trackable, SendMessageOptions.DontRequireReceiver);
							for (int i = 0; i < this.transform.childCount; i++) this.transform.GetChild(i).gameObject.SetActive(false);
						}
					}
				} // marker

			} // origin
		} // Application.isPlaying

	}

}

