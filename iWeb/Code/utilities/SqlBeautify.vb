Public Class SqlBeautify

    Private errorStr As String
    Private is_sql As String
    Private is_token As New ArrayList
    Private is_type As New ArrayList
    Private ii_token As Integer

    Public Property ErrorText() As String
        Get
            Return errorStr
        End Get
        Set(ByVal Value As String)
            errorStr = Value
        End Set
    End Property

    Public Sub New()
        Init()
    End Sub

    Private Sub Init()
        is_sql = String.empty
        ErrorText = String.empty
        ii_token = -1
        is_token.Clear()
        is_type.Clear()
        ii_token = -1
    End Sub

    Public Function Beautify(ByVal as_sql As String) As String
        'Dim li_cnt As Integer
        'clear old tokens
        Init()

        is_sql = as_sql

        'build token list for new sql
        If Not BuildTokens() Then Return as_sql

        '	Finally Format the sql
        If Not Format() Then Return as_sql

        Return is_sql
    End Function

    Private Function Format() As Boolean
        Dim li_cnt As Integer
        Dim li_indent As New ArrayList
        Dim li_ind_sel As New ArrayList
        Dim li_ind As Integer = 0
        Dim li_char As Integer = 0
        Dim crlf As String = Chr(13) + Chr(10)
        Dim ls_indent As String = String.empty
        Dim lb_comma As New ArrayList
        Dim li_comma As Integer

        li_comma = 0
        lb_comma.Add(False)

        li_indent.Add(0)
        li_ind_sel.Add(0)

        is_sql = String.empty

        For li_cnt = 0 To ii_token
            'ls_indent = Fill(" ", li_indent(li_ind))
            ls_indent = New String(" ", li_indent(li_ind))
            If li_cnt > 0 Then
                'not 1st time in
                If is_type(li_cnt) = String.empty And is_type(li_cnt - 1) = String.empty Then
                    is_sql += " "
                    li_char += 1
                End If
            End If

            Select Case DirectCast(is_type(li_cnt), String)
                Case "FUNC"
                    Select Case is_token(li_cnt).ToString
                        Case "INSERT", "UPDATE", "DELETE"
                            is_sql += is_token(li_cnt)
                            li_char += 6
                        Case "SELECT"
                            If li_cnt > 0 Then
                                'not 1st time in,if not 1st select and preceded by ( then newline
                                If is_type(li_cnt - 1) <> "OPEN" Then is_sql += crlf
                            End If
                            is_sql += is_token(li_cnt)
                            li_char += 6
                            lb_comma(li_comma) = False
                        Case "UNION"
                            is_sql += crlf + is_token(li_cnt)
                            li_char += 5
                        Case Else
                            If is_sql <> "" Then is_sql += crlf
                            is_sql += ls_indent
                            If li_ind_sel(li_ind) = 0 Then
                                li_char = Len(ls_indent) + 6
                                is_sql += New String(" ", 6 - Len(is_token(li_cnt))) + is_token(li_cnt)
                            Else
                                li_char = Len(ls_indent) + Len(is_token(li_cnt))
                                is_sql += is_token(li_cnt)
                            End If
                    End Select
                    Select Case DirectCast(is_token(li_cnt), String)
                        Case "INSERT"
                            is_sql += " " + is_token(li_cnt + 1) + " " + is_token(li_cnt + 2) + crlf + "      "
                            li_cnt += 2
                            li_char = 6
                        Case "DELETE"
                            is_sql += " " + is_token(li_cnt + 1) + " " + is_token(li_cnt + 2)
                            li_cnt += 2
                        Case "UNION"
                            If is_token(li_cnt + 1) = "SELECT" Then
                                is_sql += crlf
                                li_char = 0
                            End If
                        Case Else
                            If is_type(li_cnt + 1) = "FUNC" Then
                                li_cnt += 1
                                is_sql += " " + is_token(li_cnt)
                                li_char += Len(is_token(li_cnt)) + 1
                            End If
                    End Select
                    is_sql += " "
                    li_char += 1
                Case "OPEN"
                    If li_char = 0 Then
                        li_char = 7
                        'is_sql += "       "
                        is_sql += New String(" ", 7)
                    End If
                    is_sql += "("
                    li_ind += 1

                    Try
                        li_indent(li_ind) = li_char + 1
                    Catch ex As Exception
                        li_indent.Add(li_char + 1)
                    End Try

                    li_char += 1
                    If is_token(li_cnt + 1) = "SELECT" Then
                        Try
                            li_ind_sel(li_ind) = 0
                        Catch ex As Exception
                            li_ind_sel.Add(0)
                        End Try
                    Else
                        Try
                            li_ind_sel(li_ind) = 1
                        Catch ex As Exception
                            li_ind_sel.Add(1)
                        End Try
                    End If
                    li_comma += 1
                    Try
                        lb_comma(li_comma) = True
                    Catch ex As Exception
                        lb_comma.Add(True)
                    End Try
                Case "CLOSE"
                    is_sql += ")"
                    li_ind -= 1
                    li_char += 1
                    lb_comma(li_comma) = False
                    li_comma -= 1
                    If li_cnt < ii_token Then
                        If is_type(li_cnt + 1) <> "" Then
                            is_sql += " "
                            li_char += 1
                        End If
                    End If
                Case "COMMA"
                    is_sql += ","
                    li_char += 1
                    If Not lb_comma(li_comma) Then
                        is_sql += crlf
                        is_sql += ls_indent
                        li_char = Len(ls_indent)
                        If is_type(li_cnt + 1) = String.Empty Then
                            'is_sql += "       "
                            is_sql += New String(" ", 7)
                            li_char += 7
                        End If
                    End If
                Case "EQU"
                    If is_type(li_cnt - 1) = String.Empty Then
                        is_sql += " "
                        li_char += 1
                    End If
                    is_sql += is_token(li_cnt) + " "
                    li_char += Len(is_token(li_cnt)) + 1
                Case Else
                    is_sql += is_token(li_cnt)
                    li_char += Len(is_token(li_cnt))
            End Select
        Next

        Return True

    End Function

    Private Function BuildTokens() As Boolean
        '// This function will take apart the string is_sql and build an array of all the elements that go to make it up.
        Dim li_cnt As Integer = 1
        Dim ls_char As String

        Do While li_cnt <= is_sql.Length
            ls_char = Mid(is_sql, li_cnt, 1)
            Select Case ls_char
                Case Chr(10), Chr(13), " ", Chr(9)
                    li_cnt += 1
                Case "A" To "Z", "a" To "z", "$", ":", "@"
                    li_cnt = BuildWordToken(li_cnt)
                Case "0" To "9"
                    li_cnt = BuildNumberToken(li_cnt)
                Case "'", ""
                    li_cnt = BuildDelimitedToken(li_cnt)
                Case "(", ")", "+", "-", "*", "/", "&", "^", ","
                    li_cnt = BuildSingleToken(li_cnt)
                Case "<", ">", "="
                    li_cnt = BuildEqualityToken(li_cnt)
                Case Else
                    ErrorText = "Don't know what to do with (" & Mid(is_sql, li_cnt, 1) & ")"
                    Return False
            End Select
        Loop
        Return True
    End Function

    Private Function BuildEqualityToken(ByVal ai_count As Integer) As Integer
        Dim ls_temp As String = String.Empty

        Do While True
            ls_temp += Mid(is_sql, ai_count, 1)
            ai_count += 1
            If ai_count > Len(is_sql) Then Exit Do
            Select Case Mid(is_sql, ai_count, 1)
                Case "<", "=", ">", "*"
                Case Else
                    Exit Do
            End Select
        Loop
        ii_token += 1
        is_token.Add(ls_temp)
        is_type.Add("EQU")

        Return ai_count

    End Function

    Private Function BuildSingleToken(ByVal ai_count As Integer) As Integer
        ii_token += 1

        is_token.Add(Mid(is_sql, ai_count, 1))

        Select Case Mid(is_sql, ai_count, 1)
            Case "("
                is_type.Add("OPEN")
            Case ")"
                is_type.Add("CLOSE")
            Case ","
                is_type.Add("COMMA")
            Case "+"
                is_type.Add("")
            Case "-"
                is_type.Add("")
            Case "*"
                If Mid(is_sql, ai_count + 1, 1) = "=" Then
                    is_token.Add("*=")
                    is_type.Add("")
                    ai_count += 1
                Else
                    is_type.Add("")
                End If
            Case "/"
                is_type.Add("")
            Case "^"
                is_type.Add("")
            Case Else
                is_type.Add("")
        End Select

        Return ai_count + 1
    End Function

    Private Function BuildDelimitedToken(ByVal ai_count As Integer) As Integer
        'current position in sql string is passed in, which is a delimiter, eg '+&/,-        '
        'keep going through the string one character at time and adding characters to temporary string
        'until you find a matching delimiter, build string using everything between the delimiters
        'exit loop add one to the token count, use the token count as the index in token array, add the temporary string(aka the token) 
        Dim ls_delim As String
        Dim ls_temp As String = String.Empty

        ls_delim = Mid(is_sql, ai_count, 1)

        Do While True
            ls_temp += Mid(is_sql, ai_count, 1)
            ai_count += 1
            If ai_count > Len(is_sql) Then Exit Do
            Select Case Mid(is_sql, ai_count, 1)
                Case ls_delim
                    ls_temp += ls_delim
                    ai_count += 1
                    Exit Do
                Case Chr(13), Chr(10)
            End Select
        Loop

        ii_token += 1
        is_token.Add(ls_temp)
        is_type.Add("")

        Return ai_count
    End Function

    Private Function BuildNumberToken(ByVal ai_count As Integer) As Integer
        'current position in sql string is passed in
        'keep going through the string one character at time and adding characters to temporary string
        'until you find a character that numeric
        'exit loop add one to the token count, use the token count as the index in token array, add the temporary string(aka the token)
        'use the token count as the index for is_type
        Dim ls_temp As String = String.Empty
        Do While True
            ls_temp += Mid(is_sql, ai_count, 1)
            ai_count += 1
            If ai_count > Len(is_sql) Then Exit Do
            Select Case Mid(is_sql, ai_count, 1)
                Case "0" To "9", "."
                Case Else
                    Exit Do
            End Select
        Loop

        ii_token += 1
        is_token.Add(ls_temp)
        is_type.Add("")

        Return ai_count
    End Function

    Private Function BuildWordToken(ByVal ai_count As Integer) As Integer
        'current position in sql string is passed in
        'keep going through the string one character at time and adding characters to temporary string
        'until you find a character that isn't in a word
        'exit loop add one to the token count, use the token count as the index in token array, add the temporary string(aka the token) 
        Dim ls_temp As String = String.Empty
        Do While True
            ls_temp += Mid(is_sql, ai_count, 1)
            ai_count += 1
            If ai_count > Len(is_sql) Then Exit Do
            Select Case Mid(is_sql, ai_count, 1)
                Case "A" To "Z", "a" To "z", "0" To "9", "$", "_", ".", "@", "[", "]"
                Case Else
                    Exit Do
            End Select
        Loop

        ii_token += 1
        is_token.Add(ls_temp)

        'take the token, compare, if found then add the type to is_type(), using the token count(ii_token) as the index
        Select Case ls_temp.ToUpper
            Case "SELECT", "UPDATE", "INSERT", "INTO", "DELETE", "FROM", "WHERE", "VALUES", "AND", "OR", "ORDER", "BY", "WHEN", "END", "IF ", "GROUP", "SET", "UNION", "HAVING", "JOIN", "ON"
                is_type.Add("FUNC")
                is_token(ii_token) = ls_temp.ToUpper
            Case "AS", "IS", "NOT", "LIKE", "IN", "ALL"
                is_type.Add("EQU")
                is_token(ii_token) = ls_temp.ToUpper
            Case "NULL", "EXISTS", "CASE", "THEN"
                is_type.Add("")
                is_token(ii_token) = ls_temp.ToUpper
            Case Else
                is_type.Add("")
        End Select

        Return ai_count

    End Function

End Class

