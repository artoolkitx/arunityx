/*
 *  UnityARPlayerActivity.java
 *  ARToolKit5
 *
 *  Disclaimer: IMPORTANT:  This Daqri software is supplied to you by Daqri
 *  LLC ("Daqri") in consideration of your agreement to the following
 *  terms, and your use, installation, modification or redistribution of
 *  this Daqri software constitutes acceptance of these terms.  If you do
 *  not agree with these terms, please do not use, install, modify or
 *  redistribute this Daqri software.
 *
 *  In consideration of your agreement to abide by the following terms, and
 *  subject to these terms, Daqri grants you a personal, non-exclusive
 *  license, under Daqri's copyrights in this original Daqri software (the
 *  "Daqri Software"), to use, reproduce, modify and redistribute the Daqri
 *  Software, with or without modifications, in source and/or binary forms;
 *  provided that if you redistribute the Daqri Software in its entirety and
 *  without modifications, you must retain this notice and the following
 *  text and disclaimers in all such redistributions of the Daqri Software.
 *  Neither the name, trademarks, service marks or logos of Daqri LLC may
 *  be used to endorse or promote products derived from the Daqri Software
 *  without specific prior written permission from Daqri.  Except as
 *  expressly stated in this notice, no other rights or licenses, express or
 *  implied, are granted by Daqri herein, including but not limited to any
 *  patent rights that may be infringed by your derivative works or by other
 *  works in which the Daqri Software may be incorporated.
 *
 *  The Daqri Software is provided by Daqri on an "AS IS" basis.  DAQRI
 *  MAKES NO WARRANTIES, EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION
 *  THE IMPLIED WARRANTIES OF NON-INFRINGEMENT, MERCHANTABILITY AND FITNESS
 *  FOR A PARTICULAR PURPOSE, REGARDING THE DAQRI SOFTWARE OR ITS USE AND
 *  OPERATION ALONE OR IN COMBINATION WITH YOUR PRODUCTS.
 *
 *  IN NO EVENT SHALL DAQRI BE LIABLE FOR ANY SPECIAL, INDIRECT, INCIDENTAL
 *  OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *  SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 *  INTERRUPTION) ARISING IN ANY WAY OUT OF THE USE, REPRODUCTION,
 *  MODIFICATION AND/OR DISTRIBUTION OF THE DAQRI SOFTWARE, HOWEVER CAUSED
 *  AND WHETHER UNDER THEORY OF CONTRACT, TORT (INCLUDING NEGLIGENCE),
 *  STRICT LIABILITY OR OTHERWISE, EVEN IF DAQRI HAS BEEN ADVISED OF THE
 *  POSSIBILITY OF SUCH DAMAGE.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2011-2015 ARToolworks, Inc.
 *
 *  Author(s): Julian Looser, Philip Lamb
 *
 */

package org.artoolkit.ar.unity;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewGroup.LayoutParams;
import android.view.WindowManager;
import android.widget.FrameLayout;
import com.unity3d.player.UnityPlayerNativeActivity;
import org.artoolkit.ar.base.camera.CameraPreferencesActivity;
import jp.epson.moverio.bt200.DisplayControl;

//Imports below required to ask for permission to use the camera.
//NOTE: The support library aar file MUST be included in the Unity plugins folder.
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.widget.Toast;

//For Epson Moverio BT-200. BT200Ctrl.jar must be in libs/ folder.

public class UnityARPlayerActivity extends UnityPlayerNativeActivity {

    protected final static String TAG = "UnityARPlayerActivity";

    private CameraSurface previewView = null;

    // For Epson Moverio BT-200.
    private DisplayControl mDisplayControl = null;

    protected final static int PERMISSION_REQUEST_CAMERA = 77;


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

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        // For Epson Moverio BT-200.
        if (Build.MANUFACTURER.equals("EPSON") && Build.MODEL.equals("embt2")) {
            mDisplayControl = new DisplayControl(this);
            //private static final int FLAG_SMARTFULLSCREEN = 0x80000000; // For Epson Moverio BT-200.
            getWindow().addFlags(0x80000000);
        }

        super.onCreate(savedInstanceState);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        View root = findViewById(android.R.id.content);
        if (root != null) {
            root.setKeepScreenOn(true);
        }

        // This needs to be done just only the very first time the application is run,
        // or whenever a new preference is added (e.g. after an application upgrade).
        int resID = getResources().getIdentifier("preferences", "xml", getPackageName());
        PreferenceManager.setDefaultValues(this, resID, false);

		//Request permission to use the camera on android 23+
        int permissionCheck = ContextCompat.checkSelfPermission(this.getApplicationContext(), Manifest.permission.CAMERA);
        if (permissionCheck != PackageManager.PERMISSION_GRANTED)
        {
            ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.CAMERA}, PERMISSION_REQUEST_CAMERA);
        }
    }

	//Handle the result of asking the user for camera permission.
    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           String permissions[], int[] grantResults) {
        switch (requestCode) {
            case PERMISSION_REQUEST_CAMERA: {
                // If request is cancelled, the result arrays are empty.
                if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {

                    //Camera permission granted. If you NEED to show a toast, uncomment the line below.
                    //Toast.makeText(this, "Camera Access Granted", Toast.LENGTH_SHORT).show();

                } else {

                    // TODO: Fail gracefully here.
                    Toast.makeText(this, "Camera permissions are required for Augmented Reality", Toast.LENGTH_LONG).show();
                }
                return;
            }
        }
    }

    @Override
    protected void onResume() {
        Log.i(TAG, "onResume()");

        super.onResume();

        // Add camera preview as a new view.

        ViewGroup decorView = (ViewGroup) getWindow().getDecorView();

        // Create the camera preview.
        previewView = new CameraSurface(this);
        decorView.addView(previewView, new LayoutParams(128, 128));

        Log.i(TAG, "onResume() - All done!");
    }

    @Override
    protected void onPause() {
        Log.i(TAG, "onPause()");

        super.onPause();

        // Remove camera preview view.
        ViewGroup decorView = (ViewGroup) getWindow().getDecorView();
        
        // Simply remove the camera preview from the view hierarchy.
        if (previewView != null) {
            decorView.removeView(previewView);
            previewView = null; // Make sure camera is released in onPause().
        }
    }

    void launchPreferencesActivity() {
        startActivity(new Intent(this, CameraPreferencesActivity.class));
    }

    void setStereo(boolean stereo) {
        // For Epson Moverio BT-200, enable stereo mode.
        if (Build.MANUFACTURER.equals("EPSON") && Build.MODEL.equals("embt2")) {
            //int dimension = (stereo ? DIMENSION_3D : DIMENSION_2D);
            //set2d3d(dimension);
            mDisplayControl.setMode(stereo ? DisplayControl.DISPLAY_MODE_3D : DisplayControl.DISPLAY_MODE_2D, stereo); // Last parameter is 'toast'.
        }
    }
}

/*
// FOR REFERENCE, PARENT UNITY3DPLAYER CLASS FOLLOWS.

package com.unity3d.player;

import android.app.NativeActivity;
import android.content.res.Configuration;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;

public class UnityPlayerNativeActivity extends NativeActivity
{
	protected UnityPlayer mUnityPlayer;		// don't change the name of this variable; referenced from native code

	// UnityPlayer.init() should be called before attaching the view to a layout - it will load the native code.
	// UnityPlayer.quit() should be the last thing called - it will unload the native code.
	protected void onCreate (Bundle savedInstanceState)
	{
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		super.onCreate(savedInstanceState);
		
		getWindow().takeSurface(null);
		setTheme(android.R.style.Theme_NoTitleBar_Fullscreen);
		getWindow().setFormat(PixelFormat.RGB_565);

		mUnityPlayer = new UnityPlayer(this);
		if (mUnityPlayer.getSettings ().getBoolean ("hide_status_bar", true))
			getWindow ().setFlags (WindowManager.LayoutParams.FLAG_FULLSCREEN,
			                       WindowManager.LayoutParams.FLAG_FULLSCREEN);

		int glesMode = mUnityPlayer.getSettings().getInt("gles_mode", 1);
		boolean trueColor8888 = false;
		mUnityPlayer.init(glesMode, trueColor8888);

		View playerView = mUnityPlayer.getView();
		setContentView(playerView);
		playerView.requestFocus();
	}
	protected void onDestroy ()
	{
		mUnityPlayer.quit();
		super.onDestroy();
	}

	// onPause()/onResume() must be sent to UnityPlayer to enable pause and resource recreation on resume.
	protected void onPause()
	{
		super.onPause();
		mUnityPlayer.pause();
	}
	protected void onResume()
	{
		super.onResume();
		mUnityPlayer.resume();
	}
	public void onConfigurationChanged(Configuration newConfig)
	{
		super.onConfigurationChanged(newConfig);
		mUnityPlayer.configurationChanged(newConfig);
	}
	public void onWindowFocusChanged(boolean hasFocus)
	{
		super.onWindowFocusChanged(hasFocus);
		mUnityPlayer.windowFocusChanged(hasFocus);
	}
	public boolean dispatchKeyEvent(KeyEvent event)
	{
		if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
			return mUnityPlayer.onKeyMultiple(event.getKeyCode(), event.getRepeatCount(), event);
		return super.dispatchKeyEvent(event);
	}
}
*/
