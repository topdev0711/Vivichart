Imports System.Data.SqlClient
Imports System.Data.sql
Imports System.Data.Common

Public Class DBConn
    Private sqlDBconn As SqlConnection
    Private sqlCmd As SqlCommand
    Private _Server As String
    Private _Database As String
    Private _MinPool As Integer
    Private _MaxPool As Integer
    Private _Timeout As Integer

    Private iConnectionStr As String = String.Empty

    Public Reader As IDataReader
    Public iErrorMessage As String = String.Empty

    Public Function TestConnection() As Boolean
        If Not Open() Then Return False
        Close()
        Return True
    End Function

    Public Sub New(ByVal Server As String, _
                   ByVal Database As String, _
                   ByVal MinPool As Integer, _
                   ByVal MaxPool As Integer, _
                   ByVal Timeout As Integer)
        _Server = Server
        _Database = Database
        _MinPool = MinPool
        _MaxPool = MaxPool
        _Timeout = Timeout
        iConnectionStr = GetConnStr()
    End Sub

    Public Function Open() As Boolean
        Try
            sqlDBconn = New SqlConnection(iConnectionStr)
            sqlDBconn.Open()
        Catch ex As Exception
            iErrorMessage = ex.Message.Replace("Login failed for user 'com'.", "")
            Return False
        End Try
        Return True
    End Function

    Public Function Close() As Boolean
        sqlDBconn.Close()
    End Function

    Public Sub Query(ByVal aSQL As String)
        Try
            sqlCmd = New SqlCommand(aSQL, sqlDBconn)
            Reader = sqlCmd.ExecuteReader
        Catch ex As Exception
            Throw New Exception("Error executing query - " + ex.Message)
        End Try
    End Sub


    Public Function Scalar(ByVal aSQL As String) As Object
        Dim RetOBJ As Object = Nothing
        Try
            sqlCmd = New SqlCommand(aSQL, sqlDBconn)
            RetOBJ = sqlCmd.ExecuteScalar()
        Catch ex As Exception
            Throw New Exception("Error executing scalar - " + ex.Message)
        End Try
        Return RetOBJ
    End Function

    Public Function GetValue(ByVal aColumnName As String, ByVal aDefaultValue As String) As Object
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

#Region "Connection String"

    Private Function GetConnStr() As String
        'Dim pwd As String = Encryption.Decrypt("2pt6L7C")
        Dim connBuilder As New SqlConnectionStringBuilder
        With connBuilder
            .DataSource = _Server
            .InitialCatalog = _Database
            .UserID = _Database + "_iweb"
            .Password = "gojsPwd23$"
            .MinPoolSize = _MinPool
            .MaxPoolSize = _MaxPool
            .ConnectTimeout = _Timeout
            .ApplicationName = "ViviChart Settings"
        End With
        Return connBuilder.ToString
    End Function
#End Region

End Class
