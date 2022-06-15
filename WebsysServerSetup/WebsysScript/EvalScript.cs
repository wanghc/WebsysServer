using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
namespace WebsysScript
{
    class EvalScript
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        public static string Run (String txtlang, String str)
        {
            string rtn = "";
            try
            {
                MSScriptControl.ScriptControlClass s = new MSScriptControl.ScriptControlClass();
                s.AllowUI = true;
                s.Timeout = 5 * 60 * 1000; // 导出Excel 1000行*20列的数据 JS花费1分钟左右，VB下2秒左右
                s.UseSafeSubset = false;   // false表示低安全运行。设置为true时报： "ActiveX 部件不能创建对象: 'Excel.Application'"
                IntPtr hWndint = GetForegroundWindow();
                s.SitehWnd = hWndint.ToInt32();   // 指定弹出窗口在当前弹出，默认为桌面
                if (txtlang.ToLower().IndexOf("vbscript") > -1)
                {
                    s.Language = "VBScript";
                    if (0 != str.ToLower().IndexOf("function")) str = "Function vbs_Test\n" + str + "vbs_Test = 1\n End Function \n";  //默认返回空值，如果业务上有返回也不影响
                    s.Reset();
                    s.AddCode(str);
                    rtn = s.Run("vbs_Test").ToString();
                }
                else if (txtlang.ToLower().IndexOf("jscript") > -1)
                {
                    s.Language = "JScript";
                    if (0 != str.IndexOf("(function")) str = "(function test(){" + str + "return 1;})();";  //默认返回空值，如果业务上有返回也不影响
                    s.Reset();
                    rtn = s.Eval(str).ToString();
                    s = null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return rtn;
        }
    }
}
