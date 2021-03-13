using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATControl.SeaBirdInst
{
    public abstract partial class InstDeviceStateM<TData, TInfo> : InstDeviceBase
            where TData : InstDataBase, new()
            where TInfo : InstInfoBase, new()
    {
        public delegate void DataReceivedEventHandler(TData data);
        /// <summary>
        /// 仪器接收到数据 - 事件
        /// </summary>
        public event DataReceivedEventHandler DataReceivedEvent;
        /// <summary>
        /// 事件传递函数
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnDataReceived(TData data)
        {
            if (!Enable) return;

            DataReceivedEvent?.Invoke(data);
        }

        public delegate void StateChangedEventHandler(StateInst st);
        /// <summary>
        /// 仪器接收到数据 - 事件
        /// </summary>
        public event StateChangedEventHandler StateChangedEvent;
    }
}
