#! /bin/bash

#
# Download Unity
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

# Set environment variables UNITY_DOWNLOAD_HASH and UNITY_VERSION prior to calling.
set -x

# Set OS-dependent variables.
OS=`uname -s`
ARCH=`uname -m`
if [ "$OS" = "Linux" ]
then
    # Identify Linux OS. Sets useful variables: ID, ID_LIKE, VERSION, NAME, PRETTY_NAME.
    source /etc/os-release
    # Windows Subsystem for Linux identifies itself as 'Linux'. Additional test required.
    if grep -qE "(Microsoft|WSL)" /proc/version &> /dev/null ; then
        OS='Windows'
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
UNITY_DOWNLOAD_HASH=${UNITY_DOWNLOAD_HASH:-61c2feb0970d}

mkdir -p "unity_installers" && cd "unity_installers"
downloads=()

if [ "$OS" = "Windows" ]
then
    downloads+=(
        https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/Windows64EditorInstaller/UnitySetup64.exe
        https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/TargetSupportInstaller/UnitySetup-Windows-IL2CPP-Support-for-Editor-${UNITY_VERSION}.exe
    )
    #downloads+=( https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/TargetSupportInstaller/UnitySetup-Mac-Mono-Support-for-Editor-${UNITY_VERSION}.exe )
    if [[ -n $WITH_ANDROID ]]; then
        downloads+=(
            https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/TargetSupportInstaller/UnitySetup-Android-Support-for-Editor-${UNITY_VERSION}.exe
        )
    fi
elif [ "$OS" = "Darwin" ]
then
	if [ "$ARCH" = "arm64" ]; then
		downloads+=(
			https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/MacEditorInstallerArm64/Unity.pkg
		)
	else
		downloads+=(
			https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/MacEditorInstaller/Unity.pkg
		)
	fi
    downloads+=(
        https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/MacEditorTargetInstaller/UnitySetup-Mac-IL2CPP-Support-for-Editor-${UNITY_VERSION}.pkg
    )
    #downloads+=( https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/MacEditorTargetInstaller/UnitySetup-Windows-Mono-Support-for-Editor-${UNITY_VERSION}.pkg )
    if [[ -n $WITH_ANDROID ]]; then
        downloads+=(
            https://download.unity3d.com/download_unity/${UNITY_DOWNLOAD_HASH}/MacEditorTargetInstaller/UnitySetup-Android-Support-for-Editor-${UNITY_VERSION}.pkg
        )
    fi
fi

if [[ -n $WITH_ANDROID ]]; then
    # Android tools. See https://docs.unity3d.com/Manual/android-sdksetup.html#supported-dependency-versions.
    if [[ "$UNITY_VERSION" =~ 2022\.3\..* ]]; then
        if [ "$OS" = "Windows" ]; then
            downloads+=(
                https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.14.1%2B1/OpenJDK11U-jdk_x64_windows_hotspot_11.0.14.1_1.zip
                https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip
                https://dl.google.com/android/repository/210b77e4bc623bd4cdda4dae790048f227972bd2.build-tools_r32-windows.zip
                https://dl.google.com/android/repository/platform-tools_r32.0.0-windows.zip
                https://dl.google.com/android/repository/android-ndk-r23b-windows.zip
                https://dl.google.com/android/repository/commandlinetools-win-8092744_latest.zip
            )
            #
        elif [ "$OS" = "Darwin" ]; then
            downloads+=(
                https://github.com/adoptium/temurin11-binaries/releases/download/jdk-11.0.14.1%2B1/OpenJDK11U-jdk_x64_mac_hotspot_11.0.14.1_1.tar.gz
                https://dl.google.com/android/repository/sdk-tools-darwin-4333796.zip
                https://dl.google.com/android/repository/5219cc671e844de73762e969ace287c29d2e14cd.build-tools_r32-macosx.zip
                https://dl.google.com/android/repository/platform-tools_r32.0.0-darwin.zip
				https://dl.google.com/android/repository/android-ndk-r23b-darwin.zip
				https://dl.google.com/android/repository/commandlinetools-mac-8092744_latest.zip
            )
        fi
        downloads+=(
            https://dl.google.com/android/repository/platform-31_r01.zip
            https://dl.google.com/android/repository/platform-32_r01.zip
        )
    elif [[ "$UNITY_VERSION" =~ 2021\.3\..* ]]; then
        if [ "$OS" = "Windows" ]; then
            downloads+=(
                https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk8u292-b10/OpenJDK8U-jdk_x64_windows_hotspot_8u292b10.zip
                https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip
                https://dl.google.com/android/repository/efbaa277338195608aa4e3dbd43927e97f60218c.build-tools_r30.0.2-windows.zip
                https://dl.google.com/android/repository/platform-tools_r30.0.4-windows.zip
                https://dl.google.com/android/repository/android-ndk-r21d-windows-x86_64.zip
                https://dl.google.com/android/repository/commandlinetools-win-6609375_latest.zip
            )
        elif [ "$OS" = "Darwin" ]; then
            downloads+=(
                https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk8u292-b10/OpenJDK8U-jdk_x64_mac_hotspot_8u292b10.tar.gz
                https://dl.google.com/android/repository/sdk-tools-darwin-4333796.zip
                https://dl.google.com/android/repository/5a6ceea22103d8dec989aefcef309949c0c42f1d.build-tools_r30.0.2-macosx.zip
                https://dl.google.com/android/repository/fbad467867e935dce68a0296b00e6d1e76f15b15.platform-tools_r30.0.4-darwin.zip
                https://dl.google.com/android/repository/android-ndk-r21d-darwin-x86_64.zip
                https://dl.google.com/android/repository/commandlinetools-mac-6609375_latest.zip
            )
        fi
        downloads+=(
            https://dl.google.com/android/repository/platform-29_r05.zip
            https://dl.google.com/android/repository/android-platform-30.zip
        )
    elif [[ "$UNITY_VERSION" =~ 2019\.2\..* ]]; then
        if [ "$OS" = "Windows" ]; then
            downloads+=(
                https://dl.google.com/android/repository/sdk-tools-windows-4333796.zip
                https://dl.google.com/android/repository/build-tools_r28.0.3-windows.zip
                https://dl.google.com/android/repository/platform-tools_r28.0.1-windows.zip
                https://dl.google.com/android/repository/android-ndk-r16b-windows-x86_64.zip
            )
        elif [ "$OS" = "Darwin" ]; then
            downloads+=(
                https://dl.google.com/android/repository/sdk-tools-darwin-4333796.zip
                https://dl.google.com/android/repository/build-tools_r28.0.3-macosx.zip
                https://dl.google.com/android/repository/platform-tools_r28.0.1-macosx.zip
                https://dl.google.com/android/repository/android-ndk-r16b-darwin-x86_64.zip )
        fi
        downloads+=(
            https://dl.google.com/android/repository/platform-28_r06.zip
        )
    else
        echo "Warning: No Android tools URLs defined for Unity $UNITY_VERSION"
    fi
fi

# Do the downloads.
for download in "${downloads[@]}"
do
     curl -LO ${download}
done

cd ..
