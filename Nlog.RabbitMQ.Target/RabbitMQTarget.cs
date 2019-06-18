using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nlog.RabbitMQ.Target
{
    [Target("RabbitMQ")]
    public class RabbitMQTarget : TargetWithLayout
    {
        public enum CompressionTypes
        {
            None,
            GZip
        }

        private IConnection _Connection;

        private IModel _Model;

        private readonly Encoding _Encoding = Encoding.UTF8;

        private readonly Queue<Tuple<byte[], IBasicProperties, string>> _UnsentMessages = new Queue<Tuple<byte[], IBasicProperties, string>>(512);

        private object _sync = new object();

        public string VHost
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public ushort Port
        {
            get;
            set;
        }

        public Layout Topic
        {
            get;
            set;
        }

        public IProtocol Protocol
        {
            get;
            set;
        }

        public string HostName
        {
            get;
            set;
        }

        public string Exchange
        {
            get;
            set;
        }

        public string ExchangeType
        {
            get;
            set;
        }

        public bool Durable
        {
            get;
            set;
        }

        public bool Passive
        {
            get;
            set;
        }

        public string AppId
        {
            get;
            set;
        }

        public int MaxBuffer
        {
            get;
            set;
        }

        public ushort HeartBeatSeconds
        {
            get;
            set;
        }

        public bool UseJSON
        {
            get;
            set;
        }

        public bool UseSsl
        {
            get;
            set;
        }

        public string SslCertPath
        {
            get;
            set;
        }

        public string SslCertPassphrase
        {
            get;
            set;
        }

        public DeliveryMode DeliveryMode
        {
            get;
            set;
        }

        public int Timeout
        {
            get;
            set;
        }

        public RabbitMQTarget.CompressionTypes Compression
        {
            get;
            set;
        }

        [ArrayParameter(typeof(Field), "field")]
        public IList<Field> Fields
        {
            get;
            private set;
        }

        public bool UseLayoutAsMessage
        {
            get;
            set;
        }

        public RabbitMQTarget(string name)
        {
            this.Layout = "${message}";
            this.Compression = RabbitMQTarget.CompressionTypes.None;
            this.Fields = new List<Field>();
            base.Name = name;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            IBasicProperties basicProperties = this.GetBasicProperties(logEvent);
            byte[] message = this.GetMessage(logEvent);
            byte[] array = this.CompressMessage(message);
            string topic = this.GetTopic(logEvent);
            IModel model = this._Model;
            bool flag = model == null || !model.IsOpen;
            if (flag)
            {
                this.StartConnection(this._Connection, this.Timeout, true);
                model = this._Model;
            }
            bool flag2 = model == null || !model.IsOpen;
            if (flag2)
            {
                bool flag3 = !this.AddUnsent(topic, basicProperties, array);
                if (flag3)
                {
                    throw new InvalidOperationException("LogEvent discarded because RabbitMQ instance is offline and reached MaxBuffer");
                }
            }
            else
            {
                bool flag4 = true;
                try
                {
                    this.CheckUnsent(model);
                    this.Publish(model, array, basicProperties, topic);
                    flag4 = false;
                }
                catch (IOException ex)
                {
                    InternalLogger.Error(ex, "RabbitMQTarget(Name={0}): Could not send to RabbitMQ instance: {1}", new object[]
                    {
                        base.Name,
                        ex.Message
                    });
                    bool flag5 = !this.AddUnsent(topic, basicProperties, array);
                    if (flag5)
                    {
                        throw;
                    }
                }
                catch (ObjectDisposedException ex2)
                {
                    InternalLogger.Error(ex2, "RabbitMQTarget(Name={0}): Could not send to RabbitMQ instance: {1}", new object[]
                    {
                        base.Name,
                        ex2.Message
                    });
                    bool flag6 = !this.AddUnsent(topic, basicProperties, array);
                    if (flag6)
                    {
                        throw;
                    }
                }
                catch (Exception ex3)
                {
                    flag4 = false;
                    InternalLogger.Error(ex3, "RabbitMQTarget(Name={0}): Could not send to RabbitMQ instance: {1}", new object[]
                    {
                        base.Name,
                        ex3.Message
                    });
                    throw;
                }
                finally
                {
                    bool flag7 = flag4;
                    if (flag7)
                    {
                        this.StartConnection(this._Connection, Math.Min(500, this.Timeout), true);
                    }
                }
            }
        }

        private bool AddUnsent(string routingKey, IBasicProperties basicProperties, byte[] message)
        {
            bool flag = this._UnsentMessages.Count < this.MaxBuffer;
            bool result;
            if (flag)
            {
                this._UnsentMessages.Enqueue(Tuple.Create<byte[], IBasicProperties, string>(message, basicProperties, routingKey));
                result = true;
            }
            else
            {
                InternalLogger.Warn<string, int>("RabbitMQTarget(Name={0}): MaxBuffer {1} filled. Ignoring message.", base.Name, this.MaxBuffer);
                result = false;
            }
            return result;
        }

        private void CheckUnsent(IModel model)
        {
            while (this._UnsentMessages.Count > 0)
            {
                Tuple<byte[], IBasicProperties, string> tuple = this._UnsentMessages.Dequeue();
                InternalLogger.Info<string, Tuple<byte[], IBasicProperties, string>>("RabbitMQTarget(Name={0}): Publishing unsent message: {1}.", base.Name, tuple);
                this.Publish(model, tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        private void Publish(IModel model, byte[] bytes, IBasicProperties basicProperties, string routingKey)
        {
            model.BasicPublish(this.Exchange, routingKey, true, basicProperties, bytes);
        }

        private string GetTopic(LogEventInfo eventInfo)
        {
            string text = this.Topic.Render(eventInfo);
            return text.Replace("{0}", eventInfo.Level.Name);
        }

        private byte[] GetMessage(LogEventInfo info)
        {
            string messageInner = MessageFormatter.GetMessageInner(this.UseJSON, this.UseLayoutAsMessage, this.Layout, info, this.Fields);
            return this._Encoding.GetBytes(messageInner);
        }

        private IBasicProperties GetBasicProperties(LogEventInfo @event)
        {
            return new BasicProperties
            {
                ContentEncoding = "utf8",
                ContentType = (this.UseJSON || this.Layout is JsonLayout) ? "application/json" : "text/plain",
                AppId = this.AppId ?? @event.LoggerName,
                Timestamp = new AmqpTimestamp(MessageFormatter.GetEpochTimeStamp(@event)),
                UserId = this.UserName,
                DeliveryMode = (byte)this.DeliveryMode
            };
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            this.StartConnection(this._Connection, this.Timeout, false);
        }

        private void StartConnection(IConnection oldConnection, int timeoutMilliseconds, bool checkInitialized)
        {
            bool arg_3F_0;
            if (oldConnection != this._Connection)
            {
                IModel expr_30 = this._Model;
                arg_3F_0 = (expr_30 != null && expr_30.IsOpen);
            }
            else
            {
                arg_3F_0 = false;
            }
            bool flag = arg_3F_0;
            if (!flag)
            {
                Task task = Task.Factory.StartNew(delegate
                {
                    object sync = this._sync;
                    bool flag3 = false;
                    try
                    {
                        Monitor.Enter(sync, ref flag3);
                        bool flag4 = checkInitialized && !this.IsInitialized;
                        if (!flag4)
                        {
                            bool arg_77_0;
                            if (oldConnection != this._Connection)
                            {
                                IModel expr_68 = this._Model;
                                arg_77_0 = (expr_68 != null && expr_68.IsOpen);
                            }
                            else
                            {
                                arg_77_0 = false;
                            }
                            bool flag5 = arg_77_0;
                            if (!flag5)
                            {
                                InternalLogger.Info<string>("RabbitMQTarget(Name={0}): Connection attempt started...", this.Name);
                                oldConnection = (this._Connection ?? oldConnection);
                                bool flag6 = oldConnection != null;
                                if (flag6)
                                {
                                    ShutdownEventArgs reason = new ShutdownEventArgs(ShutdownInitiator.Application, 504, "Model not open to RabbitMQ instance", null);
                                    this.ShutdownAmqp(oldConnection, reason);
                                }
                                IModel model = null;
                                IConnection connection = null;
                                try
                                {
                                    connection = this.GetConnectionFac().CreateConnection();
                                    connection.ConnectionShutdown += delegate (object s, ShutdownEventArgs e)
                                    {
                                        this.ShutdownAmqp((this._Connection == connection) ? connection : null, e);
                                    };
                                    try
                                    {
                                        model = connection.CreateModel();
                                    }
                                    catch (Exception ex)
                                    {
                                        IConnection connection3 = connection;
                                        connection = null;
                                        InternalLogger.Error(ex, "RabbitMQTarget(Name={0}): Could not create model, {1}", new object[]
                                        {
                                            this.Name,
                                            ex.Message
                                        });
                                        connection3.Close(1000);
                                        connection3.Abort(1000);
                                    }
                                    bool flag7 = model != null && !this.Passive;
                                    if (flag7)
                                    {
                                        try
                                        {
                                            model.ExchangeDeclare(this.Exchange, this.ExchangeType, this.Durable, false, null);
                                        }
                                        catch (Exception ex2)
                                        {
                                            IConnection connection2 = connection;
                                            connection = null;
                                            InternalLogger.Error(ex2, string.Format("RabbitMQTarget(Name={0}): Could not declare exchange: {1}", this.Name, ex2.Message));
                                            model.Dispose();
                                            model = null;
                                            connection2.Close(1000);
                                            connection2.Abort(1000);
                                        }
                                    }
                                }
                                catch (Exception ex3)
                                {
                                    connection = null;
                                    InternalLogger.Error(ex3, string.Format("RabbitMQTarget(Name={0}): Could not connect to Rabbit instance: {1}", this.Name, ex3.Message));
                                }
                                finally
                                {
                                    bool flag8 = connection != null && model != null;
                                    if (flag8)
                                    {
                                        IModel expr_291 = this._Model;
                                        bool flag9 = expr_291 != null && expr_291.IsOpen;
                                        if (flag9)
                                        {
                                            InternalLogger.Info<string>("RabbitMQTarget(Name={0}): Connection attempt completed succesfully, but not needed", this.Name);
                                        }
                                        else
                                        {
                                            this._Connection = connection;
                                            this._Model = model;
                                            InternalLogger.Info<string>("RabbitMQTarget(Name={0}): Connection attempt completed succesfully", this.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (flag3)
                        {
                            Monitor.Exit(sync);
                        }
                    }
                });
                bool flag2 = !task.Wait(TimeSpan.FromMilliseconds((double)timeoutMilliseconds));
                if (flag2)
                {
                    InternalLogger.Warn<string>("RabbitMQTarget(Name={0}): Starting connection-task timed out, continuing", base.Name);
                }
            }
        }

        private ConnectionFactory GetConnectionFac()
        {
            return new ConnectionFactory
            {
                HostName = this.HostName,
                VirtualHost = this.VHost,
                UserName = this.UserName,
                Password = this.Password,
                RequestedHeartbeat = this.HeartBeatSeconds,
                Port = (int)this.Port,
                Ssl = new SslOption
                {
                    Enabled = this.UseSsl,
                    CertPath = this.SslCertPath,
                    CertPassphrase = this.SslCertPassphrase,
                    ServerName = this.HostName
                }
            };
        }

        private void ShutdownAmqp(IConnection connection, ShutdownEventArgs reason)
        {
            bool flag = reason.ReplyCode != 200;
            if (flag)
            {
                InternalLogger.Warn<string, ushort, string>("RabbitMQTarget(Name={0}): Connection shutdown. ReplyCode={1}, ReplyText={2}", base.Name, reason.ReplyCode, reason.ReplyText);
            }
            else
            {
                InternalLogger.Info<string, ushort, string>("RabbitMQTarget(Name={0}): Connection shutdown. ReplyCode={1}, ReplyText={2}", base.Name, reason.ReplyCode, reason.ReplyText);
            }
            object sync = this._sync;
            lock (sync)
            {
                bool flag3 = connection != null;
                if (flag3)
                {
                    IModel model = null;
                    bool flag4 = connection == this._Connection;
                    if (flag4)
                    {
                        model = this._Model;
                        this._Connection = null;
                        this._Model = null;
                    }
                    try
                    {
                        bool flag5 = reason.ReplyCode == 200 && connection.IsOpen;
                        if (flag5)
                        {
                            if (model != null)
                            {
                                model.Close();
                            }
                        }
                        else if (model != null)
                        {
                            model.Abort();
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Error(ex, "RabbitMQTarget(Name={0}): Could not close model: {1}", new object[]
                        {
                            base.Name,
                            ex.Message
                        });
                    }
                    try
                    {
                        bool flag6 = reason.ReplyCode == 200 && connection.IsOpen;
                        if (flag6)
                        {
                            connection.Close(reason.ReplyCode, reason.ReplyText, 1500);
                        }
                        else
                        {
                            connection.Abort(reason.ReplyCode, reason.ReplyText, 1500);
                        }
                    }
                    catch (Exception ex2)
                    {
                        InternalLogger.Error(ex2, "RabbitMQTarget(Name={0}): Could not close connection: {1}", new object[]
                        {
                            base.Name,
                            ex2.Message
                        });
                    }
                }
            }
        }

        protected override void CloseTarget()
        {
            ShutdownEventArgs reason = new ShutdownEventArgs(ShutdownInitiator.Application, 200, "closing target", null);
            this.ShutdownAmqp(this._Connection, reason);
            base.CloseTarget();
        }

        private byte[] CompressMessage(byte[] messageBytes)
        {
            RabbitMQTarget.CompressionTypes compression = this.Compression;
            byte[] result;
            if (compression != RabbitMQTarget.CompressionTypes.None)
            {
                if (compression != RabbitMQTarget.CompressionTypes.GZip)
                {
                    throw new NLogConfigurationException(string.Format("Compression type '{0}' not supported.", this.Compression));
                }
                result = this.CompressMessageGZip(messageBytes);
            }
            else
            {
                result = messageBytes;
            }
            return result;
        }

        private byte[] CompressMessageGZip(byte[] messageBytes)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gZipStream.Write(messageBytes, 0, messageBytes.Length);
            }
            return memoryStream.ToArray();
        }
    }
}