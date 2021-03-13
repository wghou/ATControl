using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ATControl.SeaBirdInst;
using NLog;

namespace ATControl.Utils
{
    public partial class UtilsDeviceStateM
    {
        /// <summary>
        /// 仪器设备 - 多个
        /// </summary>
        public List<InstDeviceBase> _instDevices = new List<InstDeviceBase>();
        /// <summary>
        /// 标准数据采集器
        /// </summary>
        public InstSTD sdDeviceRef;
        /// <summary>
        /// 仪器设备的端口
        /// </summary>
        public JArray _instPorts = new JArray();
        /// <summary>
        /// 当前标准仪器值的缓存
        /// </summary>
        private InstSTDData _stdDataCache = new InstSTDData();


        /// <summary>
        /// 初始化仪器设备 - 用户配置命令格式
        /// </summary>
        /// <param name="child"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool initInstDevices(JArray child, JObject cmd)
        {
            bool rlt = initInstDevices(child);

            if (rlt == true)
            {
                if ((cmd.ContainsKey("Enable") ? (bool)cmd["Enable"] : false) == false)
                {
                    InstDeviceBase.userDefinedCmdEnable = false;
                    return rlt;
                }

                if (cmd.ContainsKey("TS1") && cmd.ContainsKey("TS2")
                    && cmd.ContainsKey("TSR1") && cmd.ContainsKey("TSR2"))
                {
                    InstDeviceBase.userDefinedCmdEnable = true;
                    InstDeviceBase.userDefinedCmd.Clear();
                    InstDeviceBase.userDefinedCmd.Add("TS1", (string)cmd["TS1"]);
                    InstDeviceBase.userDefinedCmd.Add("TS2", (string)cmd["TS2"]);
                    InstDeviceBase.userDefinedCmd.Add("TSR1", (string)cmd["TSR1"]);
                    InstDeviceBase.userDefinedCmd.Add("TSR2", (string)cmd["TSR2"]);
                }
            }
            return rlt;
        }
        /// <summary>
        /// 初始化仪器设备
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public bool initInstDevices(JArray child)
        {
            try
            {
                // 设置仪器总数量
                int numInst = Math.Min(child.Count, InstDeviceBase.MaxInstNum);
                _instDevices.Clear();

                if (numInst < 1)
                {
                    //nlogger.Warn("There is not enough number of ports for Instrument: " + numInst.ToString());
                    return false;
                }

                // 添加标准仪器接口
                // todo: 在这个默认的 info 中，没有设置 testId
                InstInfoBase info = new InstInfoBase()
                {
                    InstType = TypeInst.Standard
                };
                sdDeviceRef = new InstSTD(info);
                _instDevices.Add(sdDeviceRef);


                // todo: 记录端口号、波特率等
                _instPorts = child;
            }
            catch (Exception ex)
            {
                //nlogger.Warn("Expection in : DeviceStateM.initInstDevices(): " + ex.Message);
                return false;
            }

            return true;
        }

        //20200401-220155
        /// <summary>
        /// 根据窗口输入参数，配置仪器
        /// </summary>
        public void configInstDeviceInternal()
        {
            var t1 = new Task(() => {
                bool confOk = true;
                foreach (var ist in _instDevices)
                {
                    if (ist.Enable == true &&
                    ist.GetBasicInfo().InstType != TypeInst.Undefined &&
                    ist.GetBasicInfo().InstType != TypeInst.Standard)
                    {
                        confOk &= (ist as InstSBE).SetupSBE37();
                    }
                }

                InstDeviceInitedEvent?.Invoke(confOk);
            });

            t1.Start();
        }

        /// <summary>
        /// 从数据库中，读取仪器信息
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        private bool getInstInfoFromSql(string testId)
        {
            // 根据 TestID，从远程数据库查找温度点信息 TestOrderSqlrd，配置 SensorDeviceBase.testOrders
            //InstDeviceBase.testOrders = sqlWriter.QueryValue<TestOrderSqlrd>(testIdSql);
            // 温度点列表不能为空
            //if (InstDeviceBase.testOrders.Count == 0)
            //{
            //    msgSend.ExecuteSucceed = false;
            //}

            _instDevices.RemoveRange(1, _instDevices.Count - 1);
            InstDeviceBase.DeviceIdxRecount(1);

            try
            {
                // 根据 TestID，从远程数据库查找标准器信息 InstSqlrd，配置 SensorSD
                List<InstSqlrd> instSql = sqlWriter.QueryValue<InstSqlrd>()
                                            .Where(q => q.vTestID.Equals(testId))
                                            .ToList();

                if (instSql.Count == 0) { return false; }

                // 在标准仪器中初始化 testID
                (_instDevices[0] as InstSTD).Info.testId = testId;

                foreach (var itm in instSql)
                {
                    // 根据 TestID 以及 InstrumentID，从远程数据库查找仪器信息 SensorSqlrd，配置 SensorSBE37
                    List<SensorSqlrd> srSql = sqlWriter.QueryValue<SensorSqlrd>()
                                                        .Where(q => q.vInstrumentID.Equals(itm.vInstrumentID))
                                                        .ToList();
                    foreach (var sr in srSql)
                    {
                        itm.sensors.Add(sr);
                    }

                    // 添加各类不同型号仪器对应的接口
                    // 根据 sql 反回的 InstrumentType，新建不同的仪器类型
                    // 只有当 _instDevices 中不存在 instrumentId 为 itm.instrumentId 的设备时，才新建
                    // 这样就有效避免了多次查询造成的 instrument 重复问题
                    //_instDevices[0].GetBasicInfo().instrumentId = "";
                    if (_instDevices.Where(q => q.GetBasicInfo().instrumentId.Equals(itm.vInstrumentID)).Count() == 0)
                    {
                        InstSBE ist = new InstSBE(itm);
                        _instDevices.Add(ist);
                    }
                }
            }
            catch (Exception ex)
            {
                //nlogger.Error("Exception: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}