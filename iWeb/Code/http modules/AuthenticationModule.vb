Imports System.Web.Configuration

Namespace IAW.httpmodules


    Public Class AuthenticationModule
        Implements IHttpModule, IRequiresSessionState


        Public Sub Init(ByVal context As System.Web.HttpApplication) Implements System.Web.IHttpModule.Init
            AddHandler context.PreRequestHandlerExecute, AddressOf Me.AuthenticateRequest
        End Sub

        Private Sub AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
            If TypeOf HttpContext.Current.CurrentHandler Is System.Web.UI.Page Then

                Dim objApp As HttpApplication
                objApp = CType(sender, HttpApplication)
                Dim context As HttpContext
                context = objApp.Context

                'Prevent caching, so can't be viewed offline
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache)

                If context.Request.IsAuthenticated Then
                    'test for new session if new session then logout
                    If context.Session.IsNewSession Then
                        Dim szCookieHeader As String = context.Request.Headers("Cookie")
                        If ((szCookieHeader IsNot Nothing) AndAlso (szCookieHeader.IndexOf("ASP.NET_SessionId") >= 0)) Then
                            ctx.logout()
                        End If
                    End If

                    'setup nexturl and initalpage property
                    If Not String.IsNullOrEmpty(context.Request("nexturl")) Then
                        ctx.session("NextURL") = Uri.UnescapeDataString(context.Request("nexturl"))
                        ctx.session("InitialPage") = context.Request.RawUrl.Remove(context.Request.RawUrl.IndexOf("nexturl=") - 1)
                        HttpContext.Current.Response.Redirect(ctx.session("InitialPage"))
                    End If

                    'check version
                    If context.Session("version") Is Nothing Then
                        FormsAuthentication.SignOut()
                        context.Response.Redirect(context.Request.ApplicationPath + "/general/wrong_version.aspx")
                    End If

                    StartPage.RetrieveRoles()
                End If

            End If
        End Sub

        Public Sub Dispose() Implements System.Web.IHttpModule.Dispose
        End Sub

    End Class

End Namespace
