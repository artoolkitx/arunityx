package org.artoolkitx.arx.unity;

/*
 *  ARUnityXPlugin.java
 *  artoolkitX
 *
 *  This file is part of artoolkitX.
 *
 *  artoolkitX is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  artoolkitX is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with artoolkitX.  If not, see <http://www.gnu.org/licenses/>.
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
 *  Copyright 2015-2016 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Thorsten Bux, Philip Lamb
 *
 */

import android.content.res.Configuration;

/**
 * The intention of this interface is to contain all functions that Unity3D calls to instruct
 * the Android plugin
 * See ARController.cs#Awake() function in ARUnity6 plugin
 */
interface ARUnityXPlugin {

    static final String GRANTED_MESSAGE = "OnCameraPermissionGranted";
    static final String DENIED_MESSAGE = "OnCameraPermissionDenied";
    static final String ORIENTATIONCHANGE_MESSAGE = "OnDeviceOrientationChanged";
    static final int CAMERA_PERMISSION_REQUEST = 1; //arbitrary number

    /**
     * Gets the number of the Android (SDK) version running on the current device.
     *
     * @return number of the Android version (e.g., 21 for Android 5.0)
     */
    int getAndroidVersion();

    void launchSettings();

    void setStereo(boolean stereo);


    /**
     * Check wether Unity3D is already up and running
     *
     * @return true if Unity3D isUp flag is already set
     */
    boolean isUnityRunning();

    /**
     * Set a flag from Unity3D that indicates that Unity has been started
     *
     * @param running true if Unity3D has been started otherwise false
     */
    void setUnityRunning(boolean running);

    /**
     * This is the place where the listener added in {@link #setConfigurationChangeListener(String)}
     * is called with the new configuration
     * @param config
     */
    void onConfigurationChanged(Configuration config);

    /**
     * Unity3D can add a listener which will be informed about configuration changes. (Basically
     * screen orientation changes)
     * @param listener name of the Unity3D object which will be the listener
     */
    void setConfigurationChangeListener(String listener);
}