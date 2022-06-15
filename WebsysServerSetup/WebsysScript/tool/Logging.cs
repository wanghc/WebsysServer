using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WebsysScript.tool
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
        public static int CurLogLevel = 4; //  错误会作为返回值返回，不用显示Debug,Error
        public static bool OpenLogFile()
        {
            return true;
        }
        public static bool ColseLogFile()
        {
            
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
                Console.Write("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ");
                if (values == null || values.Length == 0)
                {
                Console.WriteLine("[" + strMap[(int)level] + "]" + s);
                }
                else if (values.Length > 0)
                {
                Console.WriteLine("[" + strMap[(int)level] + "]" + string.Format(s, values));

                }
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