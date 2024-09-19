Imports Microsoft.VisualBasic

<Serializable()> Public Class TNode

    Private _ID As String
    Private _Text As String
    Private _URL As String
    Private _Open As Boolean
    Private _Level As String

    Public Sub New(ByVal aID As String, ByVal aText As String, ByVal aURL As String)
        Me._ID = aID
        Me._Text = aText
        Me._URL = aURL
        Me._Open = False
        Me._Level = 0
    End Sub

    Public Property ID() As String
        Get
            Return _ID
        End Get
        Set(ByVal Value As String)
            _ID = Value
        End Set
    End Property

    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal Value As String)
            _Text = Value
        End Set
    End Property

    Public Property URL() As String
        Get
            Return _URL
        End Get
        Set(ByVal Value As String)
            _URL = Value
        End Set
    End Property

    Public Property Open() As Boolean
        Get
            Return _Open
        End Get
        Set(ByVal Value As Boolean)
            _Open = Value
        End Set
    End Property

    Public Property Level() As String
        Get
            Return _Level
        End Get
        Set(ByVal Value As String)
            _Level = Value
        End Set
    End Property

End Class


