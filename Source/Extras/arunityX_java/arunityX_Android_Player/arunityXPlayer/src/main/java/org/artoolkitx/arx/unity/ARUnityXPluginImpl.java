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
 *  ARUnityXPluginImpl.java
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
public final class ARUnityXPluginImpl implements ARUnityXPlugin {

    private static final String TAG = ARUnityXPluginImpl.class.getSimpleName();
    private final Activity mActivity;
    private OrientationChangeListener mOrientationChangeListener = null;
    private String mCameraPermissionNotificationObject;


    private boolean mUnityRunning = false;

    public ARUnityXPluginImpl(Activity activity) {
        Log.i(TAG, "ARUnityXPluginImpl constructor called with Activity: " + activity);
        this.mActivity = activity;
    }

    @Override
    public int getAndroidVersion() {
        return Build.VERSION.SDK_INT;
    }

    @Override
    public void launchSettings() {
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
    public boolean isUnityRunning() {
        return mUnityRunning;
    }

    @Override
    public void setUnityRunning(boolean running) {
        this.mUnityRunning = running;
    }

    @Override
    public void logUnityMessage(String message) {
        Log.d(TAG,message);
    }
    
}
