using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ProcessMonitor
{
    public class ProcessInfo
    {
        private string _name = string.Empty;
        private string _identifier = string.Empty;
        private string _fileFullPath = string.Empty;

        public ProcessInfo()
        {
        }

        #region property.
        public string ID
        {
            set { _identifier = value; }
            get { return _identifier; }
        }

        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }

        public string FullPath
        {
            set { _fileFullPath = value; }
            get { return _fileFullPath; }
        }

        public void SetValues( string identifier, string name, string fileFullPath )
        {
            _identifier = identifier;
            _name = name;
            _fileFullPath = fileFullPath;
        }

        #endregion
    };

    public class ProcessMonitorManager
    {
        #region Variables
        MainForm _mainForm;

        public ProcessInfo TargetProcessInfo;
        public static Process TargetProcess;

        private bool _existPrevMonitorProcess = false;
        private volatile bool _runningMonitorProcess = false;
        #endregion

        #region Property
        public bool ExistPrevMonitorProcess
        {
            get { return _existPrevMonitorProcess; }
            set { _existPrevMonitorProcess = value; }
        }

        public bool RunningMonitorProcess
        {
            set { _runningMonitorProcess = true; }
            get { return _runningMonitorProcess; }
        }

        #endregion
        public void SetMainForm(ref MainForm mainform)
        {
            _mainForm = mainform;
        }

        #region constructor
        public ProcessMonitorManager()
        {
            TargetProcessInfo = new ProcessInfo();
        }
        #endregion

        /******************************************************
        * ProcessInfo.
        ******************************************************/
        public void SetTargetMonitorProcessInfo( ref string[] ProcessInfo )
        {
            SetTargetMonitorProcessInfo( ProcessInfo[0], ProcessInfo[1], ProcessInfo[2] );
        }

        public void SetTargetMonitorProcessInfo( string identifier, string name, string fileFullPath )
        {
            TargetProcessInfo.SetValues( identifier, name, fileFullPath );
        }

        public void PrintTargetMonitorProcessInfo()
        {
            string tempText = String.Format("TargetMonitorProcessName : {0}", TargetProcessInfo.Name );
            _mainForm.PrintAndWriteFileWithTime(tempText);

            tempText = String.Format("TargetMonitorProcessID : {0}", TargetProcessInfo.ID );
            _mainForm.PrintAndWriteFileWithTime(tempText);

            tempText = String.Format("TargetMonitorProcessFullPath : {0}", TargetProcessInfo.FullPath );
            _mainForm.PrintAndWriteFileWithTime(tempText);
        }

        public void SaveMonitorProcessInfo()
        {
            Properties.Settings.Default.MonitorProcessName = TargetProcessInfo.Name;
            Properties.Settings.Default.MonitorProcessFullName = TargetProcessInfo.FullPath;
            Properties.Settings.Default.Save();
        }

        public void SaveMonitorProcessInfo( Process MonitorProcess )
        {
            SetTargetMonitorProcessInfo( MonitorProcess.Id.ToString(),
                                         MonitorProcess.ProcessName, 
                                         MonitorProcess.MainModule.FileName );

            _runningMonitorProcess = true;
            SaveMonitorProcessInfo();
        }

        public volatile bool _runningMonitorThread = false;
        private static void MonitorProcessWorker( object Args )
        {
            ProcessMonitorManager appManager = (ProcessMonitorManager)Args;
            MainForm CurrentForm = appManager._mainForm;
            appManager._mainForm.PrintAndWriteFileWithTime( "Entry MonitorProcessWorker Thread." );

            while( true == appManager._runningMonitorThread )
            {
                try
                {
                    appManager._runningMonitorProcess = false;

                    Process[] Processlist = Process.GetProcesses();
                    foreach( Process CurProcess in Processlist )
                    {
                        // FullPath로 확인한다. 이름이 같아도 FullPath가 다르면 별개의 프로세스로 판단한다.
                        if( true == appManager._existPrevMonitorProcess && 
                                    CurProcess.ProcessName == appManager.TargetProcessInfo.Name)
                        {
                            string curProcessFullPath = CurProcess.Modules[0].FileName;
                            if (0 == string.Compare(curProcessFullPath, appManager.TargetProcessInfo.FullPath))
                            {
                                TargetProcess = CurProcess;
                                appManager._runningMonitorProcess = true;
                                appManager.SaveMonitorProcessInfo( TargetProcess );
                                
                                break;
                            }
                        }
                        else
                        {
                            if( CurProcess.ProcessName == appManager.TargetProcessInfo.Name )
                            {
                                string curProcessFullPath = CurProcess.Modules[0].FileName;
                                if( 0 == string.Compare(curProcessFullPath, appManager.TargetProcessInfo.FullPath) )
                                {
                                    // Running Process is true.
                                    TargetProcess = CurProcess;
                                    appManager._runningMonitorProcess = true;

                                    appManager.SaveMonitorProcessInfo( TargetProcess );
                                    break;
                                }
                            }
                        }
                    }

                    if( true == appManager._runningMonitorProcess )
                    {
                        string InfoText = String.Format( "Process {0} is Running!!!", appManager.TargetProcessInfo.Name );
                    }
                    else if (false == appManager._runningMonitorProcess)
                    {
                        string strText = String.Format( "Process {0} is not Running. now Try Running Process!!!!", 
                                                        appManager.TargetProcessInfo.Name);

                        CurrentForm.PrintAndWriteFileWithTime(strText);

                        // Force Running Process.
                        strText = String.Format( "Start Running Process!! [{0}], {1}]", 
                                                appManager.TargetProcessInfo.FullPath, appManager.TargetProcessInfo.Name);

                        CurrentForm.PrintAndWriteFileWithTime(strText);

                        string titleBarText = string.Format("{0}", appManager.TargetProcessInfo.FullPath);
                        CurrentForm.AppendToMainTitleBarText(ref titleBarText);
                        
                        string WorkingDirectory = System.IO.Directory.GetCurrentDirectory();

                        string PathOnly = AprilUtility.RemoveExeFileName( appManager.TargetProcessInfo.FullPath,
                                                                          appManager.TargetProcessInfo.Name );

                        if( "" != PathOnly )
                        {
                            System.IO.Directory.SetCurrentDirectory( PathOnly );
                            Process MonitorProcess = Process.Start( appManager.TargetProcessInfo.FullPath );

                            if ( true == Properties.Settings.Default.IsSendMail )
                            {
                                if ( true == System.IO.Directory.Exists( Properties.Settings.Default.SendMailDir ) )
                                {
                                    CurrentForm.PrintAndWriteFileWithTime( "Send mail to Developer's Using python" );

                                    // 서버가 죽었다고 메일을 보낸다. sendmail.py를 활용한다.
                                    string mailSubject = string.Format( "\"Crash Server Process {0}({1})\"",
                                                                        appManager.TargetProcessInfo.FullPath,
                                                                        appManager.TargetProcessInfo.Name );


                                    string sendMailProcessName = "C:\\Python27\\python.exe";

                                    ProcessStartInfo pythonProcessInfo = new ProcessStartInfo();
                                    pythonProcessInfo.CreateNoWindow = true;
                                    pythonProcessInfo.UseShellExecute = false;
                                    pythonProcessInfo.FileName = sendMailProcessName;
                                    pythonProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                    pythonProcessInfo.Arguments = "SendMail.py " + mailSubject;
                                    using ( Process SendMailProcess = Process.Start( pythonProcessInfo ) )
                                    {
                                        SendMailProcess.WaitForExit();
                                    }

                                    CurrentForm.PrintAndWriteFileWithTime( "Complete to Send " );
                                }
                            }

                            System.IO.Directory.SetCurrentDirectory( WorkingDirectory );
                            if ( MonitorProcess.ProcessName == appManager.TargetProcessInfo.Name )
                            {
                                appManager.SaveMonitorProcessInfo( MonitorProcess );
                                strText = String.Format( "Success to Run Process [{0}]", appManager.TargetProcessInfo.Name );
                                CurrentForm.PrintAndWriteFileWithTime( strText );
                            }
                        }
                        else
                        {
                            strText = String.Format( "Not Exist FullPath Directory [{0}]", appManager.TargetProcessInfo.Name );
                            CurrentForm.PrintAndWriteFileWithTime( strText );
                        }
                    }
                }
                catch( System.Exception ex )
                {
                    CurrentForm.PrintAndWriteFileWithTime(ex.Message);
                }

                Thread.Sleep( 2000 );
            }
        }

        public void StartMonitorProcessWorkerThread()
        {
            if( false == _runningMonitorThread )
            {
                _runningMonitorThread = true;
                var MonitorThread = new Thread( new ParameterizedThreadStart( MonitorProcessWorker ) );
                MonitorThread.Start( (object)this );
            }
        }
    }
}
