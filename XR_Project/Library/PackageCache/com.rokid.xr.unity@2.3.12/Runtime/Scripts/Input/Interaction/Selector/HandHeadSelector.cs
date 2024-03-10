
using System;
using UnityEngine;
using Rokid.UXR.Utility;
namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// Hand Selector
    /// </summary>
    public class HandHeadSelector : MonoBehaviour, ISelector, IHeadHandDriver
    {
        private bool press = false;
        public event Action WhenSelected;
        public event Action WhenUnselected;

        public void OnHandPress(HandType hand)
        {
            if (press == false)
            {
                press = true;
                WhenSelected?.Invoke();
            }
        }

        public void OnHandRelease()
        {
            if (press)
            {
                press = false;
                WhenUnselected?.Invoke();
            }
        }

        public void OnChangeHoldHandType(HandType hand)
        {

        }

        void Update()
        {
            if (Utils.IsUnityEditor())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    WhenSelected?.Invoke();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    WhenUnselected?.Invoke();
                }
            }
        }

        public void OnBeforeChangeHoldHandType(HandType hand)
        {

        }
    }
}


