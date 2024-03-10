using System;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofSelector : MonoBehaviour, ISelector
    {
        public event Action WhenSelected;
        public event Action WhenUnselected;

        void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0) || ThreeDofEventInput.Instance.GetRingDown())
            {
                WhenSelected?.Invoke();
            }

            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.JoystickButton0) || ThreeDofEventInput.Instance.GetRingUp())
            {
                WhenUnselected?.Invoke();
            }
        }
    }
}
