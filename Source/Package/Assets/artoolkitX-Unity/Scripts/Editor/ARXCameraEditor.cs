/*
 *  ARXCameraEditor.cs
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
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARXCamera))]
public class ARXCameraEditor : Editor
{
	private static TextAsset[] OpticalParamsAssets;
	private static int OpticalParamsAssetCount;
    private static string[] OpticalParamsFilenames;
    protected SerializedProperty ContentRotate90;
    protected SerializedProperty ContentFlipV;
    protected SerializedProperty ContentFlipH;

    protected virtual void OnEnable()
    {
        ContentRotate90 = serializedObject.FindProperty("ContentRotate90");
        ContentFlipV = serializedObject.FindProperty("ContentFlipV");
        ContentFlipH = serializedObject.FindProperty("ContentFlipH");
    }

    public static void RefreshOpticalParamsFilenames()
	{
		OpticalParamsAssets = Resources.LoadAll("ardata/optical", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		OpticalParamsAssetCount = OpticalParamsAssets.Length;
		OpticalParamsFilenames = new string[OpticalParamsAssetCount];
		for (int i = 0; i < OpticalParamsAssetCount; i++) {
			OpticalParamsFilenames[i] = OpticalParamsAssets[i].name;
		}
	}

    public override void OnInspectorGUI()
    {
		ARXCamera arc = (ARXCamera)target;
		if (arc == null) return;

        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
            Camera c = arc.gameObject.GetComponent<Camera>();
            if (c != null)
            {
                EditorGUILayout.LabelField(new GUIContent("Near plane",
        "For maximum depth-buffer precision, set this value on the attached camera to the largest acceptable value that is less than \"Far plane\", while taking into account that content closer than this to the camera will not be rendered."), new GUIContent(c.nearClipPlane.ToString()));
                EditorGUILayout.LabelField(new GUIContent("Far plane",
                    "For maximum depth-buffer precision, set this value on the attached camera to the smallest acceptable value that is greater than \"Near plane\", while taking into account that content farther than this from the camera will not be rendered."), new GUIContent(c.farClipPlane.ToString()));
            }
        }

        //
        // Stereo parameters.
        //
        EditorGUILayout.Separator();
		arc.Stereo = EditorGUILayout.Toggle("Part of a stereo pair", arc.Stereo);
		if (arc.Stereo) {
			arc.StereoEye = (ARXCamera.ViewEye)EditorGUILayout.EnumPopup("Stereo eye:", arc.StereoEye);
		}

		//
		// Optical parameters.
		//
		EditorGUILayout.Separator();

		arc.Optical = EditorGUILayout.Toggle("Optical see-through mode.", arc.Optical);

        if (!arc.Optical)
        {
            ARXCamera.ContentMode currentContentMode = arc.CameraContentMode;
            ARXCamera.ContentMode newContentMode = (ARXCamera.ContentMode)EditorGUILayout.EnumPopup("Content mode", currentContentMode);
            if (newContentMode != currentContentMode)
            {
                Undo.RecordObject(arc, "Set content mode");
                arc.CameraContentMode = newContentMode;
            }

            EditorGUILayout.PropertyField(ContentRotate90, new GUIContent("Rotate 90 deg."));
            EditorGUILayout.PropertyField(ContentFlipV, new GUIContent("Flip vertically"));
            EditorGUILayout.PropertyField(ContentFlipH, new GUIContent("Flip horizontally."));
        }
        else
        {
            arc.OpticalCalibrationMode0 = (ARXCamera.OpticalCalibrationMode)EditorGUILayout.EnumPopup("Optical calibration type", arc.OpticalCalibrationMode0);
            if (arc.OpticalCalibrationMode0 == ARXCamera.OpticalCalibrationMode.ARXOpticalParametersFile)
            {
                // Offer a popup with optical params file names.
                RefreshOpticalParamsFilenames(); // Update the list of available optical params from the resources dir
                if (OpticalParamsFilenames.Length > 0)
                {
                    int opticalParamsFilenameIndex = EditorGUILayout.Popup("Optical parameters file", arc.OpticalParamsFilenameIndex, OpticalParamsFilenames);
                    string opticalParamsFilename = OpticalParamsAssets[opticalParamsFilenameIndex].name;
                    if (opticalParamsFilename != arc.OpticalParamsFilename)
                    {
                        arc.OpticalParamsFilenameIndex = opticalParamsFilenameIndex;
                        arc.OpticalParamsFilename = opticalParamsFilename;
                        arc.OpticalParamsFileContents = OpticalParamsAssets[arc.OpticalParamsFilenameIndex].bytes;
                    }
                    arc.OpticalEyeLateralOffsetRight = EditorGUILayout.FloatField("Lateral offset right:", arc.OpticalEyeLateralOffsetRight);
                    EditorGUILayout.HelpBox("Enter an amount by which this eye should be moved to the right, relative to the video camera lens. E.g. if this is the right eye, but you're using calibrated optical paramters for the left eye, enter 0.065 (65mm).", MessageType.Info);
                }
                else
                {
                    arc.OpticalParamsFilenameIndex = 0;
                    EditorGUILayout.LabelField("Optical parameters file", "No parameters files available");
                    arc.OpticalParamsFilename = "";
                    arc.OpticalParamsFileContents = new byte[0];
                }
            }
            else
            {
                arc.OpticalEyeLateralOffsetRight = EditorGUILayout.FloatField("Lateral offset right:", arc.OpticalEyeLateralOffsetRight);
                EditorGUILayout.HelpBox("Enter an amount by which this eye is to the right of the video camera lens. E.g. if the camera is centred between the eyes, and your IPD is 0.065 (65mm), enter 0.0325 for the right eye and -0.0325 for the left eye.", MessageType.Info);
            }
		}
        serializedObject.ApplyModifiedProperties();
    }
}
