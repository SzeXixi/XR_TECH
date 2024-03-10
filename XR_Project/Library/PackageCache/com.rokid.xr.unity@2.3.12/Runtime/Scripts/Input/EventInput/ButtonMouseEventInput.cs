using System.Diagnostics;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Rokid.UXR.Module;

namespace Rokid.UXR.Interaction
{
    public class ButtonMouseEventInput : MonoSingleton<ButtonMouseEventInput>, IEventInput
    {
        public static Action<Vector2, bool> OnMouseMove;
        public static Action OnActiveButtonMouseModule;
        public static Action OnReleaseButtonMouseModule;
        public static Action OnEnterLongPressState;
        public static Action OnExitLongPressState;
        public static Action OnReHoverState;

        private Vector2 mouseDelta = Vector2.zero;
        private bool mouseChange;
        private float elapsedTime = 0;

        private Vector2 scrollDelta = Vector2.zero;
        private bool scrollChange;

        private long xDownTime = 0;
        private long yDownTime = 0;

        public float DefaultScrollSpeed = 0.5f;

        public float DefaultSpeedScale = 0.002f;

        private long GetCurrentTime()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970,
                1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        private bool isEnterLongPressState = false;
        private bool isReHoverState = false;

        public Transform Interactor { get; set; }
        public Transform CursorVisual { get; private set; }

        public Transform TurnPageIndicator { get; set; }

        private bool initialize = false;

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("ButtonMouseRayInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/ButtonMouseRayInteractor"));
                }
                Interactor = go.transform;
                Interactor.name = "ButtonMouseRayInteractor";
                Interactor.SetParent(transform);
                CursorVisual = Interactor.Find("CursorVisual");
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);

            if (TurnPageIndicator == null)
            {
                GameObject indicator = GameObject.Find("RKTurnPageIndicator");
                if (indicator == null)
                {
                    indicator = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/RKTurnPageIndicator"));
                }
                indicator.gameObject.SetActive(false);
                TurnPageIndicator = indicator.transform;
                TurnPageIndicator.name = "RKTurnPageIndicator";
                TurnPageIndicator.SetParent(transform);
            }
            initialize = true;
        }

        public void Release()
        {
            OnReleaseButtonMouseModule?.Invoke();
            Destroy(this.gameObject);
        }

        private void Update()
        {
            if (!initialize)
                return;

            bool hasKeyEvent = false;
            mouseDelta = Vector2.zero;
            scrollDelta = Vector2.zero;

            // 判断是否长按，以显示方向图标
            if (isEnterLongPressState)
            {
#if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
#else
                if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Backspace))
#endif
                {
                    if (!isReHoverState)
                    {
                        OnReHoverState();
                        isReHoverState = true;
                    }

                    TurnPageIndicator.gameObject.SetActive(false);
                    isEnterLongPressState = false;
                    OnExitLongPressState();
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)
                     || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_DOWN)
                   )
                {
                    if (!isReHoverState)
                    {
                        OnReHoverState();
                        isReHoverState = true;
                    }
                }
            }
            else
            {
                if (IsLongDown())
                {
                    TurnPageIndicator.position = CursorVisual.position;
                    TurnPageIndicator.rotation = CursorVisual.rotation;
                    TurnPageIndicator.localScale = CursorVisual.localScale;
                    TurnPageIndicator.gameObject.SetActive(true);

                    isReHoverState = false;
                    isEnterLongPressState = true;
                    OnEnterLongPressState();
                }
            }


            //  按键就进行自动切换
            if (InputModuleManager.Instance.GetButtonMouseActive() == false)
            {
                if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)
                    || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_RIGHT) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyUp(RKKeyEvent.KEY_DOWN)
                    || Input.GetKeyUp(KeyCode.JoystickButton0))
                {
                    ActiveModule();
                    return;
                }
                else
                {
                    if (isEnterLongPressState)
                    {
                        TurnPageIndicator.gameObject.SetActive(false);
                        isEnterLongPressState = false;
                        OnExitLongPressState();
                    }
                    return;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)
                    || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_LEFT) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_RIGHT)
                    )
                {
                    xDownTime = GetCurrentTime();
                    // RKLog.Info("====ButtonMouseEventInput==== : GetKeyDown: LeftArrow or RightArrow");
                    hasKeyEvent = true;
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)
                    || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_UP) || RKNativeInput.Instance.GetKeyDown(RKKeyEvent.KEY_DOWN)
                    )
                {
                    // yDownTime = GetCurrentTime();
                    // RKLog.Info("====ButtonMouseEventInput==== : GetKeyDown: UpArrow or DownArrow");
                    hasKeyEvent = true;
                }
            }


            if (Input.GetKey(KeyCode.UpArrow) || RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_UP))
            {
                if (isEnterLongPressState)
                {
                    scrollDelta[1] = -DefaultScrollSpeed;
                }
                else
                {
                    long value = GetCurrentTime() - yDownTime;
                    mouseDelta.y = -DefaultSpeedScale * Mathf.Clamp(value, 0, value);
#if UNITY_EDITOR
                    mouseDelta.y = -mouseDelta.y;
#endif
                }
                hasKeyEvent = true;
            }
            if (Input.GetKey(KeyCode.DownArrow) || RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_DOWN))
            {
                if (isEnterLongPressState)
                {
                    scrollDelta[1] = DefaultScrollSpeed;
                }
                else
                {
                    long value = GetCurrentTime() - yDownTime;
                    mouseDelta.y = DefaultSpeedScale * Mathf.Clamp(value, 0, value);
#if UNITY_EDITOR
                    mouseDelta.y = -mouseDelta.y;
#endif
                }
                hasKeyEvent = true;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_LEFT))
            {
                if (isEnterLongPressState)
                {
                    scrollDelta[0] = -DefaultScrollSpeed;
                }
                else
                {
                    long value = GetCurrentTime() - xDownTime;
                    mouseDelta.x = -DefaultSpeedScale * Mathf.Clamp(value, 0, value);
                }
                hasKeyEvent = true;
            }
            if (Input.GetKey(KeyCode.RightArrow) || RKNativeInput.Instance.GetKey(RKKeyEvent.KEY_RIGHT))
            {
                if (isEnterLongPressState)
                {
                    scrollDelta[0] = DefaultScrollSpeed;
                }
                else
                {
                    long value = GetCurrentTime() - xDownTime;
                    mouseDelta.x = DefaultSpeedScale * Mathf.Clamp(value, 0, value);
                }
                hasKeyEvent = true;
            }
            // 进入锁定状态，不移动指针
            if (!isEnterLongPressState && hasKeyEvent)
            {
                OnMouseMove?.Invoke(mouseDelta, isEnterLongPressState);
            }
        }

        public Vector2 GetMouseScrollDelta()
        {
            return scrollDelta;
        }


        #region IsLongDown
        float longDownTime = 1.1f;
        float downTime = 0;
        bool triggerLongDown;
        private bool IsLongDown()
        {
#if UNITY_EDITOR
            KeyCode pressKeyCode = KeyCode.KeypadEnter;
#else 
            KeyCode pressKeyCode = KeyCode.JoystickButton0;
#endif
            if (Input.GetKeyDown(pressKeyCode))
            {
                triggerLongDown = false;
                downTime = 0;
            }
            if (Input.GetKey(pressKeyCode) && !triggerLongDown)
            {
                downTime += Time.deltaTime;
                if (downTime >= longDownTime)
                {
                    triggerLongDown = true;
                    return true;
                }
            }
            return false;
        }
        #endregion


        public void ActiveModule()
        {
            // RKLog.KeyInfo("====ButtonMouseEventInput==== : ActiveModule");
            OnActiveButtonMouseModule?.Invoke();
            RKVirtualController.Instance.Change(ControllerType.Mouse);
            EventSystem.current.pixelDragThreshold = 5;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnMouseMove = null;
            initialize = false;
        }

        /// <summary>
        /// 是否进入长按锁定状态
        /// </summary>
        /// <returns></returns>
        public bool IsInLockState()
        {
            return InputModuleManager.Instance.GetButtonMouseActive() && isEnterLongPressState;
        }
    }
}

