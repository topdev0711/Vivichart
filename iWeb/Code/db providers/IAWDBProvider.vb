Imports System.Reflection
Imports System.Data
Imports System.Data.Common
Imports System.Text.RegularExpressions
Imports System.Collections.Generic


Public MustInherit Class IAWDBProvider

#Region "Connection Properties"

    Private _connectionString As String

    Private _UserName As String
    Private _Password As String
    Private _DBName As String
    Private _DBServer As String
    Private _ApplicationName As String
    Private _ConnectionTimeOut As Integer
    Private _MinPool As Integer
    Private _MaxPool As Integer
    Private _CommandTimeout As Integer

    Protected Overridable Property UserName() As String
        Get
            Return Me._UserName
        End Get
        Set(ByVal value As String)
            Me._UserName = value
        End Set
    End Property
    Protected Overridable Property Password() As String
        Get
            Return Me._Password
        End Get
        Set(ByVal value As String)
            Me._Password = value
        End Set
    End Property
    Protected Overridable Property DBName() As String
        Get
            Return Me._DBName
        End Get
        Set(ByVal value As String)
            Me._DBName = value
        End Set
    End Property
    Protected Overridable Property DBServer() As String
        Get
            Return Me._DBServer
        End Get
        Set(ByVal value As String)
            Me._DBServer = value
        End Set
    End Property
    Protected Overridable Property ApplicationName() As String
        Get
            Return Me._ApplicationName
        End Get
        Set(ByVal value As String)
            Me._ApplicationName = value
        End Set
    End Property
    Protected Overridable Property ConnectionTimeOut() As Integer
        Get
            Return Me._ConnectionTimeOut
        End Get
        Set(ByVal value As Integer)
            Me._ConnectionTimeOut = value
        End Set
    End Property
    Public Overridable Property ConnectionString() As String
        Get
            Return Me._connectionString
        End Get
        Set(ByVal value As String)
            Me._connectionString = value
        End Set
    End Property
    Public Overridable Property MinPool() As Integer
        Get
            Return _MinPool
        End Get
        Set(ByVal value As Integer)
            _MinPool = value
        End Set
    End Property
    Public Overridable Property MaxPool() As Integer
        Get
            Return _MaxPool
        End Get
        Set(ByVal value As Integer)
            _MaxPool = value
        End Set
    End Property
    Protected Overridable Property CommandTimeOut() As Integer
        Get
            Return Me._CommandTimeout
        End Get
        Set(ByVal value As Integer)
            Me._CommandTimeout = value
        End Set
    End Property


#End Region
#Region "General Properties"
    Private _dbConnectionType As dbConnectionType
    Private _paramCounter As Integer
    Private _IsOpen As Boolean
    Private _IsTran As Boolean
    Private _Reader As IDataReader
    Private _Connection As IDbConnection
    Private _DataAdapter As DbDataAdapter
    Private _Parameters As List(Of IDbDataParameter)
    Private _DTable As DataTable
    Private _Transaction As IDbTransaction
    Private _IsException As Boolean = False

    Public Overridable Property IsOpen() As Boolean
        Get
            Return Me._IsOpen
        End Get
        Set(ByVal value As Boolean)
            Me._IsOpen = value
        End Set
    End Property
    Public Overridable Property IsTran() As Boolean
        Get
            Return Me._IsTran
        End Get
        Set(ByVal value As Boolean)
            Me._IsTran = value
        End Set
    End Property
    Public Overridable Property Reader() As IDataReader
        Get
            Return Me._Reader
        End Get
        Set(ByVal value As IDataReader)
            Me._Reader = value
        End Set
    End Property
    Public Overridable Property Connection() As IDbConnection
        Get
            Return Me._Connection
        End Get
        Set(ByVal value As IDbConnection)
            Me._Connection = value
        End Set
    End Property
    Public Overridable Property DataAdapter() As DbDataAdapter
        Get
            Return Me._DataAdapter
        End Get
        Set(ByVal value As DbDataAdapter)
            Me._DataAdapter = value
        End Set
    End Property
    Public Overridable Property Parameters() As List(Of IDbDataParameter)
        Get
            Return Me._Parameters
        End Get
        Set(ByVal value As List(Of IDbDataParameter))
            Me._Parameters = value
        End Set
    End Property
    Public Overridable Property DTable() As DataTable
        Get
            Return Me._DTable
        End Get
        Set(ByVal value As DataTable)
            Me._DTable = value
        End Set
    End Property
    Public Overridable Property Transaction() As IDbTransaction
        Get
            Return Me._Transaction
        End Get
        Set(ByVal value As IDbTransaction)
            Me._Transaction = value
        End Set
    End Property

    Public Property DBConnectionType() As dbConnectionType
        Get
            Return _dbConnectionType
        End Get
        Set(ByVal value As dbConnectionType)
            _dbConnectionType = value
        End Set
    End Property

    Public Property IsException() As Boolean
        Get
            Return _IsException
        End Get
        Set(ByVal value As Boolean)
            _IsException = value
        End Set
    End Property

    Public Overridable ReadOnly Property exportConnectionString() As String
        Get
            Return Regex.Replace(Me._connectionString, ";Password=\w+?;", ";Password=;")
        End Get
    End Property

#End Region

    Public Sub New(ByVal aConnectionType As dbConnectionType, _
                   ByVal UserName As String, _
                   ByVal Password As String, _
                   ByVal DBName As String, _
                   ByVal DBServer As String, _
                   ByVal ApplicationName As String, _
                   ByVal ConnectionTimeOut As Integer, _
                   ByVal MinPool As Integer, _
                   ByVal MaxPool As Integer, _
                   ByVal CommandTimeout As Integer)
        Me.DBConnectionType = aConnectionType
        Me.UserName = UserName
        Me.Password = Password
        Me.DBName = DBName
        Me.DBServer = DBServer
        Me.ApplicationName = ApplicationName
        Me.ConnectionTimeOut = ConnectionTimeOut
        Me.MinPool = MinPool
        Me.MaxPool = MaxPool
        Me.CommandTimeOut = CommandTimeout
        Me.Parameters = New List(Of IDbDataParameter)
    End Sub

    Protected MustOverride Sub BuildConnectionString()
    Public MustOverride Sub Open()

    Public Overridable Sub Close()
        Connection.Close()
        IsOpen = False
    End Sub
    Public Overridable Sub TranBegin()
        IsTran = True
        Transaction = Connection.BeginTransaction
    End Sub
    Public Overridable Sub TranRollback()
        If IsTran AndAlso Transaction IsNot Nothing Then
            Transaction.Rollback()
            Transaction = Nothing
            IsTran = False
        End If
    End Sub
    Public Overridable Function TranCommit() As Boolean
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
    Public Overridable Function Read() As Boolean
        Try
            If Reader.Read() Then Return True
            Reader.Close()
            Return False
        Catch ex As Exception
            RaiseException("DB reader failed", ex)
        End Try
    End Function
    Public Overridable Function IsNull(ByVal aCol As String) As Boolean
        Dim iOrdinal As Integer
        If Reader Is Nothing Then Return True
        Try
            iOrdinal = Reader.GetOrdinal(aCol)
        Catch e As Exception
            Return True
        End Try
        If Reader(aCol) Is System.DBNull.Value Then Return True
        If Reader(aCol) Is Nothing Then Return True
        Return False
    End Function
    Public Overridable Function GetValue(ByVal aColumnName As String, ByVal aDefaultValue As String) As Object
        If Reader Is Nothing Then Return aDefaultValue
        Try
            If Reader(aColumnName) Is System.DBNull.Value Then
                Return aDefaultValue
            ElseIf Reader(aColumnName) Is Nothing Then
                Return aDefaultValue
            Else
                Return Reader(aColumnName)
            End If
        Catch ex As Exception
            Return aDefaultValue
        End Try
    End Function

    Public MustOverride Function GetTable(ByVal aSQL As String, ByVal aObj() As Object) As DataTable
    Public MustOverride Sub FillTable(ByVal aSQL As String, ByVal aObj() As Object, Optional ByVal aRequiresUpdate As Boolean = False)
    Public MustOverride Function NonQuery(ByVal aSQL As String, ByVal aOBJ() As Object) As Long
    Public MustOverride Sub Query(ByVal aSQL As String, ByVal aOBJ() As Object)
    Public MustOverride Function Scalar(ByVal aSQL As String, ByVal aParams() As Object) As Object
    Public MustOverride Sub AddParms(ByVal aOBJ() As Object)


    '///functions to support the calling of stored procs
    Public MustOverride Function ExecuteStoredProc(ByVal aSQL As String) As Integer
    Public MustOverride Function ExecuteStoredProcDataTable(ByVal aSQL As String) As DataTable
    Public MustOverride Function ExecuteStoredProcDataSet(ByVal aSQL As String) As DataSet
    Public MustOverride Function AddParameter(ByVal name As String, ByVal value As Object) As DbParameter
    Public MustOverride Function AddParameter(ByVal name As String, ByVal value As Object, ByVal direction As ParameterDirection) As DbParameter
    Public MustOverride Function AddParameter(ByVal Name As String, ByVal datatype As DbType, ByVal direction As System.Data.ParameterDirection) As DbParameter


    Protected Overridable Function eval(ByVal aSql As String, ByVal aOBJ() As Object) As String

#If DEBUG Then
        AddDebugParms(aSql, aOBJ)
#End If

        Dim result As String = String.Empty
        Select Case _dbConnectionType
            Case Global.dbConnectionType.ole
                result = Regex.Replace(aSql, "\?\?", "?")
                'result = Regex.Replace(aSql, "\?\?", AddressOf ReplaceMarker)
            Case Global.dbConnectionType.sqlclient
                _paramCounter = 0
                result = Regex.Replace(aSql, "\?\?", AddressOf ReplaceMarker)
            Case Global.dbConnectionType.odbc
                Throw New Exception("db connection type of odbc not supported")
        End Select
        Return result
    End Function

    Public Overridable Sub ClearParameters()
        Me._Parameters = New List(Of IDbDataParameter)
    End Sub

    Protected Overridable Sub AddDebugParms(ByVal aSql As String, ByVal aOBJ() As Object)
        Dim lObj As Object
        Dim param As String
        Dim ls_sql As String = aSql
        Dim startPos As Integer

        For Each lObj In aOBJ
            param = " "
            If lObj Is Nothing Then lObj = DBNull.Value
            Select Case lObj.GetType.ToString.ToLower
                Case "system.dbnull"
                    param += "NULL"
                Case "system.datetime"
                    param += "'" + DirectCast(lObj, Date).ToString("yyyy-MM-dd HH:mm:ss") + "'"
                Case "system.string"
                    param += "'" + DirectCast(lObj, String) + "'"
                Case "system.int32"
                    param += DirectCast(lObj, Integer).ToString
                Case "system.decimal"
                    param += DirectCast(lObj, Decimal).ToString
                Case "system.byte[]"
                    param += "'" + DirectCast(lObj, Byte()).ToString + "'"
            End Select

            'find the first occurence of ?? and replace with the value
            startPos = ls_sql.IndexOf("??")
            If startPos <> -1 Then
                ls_sql = ls_sql.Remove(startPos, 2)
                ls_sql = ls_sql.Insert(startPos, param)
            End If

        Next
    End Sub

    Private Function ReplaceMarker(ByVal m As Match) As String
        ' Replace each Regex match with the number of the match occurrence.
        _paramCounter += 1
        Return "@param" + _paramCounter.ToString()
    End Function

    Protected Overridable Sub RaiseException(ByVal message As String, ByVal e As Exception)
        Throw New IAWDBException(message, e)
    End Sub
    Protected Overridable Sub RaiseException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Throw New IAWDBException(e, aSql, aParams)
    End Sub

    Protected Overridable Sub RaiseSqlException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Dim EC As DBErrorCode = GetErrorCode(e)
        Throw New IAWDBException(e, aSql, aParams, EC.ErrorCode, EC.MessageRef)
    End Sub

    Protected Overridable Function GetErrorCode(ByVal aExc As DbException) As DBErrorCode
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

        Return New DBErrorCode(errorCode, lErrNo)

    End Function

    Protected Class DBErrorCode
        Public ErrorCode As String
        Public MessageRef As String
        Public Sub New(ByVal code As String, ByVal ref As String)
            Me.ErrorCode = code
            Me.MessageRef = ref
        End Sub
    End Class

End Class



