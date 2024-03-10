using System;
using UnityEngine;

namespace Rokid.UXR.Interaction {
	public class ButtonMouseSelector : MonoBehaviour, ISelector
	{
	    public event Action WhenSelected;
	    public event Action WhenUnselected;
	
	    private bool isCanClick = true;
	    public void setClickEnable(bool enable)
	    {
	        isCanClick = enable;
	    }
	    
	    void Update()
	    {
	        if (!isCanClick)
	        {
	            return;
	        }
	        
        #if UNITY_EDITOR
	        if (Input.GetKeyDown(KeyCode.KeypadEnter))
	        {
	            WhenSelected?.Invoke();
	        }
	        if (Input.GetKeyUp(KeyCode.KeypadEnter))
	        {
	            WhenUnselected?.Invoke();
	        }
        #else
	        if (Input.GetKeyDown(KeyCode.JoystickButton0))
	        {
	            WhenSelected?.Invoke();
	        }
	
	        if (Input.GetKeyUp(KeyCode.JoystickButton0))
	{
	            WhenUnselected?.Invoke();
	        }
        #endif
	    }
	}
}
