using UnityEngine;
using Rokid.UXR.Module;
using Rokid.UXR.Native;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public class TouchPadRayPose : BaseRayPose
    {

        [SerializeField, Tooltip("移动速度")]
        private float yaw;
        private float pitch;
        private float roll;

        private void Awake()
        {
            ResetPose();
            TouchPadEventInput.OnActiveTouchPadModule += OnActiveMouseModule;
            renderCamera = Camera.main;
        }

        private void OnDestroy()
        {
            TouchPadEventInput.OnActiveTouchPadModule -= OnActiveMouseModule;
        }

        private void OnEnable()
        {
            TouchPadEventInput.OnMouseMove += OnMouseMove;
        }

        private void OnDisable()
        {
            TouchPadEventInput.OnMouseMove -= OnMouseMove;
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
            // RKLog.Debug("====TouchPadRayPose====: Reset Pose");
            transform.position = MainCameraCache.mainCamera.transform.position;
            transform.rotation = MainCameraCache.mainCamera.transform.rotation;
        }


        private void OnMouseMove(Vector2 delta)
        {
            if (Utils.IsUnityEditor())
                return;
            // RKLog.Debug("====TouchPadRayPose====:" + delta);
            pitch = transform.rotation.eulerAngles.x;
            yaw = transform.rotation.eulerAngles.y;
            roll = transform.rotation.eulerAngles.z;
            pitch += delta.y;
            yaw += delta.x;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            LimitInViewField();
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

