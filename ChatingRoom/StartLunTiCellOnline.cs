using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class StartLunTiCellOnline : MonoBehaviour
    {
        public Image lunTiName;
        public int lunTiId;
        public Transform wuDaoParent;
        public Image finshIBg;
        public Image finshImage;

        public void Init(Sprite name, int id)
        {

            this.lunTiName = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "LunTiText", true).GetComponent<Image>();
            this.wuDaoParent = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "WuDaoQiuParent", true).transform;
            this.finshImage = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "FinshImage", true).GetComponent<Image>();
            this.finshIBg = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "FinshIBg", true).GetComponent<Image>();

            base.gameObject.SetActive(true);
            this.lunTiName.sprite = name;
            this.lunTiId = id;
        }
    }
}
