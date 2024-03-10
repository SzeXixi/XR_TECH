using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class HeadHandRayCaster : BaseRayCaster, IHeadHandDriver
    {
        [SerializeField]
        private HandType hand;
        private GestureInput gesInput;

        private string dragToTarget = "OnGesDragToTarget";

        protected override void Start()
        {
            base.Start();
            if (base.rayInteractor == null)
                base.rayInteractor = GetComponent<RayInteractor>();
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
            return false;
        }


        protected override bool ProcessDrag(Ray ray)
        {
            RKLog.Info("====HeadHandRayCaster====: ProcessDrag");
            // 计算拖拽点的目标位置
            var targetHitPosition = ray.origin + ray.direction * oriHitPointDis;
            var delta = targetHitPosition - oriHitPoint;
            m_SelectedObj.SendMessageUpwards(drag, delta, SendMessageOptions.DontRequireReceiver);
            m_SelectedObj.SendMessageUpwards(dragToTarget, targetHitPosition, SendMessageOptions.DontRequireReceiver);
            oriHitPoint = targetHitPosition;
            return true;
        }

        public void OnChangeHoldHandType(HandType hand)
        {
            this.hand = hand;
            gesInput.SetHandType(hand);
            TriggerPointerDown();
            RKLog.Info("====HeadHandRayCaster====: ChangeHoldHandType");
        }

        protected override bool TriggerPointerDown()
        {
            RKLog.Info("====HeadHandRayCaster====: TriggerPointerDown");
            pressTime = 0;
            Raycast(ray, Mathf.Infinity, sortedRaycastResults);
            result = FirstRaycastResult();
            UpdatePointerEventData();
            if (result.gameObject != null)
            {
                m_FirstSelectedObj = result.gameObject;
                OnFirstSelect();
            }
            else
            {
                m_FirstSelectedObj = null;
                ProcessNothingDownEvent(pointerEventData);
            }
            return true;
        }

        public void OnHandPress(HandType hand)
        {

        }

        public void OnHandRelease()
        {

        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }
    }
}
