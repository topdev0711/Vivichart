Imports System.Security.Cryptography
Imports System.Net.Http
Imports System.Threading.Tasks

<ButtonBarRequired(False), ProcessRequired(False)>
Public Class WooTest
    Inherits stub_IngenWebPage

    ' EOkz974wzPulhsYuy36vQlcprz2j1xvBFPV6XmLoIS - comment won't be in dll
    Private SecretKey As String = SawUtil.decrypt("8FD8A9B55ECD7889341B9560D86553C58F258E99C354921E78A622EE3666198662BE4C2212F7109E059248F8CE4AE301")

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Async Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        Await SendPostRequestAsync()
    End Sub

    Async Function SendPostRequestAsync() As Task
        Dim endpoint As String = ctx.RootURL + "WooHook.ashx" ' Replace with your actual endpoint
        Dim postData As String = txtPayload.Text.Trim
        Dim Resource As String = ddlbresource.SelectedValue
        Dim sendEvent As String = ddlbEvent.SelectedValue
        Dim hookID As String

        Select Case Resource + "." + sendEvent
            Case "customer.created"
                hookID = "1"
            Case "customer.updated"
                hookID = "3"
            Case "order.created"
                hookID = "2"
            Case Else
                hookID = "0"
        End Select

        Using client As New HttpClient()
            ' Set up custom headers

            client.DefaultRequestHeaders.Add("X-WC-Webhook-Source", "http://localhost/")
            client.DefaultRequestHeaders.Add("X-WC-Webhook-Topic", Resource + "." + sendEvent)
            client.DefaultRequestHeaders.Add("X-WC-Webhook-Resource", Resource)
            client.DefaultRequestHeaders.Add("X-WC-Webhook-Event", sendEvent)
            ' Compute the signature and add it as a header
            Dim signature As String = ComputeSignature(postData, SecretKey)
            client.DefaultRequestHeaders.Add("X-WC-Webhook-Signature", signature)
            client.DefaultRequestHeaders.Add("X-WC-Webhook-ID", hookID)
            client.DefaultRequestHeaders.Add("X-WC-Webhook-Delivery-ID", "9c2b78ff0cc998e15da154634c993672")

            Dim content As New StringContent(postData, Encoding.UTF8, "application/json")

            ' Send POST request
            Dim response As HttpResponseMessage = Await client.PostAsync(endpoint, content)

            If response.IsSuccessStatusCode Then
                ' If the request was successful, include the response content in the message
                Dim responseContent As String = Await response.Content.ReadAsStringAsync()
                txtStatus.Text = $"{response.StatusCode} - {responseContent}"
            Else
                ' If the request failed, return the status code
                txtStatus.Text = $"{response.StatusCode} - Failed to send request."
            End If


        End Using
    End Function

    Public Function ComputeSignature(ByVal payload As String, ByVal secret As String) As String
        Dim keyBytes = Encoding.ASCII.GetBytes(secret)
        Dim payloadBytes = Encoding.ASCII.GetBytes(payload)

        Using hmac = New HMACSHA256(keyBytes)
            Dim hash = hmac.ComputeHash(payloadBytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function

End Class