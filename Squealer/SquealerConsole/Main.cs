using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Squealer
{

    public class GitFlags
    {

        private bool _ShowUncommitted = false;
        public bool ShowUncommitted
        {
            get
            {
                return _ShowUncommitted;
            }
            set
            {
                _ShowUncommitted = value;
            }
        }

        private bool _ShowDeleted = false;
        public bool ShowDeleted
        {
            get
            {
                return _ShowDeleted & _ShowUncommitted;
            }
            set
            {
                _ShowDeleted = value;
            }
        }

        private bool _ShowHistory = false;
        public bool ShowHistory
        {
            get
            {
                return _ShowHistory;
            }
            set
            {
                _ShowHistory = value;
            }
        }

        public bool GitEnabled
        {
            get
            {
                return _ShowUncommitted;
            }
        }

    }

    static class MainType
    {

        #region  All The Definitions 

        private static Squealer.CommandCatalog.CommandDefinitionList MyCommands = new Squealer.CommandCatalog.CommandDefinitionList();
        private static Squealer.Settings MySettings = new Squealer.Settings(true);
        private static Squealer.FileHashCollection MyFileHashes = new Squealer.FileHashCollection();

        private class BatchParametersClass
        {

            public enum eOutputMode
            {
                normal,
                flags,
                encrypt,
                test,
                alter,
                convert,
                permanent,
                @string
            }

            private eOutputMode _OutputMode = eOutputMode.normal;
            public eOutputMode OutputMode
            {
                get
                {
                    return _OutputMode;
                }
                set
                {
                    _OutputMode = value;
                }
            }

        }

        #endregion

        #region  Enums 

        private enum eFileAction
        {
            directory,
            edit,
            generate,
            fix,
            compare,
            delete,
            checkout
        }

        private enum eCommandType
        {
            about,
            browse,
            hash,
            checkout,
            clear,
            compare,
            copypath,
            config,
            connection,
            delete,
            directory,
            download,
            edit,
            eztool,
            exit,
            fix,
            generate,
            help,
            helpall,
            list,
            make,
            nerfherder,
            @new,
            open,
            release,
            reverse,
            setting,
            test,
            use
        }

        #endregion

        #region  Folders 

        private class FolderClass
        {

            private string _Name;
            public string name
            {
                get
                {
                    return _Name;
                }
                set
                {
                    _Name = value;
                }
            }

            private DateTime _LastCompressed;
            public DateTime LastCompressed
            {
                get
                {
                    return _LastCompressed;
                }
                set
                {
                    _LastCompressed = value;
                }
            }

            public FolderClass(string n)
            {
                _Name = n;
                _LastCompressed = DateTime.Now.AddYears(-1);
            }

        }

        private static List<FolderClass> Folders = new List<FolderClass>();

        // Set a new working folder and remember it for later.
        private static void ChangeFolder(string newpath, ref string ProjectFolder)
        {

            System.IO.Directory.GetCurrentDirectory() = newpath; // this will throw an error if the path is not valid
            ProjectFolder = newpath;
            RememberFolder(newpath);
            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, newpath);
            Squealer.Textify.SayNewLine();

            // Temporary code to rename existing connection strings 4/3/2019
            string oldcs = newpath + @"\.Squealer_cs";
            try
            {
                if (System.IO.File.Exists(oldcs))
                {
                    Squealer.My.MyProject.Computer.FileSystem.RenameFile(oldcs, Squealer.Constants.ConnectionStringFilename);
                }
            }
            catch (Exception ex)
            {
                // suppress errors
            }

            if (MySettings.AutoCompressGit)
            {

                FolderClass f;
                f = Folders.Find(x => (x.name ?? "") == (newpath ?? ""));
                if (f is null)
                {
                    f = new FolderClass(newpath);
                    Folders.Add(f);
                }

                if (f.LastCompressed.Date < DateTime.Now.Date)
                {
                    string command = "git gc --auto";
                    Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Star, command + " ... ");
                    Squealer.GitShell.DisplayResults(f.name, command);
                    f.LastCompressed = DateTime.Now;
                    Console.WriteLine("done.");
                    Console.WriteLine();
                }

            }

        }

        private static List<string> FolderCollection()
        {

            var folders = new List<string>();
            string unsplit = MySettings.RecentProjectFolders;
            if (!string.IsNullOrWhiteSpace(unsplit))
            {
                folders.AddRange(unsplit.Split(new char[] { '|' }));
            }
            while (folders.Count > MySettings.ProjectFoldersLimit)
                folders.RemoveAt(folders.Count - 1);
            return folders;

        }

        private static int InvalidFolderIndex()
        {
            int InvalidFolderIndexRet = default;

            string f = FolderCollection().Find(x => !System.IO.Directory.Exists(x));
            if (string.IsNullOrEmpty(f)) // couldn't find any bad directories
            {
                f = FolderCollection().Find(x => Squealer.My.MyProject.Computer.FileSystem.GetFiles(x, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*" + Squealer.Constants.SquealerFileExtension).Count == 0);
                if (string.IsNullOrEmpty(f)) // couldn't find any unused directories
                {
                    InvalidFolderIndexRet = -1;
                }
                else
                {
                    InvalidFolderIndexRet = FolderCollection().IndexOf(f);
                }
            }
            else
            {
                InvalidFolderIndexRet = FolderCollection().IndexOf(f);
            }

            return InvalidFolderIndexRet;

        }

        private static void AutoRemoveFolders()
        {

            if (InvalidFolderIndex() == -1)
            {
                Squealer.Textify.WriteLine("All folders contain *" + Squealer.Constants.SquealerFileExtension);
            }
            else
            {
                while (InvalidFolderIndex() > -1)
                {
                    int i = InvalidFolderIndex();
                    Squealer.Textify.WriteLine("Remove: " + FolderCollection()[i], ConsoleColor.Red);
                    ForgetFolder(i);
                }
            }

        }

        private static string FolderString(List<string> folders)
        {
            string FolderStringRet = default;

            string s = string.Empty;
            foreach (string item in folders)
                s += item + "|";
            if (s.Length > 0)
            {
                s = s.Remove(s.Length - 1);
            }

            FolderStringRet = s;
            return FolderStringRet;

        }


        // List all remembered folders.
        public static void ListFolders(string WorkingFolder)
        {

            var folders = FolderCollection();
            int longestnickname = 0;

            if (folders.Count == 0)
            {
                throw new Exception("No saved project folders.");
            }
            else
            {

                var farray = new string[folders.Count, 4];

                for (int i = 0, loopTo = folders.Count - 1; i <= loopTo; i++)
                {
                    farray[i, 0] = i.ToString();
                    farray[i, 1] = Squealer.Misc.ProjectName(folders[i]);
                    farray[i, 2] = folders[i];
                    if (!System.IO.Directory.Exists(farray[i, 2]))
                    {
                        farray[i, 1] = "**********";
                        farray[i, 2] = "<<not found>>" + farray[i, 2];
                    }
                    if (farray[i, 1].Length > longestnickname)
                    {
                        longestnickname = farray[i, 1].Length;
                    }
                }

                int highestnumber = farray.GetLength(0) - 1;

                for (int i = 0, loopTo1 = highestnumber; i <= loopTo1; i++)
                {
                    Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Star, string.Format("{0} | ", farray[i, 0].PadLeft(highestnumber.ToString().Length)));
                    Squealer.Textify.Write(farray[i, 1].PadRight(longestnickname), ConsoleColor.Cyan);
                    Squealer.Textify.WriteLine(string.Format(" | {0}", farray[i, 2]));
                }

            }

            Squealer.Textify.SayNewLine();

        }

        // Set a remembered folder as the current working folder.
        public static void LoadFolder(string NewFolder, ref string WorkingFolder)
        {

            try
            {
                int n;
                if (int.TryParse(NewFolder, out n) && n < 100)
                {
                    // Load by project number
                    ChangeFolder(FolderCollection()[n], ref WorkingFolder);
                }
                else
                {
                    // Load by project name
                    string s = FolderCollection().Find(x => Squealer.Misc.ProjectName(x).ToLower().StartsWith(NewFolder.ToLower()));
                    if (string.IsNullOrEmpty(s))
                    {
                        s = FolderCollection().Find(x => Squealer.Misc.ProjectName(x).ToLower().Contains(NewFolder.ToLower()));
                    }
                    ChangeFolder(s, ref WorkingFolder);
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Invalid folder specification.");
            }

        }

        // Remove a folder from the list of projects.
        public static void ForgetFolder(string index)
        {
            ForgetFolder(Conversions.ToInteger(index));
        }
        public static void ForgetFolder(int index)
        {

            try
            {
                var folders = FolderCollection();
                string folder = folders[index];
                folders.Remove(folder);
                MySettings.RecentProjectFolders = FolderString(folders);
                Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "OK");
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid folder number.");
            }

        }

        // Save the folder to the list of projects.
        public static void RememberFolder(string folder)
        {

            var folders = FolderCollection();
            while (folders.Contains(folder))
                folders.Remove(folder);
            folders.Insert(0, folder);
            MySettings.RecentProjectFolders = FolderString(folders);

        }

        #endregion

        #region  Main Functions 

        // Main module. Start here.
        public static void Main()
        {

            Squealer.My.Logging.WriteLog("Startup.");

            DefineCommands();

            // Increase input buffer size.
            Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput(8192)));

            // Restore the previous window size
            ResetWindowSize();

            Squealer.Textify.SayCentered(Squealer.Constants.HomePage, true);
            Squealer.Textify.SayCentered("https://github.com/totallyphilip/squealer", true);
            Squealer.Textify.SayCentered(Squealer.My.MyProject.Application.Info.Copyright, true);
            Console.WriteLine();

            var ver = new Version(Squealer.My.Configger.LoadSetting(nameof(MySettings.LastVersionNumberExecuted), "0.0.0.0"));
            if (Squealer.My.MyProject.Application.Info.Version.CompareTo(ver) > 0) // Are we running this version for the first time?
            {
                DisplayAboutInfo(false, false);
                Squealer.My.Configger.SaveSetting(nameof(MySettings.LastVersionNumberExecuted), Squealer.My.MyProject.Application.Info.Version.ToString());
            }
            else if (!(MySettings.LastVersionCheckDate.Date == DateTime.Now.Date)) // Is a newer version available?
            {
                Squealer.My.Configger.SaveSetting(nameof(MySettings.LastVersionCheckDate), DateTime.Now);
                var v = new Squealer.VersionCheck();
                v.DisplayVersionCheckResults(MySettings.MediaSourceUrl, MySettings.IsDefaultMediaSource);
            }

            // Main process
            Console.WriteLine();
            if (Squealer.Misc.IsTodayStarWarsDay())
            {
                Console.WriteLine("May the Fourth be with you!");
                Console.WriteLine();
            }

            GetLatestEz();

            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "Type HELP to get started.");
            Console.WriteLine();

            // Restore the previous working folder
            string WorkingFolder = MySettings.LastProjectFolder;
            if (System.IO.Directory.Exists(WorkingFolder))
            {
                ChangeFolder(WorkingFolder, ref WorkingFolder);
            }

            HandleUserInput(ref WorkingFolder);

            SaveWindowSize();

            // Save the current working folder for next time
            MySettings.LastProjectFolder = WorkingFolder;

            // Delete any settings that weren't referenced
            Squealer.My.Configger.PruneSettings();

            Squealer.My.Logging.WriteLog("Shutdown.");

        }

        private static void GetLatestEz()
        {
            var v = new Squealer.VersionCheck();
            v.DownloadLatestEzBinary(MySettings.MediaSourceUrl + EzBinFilename(), EzBinPath()); // always get latest binary
        }

        private static List<string> FilesToProcess(string ProjectFolder, string Wildcard, string SearchText, bool usedialog, Squealer.SquealerObjectTypeCollection filter, bool ignoreCase, bool FindExact, bool hasPrePostCode, GitFlags gf, bool DifferentOnly)
        {

            if (DifferentOnly && MyFileHashes.Items.Count == 0)
            {
                throw new ArgumentException(string.Format("Cannot use -DIFF before {0} is executed (see {1} {0})", eCommandType.hash.ToString().ToUpper(), eCommandType.help.ToString().ToUpper()));
            }

            Wildcard = Wildcard.Replace("[", "").Replace("]", "");

            var plaincolor = new Squealer.Textify.ColorScheme(ConsoleColor.Gray, ConsoleColor.Black);
            var highlightcolor = new Squealer.Textify.ColorScheme(ConsoleColor.Cyan, ConsoleColor.Black);
            var gitcolor = new Squealer.Textify.ColorScheme(ConsoleColor.Red, ConsoleColor.Black);

            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, "");
            Squealer.Textify.Write("finding", plaincolor);

            string comma = null;


            if (filter.AllSelected)
            {
                Squealer.Textify.Write(" all", highlightcolor);
                if (gf.GitEnabled)
                {
                    Squealer.Textify.Write(" uncommitted case-sensitive", gitcolor);
                }
            }
            else
            {

                if (gf.GitEnabled)
                {
                    Squealer.Textify.Write(" uncommitted case-sensitive", gitcolor);
                }

                comma = " ";

                foreach (Squealer.SquealerObjectType t in filter.Items.Where(x => x.Selected))
                {
                    Squealer.Textify.Write(comma + t.LongType.ToString(), highlightcolor);
                    comma = ", ";
                }
            }

            if (usedialog)
            {
                Squealer.Textify.Write(" hand-picked", highlightcolor);
            }

            var EverythingIncludingDuplicates = new List<string>();
            Squealer.Textify.Write(" files", plaincolor);
            if (hasPrePostCode)
            {
                Squealer.Textify.Write(" with ", plaincolor);
                Squealer.Textify.Write("pre/post code", highlightcolor);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                Squealer.Textify.Write(" containing ", plaincolor);
                Squealer.Textify.Write("\"" + SearchText + "\"" + Interaction.IIf(ignoreCase, "", "(case-sensitive)").ToString(), highlightcolor);
            }

            if (DifferentOnly)
            {
                Squealer.Textify.Write(" different than", plaincolor);
                Squealer.Textify.Write(string.Format(" {1} ({0})", MyFileHashes.Project, MyFileHashes.Branch), highlightcolor);
            }

            Squealer.Textify.Write(" matching", plaincolor);

            comma = "";

            foreach (string s in Wildcard.Split(new char[] { '|' }))
            {
                if (s.ToLower().Contains(Squealer.Constants.SquealerFileExtension.ToLower()))
                {
                    Console.WriteLine();
                    throw new ArgumentException(s.Trim() + " search term contains explicit reference To " + Squealer.Constants.SquealerFileExtension);
                }
                s = Squealer.Misc.WildcardInterpreter(s.Trim(), MySettings.WildcardBehavior, FindExact);
                Squealer.Textify.Write(comma + " " + s, highlightcolor);
                comma = ", ";

                if (gf.GitEnabled)
                {

                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        gf.ShowDeleted = false; // there will not be any deleted files that contain search text
                    }

                    var FoundFiles = new List<string>();

                    FoundFiles.AddRange(Squealer.GitShell.ChangedFiles(ProjectFolder, "git status -s", s, gf.ShowDeleted).FindAll(x => LikeOperator.LikeString(x.ToLower(), s.ToLower(), CompareMethod.Binary)));

                    // Remove any results that don't contain the search text
                    if (gf.GitEnabled && !string.IsNullOrEmpty(SearchText))
                    {
                        if (ignoreCase)
                        {
                            FoundFiles.RemoveAll(x => !System.IO.File.ReadAllText(x).ToLower().Contains(SearchText.ToLower()));
                        }
                        else
                        {
                            FoundFiles.RemoveAll(x => !System.IO.File.ReadAllText(x).Contains(SearchText));
                        }
                    }

                    EverythingIncludingDuplicates.AddRange(FoundFiles);
                }

                else if (string.IsNullOrEmpty(SearchText))
                {
                    EverythingIncludingDuplicates.AddRange(Squealer.My.MyProject.Computer.FileSystem.GetFiles(ProjectFolder, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, s).ToList());
                }
                else
                {
                    EverythingIncludingDuplicates.AddRange(Squealer.My.MyProject.Computer.FileSystem.FindInFiles(ProjectFolder, SearchText, ignoreCase, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, s).ToList());
                }

            }

            var DistinctFiles = new List<string>();
            DistinctFiles.AddRange(from s in EverythingIncludingDuplicates.Distinct()
                                   orderby s
                                   select s);

            Console.WriteLine();
            Console.WriteLine();

            // Remove any results that don't match hand picked files.
            if (usedialog)
            {
                var pickedfiles = GetFileList(ProjectFolder);
                DistinctFiles.RemoveAll(x => !pickedfiles.Exists(y => (y.ToLower() ?? "") == (x.ToLower() ?? "")));
            }

            // Remove any results that don't match the requested object types
            foreach (Squealer.SquealerObjectType t in filter.Items.Where(x => !x.Selected))
                DistinctFiles.RemoveAll(x => Squealer.SquealerObjectType.Eval(XmlGetObjectType(x)) == t.LongType);

            // Remove any results that don't have pre/post code
            if (hasPrePostCode)
            {
                DistinctFiles.RemoveAll(x => !PrePostCodeExists(x));
            }

            // Remove any results that are the same as the baseline.
            if (DifferentOnly)
            {
                DistinctFiles.RemoveAll(x => MyFileHashes.MatchExists(x));
            }

            return DistinctFiles;

        }

        // Enumerate through files in the working folder and take some action on them.
        private static void ProcessFiles(List<string> FileListing, eFileAction Action, BatchParametersClass bp, Squealer.SquealerObjectType.eType TargetFileType, GitFlags git)
        {

            int FileCount = 0;
            int SkippedFiles = 0;
            string GeneratedOutput = string.Empty;

            if (bp.OutputMode == BatchParametersClass.eOutputMode.string)
            {
                Console.Write(eCommandType.directory.ToString().ToLower() + " - x ");
            }
            else
            {

                if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Full)
                {
                    Squealer.Textify.Write("Type Flags ");
                }
                else if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Compact)
                {
                    Squealer.Textify.Write("   ");
                }
                else if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Symbolic)
                {
                    Squealer.Textify.Write(" ");
                }

                Squealer.Textify.WriteLine("FileName");

                if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Full)
                {
                    Squealer.Textify.Write("---- ----- ");
                }
                else if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Compact)
                {
                    Squealer.Textify.Write("-- ");
                }

                Squealer.Textify.WriteToEol('-');
                Console.WriteLine();

            }


            int NextPercentageStep = MySettings.OutputPercentageIncrement;


            foreach (string FileName in FileListing)
            {

                if (Console.KeyAvailable)
                {
                    throw new Exception("Keyboard interrupt.");
                }

                BracketCheck(FileName);

                var info = new System.IO.FileInfo(FileName);

                var obj = new Squealer.SquealerObject(FileName);


                if (bp.OutputMode == BatchParametersClass.eOutputMode.string)
                {
                    if (FileCount > 0)
                    {
                        Console.Write("|");
                    }
                    Console.Write(info.Name.Replace(Squealer.Constants.SquealerFileExtension, ""));
                }
                else
                {
                    var fg = ConsoleColor.Gray;




                    if (obj.Type.LongType == Squealer.SquealerObjectType.eType.Invalid)
                    {
                        fg = ConsoleColor.Red;
                    }


                    if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Full)
                    {
                        Squealer.Textify.Write(" " + obj.Type.ShortType.ToString().PadRight(4) + obj.FlagsSummary);
                    }
                    else if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Compact)
                    {
                        Squealer.Textify.Write(obj.Type.ShortType.ToString().PadRight(2));
                        if (string.IsNullOrWhiteSpace(obj.FlagsSummary))
                        {
                            Squealer.Textify.Write(" ");
                        }
                        else
                        {
                            Squealer.Textify.Write("*");
                        }
                    }
                    else if (MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Symbolic)
                    {
                        if (string.IsNullOrWhiteSpace(obj.FlagsSummary))
                        {
                            Squealer.Textify.Write(" ");
                        }
                        else
                        {
                            Squealer.Textify.Write("*");
                        }
                    }


                    Squealer.Textify.Write(info.Name.Replace(Squealer.Constants.SquealerFileExtension, ""), fg);

                    string symbol = string.Empty;
                    switch (obj.Type.LongType)
                    {
                        case Squealer.SquealerObjectType.eType.StoredProcedure:
                            {
                                symbol = Squealer.Constants.Psymbol;
                                break;
                            }
                        case Squealer.SquealerObjectType.eType.ScalarFunction:
                            {
                                symbol = Squealer.Constants.FNsymbol;
                                break;
                            }
                        case Squealer.SquealerObjectType.eType.InlineTableFunction:
                            {
                                symbol = Squealer.Constants.IFsymbol;
                                break;
                            }
                        case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                            {
                                symbol = Squealer.Constants.TFsymbol;
                                break;
                            }
                        case Squealer.SquealerObjectType.eType.View:
                            {
                                symbol = Squealer.Constants.Vsymbol;
                                break;
                            }
                    }

                    if (MySettings.AlwaysShowSymbols || MySettings.DirectoryStyleSelected == Squealer.Settings.DirectoryStyle.Symbolic)
                    {
                        Squealer.Textify.Write(symbol, ConsoleColor.Green);
                    }

                }

                try
                {

                    string gitstatuscode = string.Empty;
                    if (git.ShowUncommitted)
                    {
                        gitstatuscode = " " + Squealer.GitShell.FileSearch(info.DirectoryName, "git status -s ", info.Name)[0].Replace(info.Name, "").TrimStart();
                    }

                    switch (Action)
                    {
                        case eFileAction.directory:
                            {
                                if (bp.OutputMode == BatchParametersClass.eOutputMode.flags && obj.FlagsList.Count > 0)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("           {");
                                    foreach (string s in obj.FlagsList)
                                        Console.WriteLine("             " + s);
                                    Console.Write("           }");
                                }

                                break;
                            }
                        case eFileAction.fix:
                            {
                                if (bp.OutputMode == BatchParametersClass.eOutputMode.normal)
                                {
                                    if (RepairXmlFile(false, info.FullName))
                                    {
                                        Squealer.Textify.Write(string.Format(" ... {0}", eCommandType.fix.ToString().ToUpper()), ConsoleColor.Green);
                                    }
                                    else
                                    {
                                        SkippedFiles += 1;
                                    }
                                }
                                else if (ConvertXmlFile(info.FullName, TargetFileType))
                                {
                                    Squealer.Textify.Write(string.Format(" ... {0}", BatchParametersClass.eOutputMode.convert.ToString().ToUpper()));
                                }
                                else
                                {
                                    SkippedFiles += 1;
                                }

                                break;
                            }

                        case eFileAction.edit:
                            {
                                EditFile(info.FullName);
                                break;
                            }
                        case eFileAction.checkout:
                            {
                                Squealer.GitShell.DisplayResults(info.DirectoryName, "git checkout -- " + info.Name);
                                break;
                            }
                        case eFileAction.generate:
                            {
                                GeneratedOutput += ExpandIndividual(info, GetStringReplacements(new System.IO.FileInfo(FileListing[0]).DirectoryName), bp, FileCount + 1, FileListing.Count, MySettings.OutputStepStyleSelected == Squealer.Settings.OutputStepStyle.Detailed);

                                if (MySettings.OutputStepStyleSelected == Squealer.Settings.OutputStepStyle.Percentage)
                                {
                                    double CurrentPercentage = (FileCount + 1) / (double)FileListing.Count * 100d;
                                    if (CurrentPercentage >= NextPercentageStep || FileCount == 0 && CurrentPercentage < 1d)
                                    {
                                        while (NextPercentageStep <= CurrentPercentage)
                                            NextPercentageStep += MySettings.OutputPercentageIncrement;
                                        GeneratedOutput += Constants.vbCrLf + string.Format("print '{0}% ({1}/{2})';", (object)(NextPercentageStep - MySettings.OutputPercentageIncrement), FileCount + 1, FileListing.Count) + Constants.vbCrLf;
                                    }
                                }

                                break;
                            }

                        case eFileAction.compare:
                            {
                                string RootName = info.Name.Replace(Squealer.Constants.SquealerFileExtension, "");
                                GeneratedOutput += string.Format("insert #CodeToDrop ([Type], [Schema], [Name]) values ('{0}','{1}','{2}');", obj.Type.GeneralType, SchemaName(RootName), RoutineName(RootName)) + Constants.vbCrLf;
                                break;
                            }
                        case eFileAction.delete:
                            {
                                var trashcan = Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin;
                                if (bp.OutputMode == BatchParametersClass.eOutputMode.permanent)
                                {
                                    trashcan = Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently;
                                }
                                System.IO.File.Delete(info.FullName, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, trashcan);
                                break;
                            }
                    }
                    if (!(bp.OutputMode == BatchParametersClass.eOutputMode.string))
                    {

                        if (git.ShowUncommitted)
                        {
                            Squealer.Textify.Write(gitstatuscode, ConsoleColor.Red);
                        }
                        if (git.ShowHistory)
                        {
                            Squealer.GitShell.DisplayResults(info.DirectoryName, "git log --pretty=format:\"%h (%cr) %s\" " + info.Name);
                        }

                        Console.WriteLine();

                    }
                    FileCount += 1;
                }
                catch (Exception ex)
                {
                    Squealer.Textify.WriteLine(" ... FAILED!", ConsoleColor.Red);
                    throw new Exception(ex.Message);
                }

            }

            if (FileCount > 0)
            {
                if (bp.OutputMode == BatchParametersClass.eOutputMode.string)
                {
                    Squealer.Textify.SayNewLine();
                }
                Squealer.Textify.SayNewLine();
            }

            string SummaryLine = "{4}/{0} files ({3} skipped) (action:{1}, mode:{2})";
            if (SkippedFiles == 0)
            {
                SummaryLine = "{0} files (action:{1}, mode:{2})";
            }

            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format(SummaryLine, FileCount.ToString(), Action.ToString(), bp.OutputMode.ToString(), SkippedFiles.ToString(), (FileCount - SkippedFiles).ToString()));


            if ((Action == eFileAction.generate || Action == eFileAction.compare) && FileCount > 0)
            {

                if (Action == eFileAction.compare)
                {

                    GeneratedOutput = Squealer.My.Resources.Resources.CompareObjects.Replace("{RoutineList}", GeneratedOutput).Replace("{ExcludeFilename}", Squealer.Constants.AutocreateFilename);

                    string ignoredschemas = "'cdc'";
                    if (MySettings.EnableEzObjects)
                    {
                        ignoredschemas += ",'" + MySettings.EzSchema + "'";
                    }

                    GeneratedOutput = GeneratedOutput.Replace("{schemas-to-ignore}", ignoredschemas);
                }


                else if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test))
                {
                    if (MySettings.DetectDeprecatedSquealerObjects)
                    {
                        GeneratedOutput = Squealer.My.Resources.Resources._TopScript + GeneratedOutput;
                    }
                    if (MySettings.TrackFailedItems)
                    {
                        GeneratedOutput = Squealer.My.Resources.Resources.TrackFailedItems_Start + Constants.vbCrLf + GeneratedOutput + Constants.vbCrLf + Squealer.My.Resources.Resources.TrackFailedItems_End;
                    }
                    if (MySettings.EnableEzObjects)
                    {
                        GeneratedOutput = EzText(false).Replace("{Schema}", MySettings.EzSchema) + GeneratedOutput;
                    }
                }

                if (MySettings.OutputToClipboard)
                {
                    Console.WriteLine();
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "Output copied to Windows clipboard.");
                    Clipboard.SetText(GeneratedOutput);
                }
                else
                {
                    var tempfile = new Squealer.TempFileHandler(".sql");
                    tempfile.Writeline(GeneratedOutput);
                    tempfile.Show();
                }

            }

            Squealer.Textify.SayNewLine();

        }

        #endregion

        #region  Commands 
        private static void DefineCommands()
        {

            Squealer.CommandCatalog.CommandDefinition cmd;
            Squealer.CommandCatalog.CommandSwitch opt;

            // the un-command
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.nerfherder.ToString(), "nerf" }, new[] { "This command is as useless as a refrigerator on Hoth." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Visible = false;
            MyCommands.Items.Add(cmd);

            // show ez script only
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.eztool.ToString() }, new[] { "Display the EZ script from hidden options." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("encrypt;convert .sql to .bin.new"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("extract;convert .bin or embedded resource file to .sql"));
            cmd.Visible = false;
            MyCommands.Items.Add(cmd);

            // open folder
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.open.ToString(), "cd" }, new[] { "Open folder {options}.", "This folder path will be saved for quick access. See " + eCommandType.list.ToString().ToUpper() + " command. Omit path to open folder dialog." }, Squealer.CommandCatalog.eCommandCategory.folder, "<path>", false);
            cmd.Examples.Add("% " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            cmd.IgnoreSwitches = true;
            MyCommands.Items.Add(cmd);

            // list folders
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.list.ToString(), "l" }, new[] { "List or remove project folders.", "Set maximum list size in General tab of settings. Removing folders from the list does NOT delete folders from the filesystem." }, Squealer.CommandCatalog.eCommandCategory.folder, "project folder number", false);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("trim;remove all invalid or non-Squealer folders"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("remove;remove a specific project folder"));
            cmd.Examples.Add("% -trim -- clean up list");
            cmd.Examples.Add("% 3 -remove -- remove project folder 3 from list");
            MyCommands.Items.Add(cmd);

            // use folder
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.use.ToString() }, new[] { "Reopen a saved folder.", "See " + eCommandType.list.ToString().ToUpper() + " command." }, Squealer.CommandCatalog.eCommandCategory.folder, "<project name or folder number>", true);
            cmd.Examples.Add("% 3");
            cmd.Examples.Add("% northwind");
            MyCommands.Items.Add(cmd);

            // browse
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.browse.ToString(), "b" }, new[] { "Open file browser.", "Browse files in the current project folder. If {options} is specified, only matching files will be displayed." }, Squealer.CommandCatalog.eCommandCategory.folder, Squealer.CommandCatalog.CommandDefinition.WildcardText, false);
            cmd.Examples.Add("% *employee*");
            MyCommands.Items.Add(cmd);

            // copy path
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.copypath.ToString(), "cp" }, new[] { "Copy working folder path to clipboard.", "Copies the full path of the current working folder into the Windows clipboard.." }, Squealer.CommandCatalog.eCommandCategory.folder);
            MyCommands.Items.Add(cmd);

            // checkout
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.checkout.ToString(), "undo" }, new[] { "Git checkout.", "Checkout objects from Git and discard local changes." }, Squealer.CommandCatalog.eCommandCategory.file, true, true);
            MyCommands.Items.Add(cmd);

            // dir
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.directory.ToString(), "dir" }, new[] { "Directory.", string.Format("List {0} objects in the current working folder.", Squealer.My.MyProject.Application.Info.ProductName) }, Squealer.CommandCatalog.eCommandCategory.file, false, true);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("h;show git history"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("f;show flags"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("str;string output"));
            cmd.Examples.Add("% -cs dbo.* / Han shot first -- find all dbo.* files containing \" Han shot first\" (with leading space and capital H)");
            cmd.Examples.Add("% -p -v /Solo -- find all stored procedures and views containing \"Solo\" (or \"solo\" or \"SOLO\" or \"soLO\")");
            MyCommands.Items.Add(cmd);

            // new file
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.new.ToString() }, new[] { string.Format("Create a new {0} file.", Squealer.My.MyProject.Application.Info.ProductName), "Default schema is \"dbo\"." }, Squealer.CommandCatalog.eCommandCategory.file, Squealer.CommandCatalog.CommandDefinition.FilenameText, true);
            foreach (string s in new Squealer.SquealerObjectTypeCollection().ObjectTypesOptionString(false).Split(new char[] { '|' }))
                cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch(s, s.StartsWith("p")));
            cmd.Examples.Add("% AddEmployee -- create new stored procedure dbo.AddEmployee");
            cmd.Examples.Add("% -v myschema.Employees -- create new view myschema.Employees");
            MyCommands.Items.Add(cmd);

            // edit files
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.edit.ToString(), "e" }, new[] { string.Format("Edit {0} files.", Squealer.My.MyProject.Application.Info.ProductName), string.Format("Uses your configured text editor. See {0} command.", eCommandType.setting.ToString().ToUpper()) }, Squealer.CommandCatalog.eCommandCategory.file, false, true);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("all;override file limit"));
            cmd.Examples.Add("% dbo.AddEmployee");
            cmd.Examples.Add("% dbo.*");
            MyCommands.Items.Add(cmd);

            // fix files
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.fix.ToString() }, new[] { string.Format("Rewrite {0} files (DESTRUCTIVE).", Squealer.My.MyProject.Application.Info.ProductName), string.Format("Files will be validated and reformatted to {0} specifications. Optionally convert files to a different type.", Squealer.My.MyProject.Application.Info.ProductName) }, Squealer.CommandCatalog.eCommandCategory.file, false, true);
            opt = new Squealer.CommandCatalog.CommandSwitch("c;convert to");
            foreach (string s in new Squealer.SquealerObjectTypeCollection().ObjectTypesOptionString(false).Split(new char[] { '|' }))
                opt.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitchOption(s));
            cmd.Options.Items.Add(opt);
            cmd.Examples.Add("% dbo.*");
            cmd.Examples.Add("% -c:p * -- convert everything to stored procedures");
            cmd.Examples.Add("% -v -p -c:if * -- convert views and stored procedures to inline table-valued functions");
            MyCommands.Items.Add(cmd);

            // generate
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.generate.ToString(), "gen" }, new[] { "Generate SQL Server CREATE or ALTER scripts.", string.Format("Output is written to a temp file and opened with your configured text editor. See {0} command.", eCommandType.setting.ToString().ToUpper()) }, Squealer.CommandCatalog.eCommandCategory.file, false, true);
            opt = new Squealer.CommandCatalog.CommandSwitch("m;output mode");
            opt.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitchOption("alt;alter, do not drop original"));
            opt.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitchOption("t;test script, limit 1 object"));
            opt.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitchOption("e;with encryption"));
            cmd.Options.Items.Add(opt);
            cmd.Examples.Add("% dbo.*");
            cmd.Examples.Add("% -m:alt -v dbo.* -- generate ALTER scripts for dbo.* VIEW objects");
            cmd.Examples.Add(string.Format("% -diff * -- generate files that have changed (see {0} command)", eCommandType.hash.ToString().ToUpper()));
            MyCommands.Items.Add(cmd);

            // baseline
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.hash.ToString() }, new[] { string.Format("Calculate the hash values for all {0} files.", Squealer.My.MyProject.Application.Info.ProductName), string.Format("This is useful when working with source control such as Git. For example, {0} your files, then check out a different branch, then {1} only files that are different from the {0}. The hash values are kept in memory only; nothing is written to disk.", eCommandType.hash.ToString().ToUpper(), eCommandType.generate.ToString().ToUpper()) }, Squealer.CommandCatalog.eCommandCategory.file);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("i;information display"));
            MyCommands.Items.Add(cmd);

            // compare
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.compare.ToString() }, new[] { string.Format("Compare {0} with SQL Server.", Squealer.My.MyProject.Application.Info.ProductName), string.Format("This generates a T-SQL query to discover any SQL Server objects that are not in {0}, and any {0} objects that are not in SQL Server.", Squealer.My.MyProject.Application.Info.ProductName) }, Squealer.CommandCatalog.eCommandCategory.file, false, true);
            MyCommands.Items.Add(cmd);

            // delete
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.delete.ToString(), "del" }, new[] { string.Format("Delete {0} files.", Squealer.My.MyProject.Application.Info.ProductName), "Objects will be sent to the Recycle Bin by default." }, Squealer.CommandCatalog.eCommandCategory.file, true, true);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("e;permanently erase"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("all;override file limit"));
            cmd.Examples.Add("% dbo.AddEmployee");
            cmd.Examples.Add("% dbo.*");
            MyCommands.Items.Add(cmd);


            // make
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.make.ToString() }, new[] { string.Format("Automatically create {0} files.", Squealer.My.MyProject.Application.Info.ProductName), "Create default INSERT, UPDATE, SELECT, and DELETE files for the target database. Define the target database with the " + eCommandType.connection.ToString().ToUpper() + " command." }, Squealer.CommandCatalog.eCommandCategory.file);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch(string.Format("r;replace existing {0} objects only", Squealer.My.MyProject.Application.Info.ProductName)));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("nocomment;omit data source and timestamp from comment section"));
            MyCommands.Items.Add(cmd);

            // reverse engineer
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.reverse.ToString() }, new[] { string.Format("Reverse engineer SQL objects.", Squealer.My.MyProject.Application.Info.ProductName), "Reverse engineer existing SQL Server procs, views, and functions from the target database. Define the target database with the " + eCommandType.connection.ToString().ToUpper() + " command. Duplicate filenames will not be overwritten. Results are NOT GUARANTEED and require manual review and edits." }, Squealer.CommandCatalog.eCommandCategory.file);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("clean;attempt to clean up imported code previously generated by " + Squealer.My.MyProject.Application.Info.ProductName));
            MyCommands.Items.Add(cmd);


            // help
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.help.ToString(), "h" }, new[] { "{command} {options} for help.", "Use {command} alone for list of commands." }, Squealer.CommandCatalog.eCommandCategory.other, "<command>", false);
            cmd.Examples.Add("% " + eCommandType.generate.ToString());
            MyCommands.Items.Add(cmd);

            // helpall
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.helpall.ToString() }, new[] { "Show all help including hidden commands." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Visible = false;
            MyCommands.Items.Add(cmd);


            // config
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.config.ToString(), "c" }, new[] { "Edit " + Squealer.Constants.ConfigFilename + ".", "This file configures how " + Squealer.My.MyProject.Application.Info.ProductName + " operates in your current working folder." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("new;create new default file"));
            MyCommands.Items.Add(cmd);

            // setting
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.setting.ToString(), "set" }, new[] { "Display application settings." }, Squealer.CommandCatalog.eCommandCategory.other);
            MyCommands.Items.Add(cmd);

            // connection string
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.connection.ToString(), "cs" }, new[] { "Define the SQL Server connection string.", string.Format("The connection string is encrypted for the current local user and current working folder, and is required for some {0} commands. If you are using version control, you should add \"{1}\" to your ignore list.", Squealer.My.MyProject.Application.Info.ProductName, Squealer.Constants.ConnectionStringFilename) }, Squealer.CommandCatalog.eCommandCategory.other, "<connectionstring>", false);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("e;edit current connection string"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("t;test connection", true));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("set;encrypt and save the connection string"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("show;display the connection string"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("forget;discard the saved connection string"));
            cmd.Examples.Add("% -set " + Squealer.Constants.DefaultConnectionString);
            MyCommands.Items.Add(cmd);

            // cls
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.clear.ToString(), "cls" }, new[] { "Clear the console." }, Squealer.CommandCatalog.eCommandCategory.other);
            MyCommands.Items.Add(cmd);

            // about
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.about.ToString() }, new[] { "Check for updates and display program information." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("whatsnew;display what's new"));
            cmd.Options.Items.Add(new Squealer.CommandCatalog.CommandSwitch("changelog;display full change log"));
            MyCommands.Items.Add(cmd);

            // download
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.download.ToString() }, new[] { string.Format("Download the latest version of {0}.", Squealer.My.MyProject.Application.Info.ProductName) }, Squealer.CommandCatalog.eCommandCategory.other);
            MyCommands.Items.Add(cmd);

            // exit
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.exit.ToString(), "x" }, new[] { "Quit." }, Squealer.CommandCatalog.eCommandCategory.other);
            MyCommands.Items.Add(cmd);

            // test 
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.test.ToString() }, new[] { "Hidden command. Debugging/testing only." }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Visible = false;
            MyCommands.Items.Add(cmd);

            // release
            cmd = new Squealer.CommandCatalog.CommandDefinition(new[] { eCommandType.release.ToString() }, new[] { string.Format("Create files for {0} release.", Squealer.My.MyProject.Application.Info.Version) }, Squealer.CommandCatalog.eCommandCategory.other);
            cmd.Visible = false;
            MyCommands.Items.Add(cmd);

        }

        private static bool StringInList(List<string> l, string s)
        {
            return l.Exists(x => (x.ToLower() ?? "") == (s.ToLower() ?? ""));
        }


        // The main command interface loop.
        private static void HandleUserInput(ref string WorkingFolder)
        {

            var MySwitches = new List<string>();
            string UserInput = null;
            string RawUserInput = string.Empty;

            var MyCommand = MyCommands.FindCommand(eCommandType.nerfherder.ToString());
            bool SwitchesValidated = true;
            string MySearchText = string.Empty;
            var ObjectTypeFilter = new Squealer.SquealerObjectTypeCollection();
            bool FirstLoop = true;



            while (!(MyCommand is not null && (MyCommand.Keyword ?? "") == (eCommandType.exit.ToString() ?? "") && SwitchesValidated && string.IsNullOrEmpty(UserInput)))
            {

                try
                {

                    if (MyCommand is not null && (MyCommand.Keyword ?? "") == (eCommandType.nerfherder.ToString() ?? "") && FirstLoop)
                    {
                    }

                    // do nothing


                    else if (!SwitchesValidated)
                    {

                        throw new Exception("Invalid command switch.");
                    }


                    else if (MyCommand is not null && MyCommand.ParameterRequired && string.IsNullOrEmpty(UserInput))
                    {

                        throw new Exception("Required parameter is missing.");
                    }


                    else if (MyCommand is not null && string.IsNullOrEmpty(MyCommand.ParameterDefinition) && !string.IsNullOrEmpty(UserInput))
                    {

                        throw new Exception("Unexpected command parameter.");
                    }


                    else if (MyCommand is null)
                    {

                        throw new Exception(Squealer.Constants.BadCommandMessage);
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.about.ToString() ?? "") && SwitchesValidated)
                    {

                        DisplayAboutInfo(StringInList(MySwitches, "whatsnew"), StringInList(MySwitches, "changelog"));
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.clear.ToString() ?? ""))
                    {

                        Console.Clear();
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.copypath.ToString() ?? "") && string.IsNullOrEmpty(UserInput))
                    {

                        Clipboard.SetText(WorkingFolder);
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("Copied: {0}", WorkingFolder));
                        Console.WriteLine();
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.download.ToString() ?? "") && string.IsNullOrEmpty(UserInput))
                    {

                        var v = new Squealer.VersionCheck();
                        v.DownloadLatestInstaller(MySettings.MediaSourceUrl);
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.eztool.ToString() ?? "") && StringInList(MySwitches, "encrypt"))
                    {

                        EzConvertSqlToNewBin();
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.eztool.ToString() ?? "") && StringInList(MySwitches, "extract"))
                    {

                        EzExtractSqlToFile();
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.eztool.ToString() ?? "") && MySwitches.Count == 0)
                    {

                        var f = new Squealer.TempFileHandler("sql");
                        f.Writeline(EzText(false).Replace("{Schema}", MySettings.EzSchema));
                        f.Show();
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.hash.ToString() ?? "") && StringInList(MySwitches, "i") && string.IsNullOrWhiteSpace(UserInput))
                    {

                        Squealer.Textify.WriteLine("Latest baseline:", ConsoleColor.White);
                        Console.WriteLine();
                        Console.WriteLine(string.Format("Last snapshot at {0}", MyFileHashes.CacheDate.ToString()));
                        Console.WriteLine(string.Format("{0} files", MyFileHashes.Items.Count.ToString()));
                        Console.WriteLine(string.Format("project: {0}, branch: {1}", MyFileHashes.Project, MyFileHashes.Branch));
                        Console.WriteLine();
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.hash.ToString() ?? "") && string.IsNullOrWhiteSpace(UserInput))
                    {

                        Console.Write("Calculating hashes..");

                        var spinny = new Squealer.SpinCursor();

                        int i = 0;

                        MyFileHashes.Reset(Squealer.Misc.ProjectName(WorkingFolder), Squealer.GitShell.CurrentBranch(WorkingFolder));
                        foreach (string f in Squealer.My.MyProject.Computer.FileSystem.GetFiles(WorkingFolder, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*" + Squealer.Constants.SquealerFileExtension))
                        {
                            i += 1;
                            MyFileHashes.AddItem(f);
                            spinny.Animate();
                        }

                        Console.WriteLine();
                        Console.WriteLine(string.Format("{0} files hashed.", i.ToString()));
                        Console.WriteLine();
                        Squealer.Textify.WriteLine("New baseline stored in memory.", ConsoleColor.White);
                        Console.WriteLine();
                    }

                    // foooooooooooooooooooooo




                    else if ((MyCommand.Keyword ?? "") == (eCommandType.config.ToString() ?? ""))
                    {

                        // Try to make a new file
                        if (StringInList(MySwitches, "new"))
                        {
                            if (System.IO.File.Exists(WorkingFolder + @"\" + Squealer.Constants.ConfigFilename))
                            {
                                throw new Exception("Config file already exists.");
                            }
                            else
                            {
                                Squealer.My.MyProject.Computer.FileSystem.WriteAllText(WorkingFolder + @"\" + Squealer.Constants.ConfigFilename, Squealer.My.Resources.Resources.UserConfig, false);
                                MainType.SayFileAction("config file created", WorkingFolder, Squealer.Constants.ConfigFilename);
                                Squealer.Textify.SayNewLine();
                            }
                            Squealer.Textify.SayNewLine();
                        }

                        // Now edit 
                        MainType.OpenInTextEditor(Squealer.Constants.ConfigFilename, WorkingFolder);
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.delete.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.directory.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.checkout.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.generate.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.edit.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.fix.ToString() ?? "") || (MyCommand.Keyword ?? "") == (eCommandType.compare.ToString() ?? ""))





                    {


                        int FileLimit = int.MaxValue;
                        var action = eFileAction.directory;
                        var bp = new BatchParametersClass();
                        var targetftype = Squealer.SquealerObjectType.eType.Invalid; // for object conversion only
                        var gf = new GitFlags();
                        gf.ShowUncommitted = StringInList(MySwitches, "u");

                        if (!MyCommand.ParameterRequired && string.IsNullOrWhiteSpace(UserInput))
                        {
                            UserInput = "*";
                        }

                        bool usedialog = false;
                        if (!string.IsNullOrWhiteSpace(UserInput) && UserInput == "#")
                        {
                            usedialog = true;
                            UserInput = "*";
                        }


                        if ((MyCommand.Keyword ?? "") == (eCommandType.delete.ToString() ?? ""))
                        {

                            action = eFileAction.delete;

                            if (StringInList(MySwitches, "e"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.permanent;
                            }

                            FileLimit = 20;
                        }



                        else if ((MyCommand.Keyword ?? "") == (eCommandType.checkout.ToString() ?? ""))
                        {

                            action = eFileAction.checkout;
                            gf.ShowUncommitted = true;
                            gf.ShowDeleted = true;
                        }


                        // FileLimit = 20




                        else if ((MyCommand.Keyword ?? "") == (eCommandType.directory.ToString() ?? ""))
                        {

                            if (gf.ShowUncommitted)
                            {
                                gf.ShowDeleted = true;
                            }
                            if (StringInList(MySwitches, "h"))
                            {
                                gf.ShowHistory = true;
                            }
                            if (StringInList(MySwitches, "f"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.flags;
                            }
                            else if (StringInList(MySwitches, "str"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.string;
                            }
                        }


                        else if ((MyCommand.Keyword ?? "") == (eCommandType.edit.ToString() ?? ""))
                        {

                            action = eFileAction.edit;

                            FileLimit = 10;
                        }


                        else if ((MyCommand.Keyword ?? "") == (eCommandType.generate.ToString() ?? ""))
                        {

                            action = eFileAction.generate;

                            if (StringInList(MySwitches, "m:t"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.test;
                                FileLimit = 1;
                            }
                            else if (StringInList(MySwitches, "m:e"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.encrypt;
                            }
                            else if (StringInList(MySwitches, "m:alt"))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.alter;
                            }
                        }


                        else if ((MyCommand.Keyword ?? "") == (eCommandType.fix.ToString() ?? ""))
                        {

                            action = eFileAction.fix;

                            string convertswitch = MySwitches.Find(x => x.Split(new char[] { ':' })[0].ToLower() == "c");
                            if (!string.IsNullOrWhiteSpace(convertswitch))
                            {
                                targetftype = Squealer.SquealerObjectType.Eval(convertswitch.Split(new char[] { ':' })[1]);
                            }

                            if (!(targetftype == Squealer.SquealerObjectType.eType.Invalid))
                            {
                                bp.OutputMode = BatchParametersClass.eOutputMode.convert;
                            }
                        }

                        else if ((MyCommand.Keyword ?? "") == (eCommandType.compare.ToString() ?? ""))
                        {

                            action = eFileAction.compare;

                        }




                        bool ignoreCase = !StringInList(MySwitches, "cs");
                        bool findexact = StringInList(MySwitches, "x");
                        bool ignorefilelimit = StringInList(MySwitches, "all");
                        bool findPrePost = StringInList(MySwitches, "code");

                        var SelectedFiles = FilesToProcess(WorkingFolder, UserInput, MySearchText, usedialog, ObjectTypeFilter, ignoreCase, findexact, findPrePost, gf, StringInList(MySwitches, "diff"));

                        ThrowErrorIfOverFileLimit(FileLimit, SelectedFiles.Count, ignorefilelimit);

                        MainType.ProcessFiles(SelectedFiles, action, bp, targetftype, gf);
                    }







                    else if ((MyCommand.Keyword ?? "") == (eCommandType.help.ToString() ?? ""))
                    {

                        if (string.IsNullOrEmpty(UserInput))
                        {
                            MyCommands.ShowHelpCatalog(false);
                        }
                        else
                        {
                            var HelpWithCommand = MyCommands.FindCommand(UserInput);

                            if (HelpWithCommand is not null)
                            {
                                HelpWithCommand.ShowHelp();
                            }
                            else
                            {
                                throw new Exception("Unknown command.");
                            }
                        }
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.helpall.ToString() ?? "") && string.IsNullOrWhiteSpace(UserInput))
                    {

                        MyCommands.ShowHelpCatalog(true);
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.list.ToString() ?? "") && StringInList(MySwitches, "remove") && !string.IsNullOrEmpty(UserInput))
                    {

                        ForgetFolder(UserInput);
                        Squealer.Textify.SayNewLine();
                    }




                    else if ((MyCommand.Keyword ?? "") == (eCommandType.list.ToString() ?? "") && StringInList(MySwitches, "trim") && string.IsNullOrEmpty(UserInput))
                    {

                        AutoRemoveFolders();
                        Squealer.Textify.SayNewLine();
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.list.ToString() ?? "") && MySwitches.Count == 0 && string.IsNullOrEmpty(UserInput))
                    {

                        ListFolders(WorkingFolder);
                    }





                    else if ((MyCommand.Keyword ?? "") == (eCommandType.@new.ToString() ?? ""))
                    {

                        var filetype = Squealer.SquealerObjectType.eType.StoredProcedure;
                        if (ObjectTypeFilter.SelectedCount > 0)
                        {
                            filetype = ObjectTypeFilter.Items.Find(x => x.Selected).LongType;
                        }

                        BracketCheck(UserInput);

                        string f = MainType.CreateNewFile(WorkingFolder, filetype, UserInput);

                        if (MySettings.AutoEditNewFiles && !string.IsNullOrEmpty(f))
                        {
                            EditFile(f);
                        }
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.open.ToString() ?? ""))
                    {

                        if (string.IsNullOrWhiteSpace(UserInput))
                        {
                            var f = new FolderBrowserDialog();
                            f.ShowDialog();
                            UserInput = f.SelectedPath;
                            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Carat, UserInput);
                        }
                        ChangeFolder(UserInput, ref WorkingFolder);
                    }




                    else if ((MyCommand.Keyword ?? "") == (eCommandType.setting.ToString() ?? ""))
                    {

                        bool w = MySettings.LockWindowSize;
                        MySettings.Show();
                        if (MySettings.LockWindowSize && !w)
                        {
                            SaveWindowSize();
                        }
                    }


                    else if ((MyCommand.Keyword ?? "") == (eCommandType.browse.ToString() ?? ""))
                    {

                        // OpenExplorer(Misc.WildcardInterpreter(UserInput, MySettings.WildcardBehavior, False), WorkingFolder)


                        var SelectedFiles = MainType.OpenExplorer(Squealer.Misc.WildcardInterpreter(UserInput, MySettings.WildcardBehavior, false), WorkingFolder); // FilesToProcess(WorkingFolder, UserInput, MySearchText, True, ObjectTypeFilter, True, False, False, False, New GitFlags)

                        if (SelectedFiles.Count > 0)
                        {
                            MainType.ProcessFiles(SelectedFiles, eFileAction.edit, new BatchParametersClass(), Squealer.SquealerObjectType.eType.Invalid, new GitFlags());
                        }
                    }






                    else if ((MyCommand.Keyword ?? "") == (eCommandType.use.ToString() ?? ""))
                    {

                        LoadFolder(UserInput, ref WorkingFolder);
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.connection.ToString() ?? "") && StringInList(MySwitches, "set") && !string.IsNullOrEmpty(UserInput))
                    {
                        SetConnectionString(WorkingFolder, UserInput);
                    }
                    else if ((MyCommand.Keyword ?? "") == (eCommandType.connection.ToString() ?? "") && StringInList(MySwitches, "show") && string.IsNullOrEmpty(UserInput))
                    {
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, GetConnectionString(WorkingFolder));
                        Squealer.Textify.SayNewLine();
                    }
                    else if ((MyCommand.Keyword ?? "") == (eCommandType.connection.ToString() ?? "") && (StringInList(MySwitches, "t") || MySwitches.Count == 0) && string.IsNullOrEmpty(UserInput))
                    {
                        TestConnectionString(WorkingFolder);
                    }
                    else if ((MyCommand.Keyword ?? "") == (eCommandType.connection.ToString() ?? "") && StringInList(MySwitches, "forget") && string.IsNullOrEmpty(UserInput))
                    {
                        ForgetConnectionString(WorkingFolder);
                    }
                    else if ((MyCommand.Keyword ?? "") == (eCommandType.connection.ToString() ?? "") && StringInList(MySwitches, "e") && string.IsNullOrEmpty(UserInput))
                    {
                        string cs;
                        try
                        {
                            cs = GetConnectionString(WorkingFolder);
                        }
                        catch (Exception ex)
                        {
                            cs = Squealer.Constants.DefaultConnectionString;
                        }
                        cs = Interaction.InputBox("Connection String", "", cs);
                        if (!string.IsNullOrWhiteSpace(cs))
                        {
                            SetConnectionString(WorkingFolder, cs);
                            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, cs);
                            Squealer.Textify.SayNewLine();
                        }
                    }



                    else if ((MyCommand.Keyword ?? "") == (eCommandType.make.ToString() ?? ""))
                    {


                        Automagic(GetConnectionString(WorkingFolder), WorkingFolder, StringInList(MySwitches, "r"), !StringInList(MySwitches, "nocomment"));
                    }




                    else if ((MyCommand.Keyword ?? "") == (eCommandType.reverse.ToString() ?? ""))
                    {

                        ReverseEngineer(GetConnectionString(WorkingFolder), WorkingFolder, StringInList(MySwitches, "clean"));
                    }





                    else if ((MyCommand.Keyword ?? "") == (eCommandType.release.ToString() ?? ""))
                    {

                        var v = new Squealer.VersionCheck();
                        v.CreateMetadata(MySettings.MediaSourceUrl);
                    }



                    else if (MyCommand.Keyword == "test") // footest
                    {

                        foreach (string f in Squealer.My.MyProject.Computer.FileSystem.GetFiles(WorkingFolder, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.sqlr"))
                        {
                            if (!MyFileHashes.MatchExists(f))
                            {
                                Console.Write(f.Remove(0, f.LastIndexOf(@"\")));
                                Console.WriteLine(" ... DIFFERENT");
                            }

                        }
                    }


                    else
                    {
                        throw new Exception(Squealer.Constants.BadCommandMessage);
                    }
                }

                catch (Exception ex)
                {

                    Squealer.Textify.SayError(ex.Message);

                    if (MyCommand is null || (MyCommand.Keyword ?? "") == (eCommandType.nerfherder.ToString() ?? ""))
                    {
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "Try: HELP");
                    }
                    else
                    {
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "Try: HELP " + MyCommand.Keyword.ToUpper());
                    }

                    Squealer.Textify.SayNewLine();

                    Squealer.My.Logging.WriteLog("Command error: " + RawUserInput);
                    Squealer.My.Logging.WriteLog(ex.Message + Constants.vbCrLf + ex.StackTrace);


                }

                FirstLoop = false;

                Console.Title = Squealer.Misc.TitleText(MySettings.ShowProjectNameInTitleBar, MySettings.ShowProjectDirectoryInTitleBar, WorkingFolder);

                UserInput = string.Empty;
                while (string.IsNullOrWhiteSpace(UserInput))
                {

                    if (!System.IO.Directory.Exists(WorkingFolder))
                    {
                        Squealer.Textify.SayError(WorkingFolder, Squealer.Textify.eSeverity.error, true);
                        Console.WriteLine();
                    }

                    if (MySettings.ShowProjectNameInCommandPrompt)
                    {
                        Squealer.Textify.Write(string.Format("[{0}] ", Squealer.Misc.ProjectName(WorkingFolder)), ConsoleColor.DarkYellow);
                    }
                    if (MySettings.ShowGitBranch)
                    {
                        string s = Squealer.GitShell.CurrentBranch(WorkingFolder);
                        var c = ConsoleColor.DarkGreen;
                        if ((s ?? "") == Squealer.GitShell.GitErrorMessage)
                        {
                            c = ConsoleColor.Red;
                        }
                        Squealer.Textify.Write(string.Format("({0}) ", s), c);
                    }
                    Squealer.Textify.Write("> ", ConsoleColor.DarkYellow);
                    ClearKeyboard();

                    var KA = new Squealer.KeepAlive();
                    if (MySettings.KeepScreenAlive)
                    {
                        KA.KeepMonitorActive();
                    }
                    UserInput = Console.ReadLine();
                    if (MySettings.LockWindowSize)
                    {
                        ResetWindowSize();
                    }
                    KA.RestoreMonitorSettings();
                    Squealer.Textify.SayNewLine();

                }

                RawUserInput = UserInput;

                // Separate command text from search text
                if (UserInput.Contains("/"))
                {
                    int n = UserInput.IndexOf("/");
                    MySearchText = UserInput.Substring(n + 1);
                    UserInput = UserInput.Substring(0, n);
                }
                else
                {
                    MySearchText = string.Empty;
                }


                string keyword = UserInput.Trim().Split(new char[] { ' ' })[0]; // get the first solid word
                MyCommand = MyCommands.FindCommand(keyword);
                var SplitInput = new List<string>();
                if (MyCommand is not null)
                {
                    SplitInput.AddRange(UserInput.Trim().Split(new char[] { ' ' }));
                }
                UserInput = string.Empty;
                MySwitches.Clear();

                // Go through each piece of the user command and pull out all the switches.
                while (SplitInput.Count > 0)
                {

                    string rawinput = SplitInput[0];

                    if (rawinput.StartsWith("-") && !MyCommand.IgnoreSwitches) // -Ex:Opt:JUNK
                    {

                        string switchinput = rawinput.Remove(0, 1).ToLower(); // -Ex:Opt:JUNK -> ex:opt:junk

                        string switchkeyword = switchinput.Split(new char[] { ':' })[0]; // ex:opt:junk -> ex
                        string switchoption = string.Empty;
                        if (switchinput.Contains(":"))
                        {
                            switchoption = ":" + switchinput.Split(new char[] { ':' })[1]; // ex:opt:junk -> :opt
                        }

                        MySwitches.Add(switchkeyword + switchoption);
                    }

                    else
                    {
                        UserInput += " " + rawinput;
                    }

                    SplitInput.RemoveAt(0);

                }



                // Separate the command from everything after it
                UserInput = UserInput.Trim();
                if (string.IsNullOrEmpty(UserInput))
                {
                    MyCommand = MyCommands.FindCommand(eCommandType.nerfherder.ToString()); // this is a dummy command just so the command object is not Nothing
                }
                else
                {
                    // Dim keyword As String = UserInput.Split(New Char() {" "c})(0)
                    keyword = UserInput.Split(new char[] { ' ' })[0];
                    // MyCommand = MyCommands.FindCommand(keyword)
                    UserInput = UserInput.Remove(0, keyword.Length).Trim();
                }









                // Test the switches
                SwitchesValidated = true;
                if (MyCommand is not null)
                {
                    foreach (string s in MySwitches)
                    {

                        var opt = MyCommand.Options.Items.Find(x => (x.Keyword ?? "") == (s.Split(new char[] { ':' })[0] ?? ""));

                        if (opt is null) // Is the switch legit?
                        {
                            SwitchesValidated = false;
                        }
                        else
                        {

                            if (opt.Options.Items.Count > 0 && !s.Contains(":")) // Did user omit required switch option?
                            {
                                SwitchesValidated = false;
                            }

                            if (s.Contains(":"))
                            {
                                if (!opt.Options.Items.Exists(x => (x.Keyword ?? "") == (s.Split(new char[] { ':' })[1] ?? ""))) // Is the switch option legit?
                                {
                                    SwitchesValidated = false;
                                }
                            }

                        }
                    }
                }

                // Move object types from the switch list to the object type list
                ObjectTypeFilter.SetAllFlags(false);

                while (MySwitches.Exists(x => Squealer.SquealerObjectType.Validated(x)))
                {
                    var t = Squealer.SquealerObjectType.Eval(MySwitches.Find(x => !(Squealer.SquealerObjectType.Eval(x) == Squealer.SquealerObjectType.eType.Invalid)));
                    ObjectTypeFilter.SetOneFlag(t, true);
                    MySwitches.Remove(Squealer.SquealerObjectType.ToShortType(t).ToString());
                }


                if (ObjectTypeFilter.NoneSelected)
                {
                    ObjectTypeFilter.SetAllFlags(true);
                }

            }

        }

        private static List<string> GetFileList(string WhichFolder)
        {

            var dialog = new OpenFileDialog();

            dialog.FileName = ""; // Default file name
            dialog.DefaultExt = ".sqlr"; // Default file extension
            dialog.Filter = "Text Files (*.sqlr)|*.sqlr";
            dialog.Multiselect = true;
            dialog.RestoreDirectory = true;
            dialog.InitialDirectory = WhichFolder;

            // Show open file dialog box
            dialog.ShowDialog();

            var fnames = new List<string>();
            foreach (string fn in dialog.FileNames)
                fnames.Add(fn);

            return fnames;

        }

        #endregion

        #region  XML Default Values 

        // If the first parameter is null, return the second.
        private static bool AttributeDefaultBoolean(System.Xml.XmlNode attr, bool deefalt)
        {

            if (attr is null)
            {
                return deefalt;
            }
            else
            {
                return Conversions.ToBoolean(Interaction.IIf(string.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value));
            }

        }

        // If the first parameter is null, return the second.
        private static string AttributeDefaultString(System.Xml.XmlNode attr, string deefalt)
        {

            if (attr is null)
            {
                return deefalt;
            }
            else
            {
                return Interaction.IIf(string.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value).ToString();
            }

        }

        // If the first parameter is null, return the second.
        private static int AttributeDefaultInteger(System.Xml.XmlNode attr, int deefalt)
        {

            if (attr is null)
            {
                return deefalt;
            }
            else
            {
                return int.Parse(Interaction.IIf(string.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value).ToString());
            }

        }

        #endregion

        #region  XML Reading 

        private static string XmlGetObjectType(string FileName)
        {

            try
            {
                var Reader = new System.Xml.XmlDocument();
                Reader.Load(FileName);
                var Node = Reader.SelectSingleNode("/" + Squealer.My.MyProject.Application.Info.ProductName);

                // Get the type.
                return Node.Attributes["Type"].Value.ToString();
            }
            catch (Exception ex)
            {
                return Squealer.SquealerObjectType.eShortType.err.ToString();
            }

        }

        private static bool PrePostCodeExists(string FileName)
        {

            var InputXml = new System.Xml.XmlDocument();

            InputXml.Load(FileName);

            System.Xml.XmlElement InRoot = (System.Xml.XmlElement)InputXml.SelectSingleNode(Squealer.My.MyProject.Application.Info.ProductName);

            bool HasCode = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(InRoot.SelectSingleNode("PreCode").InnerText))
                {
                    HasCode = true;
                }
            }
            catch (Exception ex)
            {
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(InRoot.SelectSingleNode("PostCode").InnerText))
                {
                    HasCode = true;
                }
            }
            catch (Exception ex)
            {
            }

            return HasCode;

        }


        // Get all the parameters.
        private static DataTable GetParameters(System.Xml.XmlDocument InXml)
        {
            DataTable GetParametersRet = default;

            var Parameters = new DataTable();

            {
                var withBlock = Parameters.Columns;
                withBlock.Add("Name", typeof(string));
                withBlock.Add("Type", typeof(string));
                withBlock.Add("Output", typeof(bool));
                withBlock.Add("ReadOnly", typeof(bool));
                withBlock.Add("DefaultValue", typeof(string));
                withBlock.Add("Comments", typeof(string));
            }

            foreach (System.Xml.XmlNode Node in InXml.SelectNodes(Squealer.My.MyProject.Application.Info.ProductName + "/Parameters/Parameter"))


                Parameters.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), string.Empty), AttributeDefaultString(Node.Attributes.GetNamedItem("Type"), string.Empty), AttributeDefaultString(Node.Attributes.GetNamedItem("Output"), bool.FalseString), AttributeDefaultString(Node.Attributes.GetNamedItem("ReadOnly"), bool.FalseString), AttributeDefaultString(Node.Attributes.GetNamedItem("DefaultValue"), string.Empty), AttributeDefaultString(Node.Attributes.GetNamedItem("Comments"), string.Empty));

            GetParametersRet = Parameters;
            return GetParametersRet;

        }

        // Get all the table columns.
        private static DataTable GetColumns(System.Xml.XmlDocument InXml)
        {
            DataTable GetColumnsRet = default;

            var Columns = new DataTable();

            {
                var withBlock = Columns.Columns;
                withBlock.Add("Name", typeof(string));
                withBlock.Add("Type", typeof(string));
                withBlock.Add("Nullable", typeof(bool));
                withBlock.Add("Identity", typeof(bool));
                withBlock.Add("IncludeInPrimaryKey", typeof(bool));
                withBlock.Add("Comments", typeof(string));
            }

            foreach (System.Xml.XmlNode Node in InXml.SelectNodes(Squealer.My.MyProject.Application.Info.ProductName + "/Table/Column"))

                Columns.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), string.Empty), AttributeDefaultString(Node.Attributes.GetNamedItem("Type"), string.Empty), AttributeDefaultBoolean(Node.Attributes.GetNamedItem("Nullable"), true), AttributeDefaultBoolean(Node.Attributes.GetNamedItem("Identity"), false), AttributeDefaultBoolean(Node.Attributes.GetNamedItem("IncludeInPrimaryKey"), false), AttributeDefaultString(Node.Attributes.GetNamedItem("Comments"), string.Empty));

            GetColumnsRet = Columns;
            return GetColumnsRet;

        }

        // Get all the users.
        private static DataTable GetUsers(System.Xml.XmlDocument InXml)
        {
            DataTable GetUsersRet = default;

            var Users = new DataTable();

            {
                var withBlock = Users.Columns;
                withBlock.Add("Name", typeof(string));
            }

            foreach (System.Xml.XmlNode Node in InXml.SelectNodes(Squealer.My.MyProject.Application.Info.ProductName + "/Users/User"))


                Users.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), string.Empty));

            GetUsersRet = Users;
            return GetUsersRet;

        }

        #endregion

        #region  XML Processing 

        // Load and clean up the XML keeping the original file type.
        private static System.Xml.XmlDocument FixedXml(bool ApplyDefaultUsers, string fqfn)
        {
            var obj = new Squealer.SquealerObject(fqfn);
            return FixedXml(ApplyDefaultUsers, fqfn, obj);
        }

        // Load and clean up the XML using the specified target file type.
        private static System.Xml.XmlDocument FixedXml(bool ApplyDefaultUsers, string fqfn, Squealer.SquealerObject obj)
        {
            System.Xml.XmlDocument FixedXmlRet = default;

            var OutputXml = new System.Xml.XmlDocument();

            var InputXml = new System.Xml.XmlDocument();

            InputXml.Load(fqfn);

            System.Xml.XmlElement InRoot = (System.Xml.XmlElement)InputXml.SelectSingleNode(Squealer.My.MyProject.Application.Info.ProductName);

            OutputXml.AppendChild(OutputXml.CreateXmlDeclaration("1.0", "us-ascii", null));

            // Header
            OutputXml.AppendChild(OutputXml.CreateComment(" Flags example: \"x;exclude from project|r;needs refactoring\" (recommend single-character flags) "));

            var OutRoot = OutputXml.CreateElement(Squealer.My.MyProject.Application.Info.ProductName);
            OutputXml.AppendChild(OutRoot);

            OutRoot.SetAttribute("Type", obj.Type.LongType.ToString());
            OutRoot.SetAttribute("Flags", obj.Flags);
            OutRoot.SetAttribute("WithOptions", obj.WithOptions);

            // Pre-Code.
            var OutPreCode = OutputXml.CreateElement("PreCode");
            var CDataPreCode = OutputXml.CreateCDataSection(""); // CData disables the XML parser so that special characters can exist in the inner text.

            OutRoot.AppendChild(OutputXml.CreateComment(" Optional T-SQL to execute before the main object is created. "));
            OutRoot.AppendChild(OutPreCode);

            string InPreCode = string.Empty;
            try
            {
                InPreCode = InRoot.SelectSingleNode("PreCode").InnerText;
            }
            catch (Exception ex)
            {
                InPreCode = "";
            }

            CDataPreCode.InnerText = string.Concat(Constants.vbCrLf, Constants.vbCrLf, InPreCode.Trim(), Constants.vbCrLf, Constants.vbCrLf);
            OutPreCode.AppendChild(CDataPreCode);


            // Comments.
            // -- comment help--
            System.Xml.XmlComment CommentHelp = null;

            switch (obj.Type.LongType)
            {
                case Squealer.SquealerObjectType.eType.StoredProcedure:
                    {
                        CommentHelp = OutputXml.CreateComment(" Describe the purpose of this procedure, the return values, and any difficult concepts. ");
                        break;
                    }
                case Squealer.SquealerObjectType.eType.InlineTableFunction:
                case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                    {
                        CommentHelp = OutputXml.CreateComment(" Describe the output of this view and any difficult concepts. ");
                        break;
                    }
                case Squealer.SquealerObjectType.eType.ScalarFunction:
                    {
                        CommentHelp = OutputXml.CreateComment(" Describe the output of this scalar function and any difficult concepts. ");
                        break;
                    }
                case Squealer.SquealerObjectType.eType.View:
                    {
                        CommentHelp = OutputXml.CreateComment(" Describe the output of this view and any difficult concepts. ");
                        break;
                    }

                default:
                    {
                        throw new Exception("Missing or invalid object type. Check: <Squealer Type=\"???\"> in " + fqfn);
                    }
            }
            OutRoot.AppendChild(CommentHelp);

            // -- actual comment--
            var OutComments = OutputXml.CreateElement("Comments");
            var CDataComment = OutputXml.CreateCDataSection(""); // CData disables the XML parser so that special characters can exist in the inner text.
            OutRoot.AppendChild(OutComments);

            try
            {
                CDataComment.InnerText = InRoot.SelectSingleNode("Comments").InnerText.Replace("/*", string.Empty).Replace("*/", string.Empty);
            }
            catch (Exception ex)
            {
                CDataComment.InnerText = string.Empty;
            }

            CDataComment.InnerText = string.Concat(Constants.vbCrLf, Constants.vbCrLf, CDataComment.InnerText.Trim(), Constants.vbCrLf, Constants.vbCrLf);

            OutComments.AppendChild(CDataComment);


            // Parameters.
            if (!(obj.Type.LongType == Squealer.SquealerObjectType.eType.View))
            {

                var OutParameters = OutputXml.CreateElement("Parameters");
                OutRoot.AppendChild(OutParameters);

                var InParameters = GetParameters(InputXml);

                if (InParameters.Rows.Count == 0)
                {
                    OutParameters.AppendChild(OutputXml.CreateComment("<Parameter Name=\"MyParameter\" Type=\"varchar(50)\" " + Interaction.IIf(obj.Type.LongType == Squealer.SquealerObjectType.eType.StoredProcedure, "Output=\"False\" ", string.Empty).ToString() + "DefaultValue=\"\" Comments=\"\" />"));
                }
                else
                {
                    foreach (DataRow InParameter in InParameters.Select())
                    {
                        var OutParameter = OutputXml.CreateElement("Parameter");
                        OutParameter.SetAttribute("Name", InParameter["Name"].ToString());
                        OutParameter.SetAttribute("Type", InParameter["Type"].ToString());
                        if (obj.Type.LongType == Squealer.SquealerObjectType.eType.StoredProcedure)
                        {
                            OutParameter.SetAttribute("Output", InParameter["Output"].ToString());
                        }
                        // If obj.Type.LongType = SquealerObjectType.eType.ScalarFunction Then
                        OutParameter.SetAttribute("ReadOnly", InParameter["ReadOnly"].ToString());
                        // End If
                        OutParameter.SetAttribute("DefaultValue", InParameter["DefaultValue"].ToString());
                        OutParameter.SetAttribute("Comments", InParameter["Comments"].ToString());
                        OutParameters.AppendChild(OutParameter);
                    }
                }

            }

            // Returns.
            if (obj.Type.LongType == Squealer.SquealerObjectType.eType.ScalarFunction)
            {
                OutRoot.AppendChild(OutputXml.CreateComment(" Define the data type For @Result, your scalar Return variable. "));
                var OutReturns = OutputXml.CreateElement("Returns");
                OutRoot.AppendChild(OutReturns);
                string Returns = null;
                try
                {
                    Returns = ((System.Xml.XmlElement)InRoot.SelectSingleNode("Returns")).GetAttribute("Type");
                }
                catch (Exception ex)
                {
                    Returns = string.Empty;
                }
                OutReturns.SetAttribute("Type", Returns);
            }

            // Table.
            if (obj.Type.LongType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction || obj.Type.LongType == Squealer.SquealerObjectType.eType.View)
            {

                if (obj.Type.LongType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction)
                {
                    OutRoot.AppendChild(OutputXml.CreateComment(" Define the column(s) for @TableValue, your table-valued return variable. "));
                }
                else
                {
                    OutRoot.AppendChild(OutputXml.CreateComment(" Define the column(s) to return from this view. "));
                }
                var OutTable = OutputXml.CreateElement("Table");
                OutRoot.AppendChild(OutTable);

                if (obj.Type.LongType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction)
                {
                    bool Clustered = false;
                    try
                    {
                        Clustered = Conversions.ToBoolean(((System.Xml.XmlElement)InRoot.SelectSingleNode("Table")).GetAttribute("PrimaryKeyClustered"));
                    }
                    catch (Exception ex)
                    {
                        Clustered = false;
                    }
                    OutTable.SetAttribute("PrimaryKeyClustered", Clustered.ToString());
                }

                var InColumns = GetColumns(InputXml);

                if (InColumns.Rows.Count == 0)
                {
                    // Create a dummy/example column.
                    if (obj.Type.LongType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction)
                    {
                        var OutColumn = OutputXml.CreateElement("Column");
                        OutColumn.SetAttribute("Name", "MyColumn");
                        OutColumn.SetAttribute("Type", "varchar(50)");
                        OutColumn.SetAttribute("Nullable", "False");
                        OutColumn.SetAttribute("Identity", "False");
                        OutColumn.SetAttribute("IncludeInPrimaryKey", "False");
                        OutColumn.SetAttribute("Comments", "");
                        OutTable.AppendChild(OutColumn);
                    }
                    else
                    {
                        OutTable.AppendChild(OutputXml.CreateComment("<Column Name=\"MyColumn\" Comments=\"\" />"));
                    }
                }
                else
                {
                    foreach (DataRow InColumn in InColumns.Select())
                    {
                        bool Nullable = Conversions.ToBoolean(InColumn["Nullable"]);
                        bool Identity = Conversions.ToBoolean(InColumn["Identity"]);
                        bool IncludeInPrimaryKey = Conversions.ToBoolean(InColumn["IncludeInPrimaryKey"]);
                        string Type = InColumn["Type"].ToString();
                        if (Type.IndexOf(" Not null") > 0)
                        {
                            Nullable = false;
                            Type = Type.Replace(" Not null", string.Empty);
                        }
                        else if (Type.IndexOf(" null") > 0)
                        {
                            Nullable = true;
                            Type = Type.Replace(" null", string.Empty);
                        }
                        var OutColumn = OutputXml.CreateElement("Column");
                        OutColumn.SetAttribute("Name", InColumn["Name"].ToString());
                        if (obj.Type.LongType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction)
                        {
                            OutColumn.SetAttribute("Type", Type);
                            OutColumn.SetAttribute("Nullable", Nullable.ToString());
                            OutColumn.SetAttribute("Identity", Identity.ToString());
                            OutColumn.SetAttribute("IncludeInPrimaryKey", IncludeInPrimaryKey.ToString());
                        }
                        OutColumn.SetAttribute("Comments", InColumn["Comments"].ToString());
                        OutTable.AppendChild(OutColumn);
                    }
                }

            }

            // Code.
            var OutCode = OutputXml.CreateElement("Code");
            var CDataCode = OutputXml.CreateCDataSection(""); // CData disables the XML parser so that special characters can exist in the inner text.

            OutRoot.AppendChild(OutCode);

            string InCode = string.Empty;
            try
            {
                InCode = InRoot.SelectSingleNode("Code").InnerText;
            }
            catch (Exception ex)
            {
                InCode = string.Empty;
            }

            if (string.IsNullOrEmpty(InCode.Trim()))
            {
                InCode = "/***********************************************************************" + Constants.vbCrLf + Constants.vbTab + "Comments." + Constants.vbCrLf + "***********************************************************************/" + Constants.vbCrLf + Constants.vbCrLf;


                switch (obj.Type.LongType)
                {
                    case Squealer.SquealerObjectType.eType.InlineTableFunction:
                        {
                            InCode += "select 'hello world! love, ``this``' as [MyColumn]";
                            break;
                        }
                    case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                        {
                            InCode += "insert @TableValue select 'hello world! love, ``this``'";
                            break;
                        }
                    case Squealer.SquealerObjectType.eType.ScalarFunction:
                        {
                            InCode += "set @Result = 'hello world! love, ``this``'";
                            break;
                        }
                    case Squealer.SquealerObjectType.eType.StoredProcedure:
                        {
                            InCode += "select 'hello world! love, ``this``'" + Constants.vbCrLf + Constants.vbCrLf + Constants.vbCrLf + "--optional (see https://docs.microsoft.com/en-us/sql/t-sql/language-elements/return-transact-sql?view=sql-server-ver15)" + Constants.vbCrLf + "--set @Squealer_ReturnValue = [ integer_expression ]";





                            break;
                        }
                    case Squealer.SquealerObjectType.eType.View:
                        {
                            InCode += "select 'hello world! love, ``this``' as hello";
                            break;
                        }
                }



            }

            CDataCode.InnerText = string.Concat(Constants.vbCrLf, Constants.vbCrLf, InCode.Trim(), Constants.vbCrLf, Constants.vbCrLf);
            OutCode.AppendChild(CDataCode);


            // Users.
            var OutUsers = OutputXml.CreateElement("Users");
            OutRoot.AppendChild(OutUsers);
            DataTable InUsers;
            if (ApplyDefaultUsers)
            {
                InUsers = GetDefaultUsers(new System.IO.FileInfo(fqfn).DirectoryName);
            }
            else
            {
                InUsers = GetUsers(InputXml);
            }

            if (InUsers.Rows.Count == 0)
            {
                OutUsers.AppendChild(OutputXml.CreateComment(" <User Name=\"MyUser\"/> "));
            }
            else
            {
                foreach (DataRow User in InUsers.Select("", "Name asc"))
                {
                    var OutUser = OutputXml.CreateElement("User");
                    OutUser.SetAttribute("Name", User["Name"].ToString().Trim());
                    OutUsers.AppendChild(OutUser);
                }
            }


            // Post-Code.
            var OutPostCode = OutputXml.CreateElement("PostCode");
            var CDataPostCode = OutputXml.CreateCDataSection(""); // CData disables the XML parser so that special characters can exist in the inner text.

            OutRoot.AppendChild(OutputXml.CreateComment(" Optional T-SQL to execute after the main object is created. "));
            OutRoot.AppendChild(OutPostCode);

            string InPostCode = string.Empty;
            try
            {
                InPostCode = InRoot.SelectSingleNode("PostCode").InnerText;
            }
            catch (Exception ex)
            {
                InPostCode = "";
            }

            CDataPostCode.InnerText = string.Concat(Constants.vbCrLf, Constants.vbCrLf, InPostCode.Trim(), Constants.vbCrLf, Constants.vbCrLf);
            OutPostCode.AppendChild(CDataPostCode);


            FixedXmlRet = OutputXml;
            return FixedXmlRet;

        }


        // Fix a root file and replace the original.
        private static bool ConvertXmlFile(string fqfn, Squealer.SquealerObjectType.eType oType)
        {

            var obj = new Squealer.SquealerObject(fqfn);
            obj.Type.LongType = oType;
            var NewXml = FixedXml(false, fqfn, obj); // Fix it.

            return IsXmlReplaced(fqfn, NewXml);

        }

        // Fix a root file and replace the original.
        private static bool RepairXmlFile(bool IsNew, string fqfn)
        {
            var NewXml = FixedXml(IsNew, fqfn); // Fix it.
            return IsXmlReplaced(fqfn, NewXml);
        }

        private static bool IsXmlReplaced(string existingfilename, System.Xml.XmlDocument newxml)
        {

            var ExistingXml = new System.Xml.XmlDocument();
            ExistingXml.Load(existingfilename);

            bool different = false;

            if (!((newxml.InnerXml ?? "") == (ExistingXml.InnerXml ?? "")))
            {
                // Is XML different?
                newxml.Save(existingfilename);
                different = true;
            }
            else
            {
                // XML is the same, but is whitespace different?
                string tempfilename = System.IO.Path.GetTempFileName();
                newxml.Save(tempfilename);
                if (!((System.IO.File.ReadAllText(tempfilename) ?? "") == (System.IO.File.ReadAllText(existingfilename) ?? "")))
                {
                    Squealer.My.MyProject.Computer.FileSystem.MoveFile(tempfilename, existingfilename, true);
                    different = true;
                }
            }

            return different;

        }

        // Create a new proc or function.

        private static string CreateNewFile(string WorkingFolder, Squealer.SquealerObjectType.eType FileType, string filename)
        {
            return CreateNewFile(WorkingFolder, FileType, filename, (Squealer.ParameterCollection)null, null, null);
        }

        private static string CreateNewFile(string WorkingFolder, Squealer.SquealerObjectType.eType FileType, string filename, Squealer.ParameterCollection Parameters, string definition, List<string> userlist)
        {
            string CreateNewFileRet = default;

            string Template = string.Empty;
            switch (FileType)
            {
                case Squealer.SquealerObjectType.eType.InlineTableFunction:
                    {
                        Template = Squealer.My.Resources.Resources.IF_Template;
                        break;
                    }
                case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                    {
                        Template = Squealer.My.Resources.Resources.TF_Template;
                        break;
                    }
                case Squealer.SquealerObjectType.eType.ScalarFunction:
                    {
                        Template = Squealer.My.Resources.Resources.FN_Template;
                        break;
                    }
                case Squealer.SquealerObjectType.eType.StoredProcedure:
                    {
                        Template = Squealer.My.Resources.Resources.P_Template;
                        break;
                    }
                case Squealer.SquealerObjectType.eType.View:
                    {
                        Template = Squealer.My.Resources.Resources.V_Template;
                        break;
                    }
            }
            Template = Template.Replace("{RootType}", FileType.ToString()).Replace("{THIS}", Squealer.Constants.MyThis);

            bool IsNew = true;


            if (Parameters is null)
            {
                Template = Template.Replace("{ReturnDataType}", "varchar(100)");
            }
            else
            {

                IsNew = false;

                if (FileType == Squealer.SquealerObjectType.eType.ScalarFunction)
                {
                    Template = Template.Replace("{ReturnDataType}", Parameters.ReturnType().Type);
                }

                string parms = string.Empty;
                foreach (Squealer.ParameterClass p in Parameters.Items())
                    parms += string.Format("<Parameter Name=\"{0}\" Type=\"{1}\" Output=\"{2}\" />", p.Name, p.Type, p.IsOutput.ToString());
                Template = Template.Replace("<!--Parameters-->", parms);
            }

            if (userlist is not null)
            {

                IsNew = false;

                string users = string.Empty;
                foreach (string s in userlist)
                    users += string.Format("<User Name=\" {0}\"/>", s);
                Template = Template.Replace("<Users/>", string.Format("<Users>{0}</Users>", users));

            }

            // Did user forget the "-" prefix before the object type switch? ex: tf instead of -tf
            foreach (string s in Enum.GetNames(typeof(Squealer.SquealerObjectType.eShortType)))
            {
                if (filename.ToLower().StartsWith(s.ToLower() + " "))
                {
                    Squealer.Textify.SayError("Did you mean:  " + eCommandType.@new.ToString() + " -" + s + " " + filename.Remove(0, s.Length + 1), Squealer.Textify.eSeverity.warning);
                }
            }

            // Make sure all new programs have a schema.
            if (!filename.Contains("."))
            {
                filename = string.Concat("dbo.", filename);
            }

            // Dim fqTemp As String = My.Computer.FileSystem.GetTempFileName
            string fqTarget = WorkingFolder + @"\" + filename + Squealer.Constants.SquealerFileExtension;

            if (definition is not null)
            {
                Template = Template.Replace("<Code/>", string.Format("<Code><![CDATA[{0}]]></Code>", definition));
            }

            // Careful not to overwrite existing file.
            if (System.IO.File.Exists(fqTarget))
            {
                if (IsNew)
                {
                    Squealer.Textify.SayError("File already exists.");
                }
                CreateNewFileRet = string.Empty;
            }
            else
            {
                Squealer.My.MyProject.Computer.FileSystem.WriteAllText(fqTarget, Template, false, System.Text.Encoding.ASCII);
                RepairXmlFile(IsNew, fqTarget);
                if (IsNew)
                {
                    Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, "OK");
                    Squealer.Textify.WriteLine(" (" + filename + ")");
                }
                CreateNewFileRet = fqTarget;
            }

            if (IsNew)
            {
                Squealer.Textify.SayNewLine();
            }

            return CreateNewFileRet;

        }


        #endregion

        #region  Proc Generation 

        // Expand one root file.
        private static string ExpandIndividual(System.IO.FileInfo info, DataTable StringReplacements, BatchParametersClass bp, int cur, int tot, bool printsteps)
        {
            string ExpandIndividualRet = default;

            var oType = Squealer.SquealerObjectType.Eval(XmlGetObjectType(info.FullName));
            string RootName = info.Name.Replace(Squealer.Constants.SquealerFileExtension, "");

            var InXml = FixedXml(false, info.FullName);
            System.Xml.XmlElement InRoot = (System.Xml.XmlElement)InXml.SelectSingleNode(Squealer.My.MyProject.Application.Info.ProductName);
            string Block = null;

            var CodeBlocks = new List<string>();


            // Pre-Code
            if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test))
            {
                string InPreCode = "";
                if (printsteps)
                {
                    InPreCode = string.Format("print '{2}/{3} creating {0}, {1}'", Squealer.Constants.MyThis, oType.ToString(), (object)cur, (object)tot) + Constants.vbCrLf + "go" + Constants.vbCrLf;
                }
                try
                {
                    InPreCode += InRoot.SelectSingleNode("PreCode").InnerText;
                }
                catch (Exception ex)
                {
                }

                if (!string.IsNullOrWhiteSpace(InPreCode))
                {
                    InPreCode = Constants.vbCrLf + "-- additional code to execute after " + oType.ToString() + " is created" + Constants.vbCrLf + InPreCode;
                    CodeBlocks.Add(InPreCode);
                }

            }


            // Drop 
            if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test) && !(bp.OutputMode == BatchParametersClass.eOutputMode.alter))
            {
                CodeBlocks.Add(Squealer.My.Resources.Resources.DropAny.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Schema}", SchemaName(RootName)).ToString());
            }

            // Comments
            string OutComments = null;
            try
            {
                OutComments = InRoot.SelectSingleNode("Comments").InnerText.Replace("/*", string.Empty).Replace("*/", string.Empty);
            }
            catch (Exception ex)
            {
                OutComments = string.Empty;
            }
            Block = Squealer.My.Resources.Resources.Comment.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Comments}", OutComments).Replace("{Schema}", SchemaName(RootName)).Replace("{SquealerVersion}", "Squealer " + Squealer.My.MyProject.Application.Info.Version.ToString());

            // Create
            if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test))
            {
                string SqlCreate = string.Empty;
                switch (oType)
                {
                    case Squealer.SquealerObjectType.eType.StoredProcedure:
                        {
                            SqlCreate = Squealer.My.Resources.Resources.P_Create;
                            break;
                        }
                    case Squealer.SquealerObjectType.eType.ScalarFunction:
                    case Squealer.SquealerObjectType.eType.InlineTableFunction:
                    case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                        {
                            SqlCreate = Squealer.My.Resources.Resources.FN_Create;
                            break;
                        }
                    case Squealer.SquealerObjectType.eType.View:
                        {
                            SqlCreate = Squealer.My.Resources.Resources.V_Create;
                            break;
                        }
                }
                if (bp.OutputMode == BatchParametersClass.eOutputMode.alter)
                {
                    SqlCreate = SqlCreate.Replace("create", "alter");
                }
                Block += SqlCreate.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Schema}", SchemaName(RootName));
            }

            // Parameters
            var InParameters = GetParameters(InXml);
            int ParameterCount = 0;
            var DeclareList = new ArrayList();
            var SetList = new ArrayList();
            string OutputParameters = string.Empty;
            string ErrorLogParameters = string.Empty;

            foreach (DataRow Parameter in InParameters.Select())
            {

                string def = null;

                if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                {

                    def = "declare @" + Parameter["Name"].ToString() + " " + Parameter["Type"].ToString();
                    if (string.IsNullOrEmpty(Parameter["DefaultValue"].ToString()))
                    {
                        def = def + " = ";
                    }
                    else
                    {
                        def = def + " = " + Parameter["DefaultValue"].ToString();
                    }
                    def = def + ";";
                    if (!string.IsNullOrEmpty(Parameter["Comments"].ToString()))
                    {
                        def = def + " -- " + Parameter["Comments"].ToString();
                    }
                }

                else
                {

                    // def = ""
                    if (ParameterCount == 0) // <InParameters.Rows.Count Then
                    {
                        def = "";
                    }
                    else
                    {
                        def = ",";
                    }

                    // Write parameters as actual parameters.
                    ParameterCount += 1;
                    def = def + "@" + Parameter["Name"].ToString() + " " + Parameter["Type"].ToString();
                    if (!string.IsNullOrEmpty(Parameter["DefaultValue"].ToString()))
                    {
                        def = def + " = " + Parameter["DefaultValue"].ToString();
                    }
                    if ((Parameter["Output"].ToString() ?? "") == (bool.TrueString ?? ""))
                    {
                        def = def + " output";
                    }
                    if ((Parameter["ReadOnly"].ToString() ?? "") == (bool.TrueString ?? ""))
                    {
                        def = def + " readonly";
                    }
                    if (!string.IsNullOrEmpty(Parameter["Comments"].ToString()))
                    {
                        def = def + " -- " + Parameter["Comments"].ToString();
                    }
                    // Write out error logging section.
                    if (Parameter["Type"].ToString().ToLower().Contains("max") || bool.Parse(Parameter["ReadOnly"].ToString()))
                    {
                        string whynot = Constants.vbCrLf + Constants.vbTab + Constants.vbTab + string.Format("--parameter @{0} cannot be logged due to its 'max' or 'readonly' definition", Parameter["Name"].ToString());
                        ErrorLogParameters += whynot;
                    }
                    else
                    {
                        ErrorLogParameters += Constants.vbCrLf + Squealer.My.Resources.Resources.P_ErrorParameter.Replace("{ErrorParameterNumber}", ParameterCount.ToString()).Replace("{ErrorParameterName}", Parameter["Name"].ToString());
                    }

                }

                DeclareList.Add(def);

            }
            foreach (string s in DeclareList)
                Block = Block + Constants.vbCrLf + s;
            foreach (string s in SetList)
                Block = Block + Constants.vbCrLf + s;


            // Table (View)
            if (oType == Squealer.SquealerObjectType.eType.View)
            {

                var InColumns = GetColumns(InXml);

                if (InColumns.Rows.Count > 0 && !(bp.OutputMode == BatchParametersClass.eOutputMode.test))
                {

                    Block += Constants.vbCrLf + "(" + Constants.vbCrLf;

                    int ColumnCount = 0;

                    foreach (DataRow Column in InColumns.Select())
                    {

                        string c = string.Empty;
                        if (ColumnCount > 0)
                        {
                            c = ",";
                        }

                        ColumnCount += 1;

                        c += string.Format("[{0}]", Column["Name"].ToString());
                        if (!string.IsNullOrEmpty(Column["Comments"].ToString()))
                        {
                            c = c + " -- " + Column["Comments"].ToString();
                        }

                        Block += c + Constants.vbCrLf;

                    }

                    Block += Constants.vbCrLf + ")" + Constants.vbCrLf;

                }

            }



            // Table (MultiStatementTableFunction)
            if (oType == Squealer.SquealerObjectType.eType.MultiStatementTableFunction)
            {

                if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                {
                    Block = Block + Squealer.My.Resources.Resources.TF_TableTest;
                }
                else
                {
                    Block = Block + Squealer.My.Resources.Resources.TF_Table;
                }

                string PrimaryKey = string.Empty; // If this never gets filled, then there is no primary key.
                bool Clustered = Conversions.ToBoolean(((System.Xml.XmlElement)InRoot.SelectSingleNode("Table")).GetAttribute("PrimaryKeyClustered"));

                var InColumns = GetColumns(InXml);
                int ColumnCount = 0;
                foreach (DataRow Column in InColumns.Select())
                {

                    string c = string.Empty;
                    if (ColumnCount > 0)
                    {
                        c = ",";
                    }

                    ColumnCount += 1;

                    c += string.Format("[{0}]", Column["Name"].ToString());
                    c += " " + Column["Type"].ToString() + " ";
                    if (!Conversions.ToBoolean(Column["Nullable"]))
                    {
                        c += "Not ";
                    }
                    c += "null";
                    if (Conversions.ToBoolean(Column["Identity"]))
                    {
                        c += " identity";
                    }
                    if (!string.IsNullOrEmpty(Column["Comments"].ToString()))
                    {
                        c = c + " -- " + Column["Comments"].ToString();
                    }

                    Block += c + Constants.vbCrLf;

                    if ((Column["IncludeInPrimaryKey"].ToString() ?? "") == (bool.TrueString ?? ""))
                    {
                        if (PrimaryKey.Length > 0)
                        {
                            PrimaryKey += ",";
                        }
                        PrimaryKey += Column["Name"].ToString();
                    }
                }

                if (PrimaryKey.Length > 0)
                {
                    Block += "primary key " + Interaction.IIf(Clustered, "clustered ", "").ToString() + "(" + PrimaryKey + ")";
                }

            }



            string InCode = string.Empty;
            try
            {
                InCode = InRoot.SelectSingleNode("Code").InnerText;
            }
            catch (Exception ex)
            {
            }
            bool NoMagic = InCode.Trim().ToLower().StartsWith("--nomagic");


            // Begin
            string BeginBlock = null;
            switch (oType)
            {
                case Squealer.SquealerObjectType.eType.StoredProcedure:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            BeginBlock = Squealer.My.Resources.Resources.P_BeginTest;
                        }
                        else if (NoMagic)
                        {
                            BeginBlock = Squealer.My.Resources.Resources.P_BeginNoMagic;
                        }
                        else
                        {
                            BeginBlock = Squealer.My.Resources.Resources.P_Begin;
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.ScalarFunction:
                    {
                        string Returns = null;
                        Returns = ((System.Xml.XmlElement)InRoot.SelectSingleNode("Returns")).GetAttribute("Type");
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            BeginBlock = Squealer.My.Resources.Resources.FN_BeginTest.Replace("{ReturnDataType}", Returns);
                        }
                        else
                        {
                            BeginBlock = Squealer.My.Resources.Resources.FN_Begin.Replace("{ReturnDataType}", Returns);
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.InlineTableFunction:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            BeginBlock = string.Empty;
                        }
                        else
                        {
                            BeginBlock = Squealer.My.Resources.Resources.IF_Begin;
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            BeginBlock = Squealer.My.Resources.Resources.Tf_BeginTest;
                        }
                        else
                        {
                            BeginBlock = Squealer.My.Resources.Resources.Tf_Begin;
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.View:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            BeginBlock = string.Empty;
                        }
                        else
                        {
                            BeginBlock = Squealer.My.Resources.Resources.V_Begin;
                        }

                        break;
                    }
            }


            var obj = new Squealer.SquealerObject(info.FullName);
            string WithOptions = obj.WithOptions;
            if (bp.OutputMode == BatchParametersClass.eOutputMode.encrypt)
            {
                if (string.IsNullOrWhiteSpace(obj.WithOptions))
                {
                    WithOptions = "encryption";
                }
                else if (!WithOptions.ToLower().Contains("encryption"))
                {
                    WithOptions = WithOptions + ",encryption";
                }
            }

            if (string.IsNullOrWhiteSpace(WithOptions))
            {
                BeginBlock = BeginBlock.Replace("{WithOptions}", string.Empty);
            }
            else
            {
                BeginBlock = BeginBlock.Replace("{WithOptions}", "with " + WithOptions);
            }

            Block += BeginBlock;

            // Code
            Block += InCode;

            // End
            switch (oType)
            {
                case Squealer.SquealerObjectType.eType.StoredProcedure:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            Block += Squealer.My.Resources.Resources.P_EndTest;
                        }
                        else if (NoMagic)
                        {
                            Block += Squealer.My.Resources.Resources.P_EndNoMagic.Replace("{Parameters}", ErrorLogParameters);
                        }
                        else
                        {
                            Block += Squealer.My.Resources.Resources.P_End.Replace("{Parameters}", ErrorLogParameters);
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.ScalarFunction:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            Block = Block + Squealer.My.Resources.Resources.FN_EndTest;
                        }
                        else
                        {
                            Block = Block + Squealer.My.Resources.Resources.FN_End;
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.MultiStatementTableFunction:
                    {
                        if (bp.OutputMode == BatchParametersClass.eOutputMode.test)
                        {
                            Block = Block + Squealer.My.Resources.Resources.TF_EndTest;
                        }
                        else
                        {
                            Block = Block + Squealer.My.Resources.Resources.TF_End;
                        }

                        break;
                    }
                case Squealer.SquealerObjectType.eType.View:
                    {
                        break;
                    }
                    // nothing to add
            }

            // Save the block.
            CodeBlocks.Add(Block);

            if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test) && !(bp.OutputMode == BatchParametersClass.eOutputMode.alter))
            {

                Block = string.Empty;

                // Grant Execute
                var InUsers = GetUsers(InXml);

                if (InUsers.Rows.Count > 0)
                {
                    Block = string.Format("if object_id('``this``','{0}') is not null", Squealer.SquealerObjectType.ToShortType(oType));
                    Block += Constants.vbCrLf + "begin";
                    foreach (DataRow User in InUsers.Select("", "Name asc"))
                    {
                        string GrantStatement;
                        if (oType == Squealer.SquealerObjectType.eType.StoredProcedure || oType == Squealer.SquealerObjectType.eType.ScalarFunction)
                        {
                            GrantStatement = Squealer.My.Resources.Resources.GrantExecute;
                        }
                        else
                        {
                            GrantStatement = Squealer.My.Resources.Resources.GrantSelect;
                        }
                        Block = Block + Constants.vbCrLf + GrantStatement.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{User}", User["Name"].ToString()).Replace("{Schema}", SchemaName(RootName));
                    }
                    Block += Constants.vbCrLf + "end" + Constants.vbCrLf + "else" + Constants.vbCrLf + "begin" + Constants.vbCrLf + string.Format("print 'Permissions not granted on {0}.';", Squealer.Constants.MyThis) + Constants.vbCrLf + Interaction.IIf(MySettings.TrackFailedItems, string.Format("insert ##RetryFailedSquealerItems (ProcName) values ('{0}');", Squealer.Constants.MyThis), "").ToString() + Constants.vbCrLf + "end";






                }

                if (!string.IsNullOrEmpty(Block))
                {
                    CodeBlocks.Add(Block);
                }

            }


            // Post-Code
            if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test))
            {
                string InPostCode = string.Empty;
                try
                {
                    InPostCode = InRoot.SelectSingleNode("PostCode").InnerText;
                }
                catch (Exception ex)
                {
                }

                if (!string.IsNullOrWhiteSpace(InPostCode))
                {

                    InPostCode = Constants.vbCrLf + "-- additional code to execute after " + oType.ToString() + " is created" + Constants.vbCrLf + string.Format("if object_id('``this``','{0}') is not null", Squealer.SquealerObjectType.ToShortType(oType)) + Constants.vbCrLf + "begin" + Constants.vbCrLf + InPostCode + Constants.vbCrLf + "end" + Constants.vbCrLf + "else print 'PostCode not executed.'";





                    CodeBlocks.Add(InPostCode);
                }

            }


            // Now add all the GOs
            ExpandIndividualRet = string.Empty;
            foreach (string s in CodeBlocks)
            {
                ExpandIndividualRet += s + Constants.vbCrLf;
                if (!(bp.OutputMode == BatchParametersClass.eOutputMode.test))
                {
                    ExpandIndividualRet += "go" + Constants.vbCrLf;
                }
            }



            // Add top/bottom markers
            ExpandIndividualRet = SpitDashes(string.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)), "<BOF>") + Constants.vbCrLf + ExpandIndividualRet + Constants.vbCr + SpitDashes(string.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)), "<EOF>") + Constants.vbCrLf + Constants.vbCrLf;


            // Do string replacements.
            ExpandIndividualRet = ExpandIndividualRet.Replace(Squealer.Constants.MyThis, string.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)));
            foreach (DataRow Replacement in StringReplacements.Select()) // .Select("")
                ExpandIndividualRet = ExpandIndividualRet.Replace(Replacement["Original"].ToString(), Replacement["Replacement"].ToString());
            return ExpandIndividualRet;


        }

        #endregion

        #region  Ez 

        private static string EzSqlPath()
        {
            return Squealer.My.Configger.AppDataFolder + @"\ezscript.sql";
        }

        private static string EzBinFilename()
        {
            return "ezscript.bin";
        }

        private static string EzBinPath()
        {
            return Squealer.My.Configger.AppDataFolder + @"\" + EzBinFilename();
        }
        private static string EzNewBinPath()
        {
            return EzBinPath() + ".new";
        }

        private static string EzText(bool TraceIt)
        {
            string s;
            try
            {
                try
                {
                    // attempt to read plain text file from disk because local customization takes precedence
                    s = System.IO.File.ReadAllText(EzSqlPath());
                    if (TraceIt)
                    {
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("reading {0}", EzSqlPath()));
                    }
                }
                catch (Exception ex)
                {
                    // attempt to read encrypted file from disk
                    s = Squealer.Misc.DecryptedString(System.IO.File.ReadAllBytes(EzBinPath()));
                    if (TraceIt)
                    {
                        Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("reading {0}", EzBinPath()));
                    }
                }
            }
            catch (Exception ex)
            {
                // default text
                s = Squealer.My.Resources.Resources.EzObjects;
                if (TraceIt)
                {
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("reading resource file"));
                }
            }
            return Constants.vbCrLf + s.Trim() + Constants.vbCrLf;
        }

        private static void EzExtractSqlToFile()
        {
            Squealer.My.MyProject.Computer.FileSystem.WriteAllText(EzSqlPath(), EzText(true), false);
            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("writing {0}", EzSqlPath()), 0, new Squealer.Textify.ColorScheme(ConsoleColor.Cyan));
            Console.WriteLine();
        }

        private static void EzConvertSqlToNewBin()
        {
            Squealer.My.MyProject.Computer.FileSystem.WriteAllBytes(EzNewBinPath(), Squealer.Misc.EncryptedBytes(EzText(true)), false);
            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, string.Format("writing {0}", EzNewBinPath()), 0, new Squealer.Textify.ColorScheme(ConsoleColor.Cyan));
            Console.WriteLine();
        }

        #endregion

        #region  Misc 

        private static void ResetWindowSize()
        {
            try
            {
                // This fails if the console was in full-screen mode at previous exit
                Console.SetWindowSize(Squealer.My.Configger.LoadSetting("WindowWidth", 130), Squealer.My.Configger.LoadSetting("WindowHeight", 30));
            }
            catch (Exception ex)
            {
            }
            Console.BufferWidth = Console.WindowWidth;
        }

        private static void SaveWindowSize()
        {
            Squealer.My.Configger.SaveSetting("WindowWidth", Console.WindowWidth);
            Squealer.My.Configger.SaveSetting("WindowHeight", Console.WindowHeight);
        }

        private static string SpitDashes(string s, string marker)
        {
            return new string('-', 5) + " " + s + " " + new string('-', 100 - s.Length) + " " + marker;
        }

        private static void BracketCheck(string s)
        {
            if (s.Contains(Conversions.ToString('[')) || s.Contains(Conversions.ToString(']')))
            {
                throw new Exception("Illegal square bracket character in filename: " + s);
            }
        }

        private static void DisplayAboutInfo(bool ShowWhatsNew, bool ShowFullHistory)
        {

            Console.WriteLine(string.Format("{0} v.{1}", Squealer.My.MyProject.Application.Info.Title, Squealer.My.MyProject.Application.Info.Version));
            Console.WriteLine(Squealer.My.MyProject.Application.Info.Copyright);
            Console.WriteLine();

            var v = new Squealer.VersionCheck();
            v.DisplayVersionCheckResults(MySettings.MediaSourceUrl, MySettings.IsDefaultMediaSource);
            Console.WriteLine();

            if (ShowWhatsNew)
            {
                Squealer.Textify.WriteLine("New in this release:", new Squealer.Textify.ColorScheme(ConsoleColor.Cyan));
                Console.WriteLine();
                Console.WriteLine(Squealer.VersionCheck.WhatsNew());
                Console.WriteLine();
            }

            if (ShowFullHistory)
            {
                Squealer.Textify.WriteLine("Change log:", new Squealer.Textify.ColorScheme(ConsoleColor.Cyan));
                Console.WriteLine();
                Console.WriteLine(Squealer.VersionCheck.ChangeLog());
                Console.WriteLine();
            }

        }

        private static List<string> OpenExplorer(string wildcard, string WorkingFolder)
        {
            var f = new OpenFileDialog();
            f.InitialDirectory = WorkingFolder;
            f.FileName = wildcard;
            f.Multiselect = true;
            var s = new List<string>();
            if (!(f.ShowDialog() == DialogResult.Cancel))
            {
                s.AddRange(f.FileNames);
            }
            return s;
        }

        private static void ThrowErrorIfOverFileLimit(int limit, int n, bool OverrideSafety)
        {
            if (n > limit && !OverrideSafety)
            {
                throw new Exception(string.Format("Too many files. Limit {0}, found {1}.", limit.ToString(), n.ToString()));
            }
        }

        // Clear the keyboard input buffer silently.
        private static void ClearKeyboard()
        {
            while (Console.KeyAvailable)
                Console.ReadKey(true);
        }

        // Return the schema name from the filename.
        internal static string SchemaName(string DisplayName)
        {
            int i = DisplayName.IndexOf('.');
            if (i < 0)
            {
                return "dbo";
            }
            else
            {
                return DisplayName.Substring(0, i);
            }
        }

        // Return the routine name from the filename.
        internal static string RoutineName(string DisplayName)
        {
            int i = DisplayName.IndexOf('.');
            if (i < 0)
            {
                return DisplayName;
            }
            else
            {
                return DisplayName.Substring(i + 1);
            }
        }

        #endregion

        #region  Config File 

        // Grab the default users from the project config file.
        private static DataTable GetDefaultUsers(string WorkingFolder)
        {
            DataTable GetDefaultUsersRet = default;

            var DefaultUsers = new DataTable();
            {
                var withBlock = DefaultUsers.Columns;
                withBlock.Add("Name", typeof(string));
            }

            var Reader = new System.Xml.XmlDocument();
            Reader.Load(WorkingFolder + @"\" + Squealer.Constants.ConfigFilename);
            var Nodes = Reader.SelectNodes("/Settings/DefaultUsers/User");

            foreach (System.Xml.XmlNode Node in Nodes)
                DefaultUsers.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), string.Empty));

            GetDefaultUsersRet = DefaultUsers;
            return GetDefaultUsersRet;

        }

        // Grab the string replacements from the project config file.
        private static DataTable GetStringReplacements(string WorkingFolder)
        {
            DataTable GetStringReplacementsRet = default;

            var StringReplacements = new DataTable();
            {
                var withBlock = StringReplacements.Columns;
                withBlock.Add("Original", typeof(string));
                withBlock.Add("Replacement", typeof(string));
            }

            try
            {
                var Reader = new System.Xml.XmlDocument();
                Reader.Load(WorkingFolder + @"\" + Squealer.Constants.ConfigFilename);
                var Nodes = Reader.SelectNodes("/Settings/StringReplacements/String");

                foreach (System.Xml.XmlNode Node in Nodes)
                    StringReplacements.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Original"), string.Empty), AttributeDefaultString(Node.Attributes.GetNamedItem("Replacement"), string.Empty));
            }
            catch (Exception ex)
            {
            }

            GetStringReplacementsRet = StringReplacements;
            return GetStringReplacementsRet;

        }



        #endregion

        #region  Console Output 

        // Display a notice for a file.
        private static void SayFileAction(string notice, string path, string file)
        {
            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, notice + ":");
            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Arrow, path + Interaction.IIf(string.IsNullOrEmpty(file), "", @"\" + file).ToString());
        }

        #endregion

        #region  Connection String 

        private static void SetConnectionString(string workingfolder, string cs)
        {

            string f = string.Format(@"{0}\{1}", workingfolder, Squealer.Constants.ConnectionStringFilename);

            if (System.IO.File.Exists(f))
            {
                System.IO.File.Delete(f);
            }
            Squealer.My.MyProject.Computer.FileSystem.WriteAllBytes(f, Squealer.Misc.EncryptedBytes(cs), false);
            System.IO.File.SetAttributes(f, System.IO.FileAttributes.Hidden);

            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "OK");
            Squealer.Textify.SayNewLine();

        }
        private static void ForgetConnectionString(string workingfolder)
        {

            string f = string.Format(@"{0}\{1}", workingfolder, Squealer.Constants.ConnectionStringFilename);
            System.IO.File.Delete(f);
            Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "OK");
            Squealer.Textify.SayNewLine();

        }

        private static string GetConnectionString(string workingfolder)
        {
            string GetConnectionStringRet = default;

            string f = string.Format(@"{0}\{1}", workingfolder, Squealer.Constants.ConnectionStringFilename);

            if (!System.IO.File.Exists(f))
            {
                throw new Exception("Connection string not defined.");
            }

            GetConnectionStringRet = Squealer.Misc.DecryptedString(System.IO.File.ReadAllBytes(f));
            return GetConnectionStringRet;

        }

        private static void TestConnectionString(string workingfolder)
        {

            string cs = GetConnectionString(workingfolder);

            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Arrow, cs);

            using (var DbTest = new System.Data.SqlClient.SqlConnection(cs))
            {

                DbTest.Open();

                var DbReader = new System.Data.SqlClient.SqlCommand("select @@SERVERNAME,DB_NAME(),@@VERSION" + ",(select count(1) from sys.tables)" + ",(select count(1) from sys.objects o where o.type = 'p')" + ",(select count(1) from sys.objects o where o.type = 'fn')" + ",(select count(1) from sys.objects o where o.type = 'if')" + ",(select count(1) from sys.objects o where o.type = 'tf')" + ",(select count(1) from sys.objects o where o.type = 'v')" + ";", DbTest).ExecuteReader();







                while (DbReader.Read())
                {

                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, DbReader.GetString(2)); // @@version
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("[{0}].[{1}]", DbReader.GetString(0), DbReader.GetString(1)));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} table(s)", DbReader.GetInt32(3).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} stored procedure(s)", DbReader.GetInt32(4).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} scalar function(s)", DbReader.GetInt32(5).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} inline table-valued function(s)", DbReader.GetInt32(6).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} multi-statement table-valued function(s)", DbReader.GetInt32(7).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Arrow, string.Format("{0} views(s)", DbReader.GetInt32(8).ToString()));
                    Squealer.Textify.SayBulletLine(Squealer.Textify.eBullet.Hash, "OK");

                }

                Squealer.Textify.SayNewLine();

            }

        }

        #endregion

        #region  Automagic 

        private enum eAutoProcType
        {
            Insert,
            Update,
            Delete,
            Get,
            List
        }

        private static void Automagic(string cs, string WorkingFolder, bool ReplaceOnly, bool datasourcecomment)
        {

            Squealer.Textify.Write("Reading tables..");

            int ProcCount = 0;

            using (var DbTables = new System.Data.SqlClient.SqlConnection(cs))
            {

                DbTables.Open();

                var TableReader = new System.Data.SqlClient.SqlCommand(Squealer.My.Resources.Resources.AutoGetTables, DbTables).ExecuteReader();

                var spinny = new Squealer.SpinCursor();


                while (TableReader.Read())
                {

                    string SchemaName = TableReader.GetString(0);
                    string TableName = TableReader.GetString(1);
                    int TableId = TableReader.GetInt32(2);
                    eAutoProcType AutoProcType = (eAutoProcType)Enum.Parse(typeof(eAutoProcType), TableReader.GetString(3));

                    var GenOutputs = new List<string>();
                    var GenParameters = new List<string>();
                    string GenWhereClause = "";
                    var GenFromColumns = new List<string>();
                    var GenValuesColumns = new List<string>();

                    spinny.Animate();

                    using (var DbColumns = new System.Data.SqlClient.SqlConnection(cs))
                    {

                        DbColumns.Open();

                        var ColumnReader = new System.Data.SqlClient.SqlCommand(Squealer.My.Resources.Resources.AutoGetColumns.Replace("{TableId}", TableReader["table_id"].ToString()), DbColumns).ExecuteReader();

                        while (ColumnReader.Read())
                        {

                            string ColName = ColumnReader.GetString(0);
                            string ColType = ColumnReader.GetString(1).ToLower();
                            bool ColIsIdentity = ColumnReader.GetBoolean(2);
                            bool ColIsRowGuid = ColumnReader.GetBoolean(3);
                            int ColId = ColumnReader.GetInt32(4);
                            short ColMaxLength = ColumnReader.GetInt16(5);
                            byte ColPrecision = ColumnReader.GetByte(6);
                            byte ColScale = ColumnReader.GetByte(7);
                            string ColDefaultValue = ColumnReader[8].ToString();
                            bool ColHasDefault = ColDefaultValue.Trim().Length > 0;
                            bool ColIsPk = ColumnReader.GetBoolean(9);
                            bool ColIsComputed = ColumnReader.GetBoolean(10);
                            bool ColIsGuidIdentity = ColType.Contains("uniqueidentifier") && (ColDefaultValue.Contains("newid") || ColDefaultValue.Contains("newsequentialid"));


                            // DEFINE PARAMETERS

                            // Add scale and precision to column type
                            if (ColType.Contains("char") || ColType.Contains("binary"))
                            {
                                ColType += string.Format("({0})", ColMaxLength.ToString().Replace("-1", "max"));
                            }
                            else if (ColType.Contains("decimal") || ColType.Contains("numeric"))
                            {
                                ColType += string.Format("({0},{1})", ColPrecision.ToString(), ColScale.ToString());
                            }

                            // Add all parameters (with output IDs) for write procs; only add key parameters for read procs
                            if (AutoProcType == eAutoProcType.Insert || AutoProcType == eAutoProcType.Update || (AutoProcType == eAutoProcType.Delete || AutoProcType == eAutoProcType.Get) && ColIsPk)
                            {
                                GenParameters.Add(string.Format("<Parameter Name=\"{0}\" Type=\"{1}\" Output=\"{2}\" />", ColName.Replace(" ", "_"), ColType, AutoProcType == eAutoProcType.Insert && (ColIsIdentity || ColIsRowGuid)));
                            }


                            // DETECT OUTPUT COLUMNS

                            // Ignore columns with MAX width specification because those could be enormous (ex: file attachments).
                            if (AutoProcType == eAutoProcType.Insert && !(ColMaxLength == -1))
                            {
                                GenOutputs.Add(string.Format("{0}|{1}", ColName, ColType));
                            }


                            // BUILD THE INSERT/UPDATE/SELECT COLUMNS (never build DELETE columns)

                            if (AutoProcType == eAutoProcType.Insert & !ColIsIdentity & !ColIsGuidIdentity & !ColIsComputed || AutoProcType == eAutoProcType.Update & !ColIsPk & !ColIsComputed || AutoProcType == eAutoProcType.Get || AutoProcType == eAutoProcType.List)


                            {

                                string c = string.Format("[{0}]", ColName);
                                if (AutoProcType == eAutoProcType.Update)
                                {
                                    c += string.Format(" = @{0}", ColName.Replace(" ", "_"));
                                }
                                if ((AutoProcType == eAutoProcType.Insert || AutoProcType == eAutoProcType.Update) && ColHasDefault)
                                {
                                    c += " -- default: " + ColDefaultValue;
                                }
                                GenFromColumns.Add(c);

                                if (AutoProcType == eAutoProcType.Insert)
                                {
                                    if (ColIsRowGuid && !ColHasDefault)
                                    {
                                        c = "newid()";
                                    }
                                    else if (ColHasDefault)
                                    {
                                        c = ColDefaultValue;
                                    }
                                    else
                                    {
                                        c = "@" + ColName.Replace(" ", "_");
                                    }
                                    GenValuesColumns.Add(c);
                                }

                            }


                            // BUILD THE WHERE CLAUSE

                            if (ColIsPk && (AutoProcType == eAutoProcType.Update || AutoProcType == eAutoProcType.Delete || AutoProcType == eAutoProcType.Get))
                            {

                                GenWhereClause += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(GenWhereClause.Length > 0, "and ", "").ToString() + string.Format("[{0}] = @{0}", ColName);

                            }



                        }

                    }

                    string OutputUsers = "";

                    foreach (DataRow User in GetDefaultUsers(WorkingFolder).Rows)
                        OutputUsers += string.Format("<User Name=\"{0}\" />", User["Name"]);


                    string AutoCode = "/***********************************************************************" + Constants.vbCrLf + string.Format("	This code was generated by {0}.", Squealer.My.MyProject.Application.Info.Title) + Constants.vbCrLf + "***********************************************************************/";


                    if (AutoProcType == eAutoProcType.Delete)
                    {
                        AutoCode += Constants.vbCrLf + Constants.vbCrLf + "-- do you need an UPDATE statement to create an audit log entry?";
                    }

                    string[] GenOutputsString = new[] { "", "", "" };

                    if (GenOutputs.Count > 0)
                    {

                        for (int i = 0, loopTo = GenOutputs.Count - 1; i <= loopTo; i++)
                        {
                            GenOutputsString[0] += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(i == 0, "", ",").ToString() + string.Format("[{0}] {1}", GenOutputs[i].Split('|')[0], GenOutputs[i].Split('|')[1]);
                            GenOutputsString[1] += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(i == 0, "", ",").ToString() + string.Format("inserted.[{0}]", GenOutputs[i].Split('|')[0]);
                            GenOutputsString[2] += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(i == 0, "", ",").ToString() + string.Format("@{0} = o.[{1}]", GenOutputs[i].Split('|')[0].Replace(" ", "_"), GenOutputs[i].Split('|')[0]);
                        }

                        AutoCode += Constants.vbCrLf + Constants.vbCrLf + "declare @Outputs as table" + Constants.vbCrLf + "(" + GenOutputsString[0] + Constants.vbCrLf + ");";
                        GenOutputsString[1] = "output" + GenOutputsString[1] + Constants.vbCrLf + "into" + Constants.vbCrLf + Constants.vbTab + "@outputs";
                        GenOutputsString[2] = "select" + GenOutputsString[2] + Constants.vbCrLf + "from" + Constants.vbCrLf + Constants.vbTab + "@outputs o;";

                    }

                    AutoCode += Constants.vbCrLf;

                    if (AutoProcType == eAutoProcType.Get || AutoProcType == eAutoProcType.List)
                    {
                        AutoCode += Constants.vbCrLf + "select";
                    }
                    else
                    {
                        AutoCode += Constants.vbCrLf + AutoProcType.ToString().ToLower();
                    }

                    if (AutoProcType == eAutoProcType.Insert || AutoProcType == eAutoProcType.Update || AutoProcType == eAutoProcType.Delete)
                    {
                        AutoCode += Constants.vbCrLf + Constants.vbTab + string.Format("[{0}].[{1}]", SchemaName, TableName);
                    }

                    switch (AutoProcType)
                    {
                        case eAutoProcType.Insert:
                            {
                                AutoCode += Constants.vbCrLf + "(";
                                break;
                            }
                        case eAutoProcType.Update:
                            {
                                AutoCode += Constants.vbCrLf + "set";
                                break;
                            }
                    }

                    bool ValidOutput = true;

                    if (AutoProcType == eAutoProcType.Update && GenFromColumns.Count == 0)
                    {
                        ValidOutput = false;
                    }


                    for (int i = 0, loopTo1 = GenFromColumns.Count - 1; i <= loopTo1; i++)
                        AutoCode += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(i == 0, "", ",").ToString() + GenFromColumns[i];

                    if (AutoProcType == eAutoProcType.Insert)
                    {
                        AutoCode += Constants.vbCrLf + ")" + Constants.vbCrLf + GenOutputsString[1] + Constants.vbCrLf + "values" + Constants.vbCrLf + "(";
                    }

                    for (int i = 0, loopTo2 = GenValuesColumns.Count - 1; i <= loopTo2; i++)
                        AutoCode += Constants.vbCrLf + Constants.vbTab + Interaction.IIf(i == 0, "", ",").ToString() + GenValuesColumns[i];


                    if (AutoProcType == eAutoProcType.Insert)
                    {
                        AutoCode += Constants.vbCrLf + ");" + Constants.vbCrLf + Constants.vbCrLf + GenOutputsString[2];
                    }

                    // AutoCode &= GenFromClause

                    switch (AutoProcType)
                    {
                        case eAutoProcType.Get:
                            {
                                AutoCode += Constants.vbCrLf + "from" + Constants.vbCrLf + Constants.vbTab + string.Format("[{0}].[{1}]", SchemaName, TableName);
                                break;
                            }
                        case eAutoProcType.List:
                            {
                                AutoCode += Constants.vbCrLf + "from" + Constants.vbCrLf + Constants.vbTab + string.Format("[{0}].[{1}]", SchemaName, TableName);
                                break;
                            }
                    }

                    if (AutoProcType == eAutoProcType.List)
                    {
                        AutoCode += Constants.vbCrLf + "--order by" + Constants.vbCrLf + "--" + Constants.vbTab + "?";
                    }

                    if (!string.IsNullOrWhiteSpace(GenWhereClause))
                    {
                        AutoCode += Constants.vbCrLf + "where" + GenWhereClause;
                    }

                    string GenParametersString = "";
                    foreach (string s in GenParameters)
                        GenParametersString += s;


                    if (ValidOutput)
                    {

                        string filename = string.Format(@"{0}\{1}.{5}{2}{3}{4}", WorkingFolder, SchemaName, TableName, AutoProcType, Squealer.Constants.SquealerFileExtension, Squealer.Constants.AutocreateFilename);

                        if (!ReplaceOnly || System.IO.File.Exists(filename))
                        {

                            string comments = string.Format("Basic {0} for [{1}].[{2}]", AutoProcType.ToString().ToUpper(), SchemaName, TableName);
                            if (datasourcecomment)
                            {
                                comments += string.Format(Constants.vbCrLf + "Generated from [{0}].[{1}] on {2}", DbTables.DataSource, DbTables.Database, DateTime.Now);
                            }

                            Squealer.My.MyProject.Computer.FileSystem.WriteAllText(filename, Squealer.My.Resources.Resources.AutoProcTemplate.Replace("{Comments}", comments).Replace("{Parameters}", GenParametersString).Replace("{Code}", AutoCode).Replace("{Users}", OutputUsers), false);




                            // Overwrite

                            RepairXmlFile(false, filename);

                            ProcCount += 1;

                        }

                    }


                    if (Console.KeyAvailable)
                    {
                        throw new Exception("Keyboard interrupt.");
                    }

                }

                spinny.Finish();

            }

            Squealer.Textify.SayNewLine();
            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, string.Format("{0} files automatically created.", ProcCount.ToString()));
            Squealer.Textify.SayNewLine(2);

        }

        private static void ReverseEngineer(string cs, string WorkingFolder, bool DoCleanup)
        {

            Console.Write("Reading procs, views, functions..");

            int ProcCount = 0;
            int SkippedCount = 0;
            var tempfile = new Squealer.TempFileHandler(".txt");

            using (var DbObjects = new System.Data.SqlClient.SqlConnection(cs))
            {

                DbObjects.Open();

                var ObjectReader = new System.Data.SqlClient.SqlCommand(Squealer.My.Resources.Resources.ObjectList, DbObjects).ExecuteReader();

                var spinny = new Squealer.SpinCursor();

                while (ObjectReader.Read()) // loop thru procs, views, functions
                {

                    var ParameterList = new Squealer.ParameterCollection();
                    var UserList = new List<string>();

                    string ObjectName = ObjectReader.GetString(0);
                    string ObjectType = ObjectReader.GetString(1).ToLower();
                    string ObjectDefinition = ObjectReader.GetString(2);
                    int ObjectId = ObjectReader.GetInt32(3);

                    var filetype = Squealer.SquealerObjectType.Eval(ObjectType);

                    using (var DbParameters = new System.Data.SqlClient.SqlConnection(cs))
                    {

                        DbParameters.Open();

                        var ParameterReader = new System.Data.SqlClient.SqlCommand(Squealer.My.Resources.Resources.ObjectParameters.Replace("@ObjectId", ObjectId.ToString()), DbParameters).ExecuteReader();

                        while (ParameterReader.Read()) // loop thru parameters
                        {

                            string ParameterName = ParameterReader.GetString(0);
                            string ParameterType = ParameterReader.GetString(1);
                            bool IsOutput = ParameterReader.GetBoolean(2);
                            short MaxLength = ParameterReader.GetInt16(3);

                            ParameterList.Add(new Squealer.ParameterClass(ParameterName, ParameterType, (int)MaxLength, IsOutput));

                        }

                    }

                    using (var DbUsers = new System.Data.SqlClient.SqlConnection(cs))
                    {

                        DbUsers.Open();

                        var UserReader = new System.Data.SqlClient.SqlCommand(Squealer.My.Resources.Resources.ObjectPermissions.Replace("@ObjectId", ObjectId.ToString()), DbUsers).ExecuteReader();

                        while (UserReader.Read()) // loop thru user permissions granted
                        {

                            string UserName = UserReader.GetString(0);

                            UserList.Add(UserName);

                        }

                    }

                    if (DoCleanup)
                    {

                        // delete head
                        try
                        {
                            string s = "create " + Squealer.SquealerObjectType.EvalSimpleType(filetype);
                            ObjectDefinition = ObjectDefinition.Remove(0, ObjectDefinition.ToLower().IndexOf(s) + s.Length + 1);
                        }
                        catch (Exception ex)
                        {
                        }

                        // delete tail
                        try
                        {
                            string s = string.Empty;
                            switch (filetype)
                            {
                                case Squealer.SquealerObjectType.eType.StoredProcedure:
                                    {
                                        s = "YOUR CODE ENDS HERE.";
                                        break;
                                    }
                                case Squealer.SquealerObjectType.eType.ScalarFunction:
                                    {
                                        s = "Return the function result.";
                                        break;
                                    }
                            }
                            if (!string.IsNullOrEmpty(s))
                            {
                                ObjectDefinition = ObjectDefinition.Substring(0, ObjectDefinition.IndexOf(s));
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        // delete everything between parameters and beginning of code
                        try
                        {

                            string s = string.Empty;
                            string s2 = string.Empty;
                            switch (filetype)
                            {
                                case Squealer.SquealerObjectType.eType.StoredProcedure:
                                    {
                                        s = "Begin the transaction. Start the TRY..CATCH wrapper.";
                                        s2 = "YOUR CODE BEGINS HERE.";
                                        break;
                                    }
                                case Squealer.SquealerObjectType.eType.ScalarFunction:
                                    {
                                        s = "returns";
                                        s2 = "declare @Result " + ParameterList.ReturnType().Type;
                                        break;
                                    }
                            }
                            if (!string.IsNullOrEmpty(s))
                            {
                                int startpos = ObjectDefinition.IndexOf(s);
                                int charcount = ObjectDefinition.IndexOf(s2) + s2.Length - startpos;
                                ObjectDefinition = ObjectDefinition.Remove(startpos, charcount);
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }

                    ObjectDefinition = "-- reverse engineered on " + DateTime.Now.ToString() + Constants.vbCrLf + Constants.vbCrLf + ObjectDefinition;

                    string f = MainType.CreateNewFile(WorkingFolder, filetype, ObjectName, ParameterList, ObjectDefinition, UserList);
                    if (string.IsNullOrEmpty(f))
                    {
                        SkippedCount += 1;
                        tempfile.Writeline(string.Format("{0} -- duplicate ({1})", ObjectName, filetype.ToString()));
                    }
                    else
                    {
                        ProcCount += 1;
                        tempfile.Writeline(ObjectName + " -- OK");
                    }
                    spinny.Animate();

                    if (Console.KeyAvailable)
                    {
                        throw new Exception("Keyboard interrupt.");
                    }

                }

                spinny.Finish();

            }

            Squealer.Textify.SayNewLine();
            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, string.Format("{0} files reverse engineered; {1} skipped (duplicate filename).", ProcCount.ToString(), SkippedCount.ToString()));
            Squealer.Textify.SayNewLine();
            Squealer.Textify.SayBullet(Squealer.Textify.eBullet.Hash, "Results not guaranteed!", 0, new Squealer.Textify.ColorScheme(ConsoleColor.Yellow));
            Squealer.Textify.SayNewLine(2);

            tempfile.Show();

        }


        #endregion

        #region  Text Editor 

        // Edit one or more files.
        private static void OpenInTextEditor(string filename, string path)
        {
            EditFile(path + @"\" + filename);
        }
        private static void EditFile(string filename)
        {
            if (filename.EndsWith(".sql") && !MySettings.OpenWithDefault.SqlFiles || filename.EndsWith(Squealer.Constants.ConfigFilename) && !MySettings.OpenWithDefault.ConfigFiles || filename.EndsWith(Squealer.Constants.SquealerFileExtension) && !MySettings.OpenWithDefault.SquealerFiles)

            {
                Squealer.EasyShell.StartProcess(MySettings.TextEditorPath, filename);
            }
            else
            {
                Squealer.EasyShell.StartProcess(filename);
            }
        }

        #endregion

    }
}