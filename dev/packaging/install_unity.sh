#! /bin/bash

#
# Install Unity
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

# Set environment variables UNITY_VERSION prior to calling.
set -x
# Disable git-bash's attempts to convert paths that messes up forward slashes.
export MSYS_NO_PATHCONV=1

# Set OS-dependent variables.
OS=`uname -s`
ARCH=`uname -m`
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

# Parse parameters
while test $# -gt 0
do
    case "$1" in
        --with-android) WITH_ANDROID=1
            ;;
        --*) echo "bad option $1"
            ;;
        *) echo "bad argument $1"
            ;;
    esac
    shift
done


# If user didn't specify a version, use a default.
UNITY_VERSION=${UNITY_VERSION:-2022.3.20f1}

cd "unity_installers"

if [ "$OS" = "Windows" ]
then
    # Installs to C:\Program Files\Unity\Editor
    ./UnitySetup64.exe /S /D=C:\\Program\ Files\\Unity
    ./UnitySetup-Windows-IL2CPP-Support-for-Editor-${UNITY_VERSION}.exe /S /D=C:\\Program\ Files\\Unity
    if [[ -n $WITH_ANDROID ]]; then
        ./UnitySetup-Android-Support-for-Editor-${UNITY_VERSION}.exe /S /D=C:\\Program\ Files\\Unity
    fi
    #./UnitySetup-Mac-Mono-Support-for-Editor-${UNITY_VERSION}.exe /S /D=C:\\Program\ Files\\Unity
elif [ "$OS" = "Darwin" ]
then
    # Installs to /Applications/Unity/Editor
    sudo installer -pkg Unity.pkg -target / -verbose
    sudo installer -pkg UnitySetup-Mac-IL2CPP-Support-for-Editor-${UNITY_VERSION}.pkg -target / -verbose
    if [[ -n $WITH_ANDROID ]]; then
        sudo installer -pkg UnitySetup-Android-Support-for-Editor-${UNITY_VERSION}.pkg -target / -verbose
    fi
    #sudo installer -pkg UnitySetup-Windows-Mono-Support-for-Editor-${UNITY_VERSION}.pkg -target / -verbose
fi

if [[ -n $WITH_ANDROID ]]; then
    if [[ "$UNITY_VERSION" =~ 2022\.3\..* ]]; then
        if [ "$OS" = "Windows" ]; then
            sdkInstallRoot=$(${WINPATH} -u C:\\Program\ Files\\Unity\ ${UNITY_VERSION}\\Editor\\Data\\PlaybackEngines\\AndroidPlayer)
            # JDK
            unzip "OpenJDK11U-jdk_x64_windows_hotspot_11.0.14.1_1.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/jdk-11.0.14.1+1" "${sdkInstallRoot}/OpenJDK"
            # SDK
            unzip -q "sdk-tools-windows-4333796.zip" -d "${sdkInstallRoot}/SDK"
            unzip -q "210b77e4bc623bd4cdda4dae790048f227972bd2.build-tools_r32-windows.zip" -d "${sdkInstallRoot}/SDK/build-tools"
            mv "${sdkInstallRoot}/SDK/build-tools/android-12" "${sdkInstallRoot}/SDK/build-tools/32.0.0"
            unzip -q "platform-tools_r32.0.0-windows.zip" -d "${sdkInstallRoot}/SDK"
            unzip -q "android-ndk-r23b-windows.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/android-ndk-r23b" "${sdkInstallRoot}/NDK"
            mkdir "${sdkInstallRoot}/SDK/cmdline-tools"
            unzip -q "commandlinetools-win-8092744_latest.zip" -d "${sdkInstallRoot}/SDK/cmdline-tools"
            mv "${sdkInstallRoot}/SDK/cmdline-tools/cmdline-tools" "${sdkInstallRoot}/SDK/cmdline-tools/6.0"
        elif [ "$OS" = "Darwin" ]; then
            sdkInstallRoot='/Applications/Unity/Editor/PlaybackEngines/AndroidPlayer'
            # JDK. Mac package layout differs.
            tar xzf "OpenJDK11U-jdk_x64_mac_hotspot_11.0.14.1_1.tar.gz" -C "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/jdk-11.0.14.1+1/Contents/Home" "${sdkInstallRoot}/OpenJDK"
            rm -rf "${sdkInstallRoot}/jdk-11.0.14.1+1"
            # SDK
            unzip -q "sdk-tools-darwin-4333796.zip" -d "${sdkInstallRoot}/SDK"
            unzip -q "5219cc671e844de73762e969ace287c29d2e14cd.build-tools_r32-macosx.zip" -d "${sdkInstallRoot}/SDK/build-tools"
            mv "${sdkInstallRoot}/SDK/build-tools/android-12" "${sdkInstallRoot}/SDK/build-tools/32.0.0"
            unzip -q "platform-tools_r32.0.0-darwin.zip" -d "${sdkInstallRoot}/SDK"
            unzip -q "android-ndk-r23b-darwin.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/android-ndk-r23b" "${sdkInstallRoot}/NDK"
            mkdir "${sdkInstallRoot}/SDK/cmdline-tools"
            unzip -q "commandlinetools-mac-8092744_latest.zip" -d "${sdkInstallRoot}/SDK/cmdline-tools"
            mv "${sdkInstallRoot}/SDK/cmdline-tools/cmdline-tools" "${sdkInstallRoot}/SDK/cmdline-tools/6.0"
        fi
        unzip -q "platform-31_r01.zip" -d "${sdkInstallRoot}/SDK/platforms"
        mv "${sdkInstallRoot}/SDK/platforms/android-12" "${sdkInstallRoot}/SDK/platforms/android-31"
        unzip -q "platform-32_r01.zip" -d "${sdkInstallRoot}/SDK/platforms"
        mv "${sdkInstallRoot}/SDK/platforms/android-12" "${sdkInstallRoot}/SDK/platforms/android-32"
    elif [[ "$UNITY_VERSION" =~ 2021\.3\..* ]]; then
        if [ "$OS" = "Windows" ]; then
            sdkInstallRoot=$(${WINPATH} -u C:\\Program\ Files\\Unity\ ${UNITY_VERSION}\\Editor\\Data\\PlaybackEngines\\AndroidPlayer)
            unzip "OpenJDK8U-jdk_x64_windows_hotspot_8u292b10.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/jdk8u292-b10" "${sdkInstallRoot}/OpenJDK"
            unzip "sdk-tools-windows-4333796.zip" -d "${sdkInstallRoot}/SDK"
            unzip "efbaa277338195608aa4e3dbd43927e97f60218c.build-tools_r30.0.2-windows.zip" -d "${sdkInstallRoot}/SDK/build-tools"
            mv "${sdkInstallRoot}/SDK/build-tools/android-11" "${sdkInstallRoot}/SDK/build-tools/30.0.2"
            unzip "platform-tools_r30.0.4-windows.zip" -d "${sdkInstallRoot}/SDK"
            unzip "android-ndk-r21d-windows-x86_64.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/android-ndk-r21d" "${sdkInstallRoot}/NDK"
            mkdir "${sdkInstallRoot}/SDK/cmdline-tools"
            unzip -q "commandlinetools-win-6609375_latest.zip" -d "${sdkInstallRoot}/SDK/cmdline-tools"
            mv "${sdkInstallRoot}/SDK/cmdline-tools/tools" "${sdkInstallRoot}/SDK/cmdline-tools/2.1"
        elif [ "$OS" = "Darwin" ]; then
            sdkInstallRoot='/Applications/Unity/Editor/PlaybackEngines/AndroidPlayer'
            tar xzf "OpenJDK8U-jdk_x64_mac_hotspot_8u292b10.tar.gz" -C "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/jdk8u292-b10" "${sdkInstallRoot}/OpenJDK"
            unzip "sdk-tools-darwin-4333796.zip" -d "${sdkInstallRoot}/SDK"
            unzip "5a6ceea22103d8dec989aefcef309949c0c42f1d.build-tools_r30.0.2-macosx.zip" -d "${sdkInstallRoot}/SDK/build-tools"
            mv "${sdkInstallRoot}/SDK/build-tools/android-11" "${sdkInstallRoot}/SDK/build-tools/30.0.2"
            unzip "fbad467867e935dce68a0296b00e6d1e76f15b15.platform-tools_r30.0.4-darwin.zip" -d "${sdkInstallRoot}/SDK"
            unzip "android-ndk-r21d-darwin-x86_64.zip" -d "${sdkInstallRoot}"
            mv "${sdkInstallRoot}/android-ndk-r21d" "${sdkInstallRoot}/NDK"
            mkdir "${sdkInstallRoot}/SDK/cmdline-tools"
            unzip -q "commandlinetools-mac-6609375_latest.zip" -d "${sdkInstallRoot}/SDK/cmdline-tools"
            mv "${sdkInstallRoot}/SDK/cmdline-tools/tools" "${sdkInstallRoot}/SDK/cmdline-tools/2.1"
        fi
        unzip "platform-29_r05.zip" -d "${sdkInstallRoot}/SDK/platforms"
        mv "${sdkInstallRoot}/SDK/platforms/android-10" "${sdkInstallRoot}/SDK/platforms/android-29"
        unzip "platform-30_r03.zip" -d "${sdkInstallRoot}/SDK/platforms"
        mv "${sdkInstallRoot}/SDK/platforms/android-11" "${sdkInstallRoot}/SDK/platforms/android-30"
    else
        echo "Warning: No Android tools install procedure defined for Unity $UNITY_VERSION"
    fi
fi

cd ..
