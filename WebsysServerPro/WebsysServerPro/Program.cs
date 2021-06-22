using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
namespace WebsysServerPro
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (IsRunning())
            {
                //MessageBox.Show("保护程序已经在运行!", "医为客户端保护", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static bool IsRunning()
        {
            Process current = default(Process);
            current = System.Diagnostics.Process.GetCurrentProcess();
            Process[] processes = null;
            processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);
            Process process = default(Process);
            foreach (Process tempLoopVar_process in processes)
            {
                process = tempLoopVar_process;
                if (process.Id != current.Id)
                {
                    if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
