Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports IAW.controls
'Imports System.Reflection


Public Class ButtonBar
    Inherits System.Web.UI.WebControls.CompositeControl

    Private _tbl As Table
    Private _activityIndicator As Panel
    Private _visibleOnClient As Boolean
    Private _showChangesPending As Boolean

    Public Delegate Sub ButtonBarEventHandler(ByVal sender As Object, ByVal e As ButtonBarEventArgs)
    Public Event OnControlAdded As ButtonBarEventHandler
    Public Event OnRenderBackButton As EventHandler

#Region "properties"
    Public Property AsyncPostback() As Boolean
        Get
            Return Me.ViewState("AsyncPostback")
        End Get
        Set(ByVal value As Boolean)
            Me.ViewState("AsyncPostback") = value
        End Set
    End Property
    Public Property VisibleOnClient() As Boolean
        Get
            If ViewState("_visibleOnClient") Is Nothing Then Return True
            Return ViewState("_visibleOnClient")
        End Get
        Set(ByVal value As Boolean)
            ViewState("_visibleOnClient") = value
        End Set
    End Property
    Public WriteOnly Property align() As HorizontalAlign
        Set(ByVal value As HorizontalAlign)
            Select Case value
                Case HorizontalAlign.Center
                    Me.Style.Add("text-align", "center")
                Case HorizontalAlign.Right
                    Me.Style.Add("text-align", "right")
                Case Else
                    Me.Style.Add("text-align", "left")
            End Select
        End Set
    End Property
    Public Property table() As Table
        Get
            Return _tbl
        End Get
        Set(ByVal value As Table)
            _tbl = value
        End Set
    End Property
    Public ReadOnly Property tableStyle() As CssStyleCollection
        Get
            Return _tbl.Style
        End Get
    End Property
    Public Property tblID() As String
        Get
            Return _tbl.ID
        End Get
        Set(ByVal value As String)
            _tbl.ID = value
        End Set
    End Property
    Public Property tableCss() As String
        Get
            Return _tbl.CssClass
        End Get
        Set(ByVal value As String)
            _tbl.CssClass = value
        End Set
    End Property
    Public ReadOnly Property backButton() As IAWHyperLink
        Get
            Return CType(Me.FindControl("backBtn"), IAWHyperLink)
        End Get
    End Property
    Public WriteOnly Property ShowChangesPendingBtn() As Boolean
        Set(ByVal value As Boolean)
            If value And Not _showChangesPending Then
                '///only do it the first time
                Dim btn As New IAWHyperLink(ctx.virtualDir + "/change_request_list.aspx?pr=change_requests", "Pending Changes")
                btn.ID = "PendingChanges"
                addControl(btn)
                _showChangesPending = True
            ElseIf Not value And _showChangesPending Then
                '///remove if present
                Dim btn As IAWHyperLink = Me._tbl.FindControl("PendingChanges")
                If btn IsNot Nothing Then
                    _tbl.Controls.Remove(btn)
                    _showChangesPending = False
                End If
            End If

        End Set
    End Property
    Public Overrides Property Visible() As Boolean
        Get
            If ViewState("buttonbarvisible") Is Nothing Then
                MyBase.Attributes.Remove("style")
                MyBase.Attributes.Add("style", "display:block")
                Return True
            End If
            Return ViewState("buttonbarvisible")
        End Get
        Set(ByVal value As Boolean)
            ViewState("buttonbarvisible") = value
            MyBase.Attributes.Remove("style")
            If value = True Then
                MyBase.Attributes.Add("style", "display:block")
            Else
                MyBase.Attributes.Add("style", "display:none")
            End If
        End Set
    End Property

    Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
        Get
            Return HtmlTextWriterTag.Div
        End Get
    End Property
#End Region

    Public Sub New()
        Me.ID = "bBar"

        _activityIndicator = New Panel
        _activityIndicator.Attributes.Add("style", "display:none")
        _activityIndicator.ID = "activityIndicator"
        _activityIndicator.CssClass = "activityIndicator"


        _tbl = New Table
        _tbl.CellSpacing = 0
        _tbl.CellPadding = 0
        _tbl.CssClass = "buttonBar"
        _tbl.ID = "buttonBar"

        add()
        addBackButton()
    End Sub

    Protected Overrides Sub CreateChildControls()
        'If Not VisibleOnClient Then MyBase.Attributes.Add("style", "visibility:hidden")
        If Not VisibleOnClient Then MyBase.Attributes.Add("style", "display:none")
        MyBase.CssClass = "buttonBar"
        MyBase.CreateChildControls()
        Me.Controls.Add(_tbl)

        Dim lbl As New IAWLabel
        lbl.Text = "::LT_S0315"  'please wait
        Dim img As New IAWImage
        img.ImageUrl = ctx.themeGraphicsDir + "wait.gif"
        img.AlternateText = "::LT_S0315"  'please wait
        Me._activityIndicator.Controls.Add(img)
        Me._activityIndicator.Controls.Add(New LiteralControl("&#160;"))
        Me._activityIndicator.Controls.Add(lbl)
        Me.Controls.Add(Me._activityIndicator)
    End Sub

    Public Sub add()
        Dim tr As TableRow
        tr = _tbl.Rows(_tbl.Rows.Add(New TableRow))
        Dim td As New TableCell
        td.CssClass = "buttonBarEndCell"
        _tbl.Rows(0).Cells.Add(td)
    End Sub

    Public Function addLink(ByVal aUrl As String) As IAWHyperLink
        Return addLink(aUrl, "")
    End Function

    Public Function addLink(ByVal aUrl As String, ByVal aLinkText As String) As IAWHyperLink
        Dim btn As New IAWHyperLink(aUrl, aLinkText)
        addControl(btn)
        Return btn
    End Function

    Public Function addLinkButton(ByVal aHandler As EventHandler) As IAWHyperLinkButton
        Return addLinkButton(aHandler, "")
    End Function

    Public Function addLinkButton(ByVal aHandler As EventHandler, ByVal aLinkText As String) As IAWHyperLinkButton
        Dim btn As New IAWHyperLinkButton(aHandler, aLinkText)
        addControl(btn)
        If Me.AsyncPostback Then
            ScriptManager.GetCurrent(Me.Page).RegisterAsyncPostBackControl(btn)
        End If
        Return btn
    End Function

    Private Sub addControl(ByVal aControl As Control)
        Dim td As New TableCell
        td.CssClass = "buttonBar"
        td.Attributes.Add("nowrap", "nowrap")
        If aControl IsNot Nothing Then td.Controls.Add(aControl)
        Dim tr As TableRow

        If _tbl.Rows.Count = 0 Then
            tr = _tbl.Rows(_tbl.Rows.Add(New TableRow))
            tr.ID = "buttonBarTR"
        Else
            tr = _tbl.Rows(0)
        End If
        tr.Controls.AddAt(tr.Controls.Count - 1, td)
        RaiseEvent OnControlAdded(Me, New ButtonBarEventArgs(td, aControl))
    End Sub

    Private Sub addBackButton()
        Dim btn As New IAWHyperLink
        btn.ID = "backBtn"
        btn.Text = "Back"
        btn.EnableDirtyPageCheck = False
        If ctx.session("InitialPage") = ctx.rawUrl Then
            btn.addCmdParameter = False
            btn.Url = ctx.session("NextURL").ToString
        Else
            btn.Url = ""
        End If
        btn.ToolTip = "Back to previous page"
        RaiseEvent OnRenderBackButton(btn, New EventArgs())
        addControl(btn)
    End Sub

    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
        '///hide the back btn if the url =""
        If Not VisibleOnClient Then MyBase.Attributes.Add("style", "display:none")
        If Me.backButton.Url = "" Then
            Me.backButton.Visible = False
        End If
        MyBase.Render(writer)
    End Sub

    Private Sub ButtonBar_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
        If Me.AsyncPostback Then
            Dim js As New StringBuilder
            js.AppendLine("Sys.WebForms.PageRequestManager.getInstance().add_endRequest(")
            js.AppendLine("  function(e){showbuttons();}")
            js.AppendLine(")")
            ctx.Sys_Application_Add_load(Me, "ButtonBar_PreRender", js.ToString)
        End If
    End Sub

End Class


Public Class ButtonBarEventArgs
    Inherits EventArgs

    Public cell As TableCell
    Public LinkButton As IAWHyperLinkButton
    Public HyperLink As IAWHyperLink

    Public Sub New(ByVal aCell As TableCell, ByVal aControl As Control)
        cell = aCell
        Dim t As Type = aControl.GetType
        If TypeOf aControl Is IAWHyperLink Then
            HyperLink = aControl
        ElseIf (TypeOf aControl Is IAWHyperLinkButton) Then
            LinkButton = aControl
        End If
    End Sub


End Class
