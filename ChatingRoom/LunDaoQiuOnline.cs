using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoQiuOnline : MonoBehaviour
    {
        public Image lunDaoQiuImage;
        public Text curLevel;
        public int wudaoId;
        public int level;
        public bool isNull;

        public void Start()
        {

        }

        public void SetNull()
        {
            this.lunDaoQiuImage = GetComponent<Image>();
            this.curLevel = GetComponentInChildren<Text>();
            this.lunDaoQiuImage.gameObject.SetActive(false);
            this.wudaoId = -1;
            this.isNull = true;
            this.level = 0;
        }
        public void SetData(int id, int curLevel)
        {
            this.isNull = false;
            this.wudaoId = id;
            this.level = curLevel;
            this.curLevel.text = curLevel.ToString();
            this.lunDaoQiuImage.sprite = ChatingRoomManager.lunTiNameSpriteList[id];
            this.lunDaoQiuImage.gameObject.SetActive(true);
        }
        public LunDaoQiuOnline LevelUp()
        {
            this.level++;
            this.curLevel.text = this.level.ToString();
            return this;
        }
    }
}
