/*
 *  ARTrackedCamera.cs
 *  ARToolKit for Unity
 *
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
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A class which directly associates an ARMarker with a Unity Camera object.
/// 
/// To get a list of foreground Camera objects, do:
///
///     List<Camera> foregroundCameras = new List<Camera>();
///     ARCamera[] arCameras = FindObjectsOfType<ARCamera>(); // (or FindObjectsOfType(typeof(ARCamera)) as ARCamera[])
///     foreach (ARCamera arc in arCameras) {
///         foregroundCameras.Add(arc.gameObject.camera);
///     }
/// </summary>
/// 
[RequireComponent(typeof(Transform))]   // A Transform is required to update the position and orientation from tracking
[ExecuteInEditMode]                     // Run in the editor so we can keep the scale at 1
public class ARTrackedCamera : ARCamera
{
	private const string LogTag = "ARTrackedCamera: ";

	public float secondsToRemainVisible = 0.0f;		// How long to remain visible after tracking is lost (to reduce flicker)

	[NonSerialized]
	protected int cullingMask = -1;					// Correct culling mask for content (set to 0 when not visible)

	private bool lastArVisible = false;
	
	// Private fields with accessors.
	[SerializeField]
	private string _markerTag = "";					// Unique tag for the marker to get tracking from
	
	public string MarkerTag
	{
		get
		{
			return _markerTag;
		}
		
		set
		{
			_markerTag = value;
			_marker = null;
		}
	}
	
	// Return the marker associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public override ARMarker GetMarker()
	{
		if (_marker == null) {
			// Locate the marker identified by the tag
			ARMarker[] ms = FindObjectsOfType<ARMarker>();
			foreach (ARMarker m in ms) {
				if (m.Tag == _markerTag) {
					_marker = m;
					break;
				}
			}
		}
		return _marker;
	}

	public virtual void Start()
	{
		// Store the camera's initial culling mask. When the marker is tracked, this mask will be used
		// so that the virtual objects are rendered. When tracking is lost, 0 will be used, so that no 
		// objects are displayed.
		if (cullingMask == -1) {
			cullingMask = this.gameObject.GetComponent<Camera>().cullingMask;
		}
	}

	protected override void ApplyTracking()
	{
		if (arVisible || (timeLastUpdate - timeTrackingLost < secondsToRemainVisible)) {
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = cullingMask;
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerFound", GetMarker(), SendMessageOptions.DontRequireReceiver);
			}
			transform.localPosition = arPosition; // TODO: Change to transform.position = PositionFromMatrix(origin.transform.localToWorldMatrix * pose) etc;
			transform.localRotation = arRotation;
			if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerTracked", GetMarker(), SendMessageOptions.DontRequireReceiver);
		} else {
			if (arVisible != lastArVisible) {
				this.gameObject.GetComponent<Camera>().cullingMask = 0;
				if (eventReceiver != null) eventReceiver.BroadcastMessage("OnMarkerLost", GetMarker(), SendMessageOptions.DontRequireReceiver);
			}
		}
		lastArVisible = arVisible;
	}

}
