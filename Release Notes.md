# artoolkitX for Unity Release Notes
------------------------------------

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
