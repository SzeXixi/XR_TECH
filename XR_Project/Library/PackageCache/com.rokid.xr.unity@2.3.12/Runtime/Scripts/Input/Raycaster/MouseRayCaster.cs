using UnityEngine;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Mouse Ray Caster
    /// </summary>
    public class MouseRayCaster : BaseRayCaster
    {
        protected override void Init()
        {
            base.Init();
            pointerEnter = "OnMousePointerEnter";
            pointerExit = "OnMousePointerExit";
            pointerHover = "OnMousePointerHover";
            pointerClick = "OnMousePointerClick";
            dragBegin = "OnMouseBeginDrag";
            drag = "OnMouseRayDrag";
            dragEnd = "OnMouseEndDrag";
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
            return !dragging && (m_SelectedObj.GetComponent<IMouseBeginDrag>() != null || m_SelectedObj.GetComponentInParent<IMouseBeginDrag>() != null) && Vector3.SqrMagnitude(delta) >= m_DragThreshold * m_DragThreshold;
        }
    }
}
