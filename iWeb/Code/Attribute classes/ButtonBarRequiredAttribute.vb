Imports Microsoft.VisualBasic

''' <summary>
''' 
''' </summary>
''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Class)> _
    Public Class ButtonBarRequiredAttribute
        Inherits System.Attribute

    Private _buttonBarRequired As Boolean

    Public ReadOnly Property buttonBarRequired() As Boolean
        Get
            Return _buttonBarRequired
        End Get
    End Property

        Public Sub New(ByVal value As Boolean)
        _buttonBarRequired = value
        End Sub
    End Class
