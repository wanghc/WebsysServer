using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WebsysScript.tool;
using System.Web;
namespace WebsysScript {
    class EvalDll {
        private object _obj;
        private Type _type;
        private Boolean success;
        private string errorMsg;
        static string PCOUNT = "P_COUNT";
        static string P = "P_";
        /// <summary>
        /// 加载某dll，并创建Cls实例
        /// </summary>
        /// <param name="LocalDllStoreFile">动态库路径</param>
        /// <param name="Cls">需实例化的类名</param>
        /// <returns>0表示 成功,其它为失败</returns>
        public void InitObject(string LocalDllStoreFile, string Cls) {
            Logging.Debug("LoadFrom 开始 {0}", LocalDllStoreFile);
            Assembly ass = null;
            success = false;
            try {
                try {
                    ass = Assembly.LoadFrom(LocalDllStoreFile);
                } catch (Exception ex1) {
                    errorMsg = "-1^加载" + LocalDllStoreFile + "失败," + ex1.Message;
                    if (null != ex1.InnerException) errorMsg += ex1.InnerException.Message + ":" + ex1.InnerException.Source;
                    throw new Exception(errorMsg);  //抛出异常才能保证txt文件不会删除
                }
                //Assembly ass = Assembly.Load(File.ReadAllBytes(LocalDllStoreFile));  // 护理医嘱单DoctorSheet使用Load方法调用不能加载依赖项；组件layout可以
                Logging.Debug("LoadFrom 结束 {0}", LocalDllStoreFile);
                Logging.Debug("ass.GetType 开始 {0}", Cls);
                _type = ass.GetType(Cls);
                Logging.Debug("ass.GetType 结束 {0}", Cls);
                if (_type == null) {
                    errorMsg = "-2^获得类定义失败," + Cls;
                    throw new Exception(errorMsg);
                };
                try {
                    _obj = Activator.CreateInstance(_type);
                }catch(Exception ex2) {
                    errorMsg = "-3^创建类（"+ _type + "）对象失败," + ex2.Message;
                    if (null != ex2.InnerException) errorMsg += ex2.InnerException.Message + ":" + ex2.InnerException.Source;
                    throw new Exception(errorMsg);  //抛出异常才能保证txt文件不会删除
                }
                Logging.Debug("CreateInstance 成功", LocalDllStoreFile);
                success = true;
            } catch (Exception ex) {
                success = false;
                throw ex;  //抛出异常才能保证txt文件不会删除
            }
        }
        /// <summary>
        /// 调用当前对象的方法
        /// </summary>
        /// <param name="m"></param>
        /// <param name="Arg"></param>
        /// <returns></returns>
        public object Invoke(string m, params object[] Arg) {
            object mthRtnValue = null;
            success = false;
            try {
                MethodInfo method = _type.GetMethod(m);
                if (method == null) {
                    errorMsg = "-4^获得方法（"+m+"）定义失败。" ;
                    throw new Exception(errorMsg);  //抛出异常才能保证txt文件不会删除;
                }
                object[] newArg = null;
                try {
                    ParameterInfo[] param = method.GetParameters();
                    newArg = new object[param.Length];
                    for (var i = 0; i < param.Length; i++) {
                        newArg[i] = param[i].DefaultValue;
                        if (Arg.Length > i) {
                            newArg[i] = Arg[i];
                        }
                    }
                } catch (Exception e1){
                    errorMsg = "-5^获得方法的默认参数失败,"+e1.Message;
                    if (null != e1.InnerException) errorMsg += e1.InnerException.Message + ":" + e1.InnerException.Source;
                    throw new Exception(errorMsg);  //抛出异常才能保证txt文件不会删除;
                }
                try {
                    mthRtnValue = method.Invoke(_obj, newArg);
                } catch (Exception e2) {
                    errorMsg = "-6^调用"+m+"方法失败。" + e2.Message;
                    if (null != e2.InnerException) errorMsg += e2.InnerException.Message + ":" + e2.InnerException.Source;
                    throw new Exception(errorMsg);  //抛出异常才能保证txt文件不会删除;
                }
                if (mthRtnValue != null) Logging.Debug(mthRtnValue.ToString());
                success = true;
            } catch (Exception e) {
                success = false;
                Logging.Error(e);
                throw e;  //抛出异常才能保证txt文件不会删除;
            }
            return mthRtnValue;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool SetPValue(string FieldName, string Value) {
            try {
                Type Ts = _obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName).PropertyType);
                Ts.GetProperty(FieldName).SetValue(_obj, v, null);
                return true;
            } catch {
                return false;
            }
        }
        /// <summary>
        /// 设置字段值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool SetFValue(string FieldName, string Value) {
            try {
                Type Ts = _obj.GetType();
                object v = Convert.ChangeType(Value, Ts.GetField(FieldName).FieldType);
                Ts.GetField(FieldName).SetValue(_obj, v);
                return true;
            } catch {
                return false;
            }
        }
        /// <summary>
        ///  从query串中获得focus信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string[] GetFocusInfo(string query) {
            string[] strArr = new string[] { "", "", "" };
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++) {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm)) {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue = value;
                    if (key.Equals("_focusClassName")) strArr[0] = deValue;
                    if (key.Equals("_focusWindowName")) strArr[1] = deValue;
                    if (key.Equals("_focusLazyTime")) strArr[2] = deValue;
                }
            }
            return strArr;
        }
        public static string decodeURIComponent(string str, Encoding encoding) {
            return System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.GetEncoding("utf-8"));
        }
        public static string encodeURIComponent(string str, Encoding encoding) {
            return System.Web.HttpUtility.UrlEncode(str, System.Text.Encoding.GetEncoding("utf-8"));
        }
        /// <summary>
        /// 将获取的formData存入字典数组
        /// </summary>
        public static Dictionary<String, String> DataToDict(string formData) {
            try {
                String[] dataArry = formData.Split('&');
                Dictionary<String, String> dataDic = new Dictionary<string, string>();
                for (int i = 0; i <= dataArry.Length - 1; i++) {
                    String dataParm = dataArry[i];
                    if (dataParm != "") {
                        int dIndex = dataParm.IndexOf("=");
                        String key = dataParm.Substring(0, dIndex);
                        String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                        String deValue = decodeURIComponent(value,Encoding.UTF8);
                        if (key != "__VIEWSTATE") {
                            dataDic.Add(key, deValue);
                        }
                    }
                }
                return dataDic;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        public static string[] ParseMthArg(String mthParamStr) {
            Dictionary<string, string> data = DataToDict(mthParamStr);
            string[] Args = null;
            if (data.ContainsKey(PCOUNT)) Args = new string[Int16.Parse(data[PCOUNT])];
            if (Args == null) return Args;
            for (var i = 0; i < Args.Length; i++) Args[i] = data.ContainsKey(P + i) ? data[P + i] : "";
            return Args;
        }
        /**
         * _dllDir=http%3A%2F%2F127.0.0.1%2Fdthealth%2Fweb%2Faddins%2Fplugin%2FPrjSetTime%2FInterop.PrjSetTime.dll%2CSetTime.dll
            &_version=1.0.0.0 &_clientIPExp=127.0.0.1  &VYear=2019 &VMonth=9 &VDay=17 &VHour=9 &VMinute=55 &M_SetTime=

         * url=/websys/1/Interop.Lodop/Lodop.LodopXClass
         * query=M_FORMAT=p_count=1&p_1=222&p_333   &    M_PRINT=  &  M_ADD_PRINT_TEXT=
         * 
         */
        public static string RunMths(string localDllFileName,string url, string query) {
            string[] arr = url.Split(new char[] { '/' });
            string dllpath = url;
            if (arr.Length < 4) return "-101^请求路径错误";
            EvalDll evalDll = new EvalDll();
            evalDll.InitObject(localDllFileName, arr[3]);
            if (!evalDll.success) return evalDll.errorMsg;
            string[] focusArr = GetFocusInfo(query);
            object lastMthRtn = "";
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++) {
                string dataParm = dataArry[i];
                if (String.IsNullOrEmpty(dataParm)) continue;
                int dIndex = dataParm.IndexOf("=");
                String key = dataParm.Substring(0, dIndex);
                String deValue = decodeURIComponent( dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1),Encoding.UTF8);
                
                if (key.StartsWith("_dllDir")) continue;
                if (key.StartsWith("_version")) continue;
                /*跳出系统参数 20211001*/
                if (key.StartsWith("_clientIPExp")) continue;
                if (key.StartsWith("_focusClassName")) continue;
                if (key.StartsWith("_focusWindowName")) continue;
                if (key.StartsWith("_focusLazyTime")) continue;
                if (key.StartsWith("M_")) {
                    string keyMethod = key.Substring(2);
                    if (!String.IsNullOrEmpty(focusArr[0]) || !String.IsNullOrEmpty(focusArr[1])) {
                        System.Threading.Thread focusThread = new System.Threading.Thread(new Mgr(focusArr[0], focusArr[1], focusArr[2]).FocusWindow);
                        focusThread.Start();
                    }                    
                    string[] Arg = null;
                    try {
                        if (deValue.Contains(PCOUNT)) {
                            Arg = ParseMthArg(deValue);
                            lastMthRtn = evalDll.Invoke(keyMethod, Arg);
                            // new trakWebEdit3.TrakWebClass().ShowLayout(Arg[0],Arg[1],Arg[2],Arg[3]);
                            // new DHCOPPrint.ClsBillPrintClass().ToPrintHDLPStr(Arg[0],Arg[1],Arg[2]);
                        } else {
                            lastMthRtn = evalDll.Invoke(keyMethod);
                        }
                    } catch (Exception e) {
                        if (Arg == null) throw new Exception("调用" + keyMethod + "方法发生异常," + e.Message);
                        else throw new Exception("调用" + keyMethod + "方法发生异常," + e.Message + ", p=" + String.Join(",", Arg));
                    }
                } else {
                    if (!evalDll.SetFValue(key, deValue)) {
                        Logging.Warn(key + "字段写入值:" + deValue + "时失败.");
                    }
                    if (!evalDll.SetPValue(key, deValue)) {
                        Logging.Warn(key + "属性写入值:" + deValue + "时失败.");
                    }
                }
            }
            if (lastMthRtn == null) return null;
            String lastMthRtnStr = lastMthRtn.ToString();
            lastMthRtnStr = encodeURIComponent(lastMthRtnStr,Encoding.UTF8);
            return lastMthRtnStr;
        }
        public static string Run(String txtlang, String code) {
            string rtn = "";
            string[] arr = code.Split('?');
            try {
                string localDllFileName = txtlang.Substring(2, txtlang.Length-4);
                rtn = RunMths(localDllFileName, arr[0], arr[1]);
            } catch (Exception ex) {
                var em = "调用DLL方法发生异常：" + ex.Message;
                if (null!=ex.InnerException) {
                    em += ex.InnerException.Message;
                }
                throw new Exception(em);
            }
            return rtn;
        }
    }
}
