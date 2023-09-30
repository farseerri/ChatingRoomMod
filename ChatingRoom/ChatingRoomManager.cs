using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Steamworks;
using UnityEngine.UI;
using YSGame;
using JSONClass;
using LitJson;
using Bag;
using GUIPackage;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Spine.Unity;
using SkySwordKill.Next.DialogTrigger;
using GoogleMobileAds.Api;
using System.Runtime.InteropServices.ComTypes;
using UltimateSurvival.StandardAssets;

namespace ChatingRoom
{
    [BepInPlugin("michangsheng.mumu.chatingroom", "ChatingRoom", "1.0")]
    public class ChatingRoomManager : BaseUnityPlugin
    {
        Harmony harmony;
        public static ChatingRoomManager Inst;

        public static GameObject chatingRoomEnterButtonPrefab;
        public static GameObject chatingRoomMainPanelPrefab;
        public static GameObject userbarlPrefab;
        public static GameObject friendbarPrefab;
        public static GameObject groupPanelEnterButtonPrefab;

        public static GameObject dropdownPrefab;
        public static GameObject customLunDaoManagerPrefab;
        public static GameObject dialogBoxPrefab;
        public static GameObject startLunTiCellPrefab;
        public static GameObject wuDaoQiuPrefab;
        public static GameObject lunDaoQiuSlotPrefab;
        public static GameObject playerCardPrefab;
        public static GameObject groupMainPanelPrefab;

        public static ChatingRoomPanelBase chatingRoomPanel;
        public static CSteamID targetID;
        public static bool toolTipPositionfix = false;
        public static GameObject cardsPanel;
        public static GameObject dialogBox;
        public static List<string> lastMessageList = new List<string>();
        public static int lastMessageHoldCount = 100;
        public static Vector3 yourPostion;
        public static GameObject onlinePlayerIconPrefab;
        public static GameObject playerDataPanelPrefab;
        public static OnlinePlayersPanel onlinePlayersPanel;
        public static PlayerDataPanel playerDataPanel;

        //public enum 联机论道枚举
        //{
        //    未启动,
        //    初始化,
        //    我方阶段,
        //    对方阶段
        //}

        //public static 联机论道枚举 联机论道状态 = 联机论道枚举.未启动;

        public class OtherPlayerOnlineDataClass
        {
            public string name;
            public string wuDaoJsonString;
            public string faceJsonString;
            public List<string> wuDaoLevelList;
            public int StatusId;
            public List<string> lunTiList;
            public OtherPlayerOnlineDataClass()
            {
                name = "";
                wuDaoJsonString = "";
                faceJsonString = "";
                wuDaoLevelList = new List<string>();
                StatusId = 0;
                lunTiList = new List<string>();
            }
        }
        public static OtherPlayerOnlineDataClass myLunDaoOnlineData;

        public class CustomMessage
        {
            public string senderID;
            public string receiveID;
            public string commandID;
            public string commandContent;
        }

        public class CustomPlayerDataClass
        {
            public string itemData;
            public string customPrice;
            public string xiuxianName;
            public string sex;
            public string age;
            public string menpai;
            public string jingJie;
            public string shenShi;
            public string faceID;
            public string lunDaoState;
            public string gameFaceData;
        }




        public static CustomMessage sendMessage;
        public static CustomMessage receiveMessage;



        private struct Lobby
        {
            public CSteamID Id;
            public string Name;
            public string gameName;
            public int playerCount;
            public int maxPlayers;
        }


        private List<Lobby> lobbies = new List<Lobby>();
        private static CSteamID lobbyID;
        static string lobbyName = "";
        static string lobbyNamePre = "MiChangShengChatingRoom";

        private Callback<LobbyCreated_t> lobbyCreatedCallback;
        private Callback<LobbyMatchList_t> lobbyMatchListCallback;
        private Callback<LobbyEnter_t> lobbyEnteredCallback;
        private static Callback<LobbyChatMsg_t> lobbyChatMsgCallback;
        private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;
        private Callback<AvatarImageLoaded_t> avatarImageLoaded;
        private Callback<P2PSessionRequest_t> p2PSessionRequestCallback;

        public static GameObject chatingBarPrefab;
        public static Text chatingText;



        public static Button chatingRoomEnterButton;
        public static Button groupPanelEnterButton;
        private static string yourNowScene = "";
        public static string jiaoYiCangKuId = "130";



        public static Dictionary<CSteamID, string> members;

        public static Color greenChatringBarColor = new Color32(0x2F, 0x43, 0x45, 0xff);

        public static SensitiveWordsFilter sensitiveWordsFilter;

        public static LunDaoManagerOnline lunDaoManagerOnline;
        public static Sprite tempSprite;
        public static List<Sprite> lunTiNameSpriteList;
        //public static bool is聊天框弹出时置顶 = false;
        public static int activePlayerCount = 0;
        public string Path
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            }
        }

        static public string ReadFileByString(string filename, Encoding encoding)
        {
            string readFileBuffer = "";
            StreamReader streamReader = new StreamReader(filename, encoding);
            readFileBuffer = streamReader.ReadToEnd();
            streamReader.Close();
            return readFileBuffer;
        }

        static public string WriteFileByString(string outputFileName, string inputString, Encoding encoding)
        {
            string err = "";
            try
            {
                byte[] data = encoding.GetBytes(inputString);

                FileStream fs = new FileStream(outputFileName, FileMode.Create);

                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Close();
            }
            catch (Exception e)
            {
                err = e.ToString();
            }
            return err;
        }



        public void Awake()
        {
            ChatingRoomManager.Inst = this;
        }


        public IEnumerator Start()
        {

            sensitiveWordsFilter = new SensitiveWordsFilter();

            //harmony = Harmony.CreateAndPatchAll(typeof(ChatingRoomManager));
            harmony = new Harmony("michangsheng.mumu.chatingroom");


            var original_YSNewSaveSystem_LoadSave = AccessTools.Method(typeof(YSNewSaveSystem), "LoadSave");
            var ySNewSaveSystem_LoadSave_Prefix = new HarmonyMethod(typeof(ChatingRoomManager), "YSNewSaveSystem_LoadSave_Prefix");
            ySNewSaveSystem_LoadSave_Prefix.priority = -90;
            var ySNewSaveSystem_LoadSave_Postfix = new HarmonyMethod(typeof(ChatingRoomManager), "YSNewSaveSystem_LoadSave_Postfix");
            ySNewSaveSystem_LoadSave_Postfix.priority = -90;
            harmony.Patch(original_YSNewSaveSystem_LoadSave, ySNewSaveSystem_LoadSave_Prefix, ySNewSaveSystem_LoadSave_Postfix);

            var original_ToolTipsMag_PCSetPosition = AccessTools.Method(typeof(ToolTipsMag), "PCSetPosition");
            var toolTipsMag_PCSetPosition_Postfix = new HarmonyMethod(typeof(ChatingRoomManager), "ToolTipsMag_PCSetPosition_Postfix");
            toolTipsMag_PCSetPosition_Postfix.priority = -90;
            harmony.Patch(original_ToolTipsMag_PCSetPosition, null, toolTipsMag_PCSetPosition_Postfix);


            var original_UIMapPanel_ShowPanel = AccessTools.Method(typeof(UIMapPanel), "ShowPanel");
            var uIMapPanel_ShowPanel_Prefix = new HarmonyMethod(typeof(ChatingRoomManager), "UIMapPanel_ShowPanel_Prefix");
            uIMapPanel_ShowPanel_Prefix.priority = -90;
            harmony.Patch(original_UIMapPanel_ShowPanel, uIMapPanel_ShowPanel_Prefix, null);


            var original_NewMainUIManager_Awake = AccessTools.Method(typeof(NewMainUIManager), "Awake");
            var newMainUIManager_Awake_Prefix = new HarmonyMethod(typeof(ChatingRoomManager), "NewMainUIManager_Awake_Prefix");
            newMainUIManager_Awake_Prefix.priority = -90;
            harmony.Patch(original_NewMainUIManager_Awake, newMainUIManager_Awake_Prefix, null);


            var original_NextScene_Start = AccessTools.Method(typeof(NextScene), "Start");
            var nextScene_Start_Prefix = new HarmonyMethod(typeof(ChatingRoomManager), "NextScene_Start_Prefix");
            nextScene_Start_Prefix.priority = -90;
            harmony.Patch(original_NextScene_Start, nextScene_Start_Prefix, null);


            yield return StartCoroutine(this.LoadAssetAsync());

            Console.WriteLine("聊天室mod启动");

            lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
            lobbyEnteredCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
            p2PSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);

            yield return new WaitForSeconds(2f);


            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            bool isMCSExtraToolsExist = false;
            Assembly mCSExtraToolsAssembly = null;
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().ToString().Contains("ExtraUtilities"))
                {
                    Console.WriteLine("检测到ExtraUtilities插件，载入热键冲突修复程序");

                    Type otherType = assembly.GetType("UniqueCream.ExtraUtilities._GaoShi");

                    MethodInfo otherMethod = otherType.GetMethod("ResetGaoShi");

                    if (otherMethod != null)
                    {
                        HarmonyMethod prefix = new HarmonyMethod(typeof(ChatingRoomManager).GetMethod("ResetGaoShiPrefixMethod"));
                        harmony.Patch(otherMethod, prefix, null);
                    }
                }

                if (assembly.GetName().ToString().Contains("MCSExtraTools"))
                {
                    isMCSExtraToolsExist = true;
                    mCSExtraToolsAssembly = assembly;
                }

            }

            var original_Tools_loadMapScenes = AccessTools.Method(typeof(Tools), "loadMapScenes");
            if (isMCSExtraToolsExist)
            {
                Console.WriteLine("检测到MCSExtraTools插件，进行场景淡入淡出回调函数适配");
                if (mCSExtraToolsAssembly != null)
                {
                    Type otherType = mCSExtraToolsAssembly.GetType("UniqueCream.MCSExtraTools.RecreateSceneOverAnimation.Interface.SceneOverAnimationBase");
                    MethodInfo otherMethod = otherType.GetMethod("EndAnimation");
                    if (otherMethod != null)
                    {
                        HarmonyMethod tools_loadMapScenes_Prefix_Partial = new HarmonyMethod(typeof(ChatingRoomManager).GetMethod("Tools_loadMapScenes_Prefix_Partial"));
                        tools_loadMapScenes_Prefix_Partial.priority = -90;
                        harmony.Patch(original_Tools_loadMapScenes, tools_loadMapScenes_Prefix_Partial, null);

                        HarmonyMethod postfix = new HarmonyMethod(typeof(ChatingRoomManager).GetMethod("EndAnimationPostFix"));
                        harmony.Patch(otherMethod, null, postfix);
                    }
                }
            }
            else
            {
                Console.WriteLine("没有检测到MCSExtraTools插件，直接适配游戏本体接口");

                HarmonyMethod tools_loadMapScenes_Prefix_Completed = new HarmonyMethod(typeof(ChatingRoomManager), "Tools_loadMapScenes_Prefix_Completed");
                tools_loadMapScenes_Prefix_Completed.priority = -90;
                harmony.Patch(original_Tools_loadMapScenes, tools_loadMapScenes_Prefix_Completed, null);
            }






            lunTiNameSpriteList = new List<Sprite>();
            yield return LoadSpriteCoroutine("file://" + this.Path + "/空.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/金.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/木.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/水.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/火.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/土.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/神.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/体.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/剑.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/气.png");
            lunTiNameSpriteList.Add(tempSprite);
            yield return LoadSpriteCoroutine("file://" + this.Path + "/阵.png");
            lunTiNameSpriteList.Add(tempSprite);



        }




        public static bool ResetGaoShiPrefixMethod()
        {
            //Console.WriteLine("修正了与ExtraUtilities插件的冲突");
            if (ChatingRoomManager.chatingRoomPanel == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Update()
        {
            if (ChatingRoomManager.onlinePlayersPanel != null && ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator != null)
            {
                uint p2PreceiveMessagesCount;
                if (SteamNetworking.IsP2PPacketAvailable(out p2PreceiveMessagesCount))
                {
                    // allocate buffer and needed variables
                    var buffer = new byte[p2PreceiveMessagesCount];
                    uint bytesRead;
                    CSteamID remoteId;
                    // read the message into the buffer
                    if (SteamNetworking.ReadP2PPacket(buffer, p2PreceiveMessagesCount, out bytesRead, out remoteId))
                    {
                        // convert to string
                        char[] chars = new char[bytesRead / sizeof(char)];
                        Buffer.BlockCopy(buffer, 0, chars, 0, (int)p2PreceiveMessagesCount);
                        string message = new string(chars, 0, chars.Length);
                        Console.WriteLine("Received a P2P message: " + message);
                        ChatingRoomManager.Inst.PraseReceivedChatingRoomMessage(message);
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendNormalChatingMessage();
            }




        }







        public void SendNormalChatingMessage()
        {
            if (chatingRoomPanel != null)
            {
                if (SteamUser.GetSteamID().m_SteamID.ToString() == "76561198091009835")
                {
                    if (chatingRoomPanel.currentHexColor == ColorUtility.ToHtmlStringRGB(Color.white))
                    {
                        chatingRoomPanel.currentHexColor = ColorUtility.ToHtmlStringRGB(Color.red);
                    }

                }

                string htmlString = "<color=#" + chatingRoomPanel.currentHexColor + ">" + ChatingRoomMainPanel.inputText.text + "</color>";

                SendChatMessage("", "普通消息", htmlString);
                ChatingRoomMainPanel.inputText.text = "";
            }
        }


        public static void EndAnimationPostFix()
        {
            Console.WriteLine("检测场景淡入:" + yourNowScene);
            PopChatingRoomUI();
        }

        [HarmonyPostfix, HarmonyPatch(declaringType: typeof(RoundManager), methodName: "gameStart")]
        public static void RoundManager_gameStart_Prefix(ref RoundManager __instance)
        {
            if (ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator != null)
            {
                ChatingRoomManager.onlinePlayersPanel.StopAllCoroutines();
                ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = null;
            }

            foreach (Transform child in onlinePlayersPanel.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(RoundManager), methodName: "OnDestroy")]
        public static bool RoundManager_OnDestroy_Prefix(ref RoundManager __instance)
        {
            if (ChatingRoomManager.onlinePlayersPanel != null && ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator != null)
            {
                ChatingRoomManager.onlinePlayersPanel.StopAllCoroutines();
                ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = null;
            }
            ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = ChatingRoomManager.Inst.RefreshMemberListEnumerator();
            onlinePlayersPanel.StartCoroutine(ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator);
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(Tools), methodName: "loadMapScenes", argumentTypes: new Type[] { typeof(string), typeof(bool) })]
        public static bool Tools_loadMapScenes_Prefix_Completed(ref Tools __instance, string name, bool LastSceneIsValue)
        {
            yourNowScene = name;
            PopChatingRoomUI();
            ChatingRoomManager.SetLobbyMemberData("nowScene", yourNowScene);
            return true;
        }


        public static void PopChatingRoomUI()
        {
            if (CheckCanPopChatingRoomUI(yourNowScene))
            {

                //is聊天框弹出时置顶 = false;
                if (chatingRoomEnterButton == null)
                {
                    GameObject chatingRoomEnterButtonGO = UnityEngine.Object.Instantiate<GameObject>(chatingRoomEnterButtonPrefab, UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
                    chatingRoomEnterButton = chatingRoomEnterButtonGO.GetComponent<Button>();
                    chatingRoomEnterButton.transform.localPosition = new Vector3(-685, 225);
                    chatingRoomEnterButton.transform.localScale = Vector3.one;
                    chatingRoomEnterButton.name = "ChatingRoomEnterButton";
                    chatingRoomEnterButton.onClick.AddListener(() => PopChatingRoomMainPanel(true));
                    PopChatingRoomMainPanel(true);
                }
                Console.WriteLine("进入了可聊区域");

            }
            else
            {
                // is聊天框弹出时置顶 = false;

                if (chatingRoomEnterButton != null)
                {
                    Destroy(chatingRoomEnterButton.gameObject);
                    chatingRoomEnterButton = null;
                }
                if (chatingRoomPanel != null)
                {

                    chatingRoomPanel.Close();
                }
            }
        }





        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(Tools), methodName: "loadMapScenes", argumentTypes: new Type[] { typeof(string), typeof(bool) })]
        public static bool Tools_loadMapScenes_Prefix_Partial(ref Tools __instance, string name, bool LastSceneIsValue)
        {
            yourNowScene = name;
            ChatingRoomManager.SetLobbyMemberData("nowScene", yourNowScene);
            foreach (Transform child in onlinePlayersPanel.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
            return true;
        }



        //public static void 特殊mod场景进入()
        //{
        //    is聊天框弹出时置顶 = true;
        //    if (chatingRoomEnterButton == null)
        //    {
        //        GameObject chatingRoomEnterButtonGO = UnityEngine.Object.Instantiate<GameObject>(chatingRoomEnterButtonPrefab, UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
        //        chatingRoomEnterButton = chatingRoomEnterButtonGO.GetComponent<Button>();
        //        chatingRoomEnterButton.transform.localPosition = new Vector3(-685, 225);
        //        chatingRoomEnterButton.transform.localScale = Vector3.one;
        //        chatingRoomEnterButton.name = "ChatingRoomEnterButton";
        //        chatingRoomEnterButton.onClick.AddListener(() => 搜索已存在的Lobby(lobbyName));
        //    }
        //    Console.WriteLine("特殊MOD进入聊天室");
        //    lobbyName = lobbyNamePre;
        //    搜索已存在的Lobby(lobbyName);
        //}



        //public static void 特殊mod场景退出()
        //{
        //    Console.WriteLine("特殊MOD退出聊天室");
        //    is聊天框弹出时置顶 = false;
        //    //删除聊天室界面();
        //    if (chatingRoomEnterButton != null)
        //    {
        //        Destroy(chatingRoomEnterButton.gameObject);
        //        chatingRoomEnterButton = null;
        //    }
        //}



        public static void CreateGroup()
        {
            lobbyName = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID()) + "(" + SteamUser.GetSteamID() + ")的队伍";
            SeachingExistingChatingRoom(lobbyName);
        }






        public static void SearchPlayingSameGameFrirends()
        {
            CSteamID steamIDFriend;
            int nFriends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll);

            Dictionary<CSteamID, string> friendList = new Dictionary<CSteamID, string>();

            for (int i = 0; i < nFriends; i++)
            {
                steamIDFriend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll);

                // 检查好友是否正在玩与你相同的游戏
                FriendGameInfo_t friendGameInfo;
                if (SteamFriends.GetFriendGamePlayed(steamIDFriend, out friendGameInfo))
                {
                    if (friendGameInfo.m_gameID.AppID() == SteamUtils.GetAppID())
                    {
                        // 好友正在玩与你相同的游戏，将其Steam ID添加到列表中
                        Console.WriteLine("Friend " + SteamFriends.GetFriendPersonaName(steamIDFriend) + " is playing the same game!");
                        string friendName = SteamFriends.GetFriendPersonaName(steamIDFriend);
                        friendList.Add(steamIDFriend, friendName);
                    }
                }
            }



            foreach (Transform child in GroupMainPanel.friendsScrollRect.content)
            {
                Destroy(child.gameObject);
            }

            foreach (KeyValuePair<CSteamID, string> keyValuePair in friendList)
            {
                AddFriendToPanel(keyValuePair, GroupMainPanel.friendsScrollRect.content, () => { }, () => { });
            }

        }



        public static bool CheckCanPopChatingRoomUI(string inputString)
        {
            if (inputString == "S113" || inputString == "S123" || inputString == "S133" || inputString == "S143" || inputString == "S153" || inputString == "S70" || inputString == "S1236" || inputString == "S1312" || inputString == "S1327" || inputString == "S1305" || inputString == "S1332" || inputString == "S1344")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //public static bool 检测特殊mod场景进入(string inputString)
        //{
        //    if (inputString == "灵界聊天室进入" || inputString == "虚鹏界进入")
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static bool 检测特殊mod场景退出(string inputString)
        //{
        //    if (inputString == "灵界聊天室退出" || inputString == "虚鹏界退出")
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(YSNewSaveSystem), methodName: "LoadSave")]
        public static bool YSNewSaveSystem_LoadSave_Prefix()
        {
            Console.WriteLine("开始读档了!");

            DestoryChatingRoomPanel();

            return true;
        }


        [HarmonyPostfix, HarmonyPatch(declaringType: typeof(YSNewSaveSystem), methodName: "LoadSave")]

        public static void YSNewSaveSystem_LoadSave_Postfix()
        {

            Console.WriteLine("游戏载入完成，搜索聊天室");
            if (onlinePlayersPanel == null)
            {
                onlinePlayersPanel = new GameObject("OnlinePlayersPanel").AddComponent<OnlinePlayersPanel>();
                onlinePlayersPanel.gameObject.AddComponent<Canvas>();
                onlinePlayersPanel.gameObject.AddComponent<CanvasScaler>();
                onlinePlayersPanel.gameObject.AddComponent<GraphicRaycaster>();
                onlinePlayersPanel.transform.parent = null;
                DontDestroyOnLoad(onlinePlayersPanel.gameObject);
            }
            //lobbyName = lobbyNamePre + "_" + name;
            lobbyName = lobbyNamePre;
            SeachingExistingChatingRoom(lobbyName);
            if (ChatingRoomManager.onlinePlayersPanel != null && ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator != null)
            {
                ChatingRoomManager.onlinePlayersPanel.StopAllCoroutines();
                ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = null;
            }
            ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = ChatingRoomManager.Inst.RefreshMemberListEnumerator();
            ChatingRoomManager.onlinePlayersPanel.StartCoroutine(ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator);


        }



        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(NewMainUIManager), methodName: "Awake")]
        public static bool NewMainUIManager_Awake_Prefix(ref NewMainUIManager __instance)
        {
            //删除聊天室界面();
            if (chatingRoomEnterButton != null)
            {
                Destroy(chatingRoomEnterButton.gameObject);
                chatingRoomEnterButton = null;
            }
            //if (ChatingRoomManager.onlinePlayersPanel != null && ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator != null)
            //{
            //    ChatingRoomManager.onlinePlayersPanel.StopAllCoroutines();
            //    ChatingRoomManager.onlinePlayersPanel.RefreshEnumerator = null;
            //}
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NextScene), "Start")]
        public static bool NextScene_Start_Prefix(ref NextScene __instance)
        {

            //删除聊天室界面();
            if (chatingRoomEnterButton != null)
            {
                Destroy(chatingRoomEnterButton.gameObject);
                chatingRoomEnterButton = null;
            }



            return true;
        }

        public static void SendP2PMessage(CSteamID receiver, string commandID, string commandContent)
        {
            string senderID = SteamUser.GetSteamID().m_SteamID.ToString();

            sendMessage = new CustomMessage();
            sendMessage.senderID = senderID;
            sendMessage.receiveID = receiver.m_SteamID.ToString();
            sendMessage.commandID = commandID;
            sendMessage.commandContent = commandContent;

            string inputString = JsonMapper.ToJson(sendMessage);

            //byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(inputString);

            // allocate new bytes array and copy string characters as bytes
            byte[] inputBytes = new byte[inputString.Length * sizeof(char)];
            System.Buffer.BlockCopy(inputString.ToCharArray(), 0, inputBytes, 0, inputBytes.Length);

            SteamNetworking.SendP2PPacket(receiver, inputBytes, (uint)inputBytes.Length, EP2PSend.k_EP2PSendReliable);
        }

        public static void SendChatMessage(string target, string commandID, string commandContent)
        {
            string senderID = SteamUser.GetSteamID().m_SteamID.ToString();

            sendMessage = new CustomMessage();
            sendMessage.senderID = senderID;
            sendMessage.receiveID = target;
            sendMessage.commandID = commandID;
            sendMessage.commandContent = commandContent;

            string inputString = JsonMapper.ToJson(sendMessage);

            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(inputString);

            SteamMatchmaking.SendLobbyChatMsg(lobbyID, inputBytes, inputBytes.Length);
        }


        public void CreateLobby()
        {
            if (SteamManager.Initialized)
            {
                // 创建Lobby
                SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 250);

            }
        }

        private void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            CSteamID clientId = request.m_steamIDRemote;
            //if (ExpectingClient(clientId))
            //{
            SteamNetworking.AcceptP2PSessionWithUser(clientId);
            //}
            //else
            //{
            //    Debug.LogWarning("Unexpected session request from " + clientId);
            //}
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {

            if (callback.m_eResult == EResult.k_EResultOK)
            {
                Console.WriteLine("Lobby created,LobbyName:" + lobbyName + " Lobby ID: " + callback.m_ulSteamIDLobby);
                lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
                SteamMatchmaking.SetLobbyData(lobbyID, "name", lobbyName); ;

            }
            else
            {
                Debug.LogError("Failed to create lobby: " + callback.m_eResult);
            }

        }

        public void JoinLobby(CSteamID lobbyId)
        {
            // 发送加入Lobby请求
            SteamMatchmaking.JoinLobby(lobbyId);
            //SteamFriends.ActivateGameOverlay("Friends");
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            EChatRoomEnterResponse response = (EChatRoomEnterResponse)Enum.ToObject(typeof(EChatRoomEnterResponse), callback.m_EChatRoomEnterResponse);

            if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                Console.WriteLine("Entered lobby: " + callback.m_ulSteamIDLobby);
                lobbyID = new CSteamID(callback.m_ulSteamIDLobby);


                if (lobbyChatMsgCallback == null)
                {
                    lobbyChatMsgCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);

                    KBEngine.Avatar player = PlayerEx.Player;
                    ChatingRoomManager.SetLobbyMemberData("xiuXianMing", player.name);
                    ChatingRoomManager.SetLobbyMemberData("menPai", Tools.getStr("menpai" + player.menPai));
                }

            }
            else
            {
                Debug.LogError("Failed to enter lobby: " + callback.m_EChatRoomEnterResponse);
            }

        }


        public static void SeachingExistingChatingRoom(string lobbyName)
        {
            string yourSteamName = SteamFriends.GetFriendPersonaName(SteamUser.GetSteamID());
            KBEngine.Avatar player = PlayerEx.Player;
            //Console.WriteLine("玩家名字:" + player.name);
            if (SensitiveWordsFilter.HasSensitiveWords(player.name) || SensitiveWordsFilter.HasSensitiveWords(yourSteamName))
            {
                Console.WriteLine("Steam名或修仙名含有敏感词，禁止进入聊天室");
                UIPopTip.Inst.Pop("Steam名或修仙名含有敏感词，禁止进入聊天室", PopTipIconType.叹号);
                return;
            }

            SteamMatchmaking.AddRequestLobbyListStringFilter("name", lobbyName, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(10);
            SteamMatchmaking.RequestLobbyList();
        }


        private class PlayerCountComparer : IComparer<Lobby>
        {
            public int Compare(Lobby x, Lobby y)
            {
                if (x.playerCount < y.playerCount)
                {
                    return 1;
                }
                else if (x.playerCount > y.playerCount)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }



        private void OnLobbyMatchList(LobbyMatchList_t pCallback)
        {
            lobbies.Clear();

            // 显示搜索到的Lobby数量
            Console.WriteLine("搜索到 " + pCallback.m_nLobbiesMatching + " 个Lobby");

            // 遍历所有搜索到的Lobby
            for (int i = 0; i < pCallback.m_nLobbiesMatching; i++)
            {
                // 获取Lobby信息
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "name");
                string gameName = SteamMatchmaking.GetLobbyData(lobbyId, "game");
                int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
                int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

                // 将Lobby添加到列表中
                lobbies.Add(new Lobby { Id = lobbyId, Name = lobbyName, playerCount = playerCount, maxPlayers = maxPlayers, gameName = gameName });

            }

            if (lobbies.Count > 0)
            {
                lobbies.Sort(new PlayerCountComparer());

                foreach (Lobby lobby in lobbies)
                {
                    Console.WriteLine("搜到Lobby:" + lobby.Name + " Id=" + lobby.Id + " 人数" + lobby.playerCount + "/" + lobby.maxPlayers);
                }

                if (lobbies[0].Name == lobbyName)
                {
                    if (lobbies[0].playerCount < lobbies[0].maxPlayers)
                    {
                        Console.WriteLine("Lobby未满，加入");
                        JoinLobby(lobbies[0].Id);
                        //if (lobbyName == lobbyNamePre)
                        //{
                        //    if (chatingRoomPanel == null)
                        //    {
                        //        PopChatingRoomMainPanel(is聊天框弹出时置顶);
                        //    }
                        //}
                        //else
                        //{

                        //}

                    }
                    else
                    {
                        Console.WriteLine("Lobby已满，请稍后尝试");
                    }
                }
            }
            else
            {
                Console.WriteLine("没有搜索到存在的Lobby,创建Lobby");
                CreateLobby();
            }

            //if (chatingRoomPanel == null)
            //{
            //    PopChatingRoomMainPanel(is聊天框弹出时置顶);
            //}
        }

        private void OnLobbyChatMessage(LobbyChatMsg_t chatMsg)
        {
            if (chatMsg.m_ulSteamIDLobby == lobbyID.m_SteamID)
            {
                byte[] data = new byte[4096];
                CSteamID steamID;
                EChatEntryType chatEntryType;

                int dataSize = SteamMatchmaking.GetLobbyChatEntry(lobbyID, (int)chatMsg.m_iChatID, out steamID, data, data.Length, out chatEntryType);
                if (dataSize > 0)
                {
                    //string userName = SteamFriends.GetFriendPersonaName(steamID);
                    string message = System.Text.Encoding.UTF8.GetString(data, 0, dataSize);
                    PraseReceivedChatingRoomMessage(message);
                    StartCoroutine(AutoScrollChatingBar());
                }

            }
        }

        public IEnumerator AutoScrollChatingBar()
        {
            yield return null;
            if (chatingRoomPanel != null)
            {
                if (Input.GetMouseButton(0))
                {

                }
                else if (chatingRoomPanel.chatingScrollRect.verticalScrollbar.value > 0.9)
                {

                    chatingRoomPanel.chatingScrollRect.verticalNormalizedPosition = 0;
                }
                else if (chatingRoomPanel.chatingScrollRect.verticalScrollbar.value > 0.2f && chatingRoomPanel.chatingScrollRect.verticalScrollbar.value <= 0.9)
                {

                }
                else if (chatingRoomPanel.chatingScrollRect.verticalScrollbar.value > 0f && chatingRoomPanel.chatingScrollRect.verticalScrollbar.value <= 0.2f)
                {


                    chatingRoomPanel.chatingScrollRect.verticalNormalizedPosition = 0;
                }
            }


        }



        public void PraseReceivedChatingRoomMessage(string message)
        {
            try
            {
                CustomMessage messgeJsonObj = JsonMapper.ToObject<CustomMessage>(message);

                switch (messgeJsonObj.commandID)
                {
                    case "普通消息":
                        {
                            ulong otherID = ulong.Parse(messgeJsonObj.senderID);
                            string memberName = SteamMatchmaking.GetLobbyMemberData(lobbyID, (CSteamID)otherID, "xiuXianMing");
                            if (memberName == null || memberName == "")
                            {
                                memberName = SteamFriends.GetFriendPersonaName((CSteamID)otherID);
                            }


                            string commandContent = memberName + "说:" + messgeJsonObj.commandContent;
                            Console.WriteLine(commandContent);


                            while (lastMessageList.Count > lastMessageHoldCount)
                            {
                                lastMessageList.RemoveAt(0);
                            }

                            lastMessageList.Add(commandContent);


                            Console.WriteLine("当前聊天记录数:" + lastMessageList.Count);


                            Image chatingBar = Instantiate(ChatingRoomManager.chatingBarPrefab, ChatingRoomMainPanel.chatingContent).GetComponent<Image>();
                            Text chatingText = chatingBar.GetComponentInChildren<Text>();

                            if (ChatingRoomMainPanel.lastChatingColor == null)
                            {
                                ChatingRoomMainPanel.lastChatingColor = greenChatringBarColor;
                            }

                            if (ChatingRoomMainPanel.lastChatingColor == Color.white)
                            {
                                chatingBar.color = greenChatringBarColor;
                            }
                            else
                            {
                                chatingBar.color = Color.white;
                            }
                            ChatingRoomMainPanel.lastChatingColor = chatingBar.color;
                            chatingText.text = SensitiveWordsFilter.Filter(commandContent) + "\r\n";

                        }
                        break;

                    case "请求摆摊数据":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "请求摆摊数据";
                            //Console.WriteLine(commandContent);
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {


                                if (chatingRoomPanel.yourWTSItemItemCell.sprite.sprite == null || ChatingRoomMainPanel.isPublishing == false)
                                {
                                    Console.WriteLine("接收到了摆摊数据请求，拒绝回复摆摊信息");

                                    string sendString = JsonMapper.ToJson(chatingRoomPanel.yourWTSItemItemCell.SetPlayerData());

                                    SendChatMessage(messgeJsonObj.senderID.ToString(), "拒绝回复摆摊信息", sendString);
                                }
                                else
                                {
                                    Console.WriteLine("接收到了摆摊数据请求，回复摆摊信息");
                                    SendChatMessage(messgeJsonObj.senderID.ToString(), "回复摆摊信息", chatingRoomPanel.yourWTSItemItemCell.playerDataString);
                                }

                            }
                        }
                        break;
                    case "野外请求玩家数据":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "野外请求玩家数据";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                ChatingRoomManager.CustomPlayerDataClass customPlayerData = ChatingRoomManager.Inst.SetPlayerData();
                                customPlayerData.itemData = "";
                                string playerDataString = JsonMapper.ToJson(customPlayerData);
                                SendChatMessage(messgeJsonObj.senderID.ToString(), "回复野外玩家数据", playerDataString);
                            }
                        }
                        break;
                    case "回复摆摊信息":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "回复了摆摊数据";
                            //Console.WriteLine(commandContent);
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                Console.WriteLine("接收到了摆摊回复:" + messgeJsonObj.commandContent);

                                CustomPlayerDataClass customPlayerData = JsonMapper.ToObject<CustomPlayerDataClass>(messgeJsonObj.commandContent);

                                JSONObject jsonObj = JSONObject.Create(customPlayerData.itemData, -2, false, false);

                                KBEngine.ITEM_INFO item_Info = KBEngine.ITEM_INFO.FromJSONObject(jsonObj);

                                BaseItem baseItem = BaseItem.Create(item_Info.itemId, (int)item_Info.itemCount, item_Info.uuid, item_Info.Seid);

                                ChatingRoomManager.chatingRoomPanel.otherPlayerItemCell.SetPlayerDataWithItem(baseItem, int.Parse(customPlayerData.customPrice), jsonObj.ToString(false));

                                Text priceText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "Price", true).GetComponent<Text>();

                                Text stateText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "State", true).GetComponent<Text>();


                                KBEngine.Avatar player = PlayerEx.Player;
                                int otherPlayerShenShi = int.Parse(customPlayerData.shenShi);
                                if (otherPlayerShenShi > player.shengShi)
                                {
                                    UIPopTip.Inst.Pop("对方是前辈高人，你无法探查他的状态", PopTipIconType.叹号);
                                    stateText.text = customPlayerData.sex + " " + "XXXX" + " " + "XX" + " " + "XXXX";
                                }
                                else
                                {
                                    UIPopTip.Inst.Pop("对方神识屏蔽不了你，他的状态你一目了然", PopTipIconType.叹号);
                                    stateText.text = customPlayerData.sex + " " + customPlayerData.menpai + " " + customPlayerData.age + " " + customPlayerData.jingJie;
                                }
                                priceText.text = customPlayerData.customPrice;

                                ChatingRoomMainPanel.lunDaoInviteButton.onClick.RemoveAllListeners();
                                ChatingRoomMainPanel.lunDaoInviteButton.onClick.AddListener(() => InviteAndSelectLundao(targetID));
                                ChatingRoomMainPanel.addFriendButton.onClick.RemoveAllListeners();
                                ChatingRoomMainPanel.addFriendButton.onClick.AddListener(() => AddSteamFirend(targetID));

                                ShowPlayerFace(customPlayerData, ChatingRoomMainPanel.faceObj.transform);
                            }
                        }
                        break;
                    case "回复野外玩家数据":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "回复了摆摊数据";
                            //Console.WriteLine(commandContent);
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                Console.WriteLine("接收到了野外玩家数据回复:" + messgeJsonObj.commandContent);

                                CustomPlayerDataClass customPlayerData = JsonMapper.ToObject<CustomPlayerDataClass>(messgeJsonObj.commandContent);
                                //JSONObject jsonObj = JSONObject.Create(customPlayerData.itemData, -2, false, false);

                                //KBEngine.ITEM_INFO item_Info = KBEngine.ITEM_INFO.FromJSONObject(jsonObj);

                                //BaseItem baseItem = BaseItem.Create(item_Info.itemId, (int)item_Info.itemCount, item_Info.uuid, item_Info.Seid);

                                //ChatingRoomManager.chatingRoomPanel.otherPlayerItemCell.SetPlayerDataWithItem(baseItem, int.Parse(customPlayerData.customPrice), jsonObj.ToString(false));

                                //Text priceText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "Price", true).GetComponent<Text>();

                                //Text stateText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "State", true).GetComponent<Text>();


                                KBEngine.Avatar player = PlayerEx.Player;
                                int otherPlayerShenShi = int.Parse(customPlayerData.shenShi);
                                if (otherPlayerShenShi > player.shengShi)
                                {
                                    UIPopTip.Inst.Pop("对方是前辈高人，你无法探查他的状态", PopTipIconType.叹号);
                                    PlayerDataPanel.stateText.text = customPlayerData.sex + " " + "XXXX" + " " + "XX" + " " + "XXXX";
                                }
                                else
                                {
                                    UIPopTip.Inst.Pop("对方神识屏蔽不了你，他的状态你一目了然", PopTipIconType.叹号);
                                    PlayerDataPanel.playerName.text = customPlayerData.xiuxianName;
                                    PlayerDataPanel.stateText.text = customPlayerData.sex + " " + customPlayerData.menpai + " " + customPlayerData.age + " " + customPlayerData.jingJie;
                                }
                                //priceText.text = customPlayerData.customPrice;

                                //PlayerDataPanel.lunDaoInviteButton.onClick.RemoveAllListeners();
                                //PlayerDataPanel.lunDaoInviteButton.onClick.AddListener(() => InviteAndSelectLundao(targetID));
                                //PlayerDataPanel.addFriendButton.onClick.RemoveAllListeners();
                                //PlayerDataPanel.addFriendButton.onClick.AddListener(() => AddSteamFirend(targetID));

                                ShowPlayerFace(customPlayerData, PlayerDataPanel.faceObj.transform);
                                ChatingRoomManager.playerDataPanel.gameObject.SetActive(true);
                            }
                        }
                        break;
                    case "拒绝回复摆摊信息":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "拒绝向" + receiveName + "回复摆摊数据";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                ulong otherID = ulong.Parse(messgeJsonObj.senderID);
                                string memberName = SteamFriends.GetFriendPersonaName((CSteamID)otherID);

                                Console.WriteLine(memberName + ":没有摆摊");
                                UIPopTip.Inst.Pop(memberName + ":没有摆摊", PopTipIconType.叹号);

                                CustomPlayerDataClass customPlayerData = JsonMapper.ToObject<CustomPlayerDataClass>(messgeJsonObj.commandContent);
                                Text priceText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "Price", true).GetComponent<Text>();
                                Text stateText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "State", true).GetComponent<Text>();

                                KBEngine.Avatar player = PlayerEx.Player;
                                int otherPlayerShenShi = int.Parse(customPlayerData.shenShi);
                                if (otherPlayerShenShi > player.shengShi)
                                {
                                    UIPopTip.Inst.Pop("对方是前辈高人，你无法探查他的状态", PopTipIconType.叹号);
                                    stateText.text = customPlayerData.sex + " " + "XXXX" + " " + "XX" + " " + "XXXX";
                                }
                                else
                                {
                                    UIPopTip.Inst.Pop("对方神识屏蔽不了你，他的状态你一目了然", PopTipIconType.叹号);
                                    stateText.text = customPlayerData.sex + " " + customPlayerData.menpai + " " + customPlayerData.age + " " + customPlayerData.jingJie;
                                }

                                chatingRoomPanel.otherPlayerItemCell.itemName.text = "";
                                chatingRoomPanel.otherPlayerItemCell.price = 0;
                                chatingRoomPanel.otherPlayerItemCell.count.text = "";
                                chatingRoomPanel.otherPlayerItemCell.qsprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                chatingRoomPanel.otherPlayerItemCell.sprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                chatingRoomPanel.otherPlayerItemCell.baseItem = null;
                                priceText.text = "0";

                                ChatingRoomMainPanel.lunDaoInviteButton.onClick.RemoveAllListeners();
                                ChatingRoomMainPanel.lunDaoInviteButton.onClick.AddListener(() => InviteAndSelectLundao(targetID));
                                ChatingRoomMainPanel.addFriendButton.onClick.RemoveAllListeners();
                                ChatingRoomMainPanel.addFriendButton.onClick.AddListener(() => AddSteamFirend(targetID));

                                ShowPlayerFace(customPlayerData, ChatingRoomMainPanel.faceObj.transform);

                            }
                        }
                        break;

                    case "请求购买":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "请求购买物品";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                Console.WriteLine("接收到购买请求:" + messgeJsonObj.commandContent);
                                CustomPlayerDataClass customItemClass = JsonMapper.ToObject<CustomPlayerDataClass>(messgeJsonObj.commandContent);

                                JSONObject jsonObj = JSONObject.Create(customItemClass.itemData, -2, false, false);

                                KBEngine.ITEM_INFO item_Info = KBEngine.ITEM_INFO.FromJSONObject(jsonObj);

                                BaseItem baseItem = BaseItem.Create(item_Info.itemId, (int)item_Info.itemCount, item_Info.uuid, item_Info.Seid);

                                if (!ChatingRoomMainPanel.isPublishing)
                                {

                                    ChatingRoomManager.SendChatMessage(messgeJsonObj.senderID, "物品被抢", "");
                                }
                                else if (baseItem.Uid == chatingRoomPanel.yourWTSItemItemCell.baseItem.Uid || baseItem.Count == chatingRoomPanel.yourWTSItemItemCell.baseItem.Count)
                                {

                                    ChatingRoomManager.SendChatMessage(messgeJsonObj.senderID, "确认购买", chatingRoomPanel.yourWTSItemItemCell.playerDataString);

                                    CangKuManager.Inst.GetCangKu(ChatingRoomManager.jiaoYiCangKuId).RemoveItem(baseItem.Uid, baseItem.Count);

                                    KBEngine.Avatar player = PlayerEx.Player;

                                    ulong price = ulong.Parse(customItemClass.customPrice);
                                    ulong money = player.money;
                                    player.money += price;

                                    if (chatingRoomPanel != null)
                                    {
                                        chatingRoomPanel.yourWTSItemItemCell.itemName.text = "";
                                        chatingRoomPanel.yourWTSItemItemCell.price = 0;
                                        chatingRoomPanel.yourWTSItemItemCell.count.text = "";
                                        chatingRoomPanel.yourWTSItemItemCell.qsprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                        chatingRoomPanel.yourWTSItemItemCell.sprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                        chatingRoomPanel.yourWTSItemItemCell.baseItem = null;
                                        chatingRoomPanel.yourWTSItemItemCell.tips.text = "有玩家购买了你卖的物品";
                                        chatingRoomPanel.sellPriceInputField.text = "0";
                                    }

                                    UIPopTip.Inst.Pop("有玩家购买了你卖的物品", PopTipIconType.叹号);
                                    ChatingRoomManager.SetLobbyMemberData("isPublishing", "false");
                                    ChatingRoomMainPanel.isPublishing = false;
                                }
                                else
                                {

                                    ChatingRoomManager.SendChatMessage(messgeJsonObj.senderID, "对方的物品信息过期，试图买你曾经卖的物品", "");
                                }

                            }


                        }
                        break;

                    case "确认购买":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "确认购买物品";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                Console.WriteLine("接收到了确认购买:" + messgeJsonObj.commandContent);
                                KBEngine.Avatar player = PlayerEx.Player;
                                ulong price = (ulong)chatingRoomPanel.otherPlayerItemCell.price;
                                if (price > player.money)
                                {
                                    UIPopTip.Inst.Pop("你的灵石不够！", PopTipIconType.叹号);
                                }
                                else
                                {
                                    player.addItem(chatingRoomPanel.otherPlayerItemCell.baseItem.Id, chatingRoomPanel.otherPlayerItemCell.baseItem.Seid, chatingRoomPanel.otherPlayerItemCell.baseItem.Count);
                                    if (price > 0UL)
                                    {
                                        ulong money = player.money;
                                        player.money -= price;
                                    }

                                    chatingRoomPanel.otherPlayerItemCell.itemName.text = "";
                                    chatingRoomPanel.otherPlayerItemCell.price = 0;
                                    chatingRoomPanel.otherPlayerItemCell.count.text = "";
                                    chatingRoomPanel.otherPlayerItemCell.qsprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                    chatingRoomPanel.otherPlayerItemCell.sprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                    chatingRoomPanel.otherPlayerItemCell.baseItem = null;
                                    Text priceText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "Price", true).GetComponent<Text>();
                                    Text stateText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "State", true).GetComponent<Text>();
                                    priceText.text = "0";
                                    stateText.text = "";
                                    UIPopTip.Inst.Pop("交易完成", PopTipIconType.叹号);
                                }
                            }

                        }
                        break;

                    case "物品被抢":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "告知" + receiveName + "物品被抢";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                UIPopTip.Inst.Pop("物品被抢！", PopTipIconType.叹号);
                                chatingRoomPanel.otherPlayerItemCell.itemName.text = "";
                                chatingRoomPanel.otherPlayerItemCell.price = 0;
                                chatingRoomPanel.otherPlayerItemCell.count.text = "";
                                chatingRoomPanel.otherPlayerItemCell.qsprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                chatingRoomPanel.otherPlayerItemCell.sprite.sprite = ChatingRoomManager.chatingRoomPanel.emptySprite;
                                chatingRoomPanel.otherPlayerItemCell.baseItem = null;
                                Text priceText = ChatingRoomManager.Inst.GetChild<Text>(chatingRoomPanel.otherPlayerItemCell.gameObject, "Price", true).GetComponent<Text>();
                                priceText.text = "0";
                            }

                        }
                        break;

                    case "邀请论道":
                        {


                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "向" + receiveName + "发送论道邀请";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                if (messgeJsonObj.receiveID == messgeJsonObj.senderID)
                                {
                                    UIPopTip.Inst.Pop("你自己不能和自己论道", PopTipIconType.叹号);
                                    return;
                                }

                                if (lunDaoManagerOnline != null || dialogBox != null)
                                {
                                    UIPopTip.Inst.Pop("你已经在论道了，拒绝了" + senderName + "的论道邀请", PopTipIconType.叹号);
                                    return;
                                }

                                targetID = new CSteamID(ulong.Parse(messgeJsonObj.senderID));
                                LunDaoManagerOnline.otherPlayerOnlineDataClass = JsonMapper.ToObject<OtherPlayerOnlineDataClass>(messgeJsonObj.commandContent);
                                Console.WriteLine("接收到了论道邀请:" + messgeJsonObj.commandContent);
                                dialogBox = Instantiate<GameObject>(dialogBoxPrefab, chatingRoomPanel.transform);
                                dialogBox.transform.localPosition = Vector3.zero;
                                Text titleText = GetGameObjectStatic<Text>(dialogBox.gameObject, "TitleText", true).GetComponent<Text>();
                                Button acceptButton = GetGameObjectStatic<Button>(dialogBox.gameObject, "Accept", true).GetComponent<Button>();
                                Button refuseButton = GetGameObjectStatic<Button>(dialogBox.gameObject, "Refuse", true).GetComponent<Button>();
                                GameObject toggleGroup = GetGameObjectStatic<RectTransform>(dialogBox.gameObject, "ToggleGroup", true);
                                List<Toggle> toggleList = toggleGroup.GetComponentsInChildren<Toggle>().ToList();

                                foreach (string luntiName in LunDaoManagerOnline.otherPlayerOnlineDataClass.lunTiList)
                                {
                                    foreach (Toggle toggle in toggleList)
                                    {
                                        if (toggle.name == luntiName)
                                        {
                                            toggle.isOn = true;
                                        }
                                    }
                                }



                                acceptButton.onClick.AddListener(() => AccpetOtherLundaoInviting());
                                refuseButton.onClick.AddListener(() => RefuseOtherLundaoInviting());
                                titleText.text = senderName + "邀请与你一起论道，是否接受？可更改他的议题:";
                            }
                        }
                        break;
                    case "接受论道邀请":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "接受了" + receiveName + "的论道邀请";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                if (lunDaoManagerOnline != null || dialogBox != null)
                                {
                                    UIPopTip.Inst.Pop("你已经在论道了，无法和" + senderName + "进行论道", PopTipIconType.叹号);
                                    return;
                                }

                                LunDaoManagerOnline.otherPlayerOnlineDataClass = JsonMapper.ToObject<OtherPlayerOnlineDataClass>(messgeJsonObj.commandContent);
                                lunDaoManagerOnline = PopCustomLunDaoManager().AddComponent<LunDaoManagerOnline>();

                            }
                        }
                        break;

                    case "拒绝论道邀请":
                        {
                            string senderName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.senderID));
                            string receiveName = SteamFriends.GetFriendPersonaName((CSteamID)ulong.Parse(messgeJsonObj.receiveID));
                            string commandContent = senderName + "拒绝了" + receiveName + "的论道邀请";
                            if (messgeJsonObj.receiveID == SteamUser.GetSteamID().m_SteamID.ToString())
                            {
                                UIPopTip.Inst.Pop(senderName + "正忙，无法和你进行论道", PopTipIconType.叹号);
                            }
                        }
                        break;



                    default:
                        {

                        }
                        break;
                }


            }
            catch (Exception e)
            {
                UIPopTip.Inst.Pop("命令解析失败", PopTipIconType.叹号);
                Console.WriteLine("命令解析失败:" + e.ToString());
            }

        }

        public void ShowPlayerFace(CustomPlayerDataClass customPlayerData, Transform parent)
        {

            try
            {


                if (ChatingRoomMainPanel.otherPlayeSetRandomFace != null)
                {
                    Destroy(ChatingRoomMainPanel.otherPlayeSetRandomFace.gameObject);
                }
                JSONObject otherFaceJson = JSONObject.Create(customPlayerData.gameFaceData, -2, false, false);
                Console.WriteLine("otherFaceJson:" + otherFaceJson);

                GameObject otherSetRandomFaceParent = Resources.Load<GameObject>("Prefabs/SayDialog").GetComponentInChildren<PlayerSetRandomFace>().transform.parent.gameObject;
                otherSetRandomFaceParent.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                ChatingRoomMainPanel.otherPlayeSetRandomFace = Instantiate(otherSetRandomFaceParent, parent).GetComponentInChildren<PlayerSetRandomFace>();


                ChatingRoomManager.randomAvatar(ref ChatingRoomMainPanel.otherPlayeSetRandomFace, otherFaceJson);


                ChatingRoomMainPanel.otherPlayeSetRandomFace.transform.parent.localPosition = new Vector3(0, -300, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }




        private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby == lobbyID.m_SteamID)
            {

                // Get the lobby name by ID
                //string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "name");

                //// Get the number of players in the lobby
                //int playerCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

                //// Get the maximum number of players allowed in the lobby
                //int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);

                //Console.WriteLine("Lobby Name: " + lobbyName + "进入成功");



                //Display the lobby information
                //Console.WriteLine("Lobby Name: " + lobbyName);
                //Console.WriteLine("Player Count: " + playerCount);
                //Console.WriteLine("Max Players: " + maxPlayers);

                //for (int i = 0; i < SteamMatchmaking.GetLobbyDataCount(lobbyID); i++)
                //{
                //    string key, value;
                //    if (SteamMatchmaking.GetLobbyDataByIndex(lobbyID, i, out key, Steamworks.Constants.k_cubChatMetadataMax, out value, Steamworks.Constants.k_cubChatMetadataMax))
                //    {
                //        Console.WriteLine(key + " : " + value);
                //    }
                //}



            }

        }

        public static void ExitLobby()
        {

            Console.WriteLine("退出Lobby:" + lobbyID);
            SteamMatchmaking.LeaveLobby(lobbyID);
            if (lobbyChatMsgCallback != null)
            {
                lobbyChatMsgCallback.Dispose();
                lobbyChatMsgCallback = null;
            }
        }

        public Dictionary<CSteamID, string> GetLobbyMembers()
        {
            Dictionary<CSteamID, string> memberNames = new Dictionary<CSteamID, string>();
            int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            for (int i = 0; i < numMembers; i++)
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
                if (memberId.IsValid())
                {
                    string memberName = SteamFriends.GetFriendPersonaName(memberId);
                    memberNames.Add(memberId, memberName);
                }
            }

            return memberNames;
        }

        public IEnumerator RefreshMemberListEnumerator()
        {

            KBEngine.Avatar player = PlayerEx.Player;
            string selfName = player.name;
            string selfGender = PlayerEx.Player.Sex.ToString();
            string levelType = PlayerEx.Player.getLevelType().ToString();
            while (true)
            {
                if (yourNowScene == "AllMaps" || yourNowScene == "Sea1" || yourNowScene == "Sea2" || yourNowScene == "Sea3" || yourNowScene == "Sea4" || yourNowScene == "Sea5" || yourNowScene == "Sea6" || yourNowScene == "Sea7" || yourNowScene == "Sea8")
                {
                    GameObject mapPlayerGameObject = GetGameObjectStatic<MapPlayerController>(null, "MapPlayer(Clone)", false);
                    if (mapPlayerGameObject != null)
                    {
                        yourPostion = mapPlayerGameObject.transform.localPosition;
                        string yourPostionString = SetVec3ByString(yourPostion);
                        ChatingRoomManager.SetLobbyMemberData("yourPostion", yourPostionString);
                        ChatingRoomManager.SetLobbyMemberData("gender", selfGender);
                        ChatingRoomManager.SetLobbyMemberData("levelType", levelType);


                    }

                }



                members = GetLobbyMembers();

                activePlayerCount = 0;

                foreach (Transform child in onlinePlayersPanel.transform)
                {

                    GameObject.DestroyImmediate(child.gameObject);
                }


                foreach (KeyValuePair<CSteamID, string> keyValuePair in members)
                {

                    string isChating = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "isChating");
                    if (isChating == "true")
                    {
                        activePlayerCount++;
                    }

                    string playerNowScene = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "nowScene");
                    Vector3 playerPosition = Vector3.zero;
                    if (playerNowScene == "AllMaps" || playerNowScene == "Sea1" || playerNowScene == "Sea2" || playerNowScene == "Sea3" || playerNowScene == "Sea4" || playerNowScene == "Sea5" || playerNowScene == "Sea6" || playerNowScene == "Sea7" || playerNowScene == "Sea8")
                    {

                        string playerPositionString = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "yourPostion");
                        string xiuXianMing = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "xiuXianMing");
                        string playerGenderString = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "gender");
                        string playerGender;
                        string menPai = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "menPai");

                        if (playerGenderString == "1")
                        {
                            playerGender = "男";
                        }
                        else if (playerGenderString == "2")
                        {
                            playerGender = "女";
                        }
                        else
                        {
                            playerGender = "未知";
                        }
                        playerPosition = GetVec3ByString(playerPositionString);
                        Console.WriteLine(xiuXianMing + "(" + playerGender + ")的位置:" + playerNowScene + " " + playerPosition);


                        if (xiuXianMing != selfName)
                        {

                            if (playerNowScene == SceneEx.NowSceneName)
                            {

                                GameObject onlinePlayerIconPrefabGO;
                                onlinePlayerIconPrefabGO = GetGameObjectStatic<Transform>(onlinePlayersPanel.gameObject, xiuXianMing, false);

                                if (onlinePlayerIconPrefabGO == null)
                                {

                                    onlinePlayerIconPrefabGO = UnityEngine.Object.Instantiate<GameObject>(onlinePlayerIconPrefab, onlinePlayersPanel.transform);
                                    OnlinePlayerIconButton onlinePlayerIconButton = onlinePlayerIconPrefabGO.AddComponent<OnlinePlayerIconButton>();
                                    onlinePlayerIconButton.playerSteamID = keyValuePair.Key;
                                    onlinePlayerIconButton.playerName = xiuXianMing;
                                    onlinePlayerIconButton.playerGender = playerGender;
                                    onlinePlayerIconButton.menPai = menPai;

                                    SkeletonDataAsset data;

                                    var obj = new GameObject("骨骼");
                                    obj.transform.SetParent(onlinePlayerIconPrefabGO.transform);
                                    obj.transform.localPosition = new Vector3(0f, 0f, 0f);
                                    obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


                                    //if (SceneEx.NowSceneName.StartsWith("Sea"))
                                    //{
                                    //    var spine = obj.AddComponent<SkeletonAnimation>();
                                    //    data = MapPlayerController.Inst?.SeaShow?.ChuanGuGe;
                                    //    spine.initialSkinName = "boat_" + 1;
                                    //    //spine.startingAnimation = "C_idle";
                                    //    spine.AnimationName = "C_idle";
                                    //    spine.loop = true;
                                    //}
                                    //else
                                    //{
                                    var spine = obj.AddComponent<SkeletonGraphic>();
                                    data = Resources.Load<SkeletonDataAsset>("Spine/MapPlayer/MapPlayerYuJian/MapPlayerYuJian_SkeletonData");
                                    spine.initialSkinName = (playerGender == "女") ? "女" : "男";
                                    spine.startingAnimation = "Idle";
                                    spine.skeletonDataAsset = data;
                                    spine.startingLoop = true;
                                    spine.Initialize(true);
                                    //}

                                }
                                onlinePlayerIconPrefabGO.transform.localScale = new Vector3(0.01f, 0.01f, 0f);



                                onlinePlayerIconPrefabGO.transform.position = playerPosition;
                                onlinePlayerIconPrefabGO.name = xiuXianMing;
                                Text onlinePlayerText = onlinePlayerIconPrefabGO.GetComponentInChildren<Text>();
                                onlinePlayerText.text = xiuXianMing;

                                SetTextMenpaiColor(onlinePlayerText, menPai);

                            }

                        }

                    }

                }

                if (chatingRoomEnterButton != null)
                {

                    string htmlString = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.green) + ">" + activePlayerCount + "</color>" + "/" + "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">" + members.Count + "</color>";
                    GameObject memberCountGo = GetGameObjectStatic<Text>(chatingRoomEnterButton.gameObject, "MemberCount", true);
                    memberCountGo.GetComponentInChildren<Text>().text = htmlString;

                }



                if (chatingRoomPanel != null && chatingRoomPanel.isActiveAndEnabled)
                {

                    SteamMatchmaking.SetLobbyMemberData(lobbyID, "isChating", "true");

                    if (ChatingRoomMainPanel.memberScrollRectContent != null)
                    {
                        foreach (Transform child in ChatingRoomMainPanel.memberScrollRectContent)
                        {
                            Destroy(child.gameObject);
                        }

                        chatingText = ChatingRoomPanelBase.chatingContent.GetComponentInChildren<Text>();

                        foreach (KeyValuePair<CSteamID, string> keyValuePair in members)
                        {
                            AddMemberToPanel(keyValuePair, ChatingRoomMainPanel.memberScrollRectContent, () => SelectSomeOne(keyValuePair));
                        }
                    }



                }
                else
                {
                    SteamMatchmaking.SetLobbyMemberData(lobbyID, "isChating", "false");
                }

                yield return new WaitForSeconds(10f);
            }

        }

        public static void AddMemberToPanel(KeyValuePair<CSteamID, string> keyValuePair, RectTransform parent, UnityAction clickCall)
        {
            GameObject userbarGo = UnityEngine.Object.Instantiate<GameObject>(userbarlPrefab, parent);
            userbarGo.name = keyValuePair.Value;
            List<Text> texts = userbarGo.GetComponentsInChildren<Text>().ToList();
            Image IsSelling = GetGameObjectStatic<Image>(userbarGo, "IsSelling", true).GetComponent<Image>();

            string xiuXianMing = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "xiuXianMing");

            string menPai = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "menPai");
            string isChating = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "isChating");



            //Console.WriteLine(xiuXianMing + "的门派:" + menPai);
            foreach (Text t in texts)
            {
                if (t.name == "XiuXianName")
                {
                    //t.text = keyValuePair.Key.ToString();
                    t.text = SensitiveWordsFilter.Filter(xiuXianMing);

                    SetTextMenpaiColor(t, menPai);
                }

                if (t.name == "Name")
                {
                    t.text = SensitiveWordsFilter.Filter(keyValuePair.Value);
                    if (isChating == "true")
                    {

                        t.color = Color.white;
                    }
                    else
                    {
                        t.color = Color.gray;
                    }
                }

            }


            string result = SteamMatchmaking.GetLobbyMemberData(lobbyID, keyValuePair.Key, "isPublishing");

            if (result == "true")
            {
                IsSelling.gameObject.SetActive(true);
            }
            else
            {
                IsSelling.gameObject.SetActive(false);
            }

            //Console.WriteLine(keyValuePair.Value + "IsSelling:" + result);
            Button button = userbarGo.GetComponent<Button>();

            button.onClick.AddListener(clickCall);
        }


        public static void AddFriendToPanel(KeyValuePair<CSteamID, string> keyValuePair, RectTransform parent, UnityAction inviteClickCall, UnityAction joinClickCall)
        {
            GameObject friendbarGO = UnityEngine.Object.Instantiate<GameObject>(friendbarPrefab, parent);
            friendbarGO.name = keyValuePair.Value;
            Text name = GetGameObjectStatic<Text>(friendbarGO, "Name", true).GetComponent<Text>();
            name.text = keyValuePair.Value;
            Button inviteButton = GetGameObjectStatic<Button>(friendbarGO, "Invite", true).GetComponent<Button>();
            inviteButton.onClick.AddListener(inviteClickCall);
            Button joinButton = GetGameObjectStatic<Button>(friendbarGO, "Join", true).GetComponent<Button>();
            joinButton.onClick.AddListener(joinClickCall);



        }


        public void AddSteamFirend(CSteamID targetID)
        {
            string targetName = SteamFriends.GetFriendPersonaName(targetID);
            Console.WriteLine("弹出向" + targetName + "发送好友邀请对话框");
            //SteamFriends.ActivateGameOverlayToUser("steamid", targetID);
            SteamFriends.ActivateGameOverlayToUser("friendadd", targetID);
        }



        public void SelectSomeOne(KeyValuePair<CSteamID, string> keyValuePair)
        {
            Console.WriteLine("选中了:" + keyValuePair.Value + ":" + keyValuePair.Key);
            targetID = keyValuePair.Key;

            SendChatMessage(targetID.m_SteamID.ToString(), "请求摆摊数据", "");

            //if (!SteamFriends.RequestUserInformation(targetID, false))
            //{
            chatingRoomPanel.otherAvatarImage.texture = null;
            SteamFriends.GetLargeFriendAvatar(targetID);

            ChatingRoomMainPanel.lunDaoInviteButton.interactable = true;
            //}
            //Console.WriteLine("imageID:" + imageID);
        }

        private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
        {
            // 将Steam头像转换为Unity Texture2D对象
            Console.WriteLine("图片回调触发,callback.m_iImage:" + callback.m_iImage);
            Texture2D avatarTexture = GetSteamImageAsTexture(callback.m_iImage);
            if (chatingRoomPanel != null)
            {
                chatingRoomPanel.otherAvatarImage.texture = avatarTexture;
            }
            PlayerDataPanel.otherAvatarImage.texture = avatarTexture;

        }


        private Texture2D GetSteamImageAsTexture(int imageID)
        {
            // 获取图像大小
            Texture2D texture = null;
            uint width = 0;
            uint height = 0;
            if (SteamUtils.GetImageSize(imageID, out width, out height))
            {
                byte[] imageBuffer = new byte[width * height * 4];
                SteamUtils.GetImageRGBA(imageID, imageBuffer, (int)(width * height * 4));

                // 创建纹理并返回
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(imageBuffer);
                texture.Apply();
            }
            return texture;

        }




        public void OnDestroy()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
            lobbyCreatedCallback.Dispose();
            lobbyEnteredCallback.Dispose();
            if (lobbyChatMsgCallback != null)
            {
                lobbyChatMsgCallback.Dispose();
                lobbyChatMsgCallback = null;
            }

            lobbyDataUpdateCallback.Dispose();
        }





        public static GameObject PopCustomLunDaoManager()
        {
            if (ChatingRoomManager.chatingRoomPanel != null)
            {
                GameObject gameObject = Instantiate<GameObject>(customLunDaoManagerPrefab, ChatingRoomManager.chatingRoomPanel.transform);
                gameObject.transform.localPosition = new Vector3(59.8f, 10f, 0);
                gameObject.transform.localScale = Vector3.one;
                gameObject.name = "CustomLunDaoManager";
                Console.WriteLine("弹出自定义论道面板");
                gameObject.GetComponent<RectTransform>().SetAsLastSibling();
                return gameObject;
            }
            else
            {
                return null;
            }
        }




        public static void PopChatingRoomMainPanel(bool setTop)
        {
            if (chatingRoomPanel == null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chatingRoomMainPanelPrefab, UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
                gameObject.transform.localPosition = new Vector3(0, 0, 0);
                gameObject.transform.localScale = Vector3.one;
                gameObject.name = "ChatingRoomMainPanel";
                chatingRoomPanel = gameObject.AddComponent<ChatingRoomMainPanel>();
                Console.WriteLine("弹出聊天室主菜单");

                if (setTop)
                {
                    chatingRoomPanel.GetComponent<RectTransform>().SetAsLastSibling();
                }
                else
                {
                    chatingRoomPanel.GetComponent<RectTransform>().SetAsFirstSibling();
                }

            }


        }


        public static void PopGroupMainPanel()
        {
            if (chatingRoomPanel == null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(groupMainPanelPrefab, UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform);
                gameObject.transform.localPosition = new Vector3(0, 0, 0);
                gameObject.transform.localScale = Vector3.one;
                gameObject.name = "GroupMainPanel";
                chatingRoomPanel = gameObject.AddComponent<GroupMainPanel>();
                Console.WriteLine("弹出组队主菜单");
                chatingRoomPanel.GetComponent<RectTransform>().SetAsFirstSibling();
            }


        }


        public IEnumerator LoadAssetAsync()
        {
            AssetBundleCreateRequest mumuAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(this.Path + "/chatingroomui");
            yield return mumuAssetBundleCreateRequest;
            AssetBundle mumuAssetBundle = mumuAssetBundleCreateRequest.assetBundle;
            if (mumuAssetBundle == null)
            {
                Debug.LogError("Failed to load AssetBundle:chatingroomui!");
                yield break;
            }

            chatingRoomEnterButtonPrefab = mumuAssetBundle.LoadAsset<GameObject>("ChatingRoomEnterButton");
            chatingRoomMainPanelPrefab = mumuAssetBundle.LoadAsset<GameObject>("ChatingRoomMainPanel");
            userbarlPrefab = mumuAssetBundle.LoadAsset<GameObject>("Userbar");
            dropdownPrefab = mumuAssetBundle.LoadAsset<GameObject>("Dropdown");
            chatingBarPrefab = mumuAssetBundle.LoadAsset<GameObject>("ChatingBar");
            customLunDaoManagerPrefab = mumuAssetBundle.LoadAsset<GameObject>("CustomLunDaoManager");
            dialogBoxPrefab = mumuAssetBundle.LoadAsset<GameObject>("DialogBox");
            startLunTiCellPrefab = mumuAssetBundle.LoadAsset<GameObject>("StartLunTiCell");
            wuDaoQiuPrefab = mumuAssetBundle.LoadAsset<GameObject>("WuDaoQiu");
            lunDaoQiuSlotPrefab = mumuAssetBundle.LoadAsset<GameObject>("LunDaoQiu");
            playerCardPrefab = mumuAssetBundle.LoadAsset<GameObject>("LunDaoPlayerCard");
            groupMainPanelPrefab = mumuAssetBundle.LoadAsset<GameObject>("GroupMainPanel");
            friendbarPrefab = mumuAssetBundle.LoadAsset<GameObject>("Friendbar");
            groupPanelEnterButtonPrefab = mumuAssetBundle.LoadAsset<GameObject>("GroupPanelEnterButton");
            onlinePlayerIconPrefab = mumuAssetBundle.LoadAsset<GameObject>("OnlinePlayer");
            playerDataPanelPrefab = mumuAssetBundle.LoadAsset<GameObject>("PlayerDataPanel");
            yield break;
        }

        public GameObject GetChild<T>(GameObject gameObject, string name, bool showError = true) where T : Component
        {
            foreach (T t in gameObject.GetComponentsInChildren<T>(true))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            if (showError)
            {
                Debug.LogError("对象" + gameObject.name + "不存在子对象" + name);
            }
            return null;
        }

        public static GameObject GetGameObjectStatic<T>(GameObject gameObject, string name, bool showError = true) where T : Component
        {
            List<T> gos = new List<T>();

            if (gameObject == null)
            {
                gos = FindObjectsOfType<T>().ToList();
            }
            else
            {
                gos = gameObject.GetComponentsInChildren<T>(true).ToList();
            }

            foreach (T t in gos)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            if (showError)
            {
                Debug.LogError("对象" + gameObject.name + "不存在子对象" + name);
            }
            return null;
        }

        public static void DestoryChatingRoomPanel()
        {
            ChatingRoomManager.ExitLobby();
            if (chatingRoomEnterButton != null)
            {
                Destroy(chatingRoomEnterButton.gameObject);
                chatingRoomEnterButton = null;
            }

            if (chatingRoomPanel != null)
            {

                chatingRoomPanel.Close();
            }

        }


        [HarmonyPostfix, HarmonyPatch(declaringType: typeof(ToolTipsMag), methodName: "PCSetPosition")]
        public static void ToolTipsMag_PCSetPosition_Postfix(ref ToolTipsMag __instance)
        {
            if (toolTipPositionfix)
            {

                float num = Input.mousePosition.x;
                float y = (float)Screen.height * 4f / 5f;
                num = (float)Screen.width / 2f;
                __instance.Panel.transform.position = NewUICanvas.Inst.Camera.ScreenToWorldPoint(new Vector2(num, y));
            }

        }

        [HarmonyPrefix, HarmonyPatch(declaringType: typeof(UIMapPanel), methodName: "ShowPanel")]
        public static bool UIMapPanel_ShowPanel_Prefix(ref UIMapPanel __instance)
        {
            Console.WriteLine("触发OpenMap");
            if (ChatingRoomManager.chatingRoomPanel == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static void SetLobbyMemberData(string key, string value)
        {
            SteamMatchmaking.SetLobbyMemberData(lobbyID, key, value);
        }

        public void InviteAndSelectLundao(CSteamID targetID)
        {
            if (dialogBox == null)
            {
                dialogBox = Instantiate<GameObject>(dialogBoxPrefab, chatingRoomPanel.transform);
                string targetName = SteamFriends.GetFriendPersonaName(targetID);
                dialogBox.transform.localPosition = Vector3.zero;
                Text titleText = GetGameObjectStatic<Text>(dialogBox.gameObject, "TitleText", true).GetComponent<Text>();
                Button acceptButton = GetGameObjectStatic<Button>(dialogBox.gameObject, "Accept", true).GetComponent<Button>();
                Button refuseButton = GetGameObjectStatic<Button>(dialogBox.gameObject, "Refuse", true).GetComponent<Button>();

                acceptButton.onClick.AddListener(() => InviteLundaoButtonHandle(targetID));
                refuseButton.onClick.AddListener(() => Destroy(dialogBox.gameObject));
                titleText.text = "你提议与" + targetName + "一起论道，请选择提议的论题:";

            }
            else
            {
                UIPopTip.Inst.Pop("你已经在论道了", PopTipIconType.叹号);
            }

        }


        public void InviteLundaoButtonHandle(CSteamID targetID)
        {

            string targetName = SteamFriends.GetFriendPersonaName(targetID);

            KBEngine.Avatar avatar = Tools.instance.getPlayer();
            myLunDaoOnlineData = new OtherPlayerOnlineDataClass();
            myLunDaoOnlineData.name = avatar.name;
            myLunDaoOnlineData.wuDaoJsonString = avatar.WuDaoJson.ToString();
            myLunDaoOnlineData.faceJsonString = jsonData.instance.AvatarRandomJsonData["1"].ToString();
            myLunDaoOnlineData.StatusId = avatar.LunDaoState;

            myLunDaoOnlineData.lunTiList = new List<string>();
            myLunDaoOnlineData.wuDaoLevelList = new List<string>();
            foreach (JSONObject jsonObj in jsonData.instance.WuDaoAllTypeJson.list)
            {
                int level = Tools.instance.getPlayer().wuDaoMag.getWuDaoLevelByType(jsonObj["id"].I);

                myLunDaoOnlineData.wuDaoLevelList.Add(level.ToString());
            }

            GameObject toggleGroup = GetGameObjectStatic<RectTransform>(dialogBox.gameObject, "ToggleGroup", true);
            List<Toggle> toggleList = toggleGroup.GetComponentsInChildren<Toggle>().ToList();

            foreach (Toggle toggle in toggleList)
            {
                if (toggle.isOn)
                {
                    myLunDaoOnlineData.lunTiList.Add(toggle.name);
                }
            }

            if (myLunDaoOnlineData.lunTiList.Count == 0)
            {
                UIPopTip.Inst.Pop("必须选择一个论题", PopTipIconType.叹号);
                return;
            }

            if (myLunDaoOnlineData.lunTiList.Count > 5)
            {
                UIPopTip.Inst.Pop("选择论题过多，不能超过5个", PopTipIconType.叹号);
                return;
            }

            string myOnlineDataString = JsonMapper.ToJson(myLunDaoOnlineData);


            Console.WriteLine("你向" + targetName + "发送了论道邀请");

            SendChatMessage(targetID.m_SteamID.ToString(), "邀请论道", myOnlineDataString);
            ChatingRoomMainPanel.lunDaoInviteButton.interactable = false;

            if (dialogBox.gameObject != null)
            {
                GameObject.Destroy(dialogBox.gameObject);
            }
        }




        public void AccpetOtherLundaoInviting()
        {
            GameObject toggleGroup = GetGameObjectStatic<RectTransform>(dialogBox.gameObject, "ToggleGroup", true);
            List<Toggle> toggleList = toggleGroup.GetComponentsInChildren<Toggle>().ToList();
            myLunDaoOnlineData = new OtherPlayerOnlineDataClass();
            myLunDaoOnlineData.lunTiList = new List<string>();
            foreach (Toggle toggle in toggleList)
            {
                if (toggle.isOn)
                {
                    myLunDaoOnlineData.lunTiList.Add(toggle.name);
                }
            }

            if (myLunDaoOnlineData.lunTiList.Count == 0)
            {
                UIPopTip.Inst.Pop("必须选择一个论题", PopTipIconType.叹号);
                return;
            }


            if (myLunDaoOnlineData.lunTiList.Count > 5)
            {
                UIPopTip.Inst.Pop("选择论题过多，不能超过5个", PopTipIconType.叹号);
                return;
            }


            KBEngine.Avatar avatar = Tools.instance.getPlayer();

            myLunDaoOnlineData.name = avatar.name;
            myLunDaoOnlineData.wuDaoJsonString = avatar.WuDaoJson.ToString();
            myLunDaoOnlineData.faceJsonString = jsonData.instance.AvatarRandomJsonData["1"].ToString();
            myLunDaoOnlineData.StatusId = avatar.LunDaoState;
            myLunDaoOnlineData.wuDaoLevelList = new List<string>();
            foreach (JSONObject jsonObj in jsonData.instance.WuDaoAllTypeJson.list)
            {
                int level = Tools.instance.getPlayer().wuDaoMag.getWuDaoLevelByType(jsonObj["id"].I);
                myLunDaoOnlineData.wuDaoLevelList.Add(level.ToString());
            }

            Console.WriteLine("myLunDaoOnlineData.lunTiList.Count:" + myLunDaoOnlineData.lunTiList.Count);
            string myOnlineDataString = JsonMapper.ToJson(myLunDaoOnlineData);
            Console.WriteLine("myOnlineDataString:" + myOnlineDataString);
            ChatingRoomManager.SendChatMessage(targetID.m_SteamID.ToString(), "接受论道邀请", myOnlineDataString);
            if (dialogBox != null)
            {
                Destroy(dialogBox.gameObject);
            }

            LunDaoManagerOnline.otherPlayerOnlineDataClass.lunTiList = myLunDaoOnlineData.lunTiList;
            lunDaoManagerOnline = PopCustomLunDaoManager().AddComponent<LunDaoManagerOnline>();
        }

        public void RefuseOtherLundaoInviting()
        {

            ChatingRoomManager.SendChatMessage(targetID.m_SteamID.ToString(), "拒绝论道邀请", "");
            Destroy(dialogBox.gameObject);
        }

        public IEnumerator LoadSpriteCoroutine(string url)
        {

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                tempSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }

        public static void randomAvatar(ref PlayerSetRandomFace playerSetRandomFace, JSONObject faceJson)
        {
            try
            {
                JSONObject jsonobject = faceJson;
                KBEngine.Avatar player = PlayerEx.Player;

                int num = player.Face;
                string text = player.FaceWorkshop;

                bool flag = false;
                if (num != 0 && playerSetRandomFace.GetComponent<SkeletonGraphic>() != null)
                {
                    Sprite sprite;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sprite = ModResources.LoadSprite(string.Format("workshop_{0}_{1}", text, num));
                    }
                    else
                    {
                        sprite = ModResources.LoadSprite(string.Format("Effect/Prefab/gameEntity/Avater/Avater{0}/{1}", num, num));
                    }
                    if (sprite != null)
                    {
                        flag = true;
                        playerSetRandomFace.BaseImage.SetActive(true);
                        playerSetRandomFace.BaseSpine.SetActive(false);
                        Image componentInChildren = playerSetRandomFace.BaseImage.GetComponentInChildren<Image>();
                        componentInChildren.sprite = sprite;
                        if (playerSetRandomFace.SpriteUseOffset)
                        {
                            string text2 = "P" + sprite.name;
                            if (TouXiangPianYi.DataDict.ContainsKey(text2))
                            {
                                TouXiangPianYi touXiangPianYi = TouXiangPianYi.DataDict[text2];
                                componentInChildren.rectTransform.anchoredPosition = new Vector2((float)touXiangPianYi.PX / 100f, (float)touXiangPianYi.PY / 100f);
                                componentInChildren.rectTransform.localScale = new Vector3((float)touXiangPianYi.SX / 100f, (float)touXiangPianYi.SY / 100f, 1f);
                            }
                        }
                    }
                }
                if (playerSetRandomFace.BaseImage != null)
                {
                    playerSetRandomFace.BaseImage.SetActive(flag);
                }
                if (playerSetRandomFace.BaseSpine != null)
                {
                    playerSetRandomFace.BaseSpine.SetActive(!flag);
                }
                if (!flag)
                {
                    playerSetRandomFace.setFaceByJson(jsonobject, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("设置形象时出错，错误:{0}\n{1}", ex.Message, ex.StackTrace));
                throw ex;
            }
        }
        public static Vector3 GetVec3ByString(string p_sVec3)
        {
            if (p_sVec3.Length <= 0)
                return Vector3.zero;

            string[] tmp_sValues = p_sVec3.Trim(' ').Split(',');
            if (tmp_sValues != null && tmp_sValues.Length == 3)
            {
                float tmp_fX = float.Parse(tmp_sValues[0]);
                float tmp_fY = float.Parse(tmp_sValues[1]);
                float tmp_fZ = float.Parse(tmp_sValues[2]);

                return new Vector3(tmp_fX, tmp_fY, tmp_fZ);
            }
            return Vector3.zero;
        }

        public static string SetVec3ByString(Vector3 vector3)
        {
            return vector3.x.ToString() + "," + vector3.y.ToString() + "," + vector3.z.ToString();
        }

        public ChatingRoomManager.CustomPlayerDataClass SetPlayerData()
        {
            KBEngine.Avatar avatar = Tools.instance.getPlayer();
            string jingJie = LevelUpDataJsonData.DataDict[(int)avatar.level].Name;
            ChatingRoomManager.CustomPlayerDataClass customPlayerData = new ChatingRoomManager.CustomPlayerDataClass();

            customPlayerData.customPrice = "0";
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

        public static void SetTextMenpaiColor(Text text, string menPai)
        {
            switch (menPai)
            {
                case "金虹剑派":
                    {
                        text.color = Color.yellow;
                    }
                    break;

                case "竹山宗":
                    {
                        text.color = Color.green;
                    }
                    break;
                case "星河剑派":
                    {

                        text.color = new Color32(128, 255, 255, 255);
                    }
                    break;
                case "离火门":
                    {
                        text.color = Color.red;
                    }
                    break;

                case "化尘教":
                    {
                        text.color = new Color32(224, 167, 5, 255);
                    }
                    break;
                case "散修":
                    {
                        text.color = Color.white;
                    }
                    break;
                default:
                    {
                        text.color = Color.black;
                    }
                    break;
            }
        }
    }
}
