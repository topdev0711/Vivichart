
Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWPanel runat=server></{0}:IAWPanel>")>
    Public Class IAWPanel
        Inherits Panel

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

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal text.")>
        Public WriteOnly Property orgGroupingText As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            If Not TranslateText Then
                MyBase.Render(writer)
                Return
            End If

            ' Retrieve the grouping text from the Text property
            Dim storedText As String = Me.GroupingText

            ' Render the translated text to the output
            Me.GroupingText = ctx.Translate(storedText)

            MyBase.Render(writer)

            ' restore the grouping text to its correct value
            Me.GroupingText = storedText
        End Sub

    End Class

End Namespace
