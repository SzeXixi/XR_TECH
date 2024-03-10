using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{

    /// <summary>
    /// Draggable 拖拽组件
    /// </summary>
    [RequireComponent(typeof(RayInteractable))]
    public class Draggable : MonoBehaviour, IGesDragToTarget, IGesBeginDrag, IGesEndDragWithData, IBezierCurveDrag, IRayBeginDrag, IRayDrag, IRayEndDragWithData, IMouseBeginDrag, IMouseDrag, IMouseEndDragWithData, IDraggable
    {

        [SerializeField, Tooltip("拖拽平滑速度")]
        private int smoothSpeed = 10;
        [SerializeField, Tooltip("跟随该拖拽物体对象")]
        private Transform followObj;
        private Vector3 offsetPos;
        private Vector3 allDelta;
        private Vector3 oriPos;
        private Dictionary<int, BezierPointerData> bezierPointerDatas = new Dictionary<int, BezierPointerData>();
        private bool dragging = false;
        private bool gesDragging = false;

        private void Start()
        {
            if (followObj != null)
            {
                offsetPos = transform.InverseTransformPoint(followObj.position);
            }
        }

        public void SetOrUpdateFollowObj(Transform followObj)
        {
            this.followObj = followObj;
            offsetPos = transform.InverseTransformPoint(followObj.position);
        }

        public void OnGesBeginDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            oriPos = transform.position;
            AddBezierPointerData(eventData);
            dragging = true;
            gesDragging = true;
        }


        private void AddBezierPointerData(PointerEventData eventData)
        {
            BezierPointerData bezierPointerData = new BezierPointerData();
            bezierPointerData.pointerId = eventData.pointerId;
            bezierPointerData.hitLocalNormal = transform.InverseTransformVector(eventData.pointerCurrentRaycast.worldNormal);
            bezierPointerData.hitLocalPos = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            bezierPointerDatas.Add(bezierPointerData.pointerId, bezierPointerData);
        }

        private void RemoveBezierPointerData(PointerEventData eventData)
        {
            bezierPointerDatas.Remove(eventData.pointerId);
        }

        private BezierPointerData GetBezierPointerData(int pointerId)
        {
            if (bezierPointerDatas.TryGetValue(pointerId, out BezierPointerData bezierPointerData))
            {
                return bezierPointerData;
            }
            else
            {
                RKLog.KeyInfo($"====Draggable====: Can not find pointerId {pointerId}");
                return null;
            }
        }

        public void OnGesEndDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            RemoveBezierPointerData(eventData);
            if (bezierPointerDatas.Count == 0)
            {
                dragging = false;
                gesDragging = false;
            }
        }

        public void OnRayBeginDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            oriPos = transform.position;
            AddBezierPointerData(eventData);
            dragging = true;
        }

        public void OnRayDrag(Vector3 delta)
        {
            allDelta += delta;
        }

        public void OnRayEndDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            dragging = false;
            RemoveBezierPointerData(eventData);
        }

        public void OnMouseBeginDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            oriPos = transform.position;
            AddBezierPointerData(eventData);
            dragging = true;
        }

        public void OnMouseRayDrag(Vector3 delta)
        {
            allDelta += delta;
        }

        public void OnMouseEndDrag(PointerEventData eventData)
        {
            allDelta = Vector3.zero;
            dragging = false;
            RemoveBezierPointerData(eventData);
        }

        public bool IsEnablePinchBezierCurve()
        {
            return true && this.enabled;
        }

        public bool IsEnableGripBezierCurve()
        {
            return true && this.enabled;
        }

        public bool IsInBezierCurveDragging()
        {
            return dragging && this.enabled;
        }

        public Vector3 GetBezierCurveEndPoint(int pointerId)
        {
            BezierPointerData pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null)
            {
                return transform.TransformPoint(pointerData.hitLocalPos);
            }
            else
            {
                return transform.TransformPoint(Vector3.zero);
            }
        }

        public Vector3 GetBezierCurveEndNormal(int pointerId)
        {
            BezierPointerData pointerData = GetBezierPointerData(pointerId);
            if (pointerData != null)
            {
                return transform.TransformPoint(pointerData.hitLocalNormal);
            }
            else
            {
                return transform.TransformPoint(Vector3.zero);
            }
        }

        private void Update()
        {
            if (this.enabled && dragging && !gesDragging)
            {
                Vector3 targetPos = oriPos + allDelta;
                this.transform.position = Vector3.Slerp(this.transform.position, targetPos, Time.deltaTime * smoothSpeed);
                // this.transform.position = Utils.SmoothTo(this.transform.position, targetPos, 0.01f, Time.deltaTime);
                if (followObj != null)
                    followObj.position = transform.TransformPoint(offsetPos);
            }
        }

        public void OnGesDragToTarget(Vector3 targetPos)
        {
            if (this.enabled == false)
                return;
            this.transform.position = Vector3.Slerp(this.transform.position, targetPos, Time.deltaTime * smoothSpeed);
            // this.transform.position = Utils.SmoothTo(this.transform.position, targetPos, 0.01f, Time.deltaTime);
            if (followObj != null)
                followObj.position = transform.TransformPoint(offsetPos);
        }
    }
}

