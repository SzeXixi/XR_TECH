﻿using System;
using UnityEngine;

namespace Rokid.UXR.Module
{
    /// <summary>
    ///  Android Native Input
    /// </summary>
    public class RKNativeInput : MonoSingleton<RKNativeInput>
    {
        bool isDebug = false;
        private bool m_Init = false;

        public RKKeyEvent keyState;
        public RKKeyEvent keyDownState;
        public RKKeyEvent keyUpState;

        public float lrHorizontalAxis = 0;
        public float lrHVerticalAxis = 0;
        public float rrHorizontalAxis = 0;
        public float rrHVerticalAxis = 0;

        private void LateUpdate()
        {
            keyDownState = 0;
            keyUpState = 0;
        }

        protected override void OnSingletonInit()
        {
            DontDestroyOnLoad(this.gameObject);
            Initialized();
            this.gameObject.name = "RKInput";
            this.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialized()
        {
            if (m_Init) return;
            m_Init = true;
        }

        public bool GetKey(RKKeyEvent key)
        {
            return (keyState & key) != 0;
        }

        public bool GetKeyDown(RKKeyEvent key)
        {
            return (keyDownState & key) != 0;
        }

        public bool GetKeyUp(RKKeyEvent key)
        {
            return (keyUpState & key) != 0;
        }

        /**
         * return axis value from -1 to 1;
         */
        public float GetAxis(AxisEvent axisName)
        {
            switch (axisName)
            {
                case AxisEvent.Horizontal_Left:
                    return lrHorizontalAxis;
                case AxisEvent.Vertical_Left:
                    return lrHVerticalAxis;
                case AxisEvent.Horizontal_Right:
                    return rrHorizontalAxis;
                case AxisEvent.Vertical_Right:
                    return rrHVerticalAxis;
                default:
                    return 0f;
            }
        }

        //Android Key Event Callback
        void OnKeyEvent(string keyEvent)
        {
            RKLog.Debug("UXR-PLUGIN::OnKeyEvent = " + keyState);
            keyState = (RKKeyEvent)int.Parse(keyEvent);
        }

        void OnKeyDownEvent(string keyDownEvent)
        {
            RKLog.Debug("UXR-PLUGIN::keyDownEvent = " + keyDownState);
            keyDownState |= (RKKeyEvent)int.Parse(keyDownEvent);
        }

        void OnKeyUpEvent(string keyUpEvent)
        {
            RKLog.Debug("UXR-PLUGIN::keyUpEvent = " + keyUpState);
            keyUpState |= (RKKeyEvent)int.Parse(keyUpEvent);
        }

        void OnAxisEvent(string axisEvent)
        {
            RKLog.Debug("UXR-PLUGIN::axisEvent = " + axisEvent);
            string[] strAxis = axisEvent.Split('|');
            if ("LR".Equals(strAxis[0]))
            {
                lrHorizontalAxis = float.Parse(strAxis[1]);
                lrHVerticalAxis = float.Parse(strAxis[2]);
            }
            else if ("RR".Equals(strAxis[0]))
            {
                rrHorizontalAxis = float.Parse(strAxis[1]);
                rrHVerticalAxis = float.Parse(strAxis[2]);
            }
        }
    }

    public enum RKKeyEvent
    {
        KEY_LEFT = 1 << 0,
        KEY_RIGHT = 1 << 1,
        KEY_UP = 1 << 2,
        KEY_DOWN = 1 << 3,
        KEY_BACK = 1 << 4,
        KEY_HOME = 1 << 5,
        KEY_OK = 1 << 6,
        KEY_X = 1 << 7,
        KEY_Y = 1 << 8,
        KEY_B = 1 << 9,
        KEY_A = 1 << 10,
        KEY_SELECT = 1 << 11,
        KEY_START = 1 << 12,
        KEY_RR_LEFT = 1 << 13,
        KEY_RR_RIGHT = 1 << 14,
        KEY_RR_UP = 1 << 15,
        KEY_RR_DOWN = 1 << 16,
        KEY_RESET = 1 << 17,
        KEY_TOOLS = 1 << 18,
        KEY_POWER = 1 << 19,
        KEY_RESET_RAY = 1 << 20,
        KEY_MENU_BTN1_LOCK = 1 << 21,
        KEY_MENU_BTN1_UNLOCK = 1 << 22,
        KEY_MENU_BTN2 = 1 << 23,
        KEY_MENU_BTN3 = 1 << 24,
        KEY_MOUSE_SECONDARY = 1 << 25,
        KEY_MOUSE_THERIARY = 1 << 26,
    }

    public enum AxisEvent
    {
        Horizontal_Left,
        Vertical_Left,
        Horizontal_Right,
        Vertical_Right
    }
}