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
ARUNITYX_HOME=$OURDIR/../.. 

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

# If there is still no config location then there probably is no clone of the artoolkitX source available
if [ ! -f ${CONFIG_LOCATION} ]; then
    # Check if the plugins are available
    if [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/Android/arxjUnity.jar ] ||  [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/iOS/libARX.a ] || [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/x86_64/ARX.dll ] || [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/ARX.bundle ]; then
        # Plugins are available, build using the version number inside version.txt
        VERSION=`cat $ARUNITYX_HOME/version.txt`
    else
        #Print if we didn't built at all.
        echo "You need to run ./build.sh <plattform> before packaging."
        exit -1
    fi
else
    VERSION=`sed -En -e 's/.*AR_HEADER_VERSION_STRING[[:space:]]+"([0-9]+\.[0-9]+(\.[0-9]+)*)".*/\1/p' ${CONFIG_LOCATION}`
    # If the tiny version number is 0, drop it.
    VERSION=`echo -n "${VERSION}" | sed -E -e 's/([0-9]+\.[0-9]+)\.0/\1/'`
fi



# Rename version, where appropriate.
sed -Ei "" "s/artoolkitX for Unity Version (([0-9]+\.[0-9]+)(\.[0-9]+)?(r[0-9]+)?)/artoolkitX for Unity Version $VERSION/" $ARUNITYX_HOME/Source/Package/Assets/ARToolKitX-Unity/Scripts/Editor/ARToolKitMenuEditor.cs

# Build the unitypackage.
/Applications/Unity$UNITY_VERSION/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod ARToolKitPackager.CreatePackage -projectPath $ARUNITYX_HOME/Source/Package ARUnityX-${VERSION}.unitypackage

mv $ARUNITYX_HOME/Source/Package/arunityX-${VERSION}.unitypackage $ARUNITYX_HOME
