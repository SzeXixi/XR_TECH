using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using static UnityEngine.EventSystems.PointerEventData;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public abstract class BaseRayCaster : MonoBehaviour
    {
        /// <summary>
        /// Threshold for determining whether to start dragging
        /// </summary>
        public float m_DragThreshold = 0.01f;
        /// <summary>
        /// Cache the current hovering object
        /// </summary>
        [SerializeField]
        protected GameObject m_HoveringObj;

        /// <summary>
        /// Cache selected object
        /// </summary>
        [SerializeField]
        protected GameObject m_SelectedObj;

        [SerializeField]
        protected GameObject m_FirstSelectedObj;
        [Tooltip("This attribute only affects UI graphics, detecting the front and back sides of the UI")]

        [SerializeField]
        private bool ignoreReversedGraphics = true;
        [Tooltip("Is detect UI graphics")]
        [SerializeField]
        private bool raycastGraphic = true;

        [Tooltip("Is detect physical collisions")]
        [SerializeField]
        private bool raycastPhysical = true;
        [SerializeField]
        protected Transform rayOrigin;
        protected Ray ray;
        protected Vector3 oriHitPoint;

        /// <summary> The hits. </summary>
        private static readonly RaycastHit[] hits = new RaycastHit[64];

        /// <summary>
        /// 点击的阈值
        /// </summary>
        protected float clickTime = 0.5f;
        protected float pressTime = 0;

        /// <summary>
        /// 是否正在在拖拽中
        /// </summary>
        [SerializeField]
        protected bool dragging = false;

        /// <summary>
        /// 缓存开始拖拽的时候射线原点到触碰点的距离
        /// </summary>
        protected float oriHitPointDis = 0;

        [SerializeField]
        protected BaseInput m_InputOverride;

        private BaseInput m_DefaultInput;

        [SerializeField]
        protected RaycastResult result;
        [SerializeField]
        protected PointerEventData pointerEventData;
        [SerializeField]
        protected RayInteractor rayInteractor;

        [SerializeField]
        protected Camera eventCamera;

        [SerializeField]
        protected bool sendGlobalEvent = true;


        protected Vector2 screenCenterPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        public LayerMask raycastMask = (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4 | 1 << 5 | 1 << 6 | 1 << 7);

        private static readonly Comparison<RaycastResult> s_RaycastComparer = RaycastComparer;

        protected readonly List<RaycastResult> sortedRaycastResults = new List<RaycastResult>();


        #region Event names can be overwritten
        protected string pointerEnter = "OnRayPointerEnter";
        protected string pointerExit = "OnRayPointerExit";
        protected string pointerHover = "OnRayPointerHover";
        protected string pointerClick = "OnRayPointerClick";
        protected string dragBegin = "OnRayBeginDrag";
        protected string drag = "OnRayDrag";
        protected string dragEnd = "OnRayEndDrag";

        #endregion


        public BaseInput input
        {
            get
            {
                if (m_InputOverride != null)
                {
                    return m_InputOverride;
                }
                if (m_DefaultInput == null)
                {
                    BaseInput[] components = GetComponents<BaseInput>();
                    foreach (BaseInput baseInput in components)
                    {
                        if (baseInput != null && baseInput.GetType() == typeof(BaseInput))
                        {
                            m_DefaultInput = baseInput;
                            break;
                        }
                    }
                    if (m_DefaultInput == null)
                    {
                        m_DefaultInput = base.gameObject.AddComponent<BaseInput>();
                    }
                }
                return m_DefaultInput;
            }
        }

        public BaseInput inputOverride
        {
            get
            {
                return m_InputOverride;
            }
            set
            {
                m_InputOverride = value;
            }
        }

        protected virtual void Init()
        {
            pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.button = InputButton.Left;
            if (rayInteractor == null)
            {
                rayInteractor = GetComponent<RayInteractor>();
            }
        }

        private int GetRayIdentifier()
        {
            if (rayInteractor == null)
            {
                return 0;
            }
            else
            {
                return rayInteractor.realId;
            }
        }

        protected virtual void Start()
        {
            Init();
            if (eventCamera == null)
            {
                eventCamera = GetEventCamera();
            }
        }

        protected virtual Camera GetEventCamera()
        {
            return MainCameraCache.mainCamera;
        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnDisable()
        {
            if (dragging)
            {
                RKLog.Debug("====BaseRayCaster====: dragEnd");
                if (!ProcessEndDrag(ray))
                    m_SelectedObj?.SendMessageUpwards(dragEnd, pointerEventData, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (pressTime < clickTime && m_SelectedObj == result.gameObject && result.gameObject != null)
                    //发送点击事件
                    m_SelectedObj?.SendMessageUpwards(pointerClick, SendMessageOptions.DontRequireReceiver);
            }
            dragging = false;
            m_SelectedObj = null;
            m_FirstSelectedObj = null;
        }

        protected void UpdatePointerEventData()
        {
            pointerEventData.pointerCurrentRaycast = result;
            pointerEventData.pointerId = GetRayIdentifier();
        }



        private void Update()
        {
            if (rayOrigin != null)
            {
                ray.origin = rayOrigin.position;
                ray.direction = rayOrigin.forward;
            }
            else
            {
                ray.origin = transform.position;
                ray.direction = transform.forward;
            }
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
            if (input.GetMouseButtonDown(0))
            {
                if (!TriggerPointerDown())
                {
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
                }
            }
            else if (!dragging)
            {
                Raycast(ray, Mathf.Infinity, sortedRaycastResults);
                result = FirstRaycastResult();
                UpdatePointerEventData();
                if (input.GetMouseButtonUp(0) && result.gameObject == null)
                {
                    ProcessNothingUpEvent(pointerEventData);
                }
                if (result.gameObject != null)
                {
                    if (m_HoveringObj == null)
                    {
                        m_HoveringObj = result.gameObject;
                        // RKLog.Debug($"====BaseRayCaster====: pointerEnter:{m_HoveringObj.name}");
                        m_HoveringObj.SendMessageUpwards(pointerEnter, SendMessageOptions.DontRequireReceiver);
                        if (sendGlobalEvent)
                        {
                            RKPointerLisener.OnPointerEnter?.Invoke(pointerEventData);
                            RKPointerLisener.OnPhysicalPointerEnter?.Invoke(pointerEventData);
                        }
                    }
                    else
                    {
                        if (m_HoveringObj != result.gameObject)
                        {
                            // RKLog.Debug($"====BaseRayCaster====: pointerExit: {m_HoveringObj.name}");
                            m_HoveringObj.SendMessageUpwards(pointerExit, SendMessageOptions.DontRequireReceiver);
                            if (sendGlobalEvent)
                            {
                                RKPointerLisener.OnPointerExit?.Invoke(pointerEventData, m_HoveringObj);
                                RKPointerLisener.OnPhysicalPointerExit?.Invoke(pointerEventData, m_HoveringObj);
                            }

                            m_HoveringObj = result.gameObject;
                            // RKLog.Debug($"====BaseRayCaster====: pointerEnter {m_HoveringObj.name}");
                            m_HoveringObj.SendMessageUpwards(pointerEnter, SendMessageOptions.DontRequireReceiver);
                            if (sendGlobalEvent)
                            {
                                RKPointerLisener.OnPointerEnter?.Invoke(pointerEventData);
                                RKPointerLisener.OnPhysicalPointerEnter?.Invoke(pointerEventData);
                            }
                        }
                        m_HoveringObj.SendMessageUpwards(pointerHover, result, SendMessageOptions.DontRequireReceiver);
                        if (sendGlobalEvent)
                        {
                            RKPointerLisener.OnPointerHover?.Invoke(pointerEventData);
                            RKPointerLisener.OnPhysicalPointerHover?.Invoke(pointerEventData);
                        }
                        if (input.GetMouseButton(0) && !dragging)
                        {
                            pressTime += Time.deltaTime;
                            if (m_FirstSelectedObj == m_HoveringObj)
                            {
                                m_SelectedObj = m_HoveringObj;
                                Vector3 delta = CalDragDelta();//拖拽移动的向量
                                // RKLog.Debug($"====BaseRayCaster====: JudgeDrag {delta},{m_DragThreshold}");
                                if (CanDrag(delta))
                                {
                                    OnBeginDrag();
                                    // RKLog.Debug("====BaseRayCaster====: dragBegin");
                                    if (!ProcessBeginDrag(ray))
                                    {
                                        m_SelectedObj.SendMessageUpwards(dragBegin, pointerEventData, SendMessageOptions.DontRequireReceiver);
                                        if (sendGlobalEvent)
                                            RKPointerLisener.OnPointerDragBegin?.Invoke(pointerEventData);
                                    }
                                    dragging = true;
                                }
                            }
                        }
                    }
                }
                else if (m_HoveringObj != null)
                {
                    // RKLog.Debug($"====BaseRayCaster====: pointerExit: {m_HoveringObj.name}");
                    m_HoveringObj.SendMessageUpwards(pointerExit, SendMessageOptions.DontRequireReceiver);
                    RKPointerLisener.OnPointerExit?.Invoke(pointerEventData, m_HoveringObj);
                    RKPointerLisener.OnPhysicalPointerExit?.Invoke(pointerEventData, m_HoveringObj);
                    result.gameObject = null;
                    m_HoveringObj = null;
                }
            }

            if (dragging)
            {
                ProcessDrag(ray);
                if (sendGlobalEvent)
                    RKPointerLisener.OnPointerDrag?.Invoke(pointerEventData);
            }

            //处理点击释放逻辑
            if (input.GetMouseButtonUp(0))
            {
                if (dragging == false)
                {
                    if (pressTime < clickTime && m_SelectedObj == result.gameObject && result.gameObject != null)
                    {
                        //发送点击事件
                        m_SelectedObj.SendMessageUpwards(pointerClick, SendMessageOptions.DontRequireReceiver);
                    }
                    m_SelectedObj = null;
                }
            }

            //处理拖拽释放逻辑
            if (dragging && DragRelease())
            {
                if (sendGlobalEvent)
                    RKPointerLisener.OnPointerDragEnd?.Invoke(pointerEventData);
                if (m_SelectedObj != null)
                {
                    // RKLog.Debug("====BaseRayCaster====: dragEnd 2");
                    if (!ProcessEndDrag(ray))
                    {
                        m_SelectedObj.SendMessageUpwards(dragEnd, pointerEventData, SendMessageOptions.DontRequireReceiver);
                    }
                    m_SelectedObj = null;
                }
                dragging = false;
            }
        }

        protected virtual bool CanDrag(Vector3 delta)
        {
            return !dragging && Vector3.SqrMagnitude(delta) >= m_DragThreshold * m_DragThreshold;
        }

        protected virtual bool DragRelease()
        {
            return !input.GetMouseButton(0);
        }

        protected virtual bool TriggerPointerDown()
        {
            return false;
        }

        protected virtual void OnFirstSelect()
        {
            oriHitPoint = result.worldPosition;
        }

        protected virtual Vector3 CalDragDelta()
        {
            // RKLog.Debug($"====BaseRayCaster==== dragDelta:{result.worldPosition - oriHitPoint}");
            return result.worldPosition - oriHitPoint;//拖拽移动的向量
        }

        protected virtual void OnBeginDrag()
        {
            oriHitPointDis = Vector3.Distance(result.worldPosition, ray.origin);
            oriHitPoint = result.worldPosition;
        }

        protected virtual bool ProcessBeginDrag(Ray ray) { return false; }

        protected virtual bool ProcessEndDrag(Ray ray) { return false; }

        protected virtual bool ProcessDrag(Ray ray) { return false; }

        protected virtual void ProcessNothingUpEvent(PointerEventData eventData)
        {
            if (sendGlobalEvent)
                RKPointerLisener.OnPointerNothingUp?.Invoke(eventData);
        }

        protected virtual void ProcessNothingDownEvent(PointerEventData eventData)
        {
            if (sendGlobalEvent)
                RKPointerLisener.OnPointerNothingDown?.Invoke(eventData);
        }

        private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
        {
            if (lhs.module != rhs.module)
            {
                var lhsEventCamera = lhs.module.eventCamera;
                var rhsEventCamera = rhs.module.eventCamera;
                if (lhsEventCamera != null && rhsEventCamera != null && lhsEventCamera.depth != rhsEventCamera.depth)
                {
                    // need to reverse the standard compareTo
                    if (lhsEventCamera.depth < rhsEventCamera.depth)
                        return 1;
                    if (lhsEventCamera.depth == rhsEventCamera.depth)
                        return 0;

                    return -1;
                }

                if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                    return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

                if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                    return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
            }

            if (lhs.sortingLayer != rhs.sortingLayer)
            {
                // Uses the layer value to properly compare the relative order of the layers.
                var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
                var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
                return rid.CompareTo(lid);
            }

            if (lhs.sortingOrder != rhs.sortingOrder)
                return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

            // comparing depth only makes sense if the two raycast results have the same root canvas (case 912396)
            if (lhs.module != null && rhs.module != null)
            {
                if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
                    return rhs.depth.CompareTo(lhs.depth);
            }

            if (lhs.distance != rhs.distance)
                return lhs.distance.CompareTo(rhs.distance);

            return lhs.index.CompareTo(rhs.index);
        }


        /// <summary> Raycasts. </summary>
        /// <param name="ray">            The ray.</param>
        /// <param name="distance">       The distance.</param>
        /// <param name="raycastResults"> The raycast results.</param>
        public void Raycast(Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            raycastResults.Clear();
            if (raycastGraphic)
                GraphicRaycast(ignoreReversedGraphics, ray, distance, raycastResults);
            if (raycastPhysical)
                PhysicsRaycast(ray, distance, raycastResults);
            raycastResults.Sort(s_RaycastComparer);
        }


        /// <summary> Physics raycast. </summary>
        /// <param name="ray">            The ray.</param>
        /// <param name="distance">       The distance.</param>
        /// <param name="raycastResults"> The raycast results.</param>
        public virtual void PhysicsRaycast(Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            var hitCount = Physics.RaycastNonAlloc(ray, hits, distance, raycastMask);
            for (int i = 0; i < hitCount; ++i)
            {
                // RKLog.Debug("=====BaseRayCast==== Physical Raycast:" + hits[i].collider.gameObject.name);
                if (!hits[i].collider.GetComponent<RayInteractable>() && !hits[i].collider.GetComponentInParent<RayInteractable>())
                    continue;
                raycastResults.Add(new RaycastResult
                {
                    gameObject = hits[i].collider.gameObject,
                    distance = hits[i].distance,
                    worldPosition = hits[i].point,
                    worldNormal = hits[i].normal,
                    screenPosition = screenCenterPoint,
                    index = raycastResults.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                });
            }
        }

        public RaycastResult FirstRaycastResult()
        {
            for (int i = 0, imax = sortedRaycastResults.Count; i < imax; ++i)
            {
                // RKLog.Debug("=====BaseRayCast==== FirstRaycastResult:" + sortedRaycastResults[i].gameObject.name);
                return sortedRaycastResults[i];
            }
            return default(RaycastResult);
        }

        public RaycastResult FirstRaycastResult(List<RaycastResult> sortedRaycastResults)
        {
            for (int i = 0, imax = sortedRaycastResults.Count; i < imax; ++i)
            {
                // if (!sortedRaycastResults[i].isValid)
                //     continue;
                return sortedRaycastResults[i];
            }
            return default(RaycastResult);
        }

        /// <summary>
        /// 获取事件位置
        /// </summary>
        protected virtual Vector2 GetEventPosition(Ray ray, Camera eventCamera, Graphic graphic)
        {
            if (Utils.GetWorldPointInRectangle(graphic, ray, out Vector3 worldPoint))
            {
                return eventCamera.WorldToScreenPoint(worldPoint);
            }
            return screenCenterPoint;
        }

        /// <summary> Graphic raycast. </summary>
        /// <param name="canvas">                 The canvas.</param>
        /// <param name="ignoreReversedGraphics"> True to ignore reversed graphics.</param>
        /// <param name="ray">                    The ray.</param>
        /// <param name="distance">               The distance.</param>
        /// <param name="raycaster">              The raycaster.</param>
        /// <param name="raycastResults">         The raycast results.</param>
        public virtual void GraphicRaycast(bool ignoreReversedGraphics, Ray ray, float distance, List<RaycastResult> raycastResults)
        {
            foreach (Canvas canvas in CanvasRegister.canvasList)
            {
                var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
                for (int i = 0; i < graphics.Count; ++i)
                {
                    var graphic = graphics[i];

                    // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                    if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                        continue;

                    Vector2 screenPoint = GetEventPosition(ray, eventCamera, graphic);

                    if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, screenPoint, eventCamera))
                        continue;

                    if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                        continue;

                    if (eventCamera != null && ignoreReversedGraphics && Vector3.Dot(eventCamera.transform.forward, graphic.transform.forward) < 0)
                        continue;

                    float dist;
                    //平面和射线的交点
                    new Plane(graphic.transform.forward, graphic.transform.position).Raycast(ray, out dist);
                    if (float.IsNaN(dist) || dist > distance)
                        continue;

                    if (graphic.Raycast(screenPoint, eventCamera))
                    {
                        raycastResults.Add(new RaycastResult
                        {
                            gameObject = graphic.gameObject,
                            distance = dist,
                            worldPosition = ray.GetPoint(dist),
                            worldNormal = -graphic.transform.forward,
                            screenPosition = screenCenterPoint,
                            index = raycastResults.Count,
                            depth = graphic.depth,
                            sortingLayer = canvas.sortingLayerID,
                            sortingOrder = canvas.sortingOrder
                        });
                    }
                }
            }
        }
    }
}
