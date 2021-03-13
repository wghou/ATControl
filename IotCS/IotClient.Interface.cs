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
        /// <summary>
        /// 消息的类型/主题
        /// </summary>
        public enum IotTopic : int
        {
            /// <summary> 控温表数据 </summary>
            ParamT = 0,
            /// <summary> 继电器数据 </summary>
            Relay = 1,
            /// <summary> 错误信息 </summary>
            Error,
            /// <summary> 自动控温步骤 </summary>
            DeviceState,
            /// <summary> 自动采样状态 </summary>
            SampleState,
            /// <summary> 仪器的状态数据 </summary>
            InstState,
            /// <summary> 仪器的测量值 </summary>
            InstValue,
            /// <summary> 向设备写入的指令 </summary>
            DeviceCmd
        }


        /// <summary>
        /// 用于 MQTT 通信的类
        /// </summary>
        public partial class IotPorts
        {
            /// <summary>
            /// 初始化 IotCSClient
            /// </summary>
            /// <param name="cfg"></param>
            /// <param name="topicsSub"></param>
            /// <returns></returns>
            public bool configIotPorts(JObject cfg, params IotTopic[] topicsSub)
            {
                bool confOK = true;

                try
                {
                    // 配置用户接口 - MqttCloud
                    if (cfg.ContainsKey("MqttCloud"))
                    {
                        JObject child = (JObject)cfg["MqttCloud"];

                        Enable = child.ContainsKey("Enable") ? (bool)child["Enable"] : true;
                        if (Enable)
                        {
                            confOK = _mqttCloud.Initialize(child, topicsSub);
                            _mqttCloud.MessageReceievedEvent += _mqttCloud_MessageReceievedEvent;

                            if (!confOK) nlogger.Error("配置 MqttCloud 失败");
                            else nlogger.Debug("配置 MqttCloud 成功");
                        }
                    }

                    // bug: 如果多次调用 config，就会注册多个事件处理函数
                    _mqttCloud.ClientConnectedEvent += _mqttCloud_ClientConnectedEvent;
                }
                catch (Exception ex)
                {

                    confOK = false;
                }

                return confOK;
            }

            /// <summary>
            /// 
            /// </summary>
            private void _mqttCloud_ClientConnectedEvent()
            {
                // 触发事件
                UserPortConnectedEvent?.Invoke();
            }


            /// <summary>
            /// 检查接口是否连接/启用
            /// </summary>
            /// <param name="port"></param>
            /// <returns></returns>
            public bool isConnected()
            {
                if (!Enable) return false;

                return _mqttCloud.isConnected;
            }


            /// <summary>
            /// 向特定接口发布消息
            /// </summary>
            /// <param name="topic"></param>
            /// <param name="Message"></param>
            /// <param name="isWait"></param>
            /// <param name="ports"></param>
            /// <returns></returns>
            public bool PublishMessage(IotTopic topic, JObject message, bool isWait = false)
            {
                if (!Enable) return true;

                _mqttCloud.Publish(topic, message.ToString(), isWait);
                return true;
            }


            /// receive message from the mqtt cloud
            private void _mqttCloud_MessageReceievedEvent(IotTopic topic, string message)
            {
                // 初步解析 message，并根据 d_s 标志位判断是显示类型的消息，还是设置类型的消息
                // 分别调用各自的事件函数
                JObject jMsg = (JObject)JsonConvert.DeserializeObject(message);

                IotPortReceiveMessageEvent?.Invoke(topic, jMsg);

                nlogger.Debug("收到来自 mqttCloud 的数据: SubTopic - " + topic.ToString() + "  message - " + message);
            }
        }
    }
}
