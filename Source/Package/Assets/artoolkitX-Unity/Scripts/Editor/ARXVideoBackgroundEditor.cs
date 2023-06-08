/*
 *  ARXVideoBackgroundEditor.cs
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
 *  Copyright 2023 Philip Lamb.
 *
 *  Author(s): Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ARXVideoBackground))] 
public class ARXVideoBackgroundEditor : Editor 
{
	protected SerializedProperty BackgroundLayer;

    protected virtual void OnEnable()
    {
        BackgroundLayer = serializedObject.FindProperty("BackgroundLayer");
    }

    public override void OnInspectorGUI()
    {
		ARXVideoBackground arvb = (ARXVideoBackground)target;
		if (arvb == null) return;

		this.serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

        bool u = EditorGUILayout.Toggle("Use this video background", arvb.UseVideoBackground);
        if (u != arvb.UseVideoBackground)
        {
            Undo.RecordObject(arvb, (u ? "En" : "Dis") + "able video background");
            arvb.UseVideoBackground = u;
        }
        ARCamera arc = arvb.gameObject.GetComponent<ARCamera>();
        if (arc && u & arc.Optical)
        {
            EditorGUILayout.HelpBox("This video background is enabled, even though the ARCamera is in optical see-through mode.", MessageType.Warning);
        }

        BackgroundLayer.intValue = EditorGUILayout.LayerField("Layer", BackgroundLayer.intValue);

		this.serializedObject.ApplyModifiedProperties();
	}
}
