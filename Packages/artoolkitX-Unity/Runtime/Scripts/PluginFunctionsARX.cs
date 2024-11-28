/*
 *  PluginFunctionsARX.cs
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
 *  Copyright 2017-2018 Realmax, Inc.
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Philip Lamb, Julian Looser, Dan Bell, Thorsten Bux.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// A concrete implentation of the IPluginFunctions interface that calls into a local
/// instance of the ARX library via P/Invoke. This class performs any marshalling required
/// between managed and unmanaged code.
/// </summary>
public class PluginFunctionsARX : IPluginFunctions
{
    [NonSerialized]
    private bool inited = false;

    // Delegate instance.
    private PluginFunctionsLogCallback logCallback = null;
    private GCHandle logCallbackGCH;
    private PluginFunctionsTrackableEventCallback trackableEventCallback = null;
    private GCHandle trackableEventCallbackGCH;

    private readonly static int ARW_TRACKER_OPTION_NFT_MULTIMODE = 0,                 ///< bool.
                       ARW_TRACKER_OPTION_SQUARE_THRESHOLD = 1,                       ///< Threshold value used for image binarization. int in range [0-255].
                       ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE = 2,                  ///< Threshold mode used for image binarization. int.
                       ARW_TRACKER_OPTION_SQUARE_LABELING_MODE = 3,                   ///< int.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE = 4,          ///< int.
                       ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE = 5,                     ///< float in range (0-0.5).
                       ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE = 6,                ///< int.
                       ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE = 7,                 ///< int.
                       ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE = 8,                      ///< Enables or disable state of debug mode in the tracker. When enabled, a black and white debug image is generated during marker detection. The debug image is useful for visualising the binarization process and choosing a threshold value. bool.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE = 9,                    ///< Number of rows and columns in square template (pattern) markers. Defaults to AR_PATT_SIZE1, which is 16 in all versions of ARToolKit prior to 5.3. int.
                       ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX = 10,              ///< Maximum number of square template (pattern) markers that may be loaded at once. Defaults to AR_PATT_NUM_MAX, which is at least 25 in all versions of ARToolKit prior to 5.3. int.
                       /*ARW_TRACKER_OPTION_2D_TRACKER_FEATURE_TYPE = 11,*/           ///< Feature detector type used in the 2d Tracker - 0 AKAZE, 1 ORB, 2 BRISK, 3 KAZE, 4 SIFT
                       ARW_TRACKER_OPTION_2D_MAXIMUM_MARKERS_TO_TRACK = 12,           ///< Maximum number of markers able to be tracked simultaneously. Defaults to 1. Should not be set higher than the number of 2D markers loaded.
                       ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES = 13, ///< If true, when the square tracker is detecting matrix (barcode) markers, new trackables will be created for unmatched markers. Defaults to false. bool.
                       ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES_DEFAULT_WIDTH = 14, ///< If ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES is true, this value will be used for the initial width of new trackables for unmatched markers. Defaults to 80.0f. float.
                       ARW_TRACKER_OPTION_2D_THREADED = 15;                           ///< bool, If false, 2D tracking updates synchronously, and arwUpdateAR will not return until 2D tracking is complete. If true, 2D tracking updates asychronously on a secondary thread, and arwUpdateAR will not block if the track is busy. Defaults to true.

    override public bool IsConfigured()
    {
        return true;
    }

    override public bool Configure(string config)
    {
        return true;
    }

    override public bool IsInited()
    {
        return this.inited;
    }

    override public void arwRegisterLogCallback(PluginFunctionsLogCallback lcb)
    {
        if (lcb != null)
        {
            // If setting, create the callback stub prior to registering the callback on the native side.
            if (logCallback != null)
            {
                // But if already set, unset first.
                ARX_pinvoke.arwRegisterLogCallback(null);
                logCallbackGCH.Free();
            }
            logCallbackGCH = GCHandle.Alloc(lcb); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487
        }
        ARX_pinvoke.arwRegisterLogCallback(lcb);
        if (lcb == null && logCallback != null)
        {
            // If unsetting, free the callback stub after deregistering the callback on the native side.
            if (logCallbackGCH != null) logCallbackGCH.Free();
        }
        logCallback = lcb;
    }

    override public void arwSetLogLevel(int logLevel)
    {
        ARX_pinvoke.arwSetLogLevel(logLevel);
    }

    override public bool arwInitialiseAR(int pattSize = 16, int pattCountMax = 25, int matrixCodeType = 0x03)
    {
        bool ok = ARX_pinvoke.arwInitialiseAR();
        if (ok) {
            arwSetPatternSize(pattSize);
            arwSetPatternCountMax(pattCountMax);
            arwSetMatrixCodeType(matrixCodeType);
            this.inited = true;
        }
        return ok;
    }

    override public void arwSetPatternSize(int size)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_SIZE, size);
    }

    override public void arwSetPatternCountMax(int count)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_COUNT_MAX, count);
    }

    override public string arwGetARToolKitVersion()
    {
        StringBuilder sb = new StringBuilder(128);
        bool ok = ARX_pinvoke.arwGetARToolKitVersion(sb, sb.Capacity);
        if (ok) return sb.ToString();
        else return "unknown";
    }

    override public int arwGetError()
    {
        return ARX_pinvoke.arwGetError();
    }

    override public bool arwShutdownAR()
    {
        bool ok = ARX_pinvoke.arwShutdownAR();
        if (ok) this.inited = false;
        return ok;
    }

    override public bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen)
    {
        return ARX_pinvoke.arwStartRunningB(vconf, cparaBuff, cparaBuffLen);
    }

    override public bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen)
    {
        return ARX_pinvoke.arwStartRunningStereoB(vconfL, cparaBuffL, cparaBuffLenL, vconfR, cparaBuffR, cparaBuffLenR, transL2RBuff, transL2RBuffLen);
    }

    override public bool arwIsRunning()
    {
        return ARX_pinvoke.arwIsRunning();
    }

    override public bool arwStopRunning()
    {
        return ARX_pinvoke.arwStopRunning();
    }

    override public bool arwGetProjectionMatrix(float nearPlane, float farPlane, float[] matrix)
    {
        return ARX_pinvoke.arwGetProjectionMatrix(nearPlane, farPlane, matrix);
    }

    override public bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, float[] matrixL, float[] matrixR)
    {
        return ARX_pinvoke.arwGetProjectionMatrixStereo(nearPlane, farPlane, matrixL, matrixR);
    }

    override public bool arwGetProjectionMatrixForViewportSizeAndFittingMode(int width, int height, int scaleMode, int hAlign, int vAlign, float nearPlane, float farPlane, float[] matrix)
    {
        return ARX_pinvoke.arwGetProjectionMatrixForViewportSizeAndFittingMode(width, height, scaleMode, hAlign, vAlign, nearPlane, farPlane, matrix);
    }

    override public bool arwGetProjectionMatrixForViewportSizeAndFittingModeStereo(int width, int height, int scaleMode, int hAlign, int vAlign, float nearPlane, float farPlane, float[] matrixL, float[] matrixR)
    {
        return ARX_pinvoke.arwGetProjectionMatrixForViewportSizeAndFittingModeStereo(width, height, scaleMode, hAlign, vAlign, nearPlane, farPlane, matrixL, matrixR);
    }

    override public bool arwGetVideoParams(out int width, out int height, out int pixelSize, out String pixelFormatString)
    {
        StringBuilder sb = new StringBuilder(128);
        bool ok = ARX_pinvoke.arwGetVideoParams(out width, out height, out pixelSize, sb, sb.Capacity);
        if (!ok) pixelFormatString = "";
        else pixelFormatString = sb.ToString();
        return ok;
    }

    override public bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, out String pixelFormatL, out int widthR, out int heightR, out int pixelSizeR, out String pixelFormatR)
    {
        StringBuilder sbL = new StringBuilder(128);
        StringBuilder sbR = new StringBuilder(128);
        bool ok = ARX_pinvoke.arwGetVideoParamsStereo(out widthL, out heightL, out pixelSizeL, sbL, sbL.Capacity, out widthR, out heightR, out pixelSizeR, sbR, sbR.Capacity);
        if (!ok) {
            pixelFormatL = "";
            pixelFormatR = "";
		} else {
            pixelFormatL = sbL.ToString();
            pixelFormatR = sbR.ToString();
        }
        return ok;
    }

    override public bool arwCapture()
    {
        return ARX_pinvoke.arwCapture();
    }

    override public bool arwUpdateAR()
    {
        return ARX_pinvoke.arwUpdateAR();
    }

    override public bool arwUpdateTexture32([In, Out]Color32[] colors32)
    {
        GCHandle handle = GCHandle.Alloc(colors32, GCHandleType.Pinned);
        IntPtr address = handle.AddrOfPinnedObject();
        bool ok = ARX_pinvoke.arwUpdateTexture32(address);
        handle.Free();
        return ok;
    }

    override public bool arwUpdateTexture32Stereo([In, Out]Color32[] colors32L, [In, Out]Color32[] colors32R)
    {
        GCHandle handle0 = GCHandle.Alloc(colors32L, GCHandleType.Pinned);
        GCHandle handle1 = GCHandle.Alloc(colors32R, GCHandleType.Pinned);
        IntPtr address0 = handle0.AddrOfPinnedObject();
        IntPtr address1 = handle1.AddrOfPinnedObject();
        bool ok = ARX_pinvoke.arwUpdateTexture32Stereo(address0, address1);
        handle0.Free();
        handle1.Free();
        return ok;
    }

    override public int arwGetTrackablePatternCount(int markerID)
    {
        return ARX_pinvoke.arwGetTrackablePatternCount(markerID);
    }

    override public bool arwGetTrackablePatternConfig(int markerID, int patternID, float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY)
    {
        return ARX_pinvoke.arwGetTrackablePatternConfig(markerID, patternID, matrix, out width, out height, out imageSizeX, out imageSizeY);
    }

    override public bool arwGetTrackablePatternImage(int markerID, int patternID, [In, Out]Color32[] colors32)
    {
        return ARX_pinvoke.arwGetTrackablePatternImage(markerID, patternID, colors32);
    }

    override public bool arwGetTrackableOptionBool(int markerID, int option)
    {
        return ARX_pinvoke.arwGetTrackableOptionBool(markerID, option);
    }

    override public void arwSetTrackableOptionBool(int markerID, int option, bool value)
    {
        ARX_pinvoke.arwSetTrackableOptionBool(markerID, option, value);
    }

    override public int arwGetTrackableOptionInt(int markerID, int option)
    {
        return ARX_pinvoke.arwGetTrackableOptionInt(markerID, option);
    }

    override public void arwSetTrackableOptionInt(int markerID, int option, int value)
    {
        ARX_pinvoke.arwSetTrackableOptionInt(markerID, option, value);
    }

    override public float arwGetTrackableOptionFloat(int markerID, int option)
    {
        return ARX_pinvoke.arwGetTrackableOptionFloat(markerID, option);
    }

    override public void arwSetTrackableOptionFloat(int markerID, int option, float value)
    {
        ARX_pinvoke.arwSetTrackableOptionFloat(markerID, option, value);
    }

    override public string arwGetTrackableOptionString(int markerID, int option)
    {
        StringBuilder sb = new StringBuilder(1024);
        bool ok = ARX_pinvoke.arwGetTrackableOptionString(markerID, option, sb, sb.Capacity);
        if (ok) return sb.ToString();
        else return "";
    }

    override public void arwSetTrackableOptionString(int markerID, int option, string value)
    {
        ARX_pinvoke.arwSetTrackableOptionString(markerID, option, value);
    }

    override public void arwSetVideoDebugMode(bool debug)
    {
        ARX_pinvoke.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE, debug);
    }

    override public bool arwGetVideoDebugMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_DEBUG_MODE);
    }

    override public void arwSetVideoThreshold(int threshold)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD, threshold);
    }

    override public int arwGetVideoThreshold()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD);
    }

    override public void arwSetVideoThresholdMode(int mode)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE, mode);
    }

    override public int arwGetVideoThresholdMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_THRESHOLD_MODE);
    }

    override public void arwSetLabelingMode(int mode)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE, mode);
    }

    override public int arwGetLabelingMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_LABELING_MODE);
    }

    override public void arwSetBorderSize(float size)
    {
        ARX_pinvoke.arwSetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE, size);
    }

    override public float arwGetBorderSize()
    {
        return ARX_pinvoke.arwGetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_BORDER_SIZE);
    }

    override public void arwSetPatternDetectionMode(int mode)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE, mode);
    }

    override public int arwGetPatternDetectionMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_PATTERN_DETECTION_MODE);
    }

    override public void arwSetMatrixCodeType(int type)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE, type);
    }

    override public int arwGetMatrixCodeType()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_MATRIX_CODE_TYPE);
    }

    override public void arwSetImageProcMode(int mode)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE, mode);
    }

    override public int arwGetImageProcMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_SQUARE_IMAGE_PROC_MODE);
    }

    override public void arwSetNFTMultiMode(bool on)
    {
        ARX_pinvoke.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE, on);
    }

    override public bool arwGetNFTMultiMode()
    {
        return ARX_pinvoke.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_NFT_MULTIMODE);
    }

    override public void arwSet2DMaxMarkersToTrack(int maxMarkersToTrack)
    {
        ARX_pinvoke.arwSetTrackerOptionInt(ARW_TRACKER_OPTION_2D_MAXIMUM_MARKERS_TO_TRACK, maxMarkersToTrack);
    }

    override public bool arwGet2DThreaded()
    {
        return ARX_pinvoke.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_2D_THREADED);
    }

    override public void arwSet2DThreaded(bool threaded)
    {
        ARX_pinvoke.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_2D_THREADED, threaded);
    }

    override public int arwGet2DMaxMarkersToTrack()
    {
        return ARX_pinvoke.arwGetTrackerOptionInt(ARW_TRACKER_OPTION_2D_MAXIMUM_MARKERS_TO_TRACK);
    }

    override public int arwAddTrackable(string cfg)
    {
        return ARX_pinvoke.arwAddTrackable(cfg);
    }

    override public bool arwRemoveTrackable(int markerID)
    {
        return ARX_pinvoke.arwRemoveTrackable(markerID);
    }

    override public int arwRemoveAllTrackables()
    {
        return ARX_pinvoke.arwRemoveAllTrackables();
    }

    override public bool arwQueryTrackableVisibilityAndTransformation(int markerID, float[] matrix)
    {
        return ARX_pinvoke.arwQueryTrackableVisibilityAndTransformation(markerID, matrix);
    }

    override public bool arwQueryTrackableVisibilityAndTransformationStereo(int markerID, float[] matrixL, float[] matrixR)
    {
        return ARX_pinvoke.arwQueryTrackableVisibilityAndTransformationStereo(markerID, matrixL, matrixR);
    }

    override public bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float projectionNearPlane, float projectionFarPlane, out float fovy_p, out float aspect_p, float[] m, float[] p)
    {
        return ARX_pinvoke.arwLoadOpticalParams(optical_param_name, optical_param_buff, optical_param_buffLen, projectionNearPlane, projectionFarPlane, out fovy_p, out aspect_p, m, p);
    }

    override public int arwCreateVideoSourceInfoList(string config)
    {
        return ARX_pinvoke.arwCreateVideoSourceInfoList(config);
    }

    override public bool arwGetVideoSourceInfoListEntry(int index, out string name, out string model, out string UID, out int flags, out string openToken)
    {
        StringBuilder sbName = new StringBuilder(1024);
        StringBuilder sbModel = new StringBuilder(1024);
        StringBuilder sbUID = new StringBuilder(1024);
        StringBuilder sbOpenToken = new StringBuilder(1024);
        bool ok = ARX_pinvoke.arwGetVideoSourceInfoListEntry(index, sbName, sbName.Capacity, sbModel, sbModel.Capacity, sbUID, sbUID.Capacity, out flags, sbOpenToken, sbOpenToken.Capacity);
        if (!ok)
        {
            name = "";
            model = "";
            UID = "";
            flags = 0;
            openToken = "";
        }
        else
        {
            name = sbName.ToString();
            model = sbModel.ToString();
            UID = sbUID.ToString();
            flags = 0;
            openToken = sbOpenToken.ToString();
        }
        return ok;
    }

    override public void arwDeleteVideoSourceInfoList()
    {
        ARX_pinvoke.arwDeleteVideoSourceInfoList();
    }

    override public void arwSetSquareMatrixModeAutocreateNewTrackables(bool on, float defaultWidth = 0.08f, PluginFunctionsTrackableEventCallback tecb = null)
    {
        if (!on) tecb = null;
        if (tecb != null)
        {
            // If setting, create the callback stub prior to registering the callback on the native side.
            if (trackableEventCallback != null)
            {
                // But if already set, unset first.
                ARX_pinvoke.arwRegisterTrackableEventCallback(null);
                trackableEventCallbackGCH.Free();
            }
            trackableEventCallbackGCH = GCHandle.Alloc(tecb); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        }
        ARX_pinvoke.arwRegisterTrackableEventCallback(tecb);
        if (tecb == null && trackableEventCallback != null)
        {
            // If unsetting, free the callback stub after deregistering the callback on the native side.
            trackableEventCallbackGCH.Free();
        }
        trackableEventCallback = tecb;
        ARX_pinvoke.arwSetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES_DEFAULT_WIDTH, defaultWidth * 1000.0f);
        ARX_pinvoke.arwSetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES, on);
    }

    override public bool arwGetSquareMatrixModeAutocreateNewTrackables(out bool on, out float defaultWidth, out PluginFunctionsTrackableEventCallback tecb)
    {
        on = ARX_pinvoke.arwGetTrackerOptionBool(ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES);
        defaultWidth = ARX_pinvoke.arwGetTrackerOptionFloat(ARW_TRACKER_OPTION_SQUARE_MATRIX_MODE_AUTOCREATE_NEW_TRACKABLES_DEFAULT_WIDTH) * 0.001f;
        tecb = trackableEventCallback;
        return true;
    }

    override public int arwVideoPushInit(int videoSourceIndex, int width, int height, string pixelFormat, int cameraIndex, int cameraPosition)
    {
        return ARX_pinvoke.arwVideoPushInit(videoSourceIndex, width, height, pixelFormat, cameraIndex, cameraPosition);
    }

    private struct arwVideoPushReleaseCallbackStubData
    {
        public PluginFunctionsVideoPushReleaseCallback releaseCallback;
        public object releaseCallbackUserdata;
    }

    [AOT.MonoPInvokeCallback(typeof(ARX_pinvoke.arwVideoPushReleaseCallback))]
    private static void arwVideoPushReleaseCallbackStub(IntPtr userData)
    {
        GCHandle gch = GCHandle.FromIntPtr(userData);
        arwVideoPushReleaseCallbackStubData data = (arwVideoPushReleaseCallbackStubData)gch.Target;
        data.releaseCallback.Invoke(data.releaseCallbackUserdata);
        gch.Free();
    }

    override public int arwVideoPush(int videoSourceIndex,
                NativeArray<byte> buf0, int buf0PixelStride, int buf0RowStride,
                NativeArray<byte>? buf1 = null, int buf1PixelStride = 0, int buf1RowStride = 0,
                NativeArray<byte>? buf2 = null, int buf2PixelStride = 0, int buf2RowStride = 0,
                NativeArray<byte>? buf3 = null, int buf3PixelStride = 0, int buf3RowStride = 0,
                PluginFunctionsVideoPushReleaseCallback releaseCallback = null, object releaseCallbackUserdata = null)
    {
#if ARX_ALLOW_UNITY_VIDEO_PROVIDERS
        ARX_pinvoke.arwVideoPushReleaseCallback callback = null;
        IntPtr callbackUserdata = IntPtr.Zero;
        if (releaseCallback != null)
        {
            callback = arwVideoPushReleaseCallbackStub;
            callbackUserdata = GCHandle.ToIntPtr(GCHandle.Alloc(new arwVideoPushReleaseCallbackStubData { releaseCallback = releaseCallback, releaseCallbackUserdata = releaseCallbackUserdata }));
        }
        return ARX_pinvoke.arwVideoPush(videoSourceIndex,
                                        buf0.GetIntPtr(), buf0.Length, buf0PixelStride, buf0RowStride,
                                        buf1.HasValue ? buf1.Value.GetIntPtr() : IntPtr.Zero, buf1.HasValue ? buf1.Value.Length : 0, buf1PixelStride, buf1RowStride,
                                        buf2.HasValue ? buf2.Value.GetIntPtr() : IntPtr.Zero, buf2.HasValue ? buf2.Value.Length : 0, buf2PixelStride, buf2RowStride,
                                        buf3.HasValue ? buf3.Value.GetIntPtr() : IntPtr.Zero, buf3.HasValue ? buf3.Value.Length : 0, buf3PixelStride, buf3RowStride,
                                        callback, callbackUserdata);
#else
        return -1;
#endif
    }

    override public int arwVideoPushFinal(int videoSourceIndex)
    {
        return ARX_pinvoke.arwVideoPushFinal(videoSourceIndex);
    }

}
