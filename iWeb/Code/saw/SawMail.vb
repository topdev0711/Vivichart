Imports System.Net.Mail
Imports System.Net

Public Class SawMail
    Public Shared Function Send(ByVal aFrom As String, ByVal aTo As String, ByVal aCC As String, ByVal aSubj As String, ByVal aMsg As String) As String
        Dim Result As String = String.Empty
        Dim myMail As MailMessage

        Dim _smtpServer As String = SawDB.ExecScalar("select top 1 smtp_host from global_sysadm WITH (NOLOCK)")
        Dim lServerPort As Integer = SawDB.ExecScalar("select top 1 smtp_port from global_sysadm WITH (NOLOCK)")
        Dim lUsername As String = SawDB.ExecScalar("select top 1 mail_message_user from global_sysadm WITH (NOLOCK)", String.Empty)
        Dim lPAssword As String = SawDB.ExecScalar("select top 1 mail_message_pwd from global_sysadm WITH (NOLOCK)", String.Empty)

        Try
            myMail = New MailMessage(New MailAddress(aFrom), New MailAddress(aTo))
            If aCC <> String.Empty Then
                myMail.CC.Add(New MailAddress(aCC))
            End If
            myMail.Subject = aSubj
            myMail.Body = aMsg

            Dim client As New SmtpClient()

            'leave in for now SJB 13/09/05
            If lUsername <> String.Empty And lPAssword <> String.Empty Then
                Dim myCredentials As New NetworkCredential(lUsername, lPAssword)
                client.Credentials = myCredentials
            End If

            client.Port = lServerPort
            If _smtpServer IsNot Nothing Or _smtpServer <> "" Then
                client.Host = _smtpServer
            End If

            client.Send(myMail)
        Catch e As Exception
            Result = e.Message
        End Try
        Return Result
    End Function

End Class
