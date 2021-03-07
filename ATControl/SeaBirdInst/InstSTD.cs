using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using NLog;

namespace ATControl
{
    namespace SeaBirdInst
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed partial class InstSTD : InstDeviceStateM<InstSTDData, InstInfoBase>
        {
            public InstSTD(InstInfoBase info) : base(info)
            {
                Info.InstType = TypeInst.Standard;

                // 配置仪器相关状态机
                ConfigInstStateless();
            }

            /// <summary>
            /// 检测标准温度仪器的波动度
            /// </summary>
            /// <param name="cnt"></param>
            /// <param name="crt"></param>
            /// <returns></returns>
            public bool CheckFluc(int cnt, double crt)
            {
                if (!Enable) return false;

                double fluc = 0;
                if (_instData.Count == 0 || _instData.Count < cnt)
                {
                    // If there is not temperature data in list, output extreme fluctuation

                    return false;
                }
                else
                {
                    fluc = _instData.GetRange(_instData.Count - cnt, cnt).Max().vStandardT -
                        _instData.GetRange(_instData.Count - cnt, cnt).Min().vStandardT;

                    if (fluc < crt) { return true; }
                    else { return false; }
                }
            }

            /// <summary>
            /// 执行 Store 步骤
            /// </summary>
            protected override void internalStoreStep()
            {
                // 设备未启用
                if (Enable == false) return;

                // todo: 去掉所采集数据中跳变的成分


                // 执行基类中的 internalStoreStep()
                base.internalStoreStep();
            }

            /// <summary>
            /// 由收到的字符串解析为指令。
            /// 其实就是检查 <Executed/>
            /// </summary>
            /// <param name="str"> 串口接收到的字符串 </param>
            /// <returns> 是否成功解析为指令 </returns>
            protected override bool ResolveStr2Cmd(string str)
            {
                return false;
            }
            /// <summary>
            /// 由收到的字符串解析为数据。
            /// 根据不同的传感器/配置，进行数据的解析
            /// </summary>
            /// <param name="str"> 串口接收到的字符串 </param>
            /// <param name="dt"> 解析得到的数据，为空表示 暂时未解析成一组数据 </param>
            /// <returns> 是否成功解析为数据 </returns>
            protected override bool ResolveStr2Data(string str, out InstSTDData data)
            {
                data = new InstSTDData();

                try
                {
                    // 日期（yyyy_MM_dd HH:mm: ss）-电导率频率 - 标准电导率示值 - 温度频率 - 标准温度示值 - 盐度 - 标志数
                    string[] valStrs = str.Split(',');

                    data.vTestID = Info.testId;
                    data.InstIdx = Info.InstIdx_NotUsed;
                    data.InstType = Info.InstType;
                    data.vTitularValue = currentTemptPoint;
                    data.vStandardC = double.Parse(valStrs[2]);
                    data.vStandardT = double.Parse(valStrs[4]);
                    data.vStandardS = double.Parse(valStrs[5]);
                    data.measureTime = DateTime.ParseExact(valStrs[0], "yyyy:MM:dd:HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    data.addTime = data.measureTime;
                    data.updateTime = data.measureTime;
                }
                catch (Exception ex)
                {
                    nlogger.Error("error in InstSTD.ResolveStr2Data");
                    return false;
                }

                return true;
            }
        }
    }
}
