Imports System.ComponentModel
Imports System.IO
Imports System.Threading


Partial Class IngenWebMaster
    Inherits stub_IngenWebMaster

    Private _buttonBar As ButtonBar

#Region "properties"

    Public Overrides ReadOnly Property buttonBar() As ButtonBar
        Get
            Return _buttonBar
        End Get
    End Property
    Public Overrides ReadOnly Property divButtonBarArea() As HtmlGenericControl
        Get
            Return divButtonBar
        End Get
    End Property

    Public Overrides ReadOnly Property siteMapProvider() As IAWSiteMapProvider
        Get
            Return (CType(SiteMapDataSource1.Provider, IAWSiteMapProvider))
        End Get
    End Property

    Public Overrides ReadOnly Property siteMapNode(ByVal key As String) As SiteMapNode
        Get
            If Menu1 Is Nothing Then Return Nothing
            If key Is Nothing Then Return Nothing
            Return siteMapProvider.FindSiteMapNode(key)
        End Get
    End Property

    Public Overrides ReadOnly Property menu() As Menu
        Get
            Return Menu1
        End Get
    End Property

    Public Overrides ReadOnly Property divBannerLoggedInDetails As HtmlGenericControl
        Get
            Return divBannerLoggedInDetails1
        End Get
    End Property

    Public Overrides ReadOnly Property contentPlaceHolder() As System.Web.UI.WebControls.ContentPlaceHolder
        Get
            Return Me.ContentPlaceHolder1
        End Get
    End Property

    Public Overrides ReadOnly Property JavaScriptForHead() As stub_header_js
        Get
            Return Me.header_js
        End Get
    End Property

    Public Overrides ReadOnly Property Head() As HtmlControls.HtmlHead
        Get
            Return Me.Head1
        End Get
    End Property

    Public ReadOnly Property divScrollPos() As HiddenField
        Get
            Return hdnDivSrollPos
        End Get
    End Property

    Public Property enableForceBrand() As Boolean
        Get
            If String.IsNullOrEmpty(ctx.session("enableForceBrand()")) Then Return False
            Return CBool(ctx.session("enableForceBrand()"))
        End Get
        Set(value As Boolean)
            ctx.session("enableForceBrand()") = value
        End Set
    End Property

#End Region

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        _buttonBar = New ButtonBar
        phButtonBar.Controls.Add(_buttonBar)

        lnkLogout.ToolTip = ctx.Translate("::LT_S0274") ' LT_S0274 - Log Out

        lblBannerMessage.Text = SawUtil.GetMsg("WB_A0021")
        lblBannerMessage.Visible = SawUtil.CheckMsg("WB_A0021")

        'master_helplink.CssClass = "staticHelpClass"
        Page.Header.DataBind()      ' for ResolveURL bindings

        ' inject the vars into the page header
        If Not ctx.Request.Url.ToString.Contains("client_settings.aspx") Or Not enableForceBrand Then
            Session("forceBrand") = Nothing
        End If

        Dim clientid As Integer = 0
        If Not Integer.TryParse(ctx.session("client_id"), clientid) Then
            clientid = 0
        End If

        If ctx.item("user") IsNot Nothing Then
            ctx.cssTextForHead = ChartBranding.GenerateVars(clientid, ctx.item("user"))
        Else
            ctx.cssTextForHead = ChartBranding.GenerateVars(clientid)
        End If

    End Sub

    Protected Overrides Sub CreateChildControls()
        MyBase.CreateChildControls()
        'need to do always, as attributes are not remembered on the viewstate

        If Not ctx.IsAuthenticated Then
            Me.divBannerLoggedInDetails.Visible = False
            MenuVisible(False)

            Me.lnkMessages.Visible = False
            Me.lnkMessages.Enabled = False
        Else
            'Me.MasterMenuBar.Visible = True

            lnkMessages.NavigateUrl = SawUtil.encryptQuery(ctx.virtualDir + "/messaging/MsgMail.aspx?pr=messages", True)

            If ctx.user.hasRightInProcess("messages", IAW.RightTo.view) Then
                lnkMessages.Visible = True

                lnkMessages.CssClass = lnkMessages.CssClass.Replace("blinker", "").Trim
                Dim i As Integer = IawDB.execScalar("Select count(1)
                                                       From dbo.msg M WITH (NOLOCK) 
                                                            Join dbo.msg_user MU WITH (NOLOCK) 
                                                              On MU.msg_id = M.msg_id 
                                                           WHERE MU.user_ref = @p1
                                                             And MU.folder_id = '01'
                                                             And MU.mu_seen Is null", ctx.user_ref)
                If i > 0 Then
                    lnkMessages.CssClass = lnkMessages.CssClass + " blinker"
                    If i = 1 Then
                        lnkMessages.ToolTip = String.Format(SawLang.Translate("::LT_S0275"), i) ' LT_S0275 - You have {0} unread message
                    Else
                        lnkMessages.ToolTip = String.Format(SawLang.Translate("::LT_S0276"), i) ' LT_S0276 - You have {0} unread messages
                    End If
                Else
                    lnkMessages.ToolTip = SawLang.Translate("::LT_S0277") ' LT_S0277 - You have no new messages
                End If
            Else
                lnkMessages.Visible = False
            End If

        End If

        If Not String.IsNullOrEmpty(ctx.item("bannerbar")) AndAlso ctx.item("bannerbar") = "no" Then
            MasterBannerBar.Visible = False
        End If

    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        PageTitle()

        If Not IsPostBack Then

            lnkLogout.ToolTip = ctx.Translate("::LT_S0274") ' LT_S0274 - Log Out

            If Not ctx.IsAuthenticated Then
                divBannerLoggedInDetails.Visible = False
                MenuVisible(False)
                Return
            End If

            lblProcessName.Text = ctx.GetMenuText(ctx.item(constants.processref))
            lblInfo.Text = String.Format("{0}", ctx.session("user_name"))

            Using DB As New IawDB
                Dim thePage As String = Path.GetFileName(Request.Path)

                DB.ClearParameters()
                DB.AddParameter("@Type", "99")
                DB.AddParameter("@Overwrite", "0")
                DB.AddParameter("@Text", thePage)
                DB.CallNonQueryStoredProc("dbo.dbp_web_add_log_message")
            End Using

            ' quick code to translate controls
            'LoopControls(Page.Controls)
        End If

    End Sub

    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)

        If Me.menu.Visible Then
            'http://blogs.msdn.com/jorman/archive/2006/02/06/526087.aspx
            'If you're using the new menu control that ships with ASP.NET 2.0 and SSL Termination/Acceleration, you will run into this issue.
            'The behavior the end users will see is a warning in the browser stating something similar to:
            '                    This page contains both secure and nonsecure items. 
            '                    Do you want to display the nonsecure items? 
            Dim req As HttpRequest = HttpContext.Current.Request
            If req.Url.Scheme.ToLower = "https" Then
                Dim js As String = String.Format("if(typeof {0}_Data !='undefined'){0}_Data.iframeUrl='{1}://{2}{3}{4}/blank.htm';", _
                                                 Me.menu.ClientID, _
                                                 req.Url.Scheme, _
                                                 req.Url.Host, _
                                                 ":" + req.Url.Port.ToString, _
                                                 req.ApplicationPath)

                ScriptManager.RegisterStartupScript(Me.Page, Me.GetType, "MenuHttpsWorkaround", js, True)
            End If
        End If

        MyBase.Render(writer)
    End Sub

    Private Sub PageTitle()
        Dim pTitle As String = ctx.siteConfig("browser_title").ToString

        If String.IsNullOrEmpty(pTitle) Then
            pTitle = "iawModels"
        End If
        pTitle += " "

        If Not ctx.IsAuthenticated Then
            Me.Page.Title = pTitle ' + Version.DisplayAppVer
            Return
        End If

        Dim processName As String = String.Empty

        processName = IawDB.execScalar("select process_name from dbo.process WITH (NOLOCK) where process_ref=@p1", ctx.item(constants.processref), "")

        pTitle += processName
        Me.Page.Title = pTitle
    End Sub

    Private Sub Menu1_MenuItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.MenuEventArgs) Handles Menu1.MenuItemDataBound
        ' special to change the root node to a picture (3 lines)
        If e.Item.DataPath = "m_main_menu" Then
            e.Item.Text = "&#9776;"
        Else
            Dim url As String = e.Item.NavigateUrl
            e.Item.NavigateUrl = SawUtil.AddUserToURL(url)
        End If
    End Sub

    Private Sub MenuVisible(show As Boolean)
        If show Then
            Menu1.CssClass = "MBItem"
        Else
            Menu1.CssClass = "MBItem hidden"
        End If
    End Sub

    Public Sub EnableHeaderButtons(isEnabled As Boolean)
        If isEnabled Then
            Menu1.CssClass = Menu1.CssClass.ClassRemove("noClick")
            lnkLogout.CssClass = lnkLogout.CssClass.ClassRemove("noClick")
            lnkMessages.CssClass = lnkMessages.CssClass.ClassRemove("noClock")
        Else
            Menu1.CssClass = Menu1.CssClass.ClassAdd("noClick")
            lnkLogout.CssClass = lnkLogout.CssClass.ClassAdd("noClick")
            lnkMessages.CssClass = lnkMessages.CssClass.ClassAdd("noClock")
        End If
    End Sub

End Class

