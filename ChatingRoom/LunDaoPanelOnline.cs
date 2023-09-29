using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChatingRoom
{
    public class LunDaoPanelOnline : MonoBehaviour
    {
        [SerializeField]
        private GameObject lunDaoQiuSlot;
        [SerializeField]
        private GameObject startLunTiDaoCell;
        [SerializeField]
        private GameObject wuDaoQiuCell;

        GameObject lunDaoQiuList;
        //[SerializeField]
        //private GameObject lunDaoQiuSlot;




        private Dictionary<int, List<int>> targetLunTiDictionary;
        public Dictionary<int, StartLunTiCellOnline> lunTiCtrDictionary;
        public void Init()
        {
            startLunTiDaoCell = ChatingRoomManager.startLunTiCellPrefab;
            GameObject lunTiListGO = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "LunTiList", true);

            lunDaoQiuList = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "LunDaoQiuList", true);

            wuDaoQiuCell = ChatingRoomManager.wuDaoQiuPrefab;
            lunDaoQiuSlot = ChatingRoomManager.lunDaoQiuSlotPrefab;
            this.targetLunTiDictionary = LunDaoManagerOnline.inst.lunTiMagOnline.targetLunTiDictionary;
            this.lunTiCtrDictionary = new Dictionary<int, StartLunTiCellOnline>();

            foreach (int lunTiNum in this.targetLunTiDictionary.Keys)
            {
                StartLunTiCellOnline startLunTiCellOnline = UnityEngine.Object.Instantiate<GameObject>(this.startLunTiDaoCell, lunTiListGO.transform).AddComponent<StartLunTiCellOnline>();

                startLunTiCellOnline.Init(ChatingRoomManager.lunTiNameSpriteList[lunTiNum], lunTiNum);
                for (int j = 0; j < this.targetLunTiDictionary[lunTiNum].Count; j++)
                {
                    WuDaoQiuOnline wuDaoQiu = Instantiate<GameObject>(this.wuDaoQiuCell, startLunTiCellOnline.wuDaoParent).AddComponent<WuDaoQiuOnline>();
                    wuDaoQiu.Init(ChatingRoomManager.lunTiNameSpriteList[lunTiNum], this.targetLunTiDictionary[lunTiNum][j]);
                }
                this.lunTiCtrDictionary.Add(lunTiNum, startLunTiCellOnline);

            }
        }
        public void Show()
        {
            base.gameObject.SetActive(true);
        }

        public void AddNullSlot()
        {
            LunDaoQiuOnline component = UnityEngine.Object.Instantiate<GameObject>(this.lunDaoQiuSlot, lunDaoQiuList.transform).AddComponent<LunDaoQiuOnline>();
            component.SetNull();
            component.gameObject.SetActive(true);
            LunDaoManagerOnline.inst.lunTiMagOnline.curLunDianList.Add(component);
        }

    }
}
