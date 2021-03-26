using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ATControl.Utils
{
    public partial class UtilsDeviceStateM
    {
        //
        public delegate bool SocketReceiveMessageEventHandler(JObject message);
        /// <summary>
        /// Socket 接收到消息 - 事件
        /// </summary>
        public event SocketReceiveMessageEventHandler SocketReceiveMessageEvent;
        //
        public delegate void InstDeviceInitedEventHandler(bool ok);
        /// <summary>
        /// 仪器设备是否配置成功 - 事件
        /// </summary>
        public event InstDeviceInitedEventHandler InstDeviceInitedEvent;

        //
        public delegate void DeviceClosedEventHandler();
        /// <summary>
        /// 设备关闭触发事件
        /// </summary>
        public event DeviceClosedEventHandler DeviceClosedEvent;

        //
        public delegate void SensorSDReceiveDataEventHandler(ATControl.SeaBirdInst.InstSTDData data);
        /// <summary>
        /// 标准器设备接收到消息事件
        /// </summary>
        public event SensorSDReceiveDataEventHandler InstSDReceiveDataEvent;
    }
}
