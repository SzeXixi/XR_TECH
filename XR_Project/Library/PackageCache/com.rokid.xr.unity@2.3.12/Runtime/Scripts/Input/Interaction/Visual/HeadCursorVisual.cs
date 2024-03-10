using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class HeadCursorVisual : AutoInjectBehaviour, IHeadHandDriver, ICustomCursorVisual
    {
        [SerializeField]
        private RayInteractor rayInteractor;

        [Tooltip("光标Size")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [Autowrited("Cursor_Focus")]
        private Transform focusCursor;

        [Autowrited("Cursor_Press")]
        private Transform pressCursor;

        [Autowrited("Cursor_Drag")]
        private Transform dragCursor;

        [Autowrited("Cursor_Disable")]
        private Transform disableCursor;

        private Transform customCursorVisual;
        public Transform CustomCursorVisual { get { return customCursorVisual; } set { customCursorVisual = value; customCursorVisual?.SetParent(this.transform); } }

        private Pose customTargetPose = Pose.identity;
        public Pose CustomTargetPose { get => customTargetPose; set => customTargetPose = value; }

        private bool handPress;
        private bool triggerCanDraggable = false;
        private bool trackSuccess = false;
        private bool leftHandLost = true;
        private bool rightHandLost = true;
        private float scaleRelease = 1.5f;
        private float scalePress = 1.0f;
        private float startSmoothPinchDistance = 0.06f;

        private HeadHandDriver headHandDriver;
        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
            GesEventInput.OnTrackedFailed += OnTrackedFailed;
            GesEventInput.OnTrackedSuccess += OnTrackedSuccess;
            headHandDriver = GetComponentInParent<HeadHandDriver>();
        }

        private void OnTrackedFailed(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                leftHandLost = true;
            }
            if (hand == HandType.RightHand)
            {
                rightHandLost = true;
            }
        }

        private void OnTrackedSuccess(HandType hand)
        {
            if (hand == HandType.LeftHand)
            {
                leftHandLost = false;
            }
            if (hand == HandType.RightHand)
            {
                rightHandLost = false;
            }
        }

        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
            GesEventInput.OnTrackedFailed -= OnTrackedFailed;
            GesEventInput.OnTrackedSuccess -= OnTrackedSuccess;
        }

        private void OnEnable()
        {
            RKPointerLisener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit += OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover += OnPointerHover;
        }



        private void OnDisable()
        {
            RKPointerLisener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover -= OnPointerHover;
            IsInHover = false;
        }

        private Vector3 ComputeScaleWithAngularScale(Vector3 targetPoint)
        {
            float cursorDistance = Vector3.Distance(MainCameraCache.mainCamera.transform.position, targetPoint);
            float desiredScale = Utils.ScaleFromAngularSizeAndDistance(cursorAngularSize, cursorDistance);
            return Vector3.one * desiredScale;
        }

        private void UpdateVisual()
        {
            if (rayInteractor.State == InteractorState.Disabled)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            Vector3 EndPosition;
            Vector3 EndNormal;
            if (IsInHover && HoverNormal != Vector3.zero && HoverPosition != Vector3.zero)
            {
                EndPosition = HoverPosition;
                EndNormal = HoverNormal;
            }
            else
            {
                EndPosition = rayInteractor.End;
                EndNormal = rayInteractor.Forward;
            }

            if (rayInteractor.State == InteractorState.Select || rayInteractor.State == InteractorState.Hover)
            {
                if (rayInteractor.CollisionInfo != null)
                {
                    this.transform.position = customTargetPose != Pose.identity ? customTargetPose.position : EndPosition - EndNormal * 0.001f;
                    this.transform.rotation = customTargetPose != Pose.identity ? customTargetPose.rotation : Quaternion.LookRotation(rayInteractor.CollisionInfo.Value.Normal, Vector3.up);
                }
                else
                {
                    this.transform.position = customTargetPose != Pose.identity ? customTargetPose.position : EndPosition - EndNormal * 0.001f;
                    this.transform.rotation = customTargetPose != Pose.identity ? customTargetPose.rotation : Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                }
                this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                if (triggerCanDraggable == false && CanDrag())
                {
                    triggerCanDraggable = true;
                }
            }
            else if (rayInteractor.State == InteractorState.Normal)
            {
                this.transform.rotation = Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                this.transform.position = EndPosition - EndNormal * 0.001f;
                triggerCanDraggable = false;
            }
            if(headHandDriver==null 
               ||(!leftHandLost&&(headHandDriver.activeHand&ActiveHandType.LeftHand)!=0)
               ||(!rightHandLost&&(headHandDriver.activeHand&ActiveHandType.RightHand)!=0))
            //if (!leftHandLost || !rightHandLost)
            {
                if (customCursorVisual != null)
                {
                    disableCursor.gameObject.SetActive(false);
                    focusCursor.gameObject.SetActive(false);
                    pressCursor.gameObject.SetActive(false);
                    dragCursor.gameObject.SetActive(false);
                }
                else
                {
                    disableCursor.gameObject.SetActive(false);
                    if (triggerCanDraggable)
                    {
                        focusCursor.gameObject.SetActive(false);
                        pressCursor.gameObject.SetActive(false);
                        dragCursor.gameObject.SetActive(true);
                    }
                    else
                    {
                        bool press = rayInteractor.State == InteractorState.Select;
                        focusCursor.gameObject.SetActive(!press);
                        pressCursor.gameObject.SetActive(press);
                        dragCursor.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (customCursorVisual != null)
                    customCursorVisual.gameObject.SetActive(false);
                disableCursor.gameObject.SetActive(true);
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
                dragCursor.gameObject.SetActive(false);
            }

            if (customCursorVisual != null)
            {
                disableCursor.gameObject.SetActive(false);
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
                dragCursor.gameObject.SetActive(false);
                customCursorVisual.gameObject.SetActive(true);
            }
        }

        private bool CanDrag()
        {
            // IDraggable[] draggables = rayInteractor.Interactable?.GetComponentsInChildren<IDraggable>();
            // if (draggables != null && draggables.Length > 0)
            // {
            //     return true;
            // }
            return false;
        }
        private void UpdateVisualState(InteractorStateChangeArgs args) => UpdateVisual();

        #region ForCursorHoverPose
        private bool IsInHover;
        private Vector3 HoverPosition;
        private Vector3 HoverNormal;
        private void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                IsInHover = true;
                HoverPosition = Vector3.zero;
                HoverNormal = Vector3.zero;
            }
        }
        private void OnPointerExit(PointerEventData eventData)
        {
            IsInHover = false;
            HoverPosition = Vector3.zero;
            HoverNormal = Vector3.zero;
        }
        private void OnPointerHover(PointerEventData eventData)
        {
            HoverPosition = eventData.pointerCurrentRaycast.worldPosition;
            HoverNormal = eventData.pointerCurrentRaycast.worldNormal;
        }

        public void OnChangeHoldHandType(HandType hand)
        {

        }

        public void OnHandPress(HandType hand)
        {
            handPress = true;
        }

        public void OnHandRelease()
        {
            handPress = false;
        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }

        public void CustomCursorAlpha(float alpha)
        {

        }
        #endregion
    }
}
