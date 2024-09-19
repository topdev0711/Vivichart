Imports Microsoft.VisualBasic

<Serializable()> Public Class MonthViewCell

    Private _Date As Date
    Private _URL As String
    Private _ShiftBgColor As Integer
    Private _LeaveBgColor As Integer
    Private _ShiftFgColor As Integer
    Private _LeaveFgColor As Integer
    Private _authorised As String
    Private _DurationType As String
    Private _Duration As Decimal
    Private _AbsenceType As String
    Private _AbsenceReason As String

    Public Sub New()
        Me._Date = #1/1/1900#
        Me._URL = String.Empty
        Me._ShiftBgColor = -1
        Me._LeaveBgColor = -1
        Me._ShiftFgColor = 0 ' black
        Me._LeaveFgColor = 0 ' black
        Me._authorised = "02" 'authorised
        Me._DurationType = ""
        Me._Duration = 0.0
        Me._AbsenceType = ""
        Me._AbsenceReason = ""
    End Sub

    Public Sub New(ByVal aDate As Date, _
                   ByVal aURL As String, _
                   ByVal aShiftBgColor As Integer, _
                   ByVal aShiftFgColor As Integer, _
                   ByVal aLeaveBgColor As Integer, _
                   ByVal aLeaveFgColor As Integer, _
                   ByVal aAuthorised As String, _
                   ByVal aDurationType As String, _
                   ByVal aDuration As Decimal, _
                   ByVal aAbsenceType As String, _
                   ByVal aAbsenceReason As String)
        Me._Date = aDate
        Me._URL = aURL
        Me._ShiftBgColor = aShiftBgColor
        Me._LeaveBgColor = aLeaveBgColor
        Me._ShiftFgColor = aShiftFgColor
        Me._LeaveFgColor = aLeaveBgColor
        Me._authorised = aAuthorised
        Me._DurationType = aDurationType
        Me._Duration = aDuration
        Me._AbsenceType = aAbsenceType
        Me._AbsenceReason = aAbsenceReason
    End Sub

#Region "properties"

    Public Property Day() As Date
        Get
            Return _Date
        End Get
        Set(ByVal Value As Date)
            _Date = Value
        End Set
    End Property

    Public Property ShiftBgColor() As Integer
        Get
            Return _ShiftBgColor
        End Get
        Set(ByVal Value As Integer)
            _ShiftBgColor = Value
        End Set
    End Property

    Public Property LeaveBgColor() As Integer
        Get
            Return _LeaveBgColor
        End Get
        Set(ByVal Value As Integer)
            _LeaveBgColor = Value
        End Set
    End Property

    Public Property ShiftFgColor() As Integer
        Get
            Return _ShiftFgColor
        End Get
        Set(ByVal Value As Integer)
            _ShiftFgColor = Value
        End Set
    End Property

    Public Property LeaveFgColor() As Integer
        Get
            Return _LeaveFgColor
        End Get
        Set(ByVal Value As Integer)
            _LeaveFgColor = Value
        End Set
    End Property

    Public Property Authorised() As String
        Get
            Return _authorised
        End Get
        Set(ByVal value As String)
            _authorised = value
        End Set
    End Property

    Public Property DurationType() As String
        Get
            Return _DurationType
        End Get
        Set(ByVal value As String)
            _DurationType = value
        End Set
    End Property

    Public Property Duration() As Decimal
        Get
            Return _Duration
        End Get
        Set(ByVal value As Decimal)
            _Duration = value
        End Set
    End Property

    Public Property AbsenceType() As String
        Get
            Return _AbsenceType
        End Get
        Set(ByVal value As String)
            _AbsenceType = value
        End Set
    End Property

    Public Property AbsenceReason() As String
        Get
            Return _AbsenceReason
        End Get
        Set(ByVal value As String)
            _AbsenceReason = value
        End Set
    End Property

    Public ReadOnly Property isUnauthorisedLeave() As Boolean
        Get
            If Me.LeaveBgColor <> -1 And Me.Authorised = "01" Then
                'if there is a color then there must be leave, 01 is 'not yet authorised'
                Return True
            End If
            Return False
        End Get
    End Property

#End Region

End Class
