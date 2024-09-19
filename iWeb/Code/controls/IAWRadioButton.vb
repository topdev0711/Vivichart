
Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWRadioButton runat=server />")>
    Public Class IAWRadioButton
        Inherits RadioButton

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

            ' Retrieve the radio button's Text and ToolTip properties
            Dim storedText As String = Me.Text
            Dim storedTooltip As String = Me.ToolTip

            ' Set the translated storedText and storedTooltip on the radio button
            If TranslateText Then Me.Text = ctx.Translate(storedText)
            If TranslateTooltip Then Me.ToolTip = ctx.Translate(storedTooltip)

            ' Render the radio button with the translated storedText and storedTooltip
            MyBase.Render(writer)

            ' Reset the Text and ToolTip properties to their original values
            Me.Text = storedText
            Me.ToolTip = storedTooltip
        End Sub

    End Class

End Namespace
