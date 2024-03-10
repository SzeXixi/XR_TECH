using System;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofRayPose : BaseRayPose
    {
        private HandType handType;
        private Vector3 offsetPosition = new Vector3(0.1f, -0.5f, 0);
        private Transform cameraOffset;

        protected override void Start()
        {
            base.Start();
            ThreeDofEventInput.OnPhoneRayRotation += OnPhoneRayRotation;
            // this.handType = ThreeDofEventInput.Instance.HandType;
            InitHandType(HandType.RightHand);
            cameraOffset = new GameObject("CameraOffset").transform;
            cameraOffset.SetParent(this.transform);
        }

        private void OnDestroy()
        {
            ThreeDofEventInput.OnPhoneRayRotation -= OnPhoneRayRotation;
        }

        private void OnPhoneRayRotation(Quaternion rayRotation)
        {
            if (Utils.IsUnityEditor())
                return;
            this.transform.rotation = Quaternion.Lerp(transform.rotation, rayRotation, Time.deltaTime * 25);
        }


        protected override void Update()
        {
            base.Update();
            if (ThreeDofEventInput.Instance.HandType != handType)
            {
                handType = ThreeDofEventInput.Instance.HandType;
                InitHandType(handType);
            }
        }

        private void LateUpdate()
        {
            cameraOffset.position = MainCameraCache.mainCamera.transform.position;
            Vector3 targetForward = Vector3.ProjectOnPlane(MainCameraCache.mainCamera.transform.forward, Vector3.up);
            cameraOffset.rotation = Quaternion.FromToRotation(Vector3.forward, targetForward);
            Vector3 followPosition = cameraOffset.TransformPoint(offsetPosition);
            this.transform.position = followPosition;
        }

        private void InitHandType(HandType handType)
        {
            if (Utils.IsAndroidPlatfrom())
            {
                switch (handType)
                {
                    case HandType.None:
                        offsetPosition = new Vector3(0, -0.5f, 0);
                        break;
                    case HandType.LeftHand:
                        offsetPosition = new Vector3(-0.1f, -0.5f, 0);
                        break;
                    case HandType.RightHand:
                        offsetPosition = new Vector3(0.1f, -0.5f, 0);
                        break;
                }
            }
        }
    }
}
