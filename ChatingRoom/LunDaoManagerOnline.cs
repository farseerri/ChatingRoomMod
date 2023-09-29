using JSONClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static ChatingRoom.ChatingRoomManager;

namespace ChatingRoom
{
    public class LunDaoManagerOnline : MonoBehaviour
    {
        public static LunDaoManagerOnline inst;
        public static OtherPlayerOnlineDataClass otherPlayerOnlineDataClass;


        public LunDaoAmrMagOnline lunDaoAmrMagOnline;
        public SelectLunTi selectLunTi;
        public PlayerControllerOnline playerControllerOnline;
        public OtherPlayerController otherPlayerController;
        public LunDaoCardMagOnline lunDaoCardMagOnline;
        public LunTiMagOnline lunTiMagOnline;
        public LunDaoPanelOnline lunDaoPanelOnline;
        public LunDaoSuccessOnline lunDaoSuccessPanel;
        public Dictionary<int, string> lunDaoStateNameDictionary;
        public List<int> selectLunTiList;
        //public int npcId;
        public LunDaoManagerOnline.GameState gameState;
        public KBEngine.Avatar player;
        public List<int> hasCompleteLunTi;
        public Transform AnimatorPanel;
        public List<Sprite> cardSprites;
        public List<AudioClip> musicEffectList;
        public Dictionary<int, List<int>> getWuDaoExp = new Dictionary<int, List<int>>();

        [SerializeField]
        private Text wuDaoZhi;

        public int getWuDaoZhi;

        public List<Sprite> cardSpriteList;

        public UnityEngine.GameObject playerCardTemp;

        public UnityEngine.GameObject LunDaoFailPanel;

        public Button exitButton;
        public Button failButton;
        public bool isOver;

        public enum GameState
        {
            玩家回合 = 1,
            对方回合,
            论道结束
        }

        private void Awake()
        {
            LunDaoManagerOnline.inst = this;
            //this.npcId = Tools.instance.LunDaoNpcId;
            this.hasCompleteLunTi = new List<int>();
            this.lunDaoAmrMagOnline = new LunDaoAmrMagOnline();
            this.lunDaoStateNameDictionary = new Dictionary<int, string>();
            foreach (JSONObject jsonobject in jsonData.instance.LunDaoStateData.list)
            {
                this.lunDaoStateNameDictionary.Add(jsonobject["id"].I, jsonobject["ZhuangTaiInfo"].Str);
            }
        }

        private void Start()
        {

            this.lunDaoPanelOnline = GetGameObjectStatic<RectTransform>(this.gameObject, "LunDaoPanel", true).AddComponent<LunDaoPanelOnline>();

            this.player = Tools.instance.getPlayer();
            this.lunTiMagOnline = new LunTiMagOnline();
            this.lunDaoCardMagOnline = new LunDaoCardMagOnline();

            this.playerControllerOnline = GetGameObjectStatic<RectTransform>(this.gameObject, "PlayerPanel", true).AddComponent<PlayerControllerOnline>();
            this.otherPlayerController = GetGameObjectStatic<RectTransform>(this.gameObject, "OtherPlayerPanel", true).AddComponent<OtherPlayerController>();
            this.AnimatorPanel = GetGameObjectStatic<RectTransform>(this.gameObject, "AnimatorPanel", true).transform;
            this.wuDaoZhi = GetGameObjectStatic<RectTransform>(this.gameObject, "GetWuDaoZhi", true).GetComponentInChildren<Text>();
            this.playerCardTemp = playerCardPrefab;
            this.exitButton = GetGameObjectStatic<RectTransform>(this.gameObject, "CloseLunDao", true).GetComponentInChildren<Button>();
            this.lunDaoSuccessPanel = GetGameObjectStatic<RectTransform>(this.gameObject, "SuccessPanel", true).AddComponent<LunDaoSuccessOnline>();
            this.LunDaoFailPanel = GetGameObjectStatic<RectTransform>(this.gameObject, "FailPanel", true);
            this.failButton = GetGameObjectStatic<RectTransform>(LunDaoFailPanel.gameObject, "FailClose", true).GetComponentInChildren<Button>();
            exitButton.onClick.AddListener(() => Close());
            failButton.onClick.AddListener(() => Close());
            cardSpriteList = new List<Sprite>();

            for (int i = 1; i < lunTiNameSpriteList.Count; i++)
            {
                cardSpriteList.Add(lunTiNameSpriteList[i]);
            }

            this.InitLunDao();
        }

        private void InitLunDao()
        {

            this.StartGame();
            this.playerControllerOnline.Init();
            this.otherPlayerController.Init();
            this.otherPlayerController.NpcStartRound();
        }


        public int GetLunDaoNum(string lunTiString)
        {
            switch (lunTiString)
            {
                case "金":
                    return 1;
                case "木":
                    return 2;
                case "水":
                    return 3;
                case "火":
                    return 4;
                case "土":
                    return 5;
                case "神":
                    return 6;
                case "体":
                    return 7;
                case "剑":
                    return 8;
                case "气":
                    return 9;
                case "阵":
                    return 10;
                default:
                    return 0;
            }
        }


        public void StartGame()
        {

            this.selectLunTiList = new List<int>();
            Console.WriteLine("otherPlayerOnlineDataClass.lunTiList:" + otherPlayerOnlineDataClass.lunTiList.Count);
            foreach (string lunTiString in otherPlayerOnlineDataClass.lunTiList)
            {
                Console.WriteLine("选择了论题:" + lunTiString);
                int lunTiNum = GetLunDaoNum(lunTiString);
                if (lunTiNum > 0)
                {
                    this.selectLunTiList.Add(lunTiNum);
                }
            }



            this.lunTiMagOnline.CreateLunTi(this.selectLunTiList, otherPlayerOnlineDataClass.wuDaoLevelList);

            this.lunDaoCardMagOnline.CreatePaiKu(this.selectLunTiList, otherPlayerOnlineDataClass.wuDaoLevelList);

            this.lunDaoPanelOnline.Init();

            this.lunDaoPanelOnline.Show();

            this.gameState = LunDaoManagerOnline.GameState.对方回合;

        }

        public void EndRoundCallBack()
        {
            if (this.gameState == LunDaoManagerOnline.GameState.玩家回合)
            {
                this.playerControllerOnline.PlayerStartRound();
                return;
            }
            if (this.gameState == LunDaoManagerOnline.GameState.对方回合)
            {
                this.otherPlayerController.NpcStartRound();
                return;
            }
            if (this.gameState == LunDaoManagerOnline.GameState.论道结束)
            {
                this.GameOver();
            }
        }

        public void GameOver()
        {
            if (this.isOver)
            {
                return;
            }
            this.isOver = true;
            if (this.hasCompleteLunTi.Count > 0)
            {
                base.Invoke("LunDaoSuccess", 1f);
                return;
            }
            base.Invoke("LunDaoFail", 1f);
        }


        public void ChuPaiCallBack()
        {
            this.lunTiMagOnline.CompleteLunTi();

            this.lunTiMagOnline.LunDianHeCheng();

            if (this.gameState == LunDaoManagerOnline.GameState.玩家回合 && LunDaoManagerOnline.inst.lunTiMagOnline.GetNullSlot() == -1)
            {

                this.playerControllerOnline.tips.SetActive(true);

                return;
            }

            this.playerControllerOnline.tips.SetActive(false);

        }


        public void LunDaoSuccess()
        {
            this.lunDaoSuccessPanel.Init();
        }

        public void LunDaoFail()
        {
            //if (NpcJieSuanManager.inst.lunDaoNpcList.Contains(this.npcId))
            //{
            //    NpcJieSuanManager.inst.lunDaoNpcList.Remove(this.npcId);
            //}
            //NpcJieSuanManager.inst.npcNoteBook.NoteLunDaoFail(this.npcId, this.player.name);
            this.LunDaoFailPanel.SetActive(true);
        }

        public void Close()
        {
            Tools.instance.TargetLunTiNum = this.hasCompleteLunTi.Count;
            //NpcJieSuanManager.inst.npcStatus.SetNpcStatus(this.npcId, 12);
            PanelMamager.CanOpenOrClose = true;
            GameObject.Destroy(this.gameObject);
            //Tools.instance.loadMapScenes(Tools.instance.FinalScene, true);
        }


        public void AddWuDaoZhi(int wudaoId, int addNum)
        {

            int i = int.Parse(otherPlayerOnlineDataClass.wuDaoLevelList[wudaoId]) + 1;
            int wuDaoLevelByType = Tools.instance.getPlayer().wuDaoMag.getWuDaoLevelByType(wudaoId);
            if (wuDaoLevelByType < i)
            {

                addNum = addNum * LunDaoReduceData.DataDict[i - wuDaoLevelByType].ShuaiJianXiShu / 100;
            }

            this.getWuDaoZhi += addNum;

            this.wuDaoZhi.text = this.getWuDaoZhi.ToString();

        }

        public void AddWuDaoExp(int wuDaoId)
        {
            foreach (int num in this.lunTiMagOnline.targetLunTiDictionary[wuDaoId])
            {
                if (!this.getWuDaoExp.ContainsKey(wuDaoId))
                {
                    this.getWuDaoExp.Add(wuDaoId, new List<int> { num });
                }
                else
                {
                    this.getWuDaoExp[wuDaoId].Add(num);
                }
            }
        }

        public List<int> GetSuiJiLuntTi(int num)
        {
            List<int> list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<int> list2 = new List<int>();
            for (int i = 0; i < num; i++)
            {
                int random = this.lunDaoCardMagOnline.getRandom(0, list.Count - 1);
                list2.Add(list[random]);
                list.RemoveAt(random);
            }
            return list2;
        }



    }
}
