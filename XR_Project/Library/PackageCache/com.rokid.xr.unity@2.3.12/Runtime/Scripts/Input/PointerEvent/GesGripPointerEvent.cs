using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Gesture ray enters,grip gesture type triggers
    /// </summary>
    public interface IGesGripPointerEnter
    {
        void OnGesGripPointerEnter();
    }
    /// <summary>
    /// Gesture ray exits,grip gesture type triggers
    /// </summary>
    public interface IGesGripPointerExit
    {
        void OnGesGripPointerExit();
    }
    /// <summary>
    /// Gesture Ray drag starts,grip gesture type triggers
    /// </summary>
    public interface IGesGripBeginDrag
    {
        void OnGesGripBeginDrag(PointerEventData eventData);
    }
    /// <summary>
    /// Gesture ray drag ends,grip gesture type triggers
    /// </summary>
    public interface IGesGripEndDrag
    {
        void OnGesGripEndDrag();
    }
    /// <summary>
    /// Gesture ray drag,grip gesture type triggers
    /// </summary>
    public interface IGesGripDrag
    {
        void OnGesGripDrag(Vector3 delta);
    }
    /// <summary>
    /// The gesture ray is hovering and the grip type is triggered
    /// </summary>
    public interface IGesGripPointerHover
    {
        void OnGesGripPointerHover(RaycastResult result);
    }
    /// <summary>
    /// Gesture ray click,grip gesture type trigger
    /// </summary>
    public interface IGesGripPointerClick
    {
        void OnGesGripPointerClick();
    }
}
