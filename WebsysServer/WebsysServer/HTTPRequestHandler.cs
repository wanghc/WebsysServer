using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using WebsysServer.tool;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;

namespace WebsysServer
{
    class HTTPRequestHandler
    {
        private const string ResponseFormat = "\"status\":{0},\"msg\":\"{1}\",\"rtn\":\"{2}\"";
        private HttpListenerContext ctx ;
        private ActiveObject AObj;
        private int Timeout;
        private string Msg;
        private int Status;
        private string Rtn;
        public static string PCOUNT = "P_COUNT";
        public static string P = "P_";

        public HTTPRequestHandler(HttpListenerContext ctx,int timeout)
        {
            this.ctx = ctx;
            this.Timeout = timeout;
        }
        private void LogReqHeader(HttpListenerRequest request)
        {
            Logging.Debug("-------------------------------------------------------------");
            Logging.Debug("{0} {1} HTTP/1.1", request.HttpMethod, request.RawUrl);

            if (null!=request.AcceptTypes) Logging.Debug("Accept: {0}", string.Join(",", request.AcceptTypes));
            if (null!=request.UserLanguages) Logging.Debug("Accept-Language: {0}", string.Join(",", request.UserLanguages));
            Logging.Debug("User-Agent: {0}", request.UserAgent);
            Logging.Debug("Accept-Encoding: {0}", request.Headers["Accept-Encoding"]);
            Logging.Debug("Connection: {0}", request.KeepAlive ? "Keep-Alive" : "close");
            Logging.Debug("Host: {0}", request.UserHostName);
            Logging.Debug("Pragma: {0}", request.Headers["Pragma"]);
            if (null!= request.Headers["Content-Type"] && request.HttpMethod.Equals("POST")) {
                Logging.Debug("Content-Type:{0}", request.Headers["Content-Type"]);
                if (request.Headers["Content-Type"].Contains("application/json"))
                {   
                }
                if (request.Headers["Content-Type"].Contains("application/x-www-form-urlencoded"))
                {
                }
            }
            if (request.HttpMethod.Equals("GET"))
            {
                //Logging.Debug("Request Body {0}", decodeURIComponent(string.Join("\n", request.QueryString), Encoding.GetEncoding("utf-8")));
            }
        }
        public void RequestHandler(){

            var client = this.ctx;
            if (client == null) return;
            client.Response.ContentType = "application/json";
            var request = client.Request;
            //var NotReturn = request.Headers["NotReturn-Type"]; ///.Equals("1");
            var RequestMethod = request.Headers["Request Method"];
            LogReqHeader(request);
            var coding = Encoding.UTF8;
            // 取得回应对象
            Status = 200;
            Boolean IsStaticReq = false;
            var response = client.Response;
            response.StatusCode = Status;
            response.ContentEncoding = coding;
            response.ContentType = "application/json";
            //content - type: application / javascript
            response.Headers.Set("Server", "MediWay-HTTPAPI/2.0"); //Server: Microsoft - HTTPAPI / 2.0
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
            //jquery发请json请求时，会先发出OPTIONS请求。下面这句表示同意前端请求content-type:application/json
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type,content-type,x-requested-with,NotReturn-Type"); //Access-Control-Allow-Headers
            if ("OPTIONS".Equals(request.HttpMethod))
            {
                response.OutputStream.Close();
                response.Close();
                response = null;
                return;
            }
            //response.AddHeader("Connection", "close");
            //outputStream.WriteLine("HTTP/1.0 200 OK");
            //response.OutputStream.WriteTimeout = timeout * 100;
            /**业务处理*/
            Msg ="success";
            Rtn = "";
            byte[] buffer = Encoding.UTF8.GetBytes(Rtn);
            try
            {
                string fileUrl = request.RawUrl.Replace("//", "/");
                int myind = fileUrl.IndexOf("?", 1);              // /websys/scripts/websys.invoke.js
                string reqUrl = fileUrl;
                if (myind>0) reqUrl = fileUrl.Substring(0,myind);
                String[] urlArr = reqUrl.Split('/');
                String urlFileName = urlArr[urlArr.Length - 1];
                if (request.RawUrl.Contains("/websys/"))
                {
                    MimeSettings mimeSection = System.Configuration.ConfigurationManager.GetSection("mimeSettings") as MimeSettings;
                    Dictionary<String, String> mimeDict = mimeSection.mimeMappings;
                    foreach (var item in mimeDict)
                    {
                        if (urlFileName.EndsWith("."+item.Key))
                        {
                            IsStaticReq = true;
                            response.AddHeader("Content-Type", item.Value+"; charset=utf-8");
                            int ind = fileUrl.IndexOf("/", 1);              // /websys/scripts/websys.invoke.js
                            fileUrl = fileUrl.Substring(ind + 1);
                            fileUrl = CGI.Combine(fileUrl);
                            //Rtn += File.ReadAllText(fileUrl, Encoding.UTF8);
                            fileUrl = HttpUtility.UrlDecode(fileUrl);
                            if (File.Exists(fileUrl))
                            {
                                Status = 200;
                                buffer = File.ReadAllBytes(fileUrl);
                            }
                            else
                            {
                                Status = 404;
                                buffer = Encoding.UTF8.GetBytes("{" + string.Format(ResponseFormat, Status, "文件不存在", "") + "}");
                            }
                        }
                    }
                }
                /*if (request.RawUrl.EndsWith("favicon.ico")) {
                    IsStaticReq = true;
                    Rtn = "";
                }else if (Regex.IsMatch(request.RawUrl,"(.css)|(.js)|(.html)$"))
                {
                    IsStaticReq = true;
                    if (Regex.IsMatch(request.RawUrl, "(.js)$")) response.AddHeader("Content-Type", "application/javascript; charset=utf-8");
                    if (Regex.IsMatch(request.RawUrl, "(.css)$")) response.AddHeader("Content-Type", "text/css; charset=utf-8");
                    if (Regex.IsMatch(request.RawUrl, "(.html)$")) response.AddHeader("Content-Type", "text/html; charset=utf-8");
                    string fileUrl = request.RawUrl.Replace("//", "/");
                    int ind = fileUrl.IndexOf("/", 1); // /websys/scripts/websys.invoke.js
                    fileUrl = fileUrl.Substring(ind + 1);
                    fileUrl = CGI.Combine(fileUrl);
                    Rtn += File.ReadAllText(fileUrl, Encoding.UTF8);
                } else {*/
                if (!IsStaticReq) { 
                    IsStaticReq = false;
                    string reqBodyStr = "";
                    if (request.InputStream != null) { //POST
                        var sr = new StreamReader(request.InputStream);
                        reqBodyStr = sr.ReadToEnd();
                        Logging.Debug("Request Body {0}", decodeURIComponent(decodeURIComponent(reqBodyStr, Encoding.GetEncoding("utf-8")),Encoding.GetEncoding("utf-8")));
                    }
                    //if ("1".Equals(NotReturn))
                    //{
                        //运行客户端程序时不卡死 2020-03-06
                        //byte[] buffer = Encoding.UTF8.GetBytes("{" + string.Format(ResponseFormat, Status, "NotReturn Invoke", "") + "}");
                        //response.ContentLength64 = buffer.Length;
                        //response.OutputStream.Write(buffer, 0, buffer.Length);
                        //response.OutputStream.Close();
                    //}
                    if (request.QueryString.Count > 0)
                    {
                        NameValueCollection collection = request.QueryString;
                        String[] keyArray = collection.AllKeys;
                        foreach (string key in keyArray)
                        {
                            String[] valuesArray = collection.GetValues(key);
                            foreach (string myvalue in valuesArray)
                            {
                                reqBodyStr += "&"+key + "=" + myvalue;
                            }
                        }
                        //reqBodyStr = string.Join(",", request.QueryString); //GET
                    }

                   if(Regex.IsMatch(request.RawUrl, Properties.Settings.Default.HttpServerApplication + "cmd/"))
                    {
                        Rtn = new ScriptShell().CmdRun(reqBodyStr);
                    }
                    else if (Regex.IsMatch(request.RawUrl, Properties.Settings.Default.HttpServerApplication + "mgr/"))
                    {
                        Rtn = new Mgr().MgrRun(request.RawUrl,reqBodyStr);
                    }
                    else
                    {
                        //response.OutputStream
                        Rtn = RunMths(request.RawUrl, reqBodyStr);
                    }
                }
            }catch (Exception e){
                Status = 500;
                Msg = "处理请求异常:"+e.Message;
                if (null != e.InnerException)
                {
                    Msg +=" : "+ e.InnerException.Message;
                }
                Msg = Msg.Replace("\n", "\\n").Replace("\r","\\r");
                Logging.LogUsefulException(e);
                ///静态文件报错
                buffer = Encoding.UTF8.GetBytes("{" + string.Format(ResponseFormat, Status, Msg, Rtn) + "}");
            }
            finally
            {
                /**返回响应*/
                try
                {
                    if (true) //!"1".Equals(NotReturn))
                    {
                        if (!IsStaticReq)
                        {
                            Rtn = JsonHelper.escape(Rtn);
                            Rtn = "{" + string.Format(ResponseFormat, Status, Msg, Rtn) + "}";
                            buffer = Encoding.UTF8.GetBytes(Rtn);
                        }

                        //对客户端输出相应信息.
                        response.StatusCode = Status; //200;
                        response.ContentLength64 = buffer.Length;
                        Stream output = response.OutputStream;
                        if (null != output)
                        {
                            output.Write(buffer, 0, buffer.Length);
                            //关闭输出流，释放相应资源
                            output.Close();
                            output.Dispose();
                        }
                    }
                    //Logging.Info("Thread ------------Abort ");
                    //System.Threading.Thread.CurrentThread.Abort();
                }
                catch (Exception e)
                {
                    Logging.Error("输出Response时出错。");
                    Logging.LogUsefulException(e);
                }
                finally
                {
                    try
                    {
                        if (client.Response != null) client.Response.Close();
                    }
                    catch (Exception e)
                    {
                        Logging.Error("关闭Response时出错。");
                        Logging.LogUsefulException(e);
                    }
                }
            }
        }

        public static string decodeURIComponent(string str,Encoding encoding)
        {
            return System.Web.HttpUtility.UrlDecode(str, System.Text.Encoding.GetEncoding("utf-8"));
        }
        /// <summary>
        /// 将获取的formData存入字典数组
        /// </summary>
        public static Dictionary<String, String> DataToDict(string formData)
        {
            try
            {
                String[] dataArry = formData.Split('&');
                Dictionary<String, String> dataDic = new Dictionary<string, string>();
                for (int i = 0; i <= dataArry.Length - 1; i++)
                {
                    String dataParm = dataArry[i];
                    if (dataParm != "")
                    {
                        int dIndex = dataParm.IndexOf("=");
                        String key = dataParm.Substring(0, dIndex);
                        String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                        String deValue = decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                        if (key != "__VIEWSTATE")
                        {
                            dataDic.Add(key, deValue);
                        }
                    }
                }
                return dataDic;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /**
         *url=/websys/1/Interop.Lodop/Lodop.LodopXClass
         *query=M_FORMAT=p_count=1&p_1=222&p_333   &    M_PRINT=  &  M_ADD_PRINT_TEXT=
         * 
         */
        public string RunMths(string url, string query)
        {
           
            /*DHCOPPrint.ClsBillPrint prt = new DHCOPPrint.ClsBillPrintClass();
            prt.ToPrintHDLPStr("", "", "<?xml version=\"1.0\" encoding=\"gb2312\" ?>< appsetting >"
    +"< invoice PrtPaperSet = \"HAND\" height = \"27.99\" LandscapeOrientation = \"Y\" width = \"19.58\" PrtDevice = \"\" PrtPage = \"\" PaperDesc = \"\" XMLClassMethod = \"\" XMLClassQuery = \"\" >"
    + "< ListData PrintType = \"List\" YStep = \"4.497\" XStep = \"0\" CurrentRow = \"1\" PageRows = \"30\" RePrtHeadFlag = \"Y\" BackSlashWidth = \"0\" ></ ListData >"
    + "< PLData RePrtHeadFlag = \"Y\" ></ PLData >"
    + "< PICData RePrtHeadFlag = \"N\" ></ PICData >"
    +"< TxtData RePrtHeadFlag = \"N\" >"
    +"< txtdatapara name = \"label0\" xcol = \"5.556\" yrow = \"8.73\" defaultvalue = \"我是不会换行的字符患，如何不换行说明cab包正确。width默认成0了\" printvalue = \"\" fontbold = \"false\" fontname = \"宋体\" fontsize = \"16\" />"
    +"</ TxtData ></ invoice ></ appsetting >");
            return "0000";*/
            object lastMthRtn = "";
            string[] arr = url.Split(new char[] { '/' });
            AObj = new ActiveObject();
            //AObj.Mode = int.Parse(arr[2]);
            AObj.Ass = arr[2];
            AObj.Cls = arr[3];
            String[] dataArry = query.Split('&');
            for (int i = 0; i < dataArry.Length; i++)
            {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm))
                {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue = decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                    //_192.168.1.[18-200] , 192.168.[2-4].*, 10.*.10.1
                    if (key.Equals("_clientIPExp")) AObj.ClientIPExp = deValue;
                    if (key.Equals("_dllDir")) AObj.DllPath = deValue;
                    if (key.Equals("_version")) AObj.Version = deValue;
                    if (key.Equals("_cmd")) AObj.CmdRun = deValue;
                    if (key.Equals("_focusClassName")) AObj.focusClassName = deValue;
                    if (key.Equals("_focusWindowName")) AObj.focusWindowName = deValue;
                    if (key.Equals("_focusLazyTime")) AObj.focusLazyTime = deValue;
                }
                if (!AObj.Version.Equals("") && !AObj.DllPath.Equals("") && !("".Equals(AObj.CmdRun)))
                {
                    break;
                }
            }
            string x = AObj.CreateXObject();
            if (!"0".Equals(x))
            {
                Msg = x;
                return "";
            }
            
            for (int i=0; i<dataArry.Length; i++)
            {
                string dataParm = dataArry[i];
                if (!String.IsNullOrEmpty(dataParm))
                {
                    int dIndex = dataParm.IndexOf("=");
                    String key = dataParm.Substring(0, dIndex);
                    String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                    String deValue = decodeURIComponent(value, Encoding.GetEncoding("utf-8"));
                    if (key.StartsWith("_dllDir")) continue;
                    if (key.StartsWith("_version")) continue;
                    /*跳出系统参数 20211001*/
                    if (key.StartsWith("_clientIPExp")) continue;
                    if (key.StartsWith("_focusClassName")) continue;
                    if (key.StartsWith("_focusWindowName")) continue;
                    if (key.StartsWith("_focusLazyTime")) continue;
                    if (key.StartsWith("M_"))
                    {
                        if (deValue.Contains(PCOUNT))
                        {
                            string[] Arg = ParseMthArg(deValue);
                            //new trakWebEdit3.TrakWebClass().ShowLayout(Arg[0],Arg[1],Arg[2],Arg[3]);
                            try
                            {
                                if (!("".Equals(AObj.focusClassName)) || !("".Equals(AObj.focusWindowName)))
                                {
                                    System.Threading.Thread focusThread = new System.Threading.Thread(new Mgr(AObj.focusClassName, AObj.focusWindowName, AObj.focusLazyTime).FocusWindow);
                                    focusThread.Start();
                                }
                                //new DHCOPPrint.ClsBillPrintClass().ToPrintHDLPStr(Arg[0],Arg[1],Arg[2]);
                                lastMthRtn = AObj.Invoke(key.Substring(2), Arg);
                                
                            }
                            catch (Exception e)
                            {
                                throw new Exception("调用"+key.Substring(2)+"方法发生异常,"+e.InnerException+", p="+String.Join(",",Arg));
                            }
                        }
                        else
                        {
                            try{

                                lastMthRtn = AObj.Invoke(key.Substring(2));
                            }catch (Exception e)
                            {
                                throw new Exception("调用" + key.Substring(2) + "方法发生异常" + e.InnerException);
                            }
                        }
                    }
                    else
                    {
                        if(!AObj.SetFValue(key, deValue)){
                            Logging.Error(key + "字段写入值:" + deValue + "时失败.");
                            //Msg = key+"属性写入值:"+deValue+"时失败.";
                        }
                        
                        if (!AObj.SetPValue(key, deValue))
                        {
                            Logging.Error(key + "属性写入值:" + deValue + "时失败.");
                            //Msg = key+"属性写入值:"+deValue+"时失败.";
                        }
                    }
                }
            }
            if (lastMthRtn == null) return null;
            return lastMthRtn.ToString();
        }
        public static string[] ParseMthArg(String mthParamStr){
            Dictionary<string, string> data = DataToDict(mthParamStr);
            string[] Args = null;
            if (data.ContainsKey(PCOUNT))  Args = new string[Int16.Parse(data[PCOUNT])];
            if (Args==null) return Args;
            for (var i=0;i<Args.Length;i++) Args[i] = data.ContainsKey(P+i) ? data[P + i] : "";
            return Args;
        }


        public string ParseQueryStr(string query)
        {
            string[] Args = null;
            Dictionary<string, string> data = DataToDict(query);
            Dictionary<string, string> props = new Dictionary<string, string>();
            if (data.ContainsKey(PCOUNT))
            {
                Args = new string[Int16.Parse(data[PCOUNT])];
            }
            foreach (string item in data.Keys)
            {
                if (item.Contains(P))
                {
                    if (Args == null) return "未传入p_count参数值";
                    if (!item.Substring(2).ToLower().Equals("count"))
                    {
                        int i = Int16.Parse(item.Substring(2));
                        Args[i - 1] = decodeURIComponent( data[item],Encoding.UTF8);
                    }
                }
                else
                {
                    props.Add(item, decodeURIComponent(data[item], Encoding.UTF8));
                }
            }
            AObj.Param = Args;
            AObj.Props = props;
            return "";
        }
    }
}
