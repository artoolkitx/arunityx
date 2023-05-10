/*
 *  ARVideoConfigEditor.cs
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
 *  Copyright 2023 Philip Lamb
 *
 *  Author(s): Philip Lamb
 *
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

[CustomEditor(typeof(ARVideoConfig))]
public class ARVideoConfigEditor : Editor
{
    bool[] showPlatformConfig = null;

    public override void OnInspectorGUI()
    {
        ARVideoConfig arvideoconfig = (ARVideoConfig)target;
        if (arvideoconfig == null) return;

        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

        bool needSave = false;

        // Init the per-platform turndown arrows.
        if (showPlatformConfig == null)
        {
            showPlatformConfig = new bool[arvideoconfig.configs.Count];
            for (int i = 0; i < arvideoconfig.configs.Count; i++)
            {
                if (arvideoconfig.configs[i].platform == Application.platform) {
                    showPlatformConfig[i] = true;
                }
            }
        }

        for (int pci = 0; pci < arvideoconfig.configs.Count; pci++)
        {
            // Copy the config entry from the list.
            ARVideoConfig.ARVideoPlatformConfig pc = arvideoconfig.configs[pci];

            showPlatformConfig[pci] = EditorGUILayout.Foldout(showPlatformConfig[pci], pc.name);
            if (!showPlatformConfig[pci]) continue;

            EditorGUI.BeginDisabledGroup(pc.isUsingManualConfig);

            Func<Enum, bool> checkSelectionMethod = (e) => {
                ARVideoConfig.ARVideoConfigInputSelectionMethod ism = (ARVideoConfig.ARVideoConfigInputSelectionMethod)e;
                if (!ARVideoConfig.modules[pc.module].supportsSelectionByIndex
                    && (ism == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCamera || ism == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCameraAtPosition))
                {
                    return false;
                }
                else if (!ARVideoConfig.modules[pc.module].supportsSelectionByPosition
                    && (ism == ARVideoConfig.ARVideoConfigInputSelectionMethod.CameraAtPosition || ism == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCameraAtPosition))
                {
                    return false;
                }
                return true;
            };
            ARVideoConfig.ARVideoConfigInputSelectionMethod tempIsm = (ARVideoConfig.ARVideoConfigInputSelectionMethod)EditorGUILayout.EnumPopup(new GUIContent("Select input by"), pc.inputSelectionMethod, checkSelectionMethod, false);
            if (tempIsm != pc.inputSelectionMethod)
            {
                pc.inputSelectionMethod = tempIsm;
                needSave = true;
            }

            // If we should put up a source info list, and we're on that platform, do it.
            if (pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.VideoSourceInfoList && pc.platform == Application.platform)
            {
                if (arvideoconfig.sourceInfoList == null || GUILayout.Button("Refresh"))
                {
                    if (arvideoconfig.arcontroller != null)
                    {
                        arvideoconfig.sourceInfoList = arvideoconfig.arcontroller.GetVideoSourceInfoList(ARVideoConfig.modules[pc.module].moduleSelectionString);
                    }
                }
                if (arvideoconfig.sourceInfoList != null && arvideoconfig.sourceInfoList.Count > 0)
                {
                    List<string> openTokens = arvideoconfig.sourceInfoList.Select(si => si.open_token).ToList();
                    int i = openTokens.IndexOf(pc.VideoSourceInfoListOpenToken);
                    i = i < 0 ? 0 : i; // If current option not found (i == -1), revert to first option in list.
                    i = EditorGUILayout.Popup("Video source:", i, arvideoconfig.sourceInfoList.Select(si => $"{si.name} ({si.model})").ToArray());
                    if (pc.VideoSourceInfoListOpenToken != arvideoconfig.sourceInfoList[i].open_token)
                    {
                        pc.VideoSourceInfoListOpenToken = arvideoconfig.sourceInfoList[i].open_token;
                        needSave = true;
                    }
                }
            }
            if (pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.CameraAtPosition || pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCameraAtPosition)
            {
                Func<Enum, bool> checkPosition = (e) =>
                {
                    ARController.AR_VIDEO_POSITION p = (ARController.AR_VIDEO_POSITION)e;
                    if (p == ARController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK || p == ARController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT) return true;
                    else if (p == ARController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_OTHER && pc.platform == RuntimePlatform.Android) return true;
                    return false;
                };
                ARController.AR_VIDEO_POSITION tempPos = (ARController.AR_VIDEO_POSITION)EditorGUILayout.EnumPopup(new GUIContent("Camera position"), pc.position, checkPosition, false);
                if (tempPos != pc.position)
                {
                    pc.position = tempPos;
                    needSave = true;
                }
            }
            if (pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCamera || pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCameraAtPosition)
            {
                int tempIndex = EditorGUILayout.IntField("Camera number", pc.index + 1) - 1;
                if (tempIndex != pc.index)
                {
                    pc.index = tempIndex;
                    needSave = true;
                }
            }

            switch (ARVideoConfig.modules[pc.module].sizeSelectionStrategy)
            {
                case ARVideoConfig.ARVideoSizeSelectionStrategy.AVFoundationPreset:
                    var presets = ARVideoConfig.AVFoundationPresets.Where(kv => pc.platform == RuntimePlatform.IPhonePlayer ? kv.Value.availableiOS : kv.Value.availableMac).ToList();
                    int pi = presets.FindIndex(p => p.Key == pc.AVFoundationPreset);
                    pi = pi < 0 ? 0 : pi;
                    int tempPresetIndex = EditorGUILayout.Popup("Size preset", pi, presets.Select(p => p.Value.description).ToArray());
                    if (tempPresetIndex != pi)
                    {
                        pc.AVFoundationPreset = presets[tempPresetIndex].Key;
                        needSave = true;
                    }
                    break;
                case ARVideoConfig.ARVideoSizeSelectionStrategy.SizePreference:
                    var keys = ARVideoConfig.SizePreferences.Keys.ToList();
                    int spi = keys.FindIndex(p => p == pc.sizePreference);
                    spi = spi < 0 ? 0 : spi;
                    int tempSizePrefIndex = EditorGUILayout.Popup("Size preference", spi, ARVideoConfig.SizePreferences.Values.Select(sp => sp.description).ToArray());
                    if (tempSizePrefIndex != spi)
                    {
                        pc.sizePreference = keys[tempSizePrefIndex];
                        needSave = true;
                    }
                    if (ARVideoConfig.SizePreferences[pc.sizePreference].usesWidthAndHeightFields)
                    {
                        goto case ARVideoConfig.ARVideoSizeSelectionStrategy.WidthAndHeight;
                    }
                    break;
                case ARVideoConfig.ARVideoSizeSelectionStrategy.WidthAndHeight:
                    EditorGUILayout.BeginHorizontal();
                    int tempWidth = EditorGUILayout.IntField("Width", pc.width);
                    int tempHeight = EditorGUILayout.IntField("Height", pc.height);
                    EditorGUILayout.EndHorizontal();
                    if (tempWidth != pc.width || tempHeight != pc.height)
                    {
                        pc.width = tempWidth;
                        pc.height = tempHeight;
                        needSave = true;
                    }
                    break;
                case ARVideoConfig.ARVideoSizeSelectionStrategy.None:
                default:
                    break;
            }


            EditorGUI.EndDisabledGroup();

            bool tempUmc = EditorGUILayout.Toggle("Manually override", pc.isUsingManualConfig);
            if (tempUmc != pc.isUsingManualConfig)
            {
                pc.isUsingManualConfig = tempUmc;
                needSave = true;
            }

            // If using manual config, allow user to edit it, otherwise show autoconfig string.
            string tempMc = EditorGUILayout.TextField("Video config.", pc.isUsingManualConfig ? pc.manualConfig : arvideoconfig.GetVideoConfigStringForPlatform(pc.platform));
            if (pc.isUsingManualConfig && tempMc != pc.manualConfig)
            {
                pc.manualConfig = tempMc;
                needSave = true;
            }

            // Save the changes back to the list.
            if (needSave)
            {
                arvideoconfig.configs[pci] = pc;
                EditorUtility.SetDirty(target);
            }
        } // foreaach platform
    } // OnInspectorGUI
}