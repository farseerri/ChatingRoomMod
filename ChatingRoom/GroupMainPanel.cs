using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class GroupMainPanel : ChatingRoomPanelBase
    {
        public static ScrollRect friendsScrollRect;
        public static Button searchFriendsButton;
        public static Button createGroupButton;


        public override void Awake()
        {
            base.Awake();
            searchFriendsButton = ChatingRoomManager.GetGameObjectStatic<Button>(this.gameObject, "SearchFriends", true).GetComponent<Button>();
            createGroupButton = ChatingRoomManager.GetGameObjectStatic<Button>(this.gameObject, "CreateGroup", true).GetComponent<Button>();
            friendsScrollRect = ChatingRoomManager.GetGameObjectStatic<ScrollRect>(this.gameObject, "FriendsScrollView", true).GetComponent<ScrollRect>();
        }

        public override void Start()
        {
            base.Start();
            searchFriendsButton.onClick.AddListener(() => ChatingRoomManager.SearchPlayingSameGameFrirends());
            createGroupButton.onClick.AddListener(() => ChatingRoomManager.CreateGroup());

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
