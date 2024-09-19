Imports Microsoft.VisualBasic
Imports System.Web.Configuration.Providers

Public Class ViewStateManager

    Public Sub New()
    End Sub

    Public Shared Function LoadPageState(ByVal pControl As Control) As Object
        'Dim base1 As ViewStateProviderBase = ViewStateProviderBase.Instance
        'Return base1.LoadPageState(pControl)
        'kept getting object not set to reference, could not track down, so will create sqlprovider directly
        Dim vsProv As New SqlViewStateProvider
        Return vsProv.LoadPageState(pControl)
    End Function

    Public Shared Sub SavePageState(ByVal pControl As Control, ByVal viewState As Object)
        'Dim base1 As ViewStateProviderBase = ViewStateProviderBase.Instance
        'base1.SavePageState(pControl, viewState)
        'kept getting object not set to reference, could not track down, so will create sqlprovider directly
        Dim vsProv As New SqlViewStateProvider
        vsProv.SavePageState(pControl, viewState)
    End Sub

End Class
