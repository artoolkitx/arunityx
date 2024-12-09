## Welcome to artoolkitX for Unity

### What is artoolkitX for Unity?

artoolkitX for Unity is a software development kit (SDK) consisting of script components, plugins, and utilities that help developers implement the foundation of great augmented and mixed reality applications inside the Unity development environment. The SDK includes some examples of applications that demonstrate the capabilities of artoolkitX for Unity.

artoolkitX for Unity is free to use! The SDK is licensed under the GNU Lesser General Public License version 3.0, with some additional permissions, allowing for linking into both closed- and open-source software. Please read the file license to understand your rights and obligations when using artoolkitX. Example code is generally released under a more permissive disclaimer; please read the file LICENSE.txt for more information.

## How to use this software to develop applications

This repository holds the source and Unity project for development of artoolkitX for Unity. If you want to use the built package directly from this repo, you'll find that on the `upm` branch. You can use this directly from the Unity Package Manager as follows:

1. Open the Unity package manager.
2. From the top-left of the package manager window, click the "+" symbol, and then from the pop-up menu, choose "Add pacakge from git repo...".
3. In the URL field, enter: `https://github.com/artoolkitx/arunityx.git#upm`.

Please check out [the wiki documentation][documentation] for more information on how to use artoolkitX for Unity.

## How to contribute to the development of this software

After checking out this repository, you'll need to fetch the binary artoolkitX plugins. To do this, from a bash shell (e.g. Terminal on mac OS, or git-bash or Windows Subsystem for Linux Ubuntu bash shell on Windows):

```
cd dev
./build.sh platforms
```
where `platforms` is a space-separted list of one or more of: `macos`, `windows`, `ios`, `android`, or `emscripten`. Note that to fetch the plugins for mac OS or iOS requires the command to be executed on a Mac.

Alternately, if you wish to make modifications to the plugins, you can check out and use artoolkitX directly as a git submodule:

```
cd dev
cd artoolkitx
git submodule init
git submodule update
cd ..
./build.sh --dev platforms
```

If you find a bug, please note it on our [issue tracker][issue tracker], and / or, fix it yourself and submit a [pull request][pull]!

[website]: http://www.artoolkitx.org
[documentation]: https://github.com/artoolkitx/arunityx/wiki
[issue tracker]: https://github.com/artoolkit/arunityx/issues
[pull]: https://github.com/artoolkitx/arunityx/pulls
