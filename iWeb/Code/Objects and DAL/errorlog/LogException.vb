Imports System.Data.Common

Public Class LogException

    Private AutoEmail As Boolean
    Private EmailEnabled As Boolean

    Public Sub New()
        AutoEmail = ctx.siteConfig("email_errors").ToString() = "AutoEmail"
        EmailEnabled = ctx.siteConfig("email_errors").ToString() <> "NoEmail"
    End Sub

    Public Shared Sub RecordError(ByVal Message As String, ByVal ex As Exception)
        Dim LE As New LogException
        LE.logError(Message, ex)
    End Sub

    Public Shared Sub LogIAWException(ByVal ex As Exception)
        Dim LE As New LogException
        LE.AutoEmail = False
        LE.EmailEnabled = False
        LE.logError(ex, Nothing, Nothing, False, Nothing)
    End Sub

    Public Shared Sub HandleException(ByVal ex As Exception, ByVal redirect As Boolean, Optional ByVal redirectUrl As String = Nothing)
        Dim LE As New LogException
        LE.logError(ex, Nothing, Nothing, redirect, String.Empty, redirectUrl)
    End Sub

    Public Shared Sub HandleException(ByVal ex As Exception, ByVal aSql As String, ByVal aParams As DbParameterCollection, ByVal aStack As String, ByVal redirect As Boolean, Optional ByVal redirectUrl As String = Nothing)
        Dim LE As New LogException
        LE.logError(ex, aSql, aParams, redirect, aStack, redirectUrl)
    End Sub

    Private Sub logError(ByVal Message As String,
                         ByVal ex As Exception)
        Dim ls_sql As String
        Dim logDateTime As DateTime = DateTime.Now

        Dim target As String = String.Empty
        If ex.TargetSite IsNot Nothing Then target = ex.TargetSite.ToString

        Dim stack As String = String.Empty
        stack = buildStack(ex, "", True)

        Dim msg As String = LogException.buildMessage(ex, Message)

        Dim ts As String = Version.AssemblyVersion
        Dim evtId As Integer = SawUtil.GetKey("WEB_ERROR_LOG", "wel_eventId")

        Using DB As New IawDB

            Dim DBVersion As String = DB.Scalar("SELECT min(db_revision) FROM dbo.global_parm WITH (NOLOCK)")

            Dim params(12) As Object
            params(0) = evtId
            params(1) = ts + " - " + ex.Source
            params(2) = logDateTime
            params(3) = Left(msg, 1024)
            params(4) = ""
            params(5) = ""
            params(6) = Left(target, 1024)
            params(7) = stack
            params(8) = ""
            params(9) = ""
            params(10) = ""
            params(11) = ""
            params(12) = ""

            ls_sql = "INSERT INTO WEB_ERROR_LOG 
                             (wel_eventid,wel_source,wel_datetime,wel_message,wel_form,wel_querystring,wel_targetsite,wel_stacktrace,wel_referer,wel_user_ref,wel_ip_address,wel_sql,wel_sql_params)  
                      VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13)"
            Dim rowsAffected As Integer = DB.NonQuery(ls_sql, params)

        End Using

    End Sub

    Private Sub logError(ByVal ex As Exception, _
                         ByVal aSql As String, _
                         ByVal aParams As DbParameterCollection, _
                         ByVal redirect As Boolean, _
                         ByVal aStack As String, _
                         Optional ByVal redirectUrl As String = Nothing)

        Dim ls_sql As String
        Dim logDateTime As DateTime = DateTime.Now

        'return if this method has already been called in this request, to prevent recursion
        If Not ctx.item("preventRecurse") Is Nothing Then Return
        ctx.item("preventRecurse") = "true"

        If ctx.session("ERROR-SQL") IsNot Nothing AndAlso ctx.session("ERROR-SQL") <> "" Then
            aSql = ctx.session("ERROR-SQL")
            ctx.session("ERROR-SQL") = Nothing
            If ctx.session("ERROR-Params") IsNot Nothing Then
                aParams = CType(ctx.session("ERROR-Params"), DbParameterCollection)
                ctx.session("ERROR-Params") = Nothing
            End If
        End If


        Dim referer As String = String.Empty
        If Not ctx.current.Request.ServerVariables("HTTP_REFERER") Is Nothing Then
            referer = ctx.current.Request.ServerVariables("HTTP_REFERER").ToString()
        End If

        Dim sForm As String = String.Empty
        If Not ctx.current.Request.Form Is Nothing Then
            sForm = ctx.current.Request.Form.ToString()
        End If

        Dim sQuery As String = String.Empty
        If Not ctx.current.Request.QueryString Is Nothing Then
            If ctx.item("cmd") IsNot Nothing Then
                '///if cmd then decrypt the query string
                sQuery = SawUtil.decrypt(ctx.item("cmd"))
            Else
                sQuery = ctx.current.Request.QueryString.ToString
            End If
        End If

        Dim ipAdd As String = ctx.UserIPAddress
        If String.IsNullOrEmpty(ipAdd) Then ipAdd = String.Empty

        Dim sql As String = aSql
        Dim sqlParams As String = String.Empty
        If aParams IsNot Nothing AndAlso aParams.Count > 0 Then
            For Each param As DbParameter In aParams
                If param.Value Is Nothing Then Continue For
                sqlParams += param.ParameterName + "="
                '///format parameter according to type
                Select Case param.Value.GetType.ToString.ToLower
                    Case "system.dbnull"
                        sqlParams += "NULL"
                    Case "system.datetime"
                        sqlParams += "'" + Date.Parse(param.Value).ToString(ctx.DBDateFormat) + "'"
                    Case "system.string"
                        sqlParams += "'" + param.Value.ToString + "'"
                    Case "system.int32", "system.decimal"
                        sqlParams += param.Value.ToString
                    Case Else
                        sqlParams += "'" + param.Value.ToString + "'"
                End Select
                sqlParams += " ,"
            Next
            '//remove last comma
            If Not String.IsNullOrEmpty(sqlParams) Then
                sqlParams = sqlParams.Remove(sqlParams.Length - 1)
            End If
        End If

        Dim target As String = String.Empty
        If ex.TargetSite IsNot Nothing Then target = ex.TargetSite.ToString

        Dim stack As String = String.Empty
        stack = LogException.buildStack(ex, aStack, True)

        Dim message As String = String.Empty
        message = LogException.buildMessage(ex, message)

        Dim ts As String = Version.AssemblyVersion
        Dim evtId As Integer = SawUtil.GetKey("WEB_ERROR_LOG", "wel_eventId")

        Using DB As New IawDB

            Dim sUser As String = String.Empty
            sUser = ctx.user_ref

            Dim DBVersion As String = DB.Scalar("SELECT min(db_revision) FROM dbo.global_parm WITH (NOLOCK)")

            Dim params(12) As Object
            params(0) = evtId
            params(1) = ts + " - " + ex.Source + "; DB Version=" + DBVersion
            params(2) = logDateTime
            params(3) = Left(message, 1024)
            params(4) = sForm
            params(5) = sQuery
            params(6) = left(target, 1024)
            params(7) = stack
            params(8) = Left(referer, 1024)
            params(9) = sUser
            params(10) = ipAdd
            params(11) = sql
            params(12) = Left(sqlParams, 1024)

            ls_sql = "INSERT INTO WEB_ERROR_LOG 
                             (wel_eventid,wel_source,wel_datetime,wel_message,wel_form,wel_querystring,wel_targetsite,wel_stacktrace,wel_referer,wel_user_ref,wel_ip_address,wel_sql,wel_sql_params)  
                      VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13)"
            Dim rowsAffected As Integer = DB.NonQuery(ls_sql, params)

        End Using

        'email error ?
        If AutoEmail Then
            LogException.emailLogError(evtId)
        End If

        If redirect Then
            If Not String.IsNullOrEmpty(redirectUrl) Then ctx.redirectOnError(redirectUrl)
            ctx.redirectOnError()
        End If

    End Sub

    Private Shared Function buildMessage(ByVal e As Exception, ByVal message As String)
        If Not String.IsNullOrEmpty(message) Then
            message += " : "
        End If
        message += e.Message

        If e.InnerException IsNot Nothing Then
            message = LogException.buildMessage(e.InnerException, message)
        End If

        Return message
    End Function

    Private Shared Function buildStack(ByVal e As Exception, ByVal stack As String, ByVal TopLevel As Boolean) As String
        Dim NewData As String

        If Not String.IsNullOrEmpty(e.StackTrace) Then
            If e.StackTrace.StartsWith("  ") Then
                NewData = e.StackTrace.ToString.Remove(0, 6).Replace("   at ", "<br>").Replace(ctx.current.Request.PhysicalApplicationPath, "")
            Else
                NewData = SawUtil.StackTraceToString(New StackTrace(e), TopLevel)
            End If
            NewData = Regex.Replace(NewData, "\([^\)]*\)", "()")
            NewData = Regex.Replace(NewData, ":0\n", "\n")

            If Not String.IsNullOrEmpty(NewData) Then
                If Not String.IsNullOrEmpty(stack) Then
                    stack += "<hr />"
                End If
                stack += NewData
            End If
        End If

        If e.InnerException IsNot Nothing Then
            Return LogException.buildStack(e.InnerException, stack, False)
        End If

        Return stack
    End Function

    'Private Shared Function buildStack(ByVal e As Exception, ByVal stack As String) As String
    '    Dim LogText() As String
    '    Dim SplitLine() As String
    '    Dim FileLine() As String
    '    Dim strReturn As String = String.Empty
    '    Dim sTrace As StackTrace = New StackTrace(e)

    '    LogText = sTrace.GetFrame(3).ToString.Split(" ")
    '    SplitLine = LogText(6).Split("\")
    '    Dim i As Integer = SplitLine.GetUpperBound(0)
    '    FileLine = SplitLine(i).Split(":")
    '    If FileLine.Length = 1 And LogText.Length > 7 Then
    '        SplitLine = LogText(7).Split("\")
    '        i = SplitLine.GetUpperBound(0)
    '        FileLine = SplitLine(i).Split(":")
    '    End If
    '    If FileLine.Length > 1 Then
    '        strReturn += LogText(0) + "() " + FileLine(0) + ":" + FileLine(1) + "<br>"
    '    Else
    '        strReturn += LogText(0) + "() " + FileLine(0) + ":<br>"
    '    End If

    '    stack += strReturn

    '    If e.InnerException IsNot Nothing Then
    '        Return LogException.buildStack(e.InnerException, stack)
    '    End If

    '    Return stack
    'End Function

    Public Shared Function emailLogError(ByVal eventId As String) As Boolean
        Dim LE As New LogException
        Return LE.emailError(eventId, ctx.siteConfig("email_errors_to").ToString())
    End Function

    Public Shared Function emailLogError(ByVal eventId As String, ByVal toAddress As String) As Boolean
        Dim LE As New LogException
        Return LE.emailError(eventId, toAddress)
    End Function

    Private Function emailError(ByVal eventId As String, ByVal toAddress As String) As Boolean

        Dim emailAddresses As String = toAddress

        Dim returnVal As Boolean = True
        Dim strData As String = String.Empty

        If EmailEnabled Then
            Dim strEmails As String = emailAddresses

            If (strEmails.Length > 0 And Not eventId Is Nothing) Then
                Dim DB As New SawDB
                Dim ls_sql As String
                ls_sql = "SELECT licensee_name, " _
                    + "       client_number " _
                    + "  FROM LICENSEE WITH (NOLOCK)"

                DB.Query(ls_sql)
                DB.Read()
                strData = "<b>Client:</b><br />" + DB.Reader("licensee_name") + "(" + DB.Reader("client_number") + ")"
                DB.Reader.Close()

                ls_sql = "SELECT wel_eventid, " _
                    + "       wel_datetime, " _
                    + "       wel_source, " _
                    + "       wel_sql, " _
                    + "       wel_sql_params, " _
                    + "       wel_message, " _
                    + "       wel_form, " _
                    + "       wel_querystring, " _
                    + "       wel_targetsite, " _
                    + "       wel_stacktrace, " _
                    + "       wel_referer, " _
                    + "       wel_user_ref, " _
                    + "       wel_ip_address " _
                    + "  FROM WEB_ERROR_LOG WITH (NOLOCK)" _
                    + " WHERE wel_eventid = ?? "

                DB.Query(ls_sql, New Object() {eventId})

                If DB.Read Then
                    strData += "<br /><br /><b>Source:</b> <br />" + DB.Reader("wel_source") _
                              + "<br /><br /><b>LogDateTime:</b> <br />" + DB.Reader("wel_datetime") _
                              + "<br /><br /><b>Referer:</b> <br />" + DB.Reader("wel_referer") _
                              + "<br /><br /><b>User:</b> <br />" + DB.Reader("wel_user_ref") _
                              + "<br /><br /><b>IP address:</b> <br />" + DB.Reader("wel_ip_address") _
                              + "<br /><br /><b>Message:</b> <br />" + DB.Reader("wel_message")

                    If Not DB.Reader("wel_sql") Is System.DBNull.Value AndAlso _
                       Not String.IsNullOrEmpty(DB.Reader("wel_sql")) Then
                        strData += "<br /><br /><b>Sql:</b> <br />" + DB.Reader("wel_sql") _
                                 + "<br /><br /><b>Sql Params:</b> <br />" + DB.Reader("wel_sql_params")
                    End If

                    strData += "<br /><br /><b>Querystring:</b> <br />" + DB.Reader("wel_querystring") _
                            + "<br /><br /><b>Targetsite:</b> <br />" + DB.Reader("wel_targetsite") _
                            + "<br /><br /><b>Stacktrace:</b> <br />" + DB.Reader("wel_stacktrace")

                Else
                    returnVal = False
                End If

                DB.Close()
                DB = Nothing

                If Not returnVal Then Return returnVal
                Return mailer.SendEmail(emailAddresses, "Ingenuity Web - Application Error", strData)

            End If
        Else : returnVal = False
        End If
        Return returnVal
    End Function

    '*************************************************************
    'This requires the asp user to have write rights to the event log
    'NAME:          WriteToEventLog
    'PURPOSE:       Write to Event Log
    'PARAMETERS:    Entry - Value to Write
    '               AppName - Name of Client Application. Needed because before writing
    '                         writing to event log, you must have a named EventLog source. 
    '               EventType - Entry Type, from EventLogEntryType Structure e.g., EventLogEntryType.Warning, 
    '                                                                              EventLogEntryType.Error
    '               LogNam1e: Name of Log (System, Application; Security is read-only) If you 
    '                                      specify a non-existent log, the log will be created
    'RETURNS:       True if successful
    '*************************************************************
    Public Shared Function WriteToEventLog(ByVal entry As String, _
                                  Optional ByVal appName As String = "IngenWeb", _
                                  Optional ByVal eventType As EventLogEntryType = EventLogEntryType.Information, _
                                  Optional ByVal logName As String = "Application") As Boolean
        Dim objEventLog As New EventLog
        Try
            'Register the Application as an Event Source
            If Not EventLog.SourceExists(appName) Then
                EventLog.CreateEventSource(appName, logName)
            End If

            objEventLog.Source = appName
            objEventLog.WriteEntry(entry, eventType)
            Return True
        Catch Ex As Exception
            Return False
        End Try
    End Function
End Class
