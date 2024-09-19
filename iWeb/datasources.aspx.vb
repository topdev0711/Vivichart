
<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class Datasources
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Dim SQL As String
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
    Public ReadOnly Property MaxFieldLength As Integer
        Get
            Dim MaxLen As Integer
            If ViewState("MaxFieldLength") Is Nothing Then
                SQL = "SELECT IsNull(max_string_len,500)
                         FROM dbo.clients
                        WHERE client_id = @p1"
                MaxLen = IawDB.execScalar(SQL, client_id)
                ViewState("MaxFieldLength") = MaxLen
            Else
                MaxLen = ViewState("MaxFieldLength")
            End If
            Return MaxLen
        End Get
    End Property

#End Region
#Region "Data Source"

    Private Property DataSourceID As Integer
        Get
            Return ViewState("DataSourceID")
        End Get
        Set(value As Integer)
            ViewState("DataSourceID") = value
        End Set
    End Property
    Private ReadOnly Property CleanDataSourceDT() As DataTable
        Get
            ViewState("DataSourceDT") = Nothing
            Return DataSourceDT
        End Get
    End Property
    Private ReadOnly Property DataSourceDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("DataSourceDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("select source_id,
                                                table_name,
                                                IsNull(short_ref,'') as short_ref,
                                                IsNull(zip_password,'') as zip_password,
                                                IsNull(photos_applicable,'0') as photos_applicable,
                                                primary_source
                                           from dbo.data_source WITH (NOLOCK)
                                          where client_id = @p1
                                          order by source_id", client_id)
                ViewState("DataSourceDT") = DT
            End If
            Return DT
        End Get
    End Property
    Private ReadOnly Property DataSourceDR() As DataRow
        Get
            Return DataSourceDT.Select("source_id = '" + DataSourceID.ToString + "'")(0)
        End Get
    End Property

#End Region
#Region "Occurrence Dates"
    Private Property OccurrenceDate As Date
        Get
            Return ViewState("OccurrenceDate")
        End Get
        Set(value As Date)
            ViewState("OccurrenceDate") = value
        End Set
    End Property
    Private ReadOnly Property CleanOccurrenceDateDT() As DataTable
        Get
            ViewState("OccurrenceDateDT") = Nothing
            Return OccurrenceDateDT
        End Get
    End Property
    Private ReadOnly Property OccurrenceDateDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("OccurrenceDateDT")
            If DT Is Nothing Then
                SQL = "SELECT source_date
                         FROM dbo.client_import_seg
                        WHERE source_id = @p1
                          AND seg_status IN ('03','04','10')"
                DT = IawDB.execGetTable(SQL, DataSourceID)
                ViewState("OccurrenceDateDT") = DT
            End If
            Return DT
        End Get
    End Property
#End Region
#Region "DataSource Fields"
    Private Property FieldNum As Integer
        Get
            Return ViewState("FieldNum")
        End Get
        Set(value As Integer)
            ViewState("FieldNum") = value
        End Set
    End Property
    Private ReadOnly Property CleanFieldsDT() As DataTable
        Get
            ViewState("FieldsDT") = Nothing
            Return FieldsDT
        End Get
    End Property
    Private ReadOnly Property FieldsDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("FieldsDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT field_num,
                                                field_name,
                                                field_status,
                                                field_type,
                                                dbo.dbf_puptext('data_source_field','field_type',field_type,@p3) as field_type_pt,
                                                field_type as org_field_type,
                                                field_length,
                                                IsNull(field_format,'') as field_format,
                                                dbo.dbf_puptext('data_source_field','field_format',field_format,@p3) as field_format_pt,
                                                item_ref_field,
                                                item_ref_field as org_item_ref_field,
                                                dbo.dbf_puptext('data_source_field','item_ref_field',item_ref_field,@p3) as item_ref_field_pt,
                                                IsNull(parent_ref_field,'0') as parent_ref_field
                                           FROM dbo.data_source_field WITH (NOLOCK)
                                          WHERE source_id = @p1
                                            AND source_date = @p2
                                          ORDER BY field_num", DataSourceID, OccurrenceDate, ctx.languageCode)
                ViewState("FieldsDT") = DT
            End If
            Return DT
        End Get
    End Property
    Private ReadOnly Property FieldsDR() As DataRow
        Get
            Return FieldsDT.Select("field_num = " + FieldNum.ToString)(0)
        End Get
    End Property
    Private ReadOnly Property UniqueFields As Integer
        Get
            Return FieldsDT.Select("item_ref_field in ('1','3')").Count
        End Get
    End Property
    Private ReadOnly Property NewFields As Integer
        Get
            Return FieldsDT.Select("field_status = '01'").Count
        End Get
    End Property
    Private ReadOnly Property FieldTypesDT(CurType As String) As DataTable
        Get
            Dim DT As DataTable = ViewState("FieldTypesDT")
            If DT Is Nothing Then
                DT = SawUtil.GetPupText("data_source_field", "field_type", True)
                ViewState("FieldTypesDT") = DT
            End If
            Dim DV As New DataView(DT)

            Select Case CurType

                Case "01" 'String
                    ' will never be this as it user won't be able to change from string
                    DV.RowFilter = "return_value in ('01')"

                Case "02" 'Date
                    DV.RowFilter = "return_value in ('01','02')"

                Case "03" 'integer
                    DV.RowFilter = "return_value in ('01','03','04')"

                Case "04" 'float
                    DV.RowFilter = "return_value in ('01','04')"

                Case "05" 'binary
                    DV.RowFilter = "return_value in ('01','05')"

            End Select

            Return DV.ToTable
        End Get
    End Property

    Private ReadOnly Property ItemRefFieldDT(CurType As String) As DataTable
        Get
            Dim DT As DataTable = ViewState("FieldTypesDT")
            If DT Is Nothing Then
                DT = SawUtil.GetPupText("data_source_field", "field_type", True)
                ViewState("FieldTypesDT") = DT
            End If
            Dim DV As New DataView(DT)

            Select Case CurType

                Case "01" 'String
                    ' will never be this as it user won't be able to change from string
                    DV.RowFilter = "return_value in ('01')"

                Case "02" 'Date
                    DV.RowFilter = "return_value in ('01','02')"

                Case "03" 'integer
                    DV.RowFilter = "return_value in ('01','03','04')"

                Case "04" 'float
                    DV.RowFilter = "return_value in ('01','04')"

                Case "05" 'binary
                    DV.RowFilter = "return_value in ('01','05')"

            End Select

            Return DV.ToTable
        End Get
    End Property

#End Region
#Region "DataSource Access"

    Private Property AccessID As Integer
        Get
            Return ViewState("AccessID")
        End Get
        Set(value As Integer)
            ViewState("AccessID") = value
        End Set
    End Property
    Private ReadOnly Property CleanDataSourceAccessDT() As DataTable
        Get
            ViewState("DataSourceAccessDT") = Nothing
            Return DataSourceAccessDT
        End Get
    End Property
    Private ReadOnly Property DataSourceAccessDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("DataSourceAccessDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT A.access_id,
                                                A.access_type,
                                                dbo.dbf_puptext('data_source_access','access_type',access_type, 'GBR') As access_type_text,
                                                A.access_ref,
                                                case A.access_type 
                                                  when '01' then (select dbo.dbf_puptext('role','role_name',role_name,@p2) as role_name
                                                                    from dbo.role
                                                                   where role_ref = A.access_ref)
                                                  when '02' then (select IsNull(forename,'') + ' ' + IsNull(surname,'')
                                                                    from dbo.suser
                                                                   where user_ref = A.access_ref)
                                                end as access_ref_text,
                                                case A.access_type 
                                                  when '01' then (select dbo.dbf_puptext('role','role_name',role_name,@p2) as role_name
                                                                    from dbo.role
                                                                   where role_ref = A.access_ref)
                                                  when '02' then (select IsNull(surname,'') + ' ' + IsNull(forename,'')
                                                                    from dbo.suser
                                                                   where user_ref = A.access_ref)
                                                end as access_ref_sort
                                           FROM dbo.data_source_access A WITH (NOLOCK)
                                          WHERE source_id = @p1
                                          ORDER BY 3,6", DataSourceID, ctx.languageCode)
                ViewState("DataSourceAccessDT") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property DataSourceAccessRolesDT() As DataTable
        Get
            Return IawDB.execGetTable("SELECT role_ref,
                                              dbo.dbf_puptext('role','role_name',role_name,@p2) as role_name
                                         FROM dbo.role
                                        WHERE role_ref not in (
                                              select access_ref 
                                                from dbo.data_source_access
                                               where access_type = '01'
                                                 and source_id = @p1)", DataSourceID, ctx.languageCode)
        End Get
    End Property

    Private ReadOnly Property DataSourceAccessUsers() As DataTable
        Get
            Dim SearchText As String = "%" + txtSearchUser.Text.Trim + "%"
            Return IawDB.execGetTable("SELECT user_ref,
                                              IsNull(forename,'') + ' ' + IsNull(surname,'') as name
                                         FROM dbo.suser S
                                        WHERE user_ref not in (select access_ref 
                                                                 from dbo.data_source_access
                                                                where access_type = '02'
                                                                  and source_id = @p1)
                                          AND (surname like @p2
                                           OR  forename like @p2)
                                        ORDER BY surname,forename", DataSourceID, SearchText)
        End Get
    End Property

#End Region

#Region "Page Events"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Populate()
            ModalState = ""
        End If
        SetModalState()
        grdDatasource.CssClass = "allow-hover"
        grdDates.CssClass = "allow-hover"
    End Sub
    Private Sub Populate()

        ' Fields form
        'SetDDLBPuptext(ddlbFieldFormat, "data_source_field", "field_format", True)

        ' Data Source access form
        SetDDLBPuptext(ddlbAccessType, "data_source_access", "access_type", "01")

        ' Now populate the grids for datasource and datasource access
        RefreshGrid("DataSource")

    End Sub
    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeDataSourceForm.Hide()
        mpeDataSourceAccessForm.Hide()
        mpeFieldForm.Hide()
        mpeRejectForm.Hide()
        mpeIgnorePrimaryForm.Hide()

        Select Case ModalState

            Case "DataSource"
                mpeDataSourceForm.Show()

            Case "Field"
                mpeFieldForm.Show()

            Case "DataSourceAccess"
                mpeDataSourceAccessForm.Show()

            Case "Reject"
                mpeRejectForm.Show()

            Case "IgnorePrimary"
                mpeIgnorePrimaryForm.Show()

        End Select
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)
        Select Case GridName
            Case "DataSource"
                SetGrid(grdDatasource, CleanDataSourceDT, True)
                DataSourceID = grdDatasource.SelectedValue

                pnlDataSourceDetails.GroupingText = DataSourceDR("short_ref")

                RefreshGrid("OccurrenceDates")
                RefreshGrid("DataSourceAccess")

            Case "OccurrenceDates"
                SetGrid(grdDates, CleanOccurrenceDateDT, True)
                OccurrenceDate = grdDates.SelectedValue

                RefreshGrid("Fields")

            Case "Fields"
                SetGrid(grdFieldList, CleanFieldsDT, False)
                trActionButtons.Visible = NewFields > 0

                ' if it's not a primary ds, then hide the columns for key and parent
                If DataSourceDR("primary_source") = "0" Then
                    grdFieldList.Columns(5).Visible = False
                    grdFieldList.Columns(6).Visible = False
                Else
                    grdFieldList.Columns(5).Visible = True
                    grdFieldList.Columns(6).Visible = True
                End If

            Case "DataSourceAccess"
                SetGrid(grdDataSourceAccess, CleanDataSourceAccessDT, False)

        End Select
    End Sub

    Private Sub btnFieldsAccept_Click(sender As Object, e As EventArgs) Handles btnFieldsAccept.Click

        Using DB As New IawDB
            Dim NewDatasource As Boolean = True
            Dim PrimaryField As Boolean = False
            Dim PrimarySelected As Boolean = False

            ' firstly, is this datasource marked as being a primary ds ?
            '  (if not then there won't be any unique columns on it)
            If DB.Scalar("select primary_source from dbo.data_source where source_id = @p1", DataSourceID) = "1" Then
                ' If any of the fields is a type 02 then we can't change anything as it would already have been
                ' processed and accepted as a datasoruce eith primary or secondary, once it's one, that's it.

                For Each DR As DataRow In FieldsDT.Rows
                    If DR("field_status") <> "01" Then
                        NewDatasource = False
                        Exit For
                    End If

                    ' string or integer?
                    If DR("field_type").ToString.ContainsOneOf("01", "03") Then
                        ' unqiue col or raised to primary key ?
                        If DR("item_ref_field").ToString.ContainsOneOf("1", "3") Then
                            PrimaryField = True
                        End If
                        ' an actual primary key
                        If DR("item_ref_field") = "3" Then
                            PrimarySelected = True
                        End If
                    End If
                Next

                ' can only do something if it's a new datasource
                If NewDatasource Then
                    ' did we find a unique column ?
                    If PrimaryField Then
                        ' and has the user not selected a primary key ?
                        If Not PrimarySelected Then
                            ' right, we now have to ask the user whether they want to make this data source
                            ' that does have a unqiue column a secondary data source instead of a primary one.
                            SetModalState("IgnorePrimary")
                            Return
                        End If
                    End If
                End If
            End If

            UpdateFieldLines(False)

        End Using
        RefreshGrid("Fields")

    End Sub

    Private Sub btnIgnorePrimaryCancel_Click(sender As Object, e As EventArgs) Handles btnIgnorePrimaryCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnIgnorePrimarySave_Click(sender As Object, e As EventArgs) Handles btnIgnorePrimarySave.Click
        UpdateFieldLines(True)

        SetModalState("")
    End Sub

    Private Sub UpdateFieldLines(ignorePrimary As Boolean)
        Using DB As New IawDB
            ' we need to update all the fields from the datatable that are at status 01 (new)

            For Each DR As DataRow In FieldsDT.Rows
                If DR("field_status") <> "01" Then Continue For

                Dim ItemRefField As String = DR("item_ref_field")
                If ignorePrimary Then ItemRefField = "0"

                SQL = "Update dbo.data_source_field
                          set field_status = '02',
                              field_name = @p4,
                              field_type = @p5,
                              field_length = @p6,
                              field_format = @p7,
                              item_ref_field = @p8,
                              parent_ref_field = @p9
                        where source_id = @p1
                          and source_date = @p2
                          and field_num = @p3 "
                DB.NonQuery(SQL, DataSourceID, OccurrenceDate, DR("field_num"),
                                                               DR("field_name"),
                                                               DR("field_type"),
                                                               DR("field_length"),
                                                               DR("field_format"),
                                                               ItemRefField,
                                                               DR("parent_ref_field"))
            Next

            ' now update the seg record to move it on to stage 4
            SQL = "UPDATE dbo.client_import_seg
                      SET seg_status = '04'
                    WHERE source_id = @p1
                      AND source_date = @p2
                      AND seg_status = '03'"
            DB.NonQuery(SQL, DataSourceID, OccurrenceDate)

            If ignorePrimary Then
                ' use has elected to ignore the fact that there is a unique key column, so go ahead and
                ' remove the primary table flag from the data source 
                DB.NonQuery("UPDATE dbo.data_source SET primary_source = '0' WHERE source_id = @p1", DataSourceID)
                RefreshGrid("DataSource")
            Else
                RefreshGrid("Fields")
            End If

        End Using
    End Sub

    Private Sub btnFieldsReject_Click(sender As Object, e As EventArgs) Handles btnFieldsReject.Click

        lblRejectDataSource.Text = DataSourceDR("short_ref")
        lblRejectUploadDate.Text = String.Format("{0:dd/MM/yyyy HH:mm}", OccurrenceDate)

        SQL = "select U.forename + ' ' U.surname as name
                 From client_import_seg CIS
                      Join dbo.client_import CI
                        On CI.unique_id = CIS.unique_id
                      Join dbo.suser U
                        On U.email_address = CI.email_from
                where CIS.source_id = @p1
                  And CIS.source_date = @p2"
        Dim UserName As String = IawDB.execScalar(SQL, DataSourceID, OccurrenceDate)
        If String.IsNullOrEmpty(UserName) Then
            lblRejectUploadedBy.Text = ""
        Else
            lblRejectUploadedBy.Text = UserName
        End If

        txtRejectReason.Text = ""
        rfvRejectReason.ErrorMessage = ctx.Translate("::LT_S0140") ' LT_S0140 - Required Field

        SetModalState("Reject")
    End Sub
    Private Sub btnRejectProceed_Click(sender As Object, e As EventArgs) Handles btnRejectProceed.Click
        If Not Page.IsValid Then Return

        SQL = "INSERT dbo.client_import_msg (seg_id,row_num,field_num,msg_type,msg_text,msg_text_gb)               SELECT seg_id, 0, 0, '01', @p3, @p3
                 FROM dbo.client_import_seg
                WHERE source_id = @p1
                  AND source_date = @p2"
        IawDB.execNonQuery(SQL, DataSourceID, OccurrenceDate, txtRejectReason.Text.Trim)

        SQL = "UPDATE dbo.client_import_seg
                  SET seg_status = '99'
                WHERE source_id = @p1
                  AND source_date = @p2"
        IawDB.execNonQuery(SQL, DataSourceID, OccurrenceDate)

        SetModalState("")

        RefreshGrid("OccurrenceDates")
    End Sub
    Private Sub btnRejectCancel_Click(sender As Object, e As EventArgs) Handles btnRejectCancel.Click
        SetModalState("")
    End Sub

#End Region
#Region "DataSource, Occurence Dates, Fields and Access Grids"

    Private Sub grdDatasource_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdDatasource.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDatasource, "Select$" & e.Row.RowIndex)
            e.Row.Cells(1).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDatasource, "Select$" & e.Row.RowIndex)
        End If
    End Sub
    Private Sub grdDatasource_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdDatasource.RowDataBound
        Dim ctrl As Control

        If e.Row.RowType = DataControlRowType.Header And Not ctx.IsInsertAllowed Then
            ' if not allowed to insert, remove the insert button
            ctrl = e.Row.FindControl("btnDataSourceAdd")
            If ctrl IsNot Nothing Then
                ctrl.Visible = False
            End If
        End If

        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim srcID As Integer = rv("source_id")

            If IawDB.execScalar("select count(1) from data_view where source_id = @p1", srcID) > 0 Then
                ctrl = e.Row.FindControl("btnDel")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
                ctrl = e.Row.FindControl("lblNoDeleteRow")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = True
                End If
            End If
        End If

    End Sub
    Private Sub grdDatasource_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDatasource.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing

        If e.CommandName = "New" Then
            hdnDataSourceID.Value = ""
            txtShortRef.Text = ""
            txtZipPassword.Text = ""
            cbPhotosApplicable.Checked = True

            ModalState = "DataSource"
            SetModalState()
            Return
        End If

        lblDataSourceMsg.Visible = False

        CurRowIndex = grdDatasource.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        DataSourceID = grdDatasource.DataKeys(RowIndex).Value
        DR = DataSourceDR()

        Select Case e.CommandName

            Case "Select"
                RefreshGrid("OccurrenceDates")
                RefreshGrid("DataSourceAccess")
                pnlDataSourceDetails.GroupingText = DR("short_ref")

            Case "AmendRow"
                hdnDataSourceID.Value = DataSourceID
                txtShortRef.Text = DR("short_ref")
                txtZipPassword.Text = DR("zip_password")
                cbPhotosApplicable.Checked = If(DR("photos_applicable") = "1", True, False)

                SetModalState("DataSource")

            Case "DeleteRow"
                Using DB As New IawDB(True)
                    DB.NonQuery("DELETE dbo.data_source where source_id = @p1", DataSourceID)
                End Using

                RefreshGrid("DataSource")
        End Select
    End Sub

    Private Sub grdDates_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdDates.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDates, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdDates_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDates.RowCommand
        Dim RowIndex As Integer
        Dim CurrentRow As Integer
        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        CurrentRow = grdDates.SelectedIndex
        If RowIndex = CurrentRow Then Return

        OccurrenceDate = CDate(grdDates.DataKeys(RowIndex).Value)

        Select Case e.CommandName
            Case "Select"
                RefreshGrid("Fields")

                trActionButtons.Visible = NewFields > 0

        End Select

    End Sub

    Private Sub grdFieldList_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdFieldList.RowDataBound

        ' grid columns             field_type
        '  0 - field num            01 String
        '  1 - field name           02 Date
        '  2 - field type           03 Integer
        '  3 - field langth         04 Float
        '  4 - field format         05 Boolean
        '  5 - unique ref
        '  6 - parent ref 
        '  7 - amend button

        Dim ctrl As Control

        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.DataItem Is Nothing Then Return

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            If rv("field_status") <> "01" Then
                ctrl = e.Row.FindControl("btnAmend")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
            End If

            ' based on the datatype, ensure the the columns that are not relevant get hidden
            Select Case rv("field_type")
                Case "01" ' String
                    e.Row.Cells(4).Text = ""        ' format
                Case "02" ' Date
                    e.Row.Cells(3).Text = ""        ' length
                    e.Row.Cells(5).Text = ""        ' unique ref
                    e.Row.Cells(6).Text = ""        ' parent ref
                Case "03" ' Integer
                    e.Row.Cells(3).Text = ""        ' length
                    e.Row.Cells(4).Text = ""        ' format        
                Case "04" ' Float
                    e.Row.Cells(4).Text = ""        ' format
                    e.Row.Cells(5).Text = ""        ' unique ref
                    e.Row.Cells(6).Text = ""        ' parent ref
                Case "05" ' Boolean
                    e.Row.Cells(3).Text = ""        ' length
                    e.Row.Cells(4).Text = ""        ' format
                    e.Row.Cells(5).Text = ""        ' unique ref
                    e.Row.Cells(6).Text = ""        ' parent ref
            End Select

            ' also hide the reference field if it's not unique
            If Not rv("item_ref_field").ToString.ContainsOneOf("1", "3") Then
                e.Row.Cells(5).Text = ""        ' unique ref
            End If

        End If

    End Sub
    Private Sub grdFieldList_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdFieldList.RowCommand
        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        FieldNum = grdFieldList.DataKeys(RowIndex).Value
        DR = FieldsDR()

        Select Case e.CommandName
            Case "AmendRow"
                '  0 - field num      
                '  1 - field name     
                '  2 - field type     
                '  3 - field langth   
                '  4 - field format   
                '  5 - item ref type
                '  6 - parent ref ?
                hdnIdx.Value = RowIndex.ToString

                lblFieldNum.Text = DR("field_num")
                txtFieldName.Text = DR("field_name")

                SetDDLB(ddlbFieldType, FieldTypesDT(DR("org_field_type")), "pup_text", "return_value")
                SetDDLBValue(ddlbFieldType, DR("field_type"))

                If DR("field_type").ToString.ContainsOneOf("01", "04") Then
                    trFieldLength.Visible = True
                    If DR("field_type") = "04" Then
                        lblFieldLength.Text = "Decimal Places"
                        rvFieldLength.MaximumValue = "6"
                        rvFieldLength.ErrorMessage = String.Format("{0} - {1}", 1, 6)
                    Else
                        lblFieldLength.Text = "Length"
                        rvFieldLength.MaximumValue = MaxFieldLength.ToString
                        rvFieldLength.ErrorMessage = String.Format("{0} - {1}", 1, MaxFieldLength)
                    End If
                    txtFieldLength.Text = DR("field_length")
                Else
                    trFieldLength.Visible = False
                End If

                If DR("field_type") = "02" Then
                    trFieldFormat.Visible = True
                    SetDDLBValue(ddlbFieldFormat, DR("field_format"))
                Else
                    trFieldFormat.Visible = False
                    SetDDLBValue(ddlbFieldFormat, "")
                End If

                ' string or integer and unique
                If DR("field_type").ToString.ContainsOneOf("01", "03") And
                   DR("item_ref_field").ToString.ContainsOneOf("1", "3") Then
                    trItemRefField.Visible = True
                    cbUniqueRef.Checked = DR("item_ref_field") = "3"
                Else
                    trItemRefField.Visible = False
                End If

                ' string or integer
                If DR("field_type").ToString.ContainsOneOf("01", "03") Then
                    cbParentRefField.Checked = DR("parent_ref_field") = "1"
                    trParentRefField.Visible = True
                Else
                    trParentRefField.Visible = False
                End If

                SetModalState("Field")
        End Select

    End Sub

    Private Sub grdDataSourceAccess_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDataSourceAccess.RowCommand
        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing

        If e.CommandName = "New" Then
            hdnDataSourceAccessID.Value = ""

            ddlbAccessType.SelectedValue = "01" ' role
            SetDDLB(ddlbAccessRole, DataSourceAccessRolesDT, "role_name", "role_ref")
            trRoles.Visible = True
            trSearchUser.Visible = False

            FieldFocus(ddlbAccessType)
            ModalState = "DataSourceAccess"
            SetModalState()
            Return
        End If

        lblDataSourceMsg.Visible = False

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        AccessID = grdDataSourceAccess.DataKeys(RowIndex).Value

        Select Case e.CommandName

            Case "DeleteRow"
                IawDB.execNonQuery("DELETE dbo.data_source_access where access_id = @p1", AccessID)
                RefreshGrid("DataSourceAccess")
        End Select

    End Sub

#End Region
#Region "Modal DataSource Details Dialog"

    Private Sub btnDataSourceCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDataSourceCancel.Click
        SetModalState("")
    End Sub
    Private Sub btnDataSourceSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDataSourceSave.Click
        If Not Page.IsValid Then Return

        If hdnDataSourceID.Value = "" Then
            IawDB.execNonQuery("INSERT dbo.data_source (client_id,short_ref,zip_passwrod,photos_applicable) 
                                VALUES (@p1,@p2,@p3,@p4)",
                               client_id,
                               txtShortRef.Text.Trim,
                               txtZipPassword.Text.Trim,
                               If(cbPhotosApplicable.Checked, "1", "0"))
        Else
            IawDB.execNonQuery("UPDATE dbo.data_source 
                                   SET short_ref = @p2,
                                       zip_password = @p3,
                                       photos_applicable = @p4
                                 WHERE source_id = @p1",
                               hdnDataSourceID.Value,
                               txtShortRef.Text.Trim,
                               txtZipPassword.Text.Trim,
                               If(cbPhotosApplicable.Checked, "1", "0"))
        End If

        RefreshGrid("DataSource")
        SetModalState("")
    End Sub

#End Region
#Region "Modal Field"
    Private Sub ddlbFieldType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbFieldType.SelectedIndexChanged
        Dim DataType As String = ddlbFieldType.SelectedValue

        ' type = string or float
        If DataType.ContainsOneOf("01", "04") Then
            trFieldLength.Visible = True
            If DataType = "04" Then
                lblFieldLength.Text = "Decimal Places"
                txtFieldLength.Text = "2"
                rvFieldLength.MaximumValue = "6"
                rvFieldLength.ErrorMessage = String.Format("{0} - {1}", 1, 6)
            Else
                lblFieldLength.Text = "Length"
                txtFieldLength.Text = "10"
                rvFieldLength.MaximumValue = MaxFieldLength.ToString
                rvFieldLength.ErrorMessage = String.Format("{0} - {1}", 1, MaxFieldLength)
            End If

        Else
            trFieldLength.Visible = False
        End If

        ' type = date
        If DataType = "02" Then
            trFieldFormat.Visible = True
            SetDDLBValue(ddlbFieldFormat, "")
        Else
            trFieldFormat.Visible = False
            SetDDLBValue(ddlbFieldFormat, "")
        End If

        If DataType.ContainsOneOf("01", "03") And FieldsDR("org_item_ref_field").ToString.ContainsOneOf("1", "3") Then
            trItemRefField.Visible = True
            FieldsDR("item_ref_field") = FieldsDR("org_item_ref_field")
            cbUniqueRef.Checked = FieldsDR("item_ref_field") = "3"
        Else
            trItemRefField.Visible = False
        End If

        If DataType.ContainsOneOf("01", "03") Then
            cbParentRefField.Checked = FieldsDR("parent_ref_field") = "1"
            trParentRefField.Visible = True
        Else
            trParentRefField.Visible = False
        End If

    End Sub

    Private Sub btnFieldSave_Click(sender As Object, e As EventArgs) Handles btnFieldSave.Click
        If Not Page.IsValid Then Return

        Dim DR As DataRow = FieldsDR            ' fieldnum is already set so it uses it when updating

        Dim rowindex As Integer = CInt(hdnIdx.Value)
        Dim FieldName As String = txtFieldName.Text.Trim
        Dim FieldType As String = ddlbFieldType.SelectedValue
        Dim FieldLength As Integer = 0
        Dim FieldFormat As String = ""
        Dim ItemRefField As String = "0"
        Dim ParentRefField As String = "0"

        If trFieldLength.Visible Then
            FieldLength = CInt(txtFieldLength.Text)
        End If

        If trFieldFormat.Visible Then
            FieldFormat = ddlbFieldFormat.SelectedValue
        End If

        If trItemRefField.Visible Then
            ItemRefField = If(cbUniqueRef.Checked, "3", "1")
        End If

        If trParentRefField.Visible Then
            ParentRefField = If(cbParentRefField.Checked, "1", "0")
        End If

        FieldsDT(rowindex)("field_name") = FieldName
        FieldsDT(rowindex)("field_type") = FieldType
        FieldsDT(rowindex)("field_type_pt") = SawUtil.GetPupText("data_source_field", "field_type", FieldType)
        FieldsDT(rowindex)("field_length") = FieldLength
        FieldsDT(rowindex)("field_format") = FieldFormat
        FieldsDT(rowindex)("field_format_pt") = SawUtil.GetPupText("data_source_field", "field_format", FieldFormat)
        FieldsDT(rowindex)("item_ref_field") = ItemRefField
        FieldsDT(rowindex)("item_ref_field_pt") = SawUtil.GetPupText("data_source_field", "item_ref_field", ItemRefField)
        FieldsDT(rowindex)("parent_ref_field") = ParentRefField

        SetGrid(grdFieldList, FieldsDT, False)
        FieldNum = grdFieldList.SelectedValue

        SetModalState("")
    End Sub

    Private Sub btnFieldCancel_Click(sender As Object, e As EventArgs) Handles btnFieldCancel.Click
        SetModalState("")
    End Sub

#End Region
#Region "Modal DataSource Access"

    Private Sub ddlbAccessType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbAccessType.SelectedIndexChanged
        If ddlbAccessType.SelectedValue = "01" Then
            trRoles.Visible = True
            trSearchUser.Visible = False
            SetDDLB(ddlbAccessRole, DataSourceAccessRolesDT, "role_name", "role_ref")
        Else
            trRoles.Visible = False
            trSearchUser.Visible = True
            pnlUserList.Visible = False
            txtSearchUser.Text = ""
        End If
    End Sub
    Private Sub btnSearchUser_Click(sender As Object, e As EventArgs) Handles btnSearchUser.Click
        If txtSearchUser.Text.Trim = "" Then Return

        SetGrid(grdSearchUser, DataSourceAccessUsers, False)
        pnlUserList.Visible = True
    End Sub
    Private Sub grdSearchUser_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdSearchUser.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdSearchUser, "Select$" & e.Row.RowIndex)
        End If
    End Sub
    Private Sub btnDataSourceAccessSave_Click(sender As Object, e As EventArgs) Handles btnDataSourceAccessSave.Click
        If ddlbAccessType.SelectedValue = "01" Then
            IawDB.execNonQuery("INSERT dbo.data_source_access (source_id, access_type, access_ref)
                                VALUES(@p1,@p2,@p3)", DataSourceID, "01", ddlbAccessRole.SelectedValue)
        End If

        If ddlbAccessType.SelectedValue = "02" Then
            IawDB.execNonQuery("INSERT dbo.data_source_access (source_id, access_type, access_ref)
                                VALUES(@p1,@p2,@p3)", DataSourceID, "02", grdSearchUser.SelectedValue)
        End If

        RefreshGrid("DataSourceAccess")
        SetModalState("")
    End Sub
    Private Sub btnDataSourceAccessCancel_Click(sender As Object, e As EventArgs) Handles btnDataSourceAccessCancel.Click
        SetModalState("")
    End Sub


#End Region

End Class