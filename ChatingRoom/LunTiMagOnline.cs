using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatingRoom
{
    public class LunTiMagOnline
    {
        public Dictionary<int, List<int>> targetLunTiDictionary;
        public List<LunDaoQiuOnline> curLunDianList;
        public LunTiMagOnline()
        {
            this.curLunDianList = new List<LunDaoQiuOnline>();
        }

        public void CreateLunTi(List<int> lunTiList, List<string> otherPlayerWuDaoLevel)
        {
            this.targetLunTiDictionary = new Dictionary<int, List<int>>();

            foreach (int num in lunTiList)
            {

                int num2 = int.Parse(otherPlayerWuDaoLevel[num]) + 1;
                int num3 = Tools.instance.getPlayer().wuDaoMag.getWuDaoLevelByType(num) + 1;
                if (num2 == num3)
                {
                    this.targetLunTiDictionary.Add(num, new List<int> { num2 + 1 });
                }
                else if (num2 > num3)
                {
                    this.targetLunTiDictionary.Add(num, new List<int> { num2, num3 });
                }
                else
                {
                    this.targetLunTiDictionary.Add(num, new List<int> { num3, num2 });
                }

            }
        }


        public bool CheckCanHeCheng(ref int minIndex, ref int bigIndex)
        {
            if (this.curLunDianList.Count < 1)
            {
                return false;
            }
            for (int i = 0; i < this.curLunDianList.Count; i++)
            {
                if (!this.curLunDianList[i].isNull)
                {
                    for (int j = this.curLunDianList.Count - 1; j > i; j--)
                    {
                        if (this.curLunDianList[i].wudaoId == this.curLunDianList[j].wudaoId && this.curLunDianList[i].level == this.curLunDianList[j].level)
                        {
                            minIndex = i;
                            bigIndex = j;
                            return true;
                        }
                    }
                }
            }
            minIndex = -1;
            bigIndex = -1;
            return false;
        }


        private bool CheckIsTargetLunTi(ref int wuDaoId)
        {
            foreach (int num in this.targetLunTiDictionary.Keys)
            {
                int num2 = 0;
                foreach (int num3 in this.targetLunTiDictionary[num])
                {
                    foreach (LunDaoQiuOnline lunDaoQiu in this.curLunDianList)
                    {
                        if (!lunDaoQiu.isNull && lunDaoQiu.wudaoId == num && lunDaoQiu.level == num3)
                        {
                            num2++;
                            break;
                        }
                    }
                }
                if (num2 == this.targetLunTiDictionary[num].Count)
                {
                    wuDaoId = num;
                    return true;
                }
            }
            wuDaoId = -1;
            return false;
        }


        public bool CheckIsTargetLunTi()
        {
            int num = -1;
            return this.CheckIsTargetLunTi(ref num);
        }

        public void CompleteLunTi()
        {
            int num = 0;
            if (this.CheckIsTargetLunTi(ref num))
            {
                LunDaoManagerOnline.inst.hasCompleteLunTi.Add(num);
                LunDaoManagerOnline.inst.AddWuDaoExp(num);
                int num2 = 0;
                foreach (int num3 in this.targetLunTiDictionary[num])
                {
                    num2 += jsonData.instance.WuDaoZhiJiaCheng[num3.ToString()]["JiaCheng"].I;
                }
                LunDaoManagerOnline.inst.AddWuDaoZhi(num, num2);
                this.targetLunTiDictionary.Remove(num);
                LunDaoManagerOnline.inst.lunDaoAmrMagOnline.AddCompleteLunTi(LunDaoManagerOnline.inst.lunDaoPanelOnline.lunTiCtrDictionary[num].finshIBg, LunDaoManagerOnline.inst.lunDaoPanelOnline.lunTiCtrDictionary[num].finshImage);
            }
            if (this.targetLunTiDictionary.Keys.Count < 1)
            {
                LunDaoManagerOnline.inst.gameState = LunDaoManagerOnline.GameState.论道结束;
                LunDaoManagerOnline.inst.GameOver();
            }
        }
        public void LunDianHeCheng()
        {
            int num = -1;
            int num2 = -1;
            while (this.CheckCanHeCheng(ref num, ref num2))
            {
                this.curLunDianList[num].LevelUp();
                int i = jsonData.instance.LunDaoShouYiData[this.curLunDianList[num].level.ToString()]["WuDaoZhi"].I;
                LunDaoManagerOnline.inst.AddWuDaoZhi(this.curLunDianList[num].wudaoId, i);
                this.curLunDianList[num2].SetNull();
                LunDaoManagerOnline.inst.lunDaoAmrMagOnline.AddHeCheng(this.curLunDianList[num].transform);
                this.CompleteLunTi();
            }
        }
        public int GetNullSlot()
        {
            int num = -1;
            for (int i = 0; i < this.curLunDianList.Count; i++)
            {
                if (this.curLunDianList[i].isNull)
                {
                    return i;
                }
            }
            return num;
        }
        public List<LunDaoCard> GetShengYuLunDian()
        {
            List<LunDaoCard> list = new List<LunDaoCard>();
            foreach (int num in this.targetLunTiDictionary.Keys)
            {
                foreach (int num2 in this.targetLunTiDictionary[num])
                {
                    list.Add(new LunDaoCard(num, num2));
                }
            }
            List<LunDaoCard> list2 = new List<LunDaoCard>();
            foreach (LunDaoQiuOnline lunDaoQiu in this.curLunDianList)
            {
                foreach (LunDaoCard lunDaoCard in list)
                {
                    if (lunDaoQiu.wudaoId == lunDaoCard.wudaoId && lunDaoQiu.level == lunDaoCard.level)
                    {
                        list2.Add(lunDaoCard);
                    }
                }
            }
            foreach (LunDaoCard lunDaoCard2 in list2)
            {
                list.Remove(lunDaoCard2);
            }
            return list;
        }


    }
}
