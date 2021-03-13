using NLog;
using NLog.Targets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ATControl
{
    namespace Utils
    {
        public partial class UtilsDeviceStateM
        {
            string serviceFilePath = "IotService.exe";
            string serviceName = "IotService";

            internal bool InitIotService()
            {
                try
                {
                    // 安装服务
                    if (!this.IsServiceExisted(serviceName)) this.InstallService(serviceFilePath);

                    // 启动服务
                    if (this.IsServiceExisted(serviceName))
                    {
                        this.ServiceStart(serviceName);

                        // 返回服务的运行状态
                        System.Threading.Thread.Sleep(500);
                        return IsServiceRunning(serviceName);
                    }
                    else
                    {
                        //nlogger.Error("There is no such service named \"IotService\".");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    //nlogger.Error("exception occur when install IotService." + ex.Message);
                    return false;
                }
            }

            // 检查服务是否正在运行
            private bool IsServiceRunning(string serviceName)
            {
                if (!this.IsServiceExisted(serviceName)) return false;

                using (ServiceController control = new ServiceController(serviceName))
                {
                    return control.Status == ServiceControllerStatus.Running;
                }
            }

            //判断服务是否存在
            private bool IsServiceExisted(string serviceName)
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController sc in services)
                {
                    if (sc.ServiceName.ToLower() == serviceName.ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }

            //安装服务
            private void InstallService(string serviceFilePath)
            {

                using (AssemblyInstaller installer = new AssemblyInstaller())
                {
                    installer.UseNewContext = true;
                    installer.Path = serviceFilePath;
                    IDictionary savedState = new Hashtable();
                    installer.Install(savedState);
                    installer.Commit(savedState);
                }
            }

            //卸载服务
            private void UninstallService(string serviceFilePath)
            {
                using (AssemblyInstaller installer = new AssemblyInstaller())
                {
                    installer.UseNewContext = true;
                    installer.Path = serviceFilePath;
                    installer.Uninstall(null);
                }
            }

            //启动服务
            private void ServiceStart(string serviceName)
            {
                using (ServiceController control = new ServiceController(serviceName))
                {
                    if (control.Status == ServiceControllerStatus.Stopped)
                    {
                        control.Start();
                    }
                }
            }

            //停止服务
            private void ServiceStop(string serviceName)
            {
                using (ServiceController control = new ServiceController(serviceName))
                {
                    if (control.Status == ServiceControllerStatus.Running)
                    {
                        control.Stop();
                    }
                }
            }
        }
    }
}