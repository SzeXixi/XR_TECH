using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Rokid.UXR.Module;

namespace Rokid.UXR.Interaction
{
    public class TouchPadEventInput : MonoSingleton<TouchPadEventInput>, IEventInput
    {
        public static Action<Vector2> OnMouseMove;
        public static Action OnActiveTouchPadModule;
        public static Action OnReleaseTouchPadModule;
        private Vector2 mouseDelta = Vector2.zero;
        public float X_MOVE_SCALE = 0.14f;
        public float Y_MOVE_SCALE = 0.7F;

        public Transform Interactor { get; set; }

        private bool initialize = false;

        public void Initialize(Transform parent)
        {
            if (Interactor == null)
            {
                GameObject go = GameObject.Find("TouchPadInteractor");
                if (go == null)
                {
                    go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactor/TouchPadInteractor"));
                }
                Interactor = go.transform;
                Interactor.name = "TouchPadInteractor";
                Interactor.SetParent(transform);
            }
            Interactor.SetParent(transform);
            this.transform.SetParent(parent);
            initialize = true;
        }

        public void Release()
        {
            OnReleaseTouchPadModule?.Invoke();
            Destroy(this.gameObject);
        }

        private void Start()
        {
            RKLog.Info("====TouchPadEventInput==== 注册android回调用");
        }

        private void Update()
        {
            // if (InputModuleManager.Instance.GetTouchPadActive() == false)
            // {
            //     if (Input.GetKeyDown(KeyCode.Mouse0))
            //     {
            //         ActiveModule();
            //     }
            // }
            if (!initialize)
                return;
            if (InputModuleManager.Instance.GetTouchPadActive())
            {
                if (Input.touchCount > 0)
                {
                    mouseDelta = Input.GetTouch(0).deltaPosition;
                    mouseDelta.x = mouseDelta.x * X_MOVE_SCALE;
                    mouseDelta.y = -mouseDelta.y * Y_MOVE_SCALE;
                    OnMouseMove?.Invoke(mouseDelta);
                }
            }
        }

        public void ActiveModule()
        {
            RKLog.Debug("====TouchPadEventInput==== : ActiveModule");
            OnActiveTouchPadModule?.Invoke();
            Input.ResetInputAxes();
            RKVirtualController.Instance.Change(ControllerType.Mouse);
            EventSystem.current.pixelDragThreshold = 10;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnMouseMove = null;
        }
    }
}

