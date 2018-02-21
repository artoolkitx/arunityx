using UnityEditor;

class ARToolKitPackager {
	const string MAIN_DIRECTORY = "ARToolKit5-Unity";
	const string PLUGINS_DIRECTORY = "Plugins";
	const string STREAMINGASSETS_DIRECTORY = "StreamingAssets";

	public static void CreatePackage() {
		string[] args = System.Environment.GetCommandLineArgs();
		string fileName = args[args.Length-1];
		AssetDatabase.ExportPackage(
			AssetDatabase.GetAllAssetPaths(),
			fileName,
			UnityEditor.ExportPackageOptions.Recurse);
	}
}
