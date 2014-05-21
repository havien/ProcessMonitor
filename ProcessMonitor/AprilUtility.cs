using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ProcessMonitor
{
    public class AprilUtility
    {
        public static bool g_CreatedLogDir = false;
        public static string g_LogDirName = "..\\..\\..\\logs";
        public static string g_LogFileName = "\\MonitorProcess_log.txt";
        public static FileStream g_LogFileStream;
        //public static string g_ProcessLogFileName = "\\MonitorProcess.txt";

        /******************************************************
         * Time
        ******************************************************/
        public static string GetCorrentTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        }

        /******************************************************
         * File
        ******************************************************/
        public static bool ExistFile(ref string FileName)
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
            try
            {
                DirectoryInfo NewDir = System.IO.Directory.CreateDirectory(DirName);
                g_LogFileName = g_LogDirName + g_LogFileName;
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        
        public static void CreateNewFile(ref string FileName)
        {
            //_LogFileName = _LogDir + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".dblog";
            using (FileStream stream = new FileStream(FileName, FileMode.CreateNew))
            {
                stream.Close();
            }
        }

        public static bool ReadFileToEnd(ref string FileName, ref string ReadText)
        {
            try
            {
                using (StreamReader TargetFile = new StreamReader(FileName))
                {
                    ReadText = TargetFile.ReadToEnd();
                    TargetFile.Close();
                    return true;
                }
            }
            catch(IOException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static void WriteToFileWithTime(ref string FileName, string Comment)
        {
            Comment = String.Format("[{0}] {1}", GetCorrentTimeString(), Comment);
            WriteToFileEnd(ref FileName, Comment);
        }

        public static void WriteToFile(ref string FileName, string Comment)
        {
            try
            {
                using (FileStream WriteStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    StreamWriter Writer = new StreamWriter(WriteStream);
                    //WriteStream.Seek(0, SeekOrigin.End);
                    Writer.WriteLine(Comment);
                    Writer.Close();
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public static void WriteToFileEnd(ref string FileName, string Comment)
        {
            try
            {
                using (FileStream WriteStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    StreamWriter Writer = new StreamWriter(WriteStream);
                    WriteStream.Seek(0, SeekOrigin.End);
                    Writer.WriteLine(Comment);
                    Writer.Close();
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            
        }
    }
}
