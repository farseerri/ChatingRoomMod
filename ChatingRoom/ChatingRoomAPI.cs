using SkySwordKill.Next.DialogEvent;
using SkySwordKill.Next.DialogSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatingRoom
{
    [DialogEvent("ChatingRoomAPI")]
    [DialogEvent("聊天室NEXT接口")]
    public class ChatingRoomAPI : IDialogEvent
    {
        public void Execute(DialogCommand command, DialogEnvironment env, Action callback)
        {

            var commandStr = command.GetStr(0);

            switch (commandStr)
            {
                case "进入聊天室":
                    {
                       // ChatingRoomManager.特殊mod场景进入();
                    }
                    break;
                case "退出聊天室":
                    {
                       // ChatingRoomManager.特殊mod场景退出();
                    }
                    break;
                default:
                    break;
            }
            callback?.Invoke();
        }
    }
}
