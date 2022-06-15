using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
namespace WebsysScript.tool
{
    class Mgr {
        private string lpClassName = "";
        private string lpWindowName = "";
        private Int16 focusLazyTime = 1000; /*延时1000ms*/
        public Mgr() {
            lpClassName = "";
            lpWindowName = "";
            focusLazyTime = 1000;
        }
        public Mgr(string lpClassName, string lpWindowName, string focusLazyTime) {
            this.lpClassName = lpClassName;
            this.lpWindowName = lpWindowName;
            if (Int16.TryParse(focusLazyTime, out short time)) {
                this.focusLazyTime = time;
            } else {
                this.focusLazyTime = 1000;
            }
        }
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);

        [DllImport("user32.dll", EntryPoint = "MoveWindow", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(System.IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        private static extern IntPtr GetForegroundWindow();
        /***
         * 1-恢复，2最小化，3最大化, 8以窗口原来的状态显示窗口
         */
        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);
        /**
         bvk为键值，例如回车13，bScan设置为0，dwFlags设置0表示按下，2表示抬起；dwExtraInfo也设置为0即可。
            */
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "SetFocus", CharSet = CharSet.Auto)]
        public static extern IntPtr SetFocus(IntPtr hWnd);//设定焦点
        [DllImport("user32.dll", EntryPoint = "SetWindowPos", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        public int GetInt(string value, int _default) {
            if (int.TryParse(value, out _default))
                return Convert.ToInt32(value);
            else
                return _default;
        }
        /*线程方式必须无返回值*/
        public void FocusWindow() {
            if ("".Equals(lpClassName)) lpClassName = null;
            if ("".Equals(lpWindowName)) lpWindowName = null;
            System.Threading.Thread.Sleep(this.focusLazyTime);
            IntPtr maindHwnd = FindWindow(lpClassName, lpWindowName);
            if (maindHwnd != IntPtr.Zero) {
                int focusPtr = ShowWindow(maindHwnd, 1);
                const int HWND_TOPMOST = -1;// 将窗口置于列表顶部，并位于任何最顶部窗口的前面
                const int SWP_NOSIZE = 1; // 保持窗口大小
                const int SWP_NOMOVE = 2; //保持窗口位置
                int focusPtr2 = SetWindowPos(maindHwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE); // '将窗口设为总在最前
                //IntPtr focusPtr2 = SetFocus(maindHwnd);
            }
        }
        /*[DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, int lParam);
        const int WM_SETICON = 0x80;
        public void SetIcon(){
            if ("".Equals(lpClassName)) lpClassName = null;
            if ("".Equals(lpWindowName)) lpWindowName = null;
            IntPtr maindHwnd = GetForegroundWindow(); //FindWindow(lpClassName, lpWindowName);
            if (maindHwnd != IntPtr.Zero) {
                //System.Drawing.Icon image = Properties.Resources.ico32;
                //SendMessage(maindHwnd, WM_SETICON, IntPtr.Zero, (int)image.Handle);
            }
            
        }*/
    }
}
