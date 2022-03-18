﻿'Imports System.Collections.ObjectModel
Imports System.Windows.Forms
Imports System.Collections.ObjectModel
Imports System.Management.Automation
Imports System.Management.Automation.Runspaces



Public Class GitFlags

    Private _ShowUncommitted As Boolean = False
    Public Property ShowUncommitted As Boolean
        Get
            Return _ShowUncommitted
        End Get
        Set(value As Boolean)
            _ShowUncommitted = value
        End Set
    End Property

    Private _ShowDeleted As Boolean = False
    Public Property ShowDeleted As Boolean
        Get
            Return _ShowDeleted And _ShowUncommitted
        End Get
        Set(value As Boolean)
            _ShowDeleted = value
        End Set
    End Property

    Private _ShowHistory As Boolean = False
    Public Property ShowHistory As Boolean
        Get
            Return _ShowHistory
        End Get
        Set(value As Boolean)
            _ShowHistory = value
        End Set
    End Property

    Public ReadOnly Property GitEnabled As Boolean
        Get
            Return _ShowUncommitted
        End Get
    End Property

    Public Sub New()

    End Sub

End Class

Module Main

#Region " All The Definitions "

    Private MyCommands As New CommandCatalog.CommandDefinitionList
    Private BadCommandMessage As String = "Bad command or options."
    Private UserSettings As New UserSettingsClass

    Const cDefaultConnectionString As String = "Server=MySqlServer;Initial Catalog=AdventureWorks;Trusted_Connection=True"
    Const s3VersionText As String = "https://s3-us-west-1.amazonaws.com/public-10ec013b-b521-4150-9eab-56e1e1bb63a4/Squealer/ver.txt"
    Const s3ZipFile As String = "https://s3-us-west-1.amazonaws.com/public-10ec013b-b521-4150-9eab-56e1e1bb63a4/Squealer/Squealer.zip"
    Const MyThis As String = "``this``"

    Public Class UserSettingsClass

        Private _ShowLeaderboard As Boolean
        Public Property ShowLeaderboardAtStartup As Boolean
            Get
                Return _ShowLeaderboard
            End Get
            Set(value As Boolean)
                _ShowLeaderboard = value
            End Set
        End Property

        Private _EditNew As Boolean
        Public Property EditNew As Boolean
            Get
                Return _EditNew
            End Get
            Set(value As Boolean)
                _EditNew = value
            End Set
        End Property

        Private _DirStyle As String
        Public Property DirStyle As String
            Get
                Return _DirStyle
            End Get
            Set(value As String)
                _DirStyle = value
            End Set
        End Property

        Private _RecentFolders As Integer
        Public Property RecentFolders As Integer
            Get
                Return _RecentFolders
            End Get
            Set(value As Integer)
                _RecentFolders = value
            End Set
        End Property

        Private _AutoSearch As Boolean
        Public Property AutoSearch As Boolean
            Get
                Return _AutoSearch
            End Get
            Set(value As Boolean)
                _AutoSearch = value
            End Set
        End Property

        Private _UseClipboard As Boolean
        Public Property UseClipboard As Boolean
            Get
                Return _UseClipboard
            End Get
            Set(value As Boolean)
                _UseClipboard = value
            End Set
        End Property

        Private _ShowBranch As Boolean
        Public Property ShowBranch As Boolean
            Get
                Return _ShowBranch
            End Get
            Set(value As Boolean)
                _ShowBranch = value
            End Set
        End Property

        Private _SpacesAreWildcards As Boolean
        Public Property WildcardSpaces As Boolean
            Get
                Return _SpacesAreWildcards
            End Get
            Set(value As Boolean)
                _SpacesAreWildcards = value
            End Set
        End Property

        Public LastRunVersion As String = String.Empty ' this is just to generate an intellisense name

        Private _DetectSquealerObjects As Boolean
        Public Property DetectSquealerObjects As Boolean
            Get
                Return _DetectSquealerObjects
            End Get
            Set(value As Boolean)
                _DetectSquealerObjects = value
            End Set
        End Property

        Private _LeaderboardConnectionString As String
        Public Property LeaderboardConnectionString As String
            Get
                Return _LeaderboardConnectionString
            End Get
            Set(value As String)
                _LeaderboardConnectionString = value
            End Set
        End Property

    End Class

    Public Class MyConstants
        Public Shared ReadOnly Property ObjectFileExtension As String
            Get
                Return ".sqlr"
            End Get
        End Property

        Public Shared ReadOnly Property ConfigFilename As String
            Get
                Return My.Application.Info.ProductName & ".config"
            End Get
        End Property

        Public Shared ReadOnly Property ConnectionStringFilename As String
            Get
                Return ".connectionstring"
            End Get
        End Property

        Public Shared ReadOnly Property AutocreateFilename As String
            Get
                Return String.Format("({0})", My.Application.Info.ProductName)
            End Get
        End Property

    End Class

    Private Class BatchParametersClass

        Public Enum eOutputMode
            normal
            flags
            encrypt
            test
            alter
            convert
            permanent
            [string]
        End Enum

        Private _OutputMode As eOutputMode = eOutputMode.normal
        Public Property OutputMode As eOutputMode
            Get
                Return _OutputMode
            End Get
            Set(value As eOutputMode)
                _OutputMode = value
            End Set
        End Property

    End Class

#End Region

#Region " Enums "

    Public Enum eDirectoryStyle
        full
        compact
        symbolic
        invalid
    End Enum

    Private Enum eFileAction
        directory
        edit
        generate
        fix
        compare
        delete
        checkout
    End Enum

    Private Enum eCommandType
        [about]
        [checkout]
        [clear]
        [compare]
        [config]
        [connection]
        [delete]
        [directory]
        [edit]
        [exit]
        [explore]
        [fix]
        [forget]
        [generate]
        [help]
        [list]
        [make]
        [nerfherder]
        [new]
        [open]
        [raiserror]
        [reverse]
        [setting]
        [starwars]
        [test]
        [use]
    End Enum

#End Region

#Region " Folders "

    ' Set a new working folder and remember it for later.
    Private Sub ChangeFolder(ByVal newpath As String, ByRef ProjectFolder As String)

        My.Computer.FileSystem.CurrentDirectory = newpath ' this will throw an error if the path is not valid
        ProjectFolder = newpath
        RememberFolder(newpath)
        Textify.SayBulletLine(Textify.eBullet.Hash, "OK")
        Textify.SayNewLine()

        ' Temporary code to rename existing connection strings 4/3/2019
        Dim oldcs As String = newpath & "\.Squealer_cs"
        Try
            If My.Computer.FileSystem.FileExists(oldcs) Then
                My.Computer.FileSystem.RenameFile(oldcs, MyConstants.ConnectionStringFilename)
            End If
        Catch ex As Exception
            ' suppress errors
        End Try

    End Sub

    Private Function FolderCollection() As List(Of String)

        Dim folders As New List(Of String)
        Dim unsplit As String = My.Configger.LoadSetting("Folders", String.Empty)
        If Not String.IsNullOrWhiteSpace(unsplit) Then
            folders.AddRange(My.Configger.LoadSetting("Folders", "nothing").Split(New Char() {"|"c}))
        End If
        While folders.Count > UserSettings.RecentFolders
            folders.RemoveAt(folders.Count - 1)
        End While
        Return folders

    End Function

    Private Function InvalidFolderIndex() As Integer

        Dim f As String = FolderCollection().Find(Function(x) Not My.Computer.FileSystem.DirectoryExists(x))
        If String.IsNullOrEmpty(f) Then ' couldn't find any bad directories
            f = FolderCollection().Find(Function(x) My.Computer.FileSystem.GetFiles(x, FileIO.SearchOption.SearchTopLevelOnly, "*" & MyConstants.ObjectFileExtension).Count = 0)
            If String.IsNullOrEmpty(f) Then ' couldn't find any unused directories
                InvalidFolderIndex = -1
            Else
                InvalidFolderIndex = FolderCollection().IndexOf(f)
            End If
        Else
            InvalidFolderIndex = FolderCollection().IndexOf(f)
        End If

    End Function

    Private Sub AutoRemoveFolders()

        If InvalidFolderIndex() = -1 Then
            Textify.WriteLine("All folders contain *" & MyConstants.ObjectFileExtension)
        Else
            While InvalidFolderIndex() > -1
                Dim i As Integer = InvalidFolderIndex()
                Textify.WriteLine(eCommandType.forget.ToString & " " & FolderCollection(i), ConsoleColor.Red)
                ForgetFolder(i)
            End While
        End If

    End Sub

    Private Function FolderString(ByVal folders As List(Of String)) As String

        Dim s As String = String.Empty
        For Each item As String In folders
            s &= item & "|"
        Next
        If s.Length > 0 Then
            s = s.Remove(s.Length - 1)
        End If

        FolderString = s

    End Function


    ' List all remembered folders.
    Sub ListFolders(ByVal WorkingFolder As String)

        Dim folders As List(Of String) = FolderCollection()
        Dim longestnickname As Integer = 0

        If folders.Count = 0 Then
            Throw New Exception("No remembered folders.")
        Else

            Dim farray(folders.Count - 1, 3) As String

            For i As Integer = 0 To folders.Count - 1
                farray(i, 0) = i.ToString
                farray(i, 1) = GetProjectNickname(folders(i))
                farray(i, 2) = folders(i)
                If Not My.Computer.FileSystem.DirectoryExists(farray(i, 2)) Then
                    farray(i, 1) = "**********"
                    farray(i, 2) = "<<not found>>" & farray(i, 2)
                End If
                If farray(i, 1).Length > longestnickname Then
                    longestnickname = farray(i, 1).Length
                End If
            Next

            Dim highestnumber As Integer = farray.GetLength(0) - 1

            For i As Integer = 0 To highestnumber
                Textify.SayBullet(Textify.eBullet.Star, String.Format("{0} | ", farray(i, 0).PadLeft(highestnumber.ToString.Length)))
                Textify.Write(farray(i, 1).PadRight(longestnickname), ConsoleColor.Cyan)
                Textify.WriteLine(String.Format(" | {0}", farray(i, 2)))
            Next

        End If

        Textify.SayNewLine()

    End Sub

    ' Set a remembered folder as the current working folder.
    Sub LoadFolder(ByVal NewFolder As String, ByRef WorkingFolder As String)

        Try
            Dim n As Integer
            If Integer.TryParse(NewFolder, n) AndAlso n < 100 Then
                ' Load by project number
                ChangeFolder(FolderCollection(n), WorkingFolder)
            Else
                ' Load by project name
                Dim s As String = FolderCollection.Find(Function(x) GetProjectNickname(x).ToLower.StartsWith(NewFolder.ToLower))
                If String.IsNullOrEmpty(s) Then
                    s = FolderCollection.Find(Function(x) GetProjectNickname(x).ToLower.Contains(NewFolder.ToLower))
                End If
                ChangeFolder(s, WorkingFolder)
            End If

        Catch ex As Exception
            Throw New Exception("Invalid folder specification.")
        End Try

    End Sub

    ' Remove a folder from the list of projects.
    Sub ForgetFolder(index As String)
        ForgetFolder(CInt(index))
    End Sub
    Sub ForgetFolder(index As Integer)

        Try
            Dim folders As List(Of String) = FolderCollection()
            Dim folder As String = folders(index)
            folders.Remove(folder)
            My.Configger.SaveSetting("Folders", FolderString(folders))
            Textify.SayBulletLine(Textify.eBullet.Hash, "OK")
        Catch ex As Exception
            Throw New Exception("Invalid folder number.")
        End Try

    End Sub

    ' Save the folder to the list of projects.
    Sub RememberFolder(ByVal folder As String)

        Dim folders As List(Of String) = FolderCollection()
        While folders.Contains(folder)
            folders.Remove(folder)
        End While
        folders.Insert(0, folder)
        My.Configger.SaveSetting("Folders", FolderString(folders))

    End Sub

#End Region

#Region " Main Functions "

    ' Main module. Start here.
    Sub Main()

        DefineCommands()

        ' Increase input buffer size.
        Console.SetIn(New IO.StreamReader(Console.OpenStandardInput(8192)))

        ' Load settings.
        UserSettings.LeaderboardConnectionString = My.Configger.LoadSetting(NameOf(UserSettings.LeaderboardConnectionString), String.Empty)
        UserSettings.RecentFolders = My.Configger.LoadSetting(NameOf(UserSettings.RecentFolders), 20)
        UserSettings.AutoSearch = My.Configger.LoadSetting(NameOf(UserSettings.AutoSearch), False)
        UserSettings.EditNew = My.Configger.LoadSetting(NameOf(UserSettings.EditNew), True)
        UserSettings.UseClipboard = My.Configger.LoadSetting(NameOf(UserSettings.UseClipboard), True)
        UserSettings.ShowLeaderboardAtStartup = My.Configger.LoadSetting(NameOf(UserSettings.ShowLeaderboardAtStartup), False)
        UserSettings.DetectSquealerObjects = My.Configger.LoadSetting(NameOf(UserSettings.DetectSquealerObjects), True)
        UserSettings.ShowBranch = My.Configger.LoadSetting(NameOf(UserSettings.ShowBranch), True)
        UserSettings.WildcardSpaces = My.Configger.LoadSetting(NameOf(UserSettings.WildcardSpaces), False)
        UserSettings.DirStyle = My.Configger.LoadSetting(NameOf(UserSettings.DirStyle), eDirectoryStyle.compact.ToString)
        Textify.ErrorAlert.Beep = My.Configger.LoadSetting(NameOf(Textify.ErrorAlert.Beep), False)

        ' Restore the previous working folder
        Dim WorkingFolder As String = My.Configger.LoadSetting("PreviousFolder", My.Computer.FileSystem.SpecialDirectories.MyDocuments)
        If My.Computer.FileSystem.DirectoryExists(WorkingFolder) Then
            ChangeFolder(WorkingFolder, WorkingFolder)
        End If
        Console.Clear()

        ' Restore the previous window size
        Try
            ' This fails if the console was in full-screen mode at previous exit
            Console.SetWindowSize(My.Configger.LoadSetting("WindowWidth", 130), My.Configger.LoadSetting("WindowHeight", 30))
        Catch ex As Exception
        End Try
        Console.BufferWidth = Console.WindowWidth

        ' Change log.

        'Dim fileName$ = System.Reflection.Assembly.GetExecutingAssembly().Location
        'Dim fvi As FileVersionInfo = FileVersionInfo.GetVersionInfo(fileName)
        'Dim fvAsString$ = fvi.FileVersion ' but other useful properties exist too.

        Dim ver As New Version(My.Configger.LoadSetting(NameOf(UserSettings.LastRunVersion), "0.0.0.0"))
        If My.Application.Info.Version.CompareTo(ver) > 0 Then
            DisplayChangelog()
            My.Configger.SaveSetting(NameOf(UserSettings.LastRunVersion), My.Application.Info.Version.ToString)
        End If

        ' Main process
        Console.WriteLine()
        CheckS3(True)
        If IsStarWarsDay() Then
            Console.WriteLine("May the Fourth be with you! (easter egg revealed - see HELP)")
            Console.WriteLine()
        End If
        If UserSettings.ShowLeaderboardAtStartup Then
            ShowLeaderboard(10)
        End If
        HandleUserInput(WorkingFolder)

        ' Save the window size
        My.Configger.SaveSetting("WindowWidth", Console.WindowWidth)
        My.Configger.SaveSetting("WindowHeight", Console.WindowHeight)

        ' Save the current working folder for next time
        My.Configger.SaveSetting("PreviousFolder", WorkingFolder)

        ' Delete any settings that weren't referenced
        My.Configger.PruneSettings()

    End Sub

    Private Function FilesToProcess(ByVal ProjectFolder As String, ByVal Wildcard As String, SearchText As String, usedialog As Boolean, filter As SquealerObjectTypeCollection, ignoreCase As Boolean, FindExact As Boolean, todayonly As Boolean, hasPrePostCode As Boolean, gf As GitFlags) As List(Of String)

        Wildcard = Wildcard.Replace("[", "").Replace("]", "")

        Dim plaincolor As New Textify.ColorScheme(ConsoleColor.Gray, ConsoleColor.Black)
        Dim highlightcolor As New Textify.ColorScheme(ConsoleColor.Cyan, ConsoleColor.Black)
        Dim gitcolor As New Textify.ColorScheme(ConsoleColor.Red, ConsoleColor.Black)

        Textify.SayBullet(Textify.eBullet.Hash, "")
        Textify.Write("finding", plaincolor)

        Dim comma As String = Nothing


        If todayonly Then
            Textify.Write(" today's", highlightcolor)
        End If
        If filter.AllSelected Then
            Textify.Write(" all", highlightcolor)
            If gf.GitEnabled Then
                Textify.Write(" uncommitted case-sensitive", gitcolor)
            End If
        Else

            If gf.GitEnabled Then
                Textify.Write(" uncommitted case-sensitive", gitcolor)
            End If

            comma = " "

            For Each t As SquealerObjectType In filter.Items.Where(Function(x) x.Selected)
                Textify.Write(comma & t.LongType.ToString, highlightcolor)
                comma = ", "
            Next
        End If

        If usedialog Then
            Textify.Write(" hand-picked", highlightcolor)
        End If

        Dim EverythingIncludingDuplicates As New List(Of String)
        Textify.Write(" files", plaincolor)
        If hasPrePostCode Then
            Textify.Write(" with ", plaincolor)
            Textify.Write("pre/post code", highlightcolor)
        End If
        If Not String.IsNullOrEmpty(SearchText) Then
            Textify.Write(" containing ", plaincolor)
            Textify.Write("""" & SearchText & """" & IIf(ignoreCase, "", "(case-sensitive)").ToString, highlightcolor)
        End If
        Textify.Write(" matching", plaincolor)

        comma = ""

        For Each s As String In Wildcard.Split((New Char() {"|"c}))
            If s.ToLower.Contains(MyConstants.ObjectFileExtension.ToLower) Then
                Console.WriteLine()
                Throw New ArgumentException(s.Trim & " search term contains explicit reference To " & MyConstants.ObjectFileExtension)
            End If
            s = WildcardInterpreter(s.Trim, FindExact)
            Textify.Write(comma & " " & s, highlightcolor)
            comma = ", "


            If gf.GitEnabled Then

                If Not String.IsNullOrEmpty(SearchText) Then
                    gf.ShowDeleted = False ' there will not be any deleted files that contain search text
                End If

                Dim FoundFiles As New List(Of String)

                FoundFiles.AddRange(GitChangedFiles(ProjectFolder, "git status -s", s, gf.ShowDeleted).FindAll(Function(x) x.ToLower Like s.ToLower))

                ' Remove any results that don't contain the search text
                If gf.GitEnabled AndAlso Not String.IsNullOrEmpty(SearchText) Then
                    If ignoreCase Then
                        FoundFiles.RemoveAll(Function(x) Not My.Computer.FileSystem.ReadAllText(x).ToLower.Contains(SearchText.ToLower))
                    Else
                        FoundFiles.RemoveAll(Function(x) Not My.Computer.FileSystem.ReadAllText(x).Contains(SearchText))
                    End If
                End If

                EverythingIncludingDuplicates.AddRange(FoundFiles)

            ElseIf String.IsNullOrEmpty(SearchText) Then
                EverythingIncludingDuplicates.AddRange(My.Computer.FileSystem.GetFiles(ProjectFolder, FileIO.SearchOption.SearchTopLevelOnly, s).ToList)
            Else
                EverythingIncludingDuplicates.AddRange(My.Computer.FileSystem.FindInFiles(ProjectFolder, SearchText, ignoreCase, FileIO.SearchOption.SearchTopLevelOnly, s).ToList)
            End If

        Next

        Dim DistinctFiles As New List(Of String)
        DistinctFiles.AddRange(From s In EverythingIncludingDuplicates Distinct Order By s)

        Console.WriteLine()
        Console.WriteLine()

        ' Remove any results that don't match hand picked files.
        If usedialog Then
            Dim pickedfiles As List(Of String) = GetFileList(ProjectFolder)
            DistinctFiles.RemoveAll(Function(x) Not pickedfiles.Exists(Function(y) y = x))
        End If

        ' Remove any results that don't match the requested object types
        For Each t As SquealerObjectType In filter.Items.Where(Function(x) Not x.Selected)
            DistinctFiles.RemoveAll(Function(x) SquealerObjectType.Eval(XmlGetObjectType(x)) = t.LongType)
        Next

        ' Remove any results that don't match the time constraint
        If todayonly Then
            With My.Computer.FileSystem
                DistinctFiles.RemoveAll(Function(x) Not (.GetFileInfo(x).LastWriteTime.Year = Now.Year AndAlso .GetFileInfo(x).LastWriteTime.DayOfYear = Now.DayOfYear))
            End With
        End If

        ' Remove any results that don't have pre/post code
        If hasPrePostCode Then
            DistinctFiles.RemoveAll(Function(x) Not PrePostCodeExists(x))
        End If

        Return DistinctFiles

    End Function

    ' Enumerate through files in the working folder and take some action on them.
    Private Sub ProcessFiles(ByVal FileListing As List(Of String), ByVal Action As eFileAction, bp As BatchParametersClass, ByVal TargetFileType As SquealerObjectType.eType, git As GitFlags, MakePretty As Boolean)

        Dim FileCount As Integer = 0
        Dim SkippedFiles As Integer = 0
        Dim GeneratedOutput As String = String.Empty

        If bp.OutputMode = BatchParametersClass.eOutputMode.string Then
            Console.Write(eCommandType.directory.ToString.ToLower & " - x ")
        Else

            If UserSettings.DirStyle = eDirectoryStyle.full.ToString Then
                Textify.Write("Type Flags ")
            ElseIf UserSettings.DirStyle = eDirectoryStyle.compact.ToString Then
                Textify.Write("   ")
            ElseIf UserSettings.DirStyle = eDirectoryStyle.symbolic.ToString Then
                Textify.Write(" ")
            End If

            Textify.WriteLine("FileName")

            If UserSettings.DirStyle = eDirectoryStyle.full.ToString Then
                Textify.Write("---- ----- ")
            ElseIf UserSettings.DirStyle = eDirectoryStyle.compact.ToString Then
                Textify.Write("-- ")
            End If

            Textify.WriteToEol("-"c)
            Console.WriteLine()

        End If

        For Each FileName As String In FileListing

            If Console.KeyAvailable() Then
                Throw New System.Exception("Keyboard interrupt.")
            End If

            BracketCheck(FileName)

            Dim info As IO.FileInfo = My.Computer.FileSystem.GetFileInfo(FileName)

            Dim obj As New SquealerObject(FileName)


            If bp.OutputMode = BatchParametersClass.eOutputMode.string Then
                If FileCount > 0 Then
                    Console.Write("|")
                End If
                Console.Write(info.Name.Replace(MyConstants.ObjectFileExtension, ""))
            Else
                Dim fg As ConsoleColor = ConsoleColor.Gray




                If obj.Type.LongType = SquealerObjectType.eType.Invalid Then
                    fg = ConsoleColor.Red
                End If


                If UserSettings.DirStyle = eDirectoryStyle.full.ToString Then
                    Textify.Write(" " & obj.Type.ShortType.ToString.PadRight(4) & obj.FlagsSummary)
                ElseIf UserSettings.DirStyle = eDirectoryStyle.compact.ToString Then
                    Textify.Write(obj.Type.ShortType.ToString.PadRight(2))
                    If String.IsNullOrWhiteSpace(obj.FlagsSummary) Then
                        Textify.Write(" ")
                    Else
                        Textify.Write("*")
                    End If
                ElseIf UserSettings.DirStyle = eDirectoryStyle.symbolic.ToString Then
                    If String.IsNullOrWhiteSpace(obj.FlagsSummary) Then
                        Textify.Write(" ")
                    Else
                        Textify.Write("*")
                    End If
                End If


                Textify.Write(info.Name.Replace(MyConstants.ObjectFileExtension, ""), fg)

                Dim symbol As String = String.Empty
                Select Case obj.Type.LongType
                    Case SquealerObjectType.eType.StoredProcedure
                        symbol = ""
                    Case SquealerObjectType.eType.ScalarFunction
                        symbol = "()"
                    Case SquealerObjectType.eType.InlineTableFunction
                        symbol = "*"
                    Case SquealerObjectType.eType.MultiStatementTableFunction
                        symbol = "**"
                    Case SquealerObjectType.eType.View
                        symbol = "+"
                End Select

                If UserSettings.DirStyle = eDirectoryStyle.symbolic.ToString Then
                    Textify.Write(symbol, ConsoleColor.Green)
                End If

            End If

            Try

                Dim gitstatuscode As String = String.Empty
                If git.ShowUncommitted Then
                    gitstatuscode = " " & GitResults(info.DirectoryName, "git status -s ", info.Name)(0).Replace(info.Name, "").TrimStart
                End If

                Select Case Action
                    Case eFileAction.directory
                        If bp.OutputMode = BatchParametersClass.eOutputMode.flags AndAlso obj.FlagsList.Count > 0 Then
                            Console.WriteLine()
                            Console.WriteLine("           {")
                            For Each s As String In obj.FlagsList
                                Console.WriteLine("             " & s)
                            Next
                            Console.Write("           }")
                        End If
                    Case eFileAction.fix
                        If bp.OutputMode = BatchParametersClass.eOutputMode.normal Then
                            If RepairXmlFile(False, info.FullName, MakePretty) Then
                                Textify.Write(String.Format(" ... {0}", eCommandType.fix.ToString.ToUpper), ConsoleColor.Green)
                            Else
                                SkippedFiles += 1
                            End If
                        Else
                            If ConvertXmlFile(info.FullName, TargetFileType, MakePretty) Then
                                Textify.Write(String.Format(" ... {0}", BatchParametersClass.eOutputMode.convert.ToString.ToUpper))
                            Else
                                SkippedFiles += 1
                            End If
                        End If

                    Case eFileAction.edit
                        ShellOpenFile(info.FullName)
                    Case eFileAction.checkout
                        GitCommandDo(info.DirectoryName, "git checkout -- " & info.Name, " (oops, wut happened)", Action)
                    Case eFileAction.generate
                        GeneratedOutput &= ExpandIndividual(info, GetStringReplacements(My.Computer.FileSystem.GetFileInfo(FileListing(0)).DirectoryName), bp, FileCount + 1, FileListing.Count)
                    Case eFileAction.compare
                        Dim RootName As String = info.Name.Replace(MyConstants.ObjectFileExtension, "")
                        GeneratedOutput &= String.Format("insert #CodeToDrop ([Type], [Schema], [Name]) values ('{0}','{1}','{2}');", obj.Type.GeneralType, SchemaName(RootName), RoutineName(RootName)) & vbCrLf
                    Case eFileAction.delete
                        Dim trashcan As FileIO.RecycleOption = FileIO.RecycleOption.SendToRecycleBin
                        If bp.OutputMode = BatchParametersClass.eOutputMode.permanent Then
                            trashcan = FileIO.RecycleOption.DeletePermanently
                        End If
                        My.Computer.FileSystem.DeleteFile(info.FullName, FileIO.UIOption.OnlyErrorDialogs, trashcan)
                End Select
                If Not bp.OutputMode = BatchParametersClass.eOutputMode.string Then

                    If git.ShowUncommitted Then
                        'Try
                        ' This will fail if we just now deleted the file
                        'Textify.Write(" " & GitResults(info.DirectoryName, "git status -s ", info.Name)(0).Replace(info.Name, "").TrimStart, ConsoleColor.Red)
                        Textify.Write(gitstatuscode, ConsoleColor.Red)
                        'Catch ex As Exception
                        'End Try
                    End If
                    If git.ShowHistory Then
                        GitCommandDo(info.DirectoryName, "git log --pretty=format:""%h (%cr) %s"" " & info.Name, " (no history)", Action)
                    End If

                    Console.WriteLine()

                End If
                FileCount += 1
            Catch ex As Exception
                Textify.WriteLine(" ... FAILED!", ConsoleColor.Red)
                Throw New Exception(ex.Message)
            End Try

        Next

        If FileCount > 0 Then
            If bp.OutputMode = BatchParametersClass.eOutputMode.string Then
                Textify.SayNewLine()
            End If
            Textify.SayNewLine()
        End If

        Dim SummaryLine As String = "{4}/{0} files ({3} skipped) (action:{1}, mode:{2})"
        If SkippedFiles = 0 Then
            SummaryLine = "{0} files (action:{1}, mode:{2})"
        End If

        Textify.SayBulletLine(Textify.eBullet.Hash, String.Format(SummaryLine, FileCount.ToString, Action.ToString, bp.OutputMode.ToString, SkippedFiles.ToString, (FileCount - SkippedFiles).ToString))


        If (Action = eFileAction.generate OrElse Action = eFileAction.compare) AndAlso FileCount > 0 Then

            If Action = eFileAction.compare Then
                GeneratedOutput = My.Resources.SqlDropOrphanedRoutines.Replace("{RoutineList}", GeneratedOutput).Replace("{ExcludeFilename}", MyConstants.AutocreateFilename)
            ElseIf Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                If UserSettings.DetectSquealerObjects Then
                    GeneratedOutput = My.Resources.SqlTopScript & GeneratedOutput
                End If
            End If

            If UserSettings.UseClipboard Then
                Console.WriteLine()
                Textify.SayBulletLine(Textify.eBullet.Hash, "Output copied to Windows clipboard.")
                Clipboard.SetText(GeneratedOutput)
            Else
                Dim tempfile As New TempFileHandler(".sql")
                tempfile.Writeline(GeneratedOutput)
                tempfile.Show()
            End If

        End If

        Textify.SayNewLine()

    End Sub

#End Region

#Region " Commands "

    Private Sub DefineCommands()

        Dim cmd As CommandCatalog.CommandDefinition
        Dim opt As CommandCatalog.CommandSwitch

        ' the un-command
        cmd = New CommandCatalog.CommandDefinition({eCommandType.nerfherder.ToString, "nerf"}, {"This command is as useless as a refrigerator on Hoth."}, CommandCatalog.eCommandCategory.other)
        cmd.Visible = False
        MyCommands.Items.Add(cmd)

        ' open folder
        cmd = New CommandCatalog.CommandDefinition({eCommandType.open.ToString}, {"Open folder {options}.", "This folder path will be saved for quick access. See " & eCommandType.list.ToString.ToUpper & " command. Omit path to open folder dialog."}, CommandCatalog.eCommandCategory.folder, "<path>", False)
        cmd.Examples.Add("% " & My.Computer.FileSystem.SpecialDirectories.MyDocuments)
        cmd.Examples.Add("% C:\Some Folder\Spaces Are OK, Quotes Not Needed")
        cmd.Examples.Add("% -- open Windows Explorer to select folder")
        cmd.IgnoreSwitches = True
        MyCommands.Items.Add(cmd)

        ' list folders
        cmd = New CommandCatalog.CommandDefinition({eCommandType.list.ToString, "l"}, {"List the saved folders."}, CommandCatalog.eCommandCategory.folder)
        MyCommands.Items.Add(cmd)

        ' use folder
        cmd = New CommandCatalog.CommandDefinition({eCommandType.use.ToString}, {"Reopen a saved folder.", "See " & eCommandType.list.ToString.ToUpper & " command."}, CommandCatalog.eCommandCategory.folder, "<project name or folder number>", True)
        cmd.Examples.Add("% 3")
        cmd.Examples.Add("% northwind")
        MyCommands.Items.Add(cmd)

        ' forget folder
        cmd = New CommandCatalog.CommandDefinition({eCommandType.forget.ToString}, {"Forget a saved folder.", "See " & eCommandType.list.ToString.ToUpper & " command. Either specify a folder to forget, or automatically forget all folders that do not contain any " & MyConstants.ObjectFileExtension & " files."}, CommandCatalog.eCommandCategory.folder, "<folder number>", False)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("auto;detect invalid folders"))
        cmd.Examples.Add("% 3")
        cmd.Examples.Add("% -auto")
        MyCommands.Items.Add(cmd)

        ' file explorer
        cmd = New CommandCatalog.CommandDefinition({eCommandType.explore.ToString, "fe"}, {"Open File Explorer.", "Opens the current working folder. If {options} is specified, the first matching " & My.Application.Info.ProductName & " object will be selected."}, CommandCatalog.eCommandCategory.folder, CommandCatalog.CommandDefinition.WildcardText, False)
        MyCommands.Items.Add(cmd)

        ' checkout
        cmd = New CommandCatalog.CommandDefinition({eCommandType.checkout.ToString, "undo"}, {"Git checkout.", "Checkout objects from Git and discard local changes."}, CommandCatalog.eCommandCategory.file, True, True)
        MyCommands.Items.Add(cmd)

        ' dir
        cmd = New CommandCatalog.CommandDefinition({eCommandType.directory.ToString, "dir"}, {"Directory.", String.Format("List {0} objects in the current working folder.", My.Application.Info.ProductName)}, CommandCatalog.eCommandCategory.file, False, True)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("h;show git history"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("f;show flags"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("str;string output"))
        cmd.Examples.Add("% -cs dbo.* / Han shot first -- find all dbo.* files containing "" Han shot first"" (with leading space and capital H)")
        cmd.Examples.Add("% -p -v /Solo -- find all stored procedures and views containing ""Solo"" (or ""solo"" or ""SOLO"" or ""soLO"")")
        MyCommands.Items.Add(cmd)

        ' new file
        cmd = New CommandCatalog.CommandDefinition({eCommandType.new.ToString}, {String.Format("Create a new {0} object.", My.Application.Info.ProductName), "Default schema is ""dbo""."}, CommandCatalog.eCommandCategory.file, CommandCatalog.CommandDefinition.FilenameText, True)
        For Each s As String In New SquealerObjectTypeCollection().ObjectTypesOptionString(False).Split((New Char() {"|"c}))
            cmd.Options.Items.Add(New CommandCatalog.CommandSwitch(s, s.StartsWith("p")))
        Next
        cmd.Examples.Add("% AddEmployee -- create new stored procedure dbo.AddEmployee")
        cmd.Examples.Add("% -v myschema.Employees -- create new view myschema.Employees")
        MyCommands.Items.Add(cmd)

        ' edit files
        cmd = New CommandCatalog.CommandDefinition({eCommandType.edit.ToString, "e"}, {String.Format("Edit {0} objects.", My.Application.Info.ProductName), String.Format("Uses your configured text editor. See {0} command.", eCommandType.setting.ToString.ToUpper)}, CommandCatalog.eCommandCategory.file, False, True)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("all;override file limit"))
        cmd.Examples.Add("% dbo.AddEmployee")
        cmd.Examples.Add("% dbo.*")
        MyCommands.Items.Add(cmd)

        ' fix files
        cmd = New CommandCatalog.CommandDefinition({eCommandType.fix.ToString}, {String.Format("Rewrite {0} objects (DESTRUCTIVE).", My.Application.Info.ProductName), String.Format("Original files will be rewritten To {0} specifications. Optionally convert objects to a different type.", My.Application.Info.ProductName)}, CommandCatalog.eCommandCategory.file, False, True)
        opt = New CommandCatalog.CommandSwitch("c;convert to")
        For Each s As String In New SquealerObjectTypeCollection().ObjectTypesOptionString(False).Split((New Char() {"|"c}))
            opt.Options.Items.Add(New CommandCatalog.CommandSwitchOption(s))
        Next
        cmd.Options.Items.Add(opt)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("format;beautify code"))
        cmd.Examples.Add("% dbo.*")
        cmd.Examples.Add("% -c:p * -- convert everything to stored procedures")
        cmd.Examples.Add("% -v -p -c:if * -- convert views and stored procedures to inline table-valued functions")
        MyCommands.Items.Add(cmd)

        ' generate
        cmd = New CommandCatalog.CommandDefinition({eCommandType.generate.ToString, "gen"}, {"Generate SQL Server objects.", String.Format("Output is written to a temp file and opened with your configured text editor. See {0} command.", eCommandType.setting.ToString.ToUpper)}, CommandCatalog.eCommandCategory.file, False, True)
        opt = New CommandCatalog.CommandSwitch("m;output mode")
        opt.Options.Items.Add(New CommandCatalog.CommandSwitchOption("alt;alter, do not drop original"))
        opt.Options.Items.Add(New CommandCatalog.CommandSwitchOption("t;test script, limit 1 object"))
        opt.Options.Items.Add(New CommandCatalog.CommandSwitchOption("e;with encryption"))
        cmd.Options.Items.Add(opt)
        cmd.Examples.Add("% dbo.*")
        cmd.Examples.Add("% -alt -v dbo.* -- generate ALTER scripts for dbo.* VIEW objects")
        MyCommands.Items.Add(cmd)

        ' compare
        cmd = New CommandCatalog.CommandDefinition({eCommandType.compare.ToString}, {String.Format("Compare {0} with SQL Server.", My.Application.Info.ProductName), String.Format("This generates a T-SQL query to discover any SQL Server objects that are not in {0}, and any {0} objects that are not in SQL Server.", My.Application.Info.ProductName)}, CommandCatalog.eCommandCategory.file, False, True)
        MyCommands.Items.Add(cmd)

        ' delete
        cmd = New CommandCatalog.CommandDefinition({eCommandType.delete.ToString, "del"}, {String.Format("Delete {0} objects.", My.Application.Info.ProductName), "Objects will be sent to the Recycle Bin by default."}, CommandCatalog.eCommandCategory.file, True, True)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("e;permanently erase"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("all;override file limit"))
        cmd.Examples.Add("% dbo.AddEmployee")
        cmd.Examples.Add("% dbo.*")
        MyCommands.Items.Add(cmd)


        ' make
        cmd = New CommandCatalog.CommandDefinition({eCommandType.make.ToString}, {String.Format("Automatically create {0} objects.", My.Application.Info.ProductName), "Create default insert, update, read, and delete objects for the target database. Define the target database with the " & eCommandType.connection.ToString.ToUpper & " command."}, CommandCatalog.eCommandCategory.file)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch(String.Format("r;replace existing {0} objects only", My.Application.Info.ProductName)))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("nocomment;omit data source and timestamp from comment section"))
        MyCommands.Items.Add(cmd)

        ' reverse engineer
        cmd = New CommandCatalog.CommandDefinition({eCommandType.reverse.ToString}, {String.Format("Reverse engineer SQL objects.", My.Application.Info.ProductName), "Reverse engineer existing SQL Server procs, views, and functions from the target database. Define the target database with the " & eCommandType.connection.ToString.ToUpper & " command. Duplicate filenames will not be overwritten. Results are NOT GUARANTEED and require manual review and edits."}, CommandCatalog.eCommandCategory.file)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("clean;attempt to clean up imported code previously generated by " & My.Application.Info.ProductName))
        MyCommands.Items.Add(cmd)


        ' help
        cmd = New CommandCatalog.CommandDefinition({eCommandType.help.ToString, "h"}, {"{command} for command list, or {command} {options} for details of a single command.", "Switches are ignored if a command is specified."}, CommandCatalog.eCommandCategory.other, "<command>", False)
        cmd.Examples.Add("% " & eCommandType.generate.ToString)
        MyCommands.Items.Add(cmd)


        ' config
        cmd = New CommandCatalog.CommandDefinition({eCommandType.config.ToString, "c"}, {"Display or edit " & MyConstants.ConfigFilename & ".", "This file configures how " & My.Application.Info.ProductName & " operates in your current working folder."}, CommandCatalog.eCommandCategory.other)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("e;edit existing file"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("new;create new default file"))
        MyCommands.Items.Add(cmd)

        ' setting
        cmd = New CommandCatalog.CommandDefinition({eCommandType.setting.ToString, "set"}, {"Display application settings."}, CommandCatalog.eCommandCategory.other)
        MyCommands.Items.Add(cmd)

        ' connection string
        cmd = New CommandCatalog.CommandDefinition({eCommandType.connection.ToString, "cs"}, {"Define the SQL Server connection string.", String.Format("The connection string is encrypted for the current local user and current working folder, and is required for some {0} commands. If you are using version control, you should add ""{1}"" to your ignore list.", My.Application.Info.ProductName, MyConstants.ConnectionStringFilename)}, CommandCatalog.eCommandCategory.other, "<connectionstring>", False)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("e;edit current connection string"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("t;test connection", True))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("set;encrypt and save the connection string"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("show;display the connection string"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("forget;discard the saved connection string"))
        cmd.Examples.Add("% -set " & cDefaultConnectionString)
        cmd.Examples.Add("% -get")
        MyCommands.Items.Add(cmd)

        ' cls
        cmd = New CommandCatalog.CommandDefinition({eCommandType.clear.ToString, "cls"}, {"Clear the console."}, CommandCatalog.eCommandCategory.other)
        MyCommands.Items.Add(cmd)


        ' raiserror
        cmd = New CommandCatalog.CommandDefinition({eCommandType.raiserror.ToString, "err"}, {String.Format("Display the T-SQL for raising errors inside a {0} object.", My.Application.Info.ProductName), My.Application.Info.ProductName & " has specific rules about how to raise errors."}, CommandCatalog.eCommandCategory.other)
        MyCommands.Items.Add(cmd)

        ' about
        cmd = New CommandCatalog.CommandDefinition({eCommandType.about.ToString}, {"Check for updates and display program information."}, CommandCatalog.eCommandCategory.other)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("download;download latest version"))
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("changelog;display the changelog"))
        MyCommands.Items.Add(cmd)

        ' exit
        cmd = New CommandCatalog.CommandDefinition({eCommandType.exit.ToString, "x"}, {"Quit."}, CommandCatalog.eCommandCategory.other)
        MyCommands.Items.Add(cmd)

        ' star wars
        cmd = New CommandCatalog.CommandDefinition({eCommandType.starwars.ToString, "r2"}, {"I've got a bad feeling about this.", "Jump in an X-Wing and blow something up!"}, CommandCatalog.eCommandCategory.other)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("top;display leaderboard"))
        cmd.Visible = IsStarWarsDay()
        MyCommands.Items.Add(cmd)

        ' test 
        cmd = New CommandCatalog.CommandDefinition({eCommandType.test.ToString}, {"Hidden command. Debugging/testing only."}, CommandCatalog.eCommandCategory.other)
        cmd.Options.Items.Add(New CommandCatalog.CommandSwitch("release;generate version text file"))
        cmd.Visible = False
        MyCommands.Items.Add(cmd)

    End Sub

    Private Function WildcardInterpreter(s As String, FindExact As Boolean) As String

        If UserSettings.WildcardSpaces Then
            s = s.Replace(" "c, "*"c)
        End If

        If String.IsNullOrWhiteSpace(s) Then
            s = "*"
        ElseIf UserSettings.AutoSearch AndAlso Not FindExact Then
            s = "*" & s & "*"
        End If
        While s.Contains("**")
            s = s.Replace("**", "*")
        End While
        Return s & MyConstants.ObjectFileExtension

    End Function

    Private Function StringInList(l As List(Of String), s As String) As Boolean
        Return l.Exists(Function(x) x.ToLower = s.ToLower)
    End Function


    ' The main command interface loop.
    Private Sub HandleUserInput(ByRef WorkingFolder As String)

        Dim MySwitches As New List(Of String)
        Dim UserInput As String = Nothing

        Textify.SayBulletLine(Textify.eBullet.Hash, "Type HELP to get started.")
        Console.WriteLine()

        Dim MyCommand As CommandCatalog.CommandDefinition = MyCommands.FindCommand(eCommandType.nerfherder.ToString)
        Dim SwitchesValidated As Boolean = True
        Dim MySearchText As String = String.Empty
        Dim ObjectTypeFilter As New SquealerObjectTypeCollection



        While Not (MyCommand IsNot Nothing AndAlso MyCommand.Keyword = eCommandType.exit.ToString AndAlso SwitchesValidated AndAlso String.IsNullOrEmpty(UserInput))

            Try

                If MyCommand IsNot Nothing AndAlso MyCommand.Keyword = eCommandType.nerfherder.ToString Then

                    ' do nothing


                ElseIf Not SwitchesValidated Then

                    Throw New Exception("Invalid command switch.")


                ElseIf MyCommand IsNot Nothing AndAlso MyCommand.ParameterRequired AndAlso String.IsNullOrEmpty(UserInput) Then

                    Throw New Exception("Required parameter is missing.")


                ElseIf MyCommand IsNot Nothing AndAlso String.IsNullOrEmpty(MyCommand.ParameterDefinition) AndAlso Not String.IsNullOrEmpty(UserInput) Then

                    Throw New Exception("Unexpected command parameter.")


                ElseIf MyCommand Is Nothing Then

                    Throw New System.Exception(BadCommandMessage)


                ElseIf MyCommand.Keyword = eCommandType.about.ToString Then

                    Console.WriteLine(AboutInfo)
                    Console.WriteLine()
                    CheckS3(False)

                    If StringInList(MySwitches, "download") Then
                        Textify.SayBulletLine(Textify.eBullet.Hash, "Opening remote file...")
                        Console.WriteLine()
                        Dim wc As New Net.WebClient
                        Dim fn As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments & "\Squealer.zip"
                        wc.DownloadFile(s3ZipFile, fn)
                        Textify.SayBulletLine(Textify.eBullet.Hash, "File downloaded to " & fn, ConsoleColor.White)
                        Textify.SayNewLine()
                        Textify.SayBulletLine(Textify.eBullet.Hash, "Opening local folder...")
                        Console.WriteLine()
                        OpenExplorer("Squealer.zip", My.Computer.FileSystem.SpecialDirectories.MyDocuments)
                    End If

                    If StringInList(MySwitches, "changelog") Then
                        DisplayChangelog()
                    End If




                ElseIf MyCommand.Keyword = eCommandType.clear.ToString Then

                    Console.Clear()



                ElseIf MyCommand.Keyword = eCommandType.[config].ToString Then

                    ' Try to make a new file
                    If StringInList(MySwitches, "new") Then
                        If My.Computer.FileSystem.FileExists(WorkingFolder & "\" & MyConstants.ConfigFilename) Then
                            Throw New Exception("Config file already exists.")
                        Else
                            My.Computer.FileSystem.WriteAllText(WorkingFolder & "\" & MyConstants.ConfigFilename, My.Resources.UserConfig, False)
                            SayFileAction("config file created", WorkingFolder, MyConstants.ConfigFilename)
                            Textify.SayNewLine()
                        End If
                        Textify.SayNewLine()
                    End If

                    ' Now edit 
                    If StringInList(MySwitches, "e") Then
                        OpenInTextEditor(MyConstants.ConfigFilename, WorkingFolder)
                    End If

                    ' No switches, so just display
                    If MySwitches.Count = 0 Then
                        ShowFile(WorkingFolder, MyConstants.ConfigFilename)
                        Textify.SayNewLine()
                    End If


                ElseIf MyCommand.Keyword = eCommandType.[delete].ToString _
                    OrElse MyCommand.Keyword = eCommandType.directory.ToString _
                    OrElse MyCommand.Keyword = eCommandType.checkout.ToString _
                    OrElse MyCommand.Keyword = eCommandType.[generate].ToString _
                    OrElse MyCommand.Keyword = eCommandType.edit.ToString _
                    OrElse MyCommand.Keyword = eCommandType.fix.ToString _
                    OrElse MyCommand.Keyword = eCommandType.compare.ToString Then


                    Dim FileLimit As Integer = Integer.MaxValue
                    Dim action As eFileAction = eFileAction.directory
                    Dim bp As New BatchParametersClass
                    Dim targetftype As SquealerObjectType.eType = SquealerObjectType.eType.Invalid ' for object conversion only
                    Dim todayonly As Boolean = StringInList(MySwitches, "today")
                    Dim gf As New GitFlags()
                    gf.ShowUncommitted = StringInList(MySwitches, "u")

                    Dim pretty As Boolean = False

                    If Not MyCommand.ParameterRequired AndAlso String.IsNullOrWhiteSpace(UserInput) Then
                        UserInput = "*"
                    End If

                    Dim usedialog As Boolean = False
                    If Not String.IsNullOrWhiteSpace(UserInput) AndAlso UserInput = "#" Then
                        usedialog = True
                        UserInput = "*"
                    End If


                    If MyCommand.Keyword = eCommandType.delete.ToString Then

                        action = eFileAction.delete

                        If StringInList(MySwitches, "e") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.permanent
                        End If

                        FileLimit = 20



                    ElseIf MyCommand.Keyword = eCommandType.checkout.ToString Then

                        action = eFileAction.checkout
                        gf.ShowUncommitted = True
                        gf.ShowDeleted = True


                        'FileLimit = 20




                    ElseIf MyCommand.Keyword = eCommandType.directory.ToString Then

                        If gf.ShowUncommitted Then
                            gf.ShowDeleted = True
                        End If
                        If StringInList(MySwitches, "h") Then
                            gf.ShowHistory = True
                        End If
                        If StringInList(MySwitches, "f") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.flags
                        ElseIf StringInList(MySwitches, "str") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.string
                        End If


                    ElseIf MyCommand.Keyword = eCommandType.edit.ToString Then

                        action = eFileAction.edit

                        FileLimit = 10


                    ElseIf MyCommand.Keyword = eCommandType.generate.ToString Then

                        action = eFileAction.generate

                        If StringInList(MySwitches, "m:t") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.test
                            FileLimit = 1
                        ElseIf StringInList(MySwitches, "m:e") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.encrypt
                        ElseIf StringInList(MySwitches, "m:alt") Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.alter
                        End If


                    ElseIf MyCommand.Keyword = eCommandType.fix.ToString Then

                        action = eFileAction.fix

                        If StringInList(MySwitches, "format") Then
                            pretty = True
                        End If

                        Dim convertswitch As String = MySwitches.Find(Function(x) x.Split(New Char() {":"c})(0).ToLower = "c")
                        If Not String.IsNullOrWhiteSpace(convertswitch) Then
                            targetftype = SquealerObjectType.Eval(convertswitch.Split(New Char() {":"c})(1))
                        End If

                        If Not targetftype = SquealerObjectType.eType.Invalid Then
                            bp.OutputMode = BatchParametersClass.eOutputMode.convert
                        End If

                    ElseIf MyCommand.Keyword = eCommandType.compare.ToString Then

                        action = eFileAction.compare

                    End If




                    Dim ignoreCase As Boolean = Not StringInList(MySwitches, "cs")
                    Dim findexact As Boolean = StringInList(MySwitches, "x")
                    Dim ignorefilelimit As Boolean = StringInList(MySwitches, "all")
                    Dim findPrePost As Boolean = StringInList(MySwitches, "code")

                    Dim SelectedFiles As List(Of String) = FilesToProcess(WorkingFolder, UserInput, MySearchText, usedialog, ObjectTypeFilter, ignoreCase, findexact, todayonly, findPrePost, gf)

                    ThrowErrorIfOverFileLimit(FileLimit, SelectedFiles.Count, ignorefilelimit)

                    ProcessFiles(SelectedFiles, action, bp, targetftype, gf, pretty)




                ElseIf MyCommand.Keyword = eCommandType.[forget].ToString AndAlso StringInList(MySwitches, "auto") AndAlso String.IsNullOrEmpty(UserInput) Then

                    AutoRemoveFolders()
                    Textify.SayNewLine()

                ElseIf MyCommand.Keyword = eCommandType.[forget].ToString AndAlso Not StringInList(MySwitches, "auto") AndAlso Not String.IsNullOrEmpty(UserInput) Then

                    ForgetFolder(UserInput)
                    Textify.SayNewLine()




                ElseIf MyCommand.Keyword = eCommandType.[help].ToString Then

                    If String.IsNullOrEmpty(UserInput) Then
                        MyCommands.ShowHelpCatalog()
                    Else
                        Dim HelpWithCommand As CommandCatalog.CommandDefinition = MyCommands.FindCommand(UserInput)

                        If HelpWithCommand IsNot Nothing Then
                            HelpWithCommand.ShowHelp()
                        Else
                            Throw New Exception("Unknown command.")
                        End If
                    End If





                ElseIf MyCommand.Keyword = eCommandType.[list].ToString Then

                    ListFolders(WorkingFolder)


                ElseIf MyCommand.Keyword = eCommandType.[new].ToString Then

                    Dim filetype As SquealerObjectType.eType = SquealerObjectType.eType.StoredProcedure
                    If ObjectTypeFilter.SelectedCount > 0 Then
                        filetype = ObjectTypeFilter.Items.Find(Function(x) x.Selected).LongType
                    End If

                    BracketCheck(UserInput)

                    Dim f As String = CreateNewFile(WorkingFolder, filetype, UserInput)

                    If UserSettings.EditNew AndAlso Not String.IsNullOrEmpty(f) Then
                        ShellOpenFile(f)
                    End If


                ElseIf MyCommand.Keyword = eCommandType.[open].ToString Then

                    If String.IsNullOrWhiteSpace(UserInput) Then
                        Dim f As New System.Windows.Forms.FolderBrowserDialog
                        f.ShowDialog()
                        UserInput = f.SelectedPath
                        Textify.SayBulletLine(Textify.eBullet.Carat, UserInput)
                    End If
                    ChangeFolder(UserInput, WorkingFolder)


                ElseIf MyCommand.Keyword = eCommandType.[raiserror].ToString Then

                    Console.WriteLine(My.Resources.RaiseErrors)



                ElseIf MyCommand.Keyword = eCommandType.setting.ToString Then

                    ShowSettingsDialog()




                ElseIf MyCommand.Keyword = eCommandType.explore.ToString Then

                    OpenExplorer(WildcardInterpreter(UserInput, False), WorkingFolder)





                ElseIf MyCommand.Keyword = eCommandType.[use].ToString Then

                    LoadFolder(UserInput, WorkingFolder)


                ElseIf MyCommand.Keyword = eCommandType.starwars.ToString Then

                    If StringInList(MySwitches, "top") Then
                        ShowLeaderboard(20)
                    Else
                        Dim fgColor As ConsoleColor = Console.ForegroundColor
                        Dim bgColor As ConsoleColor = Console.BackgroundColor
                        Dim fight As New GoldLeader(False)
                        fight.TryPlay(UserSettings.LeaderboardConnectionString)
                        Console.ForegroundColor = fgColor
                        Console.BackgroundColor = bgColor
                        Console.WriteLine()
                    End If



                ElseIf MyCommand.Keyword = eCommandType.connection.ToString AndAlso StringInList(MySwitches, "set") AndAlso Not String.IsNullOrEmpty(UserInput) Then
                    SetConnectionString(WorkingFolder, UserInput)
                ElseIf MyCommand.Keyword = eCommandType.connection.ToString AndAlso StringInList(MySwitches, "show") AndAlso String.IsNullOrEmpty(UserInput) Then
                    Textify.SayBulletLine(Textify.eBullet.Arrow, GetConnectionString(WorkingFolder))
                    Textify.SayNewLine()
                ElseIf MyCommand.Keyword = eCommandType.connection.ToString AndAlso (StringInList(MySwitches, "t") OrElse MySwitches.Count = 0) AndAlso String.IsNullOrEmpty(UserInput) Then
                    TestConnectionString(WorkingFolder)
                ElseIf MyCommand.Keyword = eCommandType.connection.ToString AndAlso StringInList(MySwitches, "forget") AndAlso String.IsNullOrEmpty(UserInput) Then
                    ForgetConnectionString(WorkingFolder)
                ElseIf MyCommand.Keyword = eCommandType.connection.ToString AndAlso StringInList(MySwitches, "e") AndAlso String.IsNullOrEmpty(UserInput) Then
                    Dim cs As String
                    Try
                        cs = GetConnectionString(WorkingFolder)
                    Catch ex As Exception
                        cs = cDefaultConnectionString
                    End Try
                    cs = Microsoft.VisualBasic.Interaction.InputBox("Connection String", "", cs)
                    If Not String.IsNullOrWhiteSpace(cs) Then
                        SetConnectionString(WorkingFolder, cs)
                        Textify.SayBulletLine(Textify.eBullet.Arrow, cs)
                        Textify.SayNewLine()
                    End If



                ElseIf MyCommand.Keyword = eCommandType.make.ToString Then


                    Automagic(GetConnectionString(WorkingFolder), WorkingFolder, StringInList(MySwitches, "r"), Not StringInList(MySwitches, "nocomment"))




                ElseIf MyCommand.Keyword = eCommandType.reverse.ToString Then

                    ReverseEngineer(GetConnectionString(WorkingFolder), WorkingFolder, StringInList(MySwitches, "clean"))




                ElseIf MyCommand.Keyword = "test" Then 'footest


                    If StringInList(MySwitches, "release") Then
                        Dim s As String = WorkingFolder & "\ver.txt"
                        My.Computer.FileSystem.WriteAllText(s, My.Application.Info.Version.ToString, False)
                        Console.WriteLine()
                        Console.WriteLine()
                        Console.WriteLine("generated " & s & " with " & My.Application.Info.Version.ToString)
                        Console.WriteLine()
                    End If



                    Dim scriptText As String = "dir"

                    Dim runspace As Runspace = RunspaceFactory.CreateRunspace()
                    runspace.Open()
        Dim pipeline As Pipeline = runspace.CreatePipeline()
                    pipeline.Commands.AddScript(scriptText)
                    pipeline.Commands.Add("Out-String")
                    Dim results As Collection(Of PSObject) = pipeline.Invoke()
                    runspace.Close()
                    Dim stringBuilder As Text.StringBuilder = New Text.StringBuilder()

                    For Each obj As PSObject In results
                        stringBuilder.AppendLine(obj.ToString())
                    Next

                    Console.WriteLine(stringBuilder)










                Else
                    Throw New System.Exception(BadCommandMessage)
                End If

            Catch ex As Exception

                Textify.SayError(ex.Message)

                If MyCommand Is Nothing Then
                    Textify.SayBulletLine(Textify.eBullet.Hash, "Try: HELP")
                Else
                    Textify.SayBulletLine(Textify.eBullet.Hash, "Try: HELP " & MyCommand.Keyword.ToUpper)
                End If

                Textify.SayNewLine()

            End Try

            Dim ProjectName As String = GetProjectNickname(WorkingFolder)

            Console.Title = String.Format("[{0}] {1} - {2}", ProjectName, WorkingFolder, My.Application.Info.Title) ' Info may have changed. Update the title bar on every pass. 

            Textify.Write(String.Format("[{0}]", ProjectName), ConsoleColor.DarkYellow)
            If UserSettings.ShowBranch Then
                Textify.Write(CurrentBranch(WorkingFolder, " ({0})"), ConsoleColor.DarkGreen)
            End If
            Textify.Write(" > ", ConsoleColor.DarkYellow)
            ClearKeyboard()
            UserInput = Console.ReadLine
            Textify.SayNewLine()

            ' Separate command text from search text
            If UserInput.Contains("/") Then
                Dim n As Integer = UserInput.IndexOf("/")
                MySearchText = UserInput.Substring(n + 1)
                UserInput = UserInput.Substring(0, n)
            Else
                MySearchText = String.Empty
            End If


            Dim keyword As String = UserInput.Trim.Split(New Char() {" "c})(0) 'get the first solid word
            MyCommand = MyCommands.FindCommand(keyword)



            Dim SplitInput As New List(Of String)
            SplitInput.AddRange(UserInput.Trim.Split(New Char() {" "c}))
            UserInput = String.Empty
            MySwitches.Clear()

            ' Go through each piece of the user command and pull out all the switches.
            While SplitInput.Count > 0

                Dim rawinput As String = SplitInput(0)

                If rawinput.StartsWith("-") AndAlso Not MyCommand.IgnoreSwitches Then ' -Ex:Opt:JUNK

                    Dim switchinput As String = rawinput.Remove(0, 1).ToLower ' -Ex:Opt:JUNK -> ex:opt:junk

                    Dim switchkeyword As String = switchinput.Split(New Char() {":"c})(0) ' ex:opt:junk -> ex
                    Dim switchoption As String = String.Empty
                    If switchinput.Contains(":") Then
                        switchoption = ":" & switchinput.Split(New Char() {":"c})(1) ' ex:opt:junk -> :opt
                    End If

                    MySwitches.Add(switchkeyword & switchoption)

                Else
                    UserInput &= " " & rawinput
                End If

                SplitInput.RemoveAt(0)

            End While


            ' Separate the command from everything after it
            UserInput = UserInput.Trim
            If String.IsNullOrEmpty(UserInput) Then
                MyCommand = MyCommands.FindCommand(eCommandType.nerfherder.ToString) 'this is a dummy command just so the command object is not Nothing
            Else
                'Dim keyword As String = UserInput.Split(New Char() {" "c})(0)
                keyword = UserInput.Split(New Char() {" "c})(0)
                'MyCommand = MyCommands.FindCommand(keyword)
                UserInput = UserInput.Remove(0, keyword.Length).Trim
            End If









            ' Test the switches
            SwitchesValidated = True
            If MyCommand IsNot Nothing Then
                For Each s As String In MySwitches

                    Dim opt As CommandCatalog.CommandSwitch = MyCommand.Options.Items.Find(Function(x) x.Keyword = s.Split(New Char() {":"c})(0))

                    If opt Is Nothing Then ' Is the switch legit?
                        SwitchesValidated = False
                    Else

                        If opt.Options.Items.Count > 0 AndAlso Not s.Contains(":") Then ' Did user omit required switch option?
                            SwitchesValidated = False
                        End If

                        If s.Contains(":") Then
                            If Not opt.Options.Items.Exists(Function(x) x.Keyword = s.Split(New Char() {":"c})(1)) Then ' Is the switch option legit?
                                SwitchesValidated = False
                            End If
                        End If

                    End If
                Next
            End If

            ' Move object types from the switch list to the object type list
            ObjectTypeFilter.SetAllFlags(False)

            While MySwitches.Exists(Function(x) SquealerObjectType.Validated(x))
                Dim t As SquealerObjectType.eType = SquealerObjectType.Eval(MySwitches.Find(Function(x) Not SquealerObjectType.Eval(x) = SquealerObjectType.eType.Invalid))
                ObjectTypeFilter.SetOneFlag(t, True)
                MySwitches.Remove(SquealerObjectType.ToShortType(t).ToString)
            End While


            If ObjectTypeFilter.NoneSelected Then
                ObjectTypeFilter.SetAllFlags(True)
            End If

        End While

    End Sub

    Private Function GetFileList(ByVal WhichFolder As String) As List(Of String)

        Dim dialog As New OpenFileDialog

        dialog.FileName = "" ' Default file name
        dialog.DefaultExt = ".sqlr" ' Default file extension
        dialog.Filter = "Text Files (*.sqlr)|*.sqlr"
        dialog.Multiselect = True
        dialog.RestoreDirectory = True
        dialog.InitialDirectory = WhichFolder

        ' Show open file dialog box
        dialog.ShowDialog()

        Dim fnames As New List(Of String)
        For Each fn As String In dialog.FileNames
            fnames.Add(fn)
        Next

        Return fnames

    End Function

#End Region

#Region " Settings "

    Private Sub SettingViewOne(name As String, value As String)
        Textify.SayBullet(Textify.eBullet.Arrow, "")
        Textify.Write(name, ConsoleColor.Green)
        Textify.Write(": ")
        Textify.WriteLine(value, ConsoleColor.White)
        Console.WriteLine()
    End Sub

    Private Sub ShowSettingsDialog()

        Dim f As New SquealerSettings
        f.txtLeaderboardCs.Text = UserSettings.LeaderboardConnectionString
        f.updnFolderSaves.Value = UserSettings.RecentFolders
        f.optUseWildcards.Checked = UserSettings.AutoSearch
        f.optEditNewFiles.Checked = UserSettings.EditNew
        f.chkShowLeaderboard.Checked = UserSettings.ShowLeaderboardAtStartup
        If UserSettings.UseClipboard Then
            f.rbClipboard.Checked = True
        Else
            f.rbTempFile.Checked = True
        End If
        f.optShowGitBranch.Checked = UserSettings.ShowBranch
        f.optSpacesAreWildcards.Checked = UserSettings.WildcardSpaces
        f.optBeep.Checked = Textify.ErrorAlert.Beep
        f.optDetectOldSquealerObjects.Checked = UserSettings.DetectSquealerObjects
        Select Case UserSettings.DirStyle
            Case eDirectoryStyle.compact.ToString
                f.rbCompact.Checked = True
            Case eDirectoryStyle.full.ToString
                f.rbFull.Checked = True
            Case eDirectoryStyle.symbolic.ToString
                f.rbSymbolic.Checked = True
        End Select

        f.ShowDialog()

        UserSettings.LeaderboardConnectionString = f.txtLeaderboardCs.Text
        UserSettings.RecentFolders = CInt(f.updnFolderSaves.Value)
        UserSettings.AutoSearch = f.optUseWildcards.Checked
        UserSettings.EditNew = f.optEditNewFiles.Checked
        UserSettings.ShowLeaderboardAtStartup = f.chkShowLeaderboard.Checked
        UserSettings.UseClipboard = f.rbClipboard.Checked
        UserSettings.ShowBranch = f.optShowGitBranch.Checked
        UserSettings.WildcardSpaces = f.optSpacesAreWildcards.Checked
        Textify.ErrorAlert.Beep = f.optBeep.Checked
        UserSettings.DetectSquealerObjects = f.optDetectOldSquealerObjects.Checked
        If f.rbCompact.Checked Then
            UserSettings.DirStyle = eDirectoryStyle.compact.ToString
        ElseIf f.rbFull.Checked Then
            UserSettings.DirStyle = eDirectoryStyle.full.ToString
        Else
            UserSettings.DirStyle = eDirectoryStyle.symbolic.ToString
        End If

        My.Configger.SaveSetting(NameOf(UserSettings.LeaderboardConnectionString), UserSettings.LeaderboardConnectionString)
        My.Configger.SaveSetting(NameOf(UserSettings.RecentFolders), UserSettings.RecentFolders)
        My.Configger.SaveSetting(NameOf(UserSettings.AutoSearch), UserSettings.AutoSearch)
        My.Configger.SaveSetting(NameOf(UserSettings.EditNew), UserSettings.EditNew)
        My.Configger.SaveSetting(NameOf(UserSettings.ShowLeaderboardAtStartup), UserSettings.ShowLeaderboardAtStartup)
        My.Configger.SaveSetting(NameOf(UserSettings.UseClipboard), UserSettings.UseClipboard)
        My.Configger.SaveSetting(NameOf(UserSettings.DetectSquealerObjects), UserSettings.DetectSquealerObjects)
        My.Configger.SaveSetting(NameOf(UserSettings.ShowBranch), UserSettings.ShowBranch)
        My.Configger.SaveSetting(NameOf(UserSettings.WildcardSpaces), UserSettings.WildcardSpaces)
        My.Configger.SaveSetting(NameOf(Textify.ErrorAlert.Beep), Textify.ErrorAlert.Beep)
        My.Configger.SaveSetting(NameOf(UserSettings.DirStyle), UserSettings.DirStyle)

    End Sub

#End Region

#Region " XML Default Values "

    ' If the first parameter is null, return the second.
    Private Function AttributeDefaultBoolean(ByVal attr As Xml.XmlNode, ByVal deefalt As Boolean) As Boolean

        If attr Is Nothing Then
            Return deefalt
        Else
            Return CBool(IIf(String.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value))
        End If

    End Function

    ' If the first parameter is null, return the second.
    Private Function AttributeDefaultString(ByVal attr As Xml.XmlNode, ByVal deefalt As String) As String

        If attr Is Nothing Then
            Return deefalt
        Else
            Return IIf(String.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value).ToString
        End If

    End Function

    ' If the first parameter is null, return the second.
    Private Function AttributeDefaultInteger(ByVal attr As Xml.XmlNode, ByVal deefalt As Integer) As Integer

        If attr Is Nothing Then
            Return deefalt
        Else
            Return Integer.Parse(IIf(String.IsNullOrWhiteSpace(attr.Value), deefalt, attr.Value).ToString)
        End If

    End Function

#End Region

#Region " XML Reading "

    Private Function XmlGetObjectType(ByVal FileName As String) As String

        Try
            Dim Reader As New Xml.XmlDocument
            Reader.Load(FileName)
            Dim Node As Xml.XmlNode = Reader.SelectSingleNode("/" & My.Application.Info.ProductName)

            ' Get the type.
            Return Node.Attributes("Type").Value.ToString
        Catch ex As Exception
            Return SquealerObjectType.eShortType.err.ToString
        End Try

    End Function

    Private Function PrePostCodeExists(FileName As String) As Boolean

        Dim InputXml As Xml.XmlDocument = New Xml.XmlDocument

        InputXml.Load(FileName)

        Dim InRoot As Xml.XmlElement = DirectCast(InputXml.SelectSingleNode(My.Application.Info.ProductName), Xml.XmlElement)

        Dim HasCode As Boolean = False

        Try
            If Not String.IsNullOrWhiteSpace(InRoot.SelectSingleNode("PreCode").InnerText) Then
                HasCode = True
            End If
        Catch ex As Exception
        End Try
        Try
            If Not String.IsNullOrWhiteSpace(InRoot.SelectSingleNode("PostCode").InnerText) Then
                HasCode = True
            End If
        Catch ex As Exception
        End Try

        Return HasCode

    End Function


    ' Get all the parameters.
    Private Function GetParameters(ByVal InXml As Xml.XmlDocument) As DataTable

        Dim Parameters As New DataTable

        With Parameters.Columns
            .Add("Name", GetType(String))
            .Add("Type", GetType(String))
            .Add("Output", GetType(Boolean))
            .Add("DefaultValue", GetType(String))
            .Add("Comments", GetType(String))
        End With

        For Each Node As Xml.XmlNode In InXml.SelectNodes(My.Application.Info.ProductName & "/Parameters/Parameter")

            Parameters.Rows.Add(
                AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), String.Empty),
                AttributeDefaultString(Node.Attributes.GetNamedItem("Type"), String.Empty),
                AttributeDefaultString(Node.Attributes.GetNamedItem("Output"), Boolean.FalseString),
                AttributeDefaultString(Node.Attributes.GetNamedItem("DefaultValue"), String.Empty),
                AttributeDefaultString(Node.Attributes.GetNamedItem("Comments"), String.Empty)
            )

        Next

        GetParameters = Parameters

    End Function

    ' Get all the table columns.
    Private Function GetColumns(ByVal InXml As Xml.XmlDocument) As DataTable

        Dim Columns As New DataTable

        With Columns.Columns
            .Add("Name", GetType(String))
            .Add("Type", GetType(String))
            .Add("Nullable", GetType(Boolean))
            .Add("Identity", GetType(Boolean))
            .Add("IncludeInPrimaryKey", GetType(Boolean))
            .Add("Comments", GetType(String))
        End With

        For Each Node As Xml.XmlNode In InXml.SelectNodes(My.Application.Info.ProductName & "/Table/Column")

            Columns.Rows.Add(
                    AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), String.Empty),
                    AttributeDefaultString(Node.Attributes.GetNamedItem("Type"), String.Empty),
                    AttributeDefaultBoolean(Node.Attributes.GetNamedItem("Nullable"), True),
                    AttributeDefaultBoolean(Node.Attributes.GetNamedItem("Identity"), False),
                    AttributeDefaultBoolean(Node.Attributes.GetNamedItem("IncludeInPrimaryKey"), False),
                    AttributeDefaultString(Node.Attributes.GetNamedItem("Comments"), String.Empty)
                )
        Next

        GetColumns = Columns

    End Function

    ' Get all the users.
    Private Function GetUsers(ByVal InXml As Xml.XmlDocument) As DataTable

        Dim Users As New DataTable

        With Users.Columns
            .Add("Name", GetType(String))
        End With

        For Each Node As Xml.XmlNode In InXml.SelectNodes(My.Application.Info.ProductName & "/Users/User")

            Users.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), String.Empty))

        Next

        GetUsers = Users

    End Function

#End Region

#Region " XML Processing "

    ' Load and clean up the XML keeping the original file type.
    Private Function FixedXml(ByVal ApplyDefaultUsers As Boolean, fqfn As String, MakePretty As Boolean) As Xml.XmlDocument
        Dim obj As New SquealerObject(fqfn)
        Return FixedXml(ApplyDefaultUsers, fqfn, obj, MakePretty)
    End Function

    ' Load and clean up the XML using the specified target file type.
    Private Function FixedXml(ByVal ApplyDefaultUsers As Boolean, fqfn As String, ByVal obj As SquealerObject, ByVal MakePretty As Boolean) As Xml.XmlDocument

        Dim OutputXml As Xml.XmlDocument = New Xml.XmlDocument

        Dim InputXml As Xml.XmlDocument = New Xml.XmlDocument

        InputXml.Load(fqfn)

        Dim InRoot As Xml.XmlElement = DirectCast(InputXml.SelectSingleNode(My.Application.Info.ProductName), Xml.XmlElement)

        OutputXml.AppendChild(OutputXml.CreateXmlDeclaration("1.0", "us-ascii", Nothing))

        ' Header
        OutputXml.AppendChild(OutputXml.CreateComment(" Flags example: ""x;exclude from project|r;needs refactoring"" (recommend single-character flags) "))

        Dim OutRoot As Xml.XmlElement = OutputXml.CreateElement(My.Application.Info.ProductName)
        OutputXml.AppendChild(OutRoot)

        OutRoot.SetAttribute("Type", obj.Type.LongType.ToString)
        OutRoot.SetAttribute("Flags", obj.Flags)
        OutRoot.SetAttribute("WithOptions", obj.WithOptions)

        ' Pre-Code.
        Dim OutPreCode As Xml.XmlElement = OutputXml.CreateElement("PreCode")
        Dim CDataPreCode As Xml.XmlCDataSection = OutputXml.CreateCDataSection("") ' CData disables the XML parser so that special characters can exist in the inner text.

        OutRoot.AppendChild(OutputXml.CreateComment(" Optional T-SQL to execute before the main object is created. "))
        OutRoot.AppendChild(OutPreCode)

        Dim InPreCode As String = String.Empty
        Try
            InPreCode = InRoot.SelectSingleNode("PreCode").InnerText
        Catch ex As Exception
            InPreCode = ""
        End Try

        CDataPreCode.InnerText = String.Concat(vbCrLf, vbCrLf, InPreCode.Trim, vbCrLf, vbCrLf)
        OutPreCode.AppendChild(CDataPreCode)


        ' Comments.
        ' -- comment help--
        Dim CommentHelp As Xml.XmlComment = Nothing

        Select Case obj.Type.LongType
            Case SquealerObjectType.eType.StoredProcedure
                CommentHelp = OutputXml.CreateComment(" Describe the purpose of this procedure, the return values, and any difficult concepts. ")
            Case SquealerObjectType.eType.InlineTableFunction, SquealerObjectType.eType.MultiStatementTableFunction
                CommentHelp = OutputXml.CreateComment(" Describe the output of this view and any difficult concepts. ")
            Case SquealerObjectType.eType.ScalarFunction
                CommentHelp = OutputXml.CreateComment(" Describe the output of this scalar function and any difficult concepts. ")
            Case SquealerObjectType.eType.View
                CommentHelp = OutputXml.CreateComment(" Describe the output of this view and any difficult concepts. ")
            Case Else
                Throw New Exception("Missing or invalid object type. Check: <Squealer Type=""???""> in " & fqfn)
        End Select
        OutRoot.AppendChild(CommentHelp)

        ' -- actual comment--
        Dim OutComments As Xml.XmlElement = OutputXml.CreateElement("Comments")
        Dim CDataComment As Xml.XmlCDataSection = OutputXml.CreateCDataSection("") ' CData disables the XML parser so that special characters can exist in the inner text.
        OutRoot.AppendChild(OutComments)

        Try
            CDataComment.InnerText = InRoot.SelectSingleNode("Comments").InnerText.Replace("/*", String.Empty).Replace("*/", String.Empty)
        Catch ex As Exception
            CDataComment.InnerText = String.Empty
        End Try

        CDataComment.InnerText = String.Concat(vbCrLf, vbCrLf, CDataComment.InnerText.Trim, vbCrLf, vbCrLf)

        OutComments.AppendChild(CDataComment)


        ' Parameters.
        If Not obj.Type.LongType = SquealerObjectType.eType.View Then

            Dim OutParameters As Xml.XmlElement = OutputXml.CreateElement("Parameters")
            OutRoot.AppendChild(OutParameters)

            Dim InParameters As DataTable = GetParameters(InputXml)

            If InParameters.Rows.Count = 0 Then
                OutParameters.AppendChild(OutputXml.CreateComment("<Parameter Name="" MyParameter"" Type="" varchar(50)"" " & IIf(obj.Type.LongType = SquealerObjectType.eType.StoredProcedure, "Output=""False"" ", String.Empty).ToString & "DefaultValue="""" Comments="""" />"))
            Else
                For Each InParameter As DataRow In InParameters.Select()
                    Dim OutParameter As Xml.XmlElement = OutputXml.CreateElement("Parameter")
                    OutParameter.SetAttribute("Name", InParameter.Item("Name").ToString)
                    OutParameter.SetAttribute("Type", InParameter.Item("Type").ToString)
                    If obj.Type.LongType = SquealerObjectType.eType.StoredProcedure Then
                        OutParameter.SetAttribute("Output", InParameter.Item("Output").ToString)
                    End If
                    OutParameter.SetAttribute("DefaultValue", InParameter.Item("DefaultValue").ToString)
                    OutParameter.SetAttribute("Comments", InParameter.Item("Comments").ToString)
                    OutParameters.AppendChild(OutParameter)
                Next
            End If

        End If

        ' Returns.
        If obj.Type.LongType = SquealerObjectType.eType.ScalarFunction Then
            OutRoot.AppendChild(OutputXml.CreateComment(" Define the data type For @Result, your scalar Return variable. "))
            Dim OutReturns As Xml.XmlElement = OutputXml.CreateElement("Returns")
            OutRoot.AppendChild(OutReturns)
            Dim Returns As String = Nothing
            Try
                Returns = DirectCast(InRoot.SelectSingleNode("Returns"), Xml.XmlElement).GetAttribute("Type")
            Catch ex As Exception
                Returns = String.Empty
            End Try
            OutReturns.SetAttribute("Type", Returns)
        End If

        ' Table.
        If obj.Type.LongType = SquealerObjectType.eType.MultiStatementTableFunction OrElse obj.Type.LongType = SquealerObjectType.eType.View Then

            If obj.Type.LongType = SquealerObjectType.eType.MultiStatementTableFunction Then
                OutRoot.AppendChild(OutputXml.CreateComment(" Define the column(s) for @TableValue, your table-valued return variable. "))
            Else
                OutRoot.AppendChild(OutputXml.CreateComment(" Define the column(s) to return from this view. "))
            End If
            Dim OutTable As Xml.XmlElement = OutputXml.CreateElement("Table")
            OutRoot.AppendChild(OutTable)

            If obj.Type.LongType = SquealerObjectType.eType.MultiStatementTableFunction Then
                Dim Clustered As Boolean = False
                Try
                    Clustered = CBool(DirectCast(InRoot.SelectSingleNode("Table"), Xml.XmlElement).GetAttribute("PrimaryKeyClustered"))
                Catch ex As Exception
                    Clustered = False
                End Try
                OutTable.SetAttribute("PrimaryKeyClustered", Clustered.ToString)
            End If

            Dim InColumns As DataTable = GetColumns(InputXml)

            If InColumns.Rows.Count = 0 Then
                ' Create a dummy/example column.
                If obj.Type.LongType = SquealerObjectType.eType.MultiStatementTableFunction Then
                    Dim OutColumn As Xml.XmlElement = OutputXml.CreateElement("Column")
                    OutColumn.SetAttribute("Name", "MyColumn")
                    OutColumn.SetAttribute("Type", "varchar(50)")
                    OutColumn.SetAttribute("Nullable", "False")
                    OutColumn.SetAttribute("Identity", "False")
                    OutColumn.SetAttribute("IncludeInPrimaryKey", "False")
                    OutColumn.SetAttribute("Comments", "")
                    OutTable.AppendChild(OutColumn)
                Else
                    OutTable.AppendChild(OutputXml.CreateComment("<Column Name="" MyColumn"" Comments=""""/>"))
                End If
            Else
                For Each InColumn As DataRow In InColumns.Select()
                    Dim Nullable As Boolean = CBool(InColumn.Item("Nullable"))
                    Dim Identity As Boolean = CBool(InColumn.Item("Identity"))
                    Dim IncludeInPrimaryKey As Boolean = CBool(InColumn.Item("IncludeInPrimaryKey"))
                    Dim Type As String = InColumn.Item("Type").ToString
                    If Type.IndexOf(" Not null") > 0 Then
                        Nullable = False
                        Type = Type.Replace(" Not null", String.Empty)
                    ElseIf Type.IndexOf(" null") > 0 Then
                        Nullable = True
                        Type = Type.Replace(" null", String.Empty)
                    End If
                    Dim OutColumn As Xml.XmlElement = OutputXml.CreateElement("Column")
                    OutColumn.SetAttribute("Name", InColumn.Item("Name").ToString)
                    If obj.Type.LongType = SquealerObjectType.eType.MultiStatementTableFunction Then
                        OutColumn.SetAttribute("Type", Type)
                        OutColumn.SetAttribute("Nullable", Nullable.ToString)
                        OutColumn.SetAttribute("Identity", Identity.ToString)
                        OutColumn.SetAttribute("IncludeInPrimaryKey", IncludeInPrimaryKey.ToString)
                    End If
                    OutColumn.SetAttribute("Comments", InColumn.Item("Comments").ToString)
                    OutTable.AppendChild(OutColumn)
                Next
            End If

        End If

        ' Code.
        Dim OutCode As Xml.XmlElement = OutputXml.CreateElement("Code")
        Dim CDataCode As Xml.XmlCDataSection = OutputXml.CreateCDataSection("") ' CData disables the XML parser so that special characters can exist in the inner text.

        OutRoot.AppendChild(OutCode)

        Dim InCode As String = String.Empty
        Try
            InCode = InRoot.SelectSingleNode("Code").InnerText
            If MakePretty Then
                InCode = BeautifiedCode(InCode)
            End If
        Catch ex As Exception
            InCode = String.Empty
        End Try

        If InCode.Trim = String.Empty Then
            InCode = "/***********************************************************************" _
                & vbCrLf & vbTab & "Comments." _
                & vbCrLf & "***********************************************************************/" _
                & vbCrLf & vbCrLf
            Select Case obj.Type.LongType
                Case SquealerObjectType.eType.InlineTableFunction
                    InCode &= "select 'hello world! love, {THIS}' as [MyColumn]"
                Case SquealerObjectType.eType.MultiStatementTableFunction
                    InCode &= "insert @TableValue select 'hello world! love, {THIS}'"
                Case SquealerObjectType.eType.ScalarFunction
                    InCode &= "set @Result = 'hello world! love, {THIS}'"
                Case SquealerObjectType.eType.StoredProcedure
                    InCode &= "select 'hello world! love, {THIS}'"
                Case SquealerObjectType.eType.View
                    InCode &= "select 'hello world! love, {THIS}' as hello"
            End Select




        End If

        CDataCode.InnerText = String.Concat(vbCrLf, vbCrLf, InCode.Trim, vbCrLf, vbCrLf)
        OutCode.AppendChild(CDataCode)


        ' Users.
        Dim OutUsers As Xml.XmlElement = OutputXml.CreateElement("Users")
        OutRoot.AppendChild(OutUsers)
        Dim InUsers As DataTable
        If ApplyDefaultUsers Then
            InUsers = GetDefaultUsers(My.Computer.FileSystem.GetFileInfo(fqfn).DirectoryName)
        Else
            InUsers = GetUsers(InputXml)
        End If

        If InUsers.Rows.Count = 0 Then
            OutUsers.AppendChild(OutputXml.CreateComment(" <User Name="" MyUser""/> "))
        Else
            For Each User As DataRow In InUsers.Select("", "Name asc")
                Dim OutUser As Xml.XmlElement = OutputXml.CreateElement("User")
                OutUser.SetAttribute("Name", User.Item("Name").ToString)
                OutUsers.AppendChild(OutUser)
            Next
        End If


        ' Post-Code.
        Dim OutPostCode As Xml.XmlElement = OutputXml.CreateElement("PostCode")
        Dim CDataPostCode As Xml.XmlCDataSection = OutputXml.CreateCDataSection("") ' CData disables the XML parser so that special characters can exist in the inner text.

        OutRoot.AppendChild(OutputXml.CreateComment(" Optional T-SQL to execute after the main object Is created. "))
        OutRoot.AppendChild(OutPostCode)

        Dim InPostCode As String = String.Empty
        Try
            InPostCode = InRoot.SelectSingleNode("PostCode").InnerText
        Catch ex As Exception
            InPostCode = ""
        End Try

        CDataPostCode.InnerText = String.Concat(vbCrLf, vbCrLf, InPostCode.Trim, vbCrLf, vbCrLf)
        OutPostCode.AppendChild(CDataPostCode)


        FixedXml = OutputXml

    End Function

    Private Function BeautifiedCode(code As String) As String

        Dim options As New PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatterOptions
        With options
            .BreakJoinOnSections = True
            .ExpandBetweenConditions = True
            .ExpandBooleanExpressions = True
            .ExpandCaseStatements = True
            .ExpandCommaLists = True
            .ExpandInLists = True
            .IndentString = vbTab
            .KeywordStandardization = True
            .NewClauseLineBreaks = 1
            .NewStatementLineBreaks = 2
            .SpacesPerTab = 8
            .TrailingCommas = False
            .UppercaseKeywords = False
        End With

        Dim formatter As New PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(options)

        Dim beautify As New PoorMansTSqlFormatterLib.SqlFormattingManager(formatter)

        code = beautify.Format(code)

        Return code

    End Function

    ' Fix a root file and replace the original.
    Private Function ConvertXmlFile(fqfn As String, ByVal oType As SquealerObjectType.eType, MakePretty As Boolean) As Boolean

        Dim obj As New SquealerObject(fqfn)
        obj.Type.LongType = oType
        Dim NewXml As Xml.XmlDocument = FixedXml(False, fqfn, obj, MakePretty) ' Fix it.

        Return IsXmlReplaced(fqfn, NewXml)

    End Function

    ' Fix a root file and replace the original.
    Private Function RepairXmlFile(ByVal IsNew As Boolean, fqfn As String, MakePretty As Boolean) As Boolean
        Dim NewXml As Xml.XmlDocument = FixedXml(IsNew, fqfn, MakePretty) ' Fix it.
        Return IsXmlReplaced(fqfn, NewXml)
    End Function

    Private Function IsXmlReplaced(existingfilename As String, newxml As Xml.XmlDocument) As Boolean

        Dim ExistingXml As New Xml.XmlDocument
        ExistingXml.Load(existingfilename)

        Dim different As Boolean = False

        If Not newxml.InnerXml = ExistingXml.InnerXml Then
            ' Is XML different?
            newxml.Save(existingfilename)
            different = True
        Else
            ' XML is the same, but is whitespace different?
            Dim tempfilename As String = My.Computer.FileSystem.GetTempFileName
            newxml.Save(tempfilename)
            If Not My.Computer.FileSystem.ReadAllText(tempfilename) = My.Computer.FileSystem.ReadAllText(existingfilename) Then
                My.Computer.FileSystem.MoveFile(tempfilename, existingfilename, True)
                different = True
            End If
        End If

        Return different

    End Function

    ' Create a new proc or function.

    Private Function CreateNewFile(ByVal WorkingFolder As String, ByVal FileType As SquealerObjectType.eType, ByVal filename As String) As String
        Return CreateNewFile(WorkingFolder, FileType, filename, Nothing, Nothing, Nothing)
    End Function

    Private Function CreateNewFile(ByVal WorkingFolder As String, ByVal FileType As SquealerObjectType.eType, ByVal filename As String, Parameters As ParameterCollection, definition As String, userlist As List(Of String)) As String

        Dim Template As String = String.Empty
        Select Case FileType
            Case SquealerObjectType.eType.InlineTableFunction
                Template = My.Resources.SqlTemplateInlineTableFunction
            Case SquealerObjectType.eType.MultiStatementTableFunction
                Template = My.Resources.SqlTemplateMultiStatementTableFunction
            Case SquealerObjectType.eType.ScalarFunction
                Template = My.Resources.SqlTemplateScalarFunction
            Case SquealerObjectType.eType.StoredProcedure
                Template = My.Resources.SqlTemplateProcedure
            Case SquealerObjectType.eType.View
                Template = My.Resources.SqlTemplateView
        End Select
        Template = Template.Replace("{RootType}", FileType.ToString).Replace("{THIS}", MyThis)

        Dim IsNew As Boolean = True


        If Parameters Is Nothing Then
            Template = Template.Replace("{ReturnDataType}", "varchar(100)")
        Else

            IsNew = False

            If FileType = SquealerObjectType.eType.ScalarFunction Then
                Template = Template.Replace("{ReturnDataType}", Parameters.ReturnType.Type)
            End If

            Dim parms As String = String.Empty
            For Each p As ParameterClass In Parameters.Items
                parms &= String.Format("<Parameter Name="" {0}"" Type=""{1}"" Output=""{2}"" />", p.Name, p.Type, p.IsOutput.ToString)
            Next
            Template = Template.Replace("<!--Parameters-->", parms)
        End If

        If userlist IsNot Nothing Then

            IsNew = False

            Dim users As String = String.Empty
            For Each s As String In userlist
                users &= String.Format("<User Name="" {0}""/>", s)
            Next
            Template = Template.Replace("<Users/>", String.Format("<Users>{0}</Users>", users))

        End If

        ' Did user forget the "-" prefix before the object type switch? ex: tf instead of -tf
        For Each s As String In System.Enum.GetNames(GetType(SquealerObjectType.eShortType))
            If filename.ToLower.StartsWith(s.ToLower & " ") Then
                Textify.SayError("Did you mean:  " & eCommandType.[new].ToString & " -" & s & " " & filename.Remove(0, s.Length + 1), Textify.eSeverity.warning)
            End If
        Next

        ' Make sure all new programs have a schema.
        If Not filename.Contains(".") Then
            filename = String.Concat("dbo.", filename)
        End If

        'Dim fqTemp As String = My.Computer.FileSystem.GetTempFileName
        Dim fqTarget As String = WorkingFolder & "\" & filename & MyConstants.ObjectFileExtension

        If definition IsNot Nothing Then
            Template = Template.Replace("<Code/>", String.Format("<Code><![CDATA[{0}]]></Code>", definition))
        End If

        ' Careful not to overwrite existing file.
        If My.Computer.FileSystem.FileExists(fqTarget) Then
            If IsNew Then
                Textify.SayError("File already exists.")
            End If
            CreateNewFile = String.Empty
        Else
            My.Computer.FileSystem.WriteAllText(fqTarget, Template, False, System.Text.Encoding.ASCII)
            RepairXmlFile(IsNew, fqTarget, False)
            If IsNew Then
                Textify.SayBullet(Textify.eBullet.Hash, "OK")
                Textify.WriteLine(" (" & filename & ")")
            End If
            CreateNewFile = fqTarget
        End If

        If IsNew Then
            Textify.SayNewLine()
        End If

    End Function


#End Region

#Region " Proc Generation "

    ' Expand one root file.
    Private Function ExpandIndividual(info As IO.FileInfo, StringReplacements As DataTable, bp As BatchParametersClass, cur As Integer, tot As Integer) As String

        Dim oType As SquealerObjectType.eType = SquealerObjectType.Eval(XmlGetObjectType(info.FullName))
        Dim RootName As String = info.Name.Replace(MyConstants.ObjectFileExtension, "")

        Dim InXml As Xml.XmlDocument = FixedXml(False, info.FullName, False)
        Dim InRoot As Xml.XmlElement = DirectCast(InXml.SelectSingleNode(My.Application.Info.ProductName), Xml.XmlElement)
        Dim Block As String = Nothing

        Dim CodeBlocks As New List(Of String)


        ' Pre-Code
        If Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then
            Dim InPreCode As String = String.Format("print '{2}/{3} creating {0}, {1}'", MyThis, oType.ToString, cur, tot) & vbCrLf & "go" & vbCrLf
            Try
                InPreCode &= InRoot.SelectSingleNode("PreCode").InnerText
            Catch ex As Exception
            End Try

            If Not String.IsNullOrWhiteSpace(InPreCode) Then
                InPreCode = vbCrLf & "-- additional code to execute after " & oType.ToString & " is created" & vbCrLf & InPreCode
                CodeBlocks.Add(InPreCode)
            End If

        End If


        ' Drop 
        If Not bp.OutputMode = BatchParametersClass.eOutputMode.test AndAlso Not bp.OutputMode = BatchParametersClass.eOutputMode.alter Then
            CodeBlocks.Add(My.Resources.SqlDrop.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Schema}", SchemaName(RootName)).ToString)
        End If

        ' Comments
        Dim OutComments As String = Nothing
        Try
            OutComments = InRoot.SelectSingleNode("Comments").InnerText.Replace("/*", String.Empty).Replace("*/", String.Empty)
        Catch ex As Exception
            OutComments = String.Empty
        End Try
        Block = My.Resources.SqlComment.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Comments}", OutComments).Replace("{Schema}", SchemaName(RootName))

        ' Create
        If Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then
            Dim SqlCreate As String = String.Empty
            Select Case oType
                Case SquealerObjectType.eType.StoredProcedure
                    SqlCreate = My.Resources.SqlCreateProcedure
                Case SquealerObjectType.eType.ScalarFunction, SquealerObjectType.eType.InlineTableFunction, SquealerObjectType.eType.MultiStatementTableFunction
                    SqlCreate = My.Resources.SqlCreateFunction
                Case SquealerObjectType.eType.View
                    SqlCreate = My.Resources.SqlCreateView
            End Select
            If bp.OutputMode = BatchParametersClass.eOutputMode.alter Then
                SqlCreate = SqlCreate.Replace("create", "alter")
            End If
            Block &= SqlCreate.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{Schema}", SchemaName(RootName))
        End If

        ' Parameters
        Dim InParameters As DataTable = GetParameters(InXml)
        Dim ParameterCount As Int32 = 0
        Dim DeclareList As New ArrayList
        Dim SetList As New ArrayList
        Dim OutputParameters As String = String.Empty
        Dim ErrorLogParameters As String = String.Empty

        For Each Parameter As DataRow In InParameters.Select()

            Dim def As String = Nothing

            If bp.OutputMode = BatchParametersClass.eOutputMode.test Then

                def = "declare @" & Parameter.Item("Name").ToString & " " & Parameter.Item("Type").ToString
                If Parameter.Item("DefaultValue").ToString = String.Empty Then
                    def = def & " = "
                Else
                    def = def & " = " & Parameter.Item("DefaultValue").ToString
                End If
                def = def & ";"
                If Not Parameter.Item("Comments").ToString = String.Empty Then
                    def = def & " -- " & Parameter.Item("Comments").ToString
                End If

            Else

                'def = ""
                If ParameterCount = 0 Then '<InParameters.Rows.Count Then
                    def=""
                                                Else
                    def=","
                End If

        ' Write parameters as actual parameters.
        ParameterCount += 1
        def = def & "@" & Parameter.Item("Name").ToString & " " & Parameter.Item("Type").ToString
        If Not Parameter.Item("DefaultValue").ToString = String.Empty Then
            def = def & " = " & Parameter.Item("DefaultValue").ToString
        End If
        If Parameter.Item("Output").ToString = Boolean.TrueString Then
            def = def & " output"
        End If
        If Not Parameter.Item("Comments").ToString = String.Empty Then
            def = def & " -- " & Parameter.Item("Comments").ToString
        End If
        ' Write out error logging section.
        If (Parameter.Item("Type").ToString.ToLower.Contains("max") OrElse Parameter.Item("Name").ToString.ToLower.Contains(" readonly")) Then
            Dim whynot As String = vbCrLf & vbTab & vbTab & String.Format("--parameter @{0} cannot be logged due to its 'max' or 'readonly' definition", Parameter.Item("Name").ToString)
            ErrorLogParameters &= whynot
        Else
            ErrorLogParameters &= vbCrLf & My.Resources.SqlEndProcedure2.Replace("{ErrorParameterNumber}", ParameterCount.ToString).Replace("{ErrorParameterName}", Parameter.Item("Name").ToString)
        End If

        End If

        DeclareList.Add(def)

        Next
        For Each s As String In DeclareList
            Block = Block & vbCrLf & s
        Next
        For Each s As String In SetList
            Block = Block & vbCrLf & s
        Next


        ' Table (View)
        If oType = SquealerObjectType.eType.View Then

            Dim InColumns As DataTable = GetColumns(InXml)

            If InColumns.Rows.Count > 0 AndAlso Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then

                Block &= vbCrLf & "(" & vbCrLf

                Dim ColumnCount As Integer = 0

                For Each Column As DataRow In InColumns.Select()

                    Dim c As String = String.Empty
                    If ColumnCount > 0 Then
                        c = ","
                    End If

                    ColumnCount += 1

                    c &= String.Format("[{0}]", Column.Item("Name").ToString)
                    If Not Column.Item("Comments").ToString = String.Empty Then
                        c = c & " -- " & Column.Item("Comments").ToString
                    End If

                    Block &= c & vbCrLf

                Next

                Block &= vbCrLf & ")" & vbCrLf

            End If

        End If



        ' Table (MultiStatementTableFunction)
        If oType = SquealerObjectType.eType.MultiStatementTableFunction Then

            If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                Block = Block & My.Resources.SqlTableMultiStatementTableFunctionTest
            Else
                Block = Block & My.Resources.SqlTableMultiStatementTableFunction
            End If

            Dim PrimaryKey As String = String.Empty ' If this never gets filled, then there is no primary key.
            Dim Clustered As Boolean = CBool(DirectCast(InRoot.SelectSingleNode("Table"), Xml.XmlElement).GetAttribute("PrimaryKeyClustered"))

            Dim InColumns As DataTable = GetColumns(InXml)
            Dim ColumnCount As Integer = 0
            For Each Column As DataRow In InColumns.Select()

                Dim c As String = String.Empty
                If ColumnCount > 0 Then
                    c = ","
                End If

                ColumnCount += 1

                c &= String.Format("[{0}]", Column.Item("Name").ToString)
                c &= " " & Column.Item("Type").ToString & " "
                If Not CBool(Column.Item("Nullable")) Then
                    c &= "Not "
                End If
                c &= "null"
                If CBool(Column.Item("Identity")) Then
                    c &= " identity"
                End If
                If Not Column.Item("Comments").ToString = String.Empty Then
                    c = c & " -- " & Column.Item("Comments").ToString
                End If

                Block &= c & vbCrLf

                If Column.Item("IncludeInPrimaryKey").ToString = Boolean.TrueString Then
                    If PrimaryKey.Length > 0 Then
                        PrimaryKey &= ","
                    End If
                    PrimaryKey &= Column.Item("Name").ToString
                End If
            Next

            If PrimaryKey.Length > 0 Then
                Block &= "primary key " & IIf(Clustered, "clustered ", "").ToString & "(" & PrimaryKey & ")"
            End If

        End If

        ' Begin
        Dim BeginBlock As String = Nothing
        Select Case oType
            Case SquealerObjectType.eType.StoredProcedure
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    BeginBlock = My.Resources.SqlBeginProcedureTest
                Else
                    BeginBlock = My.Resources.SqlBeginProcedure
                End If
            Case SquealerObjectType.eType.ScalarFunction
                Dim Returns As String = Nothing
                Returns = DirectCast(InRoot.SelectSingleNode("Returns"), Xml.XmlElement).GetAttribute("Type")
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    BeginBlock = My.Resources.SqlBeginScalarFunctionTest.Replace("{ReturnDataType}", Returns)
                Else
                    BeginBlock = My.Resources.SqlBeginScalarFunction.Replace("{ReturnDataType}", Returns)
                End If
            Case SquealerObjectType.eType.InlineTableFunction
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    BeginBlock = String.Empty
                Else
                    BeginBlock = My.Resources.SqlBeginInlineTableFunction
                End If
            Case SquealerObjectType.eType.MultiStatementTableFunction
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    BeginBlock = My.Resources.SqlBeginMultiStatementTableFunctionTest
                Else
                    BeginBlock = My.Resources.SqlBeginMultiStatementTableFunction
                End If
            Case SquealerObjectType.eType.View
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    BeginBlock = String.Empty
                Else
                    BeginBlock = My.Resources.SqlBeginView
                End If
        End Select


        Dim obj As New SquealerObject(info.FullName)
        Dim WithOptions As String = obj.WithOptions
        If bp.OutputMode = BatchParametersClass.eOutputMode.encrypt Then
            If String.IsNullOrWhiteSpace(obj.WithOptions) Then
                WithOptions = "encryption"
            ElseIf Not WithOptions.ToLower.Contains("encryption") Then
                WithOptions = WithOptions & ",encryption"
            End If
        End If

        If String.IsNullOrWhiteSpace(WithOptions) Then
            BeginBlock = BeginBlock.Replace("{WithOptions}", String.Empty)
        Else
            BeginBlock = BeginBlock.Replace("{WithOptions}", "with " & WithOptions)
        End If

        Block &= BeginBlock

        ' Code
        Dim InCode As String = String.Empty
        Try
            InCode = InRoot.SelectSingleNode("Code").InnerText
        Catch ex As Exception
        End Try
        Block &= InCode

        ' End
        Select Case oType
            Case SquealerObjectType.eType.StoredProcedure
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    Block &= My.Resources.SqlEndProcedureTest
                Else
                    Block &= My.Resources.SqlEndProcedure1 & ErrorLogParameters & My.Resources.SqlEndProcedure3
                End If
            Case SquealerObjectType.eType.ScalarFunction
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    Block = Block & My.Resources.SqlEndScalarFunctionTest
                Else
                    Block = Block & My.Resources.SqlEndScalarFunction
                End If
            Case SquealerObjectType.eType.MultiStatementTableFunction
                If bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                    Block = Block & My.Resources.SqlEndMultiStatementTableFunctionTest
                Else
                    Block = Block & My.Resources.SqlEndMultiStatementTableFunction
                End If
            Case SquealerObjectType.eType.View
                ' nothing to add
        End Select

        ' Save the block.
        CodeBlocks.Add(Block)

        If Not bp.OutputMode = BatchParametersClass.eOutputMode.test AndAlso Not bp.OutputMode = BatchParametersClass.eOutputMode.alter Then

            Block = String.Empty

            ' Grant Execute
            Dim InUsers As DataTable = GetUsers(InXml)

            If InUsers.Rows.Count > 0 Then
                Block = String.Format("if object_id('``this``','{0}') is not null", SquealerObjectType.ToShortType(oType))
                Block &= vbCrLf & "begin"
                For Each User As DataRow In InUsers.Select("", "Name asc")
                    Dim GrantStatement As String
                    If oType = SquealerObjectType.eType.StoredProcedure OrElse oType = SquealerObjectType.eType.ScalarFunction Then
                        GrantStatement = My.Resources.SqlGrantExecute
                    Else
                        GrantStatement = My.Resources.SqlGrantSelect
                    End If
                    Block = Block & vbCrLf & GrantStatement.Replace("{RootProgramName}", RoutineName(RootName)).Replace("{User}", User.Item("Name").ToString).Replace("{Schema}", SchemaName(RootName))
                Next
                Block &= vbCrLf & "end" _
                    & vbCrLf & "else print 'Permissions not granted.'"
            End If

            If Not Block = String.Empty Then
                CodeBlocks.Add(Block)
            End If

        End If


        ' Post-Code
        If Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then
            Dim InPostCode As String = String.Empty
            Try
                InPostCode = InRoot.SelectSingleNode("PostCode").InnerText
            Catch ex As Exception
            End Try

            If Not String.IsNullOrWhiteSpace(InPostCode) Then

                InPostCode = vbCrLf & "-- additional code to execute after " & oType.ToString & " is created" _
                    & vbCrLf & String.Format("if object_id('``this``','{0}') is not null", SquealerObjectType.ToShortType(oType)) _
                    & vbCrLf & "begin" _
                    & vbCrLf & InPostCode _
                    & vbCrLf & "end" _
                    & vbCrLf & "else print 'PostCode not executed.'"

                CodeBlocks.Add(InPostCode)
            End If

        End If


        ' Now add all the GOs
        ExpandIndividual = String.Empty
        For Each s As String In CodeBlocks
            ExpandIndividual &= s & vbCrLf
            If Not bp.OutputMode = BatchParametersClass.eOutputMode.test Then
                ExpandIndividual &= "go" & vbCrLf
            End If
        Next



        ' Add top/bottom markers
        ExpandIndividual =
            SpitDashes(String.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)), "<BOF>") _
            & vbCrLf & ExpandIndividual & vbCr _
            & SpitDashes(String.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)), "<EOF>") & vbCrLf & vbCrLf

        ' Do string replacements.
        For Each Replacement As DataRow In StringReplacements.Select() '.Select("")
            ExpandIndividual = ExpandIndividual.Replace(Replacement.Item("Original").ToString, Replacement.Item("Replacement").ToString)
        Next
        ExpandIndividual = ExpandIndividual.Replace(MyThis, String.Format("[{0}].[{1}]", SchemaName(RootName), RoutineName(RootName)))


    End Function

#End Region

#Region " Git "

    Private Function GitChangedFiles(folder As String, gc As String, glob As String, includeDeleted As Boolean) As List(Of String)

        Dim gitstream As New List(Of String)
        For Each s As String In GitResults(folder, gc, glob)
            Dim f As String = folder & "\" & s.Substring(3)
            If My.Computer.FileSystem.FileExists(f) OrElse includeDeleted Then
                gitstream.Add(f)
            End If
        Next

        Return gitstream

    End Function

    Private Function GitResults(folder As String, gc As String, glob As String) As List(Of String)

        Dim gitstream As New List(Of String)

        Try




            Dim runspace As Runspace = RunspaceFactory.CreateRunspace()
            runspace.Open()
            Dim pipeline As Pipeline = runspace.CreatePipeline()
            pipeline.Commands.AddScript($"cd ""{folder}""")
            pipeline.Commands.AddScript(String.Format("{0} glob ""{1}""", gc, glob))
            pipeline.Commands.Add("Out-String")
            Dim results As Collection(Of PSObject) = pipeline.Invoke()
            runspace.Close()

            For Each obj As PSObject In results
                gitstream.Add(obj.ToString())
            Next

















        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try

        Return gitstream

    End Function


    Private Sub GitCommandDo(folder As String, gc As String, errormessage As String, action As eFileAction)

        Try

            Using ps As System.Management.Automation.PowerShell = System.Management.Automation.PowerShell.Create()

                ps.AddScript($"cd {folder}")
                ps.AddScript(gc)
                Dim r As Collection(Of System.Management.Automation.PSObject) = ps.Invoke()

                For Each o As System.Management.Automation.PSObject In r
                    Console.WriteLine()
                    Textify.Write(o.ToString, ConsoleColor.Cyan, Textify.eLineMode.Truncate)
                Next

            End Using

        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try

    End Sub


    Private Function CurrentBranch(folder As String, sformat As String) As String

        Dim s As String = sformat.Replace("{0}", "no git")

        Try

            Using ps As System.Management.Automation.PowerShell = System.Management.Automation.PowerShell.Create()

                ps.AddScript($"cd {folder}")
                ps.AddScript("git symbolic-ref HEAD")
                Dim r As Collection(Of System.Management.Automation.PSObject) = ps.Invoke()

                For Each o As System.Management.Automation.PSObject In r
                    s = sformat.Replace("{0}", o.ToString.Trim.Replace("refs/heads/", String.Empty))
                Next

            End Using

        Catch ex As Exception

            s = sformat.Replace("{0}", "git error!")

        End Try

        Return s

    End Function

#End Region

#Region " Misc "

    Private Function IsStarWarsDay() As Boolean
        If Now.Month = 5 AndAlso Now.Day = 4 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub ShowLeaderboard(topN As Integer)
        Textify.WriteLine("Retrieving scores...")
        Console.WriteLine()
        Dim lb As New AsciiEngine.Leaderboard
        lb.SqlConnectionString = UserSettings.LeaderboardConnectionString
        lb.SqlLoadScores(topN)
        Dim i As Integer = 0
        For Each s As AsciiEngine.Leaderboard.Score In lb.Items
            i += 1
            Textify.SayCentered(String.Format("|  {0}  {1}  |", s.Signature, s.Points.ToString("00000#"), i.ToString("0#")))
        Next
        Console.WriteLine()
    End Sub
    Public Class SpinnyProgress

        Const TicksPerSecond = 10000000 ' 10 million

        Private _Index As Integer = 0
        Private _SymbolString As String
        Private _Symbols As New List(Of Char)
        Private _AnimationsPerSecond As Integer
        Private _LastTick As Long = DateTime.Now.Ticks

        Public Sub New()
            Me.New("/—\|", 4)
        End Sub

        Public Sub New(s As String)
            Me.New(s, 4)
        End Sub

        Public Sub New(AnimationsPerSecond As Integer)
            Me.New("/—\|", AnimationsPerSecond)
        End Sub

        Public Sub New(s As String, AnimationsPerSecond As Integer)
            _Symbols.AddRange(s.ToCharArray)
            _AnimationsPerSecond = AnimationsPerSecond
            Console.Write(CurrentSymbol)
        End Sub

        Private Function CurrentSymbol() As String

            If Console.CursorLeft > Console.BufferWidth - 2 Then
                Return "."
            Else
                Return _Symbols(_Index)
            End If

        End Function

        Public Sub DoStep(final As Boolean)

            Console.Write(Chr(8) & ".")

            If DateTime.Now.Ticks - _LastTick > TicksPerSecond / _AnimationsPerSecond Then
                _LastTick = DateTime.Now.Ticks
                _Index += 1
                If _Index >= _Symbols.Count Then
                    _Index = 0
                End If
            End If

            If final Then
                Console.WriteLine("*")
            Else
                Console.Write(CurrentSymbol)
            End If

        End Sub

        Public Sub DoStep()
            DoStep(False)
        End Sub

        Public Sub Finish()
            DoStep(True)
        End Sub

    End Class

    Private Function SpitDashes(s As String, marker As String) As String
        Return New String("-"c, 5) & " " & s & " " & New String("-"c, 100 - s.Length) & " " & marker
    End Function

    Private Sub BracketCheck(s As String)
        If s.Contains("["c) OrElse s.Contains("]"c) Then
            Throw New System.Exception("Illegal square bracket character in filename: " & s)
        End If
    End Sub

    Private Function ValidDirectoryStyle(s As String) As Boolean
        Select Case s.ToLower
            Case eDirectoryStyle.compact.ToString
                Return True
            Case eDirectoryStyle.full.ToString
                Return True
            Case eDirectoryStyle.symbolic.ToString
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function PadRightIfNotEmpty(s As String) As String
        If String.IsNullOrWhiteSpace(s) Then
            Return s
        Else
            Return s & " "
        End If
    End Function

    Private Function ReadChangeLog() As String

        Dim s As String = String.Empty

        Dim sr As New IO.StringReader(My.Resources.ChangeLog.TrimEnd)
        While sr.Peek <> -1
            s &= String.Format(sr.ReadLine.Replace("{THIS}", MyThis), ">>>> ", " <<<<", " - ") & vbCrLf
        End While

        Return s

    End Function

    Private Function AboutInfo() As String

        AboutInfo = My.Application.Info.Title & " v." & My.Application.Info.Version.ToString & " : " & My.Application.Info.Description _
            & vbCrLf _
            & vbCrLf & "by " & My.Application.Info.CompanyName _
            & vbCrLf & My.Application.Info.Copyright _
            & vbCrLf & """" & My.Application.Info.Trademark & """" _
            & vbCrLf _
            & vbCrLf & "SQL formatting by https://github.com/TaoK/PoorMansTSqlFormatter" _
            & vbCrLf _
            & vbCrLf & "May the Force be with you."

    End Function

    Private Sub OpenExplorer(ByVal wildcard As String, ByVal WorkingFolder As String)

        Dim files As ObjectModel.ReadOnlyCollection(Of String)

        files = My.Computer.FileSystem.GetFiles(WorkingFolder, FileIO.SearchOption.SearchTopLevelOnly, wildcard)

        If wildcard = "*" & MyConstants.ObjectFileExtension Then
            Process.Start("Explorer.exe", WorkingFolder)
        ElseIf files.Count = 0 Then
            Throw New Exception("File not found.")
        Else
            Process.Start("Explorer.exe", "/select," & WorkingFolder & "\" & My.Computer.FileSystem.GetFileInfo(files(0)).Name)
        End If

    End Sub

    ' Edit one or more files.
    Private Sub OpenInTextEditor(filename As String, path As String)
        ShellOpenFile(path & "\" & filename)
    End Sub
    Private Sub ShellOpenFile(filename As String)
        Dim myprocess As New Process()
        myprocess.StartInfo.FileName = filename
        myprocess.StartInfo.UseShellExecute = True
        'myprocess.StartInfo.RedirectStandardOutput = True
        myprocess.Start()
    End Sub

    Private Sub ThrowErrorIfOverFileLimit(limit As Integer, n As Integer, OverrideSafety As Boolean)
        If n > limit AndAlso Not OverrideSafety Then
            Throw New Exception(String.Format("Too many files. Limit {0}, found {1}.", limit.ToString, n.ToString))
        End If
    End Sub

    ' Clear the keyboard input buffer silently.
    Private Sub ClearKeyboard()
        While Console.KeyAvailable
            Console.ReadKey(True)
        End While
    End Sub

    ' Return the schema name from the filename.
    Friend Function SchemaName(ByVal DisplayName As String) As String
        Dim i As Integer = DisplayName.IndexOf("."c)
        If i < 0 Then
            Return "dbo"
        Else
            Return DisplayName.Substring(0, i)
        End If
    End Function

    ' Return the routine name from the filename.
    Friend Function RoutineName(ByVal DisplayName As String) As String
        Dim i As Integer = DisplayName.IndexOf("."c)
        If i < 0 Then
            Return DisplayName
        Else
            Return DisplayName.Substring(i + 1)
        End If
    End Function

#End Region

#Region " Config File "

    ' Grab the default users from the project config file.
    Private Function GetDefaultUsers(ByVal WorkingFolder As String) As DataTable

        Dim DefaultUsers As New DataTable
        With DefaultUsers.Columns
            .Add("Name", GetType(String))
        End With

        Dim Reader As New Xml.XmlDocument
        Reader.Load(WorkingFolder & "\" & MyConstants.ConfigFilename)
        Dim Nodes As Xml.XmlNodeList = Reader.SelectNodes("/Settings/DefaultUsers/User")

        For Each Node As Xml.XmlNode In Nodes
            DefaultUsers.Rows.Add(AttributeDefaultString(Node.Attributes.GetNamedItem("Name"), String.Empty))
        Next

        GetDefaultUsers = DefaultUsers

    End Function

    ' Grab the string replacements from the project config file.
    Private Function GetStringReplacements(ByVal WorkingFolder As String) As DataTable

        Dim StringReplacements As New DataTable
        With StringReplacements.Columns
            .Add("Original", GetType(String))
            .Add("Replacement", GetType(String))
        End With

        Try
            Dim Reader As New Xml.XmlDocument
            Reader.Load(WorkingFolder & "\" & MyConstants.ConfigFilename)
            Dim Nodes As Xml.XmlNodeList = Reader.SelectNodes("/Settings/StringReplacements/String")

            For Each Node As Xml.XmlNode In Nodes
                StringReplacements.Rows.Add(
                    AttributeDefaultString(Node.Attributes.GetNamedItem("Original"), String.Empty),
                    AttributeDefaultString(Node.Attributes.GetNamedItem("Replacement"), String.Empty)
                )
            Next
        Catch ex As Exception
        End Try

        GetStringReplacements = StringReplacements

    End Function

    ' Get the project nickname.
    Private Function GetProjectNickname(ByVal WorkingFolder As String) As String

        Try
            Dim Reader As New Xml.XmlDocument
            Reader.Load(WorkingFolder & "\" & MyConstants.ConfigFilename)
            Dim Node As Xml.XmlNode = Reader.SelectSingleNode("/Settings")
            Dim s As String = Node.Attributes("ProjectName").Value.ToString.Trim()
            If String.IsNullOrWhiteSpace(s) Then
                s = "myproject"
            End If
            If s.Length > 30 Then
                s = s.Substring(0, 30)
            End If
            Return s
        Catch ex As Exception
            Return "myproject"
        End Try

    End Function

#End Region

#Region " Console Output "

    ' Display a notice for a file.
    Private Sub SayFileAction(ByVal notice As String, ByVal path As String, ByVal file As String)
        Textify.SayBullet(Textify.eBullet.Hash, notice & ":")
        Textify.SayBullet(Textify.eBullet.Arrow, path & IIf(file = "", "", "\" & file).ToString)
    End Sub

    Private Sub ShowFile(ByVal path As String, ByVal file As String)

        Dim f As String = path & "\" & file

        SayFileAction("reading", path, file)
        Textify.SayNewLine()

        Try
            Console.WriteLine(My.Computer.FileSystem.ReadAllText(f))
        Catch ex As Exception
            Textify.SayError("File not found.", Textify.eSeverity.info)
        End Try

    End Sub

#End Region

#Region " Connection String "

    Private Sub SetConnectionString(workingfolder As String, cs As String)

        Dim f As String = String.Format("{0}\{1}", workingfolder, MyConstants.ConnectionStringFilename)
        Dim entropy As Byte() = {1, 9, 1, 1, 4, 5}
        Dim csbytes As Byte() = System.Text.Encoding.Unicode.GetBytes(cs.Trim)

        Dim encrypted As Byte() = System.Security.Cryptography.ProtectedData.Protect(csbytes, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser)
        If My.Computer.FileSystem.FileExists(f) Then
            My.Computer.FileSystem.DeleteFile(f)
        End If
        My.Computer.FileSystem.WriteAllBytes(f, encrypted, False)
        System.IO.File.SetAttributes(f, IO.FileAttributes.Hidden)

        Textify.SayBulletLine(Textify.eBullet.Hash, "OK")
        Textify.SayNewLine()

    End Sub
    Private Sub ForgetConnectionString(workingfolder As String)

        Dim f As String = String.Format("{0}\{1}", workingfolder, MyConstants.ConnectionStringFilename)
        My.Computer.FileSystem.DeleteFile(f)
        Textify.SayBulletLine(Textify.eBullet.Hash, "OK")
        Textify.SayNewLine()

    End Sub

    Private Function GetConnectionString(workingfolder As String) As String

        Dim f As String = String.Format("{0}\{1}", workingfolder, MyConstants.ConnectionStringFilename)

        If Not My.Computer.FileSystem.FileExists(f) Then
            Throw New Exception("Connection string not defined.")
        End If

        Dim entropy As Byte() = {1, 9, 1, 1, 4, 5}
        Dim decrypted As Byte() = System.Security.Cryptography.ProtectedData.Unprotect(My.Computer.FileSystem.ReadAllBytes(f), entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser)
        GetConnectionString = System.Text.Encoding.Unicode.GetString(decrypted)

    End Function

    Private Sub TestConnectionString(workingfolder As String)

        Dim cs As String = GetConnectionString(workingfolder)

        Textify.SayBullet(Textify.eBullet.Arrow, cs)

        Using DbTest As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

            DbTest.Open()

            Dim DbReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand("select @@SERVERNAME,DB_NAME(),@@VERSION" _
                & ",(select count(1) from sys.tables)" _
                & ",(select count(1) from sys.objects o where o.type = 'p')" _
                & ",(select count(1) from sys.objects o where o.type = 'fn')" _
                & ",(select count(1) from sys.objects o where o.type = 'if')" _
                & ",(select count(1) from sys.objects o where o.type = 'tf')" _
                & ",(select count(1) from sys.objects o where o.type = 'v')" _
                & ";", DbTest).ExecuteReader

            While DbReader.Read

                'Dim Result As String = 

                Textify.SayBulletLine(Textify.eBullet.Arrow, DbReader.GetString(2)) ' @@version
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("[{0}].[{1}]", DbReader.GetString(0), DbReader.GetString(1)))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} table(s)", DbReader.GetInt32(3).ToString))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} stored procedure(s)", DbReader.GetInt32(4).ToString))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} scalar function(s)", DbReader.GetInt32(5).ToString))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} inline table-valued function(s)", DbReader.GetInt32(6).ToString))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} multi-statement table-valued function(s)", DbReader.GetInt32(7).ToString))
                Textify.SayBulletLine(Textify.eBullet.Arrow, String.Format("{0} views(s)", DbReader.GetInt32(8).ToString))
                Textify.SayBulletLine(Textify.eBullet.Hash, "OK")

            End While

            Textify.SayNewLine()

        End Using

    End Sub

#End Region

#Region " Automagic "

    Private Enum eAutoProcType
        [Insert]
        [Update]
        [Delete]
        [Get]
        [List]
    End Enum

    Private Sub Automagic(cs As String, WorkingFolder As String, ReplaceOnly As Boolean, datasourcecomment As Boolean)

        Textify.Write("Reading tables..")

        Dim ProcCount As Integer = 0

        Using DbTables As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

            DbTables.Open()

            Dim TableReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand(My.Resources.AutoGetTables, DbTables).ExecuteReader

            Dim spinny As New SpinnyProgress()


            While TableReader.Read

                Dim SchemaName As String = TableReader.GetString(0)
                Dim TableName As String = TableReader.GetString(1)
                Dim TableId As Integer = TableReader.GetInt32(2)
                Dim AutoProcType As eAutoProcType = DirectCast([Enum].Parse(GetType(eAutoProcType), TableReader.GetString(3)), eAutoProcType)

                Dim GenOutputs As New List(Of String)
                Dim GenParameters As New List(Of String)
                Dim GenWhereClause As String = ""
                Dim GenFromColumns As New List(Of String)
                Dim GenValuesColumns As New List(Of String)

                spinny.DoStep()

                Using DbColumns As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

                    DbColumns.Open()

                    Dim ColumnReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand(My.Resources.AutoGetColumns.Replace("{TableId}", TableReader("table_id").ToString), DbColumns).ExecuteReader

                    While ColumnReader.Read

                        Dim ColName As String = ColumnReader.GetString(0)
                        Dim ColType As String = ColumnReader.GetString(1).ToLower
                        Dim ColIsIdentity As Boolean = ColumnReader.GetBoolean(2)
                        Dim ColIsRowGuid As Boolean = ColumnReader.GetBoolean(3)
                        Dim ColId As Integer = ColumnReader.GetInt32(4)
                        Dim ColMaxLength As Int16 = ColumnReader.GetInt16(5)
                        Dim ColPrecision As Byte = ColumnReader.GetByte(6)
                        Dim ColScale As Byte = ColumnReader.GetByte(7)
                        Dim ColDefaultValue As String = ColumnReader(8).ToString
                        Dim ColHasDefault As Boolean = ColDefaultValue.Trim.Length > 0
                        Dim ColIsPk As Boolean = ColumnReader.GetBoolean(9)
                        Dim ColIsComputed As Boolean = ColumnReader.GetBoolean(10)
                        Dim ColIsGuidIdentity As Boolean = ColType.Contains("uniqueidentifier") AndAlso (ColDefaultValue.Contains("newid") OrElse ColDefaultValue.Contains("newsequentialid"))


                        ' DEFINE PARAMETERS

                        ' Add scale and precision to column type
                        If (ColType.Contains("char") OrElse ColType.Contains("binary")) Then
                            ColType &= String.Format("({0})", ColMaxLength.ToString.Replace("-1", "max"))
                        ElseIf (ColType.Contains("decimal") OrElse ColType.Contains("numeric")) Then
                            ColType &= String.Format("({0},{1})", ColPrecision.ToString, ColScale.ToString)
                        End If

                        ' Add all parameters (with output IDs) for write procs; only add key parameters for read procs
                        If (AutoProcType = eAutoProcType.Insert OrElse AutoProcType = eAutoProcType.Update) OrElse ((AutoProcType = eAutoProcType.Delete OrElse AutoProcType = eAutoProcType.Get) AndAlso ColIsPk) Then
                            GenParameters.Add(String.Format("<Parameter Name=""{0}"" Type=""{1}"" Output=""{2}"" />", ColName.Replace(" ", "_"), ColType, (AutoProcType = eAutoProcType.Insert AndAlso (ColIsIdentity OrElse ColIsRowGuid))))
                        End If


                        ' DETECT OUTPUT COLUMNS

                        ' Ignore columns with MAX width specification because those could be enormous (ex: file attachments).
                        If AutoProcType = eAutoProcType.Insert AndAlso Not ColMaxLength = -1 Then
                            GenOutputs.Add(String.Format("{0}|{1}", ColName, ColType))
                        End If


                        ' BUILD THE INSERT/UPDATE/SELECT COLUMNS (never build DELETE columns)

                        If (AutoProcType = eAutoProcType.Insert And Not ColIsIdentity And Not ColIsGuidIdentity And Not ColIsComputed) _
                                OrElse (AutoProcType = eAutoProcType.Update And Not ColIsPk And Not ColIsComputed) _
                                OrElse (AutoProcType = eAutoProcType.Get) _
                                OrElse (AutoProcType = eAutoProcType.List) Then

                            Dim c As String = String.Format("[{0}]", ColName)
                            If AutoProcType = eAutoProcType.Update Then
                                c &= String.Format(" = @{0}", ColName.Replace(" ", "_"))
                            End If
                            If (AutoProcType = eAutoProcType.Insert OrElse AutoProcType = eAutoProcType.Update) AndAlso ColHasDefault Then
                                c &= " -- default: " & ColDefaultValue
                            End If
                            GenFromColumns.Add(c)

                            If AutoProcType = eAutoProcType.Insert Then
                                If ColIsRowGuid AndAlso Not ColHasDefault Then
                                    c = "newid()"
                                ElseIf ColHasDefault Then
                                    c = ColDefaultValue
                                Else
                                    c = "@" & ColName.Replace(" ", "_")
                                End If
                                GenValuesColumns.Add(c)
                            End If

                        End If


                        ' BUILD THE WHERE CLAUSE

                        If ColIsPk AndAlso (AutoProcType = eAutoProcType.Update OrElse AutoProcType = eAutoProcType.Delete OrElse AutoProcType = eAutoProcType.Get) Then

                            GenWhereClause &= vbCrLf & vbTab & IIf(GenWhereClause.Length > 0, "and ", "").ToString & String.Format("[{0}] = @{0}", ColName)

                        End If



                    End While

                End Using

                Dim OutputUsers As String = ""

                For Each User As DataRow In GetDefaultUsers(WorkingFolder).Rows
                    OutputUsers &= String.Format("<User Name=""{0}"" />", User.Item("Name"))
                Next


                Dim AutoCode As String = "/***********************************************************************" _
                    & vbCrLf & String.Format("	This code was generated by {0}.", My.Application.Info.Title) _
                    & vbCrLf & "***********************************************************************/"

                If AutoProcType = eAutoProcType.Delete Then
                    AutoCode &= vbCrLf & vbCrLf & "-- do you need an UPDATE statement to create an audit log entry?"
                End If

                Dim GenOutputsString As String() = {"", "", ""}

                If GenOutputs.Count > 0 Then

                    For i = 0 To GenOutputs.Count - 1
                        GenOutputsString(0) &= vbCrLf & vbTab & IIf(i = 0, "", ",").ToString & String.Format("[{0}] {1}", GenOutputs(i).Split("|"c)(0), GenOutputs(i).Split("|"c)(1))
                        GenOutputsString(1) &= vbCrLf & vbTab & IIf(i = 0, "", ",").ToString & String.Format("inserted.[{0}]", GenOutputs(i).Split("|"c)(0))
                        GenOutputsString(2) &= vbCrLf & vbTab & IIf(i = 0, "", ",").ToString & String.Format("@{0} = o.[{1}]", GenOutputs(i).Split("|"c)(0).Replace(" ", "_"), GenOutputs(i).Split("|"c)(0))
                    Next

                    AutoCode &= vbCrLf & vbCrLf & "declare @Outputs as table" & vbCrLf & "(" & GenOutputsString(0) & vbCrLf & ");"
                    GenOutputsString(1) = "output" & GenOutputsString(1) & vbCrLf & "into" & vbCrLf & vbTab & "@outputs"
                    GenOutputsString(2) = "select" & GenOutputsString(2) & vbCrLf & "from" & vbCrLf & vbTab & "@outputs o;"

                End If

                AutoCode &= vbCrLf

                If AutoProcType = eAutoProcType.Get OrElse AutoProcType = eAutoProcType.List Then
                    AutoCode &= vbCrLf & "select"
                Else
                    AutoCode &= vbCrLf & AutoProcType.ToString.ToLower
                End If

                If AutoProcType = eAutoProcType.Insert OrElse AutoProcType = eAutoProcType.Update OrElse AutoProcType = eAutoProcType.Delete Then
                    AutoCode &= vbCrLf & vbTab & String.Format("[{0}].[{1}]", SchemaName, TableName)
                End If

                Select Case AutoProcType
                    Case eAutoProcType.Insert
                        AutoCode &= vbCrLf & "("
                    Case eAutoProcType.Update
                        AutoCode &= vbCrLf & "set"
                End Select

                Dim ValidOutput As Boolean = True

                If AutoProcType = eAutoProcType.Update AndAlso GenFromColumns.Count = 0 Then
                    ValidOutput = False
                End If


                For i = 0 To GenFromColumns.Count - 1
                    AutoCode &= vbCrLf & vbTab & IIf(i = 0, "", ",").ToString & GenFromColumns(i)
                Next

                If AutoProcType = eAutoProcType.Insert Then
                    AutoCode &= vbCrLf & ")" & vbCrLf & GenOutputsString(1) & vbCrLf & "values" & vbCrLf & "("
                End If

                For i = 0 To GenValuesColumns.Count - 1
                    AutoCode &= vbCrLf & vbTab & IIf(i = 0, "", ",").ToString & GenValuesColumns(i)
                Next


                If AutoProcType = eAutoProcType.Insert Then
                    AutoCode &= vbCrLf & ");" & vbCrLf & vbCrLf & GenOutputsString(2)
                End If

                'AutoCode &= GenFromClause

                Select Case AutoProcType
                    Case eAutoProcType.Get
                        AutoCode &= vbCrLf & "from" & vbCrLf & vbTab & String.Format("[{0}].[{1}]", SchemaName, TableName)
                    Case eAutoProcType.List
                        AutoCode &= vbCrLf & "from" & vbCrLf & vbTab & String.Format("[{0}].[{1}]", SchemaName, TableName)
                End Select

                If AutoProcType = eAutoProcType.List Then
                    AutoCode &= vbCrLf & "--order by" & vbCrLf & "--" & vbTab & "?"
                End If

                If Not String.IsNullOrWhiteSpace(GenWhereClause) Then
                    AutoCode &= vbCrLf & "where" & GenWhereClause
                End If

                Dim GenParametersString As String = ""
                For Each s As String In GenParameters
                    GenParametersString &= s
                Next


                If ValidOutput Then

                    Dim filename As String = String.Format("{0}\{1}.{5}{2}{3}{4}", WorkingFolder, SchemaName, TableName, AutoProcType, MyConstants.ObjectFileExtension, MyConstants.AutocreateFilename)

                    If Not ReplaceOnly OrElse My.Computer.FileSystem.FileExists(filename) Then

                        Dim comments As String = String.Format("Basic {0} for [{1}].[{2}]", AutoProcType.ToString.ToUpper, SchemaName, TableName)
                        If datasourcecomment Then
                            comments &= String.Format(vbCrLf & "Generated from [{0}].[{1}] on {2}", DbTables.DataSource, DbTables.Database, Now)
                        End If

                        My.Computer.FileSystem.WriteAllText(filename,
                                My.Resources.AutoProcTemplate _
                                    .Replace("{Comments}", comments) _
                                    .Replace("{Parameters}", GenParametersString) _
                                    .Replace("{Code}", AutoCode) _
                                    .Replace("{Users}", OutputUsers) _
                                , False) ' Overwrite

                        RepairXmlFile(False, filename, False)

                        ProcCount += 1

                    End If

                End If


                If Console.KeyAvailable() Then
                    Throw New System.Exception("Keyboard interrupt.")
                End If

            End While

            spinny.Finish()

        End Using

        Textify.SayNewLine()
        Textify.SayBullet(Textify.eBullet.Hash, String.Format("{0} files automatically created.", ProcCount.ToString))
        Textify.SayNewLine(2)

    End Sub

    Private Sub ReverseEngineer(cs As String, WorkingFolder As String, DoCleanup As Boolean)

        Console.Write("Reading procs, views, functions..")

        Dim ProcCount As Integer = 0
        Dim SkippedCount As Integer = 0
        Dim tempfile As New TempFileHandler(".txt")

        Using DbObjects As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

            DbObjects.Open()

            Dim ObjectReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand(My.Resources.ObjectList, DbObjects).ExecuteReader

            Dim spinny As New SpinnyProgress()

            While ObjectReader.Read ' loop thru procs, views, functions

                Dim ParameterList As New ParameterCollection
                Dim UserList As New List(Of String)

                Dim ObjectName As String = ObjectReader.GetString(0)
                Dim ObjectType As String = ObjectReader.GetString(1).ToLower
                Dim ObjectDefinition As String = ObjectReader.GetString(2)
                Dim ObjectId As Integer = ObjectReader.GetInt32(3)

                Dim filetype As SquealerObjectType.eType = SquealerObjectType.Eval(ObjectType)

                Using DbParameters As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

                    DbParameters.Open()

                    Dim ParameterReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand(My.Resources.ObjectParameters.Replace("@ObjectId", ObjectId.ToString), DbParameters).ExecuteReader

                    While ParameterReader.Read ' loop thru parameters

                        Dim ParameterName As String = ParameterReader.GetString(0)
                        Dim ParameterType As String = ParameterReader.GetString(1)
                        Dim IsOutput As Boolean = ParameterReader.GetBoolean(2)
                        Dim MaxLength As Int16 = ParameterReader.GetInt16(3)

                        ParameterList.Add(New ParameterClass(ParameterName, ParameterType, MaxLength, IsOutput))

                    End While

                End Using

                Using DbUsers As SqlClient.SqlConnection = New SqlClient.SqlConnection(cs)

                    DbUsers.Open()

                    Dim UserReader As SqlClient.SqlDataReader = New SqlClient.SqlCommand(My.Resources.ObjectPermissions.Replace("@ObjectId", ObjectId.ToString), DbUsers).ExecuteReader

                    While UserReader.Read ' loop thru user permissions granted

                        Dim UserName As String = UserReader.GetString(0)

                        UserList.Add(UserName)

                    End While

                End Using

                If DoCleanup Then

                    ' delete head
                    Try
                        Dim s As String = "create " & SquealerObjectType.EvalSimpleType(filetype)
                        ObjectDefinition = ObjectDefinition.Remove(0, ObjectDefinition.ToLower.IndexOf(s) + s.Length + 1)
                    Catch ex As Exception
                    End Try

                    ' delete tail
                    Try
                        Dim s As String = String.Empty
                        Select Case filetype
                            Case SquealerObjectType.eType.StoredProcedure
                                s = "YOUR CODE ENDS HERE."
                            Case SquealerObjectType.eType.ScalarFunction
                                s = "Return the function result."
                        End Select
                        If Not String.IsNullOrEmpty(s) Then
                            ObjectDefinition = ObjectDefinition.Substring(0, ObjectDefinition.IndexOf(s))
                        End If
                    Catch ex As Exception
                    End Try

                    ' delete everything between parameters and beginning of code
                    Try

                        Dim s As String = String.Empty
                        Dim s2 As String = String.Empty
                        Select Case filetype
                            Case SquealerObjectType.eType.StoredProcedure
                                s = "Begin the transaction. Start the TRY..CATCH wrapper."
                                s2 = "YOUR CODE BEGINS HERE."
                            Case SquealerObjectType.eType.ScalarFunction
                                s = "returns"
                                s2 = "declare @Result " & ParameterList.ReturnType.Type
                        End Select
                        If Not String.IsNullOrEmpty(s) Then
                            Dim startpos As Integer = ObjectDefinition.IndexOf(s)
                            Dim charcount As Integer = ObjectDefinition.IndexOf(s2) + s2.Length - startpos
                            ObjectDefinition = ObjectDefinition.Remove(startpos, charcount)
                        End If
                    Catch ex As Exception
                    End Try

                End If

                ObjectDefinition = "-- reverse engineered on " & Now.ToString & vbCrLf & vbCrLf & ObjectDefinition

                Dim f As String = CreateNewFile(WorkingFolder, filetype, ObjectName, ParameterList, ObjectDefinition, UserList)
                If f = String.Empty Then
                    SkippedCount += 1
                    tempfile.Writeline(String.Format("{0} -- duplicate ({1})", ObjectName, filetype.ToString))
                Else
                    ProcCount += 1
                    tempfile.Writeline(ObjectName & " -- OK")
                End If
                spinny.DoStep()

                If Console.KeyAvailable() Then
                    Throw New System.Exception("Keyboard interrupt.")
                End If

            End While

            spinny.Finish()

        End Using

        Textify.SayNewLine()
        Textify.SayBullet(Textify.eBullet.Hash, String.Format("{0} files reverse engineered; {1} skipped (duplicate filename).", ProcCount.ToString, SkippedCount.ToString))
        Textify.SayNewLine()
        Textify.SayBullet(Textify.eBullet.Hash, "Results not guaranteed!", 0, New Textify.ColorScheme(ConsoleColor.Yellow))
        Textify.SayNewLine(2)

        tempfile.Show()

    End Sub


#End Region

#Region " Version Check "

    Private Sub CheckS3(silent As Boolean)

        Try
            Dim client As Net.WebClient = New Net.WebClient()
            Using reader As New IO.StreamReader(client.OpenRead(s3VersionText))

                Dim av As New Version(reader.ReadToEnd)

                If My.Application.Info.Version.CompareTo(av) < 0 Then
                    Textify.SayBulletLine(Textify.eBullet.Hash, String.Format("A new version of {0} is available. Use {1} -{2} to download.", My.Application.Info.ProductName, eCommandType.about.ToString.ToUpper, MyCommands.FindCommand(eCommandType.about.ToString).Options.Items(0).Keyword.ToUpper), New Textify.ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue))
                    Console.BackgroundColor = ConsoleColor.Black
                    Textify.SayBulletLine(Textify.eBullet.Arrow, s3ZipFile)
                    Console.WriteLine()
                ElseIf My.Application.Info.Version.CompareTo(av) = 0 AndAlso Not silent Then
                    Textify.SayBulletLine(Textify.eBullet.Hash, String.Format("You have the latest version of {0}.", My.Application.Info.ProductName), New Textify.ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue))
                ElseIf My.Application.Info.Version.CompareTo(av) < 0 OrElse (My.Application.Info.Version.CompareTo(av) > 0 AndAlso Not silent) Then
                    Textify.SayBulletLine(Textify.eBullet.Hash, String.Format("Latest version: {0}", av.ToString))
                    Textify.SayBulletLine(Textify.eBullet.Hash, String.Format("Your version: {0}", My.Application.Info.Version.ToString))
                End If

            End Using

        Catch ex As Exception
            If Not silent Then
                Textify.SayError(ex.Message)
            End If
        End Try

        Console.WriteLine()

    End Sub

    Private Sub DisplayChangelog()
        Dim f As New TempFileHandler(".txt")
        f.Writeline(AboutInfo)
        f.Writeline()
        f.Writeline()
        f.Writeline(ReadChangeLog)
        f.Show()
    End Sub


#End Region

End Module
