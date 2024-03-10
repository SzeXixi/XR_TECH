using System;
using Rokid.UXR.Utility;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class GesPinchRayCaster : BaseRayCaster
    {
        [SerializeField]
        private HandType hand;

        [SerializeField, Tooltip("是否过滤Grip手势类型的拖拽")]
        private bool filterGripToDrag;

        protected override void Init()
        {
            base.Init();
            inputOverride = GetComponent<GesturePinchInput>();
            if (inputOverride == null)
                inputOverride = gameObject.AddComponent<GesturePinchInput>();
            GetComponent<GesturePinchInput>().SetHandType(hand);
            pointerEnter = "OnGesPinchEnter";
            pointerExit = "OnGesPinchExit";
            pointerHover = "OnGesPinchHover";
            pointerClick = "OnGesPinchClick";
            dragBegin = "OnGesPinchBeginDrag";
            drag = "OnGesPinchDrag";
            dragEnd = "OnGesPinchEndDrag";
        }

        private bool GetWorldPointInRectangle(RectTransform rect, Ray ray, out Vector3 worldPoint)
        {
            worldPoint = Vector2.zero;
            Plane plane = new Plane(rect.rotation * Vector3.back, rect.position);
            float enter = 0f;
            Ray inverseRay = new Ray(ray.origin, -ray.direction);
            if (!plane.Raycast(ray, out enter))
            {
                worldPoint = ray.GetPoint(enter);
                return true;
            }
            else if (!plane.Raycast(inverseRay, out enter))
            {
                worldPoint = inverseRay.GetPoint(enter);
                return true;
            }
            return false;
        }

        protected override bool ProcessBeginDrag(Ray ray)
        {
            RectTransform rect = m_FirstSelectedObj.GetComponent<RectTransform>();
            if (GetWorldPointInRectangle(rect, ray, out Vector3 worldPoint))
            {
                RKLog.Debug($"====NearDragRayCaster==== ProcessBeginDrag");
                result.worldPosition = worldPoint;
                pointerEventData.pointerCurrentRaycast = result;
                m_SelectedObj.SendMessageUpwards(dragBegin, pointerEventData, SendMessageOptions.DontRequireReceiver);
                RKPointerLisener.OnPointerDragBegin?.Invoke(pointerEventData);
            }
            return true;
        }

        protected override bool ProcessEndDrag(Ray ray)
        {
            RKLog.Debug($"====NearDragRayCaster==== ProcessEndDrag");
            m_SelectedObj.SendMessageUpwards(dragEnd, pointerEventData, SendMessageOptions.DontRequireReceiver);
            RKPointerLisener.OnPointerDragEnd?.Invoke(pointerEventData);
            return true;
        }

        /// <summary>
        /// 重写处理拖拽的逻辑
        /// </summary>
        protected override bool ProcessDrag(Ray ray)
        {
            if (filterGripToDrag && GesEventInput.Instance.GetGestureType(hand) == GestureType.Grip)
            {
                return true;
            }
            RectTransform rect = m_FirstSelectedObj.GetComponent<RectTransform>();
            if (GetWorldPointInRectangle(rect, ray, out Vector3 worldPoint))
            {
                RKLog.Debug($"====NearDragRayCaster==== ProcessDrag");
                result.worldPosition = worldPoint;
                pointerEventData.pointerCurrentRaycast = result;
                m_SelectedObj.SendMessageUpwards(drag, pointerEventData, SendMessageOptions.DontRequireReceiver);
                RKPointerLisener.OnPointerDrag?.Invoke(pointerEventData);
            }
            return true;
        }

        protected override Vector3 CalDragDelta()
        {
            return Vector3.one;
        }

        protected override void OnFirstSelect()
        {

        }

        protected override Camera GetEventCamera()
        {
            return MainCameraCache.mainCamera;
        }
    }
}
