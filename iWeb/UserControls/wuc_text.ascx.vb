
Partial Class wuc_text
    Inherits System.Web.UI.UserControl

    Dim _CssClass As String = "ParaText"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
    End Sub

    Public Property CssClass() As String
        Get
            Return _CssClass
        End Get
        Set(ByVal Value As String)
            _CssClass = Value
        End Set
    End Property

    Public WriteOnly Property src() As String
        Set(ByVal Value As String)
            text_string.Text = SawUtil.GetMsg(Value.Trim.ToUpper)
        End Set
    End Property

    Protected Overrides Sub OnPreRender(ByVal e As System.EventArgs)
        MyBase.OnPreRender(e)
        text_string.CssClass = CssClass
    End Sub
End Class
