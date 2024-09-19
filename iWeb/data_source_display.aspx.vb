Imports System.Web.UI.DataVisualization.Charting
Imports IAW.controls

<ButtonBarRequired(False)>
Public Class data_source_display
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

#End Region

#Region "Passed Arguments"
    Private ReadOnly Property Origin As String
        Get
            Return ctx.item("origin")
        End Get
    End Property
    Private ReadOnly Property DatasourceID As Integer
        Get
            Return CInt(ctx.item("sourceid"))
        End Get
    End Property

#End Region

#Region "Data Source Fields"

    Property FieldNum As Integer
        Get
            Return ViewState("FieldNum")
        End Get
        Set(value As Integer)
            ViewState("FieldNum") = value
        End Set
    End Property
    ReadOnly Property CleanFieldsDT() As DataTable
        Get
            ViewState("FieldsDT") = Nothing
            Return FieldsDT
        End Get
    End Property
    Property FieldsIncludeText As Boolean
        Get
            If ViewState("FieldsIncludeText") Is Nothing Then
                ViewState("FieldsIncludeText") = True
            End If
            Return ViewState("FieldsIncludeText")
        End Get
        Set(value As Boolean)
            ViewState("FieldsIncludeText") = value
        End Set
    End Property

    ReadOnly Property FieldsDT(Optional includeText As Boolean = True) As DataTable
        Get
            Dim addText As Integer = If(includeText, 1, 0)

            Dim DT As DataTable = ViewState("FieldsDT")
            If DT Is Nothing Then

                DT = IawDB.execGetTable("SELECT 0 as field_num,
                                                @p3 as field_name,
                                                '' as field_type,
                                                0 as field_length
                                          WHERE @p4 = 1
                                         union
                                         SELECT field_num,
                                                field_name,
                                                field_type,
                                                field_length
                                           FROM dbo.data_source_field WITH (NOLOCK)
                                          WHERE source_id = @p1
                                            AND source_date = @p2
                                          ORDER BY field_num", DatasourceID, DistinctDate, ctx.Translate("::LT_S0064"), addText) ' LT_S0064 - Text
                ViewState("FieldsDT") = DT
            End If

            Dim DR As DataRow = DT.AsEnumerable().FirstOrDefault(Function(row) row.Field(Of Integer)("field_num") = 0)

            If includeText And DR Is Nothing Then
                DR = DT.NewRow
                DR("field_num") = 0
                DR("field_name") = ctx.Translate("::LT_S0064") ' LT_S0064 - Text
                DR("field_type") = ""
                DR("field_length") = 0
                DT.Rows.Add(DR)
            End If
            If Not includeText And DR IsNot Nothing Then
                DT.Rows.Remove(DR)
            End If

            FieldsIncludeText = includeText

            Dim DV As New DataView(DT)
            DV.Sort = "field_num"
            DT = DV.ToTable

            Return DT
        End Get
    End Property
    ReadOnly Property FieldsDR(field As Integer) As DataRow
        Get
            Return FieldsDT.Select("field_num = " + field.ToString)(0)
        End Get
    End Property
#End Region
#Region "Distinct Dates"
    Private Property DistinctDate As Date
        Get
            Return ViewState("DistinctDate")
        End Get
        Set(value As Date)
            ViewState("DistinctDate") = value
        End Set
    End Property
    Private ReadOnly Property CleanDistinctDateDT() As DataTable
        Get
            ViewState("DistinctDateDT") = Nothing
            Return DistinctDateDT
        End Get
    End Property
    Private ReadOnly Property DistinctDateDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("DistinctDateDT")
            If DT Is Nothing Then
                SQL = "select DISTINCT source_date 
                        from data_source_field 
                        where source_id = @p1"
                DT = IawDB.execGetTable(SQL, DatasourceID)
                ViewState("DistinctDateDT") = DT
            End If
            Return DT
        End Get
    End Property
#End Region
#Region "Data Source Display"

    ReadOnly Property DisplayGrid As IAWGrid
        Get
            Dim targetGrid As IAWGrid = Nothing
            Select Case hdnAccValue.Value
                Case "Display"
                    targetGrid = grdDisplay
                Case "Sort"
                    targetGrid = grdSort
            End Select
            Return targetGrid
        End Get
    End Property
    ReadOnly Property CleanDataSourceDisplayDT As DataTable
        Get
            ViewState("DataSourceDisplayDT") = Nothing
            Return DataSourceDisplayDT
        End Get
    End Property
    ReadOnly Property DataSourceDisplayDT As DataTable
        Get
            Dim i As Integer
            Dim DR As DataRow

            Dim DT As DataTable = ViewState("DataSourceDisplayDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT 
                                            '' AS line_desc,
                                            '' AS col_desc,
                                            DCD.disp_type,
                                            DCD.disp_seq,
                                            DCD.disp_line,
                                            DCD.field_num,
                                            DCD.field_text,
                                            DCD.field_format,
                                            display_name = CASE
                                                WHEN DCD.field_num = 0 THEN '""' + DCD.field_text + '""'
                                                ELSE LDSF.field_name
                                            END
                                        FROM dbo.data_source_display DCD
                                            Left OUTER JOIN dbo.data_source_field LDSF
                                              On LDSF.source_id = DCD.source_id
                                             and LDSF.source_date = DCD.source_date
                                             And LDSF.field_num = DCD.field_num
                                        WHERE DCD.source_id = @p1
                                            AND DCD.source_date = @p2
                                        ORDER BY disp_type,
                                            disp_line,
                                            disp_seq", DatasourceID, DistinctDate)

                ' Display - add the 2 lines
                ' Sort - add the 1 line
                For i = 1 To 2
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "01"
                    DR("disp_line") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)

                    If i = 1 Then
                        DR = DT.NewRow()
                        SetDisplayDefaults(DR)
                        DR("disp_type") = "02"
                        DR("disp_line") = i
                        DR("disp_seq") = 999
                        DT.Rows.Add(DR)
                    End If
                Next

                Dim DV As New DataView(DT)
                DV.Sort = "disp_type, disp_line, disp_seq"
                DT = DV.ToTable
                ViewState("DataSourceDisplayDT") = DT
            End If

            Return DT
        End Get
    End Property

    Sub SetDisplayDefaults(ByRef DR As DataRow)
        DR("disp_type") = ""
        DR("disp_seq") = 0
        DR("disp_line") = 0
        DR("field_num") = 0
        DR("field_text") = ""
    End Sub
    Sub ReSortDisplayDT()
        Dim DT As DataTable = ViewState("DataSourceDisplayDT")
        Dim DV As New DataView(DT)
        DV.Sort = "disp_type, disp_line, disp_seq"
        ViewState("DataSourceDisplayDT") = DV.ToTable
    End Sub
    Function GetTypeLineColCount(TYP As String, LINE As Integer) As Integer
        Return DataSourceDisplayDT.AsEnumerable().
                                        Count(Function(r) CStr(r("disp_type")) = TYP AndAlso
                                                          CInt(r("disp_line")) = LINE AndAlso
                                                          CInt(r("disp_seq")) <> 999 AndAlso
                                                          CInt(r("disp_seq")) <> 0)
    End Function

    ReadOnly Property DataViewDisplayType(dispType As String) As DataTable
        Get
            Dim DV As New DataView(DataSourceDisplayDT)
            DV.RowFilter = "disp_type='" + dispType + "'"
            DV.Sort = "disp_type, disp_line, disp_seq"
            Return DV.ToTable
        End Get
    End Property
    ReadOnly Property DataViewDisplayTypeCols(dispType As String) As DataTable
        Get
            Dim DV As New DataView(DataSourceDisplayDT)
            DV.RowFilter = "disp_type='" + dispType + "'"
            DV.Sort = "disp_type, disp_line, disp_seq"
            Return DV.ToTable
        End Get
    End Property

#End Region
#Region "View Display Grids"
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

        DistinctDate = CDate(grdDates.DataKeys(RowIndex).Value)

        Select Case e.CommandName
            Case "Select"
                RefreshGrid("SourceDisplay")

        End Select

    End Sub
    Sub grdFields_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdFields.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Attributes("draggable") = "true"
            e.Row.Attributes("ondragstart") = "dragStart(event)"

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            If rv("field_num") = 0 Then
                e.Row.Cells(0).Text = "<span class='iaw-font iaw-text'></span>"
            End If

        End If
    End Sub

    Private Sub grdFields_Sorting(sender As Object, e As GridViewSortEventArgs) Handles grdFields.Sorting
        ' Convert the original DataTable to an Enumerable
        Dim dataRows As IEnumerable(Of DataRow) = FieldsDT(FieldsIncludeText).AsEnumerable()

        ' Create a Func that will provide a sort key for a DataRow
        Dim sortFunc As Func(Of DataRow, Object) = Function(row) If(e.SortExpression = "field_name", row.Field(Of String)("field_name"), row.Field(Of Integer)(e.SortExpression))

        ' Sort the data with field_num=0 first, then by the SortExpression
        Dim sortedRows As IEnumerable(Of DataRow) = dataRows.OrderBy(Function(row) If(row.Field(Of Integer)("field_num") = 0, -1, 0)).ThenBy(sortFunc)

        ' Copy the sorted rows to a new DataTable
        Dim newDataTable As DataTable = sortedRows.CopyToDataTable()

        ' Bind the new DataTable to the GridView
        grdFields.DataSource = newDataTable
        grdFields.DataBind()
    End Sub

    Sub grdDisp_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdDisplay.RowDataBound,
                                                                                          grdSort.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.DataItem Is Nothing Then Return

            e.Row.Attributes("ondrop") = "drop(event)"
            e.Row.Attributes("ondragover") = "allowDrop(event)"
            e.Row.Attributes("ondragleave") = "dragLeave(event)"
            e.Row.Attributes("data-target") = "any"

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            Dim sectionCount = GetTypeLineColCount(rv("disp_type"), rv("disp_line"))

            If rv("disp_seq") = 1 Or sectionCount = 0 Then
                e.Row.Cells(0).Text = String.Format(ctx.Translate("::LT_S0260") + " {0}", rv("disp_line")) ' LT_S0260 - Line

                e.Row.Cells(0).CssClass = "grdField left draggable"
                e.Row.Cells(0).Attributes.Add("draggable", "true")
                e.Row.Cells(0).Attributes.Add("ondragstart", "dragSortStart(event)")
                e.Row.Cells(0).Attributes.Add("data-source", "column")
            Else
                e.Row.Cells(0).Text = ""
            End If
            If rv("disp_seq") = 999 Then
                e.Row.Cells(1).Text = ""
                e.Row.CssClass = "bottomSpacer"
                e.Row.FindControl("btnDel").Visible = False

                If rv("disp_type") = "01" Then
                    e.Row.FindControl("btnAmend").Visible = False
                End If
            Else
                'e.Row.CssClass = "grdField left draggable"
                If rv("field_num") <> 0 AndAlso rv("disp_type") = "01" Then
                    e.Row.FindControl("btnAmend").Visible = False
                End If

                e.Row.Cells(1).CssClass = "grdField left draggable"
                e.Row.Cells(1).Attributes.Add("draggable", "true")
                e.Row.Cells(1).Attributes.Add("ondragstart", "dragSortStart(event)")
                e.Row.Cells(1).Attributes.Add("data-source", "seq")
            End If

        End If
    End Sub
    Sub grdDisp_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDisplay.RowCommand,
                                                                                    grdSort.RowCommand

        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing
        Dim gridArgs As DataKey = Nothing
        Dim dispLine, dispSeq As Integer
        Dim dispType As String

        ' get the index of the clicked row
        RowIndex = Int32.Parse(e.CommandArgument.ToString())

        ' get the data keys for the gridrow
        gridArgs = DisplayGrid.DataKeys(RowIndex)

        dispType = gridArgs("disp_type")
        dispLine = gridArgs("disp_line")
        dispSeq = gridArgs("disp_seq")

        DR = DataSourceDisplayDT.AsEnumerable().
                            FirstOrDefault(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                                        CInt(r("disp_line")) = dispLine AndAlso
                                                        CInt(r("disp_seq")) = dispSeq)

        Select Case e.CommandName

            Case "AmendRow"
                If dispType = "02" Then Return

                Dim isText As Boolean = DR("field_num") = 0
                If dispType.ContainsOneOf("01") And Not isText Then Return

                hdnDispKey.Value = String.Format("{0}|{1}|{2}", DR("disp_type"), DR("disp_line"), DR("disp_seq"))
                txtDEtext.Text = DR("field_text")

                SetModalState("DataViewEntry")

            Case "DeleteRow"
                DR.Delete()
                DataSourceDisplayDT.AcceptChanges()
                ResequenceKeys(dispType)

                RebindDisplayGrids()
        End Select
    End Sub
    Sub RebindDisplayGrids()
        Select Case hdnAccValue.Value
            Case "Display"
                SetGrid(grdDisplay, DataViewDisplayType("01"), False)
            Case "Sort"
                SetGrid(grdSort, DataViewDisplayType("02"), False)

        End Select
        ViewDetailChanged()
    End Sub
    Sub btnAccPanel_Click(sender As Object, e As EventArgs) Handles btnAccPanel.Click
        Select Case hdnAccValue.Value
            Case "Display"
                accPanel.SelectedIndex = 0
            Case "Sort"
                accPanel.SelectedIndex = 1
        End Select

        SetGrid(grdFields, CleanFieldsDT, False)
        FieldNum = grdFields.SelectedValue

    End Sub
    Sub btnDrop_Click(sender As Object, e As EventArgs) Handles btnDrop.Click
        Dim args As String() = hdnDrop.Value.Split("|"c)
        Dim dragSource As String = args(0)
        Dim rowIndexSource As Integer = Integer.Parse(args(1))
        Dim rowIndexTarget As Integer = Integer.Parse(args(2))
        Dim sourceLine, sourceSeq, sourceField As Integer
        Dim targetLine, targetSeq As Integer
        Dim sourceType, targetType As String
        Dim sourceObj As DataKey = Nothing
        Dim targetObj As DataKey = Nothing
        Dim DR As DataRow
        Dim sourceDR As DataRow
        Dim newLine As Integer

        ' where we land will be the same for every drop
        targetObj = DisplayGrid.DataKeys(rowIndexTarget)

        targetType = targetObj("disp_type")
        targetLine = CInt(targetObj("disp_line")) * 10
        targetSeq = CInt(targetObj("disp_seq")) * 10

        If dragSource = "field" Then
            sourceField = grdFields.DataKeys(rowIndexSource)("field_num")
            sourceDR = FieldsDR(sourceField)

            ' can't drop a text field onto the AEA sort or form display
            If targetType.ContainsOneOf("02") And sourceField = 0 Then Return

            SeparateKeys(targetType)

            DR = DataSourceDisplayDT.AsEnumerable().
                                FirstOrDefault(Function(r) CStr(r("disp_type")) = targetType AndAlso
                                                           CInt(r("disp_line")) = targetLine AndAlso
                                                           CInt(r("disp_seq")) = targetSeq)

            ' actually inserting a new row
            DR = DataSourceDisplayDT.NewRow()
            SetDisplayDefaults(DR)
            DR("disp_type") = targetType
            If targetSeq = 9990 Then
                targetSeq = 9980
            Else
                targetSeq = targetSeq - 5
            End If
            DR("disp_seq") = targetSeq
            DR("disp_line") = targetLine

            DR("field_num") = sourceField

            If sourceField = 0 Then
                DR("display_name") = """ """
                DR("field_text") = " "
            Else
                DR("display_name") = sourceDR("field_name")
            End If

            DataSourceDisplayDT.Rows.Add(DR)

        Else
            ' target grid is same as source grid in this case as we're just re-ordering
            sourceObj = DisplayGrid.DataKeys(rowIndexSource)
            sourceType = sourceObj("disp_type")
            sourceLine = sourceObj("disp_line") * 10
            sourceSeq = sourceObj("disp_seq") * 10

            SeparateKeys(targetType)

            sourceDR = DataSourceDisplayDT.AsEnumerable().
                                        FirstOrDefault(Function(r) CStr(r("disp_type")) = sourceType AndAlso
                                                                    CInt(r("disp_line")) = sourceLine AndAlso
                                                                    CInt(r("disp_seq")) = sourceSeq)

            ' what happens now depends on the type of data, and also which column is being dragged as you can 
            ' re-order by the column, the row, or even the seq grid column

            If dragSource = "seq" Then
                ' moving a row within a specific grid
                sourceDR("disp_line") = targetLine

                If targetSeq = 9990 Then
                    sourceDR("disp_seq") = 9985
                Else
                    sourceDR("disp_seq") = targetSeq - 5
                End If

            Else
                ' We are moving a column, so we have to set the column numbers for all moving rows
                ' to 5 less than the target column number
                newLine = targetLine - 5
                Dim lineDRs = DataSourceDisplayDT.AsEnumerable().
                Where(Function(row) CStr(row("disp_type")) = targetType AndAlso
                CInt(row("disp_line")) = sourceLine).ToList()
                For Each colDR As DataRow In lineDRs
                    colDR("disp_line") = newLine
                Next
            End If

            DataSourceDisplayDT.AcceptChanges()
        End If

        ResequenceKeys(targetType)

        SetGrid(DisplayGrid, DataViewDisplayTypeCols(targetType), False)

        ViewDetailChanged()

    End Sub
    Sub btnAccSave_Click(sender As Object, e As EventArgs) Handles btnAccSave.Click
        SQL = "INSERT dbo.data_source_display
                      (source_id, source_date, disp_type, disp_line, disp_seq, field_num, field_text, field_format) 
               VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)"

        Using DB As New IawDB
            DB.TranBegin()
            DB.NonQuery("delete from dbo.data_source_display where source_id = @p1", DatasourceID)

            For Each rowDR As DataRow In DataSourceDisplayDT.AsEnumerable().
                                                            Where(Function(row) CInt(row("disp_seq")) <> 999 AndAlso
                                                                                CInt(row("disp_seq")) <> 0)

                DB.NonQuery(SQL, DatasourceID, DistinctDate,
                            rowDR("disp_type"),
                            rowDR("disp_line"),
                            rowDR("disp_seq"),
                            rowDR("field_num"),
                            rowDR("field_text"),
                            rowDR("field_format"))
            Next

            DB.TranCommit()
            ViewDetailUnchanged()

            RefreshGrid("SourceDisplay")
        End Using

    End Sub
    Sub btnAccCancel_Click(sender As Object, e As EventArgs) Handles btnAccCancel.Click
        ViewDetailUnchanged()
        RefreshGrid("SourceDisplay")
    End Sub
    Sub SeparateKeys(TYP As String)
        ' Function to multiply the values by 10 to create insertable gaps
        For Each row As DataRow In DataSourceDisplayDT.Rows
            If CStr(row("disp_type")) = TYP Then
                row("disp_line") = CInt(row("disp_line")) * 10
                row("disp_seq") = CInt(row("disp_seq")) * 10
            End If
        Next
        DataSourceDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceKeys(TYP As String)
        ' resequence based on the type
        Select Case TYP
            Case "01", "02"
                ResequenceLineSeq(TYP)
        End Select
        ReSortDisplayDT()

    End Sub
    Sub ResequenceLineSeq(TYP As String)
        ' Function to resequence the values back to starting from 1 for Line and Seq
        Dim groupedData = DataSourceDisplayDT.AsEnumerable().
                                                Where(Function(row) CStr(row("disp_type")) = TYP).
                                                OrderBy(Function(row) CInt(row("disp_line"))).
                                                ThenBy(Function(row) CInt(row("disp_seq"))).
                                                GroupBy(Function(row) New With {
                                                    Key .Line = CInt(row("disp_line"))
                                                }).ToList()

        Dim newLine As Integer = 1
        For Each group As IGrouping(Of Object, DataRow) In groupedData
            Dim newSeq As Integer = 1
            For Each row As DataRow In group
                row("disp_line") = newLine
                If row("disp_seq") = 999 Or row("disp_seq") = 9990 Then
                    row("disp_seq") = 999
                Else
                    row("disp_seq") = newSeq
                    newSeq += 1
                End If
            Next
            newLine += 1
        Next
        DataSourceDisplayDT.AcceptChanges()
    End Sub
#End Region
#Region "Page Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Populate()
        End If
    End Sub
    Sub Populate()
        RefreshGrid("DistinctDates")

    End Sub
    Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeDisplayEntryForm.Hide()

        Select Case ModalState

            Case "DataViewEntry"
                mpeDisplayEntryForm.Show()

        End Select
    End Sub
    Sub RefreshGrid(ByVal GridName As String)
        Select Case GridName

            Case "DistinctDates"
                SetGrid(grdDates, CleanDistinctDateDT, True)
                DistinctDate = grdDates.SelectedValue
                If grdDates.Rows.Count = 1 Then
                    grdDates.Visible = False
                End If

                RefreshGrid("SourceDisplay")

            Case "SourceDisplay"
                ViewState("DataSourceDisplayDT") = Nothing
                SetGrid(grdFields, CleanFieldsDT, False)
                SetGrid(grdDisplay, DataViewDisplayType("01"), False)
                SetGrid(grdSort, DataViewDisplayType("02"), False)
        End Select
    End Sub
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim url As String = ""

        Select Case Origin.ToLower
            Case "images"
                url = SawUtil.encryptQuery("client_images.aspx", True)
            Case "datasources"
                url = SawUtil.encryptQuery("datasources.aspx", True)
        End Select

        ctx.redirect(url)
    End Sub


#End Region
#Region "View Display Dialog"

    Private Sub btnDisplayEntryCancel_Click(sender As Object, e As EventArgs) Handles btnDisplayEntryCancel.Click
        SetModalState("")
    End Sub
    Private Sub btnDisplayEntrySave_Click(sender As Object, e As EventArgs) Handles btnDisplayEntrySave.Click
        If Not Page.IsValid Then Return

        If txtDEtext.Text.Length = 0 Then
            cvDEtext.IsValid = False
            Return
        Else
            cvDEtext.IsValid = True
        End If

        Dim DR As DataRow

        Dim dispType As String = hdnDispKey.Value.Split("|")(0)
        Dim dispLine As Integer = CInt(hdnDispKey.Value.Split("|")(1))
        Dim dispSeq As Integer = CInt(hdnDispKey.Value.Split("|")(2))

        DR = DataSourceDisplayDT.AsEnumerable().
                     FirstOrDefault(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                                CInt(r("disp_line")) = dispLine AndAlso
                                                CInt(r("disp_seq")) = dispSeq)
        DR("field_text") = txtDEtext.Text
        DR("display_name") = """" + txtDEtext.Text + """"
        DataSourceDisplayDT.AcceptChanges()

        RebindDisplayGrids()

        SetModalState("")
    End Sub
    Private Sub cvDEtext_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles cvDEtext.ServerValidate
        args.IsValid = args.Value.Length > 0
    End Sub

#End Region
#Region "Show / Hide"

    Sub ShowHideControl(ctrl As IAWTabPanel, showcontrol As Boolean)
        If showcontrol Then
            ctrl.Attributes("style") = "display: block"
        Else
            ctrl.Attributes("style") = "display: none"
        End If
    End Sub
    Sub ShowHideControl(ctrl As IAWPanel, showcontrol As Boolean)
        If showcontrol Then
            ctrl.Attributes("style") = "display: block"
        Else
            ctrl.Attributes("style") = "display: none"
        End If
    End Sub
    Sub ShowHideControl(ctrl As HtmlTableRow, showcontrol As Boolean)
        If showcontrol Then
            ctrl.Attributes("style") = "display: table-row"
        Else
            ctrl.Attributes("style") = "display: none"
        End If
    End Sub

    Sub ViewDetailChanged()
        DisableMenu()
        divAccButtons.Visible = True
    End Sub

    Sub ViewDetailUnchanged()
        EnableMenu()
        divAccButtons.Visible = False
    End Sub
    Sub DisableMenu()
        Dim menu As Menu = DirectCast(Page.Master.FindControl("Menu1"), Menu)
        If menu IsNot Nothing Then
            menu.CssClass = "MBItem noClick"
        End If
        grdDates.CssClass = "grids noClick"
    End Sub
    Sub EnableMenu()
        Dim menu As Menu = DirectCast(Page.Master.FindControl("Menu1"), Menu)
        If menu IsNot Nothing Then
            menu.CssClass = "MBItem"
        End If
        grdDates.CssClass = "grids"
    End Sub

#End Region
End Class