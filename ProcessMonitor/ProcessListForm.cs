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
        private Form1 m_OwnerForm = null;

        public ProcessListForm()
        {
            InitializeComponent();
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
            m_OwnerForm.SetTargetMonitorProcessInfo(ref names[0], ref names[1], ref names[2]);
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

        private void ProcessListForm_Load(object sender, EventArgs e)
        {
            InitProcessListView(ref this.listView1);
            m_OwnerForm = (Form1)this.Owner;

            PrintAndWriteFileWithTime("Start ProcessMonitor..");
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

        private void StartMonitorProcessWorkerThread()
        {
            m_OwnerForm.StartMonitorProcessWorkerThread();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo HitTestInfo = listView1.HitTest(e.X, e.Y);

            if( "" != HitTestInfo.Item.Text )
            {
                if( true == m_OwnerForm.m_RunningMonitorProcess )
                {
                    string Question = String.Format("Already Monitor Process [{0}], Change to this Process [{1}] ?",
                                                    m_OwnerForm.GetTargetMonitorProcessName(), HitTestInfo.Item.Text);
                    DialogResult DlgResult = MessageBox.Show(Question, "Question ?", MessageBoxButtons.YesNo);
                    if (DialogResult.Yes == DlgResult)
                    {
                        PrintAndWriteFileWithTime(Question);

                        string Info = String.Format("Start Process Monitor [{0}]!!!!!!", HitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(Info);

                        SetTargetMonitorProcessInfo(HitTestInfo.Item.SubItems);
                        m_OwnerForm.PrintTargetMonitorProcessInfo();

                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");                        
                        if( true == m_OwnerForm._RunningMonitorThread )
                        {
                            PrintAndWriteFileWithTime("Already Running MonitorProcessWorker Thread!");
                        }
                        else
                        {
                            StartMonitorProcessWorkerThread();
                            PrintAndWriteFileWithTime("Success to Start MonitorProcessWorker");
                        }

                        string strText = String.Format("{0}|{1}", HitTestInfo.Item.SubItems[0].Text,
                                                                HitTestInfo.Item.SubItems[2].Text);

                        AprilUtility.WriteToFile(ref AprilUtility.g_ProcessLogFileName, strText);

                        this.WindowState = FormWindowState.Minimized;
                        this.Visible = false;
                    }
                    else if (DialogResult.No == DlgResult)
                    {

                    }
                }
                else
                {
                    string Question = String.Format("Start Monitor [{0}] ?", HitTestInfo.Item.Text);
                    DialogResult DlgResult = MessageBox.Show(Question, "Question ?", MessageBoxButtons.YesNo);
                    if (DialogResult.Yes == DlgResult)
                    {
                        string Info = String.Format("Start Process Monitor [{0}]", HitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(Info);

                        string [] CurMonitorProcessInfo = {HitTestInfo.Item.SubItems[0].Text, 
                                                           HitTestInfo.Item.SubItems[1].Text,
                                                          HitTestInfo.Item.SubItems[2].Text};

                        m_OwnerForm.SetTargetMonitorProcessInfo(ref CurMonitorProcessInfo);

                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");

                        StartMonitorProcessWorkerThread();

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