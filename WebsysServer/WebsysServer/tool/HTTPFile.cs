using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        ///<summary>
        /// 下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        /// <param name="Filename">下载后另存为（全路径）</param>
        public static bool DownloadFile(string URL, string filename)
        {
            try
            {
                HttpWebRequest Myrq = WebRequest.Create(URL) as HttpWebRequest;
                if (URL.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                { 
                    //增加https支持 2020-03-06 
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    Myrq.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    Myrq.KeepAlive = false;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Directory.CreateDirectory(filename.Substring(0,filename.LastIndexOf("/")));
                Stream so = new FileStream(filename, System.IO.FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
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
