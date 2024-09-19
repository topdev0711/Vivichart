
<%@ Application Language="VB"  %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="System.Attribute" %>
<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text.Encoder" %>

<script runat="server">

    Private Shared DBConnectionOk As Boolean = True

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        SawDiag.Purge()
        SawDiag.Indent = 0
        SawDiag.LogVerbose("Application_Start")
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        SawDiag.Indent = 0
        SawDiag.LogVerbose("Application_End")
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)

        'only log errors with an inner exception, for some reason there is a persistent file not found error       
        Dim ex As Exception = Server.GetLastError()

        SawDiag.LogError("Application_Error")
        If ex IsNot Nothing Then
            SawDiag.LogError(ex.ToString)
            LogException.HandleException(ex, False)
        Else
        End If
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)

        'If HttpContext.Current.Request.FilePath.ToLower.EndsWith("default.aspx") Then Return

        SawDiag.LogVerbose("Session_Start - Start")
        SawDiag.Indent += 1

        setClient()
        setCulture()
        setDateFormat()
        Session("DBDateFormat") = "yyyy-MM-dd HH:mm:ss"
        'set session timeout

        ' If not licensed, tell the user
        If ctx.ApplicationWebType = WebType.iWebUnlicensed Then

            SawDiag.Indent -= 1
            SawDiag.LogVerbose("Session_Start - Error not licensed")

            Response.Redirect(Request.ApplicationPath + "/general/not_licensed.aspx")
            Return
        End If

        HttpContext.Current.Session.Timeout = ctx.siteConfigDef("timeout_interval", "20")
        CheckVersion()
        clearViewState()

        SawDiag.Indent -= 1
        SawDiag.LogVerbose("Session_Start - End")
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        SawDiag.LogVerbose("Session_End")
    End Sub

    Protected Sub Application_BeginRequest(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim request As HttpRequest = CType(sender.request, HttpRequest)
        Dim extension As String = Path.GetExtension(request.FilePath)

        If extension <> ".aspx" And extension <> ".ashx" Then
            SawDiag.LogVerbose("Application_Request : " + request.FilePath)
            Return
        End If

        ' If request.FilePath.ToLower.EndsWith("default.aspx") Then Return

        SawDiag.LogVerbose("Application_BeginRequest START : " + request.FilePath)
        SawDiag.Indent += 1

        FirstRequest.Initialise()

        CheckDbConn()

        If request.RawUrl.ToLower.Contains("nexturl=") Then
            Dim url As String = request.RawUrl.ToLower
            url = request.RawUrl.Remove(request.RawUrl.IndexOf("nexturl=") + 8)
            Dim escapedStr As String = request.RawUrl.Substring(request.RawUrl.IndexOf("nexturl=") + 8)

            'if new session and resolution has been appended to the url
            If escapedStr.Contains("&resolution") Then
                Dim res As String = escapedStr.Substring(escapedStr.IndexOf("&resolution"))
                escapedStr = escapedStr.Remove(escapedStr.IndexOf("&resolution") - 1)
                escapedStr = Uri.EscapeDataString(escapedStr) + res
            Else
                escapedStr = Uri.EscapeDataString(Uri.UnescapeDataString(escapedStr))
            End If

            HttpContext.Current.RewritePath(url + escapedStr)
        End If

        'decrypt the qs
        If ctx.item("cmd") IsNot Nothing Then
            Dim qs As String = SawUtil.decrypt(ctx.item("cmd"))
            If qs = "" Then
                SawDiag.LogError("Logged out due to URL change attempt")
                SawDiag.Indent -= 1

                SawUtil.Log("Logged out due to URL change attempt")
                ctx.logout()
            End If

            Dim QueryStringParms As New List(Of String)
            For Each nameValePair As String In qs.Split("&")
                Dim val() As String = nameValePair.Split("=")
                If val.Length = 2 Then
                    HttpContext.Current.Items(val(0).ToLower) = Server.UrlDecode(val(1))
                    QueryStringParms.Add(Server.UrlDecode(val(0)))
                Else
                    HttpContext.Current.Items(val(0).ToLower) = String.Empty
                End If
            Next
            If querystringparms.Count > 0 Then
                HttpContext.Current.Items("querystringparms") = QueryStringParms
            End If
        End If

        SawDiag.Indent -= 1
        SawDiag.LogVerbose("Application_BeginRequest END : " + request.FilePath)

    End Sub

    Protected Sub Application_EndRequest(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim request As HttpRequest = CType(sender.request, HttpRequest)
        Dim extension As String = Path.GetExtension(request.FilePath)
        If extension <> ".aspx" And extension <> ".ashx" Then Return

        'If request.FilePath.ToLower.EndsWith("default.aspx") Then Return

        SawDiag.LogVerbose("Application_EndRequest : " + request.FilePath)

        If DBConnectionOk Then
            CheckDBConnections()
        End If
    End Sub

    Sub Application_AcquireRequestState(ByVal sender As Object, ByVal e As EventArgs)
        If ctx.Request.Url.ToString.ToLower.Contains("login.aspx") Then

            ' if the url contains client, then use it to lookup the branding
            ctx.session("client_id") = Nothing

            If ctx.item("client") IsNot Nothing Then
                Dim ResDR As System.Data.DataRow = IawDB.execGetDataRow("select client_id, language_ref from dbo.clients where client_id = @p1", ctx.item("client").ToString)
                If ResDR IsNot Nothing Then
                    ctx.session("client_id") = ResDR("client_id")
                    ctx.session("language") = ResDR("language_ref")
                End If
            End If

            If ctx.item("user") IsNot Nothing Then
                Dim ResDR As System.Data.DataRow = IawDB.execGetDataRow("select client_id, language_ref from dbo.suser where user_ref = @p1", ctx.item("user").ToString)
                If ResDR IsNot Nothing Then
                    ctx.session("client_id") = ResDR("client_id")
                    ctx.session("language") = ResDR("language_ref")
                End If
            End If

        End If
    End Sub

    Private Sub clearViewState()
        'clear out any entries from the db viewstate where the timestamp
        'is has expired, ie timestamp < now - session timeout
        Dim ls_sql As String
        Dim timeOut As Integer = HttpContext.Current.Session.Timeout + 2
        ls_sql = "DELETE FROM web_viewstate " +
                 " WHERE vsTimestamp < DATEADD(mi, -" + timeOut.ToString + ", getdate()) "
        SawUtil.SQLNonQuery(ls_sql, True)
    End Sub

    Private Sub CheckDBConnections()
        If ctx.GetOpenConnections > 0 Then
            'write to web log with the aspx page and process if a connection has been left open
            Dim ls_sql As String
            ls_sql = "EXEC dbp_web_add_log_message @type='02', @text=@p1, @overwrite=1"
            IawDB.execNonQuery(ls_sql, ctx.url.LocalPath)
#If debug Then
            dim ex as New Exception("A database connection has been left open. <br>" + SawDBConnList.GetDBConnList())
            LogException.HandleException(ex,True)
#End If

        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub PageModule_PreInit(ByVal sender As Object, ByVal e As PageEventArgs)
        SawDiag.LogVerbose("PageModule_PreInit START")

        Dim p As Page = e.Page

        'check the webtype
        Dim ApplicationWebType As WebTypeAttribute = CType(GetCustomAttribute(p.GetType, GetType(WebTypeAttribute)), WebTypeAttribute)
        If CType(Application("WebType"), WebType) <> WebType.iWebCore Then
            'check the WebType if its set
            If ApplicationWebType IsNot Nothing Then
                If Not ApplicationWebType.isAccessible Then
                    'go to the access denied page
                    SawDiag.LogError("PageModule_PreInit - You are trying to access a page that is not available to this application")
                    ctx.accessDenied("You are trying to access a page that is not available to this application", p.AppRelativeVirtualPath)
                End If
            Else
                'WebType is not iWeb and there is no WebTypeAttribute therefore deny access
                SawDiag.LogError("PageModule_PreInit - You are trying to access a page that is not available to this application")
                ctx.accessDenied("You are trying to access a page that is not available to this application", p.AppRelativeVirtualPath)
            End If
        End If

        ' Check request page request is for the current session
        If Not Request.Path.Contains("access_denied") And Not Request.Path.Contains("login") Then
            If ctx.item(constants.userref) IsNot Nothing Then
                If ctx.item(constants.userref) <> ctx.user_ref Then
                    Dim str As String = String.Format("Attempt to use old URL : attempted user [{0}] - current user [{1}]", ctx.item(constants.userref), ctx.user_ref)
                    SawUtil.Log(str)
                    SawDiag.LogError(str)
                    ctx.logout()
                End If
            End If
        End If

        'assign theme
        Dim theme As String = ctx.siteConfig("theme").ToString
        Dim useMaster As Boolean
        p.EnableEventValidation = False
        p.MaintainScrollPositionOnPostBack = False

        'check if master page is required!!
        Dim Attr As Attribute = GetCustomAttribute(p.GetType, GetType(MasterPageRequiredAttribute))
        Dim masterAttr As MasterPageRequiredAttribute = CType(Attr, MasterPageRequiredAttribute)
        If masterAttr IsNot Nothing AndAlso Not masterAttr.masterRequired Then
            p.MasterPageFile = ""
        Else
            If Not String.IsNullOrEmpty(theme) Then
                If File.Exists(Server.MapPath("~/" + theme + "Master.master")) Then
                    p.MasterPageFile = "~/" + theme + "Master.master"
                    useMaster = True
                Else
                    p.MasterPageFile = "~/IngenWebMaster.master"
                    useMaster = True
                End If
            Else
                p.MasterPageFile = "~/IngenWebMaster.master"
                useMaster = True
            End If
        End If

        If Not String.IsNullOrEmpty(theme) Then
            p.Theme = theme
        Else
            p.Theme = "Silver"
        End If

        SawDiag.LogVerbose("PageModule_PreInit END")
    End Sub

    ''' <summary>
    ''' Handles global page load events
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Sub PageModule_Load(ByVal sender As Object, ByVal e As PageEventArgs)
        SawDiag.LogVerbose("PageModule_Load START")

        'fix for web menus in chrome/safari        
        Dim p As Page = e.Page
        If p.Request.UserAgent.IndexOf("AppleWebKit") > 0 Then
            p.Request.Browser.Adapters.Clear()
        End If
        If Request.ServerVariables("http_user_agent").IndexOf("Safari", StringComparison.CurrentCultureIgnoreCase) <> -1 Then
            p.ClientTarget = "uplevel"
        End If
        If Request.ServerVariables("http_user_agent").IndexOf("Chrome", StringComparison.CurrentCultureIgnoreCase) <> -1 Then
            p.ClientTarget = "uplevel"
        End If

        SawDiag.LogVerbose("PageModule_Load END")
    End Sub

    Sub PageModule_LoadComplete(ByVal sender As Object, ByVal e As PageEventArgs)
        SawDiag.LogVerbose("PageModule_LoadComplete START")
        If e.Page.Master IsNot Nothing Then
            showButtonBar(e.Page)
            disableEnterKeyPress(e.Page)
            showMenu(e.Page)
            'showLoginStatus(e.Page)
        End If
        SawDiag.LogVerbose("PageModule_LoadComplete END")
    End Sub

    'Private Sub showLoginStatus(ByVal aPage As Page)
    '    If aPage.Form IsNot Nothing Then
    '        Dim show As Boolean
    '        Dim Attr As Attribute = GetCustomAttribute(aPage.GetType, GetType(LoginStatusRequiredAttribute))
    '        Dim showAttr As LoginStatusRequiredAttribute = CType(Attr, LoginStatusRequiredAttribute)
    '        If showAttr Is Nothing Then
    '            show = True
    '        Else
    '            show = showAttr.loginStatusRequired
    '        End If

    '        Dim ls As LoginStatus
    '        Dim prop As PropertyInfo = aPage.Master.GetType.GetProperty("loginStatus")
    '        If prop IsNot Nothing Then
    '            ls = prop.GetGetMethod.Invoke(aPage.Master, Nothing)
    '            If ls IsNot Nothing Then ls.Visible = show
    '        End If
    '    End If

    'End Sub

    Private Sub showMenu(ByVal aPage As Page)
        If aPage.Form IsNot Nothing Then
            Dim show As Boolean
            Dim Attr As Attribute = GetCustomAttribute(aPage.GetType, GetType(MenuRequiredAttribute))
            Dim showAttr As MenuRequiredAttribute = CType(Attr, MenuRequiredAttribute)
            If showAttr Is Nothing Then
                show = True
            Else
                show = showAttr.menuRequired
            End If

            Dim mnu As Menu
            Dim prop As PropertyInfo = aPage.Master.GetType.GetProperty("menu")
            If prop IsNot Nothing Then
                mnu = prop.GetGetMethod.Invoke(aPage.Master, Nothing)
                If mnu IsNot Nothing Then mnu.Visible = show
            End If
        End If
    End Sub

    Private Sub disableEnterKeyPress(ByVal aPage As Page)
        If aPage.Form IsNot Nothing Then
            Dim disable As Boolean
            Dim Attr As Attribute = GetCustomAttribute(aPage.GetType, GetType(DisableEnterKeyAttribute))
            Dim disableAttr As DisableEnterKeyAttribute = CType(Attr, DisableEnterKeyAttribute)
            If disableAttr Is Nothing Then
                disable = True
            Else
                disable = disableAttr.disableEnterKey
            End If
            If disable Then
                ctx.Sys_Application_Add_load(aPage, "disableEnterKeyPress", "$addHandler(document,'keydown',PreventEnter);")
            End If
        End If
    End Sub

    Private Sub showButtonBar(ByVal aPage As Page)
        Dim visible As Boolean = True
        Dim Attr As Attribute = GetCustomAttribute(aPage.GetType, GetType(ButtonBarRequiredAttribute))
        Dim buttonBarAttr As ButtonBarRequiredAttribute = CType(Attr, ButtonBarRequiredAttribute)
        If buttonBarAttr IsNot Nothing Then visible = buttonBarAttr.buttonBarRequired

        Dim bb As HtmlGenericControl
        Dim prop As PropertyInfo = aPage.Master.GetType.GetProperty("divButtonBarArea")
        If prop IsNot Nothing Then
            bb = prop.GetGetMethod.Invoke(aPage.Master, Nothing)
            If bb IsNot Nothing Then bb.Visible = visible
        End If
    End Sub

    Private Sub CheckDbConn()
        If Not SawDB.CanConnectToDB Then
            HttpContext.Current.Trace.Warn("Site Settings Path", HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath + "/SiteSettings.config"))
            Response.Clear()
            Response.Write("There is a problem with the connection to the database.  Please contact your technical support team")
            Response.Flush()
            Response.End()
            DBConnectionOk = False
        Else
            DBConnectionOk = True
        End If
    End Sub

    Private Sub setClient()
        SawDiag.LogVerbose("setClient() START")

        Dim strName As String
        Session("client") = CType(IawDB.execScalar("SELECT client_number FROM licensee"), String).Trim
        strName = CType(IawDB.execScalar("SELECT LICENSEE_NAME FROM dbo.Licensee WITH (NOLOCK)"), String).Trim
        If (strName.IndexOf("(") > -1) Then
            strName = strName.Substring(0, strName.IndexOf("("))
        End If
        Session("clientname") = strName.Trim

        SawDiag.LogVerbose("setClient() END")
    End Sub

    Private Sub setDateFormat()
        SawDiag.LogVerbose("setDateFormat() START")

        Dim ls_date_order As String = String.Empty
        Dim lc_char, lc_prev As Char
        Dim ls_date_format As String = Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower
        Session("time_format") = "HH:mm"

        ' build a real date format string
        For Each lc_char In ls_date_format
            If lc_char <> lc_prev Then
                Select Case lc_char
                    Case "d", "m", "y"
                        If ls_date_order <> "" Then ls_date_order += "/"
                        Select Case lc_char
                            Case "d"
                                ls_date_order += "dd"
                            Case "m"
                                ls_date_order += "MM"
                            Case "y"
                                ls_date_order += "yyyy"
                        End Select
                End Select
                lc_prev = lc_char
            End If
        Next
        Session("date_format") = ls_date_order

        SawDiag.LogVerbose("setDateFormat() END")
    End Sub

    Private Sub setCulture()
        SawDiag.LogVerbose("setCulture() START")
        Dim ls_language As String

        ' unless use_language is set to true, we default to GBR

        If ctx.siteConfigDef("use_language", "false").ToString = "false" Then
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-gb")
        Else
            Try
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Request.UserLanguages(0))
            Catch ex As Exception
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB")
            End Try
        End If
        ls_language = Thread.CurrentThread.CurrentCulture.Name

        'End If
        'HttpContext.Current.Response.Cookies.Add(New HttpCookie("Cul", ls_language))

        Session("CultureName") = Thread.CurrentThread.CurrentCulture.Name
        Session("Culture") = Thread.CurrentThread.CurrentCulture

        Dim Lang As String '= Thread.CurrentThread.CurrentCulture.ThreeLetterISOLanguageName().ToUpper
        Select Case Left(ls_language, 2).ToLower
            Case "en"
                Lang = "GBR"
            Case "fr"
                Lang = "FRA"
            Case "de"
                Lang = "DEU"
            Case "es"
                Lang = "SPA"
            Case "it"
                Lang = "ITA"
            Case "pt"
                Lang = "POR"
            Case "nl"
                Lang = "NLD"
            Case Else
                Lang = "GBR"
        End Select
        Session("language") = Lang

        ' check that the language is listed in qlang.. if not, add it.

        'Dim ls_sql As String = "IF NOT EXISTS (SELECT TOP 1 1
        '                                         FROM dbo.qlang WITH (NOLOCK) 
        '                                        WHERE language_ref = @p1) 
        '                           INSERT dbo.QLANG (language_ref,language_name,font_family,culture)
        '                           VALUES (@p1,@p2,'Latin',@p3)"
        'IawDB.execNonQuery(ls_sql, Lang, Thread.CurrentThread.CurrentCulture.NativeName, Session("Culture").ToString)

        SawDiag.LogVerbose("setCulture() END")
    End Sub

    Private Sub CheckVersion()
        SawDiag.LogVerbose("CheckVersion() START")
        If Not Version.IsValidVersion Then
            Response.Redirect(Request.ApplicationPath + "/general/wrong_version.aspx")
        Else
            Session("version") = "ok"
        End If
        SawDiag.LogVerbose("CheckVersion() END")
    End Sub

    Class FirstRequest
        Private Shared s_InitialisedAlready As Boolean = False
        Private Shared s_lock As New Object()

        Public Shared Sub Initialise()
            If s_InitialisedAlready Then Return

            If Not SawDB.CanConnectToDB Then Return

            SawDiag.LogVerbose("FirstRequest.Initilise Start")

            SyncLock s_lock
                If Not s_InitialisedAlready Then

                    ' Code that runs on application startup
                    'Using DB As New IawDB
                    '    SqlCacheDependencyAdmin.DisableTableForNotifications(DB.comConenctionString(), "language_text")
                    '    SqlCacheDependencyAdmin.DisableTableForNotifications(DB.comConenctionString(), "cache_list")
                    '    SqlCacheDependencyAdmin.DisableNotifications(DB.comConenctionString())
                    'End Using
                    SiteSettings.Initialise()

                    ctx.ApplicationWebType = Licence.Check
                    If ctx.ApplicationWebType <> WebType.iWebUnlicensed Then
                        s_InitialisedAlready = True
                    End If
                End If

            End SyncLock

            SawDiag.LogVerbose("FirstRequest.Initilise End")
        End Sub

    End Class

</script>
