using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATControl
{
    namespace Utils
    {
        /// <summary>
        /// 通信所用的指令
        /// </summary>
        public enum SocketCmd : int
        {
            /// <summary> 开始自动控温 </summary>
            AutoStart = 0,
            /// <summary> 停止 </summary>
            Stop,
            /// <summary> 测量完成 </summary>
            Finished,
            /// <summary> 测试 Idx </summary>
            TestId,
            /// <summary> 设备状态 </summary>
            DeviceState,
            /// <summary> 传感器部件状态 </summary>
            DeviceStatus,
            /// <summary> 未知 </summary>
            Unknown
        }

        /// <summary>
        /// 用于 Socket 通信的数据格式 - 命令
        /// </summary>
        public class SocketCmdMessage
        {
            /// <summary> Socket Message 的类型 </summary>
            public SocketCmd cmdType { get; set; } = SocketCmd.Unknown;
            /// <summary> 该条指令是否正确执行 </summary>
            public bool ExecuteSucceed { set; get; } = false;

            public SocketCmdMessage(SocketCmd tp) { cmdType = tp; }
        }

        /// <summary>
        /// 用于 Socket 通信的数据格式 - TestIdx
        /// </summary>
        public class SocketTestIdxMessage : SocketCmdMessage
        {
            public SocketTestIdxMessage() : base(SocketCmd.TestId) { }

            /// <summary> 测试编号 </summary>
            public string TestIdx { get; set; }
        }

        /// <summary>
        /// 用于 Socket 通信的数据格式 - 设备状态
        /// </summary>
        public class SocketStateMessage : SocketCmdMessage
        {
            public SocketStateMessage() : base(SocketCmd.DeviceState) { }

            /// <summary> 设备状态 </summary>
            public int devSt { set; get; } = -1;
            /// <summary> 是否发生了错误：true 表示发生了错误 </summary>
            public bool ErrorSt { set; get; } = false;
        }

        public class SocketStatusMessage : SocketCmdMessage
        {
            public SocketStatusMessage() : base(SocketCmd.DeviceStatus) { }
            public int Kongwenzhuangtai = -1; // 当前控温状态
            public float ZhucaoTemperature = 35.0000f;//主槽设定温度
            public float Bodongdu = 0.0f; // 波动度
            public float ZhucaoTemperatureReal = 35.0000f;//主槽当前温度
            public int ZhucaoPower = 0; //主槽加热功率
            public float FucaoTemperature = 34.0000f; //辅槽设定温度
            public float FucaoTemperatureReal = 34.0000f; //辅槽当前温度
            public int FucaoPower = 0; //辅槽加热功率
            public bool Zongdianyuan = false; //总电源开关
            public bool Zhucaozhileng = false; //主槽制冷开关
            public bool Zhucaokongwen = false; //主槽控温开关
            public bool Fucaokongwen = false; //辅槽控温开关
            public bool Fucaozhileng = false; //辅槽制冷开关
            public bool Fucaoxunhuan = false; //辅槽循环开关
            public bool Fucaokuaileng = false; //辅槽快冷开关
            public bool Zhucaokuaileng = false; //主槽快冷开关
                                                //
            public bool Quyangbeng = false; // 取样泵
            public bool Quyangdiancifa = false; // 取样电磁阀
            public bool Chuyebeng = false; // 除液泵
            public bool Chuyediancifa = false; // 除液电磁阀
            public DateTime DataTime = DateTime.Now;//数据采集时间
            public double Temperature = double.NaN;//标准温度
            public double Conductivity = double.NaN;//标准电导率
            public double Salinity = double.NaN;//标准盐度
        }
    }
}
