set -x

file=$1
ipath=$2

usage() {
	echo "USAGE: $0 file[.dmg|.pkg] [installation path]"
}

if [ $# -eq 0 ] || [ $# -gt 2 ]; then
	echo "ERROR: invalid number of arguments"
	usage
	exit -1	
fi

if [ ! -z $ipath ] && [ -d $ipath ]; then
	echo "ERROR: $ipath directory already present"
	usage
	exit -1	
fi

if [ ! -f $file ]; then
	echo "ERROR: $file not a file"
	usage
	exit -1
fi

# A script to install Unity3d automatically from the command line given a dmg or pkg file.
# The resulting file is stored under /Applications/Unity$VERSION

# check assumptions
apphome=/Applications/Unity

if [ -d "$apphome" ]; then
    echo "ERROR: $apphome already present"
    exit -1
fi

filename=$(basename "$file")
extension="${filename##*.}"

if [ "$extension" == "dmg" ]; then
	echo "Detected dmg $file"
	dmg=$file
elif [ "$extension" == "pkg" ]; then
	echo "Detected pkg $file"
	pkg=$file
else
	echo "ERROR: unknown file format $file"
	exit -1
fi

if [ -n "$dmg" ]; then
	hdiutil verify $dmg || exit 1
	tempfoo=`basename $0`
	TMPFILE=`mktemp /tmp/${tempfoo}.XXXXXX` || exit 1
	hdiutil mount -readonly -nobrowse -plist $dmg > $TMPFILE
	vol=`grep Volumes $TMPFILE  | sed -e 's/.*>\(.*\)<\/.*/\1/'`
	pkg=`ls -1 "$vol"/*.pkg`
fi

installer -verbose -pkg "$pkg" -dominfo -volinfo -pkginfo
sudo installer -pkg "$pkg" -target /

if [ -n "$dmg" ]; then
	hdiutil unmount "$vol"
fi

if [ ! -d "$apphome" ]; then
    echo "ERROR: $apphome not present after installation. Something went wrong"
    exit -1
fi

appversion=`/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$apphome"/Unity.app/Contents/Info.plist`
if [ -d "$apphome$appversion" ]; then
    echo "ERROR: "$apphome$appversion" already present on disk. Something went wrong"
    sudo rm -rf "$apphome"
    exit -1
fi

if [ ! -z "$ipath" ]; then
	ipath="$apphome$appversion"
    sudo mv "$apphome" "$ipath"
    echo "Unity $appversion installed at $ipath"
else 
    echo "Unity $appversion installed at $apphome"
fi


