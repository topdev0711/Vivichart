Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class DisableEnterKeyAttribute
    Inherits System.Attribute

    Private _disableEnterKey As Boolean

    Public ReadOnly Property disableEnterKey() As Boolean
        Get
            Return _disableEnterKey
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _disableEnterKey = value
    End Sub
End Class
