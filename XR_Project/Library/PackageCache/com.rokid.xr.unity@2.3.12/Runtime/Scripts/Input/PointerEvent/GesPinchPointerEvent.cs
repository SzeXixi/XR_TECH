
using System;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{

    /// <summary>
    ///  The gesture ray enters, and the pinch gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesEnter instead")]
    public interface IGesPinchEnter
    {
        void OnGesPinchEnter();
    }

    /// <summary>
    ///  The gesture ray exits and the pinch gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesExit instead")]
    public interface IGesPinchExit
    {
        void OnGesPinchExit();
    }

    /// <summary>
    ///  The gesture ray drag begins, and the pinch gesture type triggers
    /// </summary>
    [Obsolete("Please use IGesBeginDrag instead")]
    public interface IGesPinchBeginDrag
    {
        void OnGesPinchBeginDrag(PointerEventData eventData);
    }

    /// <summary>
    ///  The gesture ray drag is over and the pinch gesture type is triggered
    /// </summary>
    [Obsolete("Please use IGesEneDrag instead")]
    public interface IGesPinchEndDrag
    {
        void OnGesPinchEndDrag(PointerEventData eventData);
    }

    /// <summary>
    ///  During the gesture ray drag, the pinch gesture type is triggered
    /// </summary>

    [Obsolete("Please use IGesPinchDrag instead")]
    public interface IGesPinchDrag
    {
        void OnGesPinchDrag(PointerEventData eventData);
    }
}
