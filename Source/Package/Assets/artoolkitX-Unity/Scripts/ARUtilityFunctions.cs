/*
 *  ARUtilityFunctions.cs
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
 *  Author(s): Julian Looser, Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class ARUtilityFunctions
{

	/// <summary>
	/// Returns the named camera or null if not found.
	/// </summary>
	/// <param name="name">Camera name to search for.</param>
	/// <returns>The named <see cref="Camera"/> or null if not found.</returns>
	public static Camera FindCameraByName(string name)
	{
	    foreach (Camera c in Camera.allCameras)
	    {
	        if (c.gameObject.name == name) return c;
	    }

	    return null;
	}


	/// <summary>
	/// Creates a Unity matrix from an array of floats.
	/// </summary>
	/// <param name="values">Array of 16 floats to populate the matrix.</param>
	/// <returns>A new <see cref="Matrix4x4"/> with the given values.</returns>
	public static Matrix4x4 MatrixFromFloatArray(float[] values)
	{
	    if (values == null || values.Length < 16) throw new ArgumentException("Expected 16 elements in values array", "values");

	    Matrix4x4 mat = new Matrix4x4();
	    for (int i = 0; i < 16; i++) mat[i] = values[i];
	    return mat;
	}

#if false
	// Posted on: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
	    // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
	    Quaternion q = new Quaternion();
	    q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
	    q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
	    q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
	    q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
	    q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
	    q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
	    q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
	    return q;
	}
#else
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		// Trap the case where the matrix passed in has an invalid rotation submatrix.
		if (m.GetColumn(2) == Vector4.zero) {
			ARController.Log("QuaternionFromMatrix got zero matrix.");
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}
#endif

	public static Vector3 PositionFromMatrix(Matrix4x4 m)
	{
	    return m.GetColumn(3);
	}

	// Convert from right-hand coordinate system with <normal vector> in direction of +x,
	// <orthorgonal vector> in direction of +y, and <approach vector> in direction of +z,
	// to Unity's left-hand coordinate system with <normal vector> in direction of +x,
	// <orthorgonal vector> in direction of +y, and <approach vector> in direction of +z.
	// This is equivalent to negating row 2, and then negating column 2.
	public static Matrix4x4 LHMatrixFromRHMatrix(Matrix4x4 rhm)
	{
		Matrix4x4 lhm = new Matrix4x4();;

		// Column 0.
		lhm[0, 0] =  rhm[0, 0];
		lhm[1, 0] =  rhm[1, 0];
		lhm[2, 0] = -rhm[2, 0];
		lhm[3, 0] =  rhm[3, 0];
		
		// Column 1.
		lhm[0, 1] =  rhm[0, 1];
		lhm[1, 1] =  rhm[1, 1];
		lhm[2, 1] = -rhm[2, 1];
		lhm[3, 1] =  rhm[3, 1];
		
		// Column 2.
		lhm[0, 2] = -rhm[0, 2];
		lhm[1, 2] = -rhm[1, 2];
		lhm[2, 2] =  rhm[2, 2];
		lhm[3, 2] = -rhm[3, 2];
		
		// Column 3.
		lhm[0, 3] =  rhm[0, 3];
		lhm[1, 3] =  rhm[1, 3];
		lhm[2, 3] = -rhm[2, 3];
		lhm[3, 3] =  rhm[3, 3];

		return lhm;
	}

	public static IntPtr GetIntPtr<T>(this NativeArray<T> array) where T : struct
	{
		unsafe
        {
			return new IntPtr(NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(array));
		}
	}

	// Creates a GameObject in layer 'layer' which renders a mesh displaying the video stream.
	// Places references to the Color array (as required), the texture and the material into the out parameters.
	public static GameObject CreateVideoGameObject(int index, int w, int h, bool flipH, bool flipV, int layer, out Texture2D vt, out Material vm)
	{
		// Check parameters.
		if (w <= 0 || h <= 0)
		{
			ARController.Log("Error: CreateVideoGameObject cannot configure video texture with invalid video size: " + w + "x" + h);
			vt = null; vm = null;
			return null;
		}

		// Create new GameObject to hold mesh.
		GameObject vbmgo = new GameObject("Video source " + index);
		if (vbmgo == null)
		{
			ARController.Log("Error: CreateVideoGameObject cannot create GameObject.");
			vt = null; vm = null;
			return null;
		}
		vbmgo.layer = layer; // Belongs in the background layer.

		// Create an video texture, an array that can be used to update it and a material to display it.
		vt = ARUtilityFunctions.CreateTexture(w, h, TextureFormat.ARGB32);
		Shader shaderSource = Shader.Find("VideoPlaneNoLight");
		vm = new Material(shaderSource); //arunityX.Properties.Resources.VideoPlaneShader;
		vm.hideFlags = HideFlags.HideAndDontSave;
		vm.mainTexture = vt;

		// Now create a mesh appropriate for displaying the video, a mesh filter to instantiate that mesh,
		// and a mesh renderer to render the material on the instantiated mesh.
		MeshFilter filter = vbmgo.AddComponent<MeshFilter>();
		filter.mesh = ARUtilityFunctions.CreateTextureMesh(1.0f, 1.0f, 2.0f, 2.0f, 0.5f, flipH, flipV);
		MeshRenderer meshRenderer = vbmgo.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		vbmgo.GetComponent<Renderer>().material = vm;

		return vbmgo;
	}


	public static TextureFormat GetTextureFormatFromARPixelFormat(string pixelFormat)
	{
		switch (pixelFormat)
		{
			case "AR_PIXEL_FORMAT_RGB": return TextureFormat.RGB24;
			case "AR_PIXEL_FORMAT_BGR": /* return TextureFormat.BGR24; */ break;
			case "AR_PIXEL_FORMAT_RGBA": return TextureFormat.RGBA32;
			case "AR_PIXEL_FORMAT_BGRA": return TextureFormat.BGRA32;
			case "AR_PIXEL_FORMAT_ABGR": /* return TextureFormat.ABGR32; */ break;
			case "AR_PIXEL_FORMAT_MONO": return TextureFormat.R8;
			case "AR_PIXEL_FORMAT_ARGB": return TextureFormat.ARGB32;
			case "AR_PIXEL_FORMAT_2vuy": break;
			case "AR_PIXEL_FORMAT_yuvs": return TextureFormat.YUY2;
			case "AR_PIXEL_FORMAT_RGB_565": return TextureFormat.RGB565;
			case "AR_PIXEL_FORMAT_RGBA_5551": /* return TextureFormat.RGBA5551; */ break;
			case "AR_PIXEL_FORMAT_RGBA_4444": return TextureFormat.RGBA4444;
			case "AR_PIXEL_FORMAT_420v": break;
			case "AR_PIXEL_FORMAT_420f": break;
			case "AR_PIXEL_FORMAT_NV21": break;
			default: break; // AR_PIXEL_FORMAT_INVALID
		}
		return (TextureFormat)0;
	}

	public static Texture2D CreateTexture(int width, int height, TextureFormat format)
	{
		// Check parameters.
		if (width <= 0 || height <= 0)
		{
			ARController.Log("Error: CreateTexture cannot configure video texture with invalid size: " + width + "x" + height);
			return null;
		}

		Texture2D vt = new Texture2D(width, height, format, false);
		vt.hideFlags = HideFlags.HideAndDontSave;
		vt.filterMode = FilterMode.Bilinear;
		vt.wrapMode = TextureWrapMode.Clamp;
		vt.anisoLevel = 0;

		// Initialise the video texture to black.
		Color32[] arr = new Color32[width * height];
		Color32 blackOpaque = new Color32(0, 0, 0, 255);
		for (int i = 0; i < arr.Length; i++) arr[i] = blackOpaque;
		vt.SetPixels32(arr);
		vt.Apply(); // Pushes all SetPixels*() ops to texture.
		arr = null;

		return vt;
	}

	public static Mesh CreateTextureMesh(float textureScaleU, float textureScaleV, float width, float height, float zPosition, bool flipX, bool flipY)
	{
		Mesh m = new Mesh();
		m.Clear();
		m.vertices = new Vector3[]
		{
			new Vector3(-width * 0.5f, 0.0f, zPosition),
			new Vector3(width * 0.5f, 0.0f, zPosition),
			new Vector3(width * 0.5f, height, zPosition),
			new Vector3(-width * 0.5f, height, zPosition),
		};
		m.normals = new Vector3[]
		{
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
		};
		float u1 = flipX ? textureScaleU : 0.0f;
		float u2 = flipX ? 0.0f : textureScaleU;
		float v1 = flipY ? textureScaleV : 0.0f;
		float v2 = flipY ? 0.0f : textureScaleV;
		m.uv = new Vector2[] {
			new Vector2(u1, v1),
			new Vector2(u2, v1),
			new Vector2(u2, v2),
			new Vector2(u1, v2)
		};
		m.triangles = new int[] {
			2, 1, 0,
			3, 2, 0
		};

		return m;
	}

}
