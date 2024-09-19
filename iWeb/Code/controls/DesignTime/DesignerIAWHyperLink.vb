Imports Microsoft.VisualBasic

Namespace IAW.controls

    Public Class DesignerIAWHyperLink
        Inherits System.Web.UI.Design.ControlDesigner

        Public Overrides Function GetDesignTimeHtml() As String
            'Return MyBase.GetDesignTimeHtml()
            Dim cntrl As WebControl = Component
            Dim controltext As String = String.Empty
            If TypeOf cntrl Is IAWHyperLink Then
                controltext = CType(Component, IAWHyperLink).Text
            ElseIf TypeOf cntrl Is IAWHyperLinkButton Then
                controltext = CType(Component, IAWHyperLinkButton).Text
            End If

            If Not String.IsNullOrEmpty(controltext) Then
                Dim writer As New System.IO.StringWriter()
                Dim html As New HtmlTextWriter(writer)
                Dim hlink As New HtmlAnchor
                hlink.InnerText = controltext
                hlink.RenderControl(html)
                Return writer.ToString()
            Else
                Return GetEmptyDesignTimeHtml()
            End If

        End Function
    End Class

End Namespace