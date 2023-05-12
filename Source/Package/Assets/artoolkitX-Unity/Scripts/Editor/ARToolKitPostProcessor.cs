/*
 *  ARToolKitPostProcessor.cs
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
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.Android;
using System.Xml;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;
using UnityEditor.iOS.Xcode;

public class ARToolKitPostProcessor : IPostprocessBuildWithReport
{
#if UNITY_STANDALONE_WIN
    private const  string   EXE           = ".exe";
	private const  string   RELATIVE_PATH = "{0}_Data/Plugins/";
	private static string[] REDIST_FILES  = { "pthreadVC2.dll", "vcredist.exe" };
	private const string FILE_NAME_STATUS = "ARToolKit Post Process Build Player: Operating of file {0}.";
#elif UNITY_IPHONE

    // Framework name, weakref.
    private static Tuple<string, bool>[] IosFrameworks =
    {
        //new Tuple<string, bool>("Accelerate.framework", false),
    };

    // Setting name, value to add.
    private static Tuple<string, string>[] IosBuildValues = {
        new Tuple<string, string>("OTHER_LDFLAGS", "-lsqlite3"),
    };

    private static StreamWriter streamWriter = null;
#endif

    int IOrderedCallback.callbackOrder => int.MaxValue;

    public void OnPostprocessBuild(BuildReport report) {
        BuildTarget target = report.summary.platform;
        string appPath = report.summary.outputPath;

#if UNITY_STANDALONE_WIN
		string[] pathSplit     = appPath.Split('/');
		string   fileName      = pathSplit[pathSplit.Length - 1];
		string   pathDirectory = appPath.TrimEnd(fileName.ToCharArray());
		Debug.Log(string.Format(FILE_NAME_STATUS, fileName));
		fileName = fileName.Trim(EXE.ToCharArray());
		
		string fromPath = Path.Combine(pathDirectory, string.Format(RELATIVE_PATH, fileName));
		if (Directory.Exists(string.Format(RELATIVE_PATH, fileName))) {
			Debug.LogError("ARTOOLKIT BUILD ERROR: Couldn't find data directory!");
			Debug.LogError("Please move DLLs from [appname]_data/Plugins to the same directory as the exe!");
			return;
		}

		// Error when copying to remote drives.
		if (fromPath.StartsWith ("//")) {
			fromPath = fromPath.Remove(0, 1);
		}

		foreach (string redistFile in REDIST_FILES) {
			File.Move(Path.Combine(fromPath, redistFile), Path.Combine(pathDirectory, redistFile));
		}
	}
#elif UNITY_IPHONE
        const string LOGFILE_NAME = "postprocess.log";

        string logPath = Path.Combine(appPath, LOGFILE_NAME);
        if (target != BuildTarget.iOS) {
            Debug.LogError("ARToolKitPostProcessor::OnIosPostProcess - Called on non iOS build target!");
            return;
        } else if (File.Exists(logPath)) {
			streamWriter = new StreamWriter(logPath, true);
			streamWriter.WriteLine("OnIosPostProcess - Beginning iOS post-processing.");
			streamWriter.WriteLine("OnIosPostProcess - WARNING - Attempting to process directory that has already been processed. Skipping.");
			streamWriter.WriteLine("OnIosPostProcess - Aborted iOS post-processing.");
			streamWriter.Close();
			streamWriter = null;
        } else {
            streamWriter = new StreamWriter(logPath);
            streamWriter.WriteLine("OnIosPostProcess - Beginning iOS post-processing.");

            try {

                string pbxprojPath = PBXProject.GetPBXProjectPath(appPath);
                if (File.Exists(pbxprojPath)) {
                    PBXProject project = new PBXProject();
                    project.ReadFromFile(pbxprojPath);

                    string g = project.GetUnityFrameworkTargetGuid();

                    streamWriter.WriteLine("OnIosPostProcess - Modifying file at " + pbxprojPath);

                    foreach (var f in IosFrameworks)
                    {
                        project.AddFrameworkToProject(g, f.Item1, f.Item2);
                    }

                    foreach (var bv in IosBuildValues)
                    {
                        project.AddBuildProperty(g, bv.Item1, bv.Item2);
                        //project.UpdateBuildProperty(g, bv.Item1, new string[] { bv.Item2 }, new string[] { });
                    }

                    project.WriteToFile(pbxprojPath);

                    streamWriter.WriteLine("OnIosPostProcess - Ending iOS post-processing successfully.");
                } else {
                    streamWriter.WriteLine("OnIosPostProcess - ERROR - File " + pbxprojPath + " does not exist!");
                }
            } catch (System.Exception e) {
                streamWriter.WriteLine("ProcessSection - ERROR - " + e.Message + " : " + e.StackTrace);

            } finally {
                streamWriter.Close();
                streamWriter = null;
            }
        }
#endif
    }
}

class ARToolKitAndroidManifestPostProcessor : IPostGenerateGradleAndroidProject
{
    const string k_AndroidUri = "http://schemas.android.com/apk/res/android";

    const string k_AndroidManifestPath = "/src/main/AndroidManifest.xml";

    XmlNode FindFirstChild(XmlNode node, string tag)
    {
        if (node.HasChildNodes)
        {
            for (int i = 0; i < node.ChildNodes.Count; ++i)
            {
                var child = node.ChildNodes[i];
                if (child.Name == tag)
                    return child;
            }
        }

        return null;
    }

    void AppendNewAttribute(XmlDocument doc, XmlElement element, string attributeName, string attributeValue)
    {
        var attribute = doc.CreateAttribute(attributeName, k_AndroidUri);
        attribute.Value = attributeValue;
        element.Attributes.Append(attribute);
    }

    void FindOrCreateTagWithAttribute(XmlDocument doc, XmlNode containingNode, string tagName,
        string attributeName, string attributeValue)
    {
        if (containingNode.HasChildNodes)
        {
            for (int i = 0; i < containingNode.ChildNodes.Count; ++i)
            {
                var child = containingNode.ChildNodes[i];
                if (child.Name == tagName)
                {
                    var childElement = child as XmlElement;
                    if (childElement != null && childElement.HasAttributes)
                    {
                        var attribute = childElement.GetAttributeNode(attributeName, k_AndroidUri);
                        if (attribute != null && attribute.Value == attributeValue)
                            return;
                    }
                }
            }
        }

        // Didn't find it, so create it
        var element = doc.CreateElement(tagName);
        AppendNewAttribute(doc, element, attributeName, attributeValue);
        containingNode.AppendChild(element);
    }

    void FindOrCreateTagWithAttributes(XmlDocument doc, XmlNode containingNode, string tagName,
        string firstAttributeName, string firstAttributeValue, string secondAttributeName, string secondAttributeValue)
    {
        if (containingNode.HasChildNodes)
        {
            for (int i = 0; i < containingNode.ChildNodes.Count; ++i)
            {
                var childNode = containingNode.ChildNodes[i];
                if (childNode.Name == tagName)
                {
                    var childElement = childNode as XmlElement;
                    if (childElement != null && childElement.HasAttributes)
                    {
                        var firstAttribute = childElement.GetAttributeNode(firstAttributeName, k_AndroidUri);
                        if (firstAttribute == null || firstAttribute.Value != firstAttributeValue)
                            continue;

                        var secondAttribute = childElement.GetAttributeNode(secondAttributeName, k_AndroidUri);
                        if (secondAttribute != null)
                        {
                            secondAttribute.Value = secondAttributeValue;
                            return;
                        }

                        // Create it
                        AppendNewAttribute(doc, childElement, secondAttributeName, secondAttributeValue);
                        return;
                    }
                }
            }
        }

        // Didn't find it, so create it
        var element = doc.CreateElement(tagName);
        AppendNewAttribute(doc, element, firstAttributeName, firstAttributeValue);
        AppendNewAttribute(doc, element, secondAttributeName, secondAttributeValue);
        containingNode.AppendChild(element);
    }

    // This ensures the Android Manifest includes support for the camera.
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // TODO: add a check as to whether ARX is enabled.
        //if (!ARX.enabled)
        //    return;

        Debug.Log("Inserting artoolkitX required attributes into AndroidManifest.xml");

        string manifestPath = path + k_AndroidManifestPath;
        var manifestDoc = new XmlDocument();
        manifestDoc.Load(manifestPath);

        var manifestNode = FindFirstChild(manifestDoc, "manifest");
        if (manifestNode == null)
            return;

        var applicationNode = FindFirstChild(manifestNode, "application");
        if (applicationNode == null)
            return;

        FindOrCreateTagWithAttribute(manifestDoc, manifestNode, "uses-permission", "name", "android.permission.CAMERA");
        FindOrCreateTagWithAttribute(manifestDoc, manifestNode, "uses-permission", "name", "android.permission.ACCESS_NETWORK_STATE");
        FindOrCreateTagWithAttribute(manifestDoc, manifestNode, "uses-permission", "name", "android.permission.INTERNET");
        FindOrCreateTagWithAttributes(manifestDoc, applicationNode, "meta-data", "name", "unityplayer.SkipPermissionsDialog", "value", "true");
        FindOrCreateTagWithAttributes(manifestDoc, manifestNode, "uses-feature", "name", "android.hardware.camera.any", "required", "true");
        FindOrCreateTagWithAttributes(manifestDoc, manifestNode, "uses-feature", "name", "android.hardware.camera", "required", "false");
        FindOrCreateTagWithAttributes(manifestDoc, manifestNode, "uses-feature", "name", "android.hardware.camera.autofocus", "required", "false");

        manifestDoc.Save(manifestPath);
    }

    void DebugPrint(XmlDocument doc)
    {
        var sw = new System.IO.StringWriter();
        var xw = XmlWriter.Create(sw);
        doc.Save(xw);
        Debug.Log(sw);
    }

    public int callbackOrder => 2;
}

