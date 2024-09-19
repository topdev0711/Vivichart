
Imports System.Security.Cryptography
Imports System.IO

Public Class WooHook
    Implements IHttpHandler

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim Request As HttpRequest = context.Request
        Dim Response As HttpResponse = context.Response

        context.Response.ContentType = "text/plain"

        Request.InputStream.Position = 0
        Dim BodyString As String = New StreamReader(Request.InputStream).ReadToEnd()

        ' Check for test webhook pattern
        ' woo commerice will send out tests to see if endpoints are correct
        Dim webhookIdPattern As String = "webhook_id=\d+"
        If Regex.IsMatch(BodyString, webhookIdPattern) Then
            DBLog(Request, BodyString, "Webhook Test")
            Response.StatusCode = 200
            Response.Write("Test webhook received")
            Return
        End If

        ' Only POST requests are valid
        If Request.HttpMethod = "POST" Then
            Try
                ' there should be a header that contains a hashed signature
                Dim receivedSignature As String = Request.Headers("X-WC-Webhook-Signature")
                If String.IsNullOrEmpty(receivedSignature) Then
                    DBLog(Request, BodyString, "Signature Missing")
                    Response.StatusCode = 400 ' Bad Request
                    Response.Write("Missing signature header")
                    Return
                End If

                ' Compute the signature for the received payload using the webhook secret
                ' EOkz974wzPulhsYuy36vQlcprz2j1xvBFPV6XmLoIS - comment won't be in dll
                Dim secret As String = SawUtil.decrypt("8FD8A9B55ECD7889341B9560D86553C58F258E99C354921E78A622EE3666198662BE4C2212F7109E059248F8CE4AE301")
                Dim computedSignature As String = ComputeSignature(BodyString, secret)
                Dim ValidData As Boolean = receivedSignature.Equals(computedSignature)

                ' Check if the signatures match
                If Not ValidData Then
                    DBLog(Request, BodyString, "Invalid Signature")

                    Response.StatusCode = 400 ' Bad Request
                    Response.Write("Invalid signature")
                    Return
                End If

                ' get the two headers that denote what action we're doing
                Dim webhookResource As String = Request.Headers("X-WC-Webhook-Resource")
                Dim webhookEvent As String = Request.Headers("X-WC-Webhook-Event")

                ' if we don't get both headers something is wrong
                If String.IsNullOrEmpty(webhookResource) Or String.IsNullOrEmpty(webhookEvent) Then
                    DBLog(Request, BodyString, "X-WC-Webhook headers missing")
                    Response.StatusCode = 400 ' Bad Request
                    Response.Write("X-WC-Webhook headers missing")
                    Return
                End If

                Try
                    Dim WA As New WooAction
                    WA.Process(webhookResource, webhookEvent, BodyString)

                Catch ex As Exception
                    DBLog(Request, BodyString, ex.Message)
                    Response.StatusCode = 400 ' Bad Request
                    Response.Write(ex.Message)
                    Return
                End Try

                DBLog(Request, BodyString, "All Ok")
                Response.StatusCode = 200
                Response.Write("Received")
            Catch ex As Exception
                ' Handle any errors that may occur
                DBLog(Request, BodyString, ex.Message)

                Response.StatusCode = 401 ' Unauthorized
                Response.Write("An Error Occurred")
            End Try
        Else
            ' Not a POST request
            DBLog(Request, BodyString, "Not a POST Request")

            Response.StatusCode = 405 ' Method Not Allowed
            Response.Write("Method Not Allowed")
        End If
    End Sub

    Private Sub DBLog(Request As HttpRequest, Body As String, Status As String)
        Dim headerSB As New StringBuilder()
        For Each key As String In Request.Headers.AllKeys
            Dim value As String = Request.Headers(key)
            headerSB.Append(key & ": " & value & vbCrLf)
        Next
        Dim Head As String = headerSB.ToString

        Dim Origin As String
        If Request.IsLocal Then
            Origin = "Local Machine"
        Else
            Origin = Request.Headers("X-Forwarded-For")
            If String.IsNullOrEmpty(Origin) Then
                Origin = Request.UserHostAddress
            Else
                Dim ipAddresses() As String = Origin.Split(",")
                If ipAddresses.Length > 0 Then
                    Origin = ipAddresses(0).Trim()
                End If
            End If
        End If

        IawDB.execNonQuery("insert dbo.WooLog(header,body,status,origin)
                            values (@p1,@p2,@p3,@p4)",
                            Head, Body, Status, Origin)
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
