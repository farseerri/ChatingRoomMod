using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ChatingRoom
{
    public class LunDaoSayWordOnline : MonoBehaviour
    {
        public Text content;
        public void Say(string msg)
        {
            this.content.text = msg;
            base.Invoke("Show", 0.1f);
        }

        // Token: 0x06001C28 RID: 7208 RVA: 0x000C83FC File Offset: 0x000C65FC
        private void Show()
        {
            base.gameObject.SetActive(true);
            base.Invoke("Hide", 2f);
        }

        // Token: 0x06001C29 RID: 7209 RVA: 0x000C331D File Offset: 0x000C151D
        public void Hide()
        {
            base.gameObject.SetActive(false);
        }

        // Token: 0x06001C2A RID: 7210 RVA: 0x000C841A File Offset: 0x000C661A
        private void Awake()
        {
            content = GetComponentInChildren<Text>();
        }

    }
}
