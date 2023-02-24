#! /bin/bash

#
# Build artoolkitX for Unity for all platforms.
#
# Copyright 2018-2023, artoolkitX Contributors.
# Author(s): Thorsten Bux <thor_artk@outlook.com>, Philip Lamb <phil@artoolkitx.org>
#

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARUNITYX_HOME=$OURDIR/..

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

ARTOOLKITX_VERSION=`cat ../artoolkitx-version.txt`

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
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/armeabi-v7a/libc++_shared.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/armeabi-v7a/libARX.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/arm64-v8a/libc++_shared.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/arm64-v8a/libARX.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86/libc++_shared.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86/libARX.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86_64/libc++_shared.so"
        rm -f "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86_64/libARX.so"
        cp "${SOURCE}/SDK/lib/armeabi-v7a/libc++_shared.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/armeabi-v7a/"
        cp "${SOURCE}/SDK/lib/armeabi-v7a/libARX.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/armeabi-v7a/"
        cp "${SOURCE}/SDK/lib/arm64-v8a/libc++_shared.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/arm64-v8a/"
        cp "${SOURCE}/SDK/lib/arm64-v8a/libARX.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/arm64-v8a/"
        cp "${SOURCE}/SDK/lib/x86/libc++_shared.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86/"
        cp "${SOURCE}/SDK/lib/x86/libARX.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86/"
        cp "${SOURCE}/SDK/lib/x86_64/libc++_shared.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86_64/"
        cp "${SOURCE}/SDK/lib/x86_64/libARX.so" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/Android/libs/x86_64/"
    elif [ "$PLATFORM" = "iOS" ] ; then
        rm -rf "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/iOS/libARX.a"
        cp -rf "${SOURCE}/SDK/lib/libARX.a" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/iOS/"
    elif [ "$PLATFORM" = "macOS" ] ; then
        rm -rf "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/ARX.bundle"
        cp -rf "${SOURCE}/SDK/Plugins/ARX.bundle" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/"
    elif [ "$PLATFORM" = "Windows" ] ; then
        rm -rf "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/x86_64/ARX.dll"
        cp -rf "${SOURCE}/SDK/bin/ARX.dll" "${ARUNITYX_HOME}/Source/Package/Assets/Plugins/x86_64/"
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
            cd "$ARTOO{ARTOOLKITX_HOME}LKITX_HOME/Source"
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
        unzip -o "${IMAGE}" -d "${MOUNTPOINT}"
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
        unzip -o "${IMAGE}" -d "${MOUNTPOINT}"
        refresh_plugin_for_platform_from_source Windows "${MOUNTPOINT}/artoolkitX"
        rm -rf "${MOUNTPOINT}"
    fi
fi