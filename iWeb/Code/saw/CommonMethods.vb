Module CommonMethods
    Private SQL As String

    Public Function BuildMessageGBR(message_ref As String, ParamArray PA() As Object) As String
        Dim CurLang As String = ctx.session("language")
        ctx.session("language") = "GBR"
        Dim Res As String = BuildMessage(message_ref, PA)
        ctx.session("language") = CurLang
        Return Res
    End Function

    Public Function BuildMessage(message_ref As String, ParamArray PA() As Object) As String
        Dim Str As String
        Dim Msg As String
        Msg = ctx.Translate("::" + message_ref)

        If String.IsNullOrEmpty(Msg) Then
            Return String.Format(ctx.Translate("::LT_S0220") + " {0}", message_ref) ' Missing message
        End If
        Str = String.Format(Msg, PA)

        Return Str
    End Function


    'Public Function BuildMessage(message_ref As String, ParamArray PA() As Object) As String
    '    Dim Str As String
    '    Dim Msg As String
    '    Using DB As New IawDB
    '        Sql = "Select top 1 message_wording
    '                 from dbo.qmessaget
    '                where message_ref = @p1
    '                  and language_ref In ('GBR',@p2)
    '                order by case when language_ref = @p2 then 1 else 2 end"
    '        Msg = DB.Scalar(SQL, message_ref, ctx.session("language"))
    '        If String.IsNullOrEmpty(Msg) Then
    '            Return String.Format("Missing message {0}", message_ref)
    '        End If
    '        Str = String.Format(Msg, PA)
    '    End Using

    '    Return Str
    'End Function

    ''' <summary>
    ''' Convert data to a string using either default or specified string
    ''' </summary>
    ''' <param name="data">data to convert</param>
    ''' <param name="fmt"></param>
    ''' <param name="str">REF target String</param>
    ''' <returns></returns>
    Public Function DataFormatter(data As Object, fmt As String, ByRef str As String) As Boolean
        Return DataFormatter(data, fmt, 0, str)
    End Function

    ''' <summary>
    ''' Convert data to a string using either default or specified string
    ''' </summary>
    ''' <param name="data">data to convert</param>
    ''' <param name="fmt">formatting string or empty</param>
    ''' <param name="len">for numerics, the number of decimals</param>
    ''' <param name="str">REF target String</param>
    ''' <returns></returns>
    Public Function DataFormatter(data As Object, fmt As String, len As Integer, ByRef str As String) As Boolean
        If data Is Nothing OrElse data Is DBNull.Value Then
            str = ""
            Return True
        End If

        Select Case True
            Case TypeOf data Is String
                Return FormatString(data, fmt, str)
            Case TypeOf data Is Integer Or TypeOf data Is Decimal
                Return FormatNumber(data, fmt, str)
            Case TypeOf data Is Date
                Return FormatDate(data, fmt, str)
            Case TypeOf data Is Boolean
                Return FormatBoolean(data, fmt, str)

        End Select

    End Function

    ''' <summary>
    ''' Format a string based on formatting rules
    ''' </summary>
    ''' <param name="str">The string to format</param>
    ''' <param name="fmt">The formatter</param>
    ''' <returns></returns>
    Private Function FormatString(data As String, fmt As String, ByRef str As String) As Boolean
        Dim n, l As Integer
        Dim s, c As String
        str = str.Trim
        fmt = fmt.Trim
        If fmt = "" Then
            str = data
            Return True
        End If
        If data.ToString = "" Then Return True

        str = data.ToString

        For Each F As String In fmt.ToLower.Split("|")
            Select Case True
                Case F = "upper"
                    str = str.ToUpper
                Case F = "lower"
                    str = str.ToLower
                Case F = "capitalise"
                    str = Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str)
                Case F.StartsWith("left")
                    If Not GetPart(F, "^", 2, n) Then Return False
                    str = Left(str, n)
                Case F.StartsWith("right")
                    If Not GetPart(F, "^", 2, n) Then Return False
                    str = Right(str, n)
                Case F.StartsWith("mid")
                    If Not GetPart(F, "^", 2, n) Then Return False
                    If F.Split("^").Length = 2 Then
                        str = Mid(str, n)
                    End If
                    If F.Split("^").Length = 3 Then
                        If Not GetPart(F, "^", 3, l) Then Return False
                        str = Mid(str, n, l)
                    End If
                Case F.StartsWith("word")
                    c = F.Split("^")(1)
                    If Not GetPart(F, "^", 3, n) Then Return False
                    str = str.Split(c)(n - 1)
                Case F.StartsWith("justify")
                    s = F.Split("^")(1)
                    If Not GetPart(F, "^", 3, n) Then Return False
                    If F.Split("^").Length = 4 Then
                        c = F.Split("^")(3)
                    Else
                        c = " "
                    End If
                    Select Case s
                        Case "left"
                            str = Left(str.Trim + New String(c, n), n)
                        Case "right"
                            str = Right(New String(c, n) + str.Trim, n)
                        Case "center"
                            l = n - str.Length / 2
                            str = Left(New String(c, l) + str.Trim + New String(c, n), n)
                    End Select
            End Select
        Next

        Return True
    End Function

    Private Function FormatNumber(data As Decimal, fmt As String, ByRef str As String) As Boolean
        Dim Group As Boolean = False
        Dim Currency As Boolean = False
        Dim Decs As Integer = 0

        If fmt = "" Then
            str = data.ToString()
            Return True
        End If

        For Each F As String In fmt.ToLower.Split("|")
            Select Case True
                Case F = "currency"
                    Currency = True
                Case F = "group"
                    Group = True
                Case F.StartsWith("decimal")
                    If Not GetPart(F, "^", 2, Decs) Then Return False
            End Select
        Next

        If Currency Then
            str = String.Format("{0:C}", data)
            Return True
        End If

        If Group Then
            str = String.Format("{0:N" + Decs.ToString() + "}", data)
        Else
            str = String.Format("{0:F" + Decs.ToString() + "}", data)
        End If

        Return True
    End Function

    Private Function FormatDate(data As Date, fmt As String, ByRef str As String) As Boolean
        If fmt = "" Then
            str = data.ToShortDateString
            Return True
        End If

        str = SawUtil.FormatDate(fmt, data)

        Return True
    End Function

    Private Function FormatBoolean(data As Boolean, fmt As String, ByRef str As String) As Boolean
        If fmt = "" Then
            str = If(data, "Yes", "No")
            Return True
        End If
        If fmt.Split("|").Length < 2 Then Return False

        str = If(data, fmt.Split("|")(0), fmt.Split("|")(1))
        Return True
    End Function


    ''' <summary>
    ''' extact number from delimited string
    ''' </summary>
    ''' <param name="str">The delimited string</param>
    ''' <param name="Delim">The delimiter</param>
    ''' <param name="N">The 'field' number to get. 1 based</param>
    ''' <param name="Res">The byref target integer</param>
    ''' <returns>True if number extracted</returns>
    Private Function GetPart(str As String, Delim As String, N As Integer, ByRef Res As Integer) As Boolean
        If str.Trim = "" Then Return False
        If N < 1 Then Return False
        If str.Split(Delim).Length < N Then Return False
        Dim S As String = str.Split(Delim)(N - 1)
        If Not Integer.TryParse(S, Res) Then Return False
        Return True
    End Function

End Module
