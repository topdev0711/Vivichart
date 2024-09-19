<WebType(WebType.iWebCore), ProcessRequired(False), MenuRequired(False)> _
Partial Class ApplicationError
    Inherits stub_IngenWebPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'If Not String.IsNullOrEmpty(ctx.clientID) Then
        '    If ctx.ONS Then
        '        msgContact.Text = "The system is unable to process your request, please try again later or contact the Capita payroll helpdesk on 08004084789 during office opening hours."
        '    End If
        'End If

        'If ctx.siteConfig IsNot Nothing AndAlso ctx.siteConfig("error_message") IsNot Nothing Then
        '    msgContact.Text = ctx.siteConfig("error_message")
        'End If

        Dim msg As String = ctx.session("UserErrorMessage")
        If Not String.IsNullOrEmpty(msg) Then
            'display friendly user error message
            labError.Visible = True
            labError.Text = msg
            ctx.session("UserErrorMessage") = Nothing
        End If

    End Sub
End Class
