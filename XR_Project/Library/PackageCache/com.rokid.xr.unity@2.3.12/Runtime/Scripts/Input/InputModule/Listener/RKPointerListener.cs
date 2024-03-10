using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Global Event Listener
    /// </summary>
    public static class RKPointerLisener
    {
        public static Action<PointerEventData> OnPointerEnter;
        public static Action<PointerEventData> OnPointerHover;
        public static Action<PointerEventData, GameObject> OnPointerExit;
        public static Action<PointerEventData> OnPointerDown;
        public static Action<PointerEventData> OnPointerUp;
        public static Action<PointerEventData> OnPointerDrag;
        public static Action<PointerEventData> OnPointerDragBegin;
        public static Action<PointerEventData> OnPointerDragEnd;
        public static Action<PointerEventData> OnPointerNothingUp;
        public static Action<PointerEventData> OnPointerNothingDown;

        // Add For UI Graphic Raycast
        public static Action<PointerEventData> OnGraphicPointerEnter;
        public static Action<PointerEventData> OnGraphicPointerHover;
        public static Action<PointerEventData> OnGraphicPointerExit;

        // Add For Physical Raycast
        public static Action<PointerEventData> OnPhysicalPointerEnter;
        public static Action<PointerEventData> OnPhysicalPointerHover;
        public static Action<PointerEventData, GameObject> OnPhysicalPointerExit;
    }
}
