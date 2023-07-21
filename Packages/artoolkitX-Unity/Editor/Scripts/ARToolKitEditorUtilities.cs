/*
 *  ARToolKitEditorUtilities.cs
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
using System.Collections.Generic;
using System.Linq;

public class ARToolKitEditorUtilities
{
    public enum AddOrRemove
    {
        Add = 0,
        Remove
    }

    public static bool HasScriptingDefine(string define)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        return allDefines.Contains(define);
    }

    public static void ChangeScriptingDefine(string define, AddOrRemove addOrRemove)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        bool changed = false;
        if (addOrRemove == AddOrRemove.Add && !allDefines.Contains(define))
        {
            allDefines.Add(define);
            changed = true;
        }
        else if (addOrRemove == AddOrRemove.Remove && allDefines.Contains(define))
        {
            allDefines.Remove(define);
            changed = true;

        }
        if (changed)
        {
            definesString = string.Join(";", allDefines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, definesString);
        }
    }
}
