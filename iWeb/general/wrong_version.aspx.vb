Imports System.Web.Configuration

<WebType(WebType.iWebCore), ProcessRequired(False), MenuRequired(False), ButtonBarRequired(False), LoginStatusRequired(False)> _
Partial Class wrong_version
    Inherits stub_IngenWebPage

    ''' <summary>
    ''' a commenbt
    ''' </summary>
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        lblSysReq.Text = ctx.Translate("::LT_S0376") ' "System Revision"
        lblSysReq_ver.Text = Version.DBVersion

        'lblWeb.Text = "Website Revision " + Version.AppVer + " only supports System Revision" + If(Version.SupportedVersions.Contains(" "), "s", "")
        If Version.SupportedVersions.Contains(" ") Then
            lblWeb.Text = String.Format(ctx.Translate("::LT_S0378"), Version.AppVer) ' "Website Revision " + Version.AppVer + " only supports System Revisions"
        Else
            lblWeb.Text = String.Format(ctx.Translate("::LT_S0377"), Version.AppVer) ' "Website Revision " + Version.AppVer + " only supports System Revision"
        End If
        lblWeb_ver.Text = Version.SupportedVersions

    End Sub
End Class