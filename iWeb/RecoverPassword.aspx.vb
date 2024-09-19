Imports IAW.controls

<ButtonBarRequired(False), ProcessRequired(False), DisableEnterKey(False), MenuRequired(False), DirtyPageHandling(False)> _
Partial Class RecoverPassword
    Inherits stub_IngenWebPage

    '----------------------------------------------------------------------------------------------------------------
    ' [Back to Login]  [Reset Screen]                  reset only visible after username continue
    '
    '1   USERNAME    [______]                          Get username and check it exists
    '1   Continue
    '
    '2   secirity question                             if valid user and security question in use, display
    '2   security answer  [___________]
    ' 
    '2   Send code to                                  if send code in use and more than one option found, display
    '2   o home email 
    '2   o work email 
    '2   o mobile     
    ' 
    '2   Continue
    '
    '3   "a code has been 
    '3   Enter Code  [__________]                      get user to enter code we sent them
    '
    '3   Continue
    '   
    '4   Change password dialog
    '4   old password      [_____________________]     old password only visible it sending new password to user
    '4   new password      [_____________________]
    '4   repleat password  [_____________________]
    '
    '4   Continue
    '
    '
    ' if code correct, then we can send them to the change password screen
    '----------------------------------------------------------------------------------------------------------------

#Region "Variables & Properties"
    Private LastSent As String = ""

    Private Property Stage() As Integer
        Get
            Dim o As Integer = ViewState("Stage")
            If o = Nothing Then
                o = 1
                ViewState("Stage") = o
            End If
            Return o
        End Get
        Set(ByVal value As Integer)
            ViewState("Stage") = value
        End Set
    End Property

    Private ReadOnly Property UserName() As String
        Get
            Return Me.txtUserName.Text.Trim
        End Get
    End Property

    Private ReadOnly Property UserRef() As String
        Get
            Dim ls_user_ref As String = IawDB.execScalar("select user_ref from suser where user_name = @p1", UserName)
            If String.IsNullOrEmpty(ls_user_ref) Then Return "" Else Return ls_user_ref
        End Get
    End Property

    Private ReadOnly Property PersonRef() As String
        Get
            Dim ls_person_ref As String = IawDB.execScalar("select person_ref from suser where user_ref = @p1", UserRef)
            If String.IsNullOrEmpty(ls_person_ref) Then Return "" Else Return ls_person_ref
        End Get
    End Property

    Private ReadOnly Property EMailAddress() As String
        Get
            Dim email As String
            email = IawDB.execScalar("SELECT isnull(e_mail_address,'') " _
                                   + "  FROM dbo.person WITH (NOLOCK) " _
                                   + " WHERE person_ref=@p1", Me.PersonRef)
            If String.IsNullOrEmpty(email) Then email = ""
            Return email
        End Get
    End Property

    Enum Recovery
        Disabled
        NewPassword
        PinCode
    End Enum

    Private ReadOnly Property RecoveryMethod() As Recovery
        Get
            Dim r As Recovery = Recovery.Disabled
            Dim s As String = ViewState("RecoveryType")
            If String.IsNullOrEmpty(s) Then
                s = IawDB.execScalar("select web_recover_pwd from dbo.global_self_serv")
                ViewState("RecoveryType") = s
            End If

            Select Case s
                Case "00"
                    r = Recovery.Disabled
                Case "01"
                    r = Recovery.NewPassword
                Case "02"
                    r = Recovery.PinCode
            End Select

            Return r
        End Get
    End Property

    Private ReadOnly Property MFAActive() As Boolean
        Get
            Dim o As Boolean = ViewState("MFAActive")
            If o = Nothing Then
                o = IawDB.execScalar("SELECT mfa_active FROM dbo.global_self_serv") = "1"
                ViewState("MFAActive") = o
            End If
            Return o
        End Get
    End Property

    Private ReadOnly Property ContactDT() As DataTable
        Get
            Dim o As DataTable = ViewState("ContactDT")
            Dim FixedData As String
            Dim Prefix As String = ""
            If o Is Nothing Then
                ' first we need to find a person ref
                Dim PersonRef As String = IawDB.execScalar("select person_ref from suser where user_ref = @p1", UserRef)
                'If String.IsNullOrEmpty(PersonRef) Then Return Nothing
                o = IawDB.execGetTable("select * from dbo.dbf_person_data_valid(@p1) where pdv_status in ('01','02','04')", PersonRef)

                ' mask any email addresses / mobile numbers
                For Each r As DataRow In o.Rows
                    FixedData = MaskEmail(r.Item("pdv_data"))
                    Select Case r.Item("pdv_type")
                        Case "01", "02"
                            Prefix = SawLang.Translate("::LT_S0180") ' LT_S0180 - Email address
                        Case "03"
                            Prefix = SawLang.Translate("::LT_S0299") ' LT_S0299 - Mobile number ending with
                    End Select

                    r.Item("pdv_data") = Prefix + " " + FixedData
                Next

                ViewState("ContactDT") = o
            End If
            Return o
        End Get
    End Property

#End Region

#Region "page events"

    Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        If ctx.IsAuthenticated Then
            ctx.logout()
        End If

        If Not Page.IsPostBack Then
            ' Just check that if MFA isn't active, then recovery password must only use send new password, not send code.
            If RecoveryMethod = Recovery.PinCode And Not MFAActive Then
                IawDB.execNonQuery("UPDATE dbo.global_self_serv SET web_recover_pwd = '01'")
                ViewState("RecoveryType") = Nothing
            End If
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        lblMsg.Text = SawUtil.GetErrMsg("WB_B0008")

        If Not Page.IsPostBack Then
            RFV_txtUserName.ErrorMessage = SawLang.Translate("::LT_S0300") ' LT_S0300 - This is a required field
            RFV_txtSecurityAnswer.ErrorMessage = SawLang.Translate("::LT_S0300") ' LT_S0300 - This is a required field
            RFV_txtSecurityCode.ErrorMessage = SawLang.Translate("::LT_S0300") ' LT_S0300 - This is a required field

            Stage = 1
            SetScreenState()
        End If
    End Sub

    Private Sub SetScreenState()
        Dim Email As String

        ' firstly, lets hide everything
        txtUserName.Enabled = False
        TR_1_sep.Visible = False
        TR_1_continue.Visible = False

        TR_2_sep1.Visible = False
        TR_2_qu.Visible = False
        TR_2_ans.Visible = False
        TR_2_sep2.Visible = False
        TR_2_Address.Visible = False
        TR_2_sep3.Visible = False
        TR_2_Continue.Visible = False

        TR_3_MsgSent.Visible = False
        TR_3_sep1.Visible = False
        TR_3_SecurityCode.Visible = False
        TR_3_labCodeFail.Visible = False
        TR_3_sep2.Visible = False
        TR_3_continue.Visible = False

        TR_4_MsgSent.Visible = False
        TR_4_sep1.Visible = False
        TR_4_ChangePwd.Visible = False
        TR_4_sep2.Visible = False
        TR_4_continue.Visible = False

        Select Case Stage
            Case 1
                txtUserName.Enabled = True
                TR_1_sep.Visible = True
                TR_1_continue.Visible = True
                txtUserName.Text = ""
                FieldFocus(txtUserName)
                MainPanel.DefaultButton = btn_1_hidden.ID

            Case 2

                If RecoveryMethod = Recovery.PinCode Then
                    If ContactDT IsNot Nothing Then
                        If ContactDT.Rows.Count > 1 Then
                            TR_2_sep2.Visible = True
                            TR_2_Address.Visible = True
                        End If
                    End If
                End If

                TR_2_sep3.Visible = True
                TR_2_Continue.Visible = True
                MainPanel.DefaultButton = btn_2_hidden.ID

            Case 3
                TR_3_MsgSent.Visible = True
                If UserRef = "" Then

                    ' attempt to get a valid email address from the system.. in a random way.
                    Email = IawDB.execScalar("select top 1 e_mail_address " + _
                                             "  from dbo.person with (nolock) " + _
                                             " where IsNull(RTrim(e_mail_address),'') <> '' " + _
                                             " order by NEWID() ")
                    If String.IsNullOrEmpty(Email) Then
                        Email = IawDB.execScalar("select top 1 e_mail_name " + _
                                                 "  from dbo.person with (nolock) " + _
                                                 " where IsNull(RTrim(e_mail_name),'') <> '' " + _
                                                 " order by NEWID() ")
                        If String.IsNullOrEmpty(Email) Then
                            Email = txtUserName.Text.Substring(0, 1) + SawUtil.RandomString(10, 12, False) + "@" + SawUtil.RandomString(6, 12, False) + ".co.uk"
                        End If
                    End If

                    Email = txtUserName.Text.Substring(0, 1) + Email

                    lab_3_MsgSent.Text = SawLang.Translate("::LT_S0301") + ' LT_S0301 - A code has been sent to
                                         "<br/>" +
                                         SawLang.Translate("::LT_S0180") + " " + MaskEmail(Email) ' LT_S0180 - Email address
                Else
                    lab_3_MsgSent.Text = LastSent
                End If

                TR_3_sep1.Visible = True
                TR_3_SecurityCode.Visible = True
                TR_3_sep2.Visible = True
                TR_3_continue.Visible = True
                txtSecurityCode.Text = ""
                FieldFocus(txtSecurityCode)
                MainPanel.DefaultButton = btn_3_hidden.ID

            Case 4
                If RecoveryMethod = Recovery.NewPassword Then
                    TR_4_MsgSent.Visible = True
                    Email = EMailAddress
                    If Email = "" Then
                        Email = SawUtil.RandomString(10, 12, False) + "@" + SawUtil.RandomString(6, 12, False) + ".co.uk"
                    End If
                    lab_4_MsgSent.Text = SawLang.Translate("::LT_S0302") + " " + MaskEmail(Email) ' LT_S0302 - A new password has been sent to
                End If

                Session("person_ref") = PersonRef   ' used by change password user control - AfterLoadProcessing()
                TR_4_sep1.Visible = True
                TR_4_ChangePwd.Visible = True
                TR_4_sep2.Visible = True
                TR_4_continue.Visible = True
                MainPanel.DefaultButton = btn_4_hidden.ID

                If RecoveryMethod = Recovery.PinCode Then
                    ChangePwd.EnterCurrentPwd = False
                Else
                    ChangePwd.EnterCurrentPwd = True
                End If
                ' refresh the change password control's 
                ChangePwd.AfterLoadProcessing()

        End Select

    End Sub

    Private Function EnsureUser() As Boolean
        Dim DT As DataTable

        lblMsg.Visible = False

        ' Check to ensure user exists, if not, log it in weh_login_audit table
        If IawDB.execScalar("Select count(1) from dbo.suser where user_name = @p1", Me.UserName) = 0 Then Return False

        DT = ContactDT
        If DT IsNot Nothing Then
            If DT.Rows.Count > 0 Then
                rblAddresses.DataSource = ContactDT
                rblAddresses.DataValueField = "pdv_type"
                rblAddresses.DataTextField = "pdv_data"
                rblAddresses.DataBind()
                rblAddresses.SelectedIndex = 0
                If RecoveryMethod = Recovery.PinCode Then
                    If DT.Rows.Count > 1 Then
                        TR_2_Address.Visible = False
                    End If
                End If
            End If
        End If

        Return True
    End Function


#End Region

#Region "click handlers"
    Private Sub btnReset_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnReset.Click
        ViewState("ContactDT") = Nothing
        txtUserName.Text = ""
        txtSecurityCode.Text = ""
        txtSecurityCode.Text = ""
        txtSecurityAnswer.Text = ""
        Stage = 1
        SetScreenState()
    End Sub

    ' Continue button after username
    Private Sub btn_1_continue_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_1_continue.Click, _
                                                                                                  btn_1_hidden.Click
        If String.IsNullOrEmpty(Me.txtUserName.Text) Then Return

        If EnsureUser() Then

            If RecoveryMethod = Recovery.PinCode And ContactDT.Rows.Count > 1 Then
                Stage = 2
            ElseIf RecoveryMethod = Recovery.PinCode And ContactDT.Rows.Count = 1 Then
                TransmitMessage()
                Stage = 3
            ElseIf RecoveryMethod = Recovery.NewPassword Then
                TransmitMessage()
                Stage = 4
            Else
                Stage = 3
            End If

            SetScreenState()
        End If
    End Sub

    ' continue button after security question or choose email address
    Private Sub btn_2_continue_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_2_continue.Click, _
                                                                                                  btn_2_hidden.Click
        If Not Page.IsValid Then Return

        If RecoveryMethod = Recovery.PinCode Then
            Stage = 3
        Else
            Stage = 4
        End If
        TransmitMessage()
        SetScreenState()
    End Sub

    Protected Sub btn_3_continue_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_3_continue.Click, _
                                                                                                    btn_3_hidden.Click
        TR_3_labCodeFail.Visible = False
        labCodeFail.Text = SawLang.Translate("::LT_S0298") ' Incorrect Code

        If btn_3_continue.Text <> "Continue" Then
            TransmitMessage()
            txtSecurityCode.Text = ""
            btn_3_continue.Text = "Continue"
            Return
        End If

        If Not Page.IsValid Then Return

        TR_3_labCodeFail.Visible = False
        labCodeFail.Text = SawLang.Translate("::LT_S0298") ' Incorrect Code

        btn_3_continue.Text = "Continue"

        Dim Result As String = ""

        txtSecurityCode.Text = txtSecurityCode.Text.Trim
        If txtSecurityCode.Text = "" Then Return

        ' we don't have a real user, so just display bad code message
        If UserRef = "" Then
            TR_3_labCodeFail.Visible = True
            Return
        End If

        Using DB As New IawDB
            DB.ClearParameters()
            DB.AddParameter("@Action", "TEST")
            DB.AddParameter("@UserRef", UserRef)
            DB.AddParameter("@Pin", txtSecurityCode.Text)
            DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
            DB.CallNonQueryStoredProc("dbo.dbp_mfa")
            Result = DB.GetParameter("@Result")

            If Result = "NO" Then
                TR_3_labCodeFail.Visible = True
                Return
            End If

            ' if it's not 'YES' then we have an error of some kind in form of x|message
            If Result <> "YES" Then
                labCodeFail.Text = Split(Result, "|")(1)
                TR_3_labCodeFail.Visible = True
                btn_3_continue.Text = "Get New Code"
                Return
            End If

        End Using

        Stage = 4
        SetScreenState()
    End Sub

    Private Sub btn_4_continue_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_4_continue.Click, _
                                                                                                  btn_4_hidden.Click
        If ChangePwd.Update Then
            If RecoveryMethod = Recovery.NewPassword Then
                ' if recovery method is password, then we can mark that email address as validated
                IawDB.execNonQuery("UPDATE dbo.person_data_valid " + _
                                   "   SET pdv_status = '02' " + _
                                   " WHERE person_ref = @p1 " + _
                                   "   AND pdv_type = '01' " + _
                                   "   AND pdv_status IN ('01', '02', '04')", PersonRef)
            End If

            Response.Redirect("~/secure/Login.aspx")
        End If
    End Sub

    Private Sub ShowMessage(ByVal text As String, ByVal Err As Boolean)
        lblMsg.Text = text
        lblMsg.ForeColor = IIf(Err, Drawing.Color.Red, Drawing.Color.Black)
        lblMsg.Visible = True
    End Sub

    Private Sub TransmitMessage()
        Dim emailPwd As SawPasswordWorked
        Dim Via As String
        Dim DestType As String
        Dim Dest As String
        Dim Txt As String
        Dim Result As String = ""

        LastSent = ""
        lblMsg.Visible = False

        Using DB As New IawDB
            If RecoveryMethod = Recovery.NewPassword Then
                'check to see if user has an email address
                If DB.Scalar("SELECT isnull(P.e_mail_address,'') " _
                           + "  FROM dbo.person P WITH (NOLOCK) JOIN suser S WITH (NOLOCK) " _
                           + "                     ON P.person_ref = S.person_ref " _
                           + " WHERE S.user_name=@p1", Me.UserName) = "" Then
                    'ShowMessage(SawUtil.GetErrMsg("WB_B0008"), True)
                    Return
                End If

                emailPwd = SawPassword.EmailNewPassword(Me.UserName)

                lblMsg.Text = emailPwd.Message
            End If

            If RecoveryMethod = Recovery.PinCode Then

                Via = rblAddresses.SelectedValue

                DB.ClearParameters()
                DB.AddParameter("@Action", "TRANSMIT")
                DB.AddParameter("@UserRef", UserRef)
                DB.AddParameter("@Pin", "")
                DB.AddParameter("@Via", Via)
                DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
                DB.CallNonQueryStoredProc("dbo.dbp_mfa")
                Result = DB.GetParameter("@Result")

                If Not Result.StartsWith("YES|") Then
                    ShowMessage(SawLang.Translate(Split(Result, "|")(1)), True)
                Else
                    DestType = Split(Result, "|")(1)
                    Dest = Split(Result, "|")(2)
                    Txt = SawLang.Translate("::LT_S0301") + "<br/>" ' LT_S0301 - A code has been sent to

                    Select Case DestType
                        Case "01", "02"
                            LastSent = Txt + SawLang.Translate("::LT_S0180") + " " + MaskEmail(Dest) ' LT_S0180 - Email address
                        Case "03"
                            LastSent = Txt + SawLang.Translate("::LT_S0299") + " " + MaskEmail(Dest) ' LT_S0299 - Mobile number ending with
                    End Select
                End If
            End If
        End Using
    End Sub

#End Region

#Region "Utils"

    'Private Sub FieldFocus(ByVal ctrl As Control)
    '    ScriptManager.GetCurrent(Me.Page).SetFocus(ctrl)
    'End Sub

    Function MaskEmail(ByVal s As String) As String
        Dim Pattern As String = "(?<=.).(?=[^@]*?.@)|(?:(?<=@.)|(?!^)\G(?=[^@]*$)).(?=.*\.)"
        If s.Trim.Length = 0 Then Return "Error, empty value"
        If Not s.Contains("@") Then
            ' probably dealing with a phone number, so just return the last 3 digits
            If s.Length < 3 Then Return s
            Return Strings.Right(s, 3)
        End If
        Return Regex.Replace(s, Pattern, "*")
    End Function

#End Region

End Class
