Imports Microsoft.VisualBasic
Imports System.ComponentModel

<AttributeUsage(AttributeTargets.Class, AllowMultiple:=True), _
 Description("Which variations of iWeb this page may be included in")> _
Public Class WebTypeAttribute
    Inherits System.Attribute

    Private _webtype As WebType

    Public ReadOnly Property isAccessible() As Boolean
        Get
            Dim Application As HttpApplicationState = HttpContext.Current.Application
            Dim ApplicationWebType As WebType

            If (Not Application("WebType") Is Nothing) Then
                ApplicationWebType = CType(Application("WebType"), WebType)
            Else
                'WebType not set
                Return True
            End If

            'iWeb complete - no restriction
            If ApplicationWebType = WebType.iWebCore Or _webtype = WebType.iWebCore Then Return True

            Return _webtype = ApplicationWebType

        End Get
    End Property

    Public Sub New()
        _webtype = WebType.iWebCore
    End Sub

    Public Sub New(ByVal value As WebType)
        _webtype = value
    End Sub

End Class
