/*
 *  PluginFunctions.cs
 *  artoolkitX for Unity
 *
 *  This file is part of artoolkitX for Unity.
 *
 *  artoolkitX for Unity is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artoolkitX for Unity is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artoolkitX for Unity.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public static class PluginFunctions
{
	[NonSerialized]
	public static bool inited = false;

	// Delegate type declaration.
	public delegate void LogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);

	// Delegate instance.
	private static LogCallback logCallback = null;
	private static GCHandle logCallbackGCH;

	public static void arwRegisterLogCallback(LogCallback lcb)
	{
		logCallback = lcb;
		if (lcb != null) {
			logCallbackGCH = GCHandle.Alloc(logCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
		}
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwRegisterLogCallback(logCallback);
		else ARNativePlugin.arwRegisterLogCallback(logCallback);
		if (lcb == null) {
			logCallbackGCH.Free();
		}
	}

	public static void arwSetLogLevel(int logLevel)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetLogLevel(logLevel);
		else ARNativePlugin.arwSetLogLevel(logLevel);
	}

	public static bool arwInitialiseAR(int pattSize = 16, int pattCountMax = 25)
	{
		bool ok;
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            ok = ARNativePluginStatic.arwInitialiseAR ();
            PluginFunctions.arwSetPatternSize(pattSize);
            PluginFunctions.arwSetPatternCountMax(pattCountMax);
//            ok = ARNativePluginStatic.arwInitialiseARWithOptions (pattSize, pattCountMax);
        } else {
            ok = ARNativePlugin.arwInitialiseAR();
            PluginFunctions.arwSetPatternSize(pattSize);
            PluginFunctions.arwSetPatternCountMax(pattCountMax);
        }
		if (ok) PluginFunctions.inited = true;
		return ok;
	}

    public static void arwSetPatternSize(int size) {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            ARNativePluginStatic.arwSetTrackerOptionInt (PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE, size);
        else
            ARNativePlugin.arwSetTrackerOptionInt (PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE, size);
    }

    public static void arwSetPatternCountMax(int count) {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            ARNativePluginStatic.arwSetTrackerOptionInt (PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX, count);
        else
            ARNativePlugin.arwSetTrackerOptionInt (PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX, count);
    }
	
	public static string arwGetARToolKitVersion()
	{
		StringBuilder sb = new StringBuilder(128);
		bool ok;
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetARToolKitVersion(sb, sb.Capacity);
		else ok = ARNativePlugin.arwGetARToolKitVersion(sb, sb.Capacity);
		if (ok) return sb.ToString();
		else return "unknown";
	}

	public static int arwGetError()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetError();
		else return ARNativePlugin.arwGetError();
	}

    public static bool arwShutdownAR()
	{
		bool ok;
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwShutdownAR();
		else ok = ARNativePlugin.arwShutdownAR();
		if (ok) PluginFunctions.inited = false;
		return ok;
	}
	
	public static bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStartRunningB(vconf, cparaBuff, cparaBuffLen);
		else return ARNativePlugin.arwStartRunningB(vconf, cparaBuff, cparaBuffLen);
	}
	
	public static bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStartRunningStereoB(vconfL, cparaBuffL, cparaBuffLenL, vconfR, cparaBuffR, cparaBuffLenR, transL2RBuff, transL2RBuffLen);
		else return ARNativePlugin.arwStartRunningStereoB(vconfL, cparaBuffL, cparaBuffLenL, vconfR, cparaBuffR, cparaBuffLenR, transL2RBuff, transL2RBuffLen);
	}

	public static bool arwIsRunning()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwIsRunning();
		else return ARNativePlugin.arwIsRunning();
	}

	public static bool arwStopRunning()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwStopRunning();
		else return ARNativePlugin.arwStopRunning();
	}

	public static bool arwGetProjectionMatrix(float nearPlane, float farPlane, float[] matrix)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetProjectionMatrix(nearPlane, farPlane, matrix);
		else return ARNativePlugin.arwGetProjectionMatrix(nearPlane, farPlane, matrix);
	}

	public static bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, float[] matrixL, float[] matrixR)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetProjectionMatrixStereo(nearPlane, farPlane, matrixL, matrixR);
		else return ARNativePlugin.arwGetProjectionMatrixStereo(nearPlane, farPlane, matrixL, matrixR);
	}

	public static bool arwGetVideoParams(out int width, out int height, out int pixelSize, out String pixelFormatString)
	{
		StringBuilder sb = new StringBuilder(128);
		bool ok;
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetVideoParams(out width, out height, out pixelSize, sb, sb.Capacity);
		else ok = ARNativePlugin.arwGetVideoParams(out width, out height, out pixelSize, sb, sb.Capacity);
		if (!ok) pixelFormatString = "";
		else pixelFormatString = sb.ToString();
		return ok;
	}

	public static bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, out String pixelFormatL, out int widthR, out int heightR, out int pixelSizeR, out String pixelFormatR)
	{
		StringBuilder sbL = new StringBuilder(128);
		StringBuilder sbR = new StringBuilder(128);
		bool ok;
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetVideoParamsStereo(out widthL, out heightL, out pixelSizeL, sbL, sbL.Capacity, out widthR, out heightR, out pixelSizeR, sbR, sbR.Capacity);
		else ok = ARNativePlugin.arwGetVideoParamsStereo(out widthL, out heightL, out pixelSizeL, sbL, sbL.Capacity, out widthR, out heightR, out pixelSizeR, sbR, sbR.Capacity);
		if (!ok) {
			pixelFormatL = "";
			pixelFormatR = "";
		} else {
			pixelFormatL = sbL.ToString();
			pixelFormatR = sbR.ToString();
		}
		return ok;
	}

	public static bool arwCapture()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwCapture();
		else return ARNativePlugin.arwCapture();
	}

	public static bool arwUpdateAR()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwUpdateAR();
		else return ARNativePlugin.arwUpdateAR();
	}
		public static bool arwUpdateTexture32([In, Out]Color32[] colors32)
	{
		bool ok;
		GCHandle handle = GCHandle.Alloc(colors32, GCHandleType.Pinned);
		IntPtr address = handle.AddrOfPinnedObject();
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwUpdateTexture32(address);
		else ok = ARNativePlugin.arwUpdateTexture32(address);
		handle.Free();
		return ok;
	}
	
	public static bool arwUpdateTexture32Stereo([In, Out]Color32[] colors32L, [In, Out]Color32[] colors32R)
	{
		bool ok;
		GCHandle handle0 = GCHandle.Alloc(colors32L, GCHandleType.Pinned);
		GCHandle handle1 = GCHandle.Alloc(colors32R, GCHandleType.Pinned);
		IntPtr address0 = handle0.AddrOfPinnedObject();
		IntPtr address1 = handle1.AddrOfPinnedObject();
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwUpdateTexture32Stereo(address0, address1);
		else ok = ARNativePlugin.arwUpdateTexture32Stereo(address0, address1);
		handle0.Free();
		handle1.Free();
		return ok;
	}

	public static int arwGetMarkerPatternCount(int markerID)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetMarkerPatternCount(markerID);
		else return ARNativePlugin.arwGetMarkerPatternCount(markerID);
	}

	public static bool arwGetMarkerPatternConfig(int markerID, int patternID, float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetMarkerPatternConfig(markerID, patternID, matrix, out width, out height, out imageSizeX, out imageSizeY);
		else return ARNativePlugin.arwGetMarkerPatternConfig(markerID, patternID, matrix, out width, out height, out imageSizeX, out imageSizeY);
	}
	
	public static bool arwGetMarkerPatternImage(int markerID, int patternID, [In, Out]Color[] colors)
	{
		bool ok;
		if (Application.platform == RuntimePlatform.IPhonePlayer) ok = ARNativePluginStatic.arwGetMarkerPatternImage(markerID, patternID, colors);
		else ok = ARNativePlugin.arwGetMarkerPatternImage(markerID, patternID, colors);
		return ok;
	}
	
	public static bool arwGetMarkerOptionBool(int markerID, int option)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetMarkerOptionBool(markerID, option);
		else return ARNativePlugin.arwGetMarkerOptionBool(markerID, option);
	}
	
	public static void arwSetMarkerOptionBool(int markerID, int option, bool value)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetMarkerOptionBool(markerID, option, value);
		else ARNativePlugin.arwSetMarkerOptionBool(markerID, option, value);
	}

	public static int arwGetMarkerOptionInt(int markerID, int option)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetMarkerOptionInt(markerID, option);
		else return ARNativePlugin.arwGetMarkerOptionInt(markerID, option);
	}
	
	public static void arwSetMarkerOptionInt(int markerID, int option, int value)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetMarkerOptionInt(markerID, option, value);
		else ARNativePlugin.arwSetMarkerOptionInt(markerID, option, value);
	}

	public static float arwGetMarkerOptionFloat(int markerID, int option)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetMarkerOptionFloat(markerID, option);
		else return ARNativePlugin.arwGetMarkerOptionFloat(markerID, option);
	}
	
	public static void arwSetMarkerOptionFloat(int markerID, int option, float value)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetMarkerOptionFloat(markerID, option, value);
		else ARNativePlugin.arwSetMarkerOptionFloat(markerID, option, value);
	}

	public static void arwSetVideoDebugMode(bool debug)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE, debug);
		else ARNativePlugin.arwSetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE, debug);
	}

	public static bool arwGetVideoDebugMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE);
		else return ARNativePlugin.arwGetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE);
	}

	public static void arwSetVideoThreshold(int threshold)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD, threshold);
        else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD, threshold);
	}

	public static int arwGetVideoThreshold()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD);
        else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD);
	}

	public static void arwSetVideoThresholdMode(int mode)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE, mode);
        else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE, mode);
	}

	public static int arwGetVideoThresholdMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE);
        else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE);
	}

	public static void arwSetLabelingMode(int mode)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_LABELING_MODE, mode);
		else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_LABELING_MODE,mode);
	}

	public static int arwGetLabelingMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_LABELING_MODE);
		else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_LABELING_MODE);
	}

    public static int ARW_TRACKER_OPTION_NFT_MULTIMODE = 0,                          ///< bool.
    						ARW_TRACKER_OPTION_SQUARE_THRESHOLD = 1,                       ///< Threshold value used for image binarization. int in range [0-255].
    						ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE = 2,                  ///< Threshold mode used for image binarization. int.
    						ARW_TRACKER_OPTION_SQUARE_LABELING_MODE = 3,                   ///< int.
    						ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE = 4,          ///< int.
    						ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE = 5,                     ///< float in range (0-0.5).
    						ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE = 6,                ///< int.
    						ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE = 7,                 ///< int.
    						ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE = 8,                      ///< Enables or disable state of debug mode in the tracker. When enabled, a black and white debug image is generated during marker detection. The debug image is useful for visualising the binarization process and choosing a threshold value. bool.
                            ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE = 9,                    ///< Number of rows and columns in square template (pattern) markers. Defaults to AR_PATT_SIZE1, which is 16 in all versions of ARToolKit prior to 5.3. int.
                            ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX = 10; 

	public static void arwSetBorderSize(float size)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionFloat(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE, size);
		else ARNativePlugin.arwSetTrackerOptionFloat(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE, size);
	}

	public static float arwGetBorderSize()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionFloat(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE);
		else return ARNativePlugin.arwGetTrackerOptionFloat(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE);
	}

	public static void arwSetPatternDetectionMode(int mode)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE, mode);
		else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE, mode);
	}

	public static int arwGetPatternDetectionMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE);
		else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE);
	}

	public static void arwSetMatrixCodeType(int type)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE, type);
		else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE, type);
	}

	public static int arwGetMatrixCodeType()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE);
		else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE);
	}

	public static void arwSetImageProcMode(int mode)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE, mode);
		else ARNativePlugin.arwSetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE, mode);
	}

	public static int arwGetImageProcMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE);
		else return ARNativePlugin.arwGetTrackerOptionInt(PluginFunctions.ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE);
	}
	
	public static void arwSetNFTMultiMode(bool on)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) ARNativePluginStatic.arwSetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_NFT_MULTIMODE, on);
		else ARNativePlugin.arwSetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_NFT_MULTIMODE, on);
	}

	public static bool arwGetNFTMultiMode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwGetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_NFT_MULTIMODE);
		else return ARNativePlugin.arwGetTrackerOptionBool(PluginFunctions.ARW_TRACKER_OPTION_NFT_MULTIMODE);
	}


	public static int arwAddMarker(string cfg)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwAddMarker(cfg);
		else return ARNativePlugin.arwAddMarker(cfg);
	}
	
	public static bool arwRemoveMarker(int markerID)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwRemoveMarker(markerID);
		else return ARNativePlugin.arwRemoveMarker(markerID);
	}

	public static int arwRemoveAllMarkers()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwRemoveAllMarkers();
		else return ARNativePlugin.arwRemoveAllMarkers();
	}

	public static bool arwQueryMarkerVisibilityAndTransformation(int markerID, float[] matrix)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwQueryMarkerVisibilityAndTransformation(markerID, matrix);
		else return ARNativePlugin.arwQueryMarkerVisibilityAndTransformation(markerID, matrix);
	}

	public static bool arwQueryMarkerVisibilityAndTransformationStereo(int markerID, float[] matrixL, float[] matrixR)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwQueryMarkerVisibilityAndTransformationStereo(markerID, matrixL, matrixR);
		else return ARNativePlugin.arwQueryMarkerVisibilityAndTransformationStereo(markerID, matrixL, matrixR);
	}
	
	public static bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float projectionNearPlane, float projectionFarPlane, out float fovy_p, out float aspect_p, float[] m, float[] p)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) return ARNativePluginStatic.arwLoadOpticalParams(optical_param_name, optical_param_buff, optical_param_buffLen, projectionNearPlane, projectionFarPlane, out fovy_p, out aspect_p, m, p);
		else return ARNativePlugin.arwLoadOpticalParams(optical_param_name, optical_param_buff, optical_param_buffLen, projectionNearPlane, projectionFarPlane, out fovy_p, out aspect_p, m, p);
	}

}
