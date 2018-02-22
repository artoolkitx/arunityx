/*
 *  UnityCameraEventListener.java
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

package org.artoolkitx.arx.unity;

import android.util.Log;

import org.artoolkitx.arx.arxj.ARController;
import org.artoolkitx.arx.arxj.ARX_jni;
import org.artoolkitx.arx.arxj.camera.CameraEventListener;

import java.nio.ByteBuffer;

class UnityCameraEventListener implements CameraEventListener {

    private static final String TAG = UnityCameraEventListener.class.getSimpleName();
    private boolean firstUpdate = true;
    private int width;
    private int height;
    private String pixelFormat;
    private int cameraIndex;
    private boolean cameraIsFrontFacing;

    @Override
    public void cameraStreamStarted(int width, int height, String pixelFormat, int cameraIndex, boolean cameraIsFrontFacing) {
        this.width = width;
        this.height = height;
        this.pixelFormat = pixelFormat;
        this.cameraIndex = cameraIndex;
        this.cameraIsFrontFacing = cameraIsFrontFacing;

        if (ARX_jni.arwAndroidVideoPushInit(0, width, height, pixelFormat, cameraIndex, (cameraIsFrontFacing ? 1 : 0)) != 0) {
            Log.e(TAG, "cameraStarted::arwAndroidVideoPushInit() failed");
        }

    }

    @Override
    public void cameraStreamFrame(byte[] frame, int frameSize) {
        if (firstUpdate) {
            if (ARX_jni.arwAndroidVideoPushInit(0, width, height, pixelFormat, cameraIndex, (cameraIsFrontFacing ? 1 : 0)) < 0) {
                Log.e(TAG, "arwAndroidVideoPushInit failed");
            } else {
                firstUpdate = false;
            }
        } else {
            ARController.getInstance().convertAndDetect1(frame, frameSize);
//            if (NativeInterface.arwAndroidVideoPush1(0, frame, frameSize) < 0) {
//                Log.e(TAG, "arwAndroidVideoPush1 failed");
//                return;
//            }
        }
    }

    @Override
    public void cameraStreamFrame(ByteBuffer[] framePlanes, int[] framePlanePixelStrides, int[] framePlaneRowStrides) {
        if (firstUpdate) {
            if (ARX_jni.arwAndroidVideoPushInit(0, width, height, pixelFormat, cameraIndex, (cameraIsFrontFacing ? 1 : 0)) < 0) {
                Log.e(TAG, "arwAndroidVideoPushInit failed");
            } else {
                firstUpdate = false;
            }
//            if (ARToolKit.getInstance().isNativeInited(0, width, height, pixelFormat, cameraIndex, (cameraIsFrontFacing ? 1 : 0))) {
//                if (NativeInterface.arwAndroidVideoPushInit(0, width, height, pixelFormat, cameraIndex, (cameraIsFrontFacing ? 1 : 0)) != 0){
//                    Log.e(TAG,"arwAndroidVideoPushInit failed");
//                } else {
//                    firstUpdate = false;
//                }
//            }
        } else {
            ARController.getInstance().convertAndDetect2(framePlanes, framePlanePixelStrides, framePlaneRowStrides);

//            int framePlaneCount = Math.min(framePlanes.length, 4); // Maximum 4 planes can be passed to native.
//            if (framePlaneCount == 1) {
//                if (NativeInterface.arwAndroidVideoPush2(0,
//                        framePlanes[0], framePlanePixelStrides[0], framePlaneRowStrides[0],
//                        null, 0, 0,
//                        null, 0, 0,
//                        null, 0, 0) < 0) {
//                    Log.e(TAG, "arwAndroidVideoPush2 failed");
//                    return;
//                }
//            } else if (framePlaneCount == 2) {
//                if (NativeInterface.arwAndroidVideoPush2(0,
//                        framePlanes[0], framePlanePixelStrides[0], framePlaneRowStrides[0],
//                        framePlanes[1], framePlanePixelStrides[1], framePlaneRowStrides[1],
//                        null, 0, 0,
//                        null, 0, 0) < 0) {
//                    Log.e(TAG, "arwAndroidVideoPush2 failed");
//                    return;
//                }
//            } else if (framePlaneCount == 3) {
//                if (NativeInterface.arwAndroidVideoPush2(0,
//                        framePlanes[0], framePlanePixelStrides[0], framePlaneRowStrides[0],
//                        framePlanes[1], framePlanePixelStrides[1], framePlaneRowStrides[1],
//                        framePlanes[2], framePlanePixelStrides[2], framePlaneRowStrides[2],
//                        null, 0, 0) < 0) {
//                    Log.e(TAG, "arwAndroidVideoPush2 failed");
//                    return;
//                }
//            } else if (framePlaneCount == 4) {
//                if (NativeInterface.arwAndroidVideoPush2(0,
//                        framePlanes[0], framePlanePixelStrides[0], framePlaneRowStrides[0],
//                        framePlanes[1], framePlanePixelStrides[1], framePlaneRowStrides[1],
//                        framePlanes[2], framePlanePixelStrides[2], framePlaneRowStrides[2],
//                        framePlanes[3], framePlanePixelStrides[3], framePlaneRowStrides[3]) < 0) {
//                    Log.e(TAG, "arwAndroidVideoPush2 failed");
//                    return;
//                }
//            }
        }
    }

    @Override
    public void cameraStreamStopped() {
        //ARToolKit is stopped from Unity
        // We only need to make sure we call the AndroidVideoPushFinal
        ARX_jni.arwAndroidVideoPushFinal(0);

        //ARToolKit.getInstance().stopAndFinal();
    }
}
