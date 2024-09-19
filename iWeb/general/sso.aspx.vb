Imports System.IO

<WebType(WebType.iWebCore), ButtonBarRequired(False), ProcessRequired(False), MenuRequired(False), DirtyPageHandling(False)> _
Partial Public Class sso
    Inherits stub_IngenWebPage

    Private Property FirstPAgeURL() As String
        Get
            Return ViewState("FirstPAgeURL")
        End Get
        Set(ByVal value As String)
            ViewState("FirstPAgeURL") = value
        End Set
    End Property

    Public Const AttributesSessionKey As String = "sso_session_key"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim isInResponseTo As Boolean = False
        Dim partnerIdP As String = Nothing
        Dim authContext As String = Nothing
        Dim userName As String = Nothing
        Dim attributes As IDictionary(Of String, String) = Nothing
        Dim targetUrl As String = Nothing
        Dim validated As SawAuth.Validate

        If Page.IsPostBack Then
            Return
        End If

        LogInfo("In sso.aspx")

        If Not ctx.Authentication_SingleSignOn Then
            ctx.accessDenied("The system is not configured for Single-Sign-On")
        End If

        Try
            'SAMLServiceProvider.ReceiveSSO(Request, isInResponseTo, partnerIdP, authContext, userName, attributes, targetUrl)
            If ctx.siteConfigDef("sso_provider_id", "") <> partnerIdP Then
                ctx.accessDenied("Authentication From Unknown Source")
            End If
            LogInfo("SSO User - " + userName)
        Catch ex As Exception
            LogInfo("Error Receiving SSO - " + ex.Message)
            LogException.LogIAWException(ex)
            ctx.accessDenied(ex.Message)
        End Try
        Session(AttributesSessionKey) = attributes

        validated = SawAuth.ValidateUser(userName, "^^^")

        Select Case validated
            Case SawAuth.Validate.failed, _
                 SawAuth.Validate.locked_out
                LogException.LogIAWException(New Exception("Authentication Failed for [" + userName + "]"))
                LogInfo("Failed Validation")
                ctx.accessDenied("")

            Case SawAuth.Validate.change_pwd,
                 SawAuth.Validate.change_qu,
                 SawAuth.Validate.ok
                LogInfo("Validated - Start at normal start page")
                FormsAuthentication.SetAuthCookie(userName, False)
                FirstPAgeURL = StartPage.GetUrl
                checkForAgreement()

            Case Else
                Throw New Exception(validated.ToString + " is not a recognised validation reason")
        End Select

    End Sub

    Protected Sub btn_agreement_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btn_agreement.Click
        ctx.redirect(FirstPAgeURL)
    End Sub

    Private Sub checkForAgreement()
        If Not IsPostBack Then
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
                ctx.redirect(FirstPAgeURL)
                Return
            End If
            writeAgreement(FullFilePath)
        Else
            ctx.redirect(FirstPAgeURL)
        End If

    End Sub

    Private Sub writeAgreement(ByVal filePath As String)
        Dim sr As New StreamReader(filePath)
        Dim agreement As String
        agreement = sr.ReadToEnd()
        Me.span_agreement.Controls.Add(New LiteralControl(agreement))
    End Sub

    Private Sub LogInfo(ByVal text As String)
        SawUtil.LogMsg("06", text)
    End Sub

End Class