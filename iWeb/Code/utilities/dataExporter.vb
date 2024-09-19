Imports System.Text.RegularExpressions
Imports System.Collections
Imports System.IO
Imports System.Data


Public Class dataExporter

    Private DB As SawDB
    Private DT As DataTable
    Private DC As DataColumn
    Private DR As DataRow
    Private _tableName As String
    Private _exportString As String
    Private _rows As ArrayList
    Private _columns As ArrayList
    Private HTTP As HttpContext
    Private _sql As String
    Public Property sql() As String
        Get
            Return _sql
        End Get
        Set(ByVal Value As String)
            _sql = Value
        End Set
    End Property


    Public Sub New(ByVal aHTTP As HttpContext)
        HTTP = aHTTP
    End Sub

    Public Function getExportSql(ByVal sql As String, ByVal tableName As String, Optional ByVal sqlParams() As Object = Nothing) As String
        populateDataTable(sql, tableName, sqlParams)
        buildExportString()
        Return _exportString
    End Function

    Public Sub copy(ByVal sql As String, ByVal tableName As String, ByVal substitutePkValues As Hashtable, Optional ByVal sqlParams() As Object = Nothing)
        populateDataTable(sql, tableName, sqlParams)
        'go through hash of pkValues
        Dim primaryKeys As IDictionaryEnumerator = substitutePkValues.GetEnumerator()
        While primaryKeys.MoveNext()
            Dim columnIndex As Integer = DT.Columns.IndexOf(primaryKeys.Key)
            If columnIndex >= 0 Then substituteValue("", primaryKeys.Value)
        End While
        buildExportString()
    End Sub

    Private Sub substituteValue(ByVal columnIndex As Integer, ByVal value As String)
        Dim rowEnumerator As IEnumerator
        rowEnumerator = _rows.GetEnumerator()
        While rowEnumerator.MoveNext()
            rowEnumerator.Current.item(columnIndex) = value
        End While
    End Sub

    Private Sub populateDataTable(ByVal sql As String, ByVal tableName As String, Optional ByVal sqlParams() As Object = Nothing)
        DB = New SawDB()
        DT = New DataTable
        _rows = New ArrayList
        _columns = New ArrayList
        _tableName = String.Empty
        _sql = sql
        _tableName = tableName
        _exportString = String.Empty

        If sqlParams Is Nothing Then
            DT = DB.GetTable(_sql)
        Else
            DT = DB.GetTable(_sql, sqlParams)
        End If

        DB.Close()
        DB = Nothing

        'build column list
        For Each DC In DT.Columns
            _columns.Add(DC.ColumnName)
            HTTP.Trace.Warn(DC.DataType.ToString)
        Next

        'add an arraylist of values for each row into _rows arraylist
        For Each DR In DT.Rows
            Dim rowValues As New ArrayList
            For Each DC In DT.Columns

                If DR(DC.ColumnName) Is System.DBNull.Value Then
                    rowValues.Add(System.DBNull.Value)
                Else
                    If DC.DataType.ToString = "System.String" Then
                        Dim s As String = DR(DC.ColumnName)
                        'SQLserver sp4 doesn't like the new line chars
                        's = Regex.Replace(s, "\n", "'+char(13)+''+char(10)+'")
                        's = Regex.Replace(s, "\r", "")
                        rowValues.Add(s)
                    Else
                        rowValues.Add(DR(DC.ColumnName))
                    End If
                End If
            Next
            _rows.Add(rowValues)
        Next
    End Sub

    Private Sub buildExportString()
        Dim columns As String = String.Empty
        Dim count As Integer = 0
        Dim values As String = String.Empty
        Dim rowEnumerator As IEnumerator
        Dim rowValuesEnumerator As IEnumerator

        'build list of columns
        rowEnumerator = _columns.GetEnumerator()
        columns += "("
        While rowEnumerator.MoveNext()
            If count <> 0 Then columns += ", "
            columns += rowEnumerator.Current
            count += 1
        End While
        columns += ")"

        'go through each row and put all the values for that rows into values and build the final export string
        rowEnumerator = _rows.GetEnumerator()
        While rowEnumerator.MoveNext()
            _exportString += "INSERT INTO " & _tableName & " " & columns & Environment.NewLine
            _exportString += "      VALUES "
            count = 0
            values = "("
            rowValuesEnumerator = rowEnumerator.Current.GetEnumerator()
            While rowValuesEnumerator.MoveNext()
                If count <> 0 Then values += ", "
                If rowValuesEnumerator.Current.Equals(System.DBNull.Value) Then
                    values += "NULL"
                Else
                    values += "'" & Regex.Replace(rowValuesEnumerator.Current, "'", "''") & "'"
                End If
                count += 1
            End While
            values += "); " + Environment.NewLine

            _exportString += values

        End While

    End Sub

End Class

