
Namespace IAW.controls

    Public Class IAWLogin
        Inherits System.Web.UI.WebControls.CompositeControl

#Region "members"
        ' Container for first login screen
        Protected Logincontainer As New Panel
        Protected LoginTable As New Table
        Protected lblLogInHeader As New IAWLabel
        Protected lblDomain As New IAWLabel
        Public ddlDomains As New DropDownList
        Protected lblUsername As New IAWLabel
        Protected txtUser As New TextBox
        Protected lblPassword As New IAWLabel
        Protected txtPassword As New TextBox

        Protected labPassword_eye As New Label
        Protected labPassword_noeye As New Label
        Protected btnLogin As New IAWHyperLinkButton
        Protected btnHiddenLogin As New Button
        Protected btnRecover As New IAWHyperLink
        Protected lblRecover As New IAWLabel
        Protected lnkRecover As New IAWHyperLink
        Protected lblLoginFail As New IAWLabel

        ' Container to allow user to select where to send MFA message
        Protected SendToContainer As New Panel
        Protected SendToTable As New Table
        Protected lblSendToHeader As New IAWLabel
        Protected lblSendToMsg As New IAWLabel
        Protected lblAddresses As New IAWLabel
        Protected rblAddresses As New RadioButtonList
        Protected btnSendToMsg As New IAWHyperLinkButton
        Protected btnHiddenSendToMsg As New Button

        ' Container to enter MFA code
        Protected SMScontainer As New Panel
        Protected SMSTable As New Table
        Protected lblSMSMsgHeader As New IAWLabel
        Protected lblSMSMsg As New IAWLabel
        Protected lblSMSCode As New IAWLabel
        Protected txtSMSCode As New TextBox
        Protected btnSubmitSMSCode As New IAWHyperLinkButton
        Protected btnHiddenSubmitSMSCode As New Button
        Protected btnGetNewSMSCode As New IAWHyperLinkButton
        Protected lblSMSFail As New IAWLabel

#End Region

#Region "properties"
        Public ReadOnly Property Password() As String
            Get
                Return Me.txtPassword.Text
            End Get
        End Property
        Public ReadOnly Property UserName() As String
            Get
                Return Me.txtUser.Text
            End Get
        End Property
        Public ReadOnly Property SMSCode() As String
            Get
                Return Me.txtSMSCode.Text.Trim
            End Get
        End Property

        Public Property PasswordRecoveryText() As String
            Get
                Dim o As Object = Me.ViewState("PasswordRecoveryText")
                If o Is Nothing Then
                    Return ""
                End If
                Return CStr(o)
            End Get
            Set(ByVal value As String)
                Me.ViewState("PasswordRecoveryText") = value
            End Set
        End Property
        Public Property PasswordRecoveryUrl() As String
            Get
                Dim o As Object = Me.ViewState("PasswordRecoveryUrl")
                If o Is Nothing Then
                    Return ""
                End If
                Return CStr(o)
            End Get
            Set(ByVal value As String)
                Me.ViewState("PasswordRecoveryUrl") = value
            End Set
        End Property
        Public Property DestinationPageUrl() As String
            Get
                Dim o As Object = Me.ViewState("DestinationPageUrl")
                If o Is Nothing Then
                    Return ""
                End If
                Return CStr(o)
            End Get
            Set(ByVal value As String)
                Me.ViewState("DestinationPageUrl") = value
            End Set
        End Property
        Public Property ActiveDirDomains() As IEnumerable
            Get
                Return Me.ViewState("ActiveDirDomains")
            End Get
            Set(ByVal value As IEnumerable)
                Me.ViewState("ActiveDirDomains") = value
            End Set
        End Property
        Private Property CurrentPage() As String
            Get
                Dim o As Object = Me.ViewState("CurrentPage")
                If o Is Nothing Then
                    Return "Login"
                End If
                Return CStr(o)
            End Get
            Set(ByVal value As String)
                Me.ViewState("CurrentPage") = value
            End Set
        End Property

        Private ReadOnly Property GlobalSelfServDR As DataRow
            Get
                Dim DR As DataRow = ViewState("GlobalSelfServDR")
                If DR Is Nothing Then
                    DR = IawDB.execGetTable("select top 1 " +
                                            "       mfa_active, " +
                                            "       IsNull(mfa_valid_hours,0) as mfa_valid_hours, " +
                                            "       web_recover_pwd, " +
                                            "       mfa_selection " +
                                            "  from dbo.global_self_serv").Rows(0)
                    ViewState("GlobalSelfServDR") = DR
                End If
                Return DR
            End Get
        End Property

        Private ReadOnly Property MFAActive() As Boolean
            Get
                Return GlobalSelfServDR("mfa_active") = "1"
            End Get
        End Property

        Private ReadOnly Property MFAHours() As Boolean
            Get
                Return GlobalSelfServDR("mfa_valid_hours") > 0
            End Get
        End Property

        Private ReadOnly Property MFASelection As Boolean
            Get
                Return GlobalSelfServDR("mfa_selection") = "02"
            End Get
        End Property

        Private ReadOnly Property SystemAdminUser() As Boolean
            Get
                Return IawDB.execScalar("select count(1) 
                                           from dbo.suser with (nolock) 
                                          where administrator = '1' 
                                            and user_ref = @p1", UserName) > 0
            End Get
        End Property

        Private ReadOnly Property RecoveryType() As String
            Get
                Return GlobalSelfServDR("web_recover_pwd")
            End Get
        End Property

        Public ReadOnly Property ShowRecovery As Boolean
            Get
                Dim Resp As Boolean = False
                If Not SystemAdminUser Then
                    If RecoveryType = "01" And mailer.IsEmailConfigured Then Resp = True
                    If RecoveryType = "02" And MFAActive Then Resp = True
                End If
                Return Resp
            End Get
        End Property

        Private ReadOnly Property ContactDT() As DataTable
            Get
                Dim o As DataTable = ViewState("ContactDT")
                Dim FixedData As String
                Dim Prefix As String = ""
                If o Is Nothing Then
                    ' first we need to find a person ref
                    Dim PersonRef As String = IawDB.execScalar("select person_ref from suser where user_ref = @p1", ctx.user_ref)
                    'If String.IsNullOrEmpty(PersonRef) Then Return Nothing
                    o = IawDB.execGetTable("select * from dbo.dbf_person_data_valid(@p1) where pdv_status in ('01','02','04')", PersonRef)

                    ' mask any email addresses / mobile numbers
                    For Each r As DataRow In o.Rows
                        FixedData = MaskEmail(r.Item("pdv_data"))
                        Select Case r.Item("pdv_type")
                            Case "01", "02"
                                Prefix = SawLang.Translate("::LT_S0180") + " " ' Email Address
                            Case "03"
                                Prefix = SawLang.Translate("::LT_S0299") + " " ' Mobile number ending with
                        End Select

                        r.Item("pdv_data") = Prefix + FixedData
                    Next

                    ViewState("ContactDT") = o
                End If
                Return o
            End Get
        End Property

        Private ReadOnly Property IsMFARequired As String
            Get
                Dim Result As String = ""

                Using DB As New IawDB
                    DB.ClearParameters()
                    DB.AddParameter("@Action", "REQUIRED")
                    DB.AddParameter("@UserRef", ctx.user_ref)
                    DB.AddParameter("@Pin", "")
                    DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
                    DB.CallNonQueryStoredProc("dbo.dbp_mfa")
                    Result = DB.GetParameter("@Result")
                End Using

                If Not Result.ContainsOneOf("YES", "NO") Then
                    SawUtil.Log("MFA failed : " + Split(Result, "|")(1))
                End If

                Return Result
            End Get
        End Property

#End Region

#Region "events"
        Public Event Authenticate As AuthenticateEventHandler
        Public Event LoginFailed As EventHandler(Of EventArgs)

        Protected Overridable Sub OnAuthenticate(ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs)
            RaiseEvent Authenticate(Me, e)
        End Sub

        Protected Overridable Sub OnLoginFailed(ByVal e As EventArgs)
            RaiseEvent LoginFailed(Me, e)
        End Sub

        ' --------------------------------------------------------------------------------------------
        ' Login Screen
        '   lblDomain           ddlDomains          -- Settings active_directory_domains not empty
        '   lblUsername         txtUser
        '   lblPassword         txtPassword         imgPasswordEye / imgPasswordNoEye
        '   btnLogin / btnHiddenLogin
        '   btnRecover                              -- global_self_serv.web_recover_pwd
        '   lblLoginFail                    
        ' Where to Send MFA
        '  lblSendToMsg                             -- Select where to send the code
        '  rblAddresses
        '  btnSendMsg  / btnHiddenSendMsg
        ' Enter MFA Code
        '  lblSMSMsg
        '  lblSMSCode           txtSMSCode
        '  btnSubmitSMSCode / btnHiddenSubmitSMSCode
        '  btnGetNewSMSCode
        '  lblSMSFail
        ' --------------------------------------------------------------------------------------------
        Public Sub New()
            Logincontainer.Visible = True
            SendToContainer.Visible = False
            SMScontainer.Visible = False

            LoginTable.CssClass = "login_table"
            SendToTable.CssClass = "login_table"
            SMSTable.CssClass = "login_table"

            With lblLogInHeader
                '.Text = ctx.Translate("::LT_S0383") ' "Log In"
                .Text = "::LT_S0383" ' "Log In"
            End With

            With lblDomain
                '.Text = ctx.Translate("::LT_M0024") ' "Domain"
                .Text = "::LT_M0024" ' "Domain"
            End With

            With lblUsername
                '.Text = ctx.Translate("::LT_S0180") ' "Email address"
                .Text = "::LT_S0180" ' "Email address"
            End With

            With txtUser
                .ID = "Username"
                .CssClass = "login_field"
                .Attributes.Add("autocomplete", "email")
                .Attributes.Add("placeholder", SawLang.Translate("::LT_S0334")) ' Enter email
            End With

            With lblPassword
                '.Text = ctx.Translate("::LT_S0384") ' "Password"
                .Text = "::LT_S0384" ' "Password"
            End With

            With txtPassword
                .ID = "Password"
                .CssClass = "login_field"
                .TextMode = TextBoxMode.Password
                .Attributes.Add("autocomplete", "off")
                .Attributes.Add("placeholder", SawLang.Translate("::LT_S0335")) ' Enter password
            End With

            With labPassword_eye
                .ID = "labPassword_eye"
                .CssClass = "fa-solid fa-eye"
                .ToolTip = ctx.Translate("::LT_S0336") ' Show Password
                .Attributes.Add("onclick", "TogglePassword();")
                .Style.Add("display", "inline")
            End With

            With labPassword_noeye
                .ID = "labPassword_noeye"
                .CssClass = "fa-solid fa-eye-slash"
                .ToolTip = ctx.Translate("::LT_S0337") ' Hide Password
                .Attributes.Add("onclick", "TogglePassword();")
                .Style.Add("display", "none")
            End With

            With lblLoginFail
                .ID = "LoginFailureText"
                .EnableViewState = False
                '.Text = ctx.Translate("::LT_S0385") '"Log In Failed"
                .Text = "::LT_S0385" '"Log In Failed"
            End With

            With btnLogin
                .ID = "LoginButton"
                .CommandName = "Login"
                '.Text = ctx.Translate("::LT_S0383") '"Log In"
                '.ToolTip = ctx.Translate("::LT_S0383") '"Log In"
                .Text = "::LT_S0383" '"Log In"
                .ToolTip = "::LT_S0383" '"Log In"
            End With
            AddHandler btnLogin.Click, AddressOf LoginButton_click

            '---------------------------------------------------------------
            'hidden btn used a default button
            'default button does not work in FFox with linkbuttons!
            With btnHiddenLogin
                .ID = "LoginButtonHidden"
                .CommandName = "Login"
                .Style.Add("display", "none")
            End With
            AddHandler btnHiddenLogin.Click, AddressOf LoginButton_click

            '---------------------------------------------------------------
            ' work out if password recovery button should be visible

            If ShowRecovery Then
                'Me.PasswordRecoveryText = SawLang.Translate("::LT_S0338") ' Forgotten Your Password?
                Me.PasswordRecoveryText = "::LT_S0338" ' Forgotten Your Password?
                Me.PasswordRecoveryUrl = "~/RecoverPassword.aspx"

                'lblRecover.Text = "Forgotten your password?"
                lnkRecover.CssClass = ""
                'lnkRecover.Text = SawLang.Translate("::LT_S0338") ' Forgotten Your Password?
                lnkRecover.Text = "::LT_S0338" ' Forgotten Your Password?
                lnkRecover.Url = "~/RecoverPassword.aspx"
                lnkRecover.TabIndex = -1
            End If

            ' -------------------------------------------------------------------------------------------------

            With lblSendToHeader
                '.Text = ctx.Translate("::LT_S0386") ' "Authentication"
                .Text = "::LT_S0386" ' "Authentication"
            End With

            With lblSendToMsg
                '.Text = ctx.Translate("::LT_S0387") ' "Please select where to send the security code"
                .Text = "::LT_S0387" ' "Please select where to send the security code"
            End With

            With rblAddresses
                .RepeatDirection = RepeatDirection.Vertical
                .DataValueField = "pdv_type"
                .DataTextField = "pdv_data"
                .SelectedIndex = 0
            End With

            With btnSendToMsg
                .ID = "btnSendToMsg"
                .CommandName = "SendToMsg"
                '.Text = ctx.Translate("::LT_S0388") ' "Send Code"
                '.ToolTip = ctx.Translate("::LT_S0388") ' "Send Code"
                .Text = "::LT_S0388" ' "Send Code"
                .ToolTip = "::LT_S0388" ' "Send Code"
            End With
            AddHandler btnSendToMsg.Click, AddressOf SendToMsg_click

            With btnHiddenSendToMsg
                .ID = "SendToMsgHidden"
                .CommandName = "Send"
                .Style.Add("display", "none")
            End With
            AddHandler btnHiddenSendToMsg.Click, AddressOf SendToMsg_click

            ' -------------------------------------------------------------------------------------------------

            With lblSMSMsgHeader
                '.Text = ctx.Translate("::LT_S0386") ' "Authentication"
                .Text = "::LT_S0386" ' "Authentication"
            End With

            With lblSMSCode
                '.Text = ctx.Translate("::LT_S0297") ' "Enter Security Code"
                .Text = "::LT_S0297" ' "Enter Security Code"
            End With

            With txtSMSCode
                .ID = "SMSCode"
                .CssClass = "login_field"
            End With

            With lblSMSFail
                .ID = "SMSFailureText"
                .EnableViewState = False
                '.Text = ctx.Translate("::LT_S0298") ' "Incorrect Code"
                .Text = "::LT_S0298" ' "Incorrect Code"
            End With

            With btnSubmitSMSCode
                .ID = "btnSubmitSMSCode"
                .CommandName = "SubmitSMSCode"
                '.Text = ctx.Translate("::LT_S0389") ' "Submit Code"
                '.ToolTip = ctx.Translate("::LT_S0389") ' "Submit Code"
                .Text = "::LT_S0389" ' "Submit Code"
                .ToolTip = "::LT_S0389" ' "Submit Code"
            End With
            AddHandler btnSubmitSMSCode.Click, AddressOf SubmitSMSCode_click

            With btnHiddenSubmitSMSCode
                .ID = "btnHiddenSubmitSMSCode"
                .CommandName = "Submit"
                .Style.Add("display", "none")
            End With
            AddHandler btnHiddenSubmitSMSCode.Click, AddressOf SubmitSMSCode_click

            With btnGetNewSMSCode
                .ID = "btnGetNewSMSCode"
                .CommandName = "GetNewSMSCode"
                '.Text = ctx.Translate("::LT_S0390") ' "New Code"
                '.ToolTip = ctx.Translate("::LT_S0391") ' "Get a new security code"
                .Text = "::LT_S0390" ' "New Code"
                .ToolTip = "::LT_S0391" ' "Get a new security code"
            End With
            AddHandler btnGetNewSMSCode.Click, AddressOf GetNewSMSCode_click

        End Sub

        Private Sub IAWLogin_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            Select Case CurrentPage
                Case "Login"
                    Me.Logincontainer.DefaultButton = Me.btnHiddenLogin.ID
                Case "SendTo"
                    Me.SendToContainer.DefaultButton = Me.btnHiddenSendToMsg.ID
                Case "SMS"
                    Me.SMScontainer.DefaultButton = Me.btnHiddenSubmitSMSCode.ID
            End Select
        End Sub
#End Region

#Region "create child controls"
        Protected Overrides Sub CreateChildControls()
            Dim TR As TableRow
            Dim TD As TableCell

            MyBase.Controls.Clear()

            ' ------------------------------------------------------------------------------------
            ' Login Page Container Controls
            ' ------------------------------------------------------------------------------------
            'header
            TR = New TableRow
            TD = New TableCell
            TD.ColumnSpan = "2"
            TD.Controls.Add(lblLogInHeader)
            TD.CssClass = "login_header"
            TR.Controls.Add(TD)
            Me.LoginTable.Rows.Add(TR)

            If Me.ActiveDirDomains IsNot Nothing Then
                'domain
                Dim domains() As String = Me.ActiveDirDomains
                BindDomains()
                If domains.Length > 1 Then
                    TR = New TableRow
                    TD = AddCell(TR, "login_label", lblDomain)
                    TD.ColumnSpan = 2
                    Me.LoginTable.Rows.Add(TR)

                    TR = New TableRow
                    TD = AddCell(TR, "login_field", ddlDomains)
                    TD.ColumnSpan = 2
                    Me.LoginTable.Rows.Add(TR)
                End If
            End If

            'username
            TR = New TableRow
            TR.VerticalAlign = VerticalAlign.Middle
            TD = AddCell(TR, "login_label", lblUsername)
            TD.ColumnSpan = 2
            Me.LoginTable.Rows.Add(TR)

            TR = New TableRow
            AddCell(TR, "login_icon", New LiteralControl("<i class='fa-solid fa-user'></i>"))
            TD = AddCell(TR, "login_field", txtUser)
            addRFV(TD, txtUser)
            Me.LoginTable.Rows.Add(TR)

            'password
            TR = New TableRow
            TR.VerticalAlign = VerticalAlign.Middle
            TD = AddCell(TR, "login_label", lblPassword)
            TD.ColumnSpan = 2
            Me.LoginTable.Rows.Add(TR)

            TR = New TableRow
            AddCell(TR, "login_icon", New LiteralControl("<i class='fa-solid fa-lock'></i>"))
            TD = AddCell(TR, "login_field", txtPassword)
            addRFV(TD, txtPassword)
            TD.Controls.Add(labPassword_eye)
            TD.Controls.Add(labPassword_noeye)
            Me.LoginTable.Rows.Add(TR)

            'failure text
            lblLoginFail.Visible = False
            TR = New TableRow
            TR.ID = "TR_failure"
            TD = AddCell(TR, "login_failure", lblLoginFail)
            TD.ColumnSpan = "2"
            TD.HorizontalAlign = HorizontalAlign.Center
            Me.LoginTable.Rows.Add(TR)

            TR = New TableRow
            TD = AddCell(TR, "login_btn", btnLogin)
            TD.ColumnSpan = 2
            TD.Controls.Add(btnHiddenLogin)
            Me.LoginTable.Rows.Add(TR)

            Me.LoginTable.Rows.Add(BlankRows(1))

            'recover password btn / login btn
            If ShowRecovery Then
                TR = New TableRow
                'TD = AddCell(TR, "", lblRecover)
                'TD.Controls.Add(New LiteralControl("&#160;&#160;"))
                TD = AddCell(TR, "", lnkRecover)
                TD.ColumnSpan = 2
                TD.HorizontalAlign = HorizontalAlign.Center
                Me.LoginTable.Rows.Add(TR)
            End If

            'If Not String.IsNullOrEmpty(PasswordRecoveryText) Then
            '    btnRecover.Text = PasswordRecoveryText
            '    btnRecover.Url = PasswordRecoveryUrl
            '    TD = AddCell(TR, "login_btn", btnRecover)
            '    TD.ColumnSpan = 2
            '    TD.Controls.Add(New LiteralControl("&#160;&#160;"))
            '    TD.Controls.Add(btnLogin)
            '    TD.Controls.Add(btnHiddenLogin)
            'Else
            '    AddCell(TR, "", Nothing)
            '    TD = AddCell(TR, "login_btn", btnLogin)
            '    TD.ColumnSpan = 2
            '    TD.Controls.Add(btnHiddenLogin)
            'End If
            'Me.LoginTable.Rows.Add(TR)

            'Me.LoginTable.Rows.Add(BlankRows(2))

            Logincontainer.Controls.Add(Me.LoginTable)
            Me.Controls.Add(Me.Logincontainer)

            ' ------------------------------------------------------------------------------------
            ' Add fields for selection of where to send MFA 
            ' ------------------------------------------------------------------------------------

            TR = New TableRow
            TD = New TableCell
            TD.ColumnSpan = "2"
            TD.Controls.Add(lblSendToHeader)
            TD.CssClass = "login_header"
            TR.Controls.Add(TD)
            Me.SendToTable.Rows.Add(TR)

            'Message sent to..
            TR = New TableRow
            TR.ID = "TR_SendToMsg"
            TD = AddCell(TR, "login_label", lblSendToMsg)
            TD.ColumnSpan = "2"
            TD.HorizontalAlign = HorizontalAlign.Left
            Me.SendToTable.Rows.Add(TR)

            ' radio button list of addresses
            TR = New TableRow
            TD = AddCell(TR, "login_field", rblAddresses)
            TD.ColumnSpan = "2"
            TD.HorizontalAlign = HorizontalAlign.Left
            Me.SendToTable.Rows.Add(TR)

            Me.SendToTable.Rows.Add(BlankRows(1))

            TR = New TableRow
            TD = AddCell(TR, "login_btn", btnSendToMsg)
            TD.ColumnSpan = "2"
            TD.Controls.Add(btnHiddenSendToMsg)
            Me.SendToTable.Rows.Add(TR)

            Me.SendToTable.Rows.Add(BlankRows(1))

            SendToContainer.Controls.Add(Me.SendToTable)
            Me.Controls.Add(Me.SendToContainer)

            ' ------------------------------------------------------------------------------------
            ' SMS Page
            ' ------------------------------------------------------------------------------------

            TR = New TableRow
            TD = New TableCell
            TD.ColumnSpan = "2"
            TD.Controls.Add(lblSMSMsgHeader)
            TD.CssClass = "login_header"
            TR.Controls.Add(TD)
            Me.SMSTable.Rows.Add(TR)

            Me.SMSTable.Rows.Add(BlankRows(1))

            'Message sent to..
            lblSMSMsg.Visible = False
            TR = New TableRow
            TR.ID = "TR_SMSMsg"
            TD = AddCell(TR, "", lblSMSMsg)
            TD.ColumnSpan = "2"
            TD.HorizontalAlign = HorizontalAlign.Center
            Me.SMSTable.Rows.Add(TR)

            Me.SMSTable.Rows.Add(BlankRows(1))

            ' Authentication Code
            TR = New TableRow
            AddCell(TR, "login_label", lblSMSCode)
            TD = AddCell(TR, "login_field", txtSMSCode)
            addRFV(TD, txtSMSCode)
            Me.SMSTable.Rows.Add(TR)

            'failure text
            lblSMSFail.Visible = False
            TR = New TableRow
            TR.ID = "TR_SMSFail"
            TD = AddCell(TR, "login_failure", lblSMSFail)
            TD.ColumnSpan = "2"
            TD.HorizontalAlign = HorizontalAlign.Center
            Me.SMSTable.Rows.Add(TR)

            Me.SMSTable.Rows.Add(BlankRows(2))

            TR = New TableRow
            TD = New TableCell
            TD.Controls.Add(btnSubmitSMSCode)
            TD.Controls.Add(btnHiddenSubmitSMSCode)
            TR.Controls.Add(TD)

            TD = New TableCell
            TD.Controls.Add(btnGetNewSMSCode)
            TR.Controls.Add(TD)
            Me.SMSTable.Rows.Add(TR)

            Me.SMSTable.Rows.Add(BlankRows(2))

            SMScontainer.Controls.Add(Me.SMSTable)
            Me.Controls.Add(Me.SMScontainer)

            Logincontainer.Visible = CurrentPage = "Login"
            SendToContainer.Visible = CurrentPage = "SendTo"
            SMScontainer.Visible = CurrentPage = "SMS"

            ' add javascript to enable show/hide password
            AddJavascript()
        End Sub

        Private Sub addRFV(ByVal td As TableCell, ByVal ctrl As Control)
            Dim rfv As New RequiredFieldValidator
            rfv.Text = "*"
            rfv.SkinID = "login"
            rfv.Display = ValidatorDisplay.Static
            rfv.EnableClientScript = True
            rfv.ControlToValidate = ctrl.ID
            td.Controls.Add(rfv)
        End Sub

        Private Sub BindDomains()
            Dim domains() As String = Me.ActiveDirDomains
            Me.ddlDomains.DataSource = domains
            Me.ddlDomains.DataBind()
        End Sub

        Private Sub AddJavascript()
            Dim script As String

            If Not Page.ClientScript.IsClientScriptBlockRegistered("showhidejs") Then
                script = "function TogglePassword() {" &
                         "  var pwd = $ID('" + txtPassword.ClientID + "');" &
                         "  var eye = $ID('" + labPassword_eye.ClientID + "');" &
                         "  var noeye = $ID('" + labPassword_noeye.ClientID + "');" &
                         "  eye.style.display = 'none';" &
                         "  noeye.style.display = 'none';" &
                         "  if (pwd.type == 'password') {" &
                         "    pwd.type = 'text';" &
                         "    noeye.style.display = 'inline-block';" &
                         "  } else {" &
                         "    pwd.type = 'password';" &
                         "    eye.style.display = 'inline-block';" &
                         "  }" &
                         "}"
                Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "showhidejs", script, True)
            End If
        End Sub

#End Region

#Region "click handlers"
        ''' <summary>
        ''' Handles login button click
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub LoginButton_click(ByVal sender As Object, ByVal e As EventArgs)
            AttemptLogin()
        End Sub

        Private Sub SendToMsg_click(ByVal sender As Object, ByVal e As EventArgs)
            StartSMSCode(rblAddresses.SelectedValue)
        End Sub

        Private Sub SubmitSMSCode_click(ByVal sender As Object, ByVal e As EventArgs)

            Dim Result As String = ""
            lblSMSFail.Visible = False

            If SMSCode = "" Then Return

            lblSMSFail.Text = ctx.Translate("::LT_S0392") ' "Incorrect Security Code"

            Using DB As New IawDB
                DB.ClearParameters()
                DB.AddParameter("@Action", "TEST")
                DB.AddParameter("@UserRef", ctx.user_ref)
                DB.AddParameter("@Pin", SMSCode)
                DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
                DB.CallNonQueryStoredProc("dbo.dbp_mfa")
                Result = DB.GetParameter("@Result")

                If Result = "NO" Then
                    lblSMSFail.Visible = True
                    Return
                End If

                ' if it's not 'YES' then we have an error of some kind in form of x|message
                If Result <> "YES" Then
                    lblSMSFail.Text = Split(Result, "|")(1)
                    lblSMSFail.Visible = True
                    Return
                End If

                ' code worked so...
                FormsAuthentication.SetAuthCookie(Me.UserName, False)
                Me.Page.Response.Redirect(Me.DestinationPageUrl, False)
            End Using

        End Sub

        Private Sub GetNewSMSCode_click(ByVal sender As Object, ByVal e As EventArgs)
            Dim Result As String = ""
            Dim desttype As String
            Dim dest As String
            Dim txt As String

            txtSMSCode.Text = ""
            lblSMSMsg.Visible = False
            lblSMSFail.Visible = False
            Using DB As New IawDB
                DB.ClearParameters()
                DB.AddParameter("@Action", "LOGIN")
                DB.AddParameter("@UserRef", ctx.user_ref)
                DB.AddParameter("@Pin", "")
                DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
                DB.CallNonQueryStoredProc("dbo.dbp_mfa")
                Result = DB.GetParameter("@Result")

                If Not Result.StartsWith("YES|") Then ' if not YES, then must be an error of some kind 
                    lblSMSFail.Text = Split(Result, "|")(1)
                    lblSMSFail.Visible = True
                Else
                    desttype = Split(Result, "|")(1)
                    dest = Split(Result, "|")(2)
                    txt = SawLang.Translate("::LT_S0301") + " <br/>" ' A code has been sent to

                    Select Case desttype
                        Case "01", "02"
                            lblSMSMsg.Text = txt + SawLang.Translate("::LT_S0180") + " " + SawUtil.MaskEmail(dest) ' Email Address
                        Case "03"
                            lblSMSMsg.Text = txt + SawLang.Translate("::LT_S0299") + "  " + SawUtil.MaskEmail(dest) ' Mobile number ending with
                    End Select

                    lblSMSMsg.Visible = True
                End If
            End Using
        End Sub

#End Region

#Region "helpers"
        ''' <summary>
        ''' Attempt to login
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub AttemptLogin()
            lblLoginFail.Visible = False

            If ((Me.Page Is Nothing) OrElse Me.Page.IsValid) Then
                Dim args2 As New AuthenticateEventArgs
                args2.Authenticated = False
                'raise the event
                Me.OnAuthenticate(args2)

                If args2.Authenticated Then
                    If MFAActive And MFAHours And Not SystemAdminUser Then
                        ' ok, so user and password are ok, we now need to ask for a code sent to them via either SMS or email
                        ' will call dbp_mfa to determine whether prompt needed

                        Select Case IsMFARequired
                            Case "YES"
                                SendToSMSPrompt()
                                Return
                            Case "NO"

                            Case Else
                                lblLoginFail.Visible = True
                                Return
                        End Select
                    End If
                    FormsAuthentication.SetAuthCookie(Me.UserName, False)
                    Me.Page.Response.Redirect(Me.DestinationPageUrl, False)
                Else
                    Me.OnLoginFailed(EventArgs.Empty)
                    'If (Me.FailureAction = LoginFailureAction.RedirectToLoginPage) Then
                    '    FormsAuthentication.RedirectToLoginPage("loginfailure=1")
                    'End If   

                    lblLoginFail.Visible = True
                End If
            End If
        End Sub

#End Region

#Region "SMS Code"

        Private Sub SendToSMSPrompt()
            lblLoginFail.Visible = False

            ' no emails or mobile numbers set up
            If ContactDT.Rows.Count = 0 Then
                lblLoginFail.Text = SawUtil.GetMsg("WB_A0023")
                lblLoginFail.Visible = True
                Return
            End If

            ' only 1 place to send the message so don't ask which one
            If ContactDT.Rows.Count = 1 Or Not MFASelection Then
                StartSMSCode("")
                Return
            End If

            rblAddresses.DataSource = ContactDT
            rblAddresses.DataBind()

            CurrentPage = "SendTo"

            Logincontainer.Visible = False
            SendToContainer.Visible = True
            SMScontainer.Visible = False
        End Sub

        Private Sub StartSMSCode(Via As String)

            ' firstly, we call the stored proc which will return either 
            '  YES - display the code fields
            '  NO  - code field not required
            ' or an Error message

            Dim Result As String = ""
            Dim desttype As String
            Dim dest As String
            Dim txt As String

            lblLoginFail.Text = SawLang.Translate("::LT_S0385") ' Login Failed

            txtSMSCode.Text = ""
            lblSMSMsg.Visible = False

            Using DB As New IawDB
                DB.ClearParameters()
                DB.AddParameter("@Action", "LOGIN")
                DB.AddParameter("@UserRef", ctx.user_ref)
                DB.AddParameter("@Pin", "")
                If Via <> "" Then
                    DB.AddParameter("@Via", Via)
                End If
                DB.AddParameter("@Result", Result, ParameterDirection.Output, DbType.AnsiString, 100)
                DB.CallNonQueryStoredProc("dbo.dbp_mfa")
                Result = DB.GetParameter("@Result")
            End Using

            If Result = "NO" Then ' Pin screen not required
                FormsAuthentication.SetAuthCookie(Me.UserName, False)
                Me.Page.Response.Redirect(Me.DestinationPageUrl, False)
                Return
            End If

            If Not Result.StartsWith("YES|") Then ' if not YES, then must be an error of some kind 
                lblLoginFail.Text = Split(Result, "|")(1)
                lblLoginFail.Visible = True
                Return
            End If

            desttype = Split(Result, "|")(1)
            dest = Split(Result, "|")(2)
            txt = SawLang.Translate("::LT_S0301") + " <br/>" 'A code has been sent to

            Select Case desttype
                Case "01", "02"
                    lblSMSMsg.Text = txt + SawLang.Translate("::LT_S0180") + " " + SawUtil.MaskEmail(dest) ' Email address
                Case "03"
                    lblSMSMsg.Text = txt + SawLang.Translate("::LT_S0299") + "  " + SawUtil.MaskEmail(dest) ' Mobile number ending with
            End Select

            lblSMSMsg.Visible = True

            ' Now we have to display the fields to get the authentication code field
            CurrentPage = "SMS"

            Logincontainer.Visible = False
            SendToContainer.Visible = False
            SMScontainer.Visible = True

        End Sub
#End Region

#Region "Utils"
        Private Function BlankRows(ByVal rows As Integer) As TableRow
            Dim TR As New TableRow
            Dim TD As TableCell

            For i As Integer = 1 To rows
                TD = New TableCell
                TD.Text = "&#160;&#160;"
                TR.Controls.Add(TD)
            Next

            Return TR
        End Function

        Private Function AddCell(ByVal TR As TableRow, ByVal CssClass As String, ByVal control As Control) As TableCell
            Dim TD As New TableCell
            If control IsNot Nothing Then
                TD.Controls.Add(control)
            End If
            TD.CssClass = CssClass
            TR.Controls.Add(TD)
            Return TD
        End Function

        Function MaskEmail(ByVal s As String) As String
            Dim Pattern As String = "(?<=.).(?=[^@]*?.@)|(?:(?<=@.)|(?!^)\G(?=[^@]*$)).(?=.*\.)"
            If s.Trim.Length = 0 Then Return SawLang.Translate("::LT_S0340") ' Error, empty value
            If Not s.Contains("@") Then
                ' probably dealing with a phone number, so just return the last 3 digits
                If s.Length < 3 Then Return s
                Return Strings.Right(s, 3)
            End If
            Return Regex.Replace(s, Pattern, "*")
        End Function

#End Region

    End Class

End Namespace
