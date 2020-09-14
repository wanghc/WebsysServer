using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebsysServer.tool
{
    class Mgr
    {
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
                lastMthRtn = MgrVersion(query);
            }
            return lastMthRtn;
        }
    }
}
