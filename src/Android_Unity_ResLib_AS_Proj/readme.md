# Generating the Android Resource Library File for Unity 5 Android Platform Build Process
#### Written by John Wolf
***

As of Unity 5, when building a Unity app for the Android platform, the Unity IDE build process no longer supports having an Android resources laid out in directories under the "res" directory in the Unity "Assets/Plugins/Android/" directory. Instead, the Android resources are required to be built into a ".aar" file; a binary distribution of an Android Library Project. A ".aar" file can contain the following entries:

- /AndroidManifest.xml (mandatory)
- /classes.jar (mandatory)
- /res/ (mandatory)
- /R.txt (mandatory)
- /assets/ (optional)
- /libs/*.jar (optional)
- /jni/\<abi\>/*.so (optional)
- /proguard.txt (optional)
- /lint.jar (optional)

A Unity forum question somewhat explains the requirement [here](http://forum.unity3d.com/threads/obsolete-providing-android-resources-in-assets-plugins-android-res-is-deprecated.315889/).

### Edit and Build the ".aar" Android Resource Library File

To edit and build the Android resource library, first open the Android Studio project of \[ARTK_for_Unity5 repo\]/src/Android_Unity_ResLib_AS_Proj with the latest available version of the Android Studio IDE. As of this writing, version 1.5.1 was used. In the IDE, in the "Project" tab, "Android" view, twirl open the "ResLib" folder icon. Under this icon are the AndroidManifest.xml file and "res" Android resource folder. Update the .xml files under the "ResLib" folder icon as required.

When the edits are complete and the resource library is ready to be built, first select the build variant , "debug" or "release," to build. The Unity Android platform build process will consume a debug or release variant ".aar" library file, however, there is probably no need to build a debug variant of the resource library so select the "release" variant. After opening the "Build Variants" tab, in the "Build Variants" window under the "Build Variant" heading, click to select "release".

As of Android Studio 1.5.1, simply switching the build variant to "release" and doing a "Build"/"Make Project" or "Build"/"Rebuild Project" does not generate the release version of the ".aar" library file. (If you know how to fix this, please let me know). To get around this, click and open the "Gradle" tab located in the upper-right handside margin of the Android Studio IDE main window. In the opened "Gradle projects" window, twirl open "Android_Unity_ResLib_AS_Proj" icon and the "Android_Unity_ResLib_AS_Proj" sub-icon. Then twirl open the "Tasks" icon followed by twirling open the "build" icon. Under the open "build" icon, double click to execute the "assembleRelease" gradle task. This will generate the release version of the ".aar"  library named: "ResLib-release.aar."

The full path of the generated Android resource library file is:

<pre>
[ARTK_for_Unity5 repo]/src/Android_Unity_ResLib_AS_Proj/
    ResLib/build/outputs/aar/ResLib-release.aar
</pre>

### Deploy the .aar Android Resource Library File

After generating the ".aar" file, copy the just generated "ResLib-release.aar" file from the path specified above and paste the file in the following directory and overwrite the previous version of the file:

<pre>
[ARTK_for_Unity5 repo]/src/Unity/Assets/Plugins/Android/
</pre>

The "ResLib-release.aar" file in the above directory is git tracked so git add/commit/push the updated ".aar" library file.

Task completed.


