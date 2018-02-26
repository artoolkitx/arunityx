/*
 *  ARControllerEditor.cs
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

[CustomEditor(typeof(ARController))]
public class ARControllerEditor : Editor
{

	public bool showVideoOptions = true;
    public bool showThresholdOptions = false;
    public bool showSquareTrackingOptions = false;
	public bool showNFTTrackingOptions = false;
	public bool showApplicationOptions = false;


    public override void OnInspectorGUI()
    {

        ARController arcontroller = (ARController)target;
        if (arcontroller == null) return;


        EditorGUILayout.LabelField("Version", "ARToolKit " + arcontroller.Version);
		

        EditorGUILayout.Separator();

        showVideoOptions = EditorGUILayout.Foldout(showVideoOptions, "Video Options");
        if (showVideoOptions)
        {
			arcontroller.videoCParamName0 = EditorGUILayout.TextField("Camera parameters", arcontroller.videoCParamName0);
			if (!string.IsNullOrEmpty(arcontroller.videoCParamName0)) {
				EditorGUILayout.HelpBox("Automatic camera parameters (if available) will be overridden.", MessageType.Info);
			}
            arcontroller.videoConfigurationWindows0 = EditorGUILayout.TextField("Video config. (Windows)", arcontroller.videoConfigurationWindows0);
            arcontroller.videoConfigurationMacOSX0 = EditorGUILayout.TextField("Video config. (Mac OS X)", arcontroller.videoConfigurationMacOSX0);
            arcontroller.videoConfigurationiOS0 = EditorGUILayout.TextField("Video config. (iOS)", arcontroller.videoConfigurationiOS0);
            arcontroller.videoConfigurationAndroid0 = EditorGUILayout.TextField("Video config. (Android)", arcontroller.videoConfigurationAndroid0);
			arcontroller.videoConfigurationWindowsStore0 = EditorGUILayout.TextField("Video config. (Windows Store)", arcontroller.videoConfigurationWindowsStore0);
			arcontroller.videoConfigurationLinux0 = EditorGUILayout.TextField("Video config. (Linux)", arcontroller.videoConfigurationLinux0);
			arcontroller.BackgroundLayer0 = EditorGUILayout.LayerField("Layer", arcontroller.BackgroundLayer0);

			arcontroller.VideoIsStereo = EditorGUILayout.Toggle("Video source is stereo", arcontroller.VideoIsStereo);
			if (arcontroller.VideoIsStereo) {
				arcontroller.videoCParamName1 = EditorGUILayout.TextField("Camera parameters (R)", arcontroller.videoCParamName1);
				if (!string.IsNullOrEmpty(arcontroller.videoCParamName1)) {
						EditorGUILayout.HelpBox("Automatic camera parameters (if available) will be overridden.", MessageType.Info);
				}
				arcontroller.videoConfigurationWindows1 = EditorGUILayout.TextField("Video config.(R) (Windows)", arcontroller.videoConfigurationWindows1);
				arcontroller.videoConfigurationMacOSX1 = EditorGUILayout.TextField("Video config.(R) (Mac OS X)", arcontroller.videoConfigurationMacOSX1);
				arcontroller.videoConfigurationiOS1 = EditorGUILayout.TextField("Video config.(R) (iOS)", arcontroller.videoConfigurationiOS1);
				arcontroller.videoConfigurationAndroid1 = EditorGUILayout.TextField("Video config.(R) (Android)", arcontroller.videoConfigurationAndroid1);
				arcontroller.videoConfigurationWindowsStore1 = EditorGUILayout.TextField("Video config.(R) (Windows Store)", arcontroller.videoConfigurationWindowsStore1);
				arcontroller.videoConfigurationLinux1 = EditorGUILayout.TextField("Video config. (Linux)", arcontroller.videoConfigurationLinux1);
				arcontroller.BackgroundLayer1 = EditorGUILayout.LayerField("Layer (R)", arcontroller.BackgroundLayer1);
				arcontroller.transL2RName = EditorGUILayout.TextField("Stereo parameters", arcontroller.transL2RName);
			}

			arcontroller.UseNativeGLTexturingIfAvailable = EditorGUILayout.Toggle("Use native GL texturing (if available)", arcontroller.UseNativeGLTexturingIfAvailable);
			if (arcontroller.UseNativeGLTexturingIfAvailable) {
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle("Allow non-RGB video internally.", false);
				EditorGUI.EndDisabledGroup();
			} else {
				arcontroller.AllowNonRGBVideo = EditorGUILayout.Toggle("Allow non-RGB video internally.", arcontroller.AllowNonRGBVideo);
			}


			ContentMode currentContentMode = arcontroller.ContentMode;
			ContentMode newContentMode = (ContentMode)EditorGUILayout.EnumPopup("Content mode", currentContentMode);
			if (newContentMode != currentContentMode) {
				arcontroller.ContentMode = newContentMode;
			}
			arcontroller.ContentRotate90 = EditorGUILayout.Toggle("Rotate 90 deg.", arcontroller.ContentRotate90);
			arcontroller.ContentFlipV = EditorGUILayout.Toggle("Flip vertically", arcontroller.ContentFlipV);
			arcontroller.ContentFlipH = EditorGUILayout.Toggle("Flip horizontally.", arcontroller.ContentFlipH);
		}

        EditorGUILayout.Separator();
		
		arcontroller.NearPlane = EditorGUILayout.FloatField("Near plane", arcontroller.NearPlane);
        arcontroller.FarPlane = EditorGUILayout.FloatField("Far plane", arcontroller.FarPlane);

		EditorGUILayout.Separator();

        showThresholdOptions = EditorGUILayout.Foldout(showThresholdOptions, "Threshold Options");
        if (showThresholdOptions)
        {
            // Threshold mode selection
            ARController.ARToolKitThresholdMode currentThreshMode = arcontroller.VideoThresholdMode;
            ARController.ARToolKitThresholdMode newThreshMode = (ARController.ARToolKitThresholdMode)EditorGUILayout.EnumPopup("Mode:", currentThreshMode);
            if (newThreshMode != currentThreshMode) {
                arcontroller.VideoThresholdMode = newThreshMode;
            }

            // Info about the selected mode
            EditorGUILayout.LabelField("", ARController.ThresholdModeDescriptions[newThreshMode]);

            // Show threshold slider only in manual or bracketing modes.
			if (newThreshMode == ARController.ARToolKitThresholdMode.Manual || newThreshMode == ARController.ARToolKitThresholdMode.Bracketing) {

                int currentThreshold = arcontroller.VideoThreshold;
                //int newThreshold = UnityEngine.Mathf.Clamp(EditorGUILayout.IntField("Threshold: ", currentThreshold), 0, 255);
                int newThreshold = EditorGUILayout.IntSlider("Threshold: ", currentThreshold, 0, 255);
                if (newThreshold != currentThreshold) {
                    arcontroller.VideoThreshold = newThreshold;
                }
            }
        }

        EditorGUILayout.Separator();
		
		showSquareTrackingOptions = EditorGUILayout.Foldout(showSquareTrackingOptions, "Square Tracking Options");
		if (showSquareTrackingOptions) {


			int currentTemplateSize = arcontroller.TemplateSize;
			int newTemplateSize = EditorGUILayout.IntField("Template size: ", currentTemplateSize);
			if (newTemplateSize != currentTemplateSize && newTemplateSize >= 16 && newTemplateSize <= 64) {
				arcontroller.TemplateSize = newTemplateSize;
			}
			int currentTemplateCountMax = arcontroller.TemplateCountMax;
			int newTemplateCountMax = EditorGUILayout.IntField("Template count max.: ", currentTemplateCountMax);
			if (newTemplateCountMax != currentTemplateCountMax && newTemplateCountMax > 0) {
				arcontroller.TemplateCountMax = newTemplateCountMax;
			}

			// Labeling mode selection.
            ARController.ARToolKitLabelingMode currentLabelingMode = arcontroller.LabelingMode;
            ARController.ARToolKitLabelingMode newLabelingMode = (ARController.ARToolKitLabelingMode)EditorGUILayout.EnumPopup("Marker borders:", currentLabelingMode);
            if (newLabelingMode != currentLabelingMode) {
                arcontroller.LabelingMode = newLabelingMode;
            }
			
			// Border size selection.
            float currentBorderSize = arcontroller.BorderSize;
            float newBorderSize = UnityEngine.Mathf.Clamp(EditorGUILayout.FloatField("Border size:", currentBorderSize), 0.0f, 0.5f);
            if (newBorderSize != currentBorderSize) {
                arcontroller.BorderSize = newBorderSize;
            }
			
             // Pattern detection mode selection.
            ARController.ARToolKitPatternDetectionMode currentPatternDetectionMode = arcontroller.PatternDetectionMode;
            ARController.ARToolKitPatternDetectionMode newPatternDetectionMode = (ARController.ARToolKitPatternDetectionMode)EditorGUILayout.EnumPopup("Pattern detection mode:", currentPatternDetectionMode);
            if (newPatternDetectionMode != currentPatternDetectionMode) {
                arcontroller.PatternDetectionMode = newPatternDetectionMode;
            }
 
			// Matrix code type selection (only when in one of the matrix modes).
			if (newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_MATRIX_CODE_DETECTION
				|| newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX
				|| newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO_AND_MATRIX) {
				
            	ARController.ARToolKitMatrixCodeType currentMatrixCodeType = arcontroller.MatrixCodeType;
            	ARController.ARToolKitMatrixCodeType newMatrixCodeType = (ARController.ARToolKitMatrixCodeType)EditorGUILayout.EnumPopup("Matrix code type:", currentMatrixCodeType);
            	if (newMatrixCodeType != currentMatrixCodeType) {
            	    arcontroller.MatrixCodeType = newMatrixCodeType;
            	}
			}

		    // Image processing mode selection.
            ARController.ARToolKitImageProcMode currentImageProcMode = arcontroller.ImageProcMode;
            ARController.ARToolKitImageProcMode newImageProcMode = (ARController.ARToolKitImageProcMode)EditorGUILayout.EnumPopup("Image processing mode:", currentImageProcMode);
            if (newImageProcMode != currentImageProcMode) {
                arcontroller.ImageProcMode = newImageProcMode;
            }
 
		}

		EditorGUILayout.Separator();
		
		showNFTTrackingOptions = EditorGUILayout.Foldout(showNFTTrackingOptions, "NFT Tracking Options");
		if (showNFTTrackingOptions) {
			arcontroller.NFTMultiMode = EditorGUILayout.Toggle("Multi-page mode", arcontroller.NFTMultiMode);
		}

		EditorGUILayout.Separator();
		showApplicationOptions = EditorGUILayout.Foldout(showApplicationOptions, "Application Options");
		if (showApplicationOptions) {
			arcontroller.AutoStartAR = EditorGUILayout.Toggle("Auto-start AR.", arcontroller.AutoStartAR);
			if (arcontroller.AutoStartAR) EditorGUILayout.HelpBox("ARController.StartAR() will be called during MonoBehavior.Start().", MessageType.Info);
			else EditorGUILayout.HelpBox("ARController.StartAR() will not be called during MonoBehavior.Start(); you must call it yourself.", MessageType.Info);
			arcontroller.QuitOnEscOrBack = EditorGUILayout.Toggle("Quit on [Esc].", arcontroller.QuitOnEscOrBack);
			if (arcontroller.QuitOnEscOrBack) EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will quit the app.", MessageType.Info);
			else EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will be ignored.", MessageType.Info);
			arcontroller.LogLevel = (ARController.AR_LOG_LEVEL)EditorGUILayout.EnumPopup("Log level:", arcontroller.LogLevel);
		}
    }
}