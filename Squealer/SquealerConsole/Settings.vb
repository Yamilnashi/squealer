﻿Public Class Settings

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

    Private _Wildcards As New WildcardClass
    Public ReadOnly Property Wildcards As WildcardClass
        Get
            Return _Wildcards
        End Get
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


    Public Sub Show()

        Dim f As New SettingsForm
        f.txtEditorProgram.Text = Me.TextEditor
        f.txtLeaderboardCs.Text = Me.LeaderboardConnectionString
        f.updnFolderSaves.Value = Me.RecentFolders
        f.chkEdgesWild.Checked = Me.Wildcards.UseEdges
        f.optEditNewFiles.Checked = Me.EditNew
        f.chkShowLeaderboard.Checked = Me.ShowLeaderboardAtStartup
        If Me.UseClipboard Then
            f.rbClipboard.Checked = True
        Else
            f.rbTempFile.Checked = True
        End If
        f.optShowGitBranch.Checked = Me.ShowBranch
        f.chkSpacesWild.Checked = Me.Wildcards.UseSpaces
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

        f.ShowDialog()

        Me.TextEditor = f.txtEditorProgram.Text
        Me.LeaderboardConnectionString = f.txtLeaderboardCs.Text
        Me.RecentFolders = CInt(f.updnFolderSaves.Value)
        Me.Wildcards.UseEdges = f.chkEdgesWild.Checked
        Me.EditNew = f.optEditNewFiles.Checked
        Me.ShowLeaderboardAtStartup = f.chkShowLeaderboard.Checked
        Me.UseClipboard = f.rbClipboard.Checked
        Me.ShowBranch = f.optShowGitBranch.Checked
        Me.Wildcards.UseSpaces = f.chkSpacesWild.Checked
        Textify.ErrorAlert.Beep = f.optBeep.Checked
        Me.DetectSquealerObjects = f.optDetectOldSquealerObjects.Checked
        If f.rbCompact.Checked Then
            Me.DirStyle = eDirectoryStyle.compact.ToString
        ElseIf f.rbFull.Checked Then
            Me.DirStyle = eDirectoryStyle.full.ToString
        Else
            Me.DirStyle = eDirectoryStyle.symbolic.ToString
        End If

        My.Configger.SaveSetting(NameOf(Me.TextEditor), Me.TextEditor)
        My.Configger.SaveSetting(NameOf(Me.LeaderboardConnectionString), Me.LeaderboardConnectionString)
        My.Configger.SaveSetting(NameOf(Me.RecentFolders), Me.RecentFolders)
        My.Configger.SaveSetting(NameOf(Me.Wildcards.UseEdges), Me.Wildcards.UseEdges)
        My.Configger.SaveSetting(NameOf(Me.EditNew), Me.EditNew)
        My.Configger.SaveSetting(NameOf(Me.ShowLeaderboardAtStartup), Me.ShowLeaderboardAtStartup)
        My.Configger.SaveSetting(NameOf(Me.UseClipboard), Me.UseClipboard)
        My.Configger.SaveSetting(NameOf(Me.DetectSquealerObjects), Me.DetectSquealerObjects)
        My.Configger.SaveSetting(NameOf(Me.ShowBranch), Me.ShowBranch)
        My.Configger.SaveSetting(NameOf(Me.Wildcards.UseSpaces), Me.Wildcards.UseSpaces)
        My.Configger.SaveSetting(NameOf(Textify.ErrorAlert.Beep), Textify.ErrorAlert.Beep)
        My.Configger.SaveSetting(NameOf(Me.DirStyle), Me.DirStyle)

    End Sub

End Class