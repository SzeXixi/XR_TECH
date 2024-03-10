using UnityEngine;
using Rokid.UXR.Utility;
using Rokid.UXR.Interaction.ThirdParty;
namespace Rokid.UXR.Interaction
{
    public class GesRayCaster : BaseRayCaster
    {
        [SerializeField]
        private HandType hand;
        private GestureInput gesInput;

        private ManipulationMoveLogic moveLogic;

        private string dragToTarget = "OnGesDragToTarget";

        protected override void Start()
        {
            base.Start();
            if (base.rayInteractor == null)
                base.rayInteractor = GetComponent<RayInteractor>();
            moveLogic = new ManipulationMoveLogic();
        }

        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<GestureInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<GestureInput>();
                }
            }
            gesInput = GetComponent<GestureInput>();
            gesInput.SetHandType(hand);
            pointerEnter = "OnGesPointerEnter";
            pointerExit = "OnGesPointerExit";
            pointerHover = "OnGesPointerHover";
            pointerClick = "OnGesPointerClick";
            dragBegin = "OnGesBeginDrag";
            drag = "OnGesDrag";
            dragEnd = "OnGesEndDrag";
        }

        protected override bool CanDrag(Vector3 delta)
        {
            return !dragging && (m_SelectedObj.GetComponent<IGesBeginDrag>() != null || m_SelectedObj.GetComponentInParent<IGesBeginDrag>() != null) && Vector3.SqrMagnitude(delta) >= m_DragThreshold * m_DragThreshold;
        }

        protected override void OnBeginDrag()
        {
            base.OnBeginDrag();
            moveLogic.Setup(transform.GetPose(), oriHitPoint, m_SelectedObj.transform.GetPose(), result.gameObject.transform.localScale);
        }

        protected override bool ProcessDrag(Ray ray)
        {
            var targetHitPosition = moveLogic.UpdateTransform(transform.GetPose(), m_SelectedObj.transform, true, false);
            var delta = targetHitPosition - oriHitPoint;
            m_SelectedObj.SendMessageUpwards(drag, delta, SendMessageOptions.DontRequireReceiver);
            m_SelectedObj.SendMessageUpwards(dragToTarget, targetHitPosition, SendMessageOptions.DontRequireReceiver);
            oriHitPoint = targetHitPosition;
            return true;
        }

        protected override bool DragRelease()
        {
            return HandUtils.CanReleseHandDrag(hand);
        }
    }
}
