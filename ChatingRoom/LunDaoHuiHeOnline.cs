using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoHuiHeOnline : MonoBehaviour
    {
        public int totalHui;
        public int curHui;
        public int shengYuHuiHe;
        [SerializeField]
        private Text curHuiText;
        [SerializeField]
        private Text shengYuHuiHeText;
        public void Init()
        {
            this.curHuiText = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "CurHuiHe", true).GetComponent<Text>();
            this.shengYuHuiHeText = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "ShengYuHui", true).GetComponent<Text>();
            this.totalHui = 5;
            this.curHui = 1;
            this.shengYuHuiHe = this.totalHui - this.curHui;
            this.upDateHuiHeText();
        }

        public void ReduceHuiHe()
        {
            this.shengYuHuiHe--;
            this.curHui++;
            if (this.curHui > this.totalHui)
            {
                LunDaoManagerOnline.inst.gameState = LunDaoManagerOnline.GameState.论道结束;
                return;
            }
            this.upDateHuiHeText();
        }

        private void upDateHuiHeText()
        {
            this.curHuiText.text = this.curHui.ToCNNumber();
            this.shengYuHuiHeText.text = "(剩余" + this.shengYuHuiHe.ToCNNumber() + "回合)";
        }


    }
}
