/*
 *  ARToolKitSampleDataHandler.cs
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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// So that samples can have their own local "StreamingAssets" folder, but still work
/// in the Unity Editor, make a copy of files in the local StreamingAssets to the
/// canonical folder, and for any copied files, add them to the gitignore file there
/// so that we don't end up with duplicates.
/// </summary>
[InitializeOnLoad]
public class ARToolKitSampleDataHandler
{
    static ARToolKitSampleDataHandler()
    {
        EditorApplication.hierarchyChanged += OnEditorHierachyChanged;
    }

    static void OnEditorHierachyChanged()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        string streamingAssetsIgnoreFile = Path.Join(streamingAssetsPath, ".gitignore");
        List<string> ignoreList = new List<string>();
        if (!Directory.Exists(streamingAssetsPath))
        {
            Directory.CreateDirectory(streamingAssetsPath);
        }
        else
        {
            if (File.Exists(streamingAssetsIgnoreFile))
            {
                ignoreList = File.ReadAllLines(streamingAssetsIgnoreFile).ToList();
            }
        }

        string currentSceneAssetsPath = Path.Join(Path.GetDirectoryName(SceneManager.GetActiveScene().path), "StreamingAssets");
        if (Directory.Exists(currentSceneAssetsPath))
        {
            bool writeIgnoreList = false;
            string[] sceneAssets = Directory.GetFiles(currentSceneAssetsPath).Where(s => !s.EndsWith(".meta")).ToArray();
            foreach (string sceneAssetSourcePath in sceneAssets)
            {
                string sceneAssetName = Path.GetFileName(sceneAssetSourcePath);
                string sceneAssetTargetPath = Path.Join(streamingAssetsPath, sceneAssetName);
                if (!File.Exists(sceneAssetTargetPath))
                {
                    File.Copy(sceneAssetSourcePath, sceneAssetTargetPath);
                    if (!ignoreList.Contains(sceneAssetName))
                    {
                        ignoreList.Add(sceneAssetName);
                        ignoreList.Add($"{sceneAssetName}.meta");
                        writeIgnoreList = true;
                    }
                }
            }
            if (writeIgnoreList)
            {
                File.WriteAllLines(streamingAssetsIgnoreFile, ignoreList);
            }
        }
    }
}
