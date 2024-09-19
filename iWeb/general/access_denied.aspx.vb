
<WebType(WebType.iWebCore), ProcessRequired(False)> _
Partial Class access_denied
    Inherits stub_IngenWebPage

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not String.IsNullOrEmpty(ctx.item("url")) AndAlso Not ctx.item("url").contains("access_denied") Then
            lblUrl.Text = ctx.item("url")
        Else
            divUrl.Visible = False
        End If

        If Not String.IsNullOrEmpty(ctx.item("msg")) Then
            lblUserMsg.Text = ctx.item("msg")
        Else
            lblUserMsg.Visible = False
        End If

        If Not ctx.IsAuthenticated Then
            divUrl.Visible = False
            ctx.buttonBar.Style.Add("visibility", "hidden")
        End If
    End Sub
End Class
