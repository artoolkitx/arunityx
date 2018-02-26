package org.artoolkitx.arx.unity;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayerActivity;

import org.artoolkitx.arx.arxj.AndroidUtils;
import org.artoolkitx.arx.arxj.camera.CameraAccessHandler;
import org.artoolkitx.arx.arxj.camera.CameraEventListener;

/*
 *  ARUnityXPlayerActivity.java
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
 *  Copyright 2017-2018 Realmax, Inc.
 *  Copyright 2015-2016 Daqri, LLC.
 *  Copyright 2010-2015 ARToolworks, Inc.
 *
 *  Author(s): Thorsten Bux
 *
 */
public class ARUnityXPlayerActivity extends UnityPlayerActivity {

    protected final static String TAG = ARUnityXPlayerActivity.class.getSimpleName();

    private FrameLayout frameLayout = null;
    private ViewGroup unityView = null;

    private CameraAccessHandler mCameraAccessHandler;
    private ViewGroup decorView;
    private ARUnityXPlugin mArtoolkitXPlugin;

    @Override
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);

        // Loading the resource names for the settings screen. This is only needs to be done
        // one time and whenever new settings are added to arxj
        int resId = getResources().getIdentifier("preferences", "xml", getPackageName());
        PreferenceManager.setDefaultValues(this, resId, false);

        mArtoolkitXPlugin = new ARUnityXPluginImpl(this);
    }

    @Override
    protected void onResume() {
        Log.i(TAG, "onResume()");
        super.onResume();

        if (mArtoolkitXPlugin.isUnityRunning()) {
            startCamera();
        } else {
            Log.i(TAG, "onResume(): Unity is not running yet.");
        }
    }

    @Override
    protected void onPause() {
        Log.i(TAG, "onPause()");

        if (mCameraAccessHandler != null) {
            mCameraAccessHandler.closeCamera();
        }
        super.onPause();
    }

    public ARUnityXPlugin getArtoolkitXPlugin() {
        return this.mArtoolkitXPlugin;
    }

    /**
     * Android informs us that the device configuration has changed. This is called when the device
     * orientation changes for example
     * @param config object containing the new device settings
     */
    @Override
    public void onConfigurationChanged(Configuration config) {
        Log.i(TAG, "onConfigurationChanged()");
        super.onConfigurationChanged(config);
    }

    protected void startCamera() {

        final Activity activity = this;

        CameraEventListener cameraEventListener = new CameraEventListenerUnityImpl();
        mCameraAccessHandler = AndroidUtils.createCameraAccessHandler(activity, cameraEventListener);
        if (mCameraAccessHandler.getCameraAccessPermissions()) {
            return;
        }

        mCameraAccessHandler.getCameraSurfaceView().surfaceCreated();
        mCameraAccessHandler.getCameraSurfaceView().surfaceChanged();
    }
}
