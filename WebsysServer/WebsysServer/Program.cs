using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using WebsysServer.tool;
namespace WebsysServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logging.CurLogLevel = Properties.Settings.Default.LogLevel;
            Logging.OpenLogFile();
            if (IsRunning())
            {
                //MessageBox.Show("监听程序已经在运行!","医为客户端管理",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Logging.Error("监听程序已经在运行!");
                Application.Exit();
                return;
            }
            
            Logging.Info("开始检查证书---");
            //b1eb8df9b91cf3080fb30f41e959def25952376a
            string privatecerthash = "b1eb8df9b91cf3080fb30f41e959def25952376a";
            if (!CheckCertByHash(privatecerthash,true,"root")) //if (!CheckCert("CN=MediWayCA",true,"root")) 
            {   // 安装受信任的颁发机构证书
                InstallMedWayCACert();
                Logging.Info("安装受信任的颁发机构证书-MediWayCA");
            }
            if (!CheckCertByHash(privatecerthash,true,"my")) //if (!CheckCert("E=wanghuicai@mediway.cn, CN=127.0.0.1, OU=MediWay", true,"my"))
            {
                string certhash = InstallServerCertToMY(); //"dd8652db5c07076d154827273642604ca8405332";
                Logging.Info("安装个人证书"+certhash);
                string ipport = "0.0.0.0:" + Properties.Settings.Default.HttpsServerPort;
                OperatingSystem os = Environment.OSVersion as OperatingSystem;
                int gosv = os.Version.Major;
                if (gosv>5)   // windows vista=6
                {
                    // netsh http add sslcert ipport=21996 certhash=dd8652db5c07076d154827273642604ca8405332 appid={{9e977cef-28ef-4d4f-968a-bff2514384c4}}
                    string d = string.Format("netsh http delete sslcert ipport={0}",ipport);
                    tool.ScriptShell.Run(d, false);
                    string c = string.Format("netsh http add sslcert ipport={0} certhash={1} appid={{9e977cef-28ef-4d4f-968a-bff2514384c4}}", ipport, certhash);
                    Logging.Info("绑定服务器证书" + c);
                    tool.ScriptShell.Run(c,false);
                }
                else
                {
                    string xpStr = string.Format("httpcfg set ssl -i {0} -h {1}", ipport, certhash);
                    Logging.Info("绑定服务器证书" + xpStr);
                    tool.ScriptShell.Run(xpStr);
                }
            }
            //2020-03-15 start
            /*System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            Application.EnableVisualStyles();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            { // 如果当前是管理员，则直接运行
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            { //创建启动对象
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                //startInfo.Arguments = String.Join(" ", Args);
                startInfo.Verb = "runas";
                System.Diagnostics.Process.Start(startInfo);
                System.Windows.Forms.Application.Exit();
            }*/
            //---end 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static bool IsRunning()
        {
            Process current = default(Process);
            current = System.Diagnostics.Process.GetCurrentProcess();
            Process[] processes = null;
            processes = System.Diagnostics.Process.GetProcessesByName(current.ProcessName);
            Process process = default(Process);
            foreach (Process tempLoopVar_process in processes)
            {
                process = tempLoopVar_process;
                
                //if (process.StartInfo.Verb.Equals("runas"))   //只有是管理员运行才算
                //{
                    if (process.Id != current.Id)
                    {
                        if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                        {
                            return true;
                        }
                    }
                //}
            }
            return false;
        }
        /// <summary>
        ///  CheckCert("CN=MediWayCA",true)
        ///  包含查询 
        /// </summary>
        /// <param name="CN"></param>
        /// <returns></returns>
        static bool CheckCert(String CN,Boolean IsContent,string myOrRoot)
        {
            Logging.Info("在"+myOrRoot+"中，检查"+CN+"证书，是否包含查询"+IsContent);
            bool result = false;
            X509Store store = null;
            if (myOrRoot.Equals("my"))
            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            }
            else
            {
                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            }
            try
            {
                store.Open(OpenFlags.MaxAllowed);
                foreach (var item in store.Certificates)
                {
                    if (IsContent)
                    {
                        if (item.SubjectName.Name.IndexOf(CN)>-1)
                        {
                            result = true;
                            break;
                        }
                    }else if (item.SubjectName.Name == CN)
                    {
                        result = true;
                        break;
                    }
                }
            }
            finally
            {
                store.Close();
            }
            Logging.Info("检查" + CN + "证书结果:" + result);
            return result;
        }
        static bool CheckCertByHash(String Hash, Boolean IsContent, string myOrRoot)
        {
            Logging.Info("在" + myOrRoot + "中，检查" + Hash + "证书，是否包含查询" + IsContent);
            bool result = false;
            X509Store store = null;
            if (myOrRoot.Equals("my"))
            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            }
            else
            {
                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            }
            try
            {
                store.Open(OpenFlags.MaxAllowed);
                foreach (var item in store.Certificates)
                {
                    if (IsContent)
                    {
                        if (item.Thumbprint.IndexOf(Hash) > -1)
                        {
                            result = true;
                            break;
                        }
                    }
                    else if (item.Thumbprint==Hash)
                    {
                        result = true;
                        break;
                    }
                }
            }
            finally
            {
                store.Close();
            }
            Logging.Info("检查" + Hash + "证书结果:" + result);
            return result;
        }
        static bool InstallMedWayCACert()
        {
            // 安装到 本地计算机-受信任的根证书颁发机构
            string contentPath = System.AppDomain.CurrentDomain.BaseDirectory;   //..bin/x86/Debug    //Application.StartupPath
            string certPath = System.IO.Path.Combine(contentPath, "private.crt"); // "MediWayCA.crt");
            Logging.Info(certPath);
            X509Certificate2 certificate1 = new X509Certificate2(certPath);
            X509Store store1 = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store1.Open(OpenFlags.MaxAllowed);
            store1.Add(certificate1);
            store1.Close();
            return true;
        }
       
        static String InstallServerCertToMY()
        {
            string contentPath = System.AppDomain.CurrentDomain.BaseDirectory;              //..bin/x86/Debug    //Application.StartupPath          
            string pfxPath = System.IO.Path.Combine(contentPath, "private.pfx"); // "server127.pfx");
            Logging.Info(pfxPath);
            X509KeyStorageFlags x509KeyStorageFlags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet; //X509KeyStorageFlags.MachineKeySet;
            X509Certificate2 certificate = new X509Certificate2(pfxPath, "12345678",x509KeyStorageFlags);
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.MaxAllowed);
            store.Add(certificate);
            store.Close();
            string hashStr = certificate.Thumbprint;
            return hashStr;
        }
    }
}