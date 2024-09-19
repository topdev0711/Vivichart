Imports System.ComponentModel
Imports System.ComponentModel.Design


Namespace IAW.controls

    <DesignerAttribute(GetType(DesignerIAWHyperLink), GetType(IDesigner)), DefaultProperty("Url"),
    ToolboxData("<{0}:IAWHyperLink runat=""server"" ID=""{1}""></{0}:IAWHyperLink>")>
    Public Class IAWHyperLink
        Inherits WebControl

        Private _isQMessage As Boolean

#Region "properties"

        <Bindable(True), Category("Behavior"), DefaultValue(""), Description("The url to navigate to")>
        Public Property Url() As String
            Get
                Dim text1 As String = CType(ViewState("Url"), String)
                If text1 IsNot Nothing Then Return text1
                Return String.Empty
            End Get
            Set(ByVal Value As String)
                ViewState("Url") = Value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(True), Description("Add the cmd url parameter")>
        Public Property addCmdParameter() As Boolean
            Get
                If ViewState("addCmdParameter") Is Nothing Then Return True
                Return ViewState("addCmdParameter")
            End Get
            Set(ByVal value As Boolean)
                ViewState("addCmdParameter") = value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(True), Description("Enable Dirty Page Check")>
        Public Property EnableDirtyPageCheck() As Boolean
            Get
                If ViewState("EnableDirtyPageCheck") Is Nothing Then Return False
                Return ViewState("EnableDirtyPageCheck")
            End Get
            Set(ByVal value As Boolean)
                ViewState("EnableDirtyPageCheck") = value
            End Set
        End Property

        <Bindable(True), Category("Behavior"), DefaultValue(""), Description("Image Url")>
        Public Overridable Property ImageUrl() As String
            Get
                Dim text1 As String = CType(Me.ViewState("ImageUrl"), String)
                If text1 IsNot Nothing Then Return text1
                Return String.Empty
            End Get
            Set(ByVal value As String)
                Me.ViewState("ImageUrl") = value
            End Set
        End Property

        <Category("Behavior"), DefaultValue(""), Description("The target window to navigate to")>
        Public Property Target() As String
            Get
                Dim text1 As String = CType(Me.ViewState("Target"), String)
                If text1 IsNot Nothing Then Return text1
                Return String.Empty
            End Get
            Set(ByVal value As String)
                Me.ViewState("Target") = value
            End Set
        End Property

        <Bindable(True), Category("Appearance"), DefaultValue(""), Description("HyperLink_Text"), PersistenceMode(PersistenceMode.InnerDefaultProperty), Localizable(True)>
        Public Overridable Property [Text]() As String
            Get
                Dim obj As Object = Me.ViewState("Text")
                If obj IsNot Nothing Then Return CType(obj, String)
                Return String.Empty
            End Get
            Set(ByVal value As String)
                If Me.HasControls Then
                    Me.Controls.Clear()
                End If
                Me.ViewState("Text") = value
                _isQMessage = False
                SawUtil.CheckClasses(Me)
            End Set
        End Property

        <Category("Appearance"), Description("Get text from Message Reference")>
        Public WriteOnly Property QMessageRef() As String
            Set(ByVal value As String)
                Me.Text = SawUtil.GetMsg(value)
                _isQMessage = True
            End Set
        End Property

        <Category("Appearance"), Description("Set the Tooltip")>
        Public Overrides Property ToolTip() As String
            Get
                If MyBase.ToolTip Is Nothing And Me.Text IsNot Nothing Then
                    MyBase.ToolTip = Me.Text
                End If
                Return MyBase.ToolTip
            End Get
            Set(ByVal value As String)
                MyBase.ToolTip = value
            End Set
        End Property

        Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
            Get
                Return HtmlTextWriterTag.A
            End Get
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText() As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return ViewState("TranslateText")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateText") = value
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

        Public Sub New(ByVal aUrl As String, ByVal aLinkText As String)
            Me.Url = aUrl
            Me.Text = aLinkText
            initialise()
            SawUtil.CheckClasses(Me)
        End Sub

        Public Sub New(ByVal aUrl As String)
            Me.Url = aUrl
            initialise()
        End Sub

        Public Sub New()
            initialise()
        End Sub

        Private Sub initialise()
            Me.CssClass = "hlink"
            Me.addCmdParameter = True
            Me.TranslateText = True

        End Sub

        Protected Overrides Sub AddAttributesToRender(ByVal writer As System.Web.UI.HtmlTextWriter)
            If (Me.Enabled AndAlso Not MyBase.IsEnabled) Then
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled")
            End If

            MyBase.AddAttributesToRender(writer)

            If (Me.Url.Length > 1 AndAlso MyBase.IsEnabled) Then

                If Me.Url.ToLower.Contains("javascript") Then
                    Me.addCmdParameter = False
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, Me.Url)
                ElseIf Me.Url.ToLower.StartsWith("www") Then
                    If Me.Url.ToLower.StartsWith("www") Then
                        Me.Url = "http://" + Me.Url
                    End If
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, MyBase.ResolveClientUrl(Me.Url))
                Else
                    If Me.addCmdParameter Then
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, MyBase.ResolveClientUrl(SawUtil.encryptQuery(Me.Url, True)))
                    Else
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, MyBase.ResolveClientUrl(Me.Url))
                    End If
                End If

            Else
                writer.AddAttribute(HtmlTextWriterAttribute.Href, "#")
            End If

            If Me.Target.Length > 0 Then
                writer.AddAttribute(HtmlTextWriterAttribute.Target, Me.Target)
            End If

            If Me.ToolTip.Length > 0 Then
                writer.AddAttribute(HtmlTextWriterAttribute.Title, translate(Me.ToolTip))
            End If

            If EnableDirtyPageCheck Then
                Dim str As String = SawLang.Translate("::LT_S0332") ' Do you want to save your changes before continuing?\n\nClick cancel to continue without saving.
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "if(DirtyPage == true){if (confirm('" + str + "')){return false;}else{return true;}}")
            End If

        End Sub

        Public Overrides Sub RenderBeginTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            MyBase.RenderBeginTag(writer)
            writer.Write("<span>")
        End Sub

        Public Overrides Sub RenderEndTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            writer.Write("</span>")
            MyBase.RenderEndTag(writer)
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As System.Web.UI.HtmlTextWriter)
            Dim txt As String = Me.ImageUrl
            If (txt.Length > 0) Then
                Dim img As New Image
                img.ImageUrl = MyBase.ResolveClientUrl(txt)
                If (Me.ToolTip.Length <> 0) Then
                    If TranslateTooltip Then
                        img.ToolTip = ctx.Translate(Me.ToolTip)
                    Else
                        img.ToolTip = SawLang.GetBaseMsg(Me.ToolTip)
                    End If
                End If
                If (Me.Text.Length <> 0) And Not Me._isQMessage Then img.AlternateText = translate(Me.Text)
                img.RenderControl(writer)
            Else
                MyBase.RenderContents(writer)
                If Me.Text.Length > 0 And Not Me._isQMessage Then
                    writer.Write(translate(Me.Text))
                Else
                    writer.Write(Me.Text)
                End If
            End If
        End Sub

        Private Function translate(ByVal value As String) As String
            If Not Me.TranslateText Then Return value
            Return SawLang.Translate(value)
        End Function

        Protected Overrides Sub LoadViewState(ByVal savedState As Object)
            If (Not savedState Is Nothing) Then
                MyBase.LoadViewState(savedState)
                Dim text1 As String = CType(Me.ViewState("url"), String)
                If (Not text1 Is Nothing) Then
                    Me.Url = text1
                End If
            End If
        End Sub


    End Class

End Namespace
