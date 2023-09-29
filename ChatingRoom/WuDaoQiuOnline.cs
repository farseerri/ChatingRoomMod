using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class WuDaoQiuOnline : MonoBehaviour
    {
        private Image wuDaoQiuImage;
        private Text wuDaoQiuLevel;

        public void Init(UnityEngine.Sprite sprite, int level)
        {
            wuDaoQiuImage = GetComponent<Image>();
            wuDaoQiuLevel = GetComponentInChildren<Text>();
            base.gameObject.SetActive(true);
            this.wuDaoQiuImage.sprite = sprite;
            this.wuDaoQiuLevel.text = level.ToString();
        }
    }
}
