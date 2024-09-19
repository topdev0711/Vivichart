Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common

Public Class WebErrorLog_DAL

    Public Shared Function GetLog() As List(Of WebErrorLog)
        'lists do not support paging and sorting
        Dim Db As New SawDB
        Dim result As New List(Of WebErrorLog)
        Dim ls_sql As String
        Dim logItem As WebErrorLog

        ls_sql = "SELECT wel_eventid, " _
               + "       wel_datetime, " _
               + "       wel_source, " _
               + "       wel_message, " _
               + "       wel_form, " _
               + "       wel_querystring, " _
               + "       wel_targetsite, " _
               + "       wel_stacktrace, " _
               + "       wel_referer, " _
               + "       wel_user_ref, " _
               + "       wel_sql, " _
               + "       wel_sql_params, " _
               + "       wel_ip_address " _
               + "  FROM dbo.WEB_ERROR_LOG WITH (NOLOCK)" _
               + "  ORDER BY wel_eventid DESC "
        Db.Query(ls_sql)

        While Db.Read
            logItem = New WebErrorLog
            With logItem
                .Id = SawUtil.Check(Db.reader, "wel_eventid", False)
                .LogDate = CType(SawUtil.Check(Db.reader, "wel_datetime", False), Date)
                .Message = SawUtil.Check(Db.reader, "wel_message", False)
                .Source = SawUtil.Check(Db.reader, "wel_source", False)
                .UserRef = SawUtil.Check(Db.reader, "wel_user_ref", False)
            End With
            result.Add(logItem)
        End While

        Db.Close()

        Return result

    End Function

    Public Shared Function GetLogEntries(ByVal eventId As Integer() ) As List(Of WebErrorLog)
        'lists do not support paging and sorting
        Dim Db As New SawDB
        Dim result As New List(Of WebErrorLog)
        Dim ls_sql As String
        Dim logItem As WebErrorLog
        Dim ids As String=""

        For Each i As Integer In eventId
            If Not String.IsNullOrEmpty(ids) Then
                ids += ","
            End If
            ids += i.ToString
        Next

        ls_sql = "SELECT wel_eventid, " _
               + "       wel_datetime, " _
               + "       wel_source, " _
               + "       wel_message, " _
               + "       wel_form, " _
               + "       wel_querystring, " _
               + "       wel_targetsite, " _
               + "       wel_stacktrace, " _
               + "       wel_referer, " _
               + "       wel_user_ref, " _
               + "       wel_sql, " _
               + "       wel_sql_params, " _
               + "       wel_ip_address " _
               + "  FROM dbo.WEB_ERROR_LOG WITH (NOLOCK) " _
               + " WHERE wel_eventid IN (" + ids + ")" _
               + "  ORDER BY wel_eventid DESC "
        Db.Query(ls_sql)

        While Db.Read
            logItem = New WebErrorLog
            With logItem
                .Id = SawUtil.Check(Db.Reader, "wel_eventid", False)
                .LogDate = CType(SawUtil.Check(Db.Reader, "wel_datetime", False), Date)
                .Message = SawUtil.Check(Db.Reader, "wel_message", False)
                .Source = SawUtil.Check(Db.Reader, "wel_source", False)
                .UserRef = SawUtil.Check(Db.Reader, "wel_user_ref", False)
                .QueryString = SawUtil.Check(Db.Reader, "wel_querystring", False)
                .Form = SawUtil.Check(Db.Reader, "wel_form", False)
                .Referer = SawUtil.Check(Db.Reader, "wel_referer", False)
                .TargetSite = SawUtil.Check(Db.Reader, "wel_targetsite", False)
                .StackTrace = SawUtil.Check(Db.Reader, "wel_stacktrace", False)
                .Sql = SawUtil.Check(Db.Reader, "wel_sql", False)
                .SqlParams = SawUtil.Check(Db.Reader, "wel_sql_params", False)
            End With
            result.Add(logItem)
        End While

        Db.Close()

        Return result

    End Function

    Public Shared Function GetLogAsDataTable() As DataTable
        Dim Db As New SawDB
        Dim DT As New DataTable
        Dim DR As DataRow
        Dim ls_sql As String

        'initialise the datatable
        DT.Columns.Add("Id", GetType(Integer))
        DT.Columns.Add("LogDate", GetType(Date))
        DT.Columns.Add("Message", GetType(String))
        DT.Columns.Add("UserRef", GetType(String))
        DT.Columns.Add("IpAddress", GetType(String))
        DT.Columns.Add("Sql", GetType(String))
        DT.Columns.Add("SqlParams", GetType(String))

        ls_sql = "SELECT wel_eventid, " _
               + "       wel_datetime, " _
               + "       wel_source, " _
               + "       wel_message, " _
               + "       wel_form, " _
               + "       wel_querystring, " _
               + "       wel_targetsite, " _
               + "       wel_stacktrace, " _
               + "       wel_referer, " _
               + "       wel_user_ref, " _
               + "       wel_sql, " _
               + "       wel_sql_params, " _
               + "       wel_ip_address " _
               + "  FROM dbo.WEB_ERROR_LOG WITH (NOLOCK) " _
               + "  ORDER BY wel_eventid DESC "
        Db.Query(ls_sql)

        While Db.Read
            DR = DT.NewRow
            DR("Id") = SawUtil.Check(Db.reader, "wel_eventid", False)
            DR("LogDate") = CType(SawUtil.Check(Db.reader, "wel_datetime", False), Date)
            DR("Message") = SawUtil.Check(Db.reader, "wel_message", False)
            DR("UserRef") = SawUtil.Check(Db.reader, "wel_user_ref", False)
            DR("IpAddress") = SawUtil.Check(Db.reader, "wel_ip_address", False)
            DR("Sql") = SawUtil.Check(Db.reader, "wel_sql", False)
            DR("SqlParams") = SawUtil.Check(Db.reader, "wel_sql_params", False)
            DT.Rows.Add(DR)
        End While

        Db.Close()

        Return DT

    End Function

    Public Shared Function GetLogPage(ByVal pageIndex As Integer, _
                                      ByVal pageSize As Integer, _
                                      ByRef totalRecords As Integer) As DataTable
        Dim DT As New DataTable
        Dim ReturnValue As DbParameter
        Dim DB As New SawDB
        ReturnValue = DB.AddParameter("@TotalRecords", totalRecords, ParameterDirection.ReturnValue)
        ReturnValue.Value = Nothing
        DB.AddParameter("@PageIndex", pageIndex)
        DB.AddParameter("@PageSize", pageSize)
        DT = DB.ExecuteStoredProcDataTable("dbo.dbp_GetErrorLog")

        If ((Not ReturnValue.Value Is Nothing) AndAlso TypeOf ReturnValue.Value Is Integer) Then
            totalRecords = CInt(ReturnValue.Value)
        End If

        DB.Close()
        DB = Nothing
        Return DT
    End Function

    Public Shared Sub DeleteItem(ByVal wel_eventid As Integer)
        Dim ls_sql As String
        ls_sql = "DELETE FROM web_error_log  " _
               + " WHERE wel_eventid = " + wel_eventid.ToString
        SawUtil.SQLNonQuery(ls_sql)
    End Sub

    Public Shared Sub DeleteSimilar(ByVal wel_eventid As Integer)
        Dim ls_sql As String
        ls_sql = "DELETE FROM E2  " _
               + " FROM web_error_log E1 JOIN web_error_log E2 " _
               + "                         ON E2.wel_message = E1.wel_message " _
               + "                        AND E2.wel_source = E1.wel_source " _
               + "WHERE E1.wel_eventid = " + wel_eventid.ToString
        SawUtil.SQLNonQuery(ls_sql)
    End Sub

End Class
