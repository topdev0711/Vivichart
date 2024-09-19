Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWRadioButtonList runat=server />")>
    Public Class IAWRadioButtonList
        Inherits RadioButtonList

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText() As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return Convert.ToBoolean(ViewState("TranslateText"))
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            If Not TranslateText Then
                MyBase.Render(writer)
                Return
            End If

            ' Temporarily store the original texts of each item
            Dim originalTexts As New List(Of String)

            For Each item As ListItem In Me.Items
                originalTexts.Add(item.Text)
                ' Translate the text and set it to the item
                item.Text = ctx.Translate(item.Text)
            Next

            ' Render the RadioButtonList with translated texts
            MyBase.Render(writer)

            ' Reset the Text properties to their original values
            For i As Integer = 0 To Me.Items.Count - 1
                Me.Items(i).Text = originalTexts(i)
            Next
        End Sub

    End Class

End Namespace
