/*
 *  ARTrackableEditor.cs
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

[CustomEditor(typeof(ARTrackable))]
public class ARTrackableEditor : Editor
{
    public bool showFilterOptions = false;

	private static TextAsset[] PatternAssets;
	private static int PatternAssetCount;
	private static string[] PatternFilenames;
	
    private static Dictionary<ARController.ARToolKitMatrixCodeType, long> barcodeCounts = new Dictionary<ARController.ARToolKitMatrixCodeType, long>() {
		{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3, 64},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_PARITY65, 32},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_HAMMING63, 8},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4, 8192},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_9_3, 512},
		{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_5_5, 32},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5, 4194304},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5_BCH_22_12_5, 4096},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5_BCH_22_7_7, 128},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_6x6, 8589934592}
//    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_GLOBAL_ID, 18446744073709551616}
	};
	
	void OnDestroy()
	{
		// Classes inheriting from MonoBehavior need to set all static member variables to null on unload.
		PatternAssets = null;
		PatternAssetCount = 0;
		PatternFilenames = null;
	}
	
	private static void RefreshPatternFilenames() 
	{
		PatternAssets = Resources.LoadAll("ardata/markers", typeof(TextAsset)).Cast<TextAsset>().ToArray();
		PatternAssetCount = PatternAssets.Length;
		
		PatternFilenames = new string[PatternAssetCount];
		for (int i = 0; i < PatternAssetCount; i++) {					
			PatternFilenames[i] = PatternAssets[i].name;				
		}
	}
	
    public override void OnInspectorGUI()
    {
   
        EditorGUILayout.BeginVertical();
		
		// Get the ARTrackable that this panel will edit.
        ARTrackable m = (ARTrackable)target;
        if (m == null) return;
		
		// Attempt to load. Might not work out if e.g. for a single marker, pattern hasn't been
		// assigned yet, or for an NFT marker, dataset hasn't been specified.
		if (m.UID == ARTrackable.NO_ID) m.Load(); 
		
		// Trackable tag
        m.Tag = EditorGUILayout.TextField("Trackable tag", m.Tag);
        EditorGUILayout.LabelField("UID", (m.UID == ARTrackable.NO_ID ? "Not loaded": m.UID.ToString()));
		
        EditorGUILayout.Separator();
		
		// Trackable type		
        ARTrackable.TrackableType t = (ARTrackable.TrackableType)EditorGUILayout.EnumPopup("Type", m.Type);
        if (m.Type != t) { // Reload on change.
			m.Unload();
			m.Type = t;
			m.Load();
		}
		
		// Description of the type of marker
        EditorGUILayout.LabelField("Description", ARTrackable.TrackableTypeNames[m.Type]);
		
        switch (m.Type) {
			
            case ARTrackable.TrackableType.Square:	
            case ARTrackable.TrackableType.SquareBarcode:
			
                if (m.Type == ARTrackable.TrackableType.Square) {
				
					// For pattern markers, offer a popup with marker pattern file names.
					RefreshPatternFilenames(); // Update the list of available markers from the resources dir
					if (PatternFilenames.Length > 0) {
						int patternFilenameIndex = EditorGUILayout.Popup("Pattern file", m.PatternFilenameIndex, PatternFilenames);
						string patternFilename = PatternAssets[patternFilenameIndex].name;
						if (patternFilename != m.PatternFilename) {
							m.Unload();
							m.PatternFilenameIndex = patternFilenameIndex;
							m.PatternFilename = patternFilename;
							m.PatternContents = PatternAssets[m.PatternFilenameIndex].text;
							m.Load();
						}
					} else {
						m.PatternFilenameIndex = 0;
						EditorGUILayout.LabelField("Pattern file", "No patterns available");
						m.PatternFilename = "";
						m.PatternContents = "";
					}
				
				} else {
				
					// For barcode markers, allow the user to specify the barcode ID.
                    long BarcodeID = EditorGUILayout.LongField("Barcode ID", m.BarcodeID);
                    if (BarcodeID < 0) BarcodeID = 0;
                    ARController arcontroller = Component.FindObjectOfType(typeof(ARController)) as ARController;
                    if (arcontroller != null) {
                        long maxBarcodeID = barcodeCounts[arcontroller.MatrixCodeType] - 1;
                        if (BarcodeID > maxBarcodeID) BarcodeID = maxBarcodeID;
                        EditorGUILayout.LabelField("(in range 0 to " + (barcodeCounts[arcontroller.MatrixCodeType] - 1) + ")");
                    }
	 				if (BarcodeID != m.BarcodeID) {
						m.Unload();
						m.BarcodeID = BarcodeID;
						m.Load();
					}
				
				}
			
				float patternWidthPrev = m.PatternWidth;
				m.PatternWidth = EditorGUILayout.FloatField("Width", m.PatternWidth);
				if (patternWidthPrev != m.PatternWidth) {
					m.Unload();
					m.Load();
				}
				m.UseContPoseEstimation = EditorGUILayout.Toggle("Cont. pose estimation", m.UseContPoseEstimation);
			
				break;
			
            case ARTrackable.TrackableType.Multimarker:
				string MultiConfigFile = EditorGUILayout.TextField("Multimarker config.", m.MultiConfigFile);
        	    if (MultiConfigFile != m.MultiConfigFile) {
					m.Unload();
					m.MultiConfigFile = MultiConfigFile;
					m.Load();
				}
        	    break;

            case ARTrackable.TrackableType.NFT:
                string NFTDataSetName = EditorGUILayout.TextField("NFT dataset name", m.NFTDataName);
				if (NFTDataSetName != m.NFTDataName) {
					m.Unload();
					m.NFTDataName = NFTDataSetName;
					m.Load();
				}

				float nftScalePrev = m.NFTScale;
				m.NFTScale = EditorGUILayout.FloatField("NFT marker scalefactor", m.NFTScale);
				if (nftScalePrev != m.NFTScale) {
					EditorUtility.SetDirty(m);
				}
				break;

            case ARTrackable.TrackableType.TwoD:
                string TwoDImageFile = EditorGUILayout.TextField("Image file", m.TwoDImageFile);
                if (TwoDImageFile != m.TwoDImageFile) {
                    m.Unload();
                    m.TwoDImageFile = TwoDImageFile;
                    m.Load();
                }

                float twoDImageWidthPrev = m.TwoDImageWidth;
                m.TwoDImageWidth = EditorGUILayout.FloatField("Image width", m.TwoDImageWidth);
                if (twoDImageWidthPrev != m.TwoDImageWidth) {
                    m.Unload();
                    m.Load();
                }
                break;
        }
		
        EditorGUILayout.Separator();
		
        showFilterOptions = EditorGUILayout.Foldout(showFilterOptions, "Filter Options");
        if (showFilterOptions) {
			m.Filtered = EditorGUILayout.Toggle("Filtered:", m.Filtered);
			m.FilterSampleRate = EditorGUILayout.Slider("Sample rate:", m.FilterSampleRate, 1.0f, 30.0f);
			m.FilterCutoffFreq = EditorGUILayout.Slider("Cutoff freq.:", m.FilterCutoffFreq, 1.0f, 30.0f);
		}

        EditorGUILayout.BeginHorizontal();

        // Draw all the marker images
        if (m.Patterns != null) {
            for (int i = 0; i < m.Patterns.Length; i++) {
                GUILayout.Label(new GUIContent("Pattern " + i + ", " + m.Patterns[i].width.ToString("n3") + " m", m.Patterns[i].texture), GUILayout.ExpandWidth(false)); // n3 -> 3 decimal places.
            }
        }
		
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

    }

}