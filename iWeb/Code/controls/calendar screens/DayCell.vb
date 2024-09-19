Imports Microsoft.VisualBasic

<Serializable()> _
Public Class DayCell

    Public Day As Date
    Public URL As String
    Public BgColor As Integer
    Public Color As Integer
    Public Authorised As Boolean
    Public Tooltip As String
    Public ReturnHere As Boolean
    Public DisplayOnly As Boolean
    Public DurationType As String

    Public Sub New()
        Day = #1/1/1900#
        URL = String.Empty
        BgColor = 16777215 ' white
        Color = 0 ' black
        Authorised = False
        Tooltip = String.Empty
        ReturnHere = False
        DisplayOnly = False
        DurationType = "01"
    End Sub

    Public Sub New(ByVal aDate As Date,
                   ByVal aURL As String,
                   ByVal aBgColor As Integer,
                   ByVal aColor As Integer,
                   ByVal aAuthorised As Boolean,
                   ByVal aDurationType As String)
        Day = aDate
        URL = aURL
        BgColor = aBgColor
        Color = aColor
        Authorised = aAuthorised
        Tooltip = String.Empty
        ReturnHere = False
        DisplayOnly = False
        DurationType = aDurationType
    End Sub

End Class



