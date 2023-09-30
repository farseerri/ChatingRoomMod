using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class PlayerDataPanel : MonoBehaviour
    {
        public static Text playerName;
        public static RawImage otherAvatarImage;
        public static Button lunDaoInviteButton;
        public static Text stateText;
        public static Button addFriendButton;
        public static GameObject faceObj;

        //public ChatingRoomItemCell otherPlayerItemCell;

        public void Start()
        {
            playerName = ChatingRoomManager.GetGameObjectStatic<Text>(this.gameObject, "PlayerName", true).GetComponent<Text>();
            //otherPlayerItemCell = ChatingRoomManager.Inst.GetChild<RectTransform>(this.gameObject, "OtherPlayerWTSItem", true).AddComponent<ChatingRoomItemCell>();
            otherAvatarImage = ChatingRoomManager.GetGameObjectStatic<RawImage>(this.gameObject, "AvatarImage", true).GetComponent<RawImage>();
            Console.WriteLine("otherAvatarImage.name:" + otherAvatarImage.name);
            lunDaoInviteButton = ChatingRoomManager.GetGameObjectStatic<Button>(this.gameObject, "LunDaoButton", true).GetComponent<Button>();
            stateText = ChatingRoomManager.GetGameObjectStatic<Text>(this.gameObject, "State", true).GetComponent<Text>();
            addFriendButton = ChatingRoomManager.GetGameObjectStatic<Button>(this.gameObject.gameObject, "AddFriend", true).GetComponent<Button>();
            faceObj = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "Face", true);

        }
        public void Update()
        {

        }




    }
}
