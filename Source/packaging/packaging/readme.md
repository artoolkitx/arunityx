# Packaging the artoolkitX for Unity Plugin

## Requirements
-   A computer that can run both bash shell scripts and Unity 3D.

## Steps
1.   Open a Terminal window, and run "arunityX-bin.sh" from this directory.
2.   The script will ask you to enter a version number, which falls in the format of #.#, #.#.#, #.#r#, or #.#.#r#, increment the version per release, according to release guidelines.
3.   The script will then ask you whether you would like to move the OS X or Windows tools. Generally, choose no by entering "n". Otherwise, the script will ask you for the path to the artoolkitX directory, built for that platform, and then copy the generated tools over to the repository.
4.   The script will output 3 files:
   -   "arunityX-${VERSION}-tools-osx.tar.gz"
      - A ".tar.gz" package of the artoolkitX tools, sutible for OS X release.
   -   "arunityX-${VERSION}-tools-win.zip"
      - A ".zip" package of the artoolkitX tools, sutible for Windows release.
   -   "arunityX.unitypackage"
      - The Unity package on its own, which is already bundled with the tools.

## Deliverables
   -   "arunityX-${VERSION}-tools-osx.tar.gz"
      - A ".tar.gz" package of the artoolkitX tools, sutible for OS X release.
   -   "arunityX-${VERSION}-tools-win.zip"
      - A ".zip" package of the artoolkitX tools, sutible for Windows release.
   -   "arunityX-${VERSION}.unitypackage"
      - A ".unitypackage" package of the artoolkitX for Unity project itself, suitable for all supported platforms.