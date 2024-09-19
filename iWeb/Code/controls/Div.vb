Imports Microsoft.VisualBasic

Public Class Div
    Inherits System.Web.UI.WebControls.Label

    Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
        Get
            Return HtmlTextWriterTag.Div
        End Get
    End Property

End Class
