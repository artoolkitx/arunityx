Read me for artoolkitX utilities package.
========================================


Contents.
---------

About this archive.
Running the utilities.
Usage information.

About this archive.
-------------------

This archive contains the artoolkitX utilities.

artoolkitX utilities are made available to you under the GNU Lesser General Public License, version 3 (LGPLv3).

artoolkitX utilities package is designed to run on Windows and Macintosh OS X platforms.


Running the utilities
---------------------

artoolkitX includes a set of utilities to perform essential functions required by artoolkitX and/or arunityX. After installation, the executables for these utilities can be found in the SDK/bin directory inside your artoolkitX directory.

  Windows:

	Each utility can be opened by double-clicking its icon in the artoolkitX\SDK\bin directory. Alternately, you can run it from the command line:

	* Open a command-line window (cmd.exe).
	* Navigate to your artoolkitX\SDK\bin directory.
	* Type (for example): artoolkitx_mk_patt.exe

  Mac OS X:
  
    The utilities should be launched from a Terminal window:
  
    * Open a Terminal window (/Applications/Utilties/Terminal).
	* Navigate to your artoolkitX/SDK/bin directory.
	* Type (for example): ./artoolkitx_mk_patt


Which utilities are included:
-----------------------------

Camera calibration utilities:
 * artoolkitx_calib_camera
 * artoolkitx_calib_optical
 
Square marker utilities:
 * artoolkitx_mk_patt
 * artoolkitx_check_id
 * artoolkitx_genMarkerSet

2D texture trackable utilities:
 * artoolkitx_check_image_2d_tracking
 * artoolkitx_image_database2d
 
Legacy natural feature tracking (NFT) utilities:
 * artoolkitx_genTexData
 * artoolkitx_dispImageSet
 * artoolkitx_dispFeatureSet
 * artoolkitx_checkResolution
 
Required patterns for the utilities, which you can print out, are found in the folder "patterns" inside the "doc" folder. Images for sample 2D trackables and NFT markers are in the folder "Marker images" inside the "doc" folder.


Usage information
-----------------
Documentation for the utilities is held in the artoolkitX documentation wiki: https://github.com/artoolkitx/artoolkitx/wiki
        

--
EOF
