using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace ATControl
{
    namespace SeaBirdInst
    {
        /// <summary>
        /// SBE 37 系列仪器
        /// </summary>
        public sealed partial class InstSBE : InstDeviceStateM<InstSBE37Data, InstSqlrd>
        {
            /// <summary>
            /// 仪器的采样方式 / 数据读取方式
            /// </summary>
            private InstSampleMode sampleMode = InstSampleMode.AutoSample_Fmt1;
            /// <summary>
            /// 当前发送给仪器的指令
            /// </summary>
            private SBE37Cmd currentCmd = SBE37Cmd.NoneCmd;
            /// <summary>
            /// 当前发送的指令，是否已返回结果
            /// </summary>
            private bool CmdExecuted = true;
            /// <summary>
            /// 指令执行完成返回标志位
            /// </summary>
            private string OutputFlag = "<Executed/>";
            /// <summary>
            /// 仪器读取数据的返回格式（默认为 format_1）
            /// </summary>
            private SBE37OutputFormat outputFormat = SBE37OutputFormat.Format_1;
            /// <summary>
            /// Configuration Data in Xml format
            /// </summary>
            private string ConfigDataXml = "";
            /// <summary>
            /// Configuration Data
            /// </summary>
            private SBE37ConfigData configData = new SBE37ConfigData();
            /// <summary>
            /// calibration coefficients in Xml format
            /// </summary>
            private string CalibCoeffXml = "";
            /// <summary>
            /// calibration coefficients
            /// </summary>
            private SBE37CalibCoeff calibCCoeff = new SBE37CalibCoeff();


            public InstSBE(InstSqlrd info) : base(info)
            {
                this.Info.FreshFromSql2Info();
                // 配置仪器相关状态机
                ConfigInstStateless();

                // todo: 
                _tickTimerInst = new System.Timers.Timer();
                _tickTimerInst.Interval = sampleIntervalSec * 500;
                _tickTimerInst.AutoReset = true;
                _tickTimerInst.Elapsed += _tickTimerSample_Elapsed;
                //_tickTimerInst.Start(); 
            }


            /// <summary>
            /// 配置当前的仪器
            /// </summary>
            /// <param name="mode"></param>
            /// <returns></returns>
            public bool SetupSBE37(InstSampleMode mode = InstSampleMode.PolledSample_Fmt10)
            {
                Enable = true;

                _tickTimerInst.Interval = sampleIntervalSec * 500;
                //nlogger.Error("wghou sampleIntervalSec: " + sampleIntervalSec.ToString());
                //_tickTimerInst.Interval = 10000;

                // 读取 calibration coefficients
                //currentCmd = SBE37Cmd.GetCC;
                //CmdExecuted = false;
                //sendCMD("GetCC");
                //while (!CmdExecuted) { System.Threading.Thread.Sleep(10); }
                //bool rlt1 = calibCCoeff.ResolveXml2Value(CalibCoeffXml);
                //if (rlt1 == false)
                //{
                //    nlogger.Error("error when read and resolve configuration data from xml string");
                //    return false;
                //}

                // 设置返回标志位
                //CmdExecuted = false;
                //bool rlt2 = sendCMD("OutputExecutedTag=N");
                //System.Threading.Thread.Sleep(10);
                //if(rlt2 == false)
                //{
                //    nlogger.Error("Error in setup instDevice 1.");
                //    return false;
                //}

                // 写入一些设置信息
                bool rlt3 = true;
                switch (mode)
                {
                    // 暂时还不用这种方法
                    case InstSampleMode.AutoSample_Fmt0:
                        rlt3 &= sendCMD("SAMPLEMODE=2");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("SAMPLEINTERVAL=4");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("OUTPUTFORMAT=0");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("AUTORUN=N");
                        System.Threading.Thread.Sleep(10);
                        outputFormat = SBE37OutputFormat.Format_0;
                        break;

                    // 暂时还不用这种方法
                    case InstSampleMode.AutoSample_Fmt1:
                        rlt3 &= sendCMD("SAMPLEMODE=2");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("SAMPLEINTERVAL=4");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("OUTPUTFORMAT=1");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("AUTORUN=N");
                        System.Threading.Thread.Sleep(10);
                        outputFormat = SBE37OutputFormat.Format_1;
                        break;

                    // 暂时还不用这种方法
                    case InstSampleMode.PolledSample_Fmt0:
                        rlt3 &= sendCMD("SAMPLEMODE=1");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("AUTORUN=N");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("OUTPUTFORMAT=0");
                        System.Threading.Thread.Sleep(10);
                        outputFormat = SBE37OutputFormat.Format_0;
                        break;

                    // 暂时还不用这种方法
                    case InstSampleMode.PolledSample_Fmt1:
                        rlt3 &= sendCMD("SAMPLEMODE=1");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("AUTORUN=N");
                        System.Threading.Thread.Sleep(10);
                        rlt3 &= sendCMD("OUTPUTFORMAT=1");
                        System.Threading.Thread.Sleep(10);
                        outputFormat = SBE37OutputFormat.Format_1;
                        break;


                    // 
                    case InstSampleMode.PolledSample_Fmt10:
                        //rlt3 &= sendCMD("SAMPLEMODE=1");
                        //System.Threading.Thread.Sleep(10);
                        //rlt3 &= sendCMD("AUTORUN=N");
                        //System.Threading.Thread.Sleep(10);
                        currentCmd = SBE37Cmd.WakeUp;
                        CmdExecuted = false;
                        rlt3 &= sendCMD("OUTPUTFORMAT=1");
                        int ii = 0;
                        while (!CmdExecuted && ii < 20) { System.Threading.Thread.Sleep(50); ii++; }

                        currentCmd = SBE37Cmd.Cfg;
                        CmdExecuted = false;
                        rlt3 &= sendCMD("OUTPUTFORMAT=1");
                        ii = 0;
                        while (!CmdExecuted && ii < 30) { System.Threading.Thread.Sleep(50); ii++; }
                        outputFormat = SBE37OutputFormat.Format_1;
                        break;

                    default:
                        break;
                }

                if (rlt3 == false)
                {
                    nlogger.Error("Error in setup instDevice 2.");
                    return false;
                }

                sampleMode = mode;

                // todo: 这里的 while 感觉不是很好
                // 读取 configuration data
                currentCmd = SBE37Cmd.GetCD;
                CmdExecuted = false;
                bool rlt4 = sendCMD("GetCD");
                int i = 0;
                while (!CmdExecuted && i < 200) { System.Threading.Thread.Sleep(50); i++; }
                if (i > 199)
                {
                    nlogger.Error("timeout when read configuration data from xml string");
                    return false;
                }

                rlt4 &= configData.ResolveXml2Value(ConfigDataXml);
                if (rlt4 == false)
                {
                    nlogger.Error("error when read and resolve configuration data from xml string");
                    return false;
                }

                //bool rlt5 = true;
                //UseExecutedFlag = useOptFlg;
                //if (UseExecutedFlag == true ) {
                //    CmdExecuted = false;
                //    rlt5 = sendCMD("OutputExecutedTag=Y");
                //    if(rlt5 == false)
                //    {
                //        return false;
                //    }

                //    while (!CmdExecuted) { System.Threading.Thread.Sleep(10); }
                //}

                return true;
            }

            /// <summary>
            /// 关闭/停止仪器工作
            /// </summary>
            /// <returns></returns>
            public override bool DisableInstDevice()
            {
                if (!Enable) return true;
                try
                {
                    // 这里的串口可能都没有初始化
                    sendCMD("QS");
                }
                catch (Exception ex) { }

                base.DisableInstDevice();
                return true;
            }

            /// <summary>
            /// 事件传递函数
            /// </summary>
            /// <param name="err"></param>
            protected override void OnErrorOccur(Err_sr err)
            {
                if (!Enable) return;

                // 只有在这四种命令格式下，才会触发错误
                switch (currentCmd)
                {
                    case SBE37Cmd.Start:
                    case SBE37Cmd.Stop:
                    case SBE37Cmd.Ts:
                    case SBE37Cmd.Tsr:
                        base.OnErrorOccur(err);
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// 进入 Measure 步骤
            /// </summary>
            protected override void internalEnterMeasureStep()
            {
                //nlogger.Error("wghou enter measure step.");

                base.internalEnterMeasureStep();

                // 确保打开了串口，确保清空串口中的数据
                try { sPort.Open(); sPort.DiscardInBuffer(); } catch { }

                switch (sampleMode)
                {
                    case InstSampleMode.AutoSample_Fmt0:
                    case InstSampleMode.AutoSample_Fmt1:
                        currentCmd = SBE37Cmd.Start;
                        CmdExecuted = true;
                        bool rlt = sendCMD("Start");
                        if (rlt == false)
                        {
                            nlogger.Error("error in sendCmd with internalEnterMeasureStep");
                            OnErrorOccur(Err_sr.Error);
                        }
                        break;

                    case InstSampleMode.PolledSample_Fmt10:
                        // 此处为前一个指令
                        currentCmd = SBE37Cmd.Tsr;
                        CmdExecuted = true;
                        sendCMD("OutputFormat=1");
                        _tickTimerInst.Start();
                        break;
                    case InstSampleMode.PolledSample_Fmt0:
                        currentCmd = SBE37Cmd.Ts;
                        CmdExecuted = true;
                        sendCMD("OutputFormat=1");
                        _tickTimerInst.Start();
                        break;

                    case InstSampleMode.PolledSample_Fmt1:
                        currentCmd = SBE37Cmd.Tsr;
                        CmdExecuted = true;
                        sendCMD("OutputFormat=1");
                        _tickTimerInst.Start();
                        break;

                    default:
                        break;
                }
            }
            /// <summary>
            /// 执行 Measure 步骤
            /// </summary>
            protected override void internalMeasureStep()
            {
                //nlogger.Error("wghou internal measure step.");

                if (sampleMode == InstSampleMode.AutoSample_Fmt0 || sampleMode == InstSampleMode.AutoSample_Fmt1)
                {

                }
                else if (sampleMode == InstSampleMode.PolledSample_Fmt0)
                {
                    currentCmd = SBE37Cmd.Tsr;
                    sendSampleCmd(currentCmd);
                }
                else if (sampleMode == InstSampleMode.PolledSample_Fmt1)
                {
                    currentCmd = SBE37Cmd.Ts;
                    sendSampleCmd(currentCmd);
                }
                else if (sampleMode == InstSampleMode.PolledSample_Fmt10)
                {
                    // 上一个指令未成功返回结果
                    // todo:
                    if (CmdExecuted == false)
                    {
                        nlogger.Error("error in sendCmd with internalMeasureStep");
                        OnErrorOccur(Err_sr.Error);
                        CmdExecuted = true;
                        return;
                    }

                    // 判断前一个指令
                    if (currentCmd == SBE37Cmd.Ts)
                    {
                        currentCmd = SBE37Cmd.Tsr;
                        sendSampleCmd(currentCmd);
                    }
                    // 判断前一个指令
                    else if (currentCmd == SBE37Cmd.Tsr)
                    {
                        currentCmd = SBE37Cmd.Ts;
                        sendSampleCmd(currentCmd);
                    }
                    else
                    {
                        nlogger.Error("code error");
                        OnErrorOccur(Err_sr.Error);
                    }
                }
            }

            /// <summary>
            /// 依据指令，发送对应的指令字符串
            /// </summary>
            /// <param name="cmd"></param>
            /// <returns></returns>
            private bool sendSampleCmd(SBE37Cmd cmd)
            {
                bool rlt = true;
                CmdExecuted = false;

                if (userDefinedCmdEnable == true)
                {
                    if (cmd == SBE37Cmd.Ts)
                    {
                        rlt &= sendCMD(userDefinedCmd["TS1"]);
                        System.Threading.Thread.Sleep(200);
                        rlt &= sendCMD(userDefinedCmd["TS2"]);
                    }
                    else if (cmd == SBE37Cmd.Tsr)
                    {
                        rlt &= sendCMD(userDefinedCmd["TSR1"]);
                        System.Threading.Thread.Sleep(200);
                        rlt &= sendCMD(userDefinedCmd["TSR2"]);
                    }
                }
                else
                {
                    switch (Info.InstType)
                    {
                        case TypeInst.SBE37SM:
                            if (cmd == SBE37Cmd.Ts)
                            {
                                rlt &= sendCMD("OutputFormat=1");
                                System.Threading.Thread.Sleep(200);
                                rlt &= sendCMD("ts");
                            }
                            else if (cmd == SBE37Cmd.Tsr)
                            {
                                rlt &= sendCMD("OutputFormat=0");
                                System.Threading.Thread.Sleep(200);
                                rlt &= sendCMD("ts");
                            }
                            break;

                        case TypeInst.SBE37SMP:
                            if (cmd == SBE37Cmd.Ts)
                            {
                                rlt &= sendCMD("ts");
                            }
                            else if (cmd == SBE37Cmd.Tsr)
                            {
                                rlt &= sendCMD("tsr");
                            }
                            break;

                        case TypeInst.SBE37SI:
                        case TypeInst.SBE37SIP:
                        case TypeInst.SBE37SMPODO:
                            if (cmd == SBE37Cmd.Ts)
                            {
                                rlt &= sendCMD("ts");
                            }
                            else if (cmd == SBE37Cmd.Tsr)
                            {
                                rlt &= sendCMD("tsr");
                            }
                            break;

                        default:
                            nlogger.Error("code error xxison");
                            break;
                    }
                }

                if (rlt == false)
                {
                    nlogger.Error("error in sendCmd with internalMeasureStep");
                    OnErrorOccur(Err_sr.Error);
                }

                return rlt;
            }

            /// <summary>
            /// 进入 Store 步骤
            /// </summary>
            protected override void internalEnterStoreStep()
            {
                if (sampleMode == InstSampleMode.AutoSample_Fmt0 || sampleMode == InstSampleMode.AutoSample_Fmt1)
                {
                    currentCmd = SBE37Cmd.Stop;
                    CmdExecuted = false;
                    bool rlt = sendCMD("stop");
                    if (rlt == false)
                    {
                        nlogger.Error("error in sendCmd with internalEnterStoreStep");
                        OnErrorOccur(Err_sr.Error);
                    }
                }
                else
                {
                    _tickTimerInst.Stop();
                }
            }

            /// <summary>
            /// 由收到的字符串解析为指令。
            /// 其实就是检查 <Executed/>
            /// </summary>
            /// <param name="str"> 串口接收到的字符串 </param>
            /// <returns> 是否成功解析为指令 </returns>
            protected override bool ResolveStr2Cmd(string str)
            {
                switch (currentCmd)
                {
                    case SBE37Cmd.GetCD:
                        if (str.ToUpper().Contains("GETCD"))
                        {
                            nlogger.Error("elimate cmd: " + str);
                            return true;
                        }
                        break;

                    case SBE37Cmd.GetCC:
                        if (str.ToUpper().Contains("GETCC"))
                        {
                            nlogger.Error("elimate cmd: " + str);
                            return true;
                        }
                        break;

                    case SBE37Cmd.Start:
                        if (str.ToUpper().Contains("START"))
                        {
                            nlogger.Error("elimate cmd: " + str);
                            return true;
                        }
                        break;

                    case SBE37Cmd.Ts:
                        if (str.ToUpper().Contains("TS") ||
                            str.ToUpper().Contains("OUTPUTFORMAT"))
                        {
                            nlogger.Error("elimate cmd: " + str);
                            return true;
                        }
                        break;
                    case SBE37Cmd.Tsr:
                        if (str.ToUpper().Contains("TS") ||
                            str.ToUpper().Contains("OUTPUTFORMAT"))
                        {
                            nlogger.Error("elimate cmd:" + str);
                            return true;
                        }
                        break;

                    case SBE37Cmd.Cfg:
                        if (str.ToUpper().Contains("OUTPUTFORMAT"))
                        {
                            nlogger.Error("elimate cmd: " + str);
                            CmdExecuted = true;
                            return true;
                        }
                        break;

                    case SBE37Cmd.WakeUp:
                        nlogger.Error("elimate cmd: wakeup + " + str);
                        return true;
                        break;

                    default:
                        break;
                }

                return false;
            }
            /// <summary>
            /// 由接收到的字符串解析为结束符 Executed
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            protected override bool ResolveStr2ExecutedFlag(string str)
            {
                if (currentCmd == SBE37Cmd.Cfg)
                {
                    CmdExecuted = true;
                }

                // 在不是 tc 和 tcr 的情况下，数据后面都有可能会跟随有 <Executed/>
                if (str.Contains(OutputFlag))
                {
                    nlogger.Trace("receive executed flag at the end of ts / tsr.");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// 由收到的字符串解析为数据。
            /// 根据不同的传感器/配置，进行数据的解析
            /// </summary>
            /// <param name="str"> 串口接收到的字符串 </param>
            /// <param name="dt"> 解析得到的数据，为空表示 暂时未解析成一组数据 </param>
            /// <returns> 是否成功解析为数据 </returns>
            protected override bool ResolveStr2Data(string str, out InstSBE37Data data)
            {
                // 程序执行到这里，说明已经通过 ResolveStr2Cmd(string str) 判断得到，
                // str 中不包含有 <Executed/>

                bool rlt = true;

                switch (currentCmd)
                {
                    case SBE37Cmd.GetCD:
                        data = null;
                        rlt = GetXmlData(str, out ConfigDataXml);
                        break;

                    case SBE37Cmd.GetCC:
                        data = null;
                        rlt = GetXmlData(str, out CalibCoeffXml);
                        break;

                    case SBE37Cmd.Start:
                    case SBE37Cmd.Stop:
                        // 是否会发生错误呢？
                        rlt = SBE37ResolveStr2Value(str, out data);
                        break;

                    case SBE37Cmd.Ts:
                        rlt = SBE37ResolveStr2Value(str, out data);
                        // 如果是 轮询 10 的方式查询数据
                        // todo: 不知道这儿这么写合理与否
                        if (sampleMode == InstSampleMode.PolledSample_Fmt10)
                        {
                            data = null;
                        }
                        break;

                    case SBE37Cmd.Tsr:
                        rlt = SBE37ResolveStr2Value(str, out data);
                        break;

                    case SBE37Cmd.Cfg:
                        CmdExecuted = true;
                        rlt = true;
                        data = null;
                        break;

                    default:
                        // 这里就产生了错误
                        // 因为现有的指令中，不会有其他的指令会返回数据
                        nlogger.Error("unknown cmd in InstBE37. ResolveStr2Data");
                        rlt = false;
                        data = null;
                        break;
                }

                return rlt;
            }


            /// <summary>
            /// 重新读取 information data in xml format, 
            /// such as configuration data & calibration coefficients
            /// </summary>
            /// <param name="str"> 第一行字符串 </param>
            /// <param name="xmlStr"> xml string 应放置的对象 </param>
            /// <returns> 是否成功获取 </returns>
            private bool GetXmlData(string str, out string xmlStr)
            {
                // 清空原有的 xml data
                xmlStr = "";

                // 读取 configuration data until get the <Executed/>
                try
                {
                    do
                    {
                        if (str.ToUpper().Contains("GETCC") || str.ToUpper().Contains("GETCD"))
                        {
                            nlogger.Error("elimate the cmd: " + str);
                        }
                        else
                        {
                            xmlStr += str + "\r";
                        }

                        //xmlStr += str + "\r";
                        str = sPort.ReadLine();
                    } while (!str.Contains(OutputFlag));
                    sPort.DiscardInBuffer();
                }
                catch (Exception ex)
                {
                    nlogger.Error("error in get xml data");
                    CmdExecuted = true;
                    return true;
                }
                CmdExecuted = true;
                return true;
            }

            /// <summary>
            /// SBE37 解析数据
            /// </summary>
            /// <param name="str"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            private bool SBE37ResolveStr2Value(string str, out InstSBE37Data data)
            {
                instData1Cache.vTestID = Info.testId;
                instData1Cache.InstIdx = Info.InstIdx_NotUsed;
                instData1Cache.InstType = Info.InstType;
                instData1Cache.vInstrumentID = Info.instrumentId;
                instData1Cache.vItemType = "";
                instData1Cache.vTitularValue = currentTemptPoint;
                instData1Cache.measureTime = DateTime.Now;
                instData1Cache.addTime = instData1Cache.measureTime;
                instData1Cache.updateTime = instData1Cache.measureTime;

                bool rlt = true;
                if (currentCmd == SBE37Cmd.Tsr)
                {
                    rlt = SBE37ResolveStr2ValueFmt0(str, out data);
                }
                else
                {
                    // 解析数据
                    switch (outputFormat)
                    {
                        case SBE37OutputFormat.Format_0:
                            rlt = SBE37ResolveStr2ValueFmt0(str, out data);
                            break;

                        case SBE37OutputFormat.Format_1:
                            rlt = SBE37ResolveStr2ValueFmt1(str, out data);
                            break;

                        case SBE37OutputFormat.Format_2:
                            data = null;
                            rlt = false;
                            nlogger.Error("error of output format: Format_2");
                            break;

                        case SBE37OutputFormat.Format_3:
                            data = null;
                            rlt = false;
                            nlogger.Error("error of output format: Format_3");
                            break;

                        default:
                            data = null;
                            rlt = false;
                            nlogger.Error("error of output format: unknown");
                            break;
                    }
                }
                return rlt;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            private bool SBE37ResolveStr2ValueFmt0(string str, out InstSBE37Data data)
            {
                data = new InstSBE37Data();
                string[] strSplit = str.Split(',');

                //nlogger.Error("wghou SBE37ResolveStr2ValueFmt0: " + str);

                int idx = 0;
                try
                {
                    int ttt = 0; double ccc = 0;
                    int ppp, vvv;
                    double oo, ot;
                    DateTime dt = DateTime.Now;

                    // tttttt =temperature A/D counts
                    if (configData.OutputTemperature.ToUpper().Contains("YES"))
                    {
                        ttt = int.Parse(strSplit[idx++]);
                    }

                    // cccc.ccc = conductivity frequency (Hz)
                    if (configData.OutputConductivity.ToUpper().Contains("YES"))
                    {
                        ccc = double.Parse(strSplit[idx++]);
                    }

                    instData1Cache.vTemperatureRaw = ttt;
                    instData1Cache.vConductivityRaw = ccc;

                    //// pppppp = pressure sensor pressure A/D counts;
                    //// vvvv = pressure sensor pressure temperature compensation A/D counts;
                    //// sent only if pressure sensor installed
                    //if (configData.PressureInstalled.ToUpper().Contains("YES") && configData.OutputPressure.ToUpper().Contains("YES"))
                    //{
                    //    ppp = int.Parse(strSplit[idx++]);
                    //    vvv = int.Parse(strSplit[idx++]);
                    //}

                    //// oo.ooo = oxygen sensor phase (µsec)
                    //// t.tttttt = oxygen sensor temperature voltage.
                    //if (configData.OutputOxygen.ToUpper().Contains("YES"))
                    //{
                    //    oo = double.Parse(strSplit[idx++]);
                    //    ot = double.Parse(strSplit[idx++]);
                    //}

                    //// dd mmm yyyy = day, month, year; sent only if OutputTime=Y
                    //// hh:mm:ss = hour, minute, second; sent only if OutputTime=Y.
                    //if (configData.OutputTime.ToUpper().Contains("YES"))
                    //{
                    //    dt = DateTime.Parse(strSplit[idx++]);
                    //    dt = DateTime.Parse(strSplit[idx++]);
                    //}

                    CmdExecuted = true;
                }
                catch (Exception ex)
                {
                    nlogger.Error("exception in SBE37ResolveStr2ValueFmt0: " + ex.Message);
                    nlogger.Error("error string: " + str);
                    return false;
                }

                // bug
                data.CopyFrom(instData1Cache);

                return true;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <param name="data"></param>
            /// <returns></returns>
            private bool SBE37ResolveStr2ValueFmt1(string str, out InstSBE37Data data)
            {
                data = new InstSBE37Data();
                string[] strSplit = str.Split(',');

                //nlogger.Error("wghou SBE37ResolveStr2ValueFmt1: " + str);

                int idx = 0;
                try
                {
                    double ttt = 0; double ccc = 0;
                    double ppp, ddd;
                    double sss = 0, vvv;
                    double rrr, sc;
                    double xxx;
                    double oo;
                    int nn;
                    DateTime dt = DateTime.Now;

                    // tttt.tttt = temperature (°C, ITS-90).
                    if (configData.OutputTemperature.ToUpper().Contains("YES"))
                    {
                        ttt = double.Parse(strSplit[idx++]);
                    }

                    // ccc.ccccc = conductivity (S/m).
                    if (configData.OutputConductivity.ToUpper().Contains("YES"))
                    {
                        ccc = double.Parse(strSplit[idx++]);
                    }

                    // ppppp.ppp = pressure (decibars)
                    // sent only if pressure sensor installed
                    if (configData.PressureInstalled.ToUpper().Contains("YES") && configData.OutputPressure.ToUpper().Contains("YES"))
                    {
                        ppp = double.Parse(strSplit[idx++]);
                    }

                    // dddd.ddd = depth (meters); 
                    // sent only if OutputDepth=Y
                    if (configData.OutputDepth.ToUpper().Contains("YES"))
                    {
                        ddd = double.Parse(strSplit[idx++]);
                    }

                    // oo.ooo = oxygen sensor phase (µsec)
                    // t.tttttt = oxygen sensor temperature voltage.
                    if (configData.OutputOxygen.ToUpper().Contains("YES"))
                    {
                        oo = double.Parse(strSplit[idx++]);
                    }

                    // ssss.ssss= salinity (psu); 
                    // sent only if OutputSal=Y
                    if (configData.OutputSalinity.ToUpper().Contains("YES"))
                    {
                        sss = double.Parse(strSplit[idx++]);
                    }

                    instData1Cache.vTemperature = ttt;
                    instData1Cache.vConductivity = ccc;
                    instData1Cache.vSalinity = sss;


                    //// vvvv.vvv = sound velocity (meters/second); 
                    //// sent only if OutputSV=Y
                    //if (configData.OutputSV.ToUpper().Contains("YES"))
                    //{
                    //    vvv = double.Parse(strSplit[idx++]);
                    //}

                    //// rrr.rrrr = local density (kg/m3); 
                    //// sent only if OutputDensity=Y
                    //if (configData.OutputDensity.ToUpper().Contains("YES"))
                    //{
                    //    rrr = double.Parse(strSplit[idx++]);
                    //}

                    //// x = specific conductivity; 
                    ////sent if OutputSC=Y
                    //if (configData.OutputSC.ToUpper().Contains("YES"))
                    //{
                    //    xxx = double.Parse(strSplit[idx++]);
                    //}

                    //// dd mmm yyyy = day, month, year; sent only if OutputTime=Y
                    //// hh:mm:ss = hour, minute, second; sent only if OutputTime=Y.
                    //if (configData.OutputTime.ToUpper().Contains("YES"))
                    //{
                    //    dt = DateTime.Parse(strSplit[idx++]);
                    //    dt = DateTime.Parse(strSplit[idx++]);
                    //}

                    //// n = sample number in FLASH memory 
                    //// sent if TxSampleNum=y
                    //if (configData.TxSampleNumber.ToUpper().Contains("YES"))
                    //{
                    //    nn = int.Parse(strSplit[idx++]);
                    //}

                    CmdExecuted = true;
                }
                catch (Exception ex)
                {
                    nlogger.Error("exception in SBE37ResolveStr2ValueFmt1: " + ex.Message);
                    nlogger.Error("error string: " + str);
                    return false;
                }

                // bug
                data.CopyFrom(instData1Cache);

                return true;
            }
        }
    }
}

