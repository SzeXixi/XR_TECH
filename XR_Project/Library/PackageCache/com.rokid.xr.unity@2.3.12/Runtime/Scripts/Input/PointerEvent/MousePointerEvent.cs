using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Triggered when the mouse enters
    /// </summary>
    public interface IMouseEnter
    {
        void OnMouseEnter();
    }

    /// <summary>
    /// Triggered when the mouse exits
    /// </summary>
    public interface IMouseExit
    {
        void OnMouseExit();
    }

    /// <summary>
    /// Triggered when mouse drag starts
    /// </summary>
    public interface IMouseBeginDrag
    {
        void OnMouseBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the mouse drag ends
    /// </summary>
    public interface IMouseEndDragWithData
    {
        void OnMouseEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the mouse drag ends
    /// </summary>
    [Obsolete("Use IMouseEndDragWithData instead")]
    public interface IMouseEndDrag
    {
        void OnMouseEndDrag();
    }

    /// <summary>
    /// Triggered when mouse dragging
    /// </summary>
    public interface IMouseDrag
    {
        void OnMouseRayDrag(Vector3 delta);
    }

    /// <summary>
    /// Triggered when mouse hovering
    /// </summary>
    public interface IMouseHover
    {
        void OnMouseHover();
    }

    /// <summary>
    /// Triggered when the mouse clicks
    /// </summary>
    public interface IMouseClick
    {
        void OnMouseClick();
    }
}
