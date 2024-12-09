/*
 *  ARXTrackedObjectEditor.cs
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

[CustomEditor(typeof(ARXTrackedObject))]
public class ARXTrackedObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
		ARXTrackedObject arto = (ARXTrackedObject)target;
		if (arto == null) return;

		this.serializedObject.Update();
		using (new EditorGUI.DisabledScope(true))
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("_trackableTag"), true);

		ARXTrackable trackable = arto.GetTrackable();
		EditorGUILayout.LabelField("Found trackable", trackable == null ? "no" : "yes");
		if (trackable != null) {
			string type = ARXTrackable.TrackableTypeNames[trackable.Type];
			EditorGUILayout.LabelField("Trackable UID", (trackable.UID != ARXTrackable.NO_ID ? trackable.UID.ToString() : "Not loaded") + " (" + type + ")");
		}

		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnTrackedObjectFoundActivateChildren"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnTrackedObjectFound"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnTrackedObjectTracked"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnTrackedObjectLostDeactivateChildren"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("OnTrackedObjectLost"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("secondsToRemainVisible"), true);
		EditorGUILayout.PropertyField(this.serializedObject.FindProperty("eventReceiver"), true);
		this.serializedObject.ApplyModifiedProperties();
	}
}
