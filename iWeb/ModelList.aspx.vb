
<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class ModelList
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Private SQL As String

    Public ReadOnly Property ClientAttributes As String
        Get
            Return IawDB.execScalar("select attributes from dbo.clients where client_id = @p1", client_id)
        End Get
    End Property

    Public Property ModalState() As String
        Get
            Dim B As String = ViewState("ModalState")
            If B = Nothing Then
                ViewState("ModalState") = ""
                B = False
            End If
            Return B
        End Get
        Set(ByVal value As String)
            ViewState("ModalState") = value
        End Set
    End Property

    Public ReadOnly Property client_id As Integer
        Get
            Return ctx.session("client_id")
        End Get
    End Property

#End Region

    Private ReadOnly Property ModelsDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("ModelsDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("select M.model_key,     
                                                A.tense,
                                                CASE A.tense WHEN '0' then 'Previous'
                                                             WHEN '1' then 'Current'
                                                             WHEN '2' then 'Future'
                                                             WHEN '3' then 'Other Users''' END as tense_text,
                                                IsNull(F.filter_name,'') as filter_name,
                                                S.source_id,      
                                                M.view_id,        
                                                M.user_ref,      
                                                M.effective_date,
                                                M.published,     
                                                case when JSON_VALUE(M.attributes, '$.showImages') = 'true' then '1' else '0' end AS show_photos,
                                                IsNull(B.short_ref,'') as short_ref,
                                                CASE WHEN M.view_id = 0 then M.reference else IsNull(S.view_ref,'') end as view_ref
                                           FROM dbo.dbf_model_access(@p1) A
                                                JOIN dbo.model_header M 
                                                  ON M.model_key = A.model_key
                                                LEFT OUTER JOIN dbo.data_view S
                                                  ON S.view_id = M.view_id
                                                LEFT OUTER JOIN dbo.data_source B
                                                  ON B.source_id = S.source_id
                                                LEFT OUTER JOIN dbo.data_source_filter F
                                                  ON F.source_id = B.source_id
                                                 AND F.user_ref = M.user_ref
                                          WHERE M.user_ref = @p1
                                             OR M.published = '1'
                                          ORDER BY S.view_ref, M.effective_date DESC", ctx.user_ref)
                ViewState("ModelsDT") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property filteredModelsDT As DataTable
        Get
            Dim DV As New DataView(ModelsDT)
            Dim RF As New List(Of String)

            If cbPublishedOnly.Checked Then
                RF.Add("published='1'")
            Else
                If cbPrevious.Checked Then RF.Add("tense='0'")
                If cbCurrent.Checked Then RF.Add("tense='1'")
                If cbFuture.Checked Then RF.Add("tense='2'")
                If cbOtherUsers.Checked Then RF.Add("tense='3'")
            End If

            If RF.Count > 0 Then
                DV.RowFilter = String.Join(" or ", RF.ToArray)
            Else
                DV.RowFilter = ""
            End If

            Return DV.ToTable
        End Get
    End Property

    Private ReadOnly Property ModelsDR(ByVal model_key As Integer) As DataRow
        Get
            Return ModelsDT.Select("model_key = " + model_key.ToString)(0)
        End Get
    End Property

    Private ReadOnly Property MyModelsDT() As DataTable
        Get
            Dim GV As New DataView(ModelsDT)
            GV.RowFilter = "user_ref = '" + ctx.user_ref + "'"
            Return GV.ToTable
        End Get
    End Property

    'Private ReadOnly Property DataSourceDT() As DataTable
    '    Get
    '        Dim DT As DataTable = ViewState("DataSourceDT")
    '        If DT Is Nothing Then
    '            DT = IawDB.execGetTable("select source_id,
    '                                            IsNull(source_desc,'') as source_desc
    '                                       from dbo.data_source WITH (NOLOCK) 
    '                                      where client_id = @p1
    '                                      order by source_desc", ctx.session("client_id"))
    '            ViewState("DataSourceDT") = DT
    '        End If
    '        Return DT
    '    End Get
    'End Property

    Private ReadOnly Property DataViewDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("SetsDT")
            If DT Is Nothing Then
                SQL = "SELECT view_id,
                              view_ref,
                              attributes 
                         FROM(SELECT 0 AS view_id,
                                      @p2 AS view_ref,
                                      @p3 AS attributes 
                                UNION ALL 
                               SELECT S.view_id,
                                      IsNull(S.view_ref, '') AS view_ref,
                                      IsNull(S.attributes, '') AS attributes 
                                 FROM dbo.dbf_data_source_access(@p1) BA 
                                      JOIN dbo.data_source B WITH (NOLOCK) 
                                        ON B.source_id = BA.source_id 
                                      JOIN dbo.data_view S WITH (NOLOCK) 
                                        ON S.source_id = BA.source_id 
                                WHERE S.user_ref = @p1) x 
                        ORDER BY CASE 
                                  WHEN view_id = 0 THEN '' 
                                  Else view_ref
                                 End"
                ' LT_A0086 'Non-Data'
                DT = IawDB.execGetTable(SQL, ctx.user_ref, ctx.Translate("::LT_A0086"), ClientAttributes)
                ViewState("SetsDT") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property DataViewDR(ByVal view_id As Integer) As DataRow
        Get
            Return DataViewDT.Select("view_id = " + view_id.ToString)(0)
        End Get
    End Property

    Private ReadOnly Property CleanModelReferencesDT As DataTable
        Get
            ViewState("ModelReferences") = Nothing
            Return ModelReferencesDT
        End Get
    End Property

    Protected ReadOnly Property ModelReferencesDT As DataTable
        Get
            Dim DT As DataTable = ViewState("ModelReferences")
            If DT Is Nothing Then
                SQL = "select distinct reference
                         from dbo.model_header 
                        where user_ref = @p1
                          and view_id = 0"
                DT = IawDB.execGetTable(SQL, ctx.user_ref)
                ViewState("ModelReferences") = DT
            End If
            Return DT
        End Get
    End Property

    Private Property ColSort As String
        Get
            If ViewState("ColSort") Is Nothing Then Return ""
            Return ViewState("ColSort")
        End Get
        Set(value As String)
            ViewState("ColSort") = value
        End Set
    End Property
    Private Property ColSortDir As SortDirection
        Get
            If ViewState("ColSortDir") Is Nothing Then Return SortDirection.Ascending
            Return ViewState("ColSortDir")
        End Get
        Set(value As SortDirection)
            ViewState("ColSortDir") = value
        End Set
    End Property


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not Page.IsPostBack Then
            If ctx.role.ContainsOneOf("client_user", "client_guest") Then
                If ModelsDT.Rows.Count = 0 Then
                    ' "There are no charts For you To look at yet, please try again later."
                    Me.msg.MessageText = BuildMessage("IN_IM003")
                    ErrorPanel.Visible = True
                    Panel1.Visible = False
                    Return
                End If
            Else
                If DataViewDT.Rows.Count = 0 And ModelsDT.Rows.Count = 0 Then
                    ' "No Data Sets','No Data Views have been defined, please add at least one."
                    Me.msg.MessageText = BuildMessage("IN_IM002")
                    ErrorPanel.Visible = True
                    Panel1.Visible = False
                    Return
                End If
            End If

            Populate()
            ModalState = ""

        End If
        SetModalState()
        cbOptions.Visible = ctx.IsInsertAllowed

        grdModels.CssClass = "allow-hover"

    End Sub

    Private Sub Populate()
        SetDDLB(ddlbView, DataViewDT, "view_ref", "view_id")
        RefreshGrid("Model")
    End Sub

    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeModels.Hide()

        Select Case ModalState
            Case "Model"
                mpeModels.Show()

        End Select
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "Model"
                ViewState("ModelsDT") = Nothing
                SetGrid(grdModels, filteredModelsDT, False)

        End Select
    End Sub

#Region "Models Grid & Form"

    Private Sub grdModels_RowDataB0ound(sender As Object, e As GridViewRowEventArgs) Handles grdModels.RowDataBound
        Dim ctrl As Control

        If e.Row.RowType = DataControlRowType.Header Then
            If Not ctx.IsInsertAllowed Or DataViewDT.Rows.Count = 0 Then
                ' if not allowed to insert, remove the insert button
                ctrl = e.Row.FindControl("btnModelAdd")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
            End If
        End If

        If e.Row.RowType = DataControlRowType.DataRow Then

            ' if not allowed to update then hide the update button..  
            Dim AmendAllowed As Boolean = True
            Dim DeleteAllowed As Boolean = True

            If Not ctx.IsUpdateAllowed Then AmendAllowed = False
            If Not ctx.IsDeleteAllowed Then DeleteAllowed = False

            Dim row As GridViewRow = e.Row
            If row.DataItem Is Nothing Then Return

            Dim rv As DataRowView = CType(row.DataItem, DataRowView)
            If rv("user_ref") <> ctx.user_ref Then
                AmendAllowed = False
                DeleteAllowed = False
            End If

            If Not AmendAllowed Then
                ctrl = e.Row.FindControl("btnAmend")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
            End If

            If Not DeleteAllowed Then
                ctrl = e.Row.FindControl("btnDel")
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
            End If
        End If

    End Sub

    Private Sub grdModels_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdModels.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        Dim ModelKey As Integer
        Dim DR As DataRow = Nothing

        If e.CommandName = "Sort" Then Return

        If e.CommandName = "New" Then
            hdnModelID.Value = ""
            ddlbView.SelectedIndex = 0
            dtEffectiveDate.CurrentDate = Today
            trPublished.Visible = False
            cbPublished.Checked = False
            cbShowPhotos.Checked = True

            ProtectDialogFields(True)
            ViewSelectionDisplay()

            ModalState = "Model"
            SetModalState()
            Return
        End If

        CurRowIndex = grdModels.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        ModelKey = CInt(grdModels.DataKeys(RowIndex).Value)
        DR = ModelsDR(ModelKey)

        Select Case e.CommandName

            Case "AmendRow"
                hdnModelID.Value = ModelKey.ToString
                ddlbView.SelectedValue = DR("view_id")
                dtEffectiveDate.CurrentDate = DR("effective_date")
                trPublished.Visible = True
                cbPublished.Checked = DR("published") = "1"
                cbShowPhotos.Checked = DR("show_photos") = "1"

                ViewSelectionDisplay()
                If ddlbView.SelectedValue = 0 Then
                    txtReference.Text = DR("view_ref")
                End If

                ProtectDialogFields(False)
                SetModalState("Model")

            Case "DeleteRow"

                IawDB.execNonQuery("DELETE dbo.model_header where model_key = @p1", ModelKey)

                RefreshGrid("Model")

            Case "ShowModel"
                ctx.redirect("~/Chart/Diagram.aspx?modelkey=" + ModelKey.ToString)

        End Select
    End Sub

    Private Sub grdModels_Sorting(sender As Object, e As GridViewSortEventArgs) Handles grdModels.Sorting
        Dim dvSortedView As New DataView(filteredModelsDT) With {
            .Sort = e.SortExpression
        }
        grdModels.DataSource = dvSortedView
        grdModels.DataBind()
    End Sub

    Private Sub btnSettingsCancel_Click(sender As Object, e As EventArgs) Handles btnSettingsCancel.Click
        SetModalState("")
    End Sub

    Private Sub cvEffectiveDate_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles cvEffectiveDate.ServerValidate
        args.IsValid = True

        Dim mk As Integer
        If hdnModelID.Value = "" Then
            mk = -1
        Else
            mk = CInt(hdnModelID.Value)
        End If

        Dim Reference As String = ""
        If ddlbView.SelectedValue = 0 Then
            If ddlbReference.Visible Then
                Reference = ddlbReference.SelectedValue
            Else
                Reference = txtReference.Text.Trim
                If String.IsNullOrEmpty(Reference) Then
                    SetFocus(txtReference)
                    Return
                End If
            End If

            If IawDB.execScalar("select count(1) 
                                   from dbo.model_header
                                  where user_ref = @p1
                                    and view_id = 0
                                    and reference = @p2
                                    and effective_date = @p3
                                    and model_key <> @p4",
                                    ctx.user_ref, Reference, dtEffectiveDate.CurrentDate, mk) > 0 Then
                args.IsValid = False
            End If
        Else
            If IawDB.execScalar("select count(1) 
                                   from dbo.model_header
                                  where user_ref = @p1
                                    and view_id = @p2
                                    and effective_date = @p3
                                    and model_key <> @p4",
                            ctx.user_ref, ddlbView.SelectedValue, dtEffectiveDate.CurrentDate, mk) > 0 Then
                args.IsValid = False
            End If
        End If
    End Sub

    Private Sub btnSettingsSave_Click(sender As Object, e As EventArgs) Handles btnSettingsSave.Click
        If Not Page.IsValid Then Return
        Dim ModelKey As Integer
        Dim attributes As String = ""
        Dim Reference As String = ""

        If hdnModelID.Value = "" Then
            If ddlbView.SelectedValue = 0 Then
                attributes = ClientAttributes
            Else
                Dim ViewDR As DataRow = DataViewDR(ddlbView.SelectedValue)
                attributes = ViewDR("attributes")
            End If
        Else
            ModelKey = CInt(hdnModelID.Value)
            attributes = IawDB.execScalar("select attributes
                                             from dbo.model_header
                                            where model_key = @p1", ModelKey)
        End If

        If ddlbView.SelectedValue = 0 Then
            If ddlbReference.Visible Then
                Reference = ddlbReference.SelectedValue
            Else
                Reference = txtReference.Text
            End If
        End If

        Dim CA As New ChartAttrib(attributes)
        CA.Data.showImages = cbShowPhotos.Checked
        attributes = CA.GetData

        If hdnModelID.Value = "" Then
            SQL = "INSERT dbo.model_header
                          (view_id,
                           user_ref,
                           effective_date,
                           published,
                           attributes,
                           reference)
                   VALUES (@p1,@p2,@p3,@p4,@p5,@p6)
                   SELECT SCOPE_IDENTITY()"
            ModelKey = IawDB.execScalar(SQL,
                           ddlbView.SelectedValue,
                           ctx.user_ref,
                           dtEffectiveDate.CurrentDate,
                           If(cbPublished.Checked, "1", "0"),
                           attributes, Reference)

            ctx.redirect("~/Chart/Diagram.aspx?modelkey=" + ModelKey.ToString)
            Return
        Else
            IawDB.execNonQuery("UPDATE dbo.model_header
                                   SET view_id = @p2,
                                       user_ref = @p3,
                                       effective_date = @p4, 
                                       published = @p5,
                                       attributes = @p6,
                                       reference = @p7
                                 WHERE model_key = @p1",
                               ModelKey,
                               CInt(ddlbView.SelectedValue),
                               ctx.user_ref,
                               dtEffectiveDate.CurrentDate,
                               If(cbPublished.Checked, "1", "0"),
                               attributes, Reference)
        End If

        RefreshGrid("Model")
        SetModalState("")
    End Sub

#End Region
#Region "Grid & Form"

    Private Sub cbfilter(sender As Object, e As EventArgs) Handles cbPrevious.CheckedChanged,
                                                                   cbCurrent.CheckedChanged,
                                                                   cbFuture.CheckedChanged,
                                                                   cbOtherUsers.CheckedChanged,
                                                                   cbPublishedOnly.CheckedChanged
        RefreshGrid("Model")
    End Sub

    Private Sub ddlbView_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbView.SelectedIndexChanged
        ViewSelectionDisplay()
    End Sub
    Private Sub ViewSelectionDisplay()
        If ddlbView.SelectedValue = 0 Then
            SetDDLB(ddlbReference, CleanModelReferencesDT, "reference", "reference")
            trReference.Visible = True
            btnRef(False)
        Else
            trReference.Visible = False
        End If

    End Sub

    Private Sub btnRefAdd_Click(sender As Object, e As ImageClickEventArgs) Handles btnRefAdd.Click
        btnRef(True)
        SetFocus(txtReference)
    End Sub

    Private Sub btnRefCancel_Click(sender As Object, e As ImageClickEventArgs) Handles btnRefCancel.Click
        btnRef(False)
    End Sub

    Private Sub ProtectDialogFields(newRow As Boolean)
        ddlbView.Enabled = newRow
        'ddlbReference.Enabled = newRow
        'txtReference.Enabled = newRow
        'dtEffectiveDate.Enabled = newRow
        'btnRefAdd.Visible = newRow
    End Sub

    Private Sub btnRef(addPressed As Boolean)
        If ModelReferencesDT.Rows.Count = 0 Then
            ' there aren't any existing references, so force textbox
            ddlbReference.Visible = False
            txtReference.Text = ""
            txtReference.Visible = True
            btnRefAdd.Visible = False
            btnRefCancel.Visible = False
        Else
            ddlbReference.Visible = Not addPressed
            txtReference.Text = ""
            txtReference.Visible = addPressed
            btnRefAdd.Visible = Not addPressed
            btnRefCancel.Visible = addPressed
        End If

    End Sub

#End Region

End Class