'------------------------------------------
' Filename: ChartFunctions.vb
'
' Description:
' Database related function calls for all
' functions related to GoJs Models
' 
'------------------------------------------
Imports System.Drawing
Imports Newtonsoft.Json

Public Class ChartFunctions
    Dim SQL As String
    Private ReadOnly Property ModelKey As Integer
        Get
            Return ctx.session("ModelKey")
        End Get
    End Property
    Private Property ClientID As Integer
        Get
            Return CInt(ctx.session("ClientID"))
        End Get
        Set(value As Integer)
            ctx.session("ClientID") = value
        End Set
    End Property

    Private _ModelHeader As DataRow
    Private _ViewID As Integer = -1
    Private ReadOnly Property IsNarrativeModel As Boolean
        Get
            If _ViewID = -1 Then
                SQL = "SELECT view_id FROM dbo.model_header where model_key = @p1"
                _ViewID = IawDB.execScalar(SQL, ModelKey)
            End If
            Return _ViewID = 0
        End Get
    End Property
    Private ReadOnly Property ModelHeader As DataRow
        Get
            If _ModelHeader Is Nothing Then
                SQL = "SELECT H.model_key,H.view_id,H.user_ref,H.published,H.attributes,
                                          V.data_effective as occurrence_date,
                                          dbo.dbf_dateonly(V.data_effective) as effective_date,
                                          V.user_ref,V.view_ref,V.det_pop_type,V.det_source_id,V.det_field_from,V.det_field_to,
                                          V.aea_text,V.allow_drilldown,
                                          DS.source_id,DS.client_id,DS.table_name,DS.primary_source,
                                          DS.photos_applicable,
                                          IsNull(F.filter_name,'') as filter_name,
                                          R.source_id as rel_source_id,
                                          R.table_name as rel_table_name,
                                          IsNull(DSF.field_column,'') as field_column,
                                          IsNull(DSF.field_type,'') as field_type,
                                          IsNull(DST.field_column,'') as rel_field_column,
                                          IsNull(DST.field_type,'') as rel_field_type,
                                          JSON_VALUE(C.attributes, '$.canOverride') as client_canOverride, 
                                          JSON_VALUE(V.attributes, '$.canOverride') as view_canOverride
                                     FROM dbo.model_header H
                                          JOIN dbo.data_view V
                                            ON V.view_id = H.view_id
                                          JOIN dbo.data_source DS
                                            ON DS.source_id = V.source_id
                                          JOIN dbo.clients C
                                            ON C.client_id = DS.client_id 
                                          LEFT OUTER JOIN dbo.data_source_filter F
                                            ON F.source_id = DS.source_id
                                           AND F.user_ref = V.user_ref
                                          LEFT OUTER JOIN dbo.data_source R
                                            ON R.source_id = V.det_source_id
                                          LEFT OUTER JOIN dbo.data_source_field DSF
                                            ON DSF.source_id = V.source_id
                                           AND DSF.source_date = V.data_effective
                                           AND DSF.field_num = V.det_field_from
                                          LEFT OUTER JOIN dbo.data_source_field DST
                                            ON DST.source_id = V.det_source_id
                                           AND DST.source_date = V.data_effective
                                           AND DST.field_num = V.det_field_to
                                    WHERE model_key = @p1"
                _ModelHeader = IawDB.execGetDataRow(SQL, ModelKey)
            End If
            Return _ModelHeader
        End Get
    End Property
    Private ReadOnly Property NarrativeModelHeader As DataRow
        Get
            If _ModelHeader Is Nothing Then
                Using DB As New IawDB
                    SQL = "SELECT H.model_key,H.user_ref,H.published,H.attributes,H.view_id,
                                          H.allow_drilldown, H.effective_date, H.reference,
                                          '0' as photos_applicable,
                                          '' as aea_text,
                                          C.client_id,
                                          JSON_VALUE(C.attributes, '$.canOverride') as client_canOverride,
                                          JSON_VALUE(C.attributes, '$.canOverride') as view_canOverride
                                     FROM dbo.model_header H
                                          JOIN dbo.suser U
                                            ON U.user_ref = H.user_ref
                                          JOIN dbo.clients C
                                            ON C.client_id = U.client_id
                                    WHERE model_key = @p1"
                    _ModelHeader = IawDB.execGetDataRow(SQL, ModelKey)
                End Using
            End If
            Return _ModelHeader
        End Get
    End Property

    Private _ViewDisplay As DataTable
    Private ReadOnly Property ViewDisplayDT As DataTable
        Get
            If _ViewDisplay Is Nothing Then
                SQL = "select D.disp_type, D.disp_line, D.disp_col, D.disp_seq,
                                          D.field_num, D.field_text, D.field_format, D.field_align,
                                          F.field_name,F.field_column,F.field_type,F.field_length
                                     from dbo.data_view V
                                          join dbo.data_view_display D
                                            on D.view_id = V.view_id
                                          left outer join dbo.data_source_field F
                                            on F.source_id = V.source_id
                                           and F.field_num = D.field_num
                                           and F.source_date = V.data_effective
                                    where V.view_id = @p1
                                    order by disp_type, disp_line, disp_col, disp_seq "
                _ViewDisplay = IawDB.execGetTable(SQL, ModelHeader("view_id"))
            End If
            Return _ViewDisplay
        End Get
    End Property
    Public Enum DispType
        OCAData = 1
        AEASort = 2
        AEAHeaders = 3
        AEAData = 4
        DetailForm = 5
        ListHeaders = 6
        ListData = 7
        CoParent = 8
    End Enum
    Private ReadOnly Property DisplayData(DisT As DispType) As DataTable
        Get
            Dim DType As String = "00"
            Select Case DisT
                Case DispType.OCAData
                    DType = "01"
                Case DispType.AEASort
                    DType = "02"
                Case DispType.AEAHeaders
                    DType = "03"
                Case DispType.AEAData
                    DType = "04"
                Case DispType.DetailForm
                    DType = "05"
                Case DispType.ListHeaders
                    DType = "06"
                Case DispType.ListData
                    DType = "07"
                Case DispType.CoParent
                    DType = "08"
            End Select
            Dim DBV As New DataView(ViewDisplayDT)
            DBV.RowFilter = "disp_type = '" + DType + "'"
            Return DBV.ToTable
        End Get
    End Property

    Private _RelatedDataSourceFieldsDT As DataTable = Nothing
    Private _LastRelatedDataSourceID As Integer = 0
    Private ReadOnly Property RelatedDataSourceFieldsDT(RelatedDataSourceID As Integer) As DataTable
        Get
            If _LastRelatedDataSourceID <> RelatedDataSourceID Then _RelatedDataSourceFieldsDT = Nothing
            If _RelatedDataSourceFieldsDT Is Nothing Then
                SQL = "select field_num,field_name,field_column,field_type,field_length
                                     from dbo.data_source_field
                                    where source_id = @p1"
                _RelatedDataSourceFieldsDT = IawDB.execGetTable(SQL, RelatedDataSourceID)
                _LastRelatedDataSourceID = RelatedDataSourceID
            End If
            Return _RelatedDataSourceFieldsDT
        End Get
    End Property
    Private ReadOnly Property RelatedDataSourceFieldsDR(RelatedDataSourceID As Integer, RelatedFieldNum As Integer) As DataRow
        Get
            Return RelatedDataSourceFieldsDT(RelatedDataSourceID).Select("field_num=" + RelatedFieldNum.ToString)(0)
        End Get
    End Property

    Private _DataFilter As DSFilterReturn = Nothing
    Private ReadOnly Property DataFilter(CurrentParmCount As Integer) As DSFilterReturn
        Get
            If _DataFilter Is Nothing Then
                Dim SourceID As Integer = ModelHeader("source_id")
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

    Private _Fonts As DataTable
    Private ReadOnly Property FontsDT() As DataTable
        Get
            If _Fonts Is Nothing Then
                SQL = "select F.font_name,F.font_string
                                     From dbo.qlang L
                                          Join dbo.font F
                                            on F.font_family = L.font_family
                                    where L.language_ref = @p1
                                   union
                                   select C.font_name,C.font_string
                                     From dbo.qlang L
                                          Join dbo.client_font C
                                            on C.font_family = L.font_family
                                    where L.language_ref = @p1
                                      and C.client_id = @p2"
                _Fonts = IawDB.execGetTable(SQL, ctx.languageCode, ClientID)
            End If
            Return _Fonts
        End Get
    End Property
    Public Function GetModelParms() As Object
        ' initialising, so clear data
        _ModelHeader = Nothing
        _ViewDisplay = Nothing

        Dim json As New Dictionary(Of String, Object)

        Dim HeadDR As DataRow

        If IsNarrativeModel Then
            HeadDR = NarrativeModelHeader
        Else
            HeadDR = ModelHeader
        End If
        Dim FormattedName As String

        ' store the clientID on the session so that the get photo will know which client to get the photo for
        ClientID = HeadDR("client_id")

        Dim CA As ChartAttrib
        Dim att As String = HeadDR("attributes")

        If String.IsNullOrEmpty(att) Then
            CA = New ChartAttrib            ' defaults
        Else
            CA = New ChartAttrib(att)
        End If

        Dim BackRepeat As String = "03"
        Dim BackContent As String = CA.Data.backgroundContent
        Dim gradient As String = ""
        If CA.Data.backgroundType.ToLower.ContainsOneOf("image", "gradient") Then
            SQL = "select image_repeat, content, structure from dbo.background where unique_id = @p1"
            Dim DR As DataRow
            DR = IawDB.execGetDataRow(SQL, CA.Data.backgroundID)

            BackRepeat = DR.GetValue("image_repeat", "03")
            BackContent = DR.GetValue("content", "")
            gradient = DR.GetValue("structure", "")
        End If

        Dim PhotosApplicable As Boolean = HeadDR("photos_applicable")

        With CA.Data
            json.Add("line_attr", .lines)
            json.Add("backgroundType", .backgroundType)
            json.Add("background", BackContent)
            json.Add("backgroundContent", .backgroundContent)
            json.Add("gradient", gradient)
            json.Add("backgroundID", .backgroundID)
            json.Add("backgroundRepeat", BackRepeat)

            json.Add("chartDirection", .chartDirection)
            json.Add("image_position", .imagePosition.ToLower)
            json.Add("image_shape", .imageShape)

            With .node
                json.Add("node_fg", .foreground)
                json.Add("node_bg", .background)
                json.Add("node_border_fg", .border)
                json.Add("node_text_bg", .textBackground)
                json.Add("node_text_bg_block", .textBackgroundBlock)
                json.Add("node_icon_fg", .iconColour)
                json.Add("node_icon_hover", .iconHover)
            End With

            With .highlight
                json.Add("node_highlight_fg", .foreground)
                json.Add("node_highlight_bg", .background)
                json.Add("node_highlight_border", .border)
            End With

            With .tooltip
                json.Add("node_tt_fg", .foreground)
                json.Add("node_tt_bg", .background)
                json.Add("node_tt_border", .border)
            End With

            json.Add("showShadow", .showShadow)
            json.Add("shadowColour", .shadowColour)

            With .link
                json.Add("linkType", .type)
                json.Add("linkColour", .colour)
                json.Add("linkHover", .hover)
                json.Add("linkWidth", .width)
                json.Add("linkTooltipForeground", .tooltip.foreground)
                json.Add("linkTooltipBackground", .tooltip.background)
                json.Add("linkTooltipBorder", .tooltip.border)
            End With

            json.Add("max_node_height", .maxHeight)
            json.Add("max_node_width", .maxWidth)
            json.Add("node_width", .nodeWidth)
            json.Add("node_height", .nodeHeight)

            json.Add("photos_applicable", PhotosApplicable)
            json.Add("show_photos", If(PhotosApplicable = False, False, .showImages))
            json.Add("image_height", .imageHeight)
            json.Add("node_corners", .corners)

            With .Buttons
                json.Add("button_position", .position)
                json.Add("button_font", .font)
                json.Add("button_shape", .shape)
                json.Add("button_text_colour", .normal.foreground)
                json.Add("button_back_colour", .normal.background)
                json.Add("button_border_colour", .normal.border)
                json.Add("button_text_hover", .hover.foreground)
                json.Add("button_back_hover", .hover.background)
                json.Add("button_border_hover", .hover.border)
                json.Add("button_detail_text", .detailText)
                json.Add("button_note_text", .noteText)
            End With

        End With

        json.Add("model_editable", If(ctx.session("user_ref") = HeadDR("user_ref"), True, False))
        json.Add("allow_drilldown", HeadDR.GetValue("allow_drilldown", "1") = "1")
        json.Add("narrative_model", IsNarrativeModel)

        json.Add("can_override", HeadDR.GetValue("client_canoverride", "true") = "true" And
                                 HeadDR.GetValue("view_canoverride", "true") = "true")

        ' Set the text used on the chart
        json.Add("aea_text", HeadDR.GetValue("aea_text", ctx.Translate("::LT_S0259")))   ' Available

        If IsNarrativeModel Then
            FormattedName = HeadDR.GetValue("reference", "")
        Else
            FormattedName = String.Format("{0} {1}",
                            HeadDR.GetValue("view_ref", ""),
                            HeadDR.GetValue("filter_name", ""))
        End If

        json.Add("oca_text", FormattedName)
        json.Add("chart_date", HeadDR.GetDate("effective_date").ToShortDateString)

        json.Add("hash_char", ctx.Translate("::LT_S0192"))                ' #
        json.Add("vacant_text", ctx.Translate("::LT_A0002"))              ' Vacant
        json.Add("parent_group_text", ctx.Translate("::LT_A0003"))        ' Parent Group
        json.Add("dialog_team_details", ctx.Translate("::LT_A0006"))      ' Text Node
        json.Add("dialog_label_details", ctx.Translate("::LT_A0007"))     ' Note Details
        json.Add("dialog_node_details", ctx.Translate("::LT_S0227"))      ' Node Details
        json.Add("dialog_vacant_details", ctx.Translate("::LT_A0009"))    ' Vacant Node

        Dim C As New List(Of String)
        Dim A As New List(Of String)
        For Each DR As DataRow In DisplayData(DispType.AEAHeaders).Rows
            If DR("disp_line") <> 0 Then Continue For
            If DR("field_num") = 0 Then
                C.Add(DR("field_text"))
            Else
                C.Add(DR("field_name"))
            End If
            Select Case DR("field_align").tolower
                Case "01"
                    A.Add("left")
                Case "02"
                    A.Add("center")
                Case "03"
                    A.Add("right")
            End Select
        Next
        json.Add("AEACols", String.Join("|", C.ToArray))
        json.Add("AEAColsAlign", String.Join("|", A.ToArray))

        ' See if there are any sort columns defined and if so, send array with sort indicator
        '  we need to return a delimited string for each col number saying whether there's a 
        '  sort definition for that column or not
        Dim NumberOfCols As Integer = C.Count

        Dim S As New List(Of Integer)
        Dim ThisLine As Integer = -1
        Dim lastLine As Integer = -1
        For Each DR As DataRow In DisplayData(DispType.AEASort).Rows
            If lastLine <> -1 Then
                If lastLine <> DR("disp_col") Then
                    S.Add(ThisLine)
                    lastLine = ThisLine
                End If
            End If
            ThisLine = DR("disp_col")
            lastLine = DR("disp_col")
        Next
        If ThisLine <> -1 Then
            S.Add(ThisLine)
            C = New List(Of String)
            For i As Integer = 1 To NumberOfCols
                If S.Contains(i) Then
                    C.Add("true")
                Else
                    C.Add("false")
                End If
            Next
        End If
        json.Add("AEAColsSort", String.Join("|", C.ToArray))

        ' How many rows are we displaying in the Detail nodes
        Dim MaxDispLine As Integer = 0
        For Each D As DataRow In DisplayData(DispType.OCAData).Rows
            If MaxDispLine < D("disp_line") Then
                MaxDispLine = D("disp_line")
            End If
        Next
        json.Add("display_lines", MaxDispLine)

        ' Add the fonts for the current language's font_family
        Dim fonts As New List(Of Dictionary(Of String, Object))
        For Each row As DataRow In FontsDT.Rows
            Dim font As New Dictionary(Of String, Object)
            font("font_name") = row("font_name")
            font("font_string") = row("font_string")
            fonts.Add(font)
        Next
        json.Add("fonts", fonts)

        Return json
    End Function
    Public Function SaveModelParms(jsonData As String) As Object

        Dim CA As ChartAttrib
        Dim att As String

        If IsNarrativeModel Then
            att = NarrativeModelHeader("attributes")
        Else
            att = ModelHeader("attributes")
        End If

        If String.IsNullOrEmpty(att) Then
            CA = New ChartAttrib            ' defaults
        Else
            CA = New ChartAttrib(att)
        End If

        Dim MD As ModelDetails = JsonConvert.DeserializeObject(Of ModelDetails)(jsonData)

        With CA.Data
            .backgroundContent = MD.backgroundContent
            .backgroundID = MD.backgroundID
            .backgroundType = MD.backgroundType
            .chartDirection = MD.chartDirection
            .corners = MD.node_corners
            .highlight.background = MD.node_highlight_bg
            .highlight.border = MD.node_highlight_border
            .highlight.foreground = MD.node_highlight_fg
            .lines = MD.line_attr
            .node.background = MD.node_bg
            .node.border = MD.node_border_fg
            .node.foreground = MD.node_fg
            .node.textBackground = MD.node_text_bg
            .node.textBackgroundBlock = MD.node_text_bg_block
            .node.iconColour = MD.node_icon_fg
            .node.iconHover = MD.node_icon_hover
            .nodeHeight = MD.node_height
            .nodeWidth = MD.node_width
            .showImages = MD.show_photos
            .imagePosition = MD.image_position
            .imageShape = MD.image_shape
            .imageHeight = MD.image_height
            .link.colour = MD.linkColour
            .link.hover = MD.linkHover
            .link.type = MD.linkType
            .link.width = MD.linkWidth
            .link.tooltip.foreground = MD.linkTooltipForeground
            .link.tooltip.background = MD.linkTooltipBackground
            .link.tooltip.border = MD.linkTooltipBorder
            .tooltip.foreground = MD.node_tt_fg
            .tooltip.background = MD.node_tt_bg
            .tooltip.border = MD.node_tt_border
            .showShadow = MD.showShadow
            .shadowColour = MD.shadowColour
            .Buttons.position = MD.button_position
            .Buttons.font = MD.button_font
            .Buttons.shape = MD.button_shape
            .Buttons.normal.foreground = MD.button_text_colour
            .Buttons.normal.background = MD.button_back_colour
            .Buttons.normal.border = MD.button_border_colour
            .Buttons.hover.foreground = MD.button_text_hover
            .Buttons.hover.background = MD.button_back_hover
            .Buttons.hover.border = MD.button_border_hover
            .Buttons.detailText = MD.button_detail_text
            .Buttons.noteText = MD.button_note_text
        End With

        IawDB.execNonQuery("update dbo.model_header 
                                       set attributes = @p2
                                     where model_key = @p1 ",
                                        ModelKey, CA.GetData)

        Return "Ok"
    End Function
    Public Function GetUnallocatedItems(Optional ByRef ItemRef As String = "%") As Object
        Dim DT As DataTable
        Dim DR As DataRow
        Dim Model As DataRow
        Dim ResDT As New DataTable
        Dim C As List(Of String)
        Dim S As List(Of String)
        Dim LastCol As Integer
        Dim FieldText As String

        Model = ModelHeader

        ResDT.Columns.AddRange(New DataColumn() {New DataColumn("item_ref", GetType(String)),
                                                             New DataColumn("name", GetType(String)),
                                                             New DataColumn("sort_name", GetType(String))})

        If IsNarrativeModel Then
            Return GenericFunctions.ConvertToJSON(ResDT)
        End If

        Dim OccurrenceDate As Date = Model("occurrence_date")

        If String.IsNullOrEmpty(SQL) Then SQL = ""

        ' construct the SQL to retrieve the datasource (basis) records
        C = New List(Of String)
        For Each DR In DisplayData(DispType.AEAData).Rows
            If DR("field_num") = 0 Then Continue For
            C.Add(DR("field_column"))
        Next
        ' and add in any sorting columns you may need
        For Each DR In DisplayData(DispType.AEASort).Rows
            If DR("field_num") = 0 Then Continue For
            If Not C.Contains(DR("field_column")) Then
                C.Add(DR("field_column"))
            End If
        Next

        If ItemRef = "%" Then
            ' retrieve any rowfilter for this user for this datasource
            Dim colFilter As String = ""
            Dim parmObj As New List(Of Object)
            parmObj.Add(ModelKey)

            Dim DF As DSFilterReturn = DataFilter(1)
            If DF IsNot Nothing Then
                colFilter = " AND " + DF.SQLString
                For Each obj As Object In DF.SQLParamArray
                    parmObj.Add(obj)
                Next
            End If

            ' return all records that aren't already on the model
            SQL = "select T.__item_ref, " + String.Join(",", C.ToArray) +
                              "  From dbo." + Model("table_name") + " T
                                where __start_date = '" + OccurrenceDate.ToString("yyyy-MM-dd HH:mm:ss") + "'
                                  and __item_ref not in (select item_ref
                                                           from dbo.model_detail
                                                          where JSON_VALUE(attributes, '$.node_type') = 'Detail'
                                                            and model_key = @p1)" + colFilter
            DT = IawDB.execGetTable(SQL, parmObj.ToArray)
        Else
            ' return the individual record specified in itemref parameter
            SQL = "select T.__item_ref, " + String.Join(",", C.ToArray) +
                              "  From dbo." + Model("table_name") + " T
                                where __start_date = @p2
                                  and __item_ref = @p1"
            DT = IawDB.execGetTable(SQL, ItemRef, OccurrenceDate)
        End If

        For Each DR In DT.Rows
            ' Construct the AEA display columns
            FieldText = ""
            LastCol = -1
            C = New List(Of String)

            For Each D As DataRow In DisplayData(DispType.AEAData).Rows
                If LastCol <> -1 Then
                    If LastCol <> D("disp_col") Then
                        C.Add(FieldText)
                        FieldText = ""
                        LastCol = D("disp_col")
                    End If
                End If
                If D("field_num") = 0 Then
                    FieldText += D("field_text")
                Else
                    FieldText += DR.GetValue(D("field_column"), "")
                End If

                LastCol = D("disp_col")
            Next
            C.Add(FieldText)

            ' Construct the AEA sort columns
            ' added complication that there may not be a sort entry for a column, but we must add the delimiter
            ' so that the sort values actually match up to later columns

            Try

                S = New List(Of String)
                For i As Integer = 1 To C.Count
                    FieldText = ""
                    For Each D As DataRow In DisplayData(DispType.AEASort).Rows
                        If D("disp_col") <> i Then Continue For
                        If FieldText <> "" Then FieldText += "-"
                        FieldText += DR.GetValue(D("field_column"), "")
                    Next
                    S.Add(FieldText)
                Next
            Catch ex As Exception
                Throw
            End Try

            ResDT.Rows.Add(DR("__item_ref"),
                                          String.Join("|", C.ToArray),
                                          String.Join("|", S.ToArray))
        Next

        Return GenericFunctions.ConvertToJSON(ResDT)

    End Function
    Public Function GetNodePicture(ItemRef As String, nodeHeight As Integer) As Object
        Dim json As New Dictionary(Of String, Object)
        Dim b64img As String = ""
        Dim ImageWidth As Integer = 0
        Dim byteImage As Byte()
        Dim ImagePhoto As Image

        SQL = "select top 1 item_binary
                 from dbo.client_document
                where client_id = @p1
                  and item_ref = @p2
                order by item_date DESC"
        Dim DT As DataTable = IawDB.execGetTable(SQL, ClientID, ItemRef)

        If DT.Rows.Count > 0 Then
            If DT.Rows(0)("item_binary") IsNot System.DBNull.Value Then
                byteImage = DT.Rows(0)("item_binary")
                Using ms As New IO.MemoryStream(byteImage)
                    ImagePhoto = GenericFunctions.ResizeImage(Image.FromStream(ms), New Size(0, nodeHeight - 2), GenericFunctions.SpecifyAxis.YAxis)
                    ImageWidth = ImagePhoto.Width
                    b64img = "data:image/png;base64," + GenericFunctions.ImageToBase64(ImagePhoto)
                End Using
            End If
        End If

        json.Add("node_picture", b64img)
        json.Add("picture_width", ImageWidth)

        Return json
    End Function
    Public Function GetModelData() As Object
        Dim DT As DataTable
        Dim ResDT As DataTable
        Dim byteImage As Byte()
        Dim ImagePhoto As Image
        Dim b64img As String
        Dim ImageWidth As Integer
        Dim NodeHeight As Integer
        Dim Line1, Line2, Line3, Line4, Line5, Line6 As String
        Dim DsLine1, DsLine2, DsLine3, DsLine4, DsLine5, DsLine6 As String
        Dim Model As DataRow
        Dim EffDate As DateTime
        Dim ReadonlyModel As Boolean
        Dim C As List(Of String)
        Dim LastLine As Integer
        Dim FieldText As String
        Dim DefinitionType As Integer
        Dim ND As DiagramNode

        If IsNarrativeModel Then
            Return GetNarrativeModelData()
        End If

        ' create a datatable and add the result columns
        ResDT = CreateDataTable()

        Model = ModelHeader
        EffDate = Model.GetDate("effective_date").Date
        ReadonlyModel = Model.GetValue("user_ref", "") <> ctx.user_ref

        ' Construct the Node display fields

        C = New List(Of String)
        Dim NewItem As String
        ' get list of all the columns needed for the node display
        For Each D As DataRow In DisplayData(DispType.OCAData).Rows
            If D("field_num") <> 0 Then
                FieldText = D("field_column")
                If FieldText <> "" Then
                    NewItem = ""
                    Select Case D("field_type")
                        Case "01"   ' String
                            NewItem = "IsNull(T." + FieldText + ",'') as " + FieldText
                        Case "02"   ' Date
                            C.Add("IsNull(dbo.dbf_format_date('o mmm, yyyy',T." + FieldText + "),'') as " + FieldText)
                        Case "03", "04", "05"
                            NewItem = "IsNull(Convert(nvarchar(max),T." + FieldText + "),'') as " + FieldText
                    End Select
                    If NewItem <> "" AndAlso Not C.Contains(NewItem) Then
                        C.Add(NewItem)
                    End If
                End If
            End If
        Next

        Dim DisT As DispType = DispType.CoParent
        If DisplayData(DispType.CoParent).Rows.Count = 0 Then
            DisT = DispType.OCAData
        End If

        For Each D As DataRow In DisplayData(DisT).Rows
            If D("field_num") <> 0 Then
                FieldText = D("field_column")
                If FieldText <> "" Then
                    NewItem = ""
                    Select Case D("field_type")
                        Case "01"   ' String
                            NewItem = "IsNull(T." + FieldText + ",'') as " + FieldText
                        Case "02"   ' Date
                            C.Add("IsNull(dbo.dbf_format_date('o mmm, yyyy',T." + FieldText + "),'') as " + FieldText)
                        Case "03", "04", "05"
                            NewItem = "IsNull(Convert(nvarchar(max),T." + FieldText + "),'') as " + FieldText
                    End Select
                    If NewItem <> "" AndAlso Not C.Contains(NewItem) Then
                        C.Add(NewItem)
                    End If
                End If
            End If
        Next

        SQL = "select M.detail_key,
                      M.item_ref,
                      M.attributes,
                      S.det_pop_type,
                      (select count(1) from dbo.model_detail 
                        where model_key = M.model_key
                          and parent_key = M.detail_key
                          and JSON_VALUE(attributes,'$.co_child') = 'true') as group_count,
                      PP.item_binary as node_picture," +
                      String.Join(",", C.ToArray) +
                      GetRelatedLookupSQL("Count") + "
                 from dbo.model_detail M 
                      join dbo.model_header H
                        on H.model_key = M.model_key
                      join dbo.data_view S
                        on S.view_id = H.view_id
                      left outer join dbo." + Model("table_name") + " T
                        on Convert(nvarchar(1000),T.__item_ref) = convert(nvarchar(1000),M.item_ref)
                       and T.__start_date = S.data_effective
                      left outer join dbo.client_document PP
                        on PP.item_ref = M.item_ref
                       and PP.client_id = @p2
                       and PP.unique_id = (select top 1 unique_id
                                             from dbo.client_document
                                            where client_id = PP.client_id
                                              and item_ref = PP.item_ref
                                            order by unique_id desc)
                where M.model_key = @p1
                order by M.parent_key, JSON_VALUE(M.attributes,'$.sequence')"
        DT = IawDB.execGetTable(SQL, ModelKey, ClientID)
        For Each DR As DataRow In DT.Rows

            ND = JsonConvert.DeserializeObject(Of DiagramNode)(DR("attributes"))

            b64img = ""
            ImageWidth = 0
            Line1 = ""
            Line2 = ""
            Line3 = ""
            Line4 = ""
            Line5 = ""
            Line6 = ""
            DsLine1 = ""
            DsLine2 = ""
            DsLine3 = ""
            DsLine4 = ""
            DsLine5 = ""
            DsLine6 = ""

            Dim ShowDetail As Boolean = False

            Select Case ND.node_type
                Case "Detail"

                    If ND.co_child Then
                        DefinitionType = DispType.CoParent
                        If DisplayData(DefinitionType).Rows.Count = 0 Then
                            DefinitionType = DispType.OCAData
                        End If
                    Else
                        DefinitionType = DispType.OCAData
                    End If

                    If DR("node_picture") IsNot System.DBNull.Value Then
                        NodeHeight = ND.node_height
                        byteImage = DR("node_picture")
                        Using ms As New IO.MemoryStream(byteImage)
                            ImagePhoto = GenericFunctions.ResizeImage(Image.FromStream(ms), New Size(0, NodeHeight - 2), GenericFunctions.SpecifyAxis.YAxis)
                            ImageWidth = ImagePhoto.Width
                            b64img = "data:image/png;base64," + GenericFunctions.ImageToBase64(ImagePhoto)
                        End Using
                    End If

                    ' get the display data for the node
                    FieldText = ""
                    LastLine = -1
                    C = New List(Of String)
                    For Each D As DataRow In DisplayData(DefinitionType).Rows
                        If LastLine <> -1 Then
                            If LastLine <> D("disp_line") Then
                                C.Add(FieldText)
                                FieldText = ""
                                LastLine = D("disp_line")
                            End If
                        End If
                        If D("field_num") = 0 Then
                            FieldText += D("field_text")
                        Else
                            Dim tmpfield As String = ""
                            DataFormatter(DR.GetValue(D("field_column"), ""), D("field_format"), tmpfield)
                            FieldText += tmpfield
                            'FieldText += DR.GetValue(D("field_column"), "")
                        End If
                        LastLine = D("disp_line")
                    Next
                    C.Add(FieldText)
                    For i As Integer = 0 To C.Count - 1
                        If i = 0 Then Line1 = C(i)
                        If i = 1 Then Line2 = C(i)
                        If i = 2 Then Line3 = C(i)
                        If i = 3 Then Line4 = C(i)
                        If i = 4 Then Line5 = C(i)
                        If i = 5 Then Line6 = C(i)
                    Next

                    ' ds (datasource) lines will be what comes from the DB in case we need to undo any user change
                    DsLine1 = Line1
                    DsLine2 = Line2
                    DsLine3 = Line3
                    DsLine4 = Line4
                    DsLine5 = Line5
                    DsLine6 = Line6

                    If ND.line1 <> "" Then Line1 = ND.line1
                    If ND.line2 <> "" Then Line2 = ND.line2
                    If ND.line3 <> "" Then Line3 = ND.line3
                    If ND.line4 <> "" Then Line4 = ND.line4
                    If ND.line5 <> "" Then Line5 = ND.line5
                    If ND.line6 <> "" Then Line6 = ND.line6

                    Select Case DR.GetValue("det_pop_type", "03")
                        Case "01"       ' Detail
                            ShowDetail = True
                        Case "02"       ' List - have to see if there are any
                            ShowDetail = DR.GetValue("list_count", 0) > 0
                        Case "03"       ' None
                            ShowDetail = False
                    End Select

                Case "Team"
                    Line1 = ND.line1
                    Line2 = ND.line2
                    Line3 = ND.line3
                    Line4 = ND.line4
                    Line5 = ND.line5
                    Line6 = ND.line6

                Case "Label"

                    ' if readonly model and private label then ignore label
                    If ND.private_label And ReadonlyModel Then
                        Continue For
                    End If

                Case "Vacant"
                    Line1 = ND.line1
                    Line2 = ND.line2
                    Line3 = ND.line3
                    Line4 = ND.line4
                    Line5 = ND.line5
                    Line6 = ND.line6

                Case "ParentGroup"
                    Line1 = ND.line1

            End Select

            Dim lineAttr As String = JsonConvert.SerializeObject(ND.line_attr)
            If String.IsNullOrEmpty(lineAttr) Then
                lineAttr = ModelHeader.GetValue("line_attr")
            End If

            ResDT.Rows.Add(DR("detail_key"),                          '  1
                           ND.parent_key,                             '  2
                           ND.node_type,                              '  3
                           ND.node_width,                             '  4
                           ND.node_height,                            '  5
                           ND.node_corners,                           '  6
                           ND.node_fg,                                '  7
                           ND.node_bg,                                '  8
                           ND.node_border_fg,                         '  9
                           ND.node_text_bg,                           '  9a
                           ND.node_text_bg_block,                     '  9b
                           ND.node_icon_fg,                           '  9c
                           ND.node_icon_hover,                        '  9d
                           ND.node_tt_fg,                             ' 10
                           ND.node_tt_bg,                             ' 11
                           ND.node_tt_border,                         ' 12
                           lineAttr,                                  ' 13
                           ND.show_photo,                             ' 14
                           ND.image_height,                           ' 14a
                           ND.assistant,                              ' 15
                           ND.tree_expanded,                          ' 16
                           ND.group_expanded,                         ' 16a
                           ShowDetail,                                ' 17 
                           ND.sequence,                               ' 18
                           ND.tooltip,                                ' 19
                           ND.item_ref,                               ' 20
                           Line1,                                     ' 21
                           Line2,                                     ' 22
                           Line3,                                     ' 23
                           Line4,                                     ' 24
                           Line5,                                     ' 25
                           Line6,                                     ' 26
                           b64img,                                    ' 27
                           ND.pos_x,                                  ' 28
                           ND.pos_y,                                  ' 29
                           ND.label_text,                             ' 30
                           ND.label_icon,                             ' 31
                           ND.label_shape,                            ' 31a
                           ND.private_label,                          ' 32
                           ND.note_width,                             ' 32a
                           ImageWidth,                                ' 33
                           ND.co_child,                               ' 34
                           DsLine1,                                   ' 35
                           DsLine2,                                   ' 36
                           DsLine3,                                   ' 37
                           DsLine4,                                   ' 38
                           DsLine5,                                   ' 39
                           DsLine6,                                   ' 40
                           ND.nodes_across,                           ' 41
                           DR("group_count"),                         ' 41a
                           ND.showShadow,                             ' 42
                           ND.shadowColour,                           ' 43
                           ND.linkColour,                             ' 44
                           ND.linkHover,                              ' 45
                           ND.linkWidth,                              ' 46
                           ND.linkType,                               ' 47
                           ND.linkTooltipForeground,                  ' 48
                           ND.linkTooltipBackground,                  ' 49
                           ND.linkTooltipBorder,                      ' 50
                           ND.linkTooltip                             ' 51
                           )
        Next

        ''---------------------------------------------------------------------------------

        Return GenericFunctions.ConvertToJSON(ResDT)

    End Function
    Private Function GetNarrativeModelData() As Object
        Dim ResDT As DataTable = CreateDataTable()
        Dim Model As DataRow = NarrativeModelHeader
        Dim ReadonlyModel As Boolean = Model.GetValue("user_ref", "") <> ctx.user_ref
        Dim ND As DiagramNode
        Dim DT As DataTable

        SQL = "select M.detail_key,
                      M.item_ref,
                      M.attributes,
                      (select count(1) 
                         from dbo.model_detail 
                        where model_key = M.model_key
                          and parent_key = M.detail_key
                          and JSON_VALUE(attributes,'$.co_child') = 'true') as group_count
                 from dbo.model_detail M
                where M.model_key = @p1
                order by M.parent_key, JSON_VALUE(M.attributes,'$.sequence')"
        DT = IawDB.execGetTable(SQL, ModelKey, ClientID)
        For Each DR As DataRow In DT.Rows

            ND = JsonConvert.DeserializeObject(Of DiagramNode)(DR("attributes"))

            If ND.node_type = "Label" And
                        ND.private_label And
                        ReadonlyModel Then Continue For

            Dim lineAttr As String = JsonConvert.SerializeObject(ND.line_attr)
            If String.IsNullOrEmpty(lineAttr) Then
                lineAttr = ModelHeader.GetValue("line_attr")
            End If

            ResDT.Rows.Add(DR("detail_key"),                          '  1
                           ND.parent_key,                             '  2
                           ND.node_type,                              '  3
                           ND.node_width,                             '  4
                           ND.node_height,                            '  5
                           ND.node_corners,                           '  6
                           ND.node_fg,                                '  7
                           ND.node_bg,                                '  8
                           ND.node_border_fg,                         '  9
                           ND.node_text_bg,                           '  9a
                           ND.node_text_bg_block,                     '  9b
                           ND.node_icon_fg,                           '  9c
                           ND.node_icon_hover,                        '  9d
                           ND.node_tt_fg,                             ' 10
                           ND.node_tt_bg,                             ' 11
                           ND.node_tt_border,                         ' 12
                           lineAttr,                                  ' 13
                           ND.show_photo,                             ' 14
                           ND.image_height,                           ' 14a
                           ND.assistant,                              ' 15
                           ND.tree_expanded,                          ' 16
                           ND.group_expanded,                         ' 16a
                           False,                                     ' 17 
                           ND.sequence,                               ' 18
                           ND.tooltip,                                ' 19
                           ND.item_ref,                               ' 20
                           ND.line1,                                  ' 21
                           ND.line2,                                  ' 22
                           ND.line3,                                  ' 23
                           ND.line4,                                  ' 24
                           ND.line5,                                  ' 25
                           ND.line6,                                  ' 26
                           "",                                        ' 27
                           ND.pos_x,                                  ' 28
                           ND.pos_y,                                  ' 29
                           ND.label_text,                             ' 30
                           ND.label_icon,                             ' 31
                           ND.label_shape,                            ' 31a
                           ND.private_label,                          ' 32
                           ND.note_width,                             ' 32a
                           0,                                         ' 33
                           ND.co_child,                               ' 34
                           ND.line1,                                  ' 35
                           ND.line2,                                  ' 36
                           ND.line3,                                  ' 37
                           ND.line4,                                  ' 38
                           ND.line5,                                  ' 39
                           ND.line6,                                  ' 40
                           ND.nodes_across,                           ' 41
                           DR("group_count"),                         ' 41a
                           ND.showShadow,                             ' 42
                           ND.shadowColour,                           ' 43
                           ND.linkColour,                             ' 44
                           ND.linkHover,                              ' 45
                           ND.linkWidth,                              ' 46
                           ND.linkType,                               ' 47
                           ND.linkTooltipForeground,                  ' 48
                           ND.linkTooltipBackground,                  ' 49
                           ND.linkTooltipBorder,                      ' 50
                           ND.linkTooltip                             ' 51
                          )
        Next

        ''---------------------------------------------------------------------------------

        Return GenericFunctions.ConvertToJSON(ResDT)

    End Function
    Private Function CreateDataTable()
        Dim resDT As New DataTable

        resDT.Columns.AddRange(New DataColumn() {New DataColumn("detail_key", GetType(Integer)),         '  1
                                                 New DataColumn("parent_key", GetType(Integer)),         '  2
                                                 New DataColumn("node_type", GetType(String)),           '  3
                                                 New DataColumn("node_width", GetType(Integer)),         '  4
                                                 New DataColumn("node_height", GetType(Integer)),        '  5
                                                 New DataColumn("node_corners", GetType(String)),        '  6
                                                 New DataColumn("node_fg", GetType(String)),             '  7
                                                 New DataColumn("node_bg", GetType(String)),             '  8
                                                 New DataColumn("node_border_fg", GetType(String)),      '  9
                                                 New DataColumn("node_text_bg", GetType(String)),        '  9a
                                                 New DataColumn("node_text_bg_block", GetType(Boolean)), '  9b
                                                 New DataColumn("node_icon_fg", GetType(String)),        '  9c
                                                 New DataColumn("node_icon_hover", GetType(String)),     '  9d
                                                 New DataColumn("node_tt_fg", GetType(String)),          ' 10
                                                 New DataColumn("node_tt_bg", GetType(String)),          ' 11
                                                 New DataColumn("node_tt_border", GetType(String)),      ' 12
                                                 New DataColumn("line_attr", GetType(String)),           ' 13
                                                 New DataColumn("show_photo", GetType(Boolean)),         ' 14
                                                 New DataColumn("image_height", GetType(Integer)),       ' 14a
                                                 New DataColumn("assistant", GetType(Boolean)),          ' 15
                                                 New DataColumn("tree_expanded", GetType(Boolean)),      ' 16
                                                 New DataColumn("group_expanded", GetType(Boolean)),     ' 16a
                                                 New DataColumn("show_detail", GetType(Boolean)),        ' 17
                                                 New DataColumn("sequence", GetType(Integer)),           ' 18
                                                 New DataColumn("tooltip", GetType(String)),             ' 19
                                                 New DataColumn("item_ref", GetType(String)),            ' 20
                                                 New DataColumn("line1", GetType(String)),               ' 21
                                                 New DataColumn("line2", GetType(String)),               ' 22
                                                 New DataColumn("line3", GetType(String)),               ' 23
                                                 New DataColumn("line4", GetType(String)),               ' 24
                                                 New DataColumn("line5", GetType(String)),               ' 25
                                                 New DataColumn("line6", GetType(String)),               ' 26
                                                 New DataColumn("node_picture", GetType(String)),        ' 27
                                                 New DataColumn("pos_x", GetType(Decimal)),              ' 28
                                                 New DataColumn("pos_y", GetType(Decimal)),              ' 29
                                                 New DataColumn("label_text", GetType(String)),          ' 30
                                                 New DataColumn("label_icon", GetType(String)),          ' 31
                                                 New DataColumn("label_shape", GetType(String)),         ' 31a
                                                 New DataColumn("private_label", GetType(Boolean)),      ' 32
                                                 New DataColumn("note_width", GetType(Integer)),         ' 32a
                                                 New DataColumn("picture_width", GetType(Integer)),      ' 33
                                                 New DataColumn("co_child", GetType(Boolean)),           ' 34
                                                 New DataColumn("ds_line1", GetType(String)),            ' 35
                                                 New DataColumn("ds_line2", GetType(String)),            ' 36
                                                 New DataColumn("ds_line3", GetType(String)),            ' 37
                                                 New DataColumn("ds_line4", GetType(String)),            ' 38
                                                 New DataColumn("ds_line5", GetType(String)),            ' 39
                                                 New DataColumn("ds_line6", GetType(String)),            ' 40
                                                 New DataColumn("nodes_across", GetType(Integer)),       ' 41
                                                 New DataColumn("group_count", GetType(Integer)),        ' 41a
                                                 New DataColumn("showShadow", GetType(Boolean)),         ' 42
                                                 New DataColumn("shadowColour", GetType(String)),        ' 43
                                                 New DataColumn("linkColour", GetType(String)),          ' 44
                                                 New DataColumn("linkHover", GetType(String)),           ' 45
                                                 New DataColumn("linkWidth", GetType(Decimal)),          ' 46
                                                 New DataColumn("linkType", GetType(String)),            ' 47
                                                 New DataColumn("linkTooltipForeground", GetType(String)),' 48
                                                 New DataColumn("linkTooltipBackground", GetType(String)),' 49
                                                 New DataColumn("linkTooltipBorder", GetType(String)),    ' 50
                                                 New DataColumn("linkTooltip", GetType(String))           ' 51
                                                })

        Return resDT
    End Function
    Public Function GetModelDataItem(ByVal ItemRef As String, nodeHeight As Integer) As Object
        Return GetModelDataItemForType("Detail", ItemRef, nodeHeight)
    End Function
    Public Function GetModelDataItemForType(RequiredNodetype As String, ByVal ItemRef As String, nodeHeight As Integer) As Object
        Dim DT As DataTable
        Dim ResDT As New DataTable
        Dim byteImage As Byte()
        Dim ImagePhoto As Image
        Dim b64img As String
        Dim ImageWidth As Integer = 0
        Dim Line1 As String
        Dim Line2 As String
        Dim Line3 As String
        Dim Line4 As String
        Dim Line5 As String
        Dim Line6 As String
        Dim Model As DataRow
        Dim EffDate As DateTime
        Dim NodeType As String = ""
        Dim ReadonlyModel As Boolean
        Dim C As List(Of String)
        Dim LastLine As Integer
        Dim FieldText As String
        Dim DisplayType As DispType = If(RequiredNodetype.ToLower = "coparent", DispType.CoParent, DispType.OCAData)

        ' if parent group isn't defined, then use the normal definition
        If DisplayType = DispType.CoParent Then
            If DisplayData(DisplayType).Rows.Count = 0 Then
                DisplayType = DispType.OCAData
            End If
        End If

        Model = ModelHeader
        EffDate = Model.GetDate("effective_date").Date
        ReadonlyModel = Model.GetValue("user_ref", "") <> ctx.user_ref

        Dim OccurrenceDate As Date = Model("occurrence_date")

        ResDT.Columns.AddRange(New DataColumn() {New DataColumn("line1", GetType(String)),
                                                New DataColumn("line2", GetType(String)),
                                                New DataColumn("line3", GetType(String)),
                                                New DataColumn("line4", GetType(String)),
                                                New DataColumn("line5", GetType(String)),
                                                New DataColumn("line6", GetType(String)),
                                                New DataColumn("node_picture", GetType(String)),
                                                New DataColumn("picture_width", GetType(Integer)),
                                                New DataColumn("show_detail", GetType(Boolean))
                                            })

        ' Construct the Node display fields

        C = New List(Of String)
        ' get list of all the columns needed for the node display
        For Each D As DataRow In DisplayData(DisplayType).Rows
            If D("field_num") <> 0 Then
                FieldText = D("field_column")
                'If FieldText <> "" Then C.Add("IsNull(T." + FieldText + ",'') as " + FieldText)

                If FieldText <> "" Then
                    Select Case D("field_type")
                        Case "01"   ' String
                            C.Add("IsNull(T." + FieldText + ",'') as " + FieldText)
                        Case "02"   ' Date
                            C.Add("IsNull(dbo.dbf_format_date('o mmm, yyyy',T." + FieldText + "),'') as " + FieldText)
                        Case "02", "03", "04", "05"
                            C.Add("IsNull(Convert(nvarchar(max),T." + FieldText + "),'') as " + FieldText)
                    End Select
                End If

            End If
        Next

        'TODO add line to determine display of popup details
        Dim ShowDetail As Boolean = False

        SQL = "select PP.item_binary," +
                      String.Join(",", C.ToArray) +
                      GetRelatedLookupSQL("Count") + "
                 from dbo." + Model("table_name") + " T
                      left outer join dbo.client_document PP
                        on Convert(nvarchar(1000),PP.item_ref) = convert(nvarchar(1000),T.__item_ref)
                       and PP.client_id = @p2
                       and PP.item_type = '01'
                       and PP.unique_id = (select top 1 unique_id
                                             from dbo.client_document
                                            where client_id = PP.client_id
                                              and item_ref = PP.item_ref
                                            order by unique_id desc)
                where T.__item_ref = @p1
                  and T.__start_date = @p3"
        DT = IawDB.execGetTable(SQL, ItemRef, ClientID, OccurrenceDate)
        For Each DR As DataRow In DT.Rows
            b64img = ""
            ImageWidth = 0
            Line1 = ""
            Line2 = ""
            Line3 = ""
            Line4 = ""
            Line5 = ""
            Line6 = ""

            If DR("item_binary") IsNot System.DBNull.Value Then
                byteImage = DR("item_binary")
                Using ms As New IO.MemoryStream(byteImage)
                    ImagePhoto = GenericFunctions.ResizeImage(Image.FromStream(ms), New Size(0, nodeHeight - 2), GenericFunctions.SpecifyAxis.YAxis)
                    ImageWidth = ImagePhoto.Width
                    b64img = "data:image/png;base64," + GenericFunctions.ImageToBase64(ImagePhoto)
                End Using
            End If

            ' get the display data for the node
            FieldText = ""
            LastLine = -1
            C = New List(Of String)
            For Each D As DataRow In DisplayData(DisplayType).Rows
                If LastLine <> -1 Then
                    If LastLine <> D("disp_line") Then
                        C.Add(FieldText)
                        FieldText = ""
                        LastLine = D("disp_line")
                    End If
                End If
                If D("field_num") = 0 Then
                    FieldText += D("field_text")
                Else
                    FieldText += DR.GetValue(D("field_column"), "")
                End If
                LastLine = D("disp_line")
            Next
            C.Add(FieldText)
            For i As Integer = 0 To C.Count - 1
                If i = 0 Then Line1 = C(i)
                If i = 1 Then Line2 = C(i)
                If i = 2 Then Line3 = C(i)
                If i = 3 Then Line4 = C(i)
                If i = 4 Then Line5 = C(i)
                If i = 5 Then Line6 = C(i)
            Next

            'TODO
            Select Case Model.GetValue("det_pop_type", "03")
                Case "01"       ' Detail
                    ShowDetail = True
                Case "02"       ' List - have to see if there are any
                    ShowDetail = DR.GetValue("list_count", 0) > 0
                Case "03"       ' None
                    ShowDetail = False
            End Select

            ResDT.Rows.Add(Line1, Line2, Line3, Line4, Line5, Line6, b64img, ImageWidth, ShowDetail)
        Next

        ''---------------------------------------------------------------------------------

        Return GenericFunctions.ConvertToJSON(ResDT)

    End Function
    Public Function SaveModel(ModelData As String) As Object
        Dim DN As DiagramNode
        Dim DNL As List(Of DiagramNode) = JsonConvert.DeserializeObject(Of List(Of DiagramNode))(ModelData)

        Using DB As New IawDB

            Try
                DB.TranBegin()

                DB.NonQuery("delete from model_detail where model_key = @p1", ModelKey)

                SQL = "INSERT dbo.model_detail (model_key,detail_key,parent_key,item_ref,attributes)
                                   VALUES (@p1,@p2,@p3,@p4,@p5)"

                For Each DN In DNL

                    ' Construct the json string to store in 'attributes' column
                    Dim attr As String = JsonConvert.SerializeObject(DN, Formatting.None)

                    DB.NonQuery(SQL, ModelKey,
                                                 DN.detail_key,
                                                 DN.parent_key,
                                                 DN.item_ref,
                                                 attr)
                Next

                DB.TranCommit()
            Catch ex As Exception

                DB.TranRollback()
                Throw
            End Try
        End Using

        Return "OK"
    End Function
    Private Class DisplayFormEntry
        Public Label As String
        Public data As String
        Public Sub New()
            Label = ""
            data = ""
        End Sub
    End Class
    Public Function GetDetailInfo(ItemRef As String) As Object
        Dim json As New Dictionary(Of String, Object)
        Dim DR, relDR As DataRow
        Dim LastCol As Integer
        Dim Txt As String = ""

        Dim Matrix As Dictionary(Of Integer, Dictionary(Of Integer, DisplayFormEntry))
        Dim fDet As Dictionary(Of Integer, DisplayFormEntry)
        Dim line As Integer
        Dim col As Integer
        Dim Data As Object
        Dim FormattedString As String = ""

        Dim detail_pop_type As String = ModelHeader("det_pop_type")

        If detail_pop_type = "01" Then
            ' doing a form from the current datasource

            SQL = "select * 
                     from dbo." + ModelHeader("table_name") + "
                    where __item_ref = @p1"
            Dim TableData As DataRow = IawDB.execGetTable(SQL, ItemRef).Rows(0)

            ' this one has been sorted so that we get line then column so we can do things in the right order
            ' first pass to find max cols on a line
            Dim maxCol As Integer = 0
            Dim maxLine As Integer = 0
            For Each DR In DisplayData(DispType.DetailForm).Rows
                If DR("disp_col") > maxCol Then maxCol = DR("disp_col")
                If DR("disp_line") > maxLine Then maxLine = DR("disp_line")
            Next

            ' Initialize Matrix
            Matrix = New Dictionary(Of Integer, Dictionary(Of Integer, DisplayFormEntry))
            For i As Integer = 1 To maxLine
                fDet = New Dictionary(Of Integer, DisplayFormEntry)
                For j As Integer = 1 To maxCol
                    fDet.Add(j, New DisplayFormEntry)
                Next
                Matrix.Add(i, fDet)
            Next

            For Each DR In DisplayData(DispType.DetailForm).Rows
                line = DR("disp_line")
                col = DR("disp_col")

                If DR.GetValue("field_text", "") <> "" Then
                    Matrix(line)(col).Label = DR.GetValue("field_text", "")
                Else
                    Matrix(line)(col).Label = DR.GetValue("field_name", "")
                End If
                FormattedString = ""
                If Not DR("field_column") Is System.DBNull.Value Then
                    Data = TableData(DR("field_column"))
                    If Not DataFormatter(Data, DR("field_format"), DR("field_length"), FormattedString) Then
                        Throw New Exception(BuildMessage("ER_IM033", DR.GetValue("field_name", "")))
                    End If
                End If
                Matrix(line)(col).data = FormattedString
            Next

            Dim S As New StringBuilder
            S.Append("<table class=""note-form-table"">")
            For Each linekv As KeyValuePair(Of Integer, Dictionary(Of Integer, DisplayFormEntry)) In Matrix
                S.Append("<tr>")
                For Each colkv As KeyValuePair(Of Integer, DisplayFormEntry) In linekv.Value
                    S.Append("<td class=""note-td-label"">" + colkv.Value.Label + "</td>")
                    S.Append("<td class=""note-td-data"">" + colkv.Value.data + "</td>")
                Next
                S.Append("</tr>")
            Next
            S.Append("</table>")
            Txt = S.ToString

        Else

            SQL = GetRelatedLookupSQL("Query")
            Dim TableData As DataTable = IawDB.execGetTable(SQL, ItemRef)
            Dim relSourceID As Integer = ModelHeader("rel_source_id")
            Dim EvenRow As Boolean = True

            Dim S As New StringBuilder
            S.Append("<table>")

            ' ok we first need to create the list detail headers
            S.Append("<tr class='listheader'>")
            For Each FLD As DataRow In DisplayData(DispType.ListHeaders).Rows
                S.Append("<th>")
                If FLD("field_text") <> "" Then
                    S.Append(FLD("field_text"))
                Else
                    relDR = RelatedDataSourceFieldsDR(relSourceID, FLD("field_num"))
                    S.Append(relDR("field_name"))
                End If
                S.Append("</th>")
            Next
            S.Append("</tr>")

            Dim C As List(Of String)

            ' now we create the data for each row
            For Each DR In TableData.Rows
                If EvenRow Then
                    S.Append("<tr class=""listrow"">")
                Else
                    S.Append("<tr class=""listaltrow"">")
                End If

                C = New List(Of String)
                Txt = ""
                LastCol = -1
                For Each FLD As DataRow In DisplayData(DispType.ListData).Rows
                    If LastCol <> -1 Then
                        If LastCol <> FLD("disp_col") Then
                            C.Add(Txt)
                            Txt = ""
                            LastCol = FLD("disp_col")
                        End If
                    End If
                    If FLD("field_num") = 0 Then
                        Txt += FLD("field_text")
                    Else
                        relDR = RelatedDataSourceFieldsDR(relSourceID, FLD("field_num"))

                        Data = DR(relDR("field_column"))
                        FormattedString = ""
                        If Not DataFormatter(Data, FLD("field_format"), FLD.GetValue("field_length", 0), FormattedString) Then
                            Throw New Exception(BuildMessage("ER_IM033", FLD("field_name")))
                        End If
                        Txt += FormattedString
                    End If
                    LastCol = FLD("disp_col")
                Next
                C.Add(Txt)
                S.Append("<td>" + String.Join("</td><td>", C) + "</td>")
                S.Append("</tr>")
            Next

            S.Append("</table>")
            Txt = S.ToString
        End If

        json.Add("html", Txt)

        Return json
    End Function
    Private Function GetRelatedLookupSQL(forWhat As String) As String
        ' only do soemthing if needing secondary DS list 
        If ModelHeader("det_pop_type") <> "02" Then Return ""

        Dim TableName As String = ModelHeader("table_name")
        Dim det_field_from As Integer = ModelHeader("det_field_from")
        Dim det_field_to As Integer = ModelHeader("det_field_to")
        Dim sourceID As Integer = ModelHeader("source_id")
        Dim relSourceID As Integer = ModelHeader("rel_source_id")
        Dim relTableName As String = ModelHeader("rel_table_name")
        Dim relColName As String = ModelHeader("rel_field_column")

        If String.IsNullOrEmpty(relTableName) Then Return ""

        ' get the field for the current data source

        Dim ColName As String = IawDB.execScalar("select top 1 field_column
                                                                from dbo.data_source_field
                                                               where source_id = @p1
                                                                 and source_date <= @p2
                                                                 and field_num = @p3",
                                                             ModelHeader("source_id"),
                                                             ModelHeader("occurrence_date"),
                                                             ModelHeader("det_field_from"))

        Select Case forWhat.ToLower
            Case "query"
                SQL = "select R.* 
                                     from dbo." + TableName + " T
                                          join dbo." + relTableName + " R
                                            on R." + relColName + " = T." + ColName + "
                                    where T.__item_ref = @p1"
            Case "count"
                SQL = ",(select count(1) from dbo." + relTableName +
                                    " where convert(nvarchar(1000)," + relColName + ") = convert(nvarchar(1000),T." + ColName + ")) as 'list_count'"
        End Select

        Return SQL
    End Function
    Public Class ModelDetails
        Public Property backgroundContent As String
        Public Property backgroundID As String
        Public Property backgroundRepeat As String
        Public Property backgroundType As String
        Public Property chartDirection As Integer
        Public Property line_attr As ChartAttrib.Lines
        Public Property node_fg As String
        Public Property node_bg As String
        Public Property node_border_fg As String
        Public Property node_text_bg As String
        Public Property node_text_bg_block As Boolean
        Public Property node_icon_fg As String
        Public Property node_icon_hover As String
        Public Property node_corners As String
        Public Property node_height As Integer
        Public Property node_width As Integer
        Public Property node_highlight_fg As String
        Public Property node_highlight_bg As String
        Public Property node_highlight_border As String
        Public Property node_tt_fg As String
        Public Property node_tt_bg As String
        Public Property node_tt_border As String
        Public Property show_photos As Boolean
        Public Property image_position As String
        Public Property image_shape As String
        Public Property image_height As Integer
        Public Property showShadow As Boolean
        Public Property shadowColour As String
        Public Property linkColour As String
        Public Property linkHover As String
        Public Property linkType As String
        Public Property linkWidth As Decimal
        Public Property linkTooltipForeground As String
        Public Property linkTooltipBackground As String
        Public Property linkTooltipBorder As String
        Public Property button_position As String
        Public Property button_font As String
        Public Property button_shape As String
        Public Property button_text_colour As String
        Public Property button_back_colour As String
        Public Property button_border_colour As String
        Public Property button_text_hover As String
        Public Property button_back_hover As String
        Public Property button_border_hover As String
        Public Property button_detail_text As String
        Public Property button_note_text As String
    End Class
    Public Class DiagramNode
        Public Property detail_key As Integer
        Public Property parent_key As Integer
        Public Property item_ref As String
        Public Property pos_x As Decimal = 0.0
        Public Property pos_y As Decimal = 0.0
        Public Property node_width As Integer = 220
        Public Property node_height As Integer = 70
        Public Property node_corners As String = "Rectangle"
        Public Property assistant As Boolean = False
        Public Property node_fg As String = "#000000"
        Public Property node_bg As String = "#ffffff"
        Public Property node_border_fg As String = "#000000"
        Public Property node_text_bg As String = ""
        Public Property node_text_bg_block As Boolean = False
        Public Property node_icon_fg As String = "#000000"
        Public Property node_icon_hover As String = "#000080"
        Public Property show_photo As Boolean = False
        Public Property image_height As Integer = 100
        Public Property tree_expanded As Boolean = True
        Public Property group_expanded As Boolean = True
        Public Property node_type As String = "Detail"
        Public Property line1 As String = ""
        Public Property line2 As String = ""
        Public Property line3 As String = ""
        Public Property line4 As String = ""
        Public Property line5 As String = ""
        Public Property line6 As String = ""
        Public Property node_tt_fg As String = "#000000"
        Public Property node_tt_bg As String = "#D7E5F0"
        Public Property node_tt_border As String = "#000000"
        Public Property tooltip As String = ""
        Public Property label_text As String = ""
        Public Property label_icon As String = ""
        Public Property label_shape As String = "Circle"
        Public Property private_label As Boolean = False
        Public Property note_width As Integer = 600
        Public Property sequence As Integer = 0
        Public Property line_attr As ChartAttrib.Lines
        Public Property co_child As Boolean = False
        Public Property nodes_across As Integer = 0
        Public Property showShadow As Boolean = False
        Public Property shadowColour As String = "#000000"
        Public Property linkColour As String = "#000000"
        Public Property linkHover As String = "#0000ff"
        Public Property linkWidth As Decimal = 1.5
        Public Property linkType As String = "solid"
        Public Property linkTooltipForeground As String = "#000000"
        Public Property linkTooltipBackground As String = "#D7E5F0"
        Public Property linkTooltipBorder As String = "#000000"
        Public Property linkTooltip As String = ""
    End Class
End Class
