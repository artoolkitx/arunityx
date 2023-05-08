## Introduction
As a quick start with Unity and artoolkitX you can use one of the example scenes shipped with the arunityX.unitypackage.

### Development Requirements

- macOS OR Windows

- [Xcode](http://developer.apple.com/xcode/) - Required for the mac OS version of the Unity Editor to target Apple macOS and iOS platforms with Unity. Xcode 14 was used at the time of this writing.

- [Unity](https://store.unity.com/) - the free, Personal Edition, is sufficient for a start. Version 2021.3 was used at the time of this writing. The Unity Editor is supported on the desktop platforms of macOS and Windows.

### Build or fetch artoolkitX libraries

If using the artoolkitX for Unity package from source, please note that the source repository does not include the compiled artoolkitX native plugins, so it is necessary to fetch the artoolkitX libraries, or alternately build artoolkitX from source. The `build.sh` script controls this. Normally it downloads the version of artoolkitX as listed in the artoolkitx-version.txt in the root of this repository:

1. Open Terminal on macOS or Git Bash on Windows
2. `cd Source`
3. Run `./build.sh [Target platform(s)]`, where `[Target platform(s)]` is one or more of *macos, ios, android, windows*.

This will fetch all the dependencies and place them inside the ```Plugins``` directory.

Alernately, if you wish to build artoolkitX from source (e.g. if modifying the native plugin source, or doing native debugging), then you can get artoolkitX via the git submodule and build it as below:
1. Open Terminal on macOS or Git Bash on Windows
2. `cd Source`
3. `git submodule init`
4. `git submodule update`
5. `./build.sh --dev [Target platform(s)]`, where `[Target platform(s)]` is one or more of *macos, ios, android, windows*. Note that you will only be able to build for mac OS or iOS on a mac OS host, for Windows on a Windows host, and Linux on a Linux host. Android can be built on any host platform. 

### Bundle the arunityX.unitpackage
To create a arunityX.unitypackage, which can be imported into other Unity3D projects, one needs to run `./package.sh` located inside the `packaging` directory.


### Import the arunityX.unitypackge Plugin
The arunityX.unitypackage created in the previous step can be imported into other Unity3D projects.

* Start Unity3D
* Create a new 3D scene
* Navigate to:
  * ```Assets > Import Package > Custom Package... ```
* Select the created arunityX.unitypackage

(During the import you might see some warning and/or error messages. Usually, you can ignore them and install the plugin regardless of the messages.)

### The Plugin Structure

In the lower left corner in the Project-Tab you can see the arunityX-Unity plugin in form of a folder. Inside this folder, you can find all the Scripts, Resources and Test Scenes that were imported with the arunityX.unitypackage. Additionally, the StreamingAssets directory contains  all resources that are available to your Unity app during runtime.

### Run the Example Scene

Navigate to:
* artoolkitX-Unity > Example Scenes
* Double one of the example scenes
  * This will load the scene into the Hierarchy-Tab
* Select the 'Play'-Button to run the scene inside the Unity3D Editor
* Present on or multiple of the trackables to the camera. A cube should appear on the trackable.

