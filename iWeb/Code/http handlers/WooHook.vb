Imports System.Web
Imports Newtonsoft.Json
Imports System.Security.Cryptography
Imports System.IO
Imports System.Text


Public Class WooHookx
    Implements IHttpHandler

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim Resp As HttpResponse = context.Response

        context.Response.ContentType = "text/plain"

        If context.Request.HttpMethod = "POST" Then
            Try
                Dim receivedSignature As String = context.Request.Headers("X-WC-Webhook-Signature")
                If String.IsNullOrEmpty(receivedSignature) Then
                    ' Signature header missing. Respond with an appropriate error.
                    IawDB.execNonQuery("insert dbo.WooLog(json_string,valid_sig)
                                                values ('Missing Signature Header','E')")
                    Resp.StatusCode = 400 ' Bad Request
                    Resp.Write("Missing signature header")
                    Return
                End If

                ' Read the request payload
                Dim jsonData As String = New StreamReader(context.Request.InputStream).ReadToEnd()

                ' Compute the signature for the received payload using your webhook secret
                Dim secret As String = "EOkz974wzPulhsYuy36vQlcprz2j1xvBFPV6XmLoIS"
                Dim computedSignature As String = ComputeSignature(jsonData, secret)
                Dim ValidData As Boolean = receivedSignature.Equals(computedSignature)

                IawDB.execNonQuery("insert dbo.WooLog(json_string,valid_sig)
                                            values (@p1,@p2)",
                                            jsonData,
                                            If(ValidData, "Y", "N"))

                ' Check if the signatures match
                If Not ValidData Then
                    Resp.StatusCode = 400 ' Bad Request
                    Resp.Write("Invalid signature")
                    Return
                End If

                Resp.StatusCode = 200
                Resp.Write("Received")
            Catch ex As Exception
                ' Handle any errors that may occur
                IawDB.execNonQuery("insert dbo.WooLog(json_string,valid_sig)
                                            values (@p1,'E')", ex.Message)

                Resp.StatusCode = 401 ' Unauthorized
                Resp.Write("An Error Occurred")   ' : " & ex.Message)
            End Try
        Else
            ' Not a POST request
            IawDB.execNonQuery("insert dbo.WooLog(json_string,valid_sig)
                                        values ('Not a POST Request','E')")
            Resp.StatusCode = 405 ' Method Not Allowed
            Resp.Write("Method Not Allowed")
        End If
    End Sub

    Public Function ComputeSignature(ByVal payload As String, ByVal secret As String) As String
        Dim keyBytes = Encoding.ASCII.GetBytes(secret)
        Dim payloadBytes = Encoding.ASCII.GetBytes(payload)

        Using hmac = New HMACSHA256(keyBytes)
            Dim hash = hmac.ComputeHash(payloadBytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function

End Class
