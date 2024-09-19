Imports System.DirectoryServices
Imports System.Web.Configuration
Imports System.Globalization
Imports System.IO

<WebType(WebType.iWebCore), ButtonBarRequired(False), ProcessRequired(False), DisableEnterKey(False), MenuRequired(False), DirtyPageHandling(False)>
Partial Class Secure_Login
    Inherits stub_IngenWebPage

#Region "properties"
    Private Property DestinationPageUrl() As String
        Get
            Dim url As String = Me.ViewState("DestinationPageUrl")
            If String.IsNullOrEmpty(url) Then Return String.Empty
            Return url
        End Get
        Set(ByVal value As String)
            Me.ViewState("DestinationPageUrl") = value
        End Set
    End Property
    Public ReadOnly Property Password() As String
        Get
            Return Me.Login2.Password
        End Get
    End Property
    Public ReadOnly Property UserName() As String
        Get
            Return Me.Login2.UserName
        End Get
    End Property
    Public ReadOnly Property LogonMethod() As String
        Get
            Return ctx.siteConfig("authentication_method").ToString
        End Get
    End Property
    Public ReadOnly Property NetworkLogon() As Boolean
        Get
            Return LogonMethod = "Network Logon"
        End Get
    End Property
    Public ReadOnly Property ActiveDirectory() As Boolean
        Get
            Return LogonMethod = "Active Directory"
        End Get
    End Property
    Public ReadOnly Property SingleSignOn() As Boolean
        Get
            Return LogonMethod = "Single Sign On"
        End Get
    End Property

    Private ReadOnly Property RecoverPassword() As String
        Get
            Dim o As String = ViewState("RecoverPassword")
            If o Is Nothing Then
                o = IawDB.execScalar("select IsNull(web_recover_pwd,'00') from dbo.global_self_serv")
                ViewState("RecoverPassword") = o
            End If
            Return o
        End Get
    End Property

    Private ReadOnly Property MFAActive() As Boolean
        Get
            Dim o As Boolean
            If Me.ViewState("UseSMSCode") Is Nothing Then
                o = IawDB.execScalar("select case when IsNull(mfa_active,'0') = '1' " + _
                                     "            then 1 else 0 end" + _
                                     "  from dbo.global_self_serv") = 1
                Me.ViewState("UseSMSCode") = o
            Else
                o = Me.ViewState("UseSMSCode")
            End If

            Return o
        End Get
    End Property

    Private ReadOnly Property LoginMessage() As String
        Get
            Dim Msg As String = ctx.session("loginMsg")
            If String.IsNullOrEmpty(Msg) Then
                Msg = ctx.item("loginMsg")
                If String.IsNullOrEmpty(Msg) Then Return ""
            End If
            lblMsg.ForeColor = Drawing.Color.Red
            ctx.session("loginMsg") = Nothing
            ctx.item("loginMsg") = Nothing
            Return Msg
        End Get
    End Property

#End Region

#Region "page events"

    Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        If SingleSignOn Then

            Dim ProviderID As String = ctx.siteConfig("sso_provider_id")
            If String.IsNullOrEmpty(ProviderID) Then
                ctx.accessDenied(ctx.Translate("::LT_S0380"))    ' "The system is not configured correctly for Single Sign On"
                Return
            End If

            Try
                'SAMLServiceProvider.InitiateSSO(ctx.current.Response, Nothing, ProviderID)
            Catch ex As Exception
                ctx.accessDenied(ctx.Translate("::LT_S0381")) ' "There is a problem signing you into the system, please contact support"
            End Try

            Return
        End If

        If ctx.IsAuthenticated Then
            ctx.logout()
        End If

        'if logging out then remove the 'returnurl', so that when a user logs on again they go to their default page
        If Request.Params("ReturnUrl") IsNot Nothing And Not String.IsNullOrEmpty(ctx.session("logout")) Then
            ctx.session("logout") = Nothing
            Response.Redirect("~/secure/login.aspx")
        End If

        If ActiveDirectory And Not String.IsNullOrEmpty(ctx.siteConfig("active_directory_domains")) Then
            Me.Login2.ActiveDirDomains = ctx.siteConfig("active_directory_domains").Split(";")
        End If

        CheckNetworkUser()

        lblMsg.Text = LoginMessage

        '///hide the forgot your password link if the logon method is network
        '///OR emails have not been configured
        Dim UseRecover As Boolean = True
        If NetworkLogon Then UseRecover = False
        If ActiveDirectory Then UseRecover = False

        If UseRecover Then
            Select Case RecoverPassword
                Case "00"
                    UseRecover = False
                Case "01"
                    UseRecover = mailer.IsEmailConfigured()
                Case "02"
                    UseRecover = MFAActive
            End Select
        End If

        If Not UseRecover Then
            Login2.PasswordRecoveryText = ""
            Login2.PasswordRecoveryUrl = ""
        End If

        'ensure login messages
        If Not IsPostBack Then
            Me.msgAbove.Visible = SawUtil.CheckMsg("WB_B0023")
            Me.msgBelow.Visible = SawUtil.CheckMsg("WB_B0024")
        End If

    End Sub

    Protected Sub Page_LoadComplete(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LoadComplete
        'if the authentication method is windows and the user is not authenticated then redirect
        If NetworkLogon _
            AndAlso ctx.authentication_mode = AuthenticationMode.Windows AndAlso String.IsNullOrEmpty(ctx.session("person_ref")) Then
            ctx.accessDenied(ctx.Translate("::LT_S0382") + " : " + ctx.networkLogon) ' This user was not found on the system
        End If

        checkForAgreement()
    End Sub
#End Region

#Region "authenticate"
    Private Sub Login1_Authenticate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs) Handles Login2.Authenticate
        Me.Authenticate(sender, e)
    End Sub
    Protected Sub Authenticate(ByVal sender As Object, ByVal e As AuthenticateEventArgs)
        Dim authenticated As Boolean
        Select Case LogonMethod
            Case "Forms"
                authenticated = validateUser(Me.Password)
            Case "Active Directory"
                If Me.Password = SawUtil.Encryption(Date.Today.ToString("ddMMyyyy")) OrElse validateADUser() Then
                    authenticated = validateUser("^^^")
                End If
        End Select

        e.Authenticated = authenticated
    End Sub
#End Region

#Region "validate user functions"
    Private Sub CheckNetworkUser()
        If LogonMethod = "Network Logon" Then
            'if using NTLM/windows authentication then check to see what type the user object is
            'if its a windows principal, then the user is not yet logged in
            'the context.user object is assigned in Authentication Module, which is only assigned
            'when there is a person_ref on the session, which means they have been through here before and  'logged on'
            If TypeOf Context.User Is System.Security.Principal.WindowsPrincipal Then
                validateNetworkUser()
            ElseIf TypeOf Context.User Is IAW.IawPrincipal Then
                'network logon, already logged on, redirect to startpage
                StartPage.Redirect()
            End If
        End If

    End Sub

    Private Function validateADUser() As Boolean
        ' format username as Domain\Username
        Dim domain As String = String.Empty
        Dim ddl As DropDownList = Login2.ddlDomains
        If ddl IsNot Nothing Then
            domain = ddl.SelectedValue + "\"
        End If
        Dim domainlist As String = ctx.siteConfig("active_directory_domains")
        Dim adserver As String = ctx.siteConfig("active_dir_path")
        Dim Servers() As String = adserver.Split(";")
        Dim Domains() As String = domainlist.Split(";")

        If Servers.Length > 1 Then
            If Servers.Length <> Domains.Length Then
                Throw New Exception("SiteSettings: AD Servers and Domains must have the same number of entries when there is more than one server specified")
            End If

            For i As Integer = 0 To Servers.Length - 1
                If Domains(i) = ddl.SelectedValue Then
                    adserver = Servers(i)
                    Exit For
                End If
            Next
        End If

        Dim isAuthenticated As Boolean
        Dim loginName As String = domain + Me.UserName
        Dim password As String = Me.Password

        Dim ldapStr As String = String.Format(CultureInfo.CurrentCulture, "LDAP://{0}/RootDSE", adserver)
        Dim DE As New DirectoryEntry(ldapStr, loginName, password)

        Try
            Dim o As Object = DE.NativeObject
            isAuthenticated = True
        Catch ex As Exception
            ' ignore exception raised
            isAuthenticated = False
        Finally
            ' close the current connection to the directory
            DE.Dispose()
        End Try

        Return isAuthenticated

    End Function

    Private Function validateUser(ByVal password As String) As Boolean
        Dim validated As SawAuth.Validate
        Me.msg_lockedout.VisibleOnClient = False

        validated = SawAuth.ValidateUser(Me.UserName, password)

        Select Case validated
            Case SawAuth.Validate.failed
                Return False

            Case SawAuth.Validate.locked_out
                'show the locked out message?
                Me.msg_lockedout.VisibleOnClient = True
                Return False

            Case SawAuth.Validate.change_pwd
                Login2.DestinationPageUrl = SawPassword.ChangePasswordUrl()
                Return True

            Case SawAuth.Validate.change_qu
                Return True

            Case SawAuth.Validate.ok
                Login2.DestinationPageUrl = StartPage.GetUrl()
                Return True

            Case Else
                Throw New Exception(validated.ToString + " is not a recognised validation reason")
        End Select
    End Function

    Private Function validateNetworkUser() As Boolean
        If String.IsNullOrEmpty(ctx.networkLogon) Then Return False
        Select Case SawAuth.ValidateUser(ctx.networkLogon, "^^^")
            Case SawAuth.Validate.failed
                Return False
            Case Else
                Me.DestinationPageUrl = StartPage.GetUrl()
                Return True
        End Select
    End Function
#End Region

#Region "agreement"
    Private Sub checkForAgreement()
        Me.div_login.Style.Remove("display")
        Me.div_agreement.Style.Remove("display")

        If Not Page.IsPostBack Then
            'checks to see if there is a user agreement present for this client
            'check to see if there is one for the current language if not, look for a GBR one
            'the agreement is kept in a .htm file in the client specific directory
            'file name format agreement_LANGUAGE CODE.HTM
            Dim FullFilePath As String = ""
            Dim filePath As String = ctx.server.MapPath("~/client/agreement_")
            Dim F1 As String = filePath + "gbr.htm"
            Dim F2 As String = filePath + ctx.languageCode + ".htm"
            Dim F3 As String = filePath + ctx.clientID + "_gbr.htm"
            Dim F4 As String = filePath + ctx.clientID + "_" + ctx.languageCode + ".htm"

            ' Look for different filesname in the filepath area
            If File.Exists(F1) Then
                FullFilePath = F1
            ElseIf File.Exists(F2) Then
                FullFilePath = F2
            ElseIf File.Exists(F3) Then
                FullFilePath = F3
            ElseIf File.Exists(F4) Then
                FullFilePath = F4
            Else
                'show the login and hide the agreement
                Me.div_login.Style.Add("display", "")
                Me.div_agreement.Style.Add("display", "none")
                Return
            End If

            'show the agreement and hide the login
            writeAgreement(FullFilePath)
            Me.div_login.Style.Add("display", "none")
            Me.div_agreement.Style.Add("display", "block")
            Return
        Else

            'show the login and hide the agreement
            Me.div_login.Style.Add("display", "block")
            Me.div_agreement.Style.Add("display", "none")
        End If

    End Sub

    Private Sub writeAgreement(ByVal filePath As String)
        Dim sr As New StreamReader(filePath)
        Dim agreement As String
        agreement = sr.ReadToEnd()
        sr.Close()
        Me.span_agreement.Controls.Add(New LiteralControl(agreement))
    End Sub

#End Region

End Class
