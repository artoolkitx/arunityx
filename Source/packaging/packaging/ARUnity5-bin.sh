#! /bin/sh
#
# Find out where we are and change to ARUnity5 root.
#
OURDIR=$(cd "$(dirname "$0")"; pwd)
cd "${OURDIR}/../../"
echo "Building archive from directory \"$PWD\"."

echo "Enter the version number of this build (in format ([0-9]+.[0-9]+)(.[0-9]+)?(r[0-9]+)?) : "
read VERSION

if ([[ $VERSION =~ ^(([0-9]+.[0-9]+)(.[0-9]+)?(r[0-9]+)?)$ ]] && echo matched)
then
    echo "Setting version to $VERSION"
else
    echo "Version number invalid!"
    exit 0
fi

echo "Do you want to copy the utilities from the OS X ARToolKit5 folder? (y or n) : "
read COPYUTILS_OSX
if [ "$COPYUTILS_OSX" = "y" ]
then
    echo "Enter the ABSOLUTE path of the OSX ARToolKit5 directory?"
    echo -n "Enter : "
    read DIR_OSX
    if [[ -d "$DIR_OSX" ]];
    then
        echo "$DIR_OSX is a directory"
    else
        echo "$DIR_OSX is not valid"
        exit 0
    fi
elif [ "$COPYUTILS_OSX" = "n" ]
then
    echo "Will not copy OS X tools."
else
    echo "Please enter y or n."
    exit 0
fi

echo "Do you want to copy the utilities from the WINDOWS ARToolKit5 folder? (y or n) : "
read COPYUTILS_WIN
if [ "$COPYUTILS_WIN" = "y" ]
then
    echo "Enter the ABSOLUTE path of the WINDOWS ARToolKit5 directory?"
    echo -n "Enter : "
    read DIR_WIN
    if [[ -d $DIR_WIN ]];
    then
        echo "$DIR_WIN is a directory"
    else
        echo "$DIR_WIN is not valid"
        exit 0
    fi
elif [ "$COPYUTILS_WIN" = "n" ]
then
    echo "Will not copy WINDOWS tools."
else
    echo "Please enter y or n."
    exit 0
fi

if [ "$COPYUTILS_OSX" = "y" ]
then
    cp -vp \
        $DIR_OSX/bin/calib_camera \
        $DIR_OSX/bin/calib_optical \
        $DIR_OSX/bin/calib_stereo \
        $DIR_OSX/bin/check_id \
        $DIR_OSX/bin/checkResolution \
        $DIR_OSX/bin/dispFeatureSet \
        $DIR_OSX/bin/dispImageSet \
        $DIR_OSX/bin/genTexData \
        $DIR_OSX/bin/genMarkerSet \
        $DIR_OSX/bin/mk_patt \
        bin/
fi

if [ "$COPYUTILS_WIN" = "y" ]
then
    cp -vp \
        $DIR_WIN/bin/calib_camera.exe \
        $DIR_WIN/bin/calib_optical.exe \
        $DIR_WIN/bin/calib_stereo.exe \
        $DIR_WIN/bin/check_id.exe \
        $DIR_WIN/bin/checkResolution.exe \
        $DIR_WIN/bin/dispFeatureSet.exe \
        $DIR_WIN/bin/dispImageSet.exe \
        $DIR_WIN/bin/genMarkerSet.exe \
        $DIR_WIN/bin/genTexData.exe \
        $DIR_WIN/bin/mk_patt.exe \
        $DIR_WIN/bin/ARvideo.dll \
        $DIR_WIN/bin/DSVL.dll \
        $DIR_WIN/bin/glut32.dll \
        $DIR_WIN/bin/pthreadVC2.dll \
        $DIR_WIN/bin/opencv_calib3d2410.dll \
        $DIR_WIN/bin/opencv_core2410.dll \
        $DIR_WIN/bin/opencv_features2d2410.dll \
        $DIR_WIN/bin/opencv_flann2410.dll \
        $DIR_WIN/bin/opencv_imgproc2410.dll \
        bin/
    cp -vp \
        $DIR_WIN/bin/vcredist_x86.exe \
        redist/
    cp -vp \
        $DIR_WIN/bin64/vcredist_x64.exe \
        redist64/
fi

# Rename version, where appropriate.

ARTOOLKIT_ROOT='src/Unity/Assets/ARToolKit5-Unity'
sed -Ei "" "s/version (([0-9]+\.[0-9]+)(\.[0-9]+)?(r[0-9]+)?)/version $VERSION/" README.md
sed -Ei "" "s/ARToolKit for Unity Version (([0-9]+\.[0-9]+)(\.[0-9]+)?(r[0-9]+)?)/ARToolKit for Unity Version $VERSION/" $ARTOOLKIT_ROOT/Scripts/Editor/ARToolKitMenuEditor.cs

# Build the unitypackage.

cp ./README.md $ARTOOLKIT_ROOT
cp ./CHANGELOG.txt $ARTOOLKIT_ROOT

/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod ARToolKitPackager.CreatePackage -projectPath $(pwd)/src/Unity ARUnity5.unitypackage

rm $ARTOOLKIT_ROOT/README.md
rm $ARTOOLKIT_ROOT/CHANGELOG.txt
rm $ARTOOLKIT_ROOT/README.md.meta
rm $ARTOOLKIT_ROOT/CHANGELOG.txt.meta

mv ./src/Unity/ARUnity5.unitypackage .

# Build the archives.
# Exclude: build files and directories, version control info,
# settings files which don't carry over.

tar czvf "../ARUnity5-${VERSION}-tools-osx.tar.gz" \
    -T share/packaging/ARUnity5-tools-osx-bom \
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

rm -f "../ARUnity5-${VERSION}-tools-win.zip"
zip -r -MM "../ARUnity5-${VERSION}-tools-win.zip" . \
    -i@share/packaging/ARUnity5-tools-win-bom \
    --exclude "*/build/*" \
    --exclude "*/.DS_Store" \
    --exclude "*/local.properties" \
    --exclude "*/.gradle/*" \
    --exclude "*/.idea/*" \
    --exclude "*/*.iml" \

