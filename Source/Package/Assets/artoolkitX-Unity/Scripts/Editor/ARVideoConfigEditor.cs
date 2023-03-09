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
    public override void OnInspectorGUI()
    {
        ARVideoConfig arvideoconfig = (ARVideoConfig)target;
        if (arvideoconfig == null) return;

        for (int pci = 0; pci < arvideoconfig.configs.Count; pci++)
        {
            // Copy the config entry from the list.
            ARVideoConfig.ARVideoPlatformConfig pc = arvideoconfig.configs[pci];

            EditorGUILayout.LabelField(pc.name);

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
            pc.inputSelectionMethod = (ARVideoConfig.ARVideoConfigInputSelectionMethod)EditorGUILayout.EnumPopup(new GUIContent("Select input by"), pc.inputSelectionMethod, checkSelectionMethod, false);

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
                    pc.VideoSourceInfoListOpenToken = arvideoconfig.sourceInfoList[i].open_token;
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
                pc.position = (ARController.AR_VIDEO_POSITION)EditorGUILayout.EnumPopup(new GUIContent("Camera position"), pc.position, checkPosition, false);
            }
            if (pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCamera || pc.inputSelectionMethod == ARVideoConfig.ARVideoConfigInputSelectionMethod.NthCameraAtPosition)
            {
                pc.index = EditorGUILayout.IntField("Camera number", pc.index + 1) - 1;
            }

            EditorGUI.EndDisabledGroup();

            pc.isUsingManualConfig = EditorGUILayout.Toggle("Manually override", pc.isUsingManualConfig);

            EditorGUI.BeginDisabledGroup(!pc.isUsingManualConfig);
            if (pc.isUsingManualConfig)
            {
                pc.manualConfig = EditorGUILayout.TextField("Video config.", pc.manualConfig);
            }
            else
            {
                EditorGUILayout.TextField("Video config.", arvideoconfig.GetVideoConfigStringForPlatform(pc.platform));
            }
            EditorGUI.EndDisabledGroup();


            // Save the changes back to the list.
            arvideoconfig.configs[pci] = pc;
        } // foreaach platform
    } // OnInspectorGUI
}