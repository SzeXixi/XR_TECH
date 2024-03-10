using System;
using UnityEngine;
using Rokid.UXR.Utility;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Hand Selector
    /// </summary>
    public class HandSelector : MonoBehaviour, ISelector
    {
        [SerializeField]
        private HandType handType;
        public event Action WhenSelected;
        public event Action WhenUnselected;

        private bool dragging = false;

        private void Start()
        {
            InteractorStateChange.OnHandDragStatusChanged += OnHandDragStatusChanged;
        }

        private void OnDestroy()
        {
            InteractorStateChange.OnHandDragStatusChanged -= OnHandDragStatusChanged;
        }

        private void OnHandDragStatusChanged(HandType hand, bool dragging)
        {
            if (hand == handType)
            {
                if (dragging)
                    this.dragging = true;
            }
        }

        void Update()
        {
            if (dragging)
            {
                if (HandUtils.CanReleseHandDrag(handType))
                {
                    WhenUnselected?.Invoke();
                    dragging = false;
                }
            }
            else
            {
                if (GesEventInput.Instance.GetHandDown(handType, true) || GesEventInput.Instance.GetHandDown(handType, false))
                {
                    WhenSelected?.Invoke();
                }

                if (GesEventInput.Instance.GetHandUp(handType, true) || GesEventInput.Instance.GetHandUp(handType, false))
                {
                    WhenUnselected?.Invoke();
                }
            }
        }
    }
}


