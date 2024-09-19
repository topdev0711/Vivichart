Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWRangeValidator runat=server></{0}:IAWRangeValidator>")>
    Public Class IAWRangeValidator
        Inherits RangeValidator

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether the error message should be translated")>
        Public Property TranslateErrorMessage As Boolean
            Get
                If ViewState("TranslateErrorMessage") Is Nothing Then
                    Return True
                End If
                Return CBool(ViewState("TranslateErrorMessage"))
            End Get
            Set(value As Boolean)
                ViewState("TranslateErrorMessage") = value
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(False), Description("Whether to add line break prefix to message")>
        Public Property AddBR As Boolean
            Get
                If ViewState("AddBR") Is Nothing Then
                    Return False
                End If
                Return CBool(ViewState("AddBR"))
            End Get
            Set(value As Boolean)
                ViewState("AddBR") = value
            End Set
        End Property

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal text.")>
        Public WriteOnly Property orgErrorMessage As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            Dim originalErrorMessage As String = Me.ErrorMessage
            If TranslateErrorMessage AndAlso Not String.IsNullOrWhiteSpace(ErrorMessage) Then
                Me.ErrorMessage = ctx.Translate(ErrorMessage)
            End If
            If AddBR Then
                Me.ErrorMessage = "<br />" & Me.ErrorMessage
            End If
            MyBase.Render(writer)
            Me.ErrorMessage = originalErrorMessage
        End Sub

    End Class

End Namespace
