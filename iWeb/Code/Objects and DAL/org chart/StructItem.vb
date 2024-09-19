Imports Microsoft.VisualBasic

<Serializable()> Public Class StructItem
    Private sName As String
    Private iLen As Integer
    Private bAll As Boolean
    Private iPos As Integer
    Private sValue As String

    Public Sub New(ByVal aName As String, ByVal aPos As Integer, ByVal aLen As Integer)
        Name = aName
        Pos = aPos
        Len = aLen
        All = True
        sValue = String.Empty
    End Sub
    Public Property Pos() As Integer
        Get
            Return iPos
        End Get
        Set(ByVal Value As Integer)
            iPos = Value
        End Set
    End Property
    Public Property Len() As Integer
        Get
            Return iLen
        End Get
        Set(ByVal Value As Integer)
            iLen = Value
        End Set
    End Property
    Public Property Name() As String
        Get
            Return sName
        End Get
        Set(ByVal Value As String)
            sName = Value
        End Set
    End Property
    Public Property All() As Boolean
        Get
            Return bAll
        End Get
        Set(ByVal Value As Boolean)
            bAll = Value
        End Set
    End Property
    Public Property Value() As String
        Get
            Return sValue
        End Get
        Set(ByVal Val As String)
            sValue = Val
        End Set
    End Property

End Class
