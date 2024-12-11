/*
 *  ARToolKitPreprocessor.cs
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

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ARToolKitPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        const string cameraUsageDescRequired = "Camera usage description required, but not set in player settings.";
        const string cameraUsageDescDefault = "Required for AR tracking. Video is used in realtime, and is not stored or transmitted outside the app.";
        const bool cameraUsageDescFailIfMissing = false;

        BuildTarget platform = report.summary.platform;

        if (platform == BuildTarget.iOS)
        {
            string cameraUsageDescription = PlayerSettings.iOS.cameraUsageDescription;
            if (string.IsNullOrWhiteSpace(cameraUsageDescription)) {
                if (cameraUsageDescFailIfMissing)
                {
                    throw new BuildFailedException(cameraUsageDescRequired);
                }
                else
                {
                    Debug.LogWarning(cameraUsageDescRequired + "Set to default.");
                    PlayerSettings.iOS.cameraUsageDescription = cameraUsageDescDefault;
                }

            }
        }
        else if (platform == BuildTarget.StandaloneOSX)
        {
            string cameraUsageDescription = PlayerSettings.macOS.cameraUsageDescription;
            if (string.IsNullOrWhiteSpace(cameraUsageDescription))
            {
                if (cameraUsageDescFailIfMissing)
                {
                    throw new BuildFailedException(cameraUsageDescRequired);
                }
                else
                {
                    Debug.LogWarning(cameraUsageDescRequired + "Set to default.");
                    PlayerSettings.macOS.cameraUsageDescription = cameraUsageDescDefault;
                }
            }
        }
    }
}
