using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoCardMagOnline
    {

        public List<LunDaoCard> playerCards;
        public List<LunDaoCard> otherPlayerCards;
        public KBEngine.Avatar player;
        public System.Random random;

        public LunDaoCardMagOnline()
        {
            this.random = new System.Random();
            this.player = Tools.instance.getPlayer();
        }

        public void CreatePaiKu(List<int> lunTiList, List<string> otherPlayerWuDaoLevel)
        {
            this.otherPlayerCards = new List<LunDaoCard>();
            this.playerCards = new List<LunDaoCard>();
            Console.WriteLine("lunTiList.Count:" + lunTiList.Count);
            foreach (int num in lunTiList)
            {
                int i = int.Parse(otherPlayerWuDaoLevel[num]) + 1;
                int wuDaoLevelByType = this.player.wuDaoMag.getWuDaoLevelByType(num);
                for (int j = 1; j <= 5; j++)
                {
                    for (int k = 1; k <= 3; k++)
                    {
                        this.otherPlayerCards.Add(new LunDaoCard(num, (i - j >= 0) ? j : 0));
                        this.playerCards.Add(new LunDaoCard(num, (wuDaoLevelByType - j >= 0) ? j : 0));
                    }
                }       
            }
            Console.WriteLine("this.playerCards.Count:" + this.playerCards.Count);
        }
        public void NpcDrawCard(List<LunDaoCard> cards)
        {
            if (this.otherPlayerCards.Count < 5)
            {
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                int num;
                if (this.otherPlayerCards.Count == 1)
                {
                    num = 0;
                }
                else
                {
                    num = this.getRandom(0, this.otherPlayerCards.Count - 1);
                }
                cards.Add(new LunDaoCard(this.otherPlayerCards[num].wudaoId, this.otherPlayerCards[num].level));
                this.otherPlayerCards.RemoveAt(num);
            }
        }
        public void PlayerDrawCard(List<LunDaoPlayerCardOnline> cards)
        {
            GameObject playerCardsParent = ChatingRoomManager.GetGameObjectStatic<RectTransform>(ChatingRoomManager.lunDaoManagerOnline.gameObject, "PlayerCards", true);
            for (int i = 0; i < 5; i++)
            {
                int num = this.getRandom(0, this.playerCards.Count - 1);
                LunDaoCard lunDaoCard = new LunDaoCard(this.playerCards[num].wudaoId, this.playerCards[num].level);
                this.playerCards.RemoveAt(num);
                UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(LunDaoManagerOnline.inst.playerCardTemp, playerCardsParent.transform);
                LunDaoPlayerCardOnline lunDaoPlayerCardOnline = gameObject.AddComponent<LunDaoPlayerCardOnline>();
                lunDaoPlayerCardOnline.lunDaoCard = lunDaoCard;
                lunDaoPlayerCardOnline.cardImage = lunDaoPlayerCardOnline.GetComponentInChildren<Image>();
                lunDaoPlayerCardOnline.cardImage.sprite = LunDaoManagerOnline.inst.cardSpriteList[lunDaoCard.wudaoId - 1];

                lunDaoPlayerCardOnline.cardLevel = lunDaoPlayerCardOnline.GetComponentInChildren<Text>();
                lunDaoPlayerCardOnline.cardLevel.text = lunDaoCard.level.ToString();
                cards.Add(lunDaoPlayerCardOnline);
                lunDaoPlayerCardOnline.btn = lunDaoPlayerCardOnline.GetComponentInChildren<Button>();
                lunDaoPlayerCardOnline.btn.onClick.AddListener(() => lunDaoPlayerCardOnline.SelectCard());
                gameObject.gameObject.SetActive(true);
            }
        }

        public int getRandom(int min, int max)
        {
            return this.random.Next(min, max + 1);
        }

    }
}
