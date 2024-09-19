
Imports System.Globalization
Imports System.Threading

Public Class SawAuth

    Public Shared ReadOnly Property UsingHiddenLogin() As Boolean
        Get
            Select Case ctx.siteConfig("authentication_method").ToString
                Case "Network Logon", "Single Sign On", "Active Directory"
                    Return True
                Case Else
                    Return False
            End Select
        End Get
    End Property

    Public Enum Validate
        failed = 0
        ok = 1
        change_pwd = 2
        change_qu = 3
        locked_out = 4
    End Enum

    Public Shared Function ValidateUser(ByVal aUserID As String, ByVal aPassword As String) As SawAuth.Validate
        Dim sql As String
        Dim DT As DataTable
        Dim DR As DataRow

        Dim password As String
        Dim ls_user As String
        Dim PSC_Status As String = ""
        Dim UserRef As String = ""
        Dim validatePassword As SawAuth.Validate
        Dim AdminUser As Boolean

        ls_user = aUserID.Trim.ToLower
        password = aPassword.Trim

        If ls_user = String.Empty Or password = String.Empty Then
            Return Validate.failed
        End If

        'DB = New SawDB()

        Using DB As New IawDB

            ' See if the user exists
            sql = "SELECT U.user_ref, U.forename, U.surname, U.administrator, U.active_user,
                          U.psc_status, U.prof_status, C.client_id, C.client_name,
                          IsNull(L.language_ref,'') as language_ref,
                          IsNull(L.culture,'') as locale_ref
                     FROM dbo.suser U WITH (NOLOCK)
                          JOIN dbo.clients C WITH (NOLOCK)
                            ON C.client_id = U.client_id
                          JOIN dbo.qlang L WITH (NOLOCK)
                            ON L.language_ref = U.language_ref
                    WHERE lower(U.user_name) = lower(@p1)"
            DT = DB.GetTable(sql, ls_user)
            If DT.Rows.Count = 0 Then
                Return Validate.failed
            End If

            For Each DR In DT.Rows
                If DR("prof_status") <> "01" Then
                    WriteWebLog("Inactive User")
                    ClearSessionVars()
                    Return Validate.failed
                End If

                If DR("active_user") <> "1" Then
                    WriteWebLog("User not active")
                    ClearSessionVars()
                    Return Validate.failed
                End If

                AdminUser = DR("administrator") = "1"
                PSC_Status = DR("psc_status")
                UserRef = DR("user_ref")
                ctx.session("user_ref") = UserRef
                ctx.session("administrator") = AdminUser
                ctx.session("user_name") = DR.GetValue("forename", "") + " " + DR.GetValue("surname", "")
                ctx.session("client_id") = DR("client_id")
                ctx.session("client_name") = DR("client_name")

                If DR.GetValue("language_ref", "") <> "" Then
                    ctx.session("language") = DR("language_ref")
                End If
                If DR.GetValue("locale_ref", "") <> "" Then
                    ctx.session("locale_ref") = DR("locale_ref")
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(DR("locale_ref"))
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(DR("locale_ref"))
                    ctx.session("CultureName") = Thread.CurrentThread.CurrentCulture.Name
                    ctx.session("Culture") = Thread.CurrentThread.CurrentCulture

                    ' check that the language is listed in qlang.. if not, add it.

                    sql = "IF NOT EXISTS (SELECT top 1 1 FROM dbo.qlang WITH (NOLOCK) where language_ref = @p1)
                               INSERT INTO dbo.QLANG (language_ref,language_name,font_family) VALUES (@p1,@p2,'Latin')"
                    IawDB.execNonQuery(sql, ctx.session("language"), Thread.CurrentThread.CurrentCulture.NativeName)

                End If

                Exit For
            Next

            If password = "^^^" And Not UsingHiddenLogin Then
                WriteWebLog("Invalid Password")
                ClearSessionVars()
                Return Validate.failed
            End If

            'skip check if special network login
            If password <> "^^^" Then
                '*******************************************
                ' validate password
                '*******************************************
                validatePassword = SawPassword.ValidatePassword(UserRef, password)
                Select Case validatePassword
                    Case Validate.failed, Validate.locked_out
                        ClearSessionVars()
                        Return validatePassword
                End Select
            End If

        End Using

        ' log the fact that a user loggged in
        WriteWebLog("Logged In")

        'logged in ok
        sql = "UPDATE dbo.suser " _
            + "   SET login_fail_count = 0, " _
            + "       login_fail_date= NULL, " _
            + "       login_ok_date= ?? " _
            + " WHERE user_ref = ?? "
        SawDB.ExecScalar(sql, New Object() {Date.Now, UserRef})

        ctx.rebuildMenu()
        If Not UsingHiddenLogin Then
            If PSC_Status = "03" Then
                'password has expired or needs changing at next login
                WriteWebLog("Password Change Required")
                Return Validate.change_pwd   ' change pwd
            End If
        End If

        Return Validate.ok    ' login logged in ok
    End Function

    Private Shared Sub WriteWebLog(prefix As String)
        SawUtil.Log(String.Format("{0} - {1} : {2} : {3}{4}",
                                  prefix,
                                  ctx.session("user_name"),
                                  ctx.session("client_name"),
                                  ctx.session("culturename"),
                                  IIf(ctx.session("administrator") = "1", " :ADMIN", "")))

    End Sub

    Private Shared Sub ClearSessionVars()
        ctx.session("user_ref") = Nothing
        ctx.session("user_name") = Nothing
        ctx.session("client_id") = Nothing
        ctx.session("client_name") = Nothing
        ctx.session("administrator") = Nothing
    End Sub

End Class

