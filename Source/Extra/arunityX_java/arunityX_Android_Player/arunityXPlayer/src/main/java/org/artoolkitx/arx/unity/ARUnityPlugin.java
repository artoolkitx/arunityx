package org.artoolkitx.arx.unity;

/*
 *  ARUnityPlugin.java
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
import android.support.annotation.NonNull;

/**
 * The intention of this interface is to contain all functions that Unity3D calls to instruct
 * the Android plugin
 * See ARController.cs#Awake() function in ARUnity6 plugin
 */
interface ARUnityPlugin {

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

    void launchPreferencesActivity();

    void setStereo(boolean stereo);

    void registerForOrientationNotifications(String notificationObject);

    /**
     * Determines whether or not camera permissions are already granted to the application.
     *
     * @return true if camera permissions have been granted, false otherwise
     */
    boolean hasCameraPermissions();

    /**
     * Requests camera permissions if not already granted to the application.
     * Sends an "OnCameraPermissionGranted" message to Unity when/if the user grants camera permissions.
     * Sends an "OnCameraPermissionDenied" message to Unity when/if the user denies camera permissions.
     *
     * @param notificationObject String name of the Unity object to receive permission notifications
     * @return true if camera permissions are already granted, false otherwise
     */
    boolean checkCameraPermissions(String notificationObject);

    /**
     * Explicitly requests camera permissions for the application.
     * Sends an "OnCameraPermissionGranted" message to Unity when/if the user grants camera permissions.
     * Sends an "OnCameraPermissionDenied" message to Unity when/if the user denies camera permissions.
     *
     * @param notificationObject String name of the Unity object to receive permission notifications
     */
    void requestCameraPermissions(String notificationObject);

    /**
     * Determines whether an explanation of camera permissions should be shown to the user.
     * An explanation should be shown if permissions have been previously denied, the user has not
     * checked the 'Don't ask again' box, and device policies allow the user to grant permissions.
     *
     * @return true if an explanation should be displayed, false otherwise
     */
    boolean shouldExplainCameraPermissions();


    void onConfigurationChanged(Configuration configuration);

    void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults);

    /**
     * Check wether Unity3D is already up and running
     *
     * @return true if Unity3D isUp flag is already set
     */
    boolean isUnityUp();

    /**
     * Set the Unity3D flag that reflects that Unity3D has been started
     *
     * @param up true if Unity3D is running, false if not
     */
    void unityIsUp(boolean up);
}