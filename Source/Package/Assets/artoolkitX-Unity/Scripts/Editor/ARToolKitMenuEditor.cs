﻿/*
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
 *
 *  Author(s): Wally Young
 *
 */


using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class ARToolKitMenuEditor : MonoBehaviour {
    private const  string FIRST_RUN             = "artoolkit_first_run";
    private const  string MENU_PATH_BASE       = "artoolkitX";
    private const  string TOOLS_MENU_PATH       = MENU_PATH_BASE + "/Download Tools";
    private const  string TOOLS_UNITY_MESSAGE   = "To use artoolkitX, you will need tools to generate markers and calibrate your camera.\n" +
            "please select \"{0}\" from the menu to download those tools.";
    private const  string HOME_PAGE_URL         = "http://www.artoolkitx.org/";
    private const  string DOCUMENTATION_URL     = "https://github.com/artoolkitx/artoolkitx/wiki";
	private const  string COMMUNITY_URL         = "http://forums.artoolkitx.org/";
    private const  string SOURCE_URL            = "https://github.com/artoolkitx/artoolkitx";
    private const  string PLUGIN_SOURCE_URL     = "https://github.com/artoolkitx/arunityx";
	//private const  string TOOLS_URL             = "http://artoolkit.org/download-artoolkit-sdk#unity";
    private const  string VERSION               = MENU_PATH_BASE + "/artoolkitX for Unity Version 1.1.2";
    private const  string WINDOWS_UNITY_MESSAGE = "Thank you for choosing artoolkitX for Unity! " +
            "<b>artoolkitX requires the Microsoft C++ Redistributables to be installed on your system.</b>\n" +
            "Please select \"{0}\" from the menu above, and install the required packages.";
    private const  string GET_TOOLS_MESSAGE     = "artoolkitX for Unity Version 1.1.2! <b>To make your own markers, you'll need to download our tools.</b>\n" +
		"Please select {0} from menu above to download them.";

    static ARToolKitMenuEditor() {
        if (EditorPrefs.GetBool(FIRST_RUN, true)) {
            EditorPrefs.SetBool(FIRST_RUN, false);
            Debug.Log(string.Format(GET_TOOLS_MESSAGE, TOOLS_MENU_PATH));
#if UNITY_EDITOR_WIN
            Debug.Log(string.Format(WINDOWS_UNITY_MESSAGE, TOOLS_MENU_PATH));
#endif
        }
    }
    
    [MenuItem (VERSION, false, 0)]
    private static void Version() { }

	[MenuItem (VERSION, true, 0)]
	private static bool ValidateVersion() {
		return false;
	}

//    [MenuItem (TOOLS_MENU_PATH, false, 1)]
//    private static void DownloadTools() {
//        Application.OpenURL(TOOLS_URL);
//    }

    [MenuItem (MENU_PATH_BASE + "/Support/Home Page", false, 50)]
	private static void HomePage() {
        Application.OpenURL(HOME_PAGE_URL);
    }
    
    [MenuItem (MENU_PATH_BASE + "/Support/Documentation", false, 51)]
	private static void Documentation() {
        Application.OpenURL(DOCUMENTATION_URL);
    }
	[MenuItem (MENU_PATH_BASE + "/Build", false, 41)]
	private static void Documentdation() {
		ARToolKitPackager.CreatePackage();
	}
	[MenuItem (MENU_PATH_BASE + "/Support/Community Forums", false, 52)]
	private static void Community() {
        Application.OpenURL(COMMUNITY_URL);
    }
    
    [MenuItem (MENU_PATH_BASE + "/Source Code/View artoolkitX Source", false, 53)]
	private static void Source() {
        Application.OpenURL(SOURCE_URL);
    }
    
    [MenuItem (MENU_PATH_BASE + "/Source Code/View Unity Plugin Source", false, 54)]
	private static void PluginSource() {
        Application.OpenURL(PLUGIN_SOURCE_URL);
    }
}
