
Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWLabel runat='server' text='' tooltip=''></{0}:IAWLabel>")>
    Public Class IAWLabel
        Inherits Label

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

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether the tooltip should be translated")>
        Public Property TranslateTooltip As Boolean
            Get
                If ViewState("TranslateTooltip") Is Nothing Then Return True
                Return Convert.ToBoolean(ViewState("TranslateTooltip"))
            End Get
            Set(value As Boolean)
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

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            Dim storedText As String = Me.Text
            Dim storedTooltip As String = Me.ToolTip

            ' if starts with :: it will do the lookup
            If Not TranslateText Then
                Me.Text = SawLang.GetBaseMsg(Me.Text)
            Else
                Me.Text = ctx.Translate(storedText)
            End If
            If Not TranslateTooltip Then
                Me.ToolTip = SawLang.GetBaseMsg(Me.ToolTip)
            Else
                Me.ToolTip = ctx.Translate(storedTooltip)
            End If

            ' Render the radio button with the translated storedText and storedTooltip
            MyBase.Render(writer)

            ' Reset the Text and ToolTip properties to their original values
            Me.Text = storedText
            Me.ToolTip = storedTooltip
        End Sub

    End Class
End Namespace

