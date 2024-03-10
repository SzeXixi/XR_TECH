using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 射线投射器  Only  Use Station Pro
    /// </summary>
    public class ThreeDofRayCaster : BaseRayCaster
    {
        [SerializeField]
        public float maxDragDistance = 10;
        [SerializeField]
        public float minDragDistance = 0.5f;
        private bool dragLock;
        private float curHitPointDis;
        private Vector3 curHitPoint;
        private Vector3 targetHitPoint;
        private Vector3 oriRayPos;

        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<ThreeDofInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<ThreeDofInput>();
                }
            }
        }

        protected override bool ProcessDrag(Ray ray)
        {
            // 计算拖拽点的目标位置
            CalDeltaZ();
            targetHitPoint = rayOrigin.position + ray.direction * curHitPointDis;
            var delta = (targetHitPoint - oriHitPoint);
            m_SelectedObj.SendMessageUpwards(drag, delta, SendMessageOptions.DontRequireReceiver);
            curHitPoint = oriHitPoint = targetHitPoint;
            return true;
        }

        protected override bool CanDrag(Vector3 delta)
        {
            bool canDrag = !dragLock && !dragging && (m_SelectedObj.GetComponent<IRayBeginDrag>() != null || m_SelectedObj.GetComponentInParent<IRayBeginDrag>() != null);
            dragLock = true;
            return canDrag;
        }

        protected override bool DragRelease()
        {
            bool release = dragging && (input.GetMouseButtonDown(0) || m_SelectedObj == null || m_SelectedObj.gameObject.activeInHierarchy == false);
            return release;
        }

        private void LateUpdate()
        {
            if (!dragging && input.GetMouseButtonUp(0))
            {
                dragLock = false;
            }
        }

        protected override void OnFirstSelect()
        {
            oriHitPoint = result.worldPosition;
            oriRayPos = ray.origin;
        }

        private void CalDeltaZ()
        {
            if (Utils.IsAndroidPlatfrom())
            {
                var forward = Input.GetKey(KeyCode.UpArrow) ? 1 : (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
                Vector3 deltaZ = ray.direction * forward * 0.05f;
                Vector3 deltaRayPos = ray.origin - oriRayPos;
                oriRayPos = ray.origin;
                curHitPoint = oriHitPoint + deltaRayPos;
                curHitPointDis = Mathf.Clamp(Vector3.Distance(curHitPoint + deltaZ, ray.origin), minDragDistance, maxDragDistance);
            }
            else
            {
                Vector3 deltaZ = ray.direction * Input.mouseScrollDelta.y * 0.5f;
                Vector3 deltaRayPos = ray.origin - oriRayPos;
                oriRayPos = ray.origin;
                curHitPoint = oriHitPoint + deltaRayPos;
                curHitPointDis = Mathf.Clamp(Vector3.Distance(curHitPoint + deltaZ, ray.origin), minDragDistance, maxDragDistance);
            }
        }
    }
}
