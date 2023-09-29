using Bag;
using JSONClass;
using LitJson;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class ChatingRoomItemCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public Image qsprite;
        public Image sframe;
        public Image sprite;
        public Text itemName;
        public int price;
        public Text tips;
        public Text count;
        public BaseItem baseItem;
        public string playerDataString = "";


        private void Start()
        {


            qsprite = ChatingRoomManager.Inst.GetChild<Image>(this.gameObject, "qsprite", true).GetComponent<Image>();
            sframe = ChatingRoomManager.Inst.GetChild<Image>(this.gameObject, "frame", true).GetComponent<Image>();
            sprite = ChatingRoomManager.Inst.GetChild<Image>(this.gameObject, "sprite", true).GetComponent<Image>();
            itemName = ChatingRoomManager.Inst.GetChild<Text>(this.gameObject, "name", true).GetComponent<Text>();
            count = ChatingRoomManager.Inst.GetChild<Text>(this.gameObject, "count", true).GetComponent<Text>();

            ChatingRoomManager.chatingRoomPanel.emptySprite = qsprite.sprite;
        }

        public void SetPlayerDataWithItem(BaseItem baseItem, int price, string itemString)
        {
            this.baseItem = baseItem;
            this.qsprite.sprite = baseItem.GetQualityUpSprite();
            this.sframe.sprite = baseItem.GetQualitySprite();
            this.sprite.sprite = baseItem.GetIconSprite();
            this.itemName.text = baseItem.GetName();
            this.price = price;
            this.count.text = baseItem.Count.ToString();

            ChatingRoomManager.CustomPlayerDataClass customPlayerData = SetPlayerData();
            customPlayerData.itemData = itemString;

            this.playerDataString = JsonMapper.ToJson(customPlayerData);
            Console.WriteLine("载入数据:" + this.playerDataString);
        }


        public ChatingRoomManager.CustomPlayerDataClass SetPlayerData()
        {
            KBEngine.Avatar avatar = Tools.instance.getPlayer();
            string jingJie = LevelUpDataJsonData.DataDict[(int)avatar.level].Name;
            ChatingRoomManager.CustomPlayerDataClass customPlayerData = new ChatingRoomManager.CustomPlayerDataClass();

            customPlayerData.customPrice = price.ToString();
            customPlayerData.xiuxianName = avatar.name;

            if (avatar.Sex == 1)
            {
                customPlayerData.sex = "男";
            }
            else if (avatar.Sex == 2)
            {
                customPlayerData.sex = "女";
            }


            customPlayerData.age = avatar.age.ToString() + "岁";
            customPlayerData.menpai = Tools.getStr("menpai" + avatar.menPai);
            customPlayerData.shenShi = avatar.shengShi.ToString();
            customPlayerData.jingJie = jingJie;
            customPlayerData.faceID = avatar.Face.ToString();
            customPlayerData.lunDaoState = avatar.LunDaoState.ToString();

            if (SteamUser.GetSteamID().m_SteamID.ToString() == "76561198091009835")
            {
                customPlayerData.gameFaceData = jsonData.instance.AvatarRandomJsonData["609"].ToString();
            }
            else
            {
                customPlayerData.gameFaceData = jsonData.instance.AvatarRandomJsonData["1"].ToString();
            }

            return customPlayerData;
        }






        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (baseItem != null)
            {
                if (this.sprite.sprite.name != "Empty")
                {
                    if (ToolTipsMag.Inst == null)
                    {
                        ResManager.inst.LoadPrefab("ToolTips").Inst(NewUICanvas.Inst.transform);
                    }
                    ChatingRoomManager.toolTipPositionfix = true;
                    ToolTipsMag.Inst.Show(baseItem, price, false);
                    ChatingRoomManager.toolTipPositionfix = false;

                    Transform parent = (ToolTipsMag.Inst.LeftPanel.gameObject.activeInHierarchy == true) ? ToolTipsMag.Inst.LeftPanel.transform : ToolTipsMag.Inst.LeftPanel.transform;

                    // GameObject ciTiao = ToolTipsMag.Inst.CiTiao.Inst(parent);
                    //ciTiao.SetActive(true);
                    this.sframe.gameObject.SetActive(true);
                }

            }

        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (baseItem != null)
            {
                if (this.sprite.sprite.name != "Empty")
                {
                    ToolTipsMag.Inst.Close();
                    this.sframe.gameObject.SetActive(false);
                }
            }
        }
    }
}
