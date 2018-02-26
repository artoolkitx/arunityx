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

class CameraEventListenerUnityImpl implements CameraEventListener {

    private static final String TAG = CameraEventListenerUnityImpl.class.getSimpleName();
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

        Log.d(TAG, "cameraStreamStarted");
        if (ARController.getInstance().onlyPushVideo(width, height, pixelFormat, cameraIndex, cameraIsFrontFacing)) {
            Log.e(TAG, "cameraStarted::arwAndroidVideoPushInit() failed");
        } else {
            firstUpdate = false;
        }

    }

    @Override
    public void cameraStreamFrame(byte[] frame, int frameSize) {
        Log.d(TAG, "cameraStreamFrame: firstUpdate: " + firstUpdate);

        if (firstUpdate) {
            if (ARController.getInstance().onlyPushVideo(width, height, pixelFormat, cameraIndex, cameraIsFrontFacing)) {
                Log.e(TAG, "arwAndroidVideoPushInit failed");
            } else {
                firstUpdate = false;
            }
        } else {
            ARController.getInstance().convert1(frame, frameSize);
        }
    }

    @Override
    public void cameraStreamFrame(ByteBuffer[] framePlanes, int[] framePlanePixelStrides, int[] framePlaneRowStrides) {
        if (firstUpdate) {
            if (ARController.getInstance().onlyPushVideo(width, height, pixelFormat, cameraIndex, cameraIsFrontFacing)) {
                Log.e(TAG, "arwAndroidVideoPushInit failed");
            } else {
                firstUpdate = false;
            }
        } else {
            ARController.getInstance().convert(framePlanes, framePlanePixelStrides, framePlaneRowStrides);
        }
    }

    @Override
    public void cameraStreamStopped() {
        //ARToolKit is stopped from Unity
        //Only need to make sure AndroidVideoPushFinal is called
        ARController.getInstance().onlyFinal();
    }
}
