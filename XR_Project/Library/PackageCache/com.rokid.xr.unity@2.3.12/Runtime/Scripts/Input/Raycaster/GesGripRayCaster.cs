using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Rokid.UXR.Interaction
{
    public class GesGripRayCaster : BaseRayCaster
    {
        [SerializeField]
        private HandType hand;
        private Vector3 oriHandPos;

        private GestureGripInput physicalInput;

        protected override void Init()
        {
            base.Init();
            if (inputOverride == null)
            {
                inputOverride = GetComponent<GestureGripInput>();
                if (inputOverride == null)
                {
                    inputOverride = gameObject.AddComponent<GestureGripInput>();
                }
            }
            physicalInput = GetComponent<GestureGripInput>();
            physicalInput.SetHandType(hand);
            pointerEnter = "OnGesGripPointerEnter";
            pointerExit = "OnGesGripPointerExit";
            pointerHover = "OnGesGripPointerHover";
            pointerClick = "OnGesGripPointerClick";
            dragBegin = "OnGesGripBeginDrag";
            drag = "OnGesGripDrag";
            dragEnd = "OnGesGripEndDrag";
        }


        protected override bool ProcessDrag(Ray ray)
        {
            var delta = GesEventInput.Instance.GetHandDeltaPos(hand);
            m_SelectedObj.SendMessageUpwards(drag, delta, SendMessageOptions.DontRequireReceiver);
            return true;
        }

        protected override Vector3 CalDragDelta()
        {
            return oriHandPos - physicalInput.GetGesPosition();
        }

        protected override void OnFirstSelect()
        {
            oriHandPos = physicalInput.GetGesPosition();
        }

        protected override Camera GetEventCamera()
        {
            return MainCameraCache.mainCamera.GetComponent<Camera>();
        }


        protected override void ProcessNothingDownEvent(PointerEventData eventData)
        {
            // base.ProcessNothingDownEvent(eventData);
        }

        protected override void ProcessNothingUpEvent(PointerEventData eventData)
        {
            // base.ProcessNothingUpEvent(eventData);
        }
    }
}
