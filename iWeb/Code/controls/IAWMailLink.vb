Imports System.ComponentModel
Imports System.Web.UI
Imports System.Web

Namespace IAW.controls

    <DefaultProperty("Text"), ToolboxData("<{0}:IAWMailLink runat=server></{0}:IAWMailLink>")> _
    Public Class IAWMailLink
        Inherits System.Web.UI.WebControls.WebControl

        <Bindable(True), Category("Appearance"), DefaultValue(""), Description("The email address")> _
        Public Property Email() As String
            Get
                If ViewState("email") Is Nothing Then
                    Return String.Empty
                Else
                    Return ViewState("email").ToString()
                End If
            End Get

            Set(ByVal Value As String)
                ViewState("email") = Value
            End Set
        End Property

        <Bindable(True), _
        Category("Appearance"), _
        DefaultValue(""), _
        Description("The text to display on the link."), _
        Localizable(True), _
        PersistenceMode(PersistenceMode.InnerDefaultProperty)> _
        Public Property text() As String
            Get
                If ViewState("text") Is Nothing Then
                    Return String.Empty
                Else
                    Return ViewState("text").ToString()
                End If
            End Get
            Set(ByVal value As String)
                ViewState("text") = value
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether the text should be translated")>
        Public Property TranslateText As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return Convert.ToBoolean(ViewState("TranslateText"))
            End Get
            Set(value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal text.")>
        Public WriteOnly Property orgText As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
            Get
                Return HtmlTextWriterTag.A
            End Get
        End Property

        Protected Overrides Sub AddAttributesToRender(ByVal writer As System.Web.UI.HtmlTextWriter)
            MyBase.AddAttributesToRender(writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Href, "mailto:" + Email)
            writer.AddAttribute(HtmlTextWriterAttribute.Title, "::LT_S0341" + " " + Email) ' send an email to
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As System.Web.UI.HtmlTextWriter)
            Dim output As String = ""

            If text.Equals(String.Empty) Then
                output = Email
            Else
                If TranslateText Or text.StartsWith("::") Then
                    output = ctx.Translate(text)
                Else
                    output = text
                End If
            End If
            writer.WriteEncodedText(output)
        End Sub

    End Class

End Namespace