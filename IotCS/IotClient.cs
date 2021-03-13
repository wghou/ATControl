using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Connecting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace ATControl
{
    namespace IotCS.Client
    {
        public partial class IotPorts
        {
            private static readonly Logger nlogger = LogManager.GetCurrentClassLogger();

            /// <summary>
            /// 用于连接移动 OneNet 云平台的 mqtt 客户端 / 也可以在本地搭建 mqtt server
            /// </summary>
            private MyMqttClient _mqttCloud = new MyMqttClient();
            /// <summary>
            /// 启用？
            /// </summary>
            public bool Enable { set; get; } = false;
        }
    }
}