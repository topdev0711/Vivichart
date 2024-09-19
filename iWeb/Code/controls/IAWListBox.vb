
Namespace IAW.controls

    <ToolboxData("<{0}:IAWListbox runat=server></{0}:IAWListbox>")> _
    Public Class IAWListbox
        Inherits ListBox

        Protected Overrides Sub RenderContents(ByVal writer As System.Web.UI.HtmlTextWriter)

            Dim myFlag1 As Boolean = False
            Dim myFlag2 As Boolean = (Me.SelectionMode = ListSelectionMode.Single)
            Dim collection1 As ListItemCollection = Me.Items
            Dim listItemsCount As Integer = collection1.Count
            If listItemsCount > 0 Then
                For num2 As Integer = 0 To listItemsCount - 1
                    Dim item1 As ListItem = collection1.Item(num2)
                    writer.WriteBeginTag("option")
                    If item1.Selected Then
                        If myFlag2 Then
                            If myFlag1 Then
                                Throw New HttpException("A ListBox cannot have multiple items selected when the SelectionMode is Single")
                            End If
                            myFlag1 = True
                        End If
                        writer.WriteAttribute("selected", "selected")
                    End If
                    writer.WriteAttribute("value", item1.Value, True)
                    'The line below is why the listbox never
                    ' rendered any attributes you set for list items.
                    item1.Attributes.Render(writer) '<-- Missing line
                    writer.Write(">")
                    HttpUtility.HtmlEncode(item1.Text, writer)
                    writer.WriteEndTag("option")
                    writer.WriteLine()
                Next num2
            End If
        End Sub

    End Class

End Namespace
