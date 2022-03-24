﻿Public Class Settings
    Public Enum OutputStyle
        Detailed
        Percentage
    End Enum

    Public Class WildcardClass

        Private _UseEdges As Boolean
        Public Property UseEdges As Boolean
            Get
                Return _UseEdges
            End Get
            Set(value As Boolean)
                _UseEdges = value
            End Set
        End Property

        Private _UseSpaces As Boolean
        Public Property UseSpaces As Boolean
            Get
                Return _UseSpaces
            End Get
            Set(value As Boolean)
                _UseSpaces = value
            End Set
        End Property

    End Class

    Public Class OpenWithDefaultClass

        Private _SqlFiles As Boolean
        Public Property SqlFiles As Boolean
            Get
                Return _SqlFiles
            End Get
            Set(value As Boolean)
                _SqlFiles = value
            End Set
        End Property

        Private _ConfigFiles As Boolean
        Public Property ConfigFiles As Boolean
            Get
                Return _ConfigFiles
            End Get
            Set(value As Boolean)
                _ConfigFiles = value
            End Set
        End Property

        Private _SquealerFiles As Boolean
        Public Property SquealerFiles As Boolean
            Get
                Return _SquealerFiles
            End Get
            Set(value As Boolean)
                _SquealerFiles = value
            End Set
        End Property

    End Class

    Private _Wildcards As New WildcardClass
    Public ReadOnly Property Wildcards As WildcardClass
        Get
            Return _Wildcards
        End Get
    End Property

    Private _OpenWithDefault As New OpenWithDefaultClass
    Public ReadOnly Property OpenWithDefault As OpenWithDefaultClass
        Get
            Return _OpenWithDefault
        End Get
    End Property

    Private _LastVersionCheck As DateTime
    Public Property LastVersionCheck As DateTime
        Get
            Return _LastVersionCheck
        End Get
        Set(value As DateTime)
            _LastVersionCheck = value
        End Set
    End Property

    Private _TextEditor As String
    Public Property TextEditor As String
        Get
            Return _TextEditor
        End Get
        Set(value As String)
            _TextEditor = value
        End Set
    End Property

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

    'todo: what does this comment mean?
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

    Private _OutputStyle As OutputStyle
    Public Property OutputStyleSelected As OutputStyle
        Get
            Return _OutputStyle
        End Get
        Set(value As OutputStyle)
            _OutputStyle = value
        End Set
    End Property

    Private _Increment As Integer
    Public Property OutputIncrement As Integer
        Get
            Return _Increment
        End Get
        Set(value As Integer)
            _Increment = value
        End Set
    End Property

    Public Sub New()
        Me.New(False)
    End Sub

    Public Sub New(doload As Boolean)
        If doload Then
            LoadSettings()
        End If
    End Sub

    Public Sub LoadSettings()

        ' Load settings.
        Dim s As String
        Me.LastVersionCheck = My.Configger.LoadSetting(NameOf(Me.LastVersionCheck), New DateTime(0))
        Me.OpenWithDefault.SqlFiles = My.Configger.LoadSetting(NameOf(Me.OpenWithDefault.SqlFiles), False)
        Me.OpenWithDefault.ConfigFiles = My.Configger.LoadSetting(NameOf(Me.OpenWithDefault.ConfigFiles), False)
        Me.OpenWithDefault.SquealerFiles = My.Configger.LoadSetting(NameOf(Me.OpenWithDefault.SquealerFiles), False)
        Me.TextEditor = My.Configger.LoadSetting(NameOf(Me.TextEditor), "notepad.exe")
        Me.LeaderboardConnectionString = My.Configger.LoadSetting(NameOf(Me.LeaderboardConnectionString), String.Empty)
        Me.RecentFolders = My.Configger.LoadSetting(NameOf(Me.RecentFolders), 20)
        Me.Wildcards.UseEdges = My.Configger.LoadSetting(NameOf(Me.Wildcards.UseEdges), False)
        Me.EditNew = My.Configger.LoadSetting(NameOf(Me.EditNew), True)
        Me.UseClipboard = My.Configger.LoadSetting(NameOf(Me.UseClipboard), True)
        Me.ShowLeaderboardAtStartup = My.Configger.LoadSetting(NameOf(Me.ShowLeaderboardAtStartup), False)
        Me.DetectSquealerObjects = My.Configger.LoadSetting(NameOf(Me.DetectSquealerObjects), True)
        Me.ShowBranch = My.Configger.LoadSetting(NameOf(Me.ShowBranch), True)
        Me.Wildcards.UseSpaces = My.Configger.LoadSetting(NameOf(Me.Wildcards.UseSpaces), False)
        Me.DirStyle = My.Configger.LoadSetting(NameOf(Me.DirStyle), eDirectoryStyle.compact.ToString)
        s = My.Configger.LoadSetting(NameOf(Me.OutputStyleSelected), OutputStyle.Detailed.ToString)
        Me.OutputStyleSelected = DirectCast([Enum].Parse(GetType(OutputStyle), s), OutputStyle)
        Me.OutputIncrement = My.Configger.LoadSetting(NameOf(Me.OutputIncrement), 5)
        If Not (Me.OutputIncrement = 5 OrElse Me.OutputIncrement = 10 OrElse Me.OutputIncrement = 20 OrElse Me.OutputIncrement = 25) Then
            Me.OutputIncrement = 5
        End If
        Textify.ErrorAlert.Beep = My.Configger.LoadSetting(NameOf(Textify.ErrorAlert.Beep), False)

    End Sub

    Public Sub Show()

        Dim f As New SettingsForm
        f.ddIncrement.SelectedIndex = f.ddIncrement.FindString(Me.OutputIncrement.ToString) ' must set this before radio button because rb checked triggers an event using this value
        Select Case Me.OutputStyleSelected
            Case OutputStyle.Detailed
                f.rbDetailed.Checked = True
            Case OutputStyle.Percentage
                f.rbPercentage.Checked = True
        End Select
        f.chkOutputDefaultEditor.Checked = Me.OpenWithDefault.SqlFiles
        f.chkConfigDefaultEditor.Checked = Me.OpenWithDefault.ConfigFiles
        f.chkSquealerDefaultEditor.Checked = Me.OpenWithDefault.SquealerFiles
        f.txtEditorProgram.Text = Me.TextEditor
        f.txtLeaderboardCs.Text = Me.LeaderboardConnectionString
        f.updnFolderSaves.Value = Me.RecentFolders
        f.chkSpacesWild.Checked = Me.Wildcards.UseSpaces
        f.chkEdgesWild.Checked = Me.Wildcards.UseEdges
        f.optEditNewFiles.Checked = Me.EditNew
        f.chkShowLeaderboard.Checked = Me.ShowLeaderboardAtStartup
        If Me.UseClipboard Then
            f.rbClipboard.Checked = True
        Else
            f.rbTempFile.Checked = True
        End If
        f.optShowGitBranch.Checked = Me.ShowBranch
        f.optBeep.Checked = Textify.ErrorAlert.Beep
        f.optDetectOldSquealerObjects.Checked = Me.DetectSquealerObjects
        Select Case Me.DirStyle
            Case eDirectoryStyle.compact.ToString
                f.rbCompact.Checked = True
            Case eDirectoryStyle.full.ToString
                f.rbFull.Checked = True
            Case eDirectoryStyle.symbolic.ToString
                f.rbSymbolic.Checked = True
        End Select

        f.StartPosition = Windows.Forms.FormStartPosition.CenterScreen
        f.ShowDialog()

        Me.OutputIncrement = CInt(f.ddIncrement.SelectedItem.ToString)
        If f.rbDetailed.Checked Then
            Me.OutputStyleSelected = OutputStyle.Detailed
        Else
            Me.OutputStyleSelected = OutputStyle.Percentage
        End If
        Me.OpenWithDefault.SqlFiles = f.chkOutputDefaultEditor.Checked
        Me.OpenWithDefault.ConfigFiles = f.chkConfigDefaultEditor.Checked
        Me.OpenWithDefault.SquealerFiles = f.chkSquealerDefaultEditor.Checked
        Me.TextEditor = f.txtEditorProgram.Text
        Me.LeaderboardConnectionString = f.txtLeaderboardCs.Text
        Me.RecentFolders = CInt(f.updnFolderSaves.Value)
        Me.Wildcards.UseSpaces = f.chkSpacesWild.Checked
        Me.Wildcards.UseEdges = f.chkEdgesWild.Checked
        Me.EditNew = f.optEditNewFiles.Checked
        Me.ShowLeaderboardAtStartup = f.chkShowLeaderboard.Checked
        Me.UseClipboard = f.rbClipboard.Checked
        Me.ShowBranch = f.optShowGitBranch.Checked
        Textify.ErrorAlert.Beep = f.optBeep.Checked
        Me.DetectSquealerObjects = f.optDetectOldSquealerObjects.Checked
        If f.rbCompact.Checked Then
            Me.DirStyle = eDirectoryStyle.compact.ToString
        ElseIf f.rbFull.Checked Then
            Me.DirStyle = eDirectoryStyle.full.ToString
        Else
            Me.DirStyle = eDirectoryStyle.symbolic.ToString
        End If

        My.Configger.SaveSetting(NameOf(Me.OutputIncrement), Me.OutputIncrement)
        My.Configger.SaveSetting(NameOf(Me.OutputStyleSelected), Me.OutputStyleSelected.ToString)
        My.Configger.SaveSetting(NameOf(Me.OpenWithDefault.SqlFiles), Me.OpenWithDefault.SqlFiles)
        My.Configger.SaveSetting(NameOf(Me.OpenWithDefault.ConfigFiles), Me.OpenWithDefault.ConfigFiles)
        My.Configger.SaveSetting(NameOf(Me.OpenWithDefault.SquealerFiles), Me.OpenWithDefault.SquealerFiles)
        My.Configger.SaveSetting(NameOf(Me.TextEditor), Me.TextEditor)
        My.Configger.SaveSetting(NameOf(Me.LeaderboardConnectionString), Me.LeaderboardConnectionString)
        My.Configger.SaveSetting(NameOf(Me.RecentFolders), Me.RecentFolders)
        My.Configger.SaveSetting(NameOf(Me.Wildcards.UseSpaces), Me.Wildcards.UseSpaces)
        My.Configger.SaveSetting(NameOf(Me.Wildcards.UseEdges), Me.Wildcards.UseEdges)
        My.Configger.SaveSetting(NameOf(Me.EditNew), Me.EditNew)
        My.Configger.SaveSetting(NameOf(Me.ShowLeaderboardAtStartup), Me.ShowLeaderboardAtStartup)
        My.Configger.SaveSetting(NameOf(Me.UseClipboard), Me.UseClipboard)
        My.Configger.SaveSetting(NameOf(Me.DetectSquealerObjects), Me.DetectSquealerObjects)
        My.Configger.SaveSetting(NameOf(Me.ShowBranch), Me.ShowBranch)
        My.Configger.SaveSetting(NameOf(Textify.ErrorAlert.Beep), Textify.ErrorAlert.Beep)
        My.Configger.SaveSetting(NameOf(Me.DirStyle), Me.DirStyle)

    End Sub

End Class