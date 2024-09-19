Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Data

Public Class WebMsgLog_DAL

    Public Shared Function GetLog() As List(Of Webmsglog)
        'lists do not support paging and sorting
        Dim Db As New SawDB
        Dim result As New List(Of WebMsgLog)
        Dim ls_sql As String
        Dim logItem As WebMsgLog

        ls_sql = "SELECT msg_id, " _
               + "       msg_type, " _
               + "       msg_time, " _
               + "       msg_text" _
               + "  FROM dbo.WEB_MSG_LOG WITH (NOLOCK) " _
               + "  ORDER BY msg_id DESC "
        Db.Query(ls_sql)

        While Db.Read
            logItem = New WebMsgLog
            With logItem
                .Id = SawUtil.Check(Db.Reader, "msg_id", False)
                .MessageDate = CType(SawUtil.Check(Db.Reader, "msg_time", False), Date)
                .Message = SawUtil.Check(Db.Reader, "msg_text", False)
                .Type = SawUtil.Check(Db.Reader, "msg_type", False)
            End With
            result.Add(logItem)
        End While

        Db.Close()

        Return result

    End Function

    Public Shared Function GetLogAsDataTable() As DataTable
        Dim Db As New SawDB
        Dim DT As New DataTable
        Dim ls_sql As String

        ls_sql = "SELECT msg_id as Id, " _
               + "       msg_type as Type, " _
               + "       msg_text as Message, " _
               + "       msg_time as messagedate" _
               + "  FROM dbo.WEB_msg_LOG WITH (NOLOCK) "
        ls_sql += "  ORDER BY msg_id DESC "

        DT = Db.GetTable(ls_sql)
        Db.Close()

        Return DT


    End Function

    ''' <summary>
    ''' Deletes any items with the same message as the current item
    ''' </summary>
    ''' <param name="id"></param>
    ''' <remarks></remarks>
    Public Shared Sub DeleteItems(ByVal id As Integer)
        Dim ls_sql As String
        ls_sql = "DELETE FROM l  " _
               + "  FROM web_msg_log l JOIN(SELECT msg_text, " _
               + "                                 msg_type " _
               + "                            FROM dbo.web_msg_log WITH (NOLOCK) " _
               + "                           WHERE msg_id = ??) AS l2 " _
               + "                       ON l.msg_text = l2.msg_text " _
               + "                      AND l.msg_type = l2.msg_type "
        SawUtil.SQLNonQuery(ls_sql, New Object() {id})
    End Sub

    Public Shared Sub DeleteItem(ByVal id As Integer)
        Dim ls_sql As String
        ls_sql = "DELETE FROM dbo.web_msg_log  " _
               + " WHERE msg_id =" + id.ToString
        SawUtil.SQLNonQuery(ls_sql)
    End Sub



End Class
