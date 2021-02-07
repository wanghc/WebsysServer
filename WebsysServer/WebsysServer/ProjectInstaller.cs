using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;

namespace WebsysServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        //安装路径
        private string strInstallPath = "";

        public System.Configuration.Install.InstallEventHandler AutoStart(IDictionary aa)
        {
            //MessageBox.Show(strInstallPath);
            StartWebsys();
            return null;
        }
        public ProjectInstaller()
        {
            InitializeComponent();
            // Attach the 'Committed' event.
            //this.Committed += new InstallEventHandler(Installer_Committed);
            // Attach the 'Committing' event.
            //this.Committing += new InstallEventHandler(Installer_Committing);
        }
        private void KillProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    // 杀掉这个进程。
                    process.Kill();
                    // 等待进程被杀掉。你也可以在这里加上一个超时时间（毫秒整数）。
                    // process.WaitForExit(); //时间很久
                }
                catch (Win32Exception ex)
                {
                    // 无法结束进程，可能有很多原因。
                    // 建议记录这个异常，如果你的程序能够处理这里的某种特定异常了，那么就需要在这里补充处理。
                    // Log.Error(ex);
                }
                catch (InvalidOperationException)
                {
                    // 进程已经退出，无法继续退出。既然已经退了，那这里也算是退出成功了。
                    // 于是这里其实什么代码也不需要执行。
                }
            }
        }
        protected override void OnAfterInstall(IDictionary savedState)
        {
            //MessageBox.Show("on after");
            StartWebsys();
            base.OnAfterInstall(savedState);
        }
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            // https://www.c-sharpcorner.com/article/how-to-perform-custom-actions-and-upgrade-using-visual-studio-installer/
            //var company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
            //var applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),   company,  Assembly.GetExecutingAssembly().GetName().Name);
            //var serviceController = ServiceController.GetServices().FirstOrDefault(s=>s.ServiceName.Equals(serviceInstaller1.ServiceName));
            StopWebsys();
            base.OnBeforeInstall(savedState);
            strInstallPath = this.Context.Parameters["targetdir"];
            //MessageBox.Show("on beforeInstall"+strInstallPath);
        }
        public override void Install(IDictionary stateSaver)
        {
            //MessageBox.Show("on install");
            base.Install(stateSaver);
            // 注释掉，移入OnAfterInstall与OnBeforeInstall
            /*
             * strInstallPath = this.Context.Parameters["targetdir"];
            //安装完成执行
            base.Committed += AutoStart(stateSaver);
            */

        }
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            //commit时不能传targetdir
            //string rtn = tool.ScriptShell.Run("\"C:\\Program Files (x86)\\MediWay\\WebsysServer\\WebsysServer.exe\"");
            //MessageBox.Show("faff");
            //tool.Logging.Warn("安装完成");
            //tool.Logging.Warn(rtn);
            /*System.ServiceProcess.ServiceController[] services = System.ServiceProcess.ServiceController.GetServices();
            MessageBox.Show("服务数量："+services.Length);
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower().Contains("websysserver"))
                {
                    MessageBox.Show (s.ServiceName);
                    if (s.Status.Equals(ServiceControllerStatus.Stopped))
                    {
                        s.Start();
                    }
                }
            }*/
            //System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController("WebsysServer");

        }
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            StopWebsys();
        }
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            /*System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController("WebsysServer");
            if (sc.Status.Equals(System.ServiceProcess.ServiceControllerStatus.Running))
            {
                sc.Stop();
            }*/
        }
        private void StopWebsys()
        {
            KillProcess("WebsysServerPro");
            KillProcess("WebsysServer");
        }
        private void StartWebsys()
        {
            Process.Start(strInstallPath + "//WebsysServer.exe");
            Process.Start(strInstallPath + "//WebsysServerPro.exe");
        }
    }
}
