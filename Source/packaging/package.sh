#! /bin/bash

#
# Package ARUnity for all platforms.
#
# Copyright 2016, ARToolKit Contributors.
# Author(s): Thorsten Bux <thor_artk@outlook.com> , Philip Lamb <phil@artoolkit.org>
#

# -e = exit on errors
set -e -x

#One can pass the unity version as parameter is no version is passed the script will only look for an app called 'Unity'
UNITY_VERSION=$1

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

#Check if we are using the submodule to build
if [  -f "$OURDIR/../Extras/artoolkitX/LICENSE.txt" ] && [ -z $ARTOOLKITX_HOME ]; then
    ARTOOLKITX_HOME=$OURDIR/../Extras/artoolkitx/
fi

# Get version from <ARX/AR/ar.h> header.
cd $OURDIR
CONFIG_LOCATION="${ARTOOLKITX_HOME}/SDK/include/ARX/AR/config.h";
if [ ! -f ${CONFIG_LOCATION} ]; then
    #If only 'macos' was built config.h is in a different location
    CONFIG_LOCATION="${ARTOOLKITX_HOME}/SDK/Frameworks/ARX.framework/Headers/AR/config.h"
fi
if [ ! -f ${CONFIG_LOCATION} ]; then
    #Print if we didn't built at all.
    echo "You need to run ./build.sh <plattform> before packaging."
fi

VERSION=`sed -En -e 's/.*AR_HEADER_VERSION_STRING[[:space:]]+"([0-9]+\.[0-9]+(\.[0-9]+)*)".*/\1/p' ${CONFIG_LOCATION}`
# If the tiny version number is 0, drop it.
VERSION=`echo -n "${VERSION}" | sed -E -e 's/([0-9]+\.[0-9]+)\.0/\1/'`

# Rename version, where appropriate.
ARUNITYX_HOME=$OURDIR/../.. 
sed -Ei "" "s/artoolkitX for Unity Version (([0-9]+\.[0-9]+)(\.[0-9]+)?(r[0-9]+)?)/artoolkitX for Unity Version $VERSION/" $ARUNITYX_HOME/Source/Package/Assets/ARToolKitX-Unity/Scripts/Editor/ARToolKitMenuEditor.cs

# Build the unitypackage.
/Applications/Unity$UNITY_VERSION/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod ARToolKitPackager.CreatePackage -projectPath $ARUNITYX_HOME/Source/Package ARUnityX-${VERSION}.unitypackage

mv $ARUNITYX_HOME/Source/Package/arunityX-${VERSION}.unitypackage $ARUNITYX_HOME
