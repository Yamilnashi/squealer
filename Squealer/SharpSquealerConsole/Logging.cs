using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Squealer.My
{
    static class Logging
    {
        #region General Stuff

        private static string _AppDataFolder = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"\", GetApplicationCompanyName(), @"\", GetApplicationTitle());
        private static string _DefaultLogFile = _AppDataFolder + @"\" + "application.log";


        private static string GetApplicationCompanyName()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            return versionInfo.CompanyName;
        }

        private static string GetApplicationTitle()
        {
            AssemblyTitleAttribute titleAttribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
            return titleAttribute.Title;
        }

        #endregion

        private static void SplitLogOver10MB(string fileName)
        {
            if (File.Exists(fileName))
            {
                var info = new FileInfo(fileName);
                if (info.Length > 10000000L)
                {
                    string newName = info.Name.Replace(info.Extension, "") 
                        + $"{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}" 
                        + info.Extension;
                    
                    if (!File.Exists(info.DirectoryName + @"\" + newName))
                    {
                        File.Move(newName, fileName);
                    }
                }
            }
        }

        public static void WriteLog(string message)
        {
            WriteLog(_DefaultLogFile, message);
        }

        public static void WriteLog(string fileName, string message)
        {
            SplitLogOver10MB(fileName);
            if (!Directory.Exists(_AppDataFolder))
            {
                Directory.CreateDirectory(_AppDataFolder);
            }
            File.WriteAllText(fileName, $"[{DateTime.Now}] {message + Environment.NewLine}", System.Text.Encoding.UTF8);
        }

    }

}
