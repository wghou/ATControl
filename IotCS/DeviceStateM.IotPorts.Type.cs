using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATControl.IotCS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ATControl.SeaBirdInst;

namespace ATControl
{
    namespace Utils
    {
        /// <summary>
        /// 用于区别消息中的数据是用于显示，还是设置
        /// </summary>
        public enum IotDorS : int
        {
            /// <summary>  数据用于显示 </summary>
            Display = 0,
            /// <summary>  数据用于设置 </summary>
            Set = 1
        }

        /// <summary>
        /// 用于 Iot/MQTT 通信的数据格式 - 基类 - json convertor
        /// </summary>
        public abstract class IotMessageBase
        {
            /// <summary> 消息的主题 </summary>
            public IotTopic Topic { set; get; }

            /// <summary> 用于显示 or 设置 </summary>
            public IotDorS DorS { set; get; }

            public IotMessageBase(IotTopic tp) { Topic = tp; }
        }



        /// <summary>
        /// 控温表数据
        /// </summary>
        public class ParamT
        {
            /// <summary>
            /// 温控设备的参数值
            /// </summary>
            private float[] param = new float[10];

            /// <summary>温度设定值</summary>
            public float TemptSet { get { return param[0]; } set { param[0] = value; } }
            /// <summary>温度调整值</summary>
            public float TempCorrect { get { return param[1]; } set { param[1] = value; } }
            /// <summary>
            /// 超前调整值
            /// </summary>
            public float LeadAdjust { get { return param[2]; } set { param[2] = value; } }
            /// <summary>
            /// 模糊系数
            /// </summary>
            public float Fuzzy { get { return param[3]; } set { param[3] = value; } }
            /// <summary>
            /// 比例系数
            /// </summary>
            public float Ratio { get { return param[4]; } set { param[4] = value; } }
            /// <summary>
            /// 积分系数
            /// </summary>
            public float Integration { get { return param[5]; } set { param[5] = value; } }
            /// <summary>
            /// 功率系数
            /// </summary>
            public float Power { get { return param[6]; } set { param[6] = value; } }
            /// <summary>
            /// 当前温度值
            /// </summary>
            public float TempShow { get { return param[7]; } set { param[7] = value; } }
            /// <summary>
            /// 当前功率值
            /// </summary>
            public float PowerShow { get { return param[8]; } set { param[8] = value; } }
            /// <summary>
            /// 五分钟波动度
            /// </summary>
            public float Fluc { get { return param[9]; } set { param[9] = value; } }

            public float[] getValue() { return param; }
            public bool setValue(float[] val)
            {
                try
                {
                    val.CopyTo(param, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 用于传输温控表数据的 Json 字符串类
        /// </summary>
        public class IotParamTMessage : IotMessageBase
        {
            public IotParamTMessage() : base(IotTopic.ParamT) { }

            /// <summary>
            /// 主槽控温表数据
            /// </summary>
            public ParamT paramM { set; get; }

            /// <summary>
            /// 辅槽控温表数据
            /// </summary>
            public ParamT paramS { set; get; }
        }


        /// <summary>
        /// 十六个继电器的类
        /// </summary>
        public class Relay16
        {
            private bool[] st = new bool[16];

            /// <summary> 继电器 0 </summary>
            public bool relay_0 { get { return st[0]; } set { st[0] = value; } }

            /// <summary> 继电器 1 </summary>
            public bool relay_1 { get { return st[1]; } set { st[1] = value; } }

            /// <summary> 继电器 2 </summary>
            public bool relay_2 { get { return st[2]; } set { st[2] = value; } }

            /// <summary> 继电器 3 </summary>
            public bool relay_3 { get { return st[3]; } set { st[3] = value; } }

            /// <summary> 继电器 4 </summary>
            public bool relay_4 { get { return st[4]; } set { st[4] = value; } }

            /// <summary> 继电器 5 </summary>
            public bool relay_5 { get { return st[5]; } set { st[5] = value; } }

            /// <summary> 继电器 6 </summary>
            public bool relay_6 { get { return st[6]; } set { st[6] = value; } }

            /// <summary> 继电器 7 </summary>
            public bool relay_7 { get { return st[7]; } set { st[7] = value; } }

            /// <summary> 继电器 8 </summary>
            public bool relay_8 { get { return st[8]; } set { st[8] = value; } }

            /// <summary> 继电器 9 </summary>
            public bool relay_9 { get { return st[9]; } set { st[9] = value; } }

            /// <summary> 继电器 10 </summary>
            public bool relay_10 { get { return st[10]; } set { st[10] = value; } }

            /// <summary> 继电器 11 </summary>
            public bool relay_11 { get { return st[11]; } set { st[11] = value; } }

            /// <summary> 继电器 12 </summary>
            public bool relay_12 { get { return st[12]; } set { st[12] = value; } }

            /// <summary> 继电器 13 </summary>
            public bool relay_13 { get { return st[13]; } set { st[13] = value; } }

            /// <summary> 继电器 14 </summary>
            public bool relay_14 { get { return st[14]; } set { st[14] = value; } }

            /// <summary> 继电器 15 </summary>
            public bool relay_15 { get { return st[15]; } set { st[15] = value; } }

            public bool[] getValue() { return st; }
            public bool setValue(bool[] val)
            {
                try
                {
                    val.CopyTo(st, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }

                return true;
            }
        }


        /// <summary>
        /// 八个继电器的类
        /// </summary>
        public class Relay8
        {
            private bool[] st = new bool[8];

            /// <summary> 继电器 0 </summary>
            public bool relay_0 { get { return st[0]; } set { st[0] = value; } }

            /// <summary> 继电器 1 </summary>
            public bool relay_1 { get { return st[1]; } set { st[1] = value; } }

            /// <summary> 继电器 2 </summary>
            public bool relay_2 { get { return st[2]; } set { st[2] = value; } }

            /// <summary> 继电器 3 </summary>
            public bool relay_3 { get { return st[3]; } set { st[3] = value; } }

            /// <summary> 继电器 4 </summary>
            public bool relay_4 { get { return st[4]; } set { st[4] = value; } }

            /// <summary> 继电器 5 </summary>
            public bool relay_5 { get { return st[5]; } set { st[5] = value; } }

            /// <summary> 继电器 6 </summary>
            public bool relay_6 { get { return st[6]; } set { st[6] = value; } }

            /// <summary> 继电器 7 </summary>
            public bool relay_7 { get { return st[7]; } set { st[7] = value; } }

            public bool[] getValue() { return st; }
            public bool setValue(bool[] val)
            {
                try
                {
                    Array.Copy(val, st, 8);
                    //val.CopyTo(st, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }

                return true;
            }
        }

        // <summary>
        /// 用于传输继电器数据的 Json 字符串类
        /// </summary>
        public class IotRelay88Message : IotMessageBase
        {
            public IotRelay88Message() : base(IotTopic.Relay) { }

            /// <summary>
            /// 主继电器板卡
            /// </summary>
            public Relay8 relayM { set; get; }

            /// <summary>
            /// 辅继电器板卡
            /// </summary>
            public Relay8 relayS { set; get; }
        }

        /// <summary>
        /// 用于传输控温状态数据的 Json 字符串类
        /// </summary>
        public class IotDeviceStateMessage : IotMessageBase
        {
            public IotDeviceStateMessage() : base(IotTopic.DeviceState) { }

            /// <summary>
            /// 当前自动控温状态
            /// </summary>
            public int state { set; get; }
        }

        /// <summary>
        /// 用于传输错误状态数据的 Json 字符串类
        /// </summary>
        public class IotErrorMessage : IotMessageBase
        {
            public IotErrorMessage() : base(IotTopic.Error) { }

            /// <summary>
            /// 当前的错误状态
            /// </summary>
            public Dictionary<ErrorCode, uint> errCnt { set; get; }
        }


        /// <summary>
        /// 用于传输自动采样状态数据的 Json 字符串类
        /// </summary>
        public class IotSampleStateMessage : IotMessageBase
        {
            public IotSampleStateMessage() : base(IotTopic.SampleState) { }

            /// <summary>
            /// 当前自动采样状态
            /// </summary>
            public AutoSample.StateSample state { set; get; }
        }


        /// <summary>
        /// 向设备写入的指令
        /// </summary>
        public enum Cmd2Device : int
        {
            None = 0,
            /// <summary> 向设备请求发送状态 </summary>
            StateRqt
        }

        /// <summary>
        /// 用于向 Device 写入指令的 Json 字符串类
        /// </summary>
        public class IotCmdMessage : IotMessageBase
        {
            public IotCmdMessage() : base(IotTopic.DeviceCmd) { }

            /// <summary>
            /// 向设备写入的指令
            /// </summary>
            public Cmd2Device cmd { set; get; } = Cmd2Device.None;
        }


        /// <summary>
        /// 用于传输仪器数据的 Json 字符串类
        /// </summary>
        public class IotInstStateMessage : IotMessageBase
        {
            public IotInstStateMessage() : base(IotTopic.InstState) { }

            /// <summary>
            /// 所有仪器的数据及状态
            /// </summary>
            public List<InstInfoBase> InstInfos { get; set; }
        }

        /// <summary>
        /// 用于传输仪器数据的 Json 字符串类
        /// </summary>
        public class IotInstValueMessage : IotMessageBase
        {
            public IotInstValueMessage() : base(IotTopic.InstValue) { }

            // todo:
            // add the Instrumen value
            public InstDataShow InstData { set; get; }
        }


        /// <summary>
        /// 用于 InstDataBase 派生类的解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class JsonCreationConverter<T> : JsonConverter
        {
            protected abstract T Create(Type objectType, JObject jsonObject);
            public override bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jsonObject = JObject.Load(reader);
                var target = Create(objectType, jsonObject);
                serializer.Populate(jsonObject.CreateReader(), target);
                return target;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 用于 SensorDataBase 派生类的解析
        /// </summary>
        public class JsonInstDataConverter : JsonCreationConverter<InstDataBase>
        {
            protected override InstDataBase Create(Type objectType, JObject jsonObject)
            {
                var typeName = jsonObject["InstType"].ToObject<TypeInst>();
                switch (typeName)
                {
                    case TypeInst.Standard:
                        return new InstSTDData();
                    case TypeInst.SBE37SI:
                        return new InstSBE37Data();
                    default: return new InstUDFData();
                }
            }
        }
    }
}

