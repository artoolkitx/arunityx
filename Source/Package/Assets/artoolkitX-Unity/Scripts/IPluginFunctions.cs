using System.Runtime.InteropServices;
using UnityEngine;

public interface IPluginFunctions
{
    bool inited { get; set; }

    int arwAddMarker(string cfg);
    bool arwCapture();
    string arwGetARToolKitVersion();
    float arwGetBorderSize();
    int arwGetError();
    int arwGetImageProcMode();
    int arwGetLabelingMode();
    bool arwGetMarkerOptionBool(int markerID, int option);
    float arwGetMarkerOptionFloat(int markerID, int option);
    int arwGetMarkerOptionInt(int markerID, int option);
    bool arwGetMarkerPatternConfig(int markerID, int patternID, float[] matrix, out float width, out float height, out int imageSizeX, out int imageSizeY);
    int arwGetMarkerPatternCount(int markerID);
    bool arwGetMarkerPatternImage(int markerID, int patternID, [In, Out] Color[] colors);
    int arwGetMatrixCodeType();
    bool arwGetNFTMultiMode();
    int arwGetPatternDetectionMode();
    bool arwGetProjectionMatrix(float nearPlane, float farPlane, float[] matrix);
    bool arwGetProjectionMatrixStereo(float nearPlane, float farPlane, float[] matrixL, float[] matrixR);
    bool arwGetVideoDebugMode();
    bool arwGetVideoParams(out int width, out int height, out int pixelSize, out string pixelFormatString);
    bool arwGetVideoParamsStereo(out int widthL, out int heightL, out int pixelSizeL, out string pixelFormatL, out int widthR, out int heightR, out int pixelSizeR, out string pixelFormatR);
    int arwGetVideoThreshold();
    int arwGetVideoThresholdMode();
    bool arwInitialiseAR(int pattSize = 16, int pattCountMax = 25);
    bool arwIsRunning();
    bool arwLoadOpticalParams(string optical_param_name, byte[] optical_param_buff, int optical_param_buffLen, float projectionNearPlane, float projectionFarPlane, out float fovy_p, out float aspect_p, float[] m, float[] p);
    bool arwQueryMarkerVisibilityAndTransformation(int markerID, float[] matrix);
    bool arwQueryMarkerVisibilityAndTransformationStereo(int markerID, float[] matrixL, float[] matrixR);
    void arwRegisterLogCallback(DefaultPluginFunctions.LogCallback lcb);
    int arwRemoveAllMarkers();
    bool arwRemoveMarker(int markerID);
    void arwSetBorderSize(float size);
    void arwSetImageProcMode(int mode);
    void arwSetLabelingMode(int mode);
    void arwSetLogLevel(int logLevel);
    void arwSetMarkerOptionBool(int markerID, int option, bool value);
    void arwSetMarkerOptionFloat(int markerID, int option, float value);
    void arwSetMarkerOptionInt(int markerID, int option, int value);
    void arwSetMatrixCodeType(int type);
    void arwSetNFTMultiMode(bool on);
    void arwSetPatternCountMax(int count);
    void arwSetPatternDetectionMode(int mode);
    void arwSetPatternSize(int size);
    void arwSetVideoDebugMode(bool debug);
    void arwSetVideoThreshold(int threshold);
    void arwSetVideoThresholdMode(int mode);
    bool arwShutdownAR();
    bool arwStartRunningB(string vconf, byte[] cparaBuff, int cparaBuffLen);
    bool arwStartRunningStereoB(string vconfL, byte[] cparaBuffL, int cparaBuffLenL, string vconfR, byte[] cparaBuffR, int cparaBuffLenR, byte[] transL2RBuff, int transL2RBuffLen);
    bool arwStopRunning();
    bool arwUpdateAR();
    bool arwUpdateTexture32([In, Out] Color32[] colors32);
    bool arwUpdateTexture32Stereo([In, Out] Color32[] colors32L, [In, Out] Color32[] colors32R);
}