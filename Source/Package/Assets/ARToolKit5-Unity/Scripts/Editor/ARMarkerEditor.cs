/*
 *  ARMarkerEditor.cs
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
using System.IO;

[CustomEditor(typeof(ARMarker))]
public class ARMarkerEditor : Editor
{
    public bool showFilterOptions = false;

	private static TextAsset[] PatternAssets;
	private static int PatternAssetCount;
	private static string[] PatternFilenames;
	
	/*private static Dictionary<ARController.ARToolKitMatrixCodeType, int> barcodeCounts = new Dictionary<ARController.ARToolKitMatrixCodeType, int>() {
		{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3, 64},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_PARITY65, 32},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_HAMMING63, 8},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4, 8192},
    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_9_3, 512},
		{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_5_5, 32}
//    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5, 4194304},
//    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_6x6, 8589934592},
//    	{ARController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_GLOBAL_ID, 18446744073709551616}
	};*/
	
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
		
		// Get the ARMarker that this panel will edit.
        ARMarker m = (ARMarker)target;
        if (m == null) return;
		
		// Attempt to load. Might not work out if e.g. for a single marker, pattern hasn't been
		// assigned yet, or for an NFT marker, dataset hasn't been specified.
		if (m.UID == ARMarker.NO_ID) m.Load(); 
		
		// Marker tag
        m.Tag = EditorGUILayout.TextField("Marker tag", m.Tag);
        EditorGUILayout.LabelField("UID", (m.UID == ARMarker.NO_ID ? "Not loaded": m.UID.ToString()));
		
        EditorGUILayout.Separator();
		
		// Marker type		
        MarkerType t = (MarkerType)EditorGUILayout.EnumPopup("Type", m.MarkerType);
        if (m.MarkerType != t) { // Reload on change.
			m.Unload();
			m.MarkerType = t;
			m.Load();
		}
		
		// Description of the type of marker
        EditorGUILayout.LabelField("Description", ARMarker.MarkerTypeNames[m.MarkerType]);
		
        switch (m.MarkerType) {
			
			case MarkerType.Square:	
        	case MarkerType.SquareBarcode:
			
				if (m.MarkerType == MarkerType.Square) {
				
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
					int BarcodeID = EditorGUILayout.IntField("Barcode ID", m.BarcodeID);
					//EditorGUILayout.LabelField("(in range 0 to " + barcodeCounts[ARController.MatrixCodeType] + ")");
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
			
        	case MarkerType.Multimarker:
				string MultiConfigFile = EditorGUILayout.TextField("Multimarker config.", m.MultiConfigFile);
        	    if (MultiConfigFile != m.MultiConfigFile) {
					m.Unload();
					m.MultiConfigFile = MultiConfigFile;
					m.Load();
				}
        	    break;

			case MarkerType.NFT:
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