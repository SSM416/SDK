using CWC.Common;
using Nlog.RabbitMQ.Target;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;


namespace Log.SDK
{
    public class LogHelper
    {
        private static Func<string> userStorageFunc;

        private static Regex robotRegex;

        private Logger logger;

        private static string siteIdentifier;

        private static string build;

        static LogHelper()
        {
            build = "";
            CompilationSection compilationSection = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            build = (compilationSection.Debug ? "debug" : "release");
            siteIdentifier = ConfigurationManager.AppSettings["log_sdk_siteappid_automatic"];
            robotRegex = new Regex("spider|Googlebot|robot|bingbot|msnbot", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string text = ConfigurationManager.AppSettings["log_sdk_rabbit_username"];
            bool flag = string.IsNullOrEmpty(text);
            if (flag)
            {
                text = "loguser";
            }
            string text2 = ConfigurationManager.AppSettings["log_sdk_rabbit_password"];
            bool flag2 = string.IsNullOrEmpty(text2);
            if (flag2)
            {
                text2 = "loguser";
            }
            string text3 = ConfigurationManager.AppSettings["log_sdk_rabbit_hostname"];
            bool flag3 = string.IsNullOrEmpty(text3);
            if (flag3)
            {
                text3 = "116.196.80.214";
            }
            string text4 = ConfigurationManager.AppSettings["log_sdk_rabbit_port"];
            ushort port = 5672;
            bool flag4 = string.IsNullOrEmpty(text4);
            if (flag4)
            {
                ushort num = 0;
                bool flag5 = ushort.TryParse(text4, out num);
                if (flag5)
                {
                    port = num;
                }
            }
            string text5 = ConfigurationManager.AppSettings["log_sdk_rabbit_exchange"];
            bool flag6 = string.IsNullOrEmpty(text5);
            if (flag6)
            {
                text5 = "rmq.logs.log";
            }
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
            FileTarget fileTarget = new FileTarget("logfile");
            fileTarget.ArchiveDateFormat = "yyyyMMdd";
            string text6 = ConfigurationManager.AppSettings["logdir"];
            bool flag7 = string.IsNullOrEmpty(text6);
            if (flag7)
            {
                fileTarget.FileName = "${basedir}/logs/${shortdate}-${level}.log";
            }
            else
            {
                fileTarget.FileName = Path.Combine(text6, "${shortdate}-${level}.log");
            }
            fileTarget.Layout = new SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${message}");
            robotRegex = new Regex("spider|Googlebot|robot|bingbot|msnbot", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            loggingConfiguration.AddTarget(fileTarget);
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Warn, fileTarget));
            bool flag8 = !string.IsNullOrEmpty(siteIdentifier);
            if (flag8)
            {
                RabbitMQTarget rabbitMQTarget = new RabbitMQTarget("logmq");
                rabbitMQTarget.UserName = text;
                rabbitMQTarget.Password = text2;
                rabbitMQTarget.HostName = text3;
                rabbitMQTarget.Exchange = text5;
                rabbitMQTarget.VHost = "/";
                rabbitMQTarget.Port = port;
                rabbitMQTarget.Topic = "application.log.{0}." + build + "." + siteIdentifier;
                rabbitMQTarget.AppId = siteIdentifier;
                loggingConfiguration.AddTarget(rabbitMQTarget);
                loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, rabbitMQTarget));
            }
            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = new bool?(true);
            LogManager.Configuration = loggingConfiguration;
        }

        private LogHelper(Type type)
        {
            this.logger = LogManager.GetLogger(type.FullName);
        }

        public static LogHelper GetInstance(Type type)
        {
            return new LogHelper(type);
        }

        public static void ConfigureUserStorage(Func<string> action)
        {
            LogHelper.userStorageFunc = action;
        }

        public LogEntity RenderLogEntity(Exception ex, LogLevel level)
        {
            IDictionary data = ex.Data;
            LogEntity logEntity = new LogEntity();
            logEntity.LogTime = DateTime.Now;
            logEntity.CodeStack = ex.StackTrace;
            logEntity.BuildMode = LogHelper.build;
            logEntity.LevelName = level.ToString();
            logEntity.Message = ex.GetType().FullName + ":" + ex.Message;
            bool flag = data != null;
            if (flag)
            {
                foreach (DictionaryEntry dictionaryEntry in data)
                {
                    object expr_8D = dictionaryEntry.Value;
                    bool flag2 = !string.IsNullOrEmpty((expr_8D != null) ? expr_8D.ToString() : null);
                    if (flag2)
                    {
                        Dictionary<string, string> arg_CD_0 = logEntity.ExceptionDataDic;
                        string arg_CD_1 = dictionaryEntry.Key.ToString();
                        object expr_C1 = dictionaryEntry.Value;
                        arg_CD_0.Add(arg_CD_1, (expr_C1 != null) ? expr_C1.ToString() : null);
                    }
                }
            }
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            this.AppendWebParameter(logEntity);
            return logEntity;
        }

        public LogEntity RenderLogEntity(string msg, LogLevel level)
        {
            LogEntity logEntity = new LogEntity();
            logEntity.LogTime = DateTime.Now;
            logEntity.LevelName = level.ToString();
            logEntity.Message = msg;
            logEntity.BuildMode = LogHelper.build;
            this.AppendWebParameter(logEntity);
            return logEntity;
        }

        public void AppendWebParameter(LogEntity logEntity)
        {
            HttpRequest httpRequest = (HttpContext.Current != null) ? HttpContext.Current.Request : null;
            bool flag = httpRequest != null;
            if (flag)
            {
                bool flag2 = robotRegex.IsMatch(httpRequest.UserAgent ?? "");
                if (flag2)
                {
                    logEntity = null;
                }
                else
                {
                    logEntity.QueryString = httpRequest.Url.Query;
                    logEntity.UrlPath = httpRequest.Url.LocalPath;
                    bool flag3 = httpRequest.Form != null;
                    if (flag3)
                    {
                        foreach (string text in httpRequest.Form.Keys)
                        {
                            string[] values = httpRequest.Form.GetValues(text);
                            logEntity.PostDataDic.Add(text, string.Join(",", values));
                        }
                    }
                    bool flag4 = httpRequest.InputStream != null && httpRequest.InputStream.Length != 0L;
                    if (flag4)
                    {
                        byte[] array = new byte[HttpContext.Current.Request.InputStream.Length];
                        HttpContext.Current.Request.InputStream.Read(array, 0, array.Length);
                        try
                        {
                            string @string = Encoding.UTF8.GetString(array);
                            logEntity.PostDataDic.Add("[InputStream]", @string);
                        }
                        catch (Exception ex)
                        {
                            logEntity.PostDataDic.Add("[InputStream]", "失败：" + ex.Message);
                        }
                    }
                    bool flag5 = LogHelper.userStorageFunc != null;
                    if (flag5)
                    {
                        try
                        {
                            logEntity.User = LogHelper.userStorageFunc();
                        }
                        catch (Exception ex2)
                        {
                            logEntity.User = "Exception:" + ex2.Message;
                        }
                    }
                    else
                    {
                        bool flag6 = HttpContext.Current.User != null;
                        if (flag6)
                        {
                            FormsIdentity formsIdentity = HttpContext.Current.User.Identity as FormsIdentity;
                            bool flag7 = formsIdentity != null;
                            if (flag7)
                            {
                                logEntity.User = string.Format("Name:{0},{1}", formsIdentity.Name, (formsIdentity.Ticket != null) ? ("TicketName:" + formsIdentity.Ticket.Name + ",TicketUserData:" + formsIdentity.Ticket.UserData) : "");
                            }
                            else
                            {
                                logEntity.User = HttpContext.Current.User.Identity.Name;
                            }
                        }
                    }
                    logEntity.Method = httpRequest.HttpMethod;
                    bool flag8 = logEntity.Method.ToUpper() == "POST";
                    if (flag8)
                    {
                        logEntity.PostDataDic.Add("[ContentType]", httpRequest.ContentType);
                    }
                    string text2 = httpRequest.Headers.Get("X-Real-IP");
                    bool flag9 = string.IsNullOrEmpty(text2);
                    if (flag9)
                    {
                        text2 = httpRequest.UserHostAddress;
                    }
                    logEntity.IP = text2;
                    logEntity.UserAgent = httpRequest.UserAgent;
                    logEntity.UrlReferrer = ((httpRequest.UrlReferrer != null) ? httpRequest.UrlReferrer.AbsoluteUri : null);
                    Uri uri = new Uri(httpRequest.Url.AbsoluteUri);
                    UriBuilder uriBuilder = new UriBuilder(httpRequest.Url.AbsoluteUri);
                    string text3 = httpRequest.Headers.Get("Host");
                    bool flag10 = !string.IsNullOrEmpty(text3);
                    if (flag10)
                    {
                        string[] array2 = text3.Split(new char[]
                        {
                            ':'
                        });
                        bool flag11 = array2.Length == 1;
                        if (flag11)
                        {
                            uriBuilder.Port = -1;
                        }
                        else
                        {
                            bool flag12 = array2.Length == 2;
                            if (flag12)
                            {
                                uriBuilder.Port = int.Parse(array2[1]);
                            }
                        }
                    }
                    logEntity.Url = uriBuilder.ToString();
                }
            }
        }

        public void Info(string msg, params object[] args)
        {
            bool flag = !string.IsNullOrEmpty(msg);
            if (flag)
            {
                LogEntity obj = this.RenderLogEntity(this.FormatString(msg, args), LogLevel.Info);
                this.logger.Info(JsonHelper.SerializeObject(obj));
            }
        }

        public void Warn(string msg, params object[] args)
        {
            bool flag = !string.IsNullOrEmpty(msg);
            if (flag)
            {
                LogEntity obj = this.RenderLogEntity(this.FormatString(msg, args), LogLevel.Warn);
                this.logger.Warn(JsonHelper.SerializeObject(obj));
            }
        }

        public void Warn(Exception ex)
        {
            bool flag = ex.Data != null && ex.Data.Contains("Log.SDK.LogHelper.IsRecorded");
            if (!flag)
            {
                LogEntity obj = this.RenderLogEntity(ex, LogLevel.Warn);
                this.logger.Warn(JsonHelper.SerializeObject(obj));
                ex.Data["Log.SDK.LogHelper.IsRecorded"] = "True";
            }
        }

        public void Error(string msg, params object[] args)
        {
            bool flag = !string.IsNullOrEmpty(msg);
            if (flag)
            {
                LogEntity obj = this.RenderLogEntity(this.FormatString(msg, args), LogLevel.Error);
                this.logger.Error(JsonHelper.SerializeObject(obj));
            }
        }

        public void Error(Exception ex)
        {
            bool flag = ex.Data != null && ex.Data.Contains("Log.SDK.LogHelper.IsRecorded");
            if (!flag)
            {
                LogEntity obj = this.RenderLogEntity(ex, LogLevel.Error);
                this.logger.Error(JsonHelper.SerializeObject(obj));
                ex.Data["Log.SDK.LogHelper.IsRecorded"] = "True";
            }
        }

        private string FormatString(string format, params object[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    format = format.Replace("{" + i + "}", args[i].ToString());
                }
            }
            catch (Exception ex)
            {
                this.Warn(ex);
            }
            return format;
        }
    }
}