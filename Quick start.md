## Introduction
As a quick start with Unity3D and artoolkitX you can use one of the example scenes shipped with the arunityX.unitypackage.

### Development Requirements

- [macOS X](http://www.apple.com/mac/) OR [Windows 10](https://www.microsoftstore.com/store/msusa/en_US/cat/Windows-10-/categoryID.70036700)

- [Xcode](http://developer.apple.com/xcode/) - Required for the macOS X version of the Unity3D IDE and to target Apple macOS and iOS platforms with Unity. Version 9.3.1 was used at the time of this writing.

- [Unity3D](https://store.unity.com/?_ga=1.164065343.1498217150.1465512057) - the free, Personal Edition, is sufficient for a start. Version 2017.4.0f1 was used at the time of this writing. The Unity3D IDE, typically referred to as the "Unity Editor", is supported on the desktop platforms of macOS X and Windows 10.

### Build arunityX
You need to run the build script before you can use this repository. The reason for this is, that arunityX does not ship with the artoolkitX libraries, but it downloads them when you run the build script. It is designed this way to assure arunityX always uses a stable and up to date release of artoolkitX libraries. The version in question is noted in the version.txt in the root of this repository.

1. Open Terminal on macOS or Git Bash on Windows
2. ```cd Source```
3. Run ```./build.sh [Target platform]```. ```[Target platform]``` can be one or several of *macos, ios, Android, Windows*

This will fetch all the dependencies and place them inside the ```Plugins``` directory.

### Bundle the arunityX.unitpackage
To create a arunityX.unitypackage, which can be imported into other Unity3D projects, one needs to run ```./package.sh``` located inside the ```packaging``` directory.


### Import the arunityX.unitypackge Plugin
The arunityX.unitypackage created in the previous step can be imported into other Unity3D projects.

* Start Unity3D
* Create a new 3D scene
* Navigate to:
  * ```Assets > Import Package > Custom Package... ```
* Select the created arunityX.unitypackage

(During the import you might see some warning and/or error messages. Usually, you can ignore them and install the plugin regardless of the messages.)

### The Plugin Structure

In the lower left corner in the Project-Tab you can see the arunityX-Unity plugin in form of a folder. Inside this folder, you can find all the Scripts, Resources and Test Scenes that were imported with the arunityX.unitypackage. Additionally, the StreamingAssets directory contains  all resources that are available to your Unity3D app during runtime.

### Run the Example Scene

Navigate to:
* artoolkitX-Unity > Example Scenes
* Double one of the example scenes
  * This will load the scene into the Hierarchy-Tab
* Select the 'Play'-Button to run the scene inside the Unity3D Editor
* Present on or multiple of the trackables to the camera. A cube should appear on the trackable.

### Building for iOS with XCode

You may get `Error: Undefined symbols for architecture arm64:   "_vImageRotate90_ARGB8888"` which means you need to add the Accelerate.framework to your build:
In the project settings editor, select the "Build Phases" tab, and click the small triangle next to "Link Binary With Libraries" to view all of the frameworks in your application. Click the "+" below the list of frameworks and add the `Accelerate.framework`
