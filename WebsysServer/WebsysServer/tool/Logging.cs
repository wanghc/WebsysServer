using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebsysServer.tool
{
    public enum LogLevel
    {
        Debug = 0,
        Info,
        Warn,
        Error,
        Assert,
    }

    public class Logging
    {
        public static string LogFile;
        public static int CurLogLevel = 0;
        private static FileStream fs;
        private static StreamWriter sw;
        public static bool OpenLogFile()
        {
            try
            {
                string curpath = Path.Combine(System.Windows.Forms.Application.StartupPath, @"temp");// Path.GetFullPath(".");//Path.GetTempPath();
                if (!Directory.Exists(curpath))
                {
                    Directory.CreateDirectory(curpath);
                }
                LogFile = Path.Combine(curpath, "console.log");
                fs = new FileStream(LogFile, FileMode.Append);
                sw = new StreamWriter(fs);
                sw.AutoFlush = true;
                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public static bool ColseLogFile()
        {
            try
            {
                if (null!=sw) {
                    sw.Close();
                    sw.Dispose();
                }
                if(null!=fs) fs.Close();
            }
            catch (IOException e )
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
        private static string ToString(StackFrame[] stacks)
        {
            string result = string.Empty;
            foreach (StackFrame stack in stacks)
            {
                result += string.Format("{0}\r\n", stack.GetMethod().ToString());
            }
            return result;
        }

        public static void LogUsefulException(Exception e)
        {
            // just log useful exceptions, not all of them
            if (e is SocketException)
            {
                SocketException se = (SocketException)e;
                if (se.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    // closed by browser when sending
                    // normally happens when download is canceled or a tab is closed before page is loaded
                }
                else if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // received rst
                }
                else if (se.SocketErrorCode == SocketError.NotConnected)
                {
                    // close when not connected
                }
                else if ((uint)se.SocketErrorCode == 0x80004005)
                {
                    // already closed
                }
                else if (se.SocketErrorCode == SocketError.Shutdown)
                {
                    // ignore
                }
                else
                {
                    string s = e.Message;
                    if (null != e.InnerException)
                    {
                        s += "。innerException: " + e.InnerException.Message + ":" + e.InnerException.Source;
                    }
                    Error(s);
                    //Error(ToString(new StackTrace().GetFrames()));
                    //Console.WriteLine(e);
                    //#if DEBUG
                    //Console.WriteLine(ToString(new StackTrace().GetFrames()));
                    //#endif
                }
            }
            else
            {
                string s = e.Message;
                if (null != e.InnerException)
                {
                    s += "。innerException: " + e.InnerException.Message + ":" + e.InnerException.Source;
                }
                Error(s);
                //Error(ToString(new StackTrace().GetFrames()));

                //Console.WriteLine(e);
                //#if DEBUG
                //Console.WriteLine(ToString(new StackTrace().GetFrames()));
                //#endif
            }
        }
        public static void Debug(String s,params string[] values)
        {
            Log(LogLevel.Debug, s, values);
        }
        public static void Info(String s, params string[] values)
        {
            Log(LogLevel.Info, s, values);
        }

        public static void Warn(String s, params string[] values)
        {
            Log(LogLevel.Warn, s, values);
        }
        public static void Error(Exception e, params string[] values)
        {
            string s = e.Message + " : " + e.Source;
            if (null != e.InnerException)
            {
                s += "。innerException: "+e.InnerException.Message + ":" + e.InnerException.Source;
            }
            Log(LogLevel.Error,s , values);
        }
        public static void Error(String s,params string[] values)
        {

            Log(LogLevel.Error, s, values);
        }

        public static void Log(LogLevel level, String s, params string[] values)
        {
            if (CurLogLevel <= (int)level)
            {
                String[] strMap = new String[5]{ "Debug", "Info", "Warn", "Error", "Assert"};
                //Console.WriteLine("[" + strMap[(int)level] + "]" + string.Format(s,values));
                //try
                //{
                if (sw != null && sw.BaseStream!=null)
                {
                    sw.Write("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Thread.CurrentThread.ManagedThreadId + "-" + Thread.CurrentThread.Name + "] ");
                    if (values == null || values.Length == 0)
                    {
                        sw.WriteLine("[" + strMap[(int)level] + "]" + s);
                    }
                    else if (values.Length > 0)
                    {
                        sw.WriteLine("[" + strMap[(int)level] + "]" + string.Format(s, values));

                    }
                }
                //}catch(Exception e)
                //{
                //    Console.WriteLine("Log-Method："+e.Message);
                //}
            }
        }
        public static void LogBin(LogLevel level, string info, byte[] data, int length)
            {
#if DEBUG
                return;
                string s = "";
                for (int i = 0; i < length; ++i)
                {
                    string fs = "0" + Convert.ToString(data[i], 16);
                    s += " " + fs.Substring(fs.Length - 2, 2);
                }
                Log(level, info + s);
#endif
            }
        }
    }