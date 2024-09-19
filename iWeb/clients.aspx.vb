Imports System.Net.Mail
Imports System.IO

<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class clients
    Inherits stub_IngenWebPage
#Region "Common Properties"
    Private SQL As String
    Public defRole As String = "client_admin"
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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Populate()

        End If
        SetModalState()
    End Sub
    Private Sub Populate()
        RefreshGrid("Clients")

    End Sub
    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeClientForm.Hide()
        mpeDelete.Hide()

        Select Case ModalState
            Case "Clients"
                mpeClientForm.Show()
            Case "DeleteClient"
                mpeDelete.Show()
        End Select
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "Clients"
                SetGrid(grdClients, CleanClientsDT, False)

                Dim Found As Boolean = False
                For Each row As GridViewRow In grdClients.Rows
                    If row.RowType = DataControlRowType.DataRow Then
                        Dim lbl As Label = DirectCast(row.FindControl("lblEmailWarning"), Label)
                        If lbl.Visible = True Then
                            Found = True
                            Exit For
                        End If
                    End If
                Next
                If Not Found Then
                    grdClients.Columns(2).Visible = False
                End If

        End Select

    End Sub
#Region "Clients"
    Private Property ClientRef As String
        Get
            Return ViewState("ClientRef")
        End Get
        Set(value As String)
            ViewState("ClientRef") = value
        End Set
    End Property
    Private ReadOnly Property CleanClientsDT As DataTable
        Get
            ViewState("Clients") = Nothing
            Return ClientsDT
        End Get
    End Property

    Public ReadOnly Property ClientsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Clients")
            If DT Is Nothing Then
                SQL = "SELECT * from dbo.clients"
                DT = IawDB.execGetTable(SQL)

                ViewState("Clients") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property ClientsDR() As DataRow
        Get
            Return ClientsDT.Select("client_id = '" + ClientRef + "'")(0)
        End Get
    End Property
#End Region
    Private Sub grdClients_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdClients.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.Header Then
            If ctx.role <> "admin" Then
                CTRL = e.Row.FindControl("btnAdd")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
            End If
        End If

        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = DirectCast(e.Row.DataItem, DataRowView)
            Dim lblWarning As Label = DirectCast(e.Row.FindControl("lblEmailWarning"), Label)

            Dim client_id As Integer = grdClients.DataKeys(e.Row.RowIndex).Value

            Dim email As New MailAddress(rv("client_email"))
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
    Sub grdClients_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdClients.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer

        If e.CommandName = "New" Then
            ModalState = "Clients"
            SetModalState()
            Return
        End If

        CurRowIndex = grdClients.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        ClientRef = grdClients.DataKeys(RowIndex).Value
        'DR = ClientsDR()

        Select Case e.CommandName

            Case "AmendRow"
                Dim url = SawUtil.encryptQuery("client_details.aspx?ClientID=" + ClientRef.ToString, True)
                ctx.redirect(url)
            Case "DeleteRow"
                SetModalState("DeleteClient")
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
                cvEmail.ErrorMessage = "This is not a valid domain"
                args.IsValid = False
            Else
                args.IsValid = True
            End If

        Catch ex As FormatException
            cvEmail.ErrorMessage = "This is not a valid email address"
            args.IsValid = False
        End Try
    End Sub
    Private Sub btnClientSave_Click(sender As Object, e As EventArgs) Handles btnClientSave.Click
        If Not Page.IsValid Then Return

        Dim DR As DataRow = LanguagesDR(defLanguage)
        Dim ClientID As Integer = 0

        Dim Cust_id As String = CInt(txtCustId.Text)
        Dim Forename As String = txtForename.Text.Trim
        Dim Surname As String = txtSurname.Text.Trim
        Dim Fullname As String = Forename + " " + Surname
        Dim Company As String = txtCompanyName.Text.Trim
        Dim Email As String = txtEmail.Text.Trim

        Dim guidStr As String = Guid.NewGuid.ToString("N")
        Dim userRef As String = guidStr.Substring(guidStr.Length - 10)

        ClientID = IawDB.execScalar("INSERT dbo.clients (client_cust_id, client_name, client_email, 
                            zip_password, date_format, time_format, attributes, max_string_len,
                            max_number, max_image_size, max_data)
                            VALUES (@p1,@p2,@p3,'','dd/MM/yyyy','hh:mm:ss',@p4,500,9999999999.990000,50000,50)
                            SELECT SCOPE_IDENTITY()",
                           Cust_id, Company, Email, New ChartAttrib().GetData.ToString)

        SQL = "INSERT dbo.suser (user_ref,client_id, display_name, forename,surname,email_address,user_name,role_ref,language_ref,brand_id,active_user,locale_ref,
                                 prof_status,psc_status,administrator,mfa_pin_date,mfa_pin,mfa_status)
               VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p6,@p7,@p8,@p9,@p10,@p11,
                       '01','01','0','1900-01-01','','00')"
        IawDB.execNonQuery(SQL, userRef, ClientID, Fullname,
                                Forename, Surname, Email,
                                defRole, defLanguage, 0,
                                "1", DR.GetValue("culture", "en-GB")
                                )
        SawPassword.EmailNewPassword(Email)

        RefreshGrid("Clients")
        SetModalState("")
    End Sub
    Private Sub btnClientCancel_Click(sender As Object, e As EventArgs) Handles btnClientCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnDeleteOk_Click(sender As Object, e As EventArgs) Handles btnDeleteOk.Click
        IawDB.execNonQuery("DELETE dbo.suser where client_id = @p1;
                            DELETE dbo.clients where client_id = @p1",
                           ClientRef)

        RefreshGrid("Clients")
        SetModalState("")
    End Sub
    Private Sub btnDeleteCancel_Click(sender As Object, e As EventArgs) Handles btnDeleteCancel.Click
        SetModalState("")
    End Sub
End Class