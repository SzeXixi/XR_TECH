using System;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class MouseSelector : MonoBehaviour, ISelector
    {
        public event Action WhenSelected;
        public event Action WhenUnselected;

        void Update()
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
}
