using UnityEngine;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;
using Rokid.UXR.SubModule;

namespace Rokid.UXR.Config
{
    public class UXRSDK
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Load()
        {
            InitModule();
            InitSetting();
        }

        private static void InitSetting()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            RKLog.SetLogEnable(UXRSDKConfig.Instance.LogActive);
        }

        private static void InitModule()
        {
#if !UNITY_EDITOR
            InitGlobalFPS();
            InitTracePoseListener();
#endif
            InitMRC();
        }

        private static void InitMRC()
        {
            if (UXRSDKConfig.Instance.MRCActive)
                MainCameraCache.mainCamera.gameObject.AddComponent<MRCLoader>();
        }

        private static void InitGlobalFPS()
        {
            if (RKGlobalFpsModule.Instance.IsEnableAPM())
            {
                RKGlobalFpsModule.Instance.SetActive(true);
            }
            else
            {
                RKGlobalFpsModule.Instance.DestroyInstance();
            }
        }
        
        private static void InitTracePoseListener()
        {
            RKTracePoseListener.Instance.Initialize();
        }
        
    }

}

