#!/bin/bash

#
# Package arunityX as Unity Package Manager (upm) via GitHub branch.
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
# Copyright 2023-2024, artoolkitX Contributors.
# Author(s): Philip Lamb <phil@artoolkitx.org>
#


# fail if any commands fails
set -e
# debug log
set -x

# Get our location.
OURDIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ARUNITYX_HOME="$OURDIR/../.."
PACKAGE_PATH="${ARUNITYX_HOME}/Packages/artoolkitX-Unity"

git config --global user.name 'artoolkitx git bot'
git config --global user.email 'gitmaster@artoolkitx.org'

# First, commit the plugins.
# Find all files or links in plugins dir. Exclude .DS_Store and .meta files.
# Force add resulting list to git index.
git lfs install
find "${PACKAGE_PATH}/Runtime/Plugins" \( -type f -o -type l \) ! -name .DS_Store ! -name  '*.meta' | xargs -L1 git add -vf
git commit -m "upm packaging - add plugin binaries"

# Do a subtree split of the package folder into the upm branch.
git branch -d upm &> /dev/null || echo upm branch not found
git subtree split -P "${PACKAGE_PATH}" -b upm
git checkout upm

# Ensure package includes lfs config.
git fetch origin master
git checkout origin/master -- .gitattributes
git add .gitattributes

# Fixup samples so meta files are ignored.
if [[ -d "Samples" ]]; then
  git mv Samples Samples~
  rm -f Samples.meta
fi
git commit -am "upm packaging"

# Force-push the upm branch.
git push -f -u origin upm
