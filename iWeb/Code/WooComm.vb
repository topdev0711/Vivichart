Imports RestSharp
Imports RestSharp.Authenticators
Imports RestSharp.Authenticators.OAuth
Imports Newtonsoft.Json

Public Class WooComm
    Public ConsumerKey As String = "ck_97420a11dca674a2a80b874dfea35d922a53a4de"
    Public ConsumerSecret As String = "cs_cecb335e56c60b42afaef2414dbc38fa7f69d3ee"
    Public BaseURL As String = "https://iawresources.com/wp-json/wc/v3/"

    Public Async Function GetProducts() As Threading.Tasks.Task(Of String)
        Dim options As RestClientOptions = New RestClientOptions(BaseURL)
        options.Authenticator = New HttpBasicAuthenticator(ConsumerKey, ConsumerSecret)
        Dim client = New RestClient(options)

        'Dim endpoint As String = "customers"
        'Dim request As New RestRequest(endpoint, Method.Get)

        Dim endpoint As String = "subscriptions"
        ' Create a new RestRequest for the endpoint, filtering by customer ID.
        Dim request As New RestRequest(endpoint, Method.Get)
        request.AddQueryParameter("customer", "10")

        ' asynchronous execution
        Dim response = Await client.ExecuteAsync(request)

        If response.IsSuccessful Then
            ' The request was successful. Process the response as needed, e.g., parsing the JSON content.
            Dim content As String = response.Content
            Return content
        Else
            ' The request failed. Handle errors as appropriate.
            Return $"Error: {response.ErrorMessage}"
        End If
        Return ""
    End Function

End Class
'Dim endpoint As String = "orders"
'' Create a new RestRequest for the endpoint, filtering by customer ID.
'Dim request As New RestRequest(endpoint, Method.Get)
'request.AddQueryParameter("customer", customerId.ToString())
