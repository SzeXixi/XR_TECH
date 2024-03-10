using UnityEngine;
using Rokid.UXR.Utility;
using Rokid.UXR.Native;

namespace Rokid.UXR.Interaction
{
    public class ButtonMouseRayPose : BaseRayPose
    {

        [SerializeField, Tooltip("移动速度")]
        private float moveSpeed = 1.0f;
        private float yaw;
        private float pitch;
        private float roll;

        protected override void Start()
        {
            RKLog.Info("====ButtonMouseRayPose==== Start");
        }

        private void Awake()
        {
            ResetPose();
            RKLog.Info("====ButtonMouseRayPose====: Awake");
            ButtonMouseEventInput.OnActiveButtonMouseModule += OnActiveMouseModule;
            ButtonMouseEventInput.OnMouseMove += OnMouseMove;
            renderCamera = Camera.main;
        }

        private void OnDestroy()
        {
            RKLog.Info("====ButtonMouseRayPose====: OnDestroy");
            ButtonMouseEventInput.OnActiveButtonMouseModule -= OnActiveMouseModule;
            ButtonMouseEventInput.OnMouseMove -= OnMouseMove;
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
            RKLog.Info("====ButtonMouseRayPose====: Reset Pose");
            transform.position = MainCameraCache.mainCamera.transform.position;
            transform.rotation = MainCameraCache.mainCamera.transform.rotation;
        }


        private void OnMouseMove(Vector2 delta, bool isInLongPress)
        {
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
            transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        }
        protected override void Update()
        {
            if (!ButtonMouseEventInput.Instance.IsInLockState())
            {
                LimitInViewField();
            }
        }
    }
}

