using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace WebsysServer.tool
{
    class ScriptShell
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        public static string RunAdmin(string cmd)
        {
            Process process = new Process()
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments="/c C:\\Windows\\System32\\cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput =true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,//不显示程序窗口
                    Verb = "runas"
                }
            };
            process.Start();
            //向cmd窗口发送输入信息           
            cmd = string.IsNullOrEmpty(cmd) ? "exit" : $"{cmd}&exit";
            process.StandardInput.WriteLine(cmd);
            process.WaitForExit();
            process.StandardInput.AutoFlush = true;
            return process.StandardOutput.ReadToEnd();
        }
        /// <summary>
        /// 执行cmd命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns></returns>
        public static string Run(string cmd)
        {
            return Run(cmd, false);
        }

        /// <summary>
        /// 使用操作系统当前用户运行code
        /// 会把code写到MyCode.txt中,然后调用WebsysScript.exe
        /// </summary>
        /// <param name="mycode">jscripts或vbs代码</param>
        /// <param name="lang">语言类型:vbscript | jscript</param>
        /// <returns>""</returns>
        public static string CurrentUserRun(string mycode,String lang)
        {


            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Logging.Info("当前是管理员运行");
                //MessageBox.Show("shell="+Application.StartupPath);
                string curpath = Path.Combine(Application.StartupPath, @"temp");
                string oldCodePath = Path.Combine(curpath, "MyCode.txt");
                if (File.Exists(oldCodePath))
                {
                    File.Delete(oldCodePath);
                }
                
                String myTxtFileName = "MyCode" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt";
                string path = Path.Combine(curpath, myTxtFileName);
                Logging.Info("创建中间执行文件：" + path);
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("/*"+lang+"*/");
                    sw.Write(mycode);
                    sw.Close();
                }
                string rtn = null;
                //var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
                //var exe = Path.Combine(file.DirectoryName, file.Name.Replace(file.Extension, "") + ".exe");
                // 检测到当前进程是以管理员权限运行的，于是降权启动自己之后，把自己关掉。
                //Process.Start("explorer.exe", "cmd.exe");
                try {
                    Process p = new Process() {
                        StartInfo = {
                        FileName = "explorer.exe",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        //D:\workspace_net\WebsysServerSetup\WebsysScript\bin\x86\Debug\
                        // explorer.exe调用 exe 无法为exe传递参数
                        Arguments= Path.Combine(Application.StartupPath, @"WebsysScript.exe") ,  //"C:\\Windows\\system32\\cmd.exe", //@"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe", // "C:\\Windows\\system32\\cmd.exe", //@"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe " + lang+" \""+mycode+"\"",
                        UseShellExecute = false,    //是否使用操作系统shell启动
                        RedirectStandardInput = true,//接受来自调用程序的输入信息
                        RedirectStandardOutput = true,//由调用程序获取输出信息
                        RedirectStandardError = true,//重定向标准错误输出
                        CreateNoWindow = true,       //不显示程序窗口
                    }
                    };
                    p.Start();
                    //, @"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe " + lang+" \""+mycode+"\""); // Assembly.GetEntryAssembly().Location);
                    //p.StandardInput.WriteLine(cmd);
                    p.StandardInput.AutoFlush = true;
                    p.Close();
                } catch(Exception processExp) {
                    File.Delete(path); // 360会阻止运行,导到生成了txt后，不运行，影响下次运行时，txt运行错乱，程序一直在下面while卡死
                    rtn = "启动新process运行脚本出错,删除脚本。可能是杀毒软件影响，或权限问题";
                    Logging.Error(rtn);
                    Logging.Error(processExp);
                    return rtn;
                }
                //p.WaitForExit(); // 等待的是explorer.exe结束，而不是WebsysScript.exe的运行结束

                /*Process[] processes = null;
                processes = System.Diagnostics.Process.GetProcessesByName("cmd");
                Process process = default(Process);
                foreach (Process tempLoopVar_process in processes)
                {
                    process = tempLoopVar_process;
                    process.StandardInput.Write("ipconfig");
                    string cmdRtn = process.StandardOutput.ReadToEnd();
                    return cmdRtn;
                }*/
                // String outPutStr = p.StandardOutput.ReadToEnd(); // 20221104  这个返回值是explorer.exe进程的，不是WebsysScript.exe的返回值
                

                //System.Threading.Timer timer = new System.Threading.Timer(onTimedEventInCurrentUserRun, path, 1000, 1000);
                int loopMax = 600; // 600次即 600秒 = 10分钟
                Boolean foundResult = false;
                while (loopMax-->0) {
                    System.Threading.Thread.Sleep(1500);
                    string myHandlerTxtFileName = RenameToRuningFile(path);
                    if (!File.Exists(myHandlerTxtFileName)) continue;
                    try {
                        String line = "";
                        using (StreamReader txtsr = File.OpenText(myHandlerTxtFileName)) {
                            while (!foundResult && !txtsr.EndOfStream) {
                                line = txtsr.ReadLine();
                                if (line.IndexOf("WebsysScriptRESULT") == 0) {
                                    foundResult = true;
                                    rtn = line.Substring("WebsysScriptRESULT".Length + 1);
                                }
                            }
                            txtsr.Close();
                        }
                        if ("*/".Equals(line)) { //最后一行是注释 , 说明运行报错了
                            using (StreamReader txtsr = File.OpenText(myHandlerTxtFileName)) {
                                while (!foundResult && !txtsr.EndOfStream) {
                                    line = txtsr.ReadLine();
                                    /*把报错信息放到rtn中*/
                                    if (line.IndexOf("WebsysScript：")>-1) {
                                        foundResult = true;
                                        while (!txtsr.EndOfStream) {
                                            line = txtsr.ReadLine ();
                                            if ("*/".Equals(line)) break;
                                            rtn += line;
                                        }
                                    }
                                }
                                txtsr.Close();
                            }
                        }
                        File.Delete(myHandlerTxtFileName);
                    } catch (Exception ex) {
                        rtn = "ERRORWebsysScriptRESULT^" + myHandlerTxtFileName + ",Error:" + ex.Message;
                        foundResult = true;
                    }
                    if (!foundResult) continue;  // 没找到文件 或 没有运行完成 则继续循环
                    rtn = System.Web.HttpUtility.UrlDecode(rtn, System.Text.Encoding.GetEncoding("utf-8"));
                    break;
                }
                //sr.Close();
                return rtn;
                //Environment.Exit(0);
            }
            else
            {
                return EvalJs(mycode, lang);
            }
            return "";
        }

        private static void onTimedEventInCurrentUserRun(object state) {
            Console.WriteLine(state as string);
            throw new NotImplementedException();
        }

        public static string CurrentUserEvalJs(string str, string lang)
        {
            return CurrentUserRun(str,lang);

        }
        public static string Run(string cmd,Boolean isNotReturn)
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
            if (isNotReturn) {
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
        // 对应前台的EvalJs, 主要用于导出/打印Excel
        public static string EvalJs(string str,string language) //, string allowUI = "false", string language = "JScript")
        {
                  
            var a = 1;
            string rtn = "";
            MSScriptControl.ScriptControlClass s = new MSScriptControl.ScriptControlClass();
            s.AllowUI = true;
            s.Timeout =  Properties.Settings.Default.EvalJsTimeout * 60 * 1000; // 导出Excel 1000行*20列的数据 JS花费1分钟左右，VB下2秒左右
            s.UseSafeSubset = false;  // false表示低安全运行。设置为true时报： "ActiveX 部件不能创建对象: 'Excel.Application'"
            IntPtr hWndint = GetForegroundWindow();
            s.SitehWnd = hWndint.ToInt32();   //指定弹出窗口在当前弹出，默认为桌面
            if ("vbscript".Equals(language.ToLower()))
            {
                s.Language = "VBScript";
                if (0 != str.ToLower().IndexOf("function")) str = "Function vbs_Test\n" + str + "vbs_Test = 1\n End Function \n";  //默认返回空值，如果业务上有返回也不影响
                s.Reset();
                s.AddCode(str);
                rtn = s.Run("vbs_Test").ToString();
            }
            else
            { 
                if (0!=str.IndexOf( "(function" )) str = "(function test(){" + str + "return 1;})();";  //默认返回空值，如果业务上有返回也不影响
                s.Language = "JScript";
                s.Reset();
                rtn = s.Eval(str).ToString();
            }
            s = null;
            return rtn;

            Type obj = Type.GetTypeFromProgID("ScriptControl");
            if (obj == null) return null;
            object ScriptControl = Activator.CreateInstance(obj);
            //obj.InvokeMember("AllowUI", BindingFlags.SetProperty, null, ScriptControl, new object[] { allowUI });
            obj.InvokeMember("Language", BindingFlags.SetProperty, null, ScriptControl, new object[] { language }); //JScript
            rtn = obj.InvokeMember("Eval", BindingFlags.InvokeMethod, null, ScriptControl, new object[] { str }).ToString();
            obj = null;
            ScriptControl = null;
            return rtn;
        }
        public static string GetConfig()
        {
            CGI cgi = new CGI();
            string rtn = "{\"IP\":\"" + cgi.CurIPAddress + "\",\"Mac\":\"" + cgi.CurMacAddress + "\",\"HostName\":\"" + cgi.HostName + "\",\"IPS\":[";
            for(var i=0; i< cgi.IPList.Count();i++){
                if (i==0) rtn +="\"" +cgi.IPList[i]+"\""; //<string>[i];
                else rtn +=",\""+ cgi.IPList[i]+"\""; //<string>[i];
            }
            return rtn + "]}";
        }
        public static string Regsvr32(string fileName)
        {

            Process p = new Process();
            p.StartInfo.FileName = "regsvr32.exe";
            p.StartInfo.Arguments = "/s \"" + fileName + "\"";
            p.Start();
            return "1";
        }
        public static string Msiexec(string fileName)
        {
            Process p = new Process();
            p.StartInfo.FileName = "msiexec.exe"; // "/i \"" + regFile + "\" /qb";
            p.StartInfo.Arguments = "/i \"" + fileName + "\" /qb";
            p.Start();
            return "1";
        }
        //解压CAB
        public static string Expand(string srcCabFile, string dir)
        {
            Process p = new Process();
            p.StartInfo.FileName = "EXPAND.exe";
            p.StartInfo.Arguments = "-F:*.* \"" + srcCabFile + "\" \"" + dir + "\"";
            p.Start();
            return "1";
        }
        //安装inf
        public static string InfDefaultInstall(string infFile)
        {
            Process p = new Process();
            p.StartInfo.FileName = "InfDefaultInstall.exe";
            p.StartInfo.Arguments = "\"" + infFile + "\"";
            p.Start();
            return "1";
        }
        /**
         * 注册inf内指定要注册的DLL
         */
        public static string RegInfDll(string infFile)
        {
            String LocalDllPath = infFile.Substring(0,infFile.LastIndexOf("\\"));
            String[] infInfo = File.ReadAllLines(infFile);
            Boolean regFlag = false;
            for (int i = 0; i < infInfo.Length; i++)
            {
                if (regFlag)
                {
                    if (infInfo[i].StartsWith("%")) {

                        ScriptShell.Regsvr32(LocalDllPath + infInfo[i].Substring(infInfo[i].LastIndexOf("\\")));   // 注册CAB包中%11%\trakWebEdit3.dll ，暂时只考虑单目录
                    }else{
                        ScriptShell.Regsvr32(LocalDllPath + infInfo[i]);   //注册chrome.inf
                    }
                    //ScriptShell.Run("regsvr32.exe /s \"" + LocalDllPath +infInfo[i] + "\"");
                }
                if (infInfo[i].StartsWith("["))
                {
                    if (infInfo[i].ToLower().Equals("[registerfiles]"))
                    {
                        regFlag = true;
                    }
                    else
                    {//下一类型
                        regFlag = false;
                    }
                }
            }
            return "1";

        }
        /*处理 http://localhost:11996/websys/cmd/cmd 请求 */
        public string CmdRun(string query)
        {
            string lastMthRtn = "";
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++)
            {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm))
                {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue = HTTPRequestHandler.decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                    if (key.StartsWith("_dllDir")) continue;
                    if (key.StartsWith("_version")) continue;
                    if (key.ToUpper().StartsWith("M_RUN"))
                    {

                        if (deValue.Contains(HTTPRequestHandler.PCOUNT))
                        {
                            string[] Arg = HTTPRequestHandler.ParseMthArg(deValue);
                            if (Arg.Length > 0)
                            {
                                lastMthRtn = ScriptShell.Run(Arg[0]);
                            }
                        }
                    }
                    else if (key.ToUpper().StartsWith("M_EVALJS"))
                    {
                        if (deValue.Contains(HTTPRequestHandler.PCOUNT))
                        {
                            string[] Arg = HTTPRequestHandler.ParseMthArg(deValue);
                            if (Arg.Length > 0)
                            {
                                string lang = "JScript";
                                if (Arg.Length > 1)
                                {
                                    lang = Arg[1];
                                }
                                lastMthRtn = ScriptShell.EvalJs(Arg[0], lang);
                            }
                        }
                    }
                    else if(key.ToUpper().StartsWith("M_CURRENTUSEREVALJS"))
                    {
                        if (deValue.Contains(HTTPRequestHandler.PCOUNT))
                        {
                            string[] Arg = HTTPRequestHandler.ParseMthArg(deValue);
                            if (Arg.Length > 0)
                            {
                                string lang = "JScript";
                                if (Arg.Length > 1)
                                {
                                    lang = Arg[1];
                                }
                                lastMthRtn = ScriptShell.CurrentUserEvalJs(Arg[0], lang);
                            }
                        }
                    }
                    else if (key.ToUpper().StartsWith("M_GETCONFIG"))
                    {
                        lastMthRtn = ScriptShell.GetConfig();
                    }
                }
            }
            return lastMthRtn;
        }
        /// <summary>
        /// 调用WebsysScript.exe来运行myCode[xxx].txt
        /// </summary>
        /// <param name="myTxtFileName">运行文件的全路径</param>
        /// <param name="LocalDllStoreFile">本地动态库目录</param>
         /// <returns></returns>
        public static string InvokeProcessWebsysScript_Old(string myTxtFileName,string LocalDllStoreFile) {

            Process p = new Process() {
                StartInfo = {
                        FileName =Path.Combine(Application.StartupPath, @"WebsysScript.exe"),
                        WorkingDirectory = Path.GetDirectoryName(LocalDllStoreFile),
                        Arguments = myTxtFileName ,  //"C:\\Windows\\system32\\cmd.exe", //@"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe", // "C:\\Windows\\system32\\cmd.exe", //@"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe " + lang+" \""+mycode+"\"",
                        UseShellExecute = false,    //是否使用操作系统shell启动
                        RedirectStandardInput = true,//接受来自调用程序的输入信息
                        RedirectStandardOutput = true,//由调用程序获取输出信息
                        RedirectStandardError = true,//重定向标准错误输出
                        CreateNoWindow = true,//获取或设置指示是否在新窗口中启动该进程的值。
                    }
            };
            p.Start();
            //, @"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe " + lang+" \""+mycode+"\""); // Assembly.GetEntryAssembly().Location);
            //p.StandardInput.WriteLine(cmd);
            p.StandardInput.AutoFlush = true;
            p.WaitForExit(); // 等待的是explorer.exe结束，而不是WebsysScript.exe的运行结束
            
            //string rtn  = p.StandardOutput.ReadToEnd(); // 读到的结果有回车符
            StreamReader sr = p.StandardOutput;
            //string tempFirstLine = sr.ReadLine();
            StringBuilder sb = new StringBuilder();
            while (!sr.EndOfStream) {
                sb.Append(sr.ReadLine());
            }
            string rtn = sb.ToString(); // sr.ReadToEnd();-> 会得到每行最后面的【回车换行符】\r\n
            rtn = System.Web.HttpUtility.UrlDecode(rtn, System.Text.Encoding.GetEncoding("utf-8"));
            sr.Close();
            p.Close();
            return rtn;
        }
        /// 先WaitForExit会卡住父进程，也会等待子进程运行，有些此时子进程（mispos,读卡）会等待父进程的StandarOutput流，所以应先读取标准Output流，然后再WaitForExit
        /// https://docs.microsoft.com/zh-cn/dotnet/api/system.diagnostics.process.standardoutput?redirectedfrom=MSDN&view=net-6.0#System_Diagnostics_Process_StandardOutput
        /// 
        /// <summary>
        /// 20220809因为mispos与读卡会卡死问题修改，WaitForExit在读Output流前,会导致父子进程死锁。优化逻辑如下：
        /// 调用WebsysScript.exe来运行myCode[xxx].txt,然后把结果写入--myCode[xxx].txt的最后一行
        /// 在进程结束后,再次读取--myCode[xxx].txt文件的最后一行结果，然后再删除--myCode[xxx].txt文件
        /// 
        /// </summary>
        /// <param name="myTxtFileName">运行文件的全路径</param>
        /// <param name="LocalDllStoreFile">本地动态库目录</param>
        /// <returns></returns>
        public static string InvokeProcessWebsysScript(string myTxtFileName, string LocalDllStoreFile) {

            Process p = new Process() {
                StartInfo = {
                        FileName =Path.Combine(Application.StartupPath, @"WebsysScript.exe"),
                        WorkingDirectory = Path.GetDirectoryName(LocalDllStoreFile),
                        Arguments = myTxtFileName+" 0",  //@"D:\workspace_net\WebsysServerSetup\WebsysScript\bin\Debug\WebsysScript.exe" 0表示使用debug模式运行，发现不传0调用mispos时，取消支付时，会崩亏，和WebsysScript写有关
                        UseShellExecute = false,    //是否使用操作系统shell启动
                        //RedirectStandardInput = false,//接受来自调用程序的输入信息
                        //RedirectStandardOutput = true,//由调用程序获取输出信息
                        //RedirectStandardError = true,//重定向标准错误输出
                        CreateNoWindow = true,//获取或设置指示是否在新窗口中启动该进程的值。
                    }
            };
            p.Start();
            
            /*
             * 例如 Read， ReadLine对 ReadToEnd 进程的输出流执行同步读取操作等方法。 
             * 在关联 Process 写入流 StandardOutput 或关闭流之前，这些同步读取操作不会完成。
             * 一定要先读流再WaitForExit，不然会导致死锁
             */
            /*StreamReader sr = p.StandardOutput;
            string rtn  = sr.ReadToEnd(); // 读到的结果有回车符
            */
            p.WaitForExit();  // 等待WebsysScript.exe的运行结束
            string rtn = null;
            Boolean foundResult = false;
            string myHandlerTxtFileName = RenameToRuningFile(myTxtFileName);
            try {
                using (StreamReader txtsr = File.OpenText(myHandlerTxtFileName)) {
                    while (!foundResult && !txtsr.EndOfStream) {
                        string txt = txtsr.ReadLine();
                        if (txt.IndexOf("WebsysScriptRESULT") == 0) {
                            foundResult = true;
                            rtn = txt.Substring("WebsysScriptRESULT".Length + 1);
                            rtn += txtsr.ReadToEnd(); //不只是第一行是结果，也把后面行的值拼到字符串上 2024-05-15 by wanghc
                        }
                    }
                    txtsr.Close();
                }
                File.Delete(myHandlerTxtFileName);
            } catch (Exception ex) {
                rtn = "ERRORWebsysScriptRESULT^" + myHandlerTxtFileName +",Error:"+ ex.Message;
            }
            rtn = System.Web.HttpUtility.UrlDecode(rtn, System.Text.Encoding.GetEncoding("utf-8"));
            //sr.Close();
            p.Close();
            return rtn;
        }
        public static string RenameToRuningFile(string path) {
            string file_path = Path.GetDirectoryName(path);
            string file_name = Path.GetFileName(path);
            string newPath = Path.Combine(file_path, "--" + file_name);
            return newPath;
        }
    }
}
