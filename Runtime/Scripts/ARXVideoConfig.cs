/*
 *  ARXVideoConfig.cs
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
 *  Copyright 2023 Philip Lamb
 *
 *  Author(s): Philip Lamb
 *
 */

using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(ARXController))]
public class ARXVideoConfig : MonoBehaviour
{
    public ARXController arcontroller;
    private const string LogTag = "ARXVideoConfig: ";

    private void OnEnable()
    {
        arcontroller = gameObject.GetComponent<ARXController>();
    }

    /// <summary>
    /// Video input modules.
    /// </summary>
    [Serializable]
    public enum ARVideoModule
    {
        Dummy,
        Image,
        V4L2,
        GStreamer,
        _1394,
        AVFoundation,
        Android,
        WinMF,
        External,
    }

    /// <summary>
    /// Unity video input sources.
    /// </summary>
    [Serializable]
    public enum ARVideoUnityVideoSource
    {
        None = 0,
        WebcamTexture,
    }

    /// <summary>
    /// Strategies by which a platform can select which input to use.
    /// </summary>
    [Serializable]
    public enum ARVideoConfigInputSelectionMethod
    {
        AnyCamera,
        NthCamera,
        CameraAtPosition,
        NthCameraAtPosition,
        VideoSourceInfoList,
    }

    /// <summary>
    /// Strategies by which modules allow selection of frame sizes.
    /// </summary>
    [Serializable]
    public enum ARVideoSizeSelectionStrategy
    {
        None,
        WidthAndHeight,
        AVFoundationPreset,
        SizePreference,
    }

    /// <summary>
    /// AVFoundation presets.
    /// </summary>
    [Serializable]
    public enum ARVideoSizeSelectionStrategyAVFoundationPreset
    {
        medium,
        low,
        high,
        photo,
        cif,
        qvga,
        vga,
        _540p,
        _720p,
        _1080p,
        _2160p,
    }

    /// <summary>
    /// Size preference strategies.
    /// </summary>
    [Serializable]
    public enum ARVideoSizeSelectionStrategySizePreference
    {
        any,
        exact,
        closestsameaspect,
        closestpixelcount,
        sameaspect,
        largestwithmaximum,
        smallestwithminimum,
        largest,
        smallest,
    }

    //
    // Define the video module capabilities.
    //

    public struct ARVideoModuleInfo
    {
        public string moduleSelectionString;
        public RuntimePlatform[] runtimePlatforms;
        public bool supportsSelectionByPosition;
        public string positionSelectionString;
        public bool supportsSelectionByIndex;
        public string indexSelectionString;
        public bool indexSelectionIs1Indexed;
        public ARVideoSizeSelectionStrategy sizeSelectionStrategy;
    }

    public static readonly Dictionary<ARVideoModule, ARVideoModuleInfo> modules = new Dictionary<ARVideoModule, ARVideoModuleInfo>()
    {
        {
            ARVideoModule.Dummy,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=Dummy",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.WindowsPlayer,
                    RuntimePlatform.OSXPlayer,
                    RuntimePlatform.Android,
                    RuntimePlatform.IPhonePlayer,
                    RuntimePlatform.LinuxPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = false,
                indexSelectionString = null,
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.WidthAndHeight,
            }
        },
        {
            ARVideoModule.Image,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=Image",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.WindowsPlayer,
                    RuntimePlatform.OSXPlayer,
                    RuntimePlatform.Android,
                    RuntimePlatform.IPhonePlayer,
                    RuntimePlatform.LinuxPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = false,
                indexSelectionString = null,
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.WidthAndHeight,
            }
        },
        {
            ARVideoModule.V4L2,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=V4L2",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.LinuxPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = true,
                indexSelectionString = "-dev=/dev/video",
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.WidthAndHeight,
            }
        },
        {
            ARVideoModule.GStreamer,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=GStreamer",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.LinuxPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = false,
                indexSelectionString = null,
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.None,
            }
        },
        {
            ARVideoModule._1394,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=1394",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.LinuxPlayer,
                    RuntimePlatform.OSXPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = false,
                indexSelectionString = null,
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.None,
            }
        },
        {
            ARVideoModule.AVFoundation,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=AVFoundation",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.IPhonePlayer,
                    RuntimePlatform.OSXPlayer,
                },
                supportsSelectionByPosition = true,
                positionSelectionString = "-position=",
                supportsSelectionByIndex = true,
                indexSelectionString = "-source=",
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.AVFoundationPreset,
            }
        },
        {
            ARVideoModule.Android,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=Android -native",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.Android,
                },
                supportsSelectionByPosition = true,
                positionSelectionString = "-position=",
                supportsSelectionByIndex = true,
                indexSelectionString = "-source=",
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.SizePreference,
            }
        },
        {
            ARVideoModule.WinMF,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=WinMF",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.WindowsPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = true,
                indexSelectionString = "-devNum=",
                indexSelectionIs1Indexed = true,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.WidthAndHeight,
            }
        },
        {
            ARVideoModule.External,
            new ARVideoModuleInfo {
                moduleSelectionString = "-module=External",
                runtimePlatforms = new RuntimePlatform[] {
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.WindowsPlayer,
                    RuntimePlatform.OSXPlayer,
                    RuntimePlatform.Android,
                    RuntimePlatform.IPhonePlayer,
                    RuntimePlatform.LinuxPlayer,
                },
                supportsSelectionByPosition = false,
                positionSelectionString = null,
                supportsSelectionByIndex = false,
                indexSelectionString = null,
                indexSelectionIs1Indexed = false,
                sizeSelectionStrategy = ARVideoSizeSelectionStrategy.None,
            }
        },
    };

    //
    // Define the properties of each AVFoundation preset.
    //

    public struct ARVideoSizeSelectionStrategyAVFoundationPresetInfo
    {
        public string config;
        public bool availableMac;
        public bool availableiOS;
        public string description;
    }

    public static readonly Dictionary<ARVideoSizeSelectionStrategyAVFoundationPreset, ARVideoSizeSelectionStrategyAVFoundationPresetInfo> AVFoundationPresets = new Dictionary<ARVideoSizeSelectionStrategyAVFoundationPreset, ARVideoSizeSelectionStrategyAVFoundationPresetInfo>()
    {
        {ARVideoSizeSelectionStrategyAVFoundationPreset.medium, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=medium", availableMac=true, availableiOS=true, description="Medium quality"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.low, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=low", availableMac=true, availableiOS=true, description="Low quality"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.high, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=high", availableMac=true, availableiOS=true, description="High quality"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.photo, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=medium", availableMac=true, availableiOS=true, description="Photo quality"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.cif, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=cif", availableMac=true, availableiOS=true, description="CIF (352x288)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.qvga, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=qvga", availableMac=true, availableiOS=false, description="QVGA (320x240)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset.vga, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=vga", availableMac=true, availableiOS=true, description="VGA (640x480)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset._540p, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=540p", availableMac=true, availableiOS=false, description="540p (960x540)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset._720p, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=720p", availableMac=true, availableiOS=true, description="720p (1280x720)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset._1080p, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=1080p", availableMac=true, availableiOS=true, description="1080p (1920x1080)"}},
        {ARVideoSizeSelectionStrategyAVFoundationPreset._2160p, new ARVideoSizeSelectionStrategyAVFoundationPresetInfo{config = "-preset=2160p", availableMac=true, availableiOS=true, description="2160p (3840x2160)"}},
    };

    //
    // Define the properties of each size preference.
    //

    public struct ARVideoSizeSelectionStrategySizePreferenceInfo
    {
        public string config;
        public bool usesWidthAndHeightFields;
        public string description;
    }

    public static readonly Dictionary<ARVideoSizeSelectionStrategySizePreference, ARVideoSizeSelectionStrategySizePreferenceInfo> SizePreferences = new Dictionary<ARVideoSizeSelectionStrategySizePreference, ARVideoSizeSelectionStrategySizePreferenceInfo>()
    {
        {ARVideoSizeSelectionStrategySizePreference.any, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=any", usesWidthAndHeightFields = false, description = "Any size" } },
        {ARVideoSizeSelectionStrategySizePreference.exact, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=exact", usesWidthAndHeightFields = true, description = "Exact size" } },
        {ARVideoSizeSelectionStrategySizePreference.closestsameaspect, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=closestsameaspect", usesWidthAndHeightFields = true, description = "Closest size (with same aspect ratio)" } },
        {ARVideoSizeSelectionStrategySizePreference.closestpixelcount, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=closestpixelcount", usesWidthAndHeightFields = true, description = "Closest size (by pixel count)" } },
        {ARVideoSizeSelectionStrategySizePreference.sameaspect, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=sameaspect", usesWidthAndHeightFields = true, description = "Size with aspect ratio" } },
        {ARVideoSizeSelectionStrategySizePreference.largestwithmaximum, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=largestwithmaximum", usesWidthAndHeightFields = true, description = "Largest size with maximum" } },
        {ARVideoSizeSelectionStrategySizePreference.smallestwithminimum, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=smallestwithminimum", usesWidthAndHeightFields = true, description = "Smallest size with minimum" } },
        {ARVideoSizeSelectionStrategySizePreference.largest, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=largest", usesWidthAndHeightFields = false, description = "Largest size" } },
        {ARVideoSizeSelectionStrategySizePreference.smallest, new ARVideoSizeSelectionStrategySizePreferenceInfo { config = "-prefer=smallest", usesWidthAndHeightFields = false, description = "Smallest size" } },
    };

    //
    // Put it all together: platform config.
    //
    [Serializable]
    public struct ARVideoPlatformConfig
    {
        public RuntimePlatform platform;
        public string name;
        public ARVideoModule defaultModule; // Ought to be readonly or init-only setter, but latter not supported by Unity C# compiler (see https://docs.unity3d.com/Manual/CSharpCompiler.html and https://stackoverflow.com/a/64749403/316487).
        public ARVideoModule module;
        public ARVideoConfigInputSelectionMethod inputSelectionMethod;
        public ARXController.AR_VIDEO_POSITION position;
        public int index;
        public string VideoSourceInfoListOpenToken;
        public int width;
        public int height;
        public ARVideoSizeSelectionStrategyAVFoundationPreset AVFoundationPreset;
        public ARVideoSizeSelectionStrategySizePreference sizePreference;
        public bool isUsingManualConfig;
        public string manualConfig;
        public List<ARVideoUnityVideoSource> supportedUnityVideoSources;
        public bool isUsingUnityVideoSource;
        public ARVideoUnityVideoSource unityVideoSource;
    }

    public List<ARVideoPlatformConfig> configs = new List<ARVideoPlatformConfig>()
    {
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.WindowsEditor,
            name = "Windows (Editor)",
            defaultModule = ARVideoModule.WinMF,
            module = ARVideoModule.WinMF,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.VideoSourceInfoList,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.OSXEditor,
            name = "mac OS (Editor)",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.VideoSourceInfoList,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingManualConfig = false,
            manualConfig = "",
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.WindowsPlayer,
            name = "Windows (Player)",
            defaultModule = ARVideoModule.WinMF,
            module = ARVideoModule.WinMF,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.AnyCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingManualConfig = false,
            manualConfig = "",
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.OSXPlayer,
            name = "mac OS (Player)",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.AnyCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.Android,
            name = "Android",
            defaultModule = ARVideoModule.Android,
            module = ARVideoModule.Android,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.AnyCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 1280,
            height = 720,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.closestpixelcount,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.IPhonePlayer,
            name = "iOS",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.AnyCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.LinuxPlayer,
            name = "Linux (Player)",
            defaultModule = ARVideoModule.V4L2,
            module = ARVideoModule.V4L2,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.AnyCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 0,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        }
    };

    public List<ARVideoPlatformConfig> configsForStereoSecondInput = new List<ARVideoPlatformConfig>()
    {
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.WindowsEditor,
            name = "Windows (Editor)",
            defaultModule = ARVideoModule.WinMF,
            module = ARVideoModule.WinMF,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.VideoSourceInfoList,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.OSXEditor,
            name = "mac OS (Editor)",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.VideoSourceInfoList,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.WindowsPlayer,
            name = "Windows (Player)",
            defaultModule = ARVideoModule.WinMF,
            module = ARVideoModule.WinMF,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.NthCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.OSXPlayer,
            name = "mac OS (Player)",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.NthCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.Android,
            name = "Android",
            defaultModule = ARVideoModule.Android,
            module = ARVideoModule.Android,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.NthCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 1280,
            height = 720,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.closestpixelcount,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.IPhonePlayer,
            name = "iOS",
            defaultModule = ARVideoModule.AVFoundation,
            module = ARVideoModule.AVFoundation,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.NthCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 0,
            height = 0,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        },
        new ARVideoPlatformConfig {
            platform = RuntimePlatform.LinuxPlayer,
            name = "Linux (Player)",
            defaultModule = ARVideoModule.V4L2,
            module = ARVideoModule.V4L2,
            inputSelectionMethod = ARVideoConfigInputSelectionMethod.NthCamera,
            position = ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_UNKNOWN,
            index = 1,
            VideoSourceInfoListOpenToken = "",
            width = 640,
            height = 480,
            AVFoundationPreset = ARVideoSizeSelectionStrategyAVFoundationPreset.medium,
            sizePreference = ARVideoSizeSelectionStrategySizePreference.any,
            isUsingManualConfig = false,
            manualConfig = "",
            supportedUnityVideoSources = new List<ARVideoUnityVideoSource> { ARVideoUnityVideoSource.None, ARVideoUnityVideoSource.WebcamTexture},
            isUsingUnityVideoSource = false,
            unityVideoSource = ARVideoUnityVideoSource.None,
        }
    };

    public List<ARXController.ARVideoSourceInfoT> sourceInfoList = null;

    private ARVideoPlatformConfig? GetPlatformConfig(RuntimePlatform platform, bool stereoSecondInput = false)
    {
        List<ARVideoPlatformConfig> cs = stereoSecondInput ? configsForStereoSecondInput : configs;
        int i = cs.FindIndex(c => c.platform == platform);
        if (i >= 0)
        {
            return cs[i];
        }
        return null;
    }

    public string GetVideoConfigStringForPlatform(RuntimePlatform platform, bool stereoSecondInput = false)
    {
        string config = "";
        ARVideoPlatformConfig? pcb = GetPlatformConfig(platform, stereoSecondInput);
        if (pcb.HasValue)
        {
            ARVideoPlatformConfig pc = pcb.Value;
            if (pc.isUsingManualConfig)
            {
                config = pc.manualConfig;
            }
            else
            {
                if (!pc.isUsingUnityVideoSource)
                {
                    config = modules[pc.module].moduleSelectionString;
                    switch (pc.inputSelectionMethod)
                    {
                        case ARVideoConfigInputSelectionMethod.CameraAtPosition:
                        case ARVideoConfigInputSelectionMethod.NthCameraAtPosition:
                            if (modules[pc.module].supportsSelectionByPosition)
                            {
                                string p = null;
                                if (pc.position == ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_BACK) p = "back";
                                else if (pc.position == ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_FRONT) p = "front";
                                else if (platform == RuntimePlatform.Android && pc.position == ARXController.AR_VIDEO_POSITION.AR_VIDEO_POSITION_OTHER) p = "external";
                                if (!string.IsNullOrEmpty(p))
                                {
                                    config += " " + modules[pc.module].positionSelectionString + p;
                                }
                            }
                            if (pc.inputSelectionMethod == ARVideoConfigInputSelectionMethod.NthCameraAtPosition) goto case ARVideoConfigInputSelectionMethod.NthCamera;
                            break;
                        case ARVideoConfigInputSelectionMethod.NthCamera:
                            if (modules[pc.module].supportsSelectionByIndex)
                            {
                                config += " " + modules[pc.module].indexSelectionString + (pc.index + (modules[pc.module].indexSelectionIs1Indexed ? 1 : 0)).ToString();
                            }
                            break;
                        case ARVideoConfigInputSelectionMethod.VideoSourceInfoList:
                            config += " " + pc.VideoSourceInfoListOpenToken;
                            break;
                        case ARVideoConfigInputSelectionMethod.AnyCamera:
                        default:
                            break;
                    }
                    switch (modules[pc.module].sizeSelectionStrategy)
                    {
                        case ARVideoSizeSelectionStrategy.AVFoundationPreset:
                            config += " " + AVFoundationPresets[pc.AVFoundationPreset].config;
                            break;
                        case ARVideoSizeSelectionStrategy.SizePreference:
                            if (!SizePreferences[pc.sizePreference].usesWidthAndHeightFields || (pc.width != 0 && pc.height != 0))
                            {
                                config += " " + SizePreferences[pc.sizePreference].config;
                                if (SizePreferences[pc.sizePreference].usesWidthAndHeightFields) goto case ARVideoSizeSelectionStrategy.WidthAndHeight;
                            }
                            break;
                        case ARVideoSizeSelectionStrategy.WidthAndHeight:
                            config += " -width=" + pc.width + " -height=" + pc.height;
                            break;
                        case ARVideoSizeSelectionStrategy.None:
                        default:
                            break;
                    }
                }
                else /* pc.isUsingUnityVideoSource */
                {
                    // All Unity video sources require external module.
                    config = modules[ARVideoModule.External].moduleSelectionString;
                }
            }
        }
        return config;
    }

    public string GetVideoConfigString(bool stereoSecondInput = false)
    {
        return GetVideoConfigStringForPlatform(Application.platform, stereoSecondInput);
    }
}
