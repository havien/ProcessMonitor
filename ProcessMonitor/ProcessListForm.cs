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
        private ProcessMonitorManager _AppManager = null;
        private static string tempText = string.Empty;

        public ProcessListForm()
        {
            InitializeComponent();
        }

        private void ProcessListForm_Load(object sender, EventArgs e)
        {
            InitProcessListView(ref this.listView1);
            m_OwnerForm = (MainForm)this.Owner;
            _AppManager = m_OwnerForm.AppManager;

            PrintAndWriteFileWithTime("Start Process List Dialog..!");
        }

        private void InitProcessListView(ref ListView TargetListView)
        {
            TargetListView.FullRowSelect = true;
            AddListViewColumns( ref TargetListView );

            TargetListView.Update();
            TargetListView.View = View.Details;
            AddProcessInfo( ref TargetListView );

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
            AprilUtility.WriteToFileWithTime(ref AprilUtility.LogFileName, Text);
        }

        private void AddListViewColumns( ref ListView TargetListView )
        {
            string[] ListViewColumns = { "ProcessName", "WindowTitle", "ProcessID", "FullName" };
            int[] ListViewColumnSizes = { 250, 300, 80, 600 };

            for (int Counter = 0; Counter < ListViewColumns.Length; ++Counter)
            {
                TargetListView.Columns.Add( ListViewColumns[Counter], ListViewColumnSizes[Counter], HorizontalAlignment.Left );
            }
        }

        private void AddProcessInfo(ref ListView TargetListView)
        {
            Process[] Processlist = Process.GetProcesses();
            foreach( Process CurProcess in Processlist )
            {
                var CurItem = new ListViewItem( CurProcess.ProcessName );

                CurItem.SubItems.Add( CurProcess.MainWindowTitle );
                CurItem.SubItems.Add( CurProcess.Id.ToString() );

                try
                {
                    CurItem.SubItems.Add(CurProcess.MainModule.FileName.ToString());
                }
                catch( Win32Exception exception )
                {
                    CurItem.SubItems.Add("");
                    PrintAndWriteFileWithTime( exception.Message );
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

        public void SetTargetMonitorProcessInfo( ListViewItem.ListViewSubItemCollection subItem )
        {
            string[] names = { subItem[2].Text, subItem[0].Text, subItem[3].Text };
            _AppManager.SetTargetMonitorProcessInfo( names[0], names[1], names[2] );
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView.SelectedListViewItemCollection SelectedCollection = this.listView1.SelectedItems;

            foreach (ListViewItem Selected in SelectedCollection)
            {
               
            }   
        }

        private void MinimizeWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
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
            ListViewHitTestInfo hitTestInfo = listView1.HitTest(e.X, e.Y);

            if( "" != hitTestInfo.Item.Text )
            {
                if( true == _AppManager.RunningMonitorProcess )
                {
                    tempText = String.Format("Already Monitor Process [{0}], Change to this Process [{1}] ?",
                                            _AppManager.TargetProcessInfo.Name, hitTestInfo.Item.Text );

                    DialogResult DlgResult = MessageBox.Show(tempText, "Question ?", MessageBoxButtons.YesNo);

                    if( DialogResult.Yes == DlgResult )
                    {
                        PrintAndWriteFileWithTime(tempText);

                        tempText = String.Format("Start Process Monitor [{0}]!!!!!!", hitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(tempText);

                        // save process info to Settings.
                        _AppManager.SaveMonitorProcessInfo();

                        SetTargetMonitorProcessInfo( hitTestInfo.Item.SubItems );
                        _AppManager.PrintTargetMonitorProcessInfo();
                        
                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");                        
                        if( true == _AppManager._runningMonitorThread )
                        {
                            PrintAndWriteFileWithTime("Already Running MonitorProcessWorker Thread!");
                        }
                        else
                        {
                            _AppManager.StartMonitorProcessWorkerThread();
                            PrintAndWriteFileWithTime("Success to Start MonitorProcessWorker");
                        }

                        tempText = String.Format("{0}|{1}", hitTestInfo.Item.SubItems[0].Text,
                                                hitTestInfo.Item.SubItems[3].Text);

                        MinimizeWindow();
                    }
                    else if (DialogResult.No == DlgResult)
                    {

                    }
                }
                else
                {
                    tempText = String.Format("Start Monitor [{0}] ?", hitTestInfo.Item.Text);
                    DialogResult DlgResult = MessageBox.Show(tempText, "Question ?", MessageBoxButtons.YesNo);
                    if (DialogResult.Yes == DlgResult)
                    {
                        tempText = String.Format("Start Process Monitor [{0}]", hitTestInfo.Item.Text);
                        PrintAndWriteFileWithTime(tempText);

                        string [] curMonitorProcessInfo = {hitTestInfo.Item.SubItems[2].Text, 
                                                           hitTestInfo.Item.SubItems[0].Text,
                                                          hitTestInfo.Item.SubItems[3].Text};

                        _AppManager.SetTargetMonitorProcessInfo( ref curMonitorProcessInfo );

                        PrintAndWriteFileWithTime("Try to Start MonitorProcessWorker");
                        _AppManager.StartMonitorProcessWorkerThread();
                        PrintAndWriteFileWithTime("Success to Start MonitorProcessWorker");

                        MinimizeWindow();
                    }
                    else if (DialogResult.No == DlgResult)
                    {

                    }
                }
            }
        }
    }
}