using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class RayVisual : MonoBehaviour
    {
        [SerializeField]
        private RayInteractor rayInteractor;

        [SerializeField]
        private LineRenderer rayRenderer;

        [Tooltip("射线最小长度")]
        [SerializeField]
        protected float minRayLength;
        [Tooltip("射线最长长度")]
        [SerializeField]
        protected float maxRayLength;

        [Tooltip("射线默认长度")]
        [SerializeField]
        protected float normalRayLength;
        [SerializeField]
        private Transform rayOrigin;

        private Material rayMat;

        private bool dragging;

        [SerializeField]
        private HandType hand;

        private bool raySleep;


        private void Start()
        {
            rayInteractor.WhenPostprocessed += UpdateVisual;
            rayInteractor.WhenStateChanged += HandleStateChanged;

            RKPointerLisener.OnPointerDragEnd += OnPointerDragEnd;
            RKPointerLisener.OnPointerDragBegin += OnPointerDragBegin;

            InteractorStateChange.OnHandDragStatusChanged += OnHandDragStatusChanged;
            ThreeDofEventInput.OnRaySleep += OnRaySleep;
            rayMat = rayRenderer.material;
        }



        private void OnDestroy()
        {
            rayInteractor.WhenPostprocessed -= UpdateVisual;
            rayInteractor.WhenStateChanged -= HandleStateChanged;

            RKPointerLisener.OnPointerDragEnd -= OnPointerDragEnd;
            RKPointerLisener.OnPointerDragBegin -= OnPointerDragBegin;
            ThreeDofEventInput.OnRaySleep -= OnRaySleep;
            InteractorStateChange.OnHandDragStatusChanged -= OnHandDragStatusChanged;
        }

        private void OnRaySleep(bool sleeping)
        {
            raySleep = sleeping;
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
                        curentDragObj = GetCurrentDragObj();
                    }
                }
                else
                {
                    allCanDragObjs = null;
                    curentDragObj = null;
                    this.dragging = false;
                }
            }
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

        private void OnPointerDragBegin(PointerEventData pointerEventData)
        {
            if (hand == HandType.None)
            {
                dragging = true;
                if (rayInteractor.Interactable != null)
                {
                    allCanDragObjs = rayInteractor.Interactable.GetComponentsInChildren<IBezierCurveDrag>();
                    curentDragObj = GetCurrentDragObj();
                }
            }
        }

        private void OnPointerDragEnd(PointerEventData pointerEventData)
        {
            if (hand == HandType.None)
            {
                allCanDragObjs = null;
                curentDragObj = null;
                dragging = false;
            }
        }


        private void HandleStateChanged(InteractorStateChangeArgs args)
        {
            UpdateVisual();
        }

        private bool IsBezierCurveDragging()
        {
            return dragging && curentDragObj != null && curentDragObj.IsInBezierCurveDragging() && (curentDragObj.IsEnablePinchBezierCurve() || curentDragObj.IsEnableGripBezierCurve());
        }

        private void UpdateVisual()
        {
            LogHandRayInfo();
            rayRenderer.enabled = !raySleep;
            if (IsBezierCurveDragging())
            {
                // RKLog.KeyInfo($"====RayVisual==== UpdateVisual {rayInteractor.realId}");
                SetLineEndPoints(curentDragObj.GetBezierCurveEndPoint(rayInteractor.realId));
                DrawBezierRay();
                return;
            }
            else if (rayInteractor.State == InteractorState.Disabled)
            {
                rayRenderer.enabled = false;
                return;
            }
            switch (rayInteractor.State)
            {
                case InteractorState.Normal:
                    DrawRay(normalRayLength, false);
                    break;
                case InteractorState.Hover:
                    if (IsInHover && HoverPosition != Vector3.zero && HoverNormal != Vector3.zero)
                    {
                        DrawRay(transform.InverseTransformPoint(HoverPosition), false);
                    }
                    else
                    {
                        if (rayInteractor.CollisionInfo != null)
                        {
                            DrawRay(transform.InverseTransformPoint(rayInteractor.End).z, false);
                        }
                        else
                        {
                            DrawRay(normalRayLength, false);
                        }
                    }
                    break;
                case InteractorState.Select:
                    if (IsInHover && HoverPosition != Vector3.zero && HoverNormal != Vector3.zero)
                    {
                        DrawRay(transform.InverseTransformPoint(HoverPosition), true);
                    }
                    else
                    {
                        DrawRay(transform.InverseTransformPoint(rayInteractor.End).z, true);
                    }
                    break;
            }
        }

        #region LogHandRayInfo
        private float logTime = 5;
        private float logElapsedTime = 0;
        private void LogHandRayInfo()
        {
            logElapsedTime += Time.deltaTime;
            if (logElapsedTime > logTime)
            {
                logElapsedTime = 0;
                RKLog.KeyInfo($"====RayVisual==== KeyInfo handType: {hand}, renderPosition: {rayRenderer.GetPosition(0)},{rayRenderer.GetPosition(1)}, rayVisualPosition:{transform.position},dragging:{dragging},IsBezierCurveDragging: {IsBezierCurveDragging()},InteractorState:{rayInteractor.State} ");
            }
        }
        #endregion

        protected void DrawRay(float length, bool isPress)
        {
            rayRenderer.positionCount = 2;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.RepeatPerSegment;
            rayRenderer.SetPosition(0, rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayRenderer.SetPosition(1, new Vector3(0, 0, length > 0 ? length : 0));
            rayMat?.SetFloat("_Length", length);
            rayMat?.SetFloat("_IsPress", isPress ? 1 : 0);
        }

        protected void DrawRay(Vector3 endPosition, bool isPress)
        {
            float length = Vector3.Distance(rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0), endPosition);
            rayRenderer.positionCount = 2;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.RepeatPerSegment;
            rayRenderer.SetPosition(0, rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayRenderer.SetPosition(1, length > 0 ? endPosition : rayOrigin != null ? rayOrigin.localPosition : new Vector3(0, 0, 0));
            rayMat?.SetFloat("_Length", length);
            rayMat?.SetFloat("_IsPress", isPress ? 1 : 0);
        }

        #region DrawBezierCurve
        [SerializeField]
        private float lineStartClamp = 0.0001f;
        [SerializeField]
        private float lineEndClamp = 0.8028384f;
        private float startPointLerp = 0.33f;
        private float endPointLerp = 0.66f;
        private Vector3[] Points = new Vector3[4];
        private Vector3[] positions;
        private int lineStepCount = 16;
        private int LineStepCount
        {
            get => lineStepCount;
            set => lineStepCount = Mathf.Clamp(value, 2, 2048);
        }

        private IBezierCurveDrag[] allCanDragObjs;

        private IBezierCurveDrag curentDragObj;

        private IBezierCurveDrag GetCurrentDragObj()
        {
            if (allCanDragObjs == null || allCanDragObjs.Length == 0)
            {
                return null;
            }
            foreach (var Drag in allCanDragObjs)
            {
                if (Drag.IsInBezierCurveDragging())
                {
                    return Drag;
                }
            }
            return null;
        }
        private int PointCount { get { return 4; } }

        public Vector3 FirstPoint
        {
            get => GetPoint(0);
            set => SetPoint(0, value);
        }

        public Vector3 LastPoint
        {
            get => GetPoint(PointCount - 1);
            set => SetPoint(PointCount - 1, value);
        }

        private void SetLinePoints(Vector3 startPoint, Vector3 endPoint)
        {
            FirstPoint = startPoint;
            LastPoint = endPoint;
        }

        private void SetLineEndPoints(Vector3 endPoint)
        {
            LastPoint = endPoint;
        }

        private Vector3 GetPoint(int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return Vector3.zero;
            }

            Vector3 point = Points[pointIndex];
            point = transform.TransformPoint(point);
            return point;
        }

        public Vector3 GetPoint(float normalizedLength)
        {
            normalizedLength = Mathf.Lerp(lineStartClamp, lineEndClamp, Mathf.Clamp01(normalizedLength));
            Vector3 point = GetPointInternal(normalizedLength);
            point = transform.TransformPoint(point);
            return point;
        }

        private void SetPoint(int pointIndex, Vector3 point)
        {
            if (pointIndex < 0 || pointIndex >= PointCount)
            {
                Debug.LogError("Invalid point index");
                return;
            }

            point = transform.InverseTransformPoint(point);
            Points[pointIndex] = point;
        }

        private Vector3 InterpolateBezierPoints(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float normalizedLength)
        {
            float invertedDistance = 1f - normalizedLength;
            return invertedDistance * invertedDistance * invertedDistance * point1 +
                3f * invertedDistance * invertedDistance * normalizedLength * point2 +
                3f * invertedDistance * normalizedLength * normalizedLength * point3 +
                normalizedLength * normalizedLength * normalizedLength * point4;
        }

        private Vector3 GetPointInternal(float normalizedDistance)
        {
            return InterpolateBezierPoints(Points[0], Points[1], Points[2], Points[3], normalizedDistance);
        }

        private void DrawBezierRay()
        {
            float distance = Vector3.Distance(transform.position, LastPoint);
            Vector3 startPoint = FirstPoint;
            Vector3 expectedPoint = startPoint + transform.forward * distance;

            SetPoint(1, Vector3.Lerp(startPoint, expectedPoint, startPointLerp));
            expectedPoint = Vector3.Lerp(expectedPoint, LastPoint, endPointLerp);
            SetPoint(2, Vector3.Lerp(startPoint, expectedPoint, endPointLerp));

            rayRenderer.positionCount = LineStepCount;
            rayRenderer.useWorldSpace = false;
            rayRenderer.textureMode = LineTextureMode.Stretch;
            if (positions == null || positions.Length != rayRenderer.positionCount)
            {
                positions = new Vector3[rayRenderer.positionCount];
            }

            for (int i = 0; i < positions.Length; i++)
            {
                float normalizedDistance = (1f / (LineStepCount - 1)) * i;
                positions[i] = transform.InverseTransformPoint(GetPoint(normalizedDistance));
            }

            // Set positions
            rayRenderer.positionCount = positions.Length;
            rayRenderer.SetPositions(positions);

            rayMat?.SetFloat("_IsPress", 1);
        }
        #endregion

        #region ForRayHoverPose
        private bool IsInHover;
        private Vector3 HoverPosition;
        private Vector3 HoverNormal;
        private void OnPointerEnter(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand && eventData.pointerCurrentRaycast.gameObject != null)
            {
                IsInHover = true;
                HoverPosition = Vector3.zero;
                HoverNormal = Vector3.zero;
            }
        }
        private void OnPointerExit(PointerEventData eventData)
        {
            if (RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                IsInHover = false;
                HoverPosition = Vector3.zero;
                HoverNormal = Vector3.zero;
            }
        }
        private void OnPointerHover(PointerEventData eventData)
        {
            if (IsInHover && RayInteractor.GetHandTypeByIdentifier(eventData.pointerId) == hand)
            {
                HoverPosition = eventData.pointerCurrentRaycast.worldPosition;
                HoverNormal = eventData.pointerCurrentRaycast.worldNormal;
            }
        }
        #endregion
    }
}
