﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using WebsysServer.tool;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WebsysServer
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Form1 : Form
    {
        Thread sthread;
        HTTPServer httpServer;
        public String RestartApplicationNote = "";
        public static string version = Application.ProductVersion; // 1.0.0.10
        public Form1()
        {
            // 手动转换语言

            //WebsysServer.Properties.Resources.Culture = new System.Globalization.CultureInfo("en");
            
            InitializeComponent();
            this.AboutToolStripMenuItem1.Text = WebsysServer.Properties.Resources.About;
            this.FormBorderStyle = FormBorderStyle.None;
            //MessageBox.Show(System.Globalization.CultureInfo.CurrentCulture.Name);
            //this.ShowInTaskbar = false;
            //this.ShowIcon = false;
            cbLogLevel.Items.Add("Debug");
            cbLogLevel.Items.Add("Info");
            cbLogLevel.Items.Add("Warn");
            cbLogLevel.Items.Add("Error");
            int LogLevel = Properties.Settings.Default.LogLevel;
            int urlPort = Properties.Settings.Default.HttpServerPort;
            string urlServer = Properties.Settings.Default.HttpServerIP;
            string urlApplication = Properties.Settings.Default.HttpServerApplication;
            this.AboutToolStripMenuItem1.Text = WebsysServer.Properties.Resources.About;
            this.ExitToolStripMenuItem.Text = WebsysServer.Properties.Resources.Exit;
            this.MgrToolStripMenuItem.Text = WebsysServer.Properties.Resources.Management;
            this.hTTPSToolStripMenuItem.Text = WebsysServer.Properties.Resources.ManageHttps;
            this.hTTP界面ToolStripMenuItem.Text = WebsysServer.Properties.Resources.ManageHttp;
            cbLogLevel.SelectedIndex = LogLevel;
            tbPort.Text = urlPort.ToString();
            lbUrl.Text = "监听服务路径 http://" + urlServer + ":" + urlPort + urlApplication;
            CGI.LocalInstall = Path.Combine(System.Windows.Forms.Application.StartupPath, "");
            moveScriptTxt(Path.Combine(CGI.LocalInstall));
            /*var conf = JinianNet.JNTemplate.Configuration.EngineConfig.CreateDefault();
            conf.StripWhiteSpace = false;
            conf.ResourceDirectories = new string[] { Path.Combine(System.Windows.Forms.Application.StartupPath, @"tmpl\scripts") };
            JinianNet.JNTemplate.Engine.Configure(conf);
            */
            //var verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ProductVersion);
        }
        /*
         * 2023-08-18  by wanghc
         * 修复某些电脑不允许删除空目录问题。
         * 当目录下有文件时，不能删除并报错，导致中间件不能启动问题
         */
        public void DeleteFileWithFileType(string directory, string fileType) {
            var allFileNames = new List<string>();
            allFileNames.AddRange(Directory.GetFiles(directory, fileType));
            // 根据条件删除文件
            foreach (var item in allFileNames)
                File.Delete(item);
            // 删除空文件夹
            Directory.Delete(directory);
        }
        public string moveScriptTxt(string path) {
            try {
                var now = DateTime.Now;
                var day = now.Day; //Convert.ToInt32(now.DayOfWeek.ToString("d")); // 只备份7天的日志
                string newPath = Path.Combine(path, "bak_temp", day.ToString());
                if (Directory.Exists(newPath)) {
                    DirectoryInfo newDi = new DirectoryInfo(newPath);
                    if (0 != newDi.CreationTime.Date.CompareTo(now.Date)) {
                        DeleteFileWithFileType(newPath, "*.txt");
                        //Directory.Delete(newPath);
                    }
                }
                Directory.CreateDirectory(newPath);
                String myTemp = Path.Combine(path, "temp");
                if (Directory.Exists(myTemp)) {   // 增加判断，不然会因为目录不存在导致中间件无法重新启动 [3530041]
                    DirectoryInfo tempDir = new DirectoryInfo(Path.Combine(path, "temp"));
                    FileInfo[] fis = tempDir.GetFiles();
                    foreach (FileInfo fi in fis) {
                        if (!fi.Name.Equals("console.log")) {  // 把非console.log的日志都移走，避免运行错误脚本 [3349949]
                            fi.MoveTo(Path.Combine(newPath, fi.Name));
                        }
                    }
                } else {
                    Directory.CreateDirectory(myTemp);
                }
            } catch (Exception ex) {

            }
            return "";
        }
        public string ShutDownHttpServer()
        {
            try
            {
                int loopNum = 10;
                while (httpServer.IsAlive && loopNum > 0)
                {
                    httpServer.Dispose();  //只停止sthread.Abort，监听不会停止
                    loopNum--;
                    Logging.Warn("停止服务器线程" + sthread.ManagedThreadId.ToString("00"));
                    Thread.Sleep(200);
                    //sthread.Abort();
                    //sthread = null;
                }
                if (!httpServer.IsAlive)
                {
                    sthread.Abort();
                    sthread = null;
                }
            } catch (Exception e)
            {
                MessageBox.Show(WebsysServer.Properties.Resources.Tip, WebsysServer.Properties.Resources.AppStopListenError  +"：" + e.Message);
                return "-1";
            }
            return "";
        }
        public string StartUpHttpServer()
        {
            try
            {
                //开启一个线程处理收到的数据
                httpServer = new HTTPServer();
                httpServer.mainForm = this;
                sthread = new Thread(httpServer.Start);
                sthread.SetApartmentState(ApartmentState.STA);
                sthread.Name = "S" + sthread.ManagedThreadId; ;
                sthread.Start();
                Logging.Warn("启动服务器线程" + sthread.ManagedThreadId.ToString("00"));
            } catch (Exception e)
            {
                MessageBox.Show(WebsysServer.Properties.Resources.Tip, WebsysServer.Properties.Resources.AppStartListenError + "：" + e.Message);
                return "-1";
            }
            return "";
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            // 设置日志级别 及 开启日志记录
            int logLevel = cbLogLevel.SelectedIndex;
            Logging.CurLogLevel = logLevel;

            Properties.Settings.Default.LogLevel = logLevel;
            if ("".Equals(StartUpHttpServer())) {
                msg.Text = "启动成功";
                btnStart.Enabled = false;
                btnStart.Text = "已启动";
                btnStop.Enabled = true;
                btnStop.Text = "停止";
                cbLogLevel.Enabled = false;
            }
        }
        List<ManualResetEvent> manualResetEvents = new List<ManualResetEvent>();
        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Text = "停止中...";
            btnStop.Enabled = false;
            if ("".Equals(ShutDownHttpServer()))
            {
                btnStop.Text = "已停止";
                btnStart.Enabled = true;
                btnStart.Text = "启动";
                cbLogLevel.Enabled = true;
            }
            Logging.ColseLogFile();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;

            //notifyIcon1.Icon = Icon.FromHandle( Properties.Resources.icon_info.GetHbitmap());
            this.Hide();
            //btnStart_Click(sender, e);
            //this.WindowState = FormWindowState.Minimized;
            StartupToolStripMenuItem_Click(sender, e);
            Boolean isProStoped = true;
            foreach (var process in Process.GetProcessesByName("WebsysServerPro"))
            {
                isProStoped = false;
            }
            if (isProStoped)
            {
                RegistryKey rk = Registry.ClassesRoot;
                if (rk.OpenSubKey(@"RunWebsysServer\Shell\open\command") != null)
                {
                    string command = rk.OpenSubKey(@"RunWebsysServer\Shell\open\command").GetValue("").ToString();
                    string path = command.Substring(1, command.IndexOf(".exe") - 1) + "Pro.exe";
                    Process.Start(path);
                }
            }
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //this.WindowState = FormWindowState.Normal;
            //this.Show();
            //this.notifyIcon1.Visible = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (httpServer != null && httpServer.IsAlive)
            {
                ShutDownToolStripMenuItem_Click(sender, e);
            }
            // 退出时结束保护
            foreach (var process in Process.GetProcessesByName("WebsysServerPro"))
            {
                try
                {
                    process.Kill();
                    Logging.Warn("结束 保护程序Pro!");
                }
                catch (Win32Exception ex)
                {
                    Logging.Error(ex, "结束 保护程序Pro");
                }
                catch (InvalidOperationException ex2)
                {
                    Logging.Error(ex2, "结束 保护程序Pro");
                }
            }
            this.Close();
        }

        private void StartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 设置日志级别 及 开启日志记录
            int logLevel = cbLogLevel.SelectedIndex;
            Logging.CurLogLevel = logLevel;
            //Logging.OpenLogFile();  //打开日志移到Program.cs中
            Properties.Settings.Default.LogLevel = logLevel;
            //把配置读到对象中
            if ("".Equals(StartUpHttpServer())) {
                StartupToolStripMenuItem.Enabled = false;
                ShutDownToolStripMenuItem.Enabled = true;
                //notifyIcon1.Icon = Icon.FromHandle(Properties.Resources.ico256.GetHicon());
                //notifyIcon1.Icon = Properties.Resources.ico256;
                notifyIcon1.Icon = this.Icon;
            }
            startKeyListen();
        }

        private void ShutDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ("".Equals(ShutDownHttpServer())) {
                ShutDownToolStripMenuItem.Enabled = false;
                StartupToolStripMenuItem.Enabled = true;
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                //notifyIcon1.Icon = Properties.Resources.ico256gray;// (System.Drawing.Icon)resources.GetObject(Properties.Resources.ico256gray);
                notifyIcon1.Icon = this.Icon; // Icon.FromHandle(Properties.Resources.ico256gray.GetHicon());
            }
            stopKeyListen();
            Logging.ColseLogFile();
        }

        private KeyEventHandler myKeyEventHandeler = null;//按键钩子
        private KeyboardHook k_hook = null;

        [DllImport("user32")]
        private static extern IntPtr LoadCursorFromFile(string fileName);

        [DllImport("User32.DLL")]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);
        public const uint OCR_NORMAL = 32512;
        public const uint SPI_SETCURSORS = 87;
        public const uint SPIF_SENDWININICHANGE = 2;
        [DllImport("User32.DLL")]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);
        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            /// (int)Control.ModifierKeys == (int)Keys.Alt
            if ((int)Control.ModifierKeys == (int)Keys.Control && e.KeyCode == (Keys)Properties.Settings.Default.CursorShowHotKey) //Keys.Oemtilde)
            {
                // 安装到 本地计算机-受信任的根证书颁发机构
                string contentPath = System.AppDomain.CurrentDomain.BaseDirectory;   //..bin/x86/Debug    //Application.StartupPath
                string certPath = System.IO.Path.Combine(contentPath, "LinkPixelatedRed.ani"); // "MediWayCA.crt");
                //设置样式
                IntPtr iP = LoadCursorFromFile(certPath); //hellblazer.cur");
                int winHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                int winWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                Point centerP = new Point(winWidth / 2, 10);
                SetCursorPos(centerP.X, centerP.Y);
                SetSystemCursor(iP, OCR_NORMAL);
                SetTimeout(3000, () =>
                {
                    Action action = delegate ()
                    {
                        // 还原样式
                        SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_SENDWININICHANGE);
                    };
                    this.Invoke(action);
                });
            }
        }
        public static void SetTimeout(double interval, Action action)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                timer.Enabled = false;
                action();
            };
            timer.Enabled = true;
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        public void startKeyListen()
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                k_hook = new KeyboardHook();
                myKeyEventHandeler = new KeyEventHandler(hook_KeyDown);
                k_hook.KeyDownEvent += myKeyEventHandeler;//钩住键按下
                k_hook.Start();//安装键盘钩子
            }
        }
        /// <summary>
        /// 结束监听
        /// </summary>
        public void stopKeyListen()
        {
            Screen[] screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                if (myKeyEventHandeler != null)
                {
                    k_hook.KeyDownEvent -= myKeyEventHandeler;//取消按键事件
                    myKeyEventHandeler = null;
                    k_hook.Stop();//关闭键盘钩子
                    k_hook = null;
                }
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (httpServer != null && httpServer.IsAlive)
            {
                ShutDownToolStripMenuItem_Click(sender, e);
            }
            //notifyIcon1.Visible = false;
        }

        private void AboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //1.0.4 -- /zip解压，注册chrome.inf
            //1.0.5 -- /websys/mgr/index.html
            //1.0.6 -- /当前程序只能启动一个
            //1.0.7 -- /删除HKEY_LOCAL_MACHINE注册表启动 , 增加mgr/mgr?M_GETVERSION管理
            //1.0.11
            //1.0.12 -- 管理界面增加父目录，左下角小条去除。by 20200228
            //1.0.13 -- 2020-03-06 
            //           1.下载文件支持HTTPS
            //           2.扩展notReturn参数,进行异步调用客户端程序
            //           3.方法调用时，默认参数默认值 
            //1.0.14 -- 2020-03-15
            //           1.支持下载CAB包且注册inf中RegisterFiles下对应dll
            //var scl = new MSScriptControl.ScriptControlClass();
            //scl.Language = "jscript";
            //scl.Eval("var a=1;");
            //scl = null;
            //var t = new trakWebEdit3.TrakWebClass();
            //t.ShowLayout("","","","");
            String osStr = Environment.OSVersion.ToString();
            String netStr = Environment.Version.ToString();
            String installPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            String content = WebsysServer.Properties.Resources.MediwayClientManagementVersion + " : " + version + "{0} \r\n"+ WebsysServer.Properties.Resources.OSVersion +" : " + osStr + "\r\nframework : " + netStr + "\r\n "+ WebsysServer.Properties.Resources.ApplicationInstallationPath + " : " + installPath + "\r\n";
            if ("".Equals(RestartApplicationNote) ){
                content += WebsysServer.Properties.Resources.NormalRestart+"\r\n";
            }else{
                content += RestartApplicationNote+ "\r\n";
            }
            System.Security.Principal.WindowsIdentity current = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (new System.Security.Principal.WindowsPrincipal(current).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                content = string.Format(content, "(" + WebsysServer.Properties.Resources.Administrators + ")");
            }
            content = string.Format(content, "("+ WebsysServer.Properties.Resources.Administrators+")");
          
            MessageBox.Show(content, WebsysServer.Properties.Resources.iMedicalPluginManagement, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void MgrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*  www.cnblogs.com/xsj1989/p/4964350.html
             * 从注册表中读取默认浏览器可执行文件路径 
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
            string s = key.GetValue("").ToString();
            //s就是你的默认浏览器，不过后面带了参数，把它截去，不过需要注意的是：不同的浏览器后面的参数不一样！ 
            //"D:\Program Files (x86)\Google\Chrome\Application\chrome.exe" -- "%1" 
            System.Diagnostics.Process.Start(s.Substring(0, s.Length - 8), "http://blog.csdn.net/testcs_dn");
            */
            try
            {
                Process.Start("http://localhost:" + WebsysServer.Properties.Settings.Default.HttpServerPort + WebsysServer.Properties.Settings.Default.HttpServerApplication + "mgr/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(WebsysServer.Properties.Resources.Tip, WebsysServer.Properties.Resources.DefaultBrowserNotFound + ex.Message);
            }
        }

        private void hTTPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://localhost:" + Properties.Settings.Default.HttpsServerPort + Properties.Settings.Default.HttpServerApplication + "mgr/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(WebsysServer.Properties.Resources.Tip, WebsysServer.Properties.Resources.DefaultBrowserNotFound  + ex.Message);
            }
        }

        private void hTTP界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://localhost:" + Properties.Settings.Default.HttpServerPort + Properties.Settings.Default.HttpServerApplication + "mgr/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(WebsysServer.Properties.Resources.Tip, WebsysServer.Properties.Resources.DefaultBrowserNotFound + ex.Message);
            }
        } 
        
    }
}
