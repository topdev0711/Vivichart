Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWTabPanel runat=server></{0}:IAWTabPanel>")>
    Public Class IAWTabPanel
        Inherits AjaxControlToolkit.TabPanel

        Private Property OriginalHeaderText As String
            Get
                If ViewState("OriginalHeaderText") IsNot Nothing Then
                    Return ViewState("OriginalHeaderText").ToString()
                End If
                Return String.Empty
            End Get
            Set(value As String)
                ViewState("OriginalHeaderText") = value
            End Set
        End Property

        Private Property OriginalToolTipText As String
            Get
                If ViewState("OriginalToolTipText") IsNot Nothing Then
                    Return ViewState("OriginalToolTipText").ToString()
                End If
                Return String.Empty
            End Get
            Set(value As String)
                ViewState("OriginalToolTipText") = value
            End Set
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
        Public WriteOnly Property orgHeaderText As String
            Set(value As String)
            End Set
        End Property
        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub OnPreRender(e As EventArgs)
            If ViewState("OriginalHeaderText") Is Nothing Then
                OriginalHeaderText = Me.HeaderText
            End If

            If Not String.IsNullOrEmpty(Me.HeaderText) Then
                If TranslateText Then Me.HeaderText = ctx.Translate(OriginalHeaderText)
            End If

            If ViewState("OriginalToolTipText") Is Nothing Then
                OriginalToolTipText = Me.ToolTip
            End If

            If Not String.IsNullOrEmpty(Me.ToolTip) Then
                If TranslateTooltip Then Me.ToolTip = ctx.Translate(OriginalToolTipText)
            End If

            MyBase.OnPreRender(e)
        End Sub

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            MyBase.Render(writer)

            If Not String.IsNullOrEmpty(OriginalHeaderText) Then
                If TranslateText Then Me.HeaderText = OriginalHeaderText
            End If

            If Not String.IsNullOrEmpty(OriginalToolTipText) Then
                If TranslateTooltip Then Me.ToolTip = OriginalToolTipText
            End If
        End Sub

    End Class

End Namespace
