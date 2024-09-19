Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class MasterPageRequiredAttribute
    Inherits System.Attribute

    Private _masterRequired As Boolean

    Public ReadOnly Property masterRequired() As Boolean
        Get
            Return _masterRequired
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _masterRequired = value
    End Sub

End Class
