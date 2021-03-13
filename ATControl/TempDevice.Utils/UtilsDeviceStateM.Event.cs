using System;
using System.Collections.Generic;
using System.Text;

namespace ATControl.Utils
{
    public partial class UtilsDeviceStateM
    {
        //
        public delegate void InstDeviceInitedEventHandler(bool ok);
        /// <summary>
        /// 仪器设备是否配置成功
        /// </summary>
        public event InstDeviceInitedEventHandler InstDeviceInitedEvent;
    }
}
