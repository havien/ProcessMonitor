using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ProcessMonitor
{
    public class AprilUtility
    {
        private const string _fileExtension = ".exe";
        public static string LogDirName = ".\\logs";
        public static string LogFileName = "\\MonitorProcess_log.txt";
        public static string ProcessLogFileName = "\\MonitorProcess.txt";

        /******************************************************
         * Time
        ******************************************************/
        public static string GetCorrentTimeString()
        {
            return DateTime.Now.ToString( "yyyy-MM-dd HH-mm-ss" );
        }

        /******************************************************
         * File
        ******************************************************/
        public static bool IsExists( ref string FileName )
        {
            try
            {
                return File.Exists(FileName);
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
            
        }

        public static void CreateDirectory(ref string DirName)
        {
            System.IO.Directory.CreateDirectory(DirName);

            ProcessLogFileName = LogDirName + ProcessLogFileName;
            LogFileName = LogDirName + LogFileName;
        }
        
        public static void CreateNewFile(ref string FileName)
        {
            //_LogFileName = _LogDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".dblog";
            using( FileStream stream = new FileStream(FileName, FileMode.CreateNew) )
            {
                stream.Close();
            }
        }

        public static bool ReadFileToEnd(ref string FileName, ref string ReadText)
        {
            try
            {
                using( StreamReader targetFile = new StreamReader( FileName ) )
                {
                    ReadText = targetFile.ReadToEnd();
                    targetFile.Close();
                    return true;
                }
            }
            catch( IOException ex )
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static void WriteToFileWithTime( ref string fileName, string comment )
        {
            comment = String.Format("[{0}] {1}", GetCorrentTimeString(), comment);
            WriteToFileEnd(ref fileName, comment);
        }

        public static void WriteToFile( ref string fileName, string comment )
        {
            using( FileStream tempWriteStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write ) )
            {
                StreamWriter Writer = new StreamWriter( tempWriteStream );
                //WriteStream.Seek(0, SeekOrigin.End);
                Writer.WriteLine(comment);
                Writer.Close();
            }
        }

        public static void WriteToFileEnd( ref string fileName, string comment )
        {
            string filePath = Environment.CurrentDirectory + fileName;
            using( FileStream tempWriteStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write) )
            {
                StreamWriter Writer = new StreamWriter(tempWriteStream);
                tempWriteStream.Seek(0, SeekOrigin.End);
                Writer.WriteLine(comment );
                Writer.Close();
            }
        }

        public static string RemoveExeFileName( string fullPath, string fileName )
        {
            int tempPos = 0;
            int lastPos = 0;

            while( -1 != tempPos )
            {
                if( 0 == tempPos )
                {
                    tempPos = fullPath.IndexOf( fileName, tempPos );
                }
                else if( 0 < tempPos )
                {
                    lastPos = tempPos;
                    tempPos = fullPath.IndexOf( fileName, tempPos + 1 );
                }
            }

            if( 0 < lastPos )
            {
                var removePath = fullPath.Remove( lastPos );
                return removePath;
            }

            return "";
        }
    }
}