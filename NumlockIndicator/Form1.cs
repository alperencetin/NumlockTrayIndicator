using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace NumlockIndicator
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        static int iconStatus = 0;
        const int WM_KEYDOWN = 0x100;
        const int WH_KEYBOARD_LL = 13;
        private static IntPtr hhook = IntPtr.Zero;
        private LowLevelKeyboardProc _proc = hookProc;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = new Icon("on.ico");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }
        private static IntPtr hookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                if (key == Keys.NumLock)
                {
                    if(!Control.IsKeyLocked(Keys.NumLock)) iconStatus = 1;
                    else iconStatus = 2;
                }
            }
            return CallNextHookEx(hhook, nCode, wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            SetHook();
            BeginInvoke(new MethodInvoker(delegate
            {
                this.Hide();
            }));
            if (Control.IsKeyLocked(Keys.NumLock)) iconStatus = 1;
            else iconStatus = 2;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            UnHook();
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if(iconStatus > 0)
            {
                backgroundWorker1.ReportProgress(iconStatus);
                iconStatus = 0;
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                notifyIcon1.Icon = new Icon("on.ico");
                notifyIcon1.Text = "Numpad ON";
            }
            else
            {
                notifyIcon1.Icon = new Icon("off.ico");
                notifyIcon1.Text = "Numpad OFF";
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }
    }
}
