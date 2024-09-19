Imports System.IO
Imports System.Net.Mail
Imports NPOI.HPSF

<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class client_details
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Private SQL As String

    Public defRole As String = "client_admin"
    Public defLanguage As String = "GBR"
    Public ReadOnly Property client_id As Integer
        Get
            If ctx.item("ClientID") IsNot Nothing Then Return CInt(ctx.item("ClientID"))
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

#Region "Brand"
    Public ReadOnly Property CleanBrandsDT As DataTable
        Get
            ViewState("Brands") = Nothing
            Return BrandsDT
        End Get
    End Property

    Public ReadOnly Property BrandsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Brands")
            If DT Is Nothing Then

                SQL = "SELECT brand_id, brand_name, default_brand
                         FROM dbo.client_brand
                        WHERE client_id = @p1
                        ORDER BY brand_name"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Brands") = DT
            End If
            Return DT
        End Get
    End Property

    ReadOnly Property BrandsDR(BrandID) As DataRow
        Get
            Return BrandsDT.Select("brand_id = " + BrandID.ToString)(0)
        End Get
    End Property

    Public ReadOnly Property DefaultBrandID As Integer
        Get
            Dim DT As DataTable = BrandsDT
            If DT IsNot Nothing AndAlso DT.Rows.Count > 0 Then
                ' Find the rows where default_brand = '1'
                Dim defaultRows() As DataRow = DT.Select("default_brand = '1'")

                If defaultRows.Length > 0 Then
                    ' Return the brand_id for the first row where default_brand = '1'
                    Return defaultRows(0).Field(Of Integer)("brand_id")
                Else
                    ' If no default_brand is found, return the brand_id of the first row
                    Return DT.Rows(0).Field(Of Integer)("brand_id")
                End If
            Else
                ' If no rows are found, return 0
                Return 0
            End If
        End Get
    End Property

#End Region

#Region "ClientsAudit"
    Private ReadOnly Property CleanClientsAuditDT As DataTable
        Get
            ViewState("ClientsAudit") = Nothing
            Return ClientsAuditDT
        End Get
    End Property

    Public ReadOnly Property ClientsAuditDT As DataTable
        Get
            Dim DT As DataTable = ViewState("ClientsAudit")
            If DT Is Nothing Then
                SQL = "SELECT CA.audit_id,
                              CA.updated_at,
                              CA.updated_by,
                              dbo.dbf_puptext('clients_audit','action',CA.action, @p2) as action,
                              J.field,
                              J.oldvalue,
                              J.newvalue 
                         FROM dbo.clients_audit CA 
                              CROSS APPLY OPENJSON (CA.changes)
                               WITH (field Nvarchar(100),
                                     oldValue Nvarchar(Max),
                                     newValue Nvarchar(Max)) AS J 
                        WHERE CA.client_id = @p1
                        ORDER BY CA.updated_at DESC"
                DT = IawDB.execGetTable(SQL, client_id, ctx.languageCode)
                ViewState("ClientsAudit") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property ClientsAuditDR(audit_id As Integer) As DataRow
        Get
            Return ClientsAuditDT.Select("audit_id = '" + audit_id + "'")(0)
        End Get
    End Property
#End Region
#Region "Clients"
    ReadOnly Property ClientsDR(client_id As Integer) As DataRow
        Get
            Return IawDB.execGetDataRow("Select * from clients where client_id = @p1", client_id)
        End Get
    End Property
#End Region
#Region "ClientDomains"
    Private ReadOnly Property CleanClientDomainsDT As DataTable
        Get
            ViewState("ClientDomains") = Nothing
            Return ClientDomainsDT
        End Get
    End Property

    Public ReadOnly Property ClientDomainsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("ClientDomains")
            If DT Is Nothing Then
                SQL = "SELECT * from client_domain
                        WHERE client_id = @p1
                        ORDER BY domain_id DESC"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("ClientDomains") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property ClientDomainsDR(domain_id As Integer) As DataRow
        Get
            Return ClientDomainsDT.Select("domain_id = '" + domain_id.ToString + "'")(0)
        End Get
    End Property
#End Region
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Populate()
        End If
        ConfigureChart()
    End Sub
    Private Sub Populate()
        'Dim baseUrl As String = Request.Url.GetLeftPart(UriPartial.Authority) & Request.ApplicationPath
        'Dim fullUrl As String = baseUrl.TrimEnd("/"c) & "/secure/login.aspx?client=" + client_id.ToString

        hdnURL.Value = SawUtil.CreateLoginURL(Request, "client=" + client_id.ToString)

        SetDDLB(ddlbLanguage, LanguagesDT, "language_name", "language_ref")
        SetDDLB(ddlbBrand, BrandsDT, "brand_name", "brand_id")

        showClientDetails()
        If ctx.item("ClientID") IsNot Nothing Then
            btnBack.Visible = True
        End If
        RefreshGrid("ClientAudit")
        RefreshGrid("ClientDomains")
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "ClientAudit"
                SetGrid(grdClientAudit, CleanClientsAuditDT, False)
            Case "ClientDomains"
                SetGrid(grdClientDomains, CleanClientDomainsDT, False)
        End Select

    End Sub
    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeDomainForm.Hide()
        mpeDelete.Hide()
        mpeDomainConf.Hide()

        Select Case ModalState
            Case "Domain"
                mpeDomainForm.Show()
            Case "DeleteDomain"
                mpeDelete.Show()
            Case "DomainConf"
                mpeDomainConf.Show()
        End Select
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim url = SawUtil.encryptQuery("clients.aspx", True)
        ctx.redirect(url)
    End Sub
    Private Sub showClientDetails()
        Dim DR As DataRow = ClientsDR(client_id)
        Dim client_email As String = DR.GetValue("client_email")
        txtCompanyName.Text = DR.GetValue("client_name")
        txtEmail.Text = client_email
        hdnPrevEmail.Value = client_email
        txtZipPass.Text = DR.GetValue("zip_password")

        SetDDLBValue(ddlbBrand, DefaultBrandID.ToString)
        hdnPrevBrand.Value = DefaultBrandID.ToString
        SetDDLBValue(ddlbLanguage, DR.GetValue("language_ref", "GBR"))

        numMaxImgSize.Text = Math.Round(DR.GetValue("max_image_size") / 1024, 1)
        lblMaxData.Text = DR.GetValue("max_data").ToString
        lblServiceType.Text = DR.GetValue("service_type")
        lblServiceExpiry.Text = DR.GetValue("service_expiry")

        Dim email As New MailAddress(client_email)
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
            lblEmailWarning.Visible = True
        Else
            lblEmailWarning.Visible = False
        End If

    End Sub
    Private Sub ConfigureChart()
        Dim Data As Integer
        Dim Image As Integer
        Dim Free As Integer

        Using DB As New IawDB
            DB.ClearParameters()
            DB.AddParameter("@ClientID", client_id)
            DB.AddParameter("@TotalBytes", Data, ParameterDirection.Output)
            DB.CallNonQueryStoredProc("dbo.dbp_client_data_size")
            Data = DB.GetParameter("@TotalBytes")
        End Using

        Dim imgBytes As Integer = IawDB.execScalar("Select IsNull(SUM(IsNull(datalength(item_binary),0)),0)
                                                      from dbo.client_document
                                                      where client_id = @p1", client_id)

        Dim Logos As DataTable = IawDB.execGetTable("Select JSON_VALUE(branding,'$.bannerLogo') as logo
                                                       from dbo.client_brand
                                                      where client_id = @p1", client_id)
        Dim logoBytes As Integer
        For Each DR As DataRow In Logos.Rows
            Dim path As String = Server.MapPath("~/logos/" + DR.GetValue("logo"))
            If File.Exists(path) Then
                Dim info As New FileInfo(path)
                logoBytes += CInt(info.Length)
            End If
        Next

        Dim BGImages As DataTable = IawDB.execGetTable("select content
                                                          from dbo.background
                                                         where client_id = @p1
                                                           and background_type = '01'", client_id)
        Dim backBytes As Integer
        For Each DR As DataRow In BGImages.Rows
            Dim path As String = Server.MapPath("~/Backgrounds/" + DR.GetValue("content"))
            If File.Exists(path) Then
                Dim info As New FileInfo(path)
                backBytes += CInt(info.Length)
            End If
        Next

        Image = imgBytes + logoBytes + backBytes

        Dim maxMb As Integer = IawDB.execScalar("select max_data
                                                   from dbo.clients
                                                  where client_id = @p1", client_id)
        Dim maxBytes As Integer = maxMb * 1024 * 1024

        Free = maxBytes - (Data + Image)

        Dim Total As Integer = Data + Image + Free

        Dim DataPercent As Integer = Data / Total * 100
        Dim ImagePercent As Integer = Image / Total * 100
        Dim FreePercent As Integer = Free / Total * 100

        ' Data for the chart
        Dim xValuesList As New List(Of String)
        Dim yValuesList As New List(Of Integer)

        If DataPercent > 0 Then
            xValuesList.Add(ctx.Translate("::LT_M0020")) 'LT_M0020 = data
            yValuesList.Add(DataPercent)
        End If

        If ImagePercent > 0 Then
            xValuesList.Add(ctx.Translate("::LT_M0021")) 'LT_M0021 = Images
            yValuesList.Add(ImagePercent)
        End If

        If FreePercent > 0 Then
            xValuesList.Add(ctx.Translate("::LT_M0022")) 'LT_M0022 = Free
            yValuesList.Add(FreePercent)
        End If

        Dim xValues As String() = xValuesList.ToArray()
        Dim yValues As Integer() = yValuesList.ToArray()

        With Chart1.Series("Series1")
            .Points.DataBindXY(xValues, yValues)
            .IsValueShownAsLabel = True
            .Label = "#PERCENT{P0}" ' Shows percentage inside the chart as a whole number
            .LegendText = "#VALX"
            .LabelForeColor = Drawing.Color.White
            .Font = New Drawing.Font("Arial", 10, Drawing.FontStyle.Bold)
            .LabelBackColor = Drawing.Color.FromArgb(128, 0, 0, 0)  ' Semi-transparent black background
        End With

        ' Adjusting the Pie Label Style
        Chart1.Series("Series1")("PieLabelStyle") = "Inside" ' Set labels inside the pie slices

        ' Enhancing 3D style
        Chart1.ChartAreas("ChartArea1").Area3DStyle.Enable3D = True

        ' set background
        Chart1.ChartAreas("ChartArea1").BackColor = Drawing.Color.FromArgb(0, 0, 0, 0)
        Chart1.BackColor = Drawing.Color.FromArgb(0, 0, 0, 0)

        ' Ensure there is a legend and then adjust its properties
        If Chart1.Legends.Count = 0 Then
            Chart1.Legends.Add(New DataVisualization.Charting.Legend())
        End If

        SQL = "SELECT branding FROM dbo.client_brand WHERE client_id = @p1"
        Dim o As String = IawDB.execScalar(SQL, client_id)
        Dim Branding As ChartBranding = New ChartBranding(o)

        Dim legendTextColour As Drawing.Color = CreateDrawingColour(Branding.Data.textColour)
        Dim legendBgColour As Drawing.Color = CreateDrawingColour(Branding.Data.contentAreaBG)

        With Chart1.Legends(0)
            .Alignment = Drawing.StringAlignment.Center
            .Docking = System.Web.UI.DataVisualization.Charting.Docking.Bottom
            .ForeColor = legendTextColour
            .BackColor = legendBgColour
            .IsTextAutoFit = False
            .Font = New Drawing.Font("Arial", 12, Drawing.FontStyle.Bold)
        End With
    End Sub
    Function CreateDrawingColour(ByVal colour As String) As Drawing.Color
        Dim newColour As Drawing.Color
        If colour.StartsWith("#") Then
            newColour = Drawing.ColorTranslator.FromHtml(colour)
        Else
            newColour = RGBtoARGB(colour)
        End If

        Return newColour
    End Function
    Function RGBtoARGB(ByVal color As String) As Drawing.Color
        Dim regex As New Regex("(rgba?)\((\d+), (\d+), (\d+)(?:, ([\d.]+))?\)")

        Dim match As Match = regex.Match(color)

        If match.Success Then
            Dim red As Integer = Convert.ToInt32(match.Groups(2).Value)
            Dim green As Integer = Convert.ToInt32(match.Groups(3).Value)
            Dim blue As Integer = Convert.ToInt32(match.Groups(4).Value)
            Dim alpha As Single = If(match.Groups.Count > 5 AndAlso Not String.IsNullOrEmpty(match.Groups(5).Value), Convert.ToSingle(match.Groups(5).Value), 1)

            ' Convert alpha from 0-1 to 0-255 range
            Dim alphaByte As Byte = Convert.ToByte(alpha * 255)

            Return Drawing.Color.FromArgb(alphaByte, red, green, blue)
        Else
            ' Invalid color format
            Return Drawing.Color.Empty
        End If
    End Function

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
            cvEmail.ErrorMessage = ctx.Translate("::LT_S0092") ' LT_S0092 - This is not a valid email address'
            args.IsValid = False
        End Try
    End Sub
    Private Sub btnClientSave_Click(sender As Object, e As EventArgs) Handles btnClientSave.Click
        If Not Page.IsValid Then Return

        Dim CompanyName As String = txtCompanyName.Text.Trim
        Dim Email As String = txtEmail.Text.Trim
        Dim ZipPass As String = txtZipPass.Text.Trim
        Dim MaxImgSize As String = CInt(numMaxImgSize.Text) * 1024
        Dim LanguageRef As String = ddlbLanguage.SelectedValue
        Dim BrandID As Integer = CInt(ddlbBrand.SelectedValue)

        SQL = "UPDATE dbo.clients
                  SET client_name = @p2,
                      client_email = @p3,
                      zip_password = @p4,
                      max_image_size = @p5,
                      language_ref = @p6
                WHERE client_id = @p1"
        IawDB.execNonQuery(SQL, client_id, CompanyName, Email, ZipPass, MaxImgSize, LanguageRef)

        ' if the email address hasn't changed, don't mess with the user record
        If hdnPrevEmail.Value <> Email Then
            SQL = "UPDATE dbo.suser
                      SET user_name = @p2,
                          email_address = @p2
                    WHERE email_address = @p1"
            IawDB.execNonQuery(SQL, hdnPrevEmail.Value, Email)
        End If

        ' we have to set the default brand for the client
        If hdnPrevBrand.Value <> BrandID.ToString Then
            SQL = "Update dbo.client_brand
                      set default_brand = case when brand_id = @p2 then '1' else '0' end
                    where client_id = @p1"
            IawDB.execNonQuery(SQL, client_id, BrandID)
            ctx.Reload()
        End If

        RefreshGrid("ClientAudit")
        showClientDetails()
    End Sub
    Private Sub btnClientCancel_Click(sender As Object, e As EventArgs) Handles btnClientCancel.Click
        showClientDetails()
    End Sub

    Private Sub grdClientDomains_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdClientDomains.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.DataRow Then

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim domain As String = rv("domain_name")

            SQL = "SELECT TOP 1 1 AS Result FROM clients
                    WHERE client_email LIKE '%' + @p1 + '%'
                    UNION
                    SELECT TOP 1 1 AS Result FROM SUSER
                    WHERE email_address LIKE '%' + @p1 + '%'"
            Dim isUsed As Boolean = IawDB.execScalar(SQL, domain)

            If isUsed Then
                ' get a handle to the delete button and hide it
                CTRL = e.Row.FindControl("btnDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
                CTRL = e.Row.FindControl("lblNoDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = True
                End If
            End If
        End If
    End Sub
    Private Sub grdClientDomains_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdClientDomains.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        txtDomain.Text = ""

        If e.CommandName = "New" Then
            hdnDomainId.Value = ""
            SetModalState("Domain")
            Return
        End If

        CurRowIndex = grdClientDomains.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        Dim Domain_id As Integer = grdClientDomains.DataKeys(RowIndex).Value
        hdnDomainId.Value = Domain_id

        Select Case e.CommandName
            Case "AmendRow"
                Dim DR As DataRow = ClientDomainsDR(Domain_id)
                txtDomain.Text = DR.GetValue("domain_name")
                SetModalState("Domain")

            Case "DeleteRow"
                SetModalState("DeleteDomain")
        End Select
    End Sub

    Private Sub btnDomainSave_Click(sender As Object, e As EventArgs) Handles btnDomainSave.Click
        If Not Page.IsValid Then Return

        lblDomainConf.Text = txtDomain.Text.Trim

        If hdnDomainId.Value = "" Then
            SQL = "INSERT INTO dbo.client_domain (client_id, domain_name)
                       VALUES (@p1,@p2)"
            IawDB.execNonQuery(SQL, client_id, txtDomain.Text.Trim)
        Else
            SQL = "UPDATE dbo.client_domain SET domain_name = @p2
                    WHERE domain_id = @p1"
            IawDB.execNonQuery(SQL, hdnDomainId.Value, txtDomain.Text.Trim)
        End If

        showClientDetails()
        SetModalState("DomainConf")
    End Sub
    Private Sub btnDomainConfOk_Click(sender As Object, e As EventArgs) Handles btnDomainConfOk.Click

        RefreshGrid("ClientDomains")
        SetModalState("")
    End Sub
    Private Sub btnDomainCancel_Click(sender As Object, e As EventArgs) Handles btnDomainCancel.Click
        SetModalState("")
    End Sub
    Private Sub btnDeleteOk_Click(sender As Object, e As EventArgs) Handles btnDeleteOk.Click
        SQL = "DELETE FROM dbo.client_domain
                WHERE domain_id = @p1"
        IawDB.execNonQuery(SQL, hdnDomainId.Value)

        RefreshGrid("ClientDomains")
        SetModalState("")
    End Sub
    Private Sub btnDeleteCancel_Click(sender As Object, e As EventArgs) Handles btnDeleteCancel.Click
        SetModalState("")
    End Sub


End Class