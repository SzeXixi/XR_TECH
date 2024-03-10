using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Native;
using Rokid.UXR.Module;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public enum RingButtonsType
    {
        TouchPress = 1 << 1,
        DoubleClick = 1 << 2,
        LongPress = 1 << 3,
        Click = 1 << 4,
        Touching = 1 << 5,
    }

    public enum ThreeDofType
    {
        StationPro,
        Phone,
        Ring
    }

    /// <summary>
    /// The 3dof event input class provides an external interface for 3dof interaction
    /// </summary>
    public class ThreeDofEventInput : MonoSingleton<ThreeDofEventInput>, IEventInput
    {
        public static event Action<Quaternion> OnPhoneRayRotation;
        public static event Action<Quaternion> OnOriRot;
        public static event Action OnActiveThreeDofModule;
        public static event Action OnReleaseThreeDofModule;
        public static event Action OnThreeDofReset;
        public static event Action<bool> OnRaySleep;
        public Transform Interactor { get; set; }
        private bool dragging = false;
        private float[] data = new float[4];
        private bool initialize = false;
        private bool muteModuleActiveSound = false;
        private HandType hand = HandType.None;
        public HandType HandType { get { return hand; } set { hand = value; } }
        private Quaternion rayRotation = Quaternion.identity;
        private Quaternion preRayRotation = Quaternion.identity;
        private ThreeDofType threeDofType = ThreeDofType.StationPro;
        private float raySleepTime = 10.0f;
        private float raySleepElasptime = 0;
        private bool raySleep;
        private RingButtonsType ringButtonsEvent = 0;
        private RingButtonsType preRingButtonsEvent = 0;
        private bool ringUp;
        private bool ringDown;

        protected override void Awake()
        {
            NativeInterface.NativeAPI.OpenPhoneTracker();
            try
            {
                NativeInterface.NativeAPI.OpenRingTracker();
                NativeInterface.NativeAPI.RegisterRingButtonEvents(this.gameObject.name, "OnRingButtonsEvent");
            }
            catch (Exception e)
            {
                RKLog.Warning(e.ToString());
            }
            RKPointerLisener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd += OnPointerDragEnd;
            InputModuleManager.OnModuleActive += OnModuleActive;
        }

        private void OnRingButtonsEvent(string buttonsEvent)
        {
            // RKLog.KeyInfo($"OnRingButtonsEvent:{buttonsEvent}");
            this.ringButtonsEvent = (RingButtonsType)Convert.ToInt32(buttonsEvent);
        }

        private void ProcessRingButtonEvent()
        {
            if (threeDofType == ThreeDofType.Ring)
            {
                this.ringDown = false;
                this.ringUp = false;
                if (ContainTargetRingType(RingButtonsType.TouchPress, this.ringButtonsEvent) && !ContainTargetRingType(RingButtonsType.TouchPress, this.preRingButtonsEvent))
                {
                    this.ringDown = true;
                }
                if (!ContainTargetRingType(RingButtonsType.TouchPress, this.ringButtonsEvent) && ContainTargetRingType(RingButtonsType.TouchPress, this.preRingButtonsEvent))
                {
                    this.ringUp = true;
                }
                this.preRingButtonsEvent = this.ringButtonsEvent;
            }
        }

        private bool ContainTargetRingType(RingButtonsType containType, RingButtonsType oriType)
        {
            return (containType & oriType) == containType;
        }

        public bool GetRingButtonEvents(RingButtonsType targetButtonsType)
        {
            return (this.ringButtonsEvent & targetButtonsType) == targetButtonsType;
        }

        public bool GetRingDown()
        {
            return this.ringDown;
        }

        public bool GetRingUp()
        {
            return this.ringUp;
        }

        private void OnModuleActive(InputModuleType moduleType)
        {
            if (moduleType != InputModuleType.ThreeDof)
            {
                raySleepElasptime = 0;
                raySleep = false;
                OnRaySleep?.Invoke(false);
            }
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            dragging = true;
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void Update()
        {
            if (!initialize)
                return;

            UpdateThreeDofType();

            if (InputModuleManager.Instance.GetThreeDofActive())
            {
                if (CanResetImu())
                {
                    ResetImuAxisY();
                }

                if (Input.anyKeyDown)
                {
                    raySleepElasptime = 0;
                    raySleep = false;
                    OnRaySleep?.Invoke(false);
                }

                GetData();
                ProcessData();
                ProcessRingButtonEvent();
            }
            else if (Utils.IsAndroidPlatfrom())
            {
                if (!InputModuleManager.Instance.GetThreeDofActive() && CanActiveModule())
                {
                    ActiveModule();
                }
            }
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.H))
            {
                ActiveModule();
                ResetImuAxisY();
            }
#endif      
        }

        private void UpdateThreeDofType()
        {
            if (threeDofType != ThreeDofType.StationPro && (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                       || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                       || Input.GetKeyUp(KeyCode.JoystickButton0)))
            {
                threeDofType = ThreeDofType.StationPro;
            }
            else if (threeDofType != ThreeDofType.Phone && Input.touchCount > 0)
            {
                threeDofType = ThreeDofType.Phone;
            }
            else if (threeDofType != ThreeDofType.Ring && GetRingButtonEvents(RingButtonsType.Touching))
            {
                threeDofType = ThreeDofType.Ring;
            }
        }

        private void GetData()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    NativeInterface.NativeAPI.GetPhonePose(data);
                    break;
                case ThreeDofType.Phone:
                    NativeInterface.NativeAPI.GetPhonePose(data);
                    break;
                case ThreeDofType.Ring:
                    NativeInterface.NativeAPI.GetRingPose(data);
                    break;
            }
            rayRotation[0] = data[0];
            rayRotation[1] = data[1];
            rayRotation[2] = -data[2];
            rayRotation[3] = data[3];
        }

        private bool CanActiveModule()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    return Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                      || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                      || Input.GetKeyUp(KeyCode.JoystickButton0) || IsDoubleClick();
                case ThreeDofType.Phone:
                    return IsDoubleClick();
                case ThreeDofType.Ring:
                    return GetRingButtonEvents(RingButtonsType.Touching);
            }
            return false;
        }

        private bool CanResetImu()
        {
            return RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RESET_RAY) || IsDoubleClick();
        }

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("ThreeDofRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/ThreeDofRayInteractor"));
                }
                Interactor = go.transform;
                Interactor.name = "ThreeDofRayInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
        }

        public void Release()
        {
            OnReleaseThreeDofModule?.Invoke();
            Destroy(this.gameObject);
        }

        public void ActiveModule()
        {
            ThreeDofEventInput.OnActiveThreeDofModule?.Invoke();
            RKVirtualController.Instance.Change(ControllerType.PHONE3DOF);
            EventSystem.current.pixelDragThreshold = 60;
        }


        protected override void OnDestroy()
        {
            // SDK 不再关闭3dof 射线算法
            // NativeInterface.NativeAPI.ClosePhoneTracker();
            try
            {
                NativeInterface.NativeAPI.CloseRingTracker();
            }
            catch (System.Exception e)
            {
                RKLog.Warning(e.ToString());
            }
            OnPhoneRayRotation = null;
            RKPointerLisener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd -= OnPointerDragEnd;
            InputModuleManager.OnModuleActive -= OnModuleActive;
            initialize = false;
        }

        void ProcessData()
        {
            OnPhoneRayRotation?.Invoke(rayRotation);
            OnOriRot?.Invoke(rayRotation);
            LogThreeDofInfo(rayRotation.eulerAngles);
            if (InputModuleManager.Instance.GetThreeDofActive())
            {
                Vector3 preForward = preRayRotation * Vector3.forward;
                Vector3 forward = rayRotation * Vector3.forward;

                if (raySleep == false && Vector3.Angle(preForward, forward) < 0.05f)
                {
                    raySleepElasptime += Time.deltaTime;
                    if (raySleepElasptime > raySleepTime)
                    {
                        raySleep = true;
                        OnRaySleep.Invoke(true);
                    }
                }

                if (Vector3.Angle(preForward, forward) > 0.05f)
                {
                    raySleepElasptime = 0;
                    if (raySleep)
                    {
                        raySleep = false;
                        OnRaySleep.Invoke(false);
                    }
                }
                preRayRotation = rayRotation;
            }
        }

        #region LogThreeDofInfo
        private float logTime = 5;
        private float logElapsedTime = 0;
        private void LogThreeDofInfo(Vector3 euler)
        {
            logElapsedTime += Time.deltaTime;
            if (logElapsedTime > logTime)
            {
                logElapsedTime = 0;
                RKLog.KeyInfo($"====ThreeDofEventInput==== Ori Ray Euler : {euler} ");
            }
        }
        #endregion

        public void ResetImuAxisY()
        {
            NativeInterface.NativeAPI.RecenterPhonePose();
            OnThreeDofReset?.Invoke();
        }

        #region IsDoubleClick
        float doubleClickTime = 0.7f;
        float clickTime = 0;
        int clickCount = 0;
        private bool IsDoubleClick()
        {
            switch (threeDofType)
            {
                case ThreeDofType.StationPro:
                    if (Input.GetKeyDown(KeyCode.JoystickButton3))
                    {
                        clickCount++;
                    }
                    break;
                case ThreeDofType.Phone:
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began)
                        {
                            clickCount++;
                        }
                    }
                    break;
                case ThreeDofType.Ring:
                    // Do nothing 手环内置重置射线
                    break;
            }
            if (clickCount == 1)
            {
                clickTime += Time.deltaTime;
            }
            if (clickTime < doubleClickTime)
            {
                if (clickCount == 2)
                {
                    clickTime = 0;
                    clickCount = 0;
                    return true;
                }
            }
            else
            {
                clickCount = 0;
                clickTime = 0;
            }
            return false;
        }
        #endregion
    }
}
