Imports Microsoft.VisualBasic

Namespace IAW.controls

    Public Class DesignerIAWLabel
        Inherits System.Web.UI.Design.ControlDesigner

        Public Overrides Function GetDesignTimeHtml() As String
            'Return MyBase.GetDesignTimeHtml()

            Dim controltext As String = CType(Component, IAWLabel).Text

            If Not String.IsNullOrEmpty(controltext) Then
                'Dim writer As New System.IO.StringWriter()
                'Dim html As New HtmlTextWriter(writer)
                Dim lbl As New StringBuilder
                lbl.Append("<table border='0' cellpadding='1' cellspacing='0'>")
                lbl.AppendFormat("<tr><td>{0}</td></tr>", controltext)
                lbl.Append("</table>")
                Return lbl.ToString

                'lbl.RenderControl(html)
                'Return writer.ToString()
            Else
                Return GetEmptyDesignTimeHtml()
            End If

        End Function

    End Class

End Namespace