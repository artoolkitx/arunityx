# Packaging the ARToolKit for Unity Plugin

## Requirements
-   A computer that can run both shell scripts and Unity 3D.

## Steps
1.   Open a Terminal window, and run "ARUnity5-bin.sh" from this directory.
2.   The script will ask you to enter a version number, which falls in the format of #.#, #.#.#, #.#r#, or #.#.#r#, increment the version per release, according to release guidelines.
3.   The script will then ask you whether you would like to move the OS X or Windows tools. Generally, choose no by entering "n". Otherwise, the script will ask you for the path to the ARToolKit directory, built for that platform, and then copy the generated tools over to the repository.
4.   The script will output 3 files:
   -   "ARUnity5-${VERSION}-tools-osx.tar.gz"
      - A ".tar.gz" package of the ARToolKit tools, sutible for OS X release.
   -   "ARUnity5-${VERSION}-tools-win.zip"
      - A ".zip" package of the ARToolKit tools, sutible for Windows release.
   -   "ARUnity5.unitypackage"
      - The Unity package on its own, which is already bundled with the tools.

## Deliverables
   -   "ARUnity5-${VERSION}-tools-osx.tar.gz"
      - A ".tar.gz" package of the ARToolKit tools, sutible for OS X release.
   -   "ARUnity5-${VERSION}-tools-win.zip"
      - A ".zip" package of the ARToolKit tools, sutible for Windows release.
   -   "ARUnity5-${VERSION}.unitypackage"
      - A ".unitypackage" package of the ARToolKit for Unity project itself, suitable for all supported platforms.