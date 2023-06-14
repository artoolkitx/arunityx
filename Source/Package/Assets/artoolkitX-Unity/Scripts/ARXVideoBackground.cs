using UnityEngine;
using System;

[RequireComponent(typeof(ARCamera))]
public class ARXVideoBackground : MonoBehaviour
{
    private const string LogTag = "ARXVideoBackground: ";

    public int BackgroundLayer = 8;

    [SerializeField]
    private bool currentUseVideoBackground = true;
    public bool UseVideoBackground
    {
        get => currentUseVideoBackground;
        set
        {
            currentUseVideoBackground = value;
            if (currentUseVideoBackground)
            {
                if (cam.clearFlags == CameraClearFlags.SolidColor)
                {
                    cam.clearFlags = CameraClearFlags.Depth;
                }
                if (_videoBackgroundCamera != null) {
                    _videoBackgroundCamera.enabled = true;
                }
            }
            else
            {
                if (cam.clearFlags == CameraClearFlags.Depth)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                }
                if (_videoBackgroundCamera != null) {
                    _videoBackgroundCamera.enabled = false;
                }
            }
        }
    }

    private ARController arController;
    private ARCamera arCamera;
    private Camera cam;
    private bool _cameraStereoRightEye = false;
    private int _videoWidth;
    private int _videoHeight;
    private int _videoPixelSize;
    private string _videoPixelFormatString;
    private Matrix4x4 _videoProjectionMatrix;
    private GameObject _videoBackgroundMeshGO = null; // The GameObject which holds the MeshFilter and MeshRenderer for the background video, and also the Camera object(s) used to render them. 
    private Color32[] _videoColor32Array = null; // An array used to fetch pixels from the native side, only if not using native GL texturing.
    private Texture2D _videoTexture = null;  // Texture object with the video image.
    private Material _videoMaterial = null;  // Material which uses our "VideoPlaneNoLight" shader, and paints itself with _videoTexture.
    private GameObject _videoBackgroundCameraGO = null; // The GameObject which holds the Camera object for the mono / stereo left-eye video background.
    private Camera _videoBackgroundCamera = null; // The Camera component attached to _videoBackgroundCameraGO0. Easier to keep this reference than calling _videoBackgroundCameraGO0.GetComponent<Camera>() each time.

    void OnEnable()
    {
        arCamera = gameObject.GetComponent<ARCamera>();
        cam = gameObject.GetComponent<Camera>();
        arController = ARController.Instance;
        arController.onVideoStarted.AddListener(OnVideoStarted);
        arController.onVideoStopped.AddListener(OnVideoStopped);
        arController.onVideoFrame.AddListener(OnVideoFrame);
        arController.onScreenGeometryChanged.AddListener(OnScreenGeometryChanged);
    }

    void OnDisable()
    {
        arController.onVideoStarted.RemoveListener(OnVideoStarted);
        arController.onVideoStopped.RemoveListener(OnVideoStopped);
        arController.onVideoFrame.RemoveListener(OnVideoFrame);
        arController.onScreenGeometryChanged.RemoveListener(OnScreenGeometryChanged);
        arController = null;
        arCamera = null;
        cam = null;
    }

    public void OnVideoStarted()
    {
        // Get information on the video stream.
        string nameSuffix = arCamera.Stereo ? (arCamera.StereoEye == ARCamera.ViewEye.Left ? " (L)" : " (R)") : "";
        _cameraStereoRightEye = arCamera.Stereo && arCamera.StereoEye == ARCamera.ViewEye.Right;
        if (!_cameraStereoRightEye || !arController.VideoIsStereo)
        {
            if (!arController.PluginFunctions.arwGetVideoParams(out _videoWidth, out _videoHeight, out _videoPixelSize, out _videoPixelFormatString)) return;
        }
        else
        {
            if (!arController.PluginFunctions.arwGetVideoParamsStereo(out _, out _, out _, out _, out _videoWidth, out _videoHeight, out _videoPixelSize, out _videoPixelFormatString)) return;
        }

        // Create a game object on which to draw video texture, and a camera to observe it.
        // If there is only one video source, and we're the second (right) eye, we won't create or
        // update the video texture but instead just observe the one we assume has been created by the
        // left eye.
        if (!arController.VideoIsStereo && _cameraStereoRightEye)
        {
            _videoTexture = null;
            _videoMaterial = null;
            _videoBackgroundMeshGO = null;
            _videoColor32Array = null;
        }
        else
        {
            // Create a game object on which to draw the video.
            // Invert flipV for texture because artoolkitX video frame is top-down, Unity's is bottom-up.
            string name = "Video source" + (arController.VideoIsStereo ? nameSuffix : "");
            ARController.Log(LogTag + name + " size " + _videoWidth + "x" + _videoHeight + "@" + _videoPixelSize + "Bpp (" + _videoPixelFormatString + ")");

            _videoBackgroundMeshGO = ARUtilityFunctions.CreateVideoObject(name, _videoWidth, _videoHeight, 1000.0f, arCamera.ContentFlipH, !arCamera.ContentFlipV, BackgroundLayer, out _videoTexture, out _videoMaterial);  // 1000.0f is arbitrary distance, since we'll observe with camera using orthogonal projection. Just needs to be between near and far.
            if (_videoBackgroundMeshGO == null || _videoTexture == null || _videoMaterial == null)
            {
                ARController.Log(LogTag + "Error: unable to create video mesh.");
            }
            _videoColor32Array = /*_useNativeGLTexturing ? null : */ new Color32[_videoWidth * _videoHeight];
        }

        // Create an orthographic camera to observe it.
        // Create new GameObject to hold camera.
        _videoBackgroundCameraGO = new GameObject($"Video background{nameSuffix}");
        if (_videoBackgroundCameraGO == null)
        {
            ARController.Log(LogTag + "Error: CreateVideoBackgroundCamera cannot create GameObject.");
            return;
        }
        _videoBackgroundCamera = _videoBackgroundCameraGO.AddComponent<Camera>();
        if (_videoBackgroundCamera == null)
        {
            ARController.Log(LogTag + "Error: CreateVideoBackgroundCamera cannot add Camera to GameObject.");
            Destroy(_videoBackgroundCameraGO);
            return;
        }
        // Camera at origin.
        _videoBackgroundCamera.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        _videoBackgroundCamera.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        _videoBackgroundCamera.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        // Rendering settings.
        _videoBackgroundCamera.cullingMask = 1 << BackgroundLayer; // The background camera displays only the chosen background layer.
        _videoBackgroundCamera.depth = cam.depth - 1; // Render before foreground cameras.
        _videoBackgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        _videoBackgroundCamera.backgroundColor = Color.black;

        // If video background isn't actually wanted, disable the camera.
        // This step also decides whether to clear the main camera background or not.
        UseVideoBackground = currentUseVideoBackground;

        UpdateVideoBackgroundProjection();
    }

    private void UpdateVideoBackgroundProjection()
    {
        // Get screen size. If in a portrait mode, swap w/h.
        float w = cam.pixelWidth;
        float h = cam.pixelHeight;
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        bool swapWH = Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown;
        if (swapWH)
        {
            w = cam.pixelHeight;
            h = cam.pixelWidth;
        }
#endif

        // Work out cropping for content mode.
        if (arCamera.CameraContentMode != ARCamera.ContentMode.OneToOne)
        {
            int videoWidthFinalOrientation = (arCamera.ContentRotate90 ? _videoHeight : _videoWidth);
            int videoHeightFinalOrientation = (arCamera.ContentRotate90 ? _videoWidth : _videoHeight);
            if (arCamera.CameraContentMode == ARCamera.ContentMode.Fit || arCamera.CameraContentMode == ARCamera.ContentMode.Fill)
            {
                float scaleRatioWidth = w / (float)videoWidthFinalOrientation;
                float scaleRatioHeight = h / (float)videoHeightFinalOrientation;
                float scaleRatio = arCamera.CameraContentMode == ARCamera.ContentMode.Fill ? Math.Max(scaleRatioHeight, scaleRatioWidth) : Math.Min(scaleRatioHeight, scaleRatioWidth);
                w /= scaleRatio;
                h /= scaleRatio;
            }
            else
            { // Stretch
                w = videoWidthFinalOrientation;
                h = videoHeightFinalOrientation;
            }
        }
        // TODO: also work out content alignment offsets.

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        if (swapWH)
        {
            float temp = w;
            w = h;
            h = temp;
        }
#endif
        _videoBackgroundCamera.pixelRect = cam.pixelRect;
        _videoBackgroundCamera.orthographic = true;
        _videoBackgroundCamera.projectionMatrix = Matrix4x4.identity;
        if (arCamera.ContentRotate90) _videoBackgroundCamera.projectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90.0f, Vector3.back), Vector3.one) * _videoBackgroundCamera.projectionMatrix;
        _videoBackgroundCamera.projectionMatrix = Matrix4x4.Ortho(-w*0.5f, w*0.5f, -h*0.5f, h*0.5f, 0.0f, 2000.0f) * _videoBackgroundCamera.projectionMatrix;
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                _videoBackgroundCameraGO.transform.localRotation = Quaternion.AngleAxis(-90.0f, Vector3.back);
                break;
            case ScreenOrientation.PortraitUpsideDown:
                _videoBackgroundCameraGO.transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.back);
                break;
            case ScreenOrientation.LandscapeRight:
                _videoBackgroundCameraGO.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.back);
                break;
            case ScreenOrientation.LandscapeLeft:
            default:
                _videoBackgroundCameraGO.transform.localRotation = Quaternion.identity;
                break;
        }
#endif
    }

    public void OnVideoStopped()
    {
         bool ed = Application.isEditor;

        _videoBackgroundCamera = null;
        if (_videoBackgroundCameraGO != null)
        {
            if (ed) DestroyImmediate(_videoBackgroundCameraGO);
            else Destroy(_videoBackgroundCameraGO);
            _videoBackgroundCameraGO = null;
        }
        _videoMaterial = null;
        _videoTexture = null;
        _videoColor32Array = null;
        if (_videoBackgroundMeshGO != null)
        {
            if (ed) DestroyImmediate(_videoBackgroundMeshGO);
            else Destroy(_videoBackgroundMeshGO);
            _videoBackgroundMeshGO = null;
        }
        Resources.UnloadUnusedAssets();
    }

    public void OnVideoFrame()
    {
        if (_videoTexture != null)
        {
            if (_videoColor32Array != null)
            {
                bool updatedTexture;
                if (!_cameraStereoRightEye)
                {
                    updatedTexture = arController.PluginFunctions.arwUpdateTexture32(_videoColor32Array);
                }
                else
                {
                    updatedTexture = arController.PluginFunctions.arwUpdateTexture32Stereo(null, _videoColor32Array);
                }
                if (updatedTexture)
                {
                    _videoTexture.SetPixels32(_videoColor32Array);
                    _videoTexture.Apply(false);
                }
            }
            else
            {
                ARController.Log(LogTag + "Error: No video color array to update.");
            }
        }
    }

    public void OnScreenGeometryChanged()
    {
        UpdateVideoBackgroundProjection();
    }


    public float VideoAlpha 
    {
        get {
            if (_videoMaterial == null) return 1.0f;
            return _videoMaterial.color.a;
        }
        set {
            if (_videoMaterial != null)
            {
                Color c = _videoMaterial.color;
                c.a = Math.Clamp(value, 0.0f, 1.0f);
                _videoMaterial.color = c;
            }
        }
    }
}
