using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Log.SDK
{
    public class LogEntity
    {
        public string BuildMode
        {
            get;
            set;
        }

        public string LevelName
        {
            get;
            set;
        }

        public DateTime LogTime
        {
            get;
            set;
        }

        public string UrlReferrer
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string UrlPath
        {
            get;
            set;
        }

        public string QueryString
        {
            get;
            set;
        }

        public Dictionary<string, string> PostDataDic
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string IP
        {
            get;
            set;
        }

        public string UserAgent
        {
            get;
            set;
        }

        public string Method
        {
            get;
            set;
        }

        public string User
        {
            get;
            set;
        }

        public string CodeStack
        {
            get;
            set;
        }

        public Dictionary<string, string> ExceptionDataDic
        {
            get;
            set;
        }

        public LogEntity()
        {
            this.ExceptionDataDic = new Dictionary<string, string>();
            this.PostDataDic = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = !string.IsNullOrEmpty(this.IP);
            if (flag)
            {
                stringBuilder.AppendLine("用户主机：" + this.IP);
            }
            bool flag2 = !string.IsNullOrEmpty(this.UserAgent);
            if (flag2)
            {
                stringBuilder.AppendLine("用户代理：" + this.UserAgent);
            }
            stringBuilder.AppendLine("记录时间：" + this.LogTime.ToString("yyyy-MM-dd HH:mm:ss"));
            bool flag3 = !string.IsNullOrEmpty(this.User);
            if (flag3)
            {
                stringBuilder.AppendLine("当前用户：" + this.User);
            }
            bool flag4 = this.PostDataDic != null && this.PostDataDic.Count != 0;
            if (flag4)
            {
                stringBuilder.AppendLine("---------------------------POST params Start---------------------------");
                foreach (KeyValuePair<string, string> current in this.PostDataDic)
                {
                    stringBuilder.AppendLine(string.Format("{0}：{1}", current.Key, current.Value));
                }
                stringBuilder.AppendLine("---------------------------POST params End---------------------------");
            }
            bool flag5 = this.ExceptionDataDic != null && this.ExceptionDataDic.Count != 0;
            if (flag5)
            {
                stringBuilder.AppendLine("---------------------------Exception params Start---------------------------");
                foreach (KeyValuePair<string, string> current2 in this.ExceptionDataDic)
                {
                    stringBuilder.AppendLine(string.Format("{0}：{1}", current2.Key, current2.Value));
                }
                stringBuilder.AppendLine("---------------------------Exception Params End---------------------------");
            }
            bool flag6 = !string.IsNullOrEmpty(this.UrlReferrer);
            if (flag6)
            {
                stringBuilder.AppendLine("来源网址：" + this.UrlReferrer);
            }
            bool flag7 = !string.IsNullOrEmpty(this.Method) || !string.IsNullOrEmpty(this.Url);
            if (flag7)
            {
                stringBuilder.AppendLine(string.Format("当前网址：{0} - {1}", this.Method, this.Url));
            }
            bool flag8 = !string.IsNullOrEmpty(this.QueryString);
            if (flag8)
            {
                stringBuilder.AppendLine(string.Format("地址参数：{0}", this.QueryString));
            }
            stringBuilder.AppendLine("信息详情：" + this.Message);
            bool flag9 = !string.IsNullOrEmpty(this.CodeStack);
            if (flag9)
            {
                stringBuilder.AppendLine("代码追踪：" + this.CodeStack);
            }
            return stringBuilder.ToString();
        }
    }
}