using System;
using Microsoft.VisualBasic;

namespace Squealer.My
{

    // <HideModuleName()> ' No, don't hide it from IntelliSense!
    static class Logging
    {

        #region  General Stuff 

        // The root folder for all the settings files.
        private static string _AppDataFolder = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\", My.MyProject.Application.Info.CompanyName, @"\", My.MyProject.Application.Info.Title);
        private static string _DefaultLogFile = _AppDataFolder + @"\" + "application.log";

        #endregion
        private static void SplitLogOver10MB(string filename)
        {

            if (System.IO.File.Exists(filename))
            {

                var info = new System.IO.FileInfo(filename);

                if (info.Length > 10000000L)
                {
                    string newname = info.Name.Replace(info.Extension, "") + string.Format("{0}-{1}-{2}", DateTime.Now.Year, DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00")) + info.Extension;
                    if (!System.IO.File.Exists(info.DirectoryName + @"\" + newname))
                    {
                        My.MyProject.Computer.FileSystem.RenameFile(filename, newname);
                    }
                }
            }

        }


        public static void WriteLog(string message)
        {
            WriteLog(_DefaultLogFile, message);
        }

        public static void WriteLog(string filename, string message)
        {

            SplitLogOver10MB(filename);

            if (!System.IO.Directory.Exists(_AppDataFolder))
            {
                My.MyProject.Computer.FileSystem.CreateDirectory(_AppDataFolder);
            }

            My.MyProject.Computer.FileSystem.WriteAllText(filename, string.Format("[{0}] {1}", DateTime.Now.ToString(), message + Constants.vbCrLf), true);

        }

    }

}