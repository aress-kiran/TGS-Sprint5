using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swyfftAuto
{
    public static class Log
    {
        public static void Write(string path1)
        {
            string logFilePath = @"C:\LogsIns\Log1" + "." + "txt";
            logFilePath = System.Configuration.ConfigurationSettings.AppSettings["LogFilePath"].ToString();
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                logFileInfo.Create();
            }

            string output = "";//Newtonsoft.Json.JsonConvert.SerializeObject(quote);
            File.AppendAllText(logFilePath, "\n" + DateTime.Now.ToString() + "---" + path1 + "\n");
            TextWriter tw = new StreamWriter(logFilePath, true);
            tw.WriteLine("new line");
            tw.Close();


        }
    }
}
