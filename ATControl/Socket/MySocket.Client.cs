using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using NLog;
using System.Net;

namespace ATControl.Socket
{
    public class MySocketClient
    {
        private static readonly Logger nlogger = LogManager.GetCurrentClassLogger();

        private AsyncTcpSession _appClient = new AsyncTcpSession();


        private string ip = "127.0.0.1";
        private int port = 6003;

        //
        public delegate void MessageReceievedEventHandler(JObject message);
        /// <summary>
        /// 事件 - 接收到 post 消息
        /// </summary>
        public event MessageReceievedEventHandler MessageReceievedEvent;


        /// <summary>
        /// 初始化 socket client
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Init(JObject config)
        {
            try
            {
                // get local ip address
                foreach (IPAddress _ipaddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_ipaddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        nlogger.Trace("get the local ip address: " + _ipaddress.ToString());
                        ip = _ipaddress.ToString();
                    }
                }

                if (config.ContainsKey("IP")) ip = (string)config["IP"];
                if (config.ContainsKey("Port")) port = (int)config["Port"];

                _appClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                _appClient.Connected += _appClient_Connected;
                _appClient.DataReceived += _appClient_DataReceived;
                _appClient.Closed += _appClient_Closed;
            }
            catch (Exception ex)
            {
                nlogger.Error("error with the socket client: " + ex.Message);
                return false;
            }

            return true;
        }


        /// <summary>
        /// 通过 socket 发送数据
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool pushMessage(JObject message)
        {
            try
            {
                if (!_appClient.IsConnected)
                {
                    _appClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                }

                byte[] data = Encoding.Default.GetBytes(message.ToString());
                _appClient.Send(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                nlogger.Error("error with the socket client: " + ex.Message);
                return false;
            }

            return true;
        }

        /// socket client closed
        private void _appClient_Closed(object sender, EventArgs e)
        {
            nlogger.Warn("the client closed, reconnecting.");
            // 重连
            _appClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        /// client received message
        private void _appClient_DataReceived(object sender, DataEventArgs e)
        {
            // bug: 已修复
            //string message = Encoding.Default.GetString(e.Data);
            string message = Encoding.Default.GetString(e.Data, e.Offset, e.Length);

            // todo: 解析消息
            // 解析并触发事件
            JObject jMsg = (JObject)JsonConvert.DeserializeObject(message);
            MessageReceievedEvent?.Invoke(jMsg);
        }

        /// client connected
        private void _appClient_Connected(object sender, EventArgs e)
        {
            nlogger.Trace("client connected to the server: " + e.ToString());
        }
    }
}
