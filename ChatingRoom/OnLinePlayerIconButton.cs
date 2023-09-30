using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Steamworks;
using System.Collections;

namespace ChatingRoom
{
    public class OnlinePlayerIconButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public CSteamID playerSteamID;
        public string playerName;
        public string playerGender;
        public string menPai;
        private RectTransform rt;



        public void Start()
        {
            rt = GetComponent<RectTransform>();
        }
        public void Update()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rt.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            Console.WriteLine(playerName + " SteamID:" + playerSteamID.ToString() + " 性别:" + playerGender + " 门派:" + menPai);
            SelectSomeOneInMap(playerSteamID);

        }

        public void SelectSomeOneInMap(CSteamID targetID)
        {
            StartCoroutine(SelectSomeOneInMapIEnumerator(targetID));
        }


        public IEnumerator SelectSomeOneInMapIEnumerator(CSteamID targetID)
        {
            if (ChatingRoomManager.playerDataPanel == null)
            {
                ChatingRoomManager.playerDataPanel = Instantiate<GameObject>(ChatingRoomManager.playerDataPanelPrefab, UnityEngine.Object.FindObjectOfType<NewUICanvas>().transform).AddComponent<PlayerDataPanel>();
            }

            ChatingRoomManager.SendChatMessage(targetID.m_SteamID.ToString(), "野外请求玩家数据", "");

            // 等待otherAvatarImage不为空
            while (PlayerDataPanel.otherAvatarImage == null)
            {
                yield return null;
            }

            // 然后继续执行后面的代码
            PlayerDataPanel.otherAvatarImage.texture = null;
            SteamFriends.GetLargeFriendAvatar(targetID);
            PlayerDataPanel.lunDaoInviteButton.interactable = true;
        }




    }
}
