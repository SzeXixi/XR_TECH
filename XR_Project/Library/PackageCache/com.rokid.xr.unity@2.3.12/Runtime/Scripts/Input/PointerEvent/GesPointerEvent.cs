using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Gesture ray enters,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesPointerEnter
    {
        void OnGesPointerEnter();
    }

    /// <summary>
    /// Gesture ray exits,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesPointerExit
    {
        void OnGesPointerExit();
    }

    /// <summary>
    /// Gesture Ray drag starts,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesBeginDrag
    {
        void OnGesBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Gesture ray drag ends,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesEndDragWithData
    {
        void OnGesEndDrag(PointerEventData eventData);
    }

    /// <summary>
    /// Gesture ray drag ends,grip or pinch gesture type triggers
    /// </summary>
    [Obsolete("Use IGesEndDragWithData instead")]
    public interface IGesEndDrag
    {
        void OnGesEndDrag();
    }

    /// <summary>
    /// Gesture ray drag,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesDrag
    {
        [Obsolete("Use IGesDragToTarget instead")]
        void OnGesDrag(Vector3 delta);

    }

    /// <summary>
    /// Gesture ray drag,grip or pinch gesture type triggers
    /// </summary>
    public interface IGesDragToTarget
    {
        void OnGesDragToTarget(Vector3 targetPos);
    }
    /// <summary>
    /// The gesture ray is hovering and the grip or pinch type is triggered
    /// </summary>
    public interface IGesPointerHover
    {
        void OnGesPointerHover(RaycastResult result);
    }
    /// <summary>
    /// Gesture ray click,grip or pinch gesture type trigger
    /// </summary>
    public interface IGesPointerClick
    {
        void OnGesPointerClick();
    }
}
