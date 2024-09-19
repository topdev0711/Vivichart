
Partial Class wuc_hyperlink
    Inherits System.Web.UI.UserControl

    Public Property Text() As String
        Get
            Return link.Text
        End Get
        Set(ByVal Value As String)
            link.Text = SawLang.Translate(Value)
        End Set
    End Property

    Public Property URL() As String
        Get
            Return link.NavigateUrl
        End Get
        Set(ByVal Value As String)
            link.NavigateUrl = Value
        End Set
    End Property

    Public Property ToolTip() As String
        Get
            Return link.ToolTip
        End Get
        Set(ByVal Value As String)
            link.ToolTip = Value
        End Set
    End Property

    Public Property CssClass() As String
        Get
            Return link.CssClass
        End Get
        Set(ByVal Value As String)
            link.CssClass = Value
        End Set
    End Property

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
        'Put user code to initialize the page here
    End Sub
End Class
