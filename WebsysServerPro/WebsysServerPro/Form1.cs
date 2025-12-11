using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebsysServerPro
{
    public partial class Form1 : Form
    {
        private static int Count = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private static int WebsysServerIsRuning()
        {
            int count = 0;
            Process[] processes = null;
            processes = System.Diagnostics.Process.GetProcessesByName("WebsysServer");
            foreach (Process tempLoopVar_process in processes)
            {
                count++;
            }
            return count;
        }
        static int SendMsg2WebsysServer()
        {
            try
            {
                // WebsysServerPro 发送
                using (var client = new NamedPipeClientStream(".", "WebsysServerPipe", PipeDirection.Out))
                {
                    client.Connect(5000);
                    using (var writer = new StreamWriter(client))
                    {
                        writer.WriteLine("EXIT");
                        writer.Flush();
                        client.WaitForPipeDrain();
                    }
                    client.Dispose();
                }
                return 1;
            }
            catch (Exception ex) {

            }
            finally
            {
            }
            return -1;
        }
        static void KillWebsysAddins()
        {
            Process[] processes = null;
            processes = System.Diagnostics.Process.GetProcessesByName("WebsysServer");
            foreach (Process tempLoopVar_process in processes)
            {
                tempLoopVar_process.Kill();
            }
            return ;
        }
        static void ProWebServer()
        {
            RegistryKey rk = Registry.ClassesRoot;
            string command = rk.OpenSubKey(@"RunWebsysServer\Shell\open\command").GetValue("").ToString();
            string path = command.Substring(1, command.IndexOf(".exe")) + "exe";
            while (true){
                Count++;
                if (WebsysServerIsRuning() == 0 || (Count%360)==0)  // 一小时重启一次
                {
                    Count = 0;
                    // KillWebsysAddins();
                    int rtn = SendMsg2WebsysServer();
                    if (rtn==1)
                    {
                        // 5秒后成功结束进程 2025-8-18 SendMsg2WebsysServer通知【中间件服务】结束进程
                    }
                    else
                    {
                        KillWebsysAddins();
                    }
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = path;   //IE浏览器，可以更换
                    try{
                        process.Start();
                    }catch (Exception ex) {}
                }
                Thread.Sleep(10000); //10秒
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ///  Registry.LocalMachine 不允许访问
            //(@"Software\Microsoft\Windows\CurrentVersion\Run",true).GetValue("WebsysServer_init").ToString();
            /// 不能在form线程中直接死循环监听，与医保组客户端程序冲突。
            /// 启用了中间件保护程序就不能启动医保客户端，一直卡在启动中。启动了医保客户端程序就不能启动中间件。
            /// 修改成线程中死循环监听，解决与医保组客户端程序冲突问题。
            Thread clientThread = new Thread(ProWebServer);
            //clientThread.SetApartmentState(ApartmentState.STA);
            clientThread.Start();
        }
        /// <summary>
        /// 不显示窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender,EventArgs e)
        {
            this.Visible = false;
        }
    }
}
