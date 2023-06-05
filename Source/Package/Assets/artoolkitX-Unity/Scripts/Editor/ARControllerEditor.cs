/*
 *  ARControllerEditor.cs
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
using System.IO;

[CustomEditor(typeof(ARController))]
public class ARControllerEditor : Editor
{

	public bool showVideoOptions = true;
    public bool showThresholdOptions = false;
    public bool showSquareTrackingOptions = false;
    public bool show2DTrackingOptions = false;
	public bool showNFTTrackingOptions = false;
	public bool showApplicationOptions = false;

    protected SerializedProperty videoCParamName0;
    protected SerializedProperty BackgroundLayer0;
    protected SerializedProperty VideoIsStereo;
    protected SerializedProperty videoCParamName1;
    protected SerializedProperty BackgroundLayer1;
    protected SerializedProperty transL2RName;
    protected SerializedProperty UseNativeGLTexturingIfAvailable;
    protected SerializedProperty AllowNonRGBVideo;
    protected SerializedProperty ContentRotate90;
    protected SerializedProperty ContentFlipV;
    protected SerializedProperty ContentFlipH;
    protected SerializedProperty NearPlane;
    protected SerializedProperty FarPlane;
    protected SerializedProperty TwoDMaxMarkersToTrack;
    protected SerializedProperty TwoDThreaded;
    protected SerializedProperty NFTMultiMode;

    private readonly static Dictionary<ARController.ARToolKitThresholdMode, string> thresholdModeDescriptions = new Dictionary<ARController.ARToolKitThresholdMode, string>
    {
        {ARController.ARToolKitThresholdMode.Manual, "Uses a fixed threshold value"},
        {ARController.ARToolKitThresholdMode.Median, "Automatically adjusts threshold to whole-image median"},
        {ARController.ARToolKitThresholdMode.Otsu, "Automatically adjusts threshold using Otsu's method for foreground/background determination"},
        {ARController.ARToolKitThresholdMode.Adaptive, "Uses adaptive dynamic thresholding (warning: computationally expensive)"},
        {ARController.ARToolKitThresholdMode.Bracketing, "Automatically adjusts threshold using bracketed threshold values"}
    };

    protected virtual void OnEnable()
    {
        videoCParamName0 = serializedObject.FindProperty("videoCParamName0");
        BackgroundLayer0 = serializedObject.FindProperty("BackgroundLayer0");
        VideoIsStereo = serializedObject.FindProperty("VideoIsStereo");
        videoCParamName1 = serializedObject.FindProperty("videoCParamName1");
        BackgroundLayer1 = serializedObject.FindProperty("BackgroundLayer1");
        transL2RName = serializedObject.FindProperty("transL2RName");
        UseNativeGLTexturingIfAvailable = serializedObject.FindProperty("UseNativeGLTexturingIfAvailable");
        AllowNonRGBVideo = serializedObject.FindProperty("AllowNonRGBVideo");
        ContentRotate90 = serializedObject.FindProperty("ContentRotate90");
        ContentFlipV = serializedObject.FindProperty("ContentFlipV");
        ContentFlipH = serializedObject.FindProperty("ContentFlipH");
        NearPlane = serializedObject.FindProperty("NearPlane");
        FarPlane = serializedObject.FindProperty("FarPlane");
        TwoDMaxMarkersToTrack = serializedObject.FindProperty("currentTwoDMaxMarkersToTrack");
        TwoDThreaded = serializedObject.FindProperty("currentTwoDThreaded");
        NFTMultiMode = serializedObject.FindProperty("currentNFTMultiMode");
    }

    public override void OnInspectorGUI()
    {

        ARController arcontroller = (ARController)target;
        if (arcontroller == null) return;

        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

        EditorGUILayout.LabelField("Version", "artoolkitX " + arcontroller.Version);

        EditorGUILayout.Separator();

        showVideoOptions = EditorGUILayout.Foldout(showVideoOptions, "Video Options");
        if (showVideoOptions)
        {
            EditorGUILayout.PropertyField(videoCParamName0, new GUIContent("Camera parameters" + (VideoIsStereo.boolValue ? " (L)" : "")));
            if (!string.IsNullOrEmpty(videoCParamName0.stringValue))
            {
                EditorGUILayout.HelpBox("Automatic camera parameters (if available) will be overridden.", MessageType.Info);
            }
            BackgroundLayer0.intValue = EditorGUILayout.LayerField("Layer" + (VideoIsStereo.boolValue ? " (L)" : ""), BackgroundLayer0.intValue);

            EditorGUILayout.PropertyField(VideoIsStereo, new GUIContent("Video source is stereo"));
            if (VideoIsStereo.boolValue)
            {
                EditorGUILayout.PropertyField(videoCParamName1, new GUIContent("Camera parameters (R)"));
                if (!string.IsNullOrEmpty(videoCParamName1.stringValue))
                {
                    EditorGUILayout.HelpBox("Automatic camera parameters (if available) will be overridden.", MessageType.Info);
                }
                BackgroundLayer1.intValue = EditorGUILayout.LayerField("Layer (R)", BackgroundLayer1.intValue);
                EditorGUILayout.PropertyField(transL2RName, new GUIContent("Stereo parameters"));
            }

            // Commented-out, as native GL texturing is currently unsupported.
            //EditorGUILayout.PropertyField(UseNativeGLTexturingIfAvailable, new GUIContent("Use native GL texturing (if available)"));
            //if (UseNativeGLTexturingIfAvailable.boolValue)
            //{
            //    EditorGUI.BeginDisabledGroup(true);
            //    EditorGUILayout.Toggle("Allow non-RGB video internally.", false);
            //    EditorGUI.EndDisabledGroup();
            //}
            //else
            //{
                EditorGUILayout.PropertyField(AllowNonRGBVideo, new GUIContent("Allow non-RGB video internally.",
                    "If enabled, ARToolKit may use an optimised video format for video stream image acquisition and tracking. If disabled, video stream acquisition and tracking will be forced to an RGB format."));
            //}


            ContentMode currentContentMode = arcontroller.ContentMode;
            ContentMode newContentMode = (ContentMode)EditorGUILayout.EnumPopup("Content mode", currentContentMode);
            if (newContentMode != currentContentMode)
            {
                Undo.RecordObject(arcontroller, "Set content mode");
                arcontroller.ContentMode = newContentMode;
            }
            EditorGUILayout.PropertyField(ContentRotate90, new GUIContent("Rotate 90 deg."));
            EditorGUILayout.PropertyField(ContentFlipV, new GUIContent("Flip vertically"));
            EditorGUILayout.PropertyField(ContentFlipH, new GUIContent("Flip horizontally."));
        } // showVideoOptions

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(NearPlane, new GUIContent("Near plane",
            "For maximum depth-buffer precision, set this to the largest acceptable value that is less than \"Far plane\", while taking into account that content closer than this to the camera will not be rendered."));
        EditorGUILayout.PropertyField(FarPlane, new GUIContent("Far plane",
            "For maximum depth-buffer precision, set this to the smallest acceptable value that is greater than \"Near plane\", while taking into account that content farther than this from the camera will not be rendered."));

        EditorGUILayout.Separator();

        showSquareTrackingOptions = EditorGUILayout.Foldout(showSquareTrackingOptions, "Square Tracking Options");
        if (showSquareTrackingOptions)
        {
            showThresholdOptions = EditorGUILayout.Foldout(showThresholdOptions, "Threshold Options");
            if (showThresholdOptions)
            {
                // Threshold mode selection
                ARController.ARToolKitThresholdMode currentThreshMode = arcontroller.VideoThresholdMode;
                ARController.ARToolKitThresholdMode newThreshMode = (ARController.ARToolKitThresholdMode)EditorGUILayout.EnumPopup("Mode:", currentThreshMode);
                if (newThreshMode != currentThreshMode)
                {
                    Undo.RecordObject(arcontroller, "Set threshold mode");
                    arcontroller.VideoThresholdMode = newThreshMode;
                }

                // Info about the selected mode
                EditorGUILayout.LabelField("", thresholdModeDescriptions[newThreshMode]);

                // Show threshold slider only in manual or bracketing modes.
                if (newThreshMode == ARController.ARToolKitThresholdMode.Manual || newThreshMode == ARController.ARToolKitThresholdMode.Bracketing)
                {

                    int currentThreshold = arcontroller.VideoThreshold;
                    //int newThreshold = UnityEngine.Mathf.Clamp(EditorGUILayout.IntField("Threshold: ", currentThreshold), 0, 255);
                    int newThreshold = EditorGUILayout.IntSlider("Threshold: ", currentThreshold, 0, 255);
                    if (newThreshold != currentThreshold)
                    {
                        Undo.RecordObject(arcontroller, "Set threshold");
                        arcontroller.VideoThreshold = newThreshold;
                    }
                }
            }

            EditorGUILayout.Separator();

            // Labeling mode selection.
            ARController.ARToolKitLabelingMode currentLabelingMode = arcontroller.LabelingMode;
            ARController.ARToolKitLabelingMode newLabelingMode = (ARController.ARToolKitLabelingMode)EditorGUILayout.EnumPopup("Trackable borders:", currentLabelingMode);
            if (newLabelingMode != currentLabelingMode)
            {
                Undo.RecordObject(arcontroller, "Set labeling mode");
                arcontroller.LabelingMode = newLabelingMode;
            }

            // Pattern detection mode selection.
            ARController.ARToolKitPatternDetectionMode currentPatternDetectionMode = arcontroller.PatternDetectionMode;
            ARController.ARToolKitPatternDetectionMode newPatternDetectionMode = (ARController.ARToolKitPatternDetectionMode)EditorGUILayout.EnumPopup("Pattern detection mode:", currentPatternDetectionMode);
            if (newPatternDetectionMode != currentPatternDetectionMode)
            {
                Undo.RecordObject(arcontroller, "Set pattern detection mode");
                arcontroller.PatternDetectionMode = newPatternDetectionMode;
            }

            bool doTemplateMatching = newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR
                || newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO
                || newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX
                || newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO_AND_MATRIX;
            bool doMatrixMatching = newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_MATRIX_CODE_DETECTION
                || newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX
                || newPatternDetectionMode == ARController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO_AND_MATRIX;

            if (doTemplateMatching)
            {
                int currentTemplateSize = arcontroller.TemplateSize;
                int newTemplateSize = EditorGUILayout.IntField("Template size: ", currentTemplateSize);
                if (newTemplateSize != currentTemplateSize && newTemplateSize >= 16 && newTemplateSize <= 64)
                {
                    Undo.RecordObject(arcontroller, "Set template size");
                    arcontroller.TemplateSize = newTemplateSize;
                }
                int currentTemplateCountMax = arcontroller.TemplateCountMax;
                int newTemplateCountMax = EditorGUILayout.IntField("Template count max.: ", currentTemplateCountMax);
                if (newTemplateCountMax != currentTemplateCountMax && newTemplateCountMax > 0)
                {
                    Undo.RecordObject(arcontroller, "Set template count max.");
                    arcontroller.TemplateCountMax = newTemplateCountMax;
                }
            }

            // Matrix code type selection (only when in one of the matrix modes).
            if (doMatrixMatching)
            {
                ARController.ARToolKitMatrixCodeType currentMatrixCodeType = arcontroller.MatrixCodeType;
                ARController.ARToolKitMatrixCodeType newMatrixCodeType = (ARController.ARToolKitMatrixCodeType)EditorGUILayout.EnumPopup("Matrix code type:", currentMatrixCodeType);
                if (newMatrixCodeType != currentMatrixCodeType)
                {
                    Undo.RecordObject(arcontroller, "Set matrix code type");
                    arcontroller.MatrixCodeType = newMatrixCodeType;
                }

                bool m = EditorGUILayout.Toggle("Auto-create new trackables", arcontroller.SquareMatrixModeAutocreateNewTrackables);
                if (m != arcontroller.SquareMatrixModeAutocreateNewTrackables)
                {
                    Undo.RecordObject(arcontroller, "Set auto-create new trackables");
                    arcontroller.SquareMatrixModeAutocreateNewTrackables = m;
                }
                if (m)
                {
                    float w = EditorGUILayout.FloatField("Default size of auto-created trackables.", arcontroller.SquareMatrixModeAutocreateNewTrackablesDefaultWidth);
                    if (w != arcontroller.SquareMatrixModeAutocreateNewTrackablesDefaultWidth)
                    {
                        Undo.RecordObject(arcontroller, "Set auto-create new trackables default width");
                        arcontroller.SquareMatrixModeAutocreateNewTrackablesDefaultWidth = w;
                    }
                }
            }

            // Border size selection.
            if (!doMatrixMatching || arcontroller.MatrixCodeType != ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_GLOBAL_ID)
            {
                float currentBorderSize = arcontroller.BorderSize;
                float newBorderSize = UnityEngine.Mathf.Clamp(EditorGUILayout.FloatField("Border size:", currentBorderSize), 0.0f, 0.5f);
                if (newBorderSize != currentBorderSize)
                {
                    Undo.RecordObject(arcontroller, "Set border size");
                    arcontroller.BorderSize = newBorderSize;
                }
            }

            // Image processing mode selection.
            ARController.ARToolKitImageProcMode currentImageProcMode = arcontroller.ImageProcMode;
            ARController.ARToolKitImageProcMode newImageProcMode = (ARController.ARToolKitImageProcMode)EditorGUILayout.EnumPopup("Image processing mode:", currentImageProcMode);
            if (newImageProcMode != currentImageProcMode)
            {
                Undo.RecordObject(arcontroller, "Set image processing mode");
                arcontroller.ImageProcMode = newImageProcMode;
            }

        }

        EditorGUILayout.Separator();

        show2DTrackingOptions = EditorGUILayout.Foldout(show2DTrackingOptions, "2D Tracking Options");
        if (show2DTrackingOptions)
        {
            EditorGUILayout.PropertyField(TwoDMaxMarkersToTrack, new GUIContent("Max. number of markers to track"), null);
            EditorGUILayout.PropertyField(TwoDThreaded, new GUIContent("Threaded"), null);
        }

        EditorGUILayout.Separator();

        showNFTTrackingOptions = EditorGUILayout.Foldout(showNFTTrackingOptions, "NFT Tracking Options");
        if (showNFTTrackingOptions)
        {
            EditorGUILayout.PropertyField(NFTMultiMode, new GUIContent("Multi - page mode"), null);
        }

		EditorGUILayout.Separator();
		showApplicationOptions = EditorGUILayout.Foldout(showApplicationOptions, "Application Options");
		if (showApplicationOptions) {
			arcontroller.AutoStartAR = EditorGUILayout.Toggle("Auto-start AR.", arcontroller.AutoStartAR);
			if (arcontroller.AutoStartAR) EditorGUILayout.HelpBox("ARController.StartAR() will be called during MonoBehavior.Start().", MessageType.Info);
			else EditorGUILayout.HelpBox("ARController.StartAR() will not be called during MonoBehavior.Start(); you must call it yourself.", MessageType.Info);
			arcontroller.QuitOnEscOrBack = EditorGUILayout.Toggle("Quit on [Esc].", arcontroller.QuitOnEscOrBack);
			if (arcontroller.QuitOnEscOrBack) EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will quit the app.", MessageType.Info);
			else EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will be ignored by artoolkitX.", MessageType.Info);
			arcontroller.LogLevel = (ARController.AR_LOG_LEVEL)EditorGUILayout.EnumPopup("Log level:", arcontroller.LogLevel);
		}

        serializedObject.ApplyModifiedProperties();
    }
}