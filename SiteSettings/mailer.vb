Imports Microsoft.VisualBasic
Imports System.net.Mail
Imports System.Collections.Generic


Public Class Mailer
    Public Shared iErrorMessage As String = String.Empty

    Public Shared Function SendMail(ByVal recipients As String, ByVal subject As String, ByVal body As String) As Boolean
        Dim DB As DBConn
        Dim ls_sql As String
        Dim returnVal As Boolean = True
        Dim message As String = body
        Dim smtpHost As String
        Dim fromEmail As String
        Dim smtpPort As String
        Dim smtpSSL As String
        Dim smtpTimeout As Integer
        Dim emailAddresses As String = recipients
        Dim emailSubject As String = subject
        Dim emailUser As String
        Dim emailPwd As String

        'setup email parameters
        DB = New DBConn(Form1.txtDbServer.Text, _
                        Form1.txtDbName.Text, _
                        Form1.numMinPool.Value, _
                        Form1.numMaxPool.Value, _
                        Form1.numConnectionTimeout.Value)
        DB.Open()

        ls_sql = "SELECT smtp_host, " _
           + "       ISNULL(smtp_port,25) as smtp_port, " _
           + "       (SELECT CASE " _
           + "          WHEN ISNULL(smtp_ssl,'0') = '1' THEN 'true' ELSE 'false' " _
           + "           END ) AS smtp_ssl, " _
           + "       smtp_from_name, " _
           + "       smtp_from_email, " _
           + "       ISNULL(mail_message_user,'') as mail_message_user, " _
           + "       ISNULL(mail_message_pwd,'') as mail_message_pwd, " _
           + "       ISNULL(smtp_timeout,30) as smtp_timeout " _
           + "  FROM dbo.GLOBAL_SYSADM "

        DB.Query(ls_sql)
        DB.Reader.Read()
        smtpHost = DB.GetValue("smtp_host", "")
        smtpSSL = DB.Reader("smtp_ssl")
        smtpPort = DB.Reader("smtp_port")
        smtpTimeout = DB.Reader("smtp_Timeout")
        emailUser = DB.Reader("mail_message_user")
        emailPwd = DB.Reader("mail_message_pwd")
        fromEmail = DB.GetValue("smtp_from_email", "")
        DB.Reader.Close()
        DB.Close()

        If String.IsNullOrEmpty(emailAddresses) Or String.IsNullOrEmpty(fromEmail) Then
            'don't send email
            Return False
        End If

        'setup the email client
        Dim emailClient As New SmtpClient(smtpHost, smtpPort)

        'setup credentials (if supplied)
        If Not String.IsNullOrEmpty(emailUser) And Not String.IsNullOrEmpty(emailPwd) Then
            emailClient.Credentials = New Net.NetworkCredential(emailUser, emailPwd)
        End If

        emailClient.EnableSsl = smtpSSL
        emailClient.Timeout = smtpTimeout * 1000 ' miliseconds!

        'setup the message
        Dim msg As New System.Net.Mail.MailMessage()
        msg.From = New MailAddress(fromEmail)
        msg.Subject = emailSubject
        msg.Body = message
        For Each s As String In emailAddresses.Split(";")
            msg.To.Add(New MailAddress(s))
        Next

        msg.IsBodyHtml = True

        Try
            emailClient.Send(msg)
        Catch excm As Exception
            If excm.InnerException IsNot Nothing Then
                iErrorMessage = excm.Message + "  " + excm.InnerException.Message
            Else
                iErrorMessage = excm.Message
            End If
            emailClient = Nothing
            returnVal = False
        End Try

        Return returnVal

    End Function

    ''' <summary>
    ''' Returns true if there is an SMTP host AND a from email address configured.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function IsEmailConfigured() As Boolean
        Dim DB As DBConn
        Dim ls_sql As String
        Dim smtpHost As String
        Dim fromEmail As String

        'setup email parameters
        DB = New DBConn(Form1.txtDbServer.Text, _
                        Form1.txtDbName.Text, _
                        Form1.numMinPool.Value, _
                        Form1.numMaxPool.Value, _
                        Form1.numConnectionTimeout.Value)
        DB.Open()

        ls_sql = "SELECT smtp_host, " _
               + "       smtp_from_email " _
               + "  FROM dbo.GLOBAL_SYSADM "

        DB.Query(ls_sql)
        DB.Reader.Read()
        smtpHost = DB.GetValue("smtp_host", "")
        fromEmail = DB.GetValue("smtp_from_email", "")
        DB.Reader.Close()
        DB.Close()
        DB = Nothing

        If String.IsNullOrEmpty(smtpHost) Or String.IsNullOrEmpty(fromEmail) Then
            Return False
        Else
            Return True
        End If

    End Function

End Class
