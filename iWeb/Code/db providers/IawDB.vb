Imports System.Reflection
Imports System.Data.SqlClient
Imports System.Data.Common


Public Class IawDB
    Implements IDisposable

#Region "Vars & Properties & Constructors"
    Private AsCom As Boolean = False
    Private ConnectionID As Integer
    Private _ConnectionString As String = String.Empty
    Public IsOpen As Boolean = False
    Public Reader As SqlDataReader
    Private DataAdapter As SqlDataAdapter
    Private _DTable As DataTable
    Public Parameters As New List(Of SqlParameter)
    Private paramCounter As Integer
    Private IsTran As Boolean = False
    Private _Connection As SqlConnection
    Private Transaction As SqlTransaction
    Private Command As SqlCommand
    Private CommandBuilder As SqlCommandBuilder
    Private CommandTimeout As Integer

    Private ReadOnly Property GetApplicationName() As String
        Get
            Dim AppName As String = "ViviChart"
            If Not String.IsNullOrEmpty(ctx.user_ref) Then
                If ctx.siteConfigDef("profileuser", "") = ctx.user_ref Then
                    AppName = "ViviChart_" + ctx.user_ref
                End If
            End If
            Return AppName
        End Get
    End Property

    Private ReadOnly Property BuildConnectionString(ByVal User As String) As String
        Get
            Dim connBuilder As New SqlConnectionStringBuilder

            With connBuilder
                .DataSource = ctx.siteConfig("db_server").ToString
                .InitialCatalog = ctx.siteConfig("db_name").ToString

                If String.IsNullOrEmpty(.DataSource) Or
                   String.IsNullOrEmpty(.InitialCatalog) Then
                    Throw New Exception("The database section in siteSettings has not been setup properly")
                End If

                .UserID = connBuilder.InitialCatalog + "_" + User
                .Password = "gojsPwd23$"
                .ConnectTimeout = CInt(ctx.siteConfigDef("connection_timeout", 15))
                .ApplicationName = GetApplicationName
                .MinPoolSize = CInt(ctx.siteConfigDef("min_pool_size", 0))
                .MaxPoolSize = CInt(ctx.siteConfigDef("max_pool_size", 100))
            End With

            Return connBuilder.ToString
        End Get
    End Property

    Public ReadOnly Property comConenctionString() As String
        Get
            Return BuildConnectionString("com")
        End Get
    End Property

    Public ReadOnly Property iWebConnectionString() As String
        Get
            Return BuildConnectionString("iweb")
        End Get
    End Property

    Public ReadOnly Property ConnectionString() As String
        Get
            If AsCom Then
                Return comConenctionString
            Else
                Return iWebConnectionString
            End If
        End Get
    End Property

    Public ReadOnly Property getConnectionString() As String
        Get
            Return Regex.Replace(Me.ConnectionString, ";Password=\w+?;", ";Password=;")
        End Get
    End Property

    Private ReadOnly Property Connection() As SqlConnection
        Get
            If _Connection Is Nothing Then
                _Connection = New SqlConnection(ConnectionString)
            End If
            Return _Connection
        End Get
    End Property

    Public ReadOnly Property DTable() As DataTable
        Get
            If _DTable Is Nothing Then
                _DTable = New DataTable
            End If
            Return _DTable
        End Get
    End Property

    Public Sub New()
        AsCom = False
        Open()
    End Sub
    Public Sub New(ConnectAsCom As Boolean)
        AsCom = True
        Open()
    End Sub


#End Region

#Region "Shared Accessors"

    ''' <summary>
    ''' Test whether a database connection can be made
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CanConnect() As Boolean
        Dim ret As Boolean = False
        Using DB As New IawDB
            ret = DB.IsOpen
        End Using
        Return ret
    End Function

    ''' <summary>
    ''' Create a new connection and execute a non-query
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <param name="Args"></param>
    ''' <returns></returns>
    ''' <remarks>Args start at @p1</remarks>
    Public Shared Function execNonQuery(ByVal SQL As String,
                                        ByVal ParamArray Args() As Object) As Long
        Dim lCount As Long = 0
        Using DB As New IawDB
            lCount = DB.NonQuery(SQL, Args)
        End Using
        Return lCount
    End Function

    Public Shared Function execScalar(ByVal SQL As String,
                                      ByVal ParamArray Args() As Object) As Object
        Dim OBJ As Object = Nothing
        Using DB As New IawDB
            OBJ = DB.Scalar(SQL, Args)
        End Using
        Return OBJ
    End Function

    Public Shared Function execScalarDefault(ByVal SQL As String,
                                             ByVal Dflt As Object,
                                             ByVal ParamArray Args() As Object) As Object
        Dim OBJ As Object = Nothing
        Using DB As New IawDB
            OBJ = DB.ScalarDefault(SQL, Dflt, Args)
        End Using
        Return OBJ
    End Function

    Public Shared Function execGetTable(ByVal SQL As String,
                                        ByVal ParamArray Args() As Object) As DataTable
        Dim DT As DataTable
        Using DB As New IawDB
            DT = DB.GetTable(SQL, Args)
        End Using
        Return DT
    End Function

    Public Shared Function execGetDataRow(ByVal SQL As String,
                                          ByVal ParamArray Args() As Object) As Datarow
        Dim DR As DataRow
        Using DB As New IawDB
            DR = DB.GetDataRow(SQL, Args)
        End Using
        Return DR
    End Function


    Public Shared Function doesColumnExist(table_id As String, column_id As String) As Boolean
        Return execScalar("select case when col_length(@p1,@p2) IS null then 0 else 1 end", table_id, column_id) = 1
    End Function

#End Region

#Region "Connection Open/Close"

    ''' <summary>
    ''' Open a database connection
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Open()
        If Me.IsOpen Then Return
        For I As Integer = 1 To 10
            Try
                Me.Connection.Open()
                Me.IsOpen = True
                ctx.DbConnOpened()
#If DEBUG Then
                ConnectionID = SawDBConnList.AddDBConnList()
#End If
                Exit For
            Catch e As Exception
                Me.IsOpen = False
                If I = 10 Then
                    Throw New Exception("Unable to conenct to the database")
                End If
            End Try
            System.Threading.Thread.Sleep(500)
        Next

        ' Set the security context so that DB encryption functions will work
        Dim SecurityContext As String
        Dim Appname As String = GetApplicationName
        If Appname.Length > 20 Then Appname = Appname.Substring(0, 20)
        SecurityContext = Now().ToString("yyyy-MM-dd HH:mm:ss") + "|0|" + Appname
        AddParameter("@Context", SawUtil.encrypt(SecurityContext))
        CallNonQueryStoredProc("dbo.dbp_add_context")
        ClearParameters()
    End Sub

    ''' <summary>
    ''' Close a database transaction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Close()
        If IsOpen Then
            If Reader IsNot Nothing AndAlso Not Reader.IsClosed Then
                Reader.Close()
            End If
            Connection.Close()
            IsOpen = False
            ctx.DbConnClosed()
        End If
#If DEBUG Then
        SawDBConnList.DeleteDBConnList(ConnectionID)
#End If
    End Sub
#End Region

#Region "Transaction"

    ''' <summary>
    ''' Start a database transaction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub TranBegin()
        IsTran = True
        Transaction = Connection.BeginTransaction
    End Sub

    ''' <summary>
    ''' Rollback a database transaction
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub TranRollback()
        If IsTran AndAlso Transaction IsNot Nothing Then
            Transaction.Rollback()
            Transaction = Nothing
            IsTran = False
        End If
    End Sub

    ''' <summary>
    ''' Commit a database transaction
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function TranCommit() As Boolean
        Dim lRes As Boolean
        If Not IsTran Then Return True
        Try
            Transaction.Commit()
            Transaction = Nothing
            lRes = True
        Catch
            Transaction.Rollback()
            Transaction = Nothing
            lRes = False
        End Try
        IsTran = False
        Return lRes
    End Function

#End Region

#Region "Helpers"

    Private Sub AddParms(ByVal Args() As Object)
        If Args.Length = 0 Then Return

        Dim count As Integer = 1
        Dim param As String
        For Each Arg As Object In Args
            param = "@p" + count.ToString
            If Arg Is Nothing Then Arg = DBNull.Value

            Select Case Arg.GetType.ToString.ToLower
                Case "system.dbnull"
                    Command.Parameters.AddWithValue(param, System.DBNull.Value)
                Case "system.datetime"
                    ' if date is assigned value of nothing it actually still contains jan 1st 0001, so we just set that as a db null.
                    If Arg = #1/1/0001# Then
                        Command.Parameters.AddWithValue(param, System.DBNull.Value)
                    Else
                        Command.Parameters.Add(param, SqlDbType.DateTime).Value = Arg
                    End If

                Case "system.string"
                    Command.Parameters.Add(param, SqlDbType.NVarChar).Value = DirectCast(Arg, String)
                Case "system.int32"
                    Command.Parameters.Add(param, SqlDbType.Int).Value = Arg
                Case "system.decimal"
                    Command.Parameters.Add(param, SqlDbType.Decimal).Value = Arg
                Case "system.byte[]"
                    Command.Parameters.Add(param, SqlDbType.Image).Value = Arg
            End Select

            count += 1
        Next
    End Sub

    Private Sub NewCommand(ByVal SQL As String)
        If IsTran Then
            Command = New SqlCommand(SQL, Connection, Transaction)
        Else
            Command = New SqlCommand(SQL, Connection)
        End If
        Command.CommandTimeout = CommandTimeout
    End Sub

    Public Function GetCacheDependency() As SqlCacheDependency
        If Command IsNot Nothing Then
            Return New SqlCacheDependency(Command)
        End If
        Return Nothing
    End Function

    Private Sub CheckColumn(ByVal Col As String)
        If Reader Is Nothing Then
            Throw New Exception("Data has not been read yet")
        End If
        GetOrdinal(Col)
    End Sub

    ''' <summary>
    ''' Return the ordinal number of the specified column
    ''' </summary>
    ''' <param name="Col"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetOrdinal(ByVal Col As String) As Integer
        Dim iOrdinal As Integer
        Try
            iOrdinal = Reader.GetOrdinal(Col)
        Catch ex As Exception
            RaiseException(Col + " is not a column", ex)
        End Try
        Return iOrdinal
    End Function

    ''' <summary>
    ''' Check whether the specified column is null
    ''' </summary>
    ''' <param name="Col"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function isNull(ByVal Col As String) As Boolean
        CheckColumn(Col)
        If Reader(Col) Is DBNull.Value Then Return True
        If Reader(Col) Is Nothing Then Return True
        Return False
    End Function

    ''' <summary>
    ''' Return either the value for a column or if it's null, return the specified default
    ''' </summary>
    ''' <param name="Col"></param>
    ''' <param name="Dflt"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetValue(ByVal Col As String,
                             ByVal Dflt As Object) As Object
        CheckColumn(Col)
        Try
            If Reader(Col) Is System.DBNull.Value Then
                Return Dflt
            ElseIf Reader(Col) Is Nothing Then
                Return Dflt
            Else
                Return Reader(Col)
            End If
        Catch ex As Exception
            Throw New Exception("Cannot read data for '" + Col + "'")
        End Try
    End Function

    Public Shared Function GetValueDef(Of T)(ByVal Value As T, ByVal DefaultValue As T) As T
        If Value Is Nothing OrElse IsDBNull(Value) Then
            Return DefaultValue
        Else
            Return Value
        End If
    End Function

#End Region

#Region "Exceptions"

    Protected Sub RaiseException(ByVal message As String, ByVal e As Exception)
        If Me.Command IsNot Nothing Then
            Throw New IAWDBException(message, e, Me.Command.CommandText, Me.Command.Parameters)
        End If
        Throw New IAWDBException(message, e)
    End Sub

    Protected Sub RaiseException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Throw New IAWDBException(e, aSql, aParams)
    End Sub

    Protected Sub RaiseSqlException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Dim EC As ErrorCode = GetErrorCode(e)
        Throw New IAWDBException(e, aSql, aParams, EC.ErrorCode, EC.MessageRef)
    End Sub

    Protected Overridable Function GetErrorCode(ByVal aExc As DbException) As ErrorCode
        Dim lDupKey As Integer = 0
        Dim lBadFkey As Integer = 0
        Dim lBadRow As Integer = 0
        Dim lMissingCols As Integer = 0
        Dim lRI As Integer = 0
        Dim lInvalidCols As Integer = 0
        Dim lDupIndex As Integer = 0
        Dim lErrNo As String = String.Empty

        lDupKey = 2627       ' duplicate row
        lBadFkey = 51002     ' foreign key not resolved
        lBadRow = 51007      ' foreign key not resolved
        lMissingCols = 233   ' column(s) are NOT NULLABLE
        lRI = 51001          ' dependant rows restrict deletion
        lInvalidCols = -209  ' invalid value for a column
        lDupIndex = 2601     ' duplicate alternate key

        'use reflection
        Dim errorCollection As ICollection = Nothing
        Dim Errors As PropertyInfo = aExc.GetType.GetProperty("Errors")
        If Errors IsNot Nothing Then
            errorCollection = Errors.GetGetMethod.Invoke(aExc, Nothing)
        End If

        Dim errorCode As Integer
        Dim iterate As IEnumerator = errorCollection.GetEnumerator

        While iterate.MoveNext

            Select Case iterate.Current.GetType.ToString
                Case "System.Data.OleDb.OleDbError"
                    errorCode = CType(iterate.Current, System.Data.OleDb.OleDbError).NativeError
                Case "System.Data.SqlClient.SqlError"
                    errorCode = CType(iterate.Current, System.Data.SqlClient.SqlError).Number
                Case Else
                    Throw New Exception("error handling has not been configured for database errors of type: " + iterate.Current.GetType.ToString)
            End Select

            Select Case errorCode
                Case lDupKey
                    lErrNo = "ER_00131"
                Case lBadFkey, lBadRow
                    lErrNo = "ER_00011"
                Case lMissingCols
                    lErrNo = "ER_00012"
                Case lRI
                    lErrNo = "ER_00013"
                Case lInvalidCols
                    lErrNo = "ER_00014"
                Case lDupIndex
                    lErrNo = "ER_00131"
            End Select
            If lErrNo <> "" Then Exit While

        End While

        Return New ErrorCode(errorCode, lErrNo)

    End Function

    Protected Class ErrorCode
        Public ErrorCode As String
        Public MessageRef As String
        Public Sub New(ByVal code As String, ByVal ref As String)
            Me.ErrorCode = code
            Me.MessageRef = ref
        End Sub
    End Class

#End Region

#Region "Standard SQL"

    Public Sub Query(ByVal SQL As String,
                     ByVal ParamArray Args() As Object)
        Try
            NewCommand(SQL)
            AddParms(Args)
            Reader = Command.ExecuteReader
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
    End Sub

    Public Function NonQuery(ByVal SQL As String,
                             ByVal ParamArray Args() As Object) As Long
        Dim Count As Long
        Try
            NewCommand(SQL)
            AddParms(Args)
            Count = Command.ExecuteNonQuery()
        Catch o As SqlException
            If IsTran Then TranRollback()
            RaiseSqlException(o, SQL, Command.Parameters)
            Return 0
        Catch e As Exception
            If IsTran Then TranRollback()
            RaiseException(e, SQL, Command.Parameters)
            Return 0
        End Try
        Return Count
    End Function

    Public Function Scalar(ByVal SQL As String,
                           ByVal ParamArray Args() As Object) As Object
        Dim OBJ As Object = Nothing
        Try
            NewCommand(SQL)
            AddParms(Args)
            OBJ = Command.ExecuteScalar
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
        Return OBJ
    End Function

    Public Function ScalarDefault(ByVal SQL As String,
                                  ByVal Dflt As Object,
                                  ByVal ParamArray Args() As Object) As Object
        Dim OBJ As Object = Nothing
        Try
            NewCommand(SQL)
            AddParms(Args)
            OBJ = Command.ExecuteScalar
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
        If OBJ Is Nothing OrElse OBJ Is DBNull.Value Then OBJ = Dflt
        Return OBJ
    End Function

    Public Sub FillTable(ByVal SQL As String,
                         ByVal ParamArray Args() As Object)
        DTable.Clear()
        Try
            NewCommand(SQL)
            AddParms(Args)
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DTable)
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
    End Sub

    Public Sub FillUpdateTable(ByVal SQL As String,
                               ByVal ParamArray Args() As Object)
        DTable.Clear()
        Try
            NewCommand(SQL)
            AddParms(Args)
            DataAdapter = New SqlDataAdapter(Command)
            CommandBuilder = New SqlCommandBuilder(DataAdapter)
            DataAdapter.UpdateCommand = CommandBuilder.GetUpdateCommand
            DataAdapter.InsertCommand = CommandBuilder.GetInsertCommand
            DataAdapter.DeleteCommand = CommandBuilder.GetDeleteCommand
            DataAdapter.Fill(DTable)
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
    End Sub

    Public Function GetTable(ByVal SQL As String,
                             ByVal ParamArray Args() As Object) As DataTable
        Dim DT As New DataTable
        Try
            NewCommand(SawUtil.Substitute(SQL))
            AddParms(Args)
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DT)
            Return DT
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
            Return DT
        End Try
    End Function

    Public Function Read() As Boolean
        If Not IsOpen Then Return False

        Try
            If Reader.Read() Then Return True
            Reader.Close()
            Return False
        Catch ex As Exception
            RaiseException("DB reader failed", ex)
        End Try
    End Function

    Public Function GetDataRow(ByVal SQL As String,
                               ByVal ParamArray Args() As Object) As DataRow
        Dim DT As New DataTable
        Try
            NewCommand(SawUtil.Substitute(SQL))
            AddParms(Args)
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DT)
            If DT.Rows.Count > 0 Then
                Return DT.Rows(0)
            End If
            Return Nothing
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
            Return Nothing
        End Try
    End Function

#End Region

#Region "Stored Procedure Calls"

    Public Sub ClearParameters()
        Me.Parameters.Clear()
    End Sub

    Public Sub AddParameter(ByVal name As String, _
                            ByVal value As Object, _
                            Optional ByVal direction As System.Data.ParameterDirection = ParameterDirection.Input)
        Dim p As New SqlParameter(name, value)
        p.Direction = direction
        Me.Parameters.Add(p)
    End Sub

    Public Sub AddParameter(ByVal name As String, _
                            ByVal value As Object, _
                            ByVal direction As System.Data.ParameterDirection, _
                            ByVal datatype As System.Data.DbType, _
                            ByVal length As Integer)
        Dim p As New SqlParameter(name, value)
        p.DbType = datatype
        p.Size = length
        p.Direction = direction
        Me.Parameters.Add(p)
    End Sub

    Public Function GetParameter(ByVal name As String) As Object
        Return Command.Parameters(name).Value
    End Function

    Public Sub CallNonQueryStoredProc(ByVal SQL As String)
        Try
            NewCommand(SQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As SqlParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            Command.ExecuteNonQuery()
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
    End Sub


    Public Function CallStoredProc(ByVal SQL As String) As Integer
        Try
            NewCommand(SQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As SqlParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            Reader = Command.ExecuteReader()
            Return Reader.RecordsAffected
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
        End Try
    End Function

    Public Function CallStoredProcDataTable(ByVal SQL As String) As DataTable

        DTable.Clear()
        Try
            NewCommand(SQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As DbParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DTable)
            Command.Connection.Close()
            Return DTable
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
            Return DTable
        End Try
    End Function

    Public Function CallStoredProcDataSet(ByVal SQL As String) As DataSet
        Dim lDataSet As New DataSet
        Try
            NewCommand(SQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As DbParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(lDataSet)
            '//close the connection after the table has been filled
            Command.Connection.Close()
            Return lDataSet
        Catch e As Exception
            RaiseException(e, SQL, Command.Parameters)
            Return lDataSet
        End Try
    End Function

#End Region

#Region "Dispose"
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
            End If

            Close()
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

#End Region


End Class
