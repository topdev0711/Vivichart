<WebType(WebType.iWebCore), ProcessRequired(False)> _
Partial Class _Default
    Inherits stub_IngenWebPage

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim userAgent As String = HttpContext.Current.Request.UserAgent

        Dim isMobile As Boolean = userAgent.ContainsOneOf("Android", "webOS", "iPhone", "iPad", "iPod", "BlackBerry", "IEMobile", "Opera Mini")
        Dim isTablet As Boolean = userAgent.ContainsOneOf("Tablet", "iPad", "PlayBook", "Nexus 10", "Nexus 7", "Kindle Fire", "Xoom", "Galaxy Tab", "GT-P1000", "SCH-I800")

        If isTablet Then
            ctx.session("MobileDevice") = "Yes"
            ctx.session("DeviceType") = "Tablet"
        ElseIf isMobile Then
            ctx.session("MobileDevice") = "Yes"
            ctx.session("DeviceType") = "Mobile"
        Else
            ctx.session("isMobile") = "No"
        End If

        Response.Redirect("~/secure/login.aspx")
    End Sub

End Class
