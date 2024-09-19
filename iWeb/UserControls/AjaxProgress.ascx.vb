
Partial Class AjaxProgress
    Inherits System.Web.UI.UserControl

    Public Property DisplayText() As String
        Get
            Return lblDisplayText.Text
        End Get
        Set(ByVal value As String)
            lblDisplayText.Text = value
        End Set
    End Property

    Public Property ImageUrl() As String
        Get
            Return imgProgress.ImageUrl
        End Get
        Set(ByVal value As String)
            imgProgress.ImageUrl = value
        End Set
    End Property

End Class
