Public Class CrudEventArgs
    Inherits EventArgs

    Private _worked As Boolean
    Private _message As String

#Region "properties"
    Public ReadOnly Property Worked() As Boolean
        Get
            Return Me._worked
        End Get
    End Property

    Public ReadOnly Property Message() As String
        Get
            Return Me._message
        End Get
    End Property
#End Region

#Region "Constructors"
    Public Sub New()
        Me._worked = True
        Me._message = String.Empty
    End Sub

    Public Sub New(ByVal worked As Boolean, ByVal message As String)
        Me._worked = worked
        Me._message = message
    End Sub

    Public Sub New(ByVal worked As Boolean)
        Me._worked = worked
        Me._message = String.Empty
    End Sub

    Public Sub New(ByVal args As ValidateEventArgs)
        Me._worked = args.IsValid
        Me._message = args.Message
    End Sub

#End Region

End Class
