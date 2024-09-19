
Imports System.Reflection
Imports System.Attribute
Imports System.Web.Configuration

Public Class ctx

#Region "HttpContext.Current properties"

    Public Shared Property Cookie(ByVal cookieName As String) As String
        Get
            If HttpContext.Current.Request.Cookies(cookieName) Is Nothing Then Return String.Empty
            Return HttpContext.Current.Request.Cookies(cookieName).Value
        End Get
        Set(ByVal value As String)
            Dim cookie As New HttpCookie(cookieName, value)
            cookie.Expires = Date.Today.AddMonths(6)
            HttpContext.Current.Response.Cookies.Remove(cookieName)
            HttpContext.Current.Response.Cookies.Add(cookie)
        End Set
    End Property

    Public Shared ReadOnly Property username() As String
        Get
            Return session("user_name").ToString
        End Get
    End Property

    Public Shared ReadOnly Property user_ref() As String
        Get
            Return session("user_ref").ToString
        End Get
    End Property

    Public Shared ReadOnly Property cache() As Cache
        Get
            Dim c As Cache = HttpContext.Current.Cache
            If c Is Nothing Then Throw New Exception("Problem with the current cache")
            Return c
        End Get
    End Property

    ''' <summary>
    ''' Returns the current changeId from the AspNet_SqlCacheTablesForChangeNotification table
    ''' based upon the table name passed in.  The value is cached per request.
    ''' </summary>
    ''' <value></value>
    ''' <returns>0 if table name not present, otherwise return the changeId changeId</returns>
    ''' <remarks>The value is cached per request to stop multiple cache inserts on a single request.</remarks>
    Public Shared ReadOnly Property changeId(ByVal tablename As String) As Integer
        Get
            Dim id As Integer = 0
            Dim value As String = ctx.item(tablename + "changeId")
            If String.IsNullOrEmpty(value) Then
                Dim ls_sql As String
                ls_sql = "SELECT IsNull(Max(changeId) ,0)  " _
                       + "  FROM dbo.AspNet_SqlCacheTablesForChangeNotification " _
                       + " WHERE tableName = @p1"

                id = IawDB.execScalar(ls_sql, tablename)
                ctx.item(tablename + "changeId") = id
                Return id
            Else
                If Integer.TryParse(ctx.item(tablename + "changeId"), id) Then
                    Return id
                End If
                Return 0
            End If
        End Get
    End Property

    Public Shared ReadOnly Property current() As HttpContext
        Get
            Return HttpContext.Current
        End Get
    End Property

    Public Shared ReadOnly Property Request() As HttpRequest
        Get
            Return HttpContext.Current.Request
        End Get
    End Property

    Public Shared ReadOnly Property Response() As HttpResponse
        Get
            Return HttpContext.Current.Response
        End Get
    End Property

    Public Shared ReadOnly Property filePath() As String
        Get
            Return HttpContext.Current.Request.FilePath
        End Get
    End Property

    Public Shared Property item(ByVal key As String) As Object
        Get
            Dim obj As Object = HttpContext.Current.Items(key.ToLower)
            If obj Is Nothing Then
                obj = HttpContext.Current.Request(key)
            End If
            Return obj
        End Get
        Set(ByVal value As Object)
            HttpContext.Current.Items(key.ToLower) = value
        End Set
    End Property

    Public Shared ReadOnly Property master() As IngenWebMaster
        Get
            Dim cntrl As Control = HttpContext.Current.Handler
            Dim mp As IngenWebMaster = CType(cntrl.Page.Master, IngenWebMaster)
            Return mp
        End Get
    End Property

    Public Shared ReadOnly Property contentPlaceHolder() As ContentPlaceHolder
        Get
            Return CType(ctx.master, stub_IngenWebMaster).contentPlaceHolder
        End Get
    End Property

    Public Shared ReadOnly Property contentID() As String
        Get
            Return ctx.contentPlaceHolder.ClientID
        End Get
    End Property

    Public Shared ReadOnly Property SessionId() As String
        Get
            If HttpContext.Current.Session IsNot Nothing Then
                Return HttpContext.Current.Session.SessionID
            End If
            Return String.Empty
        End Get
    End Property

    Public Shared WriteOnly Property javaScriptFileForHead() As String
        Set(ByVal value As String)
            CType(ctx.master, stub_IngenWebMaster).JavaScriptForHead.javaScriptFiles.Add(ctx.virtualDir + "/" + value)
        End Set
    End Property
    Public Shared WriteOnly Property javaScriptForHead() As String
        Set(ByVal value As String)
            CType(ctx.master, stub_IngenWebMaster).JavaScriptForHead.javaScriptText.Add(value)
        End Set
    End Property
    Public Shared WriteOnly Property cssFileForHead() As String
        Set(ByVal value As String)
            CType(ctx.master, stub_IngenWebMaster).JavaScriptForHead.cssFiles.Add(ctx.virtualDir + "/" + value)
        End Set
    End Property
    Public Shared WriteOnly Property cssTextForHead() As String
        Set(ByVal value As String)
            CType(ctx.master, stub_IngenWebMaster).JavaScriptForHead.cssText.Add(value)
        End Set
    End Property

    Public Shared ReadOnly Property RootURL As String
        Get
            Dim request As HttpRequest = HttpContext.Current.Request
            Dim appUrl As String = HttpRuntime.AppDomainAppVirtualPath

            If appUrl <> "/" Then
                appUrl += "/"
            End If

            Dim baseUrl As String = request.Url.Scheme + "://" + request.Url.Host
            If Not request.Url.IsDefaultPort Then
                baseUrl += ":" & request.Url.Port.ToString()
            End If
            baseUrl += appUrl

            Return baseUrl
        End Get
    End Property

    Public Shared ReadOnly Property rawUrl() As String
        Get
            Return HttpContext.Current.Request.RawUrl
        End Get
    End Property

    Public Shared ReadOnly Property server() As HttpServerUtility
        Get
            Return HttpContext.Current.Server
        End Get
    End Property

    Public Shared Property session(ByVal key As String) As Object
        Get
            Try
                If HttpContext.Current.Session Is Nothing Then Return String.Empty
                If HttpContext.Current.Session(key) IsNot Nothing Then
                    Return HttpContext.Current.Session(key)
                End If
            Catch ex As Exception
            End Try
            Return String.Empty
        End Get
        Set(ByVal value As Object)
            If HttpContext.Current.Session IsNot Nothing Then
                HttpContext.Current.Session(key) = value
            End If
        End Set
    End Property

    Public Shared ReadOnly Property themeGraphicsDir() As String
        Get
            Return "~/app_themes/" + ctx.theme + "/graphics/"
        End Get
    End Property

    Public Shared ReadOnly Property HTMLGraphicsDir() As String
        Get
            Return Request.ApplicationPath + "/app_themes/" + ctx.theme + "/graphics/"
        End Get
    End Property

    Public Shared ReadOnly Property GraphicsDir As String
        Get
            Return Request.ApplicationPath + "/graphics/"
        End Get
    End Property

    Public Shared ReadOnly Property theme() As String
        Get
            Dim p As Page
            p = CType(HttpContext.Current.CurrentHandler, Page)
            Return p.Theme
        End Get
    End Property

    Public Shared ReadOnly Property UserIPAddress() As String
        Get
            Dim sIPAddress As String = ctx.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
            If String.IsNullOrEmpty(sIPAddress) Then
                Return ctx.Request.ServerVariables("REMOTE_ADDR")
            Else
                Dim ipArray As String() = sIPAddress.Split(",")
                Try
                    Return ipArray(ipArray.Length - 1)
                Catch ex As Exception
                    Return ipArray(0)
                End Try
            End If
            Return ""
        End Get
    End Property

    Public Shared ReadOnly Property url() As Uri
        Get
            Return HttpContext.Current.Request.Url
        End Get
    End Property

    Public Shared ReadOnly Property virtualDir() As String
        Get
            Return HttpContext.Current.Request.ApplicationPath
        End Get
    End Property

    Public Shared ReadOnly Property Page() As Page
        Get
            Dim cntrl As Control = HttpContext.Current.Handler
            Return CType(cntrl.Page, Page)
        End Get
    End Property

    Public Shared ReadOnly Property isFormInsert() As Boolean
        Get
            Return Not String.IsNullOrEmpty(ctx.item("add"))
        End Get
    End Property

    Public Shared ReadOnly Property isFormUpdate() As Boolean
        Get
            Return String.IsNullOrEmpty(ctx.item("add"))
        End Get
    End Property

    Public Shared ReadOnly Property LoginExtensionDays() As Integer
        Get
            Return CType(session("login_extension_days"), Integer)
        End Get
    End Property

#End Region

#Region "Session Pay Period Details"
    Public Shared Property PayGroup() As String
        Get
            Return session("pay_group")
        End Get
        Set(ByVal value As String)
            session("pay_group") = value
        End Set
    End Property
    Public Shared Property PayYear() As Integer
        Get
            Return session("pay_year")
        End Get
        Set(ByVal value As Integer)
            session("pay_year") = value
        End Set
    End Property
    Public Shared Property PayPeriod() As Integer
        Get
            Return session("pay_period")
        End Get
        Set(ByVal value As Integer)
            session("pay_period") = value
        End Set
    End Property
#End Region

#Region "authenticated user & anly codes"

    Public Shared ReadOnly Property DateFormat() As String
        Get
            Return ctx.session("date_format")
        End Get
    End Property

    Public Shared ReadOnly Property DBDateFormat() As String
        Get
            Return ctx.session("DBDateFormat")
        End Get
    End Property

    Public Shared ReadOnly Property isSysAdm() As Boolean
        Get
            Try
                Return IawDB.execScalar("select administrator from dbo.suser where user_ref = @p1", ctx.user_ref) = "1"
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property

    Public Shared ReadOnly Property user() As IAW.IawPrincipal
        Get
            If HttpContext.Current.Request.IsAuthenticated Then
                If TypeOf HttpContext.Current.User Is IAW.IawPrincipal Then
                    Return CType(HttpContext.Current.User, IAW.IawPrincipal)
                End If
                Return Nothing
            Else : Return Nothing
            End If
        End Get
    End Property

    Public Shared ReadOnly Property IsViewAllowed() As Boolean
        Get
            Return ctx.user.hasRightInProcess(ctx.process(), IAW.RightTo.view)
        End Get
    End Property
    Public Shared ReadOnly Property IsInsertAllowed() As Boolean
        Get
            Return ctx.user.hasRightInProcess(ctx.process(), IAW.RightTo.insert)
        End Get
    End Property

    Public Shared ReadOnly Property IsUpdateAllowed() As Boolean
        Get
            Return ctx.user.hasRightInProcess(ctx.process(), IAW.RightTo.update)
        End Get
    End Property

    Public Shared ReadOnly Property IsDeleteAllowed() As Boolean
        Get
            Return ctx.user.hasRightInProcess(ctx.process(), IAW.RightTo.delete)
        End Get
    End Property

    Public Shared ReadOnly Property networkLogon() As String
        Get
            Dim logon As String
            logon = ctx.current.User.Identity.Name
            If String.IsNullOrEmpty(logon) Then Return String.Empty
            Return logon.Substring(logon.LastIndexOf("\") + 1)
        End Get
    End Property

    Public Shared ReadOnly Property networkAuthLogon() As String
        Get
            Dim logon As String
            logon = ctx.current.Request.ServerVariables("Auth_User")
            If String.IsNullOrEmpty(logon) Then Return String.Empty
            Return logon.Substring(logon.LastIndexOf("\") + 1)
        End Get
    End Property

    Public Shared ReadOnly Property IsAuthenticated() As Boolean
        Get
            'if the authentication method is not network logon/NTLM/windows then return HttpContext.Current.Request.IsAuthenticated
            'otherwise check what the contect,user object if it is a windows principal then user not

            If ctx.authentication_mode = AuthenticationMode.Windows And Not TypeOf HttpContext.Current.User Is IAW.IawPrincipal Then
                Return False
            Else
                Select Case True
                    Case ctx.Authentication_Forms,
                         ctx.Authentication_ActiveDirectory,
                         ctx.Authentication_SingleSignOn
                        Return HttpContext.Current.Request.IsAuthenticated
                    Case ctx.Authentication_NetworkLogon
                        If TypeOf HttpContext.Current.User Is IAW.IawPrincipal Then
                            Return True
                        Else
                            Return False
                        End If
                End Select
            End If


        End Get
    End Property

    Public Shared ReadOnly Property authentication_mode() As AuthenticationMode
        Get
            Dim AuthSettings As AuthenticationSection = System.Web.Configuration.WebConfigurationManager.GetSection("system.web/authentication")
            Return AuthSettings.Mode
        End Get
    End Property

    Public Shared ReadOnly Property AppSetting(key As String) As String
        Get
            Dim Str As String = ""
            Try
                Str = ConfigurationManager.AppSettings(key)
            Catch ex As Exception
                Str = ""
            End Try
            Return Str
        End Get
    End Property

#End Region

#Region "config"

    Public Shared ReadOnly Property siteConfigDef(ByVal entry As String, ByVal DefaultValue As String) As String
        Get
            If siteConfig(entry) <> "" Then
                Return siteConfig(entry)
            Else
                Return DefaultValue
            End If
        End Get
    End Property

    Public Shared ReadOnly Property siteConfig() As NameValueCollection
        Get
            'Return ConfigurationManager.GetSection("siteSettings")
            Return SiteSettings.Settings
        End Get
    End Property

    Public Shared ReadOnly Property Authentication_Forms() As Boolean
        Get
            Return siteConfigDef("authentication_method", "") = "Forms"
        End Get
    End Property
    Public Shared ReadOnly Property Authentication_NetworkLogon() As Boolean
        Get
            Return siteConfigDef("authentication_method", "") = "Network Logon"
        End Get
    End Property
    Public Shared ReadOnly Property Authentication_ActiveDirectory() As Boolean
        Get
            Return siteConfigDef("authentication_method", "") = "Active Directory"
        End Get
    End Property
    Public Shared ReadOnly Property Authentication_SingleSignOn() As Boolean
        Get
            Return siteConfigDef("authentication_method", "") = "Single Sign On"
        End Get
    End Property

#End Region

#Region "menu and sitemap"
    Public Shared Property ApplicationWebType() As WebType
        Get
            Dim o As Object = HttpContext.Current.Application("WebType")
            If o Is Nothing Then
                HttpContext.Current.Application("WebType") = WebType.iWebCore
                o = HttpContext.Current.Application("WebType")
            End If
            Return CType(o, WebType)
        End Get
        Set(ByVal value As WebType)
            HttpContext.Current.Application("WebType") = value
        End Set
    End Property

    Public Shared ReadOnly Property siteMapNode(ByVal mnu_ref As String) As IAWSiteMapNode
        Get
            Dim smNode As SiteMapNode
            Dim cntrl As Control = master
            Dim prop As PropertyInfo

            prop = cntrl.GetType.GetProperty("siteMapNode")
            Dim getSiteMapNode As MethodInfo = prop.GetGetMethod
            smNode = getSiteMapNode.Invoke(cntrl, New Object() {mnu_ref})
            Return smNode
        End Get
    End Property

    Public Shared ReadOnly Property urlFromMenuItem(ByVal mnu_ref As String) As String
        Get
            Dim node As SiteMapNode = ctx.siteMapNode(mnu_ref)
            If node IsNot Nothing Then
                Return node.Url()
            Else : Return String.Empty
            End If
        End Get
    End Property

    Public Shared Sub rebuildMenu()
        Dim mp As IngenWebMaster = master
        Dim siteMenu As IAWSiteMapProvider
        Dim getMenu As MethodInfo = mp.GetType.GetProperty("siteMapProvider").GetGetMethod
        siteMenu = getMenu.Invoke(mp, Nothing)
        siteMenu.rebuildMenu()
        mp.menu.DataBind()
    End Sub

    Public Shared Function GetMenuText(process_ref As String) As String
        Dim mp As IngenWebMaster = master
        Dim siteMenu As IAWSiteMapProvider
        Dim getMenu As MethodInfo = mp.GetType.GetProperty("siteMapProvider").GetGetMethod
        siteMenu = getMenu.Invoke(mp, Nothing)
        Return siteMenu.GetMenuText(process_ref)
    End Function

#End Region

#Region "redirect"

    ''' <summary>
    ''' Redirects the request the 'Access Denied' page, passing the message through
    ''' </summary>
    ''' <param name="aMessage"></param>
    ''' <remarks></remarks>
    Public Shared Sub accessDenied(ByVal aMessage As String)
        If Not String.IsNullOrEmpty(aMessage) Then
            ctx.redirect(HttpContext.Current.Request.ApplicationPath + "/general/" + "access_denied.aspx" + "?msg=" + aMessage)
        Else
            ctx.redirect(HttpContext.Current.Request.ApplicationPath + "/general/" + "access_denied.aspx")
        End If
    End Sub

    ''' <summary>
    ''' Redirects the request the 'Access Denied' page, passing the message and the url that was trying to be reached
    ''' </summary>
    ''' <param name="aMessage"></param>
    ''' <param name="aUrl"></param>
    ''' <remarks></remarks>
    Public Shared Sub accessDenied(ByVal aMessage As String, ByVal aUrl As String)
        If Not String.IsNullOrEmpty(aMessage) And Not String.IsNullOrEmpty(aUrl) Then
            ctx.redirect(HttpContext.Current.Request.ApplicationPath + "/general/" + "access_denied.aspx" + "?url=" + aUrl + "&msg=" + aMessage)
        Else
            ctx.redirect(HttpContext.Current.Request.ApplicationPath + "/general/" + "access_denied.aspx")
        End If
    End Sub

    Public Shared Sub redirectOnError()
        HttpContext.Current.Response.Redirect("~/ApplicationError.aspx", True)
    End Sub

    Public Shared Sub redirectOnError(ByVal url As String)
        HttpContext.Current.Response.Redirect(SawUtil.encryptQuery(url, True))
    End Sub

    Public Shared Sub redirect(ByVal url As String)
        HttpContext.Current.Response.Redirect(SawUtil.encryptQuery(url, True))
    End Sub

    Public Shared Sub redirect(ByVal mnu_ref As String, ByVal isMnu As Boolean)
        Dim response As HttpResponse = HttpContext.Current.Response
        Dim smNode As SiteMapNode = Nothing
        Dim cntrl As Control = master
        Dim prop As PropertyInfo

        If Not String.IsNullOrEmpty(mnu_ref) Then
            prop = cntrl.GetType.GetProperty("siteMapNode")
            Dim getSiteMapNode As MethodInfo = prop.GetGetMethod
            smNode = getSiteMapNode.Invoke(cntrl, New Object() {mnu_ref})
            cntrl = Nothing
        End If

        If smNode IsNot Nothing Then
            response.Redirect(smNode.Url)
        Else
            StartPage.Redirect()
        End If

    End Sub

    Public Shared Function GetMenuURL(ByVal mnu_ref As String) As String
        Dim response As HttpResponse = HttpContext.Current.Response
        Dim smNode As SiteMapNode = Nothing
        Dim cntrl As Control = master
        Dim prop As PropertyInfo

        If Not String.IsNullOrEmpty(mnu_ref) Then
            prop = cntrl.GetType.GetProperty("siteMapNode")
            Dim getSiteMapNode As MethodInfo = prop.GetGetMethod
            smNode = getSiteMapNode.Invoke(cntrl, New Object() {mnu_ref})
            cntrl = Nothing
        End If

        If smNode IsNot Nothing Then Return smNode.Url

        Return StartPage.GetUrl
    End Function

    Public Shared Sub Reload()
        Response.Redirect(ctx.rawUrl)
    End Sub

#End Region

#Region "trace"

    Public Shared Sub trace(ByVal message As String)
        HttpContext.Current.Trace.Warn(message)
    End Sub

    Public Shared Sub trace(ByVal category As String, ByVal message As String)
        HttpContext.Current.Trace.Warn(category, message)
    End Sub

#End Region

#Region "Db connections"
    Public Shared Function DbConnOpened() As Integer
        Dim count As Integer
        count = ctx.item("DbConn")
        ctx.item("DbConn") = count + 1
    End Function

    Public Shared Function DbConnClosed() As Integer
        Dim count As Integer
        count = ctx.item("DbConn")
        ctx.item("DbConn") = count - 1
    End Function

    Public Shared Function GetOpenConnections() As Integer
        Dim count As Integer
        count = ctx.item("DbConn")
        Return count
    End Function
#End Region

    Public Shared Sub BannerEnabled(isEnabled As Boolean)
        If Not master Is Nothing Then
            master.EnableHeaderButtons(isEnabled) ' Or False, depending on the condition
        End If
    End Sub

    Public Shared ReadOnly Property buttonBar() As ButtonBar
        Get
            Dim bb As ButtonBar
            If master IsNot Nothing Then
                Dim getButtonBar As MethodInfo = master.GetType.GetProperty("buttonBar").GetGetMethod
                bb = getButtonBar.Invoke(master, Nothing)
                Return bb
            End If
            Return Nothing
        End Get
    End Property

    Public Shared ReadOnly Property buttonBarID() As String
        Get
            Dim bBar As ButtonBar = ctx.buttonBar
            If bBar Is Nothing Then
                Return ""
            Else
                Return bBar.ClientID
            End If
        End Get
    End Property

    Public Shared ReadOnly Property process() As String
        Get
            Dim o As Object = ctx.item(constants.processref)
            If o Is Nothing Then
                o = ctx.Request.QueryString(constants.processref)
            End If

            Return CStr(o)
        End Get
    End Property

    Public Shared ReadOnly Property role() As String
        Get
            Return ctx.item("role")
        End Get
    End Property

    Public Shared Sub logout()
        logout(Nothing)
    End Sub

    Public Shared Sub logout(ByVal msg As String)
        session("role") = Nothing
        If Not String.IsNullOrEmpty(msg) Then
            session("loginMsg") = msg
        End If
        HttpContext.Current.Response.Redirect("~/logout.aspx")

    End Sub

    Public Shared Sub LogError(ByVal ex As System.Exception)
        LogException.HandleException(ex, False, "")
    End Sub

    Public Shared Sub LogError(ByVal ex As System.Exception, ByVal redirect As Boolean, ByVal url As String)
        LogException.HandleException(ex, redirect, url)
    End Sub

#Region "client & language"
    Public Shared Function Translate(ByVal text As String) As String
        Return SawLang.Translate(text)
    End Function

    Public Shared ReadOnly Property languageCode() As String
        Get
            'Return HttpContext.Current.Response.Cookies("Cul").Value
            Return ctx.session("language")
        End Get
    End Property

    Public Shared ReadOnly Property clientID() As String
        Get
            Return ctx.session("client").ToString
        End Get
    End Property
#End Region

#Region "dirty page handling"

    'controls which will cause dirty page handling
    Public Shared Sub AddCheckControl(ByVal aPage As Page, ByVal clientID As String)
        Dim csm As ClientScriptManager = aPage.ClientScript
        Dim js As String
        js = "DirtyPage.AddCheckControl('" + clientID + "');"
        ScriptManager.RegisterStartupScript(aPage, aPage.GetType(), clientID, js, True)
    End Sub

    'controls which will NOT cause dirty page handling
    Public Shared Sub AddNoCheckField(ByVal aPage As Page, ByVal clientID As String)
        Dim csm As ClientScriptManager = aPage.ClientScript
        Dim js As String
        js = "DirtyPage.AddNoCheckField('" + clientID + "');"
        ScriptManager.RegisterStartupScript(aPage, aPage.GetType(), "nocheck" + clientID, js, True)
    End Sub

    Public Shared Sub ClearPageIsDirty()
        Dim p As Page = ctx.Page
        Dim csm As ClientScriptManager = p.ClientScript
        If p.IsPostBack Then
            'if it is a postback then clear the hidden field that sets the pageisdirty property on the client
            Dim js As String
            js = "<script type='text/javascript'>" &
                     "   DirtyPage.setPageIsDirty(false);" &
                     "</script>"
            ScriptManager.RegisterStartupScript(p, p.GetType(), "ClearPageIsDirty", js, False)
        End If
    End Sub

    Public Shared ReadOnly Property UseDirtyPageHandling() As Boolean
        Get
            Dim Attr As Attribute = GetCustomAttribute(ctx.Page.GetType, GetType(DirtyPageHandlingAttribute))
            Dim dirtyPage As DirtyPageHandlingAttribute = CType(Attr, DirtyPageHandlingAttribute)
            If dirtyPage IsNot Nothing Then
                Return dirtyPage.dirtyPageHandlingRequired
            End If
            Return True
        End Get
    End Property

    Public Shared ReadOnly Property DisableFormFocus() As Boolean
        '///the default if not disabled
        Get
            Dim Attr As Attribute = GetCustomAttribute(ctx.Page.GetType, GetType(DisableFormFocusAttribute))
            Dim disable As DisableFormFocusAttribute = CType(Attr, DisableFormFocusAttribute)
            If disable IsNot Nothing Then
                Return disable.disableFormFocus
            End If
            Return False
        End Get
    End Property


#End Region

#Region "error / exception"

    Public Shared Sub ClientOnlyPage()
        ctx.RaiseException("This is a bespoke page and is not available for general use.", "This is a bespoke page and is not available for general use.")
    End Sub

    ''' <summary>
    ''' Raises a new exception using the SystemMessage for the exception message.
    ''' UserMessage will be put into session("UserErrorMessage"), this can then be picked up on the error screen
    ''' If UserMessage start with :: then the message string will be retrived from qmessaget
    ''' </summary>
    ''' <param name="SystemMessage"></param>
    ''' <param name="UserMessage"></param>
    ''' <remarks></remarks>
    Public Shared Sub RaiseException(ByVal SystemMessage As String, ByVal UserMessage As String)
        ctx.session("UserErrorMessage") = UserMessage
        Throw New Exception(SystemMessage)
    End Sub

#End Region

#Region "javascript"

    ''' <summary>
    ''' Registers the javascript as a startup script and attachs it the Sys.Application.add_load event handler
    ''' </summary>
    ''' <param name="ctrl">The control/page issuing the script</param>
    ''' <param name="scriptId">Unique script identifier</param>
    ''' <param name="javascript">The javascript to be attached</param>
    ''' <remarks></remarks>
    Public Shared Sub Sys_Application_Add_load(ByVal ctrl As Control, ByVal scriptId As String, ByVal javascript As String)
        Dim js As New StringBuilder
        js.AppendLine("Sys.Application.add_load(function(e){")
        js.AppendLine(javascript)
        js.AppendLine("});")
        ScriptManager.RegisterStartupScript(ctrl, ctrl.GetType, scriptId, js.ToString, True)
    End Sub

#End Region

End Class

