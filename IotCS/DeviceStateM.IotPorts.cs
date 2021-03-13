using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ATControl.IotCS.Client;
using ATControl.SeaBirdInst;

namespace ATControl
{
    namespace Utils
    {
        public partial class UtilsDeviceStateM
        {
            /// <summary>
            /// 用户接口
            /// </summary>
            private IotPorts _userPorts = new IotPorts();

            bool InitIotPort(JObject child)
            {
                IotTopic[] tpSub = new IotTopic[] { IotTopic.DeviceCmd };
                bool confOK = _userPorts.configIotPorts(child, tpSub);

                _userPorts.IotPortReceiveMessageEvent += _userPorts_UserPortMsgRvSetEvent;
                TimerTickEndEvent += DeviceStateM_TimerTickEndEvent;
                StateChangedEvent += DeviceStateM_StateChangedEvent;
                RelayDeviceMStatusUpdatedEvent += DeviceStateM_RelayDeviceStatusUpdatedEvent;
                RelayDeviceSStatusUpdatedEvent += DeviceStateM_RelayDeviceStatusUpdatedEvent;
                ErrorStatusChangedEvent += DeviceStateM_ErrorStatusChangedEvent;
                DeviceClosedEvent += DeviceStateM_DeviceClosedEvent;
                SampleStateChangedEvent += DeviceStateM_SampleStateChangedEvent;

                return confOK;
            }

            // 当自动采样状态发生改变时，发布
            private void DeviceStateM_SampleStateChangedEvent(AutoSample.StateSample st)
            {
                iotPublishMessage(IotTopic.SampleState);
            }

            // 当设备关闭时，发布继电器、自动控温状态
            private void DeviceStateM_DeviceClosedEvent()
            {
                iotPublishMessage(IotTopic.Relay);
                iotPublishMessage(IotTopic.DeviceState);
                iotPublishMessage(IotTopic.SampleState);
                iotPublishMessage(IotTopic.InstState);
            }

            // 当错误状态改变时，发布错误信息
            private void DeviceStateM_ErrorStatusChangedEvent(Dictionary<ErrorCode, uint> errDict)
            {
                iotPublishMessage(IotTopic.Error);
            }

            // 当继电器状态改变时，发布
            private void DeviceStateM_RelayDeviceStatusUpdatedEvent(RelayDevice.Err_r err, bool[] ryStatus)
            {
                iotPublishMessage(IotTopic.Relay);
            }

            // 当自动控温状态改变时，发布
            private void DeviceStateM_StateChangedEvent(State st)
            {
                iotPublishMessage(IotTopic.DeviceState);
            }

            // 定时器事件，定时发布温度等数据
            private void DeviceStateM_TimerTickEndEvent()
            {
                iotPublishMessage(IotTopic.ParamT);
            }

            // 查看 iot 连接是否正确
            // 如果发现断开，则尝试重启服
            public bool isUserPortConnected
            {
                // todo: 如果服务器停止工作，则如何？

                get { return _userPorts.isConnected(); }
            }

            /// <summary>
            /// 从用户接口收到数据
            /// </summary>
            /// <param name="Ptype"></param>
            /// <param name="topic"></param>
            /// <param name="message"></param>
            private void _userPorts_UserPortMsgRvSetEvent(IotTopic topic, JObject message)
            {
                nlogger.Trace("the message topic is " + topic.ToString() + ", and the message is: " + message);

                switch (topic)
                {
                    case IotTopic.ParamT:
                        nlogger.Trace("Receive message from mqtt with topic Topic.ParamT");
                        break;

                    case IotTopic.Relay:
                        nlogger.Trace("Receive message from mqtt with topic Topic.Relay");
                        break;

                    case IotTopic.DeviceCmd:
                        iotPublishMessage(IotTopic.Relay);
                        iotPublishMessage(IotTopic.DeviceState);
                        iotPublishMessage(IotTopic.Error);
                        iotPublishMessage(IotTopic.SampleState);
                        iotPublishMessage(IotTopic.InstState);
                        break;

                    default:
                        Console.WriteLine("Unknown message receieved.");
                        break;
                }
            }

            public bool iotPublishMessage(IotTopic tp)
            {
                switch (tp)
                {
                    case IotTopic.ParamT:
                        IotParamTMessage jPt = new IotParamTMessage();
                        jPt.DorS = IotDorS.Display;
                        jPt.paramM = new ParamT();
                        jPt.paramS = new ParamT();
                        jPt.Topic = IotTopic.ParamT;
                        float[] pM = new float[10];
                        tpDeviceM.tpParam.CopyTo(pM, 0);
                        jPt.paramM.setValue(pM);
                        jPt.paramM.TempShow = tpDeviceM.temperatures.Count > 0 ? tpDeviceM.temperatures.Last() : 0.0f;
                        jPt.paramM.PowerShow = tpDeviceM.tpPowerShow;
                        // 波动度
                        float fluc = 0.0f;
                        tpDeviceM.GetFlucDurCountOrLess(_runningParameters.steadyTimeSec / _runningParameters.readTempIntervalSec, out fluc);
                        jPt.paramM.Fluc = fluc;

                        float[] pS = new float[10];
                        tpDeviceS.tpParam.CopyTo(pS, 0);
                        jPt.paramS.setValue(pS);
                        jPt.paramS.TempShow = tpDeviceS.temperatures.Count > 0 ? tpDeviceS.temperatures.Last() : 0.0f;
                        jPt.paramS.PowerShow = tpDeviceS.tpPowerShow;
                        // 波动度
                        tpDeviceS.GetFlucDurCountOrLess(_runningParameters.steadyTimeSec / _runningParameters.readTempIntervalSec, out fluc);
                        jPt.paramS.Fluc = fluc;

                        _userPorts.PublishMessage(IotTopic.ParamT, JObject.FromObject(jPt));
                        break;

                    case IotTopic.Relay:
                        IotRelay88Message jR = new IotRelay88Message();
                        jR.DorS = IotDorS.Display;
                        jR.relayM = new Relay8();
                        jR.relayS = new Relay8();
                        jR.Topic = IotTopic.Relay;
                        jR.relayM.setValue(ryDeviceM.ryStatus);
                        jR.relayS.setValue(ryDeviceS.ryStatus);

                        _userPorts.PublishMessage(IotTopic.Relay, JObject.FromObject(jR));
                        break;

                    case IotTopic.DeviceState:
                        IotDeviceStateMessage jSt = new IotDeviceStateMessage();
                        jSt.DorS = IotDorS.Display;
                        jSt.Topic = IotTopic.DeviceState;
                        jSt.state = _machine.State;

                        _userPorts.PublishMessage(IotTopic.DeviceState, JObject.FromObject(jSt));
                        break;

                    case IotTopic.Error:
                        IotErrorMessage jEr = new IotErrorMessage();
                        jEr.DorS = IotDorS.Display;
                        jEr.Topic = IotTopic.Error;
                        jEr.errCnt = _deviceCurrentError;

                        _userPorts.PublishMessage(IotTopic.Error, JObject.FromObject(jEr));
                        break;

                    case IotTopic.SampleState:
                        IotSampleStateMessage jSample = new IotSampleStateMessage();
                        jSample.DorS = IotDorS.Display;
                        jSample.Topic = IotTopic.SampleState;
                        jSample.state = _sampleMachine.State;

                        _userPorts.PublishMessage(IotTopic.SampleState, JObject.FromObject(jSample));
                        break;

                    case IotTopic.InstState:
                        IotInstStateMessage jInstState = new IotInstStateMessage();
                        jInstState.DorS = IotDorS.Display;
                        jInstState.Topic = IotTopic.InstState;
                        jInstState.InstInfos = new List<InstInfoBase>();
                        foreach (var itm in _instDevices)
                        {
                            jInstState.InstInfos.Add(itm.GetBasicInfo());
                        }

                        _userPorts.PublishMessage(IotTopic.InstState, JObject.FromObject(jInstState));
                        break;

                    case IotTopic.InstValue:
                        // todo: add the sensor value
                        nlogger.Error("code error");
                        break;

                    default:
                        break;
                }
                return true;
            }
        }
    }
}