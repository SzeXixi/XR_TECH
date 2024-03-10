using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Utility
{
    public class EventsLogUtils : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IGesPointerEnter, IGesPointerExit
    {
        [SerializeField]
        private Text logText;
        [Tooltip("是否打印(UGUI)事件")]

        [SerializeField]
        private bool logPointerEvent = true;
        [Tooltip("是否打印自定义手势事件")]

        [SerializeField]
        private bool logCustomGesEvent = true;
        private int clickCount = 0;
        private int downCount = 0;
        private int upCount = 0;
        private int enterCount = 0;
        private int exitCount = 0;
        private int gesClickCount = 0;

        private TextMesh text;

        private void Start()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text = $" OnPointerEnter, pointerEnter.Count: {++enterCount} {this.name}";
                RKLog.Info($"EventsLogUtils  OnPointerEnter => {this.name}");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text = $" OnPointerDown, pointerDown.Count: {++downCount} {this.name}";
                RKLog.Info($"EventsLogUtils  OnPointerDown => {this.name}");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerUp, pointerUp.Count: {++upCount} {this.name}";
                RKLog.Info($"EventsLogUtils  OnPointerUp => {this.name}");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerClick, pointerClick.Count: {++clickCount} {this.name}";
                RKLog.Info($"EventsLogUtils OnPointerClick => {this.name}");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (logPointerEvent)
            {
                if (logText != null)
                    logText.text += $"\n OnPointerExit, pointerExit.Count: {++exitCount} {this.name}";
                RKLog.Info($"EventsLogUtils  OnPointerExit =>  {this.name}");
            }
        }

        public void OnGesPointerEnter()
        {
            if (logCustomGesEvent)
            {
                if (logText != null)
                    logText.text = $" OnGesPointerClick, gesPointerClick.Count: {++gesClickCount} {this.name}";
                RKLog.Info($"EventsLogUtils OnGesPointerEnter => {this.name}");
            }
        }

        public void OnGesPointerExit()
        {
            if (logCustomGesEvent)
            {
                if (logText != null)
                    logText.text += $" OnGesPointerClick, gesPointerClick.Count: {++gesClickCount} {this.name}";
                RKLog.Info($"EventsLogUtils OnGesPointerExit => {this.name}");
            }
        }
    }
}
