using Bag;
using GUIPackage;
using JSONClass;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class ChatingRoomMainPanel : ChatingRoomPanelBase
    {
        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {

            ChatingRoomMainPanel.memberScrollRectContent = ChatingRoomManager.GetGameObjectStatic<RectTransform>(this.gameObject, "MemberPanel", true).GetComponentInChildren<ScrollRect>().content;


            base.Start();
        }

        public override void OnDestroy()
        {
            if (ChatingRoomManager.Inst.RefreshEnumerator != null)
            {

                StopCoroutine(ChatingRoomManager.Inst.RefreshEnumerator);
                ChatingRoomManager.Inst.RefreshEnumerator = null;
            }
            base.OnDestroy();
        }
    }
}
