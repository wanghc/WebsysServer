using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace WebsysServer.tool
{
    class CGI
    {
        public static string LocalInstall = ""; //%user%/AppData/Local/MediWay/WebsysServer/
        public string HostName { get; set; } = "unkown";
        public List<String> IPList { get; set; } = new List<string>();
        public string CurIPAddress { get; set; } = "";
        public string CurMacAddress { get; set; } = "00:00:00:00:00:00";
        public static string Combine(string filePath)
        {
            return Path.Combine(CGI.LocalInstall, filePath);
        }
        public string[] GetIp()
        {
            string hn = Dns.GetHostName();
            System.Net.IPAddress[] addressList = Dns.GetHostEntry(hn).AddressList;//IP获取一个LIST里面有一个是IP
            foreach (IPAddress _IP in addressList)
            {
                if (_IP.AddressFamily.ToString() ==  "InterNetwork")
                {
                    IPList.Add(_IP.ToString());
                }
            }/*
            for (int i = 0; i < addressList.Length; i++)
            {

                //判断是否为IP的格式
                if (System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(addressList[i]), @"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)") == true)
                {
                    IPList.Add(addressList[i].ToString());
                    
                }
            }*/
            return null;
        }
        public string GetMac()
        {
            string MyMacAddress = "";
            Process p = null;
            StreamReader reader = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo("cmd.exe");
                start.FileName = "ipconfig";
                start.Arguments = "/all";
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.RedirectStandardInput = true;
                start.UseShellExecute = false;
                p = Process.Start(start);
                reader = p.StandardOutput;
                //读取当前行
                string line = reader.ReadLine();
                //循环到出现物理地址为止
                while (!reader.EndOfStream)
                {
                    
                    if (line.ToLower().IndexOf("physical address") > 0 || line.ToLower().IndexOf("物理地址") > 0)
                    {
                        int index = line.IndexOf(":");
                        index += 2;
                        MyMacAddress = line.Substring(index);
                        CurMacAddress = MyMacAddress.Replace('-', ':');
                    }
                    if (line.ToLower().IndexOf("ipv4 ") > -1)
                    {
                        int index = line.IndexOf(":");
                        index += 2;
                        CurIPAddress = line.Substring(index).Split('(')[0];
                        //为了让上面的mac取的是正在用的
                        break;
                    }
                    //不断一个个读取
                    line = reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                Logging.Error("Get Mac Error {0}", e.Message);
            }
            finally
            {
                if (p != null)
                {

                    p.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return MyMacAddress;
        }
        public CGI()
        {
            HostName = Dns.GetHostName();
            this.GetIp();
            this.GetMac();
        }

    }
}
