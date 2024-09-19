Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class ProcessRequiredAttribute
    Inherits System.Attribute

    Private _processRequired As Boolean

    Public ReadOnly Property processRequired() As Boolean
        Get
            Return _processRequired
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _processRequired = value
    End Sub

End Class
