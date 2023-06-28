/*
 *  ARToolKitEditorSettings.cs
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

using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using System;

class ARToolKitEditorSettings : ScriptableObject
{
    public const string k_ARToolKitEditorSettingsPath = "Assets/Editor/ARToolKitEditorSettings.asset";

#pragma warning disable CS0414
    [SerializeField]
    [Tooltip("Hide or show the welcome message each time the project is opened.")]
    private bool m_hideWelcome;

    [SerializeField]
    [Tooltip("Check to allow use of Unity-based video providers. Setting this value will enable 'unsafe' mode in the project, which is required to pass video efficiently to the ARX plugin.")]
    private bool m_allowUnityVideoProviders;
#pragma warning restore CS0414

    internal static ARToolKitEditorSettings GetOrCreateSettings()
    {
        ARToolKitEditorSettings settings = AssetDatabase.LoadAssetAtPath<ARToolKitEditorSettings>(k_ARToolKitEditorSettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<ARToolKitEditorSettings>();
            settings.m_hideWelcome = false;
            settings.m_allowUnityVideoProviders = false;
            string editorAssetPath = Path.GetDirectoryName(k_ARToolKitEditorSettingsPath);
            if (!Directory.Exists(editorAssetPath))
            {
                Directory.CreateDirectory(editorAssetPath);
            }
            AssetDatabase.CreateAsset(settings, k_ARToolKitEditorSettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
}

class ARToolKitEditorSettingsProvider : SettingsProvider
{
    public const string ARToolKitEditorSettingsProviderPath = "Project/ARToolKitEditorSettingsProvider";
    public const string ARToolKitEditorSettingsProviderTitle = "artoolkitX Editor Settings";
    private SerializedObject m_CustomSettings;

    class Styles
    {
        public static GUIContent hideWelcome = new GUIContent("Hide welcome message");
        public static GUIContent allowUnityVideoProviders = new GUIContent("Allow Unity-based video providers");
    }

    public ARToolKitEditorSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
        : base(path, scope) { }

    public static bool IsSettingsAvailable()
    {
        return File.Exists(ARToolKitEditorSettings.k_ARToolKitEditorSettingsPath);
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        // This function is called when the user clicks on the MyCustom element in the Settings window.
        m_CustomSettings = ARToolKitEditorSettings.GetSerializedSettings();
    }

    public override void OnGUI(string searchContext)
    {
        SerializedProperty hideWelcome = m_CustomSettings.FindProperty("m_hideWelcome");
        SerializedProperty allowUnityVideoProviders = m_CustomSettings.FindProperty("m_allowUnityVideoProviders");

        float originalValue = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 280;
        EditorGUILayout.PropertyField(hideWelcome, Styles.hideWelcome);
        EditorGUILayout.PropertyField(allowUnityVideoProviders, Styles.allowUnityVideoProviders);
        EditorGUIUtility.labelWidth = originalValue;

        m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();

        EditorApplication.delayCall += PostSettingsActions;
    }

    private void PostSettingsActions()
    {
        if (m_CustomSettings.FindProperty("m_allowUnityVideoProviders").boolValue)
        {
            ARToolKitEditorUtilities.ChangeScriptingDefine("ARX_ALLOW_UNITY_VIDEO_PROVIDERS", ARToolKitEditorUtilities.AddOrRemove.Add);
            try
            {
                PlayerSettings.allowUnsafeCode = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error {e} setting PlayerSettings.allowUnsafeCode");
            }
        }
        else
        {
            ARToolKitEditorUtilities.ChangeScriptingDefine("ARX_ALLOW_UNITY_VIDEO_PROVIDERS", ARToolKitEditorUtilities.AddOrRemove.Remove);
        }
    }

    // Register the SettingsProvider
    [SettingsProvider]
    public static SettingsProvider CreateARToolKitEditorSettingsProvider()
    {
        ARToolKitEditorSettingsProvider provider = new ARToolKitEditorSettingsProvider(ARToolKitEditorSettingsProviderPath, SettingsScope.Project);

        // Automatically extract all keywords from the Styles.
        provider.label = ARToolKitEditorSettingsProviderTitle;
        provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
        return provider;
    }
}
