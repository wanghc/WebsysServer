using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WebsysScript
{
    class HandlerFile
    {
        public static void AfterRun(Boolean flag ,String error,String MyCodeFile)
        {
            if (flag)
            {
                //成功运行后删除文件 20220809不再删除
                // File.Delete(MyCodeFile);
                // 把运行result写入--MyCode[xxxxxx].txt中
                using (StreamWriter sw = File.AppendText(MyCodeFile)) {
                    sw.WriteLine(error);
                    sw.Flush();
                    sw.Close();
                }
            }
            else
            {   // 把运行报错写入MyCode[xxxxxx].txt中
                using (StreamWriter sw = File.AppendText(MyCodeFile))
                {
                    sw.WriteLine(error);
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        public static string RenameToRuningFile(string path) {
            string file_path = path.Substring(0, path.LastIndexOf("\\") + 1);
            string file_name = path.Substring(path.LastIndexOf("\\") + 1);
            string newPath = Path.Combine(file_path, "--" + file_name);
            System.IO.File.Move(Path.Combine(path), newPath); // 正在处理的文件名前加二个横线（--），表示正在运行txt中代码
            return newPath;
        }
        public static string RenameToRuningFile(string path, string name) {
            System.IO.File.Move(path + "\\" + name, path + "\\--" + name); // 正在处理的文件名前加二个横线（--），表示正在运行txt中代码
            return path + "\\--" + name;         
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curpath">程序的temp目录</param>
        /// <param name="MyCodeFileName">文件名或完整路径名</param>
        /// <returns></returns>
        /***
         *      运行 /temp/MyCode.txt 内的代码
         *      运行 /temp/MyCode + DateTime.Now.ToFileTimeUtc().ToString().txt 内的代码 ，可能有多个，运行成功后，删除txt，如果不成功写入报错信息到txt中
         *      一次运行会被web服务(ScriptShell)调用一次
         *      
         *      
         *      第一行 jscript
         *      第二行 代码
         *      或
         *      第一行 vbscripts
         *      第二行 代码
         *      或
         *      第一行 dll
         *      第二行 url
         * */
        public static string Handler(String curpath, String MyCodeFileName)
        {
            String rtn = "";
            Boolean success = true;
            string MyCodeFile = null;
            try {
                MyCodeFileName = MyCodeFileName.Replace("/","\\");
                if (File.Exists(MyCodeFileName)) {
                    MyCodeFile = Path.Combine(MyCodeFileName);
                } else {
                    MyCodeFile = Path.Combine(curpath, MyCodeFileName);
                }
                if (!File.Exists(MyCodeFile)) {  // 不存在MyCode文件时, 去处理MyCode[xxxx].txt
                    DirectoryInfo TheFolder = new DirectoryInfo(curpath);
                    foreach (FileInfo NextFile in TheFolder.GetFiles()) {
                        if (NextFile.Name.StartsWith("MyCode")) {
                            MyCodeFile = RenameToRuningFile(curpath, NextFile.Name);
                            break;
                        }
                    }
                } else {
                    MyCodeFile = RenameToRuningFile(MyCodeFile);
                }
            } catch (Exception e) {
                return "ERROR^" + e.Message;
            }
            string txtlang = "", code = "";
            if (File.Exists(MyCodeFile))
            {

                StringBuilder sb = new StringBuilder();
                //2021-11-16 把using提前结束。不然会占用MyCode.txt文件
                using (StreamReader sr = File.OpenText(MyCodeFile))
                {
                    txtlang = sr.ReadLine();
                    //while (!sr.EndOfStream) {
                    //    sb.Append(sr.ReadLine());
                    //}
                    // 每行后的回车不能删除，excel导出时，vbs是要回车的
                    code = sr.ReadToEnd(); //;-> 会得到每行最后面的【回车换行符】\r\n
                    sr.Close();
                }
            } else {
                return "-1^没有找到代码文件";
            }
            if (txtlang.Equals("")) return "-2^没有需要的语句需要运行";
            string errorMessage = "";
            try
            {
                if (txtlang.ToLower().IndexOf("dll") > -1) {
                    tool.Logging.Debug("开始运行:" + txtlang);
                    rtn = EvalDll.Run(txtlang, code); // 此方法会同步
                    tool.Logging.Debug("返回结果：" + rtn);
                } else if (txtlang.ToLower().IndexOf(".jar") > -1) {
                    tool.Logging.Debug("开始运行:" + txtlang);
                    String jarFileName = txtlang.Substring(2, txtlang.ToLower().IndexOf(".jar") + 3);
                    if (File.Exists(jarFileName))
                    {
                        String dirName = Path.GetDirectoryName(jarFileName);
                        tool.Logging.Debug("开始运行: dirName: " + dirName + ",code: " + code);
                        rtn = EvalCmd.Run(dirName, code, false); // 此方法会同步
                    }
                    tool.Logging.Debug("返回结果：" + rtn);
                }else if (txtlang.ToLower().IndexOf(".exe") > -1)
                {
                    tool.Logging.Debug("开始运行:" + txtlang);
                    String exeFileName = txtlang.Substring(2, txtlang.ToLower().IndexOf(".exe") + 2);
                    if (File.Exists(exeFileName))
                    {
                        String dirName = Path.GetDirectoryName(exeFileName);
                        tool.Logging.Debug("开始运行: dirName: " + dirName + ",code: " + code);
                        rtn = EvalCmd.Run(dirName, code, false); // 此方法会同步
                    }
                    tool.Logging.Debug("返回结果：" + rtn);
                }
                else {
                    rtn = EvalScript.Run(txtlang, code);
                }
            }catch(Exception ex){
                success = false;
                rtn = ex.Message;
                errorMessage = "\n\n/*\n "+ex.Source+"：\n"  + ex.Message + "\n" + ex.StackTrace + "\n*/";
            }
            if (success) errorMessage = "\n\nWebsysScriptRESULT=" + rtn;
            AfterRun(success, errorMessage, MyCodeFile);            
            if (success) return rtn;
            return "ERROR^"+rtn;

            //string MyRtnFile = Path.Combine(curpath, "MyRtn.txt");
            //using (StreamWriter sw = File.CreateText(MyRtnFile))
            //{
            //    sw.WriteLine(rtn);
            //    sw.Close();
            //}
            
            //string MyRtnFile = Path.Combine(curpath, "MyRtn.txt");
            //using (StreamWriter sw = File.CreateText(MyRtnFile))
            //{
            //    sw.WriteLine(MyCodeFile + "文件不存在");
            //    sw.Flush();
            //    sw.Close();
            //}
            
        }
    }
}
