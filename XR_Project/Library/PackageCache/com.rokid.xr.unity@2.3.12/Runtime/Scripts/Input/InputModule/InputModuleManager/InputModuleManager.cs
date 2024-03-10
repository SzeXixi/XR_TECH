using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Utility;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using Rokid.UXR.UI;

namespace Rokid.UXR.Interaction
{
    public enum InputModuleType
    {
        None = 1 << 0,
        ThreeDof = 1 << 1, //手机3dof 射线
        Gesture = 1 << 2, //手势
        Mouse = 1 << 3, // 蓝牙鼠标
        // ButtonMouse = 1 << 4, // 滑鼠
        // TouchPad = 1 << 5, // 触摸屏
    }

    [Flags]
    public enum ActiveModuleType
    {
        ThreeDof = 1 << 0, //手机 3dof 射线
        Gesture = 1 << 1, //手势
        Mouse = 1 << 2, // 蓝牙鼠标
        // ButtonMouse = 1 << 3, // 滑鼠
        // TouchPad = 1 << 4, // 触摸屏
    }

    [Flags]
    public enum ActiveHandType
    {
        LeftHand = 1 << 0, //左手
        RightHand = 1 << 1, //右手
    }


    [Flags]
    public enum ActiveHandOrientationType
    {
        Back = 1 << 0, //左手
        Palm = 1 << 1, //右手
    }


    [Flags]
    public enum ActiveHandInteractorType
    {
        Far = 1 << 0, //远场交互
        Near = 1 << 1, //进场交互
    }



    [Flags]
    public enum ActiveWatchType
    {
        DisableWatch = 1 << 0,
        EnableWatch = 1 << 1,
    }

    [Flags]
    public enum ActiveHeadHandType
    {
        NormalHand = 1 << 0,
        HeadHand = 1 << 1,
    }

    /// <summary>
    /// Input Module status 
    /// </summary>
    [Serializable]
    public class ActiveModuleStatus
    {
        public InputModuleType moduleType;

        public ActiveHandStatus leftHandStatus;

        public ActiveHandStatus rightHandStatus;
        public ActiveHeadHandType headHandType;
        public bool threeDofRaySleep;

        public override string ToString()
        {
            return $"ActiveInputModuleType: {moduleType} \r\n\r\nLeftHandStatus: {leftHandStatus.ToString()}  \r\n\r\nRightHandStatus: {rightHandStatus.ToString()}";
        }
    }

    [Serializable]
    public class ActiveHandStatus
    {
        public bool active;
        public ActiveHandInteractorType handInteractorType;
        public ActiveHandOrientationType handOrientationType;
        public ActiveWatchType activeWatchType;
        public bool handDragging;

        public override string ToString()
        {
            return $" \r\n  Active:{active} \r\n  HandDragging:{handDragging}\r\n  HandInteractorType:{handInteractorType.ToString()} \r\n  HandOrientationType:{handOrientationType.ToString()} \r\n  ActiveWatchType:{activeWatchType.ToString()}";
        }
    }


    /// <summary>
    /// This script implements the IInputModuleActive interface, which allows it to register its own activation status information to the InputModuleManager for centralized management and switching.
    /// </summary>
    public class InputModuleManager : MonoSingleton<InputModuleManager>
    {
        /// <summary>
        /// The default init module
        /// </summary>
        [SerializeField, Tooltip("默认初始化的模块")]
        private ActiveModuleType defaultInitModule;
        /// <summary>
        /// The default active module. 
        /// </summary>

        [SerializeField, Tooltip("默认激活的模块")]
        private InputModuleType defaultActiveModule;

        /// <summary>
        /// Whether to play a module switch sound.
        /// </summary>
        [HideInInspector, SerializeField, Tooltip("是否播放模块切换提示音")]
        private bool muteModuleActiveSound = false;
        /// <summary>
        /// Optional TextUI for debugging purposes.
        /// </summary>
        [Optional, SerializeField, Tooltip("调试Text")]
        private Text logText;
        [SerializeField, HideInInspector, Tooltip("模块的激活状态")]
        private ActiveModuleStatus activeModuleStatus = new ActiveModuleStatus();
        [SerializeField, Tooltip("当选择交互,设备不支持时,是否默认回退一个更为基础的交互")]
        private bool autoFallbackInput = true;
        private bool stateChanged, isInputModuleLock = false;
        private List<IInputModuleActive> moduleActives = new List<IInputModuleActive>();
        public static event Action<IInputModuleActive, bool> OnObjectActive;
        public static event Action<InputModuleType> OnModuleActive;


        public bool GetMouseActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.Mouse;
        }

        public bool GetGesActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.Gesture;
        }

        public bool GetThreeDofActive()
        {
            return activeModuleStatus.moduleType == InputModuleType.ThreeDof;
        }

        public bool GetButtonMouseActive()
        {
            return false;// activeModuleStatus.moduleType == InputModuleType.ButtonMouse;
        }

        public bool GetTouchPadActive()
        {
            return false;// activeModuleStatus.moduleType == InputModuleType.TouchPad;
        }

        public bool GetWatchModuleActive(HandType hand)
        {
            if (activeModuleStatus.moduleType == InputModuleType.Gesture)
            {
                if (hand == HandType.LeftHand)
                {
                    return activeModuleStatus.leftHandStatus.activeWatchType == ActiveWatchType.EnableWatch;
                }
                if (hand == HandType.RightHand)
                {
                    return activeModuleStatus.rightHandStatus.activeWatchType == ActiveWatchType.EnableWatch;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取当前激活的模块
        /// Gets the currently active module
        /// </summary>
        /// <returns></returns>
        public ActiveModuleStatus GetActiveModule()
        {
            return activeModuleStatus;
        }

        public void SetDefaultActiveModule(InputModuleType moduleType)
        {
            defaultActiveModule = moduleType;
        }

        public void SetMuteModuleActiveSound(bool isMute)
        {
            muteModuleActiveSound = isMute;
        }
        public bool GetMuteModuleActiveSound()
        {
            return muteModuleActiveSound;
        }

        public void Initialize()
        {
            RKLog.KeyInfo($"====InputModuleManager====: defaultInitModule : {defaultInitModule}, defaultActiveModule:{defaultActiveModule},transform:{transform.name}");
            if (HasInputModuleType(defaultInitModule, ActiveModuleType.ThreeDof))
            {
                ThreeDofEventInput.Instance.Initialize(transform);
            }

            if (HasInputModuleType(defaultInitModule, ActiveModuleType.Mouse))
            {
                MouseEventInput.Instance.Initialize(transform);
            }

            if (HasInputModuleType(defaultInitModule, ActiveModuleType.Gesture))
            {
                if (Utils.IsAndroidPlatfrom())
                {
                    if (autoFallbackInput && FuncDeviceCheck.CheckHandTrackingFunc())
                    {
                        GesEventInput.Instance.Initialize(transform);
                    }
                    else
                    {
                        string msg = Utils.IsChineseLanguage() ? "检测到您的设备不支持手势交互,已经将您的交互回退到3dof射线" : "Detected that your device does not support gesture interaction, and your interaction has been reverted to 3DOF ray";
                        UIManager.Instance().CreatePanel<TipPanel>(true).Init(msg, TipLevel.Warning, 3f);
                        ThreeDofEventInput.Instance.Initialize(transform);
                    }
                }
                else
                {
                    GesEventInput.Instance.Initialize(transform);
                }
            }

            switch (defaultActiveModule)
            {
                case InputModuleType.ThreeDof:
                    ThreeDofEventInput.Instance.ActiveModule();
                    break;
                case InputModuleType.Gesture:
                    if (Utils.IsAndroidPlatfrom())
                    {
                        if (autoFallbackInput && FuncDeviceCheck.CheckHandTrackingFunc())
                        {
                            GesEventInput.Instance.ActiveModule();
                        }
                        else
                        {
                            string msg = Utils.IsChineseLanguage() ? "检测到您的设备不支持手势交互,已经将您的交互回退到3dof射线" : "Detected that your device does not support gesture interaction, and your interaction has been reverted to 3DOF ray";
                            UIManager.Instance().CreatePanel<TipPanel>(true).Init(msg, TipLevel.Warning, 3f);
                            ThreeDofEventInput.Instance.ActiveModule();
                        }
                    }
                    else
                    {
                        GesEventInput.Instance.ActiveModule();
                    }
                    break;
                case InputModuleType.Mouse:
                    MouseEventInput.Instance.ActiveModule();
                    break;
            }

            if (EventSystem.current == null)
            {
                GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Events/RKEventSystem"));
                go.name = "RKEventSystem";
                go.transform.SetParent(transform);
            }

            InitModuleChangeAudio();
        }

        public void ForceActiveModule(InputModuleType moduleType)
        {
            this.activeModuleStatus.moduleType = moduleType;
            stateChanged = true;
        }
        private void Start()
        {
            InteractorStateChange.OnInteractorTypeChange += OnInteractorTypeChange;
            InteractorStateChange.OnHandDragStatusChanged += OnGestureDragStatusChanged;

            RKHandWatch.OnActiveWatch += OnActiveWatch;

            GesEventInput.OnHandOrHeadHandTypeChange += OnHandOrHeadHandTypeChange;

            MouseEventInput.OnActiveMouseModule += OnActiveMouseModule;
            ThreeDofEventInput.OnActiveThreeDofModule += OnActiveThreeDofModule;
            GesEventInput.OnActiveGesModule += OnActiveGesModule;

            MouseEventInput.OnReleaseMouseModule += OnReleaseMouseModule;
            ThreeDofEventInput.OnReleaseThreeDofModule += OnReleaseThreeDofModule;
            GesEventInput.OnReleaseGesModule += OnReleaseGesModule;
            ButtonMouseEventInput.OnReleaseButtonMouseModule += OnReleaseButtonMouseModule;
            TouchPadEventInput.OnReleaseTouchPadModule += OnReleaseTouchPadModule;

            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            GesEventInput.OnHandOrientationUpdate += OnHandOrientationUpdate;

            ThreeDofEventInput.OnRaySleep += OnThreeDofRaySleep;

            Initialize();
        }

        private void OnThreeDofRaySleep(bool sleeping)
        {
            if (activeModuleStatus.threeDofRaySleep != sleeping)
            {
                activeModuleStatus.threeDofRaySleep = sleeping;
                stateChanged = true;
            }
        }

        private void OnGestureDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.handDragging != dragging)
                {
                    activeModuleStatus.leftHandStatus.handDragging = dragging;
                    stateChanged = true;
                }
            }

            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.handDragging != dragging)
                {
                    activeModuleStatus.rightHandStatus.handDragging = dragging;
                    stateChanged = true;
                }
            }
        }

        private void OnActiveWatch(HandType hand, bool active)
        {
            if (hand == HandType.LeftHand)
            {
                activeModuleStatus.leftHandStatus.activeWatchType = active ? ActiveWatchType.EnableWatch : ActiveWatchType.DisableWatch;
                stateChanged = true;
            }
            if (hand == HandType.RightHand)
            {
                activeModuleStatus.rightHandStatus.activeWatchType = active ? ActiveWatchType.EnableWatch : ActiveWatchType.DisableWatch;
                stateChanged = true;
            }
        }

        private void OnHandOrientationUpdate(HandType hand, HandOrientation handOrientation)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.handOrientationType != ConvertType(handOrientation) && activeModuleStatus.leftHandStatus.handDragging == false)
                {
                    activeModuleStatus.leftHandStatus.handOrientationType = ConvertType(handOrientation);
                    stateChanged = true;
                }
            }

            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.handOrientationType != ConvertType(handOrientation) && activeModuleStatus.rightHandStatus.handDragging == false)
                {
                    activeModuleStatus.rightHandStatus.handOrientationType = ConvertType(handOrientation);
                    stateChanged = true;
                }
            }
        }

        private void OnTrackedFailed(HandType hand)
        {
            if (hand == HandType.LeftHand || hand == HandType.None)
            {
                if (activeModuleStatus.leftHandStatus.active == true)
                {
                    activeModuleStatus.leftHandStatus.active = false;
                    stateChanged = true;
                }
            }
            if (hand == HandType.RightHand || hand == HandType.None)
            {
                if (activeModuleStatus.rightHandStatus.active == true)
                {
                    activeModuleStatus.rightHandStatus.active = false;
                    stateChanged = true;
                }
            }
        }

        protected override void OnDestroy()
        {
            InteractorStateChange.OnInteractorTypeChange -= OnInteractorTypeChange;
            InteractorStateChange.OnHandDragStatusChanged -= OnGestureDragStatusChanged;

            RKHandWatch.OnActiveWatch -= OnActiveWatch;

            GesEventInput.OnHandOrHeadHandTypeChange -= OnHandOrHeadHandTypeChange;

            MouseEventInput.OnActiveMouseModule -= OnActiveMouseModule;
            ThreeDofEventInput.OnActiveThreeDofModule -= OnActiveThreeDofModule;
            GesEventInput.OnActiveGesModule -= OnActiveGesModule;
            // ButtonMouseEventInput.OnActiveButtonMouseModule -= OnActiveButtonMouseModule;
            // TouchPadEventInput.OnActiveTouchPadModule -= OnActiveTouchPadModule;

            MouseEventInput.OnReleaseMouseModule -= OnReleaseMouseModule;
            ThreeDofEventInput.OnReleaseThreeDofModule -= OnReleaseThreeDofModule;
            GesEventInput.OnReleaseGesModule -= OnReleaseGesModule;
            ButtonMouseEventInput.OnReleaseButtonMouseModule -= OnReleaseButtonMouseModule;
            TouchPadEventInput.OnReleaseTouchPadModule -= OnReleaseTouchPadModule;

            GesEventInput.OnTrackedSuccess -= OnTrackedSuccess;
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            GesEventInput.OnHandOrientationUpdate -= OnHandOrientationUpdate;

            ThreeDofEventInput.OnRaySleep -= OnThreeDofRaySleep;
        }

        private void OnInteractorTypeChange(HandType hand, InteractorType interactorType)
        {
            if (hand == HandType.LeftHand)
            {
                activeModuleStatus.leftHandStatus.handInteractorType = interactorType == InteractorType.Near ? ActiveHandInteractorType.Near : ActiveHandInteractorType.Far;
                stateChanged = true;
            }
            if (hand == HandType.RightHand)
            {
                activeModuleStatus.rightHandStatus.handInteractorType = interactorType == InteractorType.Near ? ActiveHandInteractorType.Near : ActiveHandInteractorType.Far;
                stateChanged = true;
            }
        }

        private void OnHandOrHeadHandTypeChange(HandOrHeadHandType handOrHeadHandType)
        {
            if (handOrHeadHandType == HandOrHeadHandType.NormalHand)
            {
                activeModuleStatus.headHandType = ActiveHeadHandType.NormalHand;
                stateChanged = true;
            }
            else
            {
                activeModuleStatus.headHandType = ActiveHeadHandType.HeadHand;
                stateChanged = true;
            }
        }

        private void OnActiveMouseModule()
        {
            if (!GetMouseActive() && !isInputModuleLock)
            {
                activeModuleStatus.moduleType = InputModuleType.Mouse;
                stateChanged = true;

                PlayModuleChangeAudio(activeModuleStatus.moduleType);

                OnModuleActive?.Invoke(InputModuleType.Mouse);
            }
        }

        private void OnActiveThreeDofModule()
        {
            if (!GetThreeDofActive() && !isInputModuleLock)
            {
                activeModuleStatus.moduleType = InputModuleType.ThreeDof;
                stateChanged = true;

                PlayModuleChangeAudio(activeModuleStatus.moduleType);

                OnModuleActive?.Invoke(InputModuleType.ThreeDof);
            }
        }

        private void OnActiveGesModule()
        {
            if (!GetGesActive() && !isInputModuleLock)
            {
                activeModuleStatus.moduleType = InputModuleType.Gesture;

                activeModuleStatus.leftHandStatus.handInteractorType = ConvertType(GesEventInput.Instance.GetInteractorType(HandType.LeftHand));
                activeModuleStatus.leftHandStatus.handOrientationType = ConvertType(GesEventInput.Instance.GetHandOrientation(HandType.LeftHand));

                activeModuleStatus.rightHandStatus.handInteractorType = ConvertType(GesEventInput.Instance.GetInteractorType(HandType.RightHand));
                activeModuleStatus.rightHandStatus.handOrientationType = ConvertType(GesEventInput.Instance.GetHandOrientation(HandType.RightHand));

                stateChanged = true;

                PlayModuleChangeAudio(activeModuleStatus.moduleType);

                OnModuleActive?.Invoke(InputModuleType.Gesture);
            }
        }

        private void OnReleaseGesModule()
        {
            if (GetGesActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseThreeDofModule()
        {
            if (GetThreeDofActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseMouseModule()
        {
            if (GetMouseActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseButtonMouseModule()
        {
            if (GetButtonMouseActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnReleaseTouchPadModule()
        {
            if (GetTouchPadActive())
            {
                activeModuleStatus.moduleType = InputModuleType.None;
                stateChanged = true;
            }
        }

        private void OnTrackedSuccess(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                if (activeModuleStatus.leftHandStatus.active == false)
                {
                    activeModuleStatus.leftHandStatus.active = true;
                    stateChanged = true;
                }
            }
            if (hand == HandType.RightHand)
            {
                if (activeModuleStatus.rightHandStatus.active == false)
                {
                    activeModuleStatus.rightHandStatus.active = true;
                    stateChanged = true;
                }
            }
        }

        public void RegisterActive(IInputModuleActive moudleActive)
        {
            this.moduleActives.Add(moudleActive);
            stateChanged = true;
            // RKLog.KeyInfo($"====ModuleSwitchManager====: RegisterActive {moudleActive.Go.name},{this.moduleActives.Count}");
        }

        public void UnRegisterActive(IInputModuleActive moudleActive)
        {
            this.moduleActives.Remove(moudleActive);
            // RKLog.KeyInfo($"====ModuleSwitchManager====: UnRegisterActive {moudleActive.Go.name},{this.moduleActives.Count}");
        }

        private void SetActiveEnable(IInputModuleActive active, bool enabled)
        {
            if (active.Behaviour != null)
            {
                active.Behaviour.enabled = enabled;
            }
            else
            {
                active.Go.SetActive(enabled);
                OnObjectActive?.Invoke(active, enabled);
            }
        }

        public void LockInputModuleChange()
        {
            isInputModuleLock = true;
        }

        public void ReleaseInputModuleChange()
        {
            isInputModuleLock = false;
            stateChanged = true;
        }

        private void UpdateStateChange()
        {
            // RKLog.KeyInfo($"====InputModuleManager==== Current Active Status: {activeModuleStatus}");
            stateChanged = false;
            for (int i = 0; i < moduleActives.Count; i++)
            {
                IInputModuleActive active = moduleActives[i];

                bool enabled = false;

                switch (activeModuleStatus.moduleType)
                {
                    case InputModuleType.Mouse:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Mouse))
                            enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Mouse);
                        break;
                    case InputModuleType.ThreeDof:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ThreeDof))
                            enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ThreeDof) && !activeModuleStatus.threeDofRaySleep;
                        break;
                    case InputModuleType.Gesture:
                        if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.Gesture))
                        {
                            if (JudgeHandLost(active.DisableOnHandLost, activeModuleStatus.leftHandStatus.active) && HasHandType(active.ActiveHandType, ActiveHandType.LeftHand) && HasHandInteractorType(active.ActiveHandInteractorType, activeModuleStatus.leftHandStatus.handInteractorType) && HasHandOrientationType(active.ActiveHandOrientationType, activeModuleStatus.leftHandStatus.handOrientationType) &&
                            HasWatchType(active.ActiveWatchType, activeModuleStatus.leftHandStatus.activeWatchType) && HasHeadHandType(active.ActiveHeadHandType, activeModuleStatus.headHandType))
                            {
                                enabled = true;
                            }
                            if (JudgeHandLost(active.DisableOnHandLost, activeModuleStatus.rightHandStatus.active) && HasHandType(active.ActiveHandType, ActiveHandType.RightHand) && HasHandInteractorType(active.ActiveHandInteractorType, activeModuleStatus.rightHandStatus.handInteractorType) && HasHandOrientationType(active.ActiveHandOrientationType, activeModuleStatus.rightHandStatus.handOrientationType) &&
                            HasWatchType(active.ActiveWatchType, activeModuleStatus.rightHandStatus.activeWatchType) && HasHeadHandType(active.ActiveHeadHandType, activeModuleStatus.headHandType))
                            {
                                enabled = true;
                            }
                        }
                        break;
                    // case InputModuleType.ButtonMouse:
                    //     if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ButtonMouse))
                    //         enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.ButtonMouse);
                    //     break;
                    // case InputModuleType.TouchPad:
                    //     if (HasInputModuleType(active.ActiveModuleType, ActiveModuleType.TouchPad))
                    //         enabled = HasInputModuleType(active.ActiveModuleType, ActiveModuleType.TouchPad);
                    //     break;
                    default:
                        enabled = false;
                        break;
                }
                SetActiveEnable(active, enabled);
            }
        }

        private void Update()
        {
            if (stateChanged)
            {
                UpdateStateChange();
            }
            if (logText != null)
            {
                logText.text = activeModuleStatus.ToString() + $"\r\n\r\nDragThreshold: {EventSystem.current.pixelDragThreshold}";
            }
        }

        #region  Judge Has Type
        private bool HasInputModuleType(ActiveModuleType inType, ActiveModuleType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandType(ActiveHandType inType, ActiveHandType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandOrientationType(ActiveHandOrientationType inType, ActiveHandOrientationType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHandInteractorType(ActiveHandInteractorType inType, ActiveHandInteractorType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasWatchType(ActiveWatchType inType, ActiveWatchType targetType)
        {
            return (inType & targetType) == targetType;
        }

        private bool HasHeadHandType(ActiveHeadHandType inType, ActiveHeadHandType targetType)
        {
            return (inType & targetType) == targetType;
        }


        private bool JudgeHandLost(bool disableOnHandLost, bool handLost)
        {
            if (disableOnHandLost == false)
            {
                return true;
            }
            else
            {
                return handLost;
            }
        }

        #endregion

        #region  Convert
        private ActiveHandInteractorType ConvertType(InteractorType interactorType)
        {
            switch (interactorType)
            {
                case InteractorType.Far:
                    return ActiveHandInteractorType.Far;
                case InteractorType.Near:
                    return ActiveHandInteractorType.Near;
            }
            return default(ActiveHandInteractorType);
        }

        private ActiveHandOrientationType ConvertType(HandOrientation handOrientation)
        {
            switch (handOrientation)
            {
                case HandOrientation.Back:
                    return ActiveHandOrientationType.Back;
                case HandOrientation.Palm:
                    return ActiveHandOrientationType.Palm;
            }
            return default(ActiveHandOrientationType);
        }
        #endregion


        #region PlayAudio
        private int moduleChangeAudio = -1;
        public void InitModuleChangeAudio()
        {
            NativeInterface.NativeAPI.makeSoundPool();
            moduleChangeAudio = NativeInterface.NativeAPI.loadSound("ModuleChangeAudio.wav");
        }

        public void PlayModuleChangeAudio(InputModuleType type)
        {
            if (!muteModuleActiveSound && moduleChangeAudio != -1)
            {
                NativeInterface.NativeAPI.playSound(moduleChangeAudio);
            }
        }
        #endregion

    }
}

