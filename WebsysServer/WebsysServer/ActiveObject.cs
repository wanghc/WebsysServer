using System;
using System.Collections.Generic;
using System.Reflection;
using WebsysServer.tool;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace WebsysServer
{
    [DataContract]
    internal class ActiveObject
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "desc")]
        public string Desc { get; set; }

        [DataMember(Name = "mode")]
        public int Mode { get; set; } = 0;

        /**dll文件所在目录,支持HTTP路径*/
        [DataMember(Name = "dllPath")]
        public string DllPath { get; set; } = "";
        
        /**程序dll名称*/
        [DataMember(Name = "ass")]
        public string Ass { get; set; }
        
        /**类名*/
        [DataMember(Name = "cls")]
        public string Cls { get; set; }
        
        /**方法名*/
        [DataMember(Name = "mths")]
        public List<string> Mths { get; set; }

        [DataMember(Name = "subPath")]
        public string SubPath { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; } = "";

        public string Mth { get; set; }

        public object[] Param { get; set; }

        public Dictionary<string, string> Props { get; set; }
        [DataMember(Name = "clientIPExp")]
        public string ClientIPExp { get; set; } = "";
        public string focusWindowName { get; set; } = "";
        
        public string focusClassName { get; set; } = "";

        public string focusLazyTime { get; set; } = "";

        public string Url { get; set; } ="";
        /// <summary>
        /// 表示当前是要打开某个exe   .    Qise.cmd("qise.exe")  可以管理好版本
        /// 此时不去反射出对象，直接运行exe
        /// </summary>
        public string CmdRun { get; set; } = "";

        public string GetPValue(string FieldName, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object o = Ts.GetProperty(FieldName).GetValue(obj, null);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value)) return null;
                return Value;
            }
            catch
            {
                return null;
            }
        }
        public bool SetPValue(string FieldName, string Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName).PropertyType);
                Ts.GetProperty(FieldName).SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetFValue(string FieldName, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                FieldInfo mi = Ts.GetField(FieldName); //(obj, null);
                object o = mi.GetValue(obj);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value)) return null;
                return Value;
            }
            catch
            {
                return null;
            }
        }
        public bool SetFValue(string FieldName, string Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetField(FieldName).FieldType);
                Ts.GetField(FieldName).SetValue(obj, v);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetPValue(string FieldName, string Value)
        {
            try
            {
                Type Ts = _obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName).PropertyType);
                Ts.GetProperty(FieldName).SetValue(_obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetFValue(string FieldName, string Value)
        {
            try
            {
                Type Ts = _obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetField(FieldName).FieldType);
                Ts.GetField(FieldName).SetValue(_obj, v);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private object _obj;
        private Type _type;
        /*
         * @version    dll的版本
         * @httpDllPath    dll路径。可以是网络路径http://127.0.0.1/dthealth/web/addins/ass/myselfdir-in.lodoop.dll, 也可以是空
         * @assName        myselfdir-in.lodop.dll
         * **/
        public string CreateXObject() {
            try
            {
                bool downloadSucc = false;
                // 把服务器中DLL文件路径中DLL下到   /plugin/服务器Dll路径（不包含dll名）/版本/.dll
                // DllPath=http://127.0.0.1/dthealth/web/addins/plugin/DHCOPPrint/DHCOPPrint.dll,DhtmlEd.msi
                Logging.Debug("开始CreateXObject");
                Logging.Debug(DllPath);
                if (!DllPath.Contains("/plugin/"))
                {
                    return "-4^下载目录必须在plugin下";
                }
                string[] DllPathAllFiles = DllPath.Split(',');
                if (DllPathAllFiles.Length == 0) return "-5^中间件报错.路径没有指定入口文件";
                string firstDllPathFiles = DllPathAllFiles[0];
                /*string[] DllPathOtherFiles = null;
                if (DllPath.Contains(",")) {
                    //DllPathOtherFiles = DllPathAllFiles;
                    //DllPathOtherFiles = DllPath.Substring(DllPath.IndexOf(",")+1).Split(',');
                }*/
                // 远程当前dll所在目录
                string RemoteDllPath = firstDllPathFiles.Substring(0, firstDllPathFiles.LastIndexOf("/"));
                string RemoteDllFileAllName = firstDllPathFiles.Substring(firstDllPathFiles.LastIndexOf("/"));  // DHCOPPrint.dll
                string RemoteDllSubPath = RemoteDllPath.Substring(RemoteDllPath.IndexOf("plugin"));             // /DHCOPPrint
                string RemotePluginPath = RemoteDllPath.Substring(0, RemoteDllPath.IndexOf("plugin")) + "plugin";  //http://127.../plugin

                string LocalDllPath = CGI.Combine(RemoteDllSubPath + "/" + Version + "/");
                string LocalDllStoreFile = LocalDllPath + RemoteDllFileAllName;
                Logging.Debug("isSameVersion " + LocalDllStoreFile);
                Boolean InvokeProcessWebsysScript = false; /*默认使用老方式运行dll, 如果想使用WebsysScript.exe运行dll，配置中加上,WebsysScript.exe*/
                if (DllPath.ToLower().IndexOf("websysscript.exe")>-1) {
                    InvokeProcessWebsysScript = true;
                }
                if (CGI.IsValidIP(this.ClientIPExp))
                {
                    if (HTTPFile.IsSameVersion(LocalDllStoreFile))
                    {
                        downloadSucc = true; //本地已是最新dll
                    }
                    else if ("".Equals(DllPath))
                    {
                        downloadSucc = true; //不要求下载
                    }
                    else
                    {
                        // 自动下载dll,从最后一个文件开始下载，最后下载首文件即入口
                        if (DllPathAllFiles.Length >= 1) //2020-03-05 从 >1 修改成>=1
                        {
                            for (int i = DllPathAllFiles.Length - 1; i >= 0; i--)
                            {
                                string otherFile = DllPathAllFiles[i];
                                Logging.Debug("开始下载：" + otherFile);
                                Boolean dllDownloadSuccess = true;
                                if (i == 0)
                                {
                                    dllDownloadSuccess = HTTPFile.DownloadFile(firstDllPathFiles, LocalDllStoreFile);
                                    string[] firstDllArr = firstDllPathFiles.Split('/');
                                    otherFile = firstDllArr[firstDllArr.Length - 1];
                                }
                                else if (otherFile.Contains("/"))
                                {
                                    dllDownloadSuccess = HTTPFile.DownloadFile(RemotePluginPath + "/" + otherFile, LocalDllPath + otherFile);
                                }
                                else
                                { // 只有文件名
                                    dllDownloadSuccess = HTTPFile.DownloadFile(RemoteDllPath + "/" + otherFile, LocalDllPath + otherFile);
                                }
                                if (dllDownloadSuccess) //HTTPFile.DownloadFile(RemoteDllPath + "/" + otherFile, LocalDllPath + otherFile))
                                {
                                    string regFile = string.Concat(LocalDllPath, otherFile).Replace("/", "\\");
                                    Logging.Debug("安装或注册: " + regFile);
                                    if (otherFile.ToLower().EndsWith(".msi"))
                                    {
                                        ScriptShell.Msiexec(regFile);
                                        //string cmdrtn = ScriptShell.Run("msiexec.exe /i \"" + regFile + "\" /qb");
                                        //NativeMethods.MsiInstallProduct(LocalDllPath + otherFile, "ACTION=INSTALL");
                                        //string startupPath = System.Windows.Forms.Application.StartupPath;
                                        //new MyInstaller().Install(new BackgroundWorker(), startupPath+string.Concat(LocalDllPath, otherFile).Replace("/", "\\"));
                                    }
                                    else if (otherFile.ToLower().EndsWith(".exe"))
                                    {
                                        //string cmdrtn = ScriptShell.Run("msiexec.exe /i \"" + regFile + "\" /qb");
                                        //ScriptShell.Msiexec(regFile);
                                        ScriptShell.Run(regFile, true);
                                        //if (otherFile.Contains(".dll")) new tool.COMTypeLibConverter().Com2Ass(otherFile);
                                    }
                                    else if (otherFile.ToLower().EndsWith(".dll"))
                                    {
                                        //注册相关依赖dll
                                        //ScriptShell.Run("regsvr32.exe /s \"" + regFile + "\"");
                                        ScriptShell.Regsvr32(regFile);
                                    }
                                    else if (otherFile.ToLower().EndsWith(".ocx"))
                                    {
                                        //注册相关依赖ocx
                                        //ScriptShell.Run("regsvr32.exe /s \"" + regFile + "\"");
                                        ScriptShell.Regsvr32(regFile);
                                    }
                                    else if (otherFile.ToLower().EndsWith(".zip"))
                                    {
                                        string unZipMsg = "";
                                        string contentPath = System.AppDomain.CurrentDomain.BaseDirectory; // "D:\\workspace_net\\WebsysServer\\WebsysServer\\bin\\x86\\Debug\\";

                                        Logging.Debug("解压Zip: " + contentPath + regFile + " 到 " + contentPath + LocalDllPath);
                                        new UnZipFile().unZipFile(regFile, LocalDllPath, ref unZipMsg);
                                    }
                                    else if (otherFile.ToLower().EndsWith(".cab"))
                                    {
                                        // pkgmgr /ip /m:D:\trakWebEdit3.CAB
                                        // dism /online /add-package /PackagePath:D:\trakWebEdit3.CAB
                                        //EXPAND -F:*.* E:\dthealth\app\dthis\web\addins\client\BarCode.CAB D:\xml\
                                        //extract /a f:\win98\precopy1.cab shell.dll /l c:\windows\system
                                        //-------------------------------
                                        Logging.Info("开始：EXPAND - F:*.* \"" + regFile + "\" \"" + LocalDllPath + "\"");
                                        //ScriptShell.Run("EXPAND -F:*.* \""+regFile+"\" \""+LocalDllPath+"\"");
                                        ScriptShell.Expand(regFile, LocalDllPath);
                                        string contentPath = System.AppDomain.CurrentDomain.BaseDirectory; //..bin/x86/Debug
                                                                                                           //-------------------------
                                                                                                           //Logging.Info("开始：RegInfDll "+"\"" + contentPath + regFile.Substring(0, regFile.Length - 4) + ".INF \"");
                                                                                                           //ScriptShell.RegInfDll(contentPath + regFile.Substring(0, regFile.Length - 4) + ".INF"); // RegInfDll
                                                                                                           //------------------------------------------------
                                                                                                           //Logging.Info("开始：InfDefaultInstall \"" + contentPath + regFile.Substring(0, regFile.Length - 4) + ".INF \"");
                                                                                                           //Regex.Replace(regFile, ".cab", ".INF", RegexOptions.IgnoreCase) + "\"");
                                                                                                           //ScriptShell.InfDefaultInstall(contentPath + regFile.Substring(0, regFile.Length - 4) + ".INF");
                                                                                                           //ScriptShell.Run("InfDefaultInstall.exe \""+ contentPath + regFile.Substring(0, regFile.Length - 4) + ".INF \"");
                                    }
                                }
                            }
                        }
                        if (HTTPFile.IsSameVersion(LocalDllStoreFile))
                        {
                            downloadSucc = true;
                        }
                        // 再下载运行库
                        // downloadSucc = HTTPFile.DownloadFile(firstDllPathFiles,LocalDllStoreFile); //RemoteDllPath+"/"+RemoteDllFileAllName, LocalDllStoreFile);
                        // 如果有inf文件，按inf内的要求注册dll，一般用于zip包
                        if (downloadSucc) //&& File.Exists(LocalDllPath+"/chrome.inf"))
                        {
                            string curpath = CGI.Combine(LocalDllPath); //Path.Combine(System.Windows.Forms.Application.StartupPath, LocalDllPath);
                            DirectoryInfo dirInfo = new DirectoryInfo(LocalDllPath);
                            FileInfo[] fileInfos = dirInfo.GetFiles();
                            for (int my = 0; my < fileInfos.Length; my++)
                            {
                                FileInfo fileInfo = fileInfos[my];
                                if (".inf".Equals(fileInfo.Extension.ToLower()))
                                {
                                    ScriptShell.RegInfDll(curpath + "\\" + fileInfo.Name); // "\\chrome.inf");
                                }

                            }
                        }
                        if (!(RemoteDllFileAllName.Equals("/" + Ass + ".dll")) && (RemoteDllFileAllName.Contains(".dll")))
                        {
                            // /s
                            // 把com转成asm, 且动态调用asm.dll
                        }
                        if ("".Equals(this.CmdRun) && LocalDllStoreFile.ToLower().EndsWith(".dll")) // 调用dll才 要重启
                        {
                            /// FIX: 实现应该判断当前进程中是否加载过dll，如果加载过才重启应用
                            /// 现在逻辑是下载新的DLL就会重启应用
                            /// 20220602 不再重启，使用websysScript.exe运行dll
                            /// 20220617 非exe运行dll方式下，重启中间件
                            if (!InvokeProcessWebsysScript) //(System.Windows.Forms.MessageBox.Show("要重新启动嘛？", "提示", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                            {
                                Logging.Warn("----RestartByUpgrade---");
                                Logging.Warn("重启路径："+System.Reflection.Assembly.GetExecutingAssembly().Location);
                                ScriptShell.Run(System.Reflection.Assembly.GetExecutingAssembly().Location + " RestartByUpgrade"+" "+RemoteDllFileAllName.Replace("/","") + " "+Version);
                                return "-6^升级模块插件并重启医为客户端管理应用";
                            }
                        }
                    }
                }
                else {
                    downloadSucc = true;
                }
                if (downloadSucc)
                {
                    if (!("".Equals(this.CmdRun)))
                    {
                        /// 2020-07-10 此时不dll反射运行,调用exe
                        /// 能进入这里一定是单独对象调用的----
                        /// CmdRun = qise.exe  ===>转成客户端路径==》
                        Logging.Debug("CmdRun 开始 {0}", this.CmdRun);
                        Logging.Debug("CmdRun 客户端 {0}开始", LocalDllPath+this.CmdRun);
                        /*
                         * this.CmdRun的值可能是：
                         * eg: WebsysScript.exe 
                         * eg: /BP/3.4.3.0/DHCClinic.BP.Main.exe 18912,170,197,cn_iptcp:114.242.246.235[51773]:DHC-APP 需求号 ： 3129682 
                         * eg: /APB/my file/WebsysScript.exe
                         * */
                        // ScriptShell.Run(LocalDllPath + this.CmdRun, true);
                        string pathParam = LocalDllPath + this.CmdRun;
                        if (pathParam.Contains(" ")) {
                            int spaceIndex = pathParam.IndexOf(" ");
                            int nameIndex = pathParam.IndexOf(".exe");
                            if (pathParam.Contains(".dll")) nameIndex = pathParam.IndexOf(".dll");
                            if (pathParam.Contains(".msi")) nameIndex = pathParam.IndexOf(".msi");
                            if (pathParam.Contains(".jar")) nameIndex = pathParam.IndexOf(".jar");
                            if (nameIndex>spaceIndex) { // 说明路径中包含空格    
                                string exePath = pathParam.Substring(0, nameIndex + 4);
                                if (File.Exists(exePath)){
                                    string paramStr = pathParam.Substring(nameIndex + 4);
                                    // 路径二边补双引号                       // 需求号 ： 3129682 ,基础平台-插件更新升级后，插件调用的产品，重症血透界面无法打开
                                    Logging.Debug("为客户端程序增加双引号得到：{0}，且运行", "\"" + exePath + "\"" + paramStr);
                                    ScriptShell.Run("\"" + exePath + "\"" + paramStr, true);
                                    _obj = null;
                                    return "100^成功运行命令";
                                }
                                // ScriptShell.Run("\"" + LocalDllPath + this.CmdRun + "\"", true);  // 需求号3078733 ；20221115 如果路径包含空格,则会运行失败 
                            }
                        }
                        // 2024-03-22 增加jar调用处理
                        if (pathParam.IndexOf(".jar") > 0)
                        {
                            string path = CGI.Combine("temp/MyCode" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt");
                            Logging.Debug("生成", path);
                            using (StreamWriter sw = File.CreateText(path))
                            {
                                sw.WriteLine("/*" + pathParam + "*/");
                                sw.Write("java -jar "+this.CmdRun);
                                sw.Close();
                            }
                            Logging.Debug("生成", path + "完成");
                            return "101^" + path + "^" + LocalDllStoreFile;
                            //return ScriptShell.RunWorkingDirectory(LocalDllPath,"java -jar "+this.CmdRun,false);
                        }
                        else
                        {
                            ScriptShell.Run(LocalDllPath + this.CmdRun, true);
                        }
                        //new ScriptShell().CmdRun(this.CmdRun);
                        _obj = null;
                        return "100^成功运行命令";
                    }
                    else
                    {
                        if (LocalDllStoreFile.ToLower().EndsWith(".dll"))
                        {
                            // 20220617 配置了才使用exe来运行dll   
                            if (InvokeProcessWebsysScript) { // 修改成写文本 ， 使用WebsysScript来运行文本

                                string path = CGI.Combine("temp/MyCode" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt");
                                Logging.Debug("生成", path);
                                using (StreamWriter sw = File.CreateText(path)) {
                                    sw.WriteLine("/*"+ LocalDllStoreFile + "*/");
                                    // http://localhost:11996/websys/Interop.PrjSetTime/PrjSetTime.CLSSETTIMEClass?_dllDir=http%3A%2F%2F127.0.0.1%2Fdthealth%2Fweb%2Faddins%2Fplugin%2FPrjSetTime%2FInterop.PrjSetTime.dll%2CSetTime.dll  &_version = 1.0.0.0 & _clientIPExp = 127.0.0.1 & VYear = 2019 & VMonth = 9 & VDay = 17 & VHour = 9 & VMinute = 55 & M_SetTime =
                                    sw.Write(Url);
                                    sw.Close();
                                }
                                Logging.Debug("生成", path+"完成");
                                return "101^" + path+"^"+ LocalDllStoreFile;
                            }
                            // 测试发现 在其它目录注册trakWebEdit3.dll后，只要interop.trakWebEdit3.dll在就可以反射
                            //会占用文件
                            Logging.Debug("LoadFrom 开始 {0}", LocalDllStoreFile);
                            Assembly ass = Assembly.LoadFrom(LocalDllStoreFile);
                            //Assembly ass = Assembly.Load(File.ReadAllBytes(LocalDllStoreFile));  // 护理医嘱单DoctorSheet使用Load方法调用不能加载依赖项；组件layout可以
                            Logging.Debug("LoadFrom 结束 {0}", LocalDllStoreFile);
                            Logging.Debug("ass.GetType 开始 {0}", LocalDllStoreFile);
                            _type = ass.GetType(Cls);
                            Logging.Debug("ass.GetType 结束 {0}", LocalDllStoreFile);
                            if (_type == null) return "-2^获得实类失败" + Cls;
                            _obj = Activator.CreateInstance(_type);
                            return "0";
                        }
                        else
                        {

                            return "-3^注册成功。入口不是DLL无需运行";
                        }
                    }
                }
                return "-3^未加载到要求版本的["+Ass+"]控件";
            }catch (Exception e)
            {
                return "-1^" + e.Message;

            }
        }
        public object Invoke(string m, params object[] Arg) 
        {
            object mthRtnValue = null;
            try { 
                MethodInfo method = _type.GetMethod(m);
                ParameterInfo[] param = method.GetParameters();
                object[] newArg = new object[param.Length ];
                for (var i=0;i<param.Length  ; i++)
                {
                    newArg[i] = param[i].DefaultValue;
                    if (Arg.Length>i)
                    {
                        newArg[i] = Arg[i];
                    }
                }
                mthRtnValue = method.Invoke(_obj,newArg);
                if (mthRtnValue != null) Logging.Debug(mthRtnValue.ToString());
            }
            catch (Exception e)
            {
                Logging.Error(e);
                throw;                
            }
            return mthRtnValue;
        }
        /**
         * DllPath到存储dll的目录
         * DllPath默认为空,表示当前目录下查找AssType.dll
         * DllPath=http://127.0.0.1/dthealth/web/addins/ass/时表示到这个远程目录下找AssType.dll
         * http://localhost:888/1/Interop.PrjSetTime/PrjSetTime.CLSSETTIMEClass/SetTime
         * */
        public string myTmpInvoke()
        {
            object mthRtnValue = "";
            string RtnStr = "";
            Type type;
            try
            {
                
                //new LISScreenCapture.FrmCaptureWait().ShowDialog();
                //return "ff";
                //type = Type.GetTypeFromCLSID(new Guid("6F4558E4-72DA-4CCD-963C-9EED2ECD14A9"));
                //type = Assembly.Load("DHCINSUBLL").GetType("DHCINSUBLL." + BllType);
                //Assembly ass = Assembly.LoadFrom(@""+AssType+".dll"); //@"E:\DtHealth\app\dthis\web\addins\client\trakWebEdit3\trakWebEdit3.dll");
                // LoadFrom支持从一个URL加载程序集(如http://www.abc.com/test.dll)
                if (this.Mode>0)
                {
                    //this.DllPath = CfgJson.cfgJsonDto.AssDir[this.Mode-1];
                }
                Assembly ass = Assembly.LoadFrom(DllPath + Ass.Replace("-","/") + ".dll");
                Logging.Debug("LoadFrom {0}{1}.dll",DllPath,Ass.Replace("-", "/"));
                //Assembly ass = Assembly.Load(AssType);
                type = ass.GetType(Cls);
                if (type == null) return "-2^获得实类失败。" + Cls;
                //Type mthRetType = method.ReturnType;
                ////调用的实例化方法（非静态方法）需要创建类型的一个实例
                object obj = Activator.CreateInstance(type);
                foreach (System.Reflection.PropertyInfo p in obj.GetType().GetProperties())
                {
                    if (Props.ContainsKey(p.Name))
                    {
                        String pval = Props[p.Name];
                        Logging.Debug("StartIOC {0}:{1}" , p.Name, pval);
                        if (false == SetPValue(p.Name, pval, obj))
                        {
                            Logging.Error( "Property_IOC_Error: {0}:{1}" , p.Name , pval);
                            return "-2^注入属性值出错: " + p.Name + ":" + pval;
                        }
                    }
                    else
                    {
                        Logging.Debug("DefaultValue {0}:{1}" ,p.Name , GetPValue(p.Name, obj));
                    }
                    //Console.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(userInfo));
                }
                foreach (System.Reflection.FieldInfo f in obj.GetType().GetFields())
                {
                    if (Props.ContainsKey(f.Name))
                    {
                        String pval = Props[f.Name];
                        Logging.Debug("StartIOC {0}:{1}",f.Name ,pval);
                        if (false == SetFValue(f.Name, pval, obj))
                        {
                            Logging.Error(" Field_IOC_Error :{0}:{1} ",f.Name,pval);
                            return "-2^注入属性值出错: " + f.Name + " : " + pval;
                        }
                    }
                    else
                    {
                        Logging.Debug( "DefaultValue {0}:{1} ", f.Name, GetFValue(f.Name, obj));
                    }
                }
                //Logging.Debug("Invoke {0}(\"{1}\") ",Mth, Param!=null?string.Join("\",\"", Param):"");
                
                MethodInfo method = null;
                if (Mth.Contains("."))
                {
                    /*int lastInd = Mth.LastIndexOf('.') ;
                    Type superType = Type.GetType(Mth.Substring(0,lastInd));
                    if (superType == null) {
                        return "-2^获得实类失败。" + Mth.Substring(0, lastInd);
                    }
                    superType.GetMethod(Mth.Substring(lastInd + 1)); //, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    */
                    //if (Mth.Equals("System.Windows.Forms.Form.ShowDialog")) new LISScreenCapture.FrmCaptureWait().ShowDialog();
                }
                else
                {
                    type.GetMethod(this.Mth); //, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    mthRtnValue = method.Invoke(obj, this.Param);
                }

                /*ParameterInfo[] parr = method.GetParameters();
                for (int i=0; i<parr.Length;i++)
                {
                    if (parr[i].ToString().Contains("MSXML2.IXMLDOMDocument2"))
                    {
                        MSXML2.IXMLDOMDocument2 xmlObj4 = new MSXML2.DOMDocument40Class();  //new MSXML2.DOMDocument();
                        xmlObj4.loadXML((string)Param[i]);
                        Logging.Debug("xml"+xmlObj4.text);
                        Param[i] = xmlObj4;
                    }
                }*/
                
                if (mthRtnValue != null)
                {
                    RtnStr = mthRtnValue.ToString();
                }
                Logging.Debug("调用成功。返回值："+RtnStr);
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Contains("GetMethodImpl"))
                {
                    RtnStr = "-1^获得"+Mth+"方法失败。" + ex.Message + ",DllType:" + this.Cls;
                }
                else
                {
                    RtnStr = "-1^调用发生异常。" + ex.Message + ",DllType:" + this.Cls;
                }
                
                Logging.LogUsefulException(ex);

            }
            finally
            {

            }
            return RtnStr;
        }
        public object GetActiveXObject(Guid clsid)
        {
            Type t = Type.GetTypeFromCLSID(clsid);
            if (t == null)
                return null;
            return Activator.CreateInstance(t);
        }
    }
}