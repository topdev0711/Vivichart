Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class MenuRequiredAttribute
    Inherits System.Attribute

    Private _menuRequired As Boolean

    Public ReadOnly Property menuRequired() As Boolean
        Get
            Return _menuRequired
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _menuRequired = value
    End Sub

End Class
