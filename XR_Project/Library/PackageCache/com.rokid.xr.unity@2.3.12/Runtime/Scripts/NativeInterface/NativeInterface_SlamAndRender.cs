using UnityEngine;
using System;
using Rokid.UXR.Utility;
using System.Runtime.InteropServices;

namespace Rokid.UXR.Native
{

    public enum HeadTrackingStatus
    {
        Unknow = 0,
        UnInit = 1,
        Detecting = 2,//RESERVED
        Tracking = 3,
        Track_Limited = 4,//RESERVED
        Tracking_Bad = 5,//RESERVED
        Tracking_Paused = 6,
        Tracking_Stopped = 7,
        Tracking_Error = 99
    }

    public partial class NativeInterface
    {
        public partial class NativeAPI
        {
            private static float[] frustum_left = new float[6];
            private static float[] frustum_right = new float[6];
            private static float[] position = new float[3];
            private static float[] rotation = new float[4];


            /// <summary>
            /// Get head tracking status
            /// </summary>
            /// <returns></returns>
            public static HeadTrackingStatus GetHeadTrackingStatus()
            {
                if (Utils.IsAndroidPlatfrom())
                    return (HeadTrackingStatus)Api.getHeadTrackerState();
                return HeadTrackingStatus.Tracking;
            }


            /// <summary>
            /// Update screen parameters
            /// </summary>
            [Obsolete]
            public static void UpdateScreenParams()
            {
                if (Utils.IsAndroidPlatfrom())
                    Api.UpdateScreenParams();
            }

            /// <summary>
            /// Reset center
            /// </summary>
            public static void Recenter()
            {
                if (Utils.IsAndroidPlatfrom())
                    Api.Recenter();
            }

            /// <summary>
            /// Get debug informatio
            /// </summary>
            public static string GetDebugInfo()
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    return Api.getDebugInfoU();
                }
                return null;
            }

            /// <summary>
            /// Get left and right eye projection parameters
            /// </summary>
            /// <param name="frustum_left">float[4]  {hFov,vFov,near,far}</param>
            /// <param name="frustum_right">float[4] {hFov,vFov,near,far}</param>
            /// <returns></returns>
            public static void GetUnityFrustum(ref float[] frustum_left, ref float[] frustum_right)
            {
                if (!Utils.IsAndroidPlatfrom())
                    return;
                if (frustum_left.Length != 4 || frustum_right.Length != 4)
                {
                    RKLog.Error("====NativeAnd====:+ GetUnityFrustum 参数数组长度不正确");
                    return;
                }
                if (GetFrustum())
                {
                    frustum_left[0] = GetHFov(false);
                    frustum_left[1] = GetVFov(false);
                    frustum_left[2] = NativeAPI.frustum_left[4];
                    frustum_left[3] = NativeAPI.frustum_left[5];

                    frustum_right[0] = GetHFov(true);
                    frustum_right[1] = GetVFov(true);
                    frustum_right[2] = NativeAPI.frustum_right[4];
                    frustum_right[3] = NativeAPI.frustum_right[5];
                }
            }

            /// <summary>
            /// Get left and right eye fov parameters
            /// </summary>
            /// <returns></returns>
            public static void GetUnityEyeFrustumHalf(bool isRight, ref float[] fov)
            {
                if (!Utils.IsAndroidPlatfrom())
                    return;
                if (GetFrustum())
                {
                    RKLog.KeyInfo($"====GetUnityEyeFrustumHalf====: left fov {Newtonsoft.Json.JsonConvert.SerializeObject(frustum_left)},right fov {Newtonsoft.Json.JsonConvert.SerializeObject(frustum_right)} ");
                    float[] frustum = isRight ? frustum_right : frustum_left;
                    float left = frustum[0];
                    float right = frustum[1];
                    float bottom = frustum[2];
                    float top = frustum[3];
                    float near = frustum[4];
                    float RAD2DEG = 180.0f / 3.14159265358979323846f;
                    fov[0] = RAD2DEG * (float)Math.Atan(Math.Abs(left) / near);
                    fov[1] = RAD2DEG * (float)Math.Atan(Math.Abs(right) / near);
                    fov[2] = RAD2DEG * (float)Math.Atan(Math.Abs(top) / near);
                    fov[3] = RAD2DEG * (float)Math.Atan(Math.Abs(bottom) / near);
                }
            }

            /// <summary>
            /// Get left and right eye projection parameters
            /// </summary>
            /// <param name="frustum_lest">float[6] {left,right,bottom,top,near,far}</param>
            /// <param name="frustum_right">{left,right,bottom,top,near,far}</param>
            /// <returns></returns>
            private static bool GetFrustum()
            {
                return Api.getFrustum(frustum_left, frustum_right);
            }

            /// <summary>
            /// Get vertical field of view
            /// </summary>
            /// <param name="frustum"></param>
            /// <returns></returns>
            public static float GetVFov(bool isRight)
            {
                float[] frustum = isRight ? frustum_right : frustum_left;
                float near = frustum[4];
                float top = frustum[3];
                float bottom = frustum[2];
                float RAD2DEG = 180.0f / 3.14159265358979323846f;
                float fov = RAD2DEG * (2.0f * (float)Math.Atan((top - bottom) / (2.0f * near)));
                return fov;
            }

            /// <summary>
            /// Get horizontal field of view
            /// </summary>
            /// <param name="frustum"></param>
            /// <returns></returns>
            public static float GetHFov(bool isRight)
            {
                float[] frustum = isRight ? frustum_right : frustum_left;
                float near = frustum[4];
                float left = frustum[0];
                float right = frustum[1];
                float RAD2DEG = 180.0f / 3.14159265358979323846f;
                float fov = RAD2DEG * (2.0f * (float)Math.Atan((right - left) / (2.0f * near)));
                return fov;
            }

            /// <summary>
            /// Get camera pose
            /// </summary>
            /// <returns></returns>
            [Obsolete("Use GetHeadPose instead")]
            public static Pose GetCameraPose(out long timeStamp)
            {
                return GetHeadPose(out timeStamp);
            }

            /// <summary>
            /// Get current time head pose
            /// </summary>
            /// <returns></returns>
            public static Pose GetHeadPose(out long timeStamp)
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    timeStamp = getHeadPoseRHS(position, rotation);
                    Pose pose = new Pose(new Vector3(position[0], position[1], -position[2]),
                        new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]));
                    return pose;
                }
                timeStamp = 0;
                return Pose.identity;
            }

            /// <summary>
            /// Input a timestamp to retrieve the head pose at that specific timestamp only for ges.
            /// </summary>
            /// <param name="timestamp"></param> (t-1000ms ~ t-10ms)
            /// <returns></returns>
            public static Pose GetHistoryHeadPose(long timestamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatfrom())
                {
                    getHistoryHeadPoseRHS(timestamp, position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                }
                return pose;
            }


            /// <summary>
            ///  It is current physics camera pose
            /// </summary>
            /// <returns></returns>
            public static Pose GetCameraPhysicsPose(out long timeStamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatfrom())
                {
                    timeStamp = getHeadPosePysRHS(position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                    return pose;
                }
                timeStamp = 0;
                return pose;
            }

            /// <summary>
            ///  It is history physics camera pose
            /// </summary>
            /// <param name="timestamp"></param>
            /// <returns></returns>
            public static Pose GetHistoryCameraPhysicsPose(long timestamp)
            {
                Pose pose = Pose.identity;
                if (Utils.IsAndroidPlatfrom())
                {
                    getHistoryHeadPosePysRHS(timestamp, position, rotation);
                    pose.position = new Vector3(position[0], position[1], -position[2]);
                    pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                }
                return pose;
            }

            /// <summary>
            ///  Get Head Origin Pose
            /// </summary>
            /// <returns></returns>
            public static bool TryGetHeadPoseOffset(out Pose pose)
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    int result = getHeadPoseOffset2RHS(position, rotation);
                    if (result == 0)
                    {
                        pose.position = new Vector3(position[0], position[1], -position[2]);
                        pose.rotation = new Quaternion(-rotation[0], -rotation[1], rotation[2], rotation[3]);
                        RKLog.KeyInfo($"====NativeAPI====:  OffsetYaw {pose.rotation.eulerAngles.y}");
                        return true;
                    }
                }
                pose = Pose.identity;
                return false;
            }

            /// <summary>
            /// SetTrackingType
            /// </summary>
            /// <param name="type"></param>
            public static void SetTrackingType(int type)
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    RKLog.Info($"====NativeAPI====: SetHeadTrackingType :{type}");
                    Api.setTrackingType(type);
                }
            }

            /// <summary>
            /// Get ost info
            /// </summary>
            /// <returns></returns>
            public static string GetOSTInfo()
            {
                IntPtr calPtr = Api.getGlassCalFile();
                if (calPtr != null)
                {
                    string cal = Marshal.PtrToStringAuto(calPtr);
                    return cal;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// [type:0-SLAM_CONFIG_COORDINATE
            ///  value:0-PiEngine坐标系,1-OpenGL坐标系]
            /// [type:1-SLAM_CONFIG_3DOF_FOLLOW_SLAM
            ///  value:0-纯3dof,1-跟随slam 3dof]
            /// </summary>
            /// <param name="configParam"></param>
            [Obsolete("This func is obsolete")]
            public static void ConfigSlamParam(int type, int value)
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    configSlamParam(type, value);
                }
            }

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            private static extern long getHeadPoseRHS(float[] position, float[] orientation);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            private static extern void getHistoryHeadPoseRHS(long timestamp, float[] position, float[] orientation);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            private static extern long getHeadPosePysRHS(float[] position, float[] orientation);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            private static extern int getHeadPoseOffset2RHS(float[] position, float[] orientation);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            private static extern void configSlamParam(int type, int value);
        }
    }
}
