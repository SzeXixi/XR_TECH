using Rokid.UXR.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class CursorVisual : AutoInjectBehaviour, ICustomCursorVisual
    {

        [SerializeField]
        private HandType hand;
        [SerializeField]
        private RayInteractor rayInteractor;

        [Tooltip("光标Size")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [Autowrited("Cursor_Focus")]
        private Transform focusCursor;

        [Autowrited("Cursor_Press")]
        private Transform pressCursor;
        private Transform customCursorVisual;

        public Transform CustomCursorVisual { get { return customCursorVisual; } set { customCursorVisual = value; customCursorVisual?.SetParent(this.transform); } }
        private Pose customTargetPose = Pose.identity;
        public Pose CustomTargetPose { get => customTargetPose; set => customTargetPose = value; }
        private bool dragging;
        private IBezierCurveDrag[] allCanDragObjs;
        private IBezierCurveDrag currentDragObj;
        private float scaleRelease = 1.5f;
        private float scalePress = 1.0f;
        private float startSmoothPinchDistance = 0.06f;

        private void Start()
        {
            InteractorStateChange.OnHandDragStatusChanged += OnHandDragStatusChanged;
            RKPointerLisener.OnPointerDragBegin += OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd += OnPointerDragEnd;
        }

        private void OnHandDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == this.hand)
            {
                if (dragging)
                {
                    this.dragging = true;
                    if (rayInteractor.Interactable != null)
                    {
                        allCanDragObjs = rayInteractor.Interactable.GetComponentsInChildren<IBezierCurveDrag>();
                        currentDragObj = GetCurrentDragObj();
                    }
                }
                else
                {
                    this.dragging = false;
                    currentDragObj = null;
                    allCanDragObjs = null;
                }
            }
        }

        private void OnDestroy()
        {
            InteractorStateChange.OnHandDragStatusChanged -= OnHandDragStatusChanged;
            RKPointerLisener.OnPointerDragBegin -= OnPointerDragBegin;
            RKPointerLisener.OnPointerDragEnd -= OnPointerDragEnd;
        }

        private void OnEnable()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
            RKPointerLisener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit += OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover += OnPointerHover;
        }

        private void OnDisable()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
            RKPointerLisener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover -= OnPointerHover;
            isInHover = false;
        }

        private void OnPointerDragEnd(PointerEventData eventData)
        {
            if (hand == HandType.None)
            {
                RKLog.Info("====CursorVisual====: OnPointerDragEnd");
                dragging = false;
                currentDragObj = null;
                allCanDragObjs = null;
            }
        }

        private void OnPointerDragBegin(PointerEventData eventData)
        {
            if (hand == HandType.None)
            {
                dragging = true;
                if (rayInteractor.Interactable != null)
                {
                    allCanDragObjs = rayInteractor.Interactable.GetComponentsInChildren<IBezierCurveDrag>();
                    currentDragObj = GetCurrentDragObj();
                }
            }
        }

        private bool HandDragging(GestureType GesType)
        {
            if (dragging && hand != HandType.None)
            {
                if (GesEventInput.Instance.GetGestureType(hand) == GesType)
                {
                    return true;
                }
            }
            return false;
        }

        private IBezierCurveDrag GetCurrentDragObj()
        {
            if (allCanDragObjs == null || allCanDragObjs.Length == 0)
            {
                return null;
            }
            foreach (var dragObj in allCanDragObjs)
            {
                if (dragObj.IsInBezierCurveDragging())
                {
                    return dragObj;
                }
            }
            return null;
        }

        private bool IsBezierDragging()
        {
            if (currentDragObj == null)
            {
                currentDragObj = GetCurrentDragObj();
            }

            if (dragging && currentDragObj != null)
            {
                return true;
            }

            return false;
        }

        private Vector3 ComputeScaleWithAngularScale(Vector3 targetPoint)
        {
            if (MainCameraCache.mainCamera != null)
            {
                float cursorDistance = Vector3.Distance(MainCameraCache.mainCamera.transform.position, targetPoint);
                float desiredScale = Utils.ScaleFromAngularSizeAndDistance(cursorAngularSize, cursorDistance);
                return Vector3.one * desiredScale;
            }
            return Vector3.zero;
        }

        private int GetPointerId()
        {
            // RKLog.KeyInfo($"====CursorVisual==== GetPointerId {rayInteractor.realId}");
            return rayInteractor.realId;
        }

        private void UpdateVisual()
        {
            if (rayInteractor.CollisionInfo == null || rayInteractor.State == InteractorState.Disabled || IsBezierDragging())
            {
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
                return;
            }

            if (isInHover && hoverNormal != Vector3.zero && hoverPosition != Vector3.zero)
            {
                this.transform.position = hoverPosition;
                this.transform.rotation = Quaternion.LookRotation(hoverNormal, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(hoverPosition);
            }
            else
            {
                Vector3 collisionNormal = customTargetPose != Pose.identity ? customTargetPose.rotation * Vector3.forward : IsBezierDragging() ? currentDragObj.GetBezierCurveEndNormal(GetPointerId()) : rayInteractor.CollisionInfo.Value.Normal;
                Vector3 collisionPosition = customTargetPose != Pose.identity ? customTargetPose.position : IsBezierDragging() ? currentDragObj.GetBezierCurveEndPoint(GetPointerId()) : rayInteractor.End;
                this.transform.position = collisionPosition + collisionNormal * 0.001f;
                this.transform.rotation = Quaternion.LookRotation(collisionNormal, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(collisionPosition);
            }

            bool press = rayInteractor.State == InteractorState.Select || IsBezierDragging();
            focusCursor.gameObject.SetActive(!press);
            pressCursor.gameObject.SetActive(press);
            if (customCursorVisual != null)
            {
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
            }

            if (hand != HandType.None)
            {
                if (!press)
                {
                    Vector3 index_tip = GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_TIP, hand).position;
                    Vector3 thumb_tip = GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.THUMB_TIP, hand).position;
                    float sqrDistance = Vector3.SqrMagnitude(index_tip - thumb_tip);
                    if (sqrDistance > startSmoothPinchDistance * startSmoothPinchDistance)
                    {
                        this.transform.localScale *= scaleRelease;
                    }
                    else
                    {
                        float pow = Mathf.Max(scalePress, scaleRelease * (sqrDistance / (startSmoothPinchDistance * startSmoothPinchDistance)));
                        this.transform.localScale *= pow;
                    }
                }
                else
                {
                    this.transform.localScale *= scalePress;
                }
            }
        }

        private void UpdateVisualState(InteractorStateChangeArgs args) => UpdateVisual();

        #region ForCursorHoverPose
        private bool isInHover;
        private Vector3 hoverPosition;
        private Vector3 hoverNormal;

        private void OnPointerEnter(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand && eventData.pointerCurrentRaycast.gameObject != null)
            {
                isInHover = true;
                hoverPosition = Vector3.zero;
                hoverNormal = Vector3.zero;
            }
        }
        private void OnPointerExit(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                isInHover = false;
                hoverPosition = Vector3.zero;
                hoverNormal = Vector3.zero;
            }
        }
        private void OnPointerHover(PointerEventData eventData)
        {
            if (isInHover && RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                hoverPosition = eventData.pointerCurrentRaycast.worldPosition;
                hoverNormal = eventData.pointerCurrentRaycast.worldNormal;
            }
        }

        private SpriteRenderer[] spriteRenders;
        public void CustomCursorAlpha(float alpha)
        {
            if (spriteRenders == null)
            {
                spriteRenders = GetComponentsInChildren<SpriteRenderer>(true);
            }
            if (spriteRenders != null)
            {
                for (int i = 0; i < spriteRenders.Length; i++)
                {
                    Color color = spriteRenders[i].color;
                    spriteRenders[i].color = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
        #endregion
    }
}

