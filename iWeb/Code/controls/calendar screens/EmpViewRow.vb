Imports Microsoft.VisualBasic

<Serializable()> _
Public Class EmpViewRow
    Private _PersonRef As String
    Private _EmpRef As String
    Private _FullName As String
    Private _URL As String
    Private _DayArray As New ArrayList()

    Public Sub New()
        Me._PersonRef = String.Empty
        Me._EmpRef = String.Empty
        Me._FullName = String.Empty
    End Sub

    Public Sub New(ByVal aPerson As String, ByVal aEmp As String, ByVal aFullName As String)
        Me._PersonRef = aPerson
        Me._EmpRef = aEmp
        Me._FullName = aFullName
    End Sub

    Public Sub AddEmpDay(ByVal aItem As MonthViewCell)
        _DayArray.Add(aItem)
    End Sub

    Public Property PersonRef() As String
        Get
            Return _PersonRef
        End Get
        Set(ByVal Value As String)
            _PersonRef = Value
        End Set
    End Property

    Public Property EmpRef() As String
        Get
            Return _EmpRef
        End Get
        Set(ByVal Value As String)
            _EmpRef = Value
        End Set
    End Property

    Public Property FullName() As String
        Get
            Return _FullName
        End Get
        Set(ByVal Value As String)
            _FullName = Value
        End Set
    End Property

    Public ReadOnly Property DayArray() As ArrayList
        Get
            Return _DayArray
        End Get
    End Property
End Class

