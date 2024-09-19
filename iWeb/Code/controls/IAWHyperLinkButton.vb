
Imports System.ComponentModel
Imports System.ComponentModel.Design

Namespace IAW.controls

    <DesignerAttribute(GetType(DesignerIAWHyperLink), GetType(IDesigner)),
    ToolboxData("<{0}:IAWHyperLinkButton runat=server></{0}:IAWHyperLinkButton>")>
    Public Class IAWHyperLinkButton
        Inherits LinkButton

        Private _confirmDelete As Boolean
        Private _confirmDeleteMessage As String = "::LT_S0333" ' Are you sure you want to delete this item?
        Private _isQMessage As Boolean

#Region "properties"
        Public WriteOnly Property ConfirmDelete() As String
            Set(ByVal value As String)
                Me._confirmDelete = True
                If Not String.IsNullOrEmpty(value) Then
                    Me._confirmDeleteMessage = value
                End If
            End Set
        End Property

        Public Property EnableDirtyPageCheck() As Boolean
            Get
                If MyBase.ViewState("EnableDirtyPageCheck") Is Nothing Then Return False
                Return MyBase.ViewState("EnableDirtyPageCheck")
            End Get
            Set(ByVal value As Boolean)
                MyBase.ViewState("EnableDirtyPageCheck") = value
            End Set
        End Property

        Public Property ClearsPageIsDirty() As Boolean
            Get
                If MyBase.ViewState("ClearsPageIsDirty") Is Nothing Then Return False
                Return MyBase.ViewState("ClearsPageIsDirty")
            End Get
            Set(ByVal value As Boolean)
                MyBase.ViewState("ClearsPageIsDirty") = value
            End Set
        End Property

        Public Overrides Property Text() As String
            Get
                Return MyBase.Text
            End Get
            Set(ByVal value As String)
                MyBase.Text = value
                _isQMessage = False
                SawUtil.CheckClasses(Me)
            End Set
        End Property

        Public WriteOnly Property QMessageRef() As String
            Set(ByVal value As String)
                MyBase.Text = SawUtil.GetMsg(value)
                _isQMessage = True
            End Set
        End Property

        Public Overrides Property ToolTip() As String
            Get
                Return MyBase.ToolTip
            End Get
            Set(ByVal value As String)
                MyBase.ToolTip = value
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText As Boolean
            Get
                If ViewState("donttranslate") Is Nothing Then
                    ViewState("donttranslate") = True
                End If
                Return ViewState("donttranslate")
            End Get
            Set(value As Boolean)
                ViewState("donttranslate") = value
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateTooltip() As Boolean
            Get
                If ViewState("TranslateTooltip") Is Nothing Then Return True
                Return ViewState("TranslateTooltip")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateTooltip") = value
            End Set
        End Property

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal text.")>
        Public WriteOnly Property orgText As String
            Set(value As String)
            End Set
        End Property
        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property
#End Region

#Region "constructors"
        Public Sub New(ByVal aHandler As EventHandler, ByVal aLinkText As String)
            If aHandler IsNot Nothing Then
                AddHandler Me.Click, aHandler
            End If
            initialise()
            Text = aLinkText
        End Sub

        Public Sub New(ByVal aHandler As EventHandler)
            If aHandler IsNot Nothing Then
                AddHandler Me.Click, aHandler
            End If
            initialise()
        End Sub

        Public Sub New()
            initialise()
        End Sub
#End Region

        Private Sub initialise()
            Me.CssClass = "hlink"
            Me.EnableDirtyPageCheck = False
            Me.ClearsPageIsDirty = False
            AddHandler Me.Click, AddressOf handleClick
        End Sub

        Public Sub addConfirmDelete(ByVal text As String)
            Me._confirmDelete = True
            Me._confirmDeleteMessage = text
        End Sub

        Public Sub addConfirmDelete()
            Me._confirmDelete = True
        End Sub

        Protected Sub handleClick(ByVal sender As Object, ByVal e As EventArgs)
            'if it is a btn click then clear the hidden field that sets the pageisdirty property on the client
            If Me.ClearsPageIsDirty Then
                Dim csm As ClientScriptManager = Me.Page.ClientScript
                Dim js As String
                js = "DirtyPage.setPageIsDirty(false);"
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "ClearPageIsDirty", js, True)
            End If
        End Sub

#Region "render"
        Public Overrides Sub RenderBeginTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            MyBase.RenderBeginTag(writer)
            writer.Write("<span>")
        End Sub

        Public Overrides Sub RenderEndTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            writer.Write("</span>")
            MyBase.RenderEndTag(writer)
        End Sub

#End Region


        Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
            Dim js As New StringBuilder

            Dim orgText As String = Me.Text
            Dim orgTooltip As String = Me.ToolTip

            If Not Me._isQMessage And TranslateText Then
                Me.Text = ctx.Translate(Me.Text)
            End If

            If String.IsNullOrEmpty(Me.ToolTip) Then
                Me.ToolTip = Me.Text
            Else
                If TranslateTooltip Then
                    Me.ToolTip = ctx.Translate(Me.ToolTip)
                End If
            End If

            js.AppendLine("var lnk = $get('" + Me.ClientID + "');if(lnk!=null)$addHandler(lnk, 'click',")
            js.AppendLine("function(e){")

            If Me._confirmDelete Then
                js.AppendLine("  if (!confirm('" + ctx.Translate(Me._confirmDeleteMessage) + "')){")
                js.AppendLine("    e.stopPropagation();")
                js.AppendLine("    e.preventDefault();")
                js.AppendLine("    return;")
                js.AppendLine("   }")
                If ctx.buttonBar IsNot Nothing And Me.NamingContainer IsNot Nothing Then
                    Dim id As String = Me.NamingContainer.ClientID + "_" + ctx.buttonBar.tblID
                    js.AppendLine("toggleVis('" + id + "');")
                End If
            End If

            If Me.EnableDirtyPageCheck Then
                js.AppendLine("DirtyPage.linkbutton_onclick(e);")
            Else
                js.AppendLine("DirtyPage.clickID='" + Me.ClientID + "';")
            End If

            js.AppendLine("}")
            js.AppendLine(")")

            ScriptManager.RegisterStartupScript(Me, Me.GetType, Me.ClientID, js.ToString, True)

            MyBase.Render(writer)

            Me.Text = orgText
            Me.ToolTip = orgTooltip

        End Sub

        Public Property IconClass() As String
            Get
                Return If(ViewState("IconClass"), String.Empty)
            End Get
            Set(ByVal value As String)
                ViewState("IconClass") = value
            End Set
        End Property

        Protected Overrides Sub RenderContents(ByVal writer As System.Web.UI.HtmlTextWriter)
            ' Render the Font Awesome icon using <i> tag
            If Not String.IsNullOrEmpty(Me.IconClass) Then
                writer.AddAttribute(HtmlTextWriterAttribute.Class, Me.IconClass)
                writer.RenderBeginTag(HtmlTextWriterTag.I)
                writer.RenderEndTag()
            End If

            ' Call the base class's RenderContents to render the text
            MyBase.RenderContents(writer)
        End Sub

    End Class

End Namespace
