Imports System.Globalization
Imports System.Threading

Public MustInherit Class stub_header_js
    Inherits System.Web.UI.UserControl
    Public MustOverride ReadOnly Property javaScriptFiles() As ArrayList
    Public MustOverride ReadOnly Property javaScriptText() As ArrayList
    Public MustOverride ReadOnly Property cssFiles() As ArrayList
    Public MustOverride ReadOnly Property cssText() As ArrayList
End Class


Public MustInherit Class stub_IngenWebPage
    Inherits System.Web.UI.Page

    ''' <summary>
    ''' Used by controls that need access to the Page ScriptManager
    ''' eg. the tooltip control, because it needs to emit javascript after an asych postback
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property AjaxScriptManager() As ScriptManager
        Get
            Return Nothing
        End Get
    End Property

    Public Overridable Property useSqlViewState() As Boolean
        Get
            If Me.ViewState("useSqlViewState") Is Nothing Then
                Return False
            End If
            Return Me.ViewState("useSqlViewState")
        End Get
        Set(ByVal value As Boolean)
            Me.ViewState("useSqlViewState") = value
        End Set
    End Property

    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        If Me.useSqlViewState Then
            Return ViewStateManager.LoadPageState(Me)
        Else
            Return MyBase.LoadPageStateFromPersistenceMedium()
        End If
    End Function

    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal state As Object)
        If Me.useSqlViewState Then
            ViewStateManager.SavePageState(Me, state)
        Else
            MyBase.SavePageStateToPersistenceMedium(state)
        End If
    End Sub

    Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        'check that this page is visible in PR Lite

        'adds a control that is used to store the background color to use when placing focus on a form field
        Me.Form.Controls.Add(New LiteralControl("<span id=""formFocus"" style=""display:none"" class=""formFocus"">used to hold bg color</span>"))
    End Sub

    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Context.Request.IsAuthenticated And Me.Master IsNot Nothing Then
            Dim cntrl As WebControl
            cntrl = Me.Master.FindControl("divBannerLoggedInDetails")
            If cntrl IsNot Nothing Then cntrl.Attributes.Add("style", "visibility:hidden")
            cntrl = Me.Master.FindControl("Menu1")
            If cntrl IsNot Nothing Then cntrl.Visible = False
        End If
    End Sub

    Protected Sub FieldFocus(ByVal ctrl As Control)
        ScriptManager.GetCurrent(Me.Page).SetFocus(ctrl)
    End Sub
    Protected Sub SetGrid(ByVal Grid As IAW.controls.IAWGrid, ByVal DT As DataTable)
        SetGrid(Grid, DT, True)
    End Sub
    Protected Sub SetGrid(ByVal Grid As IAW.controls.IAWGrid, ByVal DT As DataTable, ByVal SetRow As Boolean)
        Dim RowIndex As Integer

        RowIndex = Grid.SelectedIndex

        Grid.DataSource = DT
        Grid.DataBind()

        If DT.Rows.Count > 0 And SetRow Then
            If RowIndex = -1 Then
                Grid.SelectedIndex = 0
            Else
                If RowIndex >= DT.Rows.Count Then
                    Grid.SelectedIndex = DT.Rows.Count - 1
                Else
                    Grid.SelectedIndex = RowIndex
                End If
            End If
        Else
            Grid.SelectedIndex = -1
        End If
    End Sub

    Protected Function FilterDT(ByVal DT As DataTable, Filter As String) As DataTable
        Dim DV As New DataView(DT)
        DV.RowFilter = Filter
        Return DV.ToTable
    End Function

    Protected Sub SetDDLBPuptext(ByVal ddlb As DropDownList, ByVal DT As DataTable)
        SetDDLB(ddlb, DT, "pup_text", "return_value")
    End Sub

    Protected Sub SetDDLBPuptext(ByRef ddlb As DropDownList, table As String, column As String, Optional DefaultValue As String = "")
        SetDDLB(ddlb, SawUtil.GetPupText(table, column, False), "pup_text", "return_value")
        If DefaultValue <> "" Then
            SetDDLBValue(ddlb, DefaultValue)
        End If
    End Sub

    Protected Sub SetDDLB(ByVal ddlb As DropDownList, ByVal DT As DataTable, ByVal TextField As String, ByVal ValueField As String)
        ddlb.SetDatasource(DT, TextField, ValueField)
    End Sub

    Protected Sub SetDDLBValue(ByVal ddlb As DropDownList, ByVal Val As String)
        If ddlb.Items.Count = 0 Then Return
        If String.IsNullOrEmpty(Val) Then
            ddlb.SelectedIndex = 0
        End If
        For Each V As ListItem In ddlb.Items
            If V.Value = Val Then
                ddlb.SelectedValue = Val
                Return
            End If
        Next
        ddlb.SelectedIndex = 0
    End Sub

    Protected Function GetVal(ByVal DR As DataRow, ByVal Col As String, ByVal Def As String) As String
        Try
            If DR(Col) Is Nothing Then Return Def
            If DR(Col) Is System.DBNull.Value Then Return Def
            Return DR(Col)
        Catch ex As Exception
            Throw New Exception("Invalid column passed to GetVal [" + Col + "]")
        End Try
    End Function

End Class

Public MustInherit Class stub_IngenWebMaster
    Inherits System.Web.UI.MasterPage

    Public MustOverride ReadOnly Property buttonBar() As ButtonBar
    Public MustOverride ReadOnly Property divButtonBarArea() As HtmlGenericControl
    Public MustOverride ReadOnly Property siteMapProvider() As IAWSiteMapProvider
    Public MustOverride ReadOnly Property siteMapNode(ByVal key As String) As SiteMapNode
    Public MustOverride ReadOnly Property menu() As Menu
    'Public MustOverride ReadOnly Property loginStatus() As LoginStatus
    Public MustOverride ReadOnly Property divBannerLoggedInDetails As HtmlGenericControl
    Public MustOverride ReadOnly Property contentPlaceHolder() As ContentPlaceHolder
    Public MustOverride ReadOnly Property JavaScriptForHead() As stub_header_js
    Public MustOverride ReadOnly Property head() As System.Web.UI.HtmlControls.HtmlHead

    Protected Overrides Sub OnInit(ByVal e As EventArgs)
        MyBase.OnInit(e)

        ' Retrieve the culture ID from the session
        Dim cultureID As String = TryCast(Session("locale_ref"), String)
        If Not String.IsNullOrEmpty(cultureID) Then
            ' Create the CultureInfo object
            Dim cultureInfo As CultureInfo = CultureInfo.CreateSpecificCulture(cultureID)

            ' Set the culture and UI culture for the current thread
            Thread.CurrentThread.CurrentCulture = cultureInfo
            Thread.CurrentThread.CurrentUICulture = cultureInfo
        End If
    End Sub

    Protected Overrides Sub CreateChildControls()
        MyBase.CreateChildControls()
        AddHiddenFields()
        AddAttributes()
        RegisterStartUpScripts()
    End Sub

    'add attributes to page level html elements
    Protected Overridable Sub AddAttributes()
        Me.Page.Form.Attributes.Add("autocomplete", "off")
    End Sub

    'used for maintaining scroll position and form field focus
    Protected Overridable Sub AddHiddenFields()

        Dim hid As New HiddenField
        hid.ID = "__EDITING"
        Me.contentPlaceHolder.Controls.Add(hid)

        hid = New HiddenField
        hid.ID = "__SCROLLPOS"
        Me.contentPlaceHolder.Controls.Add(hid)

        hid = New HiddenField
        hid.ID = "__DIRTYPAGE"
        Me.contentPlaceHolder.Controls.Add(hid)

    End Sub


    Protected Overridable Sub RegisterStartUpScripts()
        Dim csm As ClientScriptManager = Me.Page.ClientScript

        'scroll pos
        Dim SavePos As String
        SavePos = "<script type='text/javascript'>" + _
                  "   var content = document.getElementById('div_content');" & _
                  "   content.onscroll = IAW.saveScrollPosition;" & _
                  " </script>"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "saveScroll", SavePos, False)
        

        If ctx.UseDirtyPageHandling Then
            'handle onsubmit() & onbeforeunload() for dirty page handling
            Dim onUnLoad As String
            onUnLoad = "DirtyPage.loadValues();" _
                 + "  window.onbeforeunload = DirtyPage.checkForm_onbeforeunload;"

            ScriptManager.RegisterStartupScript(Me.Page, MyBase.GetType(), "setSubmit2", onUnLoad, True)
            ScriptManager.RegisterOnSubmitStatement(Me.Page, MyBase.GetType(), "setSubmit", "return DirtyPage.checkForm_postback();")
        End If

        '///add on focus handlers first
        Dim focus As New StringBuilder
        focus.AppendLine("addOnFocusHandlers();")
        If Not ctx.DisableFormFocus Then
            'sets focus to first form field on page
            focus.AppendLine("setFocus();")
        End If
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "setFocus", focus.ToString, True)

        'element focus
        If (Me.Page.IsPostBack = True) Then
            Dim setPos As String
            setPos = "<script type='text/javascript'>" & _
                     "   IAW.setScrollPos(); "

            If Not ctx.DisableFormFocus Then
                setPos &= "   setFormFocus(); "
            End If
            setPos &= "</script>"

            ScriptManager.RegisterStartupScript(Me,Me.GetType(), "setScroll", setPos, False)
        End If

    End Sub

    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        'hide the controls on the menu bar, menubar=no is on the url

        If Not String.IsNullOrEmpty(ctx.item("menubar")) AndAlso ctx.item("menubar") = "no" Then
            Me.menu.Visible = False
            Me.divBannerLoggedInDetails.Visible = False
        End If

        If Not String.IsNullOrEmpty(ctx.item("buttonbar")) AndAlso ctx.item("buttonbar") = "no" Then
            Me.buttonBar.VisibleOnClient = False
        End If

        If Not ctx.IsAuthenticated Then
            Me.menu.Visible = False
            Me.divBannerLoggedInDetails.Style.Add("visibility", "hidden")
            Me.buttonBar.Style.Add("visibility", "hidden")
            Return
        End If

        If ctx.Authentication_NetworkLogon _
           AndAlso ctx.authentication_mode = Web.Configuration.AuthenticationMode.Windows Then
            Me.divBannerLoggedInDetails.Style.Add("visibility", "hidden")
        End If

    End Sub
End Class
