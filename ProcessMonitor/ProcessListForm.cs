using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace ProcessMonitor
{
    public partial class ProcessListForm : Form
    {
        private MainForm m_OwnerForm = null;
        private ProcessMonitorManager m_ProcessMonitorManager = null;
        private static string m_TempText = string.Empty;

        public ProcessListForm()
        {
            InitializeComponent();
        }

        private void ProcessListForm_Load(object sender, EventArgs e)
        {
            //if (false == AprilUtility.g_CreatedLogDir)
            //{
            //    string LogDir = AprilUtility.g_LogDirName;
            //    AprilUtility.CreateDirectory(ref LogDir);
            //    AprilUtility.g_CreatedLogDir = true;
            //}

            InitProcessListView(ref this.listView1);
            m_OwnerForm = (MainForm)this.Owner;
            m_ProcessMonitorManager = m_OwnerForm.m_ProcessMonitorManager;

            PrintAndWriteFileWithTime("Start Process List Dialog..!");
        }

        private void InitProcessListView(ref ListView TargetListView)
        {
            TargetListView.FullRowSelect = true;
            AddListViewColumns(ref TargetListView);

            TargetListView.Update();
            TargetListView.View = View.Details;
            AddProcessInfo(ref TargetListView);

            TargetListView.EndUpdate();
        }

        public void AppendToMainTextBox( string Text )
        {
            if( null != m_OwnerForm )
            {
                Text = String.Format("[{0}] {1}\n", AprilUtility.GetCorrentTimeString(), Text);
                m_OwnerForm.AppendToMainTextBox(ref Text);
            }
        }

        public void PrintAndWriteFileWithTime(string Text)
        {
            AppendToMainTextBox(Text);
            AprilUtility.WriteToFileWithTime(ref AprilUtility.g_LogFileName, Text);
        }

        private void AddListViewColumns(ref ListView TargetListView)
        {
            string[] ListViewColumns = { "ProcessName", "ProcessID", "FullName" };
            int[] ListViewColumnSizes = { 250, 80, 600 };

            for (int Counter = 0; Counter < ListViewColumns.Length; ++Counter)
            {
                TargetListView.Columns.Add(ListViewColumns[Counter], ListViewColumnSizes[Counter], HorizontalAlignment.Left);
            }
        }

        private void AddProcessInfo(ref ListView TargetListView)
        {
            Process[] Processlist = Process.GetProcesses();
            foreach (Process CurProcess in Processlist)
            {
                ListViewItem CurItem = new ListViewItem(CurProcess.ProcessName);

                CurItem.SubItems.Add(CurProcess.Id.ToString());

                try
                {
                    ProcessModule CurProcessModule = CurProcess.MainModule;
                    CurItem.SubItems.Add(CurProcess.MainModule.FileName.ToString());
                }
                catch (Win32Exception w)
                {
                    CurItem.SubItems.Add("");
                    PrintAndWriteFileWithTime(w.Message);
                }

                CurItem.ImageIndex = 0;
                TargetListView.Items.Add(CurItem);
            }
        }

        private void RemoveAllItems(ref ListView TargetListView)
        {
            for (int Counter = 0; Counter < TargetListView.Items.Count; ++Counter)
            {
                TargetListView.Items[Counter].Remove();
            }
        }

        public void SetTargetMonitorProcessInfo(ListViewItem.ListViewSubItemCollection subItem)
        {
            string[] names = { subItem[0].Text, subItem[1].Text, subItem[2].Text };
            m_ProcessMonitorManager.SetTargetMonitorProcessInfo(ref names[0], ref names[1], ref names[2]);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView.SelectedListViewItemCollection SelectedCollection = this.listView1.SelectedItems;

            foreach (ListViewItem Selected in SelectedCollection)
            {
               
            }   
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if( SortOrder.Ascending == listView1.Sorting )
            {
                listView1.Sorting = SortOrder.Descending;
            }
            else
            {
                listView1.Sorting = SortOrder.Ascending;
            }
            
            listView1.Sort();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo HitTestInfo = listView1.HitTest(e.X, e.Y);

            if( "" != HitTestInfo.Item.Text )
            {
                if( true == m_ProcessMonitorManager.RunningMonitorProcess )
                {
                    m_TempText = String.Format("Already Monitor Process [{0}], Change to this Process [{1}] ?",
                                                    m_ProcessMonitorManager.m_TargetProcessInfo.Name, HitTestInfo.Item.Text);
                    DialogResult DlgResult = MessageBox.Show(m_TempText, "Question ?", MessageBoxButtons.YesNo);
                    if (DialogResult.Yes == DlgResult)
                    {
                        PrintAndWriteFileWithTime(m_TempText);

                        m_TempText = String.Format("Start Process Monitor [{0}]!!!!!!", HitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(m_TempText);

                        // save process info to Settings.
                        m_ProcessMonitorManager.SaveMonitorProcessInfo();

                        SetTargetMonitorProcessInfo(HitTestInfo.Item.SubItems);
                        m_ProcessMonitorManager.PrintTargetMonitorProcessInfo();
                        

                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");                        
                        if( true == m_ProcessMonitorManager._RunningMonitorThread )
                        {
                            PrintAndWriteFileWithTime("Already Running MonitorProcessWorker Thread!");
                        }
                        else
                        {
                            m_ProcessMonitorManager.StartMonitorProcessWorkerThread();
                            PrintAndWriteFileWithTime("Success to Start MonitorProcessWorker");
                        }

                        m_TempText = String.Format("{0}|{1}", HitTestInfo.Item.SubItems[0].Text,
                                                                HitTestInfo.Item.SubItems[2].Text);


                        this.WindowState = FormWindowState.Minimized;
                        this.Visible = false;
                    }
                    else if (DialogResult.No == DlgResult)
                    {

                    }
                }
                else
                {
                    m_TempText = String.Format("Start Monitor [{0}] ?", HitTestInfo.Item.Text);
                    DialogResult DlgResult = MessageBox.Show(m_TempText, "Question ?", MessageBoxButtons.YesNo);
                    if (DialogResult.Yes == DlgResult)
                    {
                        m_TempText = String.Format("Start Process Monitor [{0}]", HitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(m_TempText);

                        string [] CurMonitorProcessInfo = {HitTestInfo.Item.SubItems[0].Text, 
                                                           HitTestInfo.Item.SubItems[1].Text,
                                                          HitTestInfo.Item.SubItems[2].Text};

                        m_ProcessMonitorManager.SetTargetMonitorProcessInfo(ref CurMonitorProcessInfo);

                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");
                        m_ProcessMonitorManager.StartMonitorProcessWorkerThread();
                        PrintAndWriteFileWithTime("Success to Start MonitorProcessWorker");
                    }
                    else if (DialogResult.No == DlgResult)
                    {

                    }
                }
            }
        }
    }
}