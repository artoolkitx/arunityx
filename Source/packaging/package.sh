#! /bin/bash

#
# Package arunityX for all platforms.
#
# This file is part of artoolkitX for Unity.
#
# artoolkitX for Unity is free software: you can redistribute it and/or modify
# it under the terms of the GNU Lesser General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# artoolkitX for Unity is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public License
# along with artoolkitX for Unity.  If not, see <http://www.gnu.org/licenses/>.
#
# Copyright 2022-2023, artoolkitX Contributors.
# Author(s): Philip Lamb <phil@artoolkitx.org>
#
# Copyright 2018, Realmax, Inc.
# Author(s): Thorsten Bux <thor_artk@outlook.com> , Philip Lamb <phil@artoolkitx.org>
#

# Define UNITY_EDITOR to path to Unity Editor executable, or default will be used.

# -e = exit on errors
set -e -x

#One can pass the unity version as parameter is no version is passed the script will only look for an app called 'Unity'
UNITY_VERSION=$1

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARUNITYX_HOME=$OURDIR/../.. 

OS=$(uname -s)
WINPATH=cygpath
if [ "$OS" = "Linux" ]
then
    # Identify Linux OS. Sets useful variables: ID, ID_LIKE, VERSION, NAME, PRETTY_NAME.
    source /etc/os-release
    # Windows Subsystem for Linux identifies itself as 'Linux'. Additional test required.
    if grep -qE "(Microsoft|WSL)" /proc/version &> /dev/null ; then
        OS='Windows'
        WINPATH=wslpath
    fi
elif [ "$OS" = "Darwin" ]
then
    # macOS
    true
elif [[ "$OS" == "CYGWIN_NT-"* ]]
then
    # bash on Cygwin.
    OS='Windows'
elif [[ "$OS" == "MINGW64_NT-"* ]]
then
    # git-bash on Windows
    OS='Windows'
fi

# Host-platform dependent options.
if [ "$OS" = "Windows" ]
then
    UNITY_EDITOR="$(${WINPATH} -u "${UNITY_EDITOR:-C:\\Program Files\\Unity\\Editor\\Unity.exe}")"
    PROJECT_PATH="$(${WINPATH} -w "${ARUNITYX_HOME}/Source/Package")"
elif [ "$OS" = "Darwin" ]
then
    UNITY_EDITOR=${UNITY_EDITOR:-'/Applications/Unity/Unity.app/Contents/MacOS/Unity'}
    PROJECT_PATH="${ARUNITYX_HOME}/Source/Package"
fi

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
    if [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/Android/libs/arm64-v8a/libARX.so ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/Android/libs/armeabi-v7a/libARX.so ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/Android/libs/x86/libARX.so ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/Android/libs/x86_64/libARX.so ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/iOS/libARX.a ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/x86_64/ARX.dll ] || \
       [ -f $ARUNITYX_HOME/Source/Package/Assets/Plugins/ARX.bundle ]; then
        # Plugins are available, build using the version number inside artoolkitx-version.txt
        VERSION=$(cat ${ARUNITYX_HOME}/artoolkitx-version.txt)
    else
        #Print if we didn't built at all.
        echo "You need to run ./build.sh <platform> before packaging."
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
"${UNITY_EDITOR}" \
    -quit \
    -batchmode \
    -nographics \
    -stackTraceLogType Full \
    -projectPath "${PROJECT_PATH}" \
    -arunityxpackagename arunityX-${VERSION}.unitypackage \
    -executeMethod ARToolKitPackager.CreatePackage

# Move the output.
mv "${ARUNITYX_HOME}/Source/Package/arunityX-${VERSION}.unitypackage" "${ARUNITYX_HOME}"
