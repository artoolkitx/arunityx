/*
 *  ARXControllerEditor.cs
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

[CustomEditor(typeof(ARXController))]
public class ARXControllerEditor : Editor
{

	public bool showVideoOptions = true;
    public bool showThresholdOptions = false;
    public bool showSquareTrackingOptions = false;
    public bool show2DTrackingOptions = false;
	public bool showNFTTrackingOptions = false;
	public bool showApplicationOptions = false;

    protected SerializedProperty videoCParamName0;
    protected SerializedProperty VideoIsStereo;
    protected SerializedProperty videoCParamName1;
    protected SerializedProperty transL2RName;
    protected SerializedProperty UseNativeGLTexturingIfAvailable;
    protected SerializedProperty AllowNonRGBVideo;
    protected SerializedProperty TwoDMaxMarkersToTrack;
    protected SerializedProperty TwoDThreaded;
    protected SerializedProperty NFTMultiMode;
    protected SerializedProperty AutoStartAR;
    protected SerializedProperty QuitOnEscOrBack;

    private readonly static Dictionary<ARXController.ARToolKitThresholdMode, string> thresholdModeDescriptions = new Dictionary<ARXController.ARToolKitThresholdMode, string>
    {
        {ARXController.ARToolKitThresholdMode.Manual, "Uses a fixed threshold value"},
        {ARXController.ARToolKitThresholdMode.Median, "Automatically adjusts threshold to whole-image median"},
        {ARXController.ARToolKitThresholdMode.Otsu, "Automatically adjusts threshold using Otsu's method for foreground/background determination"},
        {ARXController.ARToolKitThresholdMode.Adaptive, "Uses adaptive dynamic thresholding (warning: computationally expensive)"},
        {ARXController.ARToolKitThresholdMode.Bracketing, "Automatically adjusts threshold using bracketed threshold values"}
    };

    protected virtual void OnEnable()
    {
        videoCParamName0 = serializedObject.FindProperty("videoCParamName0");
        VideoIsStereo = serializedObject.FindProperty("VideoIsStereo");
        videoCParamName1 = serializedObject.FindProperty("videoCParamName1");
        transL2RName = serializedObject.FindProperty("transL2RName");
        UseNativeGLTexturingIfAvailable = serializedObject.FindProperty("UseNativeGLTexturingIfAvailable");
        AllowNonRGBVideo = serializedObject.FindProperty("AllowNonRGBVideo");
        TwoDMaxMarkersToTrack = serializedObject.FindProperty("currentTwoDMaxMarkersToTrack");
        TwoDThreaded = serializedObject.FindProperty("currentTwoDThreaded");
        NFTMultiMode = serializedObject.FindProperty("currentNFTMultiMode");
        AutoStartAR = serializedObject.FindProperty("AutoStartAR");
        QuitOnEscOrBack = serializedObject.FindProperty("QuitOnEscOrBack");
    }

    public override void OnInspectorGUI()
    {

        ARXController arcontroller = (ARXController)target;
        if (arcontroller == null) return;

        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

        EditorGUILayout.LabelField("Native plugin version", "artoolkitX " + arcontroller.Version);

        EditorGUILayout.Separator();

        showVideoOptions = EditorGUILayout.Foldout(showVideoOptions, "Video Options");
        if (showVideoOptions)
        {
            EditorGUILayout.PropertyField(videoCParamName0, new GUIContent("Camera calibration override file" + (VideoIsStereo.boolValue ? " (L)" : "")));
            if (!string.IsNullOrEmpty(videoCParamName0.stringValue))
            {
                EditorGUILayout.HelpBox("Automatic camera calibration parameters (if available) will be overridden by the parameters contained in this file.", MessageType.Info);
            }

            EditorGUILayout.PropertyField(VideoIsStereo, new GUIContent("Video source is stereo"));
            if (VideoIsStereo.boolValue)
            {
                EditorGUILayout.PropertyField(videoCParamName1, new GUIContent("Camera calibration override file (R)"));
                if (!string.IsNullOrEmpty(videoCParamName1.stringValue))
                {
                    EditorGUILayout.HelpBox("Automatic camera calibration  parameters (if available) will be overridden by the parameters contained in this file.", MessageType.Info);
                }
                EditorGUILayout.PropertyField(transL2RName, new GUIContent("Stereo calibration parameters file"));
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
        } // showVideoOptions

        EditorGUILayout.Separator();

        showSquareTrackingOptions = EditorGUILayout.Foldout(showSquareTrackingOptions, "Square Tracking Options");
        if (showSquareTrackingOptions)
        {
            showThresholdOptions = EditorGUILayout.Foldout(showThresholdOptions, "Threshold Options");
            if (showThresholdOptions)
            {
                // Threshold mode selection
                ARXController.ARToolKitThresholdMode currentThreshMode = arcontroller.VideoThresholdMode;
                ARXController.ARToolKitThresholdMode newThreshMode = (ARXController.ARToolKitThresholdMode)EditorGUILayout.EnumPopup("Mode:", currentThreshMode);
                if (newThreshMode != currentThreshMode)
                {
                    Undo.RecordObject(arcontroller, "Set threshold mode");
                    arcontroller.VideoThresholdMode = newThreshMode;
                }

                // Info about the selected mode
                EditorGUILayout.LabelField("", thresholdModeDescriptions[newThreshMode]);

                // Show threshold slider only in manual or bracketing modes.
                if (newThreshMode == ARXController.ARToolKitThresholdMode.Manual || newThreshMode == ARXController.ARToolKitThresholdMode.Bracketing)
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
            ARXController.ARToolKitLabelingMode currentLabelingMode = arcontroller.LabelingMode;
            ARXController.ARToolKitLabelingMode newLabelingMode = (ARXController.ARToolKitLabelingMode)EditorGUILayout.EnumPopup("Trackable borders:", currentLabelingMode);
            if (newLabelingMode != currentLabelingMode)
            {
                Undo.RecordObject(arcontroller, "Set labeling mode");
                arcontroller.LabelingMode = newLabelingMode;
            }

            // Pattern detection mode selection.
            ARXController.ARToolKitPatternDetectionMode currentPatternDetectionMode = arcontroller.PatternDetectionMode;
            ARXController.ARToolKitPatternDetectionMode newPatternDetectionMode = (ARXController.ARToolKitPatternDetectionMode)EditorGUILayout.EnumPopup("Pattern detection mode:", currentPatternDetectionMode);
            if (newPatternDetectionMode != currentPatternDetectionMode)
            {
                Undo.RecordObject(arcontroller, "Set pattern detection mode");
                arcontroller.PatternDetectionMode = newPatternDetectionMode;
            }

            bool doTemplateMatching = newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR
                || newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO
                || newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX
                || newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO_AND_MATRIX;
            bool doMatrixMatching = newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_MATRIX_CODE_DETECTION
                || newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX
                || newPatternDetectionMode == ARXController.ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_MONO_AND_MATRIX;

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
                ARXController.ARToolKitMatrixCodeType currentMatrixCodeType = arcontroller.MatrixCodeType;
                ARXController.ARToolKitMatrixCodeType newMatrixCodeType = (ARXController.ARToolKitMatrixCodeType)EditorGUILayout.EnumPopup("Matrix code type:", currentMatrixCodeType);
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
            if (!doMatrixMatching || arcontroller.MatrixCodeType != ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_GLOBAL_ID)
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
            ARXController.ARToolKitImageProcMode currentImageProcMode = arcontroller.ImageProcMode;
            ARXController.ARToolKitImageProcMode newImageProcMode = (ARXController.ARToolKitImageProcMode)EditorGUILayout.EnumPopup("Image processing mode:", currentImageProcMode);
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

        showNFTTrackingOptions = EditorGUILayout.Foldout(showNFTTrackingOptions, "Legacy Natural Feature Tracking Options");
        if (showNFTTrackingOptions)
        {
            EditorGUILayout.PropertyField(NFTMultiMode, new GUIContent("Multi - page mode"), null);
        }

		EditorGUILayout.Separator();
		showApplicationOptions = EditorGUILayout.Foldout(showApplicationOptions, "Application Options");
		if (showApplicationOptions) {
            EditorGUILayout.PropertyField(AutoStartAR, new GUIContent("Auto-start AR."));
			if (AutoStartAR.boolValue) EditorGUILayout.HelpBox("ARXController.StartAR() will be called during MonoBehavior.Start().", MessageType.Info);
			else EditorGUILayout.HelpBox("ARXController.StartAR() will not be called during MonoBehavior.Start(); you must call it yourself.", MessageType.Info);
            EditorGUILayout.PropertyField(QuitOnEscOrBack, new GUIContent("Quit on [Esc]."));
            if (QuitOnEscOrBack.boolValue) EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will quit the app.", MessageType.Info);
			else EditorGUILayout.HelpBox("The [esc] key (Windows, OS X) or the [Back] button (Android) will be ignored by artoolkitX.", MessageType.Info);
			ARXController.AR_LOG_LEVEL newLogLevel = (ARXController.AR_LOG_LEVEL)EditorGUILayout.EnumPopup("Log level:", arcontroller.LogLevel);
            if (newLogLevel != arcontroller.LogLevel)
            {
                Undo.RecordObject(arcontroller, "Set log level");
                arcontroller.LogLevel = newLogLevel;
            }
		}

        serializedObject.ApplyModifiedProperties();
    }
}