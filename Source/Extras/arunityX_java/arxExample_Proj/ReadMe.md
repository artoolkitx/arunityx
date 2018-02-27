# Why this project
This project aims to make the development of the arunityXPlayer plugin easier by offering a possibility to debug the arunityXPlayer- and arjX-code.
It does so by linking these two projects with a simple example (SimpleScene.unity) project which was exported from Unity3D into a Android project.

## Updating this project
Whenever changes are made to the Unity3D part of the plugin or to the Unity3D project that one would like to debug the following needs to be copied into this repository:

- arunityXPlayer-release.aar
- arxjUnity.jar
- libARX.so (armeabi-v7a and x86)
- libc++_shared.so (armeabi-v7a and x86)
- libmain.so, libunity.so, libmono.so (armeabi-v7a and x86)
- assets/bin

## Where to find those files
To get access to **libmain.so, libunity.so, libmono.so** one needs to export the Unity3D project into an
AndroidStudio project. This can be done using `File > Build settings` from inside Unity3D. On this dialog
`Switch Platform` to Android and make sure that the box `Export Project` is ticked. Additionally tick
`Development build` and `Script debugging`. The files can then be found inside the exported project in
`scr > main > jniLibs`

To create **arunityXPlayer-release.aar** you need to either open the arunityXPlayer project inside AndroidStudio
and build it. Or you use this project and locate arunityXPlayer project inside the Gradle projects tab
and build it from there. Once build the file is located inside `build > outputs > aar`

To To create **arxjUnity.jar** you need to either open the ARXJProj project inside AndroidStudio and
build the target `jarReleaseUnity`. Or you use this project and locate arxj project inside the Gradle
projects tab and build `jarReleaseUnity` it from there. The build artifact can then be located in `build > libs > arxjUnity.jar`

## Where to copy these files to
- arunityXPlayer-release.aar and arxjUnity.jar need to go into `arxExample > libs`.
- libARX.so, libc++_shared.so, libmain.so, libunity.so, libmono.s need to go into `arxExample > src > main > jniLibs`

