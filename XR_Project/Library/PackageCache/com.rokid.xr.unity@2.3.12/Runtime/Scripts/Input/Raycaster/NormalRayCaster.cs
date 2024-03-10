
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// 射线投射器
    /// </summary>
    public class NormalRayCaster : BaseRayCaster
    {
        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<NormalInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<NormalInput>();
                }
            }
        }
        protected override bool ProcessDrag(Ray ray)
        {
            // 计算拖拽点的目标位置
            var targetHitPosition = transform.position + ray.direction * oriHitPointDis;
            var delta = targetHitPosition - oriHitPoint;
            m_SelectedObj.SendMessageUpwards(drag, delta, SendMessageOptions.DontRequireReceiver);
            oriHitPoint = targetHitPosition;
            return true;
        }

        protected override bool CanDrag(Vector3 delta)
        {
            return !dragging && (m_SelectedObj.GetComponent<IRayBeginDrag>() != null || m_SelectedObj.GetComponentInParent<IRayBeginDrag>() != null) && Vector3.SqrMagnitude(delta) >= m_DragThreshold * m_DragThreshold;
        }
    }
}
