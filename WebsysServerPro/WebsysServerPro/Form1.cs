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
        private void Form1_Load(object sender, EventArgs e)
        {
            ///  Registry.LocalMachine 不允许访问
            RegistryKey rk = Registry.ClassesRoot;
            string command = rk.OpenSubKey(@"RunWebsysServer\Shell\open\command").GetValue("").ToString();
            string path = command.Substring(1, command.IndexOf(".exe"))+"exe";
            //(@"Software\Microsoft\Windows\CurrentVersion\Run",true).GetValue("WebsysServer_init").ToString();
            while (true)
            {
                if (WebsysServerIsRuning() == 0)
                {
                    //Process.Start("WebsysServer.exe");
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = path;   //IE浏览器，可以更换
                    try
                    {
                        process.Start();
                    }catch (Exception ex) {
                        
                    }
                }
                Thread.Sleep(10000); //10秒
            }
        }
    }
}
