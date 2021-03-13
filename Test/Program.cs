using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ATControl.Utils;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            UtilsDeviceStateM utilsDeviceStateM = new UtilsDeviceStateM();

            // json config file
            string confFile = "config.json";
            System.IO.StreamReader file = System.IO.File.OpenText(confFile);
            JsonTextReader reader = new JsonTextReader(file);
            JObject obj = (JObject)JToken.ReadFrom(reader);

            // 设置 socket 接口
            if (obj.ContainsKey("Socket"))
            {
                JObject child = (JObject)obj["Socket"];

                utilsDeviceStateM.InitSocketServer(child);
            }
        }
    }
}
