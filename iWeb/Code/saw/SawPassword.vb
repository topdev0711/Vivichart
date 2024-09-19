Imports System.Security.Cryptography

Public Class SawPassword

    Public Shared ReadOnly Property DefaultIterations() As Integer
        Get
            Return IawDB.execScalar("select def_psc_hash_cnt FROM dbo.global_sysadm")
        End Get
    End Property

    Public Shared ReadOnly Property GlobalSelfServDR() As DataRow
        Get
            Dim DR As DataRow = HttpContext.Current.Items("GlobalSelfServDR")
            If DR Is Nothing Then
                DR = IawDB.execGetTable("select web_use_sec_qu, " + _
                                        "       web_login_attempts," + _
                                        "       web_login_timeout, " + _
                                        "       web_recover_pwd, " + _
                                        "       lock_escalation" + _
                                        "  from dbo.global_self_serv").Rows(0)
            End If
            Return DR
        End Get
    End Property

    Public Shared ReadOnly Property WebLoginAttempts() As Integer
        Get
            Return GlobalSelfServDR("web_login_attempts")
        End Get
    End Property
    Public Shared ReadOnly Property WebLoginTimeout() As Integer
        Get
            Return GlobalSelfServDR("web_login_timeout")
        End Get
    End Property
    Public Shared ReadOnly Property PasswordRecoveryEnabled() As Boolean
        Get
            Return GlobalSelfServDR("web_recover_pwd") <> "00"
        End Get
    End Property
    Public Shared ReadOnly Property LockEscalation() As String
        Get
            Return GlobalSelfServDR("lock_escalation")
        End Get
    End Property

#Region "Password"
    ''' <summary>
    ''' Generates and emails the user a new password.  
    ''' Will also set the status on suser.psc_status to '03' meaning the user has to change password at next login
    ''' </summary>

    Public Shared Function EmailNewPassword(ByVal userName As String) As SawPasswordWorked
        'check that the user has an email, if not put out an error message
        Dim emailAddress As String = String.Empty
        Dim body As String
        Dim pwd As String
        Dim userRef As String
        Dim DR As DataRow

        If String.IsNullOrEmpty(userName) Then
            Return New SawPasswordWorked(False, ctx.Translate("::LT_S0343")) ' Please enter a username
        End If

        Using DB As New IawDB

            DR = DB.GetDataRow("SELECT isnull(user_ref,'') as user_ref,
                                       email_address 
                                  FROM dbo.suser WITH (NOLOCK) 
                                 WHERE user_name = @p1", userName)
            userRef = DR("user_ref")
            emailAddress = DR("email_address")

            If String.IsNullOrEmpty(emailAddress) Then
                Return New SawPasswordWorked(False, SawUtil.GetErrMsg("WB_B0005"))
            End If

            'generate new password
            pwd = SawPassword.GeneratePassword()

            Try
                'update psc_status to force change of password on login
                DB.Scalar("UPDATE dbo.suser " + _
                          "   SET login_fail_count = 0," + _
                          "       login_fail_date = NULL, " + _
                          "       psc_status = '03' " + _
                          " WHERE user_ref=@p1", userRef)

                SawPassword.HashGenerate(userRef, pwd)

                SawUtil.Log("New password generated", userRef)

            Catch ex As Exception
                Throw ex
            End Try

        End Using

        'If SawUtil.CheckMsg("LT_A0006") Then
        '    body = String.Format(SawUtil.GetMsg("LT_A0006"), pwd)
        'Else
        '    body = "Not in use"
        'End If

        'If body.StartsWith("Missing Message") Or body.StartsWith("Not in use") Then

        '    Select Case mailer.EmailMethod
        '        Case mailer.MailMethod.Web_Server
        '        body = "<html><body>" + String.Format("Your new Password is {0}", pwd) + _
        '                          "<br /><br />Your password must be changed at the next login." + _
        '                         "<br /><br />This is an automatically generated email, please do not reply." + _
        '                        "</body></html>"

        body = String.Format("::LT_S0344", pwd)

        'Case mailer.MailMethod.Background_Server
        '    body = String.Format("Your new Password is {0}", pwd) + Environment.NewLine + Environment.NewLine + _
        '           "Your password must be changed at the next login." + Environment.NewLine + Environment.NewLine + _
        '           "This is an automatically generated email, please do not reply."
        '    End Select
        'Else
        '    body = String.Format(SawUtil.GetMsg("LT_A0006"), pwd)
        'End If

        'send the email
        If Not mailer.SendEmail(emailAddress, ctx.siteConfig("browser_title"), body.ToString) Then
            'sending email failed
            'log error or ignore??
            Return New SawPasswordWorked(False, SawUtil.GetErrMsg("WB_B0006"))
        End If

        'worked
        Return New SawPasswordWorked(True, SawUtil.GetErrMsg("WB_B0007"))
    End Function

    ''' <summary>
    ''' Returns a randomly generated password
    ''' </summary>
    Public Shared Function GeneratePassword() As String

        Dim ls_pwd As String = ""

        Dim ls_u_alphas As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim ls_l_alphas As String = "abcdefghijklmnopqrstuvwxyz"
        Dim ls_numbers As String = "0123456789"
        Dim ls_other_chars, ls_allchars, ls_combined As String
        Dim ls_gen_uchar As String = ""
        Dim ls_gen_lchar As String = ""
        Dim ls_gen_num As String = ""
        Dim ls_gen_other As String = ""
        Dim li_loop, li_to, li_rand, li_gen_min_len, li_ucnt, li_lcnt, li_ncnt, li_ocnt As Integer
        Dim li_min_total, li_min_upper, li_min_lower, li_min_numeric, li_min_other As Integer
        Dim li_max_total, li_max_upper, li_max_lower, li_max_numeric, li_max_other As Integer

        Dim LimitStr As String
        Dim LimitArr() As String

        LimitStr = IawDB.execScalar("select psc_charset_limits from dbo.global_sysadm")
        LimitArr = LimitStr.Split(",")

        li_min_total = CInt(LimitArr(0))
        li_max_total = CInt(LimitArr(1))
        li_min_upper = CInt(LimitArr(2))
        li_max_upper = CInt(LimitArr(3))
        li_min_lower = CInt(LimitArr(4))
        li_max_lower = CInt(LimitArr(5))
        li_min_numeric = CInt(LimitArr(6))
        li_max_numeric = CInt(LimitArr(7))
        li_min_other = CInt(LimitArr(8))
        li_max_other = CInt(LimitArr(9))

        If li_min_total < (li_min_upper + li_min_lower + li_min_numeric + li_min_other) Then
            li_min_total = (li_min_upper + li_min_lower + li_min_numeric + li_min_other)
        End If

        If li_max_total > (li_max_upper + li_max_lower + li_max_numeric + li_max_other) Then
            li_max_total = (li_max_upper + li_max_lower + li_max_numeric + li_max_other)
        End If

        ls_other_chars = IawDB.execScalar("select IsNull(psc_misc_chars,'') from dbo.global_sysadm")

        li_ucnt = li_min_upper
        li_lcnt = li_min_lower
        li_ncnt = li_min_numeric
        li_ocnt = li_min_other

        li_gen_min_len = IawDB.execScalar("select IsNull(psc_gen_len,10) from dbo.global_sysadm")

        ' ok, first we generate a min number of chars for each type

        '// Ucase Alphas
        For li_loop = 1 To li_min_upper
            ls_gen_uchar += Mid(ls_u_alphas, CInt((26 * Rnd()) + 1), 1)
        Next

        ' Lcase Alphas
        For li_loop = 1 To li_min_lower
            ls_gen_lchar += Mid(ls_l_alphas, CInt((26 * Rnd()) + 1), 1)
        Next

        ' Numbers
        For li_loop = 1 To li_min_numeric
            ls_gen_num += Mid(ls_numbers, CInt((10 * Rnd()) + 1), 1)
        Next

        ' Special 
        For li_loop = 1 To li_min_other
            ls_gen_other += Mid(ls_other_chars, CInt((Len(ls_other_chars) * Rnd()) + 1), 1)
        Next

        ls_combined = ls_gen_uchar + ls_gen_lchar + ls_gen_num + ls_gen_other

        ' Ensure that the string is at least the minimum length
        ' and that no one char type exceeds its maximum length

        If li_ucnt = li_max_upper Then ls_u_alphas = ""
        If li_lcnt = li_max_lower Then ls_l_alphas = ""
        If li_ncnt = li_max_numeric Then ls_numbers = ""
        If li_ocnt = li_max_other Then ls_other_chars = ""

        Randomize(0)
        Dim R As New Random

        If Len(ls_combined) < li_gen_min_len Then
            li_to = li_gen_min_len - Len(ls_combined)
            For li_loop = 1 To li_to
                ls_allchars = ls_u_alphas + ls_l_alphas + ls_numbers + ls_other_chars

                li_rand = R.Next(0, Len(ls_allchars))
                ls_combined += ls_allchars.Substring(li_rand, 1)

                Select Case Asc(ls_allchars.Substring(li_rand, 1))
                    Case 65 To 90
                        li_ucnt += 1
                        If li_ucnt = li_max_upper Then ls_u_alphas = ""
                    Case 97 To 122
                        li_lcnt += 1
                        If li_lcnt = li_max_lower Then ls_l_alphas = ""
                    Case 48 To 57
                        li_ncnt += 1
                        If li_ncnt = li_max_numeric Then ls_numbers = ""
                    Case Else
                        li_ocnt += 1
                        If li_ocnt = li_max_other Then ls_other_chars = ""
                End Select

            Next
        End If

        ' Now randomise the order of the string
        li_to = Len(ls_combined)
        For li_loop = 1 To li_to
            li_rand = R.Next(0, Len(ls_combined))
            ls_pwd += ls_combined.Substring(li_rand, 1)
            ls_combined = ls_combined.Remove(li_rand, 1)
        Next

        Return ls_pwd

    End Function

    ''' <summary>
    ''' Returns a list of the password format requirements
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function PasswordFormat() As List(Of String)
        'psc_charset_limits  -  8,20,5,14,1,3,1,2,1,4
        'password length  8-20
        '            a-z  5-14
        '            A-Z  1-3
        '            0-9  1-2
        '         others  1-4
        '

        Dim sql As String
        Dim values As String()
        Dim miscChars As String
        Dim barReuse As Boolean = False
        Dim result As New List(Of String)
        Dim Usermsg As String

        Using DB As New IawDB
            sql = " SELECT IsNull(psc_charset_limits,'') as psc_charset_limits, " _
                + "        IsNull(lower(psc_flags),'') as psc_flags, " _
                + "        IsNull(psc_misc_chars,'') as psc_misc_chars " _
                + "   FROM dbo.GLOBAL_SYSADM WITH (NOLOCK)"
            DB.Query(sql)
            DB.Read()
            values = DB.Reader("psc_charset_limits").ToString.Split(",")
            barReuse = DB.Reader("psc_flags") = "r"
            miscChars = DB.Reader("psc_misc_chars")
            DB.Reader.Close()
        End Using

        AddMsg(result, values(0), values(1), "::LT_S0345", "::LT_S0346") ' "characters in length", "be"
        AddMsg(result, values(2), values(3), "::LT_S0347", "::LT_S0348") ' "lowercase letters", "contain"
        AddMsg(result, values(4), values(5), "::LT_S0349", "::LT_S0348") ' "uppercase letters", "contain"
        AddMsg(result, values(6), values(7), "::LT_S0350", "::LT_S0348") ' "digits", "contain"

        If miscChars <> "" Then
            AddMiscMsg(result, values(8), values(9), "::LT_S0351", miscChars, "::LT_S0348") ' "of the following characters ", miscChars, "contain"
        End If

        If barReuse Then
            AddMsg(result, 0, 0, "::LT_S0352", "::LT_S0348") ' "Previous passwords may not be used", "contain"
        End If

        Usermsg = SawUtil.GetMsg("WB_A0022").Trim
        If Not String.IsNullOrEmpty(Usermsg) Then
            AddMsg(result, 0, 0, Usermsg, "::LT_S0348") ' "contain"
        End If

        Return result
    End Function

    Private Shared Sub AddMsg(ByRef msgList As List(Of String),
                              ByVal minlen As String,
                              ByVal maxlen As String,
                              ByVal text As String,
                              ByVal prefix As String)
        Dim min As Boolean = minlen <> "0"
        Dim max As Boolean = maxlen <> "0"
        Dim FormatStr As String = ""

        If minlen = maxlen Then max = False

        ' If incoming prefix contains a literal then translate it.
        If Left(prefix, 2) = "::" Then
            prefix = ctx.Translate(prefix)
        End If

        ' If incoming text contains a literal then translate it.
        If Left(text, 2) = "::" Then
            text = ctx.Translate(text)
        End If

        Select Case True
            Case min And max
                FormatStr = String.Format(ctx.Translate("::LT_S0353"), prefix, text) ' "Must {0} between {{0}} and {{1}} {1}"
                msgList.Add(String.Format(FormatStr, minlen, maxlen))
            Case min
                FormatStr = String.Format(ctx.Translate("::LT_S0354"), prefix, text) ' "Must {0} at least {{0}} {1}"
                msgList.Add(String.Format(FormatStr, minlen))
            Case max
                FormatStr = String.Format(ctx.Translate("::LT_S0355"), prefix, text) ' "Can {0} up to {{0}} {1}"
                msgList.Add(String.Format(FormatStr, maxlen))
            Case Else
                msgList.Add(text)
        End Select
    End Sub

    Private Shared Sub AddMiscMsg(ByRef msgList As List(Of String),
                                  ByVal minlen As String,
                                  ByVal maxlen As String,
                                  ByVal text As String,
                                  ByVal chars As String,
                                  ByVal prefix As String)
        Dim min As Boolean = minlen <> "0"
        Dim max As Boolean = maxlen <> "0"
        Dim FormatStr As String = ""

        If minlen = maxlen Then max = False

        ' If incoming prefix contains a literal then translate it.
        If Left(prefix, 2) = "::" Then
            prefix = ctx.Translate(prefix)
        End If

        ' If incoming text contains a literal then translate it.
        If Left(text, 2) = "::" Then
            text = ctx.Translate(text)
        End If

        Select Case True
            Case min And max
                FormatStr = String.Format(ctx.Translate("::LT_S0353"), prefix, text) ' "Must {0} between {{0}} and {{1}} {1}"
                msgList.Add(String.Format(FormatStr, minlen, maxlen))
            Case min
                FormatStr = String.Format(ctx.Translate("::LT_S0354"), prefix, text) ' "Must {0} at least {{0}} {1}"
                msgList.Add(String.Format(FormatStr, minlen))
            Case max
                FormatStr = String.Format(ctx.Translate("::LT_S0355"), prefix, text) ' "Can {0} up to {{0}} {1}"
                msgList.Add(String.Format(FormatStr, maxlen))
            Case Else
                msgList.Add(text)
        End Select
    End Sub

    ''' <summary>
    ''' Validates and changes the users password
    ''' </summary>
    ''' <param name="userRef"></param>
    ''' <param name="oldPwd"></param>
    ''' <param name="newPwd"></param>
    ''' <param name="newPwdConfirm"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ChangePassword(ByVal userRef As String, _
                                          ByVal oldPwd As String, _
                                          ByVal newPwd As String, _
                                          ByVal newPwdConfirm As String) As SawPasswordWorked
        Dim crypt_pw_orig As String = String.Empty

        oldPwd = oldPwd.Trim
        newPwd = newPwd.Trim
        newPwdConfirm = newPwdConfirm.Trim

        If String.IsNullOrEmpty(oldPwd) _
        Or String.IsNullOrEmpty(newPwd) _
        Or String.IsNullOrEmpty(newPwdConfirm) Then
            Return New SawPasswordWorked(False, ctx.Translate("::LT_S0356")) ' "You must enter all three fields"
        End If

        'check old password is correct
        If Not SawPassword.HashCheck(userRef, oldPwd) Then
            Return New SawPasswordWorked(False, ctx.Translate("::LT_S0357")) ' "Incorrect Password"
        End If

        If oldPwd = newPwd Then
            Return New SawPasswordWorked(False, ctx.Translate("::LT_S0358")) ' "New Password must be different"
        End If

        Return ChangePassword(userRef, newPwd, newPwdConfirm)
    End Function

    ''' <summary>
    '''  Validates and changes the users password
    ''' </summary>
    ''' <param name="userRef"></param>
    ''' <param name="newPwd"></param>
    ''' <param name="newPwdConfirm"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ChangePassword(ByVal userRef As String, _
                                          ByVal newPwd As String, _
                                          ByVal newPwdConfirm As String) As SawPasswordWorked
        If newPwd <> newPwdConfirm Then
            Return New SawPasswordWorked(False, ctx.Translate("::LT_S0359")) '  "Password confirmation does not match"
        End If

        Return ChangePassword(userRef, newPwd)
    End Function

    Public Shared Function ChangePassword(ByVal userRef As String, _
                                          ByVal newPwd As String) As SawPasswordWorked
        Dim SQL As String
        'Dim crypt_pw_new As String = String.Empty

        'encrypt the new password
        'crypt_pw_new = SawUtil.Encryption(newPwd)

        'get the password format defintion
        'psc_flags           - null or R, R = bar use of earlier password
        'psc_charset_limits  - comma separated list of min/max pairs in the following order of meaning
        '                    - password length, lowercase chars, uppercase chars, numeric, other
        'psc_misc_chars      - other characters to check for
        'psc_reminder_days   - 
        'psc_expiry_days     -

        'integers to hold upper and lower limits
        Dim len_lim_lw, len_lim_up, lwr_lim_lw, lwr_lim_up, upr_lim_lw, upr_lim_up, num_lim_lw, num_lim_up, other_lim_lw, other_lim_up As Integer
        Dim psc_flags As String = String.Empty
        Dim psc_misc_chars As String = String.Empty
        Dim psc_count As Integer = 0

        Using DB As New IawDB

            SQL = "SELECT psc_expiry_days, " _
               + "        psc_reminder_days, " _
               + "        psc_flags, " _
               + "        psc_misc_chars, " _
               + "        psc_charset_limits " _
               + "   FROM dbo.GLOBAL_SYSADM WITH (NOLOCK)"
            DB.Query(SQL)
            If DB.Read() Then
                Dim minMax() As String = DB.Reader("psc_charset_limits").ToString.Split(","c)
                'rebuild min/max values as string, to use in regex
                len_lim_lw = minMax(0)
                len_lim_up = minMax(1)
                lwr_lim_lw = minMax(2)
                lwr_lim_up = minMax(3)
                upr_lim_lw = minMax(4)
                upr_lim_up = minMax(5)
                num_lim_lw = minMax(6)
                num_lim_up = minMax(7)
                other_lim_lw = minMax(8)
                other_lim_up = minMax(9)
                psc_flags = DB.GetValue("psc_flags", String.Empty)
                psc_misc_chars = DB.GetValue("psc_misc_chars", String.Empty)
                DB.Reader.Close()
            End If

            ' do we have enough of each letter or number types
            If Not isValidText(lwr_lim_lw, lwr_lim_up, newPwd, "[a-z]", psc_count) _
            Or Not isValidText(upr_lim_lw, upr_lim_up, newPwd, "[A-Z]", psc_count) _
            Or Not isValidText(num_lim_lw, num_lim_up, newPwd, "[0-9]", psc_count) Then
                Return New SawPasswordWorked(False, ctx.Translate("::LT_S0360")) '"Password is not in a valid format")
            End If

            'custom test, only if there is a char(s)
            If Not String.IsNullOrEmpty(psc_misc_chars) Then
                'insert a \ before each character
                psc_misc_chars = Regex.Replace(psc_misc_chars, "(.)", "\$1").Replace("\_", "_")

                If Not isValidText(other_lim_lw, other_lim_up, newPwd, "[" + psc_misc_chars + "]", psc_count) Then
                    Return New SawPasswordWorked(False, ctx.Translate("::LT_S0360")) '"Password is not in a valid format")
                End If
            End If

            'check to ensure no other invalid characters have been enetered
            If Regex.Matches(newPwd, "[^a-zA-Z0-9" + psc_misc_chars + "]").Count > 0 Then
                Return New SawPasswordWorked(False, ctx.Translate("::LT_S0360")) '"Password is not in a valid format")
            End If

            'check pwd length
            If Not (psc_count >= len_lim_lw And psc_count <= len_lim_up) Then
                Return New SawPasswordWorked(False, String.Format(ctx.Translate("::LT_S0361"), len_lim_lw, len_lim_up)) ' "Password must be between {0} and {1} characters."
            End If

            'check re-use of pervious password
            If psc_flags.ToLower = "r" Then
                If SawPassword.CheckHashReuse(userRef, newPwd) Then
                    Return New SawPasswordWorked(False, ctx.Translate("::LT_S0362")) ' You have used this password before, your new Password must be different.
                End If
            End If

            ' check password blacklist and user defined validation rules for passwords
            Select Case DB.Scalar("SELECT dbo.dbf_password_validation(@p1,@p2)", userRef, newPwd)
                Case -1
                    Return New SawPasswordWorked(False, ctx.Translate("::LT_S0363")) ' That password is on the banned list
                Case -2
                    Return New SawPasswordWorked(False, ctx.Translate("::LT_S0364")) ' The password does not pass validation
            End Select

            Try
                SawPassword.HashGenerate(userRef, newPwd)

                'update psc_status
                SQL = "UPDATE dbo.suser SET psc_status='01' WHERE user_ref=@p1 "
                DB.NonQuery(SQL, userRef)

                SawUtil.Log("Password changed by user", userRef)

            Catch ex As Exception
                Return New SawPasswordWorked(False, ctx.Translate("::LT_S0365")) ' There has been a problem adding the new password, please contact techical support
            End Try

        End Using

        Return New SawPasswordWorked()
    End Function

    Private Shared Function isValidText(ByVal min As Integer, _
                                    ByVal max As Integer, _
                                    ByVal text As String, _
                                    ByVal pattern As String, _
                                    ByRef count As Integer) As Boolean
        Dim isValid As Boolean
        If max = 0 Then max = 99
        Dim result As MatchCollection
        result = Regex.Matches(text, pattern)
        If result.Count >= min And result.Count <= max Then
            isValid = True
            count += result.Count
        End If
        Return isValid
    End Function

    ''' <summary>
    ''' Return the redirect url for forcing password change
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ChangePasswordUrl() As String
        Return SawUtil.encryptQuery("~/change_pwd.aspx?mustchangepwd=y&menubar=no&pr=change_pwd", True)
    End Function

    Public Shared Function ValidatePassword(ByVal userRef As String, _
                                            ByVal password As String) As SawAuth.Validate
        Dim isValidated As Boolean = True

        If String.IsNullOrEmpty(userRef) _
        Or String.IsNullOrEmpty(password) Then
            'some credentials missing
            Return SawAuth.Validate.failed
        End If

        If IsAccountLockedOut(userRef) Then
            'user is locked out because their account is locked (psc_status = '02')
            Return SawAuth.Validate.locked_out
        End If

        'do not allow the login attempt to continue if the user if locked out
        If IsLockedOut(userRef) Then
            'user is locked out by previous failed attempts
            If LockEscalation <> "01" Then
                PasswordFailed(userRef)
            End If
            Return SawAuth.Validate.locked_out
        End If

        ' Now see if the password is correct
        isValidated = SawPassword.HashCheck(userRef, password)

        If Not isValidated Then
            PasswordFailed(userRef)
            Return SawAuth.Validate.failed
        End If

        Return SawAuth.Validate.ok

    End Function

    ''' <summary>
    ''' Sets the login_fail_count and login_fail_date on SUSER, do not call this if the user is locked out
    ''' </summary>
    ''' <param name="user_ref"></param>
    ''' <remarks></remarks>
    Public Shared Sub PasswordFailed(ByVal user_ref As String)
        Dim sql As String
        'Dim web_login_attempts As Integer
        Dim web_login_timeout As Integer
        Dim login_fail_count As Integer
        Dim login_fail_date As Date

        web_login_timeout = WebLoginTimeout

        Using DB As New IawDB

            login_fail_count = DB.Scalar("select login_fail_count from dbo.suser where user_ref = @p1", user_ref)
            login_fail_date = Date.Now

            If login_fail_count < WebLoginAttempts Then
                login_fail_count += 1
            Else
                ' once past max attempts,  only keep increasing fail count if not escalation type 3
                If LockEscalation <> "03" Then
                    login_fail_count += 1
                End If
            End If

            sql = "UPDATE dbo.SUSER " _
                + "   SET login_fail_count = @p1, " _
                + "       login_fail_date = @p2 " _
                + " WHERE user_ref = @p3 "
            DB.NonQuery(sql, login_fail_count, login_fail_date, user_ref)

            If login_fail_count < WebLoginAttempts Then
                SawUtil.Log(String.Format("Login fail, attempt {0} of {1}", login_fail_count, WebLoginAttempts))
                Return
            Else
                ' we've at or above the number of login attemps
                If web_login_timeout = 0 Then
                    ' so no extensions, so just lock them out.. use setup app to unlock them
                    sql = "UPDATE dbo.SUSER " _
                        + "   SET psc_status = '02' " _
                        + " WHERE user_ref = @p1 "
                    DB.NonQuery(sql, user_ref)
                    SawUtil.Log(String.Format("Login fail, User Locked after {0} attempts", WebLoginAttempts))
                Else
                    ' we have a login retry after n minutes setting
                    ' so we don't have to do anything, just log the fail and when they can retry


                    'calculate the number of minutes to add the last failed attempt
                    'double the timeout_minutes for each login attempt above the max attempts
                    For i As Integer = 1 To (login_fail_count - WebLoginAttempts)
                        web_login_timeout = web_login_timeout * 2
                    Next

                    SawUtil.Log(String.Format("Login fail, Next login available at {0:dd/MM/yyyy HH:mm:ss}", login_fail_date.AddMinutes(web_login_timeout)))

                End If
            End If
        End Using

    End Sub

    Public Shared Function IsAccountLockedOut(ByVal user_ref As String) As Boolean
        Return IawDB.execScalar("select case when psc_status = '02' then 1 else 0 end from dbo.suser where user_ref = @p1", user_ref) = 1
    End Function

    ''' <summary>
    ''' Returns true if the user is currently locked out
    ''' This could be due to them having to wait to log in again or just being completely locked out
    ''' </summary>
    ''' <param name="user_ref"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsLockedOut(ByVal user_ref As String) As Boolean
        Dim timeout_minutes As Integer
        Dim fail_count As Integer
        Dim last_fail_date As DateTime
        Dim sql As String

        timeout_minutes = WebLoginTimeout   ' this is the duration to wait after web_login_attempts login fails

        Using DB As New IawDB
            sql = "SELECT case when administrator = '1' then 0 else login_fail_count end as login_fail_count," +
                  "       ISNULL(login_fail_date, '1900/01/01') as login_fail_date" +
                  "  FROM dbo.suser WITH (NOLOCK) " +
                  " WHERE user_ref = @p1"
            DB.Query(sql, user_ref)
            If DB.Read() Then
                fail_count = DB.Reader("login_fail_count")
                last_fail_date = DB.Reader("login_fail_date")
            End If
            DB.Reader.Close()

            If WebLoginAttempts = 0 OrElse fail_count < WebLoginAttempts Then
                'NOT locked out
                Return False
            End If

            'calculate the number of minutes to add the last failed attempt
            'double the timeout_minutes for each login attempt above the max attempts
            For i As Integer = 1 To (fail_count - WebLoginAttempts)
                timeout_minutes = timeout_minutes * 2
            Next

            'user may be locked out check the datetime that they were last allowed a login attempt
            If Date.Now < last_fail_date.AddMinutes(timeout_minutes) Then
                'login attempt not allowed as still within lockout period
                Return True
            End If

        End Using

        If LockEscalation <> "01" And fail_count > 0 Then
            sql = "UPDATE dbo.SUSER " _
                + "   SET login_fail_count = 0, " _
                + "       login_fail_date = null " _
                + " WHERE user_ref = @p1 "
            IawDB.execNonQuery(sql, user_ref)
        End If

        'NOT locked out
        Return False

    End Function

#End Region

#Region "Hashing"

    Public Shared Function GenerateSaltAndHash(ByVal Password As String, ByVal Iterations As Integer) As String
        Dim PBKDF2 As New Rfc2898DeriveBytes(Password, 32, Iterations)
        Dim hash() As Byte = PBKDF2.GetBytes(20)
        Dim salt() As Byte = PBKDF2.Salt

        Return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash)
    End Function

    Public Shared Function CheckSaltedPassword(ByVal newPassword As String, ByVal Iterations As Integer, ByVal encSalt As String, ByVal encHash As String) As Boolean
        Dim OrgSalt() As Byte = Convert.FromBase64String(encSalt)
        Dim OrgHash() As Byte = Convert.FromBase64String(encHash)
        Dim NewHash() As Byte = New Rfc2898DeriveBytes(newPassword, OrgSalt, Iterations).GetBytes(20)

        Return Convert.ToBase64String(OrgHash) = Convert.ToBase64String(NewHash)
    End Function

    Public Shared Function HashCheck(ByVal UserRef As String, ByVal Password As String) As Boolean
        ' Given the user ref and the password, work out if the password is correct

        Dim SQL, ls_hash_type, ls_hash, ls_salt As String
        Dim li_hash_iterations, li_loop As Integer

        ' go round twice in case we need to upgrade the hashes
        For li_loop = 1 To 2
            Using DB As New IawDB
                SQL = "SELECT TOP 1 " + _
                      "       hash_type, personal_sec_code, salt, hash_iterations " + _
                      "  FROM dbo.personal_sec_code " + _
                      " WHERE user_ref = @p1 " + _
                      " ORDER BY date_allocated DESC, time_allocated DESC"
                DB.Query(SQL, UserRef)
                If DB.Read() Then
                    ls_hash_type = DB.GetValue("hash_type", "00")
                    ls_hash = DB.Reader("personal_sec_code")
                    ls_salt = DB.GetValue("salt", "")
                    li_hash_iterations = DB.GetValue("hash_iterations", 10000)
                    DB.Reader.Close()
                Else
                    Return False
                End If
            End Using

            ' do we need to upgrade any of the passwords?
            If ls_hash_type = "00" Then
                HashUpgrade(UserRef)
            Else
                Return CheckSaltedPassword(Password, li_hash_iterations, ls_salt, ls_hash)
            End If
        Next

    End Function

    Public Shared Sub HashUpgrade(ByVal UserRef As String)
        '// Args : as_user_ref - either specific user ref or % to do all

        Dim SQL, ls_user, ls_personal_sec_code, result, hash, salt As String
        Dim ldt_date_allocated, ldt_time_allocated As Date
        Dim li_iterations As Integer = DefaultIterations()

        Using DB As New IawDB
            SQL = "SELECT user_ref, " + _
                  "       date_allocated," + _
                  "       time_allocated, " + _
                  "       personal_sec_code " + _
                  "  FROM dbo.personal_sec_code " + _
                  " WHERE user_ref like @p1 " + _
                  "   AND hash_type = '00' "
            DB.Query(SQL, UserRef)
            While DB.Read
                ls_user = DB.Reader("user_ref")
                ldt_date_allocated = DB.Reader("date_allocated")
                ldt_time_allocated = DB.Reader("time_allocated")
                ls_personal_sec_code = SawUtil.Encryption(DB.Reader("personal_sec_code"))

                result = GenerateSaltAndHash(ls_personal_sec_code, li_iterations)
                salt = result.Split("|")(0)
                hash = result.Split("|")(1)

                SQL = "UPDATE dbo.personal_sec_code" + _
                      "   SET personal_sec_code = @p4," + _
                      "       salt              = @p5," + _
                      "       hash_iterations   = @p6," + _
                      "       hash_type         = @p7" + _
                      " WHERE user_ref       = @p1" + _
                      "   AND date_allocated = @p2" + _
                      "   AND time_allocated = @p3"
                IawDB.execNonQuery(SQL, ls_user, ldt_date_allocated, ldt_time_allocated, hash, salt, li_iterations, "01")
            End While
        End Using
    End Sub

    Public Shared Sub HashGenerate(ByVal UserRef As String, ByVal Password As String)
        ' Given the user name and the password, create a new persoanl sec code record

        Dim ls_user_name, SQL, result, salt, hash As String
        Dim li_iterations As Integer = DefaultIterations
        Dim ldt_today, ldt_now As Date

        ls_user_name = IawDB.execScalar("select user_name from dbo.suser where user_ref = @p1", UserRef)

        result = GenerateSaltAndHash(Password, li_iterations)
        salt = result.Split("|")(0)
        hash = result.Split("|")(1)

        ldt_today = SawUtil.DateOnly(Today())
        ldt_now = SawUtil.getDBTime(Now())

        Using DB As New IawDB
            SQL = "INSERT INTO dbo.personal_sec_code  " + _
                  "    ( date_allocated,   " + _
                  "      time_allocated,   " + _
                  "      user_ref,         " + _
                  "      change_reason,    " + _
                  "      personal_sec_code," + _
                  "      salt,             " + _
                  "      hash_type,        " + _
                  "      hash_iterations ) " + _
                  "VALUES ( @p1,@p2,@p3,'01',@p4,@p5,'01',@p6 )"
            DB.NonQuery(SQL, ldt_today, ldt_now, UserRef, hash, salt, li_iterations)

            'SawUtil.Log("Changed Password")

        End Using
    End Sub

    Public Shared Function CheckHashReuse(ByVal UserRef As String, ByVal Password As String) As Boolean
        '
        ' Check whether a password has already been used before
        ' Return True  - Password used before
        '        False - Password NOT used before
        '
        ' This routine has to retrieve all previous rows from personal_sec_code
        ' and use the salt and iterations from each row to generate a hash with the new password
        ' that can be compared with the hash for that historic record.
        '
        Dim HashType, Salt, PersonalSecCode, SQL As String
        Dim HashIterations As Integer

        Using DB As New IawDB
            SQL = "SELECT hash_type," + _
                  "       personal_sec_code," + _
                  "       salt," + _
                  "       hash_iterations" + _
                  "  FROM dbo.personal_sec_code WITH (NOLOCK)" + _
                  " WHERE user_ref = @p1" + _
                  " ORDER BY date_allocated DESC, time_allocated DESC"
            DB.Query(SQL, UserRef)
            While DB.Read
                HashType = DB.Reader("hash_type")
                PersonalSecCode = DB.Reader("personal_sec_code")
                Salt = DB.Reader("salt")
                HashIterations = DB.GetValue("hash_iterations", 10000)

                If HashType = "00" Then
                    If Password = SawUtil.Encryption(PersonalSecCode) Then
                        Return True
                    End If
                Else
                    If CheckSaltedPassword(Password, HashIterations, Salt, PersonalSecCode) Then
                        Return True
                    End If
                End If
            End While
        End Using

        Return False

    End Function

#End Region


End Class


''' <summary>
''' Helper Class for chan password
''' </summary>
''' <remarks></remarks>
Public Class SawPasswordWorked
    Private _worked As Boolean
    Private _message As String

    Public ReadOnly Property Worked() As Boolean
        Get
            Return Me._worked
        End Get
    End Property

    Public ReadOnly Property Message() As String
        Get
            Return Me._message
        End Get
    End Property

    ''' <summary>
    ''' Password change was successful
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        Me._message = String.Empty
        Me._worked = True
    End Sub

    Public Sub New(ByVal worked As Boolean, ByVal message As String)
        Me._message = message
        Me._worked = Worked
    End Sub

End Class
