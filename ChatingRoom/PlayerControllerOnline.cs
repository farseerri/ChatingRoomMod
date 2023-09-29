using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class PlayerControllerOnline : MonoBehaviour
    {
        [SerializeField]
        private PlayerSetRandomFace playerSetRandomFace;
        [SerializeField]
        private Button BtnChuPai;
        [SerializeField]
        private Button BtnEnd;
        [SerializeField]
        private Text playerName;
        public LunDaoSayWordOnline sayWord;
        [SerializeField]
        private Text playerStateName;
        public int playerStateId;
        public List<LunDaoPlayerCardOnline> cards;
        public LunDaoHuiHeOnline lunDaoHuiHe;
        public LunDaoPlayerCardOnline selectCard;
        public GameObject tips;
        [SerializeField]
        private GameObject playerStateTips;
        private bool isSayWord;
        public static GameObject playerSetRandomFaceParent;
        public void HideChuPaiBtn()
        {
            this.BtnChuPai.gameObject.SetActive(false);
        }

        public void HideStateTips()
        {
            this.playerStateTips.gameObject.SetActive(false);
        }

        public void Init()
        {
            this.lunDaoHuiHe = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.lunDaoPanelOnline.gameObject, "GameHuiHe", true).AddComponent<LunDaoHuiHeOnline>();

            this.playerName = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "Name", true).GetComponent<Text>();
            this.playerName.text = LunDaoManagerOnline.inst.player.name;
            this.playerStateId = LunDaoManagerOnline.inst.player.LunDaoState;
            this.playerStateName = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "Status", true).GetComponent<Text>();
            this.playerStateName.text = LunDaoManagerOnline.inst.lunDaoStateNameDictionary[this.playerStateId];
            this.playerStateTips = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "StateTips", true);
            this.playerStateTips.GetComponentInChildren<Text>().text = jsonData.instance.LunDaoStateData[this.playerStateId.ToString()]["MiaoShu"].Str;
            this.lunDaoHuiHe.Init();
            this.BtnEnd = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "HuiHeJieSu", true).GetComponent<Button>();
            this.BtnEnd.onClick.AddListener(() => this.PlayerEndRound());
            Console.WriteLine("初始化BtnEnd");
            if (BtnEnd == null)
            {
                Console.WriteLine("this.BtnEnd空");
            }
            this.BtnChuPai = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "BtnQueDing", true).GetComponent<Button>();
            this.BtnChuPai.onClick.AddListener(() => this.PlayerUseCard());
            this.tips = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.gameObject, "Tips", true);
            this.sayWord = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "SayWords", true).AddComponent<LunDaoSayWordOnline>();
            GameObject faceObj = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.playerControllerOnline.gameObject, "Face", true);

            GameObject playerSetRandomFaceParent = Resources.Load<GameObject>("Prefabs/SayDialog").GetComponentInChildren<PlayerSetRandomFace>().transform.parent.gameObject;
            playerSetRandomFaceParent.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            this.playerSetRandomFace = Instantiate(playerSetRandomFaceParent, faceObj.transform).GetComponentInChildren<PlayerSetRandomFace>();
            this.playerSetRandomFace.SetNPCFace(1);
            playerSetRandomFace.transform.parent.localPosition = new Vector3(0, -300, 0);
        }

        //public static PlayerSetRandomFace CreateFace(string json, Transform parent)
        //{
        //    SayDialog = Resources.Load<GameObject>("Prefabs/SayDialog").GetComponentInChildren<PlayerSetRandomFace>().transform.parent.gameObject;
        //    var setRandomFace = Instantiate(SayDialog, parent).GetComponentInChildren<PlayerSetRandomFace>();
        //    setRandomFace.setFaceByJson(JSONObject.CreateStringObject(json));
        //    return setRandomFace;
        //}


        public void PlayerEndRound()
        {
            this.BtnEnd.gameObject.SetActive(false);
            this.BtnChuPai.gameObject.SetActive(false);
            this.tips.SetActive(false);
            foreach (LunDaoPlayerCardOnline card in this.cards)
            {
                LunDaoManagerOnline.inst.lunDaoCardMagOnline.playerCards.Add(new LunDaoCard(card.lunDaoCard.wudaoId, card.lunDaoCard.level));
            }
            for (int i = this.cards.Count - 1; i >= 0; i--)
            {
                Destroy(this.cards[i].gameObject);
            }
            this.cards = new List<LunDaoPlayerCardOnline>();
            if (LunDaoManagerOnline.inst.gameState != LunDaoManagerOnline.GameState.论道结束)
            {
                LunDaoManagerOnline.inst.gameState = LunDaoManagerOnline.GameState.对方回合;
                this.lunDaoHuiHe.ReduceHuiHe();
            }
            LunDaoManagerOnline.inst.EndRoundCallBack();
        }

        public void PlayerSayWord(string content)
        {
            this.sayWord.Say(content);
        }

        public void PlayerStartRound()
        {
            this.isSayWord = true;
            LunDaoManagerOnline.inst.lunDaoPanelOnline.AddNullSlot();
            this.BtnEnd.gameObject.SetActive(true);
            this.cards = new List<LunDaoPlayerCardOnline>();
            LunDaoManagerOnline.inst.lunDaoCardMagOnline.PlayerDrawCard(this.cards);
        }


        public void PlayerUseCard()
        {
            int nullSlot = LunDaoManagerOnline.inst.lunTiMagOnline.GetNullSlot();
            if (nullSlot == -1)
            {
                UIPopTip.Inst.Pop("没有空的", PopTipIconType.叹号);
            }
            if (this.isSayWord)
            {
                this.isSayWord = false;
                int num2 = LunDaoManagerOnline.inst.lunDaoCardMagOnline.getRandom(1, 5);
                string content = jsonData.instance.LunDaoSayData[this.selectCard.lunDaoCard.wudaoId.ToString()]["Desc" + num2].Str;
                this.PlayerSayWord(content);
            }
            this.BtnChuPai.gameObject.SetActive(false);
            LunDaoManagerOnline.inst.lunTiMagOnline.curLunDianList[nullSlot].SetData(this.selectCard.lunDaoCard.wudaoId, this.selectCard.lunDaoCard.level);
            this.cards.Remove(this.selectCard);
            Destroy(this.selectCard.gameObject);
            this.selectCard = null;
            LunDaoManagerOnline.inst.ChuPaiCallBack();
        }

        public void ShowChuPaiBtn()
        {
            this.BtnChuPai.gameObject.SetActive(true);
        }

        public void ShowStateTips()
        {
            this.playerStateTips.gameObject.SetActive(true);
        }
    }
}
