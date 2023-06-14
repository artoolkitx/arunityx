/*
 *  ARXTrackableEditor.cs
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

[CustomEditor(typeof(ARXTrackable))]
public class ARXTrackableEditor : Editor
{
    public bool showFilterOptions = false;

    private static Dictionary<ARXController.ARToolKitMatrixCodeType, long> barcodeCounts = new Dictionary<ARXController.ARToolKitMatrixCodeType, long>() {
		{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3, 64},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_PARITY65, 32},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3_HAMMING63, 8},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4, 8192},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_9_3, 512},
		{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_4x4_BCH_13_5_5, 32},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5, 4194304},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5_BCH_22_12_5, 4096},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_5x5_BCH_22_7_7, 128},
    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_6x6, 8589934592}
//    	{ARXController.ARToolKitMatrixCodeType.AR_MATRIX_CODE_GLOBAL_ID, 18446744073709551616}
	};

    public override void OnInspectorGUI()
    {

		// Get the ARXTrackable that this panel will edit.
        ARXTrackable m = (ARXTrackable)target;
        if (m == null) return;

		EditorGUILayout.BeginVertical();

		using (new EditorGUI.DisabledScope(true))
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

		// Attempt to load. Might not work out if e.g. for a single marker, pattern hasn't been
		// assigned yet, or for an NFT marker, dataset hasn't been specified.
		if (m.UID == ARXTrackable.NO_ID) m.Load();

		// Trackable tag
        m.Tag = EditorGUILayout.TextField("Trackable tag", m.Tag);
        EditorGUILayout.LabelField("UID", (m.UID == ARXTrackable.NO_ID ? "Not loaded": m.UID.ToString()));

        EditorGUILayout.Separator();

		// Trackable type
        ARXTrackable.TrackableType t = (ARXTrackable.TrackableType)EditorGUILayout.EnumPopup("Type", m.Type);

		// Description of the type of marker
        EditorGUILayout.LabelField("Description", ARXTrackable.TrackableTypeNames[m.Type]);

		switch (t) {

			case ARXTrackable.TrackableType.Square:
                {

					// For pattern markers, offer a popup with marker pattern file names.
					string chosenPatternFile = "";
					List<string> PatternFilenames = Directory.GetFiles(Path.Join(Application.dataPath, "Patterns")).Where(s => !s.EndsWith(".meta")).ToList();
					if (PatternFilenames.Count > 0)
					{
						List<string> popup = new List<string>(PatternFilenames.Select(p => Path.GetFileName(p)));
						popup.Insert(0, "Choose pattern file");
						int patternFilenameIndex = EditorGUILayout.Popup("Choose pattern file", 0, popup.ToArray());
						if (patternFilenameIndex != 0)
						{
							chosenPatternFile = PatternFilenames[patternFilenameIndex - 1];
						}
					}
					else
					{
						EditorGUILayout.LabelField("Choose pattern file", "No patterns available");
					}
					EditorGUILayout.LabelField("Pattern file", string.IsNullOrEmpty(chosenPatternFile) ? m.PatternFileName : chosenPatternFile);
					float patternWidth = EditorGUILayout.FloatField("Width", m.PatternWidth);
					m.UseContPoseEstimation = EditorGUILayout.Toggle("Cont. pose estimation", m.UseContPoseEstimation);

					if (m.Type != t || !string.IsNullOrEmpty(chosenPatternFile) || patternWidth != m.PatternWidth)
					{
						m.ConfigureAsSquarePattern(chosenPatternFile, patternWidth);
						EditorUtility.SetDirty(target);
					}
				}
				break;

			case ARXTrackable.TrackableType.SquareBarcode:
                {
					// For barcode markers, allow the user to specify the barcode ID.
					long barcodeID = EditorGUILayout.LongField("Barcode ID", (long)m.BarcodeID);
					if (barcodeID < 0) barcodeID = 0;
					if (ARXController.Instance)
					{
						long maxBarcodeID = barcodeCounts[ARXController.Instance.MatrixCodeType] - 1;
						if (barcodeID > maxBarcodeID) barcodeID = maxBarcodeID;
						EditorGUILayout.LabelField("(in range 0 to " + maxBarcodeID + ")");
					}

					float patternWidth = EditorGUILayout.FloatField("Width", m.PatternWidth);
					m.UseContPoseEstimation = EditorGUILayout.Toggle("Cont. pose estimation", m.UseContPoseEstimation);

					if (m.Type != t || (ulong)barcodeID != m.BarcodeID || patternWidth != m.PatternWidth)
					{
						m.ConfigureAsSquareBarcode((ulong)barcodeID, patternWidth);
						EditorUtility.SetDirty(target);
					}
				}
				break;

            case ARXTrackable.TrackableType.Multimarker:
                {
					string multiConfigFile = EditorGUILayout.TextField("Multimarker config.", m.MultiConfigFile);
					if (m.Type != t || multiConfigFile != m.MultiConfigFile)
					{
						m.ConfigureAsMultiSquare(multiConfigFile);
						EditorUtility.SetDirty(target);
					}
				}
				break;

            case ARXTrackable.TrackableType.NFT:
                {
					string nftDataSetName = EditorGUILayout.TextField("NFT dataset name", m.NFTDataName);
					if (m.Type != t || nftDataSetName != m.NFTDataName)
					{
						m.ConfigureAsNFT(nftDataSetName);
						EditorUtility.SetDirty(target);
					}

					float nftScale = EditorGUILayout.FloatField("NFT marker scalefactor", m.NFTScale);
					if (nftScale != m.NFTScale)
					{
						m.NFTScale = nftScale;
						EditorUtility.SetDirty(target);
					}
				}
				break;

            case ARXTrackable.TrackableType.TwoD:
                {
					string twoDImageFile = EditorGUILayout.TextField("Image file", m.TwoDImageFile);
					float twoDImageWidth = EditorGUILayout.FloatField("Image width", m.TwoDImageWidth);
					if (m.Type != t || twoDImageFile != m.TwoDImageFile || twoDImageWidth != m.TwoDImageWidth)
					{
						m.ConfigureAsTwoD(twoDImageFile, twoDImageWidth);
						EditorUtility.SetDirty(target);
					}
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

        //EditorGUILayout.BeginHorizontal();

        // Draw all the marker images
        if (m.Patterns != null) {
            for (int i = 0; i < m.Patterns.Length; i++) {
				float imageMinWidth = Math.Max(m.Patterns[i].imageSizeX, 32);
				float imageMinHeight = Math.Max(m.Patterns[i].imageSizeY, 32);
				GUILayout.Label(new GUIContent("Pattern " + i + ", " + m.Patterns[i].width.ToString("n3") + " m")); // n3 -> 3 decimal places.
				Rect r = EditorGUILayout.GetControlRect(false, imageMinHeight + 4.0f, GUILayout.MinWidth(4.0f + imageMinWidth));
				Rect r0 = new Rect(r.x + 2.0f, r.y + 2.0f, imageMinWidth, imageMinHeight);
				GUI.DrawTexture(r, m.Patterns[i].getTexture(), ScaleMode.ScaleToFit, false);
                //GUILayout.Label(new GUIContent("Pattern " + i + ", " + m.Patterns[i].width.ToString("n3") + " m", m.Patterns[i].getTexture()), GUILayout.ExpandWidth(false)); // n3 -> 3 decimal places.
            }
        }

        //EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

    }

}