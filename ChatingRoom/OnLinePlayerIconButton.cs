using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ChatingRoom
{
    public class OnLinePlayerIconButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform rt;
        public void Start()
        {
            rt = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rt.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }
    }
}
