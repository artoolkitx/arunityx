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
# Define UNITY_WITH_GPU (e.g. 'UNITY_WITH_GPU=') to disabled Unity's "-nographics" switch.
# If using UPM packaging, ensure git user.name and user.email are configured, e.g.
#     git config --global user.name 'firstname lastname'
#     git config --global user.email 'username@domain'

# -e = exit on errors
set -e -x

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARUNITYX_HOME="$OURDIR/../.."
PACKAGE_PATH="${ARUNITYX_HOME}/Packages/artoolkitX-Unity"
PLUGINS_BASE="${PACKAGE_PATH}/Runtime/Plugins"

# Parse parameters
while test $# -gt 0
do
    case "$1" in
        --asset-package) BUILD_ASSET_PACKAGE=1
            ;;
        --upm-package) PUBLISH_UPM_PACKAGE=1
            ;;
        --with-gpu) UNITY_WITH_GPU=
            ;;
        --log) touch "$2" && LOG="-logFile $2"; shift
            ;;
        --debug) DEBUG=
            ;;
        --dev) DEV=1
            ;;
        --*) echo "bad option $1"
            usage
            ;;
        *) echo "bad argument $1"
            usage
            ;;
    esac
    shift
done

# If neither option specified, default to build asset package.
if [[ -z $BUILD_ASSET_PACKAGE && -z $PUBLISH_UPM_PACKAGE ]]; then
    BUILD_ASSET_PACKAGE=1
fi

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
    UNITY_PACKAGE_PATH="$(${WINPATH} -w "${PACKAGE_PATH}")"
elif [ "$OS" = "Darwin" ]
then
    UNITY_EDITOR=${UNITY_EDITOR:-'/Applications/Unity/Unity.app/Contents/MacOS/Unity'}
    UNITY_PACKAGE_PATH="${PACKAGE_PATH}"
fi

# Extract details from Unity project.
VERSION=$(sed -En -e 's/.*"version": "([0-9.]+)".*/\1/p' ${PACKAGE_PATH}/package.json)

# Process for version number parts.
VERSION_MAJOR=$(echo ${VERSION} | sed -E -e 's/^([0-9]+).*/\1/')
VERSION_MINOR=$(echo ${VERSION} | sed -E -e 's/^[0-9]+\.([0-9]+).*/\1/')
# VERSION_TINY and its preceding dot can be absent, so allow for that in our regexp and set to 0 in that case.
VERSION_TINY=$(echo ${VERSION} | sed -E -e 's/^[0-9]+\.[0-9]+\.*([0-9]+)*.*/\1/')
VERSION_TINY=${VERSION_TINY:-0}
VERSION_BUILD=${VERSION_BUILD:-0}

# Check if the plugins are available
if [ ! -f "${PLUGINS_BASE}/Android/libs/arm64-v8a/libARX.so" ] && \
   [ ! -f "${PLUGINS_BASE}/Android/libs/armeabi-v7a/libARX.so" ] && \
   [ ! -f "${PLUGINS_BASE}/Android/libs/x86/libARX.so" ] && \
   [ ! -f "${PLUGINS_BASE}/Android/libs/x86_64/libARX.so" ] && \
   [ ! -f "${PLUGINS_BASE}/iOS/libARX.a" ] && \
   [ ! -f "${PLUGINS_BASE}/x86_64/ARX.dll" ] && \
   [ ! -f "${PLUGINS_BASE}/ARX.bundle" ] && \
   [ ! -f "${PLUGINS_BASE}/Web/libARX.a" ] ; then
    echo "You need to run ./build.sh <platform> before packaging."
    exit 1
fi

# Rename version, where appropriate.
sed -Ei "" "s/artoolkitX for Unity Version (([0-9]+\.[0-9]+)(\.[0-9]+)?(r[0-9]+)?)/artoolkitX for Unity Version $VERSION/" "${PACKAGE_PATH}/Editor/Scripts/ARToolKitMenuEditor.cs"

# Build the unitypackage.
if [ $BUILD_ASSET_PACKAGE ]; then
    "${UNITY_EDITOR}" \
        -quit \
        -batchmode \
        ${UNITY_WITH_GPU--nographics} \
        -stackTraceLogType Full \
        -projectPath "${UNITY_PACKAGE_PATH}" \
        -arunityxpackagename arunityX-${VERSION}.unitypackage \
        -executeMethod ARToolKitPackager.CreatePackage \
        ${LOG}

    # Move the output.
    mv "${PACKAGE_PATH}/arunityX-${VERSION}.unitypackage" "${ARUNITYX_HOME}"
fi

if [ $PUBLISH_UPM_PACKAGE ]; then
    git branch -d upm &> /dev/null || echo upm branch not found
    git subtree split -P "${PACKAGE_PATH}" -b upm
    git checkout upm
    if [[ -d "Samples" ]]; then
        git mv Samples Samples~
        rm -f Samples.meta
        git commit -am "UPM: Samples => Samples~"
    fi
    git push -f -u origin upm
fi

