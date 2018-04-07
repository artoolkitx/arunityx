# Source https://bitbucket.org/WeWantToKnow/unity3d_scripts

# Script to download and install unity given a url from unity
# supports 2 url styles
# http://beta.unity3d.com/download/xxxxxxxxx/unity_update-4.2.1f4.dmg
# http://beta.unity3d.com/download/xxxxxxxxx/MacEditorInstaller/Unity.pkg
# or http://netstorage.unity3d.com/unity/unity-4.6.1.dmg
# http://download.unity3d.com/download_unity/649f48bbbf0f/UnityDownloadAssistant-5.4.1f1.dmg
# http://netstorage.unity3d.com/unity/01f4c123905a/MacEditorInstaller/Unity-5.4.3f1.pkg?_ga=1.134650737.1498217150.1465512057

# comment out to test
#dry_run=echo

set -x

url_or_version=$1

# where am I stored - http://stackoverflow.com/questions/59895/can-a-bash-script-tell-what-directory-its-stored-in
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"

# 
usage() {
	echo "USAGE: $0 url_or_version"
}

# If version passed instead of url for dmg, run python sript to install requested version
if [[ $url_or_version != *"http://"* ]]; then
  if [[ -z "${UNITY_INSTALL_ARGS}" ]]; then
    args=--all-packages
  else
    args="${UNITY_INSTALL_ARGS}"
  fi
  python $DIR/install-unity.py $url_or_version ${args[@]}
  exit
fi

url=$url_or_version

echo "Installing Unity from $url"

# this works for both dmg and pkg style downloads
file=`echo $url | sed -e 's/.*\/\([^\/]*\)/\1/'`

echo $url | grep http://beta.unity3d.com/download/
if [[ $? -eq 0 ]]; then
	# if downloading form beta.unity3d.com, we try to extract the build number
	build=`echo $url | sed -e 's/.*download\/\([^/]*\).*/\1/'`
	targetfile="${build}_${file}"
else
	# otherwise we just use the url file path
	targetfile="${file}"
fi

echo "Downloading into $targetfile"
# we could have considered not archiving them
# filename=$(basename "$file")
# extension="${filename##*.}"
# tempfoo=`basename $0`
# targetfile=`mktemp /tmp/${tempfoo}.XXXXXX.$extension` || exit 1

$dry_run curl -C - -o $targetfile -L $url || exit 1
$dry_run sudo $DIR/install_unity3d.sh $targetfile