Imports Microsoft.VisualBasic

Public Delegate Sub PageEvent(ByVal sender As Object, ByVal e As PageEventArgs)

Public Class PageEventArgs
    Inherits EventArgs

    Private _page As Page

    Public ReadOnly Property Page() As Page
        Get
            Return _page
        End Get
    End Property

    Public Sub New(ByVal page As Page)
        _page = page
    End Sub
End Class
