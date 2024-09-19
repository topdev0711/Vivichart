Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWDropDownList runat=server></{0}:IAWDropDownList>")>
    Public Class IAWDropDownList
        Inherits DropDownList

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText() As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return False
                Return ViewState("TranslateText")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateTooltip() As Boolean
            Get
                If ViewState("TranslateTooltip") Is Nothing Then Return False
                Return ViewState("TranslateTooltip")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateTooltip") = value
            End Set
        End Property
        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            ' Backup the original tooltip value
            Dim originalTexts As New Dictionary(Of Integer, String)
            Dim originalTooltips As New Dictionary(Of Integer, String)
            Dim storedTooltip As String = Me.ToolTip
            Dim Txt As String
            Dim TT As String

            ' Translate the tooltip and set it to the control
            If Not String.IsNullOrEmpty(storedTooltip) And TranslateTooltip Then
                Me.ToolTip = ctx.Translate(storedTooltip)
            End If

            ' Backup the original text and tooltip values of the list items

            If TranslateText Or TranslateTooltip Then

                For i As Integer = 0 To Me.Items.Count - 1
                    If TranslateText Then
                        Txt = Me.Items(i).Text
                        originalTexts.Add(i, Txt)
                        Me.Items(i).Text = ctx.Translate(Txt)
                    End If

                    If TranslateTooltip Then
                        TT = Me.Items(i).Attributes("title")
                        If Not String.IsNullOrEmpty(TT) Then
                            originalTooltips.Add(i, TT)
                            Me.Items(i).Attributes("title") = ctx.Translate(TT)
                        End If
                    End If
                Next

            End If

            ' Render the control
            MyBase.Render(writer)

            ' Restore the original tooltip value
            Me.ToolTip = storedTooltip

            If TranslateText Or TranslateTooltip Then
                ' Re-apply the original text and tooltip values of the list items
                For i As Integer = 0 To Me.Items.Count - 1
                    If TranslateText Then
                        Me.Items(i).Text = originalTexts(i)
                    End If

                    If TranslateTooltip Then
                        If originalTooltips.ContainsKey(i) Then
                            Me.Items(i).Attributes("title") = originalTooltips(i)
                        End If
                    End If
                Next

            End If
        End Sub

    End Class

End Namespace