using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WebsysServer.tool
{
    class Mgr
    {
        private string lpClassName = "";
        private string lpWindowName = "";
        private Int16 focusLazyTime = 1000; /*延时1000ms*/
        public Mgr()
        {
            lpClassName = "";
            lpWindowName = "";
            focusLazyTime = 1000;
        }
        public Mgr(string lpClassName, string lpWindowName,string focusLazyTime)
        {
            this.lpClassName = lpClassName ;
            this.lpWindowName = lpWindowName;
            if(Int16.TryParse(focusLazyTime, out short time))
            {
                this.focusLazyTime = time;
            }
            else
            {
                this.focusLazyTime = 1000;
            }
        }
        /*
         * 查询插件目录
         */
        public String pluginList()
        {
            //自动启动时目录默认到c:\windows\wowsys64\下
            string pluginPath = CGI.Combine("plugin");
            if (!Directory.Exists(pluginPath))  
            {
                Directory.CreateDirectory(pluginPath);
            }
            DirectoryInfo TheFolder = new DirectoryInfo(pluginPath);
            //Logging.Debug(TheFolder.Parent.FullName);
            //Logging.Debug(TheFolder.Root.Name);
            String json = "..^";
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                DirectoryInfo versionFolder = new DirectoryInfo(pluginPath+"\\" + NextFolder.Name);
                foreach (DirectoryInfo versionNextFolder in versionFolder.GetDirectories())
                {
                    if ("".Equals(json))
                    {
                        json = NextFolder.Name + "^" + versionNextFolder.Name;
                    }
                    else
                    {
                        json += "," + NextFolder.Name + "^" + versionNextFolder.Name;
                    }
                }
            }
            return json;
            //lastMthRtn = tool.ScriptShell.Run("dir");
        }
        // 打开对应目录
        public string pluginDirOpen(string query)
        {
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++)
            {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm))
                {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue =HTTPRequestHandler.decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                    if (key.StartsWith("dir"))
                    {
                        string path = System.Windows.Forms.Application.StartupPath;
                        string[] str = deValue.Split('/');
                        for (int k = 0; k < str.Length; k++)
                        {
                            if ("".Equals(str[k]) || "..".Equals(str[k]))
                            { }
                            else
                            {
                                path += "\\" + str[k];

                            }

                        }
                        System.Diagnostics.Process.Start("explorer.exe", path); //+"\\"+deValue.Replace("/","\\"));
                    }
                }
            }
            return "";
        }
        public string logLevel(string[] arr)
        {
            string lastMthRtn="";
            string loglevel = "3";
            if (arr.Length > 4){
                //设置日志等级
                loglevel = arr[4];

                //ConfigurationManager.RefreshSection("appSettings");
                Properties.Settings.Default.LogLevel = int.Parse(loglevel);
                Properties.Settings.Default.Save();
                Logging.CurLogLevel = int.Parse(loglevel);
                lastMthRtn = "200";
            }else{
                //查询当前日志等级
                lastMthRtn = "{\"level\":" + Logging.CurLogLevel + ",\"file\":\"" + CGI.LocalInstall.Replace("\\", "\\\\") + "\\\\temp\\\\console.log\"}";
            }
            return lastMthRtn;
        }
        // 获得插件信息
        public string MgrVersion(string query)
        {
            string lastMthRtn = "";
            //query中有参数
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++)
            {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm))
                {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue =HTTPRequestHandler.decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                    if (key.StartsWith("_dllDir")) continue;
                    if (key.StartsWith("_version")) continue;
                    if (key.ToUpper().StartsWith("M_GETVERSION"))
                    {
                        lastMthRtn = Form1.version;
                    }
                }
            }
            return lastMthRtn;
            
        }

        public string ScreensList()
        {
            String rtn = "{\"screens\":[";
            Screen[] screens = Screen.AllScreens;
            int upperBound = screens.GetUpperBound(0);
            int index;
            for (index = 0; index <= upperBound; index++){
                if (index > 0) { rtn += ","; }
                // For each screen, add the screen properties to a list box.
                //"{\"DeviceName\":\"" + screens[index].DeviceName+'"'
                //+ ",\"Type\":\"+screens[index].Type.toStirng()+\""
                
                rtn += "{\"Bounds\":" 
                + "{\"X\":" + screens[index].Bounds.X + ",\"Y\":" + screens[index].Bounds.Y + ",\"Width\":" + screens[index].Bounds.Width + ",\"Height\":" + screens[index].Bounds.Height + "}"
                + ",\"WorkingArea\":"
                + "{\"X\":" + screens[index].WorkingArea.X + ",\"Y\":" + screens[index].WorkingArea.Y + ",\"Width\":" + screens[index].WorkingArea.Width + ",\"Height\":" + screens[index].WorkingArea.Height + "}"
                + ",\"PrimaryScreen\":" + (screens[index].Primary ? "true":"false") + "}";
            }
            rtn += "]}";
            return rtn;
        }
       

        
        
        public Dictionary<string, string> getDicByReqParam(string query)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
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
                    dic.Add(key, deValue);
                }
            }
            return dic;
        }
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll", EntryPoint = "MoveWindow", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(System.IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        private static extern IntPtr GetForegroundWindow();
        /***
         * 1-恢复，2最小化，3最大化, 8以窗口原来的状态显示窗口
         */
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
        /**
         bvk为键值，例如回车13，bScan设置为0，dwFlags设置0表示按下，2表示抬起；dwExtraInfo也设置为0即可。
            */
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetFocus", CharSet = CharSet.Auto)]
        public static extern IntPtr SetFocus(IntPtr hWnd);//设定焦点
        [DllImport("user32.dll", EntryPoint = "SetWindowPos", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public int GetInt(string value, int _default)
        {
            if (int.TryParse(value, out _default))
                return Convert.ToInt32(value);
            else
                return _default;
        }
        public string MoveWindow(Dictionary<string,string> dic)
        {
            var param = dic["M_moveWindow"];
            Dictionary<string, string> moveDic = getDicByReqParam(param);
            //if (!moveDic.ContainsKey("P_0")) { return "{\"error\":\"title is required param\"}"; }
            if (!moveDic.ContainsKey("P_1")) { return "{\"error\":\"x is required param\"}"; }
            if (!moveDic.ContainsKey("P_2")) { return "{\"error\":\"y is required param\"}"; }
            if (!moveDic.ContainsKey("P_3")) { return "{\"error\":\"width is required param\"}"; }
            if (!moveDic.ContainsKey("P_4")) { return "{\"error\":\"height is required param\"}"; }
            var title = moveDic["P_0"];
            var x = this.GetInt(moveDic["P_1"],0);
            var y = this.GetInt(moveDic["P_2"],0);
            var width = this.GetInt(moveDic["P_3"],400);
            var height = this.GetInt(moveDic["P_4"],400);
            var ex = "";
            if (moveDic.ContainsKey("P_5")) ex = moveDic["P_5"];
            IntPtr hWnd = new IntPtr(0);
            if (title.Equals("")) {
                hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero)
                {
                    return "{\"error\":\"not Find Foreground Window\"}";
                }
            }
            else{
                hWnd = FindWindow(null, title);
                if (hWnd == IntPtr.Zero)
                {
                    return "{\"error\":\"not Find " + title + "\"}";
                }
            }
            MoveWindow(hWnd, x, y, width, height, true);
            if (ex.Equals("ScreenF11"))
            {
                keybd_event((byte)Keys.F11, 0, 0, 0);
                keybd_event((byte)Keys.F11, 0, 2, 0);
            }else if (ex.Equals("ScreenMax"))
            {
                ShowWindow(hWnd, 3);
            }
            return "{\"error\":\"\",\"success\":\"true\"}";
        }
        /*线程方式必须无返回值*/
        public void FocusWindow()
        {
            if ("".Equals(lpClassName)) lpClassName = null;
            if ("".Equals(lpWindowName)) lpWindowName = null;
            System.Threading.Thread.Sleep(this.focusLazyTime);
            IntPtr maindHwnd = FindWindow(lpClassName, lpWindowName);
            if (maindHwnd != IntPtr.Zero)
            {
                int focusPtr = ShowWindow(maindHwnd, 1);
                const int HWND_TOPMOST  = -1;// 将窗口置于列表顶部，并位于任何最顶部窗口的前面
                const int SWP_NOSIZE = 1; // 保持窗口大小
                const int SWP_NOMOVE = 2; //保持窗口位置
                int focusPtr2 = SetWindowPos(maindHwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE); // '将窗口设为总在最前
                //IntPtr focusPtr2 = SetFocus(maindHwnd);
            }
        }
        public string MgrRun(string url, string query)
        {
            string[] arr = url.Split('?')[0].Split(new char[] { '/' });
            if (arr.Length < 4) return "";
            string op = arr[3];
            string lastMthRtn = "";

            if (op.ToLower().Equals("pluginlist"))
            {
                lastMthRtn = pluginList();
            }
            else if (op.ToLower().Equals("plugindiropen"))
            {
                lastMthRtn = pluginDirOpen(query);
            }
            else if (op.ToLower().Equals("loglevel"))
            {
                lastMthRtn = logLevel(arr);
            }
            else if (op.ToLower().Equals("mgr"))
            {
                var dic = getDicByReqParam(query);

                if (dic.ContainsKey("M_getScreens"))
                {
                    lastMthRtn = ScreensList();
                }
                else if (dic.ContainsKey("M_moveWindow"))
                {
                    // ?P_0=winTitle&P_1=x&P_2=y&P_3=width&P_4=height&P_5=ScreenF11
                    // ?P_0=winTitle&P_1=x&P_2=y&P_3=width&P_4=height&P_5=ScreenMax
                    lastMthRtn = MoveWindow(dic);
                }
                else
                {
                    lastMthRtn = MgrVersion(query);
                }
            }
            return lastMthRtn;
        }
    }
}
