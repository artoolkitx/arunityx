using System;
using System.IO;
using UnityEditor;
using UnityEngine;

class ARToolKitPackager {
	const string MAIN_DIRECTORY = "artoolkitX-Unity";
	const string PLUGINS_DIRECTORY = "Plugins";
	const string STREAMINGASSETS_DIRECTORY = "StreamingAssets";

	public static void CreatePackage() {
        string version = Application.version;
        string productName = Application.productName;
        string packageName = "arunityX.unitypackage";
        bool release = false;
        string androidKeystorePath = "";
        string androidKeystorePassword = "";
        string androidKeystoreAlias = "";
        string androidKeystoreAliasPassword = "";
#if UNITY_EDITOR_OSX
        string androidPlaybackEnginePath = Path.GetDirectoryName(UnityEditor.EditorApplication.applicationPath) + "/PlaybackEngines/AndroidPlayer";
#else
        string androidPlaybackEnginePath = UnityEditor.EditorApplication.applicationContentsPath + "/PlaybackEngines/AndroidPlayer";
#endif
        string jdkRootPath = null;
        string sdkRootPath = null;
        string ndkRootPath = null;
        string gradlePath = null;

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            // Two-part parameters.
            if (i + 1 < args.Length)
            {
                if (args[i].ToLower() == "-arunityxversion")
                {
                    version = args[i + 1];
                }
                else if (args[i].ToLower() == "-arunityxpackagename")
                {
                    packageName = args[i + 1];
                }

                // Android tools stuff.
                else if (args[i].ToLower() == "-androidjdkpath")
                {
                    jdkRootPath = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidsdkpath")
                {
                    sdkRootPath = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidndkpath")
                {
                    ndkRootPath = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidgradlepath")
                {
                    gradlePath = args[i + 1];
                }

                // Android keystore stuff.
                else if (args[i].ToLower() == "-androidkeystorepath")
                {
                    androidKeystorePath = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidkeystorepassword")
                {
                    androidKeystorePassword = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidkeystorealias")
                {
                    androidKeystoreAlias = args[i + 1];
                }
                else if (args[i].ToLower() == "-androidkeystorealiaspassword")
                {
                    androidKeystoreAliasPassword = args[i + 1];
                }
            }

            // One-part parameters.
            if (args[i].ToLower() == "-arunityxrelease")
            {
                release = true;
            }
            if (args[i].ToLower() == "-androidjdkpathdefault")
            {
#if UNITY_EDITOR_WIN
                jdkRootPath = androidPlaybackEnginePath + "/Tools/OpenJDK/Windows";
#else
                jdkRootPath = androidPlaybackEnginePath + "/Tools/OpenJDK";
#endif
            }
            if (args[i].ToLower() == "-androidsdkpathdefault")
            {
                sdkRootPath = androidPlaybackEnginePath + "/SDK";
            }
            if (args[i].ToLower() == "-androidndkpathdefault")
            {
                ndkRootPath = androidPlaybackEnginePath + "/NDK";
            }
            if (args[i].ToLower() == "-androidgradlepathdefault")
            {
                gradlePath = androidPlaybackEnginePath + "/Tools/gradle";
            }

        }

        // Version.
        string[] versionCodes = version.Split(".");
        int versionCodeMajor = int.Parse(versionCodes[0]);
        int versionCodeMinor = versionCodes.Length > 1 ? int.Parse(versionCodes[1]) : 0;
        int versionCodeTiny = versionCodes.Length > 2 ? int.Parse(versionCodes[2]) : 0;
        int versionCodeBuild = 0;
        int versionCodeInt = Math.Min(99, versionCodeMajor) * 1000000 + Math.Min(99, versionCodeMinor) * 10000 + Math.Min(99, versionCodeTiny) * 100 + Math.Min(99, versionCodeBuild);
#if UNITY_ANDROID
            PlayerSettings.Android.bundleVersionCode = versionCodeInt;
#endif
#if UNITY_IOS
            PlayerSettings.iOS.buildNumber = version;
#endif

        // Unity build settings for a development vs production release.
        // (These could also be set as part of BuildOptions passed to BuildPipeline.BuildPlayer).
        EditorUserBuildSettings.allowDebugging = !release;
        EditorUserBuildSettings.development = !release;
        EditorUserBuildSettings.connectProfiler = false;

        // Android tool setup.
#if UNITY_ANDROID
        UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = jdkRootPath;
        UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = sdkRootPath;
        UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = ndkRootPath;
        UnityEditor.Android.AndroidExternalToolsSettings.gradlePath = gradlePath;
        if (androidKeystorePath.Length > 0 && androidKeystorePassword.Length > 0 && androidKeystoreAlias.Length > 0 && androidKeystoreAliasPassword.Length > 0)
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = androidKeystorePath;
            PlayerSettings.Android.keystorePass = androidKeystorePassword;
            PlayerSettings.Android.keyaliasName = androidKeystoreAlias;
            PlayerSettings.Android.keyaliasPass = androidKeystoreAliasPassword;
        }
        else
        {
            PlayerSettings.Android.useCustomKeystore = false;
        }
#endif // UNITY_ANDROID

		AssetDatabase.ExportPackage(AssetDatabase.GetAllAssetPaths(), packageName, ExportPackageOptions.Recurse);
	}
}
