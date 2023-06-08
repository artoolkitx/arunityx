/*
 *  ARCamera.cs
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
using UnityEngine;

/// <summary>
/// A class which links an ARCamera to any available ARMarker via an AROrigin object.
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
[RequireComponent(typeof(Camera))]      // A Camera is required.
[ExecuteInEditMode]                     // Run in the editor so we can keep the scale at 1
public class ARCamera : MonoBehaviour
{
	private const string LogTag = "ARCamera: ";

	public enum ContentMode
	{
		Stretch,
		Fit,
		Fill,
		OneToOne
	}

	public readonly static Dictionary<ContentMode, string> ContentModeNames = new Dictionary<ContentMode, string>
	{
		{ContentMode.Stretch, "Stretch"},
		{ContentMode.Fit, "Fit"},
		{ContentMode.Fill, "Fill"},
		{ContentMode.OneToOne, "1:1"},
	};

	[SerializeField]
	private ContentMode currentCameraContentMode = ContentMode.Fill;
	public ContentMode CameraContentMode
	{
		get => currentCameraContentMode;
		set
		{
			if (currentCameraContentMode != value)
			{
				currentCameraContentMode = value;
				//if (_running)
				{
					//ConfigureViewports();
				}
			}
		}
	}
	public ARController.ARW_H_ALIGN CameraContentHAlign = ARController.ARW_H_ALIGN.ARW_H_ALIGN_CENTRE;
	public ARController.ARW_V_ALIGN CameraContentVAlign = ARController.ARW_V_ALIGN.ARW_V_ALIGN_CENTRE;
	public bool ContentRotate90 = false;
	public bool ContentFlipH = false;
	public bool ContentFlipV = false;


	public enum ViewEye
	{
		Left = 1,
		Right = 2,
	}
	
    public enum OpticalCalibrationMode
    {
        ManuallySpecified = 0,
        ARXOpticalParametersFile
    }

	/*public enum STEREO_DISPLAY_MODE {
		STEREO_DISPLAY_MODE_INACTIVE = 0,           // Stereo display not active.
		STEREO_DISPLAY_MODE_DUAL_OUTPUT,            // Two outputs, one displaying the left view, and one the right view.  Blue-line optional.
		STEREO_DISPLAY_MODE_QUADBUFFERED,           // One output exposing both left and right buffers, with display mode determined by the hardware implementation. Blue-line optional.
		STEREO_DISPLAY_MODE_FRAME_SEQUENTIAL,       // One output, first frame displaying the left view, and the next frame the right view. Blue-line optional.
		STEREO_DISPLAY_MODE_SIDE_BY_SIDE,           // One output. Two normally-proportioned views are drawn in the left and right halves.
		STEREO_DISPLAY_MODE_OVER_UNDER,             // One output. Two normally-proportioned views are drawn in the top and bottom halves.
		STEREO_DISPLAY_MODE_HALF_SIDE_BY_SIDE,      // One output. Two views, scaled to half-width, are drawn in the left and right halves
		STEREO_DISPLAY_MODE_OVER_UNDER_HALF_HEIGHT, // One output. Two views, scaled to half-height, are drawn in the top and bottom halves.
		STEREO_DISPLAY_MODE_ROW_INTERLACED,         // One output. Two views, normally proportioned, are interlaced, with even numbered rows drawn from the first view and odd numbered rows drawn from the second view.
		STEREO_DISPLAY_MODE_COLUMN_INTERLACED,      // One output. Two views, normally proportioned, are interlaced, with even numbered columns drawn from the first view and odd numbered columns drawn from the second view.
		STEREO_DISPLAY_MODE_CHECKERBOARD,           // One output. Two views, normally proportioned, are hatched. On even numbered rows, even numbered columns are drawn from the first view and odd numbered columns drawn from the second view. On odd numbered rows, this is reversed.
		STEREO_DISPLAY_MODE_ANAGLYPH_RED_BLUE,      // One output. Both views are rendered into the same buffer, the left view drawn only in the red channel and the right view only in the blue channel.
		STEREO_DISPLAY_MODE_ANAGLYPH_RED_GREEN,     // One output. Both views are rendered into the same buffer, the left view drawn only in the red channel and the right view only in the green channel.
	}*/
	
	private AROrigin _origin = null;
	protected ARTrackable _trackable = null;                // Instance of trackable that will be used as the origin for the camera pose.

	[NonSerialized]
	protected ARController arController;
	[NonSerialized]
	protected Camera cam;
	[NonSerialized]
	protected Vector3 arPosition = Vector3.zero;	// Current 3D position from tracking
	[NonSerialized]
	protected Quaternion arRotation = Quaternion.identity; // Current 3D rotation from tracking
	[NonSerialized]
	protected bool arVisible = false;				// Current visibility from tracking
	[NonSerialized]
	protected float timeLastUpdate = 0;				// Time when tracking was last updated.
	[NonSerialized]
	protected float timeTrackingLost = 0;			// Time when tracking was last lost.
	
	// Stereo settings.
	public bool Stereo = false;
	public ViewEye StereoEye = ViewEye.Left;
	
	// Optical settings.
	public bool Optical = false;
    public OpticalCalibrationMode OpticalCalibrationMode0 = OpticalCalibrationMode.ManuallySpecified;
	private bool opticalSetupOK = false;
	public int OpticalParamsFilenameIndex = 0;
	public string OpticalParamsFilename = "";
	public byte[] OpticalParamsFileContents = new byte[0]; // Set by the Editor.
	public float OpticalEyeLateralOffsetRight = 0.0f;
	private Matrix4x4 opticalViewMatrix; // This transform expresses the position and orientation of the physical camera in eye coordinates.


	void OnEnable()
	{
		cam = gameObject.GetComponent<Camera>();
		arController = ARController.Instance;
		if (!arController)
        {
			Debug.LogError("ARController.Instance is NULL.");
        }
		else
        {
			arController.onVideoStarted.AddListener(OnVideoStarted);
		}
	}

	void OnDisable()
	{
		if (!arController)
		{
			Debug.LogError("ARController.Instance is NULL.");
		}
		else
        {
			arController.onVideoStarted.RemoveListener(OnVideoStarted);
			arController = null;
		}
		cam = null;
	}

	public void OnVideoStarted()
    {
		// A perspective projection matrix from the tracker
		cam.orthographic = false;

		// Even though we set a custom projection matrix (which implicitly includes near and far clipping plane values)
		// it seems that in the editor, the preview camera view still reads its values from the Camera.nearClipPlane
		// and Camera.farClipPlane values.
		float camNearClipPlane = cam.nearClipPlane;
		float camFarClipPlane = cam.farClipPlane;

		if (Optical)
        {
            float fovy;
            float aspect;
            float[] m = new float[16];
            float[] p = new float[16];

            if (OpticalCalibrationMode0 == OpticalCalibrationMode.ARXOpticalParametersFile)
            {
                opticalSetupOK = arController.PluginFunctions.arwLoadOpticalParams(null, OpticalParamsFileContents, OpticalParamsFileContents.Length, camNearClipPlane, camFarClipPlane, out fovy, out aspect, m, p);
                if (!opticalSetupOK)
                {
                    ARController.Log(LogTag + "Error loading ARX optical parameters.");
                    return;
                }

                // Convert millimetres to metres.
                m[12] *= 0.001f;
                m[13] *= 0.001f;
                m[14] *= 0.001f;
                cam.projectionMatrix = ARUtilityFunctions.MatrixFromFloatArray(p);
                ARController.Log(LogTag + "Optical parameters: fovy=" + fovy + ", aspect=" + aspect + ", camera position (m)={" + m[12].ToString("F3") + ", " + m[13].ToString("F3") + ", " + m[14].ToString("F3") + "}");
            } else {
                m[0] = m[5] = m[10] = m[15] = 1.0f;
                m[1] = m[2] = m[3] = m[4] = m[6] = m[7] = m[8] = m[9] = m[11] = m[12] = m[13] = m[14] = 0.0f;
            }
			
			opticalViewMatrix = ARUtilityFunctions.MatrixFromFloatArray(m);
			if (OpticalEyeLateralOffsetRight != 0.0f) opticalViewMatrix = Matrix4x4.TRS(new Vector3(-OpticalEyeLateralOffsetRight, 0.0f, 0.0f), Quaternion.identity, Vector3.one) * opticalViewMatrix; 
			// Convert to left-hand matrix.
			opticalViewMatrix = ARUtilityFunctions.LHMatrixFromRHMatrix(opticalViewMatrix);
		} else {

			// Fetch the projection from the video source.
			float[] projRaw = new float[16];
			bool _cameraStereoRightEye = Stereo && StereoEye == ARCamera.ViewEye.Right;
			if (!_cameraStereoRightEye || !arController.VideoIsStereo)
			{
				if (!arController.PluginFunctions.arwGetProjectionMatrixForViewportSizeAndFittingMode(cam.pixelWidth, cam.pixelHeight, (int)CameraContentMode, (int)CameraContentHAlign, (int)CameraContentVAlign, camNearClipPlane, camFarClipPlane, projRaw)) return;
			}
			else
			{
				if (!arController.PluginFunctions.arwGetProjectionMatrixForViewportSizeAndFittingModeStereo(cam.pixelWidth, cam.pixelHeight, (int)CameraContentMode, (int)CameraContentHAlign, (int)CameraContentVAlign, camNearClipPlane, camFarClipPlane, null, projRaw)) return;
			}
			Matrix4x4 projectionMatrix = ARUtilityFunctions.MatrixFromFloatArray(projRaw);
			ARController.Log(LogTag + "Projection matrix: [" + Environment.NewLine + projectionMatrix.ToString().Trim() + "]");
			if (ContentRotate90) projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * projectionMatrix;
			if (ContentFlipV) projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1.0f, -1.0f, 1.0f)) * projectionMatrix;
			if (ContentFlipH) projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1.0f, 1.0f, 1.0f)) * projectionMatrix;

			cam.projectionMatrix = projectionMatrix;
		}
		
		// Don't clear anything or else we interfere with other foreground cameras
		//cam.clearFlags = CameraClearFlags.Nothing;
		
		// Renders after the clear and background cameras
		//c.depth = 0;

		// Start at origin.
		cam.transform.position = Vector3.zero;
		cam.transform.rotation = Quaternion.identity;
		cam.transform.localScale = Vector3.one;
	}
	
	// Return the origin associated with this component.
	// Uses cached value if available, otherwise performs a find operation.
	public virtual AROrigin GetOrigin()
	{
		if (_origin == null) {
			// Locate the origin in parent.
			_origin = this.gameObject.GetComponentInParent<AROrigin>();
		}
		return _origin;
	}
	
	// Get the marker, if any, currently acting as the base.
	public virtual ARTrackable GetTrackable()
	{
		AROrigin origin = GetOrigin();
		if (origin == null) return null;
		return (origin.GetBaseTrackable());
	}
	
	// Updates arVisible, arPosition, arRotation based on linked marker state.
	protected virtual void UpdateTracking()
	{
		// Note the current time
		timeLastUpdate = Time.realtimeSinceStartup;
			
		// First, ensure we have a base marker. If none, then no markers are currently in view.
        ARTrackable trackable = GetTrackable();
		if (trackable == null) {
			if (arVisible) {
				// Marker was visible but now is hidden.
				timeTrackingLost = timeLastUpdate;
				arVisible = false;
			}
		} else {
			
			if (trackable.Visible) {
				
				Matrix4x4 pose;
				if (Optical && opticalSetupOK) {
					pose = (opticalViewMatrix * trackable.TransformationMatrix).inverse;
				} else {
					pose = trackable.TransformationMatrix.inverse;
				}
				
				arPosition = ARUtilityFunctions.PositionFromMatrix(pose);
				// Camera orientation: In ARToolKit, zero rotation of the camera corresponds to looking vertically down on a marker
				// lying flat on the ground. In Unity however, if we still treat markers as being flat on the ground, we clash with Unity's
				// camera "rotation", because an unrotated Unity camera is looking horizontally.
				// So we choose to treat an unrotated marker as standing vertically, and apply a transform to the scene to
				// to get it to lie flat on the ground.
				arRotation = ARUtilityFunctions.QuaternionFromMatrix(pose);
				
				if (!arVisible) {
					// Marker was hidden but now is visible.
					arVisible = true;
				}
			} else {
				if (arVisible) {
					// Marker was visible but now is hidden.
					timeTrackingLost = timeLastUpdate;
					arVisible = false;
				}
			}
		}
	}
	
	protected virtual void ApplyTracking()
	{
		if (arVisible) {
			transform.localPosition = arPosition; // TODO: Change to transform.position = PositionFromMatrix(origin.transform.localToWorldMatrix * pose) etc;
			transform.localRotation = arRotation;
		}
	}
	
	// Note that [DefaultExecutionOrder] is used on ARTrackable to ensure the base ARTrackable has updated before we try and use the transformation.
	protected virtual void Update()
	{
		// Local scale is always 1 for now
		transform.localScale = Vector3.one;
		
		// Update tracking if we are running in Player.
		if (Application.isPlaying) {
			UpdateTracking();
			ApplyTracking();
		}
	}

	private void CycleContentMode()
	{
		switch (CameraContentMode)
		{
			case ContentMode.Fit:
				CameraContentMode = ContentMode.Fill; // Fill and OneToOne mode can potentially result in negative values for viewport x and y. Unity can't handle that.
				break;
			case ContentMode.Fill:
				CameraContentMode = ContentMode.Stretch;
				break;
			case ContentMode.Stretch:
				CameraContentMode = ContentMode.OneToOne;
				break;
			case ContentMode.OneToOne:
			default:
				CameraContentMode = ContentMode.Fit;
				break;
		}
	}

}

