#! /bin/sh
#
# Find out where we are and change to ARUnity5 root.
#
OURDIR=$(cd "$(dirname "$0")"; pwd)
echo "Building archive from directory \"$PWD\"."
ARUNITYX_HOME=$OURDIR/../..

#Check if we are using the submodule to build
if [  -f "$OURDIR/../../Extras/artoolkitX/LICENSE.txt" ] && [ -z $ARTOOLKITX_HOME ]; then
    ARTOOLKITX_HOME=$OURDIR/../../Extras/artoolkitx/
fi

# Get version from <ARX/AR/ar.h> header.
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

if ([[ $VERSION =~ ^(([0-9]+.[0-9]+)(.[0-9]+)?(r[0-9]+)?)$ ]] && echo matched)
then
    echo "Setting version to $VERSION"
else
    echo "Version number invalid!"
    exit 0
fi

echo "Do you want to copy the utilities from the macOS artoolkitX folder? (y or n) : "
read COPYUTILS_OSX
if [ "$COPYUTILS_OSX" = "y" ]
then
    DIR_OSX=$ARTOOLKITX_HOME
elif [ "$COPYUTILS_OSX" = "n" ]
then
    echo "Will not copy OS X tools."
else
    echo "Please enter y or n."
    exit 0
fi

echo "Do you want to copy the utilities from the WINDOWS artoolkitX folder? (y or n) : "
read COPYUTILS_WIN
if [ "$COPYUTILS_WIN" = "y" ]
then
    read DIR_WIN

elif [ "$COPYUTILS_WIN" = "n" ]
then
    echo "Will not copy WINDOWS tools."
else
    echo "Please enter y or n."
    exit 0
fi
cd ARUNITYX_HOME
mk -p Utilities

if [ "$COPYUTILS_OSX" = "y" ]
then
    cp -vp \
        $DIR_OSX/Utilities/calib_camera \
        $DIR_OSX/Utilities/calib_optical \
        $DIR_OSX/Utilities/calib_stereo \
        $DIR_OSX/Utilities/check_id \
        $DIR_OSX/Utilities/checkResolution \
        $DIR_OSX/Utilities/dispFeatureSet \
        $DIR_OSX/Utilities/dispImageSet \
        $DIR_OSX/Utilities/genTexData \
        $DIR_OSX/Utilities/genMarkerSet \
        $DIR_OSX/Utilities/mk_patt \
        Utilities/
fi

if [ "$COPYUTILS_WIN" = "y" ]
then
    cp -vp \
        $DIR_WIN/Utilities/calib_camera.exe \
        $DIR_WIN/Utilities/calib_optical.exe \
        $DIR_WIN/Utilities/calib_stereo.exe \
        $DIR_WIN/Utilities/check_id.exe \
        $DIR_WIN/Utilities/checkResolution.exe \
        $DIR_WIN/Utilities/dispFeatureSet.exe \
        $DIR_WIN/Utilities/dispImageSet.exe \
        $DIR_WIN/Utilities/genMarkerSet.exe \
        $DIR_WIN/Utilities/genTexData.exe \
        $DIR_WIN/Utilities/mk_patt.exe \
        $DIR_WIN/Utilities/ARvideo.dll \
        $DIR_WIN/Utilities/DSVL.dll \
        $DIR_WIN/Utilities/glut32.dll \
        $DIR_WIN/Utilities/pthreadVC2.dll \
        $DIR_WIN/Utilities/opencv_calib3d2410.dll \
        $DIR_WIN/Utilities/opencv_core2410.dll \
        $DIR_WIN/Utilities/opencv_features2d2410.dll \
        $DIR_WIN/Utilities/opencv_flann2410.dll \
        $DIR_WIN/Utilities/opencv_imgproc2410.dll \
        Utilities/
    cp -vp \
        $DIR_WIN/Utilities/vcredist_x86.exe \
        redist/
    cp -vp \
        $DIR_WIN/Utilities64/vcredist_x64.exe \
        redist64/
fi

# Build the archives.
# Exclude: build files and directories, version control info,
# settings files which don't carry over.

tar czvf "../arunityX-${VERSION}-tools-osx.tar.gz" \
    -T share/packaging/arunityX-tools-osx-bom \
    --exclude "*.o" \
    --exclude "Makefile" \
    --exclude "build" \
    --exclude "*.mode1*" \
    --exclude "*.pbxuser" \
    --exclude ".DS_Store" \
    --exclude "desktop.ini" \
    --exclude "local.properties" \
    --exclude "*/.gradle" \
    --exclude "*/.idea" \
    --exclude "*.iml" \

rm -f "../arunityX-${VERSION}-tools-win.zip"
zip -r -MM "../arunityX-${VERSION}-tools-win.zip" . \
    -i@share/packaging/arunityX-tools-win-bom \
    --exclude "*/build/*" \
    --exclude "*/.DS_Store" \
    --exclude "*/local.properties" \
    --exclude "*/.gradle/*" \
    --exclude "*/.idea/*" \
    --exclude "*/*.iml" \

