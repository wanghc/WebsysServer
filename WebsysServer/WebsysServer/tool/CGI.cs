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
            /// 中间件返回：“不知道这样的主机”
            /// 网络DNS问题，DNS服务器没有打开反向解析，而GetHostEntry是通DNS反向解析得到主机名的。我遇到了这个问题，重新配置了局域网内的DNS服务器，解决了从IP至主机名的转换问题。
            /// 另外，配置好DNS反向解析后，要重新注册主机ipconfig / registerdns，并清空dns缓存ipconfig / flushdns。
            /// Dns.GetHostEntry(hn) 修改成 Dns.GetHostAddresses，不再通过DNS解析得到IP。
            /// 解决某些云桌面电脑不能拿到IP与mac登录界面不能登录

            System.Net.IPAddress[] addressList = Dns.GetHostAddresses(hn); //Dns.GetHostEntry(hn).AddressList;//IP获取一个LIST里面有一个是IP
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
        public static Boolean IsValidIP(string ClientIPExp="")
        {
            if (ClientIPExp.Equals("")) return true;
            try
            {
                CGI cgi = new CGI();
                for (var k = 0; k < cgi.IPList.Count; k++)
                {
                    String inIP = cgi.IPList[k]; //"127.1.1.1";
                    String[] inIPList = inIP.Split('.');
                    String[] IPExpStrList = ClientIPExp.Split(',');
                    for (var i = 0; i < IPExpStrList.Length; i++)
                    {
                        // 每一个数字满足与否
                        Boolean[] Rtn = { false, false, false, false };
                        var IPItmList = IPExpStrList[i].Split('.');
                        for (var j = 0; j < 4; j++)
                        {
                            if (IPItmList[j].Equals(inIPList[j]) || IPItmList[j].Equals("*"))
                            {
                                Rtn[j] = true;
                            }
                            else
                            {
                                if (IPItmList[j].IndexOf("-") > -1)
                                {
                                    var myArr = IPItmList[j].Replace("[", "").Replace("]", "").Split('-');
                                    var myArr1 = Int16.Parse(myArr[0]);
                                    var myArr2 = Int16.Parse(myArr[1]);
                                    if (myArr1 <= Int16.Parse(inIPList[j]) && myArr2 >= Int16.Parse(inIPList[j]))
                                    {
                                        Rtn[j] = true;
                                    }
                                }
                            }
                        }
                        if (Rtn[0] && Rtn[1] && Rtn[2] && Rtn[3]) return true;
                    }
                }
            }catch (Exception ex) {}
            return false;
        }
    }
}
