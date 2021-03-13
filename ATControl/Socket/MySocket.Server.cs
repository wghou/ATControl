using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using NLog;
using SuperSocket.Facility.Protocol;

namespace ATControl.Socket
{
    public class MySocketServer
    {
        private static readonly Logger nlogger = LogManager.GetCurrentClassLogger();

        private MyAppServer _appServer = new MyAppServer();

        /// <summary>
        /// 是否启用 mqtt
        /// </summary>
        public bool Enabled { get; set; } = false;


        //
        public delegate void MessageReceievedEventHandler(JObject message);
        /// <summary>
        /// 事件 - 接收到 post 消息
        /// </summary>
        public event MessageReceievedEventHandler MessageReceievedEvent;


        ~MySocketServer()
        {
            _appServer.Stop();
        }


        /// <summary>
        /// 初始化 socket server
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Init(JObject config)
        {
            try
            {
                if (!_appServer.Setup((int)config["Port"]))
                {
                    nlogger.Error("Failed to setup!");
                    Enabled = false;
                    return false;
                }

                if (!_appServer.Start())
                {
                    nlogger.Error("Failed to start.");
                    Enabled = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: config lot client failed.");

                Enabled = false;
                return false;
            }

            _appServer.NewSessionConnected += _appServer_NewSessionConnected;
            _appServer.SessionClosed += _appServer_SessionClosed;
            _appServer.NewRequestReceived += _appServer_NewRequestReceived;

            Enabled = true;
            return true;
        }


        /// <summary>
        /// 查询 mqtt 是否连接成功
        /// </summary>
        public bool isConnected
        {
            get { return true; }
        }

        /// <summary>
        /// 通过 socket 发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool pushMessage(JObject message)
        {
            // 遍历 session
            foreach (var itm in _appServer.GetAllSessions())
            {
                itm.Send(message.ToString());
            }

            return true;
        }


        /// new session connected.
        private void _appServer_NewSessionConnected(MySession session)
        {
            nlogger.Debug("service get connection from clinet successful.");
            var count = _appServer.GetAllSessions().Count();

            nlogger.Debug("server.session.count: " + count);

            //session.Send("Welcome to SuperSocket Telnet Server.");
        }

        /// session closed
        private void _appServer_SessionClosed(MySession session, CloseReason value)
        {
            nlogger.Debug("Server lost one connection from the client.");
            var count = _appServer.GetAllSessions().Count();

            nlogger.Debug("server.session.count: " + count);
        }

        /// <summary>
        /// socket 接收到数据 - 事件处理函数
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        private void _appServer_NewRequestReceived(MySession session, MyRequestInfo requestInfo)
        {
            if (!Enabled) return;

            // StringRequestInfo 的格式
            // todo: 在使用 json convertor 的时候，如果解析错误怎么办？会发生异常吗？
            if (requestInfo.JsonString.Length != 0)
            {
                // 解析并触发事件
                // todo: exception 解析方法错误
                JObject jMsg = (JObject)JsonConvert.DeserializeObject(requestInfo.JsonString);
                MessageReceievedEvent?.Invoke(jMsg);
            }
            else
            {
                nlogger.Error("received bad message from MySocketSever: requestInfo.Body.Length == 0");
            }
        }
    }
}
