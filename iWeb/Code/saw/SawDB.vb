Imports Microsoft.VisualBasic
Imports System.Data
Imports System.Data.Common
Imports System.Reflection

<Serializable()> _
Public Class SawDB
    'Private _provider As IAWDBProvider
    Private _provider As IAWDBSqlProvider
    Private UserName As String
    Private Password As String
    Private DBName As String
    Private DBServer As String
    Private ApplicationName As String
    Private ConnectionTimeOut As Integer
    Private MinPoolSize As Integer
    Private MaxPoolSize As Integer
    Private CommandTimeout As Integer
    Private _ForceRedirect As Boolean
    Private ConnectionID As Integer

#Region "properties"
    Public ReadOnly Property Provider() As IAWDBProvider
        Get
            Return _provider
        End Get
    End Property
    Public ReadOnly Property Connection() As DbConnection
        Get
            Return _provider.Connection
        End Get
    End Property
    Public ReadOnly Property Reader() As DbDataReader
        Get
            Return _provider.Reader
        End Get
    End Property
    Public ReadOnly Property DataAdapter() As DbDataAdapter
        Get
            Return _provider.DataAdapter
        End Get
    End Property
    Public ReadOnly Property DTable() As DataTable
        Get
            Return _provider.DTable
        End Get
    End Property
    Public ReadOnly Property IsOpen() As Boolean
        Get
            Return _provider.IsOpen
        End Get
    End Property
    Public Shared ReadOnly Property ConnectionString() As String
        Get
            If ctx.cache("connection_string") Is Nothing Then
                Dim ConnStr As String = String.Empty
                Dim DB As New SawDB()
                DB.Close()
                ConnStr = DB.Provider.ConnectionString
                DB = Nothing
                ctx.cache.Insert("connection_string", ConnStr, Nothing, Date.Now.AddDays(1), Nothing, CacheItemPriority.NotRemovable, Nothing)
            End If
            Return ctx.cache("connection_string")
        End Get
    End Property
    Public Shared ReadOnly Property CanConnectToDB() As Boolean
        Get
            Dim ret As Boolean = False
            Dim str As String = String.Empty
            Dim DB As New SawDB()
            ret = DB.IsOpen
            DB.Close()
            DB = Nothing
            Return ret
        End Get
    End Property

    ''' <summary>
    ''' If set, gives the ability to force redirects 'on error'.
    ''' Currently used in Nonquery and HandleException.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ForceRedirectOnError() As Boolean
        Get
            Return Me._ForceRedirect
        End Get
        Set(ByVal value As Boolean)
            Me._ForceRedirect = value
        End Set
    End Property

    Private ReadOnly Property GetAppName()
        Get
            Dim AppN As String
            AppN = "iWeb"
            If Not String.IsNullOrEmpty(ctx.user_ref) Then
                If ctx.siteConfigDef("profileuser", "") = ctx.user_ref Then
                    AppN = "iWeb_" + ctx.user_ref
                End If
            End If
            Return AppN
        End Get
    End Property


#End Region

#Region "constructors"
    Public Sub New()
        init()
        open()
    End Sub

    Public Sub New(ByVal ForceRedirect As Boolean)
        init()
        ForceRedirectOnError = ForceRedirect
        open()
    End Sub

    'commented out by SJB 25.04.2007 when the above constructor was added
    'this one was not used anywhere in the code
    'Public Sub New(ByVal OpenConnection As Boolean)
    '    init()
    '    If OpenConnection Then
    '        open()
    '    End If
    'End Sub

    'primarily developed for Data Windows which need access
    'to an OLE connection, even if the default connection is sql
    'Public Sub New(ByVal connType As dbConnectionType)
    '    init()

    '    'Select Case connType
    '    '    Case dbConnectionType.odbc
    '    '        Throw New Exception("ODBC connections are not currently supported")
    '    '    Case dbConnectionType.ole
    '    '        _provider = New IAWDBOleProvider(Me.UserName, Me.Password, Me.DBName, Me.DBServer, "IngenWeb " + ctx.user_ref, Me.ConnectionTimeOut, Me.MinPoolSize, Me.MaxPoolSize)
    '    '    Case dbConnectionType.sqlclient
    '    _provider = New IAWDBSqlProvider(Me.UserName, Me.Password, Me.DBName, Me.DBServer, "IngenWeb " + ctx.user_ref, Me.ConnectionTimeOut, Me.MinPoolSize, Me.MaxPoolSize, Me.CommandTimeout)
    '    'End Select
    'End Sub
#End Region

    Private Sub init()
        DBServer = ctx.siteConfig("db_server").ToString
        DBName = ctx.siteConfig("db_name").ToString
        If String.IsNullOrEmpty(DBServer) Or String.IsNullOrEmpty(DBName) Then
            Throw New Exception("The siteSettings section in the web.config must contain a site tag with child tags of db_server, db_name db_connection, these tags are not allowed to be empty")
        End If
        UserName = DBName + "_iweb"
        Password = "gojsPwd23$"
        ConnectionTimeOut = CInt(ctx.siteConfigDef("connection_timeout", "15"))
        MinPoolSize = CInt(ctx.siteConfigDef("min_pool_size", "0"))
        MaxPoolSize = CInt(ctx.siteConfigDef("max_pool_size", "100"))
        CommandTimeout = CInt(ctx.siteConfigDef("command_timeout", "30"))

        ApplicationName = GetAppName

        Me._provider = Me.GetProvider()
        If Provider Is Nothing Then
            Throw New Exception("The IAWDBProvider has not been initialised, it cannot be nothing - " + ctx.session("IAWDBProvider"))
        End If
    End Sub

    Public Function Read() As Boolean
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim HasRead As Boolean
        Try
            If _provider.Read() Then
                HasRead = True
            End If
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return HasRead
    End Function

    Public Sub open()
        Try
            _provider.Open()
        Catch ex As Exception
            ctx.Response.Clear()
            ctx.Response.Write("The system is busy at the moment, please try again later")
            'ctx.Response.Flush()
            ctx.Response.End()
        End Try
        ctx.DbConnOpened()
#If DEBUG Then
        ConnectionID = SawDBConnList.AddDBConnList()
#End If
        ' Set the security context so that DB encryption functions will work
        Dim SecurityContext As String
        Dim Appname As String = GetAppName
        If Appname.Length > 20 Then Appname = Appname.Substring(0, 20)
        SecurityContext = Now().ToString("yyyy-MM-dd HH:mm:ss") + "|0|" + Appname
        NonQuery("exec dbo.dbp_add_context @Context='" + SawUtil.encrypt(SecurityContext) + "'")
    End Sub

    Public Sub Close()
        If _provider.IsOpen Then
            ctx.DbConnClosed()
        End If
        _provider.Close()

#If debug Then
    SawDBConnList.DeleteDBConnList(ConnectionID)
#End If
    End Sub

    Public Sub TranBegin()
        _provider.TranBegin()
    End Sub

    Public Sub TranRollback()
        _provider.TranRollback()
    End Sub

    Public Function TranCommit() As Boolean
        Return _provider.TranCommit
    End Function

    Public Function GetTable(ByVal aSQL As String) As DataTable
        Return GetTable(aSQL, New Object() {})
    End Function

    Public Function GetTable(ByVal aSQL As String, ByVal aObj() As Object) As DataTable
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim DT As DataTable = Nothing
        Try
            DT = _provider.GetTable(SawUtil.Substitute(aSQL), aObj)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return DT
    End Function

    Public Sub FillTable(ByVal aSQL As String)
        FillTable(aSQL, New Object() {})
    End Sub

    Public Sub FillTable(ByVal aSQL As String, ByVal aObj() As Object)
        Dim sTrace As StackTrace = New StackTrace(True)
        Try
            _provider.FillTable(SawUtil.Substitute(aSQL), aObj)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
    End Sub

    Public Sub FillTable(ByVal aSQL As String, ByVal aObj() As Object, ByVal requiresUpdate As Boolean)
        Dim sTrace As StackTrace = New StackTrace(True)
        Try
            _provider.FillTable(SawUtil.Substitute(aSQL), aObj, requiresUpdate)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
    End Sub

    ' Execute non-query SQL
    ' Returns number of rows affected or -1 for error
    Public Function NonQuery(ByVal aSQL As String, Optional ByVal aLogException As Boolean = True, Optional ByVal redirectOnError As Boolean = True) As Long
        Return NonQuery(aSQL, New Object() {}, aLogException, redirectOnError)
    End Function

    Public Function NonQueryNoSubstitute(ByVal aSQL As String, ByVal aOBJ() As Object) As Long
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim rows As Long
        Try
            rows = _provider.NonQuery(aSQL, aOBJ)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return rows
    End Function

    Public Function NonQuery(ByVal aSQL As String, _
                               ByVal aOBJ() As Object, _
                               Optional ByVal aLogException As Boolean = True, _
                               Optional ByVal redirectOnError As Boolean = True) As Long
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim rows As Long
        Try
            rows = _provider.NonQuery(SawUtil.Substitute(aSQL), aOBJ)
        Catch ex As IAWDBException
            If Not aLogException Then
                'if not logging exception then there is no point in redirecting
                redirectOnError = False
            End If
            If ex.MessageRef <> "" And Not ForceRedirectOnError Then
                ctx.session("DBError") = SawUtil.GetErrMsg(ex.MessageRef)
                '///don't redirect if there is a message to show
                redirectOnError = False
            End If
            'if redirecting then close DB
            If redirectOnError Then Close()
            If aLogException Then
                ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
                HandleException(ex, redirectOnError)
            End If
        End Try
        Return rows
    End Function


    ' Create a DataReader result set
    Public Sub Query(ByVal aSQL As String)
        Query(aSQL, New Object() {})
    End Sub

    Public Sub Query(ByVal aSQL As String, ByVal aOBJ() As Object)
        Dim sTrace As StackTrace = New StackTrace(True)
        Try
            '///close any open readers
            If _provider.Reader IsNot Nothing AndAlso Not _provider.Reader.IsClosed Then
                _provider.Reader.Close()
            End If
            _provider.Query(SawUtil.Substitute(aSQL), aOBJ)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
    End Sub

    Public Function ExecuteStoredProc(ByVal aSQL As String) As Integer
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim rows As Long
        Try
            rows = _provider.ExecuteStoredProc(aSQL)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return rows
    End Function

    Public Function ExecuteStoredProcDataTable(ByVal aSQL As String) As DataTable
        Dim sTrace As StackTrace = New StackTrace(True)
        Try
            Return _provider.ExecuteStoredProcDataTable(aSQL)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return New DataTable
    End Function

    Public Function ExecuteStoredProcDataSet(ByVal aSQL As String) As DataSet
        Dim sTrace As StackTrace = New StackTrace(True)
        Try
            Return _provider.ExecuteStoredProcDataSet(aSQL)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return New DataSet
    End Function

    Public Sub ClearParameters()
        Me._provider.ClearParameters()
    End Sub

    Public Function AddParameter(ByVal name As String, ByVal value As Object) As DbParameter
        Return _provider.AddParameter(name, value)
    End Function

    Public Function AddParameter(ByVal name As String, ByVal value As Object, ByVal direction As ParameterDirection) As DbParameter
        Return _provider.AddParameter(name, value, direction)
    End Function
    Public Function AddParameter(ByVal Name As String, ByVal datatype As DbType, ByVal direction As System.Data.ParameterDirection) As DbParameter
        Return _provider.AddParameter(Name, datatype, direction)
    End Function

    Public Function IsNull(ByVal aCol As String) As Boolean
        Return _provider.IsNull(aCol)
    End Function

    Public Function scalar(ByVal aSQL As String, ByVal aDefValue As Object) As Object
        Return scalar(aSQL, New Object() {}, aDefValue)
    End Function

    Public Function scalar(ByVal aSQL As String, ByVal aParams() As Object, ByVal aDefValue As Object) As Object
        Dim lRes As Object
        lRes = Me.scalar(aSQL, aParams)
        If lRes Is Nothing OrElse lRes Is DBNull.Value Then lRes = aDefValue
        Return lRes
    End Function

    ' Return a single result
    Public Function Scalar(ByVal aSQL As String) As Object
        Return Scalar(aSQL, New Object() {})
    End Function

    Public Function Scalar(ByVal aSQL As String, ByVal aOBJ() As Object) As Object
        Dim sTrace As StackTrace = New StackTrace(True)
        Dim obj As Object = Nothing
        Try
            obj = _provider.Scalar(SawUtil.Substitute(aSQL), aOBJ)
        Catch ex As IAWDBException
            ex.UpperStack = SawUtil.StackTraceToString(sTrace, True)
            HandleException(ex)
        End Try
        Return obj
    End Function

    Public Sub AddParms(ByVal aOBJ() As Object)
        _provider.AddParms(aOBJ)
    End Sub

    Public Function GetValue(ByVal aColumnName As String, ByVal aDefaultValue As String) As Object
        Return _provider.GetValue(aColumnName, aDefaultValue)
    End Function

    'Private Function GetProvider() As IAWDBProvider
    Private Function GetProvider() As IAWDBSqlProvider

        Return New IAWDBSqlProvider(Me.UserName, Me.Password, Me.DBName, Me.DBServer, Me.ApplicationName, Me.ConnectionTimeOut, Me.MinPoolSize, Me.MaxPoolSize, CommandTimeout)

    End Function

    '///************ handle exceptions *************
    Protected Overridable Sub HandleException(ByVal e As IAWDBException)
        HandleException(e, True)
    End Sub
    Protected Overridable Sub HandleException(ByVal e As IAWDBException, ByVal redirect As Boolean)
        If ForceRedirectOnError Then redirect = True
        LogException.HandleException(e, e.Sql, e.Param, e.UpperStack, redirect)
    End Sub


    '///************ shared function ******************

    Public Shared Function ExecNonQuery(ByVal sql As String) As Integer
        Return SawDB.ExecNonQuery(sql, New Object() {}, False)
    End Function

    Public Shared Function ExecNonQuery(ByVal sql As String, ByVal params() As Object) As Integer
        Return SawDB.ExecNonQuery(sql, params, False)
    End Function

    Public Shared Function ExecNonQuery(ByVal sql As String, ByVal params() As Object, ByVal ForceRedirect As Boolean) As Integer
        Dim DB As New SawDB
        Dim rows As Integer
        rows = DB.NonQuery(sql, params, True, True)
        DB.Close()
        DB = Nothing
        Return rows
    End Function

    Public Shared Function ExecScalar(ByVal ls_sql As String) As Object
        Return SawDB.ExecScalar(ls_sql, New Object() {}, Nothing)
    End Function

    Public Shared Function ExecScalar(ByVal ls_sql As String, ByVal defaultValue As Object) As Object
        Return ExecScalar(ls_sql, New Object() {}, defaultValue)
    End Function

    Public Shared Function ExecScalar(ByVal ls_sql As String, ByVal params() As Object) As Object
        Return SawDB.ExecScalar(ls_sql, params, Nothing)
    End Function

    Public Shared Function ExecScalar(ByVal sql As String, ByVal params() As Object, ByVal defaultValue As Object) As Object
        Dim DB As New SawDB
        Dim obj As Object
        obj = DB.scalar(sql, params, defaultValue)
        DB.Close()
        DB = Nothing
        Return obj
    End Function

    Public Shared Function ExecGetTable(ByVal sql As String) As Object
        Return SawDB.ExecGetTable(sql, New Object() {})
    End Function

    Public Shared Function ExecGetTable(ByVal sql As String, ByVal params() As Object) As Object
        Dim DB As New SawDB
        Dim DT As DataTable
        DT = DB.GetTable(SawUtil.Substitute(sql), params)
        DB.Close()
        DB = Nothing
        Return DT
    End Function

End Class
