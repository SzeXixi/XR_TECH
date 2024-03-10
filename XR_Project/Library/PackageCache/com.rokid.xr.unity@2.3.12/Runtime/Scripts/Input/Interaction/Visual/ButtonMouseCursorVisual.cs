using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class ButtonMouseCursorVisual : MonoBehaviour
    {
        [SerializeField]
        private RayInteractor rayInteractor;

        [SerializeField]
        private ButtonMouseSelector buttonMouseSelector;

        [Tooltip("光标Size")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [SerializeField]
        private Transform focusCursor;

        [SerializeField]
        private Transform pressCursor;

        private Material CursorMat;

        private bool IsPlayAnim;
        private float CurrentAnimTime;
        private float CurrentScale = 1.0f;

        private float AnimMinScale = 1f;
        private float AnimMaxScale = 2.2f;

        private float AnimMiddleTime = 0.2f;
        private float AnimTotalTime = 0.4f;

        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
            
            ButtonMouseEventInput.OnEnterLongPressState += EnterLongPressState;
            ButtonMouseEventInput.OnExitLongPressState += ExitLongPressState;
            ButtonMouseEventInput.OnReHoverState += OnReHoverState;
            
            CursorMat = GetComponentInChildren<SpriteRenderer>().material;

            IsPlayAnim = false;
        }
        
        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
            
            ButtonMouseEventInput.OnEnterLongPressState -= EnterLongPressState;
            ButtonMouseEventInput.OnExitLongPressState -= ExitLongPressState;
            ButtonMouseEventInput.OnReHoverState -= OnReHoverState;
        }

        private void OnEnable()
        {
            RKPointerLisener.OnGraphicPointerEnter += OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit += OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover += OnPointerHover;
            
            IsPlayAnim = true;
        }

        private void OnDisable()
        {
            RKPointerLisener.OnGraphicPointerEnter -= OnPointerEnter;
            RKPointerLisener.OnGraphicPointerExit -= OnPointerExit;
            RKPointerLisener.OnGraphicPointerHover -= OnPointerHover;
            
            IsPlayAnim = false;
            IsInHover = false;
        }

        private void Update()
        {
            if (IsPlayAnim)
            {
                if (CurrentAnimTime <= AnimMiddleTime)
                {
                    CurrentScale = AnimMinScale + (CurrentAnimTime / AnimMiddleTime) * (AnimMaxScale - AnimMinScale);
                    CurrentScale = Mathf.Clamp(CurrentScale, AnimMinScale, AnimMaxScale);
                    CurrentAnimTime += Time.deltaTime;

                    CursorMat?.SetFloat("_BlurSize", CurrentScale * 1.5f);
                }
                else if (CurrentAnimTime <= AnimTotalTime)
                {
                    CurrentScale = AnimMaxScale - ((CurrentAnimTime - AnimMiddleTime) / AnimMiddleTime) * (AnimMaxScale - AnimMinScale);
                    CurrentScale = Mathf.Clamp(CurrentScale, AnimMinScale, AnimMaxScale);
                    CurrentAnimTime += Time.deltaTime;

                    CursorMat?.SetFloat("_BlurSize", CurrentScale * 1.5f);
                }
                else
                {
                    IsPlayAnim = false;
                    CurrentScale = 1.0f;
                    CurrentAnimTime = 0.0f;
                    CursorMat?.SetFloat("_BlurSize", 0);
                }

                transform.localScale = ComputeScaleWithAngularScale(rayInteractor.End) * CurrentScale;
            }
        }

        private Vector3 ComputeScaleWithAngularScale(Vector3 targetPoint)
        {
            float cursorDistance = Vector3.Distance(MainCameraCache.mainCamera.transform.position, targetPoint);
            float desiredScale = Utils.ScaleFromAngularSizeAndDistance(cursorAngularSize, cursorDistance);
            return Vector3.one * desiredScale;
        }

        private void EnterLongPressState()
        {
            focusCursor.gameObject.SetActive(false);
            pressCursor.gameObject.SetActive(false);
            buttonMouseSelector.setClickEnable(false);
            // 进入锁定状态，取消选中状态
            rayInteractor.CancelWhenEnterLock();
        }

        private void OnReHoverState()
        {
            // 取消选择状态后，按键重新获取焦点
            rayInteractor.ReHoverWhenLock();
        }

        private void ExitLongPressState()
        {
            buttonMouseSelector.setClickEnable(true);
        }

        private void UpdateVisual()
        {
            if (rayInteractor.State == InteractorState.Disabled)
            {
                gameObject.SetActive(false);
                return;
            }

            if (ButtonMouseEventInput.Instance.IsInLockState())
            {
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

            this.transform.position = EndPosition - EndNormal * 0.001f;
            if (rayInteractor.State == InteractorState.Select || rayInteractor.State == InteractorState.Hover)
            {
                if (rayInteractor.CollisionInfo != null)
                {
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -EndNormal);
                    this.transform.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
                }
                else
                {
                    this.transform.rotation = Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                }
                if (!IsPlayAnim)
                {
                    this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                }
            }
            else if (rayInteractor.State == InteractorState.Normal && MainCameraCache.mainCamera != null)
            {
                this.transform.rotation = Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                if (!IsPlayAnim)
                {
                    this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
                }
            }

            bool press = rayInteractor.State == InteractorState.Select;
            focusCursor.gameObject.SetActive(!press);
            pressCursor.gameObject.SetActive(press);
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
        #endregion
    }

}
