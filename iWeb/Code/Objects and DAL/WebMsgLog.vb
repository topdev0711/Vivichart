Imports Microsoft.VisualBasic

Public Class WebMsgLog

#Region "Member Variables"

    Private _id As Integer
    Private _type As String
    Private _date As Date
    Private _message As String

#End Region

#Region "Properties"

    Public Property ID() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property

    Public Property Type() As String
        Get
            Return _type
        End Get
        Set(ByVal value As String)
            _type = value
        End Set
    End Property

    Public Property MessageDate() As Date
        Get
            Return _date
        End Get
        Set(ByVal value As Date)
            _date = value
        End Set
    End Property

    Public Property Message() As String
        Get
            Return _message
        End Get
        Set(ByVal value As String)
            _message = value
        End Set
    End Property

#End Region

#Region "Constructors"

    Public Sub New()
        ID = 0
        Message = String.Empty
        MessageDate = Date.Now
        Type = String.Empty
    End Sub

#End Region

End Class
