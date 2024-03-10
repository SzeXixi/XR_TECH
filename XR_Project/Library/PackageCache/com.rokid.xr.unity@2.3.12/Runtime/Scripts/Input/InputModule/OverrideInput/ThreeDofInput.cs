using UnityEngine;
using UnityEngine.EventSystems;

namespace Rokid.UXR.Interaction
{
    public class ThreeDofInput : BaseInput
    {
        public override bool mousePresent => true;

        public override bool touchSupported => true;

        public override int touchCount => 0;

        private bool mouseActive = false;

        public override bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0) || ThreeDofEventInput.Instance.GetRingDown();
        }

        public override bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.JoystickButton0) || ThreeDofEventInput.Instance.GetRingUp();
        }

        public override bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton0) || ThreeDofEventInput.Instance.GetRingButtonEvents(RingButtonsType.TouchPress);
        }
    }
}
