Public Class ValidateEventArgs
    Inherits EventArgs

    Private _isvalid As Boolean
    Private _message As String

#Region "properties"
    Public ReadOnly Property IsValid() As Boolean
        Get
            Return Me._isvalid
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
        Me._isvalid = True
        Me._message = String.Empty
    End Sub

    Public Sub New(ByVal isValid As Boolean, ByVal message As String)
        Me._isvalid = isValid
        Me._message = message
    End Sub

    Public Sub New(ByVal isValid As Boolean)
        Me._isvalid = isValid
        Me._message = String.Empty
    End Sub
#End Region

End Class
