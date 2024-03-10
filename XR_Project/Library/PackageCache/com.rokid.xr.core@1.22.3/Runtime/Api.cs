//-----------------------------------------------------------------------

//-----------------------------------------------------------------------

namespace Rokid.XR.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// UXR Plugin API.
    /// </summary>
    public static class ApiLegacy
    {
        private static Rect _cachedSafeArea;
        private static ScreenOrientation _cachedScreenOrientation;


        /// <summary>
        /// Updates screen parameters. This method must be called at framerate to ensure the current
        /// screen orientation is properly taken into account by the head tracker.
        /// </summary>
        public static void UpdateScreenParams()
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return;
            }

            // Only set viewport orientation if it has changed since the last check.
            if (_cachedScreenOrientation != Screen.orientation)
            {
                _cachedScreenOrientation = Screen.orientation;
                XRLoader.SetViewportOrientation(_cachedScreenOrientation);
                XRLoader.RecalculateRectangles(Screen.safeArea);
                //ReloadDeviceParams();
            }
        }

        public static void setTrackingType(int type)
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return;
            }

            setHeadTrackingType(type);
        }

        /// <summary>
        /// Recenters the head tracker.
        /// </summary>
        public static void Recenter()
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return;
            }

            recenterHeadPose();
        }

        /// <summary>
        /// Recenters the head tracker(pitch,yaw and roll).
        /// </summary>
        public static void Recenter2()
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return;
            }

            recenterHeadPoseYPR();
        }

        public static string getDebugInfoU()
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return "Not Initialized";
            }
            IntPtr keyPtr = getDebugInfo();
            string result = Marshal.PtrToStringAnsi(keyPtr);
            //Debug.Log("Debug Info: " + result);
            return result;
        }

        public static int getHeadTrackerState()
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return 1;
            }
            int state = getSlamState();
            //Debug.Log("head tracker State: " + state);
            return state;
        }


        /// <summary>
        /// 获取预测的未来某时刻Head位姿
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static long getHeadPose(float[] position, float[] orientation)
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return 1;
            }
            long ts = getHeadPoseRHS(position, orientation);

            //R2L
            position[2] = -position[2];
            orientation[0] = -orientation[0]; orientation[1] = -orientation[1];
            return ts;
        }


        /// <summary>
        /// 获取最新相机时刻(physical camera)的Head位姿
        /// </summary>
        /// <param name="position"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static long GetHeadPoseCameraBase(float[] position, float[] orientation)
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return 1;
            }
            long ts = getHeadPoseCameraBaseRHS(position, orientation);
            //R2L
            position[2] = -position[2];
            orientation[0] = -orientation[0]; orientation[1] = -orientation[1];
            return ts;
        }

        /// <summary>
        /// 获取左右眼投影参数
        /// </summary>
        /// <param name="frustum_left">float[6] {left,right,bottom,top,near,far}</param>
        /// <param name="frustum_right">{left,right,bottom,top,near,far}</param>
        /// <returns></returns>
        public static bool getFrustum(float[] frustum_left, float[] frustum_right)
        {
            if (!XRLoader._isInitialized)
            {
                Debug.LogError(
                        "Please initialize UXR loader before calling this function.");
                return false;
            }

            bool result = get_frustum(frustum_left, frustum_right);
            return result;
        }

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern void setHeadTrackingType(int type);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern void recenterHeadPose();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern void recenterHeadPoseYPR();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern IntPtr getDebugInfo();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern int getSlamState();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern long getHeadPoseRHS(float[] position, float[] orientation);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern long getHeadPoseCameraBaseRHS(float[] position, float[] orientation);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern void getHistoryHeadPosePysRHS(long timestamp, float[] position, float[] orientation);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        private static extern bool get_frustum(float[] frustum_left, float[] frustum_right);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern int getGlassProductId();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern bool isUsbConnect();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassName();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassTypeId();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassSn();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassSeed();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void setGlassBrightness(int value);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern int getGlassBrightness();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassCalFile();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern IntPtr getGlassFirmwareVersion();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern bool isPreviewing();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void getPreviewDimen(int[] data);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void openPhoneTracker();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void closePhoneTracker();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void getPhonePose(float[] oritation);

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void recenterPhonePose();

        [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
        public static extern void recenterPhonePoseYPR();    

    }
}
