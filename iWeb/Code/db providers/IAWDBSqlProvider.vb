Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data
Imports System.Data.Common
Imports System.Collections.Generic

Public Class IAWDBSqlProvider
    Inherits IAWDBProvider

    Protected Command As SqlCommand
    Public CommandBuilder As SqlCommandBuilder
    Private _dataAdapter As SqlDataAdapter


#Region "properties"

    Public Overrides Property ConnectionString() As String
        Get
            Return MyBase.ConnectionString
        End Get
        Set(ByVal value As String)
            MyBase.ConnectionString = value
        End Set
    End Property

    Public Overrides Property Connection() As IDbConnection
        Get
            If MyBase.Connection Is Nothing Then
                MyBase.Connection = New SqlConnection(ConnectionString)
            End If
            Return MyBase.Connection
        End Get
        Set(ByVal value As IDbConnection)
            MyBase.Connection = value
        End Set
    End Property

#End Region

    Public Sub New(ByVal UserName As String, _
               ByVal Password As String, _
               ByVal DBName As String, _
               ByVal DBServer As String, _
               ByVal ApplicationName As String, _
               ByVal ConnectionTimeOut As Integer, _
               ByVal MinPool As Integer, _
               ByVal MaxPool As Integer, _
               ByVal CommandTimeout As Integer)
        MyBase.New(Global.dbConnectionType.sqlclient, UserName, Password, DBName, DBServer, ApplicationName, ConnectionTimeOut, MinPool, MaxPool, CommandTimeout)
        Me.BuildConnectionString()
        IsOpen = False
    End Sub

    Protected Overrides Sub BuildConnectionString()
        Dim connBuilder As New SqlConnectionStringBuilder
        With connBuilder
            .DataSource = Me.DBServer
            .UserID = Me.UserName
            .Password = Me.Password
            .InitialCatalog = Me.DBName
            .ConnectTimeout = Me.ConnectionTimeOut
            .ApplicationName = Me.ApplicationName
            .MinPoolSize = Me.MinPool
            .MaxPoolSize = Me.MaxPool
        End With
        Me.ConnectionString = connBuilder.ToString
    End Sub

    Public Overrides Sub Open()
        If Me.IsOpen Then Return
        Me.Connection = New SqlConnection(Me.ConnectionString)
        For I As Integer = 1 To 10
            Try
                Me.Connection.Open()
                Me.IsOpen = True
                Exit For
            Catch e As Exception
                Me.IsOpen = False
                If I = 10 Then
                    Throw New Exception("Unable to conenct to the database")
                End If
            End Try
            System.Threading.Thread.Sleep(500)
        Next
    End Sub

    Private Sub NewCommand(ByVal SQL As String)
        If IsTran Then
            Command = New SqlCommand(SQL, Connection, Transaction)
        Else
            Command = New SqlCommand(SQL, Connection)
        End If
        Command.CommandTimeout = CommandTimeOut
    End Sub


    Public Overrides Function GetTable(ByVal aSQL As String, _
                                       ByVal aObj() As Object) As DataTable
        Dim DT As New DataTable
        aSQL = eval(aSQL, aObj)
        Try
            NewCommand(aSQL)
            AddParms(aObj)
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DT)
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
        End Try
        Return DT
    End Function

    Public Overrides Sub FillTable(ByVal aSQL As String, _
                                   ByVal aObj() As Object, _
                                   Optional ByVal aRequiresUpdate As Boolean = False)
        aSQL = eval(aSQL, aObj)
        If DTable Is Nothing Then
            DTable = New DataTable
        Else
            DTable.Clear()
        End If
        Try
            NewCommand(aSQL)
            AddParms(aObj)
            DataAdapter = New SqlDataAdapter(Command)
            If aRequiresUpdate Then
                CommandBuilder = New SqlCommandBuilder(DataAdapter)
                DataAdapter.UpdateCommand = CommandBuilder.GetUpdateCommand
                DataAdapter.InsertCommand = CommandBuilder.GetInsertCommand
                DataAdapter.DeleteCommand = CommandBuilder.GetDeleteCommand
            End If
            DataAdapter.Fill(DTable)
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
        End Try
    End Sub

    ' Execute non-query SQL
    ' Returns number of rows affected or -1 for error
    Overrides Function NonQuery(ByVal aSQL As String, _
                                ByVal aOBJ() As Object) As Long
        aSQL = eval(aSQL, aOBJ)
        Dim lCount As Long
        Try
            NewCommand(aSQL)
            AddParms(aOBJ)
            lCount = Command.ExecuteNonQuery()
        Catch o As SqlException
            If IsTran Then TranRollback()
            RaiseSqlException(o, aSQL, Command.Parameters)
            Return 0
        Catch e As Exception
            If IsTran Then TranRollback()
            RaiseException(e, aSQL, Command.Parameters)
            Return 0
        End Try
        Return lCount
    End Function

    Public Overrides Sub Query(ByVal aSQL As String, ByVal aOBJ() As Object)
        aSQL = eval(aSQL, aOBJ)
        Try
            NewCommand(aSQL)
            AddParms(aOBJ)
            Reader = Command.ExecuteReader
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
        End Try
    End Sub

    Public Overrides Function ExecuteStoredProc(ByVal aSQL As String) As Integer
        Try
            NewCommand(aSQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As DbParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            Reader = Command.ExecuteReader()
            Return Reader.RecordsAffected
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
        End Try
    End Function

    ''' <summary>
    ''' Returns a datatable based on the results of an executed stored procedure.
    ''' The connection is automatically closed once the datatable has been filled
    ''' </summary>
    ''' <param name="aSQL"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ExecuteStoredProcDataTable(ByVal aSQL As String) As DataTable
        If DTable Is Nothing Then
            DTable = New DataTable
        Else
            DTable.Clear()
        End If
        Try
            NewCommand(aSQL)
            Command.CommandType = CommandType.StoredProcedure
            For Each p As DbParameter In Me.Parameters
                Command.Parameters.Add(p)
            Next
            DataAdapter = New SqlDataAdapter(Command)
            DataAdapter.Fill(DTable)
            '//close the connection after the table has been filled
            Command.Connection.Close()
            Return DTable
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
            Return DTable
        End Try
    End Function
    ''' <summary>
    ''' Returns a dataset based on the results of an executed stored procedure.
    ''' This is to be used to return multiple result sets.
    ''' To use : dataset.tables(0)  dataset.tables(1)  etc
    ''' The connection is automatically closed once the datatable has been filled
    ''' </summary>
    ''' <param name="aSQL"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ExecuteStoredProcDataSet(ByVal aSQL As String) As DataSet
        Dim lDataSet As New DataSet
        Try
            NewCommand(aSQL)
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
            RaiseException(e, aSQL, Command.Parameters)
            Return lDataSet
        End Try
    End Function

    Overrides Function Scalar(ByVal aSQL As String, ByVal aOBJ() As Object) As Object
        aSQL = eval(aSQL, aOBJ)
        Dim lOBJ As Object = Nothing
        Try
            NewCommand(aSQL)
            AddParms(aOBJ)
            lOBJ = Command.ExecuteScalar
        Catch e As Exception
            RaiseException(e, aSQL, Command.Parameters)
        End Try
        Return lOBJ
    End Function

    ' Add parameters for ?? in sql 
    Public Overrides Sub AddParms(ByVal aOBJ() As Object)
        Dim lObj As Object
        Dim count As Integer = 1
        Dim param As String
        For Each lObj In aOBJ
            param = "@param" + count.ToString
            If lObj Is Nothing Then lObj = DBNull.Value
            Select Case lObj.GetType.ToString.ToLower
                Case "system.dbnull"
                    Command.Parameters.AddWithValue(param, System.DBNull.Value)
                Case "system.datetime", "system.data.sqltypes.sqldatetime"
                    Command.Parameters.Add(param, SqlDbType.DateTime).Value = lObj
                Case "system.string"
                    'trim the string first                   
                    Command.Parameters.Add(param, SqlDbType.VarChar).Value = DirectCast(lObj, String).Trim
                    'Command.Parameters.Add(param, SqlDbType.VarChar).Value = lObj
                    'Command.Parameters.Add("string", OleDbType.VarChar, CType(lObj, String).Length).Value = lObj
                Case "system.int32"
                    Command.Parameters.Add(param, SqlDbType.Int).Value = lObj
                Case "system.decimal", "system.data.sqltypes.sqldecimal"
                    Command.Parameters.Add(param, SqlDbType.Decimal).Value = lObj
                Case "system.byte[]"
                    Command.Parameters.Add(param, SqlDbType.Image).Value = lObj
            End Select

            count += 1
        Next
    End Sub

    Public Overrides Function AddParameter(ByVal name As String, ByVal value As Object) As DbParameter
        Return AddParameter(name, value, ParameterDirection.Input)
    End Function

    Public Overrides Function AddParameter(ByVal Name As String, ByVal datatype As DbType, ByVal direction As System.Data.ParameterDirection) As DbParameter
        Dim p As New SqlParameter(Name, datatype)
        p.Direction = direction
        Me.Parameters.Add(p)
        Return p
    End Function

    Public Overrides Function AddParameter(ByVal name As String, ByVal value As Object, ByVal direction As System.Data.ParameterDirection) As DbParameter
        Dim p As New SqlParameter(name, value)
        p.Direction = direction
        Me.Parameters.Add(p)
        Return p
    End Function

    Protected Overrides Sub RaiseException(ByVal message As String, ByVal e As Exception)
        If Me.Command IsNot Nothing Then
            Throw New IAWDBException(message, e, Me.Command.CommandText, Me.Command.Parameters)
        End If
        Throw New IAWDBException(message, e)
    End Sub
    Protected Overrides Sub RaiseException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Throw New IAWDBException(e, aSql, aParams)
    End Sub

    Protected Overrides Sub RaiseSqlException(ByVal e As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection)
        Dim EC As DBErrorCode = GetErrorCode(e)
        Throw New IAWDBException(e, aSql, aParams, EC.ErrorCode, EC.MessageRef)
    End Sub

End Class

