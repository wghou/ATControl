using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Server.Status;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Connecting;
using NLog;

namespace ATControl
{
    namespace IotCS.Service
    {
        public partial class IotService : ServiceBase
        {
            private MqttServer mqttServer = null;
            private IMqttServerOptions options = null;
            private int mqttPort = 5001;
            private IPAddress mqttServerUrl = new IPAddress(new byte[] { 192, 168, 1, 101 });
            private string mqttUsername = "admin";
            private string mqttPassword = "public";

            private static readonly Logger nlogger = LogManager.GetCurrentClassLogger();


            public IotService()
            {
                InitializeComponent();
            }

            /// <summary>
            /// IotService OnStart
            /// </summary>
            /// <param name="args"></param>
            protected override void OnStart(string[] args)
            {
                // todo: 引入记录服务的 nlogger file
                nlogger.Trace("IotService OnStart..");
                try
                {
                    // get local ip address
                    foreach (IPAddress _ipaddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    {
                        if (_ipaddress.AddressFamily.ToString() == "InterNetwork")
                        {
                            nlogger.Debug("get the local ip address: " + _ipaddress.ToString());
                            mqttServerUrl = _ipaddress;
                        }
                    }

                    if (mqttServer == null)
                    {
                        nlogger.Trace("create new mqttServer. IP:" + mqttServerUrl.ToString() + " port:" + mqttPort.ToString());

                        var factory = new MqttFactory();
                        mqttServer = factory.CreateMqttServer() as MqttServer;

                        options = new MqttServerOptionsBuilder()
                            .WithConnectionBacklog(1000)
                            .WithDefaultEndpointPort(mqttPort)
                            .WithDefaultEndpointBoundIPAddress(mqttServerUrl)
                            .Build();

                        (options as MqttServerOptions).ConnectionValidator = new MqttServerConnectionValidatorDelegate(new Func<MqttConnectionValidatorContext, Task>(ConnectionValidation));

                        mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(new Func<MqttServerClientConnectedEventArgs, Task>(Connected));
                        mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(new Func<MqttServerClientDisconnectedEventArgs, Task>(DisConnected));
                        mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(MqttApplicationMessageReceived));
                        mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(new Func<MqttServerClientSubscribedTopicEventArgs, Task>(SubscribeTopic));
                        mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(new Func<MqttServerClientUnsubscribedTopicEventArgs, Task>(UnSubscribeTopic));
                        mqttServer.StartedHandler = new MqttServerStartedHandlerDelegate(new Action<EventArgs>(Started));
                        mqttServer.StoppedHandler = new MqttServerStoppedHandlerDelegate(new Action<EventArgs>(Stopped));

                    }

                    // 启动 Mqtt 服务
                    mqttServer.StartAsync(options);
                }
                catch (Exception ex)
                {
                    nlogger.Error("Exception occur in OnStart, stop this IotService.");
                    this.Stop();
                }
            }

            /// <summary>
            /// IotService OnStop
            /// </summary>
            protected override void OnStop()
            {
                nlogger.Trace("IotService OnStop..");

                mqttServer.StopAsync();
            }


            private async Task ConnectionValidation(MqttConnectionValidatorContext context)
            {
                nlogger.Debug("connection validation.");

                if (context.ClientId.Length < 10)
                {
                    nlogger.Error("ClientId.Length < 10");

                    context.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    return;
                }
                if (!context.Username.Equals(mqttUsername))
                {
                    nlogger.Error("Username is not admin");

                    context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }
                if (!context.Password.Equals(mqttPassword))
                {
                    nlogger.Error("Password is not public");

                    context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    return;
                }
                nlogger.Debug("connection validation success.");

                context.ReasonCode = MqttConnectReasonCode.Success;
            }

            private async Task Connected(MqttServerClientConnectedEventArgs arg)
            {
                nlogger.Trace("client connected. cientID: " + arg.ClientId);
            }

            private async Task DisConnected(MqttServerClientDisconnectedEventArgs arg)
            {
                nlogger.Trace("client disconnected. cientID: " + arg.ClientId);
            }

            private void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs message)
            {
                nlogger.Debug("server receive message from cientID: " + message.ClientId);
                // process the message
            }

            private async Task SubscribeTopic(MqttServerClientSubscribedTopicEventArgs topicArg)
            {
                nlogger.Debug("topic subscribed." + topicArg.TopicFilter.Topic);
            }

            private async Task UnSubscribeTopic(MqttServerClientUnsubscribedTopicEventArgs topicArg)
            {
                nlogger.Debug("topic unSubscribed." + topicArg.TopicFilter.GetType());
            }

            private void Started(EventArgs e)
            {
                nlogger.Trace("mqqt started. ");
            }

            private void Stopped(EventArgs e)
            {
                nlogger.Trace("mqqt stopped. ");
            }
        }
    }
}