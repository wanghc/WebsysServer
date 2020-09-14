using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebsysServer.tool;

namespace WebsysServer
{
    class CfgJson
    {
        private const string FunTpl = "/**{3}*/\nICls.{2} = function (){{\n\tthis.mode={5};\n\tthis.ass=\"{4}{0}\";\n\tthis.cls=\"{1}\";";
        public static CfgJsonDto cfgJsonDto { get; set; }
        public CfgJson()
        {
            CfgJson.cfgJsonDto = new CfgJsonDto();
            CfgJson.cfgJsonDto.HttpServerUrl = "http://" + Properties.Settings.Default.HttpServerIP + ":{0}" + Properties.Settings.Default.HttpServerApplication;
            CfgJson.cfgJsonDto.HttpServerPort = Properties.Settings.Default.HttpServerPort;
            /*string cfgPath = Path.Combine(@"config\settings.json");
            if (File.Exists(cfgPath))
            {
                CfgJson.cfgJsonDto = JsonHelper.JsonFileToT<CfgJsonDto>(cfgPath);
                //CfgJson.GenJsFile();
            }*/
        }
        public static void GenJsFile()
        {            
            string curpath = Path.Combine(System.Windows.Forms.Application.StartupPath, @"scripts");    // Path.GetFullPath(".");//Path.GetTempPath();
            if (!Directory.Exists(curpath))
            {
                Directory.CreateDirectory(curpath);
            }
            string JsFile = Path.Combine(curpath, "websys.invoke.js");
            FileStream fs = null;
            if (File.Exists(JsFile)) {
                fs = new FileStream(JsFile, FileMode.Create);
            } else {
                fs = new FileStream(JsFile, FileMode.CreateNew);
            }
            if (fs != null)
            {
                CfgJsonDto dto = CfgJson.cfgJsonDto;
                string url = string.Format(dto.HttpServerUrl, dto.HttpServerPort);
                StreamWriter sw = new StreamWriter(fs);
                CGI cgi = new CGI();
                //JinianNet.JNTemplate.ITemplate template = JinianNet.JNTemplate.Engine.LoadTemplate(@"D:\workspace_net\WebsysServer\WebsysServer\bin\Debug\tmpl\scripts\websys.invoke.js");
                /*var template = JinianNet.JNTemplate.Engine.LoadTemplate("websys.invoke.js");
                template.Context.TempData["ServerURL"] = url;
                template.Context.TempData["CPrefix"] = VarName.JSCPrefix;
                template.Context.TempData["CGI"] = cgi;
                template.Context.TempData["AssList"] = dto.AssList;
                template.Context.TempData["AssDirList"] = dto.AssDir;
                //template.Context.CurrentPath=@""
                template.Render(sw);*/
                sw.AutoFlush = true;
                sw.Close();
                return ;
            }
        }
    }
}
