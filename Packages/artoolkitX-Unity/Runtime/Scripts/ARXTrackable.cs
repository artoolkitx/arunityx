/*
 *  ARXTrackable.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ARXTrackable objects represent a native ARXTrackable, even when the native artoolkitX is not
/// initialised.
/// To find markers from elsewhere in the Unity environment:
///   ARXTrackable[] markers = FindObjectsOfType<ARXTrackable>(); // (or FindObjectsOfType(typeof(ARXTrackable)) as ARXTrackable[]);
///
/// Script execution order is set to -100 on this component, to ensure that the updated pose information is available
/// to scene game objects during their Update(), particularly game objects with ARXCamera or ARTrackedOject components attached.
/// </summary>
///
[ExecuteInEditMode]
[DefaultExecutionOrder(-100)]
public class ARXTrackable : MonoBehaviour
{
    public enum TrackableType
    {
        Unknown = -1,       ///< Type not known, e.g. autocreated trackable.
        Square = 0,         ///< A square template (pattern) marker.
        SquareBarcode = 1,  ///< A square matrix (2D barcode) marker.
        Multimarker = 2,    ///< Multiple square markers treated as a single marker.
        NFT = 3,            ///< A legacy NFT marker.
        TwoD = 4,           ///< An artoolkitX 2D textured trackable.
    }

    public enum ARWTrackableOption : int
    {
        ARW_TRACKABLE_OPTION_TYPE = 0,                             ///< readonly int enum, trackable type as per ARW_TRACKABLE_TYPE_* enum .
        ARW_TRACKABLE_OPTION_FILTERED = 1,                         ///< bool, true for filtering enabled.
        ARW_TRACKABLE_OPTION_FILTER_SAMPLE_RATE = 2,               ///< float, sample rate for filter calculations.
        ARW_TRACKABLE_OPTION_FILTER_CUTOFF_FREQ = 3,               ///< float, cutoff frequency of filter.
        ARW_TRACKABLE_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION = 4,  ///< bool, true to use continuous pose estimate.
        ARW_TRACKABLE_OPTION_SQUARE_CONFIDENCE = 5,                ///< float, confidence value of most recent marker match
        ARW_TRACKABLE_OPTION_SQUARE_CONFIDENCE_CUTOFF = 6,         ///< float, minimum allowable confidence value used in marker matching.
        ARW_TRACKABLE_OPTION_NFT_SCALE = 7,                        ///< float, scale factor applied to NFT marker size.
        ARW_TRACKABLE_OPTION_MULTI_MIN_SUBMARKERS = 8,             ///< int, minimum number of submarkers for tracking to be valid.
        ARW_TRACKABLE_OPTION_MULTI_MIN_CONF_MATRIX = 9,            ///< float, minimum confidence value for submarker matrix tracking to be valid.
        ARW_TRACKABLE_OPTION_MULTI_MIN_CONF_PATTERN = 10,          ///< float, minimum confidence value for submarker pattern tracking to be valid.
        ARW_TRACKABLE_OPTION_MULTI_MIN_INLIER_PROB = 11,           ///< float, minimum inlier probability value for robust multimarker pose estimation (range 1.0 - 0.0).
        ARW_TRACKABLE_OPTION_SQUARE_WIDTH = 12,                    ///< float, square marker width
        ARW_TRACKABLE_OPTION_2D_SCALE = 13,                        ///< float, 2D trackable scale (i.e. width).
        ARW_TRACKABLE_OPTION_SQUARE_BARCODE_ID = 14,               ///< readonly string, for square matrix (barcode) markers, the uint64_t barcodeID converted to a string.
    }

    public readonly static Dictionary<TrackableType, string> TrackableTypeNames = new Dictionary<TrackableType, string>
    {
        {TrackableType.Square, "Single AR pattern"},
        {TrackableType.SquareBarcode, "Single AR barcode"},
        {TrackableType.Multimarker, "Multimarker AR configuration"},
        {TrackableType.NFT, "NFT dataset"},
        {TrackableType.TwoD, "2D image texture"},
        {TrackableType.Unknown, "Unknown"}
    };

    public enum ARW_TRACKABLE_EVENT_TYPE
    {
        ARW_TRACKABLE_EVENT_TYPE_NONE = 0,
        ARW_TRACKABLE_EVENT_TYPE_AUTOCREATED = 1,
        ARW_TRACKABLE_EVENT_TYPE_AUTOREMOVED = 2,
    };

    private const string LogTag = "ARXTrackable: ";

    // Quaternion to rotate from ART to Unity
    //public static Quaternion RotationCorrection = Quaternion.AngleAxis(90.0f, new Vector3(1.0f, 0.0f, 0.0f));

    // Value used when no underlying native ARXTrackable is assigned
    public const int NO_ID = -1;

    [NonSerialized] private int uid = NO_ID;
    // Current Unique Identifier (UID) assigned to this marker.
    // UID is not serialized because its value is only meaningful during a specific run.
    public int UID
    {
        get
        {
            return uid;
        }
    }
    [field: SerializeField]
    public TrackableType Type { get; private set; } = TrackableType.TwoD;

    [Tooltip("If an ARXTrackedObject has this tag, it will use the pose from this ARXTrackable.")]
    public string Tag = ""; // This links this trackable with an ARXTrackedObject or ARXTrackedCamera in the scene.

    // 2D image trackables have an image filename and image width.
    [field: SerializeField]
    public string TwoDImageFile { get; private set; } = "";
    [field: SerializeField]
    public float TwoDImageWidth { get; private set; } = 1.0f;
    public void ConfigureAsTwoD(string imageFilePath, float imageWidth)
    {
        Unload();
        Type = TrackableType.TwoD;
        if (!string.IsNullOrEmpty(imageFilePath)) TwoDImageFile = imageFilePath;
        if (imageWidth > 0.0f) TwoDImageWidth = imageWidth;
        _loadError = false;
        Load();
    }

    // If the marker is single square pattern, then it has a filename and pattern.
    // Once configured, the filename is just for display purposes.
    // Alternately, barcode markers have a user-selected ID.
    // Both types have width (of border).
    [field: SerializeField]
    public string PatternFileName { get; private set; } = "";
    [field: SerializeField]
    public string PatternContents { get; private set; } = ""; // Set by the editor.
    [field: SerializeField]
    public ulong BarcodeID { get; private set; } = 0;
    [field: SerializeField]
    public float PatternWidth { get; private set; } = 0.08f;
    public void ConfigureAsSquarePattern(string patternFilePath, float width)
    {
        Unload();
        Type = TrackableType.Square;
        if (!string.IsNullOrEmpty(patternFilePath))
        {
            PatternFileName = Path.GetFileName(patternFilePath);
            PatternContents = String.Join(" ", File.ReadAllLines(patternFilePath));
        }
        if (width > 0.0f) PatternWidth = width;
        _loadError = false;
        Load();
    }
    public void ConfigureAsSquareBarcode(ulong barcodeID, float width)
    {
        Unload();
        Type = TrackableType.SquareBarcode;
        if (barcodeID >= 0) BarcodeID = barcodeID;
        if (width > 0.0f) PatternWidth = width;
        _loadError = false;
        Load();
    }

    // If the marker is multi, it just has a config filename.
    [field: SerializeField]
    public string MultiConfigFile { get; private set; } = "";
    public void ConfigureAsMultiSquare(string multiConfigFilePath)
    {
        Unload();
        Type = TrackableType.Multimarker;
        if (!string.IsNullOrEmpty(multiConfigFilePath)) MultiConfigFile = multiConfigFilePath;
        _loadError = false;
        Load();
    }

    // NFT markers have a dataset pathname (less the extension).
    // Also, we need a list of the file extensions that make up an NFT dataset.
    [field: SerializeField]
    public string NFTDataName { get; private set; } = "";
    public static readonly string[] NFTDataExts = { "iset", "fset", "fset3" };

    public void ConfigureAsNFT(string nftDataName)
    {
        Unload();
        Type = TrackableType.NFT;
        if (!string.IsNullOrEmpty(nftDataName)) NFTDataName = nftDataName;
        _loadError = false;
        Load();
    }

    // Single markers and 2D planar surfaces have a single pattern, multi markers and NFT markers have one or more..
    private ARXPattern[] patterns;

    // Private fields with accessors.
    // Trackable configuration options.
    [SerializeField]
    private bool currentUseContPoseEstimation = false;                        // Single marker only; whether continuous pose estimation should be used.
    [SerializeField]
    private bool currentFiltered = false;
    [SerializeField]
    private float currentFilterSampleRate = 30.0f;
    [SerializeField]
    private float currentFilterCutoffFreq = 15.0f;
    [SerializeField]
    private float currentNFTScale = 1.0f;                                    // NFT marker only; scale factor applied to marker size.

    // Realtime tracking information
    private bool visible = false;                                           // Trackable is visible or not
    private Matrix4x4 transformationMatrix;                                 // Full transformation matrix as a Unity matrix.
    private Matrix4x4 transformationMatrixR;                                // For stereo video sources, full transformation matrix as viewed from the right camera, as a Unity matrix.

    [Tooltip("Register to receive an event that fires when a trackable is detected.")]
    public ARXUnityEventUnityObject OnTrackableFound = new ARXUnityEventUnityObject();
    [Tooltip("Register to receive an event that fires when a trackable is no longer detected.")]
    public ARXUnityEventUnityObject OnTrackableLost = new ARXUnityEventUnityObject();

    /// <summary>
    /// Set to true in Start on platforms where assets in StreamingAssets are not directly accessible
    /// in the file system and must be unpacked first. On Android StreamingAssets are in a .jar file,
    /// and on Web Player, they are at a URL.
    /// </summary>
    private bool _mustUnpackAssets = false;
    /// <summary>
    /// As loading may take some time, this is set to true during a load.
    /// While true, no calls to native trackable operations should be made.
    /// </summary>
    private bool _loading = false;
    /// <summary>
    /// Will be set to true if a load failed.
    /// </summary>
    private bool _loadError = false;


    /// <summary>
    /// Factory method to add a 2D image trackable to same GameObject as ARXController.
    /// </summary>
    /// <param name="imageFilePath">Filesystem path, relative to StreamingAssets folder, of image file.</param>
    /// <param name="imageWidth">Width of the image in Unity units, default 1.0 = 1 metre.</param>
    /// <returns></returns>
    static ARXTrackable Add2D(string imageFilePath, float imageWidth)
    {
        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return null;
        ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
        t.ConfigureAsTwoD(imageFilePath, imageWidth);
        return t;
    }

    /// <summary>
    /// actory method to add a square pattern marker trackable to same GameObject as ARXController.
    /// </summary>
    /// <param name="patternFilePath">Filesystem path of pattern file.</param>
    /// <param name="patternWidth">Width of marker (measured on one side of border) in Unity units, default 0.08 = 80mm.</param>
    /// <returns></returns>
    static ARXTrackable AddSquarePattern(string patternFilePath, float patternWidth)
    {
        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return null;
        ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
        t.ConfigureAsSquarePattern(patternFilePath, patternWidth);
        return t;
    }

    static ARXTrackable AddSquareBarcode(ulong barcodeID, float patternWidth)
    {
        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return null;
        ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
        t.ConfigureAsSquareBarcode(barcodeID, patternWidth);
        return t;
    }

    static ARXTrackable AddMultiSquare(string configFilePath)
    {
        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return null;
        ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
        t.ConfigureAsMultiSquare(configFilePath);
        return t;
    }

    static ARXTrackable AddNFT(string dataFilename)
    {
        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return null;
        ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
        t.ConfigureAsNFT(dataFilename);
        return t;
    }

    /// <summary>
    /// Register to receive an event that fires when a trackable is auto-added to the tracker, e.g.
    /// if a new barcode marker is tracked and auto-create trackables is enabled.
    /// The trackable is already created, so it is safe to call plugin functions on it.
    /// </summary>
    public static ARXUnityEventUnityObject OnTrackableAutoCreated = new ARXUnityEventUnityObject();

    /// <summary>
    /// Register to receive and event that fires in the case that a trackable is auto-removed from the tracker.
    /// Note that if you receive this event, the native trackable is already gone, so don't
    /// call any plugin functions on it. This event is just a courtesy for your internal book keeping.
    /// </summary>
    public static ARXUnityEventUnityObject OnTrackableAutoRemoved = new ARXUnityEventUnityObject();

    /// <summary>
    /// Adds an ARXTrackable for an auto-created trackable.
    /// </summary>
    /// <param name="UID"></param>
    /// <returns></returns>
    [AOT.MonoPInvokeCallback (typeof(PluginFunctionsTrackableEventCallback))]
    public static void OnTrackableEvent(int eventType, int UID)
    {
        switch ((ARW_TRACKABLE_EVENT_TYPE)eventType)
        {
            case ARW_TRACKABLE_EVENT_TYPE.ARW_TRACKABLE_EVENT_TYPE_AUTOCREATED:
                if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return;
                ARXTrackable t = ARXController.Instance.gameObject.AddComponent<ARXTrackable>();
                t.uid = UID;
                t.Type = (TrackableType)ARXController.Instance.PluginFunctions.arwGetTrackableOptionInt(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_TYPE);
                if (t.Type == TrackableType.SquareBarcode || t.Type == TrackableType.Square)
                {
                    t.PatternWidth = ARXController.Instance.PluginFunctions.arwGetTrackableOptionFloat(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_SQUARE_WIDTH) * 0.001f;
                    if (t.Type == TrackableType.SquareBarcode)
                    {
                        string barcodeIDString = ARXController.Instance.PluginFunctions.arwGetTrackableOptionString(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_SQUARE_BARCODE_ID);
                        ulong barcodeID;
                        if (ulong.TryParse(barcodeIDString, out barcodeID))
                        {
                            t.BarcodeID = barcodeID;
                        }
                    }
                }
                OnTrackableAutoCreated.Invoke(t);
                break;
            case ARW_TRACKABLE_EVENT_TYPE.ARW_TRACKABLE_EVENT_TYPE_AUTOREMOVED:
                if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) return;
                ARXTrackable[] ts = FindObjectsByType<ARXTrackable>(FindObjectsSortMode.None);
                foreach (ARXTrackable t1 in ts)
                {
                    if (t1.UID == UID)
                    {
                        OnTrackableAutoRemoved.Invoke(t1);
                        Destroy(t1.gameObject);
                        break;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        //ARXController.Log(LogTag + "ARXTrackable.OnDisable()");
        Unload();
    }

    private void Start()
    {
        _mustUnpackAssets = Application.streamingAssetsPath.Contains("://");
    }

    private bool FileIsCached(string basename)
    {
        if (string.IsNullOrEmpty(basename)) return false;
        return File.Exists(Path.Combine(Application.temporaryCachePath, basename));
    }

    private IEnumerator UnpackFilesFromStreamingAssetToCache(List<string> files, Action<List<string>> onFilesCachedAtPath, Action<string, bool> onError)
    {
        if (files == null || files.Count == 0)
        {
            onError("No files specified.", true);
            yield break;
        }
        List<string> cachedFilePaths = new List<string>();
        foreach (string basename in files)
        {
            string cachedPath = Path.Combine(Application.temporaryCachePath, basename);
            if (!File.Exists(cachedPath))
            {
                string file = Path.Combine(Application.streamingAssetsPath, basename); // E.g. Android "jar:file://" + Application.dataPath + "!/assets/" + basename, or Web: "http://localhost:8000/".
                using UnityWebRequest www = new UnityWebRequest(file);
                www.downloadHandler = new DownloadHandlerFile(cachedPath);
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    onError($"Could not fetch resource. Error: {www.error}.", false);
                    yield break;
                }
                cachedFilePaths.Add(cachedPath);
            }
        }
        onFilesCachedAtPath(cachedFilePaths);
    }

    // Load the native ARXTrackable structure(s) and set the UID.
    public void Load()
    {
        //ARXController.Log(LogTag + "ARXTrackable.Load()");
        if (UID != NO_ID) {
            return;
        }

        if (!ARXController.Instance || ARXController.Instance.PluginFunctions == null || !ARXController.Instance.PluginFunctions.IsInited()) {
            // If arwInitialiseAR() has not yet been called, we can't load the native trackable yet.
            // Load will be retried next Update.
            return;
        }

        if (_loading)
        {
            return;
        }
        _loading = true;

        switch (Type) {

            case TrackableType.Square:
                if (string.IsNullOrEmpty(PatternContents))
                {
                    LoadFailed("Request to load square marker but no pattern specified.", true);
                    return;
                }
                // Multiply width by 1000 to convert from metres to artoolkitX's millimetres.
                LoadNative($"single_buffer;{PatternWidth*1000.0};buffer={PatternContents}");
                break;

            case TrackableType.SquareBarcode:
                // Multiply width by 1000 to convert from metres to artoolkitX's millimetres.
                LoadNative($"single_barcode;{BarcodeID};{PatternWidth*1000.0f}");
                break;

            case TrackableType.Multimarker:
                if (string.IsNullOrEmpty(MultiConfigFile))
                {
                    LoadFailed("Request to load multi-square marker but no config file specified.", true);
                    return;
                }
                if (_mustUnpackAssets && !FileIsCached(MultiConfigFile))
                {
                    StartCoroutine(UnpackFilesFromStreamingAssetToCache(new List<string> {MultiConfigFile}, (paths) => LoadNative($"multi;{paths[0]}"), LoadFailed));
                    return;
                }
                LoadNative($"multi;{Path.Combine(Application.streamingAssetsPath, MultiConfigFile)}");
                break;

            case TrackableType.NFT:
                if (string.IsNullOrEmpty(NFTDataName))
                {
                    LoadFailed("Request to load legacy NFT marker but no data file name specified.", true);
                    return;
                }
                if (_mustUnpackAssets)
                {
                    List<string> toUnpack = new List<string>();
                    foreach (string ext in NFTDataExts)
                    {
                        string basename = $"{NFTDataName}.{ext}";
                        if (!FileIsCached(basename))
                        {
                            toUnpack.Add(basename);
                        }
                    }
                    if (toUnpack.Count > 0)
                    {
                        StartCoroutine(UnpackFilesFromStreamingAssetToCache(toUnpack, (paths) => LoadNative($"nft;{Path.Combine(Application.temporaryCachePath, NFTDataName)}"), LoadFailed));
                        return;
                    }
                    LoadNative($"nft;{Path.Combine(Application.temporaryCachePath, NFTDataName)}");
                    return;
                }
                LoadNative($"nft;{Path.Combine(Application.streamingAssetsPath, NFTDataName)}");
                break;

            case TrackableType.TwoD:
                if (string.IsNullOrEmpty(TwoDImageFile))
                {
                    LoadFailed("Request to load 2D trackable but no image file specified.", true);
                    return;
                }
                if (_mustUnpackAssets && !FileIsCached(TwoDImageFile))
                {
                    StartCoroutine(UnpackFilesFromStreamingAssetToCache(new List<string> {TwoDImageFile}, (paths) => LoadNative($"2d;{paths[0]};{TwoDImageWidth * 1000.0f}"), LoadFailed));
                    return;
                }
                LoadNative($"2d;{Path.Combine(Application.streamingAssetsPath, TwoDImageFile)};{TwoDImageWidth * 1000.0f}");
                break;

            default:
                LoadFailed($"Unknown trackable type {Type}.");
                break;
        }
    }

    private void LoadNative(string cfg)
    {
        // Get the native side to process it, and assign the resulting ARXTrackable UID.
        uid = ARXController.Instance.PluginFunctions.arwAddTrackable(cfg);
        if (UID == NO_ID) {
            LoadFailed("Error loading marker.");
            return;
        }

        // Trackable loaded. Do any additional configuration.
        //ARXController.Log("Added marker with cfg='" + cfg + "'");

        // Any additional trackable-type-specific config not included in the trackable config string used at load time.
        if (Type == TrackableType.Square || Type == TrackableType.SquareBarcode) UseContPoseEstimation = currentUseContPoseEstimation;
        if (Type == TrackableType.NFT) NFTScale = currentNFTScale;

        // Any additional config.
        Filtered = currentFiltered;
        FilterSampleRate = currentFilterSampleRate;
        FilterCutoffFreq = currentFilterCutoffFreq;

        // Create array of patterns. A single marker will have array length 1.
        int numPatterns = ARXController.Instance.PluginFunctions.arwGetTrackablePatternCount(UID);
        //ARXController.Log("Trackable with UID=" + UID + " has " + numPatterns + " patterns.");
        if (numPatterns > 0) {
            patterns = new ARXPattern[numPatterns];
            for (int i = 0; i < numPatterns; i++) {
                patterns[i] = new ARXPattern(UID, i);
            }
        }

        _loadError = false;
        _loading = false;
    }

    private void LoadFailed(string errorMessage, bool hideInEditorMode = false)
    {
        if (!hideInEditorMode || Application.isPlaying) ARXController.LogError("${LogTag}{errorMessage}");
        _loadError = true;
        _loading = false;
    }

    // Note that [DefaultExecutionOrder] is used on ARXController to ensure that a tracking update has completed before we try
    // to fetch our transformation here.
    void Update()
    {
        // Only query visibility if we are running in the Player.
        if (!Application.isPlaying)
        {
            return;
        }

        //ARXController.Log(LogTag + "ARXTrackable.Update()");
        if (!_loading)
        {
            bool v = false;
            if (ARXController.Instance && ARXController.Instance.PluginFunctions != null && ARXController.Instance.PluginFunctions.IsInited()) {

                // Lazy loading, provided we didn't already try to load and get an error.
                if (UID == NO_ID && !_loadError)
                {
                    Load();
                }
                if (UID != NO_ID)
                {
                    float[] matrixRawArray = new float[16];
                    float[] matrixRawArrayR = new float[16];
                    bool stereoVideo = ARXController.Instance.VideoIsStereo;
                    if (!stereoVideo)
                    {
                        v = ARXController.Instance.PluginFunctions.arwQueryTrackableVisibilityAndTransformation(UID, matrixRawArray);
                    }
                    else
                    {
                        v = ARXController.Instance.PluginFunctions.arwQueryTrackableVisibilityAndTransformationStereo(UID, matrixRawArray, matrixRawArrayR);
                    }
                    //ARXController.Log(LogTag + "ARXTrackable.Update() UID=" + UID + ", visible=" + v);

                    if (v)
                    {
                        matrixRawArray[12] *= 0.001f; // Scale the position from artoolkitX units (mm) into Unity units (m).
                        matrixRawArray[13] *= 0.001f;
                        matrixRawArray[14] *= 0.001f;

                        Matrix4x4 matrixRaw = ARXUtilityFunctions.MatrixFromFloatArray(matrixRawArray);
                        //.Log("arwQueryTrackableTransformation(" + UID + ") got matrix: [" + Environment.NewLine + matrixRaw.ToString("F3").Trim() + "]");

                        // artoolkitX uses right-hand coordinate system where the marker lies in x-y plane with right in direction of +x,
                        // up in direction of +y, and forward (towards viewer) in direction of +z.
                        // Need to convert to Unity's left-hand coordinate system where marker lies in x-y plane with right in direction of +x,
                        // up in direction of +y, and forward (towards viewer) in direction of -z.
                        transformationMatrix = ARXUtilityFunctions.LHMatrixFromRHMatrix(matrixRaw);

                        if (stereoVideo)
                        {
                            matrixRawArrayR[12] *= 0.001f;
                            matrixRawArrayR[13] *= 0.001f;
                            matrixRawArrayR[14] *= 0.001f;
                            Matrix4x4 matrixRawR = ARXUtilityFunctions.MatrixFromFloatArray(matrixRawArrayR);
                            //.Log("arwQueryTrackableTransformationStereo(" + UID + ") got matrixR: [" + Environment.NewLine + matrixRawR.ToString("F3").Trim() + "]");
                            transformationMatrixR = ARXUtilityFunctions.LHMatrixFromRHMatrix(matrixRawR);
                        }

                    }
                }
            }
            if (v != visible)
            {
                if (v) OnTrackableFound.Invoke(this);
                else OnTrackableLost.Invoke(this);
            }
            visible = v;
        }
        return;
    }

    // Unload any native ARXTrackable structures, and clear the UID.
    public void Unload()
    {
        //ARXController.Log(LogTag + "ARXTrackable.Unload()");
        if (UID == NO_ID) {
            return;
        }

        if (!_loading) {
            // Remove the native trackable, unless arwShutdownAR() has already been called (as it will already have been removed.)
            if (ARXController.Instance && ARXController.Instance.PluginFunctions != null && ARXController.Instance.PluginFunctions.IsInited()) {
                ARXController.Instance.PluginFunctions.arwRemoveTrackable(UID);
            }

            uid = NO_ID;
            _loadError = false;
            patterns = null; // Delete the patterns too.
            visible = false;
        }
    }

    public Matrix4x4 TransformationMatrix
    {
        get
        {
            return transformationMatrix;
        }
    }

    public Matrix4x4 TransformationMatrixR
    {
        get
        {
            return transformationMatrixR;
        }
    }

    public bool Visible
    {
        get
        {
            return visible;
        }
    }


    public ARXPattern[] Patterns
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
            if (UID != NO_ID && !_loading) {
                ARXController.Instance.PluginFunctions.arwSetTrackableOptionBool(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_FILTERED, value);
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
            if (UID != NO_ID && !_loading) {
                ARXController.Instance.PluginFunctions.arwSetTrackableOptionFloat(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_FILTER_SAMPLE_RATE, value);
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
            if (UID != NO_ID && !_loading) {
                ARXController.Instance.PluginFunctions.arwSetTrackableOptionFloat(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_FILTER_CUTOFF_FREQ, value);
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
            if (UID != NO_ID && !_loading && (Type == TrackableType.Square || Type == TrackableType.SquareBarcode)) {
                 ARXController.Instance.PluginFunctions.arwSetTrackableOptionBool(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_SQUARE_USE_CONT_POSE_ESTIMATION, value);
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
            if (UID != NO_ID && !_loading && (Type == TrackableType.NFT)) {
                 ARXController.Instance.PluginFunctions.arwSetTrackableOptionFloat(UID, (int)ARWTrackableOption.ARW_TRACKABLE_OPTION_NFT_SCALE, value);
            }
        }
    }

}
