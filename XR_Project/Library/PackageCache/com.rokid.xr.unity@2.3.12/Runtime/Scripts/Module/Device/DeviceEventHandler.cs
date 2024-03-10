using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using Rokid.UXR.UI;

namespace Rokid.UXR.Module
{
    public class DeviceEventHandler : MonoBehaviour
    {
        /// <summary>
        /// 当usb断开的时候是否自动退出应用
        /// </summary>
        [SerializeField]
        private bool quitWhenUsbDisconnect = true;

        /// <summary>
        /// 触发系统返回键的时候退出
        /// </summary>
        [SerializeField]
        private bool responseToEscape = true;
        public static event Action OnSystemEnvCheckFaild;
        private void Start()
        {
            RKLog.Debug("====DeviceEventHandler==== Inited Usb Devices!!! ");
            NativeInterface.NativeAPI.RegisterUSBStatusCallback();
            NativeInterface.NativeAPI.OnUSBConnect += OnUSBConnect;
            NativeInterface.NativeAPI.OnUSBDisConnect += OnUSBDisConnect;
            if (!NativeInterface.NativeAPI.SystemEnvCheck())
            {
                if (OnSystemEnvCheckFaild != null)
                {
                    OnSystemEnvCheckFaild?.Invoke();
                }
                else
                {
                    string msg = Utils.IsChineseLanguage() ? "当前系统版本低,请升级到最新版本" : "The current system version is low, please upgrade to the latest version.";
                    UIManager.Instance().CreatePanel<TipPanel>(true).Init(msg, TipLevel.Error, 5, () =>
                    {
                        RKLog.Error("====DeviceEventHandler==== The current system version is low, please upgrade to the latest version!!! ");
                        Quit();
                    });
                }
            }
        }
        private void OnDestroy()
        {
            NativeInterface.NativeAPI.UnRegisterUSBStatusCallback();
            NativeInterface.NativeAPI.OnUSBConnect -= OnUSBConnect;
            NativeInterface.NativeAPI.OnUSBDisConnect -= OnUSBDisConnect;
        }

        private void OnUSBConnect()
        {
            RKLog.Debug("====UsbEventHandler==== USBConnect !!!");
        }


        private void OnUSBDisConnect()
        {
            RKLog.Debug("====UsbEventHandler==== USB Disconnect !!!");
            if (quitWhenUsbDisconnect)
                Quit();
        }

        public void Quit()
        {
            RKLog.Debug("====DeviceEventHandler==== Quit");
            Application.Quit();
            NativeInterface.NativeAPI.KillProcess();
        }

        private void Update()
        {
            if (responseToEscape)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_BACK) || Input.GetKeyDown(KeyCode.JoystickButton2))
                {
                    if (SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        Quit();
                    }
                    else
                    {
                        SceneManager.LoadScene(0);
                    }
                }
            }
        }
    }
}