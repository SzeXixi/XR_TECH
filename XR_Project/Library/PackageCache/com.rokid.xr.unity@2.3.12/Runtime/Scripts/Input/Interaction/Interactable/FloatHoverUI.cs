using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Only used to detect whether it is a floating UI
namespace Rokid.UXR.Interaction
{
    public class FloatHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] 
        private bool EnableFloat = true;

        private List<PointerEventData> saveEventDatas = new List<PointerEventData>();
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EnableFloat)
            {
                saveEventDatas.Add(eventData);
                RKPointerLisener.OnGraphicPointerEnter?.Invoke(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EnableFloat)
            {
                RKPointerLisener.OnGraphicPointerExit?.Invoke(eventData);
                saveEventDatas.Remove(eventData);
            }
        }

        // public void OnPointerMove(PointerEventData eventData)
        // {
        //     if (EnableFloat)
        //     {
        //         RKPointerLisener.OnGraphicPointerHover?.Invoke(eventData);
        //     }
        // }

        private void OnDisable()
        {
            if (EnableFloat && saveEventDatas.Count > 0)
            {
                foreach (var data in saveEventDatas)
                {
                    RKPointerLisener.OnGraphicPointerExit?.Invoke(data);  
                }
                saveEventDatas.Clear();
            }
        }

        private void OnDestroy()
        {
            if (EnableFloat && saveEventDatas.Count > 0)
            {
                foreach (var data in saveEventDatas)
                {
                    RKPointerLisener.OnGraphicPointerExit?.Invoke(data);  
                }
                saveEventDatas.Clear();
            }
        }
    }
}