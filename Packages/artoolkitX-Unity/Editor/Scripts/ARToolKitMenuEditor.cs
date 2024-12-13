/*
 *  ARToolKitMenuEditor.cs
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
 *  Copyright 2023 Philip Lamb
 *
 *  Author(s): Wally Young, Philip Lamb.
 *
 */


using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ARToolKitMenuEditor : MonoBehaviour {
    private const string MENU_PATH_BASE       = "artoolkitX";
    private const string HOME_PAGE_URL         = "http://www.artoolkitx.org/";
    private const string DOCUMENTATION_URL     = "https://github.com/artoolkitx/artoolkitx/wiki";
	private const string COMMUNITY_URL         = "http://forums.artoolkitx.org/";
    private const string SOURCE_URL            = "https://github.com/artoolkitx/artoolkitx";
    private const string PLUGIN_SOURCE_URL     = "https://github.com/artoolkitx/arunityx";
    private const string TOOLS_URL             = "https://github.com/artoolkitx/artoolkitx/releases/latest";
    private const string VERSION               = "artoolkitX for Unity Version 1.3.1";
    private const string VERSION_MENU          = MENU_PATH_BASE + "/" + VERSION;
    private const string WELCOME_MESSAGE       = "Welcome to " + VERSION + ".";
    private static bool showDownloadTools = false;
    private const string GET_TOOLS_MESSAGE     = "To make your own square pictorial markers or calibrate your camera, you'll need to download the tools.";
    private const string WINDOWS_RUNTIME_MESSAGE = "artoolkitX requires the Microsoft C++ Redistributables to be installed on your system.";

    [MenuItem (VERSION_MENU, false, 0)]
    private static void Version() { }

    // Keeps the version menu item disabled.
	[MenuItem (VERSION_MENU, true, 0)]
	private static bool ValidateVersion() {
		return false;
	}

    [MenuItem(MENU_PATH_BASE + "/Open " + ARToolKitEditorSettingsProvider.ARToolKitEditorSettingsProviderTitle, false, 40)]
    private static void OpenSettings()
    {
        SettingsService.OpenProjectSettings(ARToolKitEditorSettingsProvider.ARToolKitEditorSettingsProviderPath);
    }

    [MenuItem(MENU_PATH_BASE + "/Downloads/Download SDK including tools", false, 50)]
    private static void DownloadTools()
    {
        Application.OpenURL(TOOLS_URL);
    }

    [MenuItem (MENU_PATH_BASE + "/Support/Home Page", false, 60)]
	private static void HomePage() {
        Application.OpenURL(HOME_PAGE_URL);
    }

    [MenuItem (MENU_PATH_BASE + "/Support/Documentation", false, 61)]
	private static void Documentation() {
        Application.OpenURL(DOCUMENTATION_URL);
    }

	[MenuItem (MENU_PATH_BASE + "/Support/Community Forums", false, 62)]
	private static void Community() {
        Application.OpenURL(COMMUNITY_URL);
    }

    [MenuItem (MENU_PATH_BASE + "/Source Code/View artoolkitX Source", false, 70)]
	private static void Source() {
        Application.OpenURL(SOURCE_URL);
    }

    [MenuItem (MENU_PATH_BASE + "/Source Code/View Unity Plugin Source", false, 71)]
	private static void PluginSource() {
        Application.OpenURL(PLUGIN_SOURCE_URL);
    }

    [InitializeOnLoad]
    class ARToolKitWelcomeWindow : EditorWindow
    {
        static ARToolKitWelcomeWindow()
        {
            EditorApplication.update += OnEditorStart;
        }

        static void OnEditorStart()
        {
            EditorApplication.update -= OnEditorStart;
            SerializedObject s = ARToolKitEditorSettings.GetSerializedSettings();
            SerializedProperty hideWelcome = s.FindProperty("m_hideWelcome");
            if (!s.FindProperty("m_hideWelcome").boolValue)
            {
                EditorWindow.GetWindow(typeof(ARToolKitWelcomeWindow));
            }
        }

        private void OnGUI()
        {
            SerializedObject s = ARToolKitEditorSettings.GetSerializedSettings();
            SerializedProperty hideWelcome = s.FindProperty("m_hideWelcome");

            titleContent = new GUIContent(MENU_PATH_BASE);

            GUI.skin.label.wordWrap = true;
            GUILayout.Label(WELCOME_MESSAGE);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("Click here to open " + ARToolKitEditorSettingsProvider.ARToolKitEditorSettingsProviderTitle))
            {
                OpenSettings();
            }

            if (showDownloadTools)
            {
                GUILayout.Label(GET_TOOLS_MESSAGE);
#if UNITY_EDITOR_WIN
                GUILayout.Label(WINDOWS_RUNTIME_MESSAGE);
#endif
                if (GUILayout.Button("Click here to download SDK including tools."))
                {
                    DownloadTools();
                }
            }

            GUILayout.FlexibleSpace();
            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            EditorGUILayout.PropertyField(hideWelcome, new GUIContent("Don't show this window again"), GUILayout.ExpandWidth(true));
            EditorGUIUtility.labelWidth = originalValue;

            s.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
