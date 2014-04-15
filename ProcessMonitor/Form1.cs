using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProcessMonitor
{
    public partial class Form1 : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();
            notifyIcon1.Visible = false;

            string text = String.Format("[{0}] {1}", AprilUtility.GetCorrentTimeString(), "Started ProcessMonitor!\n");
            AppendToMainTextBox(ref text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            contextMenuStrip1.Hide();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            if (string.Empty != Properties.Settings.Default.MonitorProcessName)
            {
                m_TargetMonitorProcessName = Properties.Settings.Default.MonitorProcessName;
                m_TargetMonitorProcessFileName = Properties.Settings.Default.MonitorProcessFullName;

                m_ExistPrevMonitorProcess = true;
                StartMonitorProcessWorkerThread();

                string TempText = String.Format("Exist the Previously Monitored Process! [{0}]", m_TargetMonitorProcessName);
                PrintAndWriteFileWithTime(TempText);
            }
            else
            {
                m_ExistPrevMonitorProcess = false;
            }

            //AprilUtility.CreateDirectory(ref AprilUtility.g_LogDirName);

            //if (true == AprilUtility.ExistFile(ref AprilUtility.g_ProcessLogFileName))
            //{
                // 감시하던게 있으니 계속 감시.
            //    string ReadFileText = string.Empty;
            //    bool ReadPrevInfo = AprilUtility.ReadFileToEnd(ref AprilUtility.g_ProcessLogFileName, ref ReadFileText);

            //    // 프로세스의 이름과 실행 경로를 얻고, 감시를 시작한다.
            //    if (true == ReadPrevInfo && string.Empty != ReadFileText)
            //    {
            //        string[] ProcessInfo = ReadFileText.Split('|');
            //        m_TargetMonitorProcessName = ProcessInfo[0];
            //        m_TargetMonitorProcessFileName = ProcessInfo[1];

            //        m_ExistPrevMonitorProcess = true;
            //        StartMonitorProcessWorkerThread();

            //        string TempText = String.Format("Exist the Previously Monitored Process! [{0}]", m_TargetMonitorProcessName);
            //        PrintAndWriteFileWithTime(TempText);
            //    }
            //    else
            //    {
            //        m_ExistPrevMonitorProcess = false;
            //    }
            //}
            //else
            //{
            //    m_ExistPrevMonitorProcess = false;
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            HideFromAndVisibleTray();
            SetTrayNotifyBalloonTip();
        }

        delegate void AvoidCrossThreadDelegateRichTextBox(ref string Text);
        public void AppendToMainTextBox(ref string Text)
        {
            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    AvoidCrossThreadDelegateRichTextBox tempDelegate = new AvoidCrossThreadDelegateRichTextBox(AppendToMainTextBox);
                    richTextBox1.Invoke(tempDelegate, Text);
                }
                else
                {
                    richTextBox1.AppendText(Text);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

        public void SaveMonitorProcessInfo(Process MonitorProcess)
        {
            ProcessModule CurProcessModule = MonitorProcess.MainModule;

            m_TargetMonitorProcessID = MonitorProcess.Id.ToString();
            m_TargetMonitorProcessName = MonitorProcess.ProcessName;
            m_TargetMonitorProcessFileName = CurProcessModule.FileName;

            m_RunningMonitorProcess = true;

            Properties.Settings.Default.MonitorProcessName = MonitorProcess.ProcessName;
            Properties.Settings.Default.MonitorProcessFullName = CurProcessModule.FileName;
            Properties.Settings.Default.Save();

            //string strText = String.Format("{0}|{1}", MonitorProcess.ProcessName,
            //                                     CurProcessModule.FileName);

            //AprilUtility.WriteToFile(ref AprilUtility.g_ProcessLogFileName, strText);
        }

        public volatile bool m_RunningMonitorProcess = false;
        public static Process g_TargetProcess;

        /******************************************************
        * ProcessInfo.
        ******************************************************/
        private bool m_ExistPrevMonitorProcess = false;
        private string m_TargetMonitorProcessID = "";
        private string m_TargetMonitorProcessName = "";
        private string m_TargetMonitorProcessFileName = "";

        public string GetTargetMonitorProcessName()
        {
            return m_TargetMonitorProcessName;
        }

        public string GetTargetMonitorProcessFileName()
        {
            return m_TargetMonitorProcessFileName;
        }

        public void SetTargetMonitorProcessInfo(ref string[] ProcessInfo)
        {
            SetTargetMonitorProcessInfo(ref ProcessInfo[0], ref ProcessInfo[1], ref ProcessInfo[2]);
        }

        public void SetTargetMonitorProcessInfo(ref string Name, ref string ID, ref string FileName)
        {
            m_TargetMonitorProcessName = Name;
            m_TargetMonitorProcessID = ID;
            m_TargetMonitorProcessFileName = FileName;
        }

        public void PrintTargetMonitorProcessInfo()
        {
            string TempText = String.Format("m_TargetMonitorProcessName : {0}", m_TargetMonitorProcessName);
            PrintAndWriteFileWithTime(TempText);

            TempText = String.Format("m_TargetMonitorProcessID : {0}", m_TargetMonitorProcessID);
            PrintAndWriteFileWithTime(TempText);

            TempText = String.Format("m_TargetMonitorProcessFileName : {0}", m_TargetMonitorProcessFileName);
            PrintAndWriteFileWithTime(TempText);
        }

        public volatile bool _RunningMonitorThread = false;
        private static void MonitorProcessWorker(object Args)
        {
            Form1 CurForm = (Form1)Args;
            CurForm.PrintAndWriteFileWithTime("Entry MonitorProcessWorker Thread.");

            while (true == CurForm._RunningMonitorThread)
            {
                try
                {
                    Process[] Processlist = Process.GetProcesses();
                    foreach (Process CurProcess in Processlist)
                    {
                        if (true == CurForm.m_ExistPrevMonitorProcess && CurProcess.ProcessName == CurForm.GetTargetMonitorProcessName())
                        {
                            g_TargetProcess = CurProcess;
                            CurForm.m_RunningMonitorProcess = true;
                            CurForm.SaveMonitorProcessInfo(g_TargetProcess);
                            break;
                        }
                        else
                        {
                            if (CurProcess.Id.ToString() == CurForm.m_TargetMonitorProcessID && CurProcess.ProcessName == CurForm.GetTargetMonitorProcessName())
                            {
                                // Running Process is true.
                                g_TargetProcess = CurProcess;
                                CurForm.m_RunningMonitorProcess = true;

                                CurForm.SaveMonitorProcessInfo(g_TargetProcess);
                                break;
                            }
                            else
                            {
                                CurForm.m_RunningMonitorProcess = false;
                            }
                        }
                    }

                    if( true == CurForm.m_RunningMonitorProcess )
                    {
                        string InfoText = String.Format("Process {0} is Running!!!", CurForm.m_TargetMonitorProcessName);
                        AprilUtility.WriteToFileWithTime(ref AprilUtility.g_LogFileName, InfoText);
                    }
                    else if (false == CurForm.m_RunningMonitorProcess)
                    {
                        string strText = String.Format("Process {0} is not Running. Running Process!!!!", CurForm.GetTargetMonitorProcessName());
                        CurForm.PrintAndWriteFileWithTime(strText);

                        // Force Running Process.
                        strText = String.Format("Start Running Process!! [{0}]", CurForm.GetTargetMonitorProcessName());
                        CurForm.PrintAndWriteFileWithTime(strText);

                        Process MonitorProcess = Process.Start(CurForm.GetTargetMonitorProcessFileName());
                        if (MonitorProcess.ProcessName == CurForm.GetTargetMonitorProcessName())
                        {
                            CurForm.SaveMonitorProcessInfo(MonitorProcess);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                Thread.Sleep(2000);
            }
        }

        public void StartMonitorProcessWorkerThread()
        {
            if (false == _RunningMonitorThread)
            {
                _RunningMonitorThread = true;
                Thread MonitorThread = new Thread(new ParameterizedThreadStart(MonitorProcessWorker));
                MonitorThread.Start((object)this);
            }
        }

        public void PrintAndWriteFileWithTime(string Text)
        {
            Text = String.Format("[{0}] {1}\n", AprilUtility.GetCorrentTimeString(), Text);

            AppendToMainTextBox(ref Text);
            AprilUtility.WriteToFileWithTime(ref AprilUtility.g_LogFileName, Text);
        }

        private void ShowProcessListDialog(object sender, EventArgs e)
        {
            ProcessListForm processform = new ProcessListForm();
            processform.Owner = this;
            processform.Show();
        }

        public void SetTrayNotifyBalloonTip()
        {
            notifyIcon1.Icon = SystemIcons.WinLogo;
            notifyIcon1.BalloonTipTitle = "Process Monitor";
            notifyIcon1.BalloonTipText = "None.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.None;
            notifyIcon1.Text = "Process Monitor!";
        }

        public void HideFromAndVisibleTray()
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();

            notifyIcon1.Visible = true;
        }

        public void HideTrayAndShowForm()
        {
            this.Visible = true;
            if (FormWindowState.Minimized == this.WindowState)
            {
                this.WindowState = FormWindowState.Normal;
                this.Activate();
                this.notifyIcon1.Visible = false;
            }

            notifyIcon1.Visible = false;
            this.Show();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HideTrayAndShowForm();
        }

        private void ToolStripMenuItem_ClickShow(object sender, EventArgs e)
        {
            HideTrayAndShowForm();
        }

        private void ToolStripMenuItem_ClickExit(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Dispose();
            Application.Exit();
        }
    }
}
