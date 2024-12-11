/*
 *  ARXUnityVideoSourceWebCamTexture.cs
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
 *  Copyright 2024-2024 Eden Networks Ltd.
 *
 *  Author(s): Philip Lamb.
 *
 */

using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Defines an interface via which Unity video sources can be integrated into
/// both the artoolkitX tracking and the video background rendering pipelines.
/// </summary>
public class ARXUnityVideoSourceWebCamTexture : IARXUnityVideoSource
{
    private const string LogTag = "ARXUnityVideoSourceWebCamTexture: ";

    private ARXController _arController = null;
    private WebCamTexture _webcamTexture = null;
	private struct ARWebCamTextureImage
    {
    	public Color32[] pixels;
    	public bool checkedOut;
    }
    private uint[] pixelsSwapRowTemp;
	private ARWebCamTextureImage[] _webcamTexureImages = null;
    private bool _videoPushInited = false;

    public int CameraIndex { get; } = 0;
    public ARXVideoConfig.AR_VIDEO_POSITION CameraPosition { get; } = ARXVideoConfig.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN;

    public bool OnVideoStart(ARXController arController)
    {
        if (_arController)
        {
            ARXController.LogError($"{LogTag}Null ARXController.");
            return false;
        }
        _arController = arController;
        _videoPushInited = false;
        _webcamTexture = new WebCamTexture();
        _webcamTexture.Play();
        return true;
    }

    public bool WillCallUpdateAR()
    {
        return false;
    }

    public bool OnVideoUpdate()
    {
        if (!_arController || !_webcamTexture)
        {
            ARXController.LogError($"{LogTag}Null ARXController or WebCamTexture.");
            return false;
        }

        int w = _webcamTexture.width;
        int h = _webcamTexture.height;

        if (!_videoPushInited)
        {
            if (!_webcamTexture.isPlaying || (w == 16 && h == 16)) // 16x16 is the "not ready" size, and remains until the camera is ready.
            {
                return true;
            }

            UnityEngine.Experimental.Rendering.GraphicsFormat format = _webcamTexture.graphicsFormat;

            // Allocate buffers for incoming images. Need at least 2.
            _webcamTexureImages = new ARWebCamTextureImage[2];
            for (int i = 0; i < _webcamTexureImages.Length; i++)
            {
                _webcamTexureImages[i].pixels = new Color32[w * h];
                _webcamTexureImages[i].checkedOut = false;
            }
            pixelsSwapRowTemp = new uint[w];

            // Display some information about the camera image
            ARXController.LogInfo($"{LogTag}Image info: size:{w}x{h}, format:{format}.");

            string pixelFormatString = "RGBA";
            if (_arController.PluginFunctions.arwVideoPushInit(0, w, h, pixelFormatString, CameraIndex, (int)CameraPosition) != 0)
            {
                ARXController.LogError($"{LogTag}Error: arwVideoPushInit");
                return false;
            }

            _videoPushInited = true;
        }

        if (!_webcamTexture.didUpdateThisFrame)
        {
            return true;
        }

        // Find a free buffer.
        int webcamTextureImageIndex = -1;
        for (int i = 0; i < _webcamTexureImages.Length; i++)
        {
            if (!_webcamTexureImages[i].checkedOut)
            {
                webcamTextureImageIndex = i;
                break;
            }
        }
        if (webcamTextureImageIndex == -1)
        {
            ARXController.LogWarning($"{LogTag}Out of webcam image buffers.");
            return false;
        }

        Color32[] pixels = _webcamTexureImages[webcamTextureImageIndex].pixels;
        _webcamTexture.GetPixels32(pixels);

        // Unity WebCamTexture starts at bottom, but ARX's starts at top, so manually flip V.
        Span<uint> pixelsSpan = MemoryMarshal.Cast<Color32, uint>(pixels);
        Span<uint> pixelsRowTempSpan = pixelsSwapRowTemp;
        for (int i = 0, j = h - 1; i < h / 2; i++, j--)
        {
            pixelsSpan.Slice(i*w, w).CopyTo(pixelsRowTempSpan);
            pixelsSpan.Slice(j*w, w).CopyTo(pixelsSpan.Slice(i*w, w));
            pixelsRowTempSpan.CopyTo(pixelsSpan.Slice(j*w, w));
        }

        _webcamTexureImages[webcamTextureImageIndex].checkedOut = true;
        if (_arController.PluginFunctions.arwVideoPush(0, pixels, w,
            // Release callback: unbox the index of the buffer and mark it as no longer checked-out.
            (object o) => {
                int webcamTextureImageIndex = (int)o;
                _webcamTexureImages[webcamTextureImageIndex].checkedOut = false;
            },
            webcamTextureImageIndex // Pass the boxed index of the buffer as userData to the release callback.
        ) < 0)
        {
            ARXController.LogError($"{LogTag}Error arwVideoPush");
            _webcamTexureImages[webcamTextureImageIndex].checkedOut = false;
            return false;
        }
        return true;
    }

    public Texture GetTexture()
    {
        return _webcamTexture;
    }

    public void OnVideoStop()
    {
        if (!_webcamTexture || !_webcamTexture.isPlaying)
        {
            return;
        }
        _webcamTexture.Stop();
        _webcamTexture = null;
        if (_videoPushInited)
        {
            if (_arController.PluginFunctions.arwVideoPushFinal(0) < 0)
            {
                ARXController.LogError($"{LogTag}Error arwVideoPushFinal");
            }
            _webcamTexureImages = null;
            _videoPushInited = false;
        }
        _arController = null;
    }
}