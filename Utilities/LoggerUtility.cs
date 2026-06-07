using System;
using System.IO;

namespace SSProjectSolution.Utilities
{
    public static class LoggerUtility
    {
        public static void LogError(Exception ex, string customMessage = "")
        {
            try
            {
                // Create Logs folder in the application's base directory
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Create a daily log file, e.g., Log_20231024.txt
                string logFilePath = Path.Combine(logDirectory, $"Log_{DateTime.Now:yyyyMMdd}.txt");

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date        : " + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));
                    if (!string.IsNullOrEmpty(customMessage))
                    {
                        writer.WriteLine("Custom Msg  : " + customMessage);
                    }
                    writer.WriteLine("Error Msg   : " + ex.Message);
                    writer.WriteLine("Stack Trace : " + ex.StackTrace);
                    writer.WriteLine("-----------------------------------------------------------------------------");
                }
            }
            catch
            {
                // Fail silently if logging itself fails, to avoid crashing the application
            }
        }
    }
}
