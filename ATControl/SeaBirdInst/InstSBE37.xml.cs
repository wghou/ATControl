using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ATControl
{
    namespace SeaBirdInst
    {
        /// <summary>
        /// calibration coefficients of SBE37
        /// </summary>
        public class SBE37CalibCoeff
        {
            public bool ResolveXml2Value(string xmlString) { return false; }
        }

        /// <summary>
        /// configuration data of SBE37
        /// todo: 解析、默认值等问题
        /// </summary>
        public class SBE37ConfigData
        {
            /// <summary> 仪器类型 </summary>
            public string DeviceType { get; set; }
            /// <summary> 序列号 </summary>
            public string SerialNumber { get; set; }

            /// <summary> SI SIP SMP SMP-ODO </summary>
            public string PressureInstalled { get; set; }
            ///// <summary> SI </summary>
            //public float ReferencePressure { get; set; }
            ///// <summary> SI SIP </summary>
            //public string PumpInstalled { get; set; }
            ///// <summary> SIP  SMP SMP-ODO </summary>
            //public float MinCondFreq { get; set; }
            ///// <summary> SI SIP </summary>
            //public string SampleMode { get; set; }
            ///// <summary> SI SIP SM SMP SMP-ODO </summary>
            //public string SampleDataFormat { get; set; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public string TemperatureUnits { set; get; }
            ///// <summary> SI SIP SM  SMP SMP-ODO </summary>
            //public string ConductivityUnits { get; set; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public string PressureUnits { set; get; }
            ///// <summary> SMP-ODO </summary>
            //public string OxygenUnits { set; get; }
            /// <summary> SM  SMP SMP-ODO </summary>
            public string OutputTemperature { set; get; }
            /// <summary> SM  SMP SMP-ODO </summary>
            public string OutputConductivity { set; get; }
            /// <summary> SI SIP  SMP SMP-ODO </summary>
            public string OutputPressure { get; set; }
            /// <summary> SMP-ODO </summary>
            public string OutputOxygen { set; get; }
            ///// <summary> SI SIP  SMP SMP-ODO </summary>
            //public string OutputSV { get; set; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public string OutputSC { set; get; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public float SCCoeff { set; get; }
            /// <summary> SI SIP </summary>
            public string OutputDepth { get; set; }
            /// <summary> SI SIP  SMP SMP-ODO </summary>
            public string OutputSalinity { get; set; }
            ///// <summary> SI SIP </summary>
            //public string Latitude { get; set; }
            ///// <summary> SI SIP </summary>
            //public string OutputDensity { get; set; }
            ///// <summary> SI SIP  SMP SMP-ODO </summary>
            //public string TxSampleNumber { get; set; }
            ///// <summary> SI SIP  SMP SMP-ODO </summary>
            //public int SampleInterval { get; set; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public string TxRealTime { set; get; }
            ///// <summary> SM  SMP SMP-ODO </summary>
            //public string SyncMode { set; get; }
            ///// <summary> SI SIP </summary>
            //public string OutputTime { get; set; }
            ///// <summary> SI SIP </summary>
            //public string AutoRun { get; set; }
            ///// <summary> SI SIP </summary>
            //public string StoreData { get; set; }
            ///// <summary> SMP-ODO </summary>
            //public string AdaptivePumpControl { set; get; }
            ///// <summary> SMP-ODO </summary>
            //public float nTau { set; get; }
            ///// <summary> SMP-ODO </summary>
            //public float PumpOnTime { set; get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="xmlString"></param>
            /// <returns></returns>
            public bool ResolveXml2Value(string xmlString)
            {
                XmlDocument xmlDoc = new XmlDocument();

                string xmlString2 = "";
                string[] lines = xmlString.Split(new char[] { '\r', '\n' });
                if (lines.Length <= 2) return false;

                xmlString2 = lines[0] + "\r";

                for (int i = 1; i < lines.Length; i++)
                {
                    xmlString2 += lines[i] + "\r";

                    if (lines[i].ToLower().Contains("configurationdata"))
                    {
                        break;
                    }
                }

                try
                {
                    string _pathLog = "Logs";
                    string _pathIst = _pathLog + "/InstConfigInfo";
                    // 建立日志文件夹
                    if (!Directory.Exists(_pathLog))
                        Directory.CreateDirectory(_pathLog);
                    if (!Directory.Exists(_pathIst))
                        Directory.CreateDirectory(_pathIst);

                    string confFile = _pathIst + "/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm ") + DeviceType + "-" + SerialNumber + "-";

                    for (int i = 0; i < 20; i++)
                    {
                        string fileP = confFile + i.ToString() + ".txt";
                        if (!File.Exists(fileP))
                        {
                            File.WriteAllText(fileP, xmlString2);
                            break;
                        }
                    }

                }
                catch { }

                try
                {
                    xmlDoc.LoadXml(xmlString2);

                    XmlNode node = xmlDoc.SelectSingleNode("ConfigurationData");

                    this.DeviceType = node.Attributes["DeviceType"].InnerText;
                    this.SerialNumber = node.Attributes["SerialNumber"].InnerText;
                    this.PressureInstalled = node.SelectSingleNode("PressureInstalled") == null ? "no" : node["PressureInstalled"].InnerText;
                    //this.PumpInstalled = node.SelectSingleNode("PumpInstalled") == null ? "" : node["PumpInstalled"].InnerText;
                    //this.MinCondFreq = node.SelectSingleNode("MinCondFreq") == null ? 0 : float.Parse(node["MinCondFreq"].InnerText);
                    //this.SampleMode = node.SelectSingleNode("SampleMode") == null ? "" : node["SampleMode"].InnerText;
                    //this.SampleDataFormat = node.SelectSingleNode("SampleDataFormat") == null ? "" : node["SampleDataFormat"].InnerText;
                    //this.ConductivityUnits = node.SelectSingleNode("ConductivityUnits") == null ? "" : node["ConductivityUnits"].InnerText;

                    this.OutputTemperature = node.SelectSingleNode("OutputTemperature") == null ? "yes" : node["OutputTemperature"].InnerText;
                    this.OutputConductivity = node.SelectSingleNode("OutputConductivity") == null ? "yes" : node["OutputConductivity"].InnerText;
                    this.OutputPressure = node.SelectSingleNode("OutputPressure") == null ? "yes" : node["OutputPressure"].InnerText;
                    this.OutputSalinity = node.SelectSingleNode("OutputSalinity") == null ? "yes" : node["OutputSalinity"].InnerText;
                    //this.OutputSV = node.SelectSingleNode("OutputSV") == null ? "" : node["OutputSV"].InnerText;
                    this.OutputOxygen = node.SelectSingleNode("OutputOxygen") == null ? "no" : node["OutputOxygen"].InnerText;
                    this.OutputDepth = node.SelectSingleNode("OutputDepth") == null ? "no" : node["OutputDepth"].InnerText;
                    //this.Latitude = node.SelectSingleNode("Latitude") == null ? "" : node["Latitude"].InnerText;
                    //this.OutputDensity = node.SelectSingleNode("OutputDensity") == null ? "" : node["OutputDensity"].InnerText;
                    //this.TxSampleNumber = node.SelectSingleNode("TxSampleNumber") == null ? "" : node["TxSampleNumber"].InnerText;
                    //this.SampleInterval = node.SelectSingleNode("SampleInterval") == null ? 0 : int.Parse(node["SampleInterval"].InnerText);
                    //this.OutputTime = node.SelectSingleNode("OutputTime") == null ? "" : node["OutputTime"].InnerText;
                    //this.AutoRun = node.SelectSingleNode("AutoRun") == null ? "" : node["AutoRun"].InnerText;
                    //this.StoreData = node.SelectSingleNode("StoreData") == null ? "" : node["StoreData"].InnerText;



                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

    }
}
