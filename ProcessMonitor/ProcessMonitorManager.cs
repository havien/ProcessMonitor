﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ProcessMonitor
{
    public class ProcessInfo
    {
        private const string m_FileExtension = ".exe";
        private string m_Name = string.Empty;
        private string m_ID = string.Empty;
        private string m_FullPath = string.Empty;

        public ProcessInfo()
        {
            //m_Name = string.Empty;
            //m_ID = string.Empty;
            //m_FullPath = string.Empty;
        }

        #region property.
        public string ID
        {
            set { m_ID = value; }
            get { return m_ID; }
        }

        public string Name
        {
            set { m_Name = value; }
            get { return m_Name; }
        }

        public string FullPath
        {
            set { m_FullPath = value; }
            get { return m_FullPath; }
        }

        public void SetValues(string Name, string ID, string FullPath)
        {
            m_ID = ID;
            //m_Name = string.Format("{0}{1}", Name, m_FileExtension);
            m_Name = Name;
            m_FullPath = FullPath;
        }

        #endregion
    };

    public class ProcessMonitorManager
    {
        #region Variables
        MainForm m_MainForm;
        public ProcessInfo m_TargetProcessInfo;
        private bool m_ExistPrevMonitorProcess = false;
        private volatile bool m_RunningMonitorProcess = false;
        public static Process g_TargetProcess;
        #endregion

        #region Property.
        public bool ExistPrevMonitorProcess
        {
            get { return m_ExistPrevMonitorProcess; }
            set { m_ExistPrevMonitorProcess = value; }
        }

        public bool RunningMonitorProcess
        {
            set { m_RunningMonitorProcess = true; }
            get { return m_RunningMonitorProcess; }
        }

        #endregion
        public void SetMainForm(ref MainForm mainform)
        {
            m_MainForm = mainform;
        }

        #region constructor
        public ProcessMonitorManager()
        {
            m_TargetProcessInfo = new ProcessInfo();
        }
        #endregion

        /******************************************************
        * ProcessInfo.
        ******************************************************/
        public void SetTargetMonitorProcessInfo(ref string[] ProcessInfo)
        {
            m_TargetProcessInfo.SetValues(ProcessInfo[0], ProcessInfo[1], ProcessInfo[2]);
        }

        public void SetTargetMonitorProcessInfo(ref string Name, ref string ID, ref string FullPath)
        {
            m_TargetProcessInfo.SetValues(Name, ID, FullPath);
        }

        public void SetTargetMonitorProcessInfo( string Name, string ID, string FullPath)
        {
            m_TargetProcessInfo.SetValues(Name, ID, FullPath);
        }

        public void PrintTargetMonitorProcessInfo()
        {
            string TempText = String.Format("m_TargetMonitorProcessName : {0}", m_TargetProcessInfo.Name);
            m_MainForm.PrintAndWriteFileWithTime(TempText);

            TempText = String.Format("m_TargetMonitorProcessID : {0}", m_TargetProcessInfo.ID);
            m_MainForm.PrintAndWriteFileWithTime(TempText);

            TempText = String.Format("m_TargetMonitorProcessFullPath : {0}", m_TargetProcessInfo.FullPath);
            m_MainForm.PrintAndWriteFileWithTime(TempText);
        }

        public void SaveMonitorProcessInfo()
        {
            Properties.Settings.Default.MonitorProcessName = m_TargetProcessInfo.Name;
            Properties.Settings.Default.MonitorProcessFullName = m_TargetProcessInfo.FullPath;
            Properties.Settings.Default.Save();
        }

        public void SaveMonitorProcessInfo(Process MonitorProcess)
        {
            ProcessModule CurProcessModule = MonitorProcess.MainModule;
            SetTargetMonitorProcessInfo(MonitorProcess.ProcessName, MonitorProcess.Id.ToString(), CurProcessModule.FileName);
            m_RunningMonitorProcess = true;

            SaveMonitorProcessInfo();

            //Properties.Settings.Default.MonitorProcessName = MonitorProcess.ProcessName;
            //Properties.Settings.Default.MonitorProcessFullName = CurProcessModule.FileName;
            //Properties.Settings.Default.Save();
        }

        public volatile bool _RunningMonitorThread = false;
        private static void MonitorProcessWorker(object Args)
        {
            ProcessMonitorManager CurrentObject = (ProcessMonitorManager)Args;
            MainForm CurrentForm = CurrentObject.m_MainForm;
            CurrentObject.m_MainForm.PrintAndWriteFileWithTime("Entry MonitorProcessWorker Thread.");

            while (true == CurrentObject._RunningMonitorThread)
            {
                try
                {
                    CurrentObject.m_RunningMonitorProcess = false;

                    Process[] Processlist = Process.GetProcesses();
                    foreach (Process CurProcess in Processlist)
                    {
                        if (true == CurrentObject.m_ExistPrevMonitorProcess && CurProcess.ProcessName == CurrentObject.m_TargetProcessInfo.Name)
                        {
                            g_TargetProcess = CurProcess;
                            CurrentObject.m_RunningMonitorProcess = true;
                            CurrentObject.SaveMonitorProcessInfo(g_TargetProcess);
                            break;
                        }
                        else
                        {
                            if (CurProcess.Id.ToString() == CurrentObject.m_TargetProcessInfo.ID && CurProcess.ProcessName == CurrentObject.m_TargetProcessInfo.Name)
                            {
                                // Running Process is true.
                                g_TargetProcess = CurProcess;
                                CurrentObject.m_RunningMonitorProcess = true;

                                CurrentObject.SaveMonitorProcessInfo(g_TargetProcess);
                                break;
                            }
                            else
                            {
                                //CurrentObject.m_RunningMonitorProcess = false;
                            }
                        }
                    }

                    if( true == CurrentObject.m_RunningMonitorProcess )
                    {
                        string InfoText = String.Format("Process {0} is Running!!!", CurrentObject.m_TargetProcessInfo.Name);
                        AprilUtility.WriteToFileWithTime(ref AprilUtility.g_LogFileName, InfoText);
                    }
                    else if (false == CurrentObject.m_RunningMonitorProcess)
                    {
                        string strText = String.Format("Process {0} is not Running. now Try Running Process!!!!", CurrentObject.m_TargetProcessInfo.Name);
                        CurrentForm.PrintAndWriteFileWithTime(strText);

                        // Force Running Process.
                        strText = String.Format("Start Running Process!! [{0}], {1}]", CurrentObject.m_TargetProcessInfo.FullPath, CurrentObject.m_TargetProcessInfo.Name);
                        CurrentForm.PrintAndWriteFileWithTime(strText);

                        Process MonitorProcess = Process.Start(CurrentObject.m_TargetProcessInfo.FullPath);
                        if (MonitorProcess.ProcessName == CurrentObject.m_TargetProcessInfo.Name)
                        {
                            CurrentObject.SaveMonitorProcessInfo(MonitorProcess);
                            strText = String.Format("Success to Run Process [{0}]", CurrentObject.m_TargetProcessInfo.Name);
                            CurrentForm.PrintAndWriteFileWithTime(strText);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    CurrentForm.PrintAndWriteFileWithTime(ex.Message);
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

    }
}
