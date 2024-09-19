
Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWTextbox runat=server />")>
    Public Class IAWTextbox
        Inherits TextBox

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslatePlaceholder() As Boolean
            Get
                If ViewState("TranslatePlaceholder") Is Nothing Then Return True
                Return ViewState("TranslatePlaceholder")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslatePlaceholder") = value
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
        Public WriteOnly Property orgPlaceholder As String
            Set(value As String)
            End Set
        End Property
        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)

            ' Retrieve the textbox ToolTip and placeholder values
            Dim storedTooltip As String = Me.ToolTip
            Dim storedHint As String = Me.Attributes("placeholder")

            ' Set the translated storedText and storedTooltip on the radio button
            If TranslateTooltip Then Me.ToolTip = ctx.Translate(storedTooltip)
            If TranslatePlaceholder Then Me.Attributes("placeholder") = ctx.Translate(storedHint)

            ' Render the radio button with the translated storedText and storedTooltip
            MyBase.Render(writer)

            ' Reset the Text and ToolTip properties to their original values
            If TranslateTooltip Then Me.ToolTip = storedTooltip
            If TranslatePlaceholder Then Me.Attributes("placeholder") = storedHint
        End Sub

    End Class

End Namespace
