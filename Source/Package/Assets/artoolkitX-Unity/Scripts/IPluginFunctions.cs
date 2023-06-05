/*
 *  IPluginFunctions.cs
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
 *
 *  Author(s): Philip Lamb, Julian Looser, Dan Bell, Thorsten Bux.
 *
 */

using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;

// Delegate type declaration for log callback.
public delegate void PluginFunctionsLogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);

public delegate void PluginFunctionsTrackableEventCallback(int trackableEventType, int trackableUID);

/// <summary>
/// Defines a plugin interface via which artoolkitX functionality can be invoked.
/// Concrete subclasses might e.g. implement this via a local DLL, or might instead
/// invoke a remote instance of artoolkitX over a network.
/// </summary>
public abstract class IPluginFunctions
{
    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct ARWTrackableStatus
    {
        int uid;
        bool visible;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] matrix;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] matrixR;
    }

    abstract public bool IsConfigured();
    abstract public bool Configure(string config);
    abstract public bool IsInited();

    abstract public int arwAddTrackable(string cfg);
    abstract public bool arwCapture();
    abstract public string arwGetARToolKitVersion();
    abstract public float arwGetBorderSize();
    abstract public int arwGetError();
    abstract public int arwGetImageProcMode();
    abstract public int arwGetLabelingMode();
    abstract public bool arwGetTrackableOptionBool(int markerID, int option);
    abstract public float arwGetTrackableOptionFloat(int markerID, int option);
    abstract public int arwGetTrackableOptionInt(int markerID, int option);
    abstract public bool arwGetTrackablePatternConfig(int markerID, int patternID, float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY);
    abstract public int arwGetTrackablePatternCount(int markerID);
    abstract public bool arwGetTrackablePatternImage(int markerID, int patternID, [In, Out] Color32[] colors32);
    abstract public int arwGetMatrixCodeType();
    abstract public bool arwGetNFTMultiMode();
    abstract public void arwSet2DMaxMarkersToTrack(int maxMarkersToTrack);
    abstract public int arwGet2DMaxMarkersToTrack();
    abstract public bool arwGet2DThreaded();
    abstract public void arwSet2DThreaded(bool threaded);
    abstract public int arwGetPatternDetectionMode();
    abstract public bool arwGetProjectionMatrix(float nearPlane, float farPlane, float[] matrix);
    abstract public bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, float[] matrixL, float[] matrixR);
    abstract public bool arwGetVideoDebugMode();
    abstract public bool arwGetVideoParams(out int width, out int height, out int pixelSize, out string pixelFormatString);
    abstract public bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, out string pixelFormatL, out int widthR, out int heightR, out int pixelSizeR, out string pixelFormatR);
    abstract public int arwGetVideoThreshold();
    abstract public int arwGetVideoThresholdMode();
    abstract public bool arwInitialiseAR(int pattSize = 16, int pattCountMax = 25);
    abstract public bool arwIsRunning();
    abstract public bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float projectionNearPlane, float projectionFarPlane, out float fovy_p, out float aspect_p, float[] m, float[] p);
    abstract public bool arwQueryTrackableVisibilityAndTransformation(int markerID, float[] matrix);
    abstract public bool arwQueryTrackableVisibilityAndTransformationStereo(int markerID, float[] matrixL, float[] matrixR);
    abstract public void arwRegisterLogCallback(PluginFunctionsLogCallback lcb);
    abstract public int arwRemoveAllTrackables();
    abstract public bool arwRemoveTrackable(int markerID);
    abstract public void arwSetBorderSize(float size);
    abstract public void arwSetImageProcMode(int mode);
    abstract public void arwSetLabelingMode(int mode);
    abstract public void arwSetLogLevel(int logLevel);
    abstract public void arwSetTrackableOptionBool(int markerID, int option, bool value);
    abstract public void arwSetTrackableOptionFloat(int markerID, int option, float value);
    abstract public void arwSetTrackableOptionInt(int markerID, int option, int value);
    abstract public void arwSetMatrixCodeType(int type);
    abstract public void arwSetNFTMultiMode(bool on);
    abstract public void arwSetPatternCountMax(int count);
    abstract public void arwSetPatternDetectionMode(int mode);
    abstract public void arwSetPatternSize(int size);
    abstract public void arwSetVideoDebugMode(bool debug);
    abstract public void arwSetVideoThreshold(int threshold);
    abstract public void arwSetVideoThresholdMode(int mode);
    abstract public bool arwShutdownAR();
    abstract public bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen);
    abstract public bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen);
    abstract public bool arwStopRunning();
    abstract public bool arwUpdateAR();
    abstract public bool arwUpdateTexture32([In, Out] Color32[] colors32);
    abstract public bool arwUpdateTexture32Stereo([In, Out] Color32[] colors32L, [In, Out] Color32[] colors32R);
    abstract public int arwCreateVideoSourceInfoList(string config);
    abstract public bool arwGetVideoSourceInfoListEntry(int index, out string name, out string model, out string UID, out int flags, out string openToken);
    abstract public void arwDeleteVideoSourceInfoList();
    abstract public void arwSetSquareMatrixModeAutocreateNewTrackables(bool on, float defaultWidth = 0.08f, PluginFunctionsTrackableEventCallback tecb = null);
    abstract public bool arwGetSquareMatrixModeAutocreateNewTrackables(out bool on, out float defaultWidth, out PluginFunctionsTrackableEventCallback tecb);
    abstract public int arwVideoPushInit(int videoSourceIndex, int width, int height, string pixelFormat, int cameraIndex, int cameraPosition);
    abstract public int arwVideoPush(int videoSourceIndex,
                NativeArray<byte> buf0, int buf0PixelStride, int buf0RowStride,
                NativeArray<byte>? buf1 = null, int buf1PixelStride = 0, int buf1RowStride = 0,
                NativeArray<byte>? buf2 = null, int buf2PixelStride = 0, int buf2RowStride = 0,
                NativeArray<byte>? buf3 = null, int buf3PixelStride = 0, int buf3RowStride = 0);
    abstract public int arwVideoPushFinal(int videoSourceIndex);
}