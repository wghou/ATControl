using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ATControl.Utils
{
    public abstract class UtilsDeviceStateMBase
    {
        /// <summary>
        /// 初始化仪器
        /// </summary>
        /// <param name="child"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public abstract bool initInstDevices(JArray child, JObject cmd);
        /// <summary>
        /// 初始化仪器
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public abstract bool initInstDevices(JArray child);
        /// <summary>
        /// 开始配置仪器
        /// </summary>
        public abstract void configInstDeviceInternal();

        /// <summary>
        /// 初始化 socket
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public abstract bool InitSocketServer(JObject cfg);
        /// <summary>
        /// 结束 socket
        /// </summary>
        /// <returns></returns>
        public abstract bool socketSendFinished();

        /// <summary>
        /// 开始仪器设备的测量
        /// </summary>
        /// <returns></returns>
        public abstract bool StartDeviceMeasure(float tp);
        /// <summary>
        /// 开始仪器数据的存储
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public abstract bool StartDeviceStore();
    }
}
