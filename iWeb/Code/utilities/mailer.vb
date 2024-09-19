Imports Microsoft.VisualBasic
Imports System.net.Mail
Imports System.Collections.Generic

Public Class mailer

    Public Enum MailMethod
        Web_Server = 1
        Background_Server = 2
    End Enum

    ''' <summary>
    ''' Determines whether an email address is valid
    ''' </summary>
    Public Shared Function ValidEmail(ByVal Address As String) As Boolean
        Try
            Dim Addr As New MailAddress(Address)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    ''' <summary>
    ''' Returns the email method that will be used
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Throws an exception if an unrecognised type is used</remarks>
    Public Shared ReadOnly Property EmailMethod() As MailMethod
        Get
            Dim value As String = SawDB.ExecScalar("SELECT Min(web_send_email) FROM dbo.global_self_serv WITH (NOLOCK)")
            Select Case value
                Case "01"
                    Return MailMethod.Web_Server

                Case "02"
                    Return MailMethod.Background_Server

                Case Else
                    Throw New Exception(String.Format("Mailer is not configured for EmailMethod of type - ", value))

            End Select
        End Get
    End Property

    'send sync
    Public Shared Function SendEmail(ByVal recipients As String, ByVal subject As String, ByVal body As String) As Boolean
        Return mailer.SendMail(recipients, subject, body, False)
    End Function

    'send sync
    Public Shared Function SendEmail(ByVal recipients As List(Of String), ByVal subject As String, ByVal body As String) As Boolean
        Return mailer.SendMail(String.Join(";", recipients.ToArray()), subject, body, False)
    End Function

    Private Shared Function SendMail(ByVal recipients As String, ByVal subject As String, ByVal body As String, ByVal async As Boolean) As Boolean
        Select Case EmailMethod
            Case MailMethod.Web_Server
                'web server
                Return SendMailWebServer(recipients, subject, body, False)

            Case MailMethod.Background_Server
                'BG server
                Return SendMailBGServer(recipients, subject, body, False)

        End Select
    End Function

    ''' <summary>
    ''' Sends mail via the BG server by inserting rows into q_e_mess_sent
    ''' </summary>
    ''' <param name="recipients"></param>
    ''' <param name="subject"></param>
    ''' <param name="body"></param>
    ''' <param name="async"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function SendMailBGServer(ByVal recipients As String, ByVal subject As String, ByVal body As String, ByVal async As Boolean) As Boolean
        Dim unique_id As Integer
        Dim sql As String
        sql = " INSERT INTO dbo.Q_E_MESS_SENT " _
             + "            (e_message_ref, " _
             + "             unique_id, " _
             + "             e_mail_address, " _
             + "             mail_subject_text, " _
             + "             mail_message_text, " _
             + "             status) " _
             + "      VALUES('q99999', " _
             + "             ??,          " _
             + "             ??, " _
             + "             ??, " _
             + "             ??, " _
             + "             '01') "

        For Each addr As String In recipients.Split(";")
            unique_id = SawUtil.GetKey("Q_E_MESS_SENT", "unique_id")
            SawDB.ExecNonQuery(sql, New Object() {unique_id, addr, subject, body})
        Next

        Return True

    End Function


    ''' <summary>
    ''' Uses System.Net.Mail to send mail from the web server
    ''' </summary>
    ''' <param name="recipients"></param>
    ''' <param name="subject"></param>
    ''' <param name="body"></param>
    ''' <param name="async"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function SendMailWebServer(ByVal recipients As String, ByVal subject As String, ByVal body As String, ByVal async As Boolean) As Boolean
        Dim DB As SawDB
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
        DB = New SawDB
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
           + "  FROM GLOBAL_SYSADM WITH (NOLOCK)"

        DB.Query(ls_sql)
        DB.Read()
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

        'if the from email is empty or not a proper email address
        'ignore let the app throw an error
        'If String.IsNullOrEmpty(fromEmail) Then
        'End If

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
            If async Then
                emailClient.SendAsync(msg, Nothing)
            Else
                emailClient.Send(msg)
            End If
        Catch excm As Exception
            LogException.HandleException(excm, False)
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
        Dim DB As SawDB
        Dim ls_sql As String
        Dim smtpHost As String
        Dim fromEmail As String

        'setup email parameters
        DB = New SawDB
        ls_sql = "SELECT smtp_host, " _
           + "       smtp_from_email " _
           + "  FROM GLOBAL_SYSADM WITH (NOLOCK) "

        DB.Query(ls_sql)
        DB.Read()
        smtpHost = DB.GetValue("smtp_host", "")
        fromEmail = DB.GetValue("smtp_from_email", "")
        DB.Reader.Close()
        DB.Close()

        If String.IsNullOrEmpty(smtpHost) Or String.IsNullOrEmpty(fromEmail) Then
            Return False
        Else
            Return True
        End If

    End Function

End Class
