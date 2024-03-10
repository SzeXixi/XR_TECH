using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{

    /// <summary>
    /// Triggered when the ray enters
    /// </summary>
    public interface IRayPointerEnter
    {
        void OnRayPointerEnter();
    }

    /// <summary>
    /// Triggered when the ray exits
    /// </summary>
    public interface IRayPointerExit
    {
        void OnRayPointerExit();
    }

    /// <summary>
    /// Triggered when mouse ray starts
    /// </summary>
    public interface IRayBeginDrag
    {
        void OnRayBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the ray drag ends
    /// </summary>
    public interface IRayEndDragWithData
    {
        void OnRayEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Triggered when the ray drag ends
    /// </summary>
    [Obsolete("Use IRayEndDragWithData instead")]
    public interface IRayEndDrag
    {
        void OnRayEndDrag();
    }

    /// <summary>
    /// Triggered when ray dragging
    /// </summary>
    public interface IRayDrag
    {
        void OnRayDrag(Vector3 delta);
    }

    /// <summary>
    /// Triggered when ray hovering
    /// </summary>
    public interface IRayPointerHover
    {
        void OnRayPointerHover();
    }

    /// <summary>
    /// Triggered when ray click
    /// </summary>
    public interface IRayPointerClick
    {
        void OnRayPointerClick();
    }

    public interface IBezierCurveDrag
    {
        /// <summary>
        /// Whether UI dragging with Pinch gesture is supported
        /// </summary>
        /// <returns></returns>
        bool IsEnablePinchBezierCurve();

        /// <summary>
        /// Whether to support dragging objects using Grip gestures
        /// </summary>
        /// <returns></returns>
        bool IsEnableGripBezierCurve();

        /// <summary>
        /// Whether it is currently in drag state
        /// </summary>
        /// <returns></returns>
        bool IsInBezierCurveDragging();

        /// <summary>
        /// Returns the world coordinates of the drag point
        /// </summary>
        /// <returns></returns>
        Vector3 GetBezierCurveEndPoint(int pointerId);


        /// <summary>
        /// Returns the world coordinates of the drag point
        /// </summary>
        /// <returns></returns>
        Vector3 GetBezierCurveEndNormal(int pointerId);
    }


    public class BezierPointerData
    {
        public int pointerId;
        public Vector3 hitLocalPos;
        public Vector3 hitLocalNormal;
    }
}
