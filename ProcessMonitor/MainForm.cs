using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessMonitor
{
    public partial class MainForm : Form
    {
        public ProcessMonitorManager m_ProcessMonitorManager = new ProcessMonitorManager();
        private static string m_TempText = string.Empty;
        private static string m_ProgramName = "ProcessMonitor";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            InitializeComponent();
            notifyIcon1.Visible = false;

            MainForm thisFrom = (MainForm)this;
            m_ProcessMonitorManager.SetMainForm(ref thisFrom);

            m_TempText = String.Format("[{0}] {1}", AprilUtility.GetCorrentTimeString(), "Started ProcessMonitor!\n");
            AppendToMainTextBox(ref m_TempText);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            contextMenuStrip1.Hide();
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;

            if (string.Empty != Properties.Settings.Default.MonitorProcessName)
            {
                m_ProcessMonitorManager.SetTargetMonitorProcessInfo(Properties.Settings.Default.MonitorProcessName,
                                                                    "none",
                                                                    Properties.Settings.Default.MonitorProcessFullName);

                m_ProcessMonitorManager.ExistPrevMonitorProcess = true;
                m_ProcessMonitorManager.StartMonitorProcessWorkerThread();

                m_TempText = String.Format("Exist the Previously Monitored Process! [{0}]", m_ProcessMonitorManager.m_TargetProcessInfo.Name);
                PrintAndWriteFileWithTime(m_TempText);
            }
            else
            {
                m_ProcessMonitorManager.ExistPrevMonitorProcess = false;
            }
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
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

        public void AppendToMainTitleBarText(ref string Text)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    AvoidCrossThreadDelegateRichTextBox tempDelegate = new AvoidCrossThreadDelegateRichTextBox(AppendToMainTitleBarText);
                    this.Invoke(tempDelegate, Text);
                }
                else
                {
                    this.Text = Text;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
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
            processform.ShowDialog();
        }

        public void SetTrayNotifyBalloonTip()
        {
            notifyIcon1.Icon = SystemIcons.WinLogo;
            notifyIcon1.BalloonTipTitle = m_ProgramName;
            notifyIcon1.BalloonTipText = "None.";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.None;
            notifyIcon1.Text = m_ProgramName + ", First Version";
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
            System.Environment.Exit(1);
        }
    }
}
