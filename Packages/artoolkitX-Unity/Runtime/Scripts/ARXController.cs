/*
 *  ARXController.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.Rendering;


/// <summary>
/// Manages core ARToolKit behaviour.
///
/// There should be exactly one instance of this component in the scene to provide ARToolKit functionality.
///
/// Script execution order is set to -101 on this component, to ensure that a tracking update has completed
/// prior to ARXTrackable components fetching their pose information.
/// </summary>
[ExecuteInEditMode]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-101)]
[RequireComponent(typeof(ARXVideoConfig))]
public class ARXController : MonoBehaviour
{
    //
    // Logging.
    //
    private static List<string> logMessages = new List<string>();
    private const int MaximumLogMessages = 1000;
    private const string LogTag = "ARXController: ";

    private static ARXController _instance = null;
    public static ARXController Instance
    {
        get {
            if (_instance == null)
            {
                ARXController[] c = FindObjectsOfType<ARXController>();
                if (c.Length == 0)
                {
                    LogError("There are no instances of " + typeof(ARXController) + " in the scene.");
                }
                else if (c.Length > 1)
                {
                    LogError("There is more than one instance of " + typeof(ARXController) + " in the scene.");
                }
                else
                {
                    _instance = c[0];
                }
            }
            return _instance;
        }
    }

    // Main reference to the plugin functions. Created in OnEnable, destroyed in OnDisable().
    public IPluginFunctions PluginFunctions { get; private set; }

    // Application preferences.
    public bool UseNativeGLTexturingIfAvailable = true;
    public bool AllowNonRGBVideo = true;
    public bool QuitOnEscOrBack = true;
    public bool AutoStartAR = true;

    // Video source config.
    public string videoCParamName0 = "";
    public bool VideoIsStereo = false;
    public string transL2RName = "transL2R";
    public string videoCParamName1 = "";

    //API Addition - Users can check this value to see if the camera is initialised running.
    //Usage: Used to show 'Please Wait' UIs while the camera is still initialising or markers are being loaded.
    [HideInInspector]
    public bool IsRunning { get { return _running; } }

    //
    // State.
    //

    private string _version = "";
    private bool _running = false;
    private bool _runOnUnpause = false;
    private bool _sceneConfiguredForVideo = false;
    private bool _sceneConfiguredForVideoWaitingMessageLogged = false;
    private bool _useNativeGLTexturing = false;
    // As Unity doesn't provide its own notification of screen geometry changes, we need to keep track of screen
    // parameters so we can detect size changes (and orientation changes, on mobile devices).
#if UNITY_IOS || UNITY_ANDROID
    private ScreenOrientation _screenOrientation = ScreenOrientation.LandscapeLeft;
#endif
    private int _screenWidth = 0;
    private int _screenHeight = 0;

    public IARXUnityVideoSource UnityVideoSource = null;

#if UNITY_ANDROID
    //
    //Android Plugin
    //
    private AndroidJavaObject androidPlugin = null;
#endif

    //private int _frameStatsCount = 0;
    //private float _frameStatsTimeUpdateTexture = 0.0f;
    //private float _frameStatsTimeSetPixels = 0.0f;
    //private float _frameStatsTimeApply = 0.0f;

    // Frames per second calculations
    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float lastFramerate = 0.0f;
    private float refreshTime = 0.5f;


    public enum ARToolKitThresholdMode
    {
        Manual = 0,
        Median = 1,
        Otsu = 2,
        Adaptive = 3,
        Bracketing = 4
    }

    public enum ARToolKitLabelingMode
    {
        WhiteRegion = 0,
        BlackRegion = 1,
    }

    public enum ARToolKitPatternDetectionMode
    {
        AR_TEMPLATE_MATCHING_COLOR = 0,
        AR_TEMPLATE_MATCHING_MONO = 1,
        AR_MATRIX_CODE_DETECTION = 2,
        AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX = 3,
        AR_TEMPLATE_MATCHING_MONO_AND_MATRIX = 4
    };

    public enum ARToolKitMatrixCodeType
    {
        AR_MATRIX_CODE_3x3 = 0x03,
        AR_MATRIX_CODE_3x3_PARITY65 = 0x03 | 0x100,
        AR_MATRIX_CODE_3x3_HAMMING63 = 0x03 | 0x200,
        AR_MATRIX_CODE_4x4 = 0x04,
        AR_MATRIX_CODE_4x4_BCH_13_9_3 = 0x04 | 0x300,
        AR_MATRIX_CODE_4x4_BCH_13_5_5 = 0x04 | 0x400,
        AR_MATRIX_CODE_5x5 = 0x05,
        AR_MATRIX_CODE_5x5_BCH_22_12_5 = 0x05 | 0x400,
        AR_MATRIX_CODE_5x5_BCH_22_7_7 = 0x05 | 0x500,
        AR_MATRIX_CODE_6x6 = 0x06,
        AR_MATRIX_CODE_GLOBAL_ID = 0x0e | 0xb00
    };

    public enum ARToolKitImageProcMode
    {
        AR_IMAGE_PROC_FRAME_IMAGE = 0,
        AR_IMAGE_PROC_FIELD_IMAGE = 1
    };

    public enum ARW_UNITY_RENDER_EVENTID
    {
        NOP = 0, // No operation (does nothing).
        UPDATE_TEXTURE_GL = 1,
        UPDATE_TEXTURE_GL_STEREO = 2,
    };

    public enum ARW_ERROR
    {
        ARW_ERROR_NONE = 0,
        ARW_ERROR_GENERIC = -1,
        ARW_ERROR_OUT_OF_MEMORY = -2,
        ARW_ERROR_OVERFLOW = -3,
        ARW_ERROR_NODATA = -4,
        ARW_ERROR_IOERROR = -5,
        ARW_ERROR_EOF = -6,
        ARW_ERROR_TIMEOUT = -7,
        ARW_ERROR_INVALID_COMMAND = -8,
        ARW_ERROR_INVALID_ENUM = -9,
        ARW_ERROR_THREADS = -10,
        ARW_ERROR_FILE_NOT_FOUND = -11,
        ARW_ERROR_LENGTH_UNAVAILABLE = -12,
        ARW_ERROR_DEVICE_UNAVAILABLE = -13
    };

    public enum AR_LOG_LEVEL
    {
        AR_LOG_LEVEL_DEBUG = 0,
        AR_LOG_LEVEL_INFO,
        AR_LOG_LEVEL_WARN,
        AR_LOG_LEVEL_ERROR,
        AR_LOG_LEVEL_REL_INFO
    }

    /// <summary>
    /// Specifies desired horizontal alignment of video frames in drawing graphics context.
    /// </summary>
    public enum ARW_H_ALIGN
    {
        ARW_H_ALIGN_LEFT,       ///< Align the left edge of the video frame with the left edge of the context.
        ARW_H_ALIGN_CENTRE,     ///< Align the centre of the video frame with the centre of the context.
        ARW_H_ALIGN_RIGHT       ///< Align the right edge of the video frame with the right edge of the context.
    }

    /// <summary>
    /// Specifies desired vertical alignment of video frames in drawing graphics context.
    /// </summary>
    public enum ARW_V_ALIGN
    {
        ARW_V_ALIGN_TOP,        ///< Align the top edge of the video frame with the top edge of the context.
        ARW_V_ALIGN_CENTRE,     ///< Align the centre of the video frame with the centre of the context.
        ARW_V_ALIGN_BOTTOM      ///< Align the bottom edge of the video frame with the bottom edge of the context.
    }

    // Private fields with accessors.
    [SerializeField]
    private ARToolKitThresholdMode currentThresholdMode = ARToolKitThresholdMode.Manual;
    [SerializeField]
    private int currentThreshold = 100;
    [SerializeField]
    private ARToolKitLabelingMode currentLabelingMode = ARToolKitLabelingMode.BlackRegion;
    [SerializeField]
    private int currentTemplateSize = 16;
    [SerializeField]
    private int currentTemplateCountMax = 25;
    [SerializeField]
    private float currentBorderSize = 0.25f;
    [SerializeField]
    private ARToolKitPatternDetectionMode currentPatternDetectionMode = ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
    [SerializeField]
    private ARToolKitMatrixCodeType currentMatrixCodeType = ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
    [SerializeField]
    private ARToolKitImageProcMode currentImageProcMode = ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
    [SerializeField]
    private bool currentNFTMultiMode = false;
    [Tooltip("The max. number of markers to track simultaneously. If fewer than this number are found, then detection will run each frame, as well as tracking for any detected markers.")]
    [SerializeField]
    private int currentTwoDMaxMarkersToTrack = 1;
    [Tooltip("If set, detection and tracking will run independently from frame capture and display on a separate thread.")]
    [SerializeField]
    private bool currentTwoDThreaded = true;
    [SerializeField]
    private AR_LOG_LEVEL currentLogLevel = AR_LOG_LEVEL.AR_LOG_LEVEL_INFO;
    [SerializeField]
    private bool currentSquareMatrixModeAutocreateNewTrackables = false;
    [SerializeField]
    private float currentSquareMatrixModeAutocreateNewTrackablesDefaultWidth = 0.08f;

    // Links to other components.
    private ARXVideoConfig arvideoconfig = null;

    // Notifications.
    public UnityEvent onVideoStarted = new UnityEvent();
    public UnityEvent onVideoStopped = new UnityEvent();
    public UnityEvent onVideoFrame = new UnityEvent();
    public UnityEvent onScreenGeometryChanged = new UnityEvent();

    //
    // MonoBehavior methods.
    //

    void OnEnable()
    {
        LogInfo($"{LogTag}OnEnable()");

        PluginFunctions = new PluginFunctionsARX();
        arvideoconfig = gameObject.GetComponent<ARXVideoConfig>();
#if UNITY_IOS && !UNITY_EDITOR
        ARX_pinvoke.aruRequestCamera();
        System.Threading.Thread.Sleep(2000);
#endif
        Application.runInBackground = true;

        // Register the log callback. This can be set irrespective of whether PluginFunctions.inited is true or false.
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:                        // Unity Editor on OS X.
            case RuntimePlatform.OSXPlayer:                        // Unity Player on OS X.
            case RuntimePlatform.WindowsEditor:                    // Unity Editor on Windows.
            case RuntimePlatform.WindowsPlayer:                    // Unity Player on Windows.
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WSAPlayerX86:                     // Unity Player on Windows Store X86.
            case RuntimePlatform.WSAPlayerX64:                     // Unity Player on Windows Store X64.
            case RuntimePlatform.WSAPlayerARM:                     // Unity Player on Windows Store ARM.
                PluginFunctions.arwRegisterLogCallback(Log);
                break;
            case RuntimePlatform.Android:                          // Unity Player on Android.
            case RuntimePlatform.IPhonePlayer:                     // Unity Player on iOS.
            default:
                break;
        }
        PluginFunctions.arwSetLogLevel((int)currentLogLevel);

        // ARXController is up, so init.
        if (!PluginFunctions.IsInited())
        {
            InitializeAR();
        }
    }

    private void InitializeAR()
    {
        if (!PluginFunctions.IsInited())
        {
            if (PluginFunctions.arwInitialiseAR(TemplateSize, TemplateCountMax, (int)MatrixCodeType))
            {
                // artoolkitX version number
                _version = PluginFunctions.arwGetARToolKitVersion();
                LogInfo($"{LogTag}artoolkitX version {_version} initialised.");
            }
            else
            {
                LogError($"{LogTag}Error initialising artoolkitX", this);
            }
        }
    }

    void Start()
    {
        LogInfo($"{LogTag}Start(): Application.isPlaying:{Application.isPlaying} autoStart:{AutoStartAR}.");
        if (!Application.isPlaying) return; // Editor Start.

        // Player start.
        if (AutoStartAR)
        {
            StartAR();
        }
    }

    void OnApplicationPause(bool paused)
    {
        //Log($"{LogTag}OnApplicationPause({paused})");
        if (paused)
        {
            if (_running)
            {
                StopAR();
                _runOnUnpause = true;
            }
        }
        else
        {
            if (_runOnUnpause)
            {
                StartAR();
                _runOnUnpause = false;
            }
        }
    }

    void Update()
    {
        //Log($"{LogTag}ARXController.Update()");
        if (!Application.isPlaying) return; // Editor update.

        // Player update.
        if (Input.GetKeyDown(KeyCode.Menu) || Input.GetKeyDown(KeyCode.Return)) showGUIDebug = !showGUIDebug;
        if (QuitOnEscOrBack && Input.GetKeyDown(KeyCode.Escape)) // On Android, maps to "back" button.
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        CalculateFPS();

        if (_running)
        {
            UpdateAR();
        }
    }

    // Called when the user quits the application, or presses stop in the editor.
    void OnApplicationQuit()
    {
        //Log($"{LogTag}ARXController.OnApplicationQuit()");

        StopAR();
    }

    void OnDisable()
    {
        LogInfo($"{LogTag}OnDisable()");

        if (PluginFunctions.IsInited())
        {
            FinalizeAR();
        }

        // Since we might be going away, tell users of our Log function
        // to stop calling it.
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WSAPlayerX86:
            case RuntimePlatform.WSAPlayerX64:
            case RuntimePlatform.WSAPlayerARM:
                PluginFunctions.arwRegisterLogCallback(null);
                break;
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
            default:
                break;
        }
        PluginFunctions = null;

    }

    void FinalizeAR()
    {
        //Log($"{LogTag}ARXController.FinalizeAR()");
        if (_running) {
            StopAR();
        }

        if (PluginFunctions.IsInited()) {
            LogInfo($"{LogTag}Shutting down artoolkitX");
            // arwShutdownAR() causes everything artoolkitX holds to be unloaded.
            if (!PluginFunctions.arwShutdownAR())
            {
                LogError($"{LogTag}Error shutting down artoolkitX.", this);
            }
        }
    }

    // As OnDestroy() is called from the ARXController object's destructor, don't do anything
    // here that assumes that the ARXController object is still valid. Do that sort of shutdown
    // in OnDisable() instead.
    void OnDestroy()
    {
        //Log($"{LogTag}ARXController.OnDestroy()");

        // Classes inheriting from MonoBehavior should set all static member variables to null on unload.
        logMessages.Clear();
        if (_instance == this)
        {
            _instance = null;
        }
    }

    //
    // User-callable AR methods.
    //

    public void StartAR()
    {
        StartCoroutine(StartARCo());
    }

    public IEnumerator StartARCo()
    {
        // Catch attempts to inadvertently call StartAR() twice.
        if (_running)
        {
            LogWarning($"{LogTag}WARNING: StartAR() called while already running. Ignoring.\n");
            yield break;
        }

        // For late startup after configuration, StartAR needs to ensure InitialiseAR has been called.
        if (!PluginFunctions.IsInited())
        {
            InitializeAR();
        }
        if (PluginFunctions.IsInited())
        {
            LogInfo($"{LogTag}Starting AR.");

#if UNITY_ANDROID
            bool haveCameraPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
            LogInfo(LogTag + $"haveCameraPermission={haveCameraPermission}");
            if (!haveCameraPermission)
            {
                PermissionCallbacks pcs = new PermissionCallbacks();
                pcs.PermissionGranted += (string permissionName) => StartAR();
                pcs.PermissionDenied += (string permissionName) => {
                    showGUIErrorDialogContent = "As you have denied camera access, unable to start AR tracking.";
                    showGUIErrorDialog = true;
                };
                pcs.PermissionDeniedAndDontAskAgain += (string permissionName) => {
                    showGUIErrorDialogContent = "As you have denied camera access, unable to start AR tracking.";
                    showGUIErrorDialog = true;
                };
                Permission.RequestUserPermission(Permission.Camera, pcs);
                yield break;
            }
#endif

            _sceneConfiguredForVideo = _sceneConfiguredForVideoWaitingMessageLogged = false;

            // Check rendering device.
            string renderDeviceVersion = SystemInfo.graphicsDeviceVersion;
            GraphicsDeviceType renderDeviceType = SystemInfo.graphicsDeviceType;
            bool usingOpenGL = renderDeviceType == GraphicsDeviceType.OpenGLCore || renderDeviceType == GraphicsDeviceType.OpenGLES2 || renderDeviceType == GraphicsDeviceType.OpenGLES3;
            //_useNativeGLTexturing = usingOpenGL && UseNativeGLTexturingIfAvailable;
            _useNativeGLTexturing = false; // TODO: reinstate native texturing support.
            LogInfo($"{LogTag}Render device: {renderDeviceVersion}, using {(_useNativeGLTexturing ? "native GL" : "Unity")} texturing.");

            // Init screen geometry.
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
#if UNITY_IOS || UNITY_ANDROID
            _screenOrientation = Screen.orientation;
#endif

            // Retrieve video configuration, and append any required per-platform overrides.
            // For native GL texturing we need monoplanar video; iOS and Android default to biplanar format.
            string videoConfiguration0 = arvideoconfig.GetVideoConfigString();
            string videoConfiguration1 = arvideoconfig.GetVideoConfigString(true);
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    if (_useNativeGLTexturing || !AllowNonRGBVideo)
                    {
                        if (videoConfiguration0.IndexOf("-device=AVFoundation") != -1) videoConfiguration0 += " -format=BGRA";
                        if (videoConfiguration1.IndexOf("-device=AVFoundation") != -1) videoConfiguration1 += " -format=BGRA";
                    }
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    if (_useNativeGLTexturing || !AllowNonRGBVideo)
                    {
                        if (videoConfiguration0.IndexOf("-device=WinMF") != -1) videoConfiguration0 += " -format=BGRA";
                        if (videoConfiguration1.IndexOf("-device=WinMF") != -1) videoConfiguration1 += " -format=BGRA";
                    }
                    break;
                case RuntimePlatform.Android:
                    videoConfiguration0 += " -cachedir=\"" + Application.temporaryCachePath + "\"";
                    videoConfiguration1 += " -cachedir=\"" + Application.temporaryCachePath + "\"";
                    if (_useNativeGLTexturing || !AllowNonRGBVideo)
                    {
                        if (videoConfiguration0.IndexOf("-device=Android") != -1) videoConfiguration0 += " -format=RGBA";
                        if (videoConfiguration0.IndexOf("-device=Android") != -1) videoConfiguration1 += " -format=RGBA";
                    }
                    break;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                default:
                    break;
            }

            // If using a Unity video source, start it now.
            if (arvideoconfig.IsUsingUnityVideoSource())
            {
                UnityVideoSource = new ARXUnityVideoSourceWebCamTexture();
                UnityVideoSource.OnVideoStart(this);
            }

            // Load the default camera parameters.
            byte[] cparam0 = null;
            byte[] cparam1 = null;
            byte[] transL2R = null;
            if (!string.IsNullOrEmpty(videoCParamName0))
            {
                TextAsset ta = Resources.Load("ardata/" + videoCParamName0, typeof(TextAsset)) as TextAsset;
                if (ta == null)
                {
                    // Error - the camera_para.dat file isn't in the right place
                    LogError($"{LogTag}StartAR(): Error: Camera parameters file not found at Resources/ardata/{videoCParamName0}.bytes", this);
                    yield break;
                }
                cparam0 = ta.bytes;
            }
            if (VideoIsStereo)
            {
                if (!string.IsNullOrEmpty(videoCParamName1))
                {
                    TextAsset ta = Resources.Load("ardata/" + videoCParamName1, typeof(TextAsset)) as TextAsset;
                    if (ta == null)
                    {
                        // Error - the camera_para.dat file isn't in the right place
                        LogError($"{LogTag}StartAR(): Error: Camera parameters file not found at Resources/ardata/{videoCParamName1}.bytes", this);
                        yield break;
                    }
                    cparam1 = ta.bytes;
                }
                TextAsset ta1 = Resources.Load("ardata/" + transL2RName, typeof(TextAsset)) as TextAsset;
                if (ta1 == null)
                {
                    // Error - the transL2R.dat file isn't in the right place
                    LogError($"{LogTag}StartAR(): Error: The stereo calibration file not found at Resources/ardata/{transL2RName}.bytes", this);
                    yield break;
                }
                transL2R = ta1.bytes;
            }

            // Begin video capture and marker detection.
            if (!VideoIsStereo)
            {
                LogInfo($"{LogTag}Starting artoolkitX video with vconf '{videoConfiguration0}'.");
                _running = PluginFunctions.arwStartRunningB(videoConfiguration0, cparam0, (cparam0 != null ? cparam0.Length : 0));
            }
            else
            {
                LogInfo($"{LogTag}Starting artoolkitX video with vconfL '{videoConfiguration0}', vconfR '{videoConfiguration1}'.");
                _running = PluginFunctions.arwStartRunningStereoB(videoConfiguration0, cparam0, (cparam0 != null ? cparam0.Length : 0), videoConfiguration1, cparam1, (cparam1 != null ? cparam1.Length : 0), transL2R, (transL2R != null ? transL2R.Length : 0));

            }

            if (!_running)
            {

                LogError($"{LogTag}Error starting running", this);
                ARW_ERROR error = (ARW_ERROR)PluginFunctions.arwGetError();
                if (error == ARW_ERROR.ARW_ERROR_DEVICE_UNAVAILABLE)
                {
                    showGUIErrorDialogContent = "Unable to start AR tracking. The camera may be in use by another application.";
                }
                else
                {
                    showGUIErrorDialogContent = "Unable to start AR tracking. Please check that you have a camera connected.";
                }
                showGUIErrorDialog = true;
                yield break;
            }

            // After calling arwStartRunningB/arwStartRunningStereoB, set artoolkitX configuration.
            LogInfo($"{LogTag}Setting artoolkitX tracking settings.");
            VideoThreshold = currentThreshold;
            VideoThresholdMode = currentThresholdMode;
            LabelingMode = currentLabelingMode;
            BorderSize = currentBorderSize;
            PatternDetectionMode = currentPatternDetectionMode;
            MatrixCodeType = currentMatrixCodeType;
            ImageProcMode = currentImageProcMode;
            NFTMultiMode = currentNFTMultiMode;
            TwoDMaxMarkersToTrack = currentTwoDMaxMarkersToTrack;
            TwoDThreaded = currentTwoDThreaded;
            SquareMatrixModeAutocreateNewTrackables = currentSquareMatrixModeAutocreateNewTrackables;
            SquareMatrixModeAutocreateNewTrackablesDefaultWidth = currentSquareMatrixModeAutocreateNewTrackablesDefaultWidth;

            // Prevent display sleep.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        // Remaining Unity setup happens in UpdateAR().
    }

    bool UpdateAR()
    {
        if (!_running)
        {
            return true;
        }

        if (UnityVideoSource != null)
        {
            UnityVideoSource.OnVideoUpdate();
        }

        if (!_sceneConfiguredForVideo)
        {
            // Wait for the wrapper to confirm video frames have arrived before configuring our video-dependent stuff.
            if (!PluginFunctions.arwIsRunning())
            {
                if (!_sceneConfiguredForVideoWaitingMessageLogged)
                {
                    LogInfo($"{LogTag}UpdateAR: Waiting for artoolkitX video.");
                    _sceneConfiguredForVideoWaitingMessageLogged = true;
                }
            }
            else
            {
                LogInfo($"{LogTag}UpdateAR: artoolkitX video is running. Configuring Unity scene for video.");

                onVideoStarted.Invoke();

                LogInfo($"{LogTag}Scene configured for video.");
                _sceneConfiguredForVideo = true;
            } // !running
        } // !sceneConfiguredForVideo

        // Check for screen geometry changes.
        if (Screen.width != _screenWidth || Screen.height != _screenHeight
#if UNITY_IOS || UNITY_ANDROID
            || Screen.orientation != _screenOrientation
#endif
            )
        {
#if UNITY_IOS || UNITY_ANDROID
            _screenOrientation = Screen.orientation;
#endif
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            onScreenGeometryChanged.Invoke();
        }

        bool gotFrame = PluginFunctions.arwCapture();
        if (gotFrame)
        {
            if (!PluginFunctions.arwUpdateAR()) return false;
            onVideoFrame.Invoke();
        }

        return true;
    }

    public bool StopAR()
    {
        if (!_running)
        {
            return false;
        }

        LogInfo($"{LogTag}Stopping AR.");

        if (UnityVideoSource != null)
        {
            UnityVideoSource.OnVideoStop();
            UnityVideoSource = null;
        }

        // Stop video capture and marker detection.
        if (!PluginFunctions.arwStopRunning())
        {
            LogError($"{LogTag}Error stopping AR.", this);
        }

        onVideoStopped.Invoke();

        // Reset display sleep.
        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        _running = false;
        return true;
    }

    //
    // User-callable configuration methods.
    //

    public bool DebugVideo
    {
        get
        {
            return (PluginFunctions.arwGetVideoDebugMode());
        }

        set
        {
            PluginFunctions.arwSetVideoDebugMode(value);
        }
    }

    public string Version
    {
        get
        {
            return _version;
        }
    }

    //
    // Tracker configuration
    //
    #region Tracker configuration.

    public ARToolKitThresholdMode VideoThresholdMode
    {
        get
        {
            int ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetVideoThresholdMode();
                if (ret >= 0) currentThresholdMode = (ARToolKitThresholdMode)ret;
                else currentThresholdMode = ARToolKitThresholdMode.Manual;
            }
            return currentThresholdMode;
        }

        set
        {
            currentThresholdMode = value;
            if (_running)
            {
                PluginFunctions.arwSetVideoThresholdMode((int)currentThresholdMode);
            }
        }
    }

    public int VideoThreshold
    {
        get
        {
            if (_running)
            {
                currentThreshold = PluginFunctions.arwGetVideoThreshold();
                if (currentThreshold < 0 || currentThreshold > 255) currentThreshold = 100;
            }
            return currentThreshold;
        }

        set
        {
            currentThreshold = value;
            if (_running)
            {
                PluginFunctions.arwSetVideoThreshold(value);
            }
        }
    }

    public ARToolKitLabelingMode LabelingMode
    {
        get
        {
            int ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetLabelingMode();
                if (ret >= 0) currentLabelingMode = (ARToolKitLabelingMode)ret;
                else currentLabelingMode = ARToolKitLabelingMode.BlackRegion;
            }
            return currentLabelingMode;
        }

        set
        {
            currentLabelingMode = value;
            if (_running)
            {
                PluginFunctions.arwSetLabelingMode((int)currentLabelingMode);
            }
        }
    }

    public float BorderSize
    {
        get
        {
            float ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetBorderSize();
                if (ret > 0.0f && ret < 0.5f) currentBorderSize = ret;
                else currentBorderSize = 0.25f;
            }
            return currentBorderSize;
        }

        set
        {
            currentBorderSize = value;
            if (_running)
            {
                PluginFunctions.arwSetBorderSize(currentBorderSize);
            }
        }
    }

    public int TemplateSize
    {
        get
        {
            return currentTemplateSize;
        }

        set
        {
            currentTemplateSize = value;
            LogWarning($"{LogTag}Warning: template size changed. Please reload scene.");
        }
    }

    public int TemplateCountMax
    {
        get
        {
            return currentTemplateCountMax;
        }

        set
        {
            currentTemplateCountMax = value;
            LogWarning($"{LogTag}Warning: template maximum count changed. Please reload scene.");
        }
    }

    public ARToolKitPatternDetectionMode PatternDetectionMode
    {
        get
        {
            int ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetPatternDetectionMode();
                if (ret >= 0) currentPatternDetectionMode = (ARToolKitPatternDetectionMode)ret;
                else currentPatternDetectionMode = ARToolKitPatternDetectionMode.AR_TEMPLATE_MATCHING_COLOR;
            }
            return currentPatternDetectionMode;
        }

        set
        {
            currentPatternDetectionMode = value;
            if (_running)
            {
                PluginFunctions.arwSetPatternDetectionMode((int)currentPatternDetectionMode);
            }
        }
    }

    public ARToolKitMatrixCodeType MatrixCodeType
    {
        get
        {
            int ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetMatrixCodeType();
                if (ret >= 0) currentMatrixCodeType = (ARToolKitMatrixCodeType)ret;
                else currentMatrixCodeType = ARToolKitMatrixCodeType.AR_MATRIX_CODE_3x3;
            }
            return currentMatrixCodeType;
        }

        set
        {
            currentMatrixCodeType = value;
            if (_running)
            {
                PluginFunctions.arwSetMatrixCodeType((int)currentMatrixCodeType);
            }
        }
    }

    public ARToolKitImageProcMode ImageProcMode
    {
        get
        {
            int ret;
            if (_running)
            {
                ret = PluginFunctions.arwGetImageProcMode();
                if (ret >= 0) currentImageProcMode = (ARToolKitImageProcMode)ret;
                else currentImageProcMode = ARToolKitImageProcMode.AR_IMAGE_PROC_FRAME_IMAGE;
            }
            return currentImageProcMode;
        }

        set
        {
            currentImageProcMode = value;
            if (_running)
            {
                PluginFunctions.arwSetImageProcMode((int)currentImageProcMode);
            }
        }
    }

    public bool NFTMultiMode
    {
        get
        {
            if (_running)
            {
                currentNFTMultiMode = PluginFunctions.arwGetNFTMultiMode();
            }
            return currentNFTMultiMode;
        }

        set
        {
            currentNFTMultiMode = value;
            if (_running)
            {
                PluginFunctions.arwSetNFTMultiMode(currentNFTMultiMode);
            }
        }
    }

    public bool TwoDThreaded
    {
        get
        {
            if (_running)
            {
                currentTwoDThreaded = PluginFunctions.arwGet2DThreaded();
            }
            return currentTwoDThreaded;
        }

        set
        {
            currentTwoDThreaded = value;
            if (_running)
            {
                PluginFunctions.arwSet2DThreaded(currentTwoDThreaded);
            }
        }
    }

    public int TwoDMaxMarkersToTrack
    {
        get
        {
            if (_running)
            {
                currentTwoDMaxMarkersToTrack = PluginFunctions.arwGet2DMaxMarkersToTrack();
            }
            return currentTwoDMaxMarkersToTrack;
        }

        set
        {
            currentTwoDMaxMarkersToTrack = value;
            if (_running)
            {
                PluginFunctions.arwSet2DMaxMarkersToTrack(currentTwoDMaxMarkersToTrack);
            }
        }
    }

    public AR_LOG_LEVEL LogLevel
    {
        get
        {
            return currentLogLevel;
        }

        set
        {
            currentLogLevel = value;
            PluginFunctions.arwSetLogLevel((int)currentLogLevel);
        }
    }

    public bool SquareMatrixModeAutocreateNewTrackables
    {
        get => currentSquareMatrixModeAutocreateNewTrackables;
        set
        {
            currentSquareMatrixModeAutocreateNewTrackables = value;
            if (_running)
            {
                PluginFunctions.arwSetSquareMatrixModeAutocreateNewTrackables(value, currentSquareMatrixModeAutocreateNewTrackablesDefaultWidth, ARXTrackable.OnTrackableEvent);
            }
        }
    }

    public float SquareMatrixModeAutocreateNewTrackablesDefaultWidth
    {
        get => currentSquareMatrixModeAutocreateNewTrackablesDefaultWidth;
        set
        {
            currentSquareMatrixModeAutocreateNewTrackablesDefaultWidth = value;
            if (_running)
            {
                PluginFunctions.arwSetSquareMatrixModeAutocreateNewTrackables(currentSquareMatrixModeAutocreateNewTrackables, value, ARXTrackable.OnTrackableEvent);
            }
        }
    }

    #endregion Tracker configuration.

    //
    // Internal methods.
    //

    [AOT.MonoPInvokeCallback(typeof(PluginFunctionsLogCallback))]
    public static void Log(string msg)
    {
        if (msg.StartsWith("[error]")) Log(AR_LOG_LEVEL.AR_LOG_LEVEL_ERROR, msg);
        else if (msg.StartsWith("[warning]")) Log(AR_LOG_LEVEL.AR_LOG_LEVEL_WARN, msg);
        else Log(AR_LOG_LEVEL.AR_LOG_LEVEL_INFO, msg);
    }
    public static void LogError(string msg, UnityEngine.Object sender = null)
    {
        Log(AR_LOG_LEVEL.AR_LOG_LEVEL_ERROR, msg, sender);
    }
    public static void LogWarning(string msg, UnityEngine.Object sender = null)
    {
        Log(AR_LOG_LEVEL.AR_LOG_LEVEL_WARN, msg, sender);
    }
    public static void LogInfo(string msg, UnityEngine.Object sender = null)
    {
        Log(AR_LOG_LEVEL.AR_LOG_LEVEL_INFO, msg, sender);
    }
    public static void Log(AR_LOG_LEVEL logLevel, string msg, UnityEngine.Object sender = null)
    {
        // Add the new log message to the collection. If the collection has grown too large
        // then remove the oldest messages.
        if (Debug.isDebugBuild)
        {
            logMessages.Add(msg);
            while (logMessages.Count > MaximumLogMessages) logMessages.RemoveAt(0);
        }
        switch (logLevel)
        {
            case AR_LOG_LEVEL.AR_LOG_LEVEL_ERROR:
                Debug.LogError(msg, sender);
                break;
            case AR_LOG_LEVEL.AR_LOG_LEVEL_WARN:
                Debug.LogWarning(msg, sender);
                break;
            default:
                Debug.Log(msg, sender);
                break;
        }
    }

    private void CalculateFPS()
    {
        if (timeCounter < refreshTime)
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            lastFramerate = (float)frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }

    #region Debug GUI
    // ------------------------------------------------------------------------------------
    // GUI Methods
    // ------------------------------------------------------------------------------------

    private GUIStyle[] style = new GUIStyle[3];
    private bool guiSetup = false;

    private void SetupGUI()
    {

        style[0] = new GUIStyle(GUI.skin.label);
        style[0].normal.textColor = new Color(0, 0, 0, 1);

        style[1] = new GUIStyle(GUI.skin.label);
        style[1].normal.textColor = new Color(0.0f, 0.5f, 0.0f, 1);

        style[2] = new GUIStyle(GUI.skin.label);
        style[2].normal.textColor = new Color(0.5f, 0.0f, 0.0f, 1);

        guiSetup = true;
    }

    private bool showGUIErrorDialog = false;
    private string showGUIErrorDialogContent = "";
    private Rect showGUIErrorDialogWinRect = new Rect(0.0f, 0.0f, 320.0f, 240.0f);

    private bool showGUIDebug = false;
    private bool showGUIDebugInfo = true;
    private bool showGUIDebugLogConsole = false;

    void OnGUI()
    {
        if (!guiSetup) SetupGUI();

        if (showGUIErrorDialog)
        {
            showGUIErrorDialogWinRect = GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");
            showGUIErrorDialogWinRect.x = ((float)Screen.width - showGUIErrorDialogWinRect.width) * 0.5f;
            showGUIErrorDialogWinRect.y = ((float)Screen.height - showGUIErrorDialogWinRect.height) * 0.5f;
            GUILayout.Window(0, showGUIErrorDialogWinRect, DrawErrorDialog, "Error");
        }

        if (showGUIDebug)
        {
            if (GUI.Button(new Rect(570, 250, 150, 50), "Info")) showGUIDebugInfo = !showGUIDebugInfo;
            if (showGUIDebugInfo) DrawInfoGUI();

            if (Debug.isDebugBuild) {
                if (GUI.Button(new Rect(570, 320, 150, 50), "Log")) showGUIDebugLogConsole = !showGUIDebugLogConsole;
                if (showGUIDebugLogConsole) DrawLogConsole();
            }

            //if (GUI.Button(new Rect(570, 390, 150, 50), "Content mode: " + ARXCamera.ContentModeNames[ARXCamera.CameraContentMode])) ARXCamera.CycleContentMode();
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) {
            if (GUI.Button(new Rect(400, 250, 150, 50), "Camera preferences")) {
                if (androidPlugin != null)
                    androidPlugin.Call("launchSettings");
            }
        }
#endif
            //if (GUI.Button(new Rect(400, 320, 150, 50), "Video background: " + UseVideoBackground)) UseVideoBackground = !UseVideoBackground;
            if (GUI.Button(new Rect(400, 390, 150, 50), "Debug mode: " + DebugVideo)) DebugVideo = !DebugVideo;

            ARToolKitThresholdMode currentThresholdMode = VideoThresholdMode;
            GUI.Label(new Rect(400, 460, 320, 25), "Threshold Mode: " + currentThresholdMode);
            if (currentThresholdMode == ARToolKitThresholdMode.Manual)
            {
                float currentThreshold = VideoThreshold;
                float newThreshold = GUI.HorizontalSlider(new Rect(400, 495, 270, 25), currentThreshold, 0, 255);
                if (newThreshold != currentThreshold)
                {
                    VideoThreshold = (int)newThreshold;
                }
                GUI.Label(new Rect(680, 495, 50, 25), VideoThreshold.ToString());
            }

            GUI.Label(new Rect(700, 20, 100, 25), "FPS: " + lastFramerate);
        }
    }


    private void DrawErrorDialog(int winID)
    {
        GUILayout.BeginVertical();
        GUILayout.Label(showGUIErrorDialogContent);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("OK"))
        {
            showGUIErrorDialog = false;
        }
        GUILayout.EndVertical();
    }

    //    private bool toggle = false;

    private void DrawInfoGUI()
    {
        // Basic ARToolKit information
        GUI.Label(new Rect(10, 10, 500, 25), "artoolkitX " + Version);
        //GUI.Label(new Rect(10, 30, 500, 25), "Video " + _videoWidth0 + "x" + _videoHeight0 + "@" + _videoPixelSize0 + "Bpp (" + _videoPixelFormatString0 + ")");

        // Some system information
        GUI.Label(new Rect(10, 90, 500, 25), "Graphics device: " + SystemInfo.graphicsDeviceName);
        GUI.Label(new Rect(10, 110, 500, 25), "Operating system: " + SystemInfo.operatingSystem);
        GUI.Label(new Rect(10, 130, 500, 25), "Processors: (" + SystemInfo.processorCount + "x) " + SystemInfo.processorType);
        GUI.Label(new Rect(10, 150, 500, 25), "Memory: " + SystemInfo.systemMemorySize + "MB");

#if UNITY_2022_OR_NEWER
        GUI.Label(new Rect(10, 170, 500, 25), "Resolution : " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRateRatio.value + "Hz");
#else
        GUI.Label(new Rect(10, 170, 500, 25), "Resolution : " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRate + "Hz");
#endif
        GUI.Label(new Rect(10, 190, 500, 25), "Screen : " + Screen.width + "x" + Screen.height);
        //GUI.Label(new Rect(10, 210, 500, 25), "Viewport : " + _videoBackgroundCamera0.pixelRect.xMin + "," + _videoBackgroundCamera0.pixelRect.yMin + ", " + _videoBackgroundCamera0.pixelRect.xMax + ", " + _videoBackgroundCamera0.pixelRect.yMax);
        //GUI.Label(new Rect(10, 250, 800, 100), "Base Data Path : " + BaseDataPath);


        int y = 350;

        ARXTrackable[] trackables = Component.FindObjectsOfType(typeof(ARXTrackable)) as ARXTrackable[];
        foreach (ARXTrackable m in trackables)
        {
            GUI.Label(new Rect(10, y, 500, 25), "Marker: " + m.UID + ", " + m.Visible);
            y += 25;
        }
    }


    public Vector2 scrollPosition = Vector2.zero;

    private void DrawLogConsole()
    {
        Rect consoleRect = new Rect(0, 0, Screen.width, 200);

        GUIStyle bs = new GUIStyle(GUI.skin.box);
        bs.normal.background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        bs.normal.background.SetPixel(0, 0, new Color(1, 1, 1, 1));
        bs.normal.background.Apply();

        GUI.Box(consoleRect, "", bs);

        //int numItems = logMessages.Count;

        Rect scrollViewRect = new Rect(5, 5, consoleRect.width - 10, consoleRect.height - 10);

        float height = 0;
        float width = scrollViewRect.width - 30;

        foreach (String s in logMessages)
        {
            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            height += h;
        }

        Rect contentRect = new Rect(0, 0, width, height);

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);

        float y = 0;

        IEnumerable<string> lm = logMessages;

        int i = 0;

        foreach (String s in lm)
        {
            i = 0;
            if (s.StartsWith(LogTag)) i = 1;
            else if (s.StartsWith("[")) i = 2;

            float h = GUI.skin.label.CalcHeight(new GUIContent(s), width);
            GUI.Label(new Rect(0, y, width, h), s, style[i]);

            y += h;
        }

        GUI.EndScrollView();
    }

}
#endregion Debug GUI
