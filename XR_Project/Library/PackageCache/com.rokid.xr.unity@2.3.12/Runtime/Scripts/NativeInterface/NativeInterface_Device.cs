using System;
using System.Runtime.InteropServices;
using AOT;
using Rokid.UXR.Utility;
using UnityEngine;
using Rokid.UXR.Module;
namespace Rokid.UXR.Native
{
    public partial class NativeInterface
    {
        public partial class NativeAPI
        {
            /// <summary>
            /// usb connection callback successful
            /// </summary>
            public static event Action OnUSBConnect;
            /// <summary>
            /// usb disconnect callback
            /// </summary>
            public static event Action OnUSBDisConnect;

            /// <summary>
            /// On glass bright update callback
            /// </summary>
            public static event Action<int> OnGlassBrightUpdate;

            private static AndroidJavaClass versionInfo = new AndroidJavaClass("android.os.Build$VERSION");

            private static string UXR_SERVICE_USBDEVICE = "UXRUSBDevice.";
            private const string MinSystemBuildVersion = "20240111"; //RG-stationPro sys version check

            /// <summary>
            /// Get glasses name
            /// </summary>
            /// <returns></returns>
            public static string GetGlassName()
            {
#if UNITY_EDITOR
                return "PC";
#else
                IntPtr namePtr = Api.getGlassName();
                string name = Marshal.PtrToStringAnsi(namePtr);
                return name;
#endif
            }

            /// <summary>
            /// Get the glasspid of the glasses
            /// </summary>
            /// <returns></returns>
            public static int GetGlassPID()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return 0;
                }
                return Api.getGlassProductId();
            }

            /// <summary>
            /// Get glasses sn
            /// </summary>
            /// <returns></returns>
            public static string GetGlassSN()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return null;
                }
                IntPtr snPtr = Api.getGlassSn();
                string sn = Marshal.PtrToStringAnsi(snPtr);
                return sn;
            }

            /// <summary>
            /// Get the glasses type ids
            /// </summary>
            /// <returns></returns>
            public static string GetGlassTypeId()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return null;
                }
                IntPtr typeIdPtr = Api.getGlassTypeId();
                string typeId = Marshal.PtrToStringAnsi(typeIdPtr);
                return typeId;
            }

            /// <summary>
            /// Get the glasses firmware version
            /// </summary>
            /// <returns></returns>
            public static string GetGlassFirmwareVersion()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return null;
                }
                IntPtr versionPtr = Api.getGlassFirmwareVersion();
                string version = Marshal.PtrToStringAnsi(versionPtr);
                return version;
            }

            /// <summary>
            /// Get glasses brightness
            /// </summary>
            /// <returns></returns>
            public static int GetGlassBrightness()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return 0;
                }
                return Api.getGlassBrightness();
            }

            /// <summary>
            /// Set the brightness range of the glasses to 1-100
            /// </summary>
            /// <param name="value"></param>
            public static void SetGlassBrightness(int value)
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                Api.setGlassBrightness(value);
            }

            /// <summary>
            /// Get Build INCREMENTAL
            /// </summary>
            /// <value></value>
            public static string INCREMENTAL
            {
                get
                {
                    if (Utils.IsAndroidPlatfrom())
                    {
                        return versionInfo.GetStatic<string>("INCREMENTAL");
                    }
                    return "";
                }
            }

            /// <summary>
            /// Station Pro System Check
            /// </summary>
            /// <returns></returns>
            public static bool SystemEnvCheck()
            {
                if (SystemInfo.deviceModel.Equals("Rokid RG-stationPro"))
                {
                    return NativeInterface.NativeAPI.INCREMENTAL.Split('-')[1].CompareTo(MinSystemBuildVersion) >= 0;
                }
                return true;
            }


            /// <summary>
            /// Set the glasses status callback
            /// </summary>
            [Obsolete("Use RegisterUSBStatusCallback instead")]
            public static void setUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                setOnUsbStatusUpdate(OnUsbStatusUpdateCallByC);//注册
            }

            /// <summary>
            /// Register usb status event
            /// </summary>
            public static void RegisterUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                setOnUsbStatusUpdate(OnUsbStatusUpdateCallByC);//注册
            }

            /// <summary>
            /// Unregister usb status event
            /// </summary>
            public static void UnRegisterUSBStatusCallback()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                clearOnUsbStatusUpdate();//注销
            }

            delegate void OnUsbStatusUpdate(bool isConnect);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnUsbStatusUpdate(OnUsbStatusUpdate cb);


            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnUsbStatusUpdate();

            [MonoPInvokeCallback(typeof(OnUsbStatusUpdate))]
            static void OnUsbStatusUpdateCallByC(bool isConnect)
            {
                if (isConnect)
                {
                    OnUSBConnect?.Invoke();
                }
                else
                {
                    OnUSBDisConnect?.Invoke();
                }
            }

            /// <summary>
            /// Kill the uxr process
            /// </summary>
            /// <returns></returns>
            public static bool KillProcess()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return false;
                }
                AndroidJavaObject obj = CallBridge.CallAndroid(Request.Build()
                .Name(UXR_SERVICE_USBDEVICE + "killProcess"));
                return CallBridge.CovertBool(obj);
            }

            /// <summary>
            /// Finish the uxr activity
            /// </summary>
            /// <returns></returns>
            public static bool FinishActivity()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return false;
                }
                AndroidJavaObject obj = CallBridge.CallAndroid(Request.Build()
                .Name(UXR_SERVICE_USBDEVICE + "activityFinish"));
                return CallBridge.CovertBool(obj);
            }

            /// <summary>
            /// Whether the usb is successfully connected
            /// </summary>
            /// <returns></returns>
            public static bool IsUSBConnect()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return false;
                }
                return Api.isUsbConnect();
            }

            /// <summary>
            /// Register glasses bright events
            /// </summary>
            [Obsolete("Use RegisterGlassBrightUpdate(Action<int> glassBrightUpdate) instead")]
            public static void RegisterGlassBrightUpdate()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                setOnGlassBrightUpdate(OnGlassBrightUpdateCallByC);
            }


            /// <summary>
            /// Register glasses bright events
            /// </summary>
            public static void RegisterGlassBrightUpdate(Action<int> glassBrightUpdate)
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                OnGlassBrightUpdate += glassBrightUpdate;
                setOnGlassBrightUpdate(OnGlassBrightUpdateCallByC);
            }

            /// <summary>
            /// UnRegister glasses bright events
            /// </summary>
            [Obsolete("Use UnregisterOnGlassBrightUpdate(Action<int> glassBrightUpdate) instead")]
            public static void UnregisterOnGlassBrightUpdate()
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                clearOnGlassBrightUpdate();//注销
            }


            /// <summary>
            /// UnRegister glasses bright events
            /// </summary>
            public static void UnregisterOnGlassBrightUpdate(Action<int> glassBrightUpdate)
            {
                if (!Utils.IsAndroidPlatfrom())
                {
                    return;
                }
                OnGlassBrightUpdate -= glassBrightUpdate;
                clearOnGlassBrightUpdate();//注销
            }

            /// <summary>
            /// Get persist value
            /// </summary>
            public static string GetPersistValue(string key)
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    IntPtr value = getPropertiesValue(key);
                    return Marshal.PtrToStringAnsi(value);
                }
                return null;
            }

            /// <summary>
            /// Set persist value
            /// </summary>
            public static void SetPersistValue(string key, string value)
            {
                if (Utils.IsAndroidPlatfrom())
                    setPropertiesValue(key, value);
            }

            delegate void OnGlassBrightUpdateC(int brightness);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void setOnGlassBrightUpdate(OnGlassBrightUpdateC cb);
            [DllImport(ApiConstants.UXR_GFX_PLUGIN)]
            static extern void clearOnGlassBrightUpdate();
            [MonoPInvokeCallback(typeof(OnGlassBrightUpdateC))]
            static void OnGlassBrightUpdateCallByC(int brightness)
            {
                RKLog.KeyInfo("Bright Update Call By C !!!" + brightness);
                OnGlassBrightUpdate?.Invoke(brightness);
            }

            [DllImport(ApiConstants.UXR_GFX_PLUGIN, CharSet = CharSet.Ansi)]
            static extern void setPropertiesValue(string key, string value);

            [DllImport(ApiConstants.UXR_GFX_PLUGIN, CharSet = CharSet.Ansi)]
            static extern IntPtr getPropertiesValue(string key);
        }
    }
}


