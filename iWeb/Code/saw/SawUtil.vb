Imports System.Data.Common
Imports System.Reflection
Imports System.Threading
Imports IAW.controls

Public Class SawUtil

    Public Shared Function encryptQuery(ByVal aString As String, _
                                        Optional ByVal isUrl As Boolean = False, _
                                        Optional ByVal isMenu As Boolean = False) As String
        Dim url As String = aString
        Dim delimiter As String = String.Empty

        If isUrl And url.Contains("?cmd=") Then Return url

        If (Not aString.Contains("&pr=")) And (Not aString.Contains("?pr=")) Then
            If url.Contains("?") And Not url.EndsWith("?") Then
                delimiter = "&"
            ElseIf url.Length > 1 Then
                delimiter = "?"
            End If
            url += delimiter + "pr=" + ctx.item(constants.processref)
        End If

        If Not isMenu Then
            If HttpContext.Current.Session IsNot Nothing Then
                If Not aString.ToLower.Contains("&usr=") And Not aString.ToLower.Contains("?usr=") Then
                    If url.Contains("?") And Not url.EndsWith("?") Then
                        delimiter = "&"
                    ElseIf url.Length > 1 Then
                        delimiter = "?"
                    End If
                    url += delimiter + "usr=" + ctx.user_ref
                End If
            End If
        End If

        If isUrl Then
            If url.Contains("?") And Not url.EndsWith("?") Then
                Dim qs As String = url.Substring(url.IndexOf("?") + 1)
                url = url.Remove(url.IndexOf("?"))
                url = url + "?cmd=" + encrypt(qs)
            End If
        Else
            Return "cmd=" + encrypt(url)
        End If

        Return url

    End Function

    Public Shared Function CreateLoginURL(Request As HttpRequest, ByVal parmString As String) As String
        Dim baseUrl As String = Request.Url.GetLeftPart(UriPartial.Authority) & Request.ApplicationPath
        Return baseUrl.TrimEnd("/"c) & "/secure/login.aspx?cmd=" + encrypt(parmString)
    End Function

    Public Shared Function encrypt(ByVal aString As String) As String
        Dim cryptographer As New Cryptography
        Return cryptographer.encrypt(aString)
    End Function

    Public Shared Function decrypt(ByVal aHexString As String) As String
        Dim cryptographer As New Cryptography
        Return cryptographer.decrypt(aHexString)
    End Function

    Public Shared Function getDBTime(ByVal adt As Date) As Date
        Return CDate("1900/01/01 " + Format(adt, "HH:mm:ss.fff"))
    End Function

    Public Shared Sub Log(ByVal as_text As String)
        IawDB.execNonQuery("INSERT INTO WEB_LOG (log_date, log_ip, log_user, log_text) VALUES (@p1,@p2,@p3,@p4)", Now, ctx.UserIPAddress, ctx.session("user_ref"), as_text)
    End Sub

    Public Shared Sub Log(ByVal as_text As String, ByVal as_user As String)
        IawDB.execNonQuery("INSERT INTO WEB_LOG (log_date, log_ip, log_user, log_text) VALUES (@p1,@p2,@p3,@p4)", Now, ctx.UserIPAddress, as_user, as_text)
    End Sub

    ' encrypt / decrypt strings
    Public Shared Function Encryption(ByVal as_string As String) As String
        If String.IsNullOrEmpty(as_string) Then Return String.Empty
        Return Cryptography.oldencryption(as_string)
    End Function

    'checks to see if column exists then calls subsitute to translate any $args$, 
    'returns "" if column <> exist
    Public Shared Function Check(ByVal ao_reader As DbDataReader, ByVal as_column As String, Optional ByVal doSubstitute As Boolean = True) As String
        Dim li_ordinal As Integer
        If ao_reader Is Nothing Then Return String.Empty

        Try
            li_ordinal = ao_reader.GetOrdinal(as_column)
        Catch e As Exception
            ctx.trace(e.Message)
            Return String.Empty
        End Try

        If ao_reader(as_column) Is Nothing Then Return String.Empty
        If ao_reader(as_column) Is System.DBNull.Value Then Return String.Empty

        If doSubstitute Then
            Return Substitute(ao_reader(as_column).ToString)
        Else
            Return ao_reader(as_column).ToString
        End If


    End Function

    'used by list screens to replace column name tokens with the actual column value
    Public Shared Function SubstituteValue(ByVal as_work As String, ByVal as_rowData As DataRowView) As String
        Dim ls_var As String
        Dim ls_work As String = as_work
        Dim li_pos, li_len As Integer

        Do While True
            li_pos = ls_work.IndexOf("$COLUMN_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 8, li_len - li_pos - 8).ToLower
            'uses ctx.item to get value off qs
            ls_work = ls_work.Replace("$COLUMN_" + ls_var + "$", "'" + as_rowData(ls_var).ToString + "'")
        Loop

        Return Substitute(ls_work)
    End Function

    'used by list screens to replace column name tokens with the actual column value
    Public Shared Function SubstituteValue(ByVal as_work As String, ByVal as_rowData As DataRow) As String
        Dim ls_var As String
        Dim ls_work As String = as_work
        Dim li_pos, li_len As Integer

        Do While True
            li_pos = ls_work.IndexOf("$COLUMN_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 8, li_len - li_pos - 8).ToLower
            'uses ctx.item to get value off qs
            ls_work = ls_work.Replace("$COLUMN_" + ls_var + "$", "'" + as_rowData(ls_var).ToString + "'")
        Loop

        Return Substitute(ls_work)
    End Function

    Public Shared Function SQLNonQuery(ByVal as_sql As String, ByVal aParams() As Object, Optional ByVal aLogException As Boolean = True) As Long
        Dim ll_result As Long
        Dim DB As New SawDB()
        ll_result = DB.NonQuery(as_sql, aParams, aLogException)
        DB.Close()
        DB = Nothing
        Return ll_result
    End Function

    Public Shared Function SQLNonQuery(ByVal as_sql As String, Optional ByVal aLogException As Boolean = True) As Long
        Dim ll_result As Long
        Dim DB As New SawDB()
        ll_result = DB.NonQuery(as_sql, aLogException)
        DB.Close()
        DB = Nothing
        Return ll_result
    End Function


    Public Shared Function LastDayOfMonth(ByVal aDate As Date) As Integer
        Return New Date(aDate.Year, aDate.Month, 1).AddMonths(1).AddDays(-1).Day
    End Function

    Public Shared Function Formatter(ByVal as_rowData As DataRowView, ByVal as_column As String, ByVal as_format As String) As String
        Dim ls_format As String = as_format
        Dim lb_null As Boolean = False
        If as_rowData(as_column) Is DBNull.Value Then lb_null = True

        ' getting format from a column ?
        If Left(as_format, 1) = "^" Then
            'ls_format = Mid(as_format, 2, 1) + as_rowData(Mid(as_format, 3))
            ls_format = as_rowData(Mid(as_format, 2))
        End If

        Select Case as_rowData(as_column).GetType.ToString
            Case "System.String" ' string
                Dim ls_data As String = as_rowData(as_column)
                Dim ls_result As String = String.Empty
                Dim li_cnt As Integer
                Dim li_pos As Integer = 1
                Dim ls_temp As String
                If lb_null Then Return String.Empty

                For li_cnt = 1 To ls_format.Length
                    ls_temp = Mid(ls_format, li_cnt, 1).ToLower

                    Select Case Mid(ls_format, li_cnt, 1).ToLower
                        Case "u"
                            ls_result += Mid(ls_data, li_pos, 1).ToUpper
                            li_pos += 1
                        Case "l"
                            ls_result += Mid(ls_data, li_pos, 1).ToLower
                            li_pos += 1
                        Case "x", "#"
                            ls_result += Mid(ls_data, li_pos, 1)
                            li_pos += 1
                        Case Else
                            ls_result += Mid(ls_format, li_cnt, 1)
                    End Select
                Next
                Return ls_result
            Case Else
                If lb_null Then
                    Return Format(0, ls_format)
                Else
                    Return Format(as_rowData(as_column), ls_format)
                End If
        End Select
        Return String.Empty
    End Function

    Public Shared Function GetPupText(ByVal TableID As String,
                                      ByVal ColumnID As String,
                                      ByVal AddBlankEntry As Boolean,
                                      Optional ByVal OrderByValue As Boolean = False) As DataTable
        Dim SQL As String = ""

        If AddBlankEntry Then
            SQL = "SELECT return_value = ''," +
                   "       pup_text = '-'," +
                   "       sort_group = NULL " +
                   " UNION ALL "
        End If

        SQL += "SELECT return_value," +
               "       pup_text," +
               "       sort_group " +
               "  FROM dbo.puptext " +
               " WHERE table_id = @p1 " +
               "   AND column_id = @p2 " +
               "   AND language_ref = 'GBR'"

        Dim DT As DataTable = IawDB.execGetTable(SQL, TableID, ColumnID)

        For Each DR As DataRow In DT.Rows
            If DR.GetValue("pup_text", "") = "-" Then Continue For
            If DR.GetValue("pup_text", "") = "" Then Continue For
            DR("pup_text") = ctx.Translate(DR("pup_text"))
        Next
        DT.AcceptChanges()

        Dim DV As New DataView(DT)
        If Not OrderByValue Then
            DV.Sort = "sort_group, pup_text"
        Else
            DV.Sort = "return_value"
        End If

        Return DV.ToTable()
    End Function

    Public Shared Function GetPupText(ByVal aTableid As String, ByVal aColumnid As String, ByVal aValue As String) As String
        Dim val As String = aValue
        Dim ls_sql As String
        'puptext to default to GBR if no puptext exists for the current language
        ls_sql = "Select pup_text 
                    FROM dbo.puptext WITH (NOLOCK) 
           		   WHERE table_id = @p1
           		     AND column_id = @p2
                     AND return_value = @p3
                     AND language_ref = 'GBR' "
        val = IawDB.execScalar(ls_sql, aTableid, aColumnid, aValue)
        val = ctx.Translate(val)

        Return val
    End Function

    ''' <summary>
    ''' Get language specific qmessage or fall back to GBR if missing.
    ''' </summary>
    ''' <param name="as_msg"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetMsg(ByVal as_msg As String) As String
        'Dim ls_sql As String
        Dim ls_result As String

        ls_result = ctx.Translate("::" + as_msg)

        If ls_result Is Nothing Then
            ls_result = ctx.Translate("::LT_S0220") + " " + as_msg + ". " + ctx.Translate("::LT_S0369") ' Missing Message & Please inform system administration 
        End If

        Return ls_result
    End Function

    Public Shared Function GetMsg(ByVal as_msg As String, ParamArray as_parm() As String) As String
        Dim msg As String = SawUtil.GetMsg(as_msg)
        Dim i As Integer = 1
        Dim p As Integer = 0
        For Each arg As String In as_parm
            p = msg.IndexOf("%s", p)
            If p = -1 Then Exit For
            msg = msg.Substring(0, p) & arg & msg.Substring(p + 2)
        Next
        Return msg
    End Function

    ''' <summary>
    ''' Returns FALSE if the message does not exist or is empty
    ''' </summary>
    ''' <param name="as_msg"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CheckMsg(ByVal as_msg As String) As Boolean
        Dim ls_sql As String
        ls_sql = "SELECT isnull(message_wording,'') " _
               + "  FROM QMESSAGET WITH (NOLOCK)" _
               + " WHERE message_ref = ?? " _
               + "   AND language_ref = ??"
        Return Not String.IsNullOrEmpty(SawDB.ExecScalar(ls_sql, _
                                                         New Object() {as_msg, ctx.languageCode}))
    End Function

    Public Shared Function GetMultiKey(ByVal as_table As String, _
                                       ByVal as_column As String, _
                                       ByVal ai_keycount As Integer) As String
        Dim DB As New SawDB
        Dim DT As DataTable
        Dim NewKey As String = String.Empty

        as_table = as_table.ToLower
        as_column = as_column.ToLower

        DB.ClearParameters()
        DB.AddParameter("@TableName", as_table)
        DB.AddParameter("@ColumnName", as_column)
        DB.AddParameter("@BlockSize", ai_keycount)
        DT = DB.ExecuteStoredProcDataTable("dbo.dbp_get_multi_key")

        NewKey = DT.Rows(0).Item("NewKey")

        DB.Close()
        DB = Nothing
        Return NewKey
    End Function

    ''' <summary>
    ''' Returns the next key value for the table.column
    ''' </summary>
    ''' <param name="as_table"></param>
    ''' <param name="as_column"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetKey(ByVal as_table As String, ByVal as_column As String) As String
        Return GetMultiKey(as_table, as_column, 1)
    End Function

    Public Shared Function Substitute(ByVal as_work As String) As String
        Dim ls_work As String = as_work
        Dim ls_arg As String
        Dim li_argcount As Integer

        ls_work = ls_work.Replace("$USER_REF$", GetSessVar("user_ref"))

        ls_work = ls_work.Replace("$TODAY$", Format(Today, ctx.session("DBDateFormat")))
        ls_work = ls_work.Replace("$USER_TODAY$", Format(Today, Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern))
        ls_work = ls_work.Replace("$SP_DATETIME$", Date.Now.ToString(ctx.DBDateFormat))
        ls_work = ls_work.Replace("$PATH$", GetSessVar("path"))
        ls_work = ls_work.Replace("$STYLE$", ctx.session("style"))
        ls_work = ls_work.Replace("$LANGUAGE$", GetSessVar("language"))
        ls_work = ls_work.Replace("$CLIENT$", GetSessVar("client"))


        Dim li_pos, li_len As Integer
        Dim ls_var As String
        Do While True
            li_pos = ls_work.IndexOf("$SESSION_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 9, li_len - li_pos - 9)
            ls_work = ls_work.Replace("$SESSION_" + ls_var + "$", GetSessVar(ls_var))
        Loop

        Do While True
            li_pos = ls_work.IndexOf("$QUERY_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 7, li_len - li_pos - 7)
            'uses ctx.item to get value off qs
            Dim val As String = "''"
            If Not String.IsNullOrEmpty(ctx.item(ls_var)) Then val = ctx.item(ls_var)
            ls_work = ls_work.Replace("$QUERY_" + ls_var + "$", val)
        Loop

        'substitute properties on the ctx object
        Do While True
            li_pos = ls_work.IndexOf("$CONTEXT_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 9, li_len - li_pos - 9)

            'use reflection to get required property value
            Dim prop As PropertyInfo
            Dim meth As MethodInfo
            Dim val As String = String.Empty

            prop = GetType(ctx).GetProperty(ls_var)
            If prop IsNot Nothing Then
                meth = prop.GetGetMethod
                val = meth.Invoke(Nothing, Nothing)
                ls_work = ls_work.Replace("$CONTEXT_" + ls_var + "$", val)
            Else
                'if the property is not found then set the value anyway, stops
                'the code looping and the error will be easy to find
                ls_work = ls_work.Replace("$CONTEXT_" + ls_var + "$", "CONTEXT_" + ls_var)
            End If
        Loop

        Dim ls_split() As String
        Do While True
            li_pos = ls_work.IndexOf("$GETKEY_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 8, li_len - li_pos - 8)
            ls_split = ls_var.Split(".")
            ls_work = ls_work.Replace("$GETKEY_" + ls_var + "$", GetKey(ls_split(0), ls_split(1)))
        Loop

        Do While True
            li_pos = ls_work.IndexOf("$GETKEYONINSERT_")
            If li_pos = -1 Then Exit Do
            li_len = ls_work.IndexOf("$", li_pos + 1)
            If li_len = -1 Then Exit Do
            ls_var = ls_work.Substring(li_pos + 16, li_len - li_pos - 16)
            ls_split = ls_var.Split(".")

            'only if insert
            If ctx.isFormInsert Then
                ls_work = ls_work.Replace("$GETKEYONINSERT_" + ls_var + "$", GetKey(ls_split(0), ls_split(1)))
            Else
                ls_work = ls_work.Replace("$GETKEYONINSERT_" + ls_var + "$", String.Empty)
            End If
        Loop

        For li_argcount = 1 To CInt(ctx.item("argcount"))
            ls_arg = ctx.item("ARG" + li_argcount.ToString)
            ls_work = ls_work.Replace("$ARG" + li_argcount.ToString + "$", ls_arg)
            ls_arg = HttpUtility.UrlEncode(ls_arg)
            ls_work = ls_work.Replace("$ENCODED_ARG" + li_argcount.ToString + "$", ls_arg)
        Next

        Return ls_work
    End Function

    Public Shared Function parseDouble(ByVal value As Object) As Double
        Dim d As Double
        If TypeOf value Is DBNull Then Return 0
        If Double.TryParse(value, d) Then
            Return d
        Else
            Return 0
        End If
    End Function

    Public Shared Function GetSessVar(ByVal as_name As String) As String
        If ctx.session(as_name) <> Nothing Then
            Return ctx.session(as_name)
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function GetErrMsg(ByVal aID As String) As String
        Dim lTxt As String
        lTxt = SawDB.ExecScalar("Select message_wording from qmessaget WITH (NOLOCK)" _
                        + " where language_ref = '" + ctx.languageCode + "'" _
                        + " and message_ref = '" + aID + "'")
        If lTxt Is Nothing Then
            'lTxt = "Message " + aID + " is missing - Please Advise System Administrator"
            lTxt = ctx.Translate("::LT_S0220") + " " + aID + ". " + ctx.Translate("::LT_S0369") ' Missing Message & Please inform system administration 
        End If
        Return lTxt
    End Function

    Public Shared Function IsIdentityColumn(ByVal table As String, ByVal column As String) As Boolean
        Return SawDB.ExecScalar("SELECT ISNULL(ColumnProperty(Object_id(??), ??, 'IsIdentity'),0)", New Object() {table, column}) = 1
    End Function

    Public Shared Function StackTraceToString(ByVal sTrace As StackTrace, ByVal TopLevel As Boolean) As String
        Dim LogText() As String
        Dim SplitLine() As String
        Dim FileLine() As String
        Dim strReturn As String = String.Empty
        Dim Concat As String
        Dim Frame As String

        For Ct As Integer = 0 To sTrace.FrameCount - 1
            Frame = sTrace.GetFrame(Ct).ToString
            Frame = Frame.Remove(Frame.IndexOf("(") + 1, Frame.IndexOf("(") - Frame.IndexOf(")"))

            LogText = Frame.Split(" ")
            ' concatenate all items at element 6+ into one line
            Concat = String.Empty
            For ct2 As Integer = 6 To LogText.GetUpperBound(0)
                If Concat = String.Empty Then
                    Concat = LogText(ct2)
                Else
                    Concat += " " + LogText(ct2)
                End If
            Next
            If TopLevel And Concat.StartsWith("<filename unknown>") Then Exit For

            SplitLine = Concat.Split("\")
            Dim top As Integer = SplitLine.GetUpperBound(0)
            FileLine = SplitLine(top).Split(":")
            If FileLine.Length = 1 And LogText.Length > 7 Then
                SplitLine = LogText(7).Split("\")
                top = SplitLine.GetUpperBound(0)
                FileLine = SplitLine(top).Split(":")
            End If
            If FileLine.Length > 1 Then
                strReturn += LogText(0) + "() " + FileLine(0) + ":" + FileLine(1) + "<br>"
            Else
                strReturn += LogText(0) + "() " + FileLine(0) + ":<br>"
            End If
        Next
        Return strReturn
    End Function

    Public Shared Function StandardDate(ByVal Dat As Date) As String
        If Dat.Ticks = 0 Then Return ""
        Return Dat.ToString("# MMM yyyy").Replace("#", IntToOrdinal(Dat.Day))
    End Function
    Public Shared Function FormatDate(Fmt As String, Dat As Date) As String
        If Dat.Ticks = 0 Then Return ""
        Return Dat.ToString(Fmt).Replace("#", IntToOrdinal(Dat.Day))
    End Function
    Public Shared Function FormatLongDate(Dat As Date) As String
        If Dat.Ticks = 0 Then Return ""
        Return Dat.ToString("dddd # MMMM, yyyy").Replace("#", IntToOrdinal(Dat.Day))
    End Function
    Public Shared Function FormatShortDate(Dat As Date) As String
        If Dat.Ticks = 0 Then Return ""
        Return Dat.ToString("dd/MM/yyyy")
    End Function

    ''' <summary>
    ''' Returns the ordinal for an integer, eg 1st, 2nd, 3rd
    ''' </summary>
    ''' <param name="num"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IntToOrdinal(ByVal num As Integer) As String
        Dim result As String = num.ToString()
        Select Case num Mod 100
            Case 11, 12, 13
                result += "th"
            Case Else
                Select Case num Mod 10
                    Case 1
                        result += "st"
                    Case 2
                        result += "nd"
                    Case 3
                        result += "rd"
                    Case Else
                        result += "th"
                End Select
        End Select
        Return result
    End Function

    ''' <summary>
    ''' Returns the date only part of a datetime
    ''' </summary>
    Public Shared Function DateOnly(ByVal D As Date) As Date
        Return New Date(D.Year, D.Month, D.Day)
    End Function

    Public Shared Function AddUserToURL(ByVal URL As String) As String
        Return AddArgToURL(URL, constants.userref, ctx.user_ref)
    End Function

    Public Shared Function AddArgToURL(ByVal URL As String, ByVal NewArg As String, ByVal NewVal As String) As String
        Dim qs As String = URL
        If URL.Contains("?cmd=") Then
            qs = SawUtil.decrypt(URL.Substring(URL.IndexOf("?cmd=") + 5))
            If qs.Contains(NewArg) Then Return URL
            qs = SawUtil.encrypt(qs + "&" + NewArg + "=" + NewVal)
            qs = URL.Substring(0, URL.IndexOf("?cmd=") + 5) + qs
        End If
        Return qs
    End Function

    Public Shared Sub LogMsg(ByVal type As String, ByVal txt As String)
        SawDB.ExecScalar("insert into dbo.web_msg_log (msg_type, msg_time, msg_text) " + _
                         "     values (??,getdate(),??)", New Object() {type, txt})
    End Sub

    Shared Function RandomString(ByVal intMinLength As Integer, ByVal intMaxLength As Integer, ByVal bIncludeDigits As Boolean) As String
        Static r As New Random

        ' Allowed characters variable
        Dim s As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                          "abcdefghijklmnopqrstuvwxyz"
        If bIncludeDigits Then
            s &= "0123456789"
        End If

        Dim chactersInString As Integer = r.Next(intMinLength, intMaxLength)

        Dim sb As New StringBuilder
        For i As Integer = 1 To chactersInString
            sb.Append(s.Substring(r.Next(0, s.Length), 1))
        Next

        Return sb.ToString()
    End Function

    Shared Function MaskEmail(ByVal s As String) As String
        Dim Pattern As String = "(?<=.).(?=[^@]*?.@)|(?:(?<=@.)|(?!^)\G(?=[^@]*$)).(?=.*\.)"
        If s.Trim.Length = 0 Then Return "Error, empty value"
        If Not s.Contains("@") Then
            ' probably dealing with a phone number, so just return the last 3 digits
            If s.Length < 3 Then Return s
            Return Strings.Right(s, 3)
        End If
        Return Regex.Replace(s, Pattern, "*")
    End Function

    Public Shared Sub CheckClasses(ByRef ctrl As IAWHyperLink)
        ctrl.CssClass = CheckClasses(ctrl.Text, ctrl.CssClass)
    End Sub
    Public Shared Sub CheckClasses(ByRef ctrl As IAWHyperLinkButton)
        ctrl.CssClass = CheckClasses(ctrl.Text, ctrl.CssClass)
    End Sub

    Private Shared Function CheckClasses(txt As String, clas As String) As String

        'Dim NewClass As String = ""
        'If String.IsNullOrEmpty(clas) Then clas = ""

        'If txt <> "" Then
        '    Select Case txt.ToLower
        '        Case "add"
        '            NewClass = "BtnAdd"
        '        Case "edit"
        '            NewClass = "BtnEdit"
        '        Case "view"
        '            NewClass = "BtnView"
        '        Case "delete"
        '            NewClass = "BtnDelete"
        '        Case "save", "update"
        '            NewClass = "BtnSave"
        '        Case "next", "select"
        '            NewClass = "BtnNext"
        '        Case "previous", "back"
        '            NewClass = "BtnBack"
        '        Case "ok", "yes"
        '            NewClass = "BtnConfirm"
        '        Case "close", "cancel", "no"
        '            NewClass = "BtnCancel"
        '        Case "withdraw"
        '            NewClass = "BtnWithdraw"
        '        Case Else
        '            'If clas <> "" Then NewClass = "BtnNext"
        '    End Select

        '    If NewClass <> "" Then clas += " IconBtn Icon16 " + NewClass

        'End If
        Return clas
    End Function

End Class

