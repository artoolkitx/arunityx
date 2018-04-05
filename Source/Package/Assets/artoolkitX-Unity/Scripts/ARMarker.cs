/*
 *  ARMarker.cs
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
using System.IO;
using UnityEngine;

public enum MarkerType
{
    Square,      		// A square template (pattern) marker.
    SquareBarcode,      // A square matrix (2D barcode) marker.
    Multimarker,        // Multiple square markers treated as a single marker.
    NFT,                // A legacy NFT marker.
    TwoD                // An artoolkitX 2D textured trackable.
}

public enum ARWMarkerOption : int {
        ARW_MARKER_OPTION_FILTERED = 1,
        ARW_MARKER_OPTION_FILTER_SAMPLE_RATE = 2,
        ARW_MARKER_OPTION_FILTER_CUTOFF_FREQ = 3,
        ARW_MARKER_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION = 4,
		ARW_MARKER_OPTION_SQUARE_CONFIDENCE = 5,
		ARW_MARKER_OPTION_SQUARE_CONFIDENCE_CUTOFF = 6,
		ARW_MARKER_OPTION_NFT_SCALE = 7
}

/// <summary>
/// ARMarker objects represent a native ARTrackable, even when the native artoolkitX is not
/// initialised.
/// To find markers from elsewhere in the Unity environment:
///   ARMarker[] markers = FindObjectsOfType<ARMarker>(); // (or FindObjectsOfType(typeof(ARMarker)) as ARMarker[]);
/// 
/// </summary>
/// 
[ExecuteInEditMode]
public class ARMarker : MonoBehaviour
{

    public readonly static Dictionary<MarkerType, string> MarkerTypeNames = new Dictionary<MarkerType, string>
    {
		{MarkerType.Square, "Single AR pattern"},
		{MarkerType.SquareBarcode, "Single AR barcode"},
    	{MarkerType.Multimarker, "Multimarker AR configuration"},
		{MarkerType.NFT, "NFT dataset"},
        {MarkerType.TwoD, "2D image texture"}
    };

    private const string LogTag = "ARMarker: ";
    
    // Quaternion to rotate from ART to Unity
    //public static Quaternion RotationCorrection = Quaternion.AngleAxis(90.0f, new Vector3(1.0f, 0.0f, 0.0f));

    // Value used when no underlying native ARTrackable is assigned
    public const int NO_ID = -1;

    [NonSerialized]       // UID is not serialized because its value is only meaningful during a specific run.
    public int UID = NO_ID;      // Current Unique Identifier (UID) assigned to this marker.

    // Public members get serialized
    public MarkerType MarkerType = MarkerType.Square;
    public string Tag = "";

    // If the marker is single, then it has a filename and a width
	public int PatternFilenameIndex = 0;
    public string PatternFilename = "";
	public string PatternContents = ""; // Set by the editor.
    public float PatternWidth = 0.08f;
	
	// Barcode markers have a user-selected ID.
	public long BarcodeID = 0;
	
    // If the marker is multi, it just has a config filename
    public string MultiConfigFile = "";
	
	// NFT markers have a dataset pathname (less the extension).
	// Also, we need a list of the file extensions that make up an NFT dataset.
	public string NFTDataName = "";
	#if !UNITY_METRO
	private readonly string[] NFTDataExts = {"iset", "fset", "fset3"};
	#endif
	[NonSerialized]
	public float NFTWidth; // Once marker is loaded, this holds the width of the marker in Unity units.
	[NonSerialized]
	public float NFTHeight; // Once marker is loaded, this holds the height of the marker in Unity units.

    // 2D image trackables have an image filename and image width.
    public string TwoDImageFile = "";
    public float TwoDImageWidth = 1.0f;

    // Single markers have a single pattern, multi markers have one or more, NFT have none.
	private ARPattern[] patterns;

	// Private fields with accessors.
	// Marker configuration options.
	[SerializeField]
	private bool currentUseContPoseEstimation = false;						// Single marker only; whether continuous pose estimation should be used.
	[SerializeField]
	private bool currentFiltered = false;
	[SerializeField]
	private float currentFilterSampleRate = 30.0f;
	[SerializeField]
	private float currentFilterCutoffFreq = 15.0f;
	[SerializeField]
	private float currentNFTScale = 1.0f;									// NFT marker only; scale factor applied to marker size.

    // Realtime tracking information
    private bool visible = false;                                           // Marker is visible or not
	private Matrix4x4 transformationMatrix;                                 // Full transformation matrix as a Unity matrix
//    private Quaternion rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);   // Rotation corrected for Unity
//    private Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);               // Position corrected for Unity
    

    // Initialisation.
	// When Awake() is called, the object is already instantiated and deserialised.
	// This is the right place to connect to other objects.
	// However, objects are awoken in random order, so do not assume that any
	// other object is ready to be communicated with -- it might need to be awoken first.
    void Awake()
    {
		//ARController.Log(LogTag + "ARMarker.Awake()");
        UID = NO_ID;
    }
	
	public void OnEnable()
	{
		//ARController.Log(LogTag + "ARMarker.OnEnable()");
		Load();
	}
	
	public void OnDisable()
	{
		//ARController.Log(LogTag + "ARMarker.OnDisable()");
		Unload();
	}

	#if !UNITY_METRO
	private bool unpackStreamingAssetToCacheDir(string basename)
	{
		if (!File.Exists(System.IO.Path.Combine(Application.temporaryCachePath, basename))) {
			string file = System.IO.Path.Combine(Application.streamingAssetsPath, basename); // E.g. "jar:file://" + Application.dataPath + "!/assets/" + basename;
			WWW unpackerWWW = new WWW(file);
			while (!unpackerWWW.isDone) { } // This will block in the webplayer. TODO: switch to co-routine.
			if (!string.IsNullOrEmpty(unpackerWWW.error)) {
				ARController.Log(LogTag + "Error unpacking '" + file + "'");
				return (false);
			}
			File.WriteAllBytes(System.IO.Path.Combine(Application.temporaryCachePath, basename), unpackerWWW.bytes); // 64MB limit on File.WriteAllBytes.
		}
		return (true);
	}
	#endif

	// Load the native ARTrackable structure(s) and set the UID.
    public void Load() 
    {
		if (this.enabled == false) {
			return;
		}

		//ARController.Log(LogTag + "ARMarker.Load()");
        if (UID != NO_ID) {
			return;
		}

		if (!PluginFunctions.inited) {
            // If arwInitialiseAR() has not yet been called, we can't load the native trackable yet.
            // ARController.InitialiseAR() will call this again when arwInitialiseAR() has been called.
			return;
		}

		// Work out the configuration string to pass to the native side.
        string dir = Application.streamingAssetsPath;
        string cfg = "";
		
        switch (MarkerType) {

			case MarkerType.Square:
				// Multiply width by 1000 to convert from metres to artoolkitX's millimetres.
				cfg = "single_buffer;" + PatternWidth*1000.0f + ";buffer=" + PatternContents;
				break;
			
			case MarkerType.SquareBarcode:
				// Multiply width by 1000 to convert from metres to artoolkitX's millimetres.
				cfg = "single_barcode;" + BarcodeID + ";" + PatternWidth*1000.0f;
				break;
			
            case MarkerType.Multimarker:
				#if !UNITY_METRO
				if (dir.Contains("://")) {
					// On Android, we need to unpack the StreamingAssets from the .jar file in which
					// they're archived into the native file system.
					dir = Application.temporaryCachePath;
					if (!unpackStreamingAssetToCacheDir(MultiConfigFile)) {
						dir = "";
					} else {
					
						//string[] unpackFiles = getPatternFiles;
						//foreach (string patternFile in patternFiles) {
						//if (!unpackStreamingAssetToCacheDir(patternFile)) {
						//    dir = "";
						//    break;
						//}
				    }
				}
				#endif
				
				if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(MultiConfigFile)) {
					cfg = "multi;" + System.IO.Path.Combine(dir, MultiConfigFile);
				}
                break;

			
			case MarkerType.NFT:
				#if !UNITY_METRO
				if (dir.Contains("://")) {
					// On Android, we need to unpack the StreamingAssets from the .jar file in which
					// they're archived into the native file system.
					dir = Application.temporaryCachePath;
					foreach (string ext in NFTDataExts) {
						string basename = NFTDataName + "." + ext;
						if (!unpackStreamingAssetToCacheDir(basename)) {
							dir = "";
							break;
						}
					}
				}
				#endif
			
				if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(NFTDataName)) {
					cfg = "nft;" + System.IO.Path.Combine(dir, NFTDataName);
				}
				break;

        case MarkerType.TwoD:
            #if !UNITY_METRO
            if (dir.Contains("://")) {
                // On Android, we need to unpack the StreamingAssets from the .jar file in which
                // they're archived into the native file system.
                dir = Application.temporaryCachePath;
                if (!unpackStreamingAssetToCacheDir(TwoDImageFile)) {
                    dir = "";
                }
            }
            #endif

            if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(TwoDImageFile)) {
                cfg = "2d;" + System.IO.Path.Combine(dir, TwoDImageFile) + ";" + TwoDImageWidth;
            }
            break;

            default:
                // Unknown marker type?
                break;

        }
		
		// If a valid config. could be assembled, get the native side to process it, and assign the resulting ARMarker UID.
		if (!string.IsNullOrEmpty(cfg)) {
        	UID = PluginFunctions.arwAddMarker(cfg);
			if (UID == NO_ID) {
				ARController.Log(LogTag + "Error loading marker.");
			} else {

				// Marker loaded. Do any additional configuration.
				//ARController.Log("Added marker with cfg='" + cfg + "'");
				
				if (MarkerType == MarkerType.Square || MarkerType == MarkerType.SquareBarcode) UseContPoseEstimation = currentUseContPoseEstimation;
				Filtered = currentFiltered;
				FilterSampleRate = currentFilterSampleRate;
				FilterCutoffFreq = currentFilterCutoffFreq;

				// Retrieve any required information from the configured native ARTrackable.
                if (MarkerType == MarkerType.NFT || MarkerType == MarkerType.TwoD) {
                    if (MarkerType == MarkerType.NFT) NFTScale = currentNFTScale;

					int imageSizeX, imageSizeY;
					PluginFunctions.arwGetMarkerPatternConfig(UID, 0, null, out NFTWidth, out NFTHeight, out imageSizeX, out imageSizeY);
					NFTWidth *= 0.001f;
					NFTHeight *= 0.001f;
					//ARController.Log("Got NFTWidth=" + NFTWidth + ", NFTHeight=" + NFTHeight + ".");
				
				} else {

       				// Create array of patterns. A single marker will have array length 1.
					int numPatterns = PluginFunctions.arwGetMarkerPatternCount(UID);
       				//ARController.Log("Marker with UID=" + UID + " has " + numPatterns + " patterns.");
        			if (numPatterns > 0) {
						patterns = new ARPattern[numPatterns];
			        	for (int i = 0; i < numPatterns; i++) {
            				patterns[i] = new ARPattern(UID, i);
        				}
					}

				}
			}
		}
    }

	// We use Update() here, but be aware that unless ARController has been configured to
	// execute first (Unity Editor->Edit->Project Settings->Script Execution Order) then
	// state produced by this update may lag by one frame.
    void Update()
    {
		float[] matrixRawArray = new float[16];

		//ARController.Log(LogTag + "ARMarker.Update()");
		if (UID == NO_ID || !PluginFunctions.inited) {
            visible = false;
            return;
        }

		// Query visibility if we are running in the Player.
        if (Application.isPlaying) {

			visible = PluginFunctions.arwQueryMarkerVisibilityAndTransformation(UID, matrixRawArray);
			//ARController.Log(LogTag + "ARMarker.Update() UID=" + UID + ", visible=" + visible);
			
            if (visible) {
				matrixRawArray[12] *= 0.001f; // Scale the position from artoolkitX units (mm) into Unity units (m).
				matrixRawArray[13] *= 0.001f;
				matrixRawArray[14] *= 0.001f;

				Matrix4x4 matrixRaw = ARUtilityFunctions.MatrixFromFloatArray(matrixRawArray);
				//ARController.Log("arwQueryMarkerTransformation(" + UID + ") got matrix: [" + Environment.NewLine + matrixRaw.ToString("F3").Trim() + "]");

				// artoolkitX uses right-hand coordinate system where the marker lies in x-y plane with right in direction of +x,
				// up in direction of +y, and forward (towards viewer) in direction of +z.
				// Need to convert to Unity's left-hand coordinate system where marker lies in x-y plane with right in direction of +x,
				// up in direction of +y, and forward (towards viewer) in direction of -z.
				transformationMatrix = ARUtilityFunctions.LHMatrixFromRHMatrix(matrixRaw);
			}
		}
    }
	
	// Unload any native ARTrackable structures, and clear the UID.
	public void Unload()
	{
		//ARController.Log(LogTag + "ARMarker.Unload()");
		
		if (UID == NO_ID) {
			return;
		}
		
        // Remove the native trackable, unless arwShutdownAR() has already been called (as it will already have been removed.)
        if (PluginFunctions.inited) {
        	PluginFunctions.arwRemoveMarker(UID);
		}

		UID = NO_ID;

		patterns = null; // Delete the patterns too.
	}
	
    public Matrix4x4 TransformationMatrix
    {
        get
        {                
            return transformationMatrix;
        }
    }

//    public Vector3 Position
//    {
//        get
//        {
//            return position;
//        }
//    }
//
//    public Quaternion Rotation
//    {
//        get
//        {
//            return rotation;
//        }
//    }

    public bool Visible
    {
        get
        {
            return visible;
        }
    }


    public ARPattern[] Patterns
    {
        get
        {
            return patterns;
        }
    }

    public bool Filtered
    {
        get
        {
			return currentFiltered; // Serialised.
        }

        set
        {
			currentFiltered = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionBool(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTERED, value);
			}
        }
    }

    public float FilterSampleRate
    {
        get
        {
			return currentFilterSampleRate; // Serialised.
        }

        set
        {
			currentFilterSampleRate = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTER_SAMPLE_RATE, value);
			}
        }
    }

	public float FilterCutoffFreq
    {
        get
        {
			return currentFilterCutoffFreq; // Serialised.
        }

        set
        {
			currentFilterCutoffFreq = value;
			if (UID != NO_ID) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_FILTER_CUTOFF_FREQ, value);
			}
        }
    }

	public bool UseContPoseEstimation
    {
        get
        {
			return currentUseContPoseEstimation; // Serialised.
        }

        set
        {
			currentUseContPoseEstimation = value;
			if (UID != NO_ID && (MarkerType == MarkerType.Square || MarkerType == MarkerType.SquareBarcode)) {
				PluginFunctions.arwSetMarkerOptionBool(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION, value);
			}
        }
    }

	public float NFTScale
	{
		get
		{
			return currentNFTScale; // Serialised.
		}
		
		set
		{
			currentNFTScale = value;
			if (UID != NO_ID && (MarkerType == MarkerType.NFT)) {
				PluginFunctions.arwSetMarkerOptionFloat(UID, (int)ARWMarkerOption.ARW_MARKER_OPTION_NFT_SCALE, value);
			}
		}
	}
	

}
