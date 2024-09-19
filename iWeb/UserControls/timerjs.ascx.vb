
Partial Class timerjs
    Inherits System.Web.UI.UserControl

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
    End Sub

    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
        Dim Show As Boolean = True
        Dim iTimeoutInterval As Integer
        Dim iInactivityTimeout As Integer

        iTimeoutInterval = CInt(ctx.siteConfigDef("timeout_interval", "20")) * 60000
        iInactivityTimeout = CInt(ctx.siteConfigDef("innactivity_timeout", "10")) * 60000

        '#If DEBUG Then
        '        iTimeoutInterval = 6000000
        '        iInactivityTimeout = 6000000
        '        'iTimeoutInterval = 120000
        '        'iInactivityTimeout = 60000
        '#End If

        If ctx.user Is Nothing Then Show = False
        If Not HttpContext.Current.Request.IsAuthenticated Then Show = False

        writer.Write("<script type=""text/javascript"">")
        writer.Write("var SRV_IDLE_TIME = " + iTimeoutInterval.ToString + ";")
        writer.Write("var srv_timerID = -1;")
        writer.Write("var CLI_IDLE_TIME = " + iInactivityTimeout.ToString + ";")

        If Show Then
            writer.Write("var cli_timerID = -1;")
        Else
            writer.Write("var cli_timerID = -2;")
        End If

        writer.Write("function restartTimers() {" _
                    + "  if (srv_timerID != -1) " _
                    + "    clearTimeout(srv_timerID);" _
                    + "  if (cli_timerID != -1 && cli_timerID != -2) " _
                    + "    clearTimeout(cli_timerID);" _
                    + "  srv_timerID = window.setTimeout('timeOut()',SRV_IDLE_TIME);" _
                    + "  if (cli_timerID != -2) " _
                    + "    cli_timerID = window.setTimeout('timeOut()',CLI_IDLE_TIME);" _
                    + "}")

        If Show Then
            'writer.Write("var CLI_IDLE_TIME = 15000;" _
            writer.Write("function resetTimer() {" _
                       + " if (cli_timerID != -1) " _
                       + "  {" _
                       + "      clearTimeout(cli_timerID);" _
                       + "      cli_timerID = window.setTimeout('timeOut()',CLI_IDLE_TIME);" _
                       + "  }" _
                       + "}")
        End If

        writer.Write("function resetSrvTimer() {" _
                    + "  if (srv_timerID != -1) " _
                    + "  {" _
                    + "    clearTimeout(srv_timerID);" _
                    + "    srv_timerID = window.setTimeout('timeOut()',SRV_IDLE_TIME);" _
                    + "  }" _
                    + "}")

        writer.Write("function timeOut() {")

        If Show Then
            writer.Write(" if (cli_timerID != -1) { " _
                       + "    clearTimeout(cli_timerID); }" _
                       + " IAW.inactivityTimeout = true;" _
                       + " cli_timerID = -1;")
        End If

        writer.Write(" if (srv_timerID != -1) { " _
                   + "     clearTimeout(srv_timerID); }" _
                   + " srv_timerID = -1;")

        'if timeout on single page, jump to specified redirect, else do logout
        If Not String.IsNullOrEmpty(Session("NextURL")) Then
            writer.Write(" location.href='" + Session("NextURL") + "';}")
        Else
            writer.Write(" location.href='" + ctx.virtualDir + "/logout.aspx?';}")
            'there is no logout.aspx, instead a http module picks up the url logs the user and directs to the login page
        End If

        writer.Write("function startTimer() {")

        If Show Then
            writer.Write("  cli_timerID = window.setTimeout('timeOut()',CLI_IDLE_TIME);")
        End If
        writer.Write("  srv_timerID = window.setTimeout(""timeOut()"",SRV_IDLE_TIME);")

        writer.Write("}")

        'writer.Write("function onLoading() {" _
        '           + "  startTimer();" _
        '           + "  placeFocus();" _
        '           + "}")

        'writer.Write("function onLoading() {" _
        '           + "  startTimer();" _
        '           + "}")

        writer.Write("function onUnloading() {")
        If Show Then writer.Write("  if (cli_timerID != -1 && cli_timerID != -2) { clearTimeout(cli_timerID); }")
        writer.Write("  if (srv_timerID != -1) { clearTimeout(srv_timerID); } ")
        writer.Write("}")

        writer.Write("</script>")

        If Show Then
            ScriptManager.RegisterStartupScript(Me.Page, Me.GetType, "timerjs", "$addHandler(document, ""mousemove"", resetTimer); " _
                                                                              + "$addHandler(document, ""click"", resetTimer); " _
                                                                              + "$addHandler(document, ""keydown"", resetTimer); ", True)
            'writer.Write("$addHandler(window, ""mousemove"", resetTimer); " _
            '           + "$addHandler(window, ""click"", resetTimer); " _
            '           + "$addHandler(window, ""keydown"", resetTimer); ")

            'writer.Write("window.attachEvent(""onmousemove"", resetTimer); " _
            '           + "window.attachEvent(""onclick"", resetTimer); " _
            '           + "window.attachEvent(""onkeydown"", resetTimer); ")
        End If

    End Sub
End Class
