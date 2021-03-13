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

namespace ATControl
{
    namespace IotCS.Client
    {
        internal class MyMqttClient
        {
            private MqttClient mqttClient = null;
            private IMqttClientOptions options = null;

            private string mqttServerUrl = "183.230.40.39";
            //private string mqttServerUrl = "192.168.1.101";
            private int mqttPort = 6002;
            private string mqttDeviceApiKey = "123123123";
            private string mqttProductId = "270595";
            private string mqttDeviceId = "542169339";
            private TimeSpan mqttTs2 = new TimeSpan(0, 1, 0);

            //
            // Topic 的构成形式为： topicDeviceId/SubTopic
            // 也就是说，每制作一台设备时，都会新建一个 DeviceId，该设备下，有两个子主题（Data和Control）
            // 所以，这个要在程序里面写死
            /// <summary>
            /// 设备序列号 - 在同一类产品中，每个单独的设备都有一个相应的编号
            /// </summary>
            private const string topicDeviceId = "lot_tst";
            /// <summary>
            /// 设备所订阅的主题
            /// </summary>
            private List<string> topicsSubs = new List<string>();


            /// <summary>
            /// 定时器 - 定时检查连接状态
            /// </summary>
            private System.Timers.Timer _timer;


            //
            public delegate void MessageReceievedEventHandler(IotTopic topic, string message);
            /// <summary>
            /// 事件 - 接收到消息
            /// </summary>
            public event MessageReceievedEventHandler MessageReceievedEvent;

            public delegate void ClientConnectedEventHandler();
            /// <summary>
            /// 事件 - Mqtt 连接成功
            /// </summary>
            public event ClientConnectedEventHandler ClientConnectedEvent;


            public MyMqttClient()
            {
                _timer = new System.Timers.Timer();
                _timer.Interval = 29000;
                _timer.AutoReset = true;
                _timer.Elapsed += _timer_Elapsed;
            }


            /// <summary>
            /// 是否启用 mqtt
            /// </summary>
            public bool Enabled { get; set; } = false;

            /// <summary>
            /// 查询 mqtt 是否连接成功
            /// </summary>
            public bool isConnected
            {
                get
                {
                    if (mqttClient == null) return false;
                    else if (!mqttClient.IsConnected)
                    {
                        try { mqttClient.ConnectAsync(options); } catch { }
                    }
                    return mqttClient.IsConnected;
                }
            }


            /// <summary>
            /// MQTT 初始化
            /// </summary>
            /// <returns></returns>
            public bool Initialize(JObject mqttConfig, params IotTopic[] topics)
            {
                // 清空原有主题
                topicsSubs.Clear();

                // 添加主题
                foreach (var itm in topics)
                {
                    topicsSubs.Add(topicDeviceId + "/" + Enum.GetName(typeof(IotTopic), itm));
                }

                if (mqttClient == null)
                {
                    try
                    {
                        mqttServerUrl = (string)mqttConfig["ServerUrl"];
                        mqttPort = (int)mqttConfig["Port"];
                        mqttDeviceApiKey = (string)mqttConfig["ApiKey"];
                        mqttProductId = (string)mqttConfig["ProductId"];
                        mqttDeviceId = (string)mqttConfig["DeviceId"];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: config iot client failed.");
                        return false;
                    }

                    // 新建 mqtt client 并连接
                    var factory = new MqttFactory();
                    mqttClient = factory.CreateMqttClient() as MqttClient;

                    options = new MqttClientOptionsBuilder()
                    .WithTcpServer(mqttServerUrl, mqttPort)
                    .WithCredentials(mqttProductId, mqttDeviceApiKey)
                    .WithClientId(mqttDeviceId)
                    .WithKeepAlivePeriod(mqttTs2)
                    .WithCleanSession(true)
                    .Build();

                    mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(new Func<MqttClientConnectedEventArgs, Task>(Connected));
                    mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(new Func<MqttClientDisconnectedEventArgs, Task>(Disconnected));
                    mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(MqttApplicationMessageReceived));

                    mqttClient.ConnectAsync(options);

                    _timer.Start();

                    Enabled = true;

                    return true;
                }
                else
                {
                    _timer.Start();
                    return false;
                }
            }


            /// <summary>
            /// 发布主题
            /// </summary>
            /// <param name="Message"></param>
            public void Publish(IotTopic topic, string Message, bool isWait = false)
            {
                if (!Enabled) return;

                try
                {
                    if (mqttClient == null) return;
                    if (mqttClient.IsConnected == false)
                        mqttClient.ConnectAsync(options);

                    if (mqttClient.IsConnected == false)
                    {
                        Console.WriteLine("Publish >>Connected Failed! ");
                        return;
                    }

                    string tp = topicDeviceId + "/" + Enum.GetName(typeof(IotTopic), topic);

                    Console.WriteLine("Publish >>Message: " + Message);
                    MqttApplicationMessageBuilder mamb = new MqttApplicationMessageBuilder()
                     .WithTopic(tp)
                     .WithPayload(Message)
                     .WithExactlyOnceQoS()
                     .WithRetainFlag(false);

                    if (isWait == true)
                        mqttClient.PublishAsync(mamb.Build()).Wait();
                    else
                        mqttClient.PublishAsync(mamb.Build());
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Publish exception >>" + exp.Message);
                }
            }


            /// <summary>
            /// 定时检查连接状态
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (!Enabled) return;

                if (mqttClient == null) return;

                if (mqttClient.IsConnected == false)
                    mqttClient.ConnectAsync(options);
            }


            /// <summary>
            /// client 接收到数据
            /// </summary>
            /// <param name="e"></param>
            private void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
            {
                if (!Enabled) return;

                try
                {
                    string text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    string tpString = e.ApplicationMessage.Topic;
                    string QoS = e.ApplicationMessage.QualityOfServiceLevel.ToString();
                    string Retained = e.ApplicationMessage.Retain.ToString();
                    Console.WriteLine("MessageReceived >>Topic:" + tpString + "; QoS: " + QoS + "; Retained: " + Retained + ";");
                    Console.WriteLine("MessageReceived >>Msg: " + text);

                    // 解析主题
                    string[] tp = tpString.Split('/');
                    if (tp[0] != topicDeviceId)
                    {
                        Console.WriteLine("Error: unknown topicDeviceId from server.");
                        return;
                    }

                    IotTopic tpE;
                    if (!Enum.TryParse(tp[1], out tpE))
                    {
                        Console.WriteLine("Error: unknown IotCSClient.Topic.type from server.");
                        return;
                    }

                    // 触发事件
                    MessageReceievedEvent?.Invoke(tpE, text);
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }


            /// <summary>
            /// client 成功连接到 server
            /// </summary>
            /// <param name="e"></param>
            /// <returns></returns>
            private async Task Connected(MqttClientConnectedEventArgs e)
            {
                try
                {
                    List<MqttTopicFilter> listTopic = new List<MqttTopicFilter>();
                    if (listTopic.Count() <= 0)
                    {
                        foreach (string topic in topicsSubs)
                        {
                            var topicFilterBulder = new MqttTopicFilterBuilder().WithTopic(topic).Build();
                            listTopic.Add(topicFilterBulder);
                            Console.WriteLine("Connected >>Subscribe " + topic);
                        }
                    }

                    // 订阅主题
                    await mqttClient.SubscribeAsync(listTopic.ToArray());
                    Console.WriteLine("Connected >>Subscribe Success");
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }

                ClientConnectedEvent?.Invoke();
            }


            /// <summary>
            /// client 与 server 断开
            /// </summary>
            /// <param name="e"></param>
            /// <returns></returns>
            private async Task Disconnected(MqttClientDisconnectedEventArgs e)
            {
                if (!Enabled) return;

                try
                {
                    Console.WriteLine("Disconnected >>Disconnected Server");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    try
                    {
                        await mqttClient.ConnectAsync(options);
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("Disconnected >>Exception " + exp.Message);
                    }
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                }
            }
        }
    }
}