using Bag;
using GUIPackage;
using JSONClass;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class ChatingRoomPanelBase : MonoBehaviour
    {
        //public bool TryEscClose()
        //{
        //    this.Close();
        //    return true;
        //}


        public ChatingRoomItemCell yourWTSItemItemCell;
        public ChatingRoomItemCell otherPlayerItemCell;
        public InputField sellPriceInputField;
        public static bool isPublishing = false;
        public ScrollRect chatingScrollRect;
        public RawImage otherAvatarImage;
        public GameObject setColorPanel;
        public string currentHexColor;
        public GameObject chatingPanel;

        public static Button lunDaoInviteButton;
        public static Button addFriendButton;
        public Transform dropdownParent;    // Dropdown的父物体


        private GameObject dropdown;        // Dropdown
        private Dropdown dropdownComponent; // Dropdown组件
        private Match match;                // 正则表达式匹配结果
        private bool selectingName;         // 是否正在选择名字

        public Button lunDaoButton;

        public Sprite emptySprite;

        public static RectTransform chatingContent;
        public static Color lastChatingColor;
        public static RectTransform memberScrollRectContent;
        public static GameObject faceObj;
        public static PlayerSetRandomFace otherPlayeSetRandomFace;
        public static InputField inputText;

        public void Close()
        {
            //Tools.canClickFlag = true;
            //ESCCloseManager.Inst.UnRegisterClose(this);
            UnityEngine.Object.Destroy(base.gameObject);
            UIkeyboardManager.CanUse = true;
        }



        public virtual void Awake()
        {
            chatingPanel = ChatingRoomManager.Inst.GetChild<RectTransform>(this.gameObject, "ChatingPanel", true);
            chatingScrollRect = ChatingRoomManager.Inst.GetChild<ScrollRect>(chatingPanel.gameObject, "Scroll View", true).GetComponent<ScrollRect>();
            dropdownParent = ChatingRoomManager.Inst.GetChild<RectTransform>(chatingPanel.gameObject, "DropdownParent", true).transform;
            chatingContent = chatingScrollRect.content;

        }




        public virtual void Start()
        {

            Button quitBtn = ChatingRoomManager.Inst.GetChild<Button>(this.gameObject, "QuitBtn", true).GetComponent<Button>();
            Button sendBtn = ChatingRoomManager.Inst.GetChild<Button>(this.gameObject, "SendButton", true).GetComponent<Button>();
            Button setGoodsButton = ChatingRoomManager.Inst.GetChild<Button>(this.gameObject, "SetGoodsButton", true).GetComponent<Button>();
            Button publishItemButton = ChatingRoomManager.Inst.GetChild<Button>(this.gameObject, "PublishItemButton", true).GetComponent<Button>();
            Button buyButton = ChatingRoomManager.Inst.GetChild<Button>(this.gameObject, "BuyButton", true).GetComponent<Button>();

            sellPriceInputField = ChatingRoomManager.Inst.GetChild<InputField>(this.gameObject, "SellPriceInputField", true).GetComponent<InputField>();
            yourWTSItemItemCell = ChatingRoomManager.Inst.GetChild<RectTransform>(this.gameObject, "YourWTSItem", true).AddComponent<ChatingRoomItemCell>();
            otherPlayerItemCell = ChatingRoomManager.Inst.GetChild<RectTransform>(this.gameObject, "OtherPlayerWTSItem", true).AddComponent<ChatingRoomItemCell>();

            otherAvatarImage = ChatingRoomManager.Inst.GetChild<RawImage>(this.gameObject, "AvatarImage", true).GetComponent<RawImage>();


            yourWTSItemItemCell.tips = ChatingRoomManager.Inst.GetChild<Text>(yourWTSItemItemCell.gameObject, "YourItemSellTips", true).GetComponent<Text>();


            inputText = ChatingRoomManager.Inst.GetChild<InputField>(this.gameObject, "InputText", true).GetComponent<InputField>();

            quitBtn.onClick.AddListener(() => Close());
            sendBtn.onClick.AddListener(() => ChatingRoomManager.Inst.SendNormalChatingMessage());
            setGoodsButton.onClick.AddListener(() => SetGoods(ChatingRoomManager.jiaoYiCangKuId));
            publishItemButton.onClick.AddListener(() => PublishItem());
            buyButton.onClick.AddListener(() => BuyGoods(ChatingRoomManager.targetID));

            setColorPanel = ChatingRoomManager.Inst.GetChild<RectTransform>(this.gameObject, "SetColorButtonsPanel", true);

            //lunDaoButton = ChatingRoomManager.Inst.GetChild<Button>(otherPlayerItemCell.gameObject, "LunDaoButton", true).GetComponent<Button>();
            //lunDaoButton.onClick.AddListener(() => ChatingRoomManager.开始联机论道(ChatingRoomManager.targetID));

            memberScrollRectContent = ChatingRoomManager.Inst.GetChild<RectTransform>(ChatingRoomManager.chatingRoomPanel.gameObject, "MemberPanel", true).GetComponentInChildren<ScrollRect>().content;

            foreach (Transform child in setColorPanel.transform)
            {
                Button bt = child.GetComponent<Button>();
                bt.onClick.AddListener(() => currentHexColor = ColorUtility.ToHtmlStringRGB(bt.image.color));
            }


            ChatingRoomMainPanel.inputText.onValueChanged.AddListener(OnInputValueChanged);


            lunDaoInviteButton = ChatingRoomManager.Inst.GetChild<Button>(ChatingRoomManager.chatingRoomPanel.gameObject, "LunDaoButton", true).GetComponent<Button>();

            addFriendButton = ChatingRoomManager.Inst.GetChild<Button>(ChatingRoomManager.chatingRoomPanel.gameObject, "AddFriend", true).GetComponent<Button>();
            faceObj = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.chatingRoomPanel.gameObject, "Face", true);

            currentHexColor = ColorUtility.ToHtmlStringRGB(Color.white);

            UIkeyboardManager.CanUse = false;


            foreach (string str in ChatingRoomManager.lastMessageList)
            {
                Image chatingBar = Instantiate(ChatingRoomManager.chatingBarPrefab, ChatingRoomMainPanel.chatingContent).GetComponent<Image>();
                Text chatingText = chatingBar.GetComponentInChildren<Text>();

                if (ChatingRoomMainPanel.lastChatingColor == null)
                {
                    ChatingRoomMainPanel.lastChatingColor = ChatingRoomManager.greenChatringBarColor;
                }

                if (ChatingRoomMainPanel.lastChatingColor == Color.white)
                {
                    chatingBar.color = ChatingRoomManager.greenChatringBarColor;
                }
                else
                {
                    chatingBar.color = Color.white;
                }
                lastChatingColor = chatingBar.color;
                chatingText.text = SensitiveWordsFilter.Filter(str) + "\r\n";
            }


        }

        public virtual void OnDestroy()
        {

            // 移除输入框的事件监听
            ChatingRoomMainPanel.inputText.onValueChanged.RemoveListener(OnInputValueChanged);

            if (dropdownComponent != null)
            {
                dropdownComponent.onValueChanged.RemoveListener(OnNameSelected);
            }


        }





        void OnInputValueChanged(string message)
        {
            // 判断是否正在选择名字
            if (selectingName)
            {
                return;
            }
            // 匹配@开头的字符串
            Regex regex = new Regex("^@[\\w\\d]*$");
            match = regex.Match(message);
            if (match.Success)
            {
                // 获取筛选关键字
                string query = match.Groups[1].Value;
                // 根据关键字筛选名字列表
                List<string> names = new List<string>();

                foreach (Transform child in memberScrollRectContent)
                {
                    names.Add(GetXiuXianMingFormMemberContent(child));
                }



                //foreach (KeyValuePair<CSteamID, string> keyValuePair in ChatingRoomManager.members)
                //{

                //    names.Add(keyValuePair.Value);
                //}

                // 实例化Dropdown
                dropdown = Instantiate(ChatingRoomManager.dropdownPrefab, dropdownParent);
                // 获取Dropdown组件
                dropdownComponent = dropdown.GetComponent<Dropdown>();

                List<string> filteredNames = names.FindAll(n => n.StartsWith(query));
                // 更新Dropdown选项
                dropdownComponent.ClearOptions();
                dropdownComponent.AddOptions(filteredNames);

                // 打开Dropdown
                //dropdown.SetActive(true);
                // 设置Dropdown位置
                RectTransform inputTransform = ChatingRoomMainPanel.inputText.GetComponent<RectTransform>();
                dropdown.transform.parent = dropdownParent;
                dropdown.transform.localPosition = Vector3.one;

                // 添加Dropdown的事件监听
                dropdownComponent.onValueChanged.AddListener(OnNameSelected);
                dropdown.transform.SetAsLastSibling();


            }
            else
            {

                foreach (Transform tr in dropdownParent)
                {
                    //dropdown.SetActive(false);
                    // 移除Dropdown的事件监听
                    //dropdownComponent.onValueChanged.RemoveListener(OnNameSelected);
                    tr.GetComponent<Dropdown>().onValueChanged.RemoveListener(OnNameSelected);
                    Destroy(tr.gameObject);
                    //dropdown = null;
                }


            }
        }


        public string GetXiuXianMingFormMemberContent(Transform inputTransform)
        {
            string name = "";
            Text[] texts = inputTransform.GetComponentsInChildren<Text>();
            foreach (Text t in texts)
            {
                if (t.name == "XiuXianName")
                {
                    name = t.text;

                }
            }
            return name;
        }





        void OnNameSelected(int index)
        {
            // 获取选择的名字
            string name = dropdownComponent.options[index].text;
            // 在输入框中插入名字
            string message = ChatingRoomMainPanel.inputText.text;
            message = message.Substring(0, match.Index + 1) + name + " " + message.Substring(match.Index + match.Length);
            ChatingRoomMainPanel.inputText.text = message;

            //dropdown.SetActive(false);
            //// 移除Dropdown的事件监听
            //dropdownComponent.onValueChanged.RemoveListener(OnNameSelected);
            //Destroy(dropdown.gameObject);
            //dropdown = null;
            foreach (Transform tr in dropdownParent)
            {
                tr.GetComponent<Dropdown>().onValueChanged.RemoveListener(OnNameSelected);
                Destroy(tr.gameObject);
            }
            // 设置选择名字标记
            selectingName = true;
            // 将光标移动到名字后面
            ChatingRoomMainPanel.inputText.caretPosition = match.Index + name.Length + 2;
            // 延迟一帧再将选择名字标记设为false
            StartCoroutine(ResetSelectingName());
        }






        IEnumerator ResetSelectingName()
        {
            yield return null;
            selectingName = false;
        }






        public void SetGoods(string cangKuId)
        {
            if (CangKuManager.Inst.GetCangKu(cangKuId) == null)
            {
                CangKuManager.Inst.CreateCangKu(cangKuId, "仓库", false);
            }
            CangKuManager.Inst.OpenCangKuUI(cangKuId);
            isPublishing = false;
            ChatingRoomManager.SetLobbyMemberData("isPublishing", "false");
            yourWTSItemItemCell.tips.text = "还未设定价格，未上架";
        }
        public void BuyGoods(CSteamID targetID)
        {
            if (ChatingRoomManager.chatingRoomPanel.otherPlayerItemCell.sprite.sprite == null)
            {
                UIPopTip.Inst.Pop("此道友未开张！", PopTipIconType.叹号);
            }
            else
            {

                KBEngine.Avatar player = PlayerEx.Player;
                ulong price = (ulong)ChatingRoomManager.chatingRoomPanel.otherPlayerItemCell.price;
                if (price > player.money)
                {
                    UIPopTip.Inst.Pop("你的灵石不够！", PopTipIconType.叹号);
                }
                else
                {
                    ChatingRoomManager.SendChatMessage(targetID.m_SteamID.ToString(), "请求购买", otherPlayerItemCell.playerDataString);
                }
            }
        }



        public void PublishItem()
        {

            KBEngine.ITEM_INFO_LIST ItemList = CangKuManager.Inst.GetCangKu(ChatingRoomManager.jiaoYiCangKuId).ItemList;

            if (ItemList.values.Count == 0)
            {
                UIPopTip.Inst.Pop("请先上货，在摊位放置一个物品！", PopTipIconType.叹号);
                return;
            }
            else if (ItemList.values.Count == 1)
            {
                JSONObject ob = ItemList.values[0].ToJSONObject();
                BaseItem baseItem = BaseItem.Create(ItemList.values[0].itemId, (int)ItemList.values[0].itemCount, ItemList.values[0].uuid, ItemList.values[0].Seid);
                KBEngine.Avatar player = PlayerEx.Player;

                int miniPrice = 0;
                if (baseItem is Bag.MiJiItem)
                {
                    Console.WriteLine("ItemList.values[0].itemId" + ItemList.values[0].itemId);
                    _ItemJsonData itemJsonData = _ItemJsonData.DataDict[ItemList.values[0].itemId];

                    Bag.MiJiItem mj = (Bag.MiJiItem)baseItem;
                    _ItemJsonData shu;

                    if (mj.GetMiJiType() == Bag.MiJiType.技能)
                    {
                        try
                        {
                            int skillID = Mathf.RoundToInt(float.Parse(itemJsonData.desc));
                            Skill skill = SkillDatebase.instence.Dict[skillID][1];
                            Console.Write("skillID.SkillID:" + skill.SkillID.ToString() + "skill.skill_ID" + skill.skill_ID.ToString());
                            shu = _ItemJsonData.DataDict[skillID + 3000];
                            if (shu.name == mj.GetName())
                            {
                                miniPrice = (int)(shu.price * 0.9f);
                                Console.WriteLine(shu.name + "是技能，价格:" + shu.price);
                            }
                            else if ((int)(baseItem.GetJiaoYiPrice(player.id, true, false)) == 0)
                            {
                                miniPrice = (int)(shu.price * 0.9f);
                                Console.WriteLine("退而其次，找附近的价格:" + shu.price);
                            }
                            else
                            {
                                miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                                Console.WriteLine("转换后的技能名:" + shu.name + "与原物品名:" + baseItem.GetName() + "不符，使用原价");
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("没能通过名称到对应技能书:" + e.ToString());
                            miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                        }

                    }
                    else if (mj.GetMiJiType() == Bag.MiJiType.功法)
                    {
                        try
                        {
                            int skillID = Mathf.RoundToInt(float.Parse(itemJsonData.desc));
                            Skill skill = SkillDatebase.instence.Dict[skillID][1];
                            Console.Write("skillID.SkillID:" + skill.SkillID.ToString() + "skill.skill_ID" + skill.skill_ID.ToString());
                            shu = _ItemJsonData.DataDict[skillID + 4000];

                            if (shu.name == mj.GetName())
                            {
                                miniPrice = (int)(shu.price * 0.9f);
                                Console.WriteLine(shu.name + "功法，价格:" + shu.price);
                            }
                            else if ((int)(baseItem.GetJiaoYiPrice(player.id, true, false)) == 0)
                            {
                                miniPrice = (int)(shu.price * 0.9f);
                                Console.WriteLine("退而其次，找附近的价格:" + shu.price);
                            }
                            else
                            {
                                miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                                Console.WriteLine("转换后的功法名:" + shu.name + "与原物品名:" + baseItem.GetName() + "不符，使用原价");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("没能通过名称找到对应功法:" + e.ToString());
                            miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                        }
                    }
                    else
                    {
                        miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                    }
                }
                else
                {
                    miniPrice = (int)(baseItem.GetJiaoYiPrice(player.id, true, false));
                }

                int maxPrice = (int)(miniPrice * 5);

                int price = int.Parse(sellPriceInputField.text) / baseItem.Count;


                price = Mathf.Clamp(price, miniPrice, maxPrice);
                int totalPice = price * baseItem.Count;
                sellPriceInputField.text = totalPice.ToString();
                UIPopTip.Inst.Pop("价格不能低于市场价0.9倍，也不能高于市场价5倍，防止扰乱市场", PopTipIconType.叹号);
                yourWTSItemItemCell.SetPlayerDataWithItem(baseItem, totalPice, ob.ToString(false));

                yourWTSItemItemCell.tips.text = "以" + totalPice.ToString() + "灵石摆摊";
                UIPopTip.Inst.Pop("发布完成，需要挂机摆摊，不能离线！", PopTipIconType.叹号);
                isPublishing = true;
                ChatingRoomManager.SetLobbyMemberData("isPublishing", "true");
            }
            else
            {
                UIPopTip.Inst.Pop("上货错误，摊位只能放一个物品！", PopTipIconType.叹号);
            }

        }


    }
}
