/*
 *  ARCameraEditor.cs
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
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARCamera))] 
public class ARCameraEditor : Editor 
{
	private static TextAsset[] OpticalParamsAssets;
	private static int OpticalParamsAssetCount;
	private static string[] OpticalParamsFilenames;

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
		ARCamera arc = (ARCamera)target;
		if (arc == null) return;
		
		//
		// Stereo parameters.
		//
		EditorGUILayout.Separator();
		arc.Stereo = EditorGUILayout.Toggle("Part of a stereo pair", arc.Stereo);
		if (arc.Stereo) {
			arc.StereoEye = (ARCamera.ViewEye)EditorGUILayout.EnumPopup("Stereo eye:", arc.StereoEye);
		}

		//
		// Optical parameters.
		//
		EditorGUILayout.Separator();

		arc.Optical = EditorGUILayout.Toggle("Optical see-through mode.", arc.Optical);

		if (arc.Optical) {
			// Offer a popup with optical params file names.
			RefreshOpticalParamsFilenames(); // Update the list of available optical params from the resources dir
			if (OpticalParamsFilenames.Length > 0) {
				int opticalParamsFilenameIndex = EditorGUILayout.Popup("Optical parameters file", arc.OpticalParamsFilenameIndex, OpticalParamsFilenames);
				string opticalParamsFilename = OpticalParamsAssets[opticalParamsFilenameIndex].name;
				if (opticalParamsFilename != arc.OpticalParamsFilename) {
					arc.OpticalParamsFilenameIndex = opticalParamsFilenameIndex;
					arc.OpticalParamsFilename = opticalParamsFilename;
					arc.OpticalParamsFileContents = OpticalParamsAssets[arc.OpticalParamsFilenameIndex].bytes;
				}
				arc.OpticalEyeLateralOffsetRight = EditorGUILayout.FloatField("Lateral offset right:", arc.OpticalEyeLateralOffsetRight);
				EditorGUILayout.HelpBox("Enter an amount by which this eye should be moved to the right, relative to the video camera lens. E.g. if this is the right eye, but you're using calibrated optical paramters for the left eye, enter 0.065 (65mm).", MessageType.Info);
			} else {
				arc.OpticalParamsFilenameIndex = 0;
				EditorGUILayout.LabelField("Optical parameters file", "No parameters files available");
				arc.OpticalParamsFilename = "";
				arc.OpticalParamsFileContents = new byte[0];
			}
		}
    }
}
