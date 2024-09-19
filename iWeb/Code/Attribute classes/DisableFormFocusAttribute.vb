Imports Microsoft.VisualBasic

<AttributeUsage(AttributeTargets.Class)> _
Public Class DisableFormFocusAttribute
    Inherits System.Attribute

    Private _disableFormFocus As Boolean

    Public ReadOnly Property disableFormFocus() As Boolean
        Get
            Return _disableFormFocus
        End Get
    End Property

    Public Sub New(ByVal value As Boolean)
        _disableFormFocus = value
    End Sub
End Class
