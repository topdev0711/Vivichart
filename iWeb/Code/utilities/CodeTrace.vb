
Public Class CodeTrace2

    ' used to keep track of trace calls from a single page
    Public UniqueNum As Integer
    Public TraceDic As Dictionary(Of String, ProcTrace)

    Public Sub New()
        UniqueNum = 0
        TraceDic = New Dictionary(Of String, ProcTrace)
        IawDB.execNonQuery("truncate table dbo.web_trace")

    End Sub

    Public Function Enter(ByVal txt As String) As Integer
        Dim sf As New Diagnostics.StackFrame(1)
        Dim Method As String = sf.GetMethod.Name

        If Not TraceDic.ContainsKey(Method) Then
            TraceDic.Add(Method, New ProcTrace)
        End If

        With TraceDic(Method)
            .txt = txt
            .Stamp = Now
        End With

    End Function

    Public Sub Leave()
        Dim sf As New Diagnostics.StackFrame(1)
        Dim Method As String = sf.GetMethod.Name
        Dim Total As Integer
        Dim Txt As String

        If Not TraceDic.ContainsKey(Method) Then Return

        With TraceDic(Method)
            Total += (Now - .Stamp).TotalMilliseconds
            Txt = .txt
        End With

        IawDB.execNonQuery("insert dbo.web_trace(method, txt, total) values (@p1,@p2,@p3)", Method, Txt, Total)
    End Sub
End Class

Public Class ProcTrace
    Public Stamp As Date
    Public txt As String

    Public Sub New()
        Stamp = Nothing
        txt = ""
    End Sub

End Class







