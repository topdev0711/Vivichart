Imports System.IO
Imports System.Web.Hosting

Public Class SawDiag
    Private Shared LogFileLock As New Object()

    Public Shared Indent As Integer = 0
    Public Shared DataFolder As String = ""

    Shared Function CheckDataFolderExists() As Boolean
        If DataFolder = "" Then
            DataFolder = HostingEnvironment.MapPath("~/App_Data/")
        End If
        Return Directory.Exists(DataFolder)
    End Function

    Shared Function GetDebugLevel() As Integer
        Dim ConfigLevel As String = ConfigurationManager.AppSettings("IawDebugLevel")
        If String.IsNullOrEmpty(ConfigLevel) Then Return 0
        Dim Level As Integer
        If Not Integer.TryParse(ConfigLevel, Level) Then
            Level = 0
        End If

        If Not CheckDataFolderExists() Then
            Level = 0
        End If
        Return Level
    End Function
    Shared Sub LogSvcError(txt As String)
        LogError(txt, "svc_")
    End Sub
    Shared Sub LogSvcInfo(txt As String)
        logInfo(txt, "svc_")
    End Sub
    Shared Sub LogSvcVerbose(txt As String)
        LogVerbose(txt, "svc_")
    End Sub
    Shared Sub LogError(txt As String, Optional Prefix As String = "")
        If GetDebugLevel() = 0 Then Return
        WriteLog(txt, Prefix)
    End Sub
    Shared Sub logInfo(txt As String, Optional Prefix As String = "")
        If GetDebugLevel() < 2 Then Return
        WriteLog(txt, Prefix)
    End Sub
    Shared Sub LogVerbose(txt As String, Optional Prefix As String = "")
        If GetDebugLevel() < 3 Then Return
        WriteLog(txt, Prefix)
    End Sub

    Public Shared Sub Purge()
        ' remove any files older than 2 days
        Try
            If Not CheckDataFolderExists() Then
                Return
            End If

            Dim di As New IO.DirectoryInfo(DataFolder)
            Dim diar1 As IO.FileInfo() = di.GetFiles("*.log")
            Dim dra As IO.FileInfo

            'list the names of all files in the specified directory
            For Each dra In diar1
                If dra.LastWriteTime.Date.AddDays(2) < Today Then
                    File.Delete(dra.FullName)
                End If
            Next
        Catch ex As Exception
            ' not too worried if this fails
        End Try
    End Sub

    Private Shared Sub WriteLog(Txt As String, Prefix As String)
        Dim FileName As String = ""
        If HttpContext.Current Is Nothing Then Return
        If HttpContext.Current.Server Is Nothing Then Return
        Try
            Dim FN As String = DataFolder

            If Prefix <> "" AndAlso Not Prefix.EndsWith("_") Then
                Prefix += "_"
            End If
            FileName = Path.Combine(FN, String.Format("{0}{1:yyyy-MM-dd}.log", Prefix, Now))
            Dim Line As String

            Line = String.Format("{0:HH:mm:ss.fff} : {1}{2}", Now, New String(" ", Indent * 4), Txt)

            SyncLock LogFileLock
                My.Computer.FileSystem.WriteAllText(FileName, Line + Environment.NewLine, True)
            End SyncLock

            Debug.WriteLine(Line)
        Catch ex As Exception
            ctx.LogError(ex)
            'ctx.LogError(New Exception("Couldn't write to the log file : " + FileName), False, "")
        End Try
    End Sub

End Class
