
Imports System.Drawing
Imports System.Web.Script.Serialization
Imports System.IO

<ButtonBarRequired(False), DirtyPageHandling(False), DisableEnterKey(False)>
Public Class client_images
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Dim SQL As String

    Private ReadOnly Property client_id As Integer
        Get
            Return ctx.session("client_id")
        End Get
    End Property
#End Region

#Region "Misc data"
    Private ReadOnly Property MaxImageSize As Integer
        Get
            Dim i As Integer = 50000
            If ViewState("MaxImageSize") Is Nothing Then
                i = IawDB.execScalar("select max_image_size from dbo.clients where client_id = @p1", client_id)
                ViewState("MaxImageSize") = i
            Else
                i = CInt(ViewState("MaxImageSize"))
            End If
            Return i
        End Get
    End Property

#End Region

#Region "DataSources"
    Property DataSourceID As Integer
        Get
            If ViewState("DataSourceID") Is Nothing Then
                Return -1
            End If
            Return ViewState("DataSourceID")
        End Get
        Set(value As Integer)
            ViewState("DataSourceID") = value
        End Set
    End Property

    Private ReadOnly Property PrimaryField As String
        Get
            Dim OccDate As DateTime = IawDB.execScalar("select dbo.dbf_nearest_datasource_occurrence(@p1,'1900-01-01')", DataSourceID)
            Dim Field As String

            Field = IawDB.execScalar("select field_name
                                        from dbo.data_source_field
                                       where source_id = @p1
                                         and source_date = @p2
                                         and item_ref_field = '3'", DataSourceID, OccDate)
            If String.IsNullOrEmpty(Field) Then Field = ctx.Translate("::LT_S0167") ' LT_S0167 - Unique Reference
            Return Field
        End Get
    End Property

    Private ReadOnly Property DataSourceDT As DataTable
        Get
            Dim DT As DataTable = ViewState("DatasourceDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("select DS.source_id,
                                                IsNull(DS.short_ref,'') as short_ref,
                                                DS.table_name,
                                                DS.primary_source
                                           from dbo.dbf_data_source_access(@p1) DSA
                                                JOIN dbo.data_source DS WITH (NOLOCK)
                                                    ON DS.source_id = DSA.source_id
                                          where exists (select top 1 1
                                                          from dbo.client_import_seg
                                                         where source_id = DS.source_id
                                                           and seg_status = '10')
                                            and primary_source = '1'
                                            and photos_applicable = '1'
                                          order by DS.short_ref", ctx.user_ref)
                ViewState("DataSourceDT") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property DataSourceDR() As DataRow
        Get
            Return DataSourceDT.Select("source_id = " + DataSourceID.ToString)(0)
        End Get
    End Property

#End Region

#Region "Display Data"

    Public Enum DSDType
        Data = 1
        Sort = 2
    End Enum
    Private ReadOnly Property CleanDataSourceDisplayDT As DataTable
        Get
            ViewState("DatasourceDisplay") = Nothing
            Return DataSourceDisplayDT
        End Get
    End Property
    Private ReadOnly Property DataSourceDisplayDT As DataTable
        Get
            Dim DT As DataTable = ViewState("DatasourceDisplay")
            If DT Is Nothing Then
                Dim OccDate As DateTime = IawDB.execScalar("select dbo.dbf_nearest_datasource_occurrence(@p1,'1900-01-01')", DataSourceID)

                SQL = "select DSD.disp_type,DSD.disp_seq,DSD.field_num,DSD.field_text,DSD.field_format,
                              DSF.field_column, DSF.field_type
                         from dbo.data_source_display DSD
                              left outer join dbo.data_source_field DSF
                                on DSF.source_id = DSD.source_id
                               and DSF.field_num = DSD.field_num
                               and DSF.source_date = @p2
                        where DSD.source_id = @p1"
                DT = IawDB.execGetTable(SQL, DataSourceID, OccDate)
                ViewState("DatasourceDisplay") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property DisplayData(DisT As DSDType) As DataTable
        Get
            Dim DType As String = "00"
            Select Case DisT
                Case DSDType.Data
                    DType = "01"
                Case DSDType.Sort
                    DType = "02"
            End Select
            Dim DBV As New DataView(DataSourceDisplayDT)
            DBV.RowFilter = "disp_type = '" + DType + "'"
            DBV.Sort = "disp_seq ASC"
            Return DBV.ToTable
        End Get
    End Property
    Public ReadOnly Property DDCols(DisT As DSDType) As String
        Get
            Dim Cols As New List(Of String)
            For Each DR As DataRow In DisplayData(DisT).Rows
                If DR("field_num") <> 0 Then
                    Cols.Add(DR("field_column"))
                End If
            Next
            Return String.Join(",", Cols.ToArray)
        End Get
    End Property

#End Region

#Region "Table Data"
    Private _DataFilter As DSFilterReturn = Nothing
    Private ReadOnly Property DataFilter(CurrentParmCount As Integer) As DSFilterReturn
        Get
            If _DataFilter Is Nothing Then
                Dim SourceID As Integer = DataSourceID
                Dim UserRef As Integer = ctx.session("user_ref")

                SQL = "select data_filter
                         from dbo.data_source_filter
                        where source_id = @p1
                          and user_ref = @p2"
                Dim FilterString As String = IawDB.execScalar(SQL, SourceID, UserRef)
                If String.IsNullOrEmpty(FilterString) Then
                    Return Nothing
                End If

                ' convert the json string into an object that lets us add the filters to a query
                _DataFilter = (New DatasourceFilter).ConvertStringToSQLParams(SourceID, FilterString, CurrentParmCount)
            End If
            Return _DataFilter
        End Get
    End Property

#End Region

#Region "Datasource Inage List"
    Private Property LineNo As Integer
        Get
            If ViewState("LineNo") Is Nothing Then
                ViewState("LineNo") = 0
            End If
            Return CInt(ViewState("LineNo"))
        End Get
        Set(value As Integer)
            ViewState("LineNo") = value
        End Set
    End Property

    Private ReadOnly Property CleanDatasourceImageListDT As DataTable
        Get
            ViewState("ImageList") = Nothing
            Return ImageListDT
        End Get
    End Property

    Private ReadOnly Property ImageListDT As DataTable
        Get
            Dim ImagePhoto As Image
            Dim Base64String As String
            Dim DisplayUsed As Boolean = False

            Dim DT As DataTable = ViewState("ImageList")
            If DT Is Nothing Then
                Using DB As New IawDB
                    DB.AddParameter("@SourceID", DataSourceID)
                    DT = DB.CallStoredProcDataTable("dbo.dbp_datasource_image_list")
                End Using

                ' add a column to hold the thumbnail
                Dim column As DataColumn = New DataColumn()
                column.DataType = System.Type.GetType("System.String")
                column.ColumnName = "thumbnail"
                column.MaxLength = -1 ' setting to -1 allows for max length strings
                DT.Columns.Add(column)

                Dim modifiedChecked As DataColumn = New DataColumn()
                modifiedChecked.DataType = System.Type.GetType("System.String")
                modifiedChecked.ColumnName = "modified_checked"
                modifiedChecked.MaxLength = -1
                DT.Columns.Add(modifiedChecked)

                Dim modifiedTooltip As DataColumn = New DataColumn()
                modifiedTooltip.DataType = System.Type.GetType("System.String")
                modifiedTooltip.ColumnName = "modified_tooltip"
                modifiedTooltip.MaxLength = -1
                DT.Columns.Add(modifiedTooltip)

                For Each DR As DataRow In DT.Rows
                    If DR("can_restore") Then
                        DR("modified_checked") = "fa-solid fa-check list-icon"
                        If DR("image_modified") Is System.DBNull.Value Then
                            DR("modified_tooltip") = ""
                        Else
                            DR("modified_tooltip") = ctx.Translate("::LT_S0171") + " " + Format(DR("image_modified"), "dd/MM/yyyy HH:mm") ' LT_S0171 - Modified on
                        End If
                    Else
                        DR("modified_checked") = ""
                        DR("modified_tooltip") = ""
                    End If

                    If DR.GetValue("item_display1", "") = "" And DR.GetValue("item_display2", "") = "" Then
                        DR("item_display1") = DR.GetValue("item_ref", "")
                    Else
                        DisplayUsed = True
                        If DR.GetValue("item_display1", "") = "" Then
                            DR("item_display1") = DR.GetValue("item_display2", "")
                        End If
                    End If

                    If DR("item_binary") Is System.DBNull.Value Then
                        'DR("thumbnail") = GenericFunctions.CreateBase64ImageFromText(ctx.Translate("::LT_S0014")) ' LT_S0014 - No Image
                        'DR("thumbnail") = GenericFunctions.CreateBase64CheckerPattern()
                    Else
                        ' now, for each row, generate a thumbnail and store it as a 'data:' string
                        Dim imgData As Byte() = DirectCast(DR("item_binary"), Byte())
                        Using ms As New IO.MemoryStream(imgData)
                            ImagePhoto = GenericFunctions.ResizeImage(Image.FromStream(ms), New Size(0, 30), GenericFunctions.SpecifyAxis.YAxis)
                        End Using
                        Using imgStream As New IO.MemoryStream()
                            ImagePhoto.Save(imgStream, Imaging.ImageFormat.Png)
                            imgData = imgStream.ToArray()
                        End Using
                        Base64String = Convert.ToBase64String(imgData)
                        DR("thumbnail") = Base64String
                    End If
                Next
                DT.AcceptChanges()
                ddlbSort.Visible = DisplayUsed
                ViewState("ImageList") = DT
            End If

            Return ImageListSearchAndSortDT(DT)
        End Get
    End Property
    Private ReadOnly Property ImageListDR(LineNo As Integer) As DataRow
        Get
            Return ImageListDT.Select("line_no = " + LineNo.ToString)(0)
        End Get
    End Property
    Private ReadOnly Property ImageListDR(ItemRef As String) As DataRow
        Get
            Return ImageListDT.Select("item_ref = '" + ItemRef + "'")(0)
        End Get
    End Property
    Private ReadOnly Property ImageListSearchAndSortDT(DT As DataTable) As DataTable
        Get
            Dim DV As New DataView(DT)

            Dim Srch As String = txtURfilter.Text.ToLower.Trim
            If Srch = "" Then
                DV.RowFilter = ""
            Else
                DV.RowFilter = String.Format("item_ref LIKE '%{0}%'" +
                                             " OR item_display1 Like '%{0}%'" +
                                             " OR item_display2 Like '%{0}%'", Srch)
            End If

            Dim Direction As String = "ASC"
            If btnSortDesc.Visible = True Then
                Direction = "DESC"
            End If
            If ddlbSort.SelectedValue = "ref" Then
                DV.Sort = "pk_sort " + Direction
            Else
                DV.Sort = "item_display1 " + Direction
            End If

            Return DV.ToTable()
        End Get
    End Property

#End Region
#Region "Page Events"

    Private Sub client_images_Init(sender As Object, e As EventArgs) Handles Me.Init
        useSqlViewState = True
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Select Case DataSourceDT.Rows.Count
            Case 0
                tblOuter.Visible = False
                pnlNoData.Visible = True
                Return
            Case 1
                pnlDataSource.Style("visibility") = "hidden"
        End Select

        grdDatasource.CssClass = "grid allow-hover"
        grdDatasourceImageList.CssClass = "grid allow-hover"

        ddlbSort.ToolTip = ctx.Translate("::LT_S0168") ' LT_S0168 - Sort By

        If Not Page.IsPostBack Then
            RefreshGrid("Datasource")
        End If
    End Sub

    Sub RefreshGrid(ByVal GridName As String)
        Select Case GridName
            Case "Datasource"
                If DataSourceDT.Rows.Count > 0 Then
                    grdDatasource.SelectedIndex = -1
                    SetGrid(grdDatasource, DataSourceDT, True)
                    DataSourceID = grdDatasource.DataKeys(grdDatasource.SelectedIndex).Value
                    pnlDatasourceImageList.GroupingText = PrimaryField
                    grdDatasourceImageList.SelectedIndex = -1
                    RefreshGrid("DatasourceImage")

                    ddlbSort.Items.Clear()
                    ddlbSort.Items.Add(New ListItem(PrimaryField, "ref"))
                    ddlbSort.Items.Add(New ListItem(ctx.Translate("::LT_S0169"), "desc")) ' LT_S0169 - Description
                End If

            Case "DatasourceImage"
                grdDatasourceImageList.Columns(2).Visible = True
                SetGrid(grdDatasourceImageList, CleanDatasourceImageListDT, True)
                If grdDatasourceImageList.SelectedIndex <> -1 Then
                    LineNo = grdDatasourceImageList.DataKeys(grdDatasourceImageList.SelectedIndex).Value
                    Dim DR As DataRow = ImageListDR(LineNo)
                    Selectimage(DR)
                Else
                    HideImage()
                End If

                ' if there are no ticks tisplay for modifed, then hide the column
                Dim Found As Boolean = False
                For Each DR As DataRow In CleanDatasourceImageListDT.Rows
                    If DR("modified_checked") <> "" Then
                        Found = True
                        Exit For
                    End If
                Next
                If Not Found Then
                    grdDatasourceImageList.Columns(2).Visible = False
                End If

                'reset the scroll positions
                ResetScrollPosition("divList")
        End Select
    End Sub

    Private Sub grdDatasourceImageList_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdDatasourceImageList.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDatasourceImageList, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdDatasourceImageList_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdDatasourceImageList.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rowView As DataRowView = DirectCast(e.Row.DataItem, DataRowView)
            ' Assign the base64 string to ImageUrl
            Dim imgPhoto As WebControls.Image = DirectCast(e.Row.FindControl("imgPhoto"), WebControls.Image)
            Dim lblMissing As Label = DirectCast(e.Row.FindControl("lblImageMissing"), Label)

            If rowView("item_binary") Is System.DBNull.Value Then
                imgPhoto.Visible = False
                lblMissing.Visible = True
            Else
                imgPhoto.Visible = True
                lblMissing.Visible = False
                If rowView("thumbnail").startswith("data:") Then
                    imgPhoto.ImageUrl = rowView("thumbnail")
                Else
                    Dim extension As String = Path.GetExtension(rowView("item_filename")).Substring(1)
                    imgPhoto.ImageUrl = "data:image/" + extension + ";base64," + rowView("thumbnail")
                End If
            End If

            Dim modified As Label = DirectCast(e.Row.FindControl("lblModified"), Label)
            modified.CssClass = rowView("modified_checked")
            'modified.ToolTip = rowView("modified_tooltip")

            If rowView("modified_tooltip") <> "" Then
                e.Row.ToolTip = rowView("modified_tooltip")
            End If

        End If
    End Sub

    Private Sub grdDatasourceImageList_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDatasourceImageList.RowCommand
        Dim RowIndex As Integer = Int32.Parse(e.CommandArgument.ToString())
        LineNo = grdDatasourceImageList.DataKeys(RowIndex).Value

        Dim DR As DataRow = ImageListDR(LineNo)

        Select Case e.CommandName
            Case "Select"
                Selectimage(DR)
        End Select
    End Sub

    Sub Selectimage(DR As DataRow)
        pnlImage.GroupingText = String.Format("{0} {1}", DR("item_ref"), DR("item_display1"))

        If DR("can_restore") Then
            btnRestoreImage.Visible = True
        Else
            btnRestoreImage.Visible = False
        End If

        If DR("has_image") AndAlso Not IsDBNull(DR("item_binary")) Then
            Dim ImageData As Byte() = DR("item_binary") '"CType(IawDB.execScalar("select item_binary from dbo.client_document where unique_id = @p1", DR("unique_id")), Byte())
            Dim base64String = Convert.ToBase64String(ImageData)
            Dim extension As String = Path.GetExtension(DR("item_filename")).Substring(1)
            hdnImage.Value = "data:image/" + extension + ";base64," + base64String
            lblUploadedDate.Text = ctx.Translate("::LT_S0170") + " " + Format(DR("image_date"), "dd/MM/yyyy HH:mm") ' LT_S0170 - Uploaded
            If Not DR("can_restore") Then
                btnRemoveImage.Visible = True
            Else
                btnRemoveImage.Visible = False
            End If
        Else
            HideImage()
        End If
    End Sub

    Sub HideImage()
        lblUploadedDate.Text = ""
        hdnImage.Value = ""
        btnRemoveImage.Visible = False
    End Sub

    Private Sub btnSavePostback_Click(sender As Object, e As EventArgs) Handles btnSavePostback.Click
        Dim base64String As String = hdnImage.Value
        If base64String <> "" Then
            Dim DR As DataRow = ImageListDR(LineNo)
            Dim Filetype As String

            Dim prefix As String
            If base64String.StartsWith("data:image/jpeg;base64,") Then
                prefix = "data:image/jpeg;base64,"
                Filetype = "jpeg"
            ElseIf base64String.StartsWith("data:image/png;base64,") Then
                prefix = "data:image/png;base64,"
                Filetype = "png"
            ElseIf base64String.StartsWith("data:image/gif;base64,") Then
                prefix = "data:image/gif;base64,"
                Filetype = "gif"
            ElseIf base64String.StartsWith("data:image/bmp;base64,") Then
                prefix = "data:image/bmp;base64,"
                Filetype = "bmp"
            ElseIf base64String.StartsWith("data:image/tiff;base64,") Then
                prefix = "data:image/tiff;base64,"
                Filetype = "tiff"
            ElseIf base64String.StartsWith("data:image/svg+xml;base64,") Then
                prefix = "data:image/svg+xml;base64,"
                Filetype = "svg"
            ElseIf base64String.StartsWith("data:image/webp;base64,") Then
                prefix = "data:image/webp;base64,"
                Filetype = "webp"
            Else
                ' Handle unexpected format
                Throw New FormatException("Unexpected image format")
            End If

            ' Remove the prefix
            base64String = base64String.Replace(prefix, "")

            ' Convert the base64 string back to a byte array
            Dim ImageData As Byte() = Convert.FromBase64String(base64String)

            ImageData = GenericFunctions.ReduceImageSize(ImageData, MaxImageSize, "A." + Filetype)
            Dim filename As String = DR("item_ref") + "." + Filetype
            If DR("unique_id") IsNot DBNull.Value Then
                If hdnIsNewImage.Value Then
                    SQL = "update dbo.client_document
                              set item_date = GETDATE(),
                                  item_filename = @p1,
                                  item_binary = @p2,
                                  org_binary = Null,
                                  item_modified = null
                            where unique_id = @p3"
                    IawDB.execNonQuery(SQL, filename, ImageData, DR("unique_id"))
                Else
                    SQL = "update dbo.client_document
                            set org_binary = CASE
                                        WHEN org_binary IS NULL THEN item_binary
                                        ELSE org_binary
                                       END,
                            item_binary = @p1,
                            item_modified = GETDATE()
                            where unique_id = @p2"
                    IawDB.execNonQuery(SQL, ImageData, DR("unique_id"))
                End If
            Else
                SQL = "INSERT dbo.client_document (client_id, collection_id, item_date, item_ref, 
                                                   item_type, item_desc, item_binary, item_filename)
                       VALUES (@p1,1,GETDATE(),@p2,'01','',@p3,@p4)"
                IawDB.execNonQuery(SQL, client_id,
                                   DR("item_ref"),
                                   ImageData,
                                   filename)
            End If

            SetGrid(grdDatasourceImageList, CleanDatasourceImageListDT, True)
            RefreshGrid("DatasourceImage")
            'txtURfilter.Text = ""
        End If
    End Sub
    Private Sub btnRemoveImage_Click(sender As Object, e As EventArgs) Handles btnRemoveImage.Click
        mpeRemoveImage.Show()
    End Sub
    Private Sub btnRemoveImageOk_Click(sender As Object, e As EventArgs) Handles btnRemoveImageOk.Click
        Dim DR As DataRow = ImageListDR(LineNo)
        SQL = "DELETE FROM dbo.client_document where unique_id = @p1"
        IawDB.execNonQuery(SQL, DR("unique_id"))
        RefreshGrid("DatasourceImage")
    End Sub
    Private Sub btnRemoveImageCancel_Click(sender As Object, e As EventArgs) Handles btnRemoveImageCancel.Click
        mpeRemoveImage.Hide()
    End Sub
    Private Sub btnRestoreImage_Click(sender As Object, e As EventArgs) Handles btnRestoreImage.Click
        mpeRestoreImage.Show()
    End Sub
    Private Sub btnRestoreImageOk_Click(sender As Object, e As EventArgs) Handles btnRestoreImageOk.Click
        Dim DR As DataRow = ImageListDR(LineNo)
        SQL = "update dbo.client_document
                      set item_binary = org_binary,
                          org_binary = Null,
                          item_modified = null
                    where unique_id = @p1"
        IawDB.execNonQuery(SQL, DR("unique_id"))
        RefreshGrid("DatasourceImage")
    End Sub
    Private Sub btnRestoreImageCancel_Click(sender As Object, e As EventArgs) Handles btnRestoreImageCancel.Click
        mpeRestoreImage.Hide()
    End Sub
    Private Sub txtURfilter_TextChanged(sender As Object, e As EventArgs) Handles txtURfilter.TextChanged
        RefreshGrid("DatasourceImage")
    End Sub
#End Region
#Region "Grid Events"
    Sub grdDatasource_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdDatasource.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDatasource, "Select$" & e.Row.RowIndex)
        End If
    End Sub
    Sub grdDatasource_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDatasource.RowCommand
        Dim RowIndex As Integer = Int32.Parse(e.CommandArgument.ToString())
        DataSourceID = grdDatasource.DataKeys(RowIndex).Value

        Select Case e.CommandName
            Case "Select"
                txtURfilter.Text = ""
                ddlbSort.SelectedIndex = 0
                pnlDatasourceImageList.GroupingText = PrimaryField
                grdDatasourceImageList.SelectedIndex = -1
                RefreshGrid("DatasourceImage")

                ddlbSort.Items.Clear()
                ddlbSort.Items.Add(New ListItem(PrimaryField, "ref"))
                ddlbSort.Items.Add(New ListItem(ctx.Translate("::LT_S0169"), "desc")) ' LT_S0169 - Description
            Case "AmendRow"
                Dim url = SawUtil.encryptQuery("data_source_display.aspx?origin=images&sourceid=" + DataSourceID.ToString, True)
                ctx.redirect(url)
        End Select
    End Sub

#End Region

#Region "Misc"

    Protected Sub ResetScrollPosition(divId As String)
        Dim mstr As IngenWebMaster = CType(Me.Master, IngenWebMaster)

        Dim hdnField As HiddenField = mstr.divScrollPos
        If hdnField.Value = "" Then Return

        Dim serializer As New JavaScriptSerializer()
        Dim scrollData As List(Of Dictionary(Of String, Object)) = serializer.Deserialize(Of List(Of Dictionary(Of String, Object)))(hdnField.Value)

        ' Find the div by its ID and reset its scroll position
        For Each data As Dictionary(Of String, Object) In scrollData
            If data("id").ToString() = divId Then
                data("scrollTop") = 0
                Exit For
            End If
        Next

        ' Serialize the modified data back to the hidden field
        hdnField.Value = serializer.Serialize(scrollData)
    End Sub

    Private Sub ddlbSort_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbSort.SelectedIndexChanged
        ReselectRow()
    End Sub

    Private Sub btnSortAsc_Click(sender As Object, e As EventArgs) Handles btnSortAsc.Click
        btnSortAsc.Visible = False
        btnSortDesc.Visible = True
        ReselectRow()
    End Sub

    Private Sub btnSortDesc_Click(sender As Object, e As EventArgs) Handles btnSortDesc.Click
        btnSortAsc.Visible = True
        btnSortDesc.Visible = False
        ReselectRow()
    End Sub

    Private Sub ReselectRow()
        Dim DT As DataTable = ImageListSearchAndSortDT(ImageListDT)
        SetGrid(grdDatasourceImageList, DT, True)

        Dim DR As DataRow = ImageListDR(LineNo)
        Selectimage(DR)

        ' Assuming dataView is your DataView
        For i As Integer = 0 To DT.Rows.Count - 1
            If ImageListDT(i)("line_no").ToString() = LineNo Then
                grdDatasourceImageList.SelectedIndex = i
                Exit For
            End If
        Next
    End Sub

#End Region


End Class