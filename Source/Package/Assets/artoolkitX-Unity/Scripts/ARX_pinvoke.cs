/*
 *  ARX_pinvoke.cs
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
 *  Author(s): Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Defines the external API of the ARX shared library that may be accessed via C# P/Invoke.
/// This API is not invoked directly from user scripts, but instead via an instance of PluginFunctionsARX.
/// </summary>
public static class ARX_pinvoke
{
	// The name of the external library containing the native functions
	private const string LIBRARY_NAME = "ARX";

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwRegisterLogCallback(PluginFunctionsLogCallback callback);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetLogLevel(int logLevel);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwInitialiseAR();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetARToolKitVersion([MarshalAs(UnmanagedType.LPStr)]StringBuilder buffer, int length);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern int arwGetError();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwShutdownAR();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwIsRunning();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwStopRunning();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool arwGetProjectionMatrix(float nearPlane, float farPlane, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrix);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
 #endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrixL, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrixR);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetVideoParams(out int width, out int height, out int pixelSize, [MarshalAs(UnmanagedType.LPStr)]StringBuilder pixelFormatBuffer, int pixelFormatBufferLen);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, [MarshalAs(UnmanagedType.LPStr)]StringBuilder pixelFormatBufferL, int pixelFormatBufferLenL, out int widthR, out int heightR, out int pixelSizeR, [MarshalAs(UnmanagedType.LPStr)]StringBuilder pixelFormatBufferR, int pixelFormatBufferLenR);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwCapture();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwUpdateAR();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	//public static extern bool arwUpdateTexture32([In, Out]Color32[] colors32);
	public static extern bool arwUpdateTexture32(IntPtr colors32);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	//public static extern bool arwUpdateTexture32Stereo([In, Out]Color32[] colors32L, [In, Out]Color32[] colors32R);
	public static extern bool arwUpdateTexture32Stereo(IntPtr colors32L, IntPtr colors32R);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern int arwGetTrackablePatternCount(int trackableId);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetTrackablePatternConfig(int trackableId, int patternID, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetTrackablePatternImage(int trackableId, int patternID, [In, Out]Color32[] colors32);


	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetTrackableOptionBool(int trackableId, int option);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackableOptionBool(int trackableId, int option, bool value);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern int arwGetTrackableOptionInt(int trackableId, int option);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackableOptionInt(int trackableId, int option, int value);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern float arwGetTrackableOptionFloat(int trackableId, int option);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackableOptionFloat(int trackableId, int option, float value);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackerOptionBool(int option, bool debug);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetTrackerOptionBool(int option);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackerOptionFloat(int option, float mode);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern float arwGetTrackerOptionFloat(int option);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern void arwSetTrackerOptionInt(int option, int mode);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern int arwGetTrackerOptionInt(int option);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	public static extern int arwAddTrackable(string cfg);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwRemoveTrackable(int trackableId);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	public static extern int arwRemoveAllTrackables();
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwQueryTrackableVisibilityAndTransformation(int trackableId, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrix);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwQueryTrackableVisibilityAndTransformationStereo(int trackableId, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrixL, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] matrixR);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float nearPlane, float farPlane, out float fovy_p, out float aspect_p, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] m, [Out][MarshalAs(UnmanagedType.LPArray, SizeConst=16)] float[] p);
	
    // iOS-only utility function to request camera permissions.
#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void aruRequestCamera();
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	public static extern int arwCreateVideoSourceInfoList(string config);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
#endif
	[return: MarshalAsAttribute(UnmanagedType.I1)]
	public static extern bool arwGetVideoSourceInfoListEntry(int index, [MarshalAs(UnmanagedType.LPStr)] StringBuilder nameBuf, int nameBufLen, [MarshalAs(UnmanagedType.LPStr)] StringBuilder modelBuf, int modelBufLen, [MarshalAs(UnmanagedType.LPStr)] StringBuilder UIDBuf, int UIDBufLen, out int flags_p, [MarshalAs(UnmanagedType.LPStr)] StringBuilder openTokenBuf, int openTokenBufLen);

#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
#endif
	public static extern void arwDeleteVideoSourceInfoList();
}

