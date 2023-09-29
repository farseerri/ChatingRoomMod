using JSONClass;
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
    public class OtherPlayerController : MonoBehaviour
    {
        [SerializeField]
        private PlayerSetRandomFace otherPlayeSetRandomFace;
        public LunDaoSayWordOnline sayWord;
        [SerializeField]
        private Text npcName;
        [SerializeField]
        private Text npcStateName;
        [SerializeField]
        private GameObject npcStateTips;
        public int npcStateId;
        public bool isSayWord = true;
        public List<LunDaoCard> cards;



        //public static IEnumerator CloseNPCShow(float time)
        //{
        //    yield return new WaitForSeconds(time);

        //    GameObject nPCShow = ChatingRoomManager.GetGameObjectStatic<RectTransform>(null, "NPCShow", true);
        //    if (nPCShow != null)
        //    {
        //        nPCShow.gameObject.SetActive(false);

        //    }

        //}


        public void Init()
        {
            try
            {
                JSONObject otherFaceJson = JSONObject.Create(LunDaoManagerOnline.otherPlayerOnlineDataClass.faceJsonString, -2, false, false);
                Console.WriteLine("otherFaceJson:" + otherFaceJson);
                GameObject faceObj = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.otherPlayerController.gameObject, "Face", true);
                GameObject otherrSetRandomFaceParent = Resources.Load<GameObject>("Prefabs/SayDialog").GetComponentInChildren<PlayerSetRandomFace>().transform.parent.gameObject;
                otherrSetRandomFaceParent.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                this.otherPlayeSetRandomFace = Instantiate(otherrSetRandomFaceParent, faceObj.transform).GetComponentInChildren<PlayerSetRandomFace>();
                ChatingRoomManager.randomAvatar(ref this.otherPlayeSetRandomFace, otherFaceJson);

                otherPlayeSetRandomFace.transform.parent.localPosition = new Vector3(0, -300, 0);
                //this.otherPlayeSetRandomFace.SetNPCFace(1);
                //this.otherPlayeSetRandomFace.setFaceByJson(otherFaceJson);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            this.npcName = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.otherPlayerController.gameObject, "Name", true).GetComponent<Text>();
            this.npcName.text = LunDaoManagerOnline.otherPlayerOnlineDataClass.name;
            int num = LunDaoManagerOnline.otherPlayerOnlineDataClass.StatusId;
            num = jsonData.instance.NpcStatusDate[num.ToString()]["LunDao"].I;
            if (LunDaoManagerOnline.inst.lunDaoStateNameDictionary.ContainsKey(num))
            {
                this.npcStateId = num;
            }
            else
            {
                this.npcStateId = 3;
            }
            this.npcStateName = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.otherPlayerController.gameObject, "Status", true).GetComponent<Text>();
            this.npcStateName.text = LunDaoManagerOnline.inst.lunDaoStateNameDictionary[this.npcStateId];
            this.npcStateTips = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.otherPlayerController.gameObject, "StateTips", true);
            this.npcStateTips.GetComponentInChildren<Text>().text = jsonData.instance.LunDaoStateData[this.npcStateId.ToString()]["MiaoShu"].Str;
            this.sayWord = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.otherPlayerController.gameObject, "SayWords", true).AddComponent<LunDaoSayWordOnline>();

        }


        public void NpcSayWord(string content)
        {
            this.sayWord.Say(content);
        }


        public void NpcStartRound()
        {
            this.isSayWord = true;

            LunDaoManagerOnline.inst.lunDaoPanelOnline.AddNullSlot();

            this.cards = new List<LunDaoCard>();
            LunDaoManagerOnline.inst.lunDaoCardMagOnline.NpcDrawCard(this.cards);

            foreach (LunDaoCard lunDaoCard in this.cards)
            {
                Console.WriteLine("对方抽到卡wudaoId:" + lunDaoCard.wudaoId + " level:" + lunDaoCard.level);
            }

            if (this.cards.Count == 0)
            {
                this.NpcEndRound();
                return;
            }
            base.Invoke("NpcAction", 0.5f);
        }


        private void NpcAction()
        {
            LunDaoCard lunDaoCard = this.GetCanCompleteCard();
            if (lunDaoCard != null)
            {
                this.NpcUseCard(lunDaoCard);
            }
            else
            {
                lunDaoCard = this.GetNpcCanUseCard();
                if (lunDaoCard == null)
                {
                    this.NpcEndRound();
                    return;
                }
                this.NpcUseCard(lunDaoCard);
            }
            if (LunDaoManagerOnline.inst.gameState == LunDaoManagerOnline.GameState.对方回合 && LunDaoManagerOnline.inst.lunTiMagOnline.GetNullSlot() != -1)
            {
                base.Invoke("NpcAction", 1f);
                return;
            }
            this.NpcEndRound();
        }


        public void NpcUseCard(LunDaoCard card)
        {
            if (this.isSayWord)
            {
                this.isSayWord = false;
                int random = LunDaoManagerOnline.inst.lunDaoCardMagOnline.getRandom(1, 5);
                string str = jsonData.instance.LunDaoSayData[card.wudaoId.ToString()]["Desc" + random].Str;
                this.NpcSayWord(str);
            }
            LunDaoManagerOnline.inst.lunDaoAmrMagOnline.AddChuPai(card.wudaoId);
            int nullSlot = LunDaoManagerOnline.inst.lunTiMagOnline.GetNullSlot();
            LunDaoManagerOnline.inst.lunTiMagOnline.curLunDianList[nullSlot].SetData(card.wudaoId, card.level);
            this.cards.Remove(card);
            LunDaoManagerOnline.inst.ChuPaiCallBack();
        }


        public LunDaoCard GetNpcCanUseCard()
        {
            List<LunDaoCard> shengYuLunDian = LunDaoManagerOnline.inst.lunTiMagOnline.GetShengYuLunDian();
            int num = -1;
            LunDaoCard lunDaoCard = null;
            foreach (LunDaoCard lunDaoCard2 in this.cards)
            {
                foreach (LunDaoCard lunDaoCard3 in shengYuLunDian)
                {
                    if (lunDaoCard2.wudaoId == lunDaoCard3.wudaoId && lunDaoCard2.level <= lunDaoCard3.level && num < lunDaoCard2.level)
                    {
                        num = lunDaoCard2.level;
                        lunDaoCard = lunDaoCard2;
                    }
                }
            }
            return lunDaoCard;
        }


        public LunDaoCard GetCanCompleteCard()
        {
            foreach (LunDaoCard lunDaoCard in this.cards)
            {
                bool flag = false;
                this.VirtualUseCard(lunDaoCard, delegate (int index)
                {
                    if (LunDaoManagerOnline.inst.lunTiMagOnline.CheckIsTargetLunTi())
                    {
                        flag = true;
                    }
                    LunDaoManagerOnline.inst.lunTiMagOnline.curLunDianList[index].SetNull();
                });
                if (flag)
                {
                    return lunDaoCard;
                }
            }
            return null;
        }


        public void VirtualUseCard(LunDaoCard card, UnityAction<int> action)
        {
            int nullSlot = LunDaoManagerOnline.inst.lunTiMagOnline.GetNullSlot();
            LunDaoManagerOnline.inst.lunTiMagOnline.curLunDianList[nullSlot].SetData(card.wudaoId, card.level);
            action(nullSlot);
        }


        public void NpcEndRound()
        {
            foreach (LunDaoCard lunDaoCard in this.cards)
            {
                LunDaoManagerOnline.inst.lunDaoCardMagOnline.otherPlayerCards.Add(lunDaoCard);
            }
            this.cards = new List<LunDaoCard>();
            if (LunDaoManagerOnline.inst.gameState != LunDaoManagerOnline.GameState.论道结束)
            {
                LunDaoManagerOnline.inst.gameState = LunDaoManagerOnline.GameState.玩家回合;
            }
            LunDaoManagerOnline.inst.EndRoundCallBack();
            Debug.Log("Npc回合结束");
        }

        public string GetNpcName()
        {
            return this.npcName.text;
        }

        public void ShowStateTips()
        {
            this.npcStateTips.gameObject.SetActive(true);
        }

        public void HideStateTips()
        {
            this.npcStateTips.gameObject.SetActive(false);
        }


    }
}
