using System;
using System.Diagnostics;
using System.Text;

namespace WebsysScript
{
    internal class EvalCmd
    {
        public static string Run(String dir, string cmd, Boolean isNotReturn)
        {
            Process p = new Process()
            {
                StartInfo = {
                    FileName = "cmd.exe",
                    UseShellExecute = false,    //是否使用操作系统shell启动
                    RedirectStandardInput = true,//接受来自调用程序的输入信息
                    RedirectStandardOutput = true,//由调用程序获取输出信息
                    RedirectStandardError = true,//重定向标准错误输出
                    CreateNoWindow = true,//不显示程序窗口
                    WorkingDirectory = dir,
                }
            };
            p.Start();//启动程序
            //向cmd窗口发送输入信息           
            cmd = string.IsNullOrEmpty(cmd) ? "exit" : $"if 1 equ 1 ({cmd}) & exit";
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令
            if (isNotReturn)
            {
                //不读output流，可以尽早结束线程。而else会卡住进程
                return null;
            }
            else
            {
                //获取cmd窗口的输出信息
                string cmdRtn = p.StandardOutput.ReadToEnd();
                string[] arr = cmdRtn.Split('\n');
                StringBuilder rtn = new StringBuilder(); Boolean startResult = false;
                for (var i = 0; i < arr.Length - 1; i++)
                {
                    if (startResult) rtn.Append(arr[i] + "\n");
                    if (arr[i].Contains("exit")) startResult = true;
                }
                return rtn.ToString();
            }
        }
    }
}