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
            private bool InitSocketServer(JObject cfg)
            {
                bool rt = _socketServer.Init(cfg);
                return rt;
            }

            /// <summary>
            /// 通过 socket 发送 finished 指令
            /// </summary>
            /// <returns></returns>
            public bool socketSendFinished()
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