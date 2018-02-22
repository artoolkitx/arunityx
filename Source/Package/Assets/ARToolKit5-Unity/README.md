# ARToolKit for Unity

---

## Index
-   [About this Archive.](#About this Archive)
-   [System Requirements](#System Requirements)
-   [Support](#Support)
-   [Notes](#Notes)
-   [Windows Phone 8.1 and Windows Store 8.1 Notes](#WinRT)

### About this Archive
This archive contains the ARToolKit for Unity project, plugins, utilities and examples, version 5.3.2r1.

ARToolKit for Unity version 5.3.2r1 is released under the GNU Lesser General Public License version 3, with some additional permissions. Example code is generally released under a more permissive disclaimer; please read the file LICENSE.txt for more information.

ARToolKit for Unity is designed to build on Windows, Macintosh OS X, iOS and Android platforms.

### System Requirements:
In addition to any base requirements for Unity, the ARToolKit for Unity plugin(s) have the following requirements:

-   ARToolKit for Unity 5.3.2 requires Unity Pro v4.5.5f1 or later.
-   If targeting Android, Android v4.0.3 and later is supported.
-   If targeting iOS, Xcode tools v5.0.1 or later is required. iOS v7.0 and later is supported.
-   If targeting OS X, Xcode tools v5.0.1 or later is required. OS X v10.7 and later is supported.
-   If targeting Windows Phone 8.1 or Windows Store 8.1, Visual Studio 2013 SP2 running on Windows 8.1 or later is required.

### Support
Please check out [documentation][documentation] for more information on [getting started][starting] and how to use ARToolKit for Unity. Specifically have a look at our [Unity documentation][unity], and our platform-specific documentation for [iOS][ios] and [Android][android], if using those platforms. If you have a specific question that the documentation doesn't address, please post on our [forum][forum], and a member of the community with help you. If you find a bug, please note it on our [bug tracker][bug tracker], and / or, fix it yourself and submit a [pull request][pull]!

### Notes
ARToolKit v5.2 was the first major release under an open source license in several years, and represented several years of commercial development of ARToolKit by ARToolworks during this time. See the [feature comparison][features] for more information. Also, see the CHANGELOG.txt for details of changes in this and earlier releases.

### WinRT
ARToolKit for Unity for Windows Phone 8.1 and Windows Store 8.1.

ARToolKit for Unity depends on the native ARToolKit libraries, which are packaged in a module named ARWrapper.dll. ARWrapper.dll is linked as native Windows Phone 8.1 and Windows Store 8.1 DLLs, with all the ARToolKit libraries linked into it. ARToolKit for Unity's C# scripts communicate with this DLL by use of P/Invoke. This is the same method used on all other platforms supported by ARToolKit for Unity.

Structure of an ARToolKit for Unity Windows Phone 8.1 and Windows Store 8.1 project:

    Assets/
      Plugins/
        x86/
          ARWrapper.dll               - The native Win32 DLL for running ARToolKit in the Unity Editor.
        Metro/
          WindowsPhone81
            arm/
              ARWrapper.dll           - The native WinRT DLL for running ARToolKit on the Windows Phone 8.1 device.
            x86/
              ARWrapper.dll           - The native WinRT DLL for running ARToolKit on the Windows Phone 8.1 emulator.
          Win81
            arm/
              ARWrapper.dll           - The native WinRT DLL for running ARToolKit on the Microsoft Surface device.
            x86/
              ARWrapper.dll           - The native WinRT DLL for running ARToolKit on the Microsoft Store in 32-bit mode.
            x86_64/
              ARWrapper.dll           - The native WinRT DLL for running ARToolKit on the Microsoft Store in 64-bit mode.
      ARToolKit5-Unity/
        ...                       - The ARToolKit for Unity C# scripts which run in the Unity Editor and Player.
      Editor/
        ARToolKit5-UnityEditor/
          ...                         - The ARToolKit for Unity C# scripts which run in the Unity Editor only.

To build a Unity project which uses ARToolKit for Unity on Windows Phone 8.1 or Windows Store 8.1, go to the "Build settings" window in Unity, select "Platform:Windows Store", and SDK:"Windows Phone 8.1", "Windows 8.1", or "Universal". (Do not use "Platform:Windows Phone 8", as that builds only for Windows Phone 8.0, not 8.1.)

Also make sure that you have ticked "Webcam" as a capability used by your app in the Unity build settings.

Export the Unity project as "Type: XAML C# Solution", and then open the exported Visual Studio solution.

Unity needs a little help to deal with ARWrapper. Although Unity creates a pre-build task which copies this DLLs to the root of the Visual Studio project (and thus overwrites the Win32 ARWrapper.dll with the correct WinRT ARWrapper.dll), it misses one important step: telling Visual Studio to include this file in the project and to copy it into the built app.

Each time you recreate the Visual Studio project file, you must do these steps in order:

1. Open the exported .sln file in Visual Studio 2013.  
2. Select the desired target architecture, and then choose menu  
    "Project->Build".  
3. Choose menu "Project->Show All Files"  
4. Select ARWrapper.dll, right click on it, and choose "Include in build" from the popup menu.  
5. Right-click on ARWrapper.dll, and choose "Properties" from the popup menu.  
6. Ensure that property "Copy to Output Directory" is set to "Copy if newer" or "Copy always".  
7. Choose menu "Project->Build" again.

Now you can deploy to the phone etc. and test etc. ARToolKit's log output will go to the Unity log and will also be displayed in the Visual Studio debug console.

---

This archive was assembled by:
    Philip Lamb and Wally Young
    [http://www.artoolkit.org][website]
    2016-04-18

Copyright 2015-2016 DAQRI LLC
Copyright 2011-2015 ARToolworks, Inc.

[website]: http://www.artoolkit.org
[documentation]: http://artoolkit.org/documentation/
[forum]: http://artoolkit.org/community/forums/viewforum.php?f=28
[bug tracker]: https://github.com/artoolkit/arunity5/issues
[pull]: https://github.com/artoolkit/arunity5/pulls
[android]: http://artoolkit.org/documentation/doku.php?id=4_Android:android_about
[ios]: http://artoolkit.org/documentation/doku.php?id=5_iOS:ios_about
[unity]: http://artoolkit.org/documentation/doku.php?id=6_Unity:unity_about
[starting]: http://artoolkit.org/documentation/doku.php?id=6_Unity:unity_getting_started
[features]: http://www.artoolkit.org/documentation/ARToolKit_feature_comparison
