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
import org.artoolkitx.arx.arxj.camera.CameraSurface;

/*
 *  ARUnityPlayerActivity.java
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
public class ARUnityPlayerActivity extends UnityPlayerActivity {

    protected final static String TAG = ARUnityPlayerActivity.class.getSimpleName();

    private FrameLayout frameLayout = null;
    private ViewGroup unityView = null;

    private CameraAccessHandler mCameraAccessHandler;
    private ViewGroup decorView;
    private ARUnityPlugin mArtoolkitPlugin;

    @Override
    protected void onCreate(Bundle bundle) {
        super.onCreate(bundle);

        // This needs to be done just only the very first time the application is run,
        // or whenever a new preference is added (e.g. after an application upgrade).
        int resID = getResources().getIdentifier("preferences", "xml", getPackageName());
        PreferenceManager.setDefaultValues(this, resID, false);

        //Find the Unity view. We need to find the Unity view in order to remove it and add in the video preview.
        //After adding the video preview the Unity view is added back in gain. This way we ensure that
        //the video is in the background and the Unity view (containing the Unity scene(s)) overlay the video.
        //TODO: do we really need this anymore
        this.decorView = (ViewGroup) this.getWindow().getDecorView();
        if (this.decorView == null) {
            Log.e("UnityARPlayerActivity", "Error: Failed to find the decorView.");
        } else {
            //Generally the Unity view is as position 0 in the decorView. In some cases, however, it
            //is not. That is why we look for it in the view hierarchy.
            for (int i = 0; i < this.decorView.getChildCount(); i++) {
                View decorViewChild = this.decorView.getChildAt(i);
                if (decorViewChild instanceof ViewGroup) {
                    this.unityView = (ViewGroup) decorViewChild;
                    break;
                }
            }

            if (this.unityView == null) {
                String errorMsg = "Error: Failed to find the unityView.";
                Log.e("UnityARPlayerActivity", errorMsg);
                notifyFinish(errorMsg);
            }
        }

        mArtoolkitPlugin = new ARUnityPluginImpl(this);
    }

    /**
     * Callback indicating whether the user granted or denied (camera) permissions.
     *
     * @param requestCode  request code passed from requestCameraPermissions() [CAMERA_PERMISSION_REQUEST]
     * @param permissions  String array of requested permissions [Manifest.permission.CAMERA]
     * @param grantResults array of permission results, or empty if request cancelled
     */
    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           String permissions[], int[] grantResults) {
        if (mArtoolkitPlugin != null)
            mArtoolkitPlugin.onRequestPermissionsResult(requestCode, permissions, grantResults);
        else
            Log.i(TAG, "onRequestPermissionsResult: ARToolKit-Plugin not initialized yet.");
    }


    @Override
    protected void onResume() {
        Log.i(TAG, "onResume()");
        super.onResume();

        if (mArtoolkitPlugin.isUnityUp()) {
            startCamera();
        } else {
            Log.i(TAG, "onResume(): Unity is not running yet.");
        }
    }

    public void notifyFinish(String errorMessage) {
        new AlertDialog.Builder(this)
                .setMessage(errorMessage)
                .setTitle("Error")
                .setCancelable(true)
                .setNeutralButton(android.R.string.ok,
                        new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int whichButton) {
                                finish();
                            }
                        })
                .show();
    }

    @Override
    protected void onPause() {
        Log.i(TAG, "onPause()");

        ViewGroup decorView = (ViewGroup) getWindow().getDecorView();

        if (mCameraAccessHandler != null) {
            mCameraAccessHandler.closeCamera();
        }
        if (frameLayout != null) {
            // Restore the original view hierarchy.
            frameLayout.removeAllViews();
            decorView.removeView(frameLayout);
            frameLayout = null;
        }
        if (unityView != null && unityView.getParent() == null) {
            decorView.addView(unityView);
        }
        super.onPause();
    }

    public ARUnityPlugin getARToolKitPlugin() {
        return this.mArtoolkitPlugin;
    }

    /**
     * Callback indicating the device configuration (device orientation) changed
     *
     * @param newConfig Configuration object containing the updated device settings
     */
    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        Log.i(TAG, "onConfigurationChanged()");
        mArtoolkitPlugin.onConfigurationChanged(newConfig);
        super.onConfigurationChanged(newConfig);
    }

    protected void startCamera() {

        final Activity activity = this;

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // Create a placeholder for us to insert the camera preview capture object to the
                // view hierarchy. Needed for Camera 1 API support
                frameLayout = new FrameLayout(activity);
                decorView.removeView(unityView); // We must remove the root view from its parent before we can add it somewhere else.
                decorView.addView(frameLayout);

                //API Level: 22 use camera2API
                CameraEventListener cameraEventListener = new UnityCameraEventListener();
                mCameraAccessHandler = AndroidUtils.createCameraAccessHandler(activity, cameraEventListener);
                if (mCameraAccessHandler.getCameraAccessPermissions()) {
                    return;
                }

                ((CameraSurface) mCameraAccessHandler).surfaceCreated();
                ((CameraSurface) mCameraAccessHandler).surfaceChanged();


                // Now add Unity view back in.
                // In order to ensure that Unity's view covers the camera preview each time onResume
                // is called, find the SurfaceView inside the Unity view hierarchy, and
                // set the media overlay mode on it. Add the Unity view AFTER adding the previewView.
                SurfaceView sv = findSurfaceView(unityView);
                if (sv == null) {
                    Log.w(TAG, "No SurfaceView found in Unity view hierarchy.");
                } else {
                    Log.i(TAG, "Found SurfaceView " + sv.toString() + ".");
                    sv.setZOrderMediaOverlay(true);
                }
                frameLayout.addView(unityView);
            }
        });
    }

    /**
     * Walk a view hierarchy looking for the first SurfaceView.
     * Search is depth first.
     *
     * @param v View hierarchy root.
     * @return The first SurfaceView in the hierarchy, or null if none could be found.
     */
    private SurfaceView findSurfaceView(View v) {
        if (v == null) return null;
        else if (v instanceof SurfaceView) return (SurfaceView) v;
        else if (v instanceof ViewGroup) {
            int childCount = ((ViewGroup) v).getChildCount();
            for (int i = 0; i < childCount; i++) {
                SurfaceView ret = findSurfaceView(((ViewGroup) v).getChildAt(i));
                if (ret != null) return ret;
            }
        }
        return null;
    }
}
