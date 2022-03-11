﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Squealer.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to declare @table_id int = {TableId};
        '''
        '''with ctePkCols as
        '''(
        '''	select
        '''		i.object_id as table_id
        '''		,c.column_id
        '''	from
        '''		sys.indexes i
        '''	join
        '''		sys.index_columns c
        '''		on c.object_id = i.object_id
        '''		and c.index_id = i.index_id
        '''	where
        '''		i.is_primary_key = &apos;true&apos;
        ''')
        ''',cteDefaults as
        '''(
        '''	select
        '''		c.parent_object_id as table_id
        '''		,c.parent_column_id as column_id
        '''		,c.definition
        '''	from
        '''		sys.default_constraints c
        ''')
        '''select
        '''	c.name as [column]
        '''	,ty.name as [type]
        '''	,c.is_identity
        '''	,c.is_rowguidcol
        '''	, [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property AutoGetColumns() As String
            Get
                Return ResourceManager.GetString("AutoGetColumns", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to with cteOperations as
        '''(
        '''	select &apos;Insert&apos; as [operation]
        '''	union select &apos;Update&apos;
        '''	union select &apos;Delete&apos;
        '''	union select &apos;Get&apos;
        '''	union select &apos;List&apos;
        ''')
        '''select
        '''	s.name as [schema]
        '''	,t.name as [table]
        '''	,t.object_id as [table_id]
        '''	,o.Operation
        '''from
        '''	sys.tables t
        '''join
        '''	sys.schemas s
        '''	on s.schema_id = t.schema_id
        '''cross join
        '''	cteOperations o
        '''--where
        '''--	s.name like &apos;%&apos; + @schemafilter + &apos;%&apos;
        '''--	and t.name like &apos;%&apos; + @tablefilter + &apos;%&apos;
        '''order by
        '''	s.name
        '''	,t.name
        '''	,o.Operation
        '''.
        '''</summary>
        Friend ReadOnly Property AutoGetTables() As String
            Get
                Return ResourceManager.GetString("AutoGetTables", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;StoredProcedure&quot;&gt;
        '''&lt;Comments&gt;&lt;![CDATA[{Comments}]]&gt;&lt;/Comments&gt;
        '''&lt;Parameters&gt;{Parameters}&lt;/Parameters&gt;
        '''&lt;Code&gt;&lt;![CDATA[{Code}]]&gt;&lt;/Code&gt;
        '''&lt;Users&gt;{Users}&lt;/Users&gt;
        '''&lt;/Squealer&gt;.
        '''</summary>
        Friend ReadOnly Property AutoProcTemplate() As String
            Get
                Return ResourceManager.GetString("AutoProcTemplate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to /*********************
        '''      CHANGE LOG
        '''*********************/
        '''
        '''{0}2019-02-21{1}
        '''
        '''{2}Added a switch to the GENERATE command to optionally generate test scripts instead of CREATE scripts.
        '''
        '''{2}Added a change log. :)
        '''
        '''
        '''
        '''{0}2019-03-18{1}
        '''
        '''{2}Added SQLSERVER command to save/read a connection string for each project folder.
        '''
        '''{2}Added MAKE command to automatically generate stored procedures by reading the table structures in a SQL database.
        '''
        '''{2}Bug fixes and performance improvements.
        '''
        '''
        '''
        '''{0}20 [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property ChangeLog() As String
            Get
                Return ResourceManager.GetString("ChangeLog", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property Folder() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("Folder", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to /***********************************************
        '''	INSTRUCTIONS:
        '''	Paste this script into a database of your choosing.
        '''	Upon execution, three things will be created:
        '''	1. dbo.Leaderboard (table)
        '''	2. dbo.LeaderboardAdd (stored procedure)
        '''	3. dbo.LeaderboardRead (stored procedure)
        '''	Make sure all players have permission to execute 
        '''	the two stored procedures.
        '''***********************************************/
        '''	
        '''create table [dbo].[Leaderboard] (
        '''	[Signature] char(3) not null
        '''	, [Points] int not null
        '''	 [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property LeaderboardCreate() As String
            Get
                Return ResourceManager.GetString("LeaderboardCreate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to select
        '''	concat(s.name,&apos;.&apos;,o.name) as object_name
        '''	,o.type
        '''	,m.definition
        '''	,o.object_id
        '''from
        '''	sys.objects o
        '''join
        '''	sys.sql_modules m
        '''	on m.object_id = o.object_id
        '''join
        '''	sys.schemas s
        '''	on s.schema_id = o.schema_id
        '''where
        '''	o.type in (&apos;P&apos;,&apos;FN&apos;,&apos;TF&apos;,&apos;IF&apos;,&apos;V&apos;)
        '''order by
        '''	s.name
        '''	,o.name.
        '''</summary>
        Friend ReadOnly Property ObjectList() As String
            Get
                Return ResourceManager.GetString("ObjectList", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to select
        '''	p.name parameter_name
        '''	,t.name type_name
        '''	,p.is_output
        '''	,p.max_length
        '''from
        '''	sys.parameters p
        '''join
        '''	sys.types t
        '''	on t.system_type_id = p.system_type_id
        '''where
        '''	p.object_id = @ObjectId
        '''order by
        '''	p.parameter_id.
        '''</summary>
        Friend ReadOnly Property ObjectParameters() As String
            Get
                Return ResourceManager.GetString("ObjectParameters", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to select
        '''--	concat(&apos;[&apos;,s.name,&apos;].[&apos;,o.name,&apos;]&apos;) as object_name
        '''	u.name user_name
        '''from
        '''	sys.objects o
        '''join
        '''	sys.schemas s
        '''	on s.schema_id = o.schema_id
        '''join
        '''	sys.syspermissions p
        '''	on p.id = o.object_id
        '''join
        '''	sys.sysusers u
        '''	on u.uid = p.grantee
        '''where
        '''	o.object_id = @ObjectId.
        '''</summary>
        Friend ReadOnly Property ObjectPermissions() As String
            Get
                Return ResourceManager.GetString("ObjectPermissions", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        '''</summary>
        Friend ReadOnly Property PigNose() As System.Drawing.Icon
            Get
                Dim obj As Object = ResourceManager.GetObject("PigNose", resourceCulture)
                Return CType(obj,System.Drawing.Icon)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''There are two ways to cause the stored procedure to roll back the transaction.
        '''
        '''
        '''(1) Set the return code which will bubble up to the outermost nested stored procedure. MSSQL will not generate an error.
        '''
        '''begin
        '''	set @SqlrInternalErrorNumber = ?; -- Value must be between -99999 and -1.
        '''	raiserror(&apos;&apos;,11,1);
        '''end;
        '''
        '''
        '''(2) Cause a fatal SQL crash that must be handled by the calling process outside of the outermost nested procedure.
        '''begin
        '''	set @SqlrInternalErrorNumber = -999999;
        '''	raiserror(&apos;An error h [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property RaiseErrors() As String
            Get
                Return ResourceManager.GetString("RaiseErrors", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        ''')
        '''
        '''returns table
        '''
        '''{WithOptions}
        '''as
        '''
        '''return.
        '''</summary>
        Friend ReadOnly Property SqlBeginInlineTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlBeginInlineTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        ''')
        '''
        '''{WithOptions}
        '''as
        '''begin
        '''set ansi_nulls on;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlBeginMultiStatementTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlBeginMultiStatementTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        ''')
        '''
        '''set ansi_nulls on;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlBeginMultiStatementTableFunctionTest() As String
            Get
                Return ResourceManager.GetString("SqlBeginMultiStatementTableFunctionTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/***********************************************************************
        '''	Begin the transaction. Start the TRY..CATCH wrapper.
        '''***********************************************************************/
        '''
        '''              -- !!!!!  DO NOT EDIT THIS SECTION  !!!!! --
        '''
        '''{WithOptions}
        '''as
        '''set ansi_nulls on;
        '''set nocount on;
        '''set quoted_identifier on;
        '''
        '''declare @SqlrInternalErrorNumber	int; -- Error code to return to parent process.
        '''declare @SqlrInternalNestLevel		int; -- Current nested level of procedure ca [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlBeginProcedure() As String
            Get
                Return ResourceManager.GetString("SqlBeginProcedure", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''
        '''set ansi_nulls on;
        '''set quoted_identifier on;
        '''
        '''declare @SqlrInternalErrorNumber int = 0; -- Not used in test scripts, but declared to avoid errors.
        '''
        '''begin transaction
        '''
        '''/*######################################################################
        '''                         YOUR CODE BEGINS HERE.
        '''######################################################################*/
        ''';.
        '''</summary>
        Friend ReadOnly Property SqlBeginProcedureTest() As String
            Get
                Return ResourceManager.GetString("SqlBeginProcedureTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        ''')
        '''
        '''returns {ReturnDataType}
        '''
        '''{WithOptions}
        '''as
        '''begin
        '''set ansi_nulls on;
        '''
        '''declare @Result {ReturnDataType}
        '''
        ''';.
        '''</summary>
        Friend ReadOnly Property SqlBeginScalarFunction() As String
            Get
                Return ResourceManager.GetString("SqlBeginScalarFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''set ansi_nulls on;
        '''
        '''declare @Result {ReturnDataType};
        '''
        ''';.
        '''</summary>
        Friend ReadOnly Property SqlBeginScalarFunctionTest() As String
            Get
                Return ResourceManager.GetString("SqlBeginScalarFunctionTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''{WithOptions}
        '''as
        '''.
        '''</summary>
        Friend ReadOnly Property SqlBeginView() As String
            Get
                Return ResourceManager.GetString("SqlBeginView", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/***********************************************************************
        '''
        '''title : {Schema}.{RootProgramName}
        '''{Comments}
        '''***********************************************************************/
        '''.
        '''</summary>
        Friend ReadOnly Property SqlComment() As String
            Get
                Return ResourceManager.GetString("SqlComment", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''create function [{Schema}].[{RootProgramName}]
        '''(
        '''.
        '''</summary>
        Friend ReadOnly Property SqlCreateFunction() As String
            Get
                Return ResourceManager.GetString("SqlCreateFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''create procedure [{Schema}].[{RootProgramName}]
        '''.
        '''</summary>
        Friend ReadOnly Property SqlCreateProcedure() As String
            Get
                Return ResourceManager.GetString("SqlCreateProcedure", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''create view [{Schema}].[{RootProgramName}]
        '''.
        '''</summary>
        Friend ReadOnly Property SqlCreateView() As String
            Get
                Return ResourceManager.GetString("SqlCreateView", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''if object_id(&apos;[{Schema}].[{RootProgramName}]&apos;,&apos;p&apos;) is not null
        '''	drop procedure [{Schema}].[{RootProgramName}];
        '''if object_id(&apos;[{Schema}].[{RootProgramName}]&apos;,&apos;fn&apos;) is not null
        '''	drop function [{Schema}].[{RootProgramName}];
        '''if object_id(&apos;[{Schema}].[{RootProgramName}]&apos;,&apos;if&apos;) is not null
        '''	drop function [{Schema}].[{RootProgramName}];
        '''if object_id(&apos;[{Schema}].[{RootProgramName}]&apos;,&apos;tf&apos;) is not null
        '''	drop function [{Schema}].[{RootProgramName}];
        '''if object_id(&apos;[{Schema}].[{RootProgramName}]&apos;,&apos;v&apos;) is not  [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlDrop() As String
            Get
                Return ResourceManager.GetString("SqlDrop", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to set nocount on;
        '''
        '''create table #CodeToDrop ([Type] nvarchar(10), [Schema] nvarchar(10), [Name] nvarchar(500));
        '''
        '''{RoutineList}
        '''
        '''with cteObjects as
        '''(
        '''	select
        '''		r.ROUTINE_TYPE as ObjectType
        '''		,r.ROUTINE_SCHEMA as ObjectSchema
        '''		,r.ROUTINE_NAME as ObjectName
        '''	from
        '''		INFORMATION_SCHEMA.ROUTINES r
        '''	union
        '''	select
        '''		&apos;VIEW&apos;
        '''		,v.TABLE_SCHEMA
        '''		,v.TABLE_NAME
        '''	from
        '''		INFORMATION_SCHEMA.VIEWS v
        ''')
        '''select
        '''	o.ObjectType collate database_default as ObjectType
        '''	,o.ObjectSchema collate database_default [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlDropOrphanedRoutines() As String
            Get
                Return ResourceManager.GetString("SqlDropOrphanedRoutines", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''return
        '''end.
        '''</summary>
        Friend ReadOnly Property SqlEndMultiStatementTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlEndMultiStatementTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''select * from @TableValue;.
        '''</summary>
        Friend ReadOnly Property SqlEndMultiStatementTableFunctionTest() As String
            Get
                Return ResourceManager.GetString("SqlEndMultiStatementTableFunctionTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/*######################################################################
        '''                          YOUR CODE ENDS HERE.
        '''######################################################################*/
        '''
        '''/***********************************************************************
        '''	Commit the transaction. If this is the parent process, then all
        '''	pending work will be written to the database. If this is a child
        '''    process, then the commit statement will merely decrement the
        '''	@@trancount system variable.
        '''******** [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlEndProcedure1() As String
            Get
                Return ResourceManager.GetString("SqlEndProcedure1", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 		set @SqlrInternalErrorMessage =
        '''			@SqlrInternalErrorMessage
        '''			+ char(10)
        '''			+ &apos;@{ErrorParameterName} = &apos;
        '''			+ isnull(convert(varchar(max),@{ErrorParameterName}),&apos;[NULL]&apos;);.
        '''</summary>
        Friend ReadOnly Property SqlEndProcedure2() As String
            Get
                Return ResourceManager.GetString("SqlEndProcedure2", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''	end
        '''	else
        '''	-- The exception was raised in a nested procedure, so just grab the message it bubbled up to this procedure.
        '''	begin
        '''		set @SqlrInternalErrorMessage = error_message();
        '''	end
        '''
        '''	print concat(&apos;exception - nest level is &apos;,@SqlrInternalNestLevel,&apos;; tran count is &apos;,@@trancount,&apos;; xact state is &apos;,xact_state());
        '''
        '''	if @SqlrInternalNestLevel =	0
        '''	-- We&apos;re at the outermost procedure, so rollback the whole transaction.
        '''	begin
        '''		if xact_state() in (1,-1)
        '''			rollback transaction
        '''	end
        '''	else
        '''	-- [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlEndProcedure3() As String
            Get
                Return ResourceManager.GetString("SqlEndProcedure3", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/*######################################################################
        '''                          YOUR CODE ENDS HERE.
        '''######################################################################*/
        '''
        '''-- This script defaults to ROLLBACK so you can repeat your testing.
        '''
        '''rollback transaction
        '''--commit transaction
        '''
        '''print &apos;@SqlrInternalErrorNumber = &apos; + convert(varchar,(@SqlrInternalErrorNumber));
        '''.
        '''</summary>
        Friend ReadOnly Property SqlEndProcedureTest() As String
            Get
                Return ResourceManager.GetString("SqlEndProcedureTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/***********************************************************************
        '''    Return the function result.
        '''***********************************************************************/
        '''
        '''return @Result
        '''end
        '''.
        '''</summary>
        Friend ReadOnly Property SqlEndScalarFunction() As String
            Get
                Return ResourceManager.GetString("SqlEndScalarFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''/***********************************************************************
        '''    Return the function result.
        '''***********************************************************************/
        '''
        '''select @Result as [Result]
        '''.
        '''</summary>
        Friend ReadOnly Property SqlEndScalarFunctionTest() As String
            Get
                Return ResourceManager.GetString("SqlEndScalarFunctionTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to grant execute on [{Schema}].[{RootProgramName}] to [{User}];.
        '''</summary>
        Friend ReadOnly Property SqlGrantExecute() As String
            Get
                Return ResourceManager.GetString("SqlGrantExecute", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to grant select on [{Schema}].[{RootProgramName}] to [{User}];.
        '''</summary>
        Friend ReadOnly Property SqlGrantSelect() As String
            Get
                Return ResourceManager.GetString("SqlGrantSelect", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        ''')
        '''
        '''returns @TableValue table
        '''(
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTableMultiStatementTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlTableMultiStatementTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to 
        '''
        '''declare @TableValue table
        '''(
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTableMultiStatementTableFunctionTest() As String
            Get
                Return ResourceManager.GetString("SqlTableMultiStatementTableFunctionTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;{RootType}&quot;&gt;
        '''&lt;Parameters&gt;
        '''&lt;!--Parameters--&gt;
        '''&lt;/Parameters&gt;
        '''&lt;Code/&gt;
        '''&lt;Users/&gt;
        '''&lt;/Squealer&gt;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTemplateInlineTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlTemplateInlineTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;{RootType}&quot;&gt;
        '''&lt;Parameters&gt;
        '''&lt;!--Parameters--&gt;
        '''&lt;/Parameters&gt;
        '''&lt;Code/&gt;
        '''&lt;Users/&gt;
        '''&lt;/Squealer&gt;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTemplateMultiStatementTableFunction() As String
            Get
                Return ResourceManager.GetString("SqlTemplateMultiStatementTableFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;{RootType}&quot;&gt;
        '''&lt;Parameters&gt;
        '''&lt;!--Parameters--&gt;
        '''&lt;/Parameters&gt;
        '''&lt;Code/&gt;
        '''&lt;Users/&gt;
        '''&lt;/Squealer&gt;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTemplateProcedure() As String
            Get
                Return ResourceManager.GetString("SqlTemplateProcedure", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;{RootType}&quot;&gt;
        '''&lt;Parameters&gt;
        '''&lt;!--Parameters--&gt;
        '''&lt;/Parameters&gt;
        '''&lt;Returns Type=&quot;{ReturnDataType}&quot; /&gt;
        '''&lt;Code/&gt;
        '''&lt;Users/&gt;
        '''&lt;/Squealer&gt;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTemplateScalarFunction() As String
            Get
                Return ResourceManager.GetString("SqlTemplateScalarFunction", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;Squealer Type=&quot;{RootType}&quot;&gt;
        '''&lt;Code/&gt;
        '''&lt;Users/&gt;
        '''&lt;/Squealer&gt;
        '''.
        '''</summary>
        Friend ReadOnly Property SqlTemplateView() As String
            Get
                Return ResourceManager.GetString("SqlTemplateView", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to /***********************************************************************
        '''	Delete deprecated squealer log tables.
        '''***********************************************************************/
        '''
        '''if exists (select 1 from sys.objects where name like &apos;%squealer%&apos;)
        '''	or exists (select 1 from sys.schemas where name = &apos;squealer&apos;)
        '''select 
        '''	s.name
        '''	,o.name
        '''	,o.type_desc
        '''from
        '''	sys.objects o
        '''join
        '''	sys.schemas s
        '''	on s.schema_id = o.schema_id
        '''where
        '''	o.name like &apos;%squealer%&apos;
        '''	or s.name = &apos;squealer&apos;
        '''union
        '''selec [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SqlTopScript() As String
            Get
                Return ResourceManager.GetString("SqlTopScript", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        '''&lt;!-- Project name maximum 10 characters (longer names will be truncated). --&gt;
        '''&lt;Settings ProjectName=&quot;myproject&quot;&gt;
        '''	&lt;DefaultUsers&gt;
        '''		&lt;User Name=&quot;User1&quot; /&gt;
        '''		&lt;User Name=&quot;User2&quot; /&gt;
        '''	&lt;/DefaultUsers&gt;
        '''&lt;!-- String replacements are case-sensitive and applied sequentially. Format &quot;$TEXT$&quot; is not required but recommended as a visual cue. --&gt;
        '''	&lt;StringReplacements&gt;
        '''		&lt;String Original=&quot;$YODA-QUOTE$&quot; Replacement=&quot;$AFFIRMATIVE$ or $NEGATORY$. There is no Try.&quot; Comment=&quot;&quot; / [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property UserConfig() As String
            Get
                Return ResourceManager.GetString("UserConfig", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
