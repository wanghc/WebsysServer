using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CurlSharpZF;
namespace WebsysServer.tool
{
    class HTTPFile
    {
        public static bool IsSameVersion(string filePath)
        {
            return File.Exists(filePath);
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //总是接受  
            return true;
        }
        public static int OnWriteData(byte[] buf, int size, int nmemb, object extraData) {
            var nBytes = size * nmemb;
            Writers[(string)extraData].Write(buf);
            return nBytes;
        }
        private static readonly Dictionary<string, BinaryWriter> Writers = new Dictionary<string, BinaryWriter>();

        public static bool DownloadFileByCurl(string URL, string filename) {
            
            try {
                Logging.Info("windows7 download file by curl {0}", URL);
                Curl.GlobalInit(CurlInitFlag.All);
                string filedir = filename.Substring(0, filename.LastIndexOf("/"));
                if (!Directory.Exists(filedir)) {
                    Directory.CreateDirectory(filedir);
                }
                if (!File.Exists(filename)) {
                    Writers.Add(URL, new BinaryWriter(new FileStream(filename, FileMode.Create)));
                } else {
                    return true;
                }
                CurlEasy easy = new CurlEasy();
                easy.AutoReferer = true; //是否自动重定向
                easy.FollowLocation = true;//是否本地
                easy.Url = URL;
                easy.WriteFunction = OnWriteData;
                easy.WriteData = URL;
                if (URL.StartsWith("https", StringComparison.OrdinalIgnoreCase)) {
                    easy.HttpVersion = CurlHttpVersion.Http2_0;
                    // easy.CaInfo = "curl-ca-bundle.crt";
                    easy.SslVerifyPeer = false;
                    easy.SslVerifyhost = false;

                }
                //https end判断结束
                easy.Perform();
                easy.Dispose();
                if (Writers.Count > 0) {
                    foreach (var w in Writers.Values) {
                        w.Dispose();
                    }
                    Writers.Remove(URL);
                }
                Curl.GlobalCleanup();
                return true;
            } catch (Exception ex) {
                Logging.Error("{0}下载失败{1}", URL, "行号：" + ex.StackTrace.ToString() + "错误消息：" + ex.Message.ToString());
                Curl.GlobalCleanup();
                return false;
            }
        }
        private const string windows2000 = "5.0";
        private const string windowsxp = "5.1";
        private const string windows2003 = "5.2";
        private const string windows2008 = "6.0";
        private const string windows7 = "6.1";
        private const string windows8orwindows81 = "6.2";
        private const string windows10 = "10.0";
        ///<summary>
        /// 下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        /// <param name="Filename">下载后另存为（全路径）</param>
        public static bool DownloadFile(string URL, string filename)
        {
            Logging.Info("System.Environment.OSVersion.Version : ", System.Environment.OSVersion.Version.ToString());
            if (System.Environment.OSVersion.Version.ToString().StartsWith(windows7)) { // 6.1.7601.65536
                return DownloadFileByCurl(URL,filename);
            }
            try
            {
                
                if (URL.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    //增加https支持 2020-03-06 
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    // 这里设置了协议类型。
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    //ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                    try {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                    } catch (Exception e) {
                        // 部分电脑的错误
                        //System.NotSupportedException: The requested security protocol is not supported.
                        //
                        //LogInfo.Error("https协议错误：hisURI" + HisURI, e);
                        Logging.Error("https协议错误：{0}", URL);
                        Logging.LogUsefulException(e);
                    }
                    //ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                    
                }
				HttpWebRequest Myrq = WebRequest.Create(URL) as HttpWebRequest;
				Myrq.ProtocolVersion = HttpVersion.Version11;
                Myrq.KeepAlive = false;
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Directory.CreateDirectory(filename.Substring(0, filename.LastIndexOf("/")));
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                if (osize==0) //linux服务器上没有的文件，不一定会报错404，大小为0KB。 window-IIS服务器时会进入catch
                {
                    Logging.Error("{0}大小为0，跳过", URL);
                    st.Close();
                    myrp.Close();
                    Myrq.Abort();
                    return false;
                }
                Stream so = new FileStream(filename, System.IO.FileMode.Create);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                myrp.Close();
                Myrq.Abort();
                return true;
            }catch (System.Exception e)
            {
                Logging.Error("{0}下载失败",URL);
                Logging.LogUsefulException(e);
                return false;
            }
        }
    }
}
