using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JSONClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoSuccessOnline : MonoBehaviour
    {

        [SerializeField]
        private Image siXuCell;
        [SerializeField]
        private Transform siXuCellParent;
        public List<Sprite> siXuList;
        [SerializeField]
        private Image curJinDu;
        [SerializeField]
        private Text jinDu;
        [SerializeField]
        private Text addWuDaoZhiText;
        [SerializeField]
        private Text addWuDaoDian;
        public Button closeButton;

        public void Init()
        {
            siXuCellParent = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "SiXuList", true).transform;
            siXuCell = ChatingRoomManager.wuDaoQiuPrefab.GetComponent<Image>();
            curJinDu = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "SliderFill", true).GetComponent<Image>();
            jinDu = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "CurJinDu", true).GetComponent<Text>();
            addWuDaoZhiText = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "AddNum", true).GetComponent<Text>();
            addWuDaoDian = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "SuccessContent", true).GetComponent<Text>();
            closeButton = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "SuccessClose", true).GetComponent<Button>();
            closeButton.onClick.AddListener(() => LunDaoManagerOnline.inst.Close());

            siXuList = new List<Sprite>();

            for (int i = 1; i < ChatingRoomManager.lunTiNameSpriteList.Count; i++)
            {
                siXuList.Add(ChatingRoomManager.lunTiNameSpriteList[i]);
            }


            if (base.gameObject.activeSelf)
            {
                return;
            }
            base.gameObject.SetActive(true);
            KBEngine.Avatar player = Tools.instance.getPlayer();
            int npcStateId = LunDaoManagerOnline.inst.otherPlayerController.npcStateId;
            foreach (int num in LunDaoManagerOnline.inst.getWuDaoExp.Keys)
            {
                UnityEngine.GameObject gameObject = UnityEngine.Object.Instantiate<UnityEngine.GameObject>(this.siXuCell.gameObject, this.siXuCellParent);
                gameObject.GetComponent<Image>().sprite = this.siXuList[num - 1];
                gameObject.SetActive(true);
            }
            this.AddPlayerLunDaoSiXu();
            int wuDaoZhi = this.GetWuDaoZhi();
            //if (npcStateId != 4)
            //{
            //    NpcJieSuanManager.inst.npcSetField.AddNpcWuDaoZhi(LunDaoManagerOnline.inst.npcId, wuDaoZhi);
            //    this.AddNpcWuDaoExp();
            //}
            player.ReduceLingGan(jsonData.instance.LunDaoStateData[player.LunDaoState.ToString()]["LingGanXiaoHao"].I);
            this.addWuDaoZhiText.text = wuDaoZhi.ToString();
            int wuDaoZhiLevel = player.WuDaoZhiLevel;
            int startWuDaoZhi = player.WuDaoZhi;
            int num2 = 0;
            player.WuDaoZhi += wuDaoZhi;
            player.WuDaoZhiLevel = this.GetPlayerLevel(wuDaoZhiLevel, player.WuDaoZhi, ref num2);
            int curMax = jsonData.instance.WuDaoZhiData[wuDaoZhiLevel.ToString()]["LevelUpExp"].I;
            this.curJinDu.fillAmount = (float)startWuDaoZhi / (float)curMax;
            if (player.WuDaoZhiLevel - wuDaoZhiLevel > 0)
            {
                this.addWuDaoDian.text = string.Format("悟道点+{0}({1}/{2})", player.WuDaoZhiLevel - wuDaoZhiLevel, player.WuDaoZhiLevel, WuDaoZhiData.DataList.Count - 1);
                player._WuDaoDian += player.WuDaoZhiLevel - wuDaoZhiLevel;
                this.addWuDaoDian.gameObject.SetActive(true);
                float num3 = 0.5f / (float)(player.WuDaoZhiLevel - wuDaoZhiLevel);
                this.MoreSlider(startWuDaoZhi, player.WuDaoZhi, wuDaoZhiLevel, num3, false);
            }
            else if (startWuDaoZhi > curMax)
            {
                this.jinDu.text = string.Format("({0}/极限)", startWuDaoZhi);
            }
            else
            {
                float num4 = (float)player.WuDaoZhi / (float)curMax;
                this.curJinDu.DOFillAmount(num4, 0.5f);
                DOTween.To(() => startWuDaoZhi, delegate (int x)
                {
                    startWuDaoZhi = x;
                    this.jinDu.text = string.Format("({0}/{1})", startWuDaoZhi, curMax);
                }, player.WuDaoZhi, 0.5f);
            }
            player.WuDaoZhi = num2;
            //if (NpcJieSuanManager.inst.lunDaoNpcList.Contains(LunDaoManagerOnline.inst.npcId))
            //{
            //    NpcJieSuanManager.inst.lunDaoNpcList.Remove(LunDaoManagerOnline.inst.npcId);
            //}
            //NpcJieSuanManager.inst.npcNoteBook.NoteLunDaoSuccess(LunDaoManager.inst.npcId, player.name);
        }


        private int GetWuDaoZhi()
        {
            int num = LunDaoManagerOnline.inst.getWuDaoZhi;
            float f = jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.otherPlayerController.npcStateId.ToString()]["WuDaoZhi"].f;
            float f2 = jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.playerControllerOnline.playerStateId.ToString()]["WuDaoZhi"].f;
            float num2 = (f + f2) / 100f;
            num += (int)((float)num * num2);
            if (num < 0)
            {
                num = 0;
            }
            return num;
        }


        private int GetPlayerLevel(int curLevel, int curExp, ref int endExp)
        {
            int num = jsonData.instance.WuDaoZhiData[curLevel.ToString()]["LevelUpExp"].I;
            int num2 = curLevel;
            while (curExp >= num)
            {
                num2++;
                if (!jsonData.instance.WuDaoZhiData.HasField(num2.ToString()))
                {
                    num2--;
                    break;
                }
                curExp -= num;
                num = jsonData.instance.WuDaoZhiData[num2.ToString()]["LevelUpExp"].I;
            }
            endExp = curExp;
            return num2;
        }


        private void MoreSlider(int curExp, int endExp, int curLevel, float time, bool flag = false)
        {
            int nextExp = jsonData.instance.WuDaoZhiData[curLevel.ToString()]["LevelUpExp"].I;
            if (flag)
            {
                DOTween.To(() => this.curJinDu.fillAmount, x => this.curJinDu.fillAmount = x, ((float)endExp) / ((float)nextExp), time).Play<TweenerCore<float, float, FloatOptions>>();
                DOTween.To(() => curExp, delegate (int x)
                {
                    curExp = x;
                    this.jinDu.text = string.Format("({0}/{1})", curExp, nextExp);
                }, endExp, time).Play<TweenerCore<int, int, NoOptions>>();
            }
            else
            {
                DOTween.To(() => this.curJinDu.fillAmount, x => this.curJinDu.fillAmount = x, 1f, time).Play<TweenerCore<float, float, FloatOptions>>();
                DOTween.To(() => curExp, delegate (int x)
                {
                    curExp = x;
                    this.jinDu.text = string.Format("({0}/{1})", curExp, nextExp);
                }, nextExp, time).OnComplete<TweenerCore<int, int, NoOptions>>(delegate
                {
                    if (!flag)
                    {
                        endExp -= nextExp;
                        int num = curLevel;
                        curLevel = num + 1;
                        this.curJinDu.fillAmount = (float)0f;
                        if (!jsonData.instance.WuDaoZhiData.HasField(curLevel.ToString()))
                        {
                            this.jinDu.text = string.Format("({0}/极限)", curExp);
                        }
                        else
                        {
                            nextExp = jsonData.instance.WuDaoZhiData[curLevel.ToString()]["LevelUpExp"].I;
                            if (endExp > nextExp)
                            {
                                this.MoreSlider(0, endExp, curLevel, time, false);
                            }
                            else
                            {
                                this.MoreSlider(0, endExp, curLevel, time, true);
                            }
                        }
                    }
                }).Play<TweenerCore<int, int, NoOptions>>();
            }
        }

        // Token: 0x06001C3B RID: 7227 RVA: 0x000C8D54 File Offset: 0x000C6F54
        private void AddPlayerLunDaoSiXu()
        {
            KBEngine.Avatar player = Tools.instance.getPlayer();
            Dictionary<int, List<int>> getWuDaoExp = LunDaoManagerOnline.inst.getWuDaoExp;
            int num = 0;
            int num2 = 0;
            DateTime nowTime = player.worldTimeMag.getNowTime();
            string text = string.Format("{0}年{1}月{2}日", nowTime.Year, nowTime.Month, nowTime.Day);
            string npcName = LunDaoManagerOnline.inst.otherPlayerController.GetNpcName();
            float i = (float)jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.otherPlayerController.npcStateId.ToString()]["WuDaoExp"].I;
            float num3 = (float)jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.playerControllerOnline.playerStateId.ToString()]["WuDaoExp"].I;
            float num4 = (i + num3) / 100f;
            foreach (int num5 in getWuDaoExp.Keys)
            {
                num = 0;
                num2 = 0;
                int num6 = 0;
                foreach (int num7 in getWuDaoExp[num5])
                {
                    num2 += num7;
                    num += jsonData.instance.LunDaoShouYiData[num7.ToString()]["WuDaoExp"].I;
                    num6++;
                }
                if (num6 == 2)
                {
                    if (num2 % 2 > 0)
                    {
                        num2++;
                    }
                    num2 /= 2;
                }
                else
                {
                    num2--;
                }
                num += (int)((float)num * num4);
                if (num > 0)
                {
                    string str = jsonData.instance.WuDaoAllTypeJson[num5.ToString()]["name1"].Str;
                    int num8 = num / jsonData.instance.LunDaoSiXuData[num2.ToString()]["SiXvXiaoLv"].I;
                    int num9 = LingGanTimeMaxData.DataDict[(int)player.level].MaxTime;
                    int lunDaoState = player.GetLunDaoState();
                    if (lunDaoState == 1)
                    {
                        num9 *= 4;
                    }
                    else if (lunDaoState == 2)
                    {
                        num9 *= 2;
                    }
                    if (num8 > num9)
                    {
                        num8 = num9;
                    }
                    player.wuDaoMag.AddLingGuang("对" + str + "的感悟", num5, num8, 1825, string.Concat(new string[] { "在", text, "你与", npcName, "论道时，对", str, "产生了灵光一现的感悟" }), jsonData.instance.LunDaoSiXuData[num2.ToString()]["PinJie"].I, true);
                }
            }
            if (num4 > -1f)
            {
                UIPopTip.Inst.Pop("获得新的感悟", PopTipIconType.感悟);
            }
        }

        // Token: 0x06001C3C RID: 7228 RVA: 0x000C9074 File Offset: 0x000C7274
        //private void AddNpcWuDaoExp()
        //{
        //    int npcId = LunDaoManagerOnline.inst.npcId;
        //    Dictionary<int, List<int>> getWuDaoExp = LunDaoManagerOnline.inst.getWuDaoExp;
        //    foreach (int num in getWuDaoExp.Keys)
        //    {
        //        int num2 = 0;
        //        float i = (float)jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.otherPlayerController.npcStateId.ToString()]["WuDaoExp"].I;
        //        float num3 = (float)jsonData.instance.LunDaoStateData[LunDaoManagerOnline.inst.playerControllerOnline.playerStateId.ToString()]["WuDaoExp"].I;
        //        float num4 = (i + num3) / 100f;
        //        foreach (int num5 in getWuDaoExp[num])
        //        {
        //            num2 += jsonData.instance.LunDaoShouYiData[num5.ToString()]["WuDaoExp"].I;
        //        }
        //        num2 += (int)((float)num2 * num4);
        //        if (num2 >= 0)
        //        {
        //            NpcJieSuanManager.inst.npcSetField.AddNpcWuDaoExp(npcId, num, num2);
        //        }
        //    }
        //}


    }
}
