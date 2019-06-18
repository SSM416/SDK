using CWC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CWC.Library.Api
{
    public class ApiResponse
    {
        public int code
        {
            get;
            set;
        }

        public string msg
        {
            get;
            set;
        }

        public virtual object data
        {
            get;
            set;
        }

        public static ApiResponse SystemError(string url, string msg)
        {
            return new ApiResponse
            {
                code = 10001,
                msg = string.Format("未知错误：{0} {1}", url, msg)
            };
        }

        public static ApiResponse NetworkError(string url, string msg)
        {
            return new ApiResponse
            {
                code = 10002,
                msg = string.Format("请求异常：{0} {1}", url, msg)
            };
        }

        public static ApiResponse ProtocolError(string url, string data)
        {
            return new ApiResponse
            {
                code = 10003,
                msg = string.Format("返回异常：{0} {1}", url, data)
            };
        }

        public static ApiResponse DeserializeError(string url, string data)
        {
            return new ApiResponse
            {
                code = 10004,
                msg = string.Format("转换异常：{0} {1}", url, data)
            };
        }

        public static ApiResponse ParamaterError(string arg)
        {
            return new ApiResponse
            {
                code = 10005,
                msg = string.Format("参数异常：{0}", arg)
            };
        }

        public override bool Equals(object obj)
        {
            bool flag = obj == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !(obj is ApiResponse);
                result = (!flag2 && this.Equals((ApiResponse)obj));
            }
            return result;
        }

        public bool Equals(ApiResponse returnCode)
        {
            bool flag = returnCode == null;
            return !flag && this.code == returnCode.code;
        }

        public override int GetHashCode()
        {
            return this.code.GetHashCode();
        }

        public override string ToString()
        {
            return JsonHelper.SerializeObject(this);
        }

        public string ToJsonp(string callback)
        {
            return string.Format("{0}({1});", callback, JsonHelper.SerializeObject(this));
        }

        public string ToJsonp(string callback, string data)
        {
            this.data = data;
            return this.ToJsonp(callback);
        }
    }
}