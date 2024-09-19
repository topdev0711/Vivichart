Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class DirtyPageHandlingAttribute
    Inherits System.Attribute

    Private _DirtyPageHandlingRequired As Boolean

    Public ReadOnly Property dirtyPageHandlingRequired() As Boolean
        Get
            Return _DirtyPageHandlingRequired
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _DirtyPageHandlingRequired = value
    End Sub
End Class
