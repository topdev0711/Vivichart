Imports System.Reflection

Public MustInherit Class ViewStateProviderBase
    Inherits System.Configuration.Provider.ProviderBase

    Protected Sub New()
    End Sub

    Public Shared Function Instance() As ViewStateProviderBase
        Dim cache1 As Cache = HttpRuntime.Cache
        Dim cacheKey As String = Nothing
        Dim configuration1 As ViewStateConfiguration = ViewStateConfiguration.GetConfig
        Dim provider1 As Provider = CType(configuration1.Providers.Item(configuration1.DefaultProvider), Provider)

        cacheKey = ("vsProvider::" & configuration1.DefaultProvider)
        If (cache1.Item(cacheKey) Is Nothing) Then
            cache1.Insert(cacheKey, GetProvider(provider1.Type))
        End If

        'Return CType(cache1.Item(cacheKey), ViewStateProviderBase)
        Return CType(cache1.Item(cacheKey), ViewStateProviderBase)

    End Function

    Private Shared Function GetProvider(ByVal providerName As String) As ViewStateProviderBase
        Dim t As Type = Type.GetType(providerName, True, True)
        Dim provider As ViewStateProviderBase = Activator.CreateInstance(t)
        If provider Is Nothing Then
            Throw New Exception("Unable to load the Provider - " + providerName)
        End If
        Return provider
    End Function


    Public MustOverride Function LoadPageState(ByVal pControl As Control) As Object
    Public MustOverride Sub SavePageState(ByVal pControl As Control, ByVal viewState As Object)

End Class


