using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoPlayerCardOnline : MonoBehaviour
    {
        public LunDaoCard lunDaoCard;
        public Image cardImage;
        public Text cardLevel;
        public Button btn;
        public bool isSelected;

        public void SelectCard()
        {
            if (LunDaoManagerOnline.inst.gameState != LunDaoManagerOnline.GameState.玩家回合)
            {
                return;
            }
            if (LunDaoManagerOnline.inst.playerControllerOnline.tips.activeSelf)
            {
                return;
            }
            this.isSelected = !this.isSelected;
            if (this.isSelected)
            {
                this.cardImage.gameObject.transform.localPosition = new Vector3(0f, 30.79f, 0f);
                if (LunDaoManagerOnline.inst.playerControllerOnline.selectCard != null)
                {
                    LunDaoManagerOnline.inst.playerControllerOnline.selectCard.cardImage.gameObject.transform.localPosition = Vector3.zero;
                    LunDaoManagerOnline.inst.playerControllerOnline.selectCard.isSelected = false;
                }
                LunDaoManagerOnline.inst.playerControllerOnline.selectCard = this;
                LunDaoManagerOnline.inst.playerControllerOnline.ShowChuPaiBtn();
                return;
            }
            this.cardImage.gameObject.transform.localPosition = Vector3.zero;
            LunDaoManagerOnline.inst.playerControllerOnline.selectCard = null;
            LunDaoManagerOnline.inst.playerControllerOnline.HideChuPaiBtn();
        }
    }
}
