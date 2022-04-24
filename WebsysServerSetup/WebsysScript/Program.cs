using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WebsysScript
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        //private static Mutex mut = new Mutex();
        static void Main(string[] args)
        {
            String rtn = "";
            Boolean success = true;
            /*int len = args.Length;
            for(var i=0; i<len; i++)
            {
                rtn+="第"+i+"入参："+args[i]+",";
            }*/
            string curpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"temp");// Path.GetFullPath(".");//Path.GetTempPath();
            //Console.WriteLine(curpath);
            if (!Directory.Exists(curpath))
            {
                Directory.CreateDirectory(curpath);
            }
            //mut.WaitOne(1000);

            string MyCodeFile = Path.Combine(curpath, "MyCode.txt");
            if (args.Length > 0)  //从参数中获得全路径
            {
                MyCodeFile = args[0];
            }
            if (!File.Exists(MyCodeFile))  //不存在MyCode文件时
            {
                DirectoryInfo TheFolder = new DirectoryInfo(curpath);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    if (NextFile.Name.StartsWith("MyCode"))
                    {
                        System.IO.File.Move(curpath+"\\"+NextFile.Name, curpath+"\\--" + NextFile.Name);
                        MyCodeFile = curpath + "\\--" + NextFile.Name; //正在处理的文件名前加--
                        break;
                    }
                }
            }
            //mut.ReleaseMutex();
            //Console.WriteLine(MyCodeFile);
            if (File.Exists(MyCodeFile))
            {
                string txtlang = "",str="";
                //2021-11-16 把using提前结束。不然会占用MyCode.txt文件
                using (StreamReader sr = File.OpenText(MyCodeFile))
                {
                    //Console.WriteLine(sr.ReadToEnd());
                    txtlang = sr.ReadLine();
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                try
                {
                    Thread.Sleep(2000);
                    MSScriptControl.ScriptControlClass s = new MSScriptControl.ScriptControlClass();
                    s.AllowUI = true;
                    s.Timeout = 1 * 60 * 1000; // 导出Excel 1000行*20列的数据 JS花费1分钟左右，VB下2秒左右
                    s.UseSafeSubset = false;   // false表示低安全运行。设置为true时报： "ActiveX 部件不能创建对象: 'Excel.Application'"
                    IntPtr hWndint = GetForegroundWindow();
                    s.SitehWnd = hWndint.ToInt32();   //指定弹出窗口在当前弹出，默认为桌面
                    if (txtlang.ToLower().IndexOf("vbscript") > -1)
                    {
                        s.Language = "VBScript";
                        if (0 != str.ToLower().IndexOf("function")) str = "Function vbs_Test\n" + str + "vbs_Test = 1\n End Function \n";  //默认返回空值，如果业务上有返回也不影响
                        s.Reset();
                        s.AddCode(str);
                        rtn = s.Run("vbs_Test").ToString();
                    }
                    else
                    {
                        s.Language = "JScript";
                        if (0 != str.IndexOf("(function")) str = "(function test(){" + str + "return 1;})();";  //默认返回空值，如果业务上有返回也不影响
                        s.Reset();
                        rtn = s.Eval(str).ToString();
                        s = null;
                    }
                }
                catch (Exception ex) {
                    // 报错把错误追加到txt中
                    success = false;
                    rtn = "\n\n/*\n"+ex.Source+"\n"+ex.Message+"\n"+ex.StackTrace+"\n*/";
                    using (StreamWriter sw = File.AppendText(MyCodeFile))
                    {
                        sw.WriteLine(rtn);
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (success)
                {  //成功运行后删除文件
                    File.Delete(MyCodeFile);
                }
                string MyRtnFile = Path.Combine(curpath, "MyRtn.txt");
                using (StreamWriter sw = File.CreateText(MyRtnFile))
                {
                    sw.WriteLine(rtn);
                    sw.Close();
                }
            }
            else
            {
                string MyRtnFile = Path.Combine(curpath, "MyRtn.txt");
                using (StreamWriter sw = File.CreateText(MyRtnFile))
                {
                    sw.WriteLine(MyCodeFile+"文件不存在");
                    sw.Flush();
                    sw.Close();
                }
            }
        }
    }
}
