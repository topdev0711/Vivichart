Imports Microsoft.VisualBasic
Imports System.diagnostics
Imports System.Collections.generic

Public Class SawDBConnList

    Public Shared Function AddDBConnList() As Integer
        Dim NewID As Integer
        Dim sTrace As New StackTrace(True)

        If ctx.item("DBConnListTop") Is Nothing Then
            ctx.item("DBConnListTop") = 0
            ctx.item("DBConnList") = New Dictionary(Of Integer, StackTrace)
        End If

        NewID = ctx.item("DBConnListTop") + 1
        ctx.item("DBConnListTop") = NewID
        CType(ctx.item("DBConnList"), Dictionary(Of Integer, StackTrace)).Add(NewID, sTrace)

        Return NewID
    End Function

    Public Shared Sub DeleteDBConnList(ByVal ListID As Integer)
        CType(ctx.item("DBConnList"), Dictionary(Of Integer, StackTrace)).Remove(ListID)
    End Sub

    Public Shared Function GetDBConnList() As String
        Dim LogText() As String
        Dim SplitLine() As String
        Dim FileLine() As String
        Dim strReturn As String = String.Empty
        Dim sTrace As StackTrace
        Dim ConnList As Dictionary(Of Integer, StackTrace) = ctx.item("DBConnList")

        If ConnList Is Nothing Then Return strReturn

        For Each KVP As KeyValuePair(Of Integer, StackTrace) In ConnList
            sTrace = KVP.Value

            LogText = sTrace.GetFrame(3).ToString.Split(" ")
            SplitLine = LogText(6).Split("\")
            Dim i As Integer = SplitLine.GetUpperBound(0)
            FileLine = SplitLine(i).Split(":")
            If FileLine.Length = 1 And LogText.Length > 7 Then
                SplitLine = LogText(7).Split("\")
                i = SplitLine.GetUpperBound(0)
                FileLine = SplitLine(i).Split(":")
            End If
            If FileLine.Length > 1 Then
                strReturn += LogText(0) + "() " + FileLine(0) + ":" + FileLine(1) + "<br>"
            Else
                strReturn += LogText(0) + "() " + FileLine(0) + ":<br>"
            End If

        Next

        Return strReturn
    End Function

End Class

