using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using ATControl.SeaBirdInst;
using ATControl.Socket;
using ATControl.DataBase;

namespace ATControl
{
    namespace Utils
    {
        public partial class UtilsDeviceStateM
        {
            private MySocketServer _socketServer = new MySocketServer();
            /// <summary>
            /// 写入数据 sql
            /// </summary>
            protected readonly MySqlWriter sqlWriter = new MySqlWriter();
            /// <summary>
            /// 测试的 testID
            /// </summary>
            private string testIdSql = null;

            /// <summary>
            /// 初始化网络端口
            /// </summary>
            /// <param name="cfg"></param>
            /// <returns></returns>
            public override bool InitSocketServer(JObject cfg)
            {
                bool rt = _socketServer.Init(cfg);
                if(rt != false)
                {
                    _socketServer.MessageReceievedEvent += _socketServer_MessageReceievedEvent;
                }
                return rt;
            }

            /// <summary>
            /// Socket 接收到消息 - 事件处理函数
            /// </summary>
            /// <param name="message"></param>
            private void _socketServer_MessageReceievedEvent(JObject message)
            {
                // 解析收到的指令
                SocketCmdMessage msg = message.ToObject<SocketCmdMessage>();

                // todo: 如何处理错误情况

                switch (msg.cmdType)
                {
                    // 开始控温流程
                    case SocketCmd.AutoStart:
                        msg.ExecuteSucceed = SocketReceiveMessageEvent.Invoke(message);
                        _socketServer.pushMessage(JObject.FromObject(msg));
                        break;

                    // 停止控温流程
                    case SocketCmd.Stop:
                        msg.ExecuteSucceed = SocketReceiveMessageEvent.Invoke(message);
                        _socketServer.pushMessage(JObject.FromObject(msg));
                        break;

                    // 读取仪器信息
                    case SocketCmd.TestId:
                        // 接收到 testID
                        SocketTestIdxMessage msgSend1 = message.ToObject<SocketTestIdxMessage>();
                        bool rlt = getInstInfoFromSql(msgSend1.TestIdx);
                        msgSend1.ExecuteSucceed = rlt;
                        _socketServer.pushMessage(JObject.FromObject(msgSend1));
                        break;

                    case SocketCmd.DeviceState:
                        // 返回收到的指令
                        SocketStateMessage msgSend2 = message.ToObject<SocketStateMessage>();
                        SocketReceiveMessageEvent.Invoke(message);
                        break;

                    case SocketCmd.DeviceStatus:
                        msg.ExecuteSucceed = SocketReceiveMessageEvent.Invoke(message);
                        break;

                    default:
                        //nlogger.Error("unknow socket cmd: " + msg.cmdType.ToString());
                        break;
                }
            }

            /// <summary>
            /// 通过 socket 发送 finished 指令
            /// </summary>
            /// <returns></returns>
            public override bool socketSendFinished()
            {
                // 停止仪器工作
                foreach (var itm in _instDevices)
                {
                    itm.DisableInstDevice();
                }

                // 解析收到的指令
                SocketCmdMessage msg = new SocketCmdMessage(SocketCmd.Finished);
                msg.ExecuteSucceed = true;
                _socketServer.pushMessage(JObject.FromObject(msg));
                return true;
            }
        }
    }
}