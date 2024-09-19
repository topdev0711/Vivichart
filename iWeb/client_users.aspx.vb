Imports System.Globalization
Imports System.Threading
Imports System.Net.Mail

<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class client_users
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Private SQL As String

    Public defLanguage As String = "GBR"

    Public ReadOnly Property client_id As Integer
        Get
            Return ctx.session("client_id")
        End Get
    End Property

    Public Property ModalState() As String
        Get
            Dim B As String = ViewState("ModalState")
            If B = Nothing Then
                ViewState("ModalState") = ""
                B = ""
            End If
            Return B
        End Get
        Set(ByVal value As String)
            ViewState("ModalState") = value
        End Set
    End Property

#End Region

#Region "Users"
    Private Property UserRef As String
        Get
            Return ViewState("UserRef")
        End Get
        Set(value As String)
            ViewState("UserRef") = value
        End Set
    End Property

    Private ReadOnly Property CleanUsersDT As DataTable
        Get
            ViewState("Users") = Nothing
            Return UsersDT
        End Get
    End Property

    Public ReadOnly Property UsersDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Users")
            If DT Is Nothing Then

                ' should be specific fields needed
                SQL = "SELECT U.*,
                              CAST(U.active_user as Bit) as active_user_bool,
                              dbo.dbf_puptext('role','role_name',R.role_name,@p2) as role_name
                         FROM dbo.suser U
                              JOIN dbo.role R
                                ON R.role_ref = U.role_ref
                        WHERE client_id = @p1
                        ORDER BY surname,forename"
                DT = IawDB.execGetTable(SQL, client_id, ctx.languageCode)
                ViewState("Users") = DT
            End If
            Return DT
        End Get
    End Property

    ' get the specific row from the above table
    ReadOnly Property UsersDR() As DataRow
        Get
            Return UsersDT.Select("user_ref = '" + UserRef + "'")(0)
        End Get
    End Property

#End Region
#Region "Brands"
    Private ReadOnly Property CleanBrandsDT As DataTable
        Get
            ViewState("Brands") = Nothing
            Return BrandsDT
        End Get
    End Property

    Public ReadOnly Property BrandsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Brands")
            If DT Is Nothing Then
                Dim defb As String = ctx.Translate("::LT_S0183") ' Use default

                ' should be specific fields needed
                SQL = "SELECT brand_id, brand_name
                         FROM ( SELECT 0 AS brand_id, '" + defb + "' AS brand_name, 0 AS SortOrder
                                  UNION ALL
                                SELECT brand_id, brand_name, 1 AS SortOrder 
                                  FROM dbo.client_brand
                                 WHERE client_id = @p1
                              ) AS CombinedResult
                              ORDER BY SortOrder, brand_name"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Brands") = DT
            End If
            Return DT
        End Get
    End Property
#End Region
#Region "Role"
    Public ReadOnly Property RolesDT As DataTable
        Get
            Dim DT As DataTable = ViewState("RolesDT")
            If DT Is Nothing Then
                ' should be specific fields needed
                If ctx.role = "admin" Then
                    SQL = "SELECT role_ref,
                                  dbo.dbf_puptext('role','role_name',role_name,@p2) as role_name
                             FROM dbo.role
                            ORDER BY role_name"
                Else
                    SQL = "SELECT role_ref,
                                  dbo.dbf_puptext('role','role_name',role_name,@p2) as role_name
                             FROM dbo.role
                            WHERE role_ref in ('client_admin','client_creator','client_user')
                            ORDER BY role_name"
                End If

                DT = IawDB.execGetTable(SQL, client_id, ctx.languageCode)
                ViewState("RolesDT") = DT
            End If
            Return DT
        End Get
    End Property
#End Region
#Region "Language"
    Public ReadOnly Property LanguagesDT As DataTable
        Get
            Dim DT As DataTable = ViewState("LanguagesDT")
            If DT Is Nothing Then
                SQL = "SELECT language_ref, language_name, culture
                         FROM dbo.qlang
                        ORDER BY language_name"
                DT = IawDB.execGetTable(SQL)
                ViewState("LanguagesDT") = DT
            End If
            Return DT
        End Get
    End Property

    ReadOnly Property LanguagesDR(langRef) As DataRow
        Get
            Return LanguagesDT.Select("language_ref = '" + langRef + "'")(0)
        End Get
    End Property

#End Region

#Region "Page Events"

    Private Sub client_users_Init(sender As Object, e As EventArgs) Handles Me.Init
        useSqlViewState = True
    End Sub

    Protected Async Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Try
                Dim c As New WooComm
                Dim a = Await c.GetProducts()
                Dim d = 1
            Catch ex As Exception
                Dim b = 1
            End Try

        End If

        If Not Page.IsPostBack Then
            Populate()
        End If
        SetModalState()
    End Sub

    Private Sub Populate()
        RefreshGrid("Users")
        SetDDLB(ddlbBrand, BrandsDT, "brand_name", "brand_id")
        SetDDLB(ddlbRole, RolesDT, "role_name", "role_ref")
        SetDDLB(ddlbLanguage, LanguagesDT, "language_name", "language_ref")

    End Sub

    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeUserForm.Hide()
        mpeDelete.Hide()

        Select Case ModalState
            Case "Users"
                mpeUserForm.Show()
            Case "DeleteUser"
                mpeDelete.Show()
        End Select
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "Users"
                SetGrid(grdUsers, CleanUsersDT, False)

                Dim Found As Boolean = False
                For Each row As GridViewRow In grdUsers.Rows
                    If row.RowType = DataControlRowType.DataRow Then
                        Dim lbl As Label = DirectCast(row.FindControl("lblEmailWarning"), Label)
                        If lbl.Visible = True Then
                            Found = True
                            Exit For
                        End If
                    End If
                Next
                If Not Found Then
                    grdUsers.Columns(3).Visible = False
                End If

        End Select

    End Sub

#End Region

#Region "Users Grid"

    ' add select row option on grid
    'Sub grdUsers_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdUsers.RowCreated
    '    If e.Row.RowType = DataControlRowType.DataRow Then
    '        e.Row.Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdUsers, "Select$" & e.Row.RowIndex)
    '    End If
    'End Sub

    Private Sub grdUsers_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdUsers.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.Header Then
            If Not ctx.role.ContainsOneOfCI("admin", "client_admin") Then
                CTRL = e.Row.FindControl("btnAdd")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
            End If
        End If

        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim lblWarning As Label = DirectCast(e.Row.FindControl("lblEmailWarning"), Label)
            ' is this the vivichart record
            Dim user_ref As String = rv("user_ref")

            If IawDB.execScalar("select count(1) from dbo.data_view where user_ref = @p1 ", user_ref) > 0 Then
                ' get a handle to the delete button and hide it
                CTRL = e.Row.FindControl("btnDel")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
                CTRL = e.Row.FindControl("lblNoDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = True
                End If
            End If

            Dim email As New MailAddress(rv("email_address"))
            Dim domain As String = email.Host

            If IawDB.execScalar("SELECT CASE WHEN (SELECT COUNT(1)
                                                     FROM dbo.client_domain
                                                    WHERE client_id = @p1) = 0 THEN 1
                                             ELSE CASE WHEN EXISTS(SELECT 1
                                                                     FROM dbo.client_domain
                                                                    WHERE client_id = @p1
                                                                      AND domain_name = @p2) THEN 1
                                                       ELSE 0 
                                                   END
                                        END AS IsDomainValid", client_id, domain) = 0 Then
                lblWarning.Visible = True
            Else
                lblWarning.Visible = False
            End If

        End If
    End Sub

    Sub grdUsers_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdUsers.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        Dim DR As DataRow

        If e.CommandName = "New" Then
            hdnUserRef.Value = ""    ' new 
            btnCopyLoginURL.Visible = False

            ' Initialise the dialog fields here
            txtForename.Text = ""
            txtSurname.Text = ""
            txtEmail.Text = ""
            SetDDLBValue(ddlbBrand, 0)
            SetDDLBValue(ddlbLanguage, "")
            SetDDLBValue(ddlbRole, "client_user")
            cbActive.Checked = True
            FieldFocus(txtForename)

            ModalState = "Users"
            SetModalState()
            Return
        End If

        CurRowIndex = grdUsers.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        UserRef = grdUsers.DataKeys(RowIndex).Value
        DR = UsersDR()

        Select Case e.CommandName

            Case "AmendRow"
                hdnUserRef.Value = UserRef
                btnCopyLoginURL.Visible = True

                'Dim baseUrl As String = Request.Url.GetLeftPart(UriPartial.Authority) & Request.ApplicationPath
                'Dim fullUrl As String = baseUrl.TrimEnd("/"c) & "/secure/login.aspx?user=" + UserRef

                'hdnURL.Value = SawUtil.encryptQuery(fullUrl, True)
                hdnURL.Value = SawUtil.CreateLoginURL(Request, "user=" + UserRef)

                ' assign the values to the popup screen fields
                txtForename.Text = DR.GetValue("forename", "")
                txtSurname.Text = DR.GetValue("surname", "")
                txtEmail.Text = DR.GetValue("email_address", "")
                SetDDLBValue(ddlbRole, DR.GetValue("role_ref", "client_user"))
                SetDDLBValue(ddlbLanguage, DR.GetValue("language_ref", defLanguage))
                SetDDLBValue(ddlbBrand, DR.GetValue("brand_id", 0))
                cbActive.Checked = DR.GetValue("active_user", "0") = "1"

                FieldFocus(txtForename)

                SetModalState("Users")

            Case "DeleteRow"
                SetModalState("DeleteUser")

                'IawDB.execNonQuery("DELETE dbo.suser where user_ref = @p1", UserRef)

                'RefreshGrid("Users")
        End Select
    End Sub

    Private Sub cvEmail_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles cvEmail.ServerValidate
        Try
            ' creating an email address will throw an exception if it's not valid
            Dim email As New MailAddress(txtEmail.Text.Trim)
            Dim domain As String = email.Host

            If IawDB.execScalar("SELECT CASE WHEN (SELECT COUNT(1)
                                                     FROM dbo.client_domain
                                                    WHERE client_id = @p1) = 0 THEN 1
                                             ELSE CASE WHEN EXISTS(SELECT 1
                                                                     FROM dbo.client_domain
                                                                    WHERE client_id = @p1
                                                                      AND domain_name = @p2) THEN 1
                                                       ELSE 0 
                                                   END
                                        END AS IsDomainValid", client_id, domain) = 0 Then
                cvEmail.ErrorMessage = ctx.Translate("::LT_S0184") ' LT_S0184 - This is not a valid domain
                args.IsValid = False
            Else
                args.IsValid = True
            End If

        Catch ex As FormatException
            cvEmail.ErrorMessage = ctx.Translate("::LT_S0092") ' LT_S0092 - This is not a valid email address
            args.IsValid = False
        End Try
    End Sub

    Private Sub btnUserCancel_Click(sender As Object, e As EventArgs) Handles btnUserCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnUserSave_Click(sender As Object, e As EventArgs) Handles btnUserSave.Click
        Dim UserID As Integer

        If Not Page.IsValid Then Return

        Dim DR As DataRow = LanguagesDR(ddlbLanguage.SelectedValue)
        Dim MailResult As SawPasswordWorked

        Using DB As New IawDB

            If hdnUserRef.Value = "" Then
                ' we put a guid into the user_ref because the initial insert has to have a value.  the
                ' trigger on the suser table will then assign an identify field value to the user_ref
                Dim guidStr As String = Guid.NewGuid.ToString("N")
                Dim userRef As String = guidStr.Substring(guidStr.Length - 10)

                SQL = "INSERT dbo.suser (user_ref,client_id, forename,surname,email_address,user_name,role_ref,language_ref,brand_id,active_user,locale_ref,
                                         prof_status,psc_status,administrator,mfa_pin_date,mfa_pin,mfa_status)
                       VALUES (@p1,@p2,@p3,@p4,@p5,@p5,@p6,@p7,@p8,@p9,@p10,
                               '01','01','0','1900-01-01','','');
                       SELECT SCOPE_IDENTITY()"
                UserID = DB.Scalar(SQL, userRef, client_id,
                                        txtForename.Text.Trim, txtSurname.Text.Trim, txtEmail.Text.Trim,
                                        ddlbRole.SelectedValue, ddlbLanguage.SelectedValue, ddlbBrand.SelectedValue,
                                        If(cbActive.Checked, "1", "0"), DR.GetValue("culture", "en-GB")
                                   )

                MailResult = SawPassword.EmailNewPassword(txtEmail.Text.Trim)
                If Not MailResult.Worked Then
                    txtErrorMessage.Visible = True
                    txtErrorMessage.Text = MailResult.Message
                    DB.NonQuery("delete from dbo.suser where user_id = @p1", UserID)
                    Return
                End If
                txtErrorMessage.Visible = False

            Else
                SQL = "UPDATE dbo.suser
                          SET forename = @p2,
                              surname = @p3,
                              email_address = @p4,
                              role_ref = @p5,
                              language_ref = @p6,
                              brand_id = @p7,
                              active_user = @p8,
                              locale_ref = @p9
                        WHERE user_ref = @p1"
                DB.NonQuery(SQL, hdnUserRef.Value,
                                 txtForename.Text.Trim,
                                 txtSurname.Text.Trim,
                                 txtEmail.Text.Trim,
                                 ddlbRole.SelectedValue,
                                 ddlbLanguage.SelectedValue,
                                 ddlbBrand.SelectedValue,
                                 If(cbActive.Checked, "1", "0"),
                                 DR.GetValue("culture", "en-GB")
                                )

                ' have we updated the current user ?  if so update the language
                If ctx.user_ref = hdnUserRef.Value Then
                    ctx.session("language") = ddlbLanguage.SelectedValue
                    Dim Culture As String
                    Culture = IawDB.execScalar("select culture from dbo.qlang where language_ref = @p1", ddlbLanguage.SelectedValue)
                    ctx.session("locale_ref") = Culture
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Culture)
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(Culture)
                    ctx.rebuildMenu()
                End If
            End If
        End Using

        RefreshGrid("Users")
        SetModalState("")
    End Sub

    Private Sub btnDeleteCancel_Click(sender As Object, e As EventArgs) Handles btnDeleteCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnDeleteOk_Click(sender As Object, e As EventArgs) Handles btnDeleteOk.Click
        IawDB.execNonQuery("DELETE dbo.suser where user_ref = @p1", UserRef)
        RefreshGrid("Users")
        SetModalState("")
    End Sub

#End Region

End Class