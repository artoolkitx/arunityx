# artoolkitX for Unity Release Notes
------------------------------------

## Version 1.2.9
### 2023-10-11

 * Update artoolkitX to v1.1.6.
 * Bug fix: Ensure ARXTrackedObject runs its "On Lost" routine if its ARXTrackable is reset.

## Version 1.2.8
### 2023-10-04

 * Update artoolkitX to v1.1.5.
 * Add Found/Lost events to ARXTrackable. Usually, you'll want to link to found/lost events on ARXTrackedObject instead, but in the event you need to dynamically create the ARXTrackedObject at runtime in response to an ARXTrackable being detected, you can use these events to achieve that.
 * ARController.StartAR is now directly callable again. (If you want to invoke it as a Coroutine instead, use the renamed StartARCo.)

## Version 1.2.7
### 2023-10-04

 * Minor update to add option to enable/disable automatic showing/hiding of child objects of ARXTrackedObject on trackable found/lost events.

## Version 1.2.6
### 2023-09-11

 * Update to artoolkitX v1.1.14 to fix iOS linkage issue that was causing conflict with Unity's use of libjpeg and minizip.
 * Ensure callbacks have correct types for use on AOT platforms.
 * Setting hideflags was preventing auto-created trackables being found by FindObjectsOfType e.g. in ARXTrackedObject, ARXOrigin, ARXCamera.
 * Updated ARXTrackable to not attempt to display barcode imagery for globalID barcodes. These are available online only.

## Version 1.2.5
### 2023-09-11

 * Adds static UnityEvent hooks in ARXTrackable for auto-created/removed trackables.
 * Auto-created trackables will now fetch correct barcode ID and width.
 * Corrects in-editor display of matrix (barcode) trackables patterns.
 * Added missing printable images for cube marker sample.

## Version 1.2.4
### 2023-08-04

 * Internal version (skipped).

## Version 1.2.3
### 2023-07-26

 * Fixed inclusion of mac OS bundle metafiles that had prevented correct importing of the native plugin.

## Version 1.2.2
### 2023-07-03

 * Fixed issues affecting first-run of newly-checked out repository.
 * Fixed unsafe code flag now set inside assembly-only, and not entire project.
 * Fixed Samples not working inside Unity Editor by adding a class that copies samples' local StreamingAssets to the main StreamingAssets folder.

## Version 1.2.1
### 2023-07-03

 * Fixed plugin packaging for mac OS.

## Version 1.2.1
### 2023-06-29

 * Minor change for iOS: libARX.a is prelinked and no longer requires libcurl.a or opencv2.framework in the plugins folder.

## Version 1.2
### 2023-06-29

 * No API changes, but a major change in package layout to allow for UPM packaging.
 * Added package.json and asmdefs for scripts.
 * Path-handling changes in various places to handle new layout incuding build.sh, ARXTrackableEditor, git config,
 * Removed obsolete packaging left over from ARToolKit for Unity v5.x.
 * Added some docs, and moved some (including Release Notes.txt to CHANGELOG.md) so they're accessible in the package editor.
 * Moved marker images into individual samples.
 * Known issue: the asset packaging script is currently out-of-date. Asset packaging will be added back in a future release.

## Version 1.1.11
### 2023-06-14

 * Major change in naming to new "ARX" prefix on components, to help disambiguate when using Unity's AR components.
 * Major refactor of video background handling, out of ARXController and into new ARXVideoBackground component, which attaches to an ARXCamera.
 * ARXController has new events for video start/frame/stop, which are used to init ARXCamera and ARXVideoBackground.
 * Video background no longer scales via its camera viewport (pixelrect) but scales itself via ortho projection, and it will respect Camera viewport and near/far clipping plane distances. With this change, the "fill" mode on the video background is *finally* supported.
 * Removed "ClearCamera" and merge into ARXVideoBackground, or its attached Camera when none..
 * Minor renaming of and removal of duplicated sample scenes.
 * Minor improvements to ARXController.Log function. Now won't keep on-screen logs in non-development builds.
 * Better singleton implementation in ARXController.
 * Added android logcat package.
 * Script order is now set via [DefaultExecutionOrder] on scripts, so shouldn't be necessary to manually set script execution order.
 * Restored functionality in the Fisticuffs example.
 * Fixed setting log level on ARXController.
 * Fixed local artoolkitX debug build cmd-line switch.
 * Allow quit to stop play in Editor.
 * Fixed handling of camera position string in video config.
 * Improved display of video config for target platform.

## Version 1.1.10
### 2023-05-18

* Support for asynchronous tracking (on a secondary thread) has been added to the 2D tracker. When enabled, the tracking rate can run slower than the video capture frame rate. This results in increased smoothness of the display of video frames, at the expense of some memory usage and a possible lag on lower-powered devices between the displayed frame and the tracking results. It has been enabled by default, and can be adjusted using the control on ARController under "2D tracking options".

## Version 1.1.9
### 2023-05-15

Changes:
 * Updated the Unity project to Unity 2021.3.25f1.
 * Updated the native plugin to artoolkitX v1.1.8.

## Version 1.1.8
### 2023-05-14

Changes:
 * Add UnityEvent bindings to ARTrackedObject's events (found, tracked, lost) (and also legacy ARTrackedCamera). The legacy broadcast event bindings are still available but the UnityEvent interface offers better performance and flexibility.
 * Fixed callback stub handling in the plugin interface.
 * Restored Android and iOS camera permissions requests in StartAR (that were lost in the update to v1.1.2).
 * ARController.StartAR is now a co-routine, and will no longer be called from ARController.UpdateAR.
 * On Android, removed AndroidManifest.xml and replaced with build post-processor to insert required permissions and features into the Unity-provided manifest.
 * Tweak built sample app packge ID.
 * Fixed iOS build by adding ARX's dependent libs, plus overhauled post-processor to add linking to sqlite.
Also tidied up ScreenOrienation warning.

## Version 1.1.7
### 2023-05-08

Changes:
 * The native plugins folder has been moved inside the artoolkitX-Unity subfolder of the Unity package.
 * Added support for runtime auto-detection of barcode (matrix code) markers. When enabled, a new
 callback in ARTrackable will auto-add a new ARTrackable for each matrix code trackable detected.
 * Improved support for GlobalID barcode markers.

## Version 1.1.6
### 2023-04-21

Bug fixes:
 * Fix ARControllerEditor serialization; changes made to ARController in the Unity Editor should be saved now.
 * Correct origin on 2D trackable gizmos. Correct prototype for plugin function for pattern image retrieval. Unload when changing trackable properties.

## Version 1.1.5
### 2023-04-12

Changes:
 * Refactored ARTrackable to add factory methods for creating new trackables at run time, plus one-shot config for different trackable types.
 * Change ARController to be findable by singleton instance.
 * Corrected 2D planar tracker orientation and scaling issues.
 * Clarified that "2D tracker scale factor" specifies image width, not height.
 * Overhauled ARPattern handling, including new support for 2D and NFT surfaces, plus barcodes.

## Version 1.1.4
### 2023-03-30

Changes:
 * Corrected a long-standing Windows bug that affected 2D planar tracking with 32-bit video formats.
 * Updated the OpenCV build used by the 2D planar tracker on Windows and switched to static OpenCV libraries. This saves users from having to deploy the OpenCV DLLs alongside built apps.

## Version 1.1.3
### 2023-03-23

Changes:
 * Added support for setting the number of 2D planar tracker markers that can be simultaneously tracked.

## Version 1.1.2
### 2023-03-16

This is the first official artoolkitX for Unity release in some time. As the project is now using continuous integration and deployment (CI/CD) releases will be a lot more frequent.

Changes:
 * Added some extra tracking example images.
 * Video source input config is now automatically fetched and can be configured in the Unity Editor or at runtime using the new ARVideoConfig component. This component will automatically be added when first adding an ARController to a new scene.
 * A new native video input module (Camera2) on Android removes the requirement for pushing of video frames from Java code over JNI. Some video configuration options have changed. artoolkiX for Unity will automatically add "-native" to the video configuration string to use the new module.
 * Updated artoolkitX dependency. Now downloads release artoolkitX packages; special Unity-only packages  are no longer required. Also better support for developer mode building of artoolkitX.
 * Only runs tracking update on new frames.
 * Added pinball image and 2D tracker scene.
 * Updated Unity download/install scripts.
 * 64-bit libs on Android are now in the Unity package.
 * Unity settings update for Unity 2021.3.
 * Android projects update to target API 33, minimum API 21 (Android 5.0), with Gradle 7.6, Android Gradle plugin 7.2.2.
 * On Android, the minimum supported OS is now Android 7.0 (API level 24).
 * On iOS, the minimum supported OS is now iOS v11.0.
 * On mac OS, full support for the ARM64 (Apple Silicon) CPU. The minimum supported OS is now mac OS 10.13.
 * On Windows, the default Visual Studio version is now VS 2019.
 * On Linux, the system OpenCV implementation is preferred.
 * Removed dependence on opencv imread(), and therefore highgui, imgcodecs, and various image libs.

--
EOF
