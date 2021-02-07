using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebsysServerPro
{
    public partial class Form1 : Form
    {
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
        static void ProWebServer()
        {
            RegistryKey rk = Registry.ClassesRoot;
            string command = rk.OpenSubKey(@"RunWebsysServer\Shell\open\command").GetValue("").ToString();
            string path = command.Substring(1, command.IndexOf(".exe")) + "exe";
            while (true){
                if (WebsysServerIsRuning() == 0)
                {
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
