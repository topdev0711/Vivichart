Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class LoginStatusRequiredAttribute
    Inherits System.Attribute

    Private _loginStatusRequired As Boolean

    Public ReadOnly Property loginStatusRequired() As Boolean
        Get
            Return _loginStatusRequired
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _loginStatusRequired = value
    End Sub

End Class
