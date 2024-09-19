
Namespace IAW.httphandlers

    Public Class HttpHandler_Logout
        Implements IHttpHandler, IRequiresSessionState

        Public ReadOnly Property IsReusable() As Boolean Implements System.Web.IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property

        Public Sub ProcessRequest(ByVal context As System.Web.HttpContext) Implements System.Web.IHttpHandler.ProcessRequest
            SawDiag.LogVerbose("Logged out")

            Dim msg As String = ""
            Try
                If context.Session IsNot Nothing Then
                    msg = context.Session("loginMsg")

                    FormsAuthentication.SignOut()
                    context.Session.Abandon()
                End If

                If String.IsNullOrEmpty(msg) Then
                    FormsAuthentication.RedirectToLoginPage()
                Else
                    context.Response.Redirect("~/secure/login.aspx?loginMsg=" + msg, True)
                End If
            Catch ex As Exception
                FormsAuthentication.RedirectToLoginPage()
            End Try

        End Sub

    End Class

End Namespace