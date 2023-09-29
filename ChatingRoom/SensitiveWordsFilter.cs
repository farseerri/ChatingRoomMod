using LitJson;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ChatingRoom
{
    public class SensitiveWordsFilter
    {
        public List<string> sensitiveWords;
        public static List<string> sensitiveWordsAll;
        public static List<Regex> sensitiveRegexes; // 敏感词正则表达式列表
        public static SensitiveWordsFilter Inst;

        [DllImport("kernel32.dll", EntryPoint = "LCMapStringA")]
        public static extern int LCMapString(int Locale, int dwMapFlags, byte[] lpSrcStr, int cchSrc, byte[] lpDestStr, int cchDest);
        const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        public SensitiveWordsFilter()
        {
            SensitiveWordsFilter.Inst = this;
            string sensitiveWordsPath = ChatingRoomManager.Inst.Path + "/ChatingF";
            string sensitiveWordsFileString = ChatingRoomManager.ReadFileByString(sensitiveWordsPath, Encoding.UTF8);
            //dynastiesStringList = JsonMapper.ToObject<List<string>>(dynastiesDataBuffer);

            string[] sensitiveWords = sensitiveWordsFileString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);





            sensitiveWordsAll = new List<string>();
            foreach (string str in sensitiveWords)
            {
                string textTemp = ToTraditional(str, LCMAP_SIMPLIFIED_CHINESE);
                sensitiveWordsAll.Add(textTemp);
            }

            foreach (string str in sensitiveWords)
            {
                string textTemp = ToTraditional(str, LCMAP_TRADITIONAL_CHINESE);
                sensitiveWordsAll.Add(textTemp);
            }

            sensitiveRegexes = new List<Regex>();
            foreach (string word in sensitiveWords)
            {
                // 将字符串中的特殊字符转义，并用 \b 匹配单词边界
                string regex = @"\b" + Regex.Escape(word) + @"\b";
                sensitiveRegexes.Add(new Regex(regex, RegexOptions.Compiled));
            }
        }

        //转化方法
        public static string ToTraditional(string source, int type)
        {
            byte[] srcByte2 = Encoding.Default.GetBytes(source);
            byte[] desByte2 = new byte[srcByte2.Length];
            LCMapString(2052, type, srcByte2, -1, desByte2, srcByte2.Length);
            string des2 = Encoding.Default.GetString(desByte2);
            return des2;
        }

        public static string Filter(string text)
        {
            // 遍历每个敏感词正则表达式
            foreach (Regex regex in sensitiveRegexes)
            {
                // 替换匹配的字符串为 ***
                text = regex.Replace(text, "***");
            }

            return text;
        }


        public static bool HasSensitiveWords(string text)
        {
            bool hasSensitiveWords = false;
            foreach (Regex regex in sensitiveRegexes)
            {
                if (regex.IsMatch(text))
                {
                    hasSensitiveWords = true;
                }
            }
            return hasSensitiveWords;
        }

    }
}
