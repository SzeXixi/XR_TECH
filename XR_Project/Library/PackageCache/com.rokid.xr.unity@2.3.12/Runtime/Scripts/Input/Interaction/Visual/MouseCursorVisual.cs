using UnityEngine;
using Rokid.UXR.Utility;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class MouseCursorVisual : MonoBehaviour, ICustomCursorVisual
    {
        [SerializeField]
        private RayInteractor rayInteractor;

        [Tooltip("光标Size")]
        [SerializeField]
        private float cursorAngularSize = 100;

        [SerializeField]
        private Transform focusCursor;

        [SerializeField]
        private Transform pressCursor;

        private Transform customCursorVisual;
        public Transform CustomCursorVisual { get { return customCursorVisual; } set { customCursorVisual = value; customCursorVisual?.SetParent(this.transform); } }

        private Pose customTargetPose = Pose.identity;
        public Pose CustomTargetPose { get => customTargetPose; set => customTargetPose = value; }

        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += UpdateVisualState;
        }

        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= UpdateVisualState;
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
                    Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, -rayInteractor.CollisionInfo.Value.Normal);
                    this.transform.eulerAngles = customTargetPose != Pose.identity ? customTargetPose.rotation.eulerAngles : new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
                }
                else
                {
                    this.transform.position = customTargetPose != Pose.identity ? customTargetPose.position : EndPosition - EndNormal * 0.001f;
                    this.transform.rotation = customTargetPose != Pose.identity ? customTargetPose.rotation : Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                }
                this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
            }
            else if (rayInteractor.State == InteractorState.Normal)
            {
                this.transform.position = EndPosition;
                this.transform.rotation = Quaternion.LookRotation(EndPosition - MainCameraCache.mainCamera.transform.position, Vector3.up);
                this.transform.localScale = ComputeScaleWithAngularScale(EndPosition);
            }

            bool press = rayInteractor.State == InteractorState.Select;
            focusCursor.gameObject.SetActive(!press);
            pressCursor.gameObject.SetActive(press);

            if (customCursorVisual != null)
            {
                focusCursor.gameObject.SetActive(false);
                pressCursor.gameObject.SetActive(false);
            }
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

        public void CustomCursorAlpha(float alpha)
        {

        }
        #endregion
    }
}
