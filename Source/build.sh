#! /bin/bash

#
# Build artoolkitX for Unity for all platforms.
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
# Copyright 2018-2023, artoolkitX Contributors.
# Author(s): Thorsten Bux <thor_artk@outlook.com>, Philip Lamb <phil@artoolkitx.org>
#

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARUNITYX_HOME="$OURDIR/.."
PLUGINS_BASE="${ARUNITYX_HOME}/Source/Package/Assets/artoolkitX-Unity/Plugins"

function usage {
    echo "Usage: $(basename $0) [--debug] (macos | windows | ios | linux | android) [--dev]"
    exit 1
}

if [ $# -eq 0 ]; then
    usage
fi

# -e = exit on errors
set -e

# -x = debug
set -x

# Parse parameters
while test $# -gt 0
do
    case "$1" in
        macos) BUILD_MACOS=1
            ;;
        ios) BUILD_IOS=1
            ;;
        linux) BUILD_LINUX=1
            ;;
        android) BUILD_ANDROID=1
            ;;
        windows) BUILD_WINDOWS=1
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

# Set OS-dependent variables.
OS=`uname -s`
ARCH=`uname -m`
TAR='/usr/bin/tar'
if [ "$OS" = "Linux" ]
then
    CPUS=`/usr/bin/nproc`
    TAR='/bin/tar'
    # Identify Linux OS. Sets useful variables: ID, ID_LIKE, VERSION, NAME, PRETTY_NAME.
    source /etc/os-release
    # Windows Subsystem for Linux identifies itself as 'Linux'. Additional test required.
    if grep -qE "(Microsoft|WSL)" /proc/version &> /dev/null ; then
        OS='Windows'
    fi
elif [ "$OS" = "Darwin" ]
then
    CPUS=`/usr/sbin/sysctl -n hw.ncpu`
elif [[ "$OS" == "CYGWIN_NT-"* ]]
then
    # bash on Cygwin.
    CPUS=`/usr/bin/nproc`
    OS='Windows'
elif [[ "$OS" == "MINGW64_NT-"* ]]
then
    # git-bash on Windows
    CPUS=`/usr/bin/nproc`
    OS='Windows'
else
    CPUS=1
fi

ARTOOLKITX_VERSION=$(cat ${ARUNITYX_HOME}/artoolkitx-version.txt)

# Locate ARTOOLKITX_HOME or clone into submodule
locate_artoolkitx() {
    if [ ! -f "${OURDIR}/Extras/artoolkitx/LICENSE.txt" ] && [ -z "${ARTOOLKITX_HOME}" ]; then
        echo "artoolkitX not found. Please set ARTOOLKITX_HOME or clone submodule"

        read -p "Would you like to use the submodule (recommended) y/n" -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]] ; then
            git submodule init
            git submodule update --remote
        else
            echo "Build failed!, Exit"
            exit 1;
        fi
    fi

    #Set ARTOOLKITX_HOME for internal use
    #Are we using the submodule?
    if [  -f "${OURDIR}/Extras/artoolkitx/LICENSE.txt" ] && [ -z "${ARTOOLKITX_HOME}" ]; then
        ARTOOLKITX_HOME="${OURDIR}/Extras/artoolkitx"
    fi
}

refresh_plugin_for_platform_from_source() {
    #Empty the existing plugin directory
    PLATFORM=$1
    SOURCE="$2"
    if [ "$PLATFORM" = "Android" ] ; then
        for ABI in armeabi-v7a arm64-v8a x86 x86_64; do
            for LIB in libc++_shared.so libARX.so; do
                rm -f "${PLUGINS_BASE}/Android/libs/${ABI}/${LIB}"
                cp "${SOURCE}/SDK/lib/${ABI}/${LIB}" "${PLUGINS_BASE}/Android/libs/${ABI}"
            done
        done
    elif [ "$PLATFORM" = "iOS" ] ; then
        rm -f "${PLUGINS_BASE}/iOS/libARX.a"
        cp "${SOURCE}/SDK/lib/libARX.a" "${PLUGINS_BASE}/iOS/"
    elif [ "$PLATFORM" = "macOS" ] ; then
        rm -rf "${PLUGINS_BASE}/ARX.bundle"
        cp -r "${SOURCE}/SDK/Plugins/ARX.bundle" "${PLUGINS_BASE}/"
    elif [ "$PLATFORM" = "Windows" ] ; then
        rm -f "${PLUGINS_BASE}/x86_64/ARX.dll"
        cp "${SOURCE}/SDK/bin/ARX.dll" "${PLUGINS_BASE}/x86_64/"
    fi
}

find_or_fetch_artoolkitx() {
    if [ ! -f "${1}" ] ; then
        echo "Downloading ${1}..."
        curl --location "https://github.com/artoolkitx/artoolkitx/releases/download/${ARTOOLKITX_VERSION}/${1}" -O
    fi
}

#
# If a DEV build is running the script uses the path to artoolkitX source to build artoolkitX plugin-libraries from there.
# If no dev build is running (default) the artoolkitX libraries are downloaded from GitHub release using the version provided.
#
if [ $DEV ] ; then 

    if [ "$OS" = "Darwin" ] ; then
    # ======================================================================
    #  Build platforms hosted by macOS
    # ======================================================================

        locate_artoolkitx

        if [ $BUILD_ANDROID ] ; then 
            cd "${ARTOOLKITX_HOME}/Source"
            ./build.sh android
            refresh_plugin_for_platform_from_source Android  "${ARTOOLKITX_HOME}"
        fi
        if [ $BUILD_IOS ] ; then
            cd "${ARTOOLKITX_HOME}/Source"
            ./build.sh ios
            refresh_plugin_for_platform_from_source iOS "${ARTOOLKITX_HOME}"
        fi
        if [ $BUILD_MACOS ] ; then
            cd "${ARTOOLKITX_HOME}/Source"
            ./build.sh macos
            refresh_plugin_for_platform_from_source macOS "${ARTOOLKITX_HOME}"
        fi
    fi

    if [ "$OS" = "Windows" ] ; then 
    # ======================================================================
    #  Build platforms hosted by windows
    # ======================================================================

        locate_artoolkitx

        if [ $BUILD_ANDROID ] ; then 
            cd "${ARTOOLKITX_HOME}/Source"
            ./build.sh android
            refresh_plugin_for_platform_from_source Android "${ARTOOLKITX_HOME}"
        fi
        if [ $BUILD_WINDOWS ] ; then
            cd "${ARTOOLKITX_HOME}/Source"
            ./build.sh windows
            refresh_plugin_for_platform_from_source Windows "${ARTOOLKITX_HOME}"
        fi
    fi
else 
    # ======================================================================
    #  Download plugins (Android, iOS, macOS, Windows)
    # ======================================================================

    if [ $BUILD_ANDROID ] ; then
        cd "${OURDIR}"
        MOUNTPOINT=mnt$$
        IMAGE="artoolkitx-${ARTOOLKITX_VERSION}-Android.zip"
        find_or_fetch_artoolkitx "${IMAGE}"
        unzip -q -o "${IMAGE}" -d "${MOUNTPOINT}"
        refresh_plugin_for_platform_from_source Android "${MOUNTPOINT}/artoolkitX"
        rm -rf "${MOUNTPOINT}"
    fi
    if [ $BUILD_IOS ] ; then 
        cd "${OURDIR}"
        MOUNTPOINT=mnt$$
        IMAGE="artoolkitX.for.iOS.v${ARTOOLKITX_VERSION}.dmg"
        find_or_fetch_artoolkitx "${IMAGE}"
        mkdir -p "${MOUNTPOINT}"
        hdiutil attach "${IMAGE}" -noautoopen -quiet -mountpoint "${MOUNTPOINT}"
        refresh_plugin_for_platform_from_source iOS "${MOUNTPOINT}/artoolkitX"
        hdiutil detach "${MOUNTPOINT}" -quiet -force
        rmdir "${MOUNTPOINT}"
    fi
    if [ $BUILD_MACOS ] ; then 
        cd "${OURDIR}"
        MOUNTPOINT=mnt$$
        IMAGE="artoolkitX.for.macOS.v${ARTOOLKITX_VERSION}.dmg"
        find_or_fetch_artoolkitx "${IMAGE}"
        mkdir -p "${MOUNTPOINT}"
        hdiutil attach "${IMAGE}" -noautoopen -quiet -mountpoint "${MOUNTPOINT}"
        refresh_plugin_for_platform_from_source macOS "${MOUNTPOINT}/artoolkitX"
        hdiutil detach "${MOUNTPOINT}" -quiet -force
        rmdir "${MOUNTPOINT}"
    fi
    if [ $BUILD_WINDOWS ] ; then
        cd "${OURDIR}"
        MOUNTPOINT=mnt$$
        IMAGE="artoolkitX-${ARTOOLKITX_VERSION}-Windows.zip"
        find_or_fetch_artoolkitx "${IMAGE}"
        unzip -q -o "${IMAGE}" -d "${MOUNTPOINT}"
        refresh_plugin_for_platform_from_source Windows "${MOUNTPOINT}/artoolkitX"
        rm -rf "${MOUNTPOINT}"
    fi
fi