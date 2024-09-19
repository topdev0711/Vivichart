Imports Microsoft.VisualBasic

<Serializable()> _
Public Class YearCell

    Public Day As Date
    Public URL As String
    Public ShiftBgColor As Integer
    Public LeaveBgColor As Integer
    Public ShiftFgColor As Integer
    Public LeaveFgColor As Integer
    Public Authorised As Boolean
    Public Tooltip As String
    Public DurationType As String

    Public Sub New()
        Day = #1/1/1900#
        URL = String.Empty
        ShiftBgColor = -1
        LeaveBgColor = -1
        ShiftFgColor = 0 ' black
        LeaveFgColor = 0 ' black
        Authorised = False
        Tooltip = String.Empty
        DurationType = "01"
    End Sub

    Public Sub New(ByVal aDate As Date, ByVal aURL As String, ByVal aShiftBgColor As Integer, ByVal aShiftFgColor As Integer, ByVal aLeaveBgColor As Integer, ByVal aLeaveFgColor As Integer, ByVal aAuthorised As Boolean, ByVal aDurationType As String)
        Day = aDate
        URL = aURL
        ShiftBgColor = aShiftBgColor
        LeaveBgColor = aLeaveBgColor
        ShiftFgColor = aShiftFgColor
        LeaveFgColor = aLeaveBgColor
        Authorised = aAuthorised
        Tooltip = String.Empty
        DurationType = aDurationType
    End Sub


End Class