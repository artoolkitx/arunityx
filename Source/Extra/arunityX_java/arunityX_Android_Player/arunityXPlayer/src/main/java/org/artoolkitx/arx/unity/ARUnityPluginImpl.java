package org.artoolkitx.arx.unity;

import android.Manifest;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.os.Build;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.view.Display;
import android.view.Surface;

import com.unity3d.player.UnityPlayer;

import org.artoolkitx.arx.arxj.camera.CameraPreferencesActivity;


/*
 *  ARUnityPluginImpl.java
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
public final class ARUnityPluginImpl implements ARUnityPlugin {

    private static final String TAG = ARUnityPluginImpl.class.getSimpleName();
    private final Activity mActivity;
    private String mOrientationNotificationObject = null;
    private String mCameraPermissionNotificationObject;

    private int mOrientation = Configuration.ORIENTATION_UNDEFINED;
    private int mRotation = Surface.ROTATION_0;
    private boolean mIsUnityUp = false;

    public ARUnityPluginImpl(Activity activity) {
        Log.i(TAG, "ARUnityPluginImpl constructor called with Activity: " + activity);
        this.mActivity = activity;
    }

    @Override
    public int getAndroidVersion() {
        return Build.VERSION.SDK_INT;
    }

    @Override
    public void launchPreferencesActivity() {
        mActivity.startActivity(new Intent(mActivity, CameraPreferencesActivity.class));
    }

    @Override
    public void setStereo(boolean stereo) {
        // For Epson Moverio BT-200, enable stereo mode.
//        if (Build.MANUFACTURER.equals("EPSON") && Build.MODEL.equals("embt2")) {
//            //int dimension = (stereo ? DIMENSION_3D : DIMENSION_2D);
//            //set2d3d(dimension);
//            mDisplayControl = new DisplayControl(mActivity);
//            mDisplayControl.setMode(stereo ? DisplayControl.DISPLAY_MODE_3D : DisplayControl.DISPLAY_MODE_2D, stereo); // Last parameter is 'toast'.
//        }
    }

    @Override
    public boolean hasCameraPermissions() {
        Log.i(TAG, "hasCameraPermissions()");
        int status = ContextCompat.checkSelfPermission(mActivity, Manifest.permission.CAMERA);
        return (status == PackageManager.PERMISSION_GRANTED);
    }

    @Override
    public boolean checkCameraPermissions(String notificationObject) {
        Log.i(TAG, "checkCameraPermissions()");

        boolean authorized = hasCameraPermissions();
        if (!authorized)
            requestCameraPermissions(notificationObject);

        return authorized;
    }

    @Override
    public void requestCameraPermissions(String notificationObject) {
        Log.i(TAG, "requestCameraPermissions()");

        if ((notificationObject != null) && !notificationObject.isEmpty())
            mCameraPermissionNotificationObject = notificationObject;
        else
            mCameraPermissionNotificationObject = null;

        final String[] permissions = new String[]{Manifest.permission.CAMERA};
        ActivityCompat.requestPermissions(mActivity, permissions, CAMERA_PERMISSION_REQUEST);

    }

    @Override
    public boolean shouldExplainCameraPermissions() {
        Log.i(TAG, "shouldExplainCameraPermissions()");

        //noinspection SimplifiableIfStatement
        if (hasCameraPermissions())
            return false;

        return ActivityCompat.shouldShowRequestPermissionRationale(mActivity, Manifest.permission.CAMERA);
    }

    public void onConfigurationChanged(Configuration newConfig) {
        if (mOrientationNotificationObject == null)
            return;

        int rotation = Surface.ROTATION_0;
        Display display = mActivity.getWindowManager().getDefaultDisplay();
        if (display != null)
            rotation = display.getRotation();

        if ((newConfig.orientation == mOrientation) && (rotation == mRotation))
            return;

        mOrientation = newConfig.orientation;
        mRotation = rotation;

        String value = "0";
        if (mOrientation == Configuration.ORIENTATION_PORTRAIT) {
            value = "1";
            if ((mRotation == Surface.ROTATION_180) || (mRotation == Surface.ROTATION_90))
                value = "2";
        } else if (mOrientation == Configuration.ORIENTATION_LANDSCAPE) {
            value = "3";
            if ((mRotation == Surface.ROTATION_180) || (mRotation == Surface.ROTATION_270))
                value = "4";
        }

        UnityPlayer.UnitySendMessage(mOrientationNotificationObject, ORIENTATIONCHANGE_MESSAGE, value);
    }

    /**
     * Registers a Unity object to receive "OnDeviceOrientationChanged" messages.
     *
     * @param object String name of the Unity object to receive device orientation notifications
     */
    public void registerForOrientationNotifications(String object) {
        Log.i(TAG, "registerForOrientationNotifications()");

        if ((object != null) && !object.isEmpty())
            mOrientationNotificationObject = object;
        else
            mOrientationNotificationObject = null;

    }

    /**
     * Callback indicating whether the user granted or denied (camera) permissions.
     *
     * @param requestCode  request code passed from requestCameraPermissions() [CAMERA_PERMISSION_REQUEST]
     * @param permissions  String array of requested permissions [Manifest.permission.CAMERA]
     * @param grantResults array of permission results, or empty if request cancelled
     */
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        Log.i(TAG, "onRequestPermissionsResult()");
        if ((requestCode != CAMERA_PERMISSION_REQUEST) || mCameraPermissionNotificationObject == null) {
            return;
        } else {
            // If request is cancelled, the result arrays are empty.
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                UnityPlayer.UnitySendMessage(mCameraPermissionNotificationObject, GRANTED_MESSAGE, GRANTED_MESSAGE);

                //Camera permission granted. If you NEED to show a toast, uncomment the line below.
                //Toast.makeText(this, "Camera Access Granted", Toast.LENGTH_SHORT).show();

            } else {
                UnityPlayer.UnitySendMessage(mCameraPermissionNotificationObject, DENIED_MESSAGE, DENIED_MESSAGE);
            }
        }
    }

    @Override
    public boolean isUnityUp() {
        return mIsUnityUp;
    }

    @Override
    public void unityIsUp(boolean up) {
        this.mIsUnityUp = up;
    }
}
