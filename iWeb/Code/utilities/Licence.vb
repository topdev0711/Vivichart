Public Class Licence

    ''' <summary>
    ''' Works out what the licence state is..
    '''  Old method, can return webpayonly or core
    '''  new method, can return core or not licensed (we don't bother with webpayonly anymore)
    ''' </summary>
    ''' <returns>WebType enumeration</returns>
    Public Shared Function Check() As WebType
        If AppInUse("web") Then
            Return WebType.iWebCore
        Else
            Return WebType.iWebUnlicensed
        End If
    End Function

    Public Shared Function AppInUse(ByVal lic As String) As Boolean
        Dim Crypt As New Cryptography
        Dim LicStr As String = Crypt.decrypt(SawDB.ExecScalar("SELECT licence FROM dbo.licensee"), "ufz4Ht50", "uVhW0CzF")
        Dim x As Integer = 1
        Using DB As New IawDB
            If x = 1 Then x = 2
            DB.Query(LicStr)
            If DB.Read Then
                Dim DE As Date = DB.Reader("expiry_date")
                If DE < Today Then
                    LicStr = ""
                End If
                DB.Reader.Close()
            End If

        End Using

        Return LicStr.Contains(lic + "='1'")
    End Function

End Class
