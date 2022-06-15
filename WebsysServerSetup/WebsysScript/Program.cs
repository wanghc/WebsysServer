using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WebsysScript.tool;

namespace WebsysScript
{
    class Program
    {
        /// <summary>
        /// 第一个入参文件名
        /// 第二个入参调试级别
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            String rtn = "";
            string TEMPPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"temp");// Path.GetFullPath(".");//Path.GetTempPath();
            if (!Directory.Exists(TEMPPath)) {
                Directory.CreateDirectory(TEMPPath);
            }
            string TxtFileName = "MyCode.txt";
            if (args.Length > 0) { TxtFileName = args[0]; }
            if (args.Length > 1) { Logging.CurLogLevel = int.Parse(args[1]); }
            Logging.Debug("开始处理：" + TEMPPath);
            Logging.Debug("文件："+ TxtFileName);
            rtn = HandlerFile.Handler(TEMPPath, TxtFileName);
            Console.WriteLine(rtn);
            /*
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm1 = new Form1();
            frm1.args = args;
            Application.Run(frm1);
            */
        }
    }
}
