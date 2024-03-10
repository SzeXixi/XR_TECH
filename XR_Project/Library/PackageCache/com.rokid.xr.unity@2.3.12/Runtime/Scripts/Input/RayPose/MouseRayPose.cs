using UnityEngine;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;
using System;

namespace Rokid.UXR.Interaction
{
    public class MouseRayPose : BaseRayPose
    {

        [SerializeField, Tooltip("移动速度")]
        private float moveSpeed = 1.0f;
        private float yaw;
        private float pitch;
        private float roll;
        private bool dragging = false;

        private void Awake()
        {
            ResetPose();
            MouseEventInput.OnActiveMouseModule += OnActiveMouseModule;
            RKPointerLisener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd += OnPointerDragEnd;
            renderCamera = Camera.main;
        }

        private void OnPointerDragEnd(PointerEventData data)
        {
            dragging = false;
        }

        private void OnPointerDragBegin(PointerEventData data)
        {
            dragging = true;
        }

        private void OnDestroy()
        {
            MouseEventInput.OnActiveMouseModule -= OnActiveMouseModule;
            RKPointerLisener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd -= OnPointerDragEnd;
        }

        private void OnEnable()
        {
            MouseEventInput.OnMouseMove += OnMouseMove;
        }

        private void OnDisable()
        {
            MouseEventInput.OnMouseMove -= OnMouseMove;
        }

        private void OnActiveMouseModule()
        {
            ResetPose();
        }

        /// <summary>
        /// 重置射线位姿
        /// </summary>
        private void ResetPose()
        {
            RKLog.Debug("====MouseRayPose====: Reset Pose");
            transform.position = MainCameraCache.mainCamera.transform.position;
            transform.rotation = MainCameraCache.mainCamera.transform.rotation;
        }

        private void OnMouseMove(Vector2 delta)
        {
            if (Utils.IsUnityEditor())
                return;
            RKLog.Debug("====MouseRayPose====:" + delta);
            pitch = transform.rotation.eulerAngles.x;
            yaw = transform.rotation.eulerAngles.y;
            roll = transform.rotation.eulerAngles.z;
#if UNITY_EDITOR
            moveSpeed = 1;
            pitch += -delta.y * moveSpeed;
#else
            pitch += delta.y * moveSpeed;
#endif
            yaw += delta.x * moveSpeed;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);

            if (!dragging)
            {
                LimitInViewField();
            }
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            LimitInViewField();
        }
#endif
    }

}

