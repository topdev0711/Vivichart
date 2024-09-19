Imports System.Globalization
Imports IAW.controls

<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class dataviews
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
#Region "Clients"

    ReadOnly Property ClientGlobalDR As DataRow
        Get
            Dim DT As DataTable = ViewState("ClientGlobalDT")
            If DT Is Nothing Then
                SQL = "SELECT attributes
                         FROM dbo.clients
                        WHERE client_id = @p1"
                DT = IawDB.execGetTable(SQL, client_id)
                ViewState("ClientGlobalDT") = DT
            End If
            Return DT.Rows(0)
        End Get
    End Property
    ReadOnly Property ClientAttributes As ChartAttrib
        Get
            Return New ChartAttrib(ClientGlobalDR("Attributes"))
        End Get
    End Property

#End Region
#Region "Data Source"

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
    ReadOnly Property CleanDataSourceDT() As DataTable
        Get
            ViewState("DataSourceDT") = Nothing
            Return DataSourceDT
        End Get
    End Property
    ReadOnly Property DataSourceDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("DataSourceDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT DS.source_id,
                                                IsNull(DS.short_ref,'') as short_ref,
                                                DS.table_name,
                                                DS.primary_source
                                           FROM dbo.dbf_data_source_access(@p1) DSA
                                                JOIN dbo.data_source DS WITH (NOLOCK)
                                                    ON DS.source_id = DSA.source_id
                                          WHERE EXISTS (SELECT TOP 1 1
                                                          FROM dbo.client_import_seg
                                                         WHERE source_id = DS.source_id
                                                           AND seg_status = '10')
                                            AND primary_source = '1'
                                          ORDER BY DS.short_ref", ctx.user_ref)
                ViewState("DataSourceDT") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property DataSourceDR() As DataRow
        Get
            If DataSourceID = -1 Then Return Nothing
            Return DataSourceDT.Select("source_id = '" + DataSourceID.ToString + "'")(0)
        End Get
    End Property

#End Region
#Region "popDataSourceDT"

    ReadOnly Property popDataSourceDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("popDataSourceDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT DS.source_id,
                                                IsNull(DS.short_ref,'') as short_ref
                                           FROM dbo.dbf_data_source_access(@p1) DSA
                                                JOIN dbo.data_source DS WITH (NOLOCK)
                                                  ON DS.source_id = DSA.source_id
                                          WHERE EXISTS (SELECT top 1 1
                                                          FROM dbo.client_import_seg
                                                         WHERE source_id = DS.source_id
                                                           AND seg_status = '10')
                                          ORDER BY DS.short_ref", ctx.user_ref)
                ViewState("popDataSourceDT") = DT
            End If
            Return DT
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
                Dim EffDate As DateTime

                If DataViewDT.Rows.Count > 0 Then
                    EffDate = DataViewDR("data_effective")
                Else
                    EffDate = New Date(1900, 1, 1)
                End If

                DT = IawDB.execGetTable("SELECT 0 as field_num,
                                                @p3 as field_name,
                                                '' as field_type,
                                                0 as field_length
                                          WHERE @p4 = 1
                                         UNION
                                         SELECT field_num,
                                                field_name,
                                                field_type,
                                                field_length
                                           FROM dbo.data_source_field WITH (NOLOCK)
                                          WHERE source_id = @p1
                                            AND source_date = @p2
                                          ORDER BY field_num", DataSourceID, EffDate, ctx.Translate("::LT_S0064"), addText) ' LT_S0064 - Text
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
    ReadOnly Property LinkedFieldsDT(dsID As Integer, includeText As Boolean) As DataTable
        Get
            Dim DT As DataTable
            Dim EffDate As DateTime
            Dim addText As Integer = If(includeText, 1, 0)

            If DataViewDT.Rows.Count > 0 Then
                EffDate = DataViewDR("data_effective")
            Else
                EffDate = New Date(1900, 1, 1)
            End If

            DT = IawDB.execGetTable("SELECT 0 as field_num,
                                            @p3 as field_name,
                                            '' as field_type,
                                            0 as field_length
                                      WHERE @p4 = 1
                                     UNION
                                     SELECT field_num,
                                            field_name,
                                            field_type,
                                            field_length
                                       FROM dbo.data_source_field WITH (NOLOCK)
                                      WHERE source_id = @p1
                                        AND source_date = (SELECT ISNULL(P.source_date,N.source_date)
                                                             FROM (SELECT MAX(source_date) AS source_date
                                                                     FROM dbo.data_source_field
                                                                    WHERE source_id = @p1
                                                                      AND source_date <= @p2) P,
                                                                  (SELECT MIN(source_date) AS source_date
                                                                     FROM dbo.data_source_field
                                                                    WHERE source_id = @p1
                                                                      AND source_date > @p2) N)
                                      ORDER BY field_num",
                                    dsID, EffDate, ctx.Translate("::LT_S0064"), addText) ' LT_S0064- Text
            Return DT
        End Get
    End Property
    ReadOnly Property LinkedFieldsDR(dsID As Integer, field As Integer) As DataRow
        Get
            Return LinkedFieldsDT(dsID, True).Select("field_num = " + field.ToString)(0)
        End Get
    End Property

#End Region
#Region "Data View"

    Property ViewID As Integer
        Get
            Return ViewState("ViewID")
        End Get
        Set(value As Integer)
            ViewState("ViewID") = value
        End Set
    End Property
    ReadOnly Property CleanDataViewDT() As DataTable
        Get
            ViewState("DataViewDT") = Nothing
            Return DataViewDT
        End Get
    End Property
    ReadOnly Property DataViewDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("DataViewDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT view_id,
                                                user_ref,
                                                view_ref,
                                                source_id,
                                                data_effective,
                                                det_pop_type,
                                                det_source_id,
                                                det_field_from,
                                                det_field_to,
                                                IsNull(aea_text,'Available') as aea_text,
                                                IsNull(photos_applicable,'0') as photos_applicable,
                                                IsNull(allow_drilldown,'0') as allow_drilldown,
                                                IsNull(attributes,'') as attributes
                                           FROM dbo.data_view 
                                          WHERE source_id = @p1
                                            AND user_ref = @p2
                                          ORDER BY view_ref, data_effective DESC",
                                        DataSourceID, ctx.user_ref)
                ViewState("DataViewDT") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property DataViewDR() As DataRow
        Get
            Return DataViewDT.Select("view_id = '" + ViewID.ToString + "'")(0)
        End Get
    End Property
    ReadOnly Property ViewAttributes As ChartAttrib
        Get
            Return New ChartAttrib(DataViewDR("Attributes"))
        End Get
    End Property

#End Region
#Region "Data View Display"

    ReadOnly Property DisplayGrid As IAWGrid
        Get
            Dim targetGrid As IAWGrid = Nothing
            Select Case hdnAccValue.Value
                Case "OCA"
                    targetGrid = grdDispOCA
                Case "CoParent"
                    targetGrid = grdCoParent
                Case "AEAHead"
                    targetGrid = grdAEAHead
                Case "AEAFields"
                    targetGrid = grdAEAFields
                Case "AEASort"
                    targetGrid = grdAEASort
                Case "FormDisplay"
                    targetGrid = grdFormDisplay
                Case "ListHead"
                    targetGrid = grdListHeaders
                Case "ListData"
                    targetGrid = grdListData
                Case "ListSort"
                    targetGrid = grdListSort
            End Select
            Return targetGrid
        End Get
    End Property
    ReadOnly Property CleanDataViewDisplayDT As DataTable
        Get
            ViewState("DataViewDisplayDT") = Nothing
            Return DataViewDisplayDT
        End Get
    End Property
    ReadOnly Property DataViewDisplayDT As DataTable
        Get
            Dim i, j As Integer
            Dim DR As DataRow
            Dim maxCol, maxLine, tmpMax As Integer

            Dim DT As DataTable = ViewState("DataViewDisplayDT")
            If DT Is Nothing Then
                DT = IawDB.execGetTable("SELECT '' AS line_desc,
                                                '' AS col_desc,
                                                DVD.disp_type,
                                                DVD.disp_line,
                                                DVD.disp_col,
                                                DVD.disp_seq,
                                                DVD.field_num,
                                                DVD.field_text,
                                                DVD.field_format,
                                                DVD.field_align,
                                                DSF.field_type,
                                                CASE WHEN DVD.disp_type IN ('01','02','03','04','05','08')
                                                     THEN DSF.field_name
                                                     ELSE LDSF.field_name
                                                 END as field_name,
                                                display_name = CASE WHEN DVD.field_num = 0 THEN '""' + DVD.field_text + '""' 
                                                                    ELSE CASE WHEN DVD.disp_type IN ('01','02','03','04','05','08')
                                                                              THEN DSF.field_name
                                                                              ELSE LDSF.field_name
                                                                          END
                                                                END
                                                , '[' + DVD.field_text + ']' as temp_text
                                                , datalength(dvd.field_text) as temp_len
                                            FROM dbo.data_view_display DVD 
                                                    Join dbo.data_view DV 
                                                    On DV.view_id = DVD.view_id 
                                                    Left OUTER JOIN dbo.data_source_field DSF 
                                                    On DSF.source_id = DV.source_id 
                                                    And DSF.source_date = DV.data_effective 
                                                    And DSF.field_num = DVD.field_num 

                                                    Left OUTER JOIN dbo.data_source_field LDSF 
                                                    On LDSF.source_id = DV.det_source_id
                                                    And LDSF.source_date = dbo.dbf_nearest_datasource_occurrence(dv.det_source_id,DV.data_effective) 
                                                    And LDSF.field_num = DVD.field_num

                                            WHERE DVD.view_id = @p1
                                            ORDER BY disp_type, disp_line, disp_col, disp_seq", ViewID)

                ' for each of the disp types we need to add the grouping lines too
                ' OCAData      "01"
                ' AEASort      "02"
                ' AEAHeaders   "03"
                ' AEAData      "04"
                ' DetailForm   "05"
                ' ListHeaders  "06"
                ' ListData     "07"
                ' CoParent     "08"
                ' ListSort     '09'

                ' OCA - add the 6 lines
                ' CoParent - add the 6 lines
                For i = 1 To 6
                    ' CheckLineExists(DT, "01", i) Then Continue For
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "01"
                    DR("disp_line") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)

                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "08"
                    DR("disp_line") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                ' we want the max number of cols for the AEA types.
                maxCol = GetMaxCol(DT, "02")             ' AEA Sort
                tmpMax = GetMaxCol(DT, "03")             ' AEA Head
                If tmpMax > maxCol Then maxCol = tmpMax
                tmpMax = GetMaxCol(DT, "04")             ' AEA data
                If tmpMax > maxCol Then maxCol = tmpMax

                ' AEA headers
                For i = 1 To maxCol
                    If CheckColExists(DT, "03", i) Then Continue For
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "03"
                    DR("disp_col") = i
                    DR("disp_seq") = 0
                    DT.Rows.Add(DR)
                Next
                ' add spacer at bottom for sort drag drop
                DR = DT.NewRow()
                SetDisplayDefaults(DR)
                DR("disp_type") = "03"
                DR("disp_col") = 999
                DR("disp_seq") = 999
                DT.Rows.Add(DR)

                ' AEA Data
                For i = 1 To maxCol
                    ' if there are no entries for a col, ensure we have a placeholder
                    If Not CheckColExists(DT, "04", i) Then
                        DR = DT.NewRow()
                        SetDisplayDefaults(DR)
                        DR("disp_type") = "04"
                        DR("disp_col") = i
                        DR("disp_seq") = 0
                        DT.Rows.Add(DR)
                    End If
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "04"
                    DR("disp_col") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                ' AEA Sort
                For i = 1 To maxCol
                    ' if there are no entries for a col, ensure we have a placeholder
                    If Not CheckColExists(DT, "02", i) Then
                        DR = DT.NewRow()
                        SetDisplayDefaults(DR)
                        DR("disp_type") = "02"
                        DR("disp_col") = i
                        DR("disp_seq") = 0
                        DT.Rows.Add(DR)
                    End If

                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "02"
                    DR("disp_col") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                ' Display Form

                maxCol = GetMaxCol(DT, "05")
                maxLine = GetMaxLine(DT, "05")
                For j = 1 To maxCol
                    For i = 1 To maxLine
                        If CheckLineColExists(DT, "05", i, j) Then Continue For
                        'MaxSeq = GetMaxLineColSeq(DT, "05", i, j) + 1
                        DR = DT.NewRow()
                        SetDisplayDefaults(DR)
                        DR("disp_type") = "05"
                        DR("disp_line") = i
                        DR("disp_col") = j
                        DR("disp_seq") = 1
                        DT.Rows.Add(DR)
                    Next
                    ' add a spacer line at the bottom to allow sorting
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "05"
                    DR("disp_line") = 999
                    DR("disp_col") = j
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                ' Display List Head
                maxCol = GetMaxCol(DT, "06")             ' List Head
                tmpMax = GetMaxCol(DT, "07")             ' List Data
                If tmpMax > maxCol Then maxCol = tmpMax

                ' List headers
                For i = 1 To maxCol
                    If CheckColExists(DT, "06", i) Then Continue For
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "06"
                    DR("disp_col") = i
                    DR("disp_seq") = 0
                    DT.Rows.Add(DR)
                Next
                ' add spacer at bottom for sort drag drop
                DR = DT.NewRow()
                SetDisplayDefaults(DR)
                DR("disp_type") = "06"
                DR("disp_col") = 999
                DR("disp_seq") = 999
                DT.Rows.Add(DR)

                ' List Data
                For i = 1 To maxCol
                    'If CheckColExists(DT, "07", i) Then Continue For
                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "07"
                    DR("disp_col") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                ' List Sort - there is only ever 1 column because it's just a single sort sequence
                For i = 1 To 1
                    ' if there are no entries for a col, ensure we have a placeholder
                    If Not CheckColExists(DT, "09", i) Then
                        DR = DT.NewRow()
                        SetDisplayDefaults(DR)
                        DR("disp_type") = "09"
                        DR("disp_col") = i
                        DR("disp_seq") = 0
                        DT.Rows.Add(DR)
                    End If

                    DR = DT.NewRow()
                    SetDisplayDefaults(DR)
                    DR("disp_type") = "09"
                    DR("disp_col") = i
                    DR("disp_seq") = 999
                    DT.Rows.Add(DR)
                Next

                Dim DV As New DataView(DT)
                DV.Sort = "disp_type, disp_col, disp_line, disp_seq"
                DT = DV.ToTable
                ViewState("DataViewDisplayDT") = DT
            End If

            Return DT
        End Get
    End Property

    Sub SetDisplayDefaults(ByRef DR As DataRow)
        DR("disp_type") = ""
        DR("disp_line") = 0
        DR("disp_col") = 0
        DR("disp_seq") = 0
        DR("field_num") = 0
        DR("field_text") = ""
        DR("field_format") = ""
        DR("field_align") = "01"
        DR("field_type") = ""
        DR("field_name") = ""
    End Sub
    Sub ReSortDisplayDT()
        Dim DT As DataTable = ViewState("DataViewDisplayDT")
        Dim DV As New DataView(DT)
        DV.Sort = "disp_type, disp_col, disp_line, disp_seq"
        ViewState("DataViewDisplayDT") = DV.ToTable
    End Sub

    Function CheckLineExists(DT As DataTable, TYP As String, LINE As Integer) As Boolean
        Return DT.AsEnumerable().
                                       Any(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                         CInt(row("disp_line")) = LINE)
    End Function
    Function CheckColExists(DT As DataTable, TYP As String, COL As Integer) As Boolean
        Return DT.AsEnumerable().
                                       Any(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                         CInt(row("disp_col")) = COL)
    End Function
    Function CheckLineColExists(DT As DataTable, TYP As String, LINE As Integer, COL As Integer) As Boolean
        Return DT.AsEnumerable().
                                       Any(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                         CInt(row("disp_line")) = LINE AndAlso
                                                         CInt(row("disp_col")) = COL)
    End Function

    Function GetMaxCol(DT As DataTable, TYP As String) As Integer
        Return DT.AsEnumerable().
                                       Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                           CInt(row("disp_seq")) <> 999 AndAlso
                                                           CInt(row("disp_seq")) <> 0).
                                       Select(Function(row) CInt(row("disp_col"))).
                                       DefaultIfEmpty(1).
                                       Max()
    End Function
    Function GetMaxLine(DT As DataTable, TYP As String) As Integer
        Return DT.AsEnumerable().
                                       Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                           CInt(row("disp_seq")) < 999 AndAlso
                                                           CInt(row("disp_seq")) <> 0).
                                       Select(Function(row) CInt(row("disp_line"))).
                                       DefaultIfEmpty(1).
                                       Max()
    End Function

    Function GetMaxLineCol(DT As DataTable, TYP As String, LIN As Integer) As Integer
        Return DT.AsEnumerable().
                                       Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                            CInt(row("disp_line")) > LIN AndAlso
                                                           CInt(row("disp_seq")) < 999 AndAlso
                                                           CInt(row("disp_seq")) <> 0).
                                       Select(Function(row) CInt(row("disp_col"))).
                                       DefaultIfEmpty(1).
                                       Max()
    End Function
    Function GetMaxLineColSeq(DT As DataTable, TYP As String, LIN As Integer, COL As Integer) As Integer
        Return DT.AsEnumerable().
                                       Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                           CInt(row("disp_line")) = LIN AndAlso
                                                           CInt(row("disp_col")) = COL AndAlso
                                                           CInt(row("disp_seq")) <> 999 AndAlso
                                                           CInt(row("disp_seq")) <> 0).
                                       Select(Function(row) CInt(row("disp_seq"))).
                                       DefaultIfEmpty(0).
                                       Max()
    End Function
    Function GetTypeLineColCount(TYP As String, LINE As Integer, COL As Integer) As Integer
        Return DataViewDisplayDT.AsEnumerable().
                                        Count(Function(r) CStr(r("disp_type")) = TYP AndAlso
                                                          CInt(r("disp_line")) = LINE AndAlso
                                                          CInt(r("disp_col")) = COL AndAlso
                                                          CInt(r("disp_seq")) <> 999 AndAlso
                                                          CInt(r("disp_seq")) <> 0)
    End Function

    ReadOnly Property DataViewDisplayType(dispType As String) As DataTable
        Get
            Dim DV As New DataView(DataViewDisplayDT)
            DV.RowFilter = "disp_type='" + dispType + "'"
            DV.Sort = "disp_type, disp_line, disp_col, disp_seq"
            Return DV.ToTable
        End Get
    End Property
    ReadOnly Property DataViewDisplayTypeCols(dispType As String) As DataTable
        Get
            Dim DV As New DataView(DataViewDisplayDT)
            DV.RowFilter = "disp_type='" + dispType + "'"
            DV.Sort = "disp_type, disp_col, disp_line, disp_seq"
            Return DV.ToTable
        End Get
    End Property

#End Region
#Region "Occurrence Dates"

    Property OccurrenceDate As Date
        Get
            Return ViewState("OccurrenceDate")
        End Get
        Set(value As Date)
            ViewState("OccurrenceDate") = value
        End Set
    End Property
    ReadOnly Property CleanOccurrenceDateDT() As DataTable
        Get
            ViewState("OccurrenceDateDT") = Nothing
            Return OccurrenceDateDT
        End Get
    End Property
    ReadOnly Property OccurrenceDateDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("OccurrenceDateDT")
            If DT Is Nothing Then
                Dim DR As DataRow = DataSourceDR
                SQL = "SELECT distinct
                                                          __start_date as source_date,
                                                          dbo.dbf_format_date('dd/MM/yyyy HH:mm',__start_date) as source_date_text
                                                     FROM dbo." + DR("table_name") + "
                                                    ORDER BY __start_date DESC"
                DT = IawDB.execGetTable(SQL)
                ViewState("OccurrenceDateDT") = DT
            End If
            Return DT
        End Get
    End Property

#End Region
#Region "Fonts"

    ReadOnly Property FontsDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("Fonts")
            If DT Is Nothing Then
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
                DT = IawDB.execGetTable(SQL, ctx.languageCode, client_id)
                ViewState("Fonts") = DT
            End If
            Return DT
        End Get
    End Property

#End Region
#Region "Dropdowns"
    Private ReadOnly Property BackgroundTypesDT As DataTable
        Get
            Dim DT As New DataTable
            DT.Columns.Add("ddType")
            DT.Columns.Add("ddText")

            DT.Rows.Add("SolidColour", SawLang.Translate("::LT_S0257")) ' LT_S0257 - Single Colour
            If GradientsDT.Rows.Count > 0 Then
                DT.Rows.Add("Gradient", SawLang.Translate("::LT_S0150"))  ' LT_S0150 - Gradient
            End If
            If BackgroundsDT.Rows.Count > 0 Then
                DT.Rows.Add("Image", SawLang.Translate("::LT_S0151")) ' LT_S0151- Image
            End If

            Return DT
        End Get
    End Property

    Public ReadOnly Property BackgroundsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Backgrounds")
            If DT Is Nothing Then
                SQL = "select unique_id, content, description
                         From dbo.background
                        where client_id in (0,@p1)
                          and background_type = '01'
                        order by client_id desc, description asc"

                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Backgrounds") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property BackgroundsDR(unique_id As Integer) As DataRow
        Get
            Return BackgroundsDT.Select("unique_id = " + unique_id.ToString)(0)
        End Get
    End Property

    Public ReadOnly Property GradientsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Gradients")
            If DT Is Nothing Then
                SQL = "select unique_id, content, description
                         From dbo.background
                        where client_id in (0,@p1)
                          and background_type = '02'
                        order by client_id desc, description asc"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Gradients") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property GradientsDR(unique_id As Integer) As DataRow
        Get
            Return GradientsDT.Select("unique_id = " + unique_id.ToString)(0)
        End Get
    End Property

    'Private ReadOnly Property DirectionDT As DataTable
    '    Get
    '        Dim DT As New DataTable
    '        DT.Columns.Add("ddType")
    '        DT.Columns.Add("ddText")

    '        DT.Rows.Add("90", "Top Down")
    '        DT.Rows.Add("0", "Left to Right")
    '        DT.Rows.Add("180", "Right to Left")

    '        Return DT
    '    End Get
    'End Property

#End Region

#Region "Page Events"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Populate()
            ModalState = ""
        End If
        SetModalState()
        'AddAttributeDefaultNames()

        grdDataView.CssClass = "allow-hover"
        grdDatasource.CssClass = "allow-hover"
    End Sub
    Sub Populate()

        If CleanDataSourceDT.Rows.Count = 0 Then
            lblDataViewMsg.Text = SawLang.Translate("::LT_S0258") ' LT_S0258 - There are currently no Data Sources, please import one
            lblDataViewMsg.Visible = True
            Return
        End If

        SetDDLB(ddlbDataSource, DataSourceDT, "short_ref", "source_id")
        DataSourceID = ddlbDataSource.SelectedValue

        SetDDLB(ddlbOccurrenceDate, CleanOccurrenceDateDT, "source_date_text", "source_date")
        OccurrenceDate = ddlbOccurrenceDate.SelectedValue

        SetDDLBPuptext(ddlbShowDetailsType, "data_view", "det_pop_type")
        SetDDLB(ddlbDetailsDataSource, popDataSourceDT, "short_ref", "source_id")

        SetDDLB(ddlbFont, FontsDT, "font_name", "font_name")
        SetDDLB(ddlbBackgroundType, BackgroundTypesDT, "ddText", "ddType")
        SetDDLB(ddlbBackgroundImage, BackgroundsDT, "description", "unique_id")
        SetDDLB(ddlbBackgroundGradient, GradientsDT, "description", "unique_id")

        SetDDLBPuptext(ddlbDEbool, "data_view_display", "boolean_format")
        SetDDLBPuptext(ddlbDEDateFormat, "data_source_field", "field_format")

        RefreshGrid("DataSource")

    End Sub
    Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeDataViewForm.Hide()
        mpeDisplayEntryForm.Hide()

        Select Case ModalState

            Case "DataView"
                mpeDataViewForm.Show()

            Case "DataViewEntry"
                mpeDisplayEntryForm.Show()

        End Select
    End Sub
    Sub RefreshGrid(ByVal GridName As String)
        Select Case GridName
            Case "DataSource"
                SetGrid(grdDatasource, CleanDataSourceDT, True)
                DataSourceID = grdDatasource.SelectedValue

                RefreshGrid("OccurrenceDate")
                RefreshGrid("DataView")

            Case "OccurrenceDate"

                SetDDLB(ddlbOccurrenceDate, CleanOccurrenceDateDT, "source_date_text", "source_date")
                OccurrenceDate = ddlbOccurrenceDate.SelectedValue

            Case "DataView"
                SetGrid(grdDataView, CleanDataViewDT, True)
                ViewID = grdDataView.SelectedValue

                Dim showHide As Boolean

                showHide = DataSourceDR("primary_source") = "1"

                ShowHideControl(trTxtAEA, showHide)
                ShowHideControl(trShowDetailsType, showHide)
                ShowHideControl(trPhotosApplicable, showHide)
                ShowHideControl(trAllowDrilldown, showHide)

                ShowHideControl(trLinkedDataSource, showHide)
                ShowHideControl(trDetailFieldFrom, showHide)
                ShowHideControl(trDetailFieldTo, showHide)

                tpChart.Visible = showHide And ClientAttributes.Data.canOverride
                tpNodes.Visible = showHide And ClientAttributes.Data.canOverride
                tpFont.Visible = showHide And ClientAttributes.Data.canOverride

                If DataViewDT.Rows.Count > 0 Then
                    SetModelDefaults(ViewAttributes)
                    ShowHideControl(pnlDisplay, True)
                    RefreshGrid("DataDetails")
                Else
                    ShowHideControl(pnlDisplay, False)
                End If

            Case "DataDetails"

                SetGrid(grdFields, CleanFieldsDT, False)
                FieldNum = grdFields.SelectedValue

                ViewState("DataViewDisplayDT") = Nothing
                SetGrid(grdDispOCA, DataViewDisplayType("01"), False)
                SetGrid(grdCoParent, DataViewDisplayType("08"), False)
                SetGrid(grdAEAHead, DataViewDisplayType("03"), False)
                SetGrid(grdAEAFields, DataViewDisplayType("04"), False)
                SetGrid(grdAEASort, DataViewDisplayType("02"), False)
                SetGrid(grdFormDisplay, DataViewDisplayTypeCols("05"), False)
                SetGrid(grdListHeaders, DataViewDisplayType("06"), False)
                SetGrid(grdListData, DataViewDisplayType("07"), False)
                SetGrid(grdListSort, DataViewDisplayType("09"), False)

                AccordianVisibility()

        End Select
    End Sub
    Sub AccordianVisibility()
        accPanelFormDisplay.Visible = False
        accPanelListHeader.Visible = False
        accPanelListData.Visible = False
        accPanelListSort.Visible = False

        Select Case DataViewDR("det_pop_type")
            Case "01"
                accPanelFormDisplay.Visible = True
            Case "02"
                accPanelListHeader.Visible = True
                accPanelListData.Visible = True
                accPanelListSort.Visible = True
        End Select
    End Sub

#End Region

#Region "Data Source Grid"

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
                SetDDLB(ddlbOccurrenceDate, CleanOccurrenceDateDT, "source_date_text", "source_date")
                OccurrenceDate = ddlbOccurrenceDate.SelectedValue

                RefreshGrid("DataView")

        End Select
    End Sub

#End Region

#Region "Data View Grid"

    Sub grdDataView_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdDataView.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDataView, "Select$" & e.Row.RowIndex)
            e.Row.Cells(1).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdDataView, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdDataView_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdDataView.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            Dim lView As Integer = rv("view_id")
            If IawDB.execScalar("select count(1) from model_header where view_id = @p1", lView) > 0 Then
                CTRL = e.Row.FindControl("btnDel")
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

    Sub grdDataView_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDataView.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        Dim DR As DataRow

        If e.CommandName = "New" Then
            hdnViewID.Value = ""    ' new 

            DR = DataSourceDR

            ddlbDataSource.Enabled = True
            SetDDLBValue(ddlbDataSource, DR("source_id"))

            ddlbOccurrenceDate.Enabled = True

            txtReference.Text = ""
            txtAEA.Text = ctx.Translate("::LT_S0259") + " " + DataSourceDR("short_ref") ' LT_S0259 - Available

            ' linked datasource for lookups

            SetDDLBValue(ddlbDetailsDataSource, "03")
            trLinkedDataSource.Visible = False
            trDetailFieldFrom.Visible = False
            trDetailFieldTo.Visible = False

            cbPhotosApplicable.Checked = True
            cbAllowDrilldown.Checked = True

            SetModelDefaults(ClientAttributes)
            SetSliderRanges()

            tcMainContainer.ActiveTab = tpGeneral

            FieldFocus(txtReference)

            ModalState = "DataView"
            SetModalState()
            Return
        End If

        lblDataViewMsg.Visible = False

        CurRowIndex = grdDataView.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        ViewID = grdDataView.DataKeys(RowIndex).Value
        DR = DataViewDR()

        Select Case e.CommandName

            Case "Select"
                RefreshGrid("DataDetails")

            Case "AmendRow"
                grdDataView.SelectedIndex = RowIndex
                hdnViewID.Value = ViewID

                SetDDLBValue(ddlbDataSource, DR("source_id"))
                SetDDLBValue(ddlbOccurrenceDate, DR("data_effective"))
                ddlbDataSource.Enabled = False
                ddlbOccurrenceDate.Enabled = False

                txtReference.Text = DR("view_ref")
                txtAEA.Text = DR("aea_text")

                SetDDLBValue(ddlbShowDetailsType, DR("det_pop_type"))

                If DR("det_pop_type") <> "02" Then
                    trLinkedDataSource.Visible = False
                    trDetailFieldFrom.Visible = False
                    trDetailFieldTo.Visible = False
                Else
                    trLinkedDataSource.Visible = True
                    trDetailFieldFrom.Visible = True
                    trDetailFieldTo.Visible = True
                    SetDDLBValue(ddlbDetailsDataSource, DR("det_source_id"))
                    SetDDLB(ddlbDetailFieldFrom, FieldsDT(False), "field_name", "field_num")
                    SetDDLB(ddlbDetailFieldTo, LinkedFieldsDT(ddlbDetailsDataSource.SelectedValue, False), "field_name", "field_num")
                    SetDDLBValue(ddlbDetailFieldFrom, DR("det_field_from"))
                    SetDDLBValue(ddlbDetailFieldTo, DR("det_field_to"))
                End If

                cbPhotosApplicable.Checked = If(DR("photos_applicable") = "1", True, False)
                cbAllowDrilldown.Checked = If(DR("allow_drilldown") = "1", True, False)

                SetModelDefaults(ViewAttributes)
                SetSliderRanges()

                tcMainContainer.ActiveTab = tpGeneral

                'ShowHideControl(tpAttrib, ClientAttributes.Data.canOverride)
                'tpAttrib.Visible = ClientAttributes.Data.canOverride

                FieldFocus(txtReference)

                SetModalState("DataView")

            Case "DeleteRow"
                IawDB.execNonQuery("DELETE dbo.data_view where view_id = @p1", ViewID)

                RefreshGrid("DataView")
        End Select
    End Sub

#End Region

#Region "Data View Details Dialog"

    Sub btnModelBasisCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDataViewCancel.Click
        SetModalState("")
    End Sub

    Sub btnDataViewSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDataViewSave.Click
        If Not Page.IsValid Then Return

        If hdnViewID.Value = "" Then
            SQL = "INSERT dbo.data_view (
                          user_ref, view_ref, source_id, data_effective,
                          aea_text, 
                          photos_applicable, allow_drilldown,
                          det_pop_type, det_source_id,
                          det_field_from, det_field_to)
                  VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11)"
            IawDB.execNonQuery(SQL,
                        ctx.user_ref, txtReference.Text.Trim, DataSourceID, CDate(ddlbOccurrenceDate.SelectedValue),
                        txtAEA.Text.Trim,
                        If(cbPhotosApplicable.Checked, "1", "0"),
                        If(cbAllowDrilldown.Checked, "1", "0"),
                        ddlbShowDetailsType.SelectedValue, ddlbDetailsDataSource.SelectedValue,
                        ddlbDetailFieldFrom.SelectedValue, ddlbDetailFieldTo.SelectedValue)
        Else
            IawDB.execNonQuery("UPDATE dbo.data_view
                                   SET view_ref = @p2, 
                                       aea_text = @p3,
                                       photos_applicable = @p4,
                                       allow_drilldown = @p5,
                                       det_pop_type = @p6,
                                       det_source_id = @p7,
                                       det_field_from = @p8,
                                       det_field_to = @p9
                                 WHERE view_id = @p1",
                                hdnViewID.Value,
                                txtReference.Text.Trim,
                                txtAEA.Text.Trim,
                                If(cbPhotosApplicable.Checked, "1", "0"),
                                If(cbAllowDrilldown.Checked, "1", "0"),
                                ddlbShowDetailsType.SelectedValue,
                                ddlbDetailsDataSource.SelectedValue,
                                ddlbDetailFieldFrom.SelectedValue,
                                ddlbDetailFieldTo.SelectedValue)
        End If
        SaveAttributes()

        RefreshGrid("DataView")
        SetModalState("")
    End Sub

    Sub ddlbShowDetailsType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbShowDetailsType.SelectedIndexChanged

        If ddlbShowDetailsType.SelectedValue <> "02" Then
            trLinkedDataSource.Visible = False
            trDetailFieldFrom.Visible = False
            trDetailFieldTo.Visible = False
        Else
            trLinkedDataSource.Visible = True
            trDetailFieldFrom.Visible = True
            trDetailFieldTo.Visible = True

            SetDDLB(ddlbDetailFieldFrom, FieldsDT(False), "field_name", "field_num")
            SetDDLB(ddlbDetailFieldTo, LinkedFieldsDT(ddlbDetailsDataSource.SelectedValue, False), "field_name", "field_num")

        End If
    End Sub
    Sub ddlbDetailsDataSource_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlbDetailsDataSource.SelectedIndexChanged
        SetDDLB(ddlbDetailFieldTo, LinkedFieldsDT(ddlbDetailsDataSource.SelectedValue, 0), "field_name", "field_num")
    End Sub

    Sub SetSliderRanges()
        slideNodeWidth.Maximum = CInt(numMaxNodeWidth.Text)
        slideNodeWidth.Steps = ((slideNodeWidth.Maximum - slideNodeWidth.Minimum) / 10) + 1

        slideNodeHeight.Maximum = CInt(numMaxNodeHeight.Text)
        slideNodeHeight.Steps = ((slideNodeHeight.Maximum - slideNodeHeight.Minimum) / 10) + 1
    End Sub
    Sub SetModelDefaults(CA As ChartAttrib)

        ' hdnAttrib is used to handle the fonts tab client side (uses the lineAttr js control)
        '  .line1.font, .line1.fintSize, .line1.colour, etc
        hdnAttrib.Value = CA.GetData

        With CA.Data
            numMaxNodeHeight.Text = .maxHeight.ToString
            numMaxNodeWidth.Text = .maxWidth.ToString
            numNodeHeight.Text = .nodeHeight.ToString
            numNodeWidth.Text = .nodeWidth.ToString

            If .backgroundType = "none" Then .backgroundType = "SolidColour"
            Try
                ddlbBackgroundType.SelectedValue = If(String.IsNullOrEmpty(.backgroundType), "SolidColour", .backgroundType)
            Catch ex As Exception
                ddlbBackgroundType.SelectedValue = "SolidColour"
            End Try

            Select Case .backgroundType
                Case "SolidColour"
                    txtModelbg.Text = .backgroundContent
                Case "Gradient"
                    ddlbBackgroundGradient.SelectedValue = .backgroundID
                Case Else
                    ddlbBackgroundImage.SelectedValue = .backgroundID
            End Select

            ddlbDirection.SelectedValue = If(.chartDirection = Nothing, 90, .chartDirection)
            ddlbImagePosition.SelectedValue = .imagePosition

            With .node
                txtNodefg.Text = .foreground
                txtNodeTxtBg.Text = .textBackground
                txtNodebg.Text = .background
                txtNodeBorder.Text = .border
                cbNodeTxtBlock.Checked = .textBackgroundBlock
                txtIconfg.Text = .iconColour
                txtIconHover.Text = .iconHover
            End With

            With .highlight
                txtHighlightfg.Text = .foreground
                txtHighlightbg.Text = .background
                txtHighlightbBorder.Text = .border
            End With

            With .tooltip
                txtTTfg.Text = .foreground
                txtTTbg.Text = .background
                txtTTBorder.Text = .border
            End With

            rbCornerRectangle.Checked = (.corners = "Rectangle")
            rbCornerRoundedRectangle.Checked = (.corners = "RoundedRectangle")
            cbShowShadow.Checked = .showShadow
            txtShadow.Text = .shadowColour

            With .link
                txtlinkColour.Text = .colour
                txtLinkHover.Text = .hover
                numLinkWidth.Text = .width.ToString
                ddlbLinkStyle.SelectedValue = .type
                With .tooltip
                    txtLinkTooltipfg.Text = .foreground
                    txtLinkTooltipbg.Text = .background
                    txtLinkTooltipBorder.Text = .border
                End With
            End With

            'trUseImages.Visible = .imagesApplicable
            cbSettigsFixed.Checked = .canOverride
        End With

    End Sub

    Sub SaveAttributes()

        Dim CA As New ChartAttrib(hdnAttrib.Value)

        With CA.Data
            .maxHeight = CInt(numMaxNodeHeight.Text)
            .maxWidth = CInt(numMaxNodeWidth.Text)
            .nodeHeight = CInt(numNodeHeight.Text)
            .nodeWidth = CInt(numNodeWidth.Text)
            .chartDirection = ddlbDirection.SelectedValue
            .imagePosition = ddlbImagePosition.SelectedValue
            .canOverride = cbSettigsFixed.Checked

            .backgroundType = ddlbBackgroundType.SelectedValue
            .backgroundContent = "rgba(0,0,0,0)"  ' transparent
            .backgroundID = 0                     ' not set

            Select Case ddlbBackgroundType.SelectedValue
                Case "SolidColour"
                    .backgroundContent = txtModelbg.Text
                Case "Gradient"
                    .backgroundID = ddlbBackgroundGradient.SelectedValue
                    .backgroundContent = GradientsDR(ddlbBackgroundGradient.SelectedValue)("content")
                Case Else
                    .backgroundID = ddlbBackgroundImage.SelectedValue
                    .backgroundContent = BackgroundsDR(ddlbBackgroundImage.SelectedValue)("content")
            End Select

            With .node
                .foreground = txtNodefg.Text
                .textBackground = txtNodeTxtBg.Text
                .background = txtNodebg.Text
                .border = txtNodeBorder.Text
                .textBackgroundBlock = cbNodeTxtBlock.Checked
                .iconColour = txtIconfg.Text
                .iconHover = txtIconHover.Text
            End With

            'With .note
            '    .foreground = txtNotefg.Text
            '    .background = txtNotebg.Text
            '    .border = txtNoteBorder.Text
            'End With

            With .highlight
                .foreground = txtHighlightfg.Text
                .background = txtHighlightbg.Text
                .border = txtHighlightbBorder.Text
            End With

            With .tooltip
                .foreground = txtTTfg.Text
                .background = txtTTbg.Text
                .border = txtTTBorder.Text
            End With
            .corners = If(rbCornerRectangle.Checked, "Rectangle", "RoundedRectangle")
            .showShadow = cbShowShadow.Checked
            .shadowColour = txtShadow.Text
            With .link
                .colour = txtlinkColour.Text
                .hover = txtLinkHover.Text
                .width = Decimal.Parse(numLinkWidth.Text, CultureInfo.InvariantCulture)
                .type = ddlbLinkStyle.SelectedValue
                With .tooltip
                    .foreground = txtLinkTooltipfg.Text
                    .background = txtLinkTooltipbg.Text
                    .border = txtLinkTooltipBorder.Text
                End With
            End With

        End With

        SQL = "UPDATE dbo.data_view
                  SET attributes = @p2
                WHERE view_id = @p1"
        IawDB.execNonQuery(SQL, ViewID, CA.GetData)

    End Sub
    Sub numMaxNodeHeight_TextChanged(sender As Object, e As EventArgs) Handles numMaxNodeHeight.TextChanged
        Try
            Dim mw As Integer = CInt(numMaxNodeHeight.Text)
            Dim w As Integer = CInt(numNodeHeight.Text)

            slideNodeHeight.Maximum = mw
            SetSliderRanges()

            If w > mw Then
                numNodeHeight.Text = mw
            End If

        Catch ex As Exception
        End Try
    End Sub
    Sub numMaxNodeWidth_TextChanged(sender As Object, e As EventArgs) Handles numMaxNodeWidth.TextChanged
        Try
            Dim mw As Integer = CInt(numMaxNodeWidth.Text)
            Dim w As Integer = CInt(numNodeWidth.Text)

            slideNodeWidth.Maximum = mw
            SetSliderRanges()
            If w > mw Then
                numNodeWidth.Text = mw
            End If
        Catch ex As Exception
        End Try
    End Sub

#End Region

#Region "View Display Grids"

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


    Sub grdDisp_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdDispOCA.RowDataBound,
                                                                                  grdCoParent.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.DataItem Is Nothing Then Return

            e.Row.Attributes("ondrop") = "drop(event)"
            e.Row.Attributes("ondragover") = "allowDrop(event)"
            e.Row.Attributes("ondragleave") = "dragLeave(event)"
            e.Row.Attributes("data-target") = "any"

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            Dim sectionCount = GetTypeLineColCount(rv("disp_type"), rv("disp_line"), rv("disp_col"))

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
                e.Row.FindControl("btnAmend").Visible = False
                e.Row.FindControl("btnDel").Visible = False
            Else
                'e.Row.CssClass = "grdField left draggable"
                'If rv("field_num") <> 0 And rv("disp_seq") <> 1 Then
                '    e.Row.FindControl("btnAmend").Visible = False
                'End If

                e.Row.Cells(1).CssClass = "grdField left draggable"
                e.Row.Cells(1).Attributes.Add("draggable", "true")
                e.Row.Cells(1).Attributes.Add("ondragstart", "dragSortStart(event)")
                e.Row.Cells(1).Attributes.Add("data-source", "seq")
            End If

        End If
    End Sub
    Sub grdColumns_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdAEAHead.RowDataBound,
                                                                                             grdAEAFields.RowDataBound,
                                                                                             grdAEASort.RowDataBound,
                                                                                             grdListHeaders.RowDataBound,
                                                                                             grdListData.RowDataBound,
                                                                                             grdListSort.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.DataItem Is Nothing Then Return

            e.Row.Attributes("ondrop") = "drop(event)"
            e.Row.Attributes("ondragover") = "allowDrop(event)"
            e.Row.Attributes("ondragleave") = "dragLeave(event)"

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            Dim sectionCount As Integer = GetTypeLineColCount(rv("disp_type"), rv("disp_line"), rv("disp_col"))

            If rv("disp_type") <> "09" Then
                If rv("disp_seq") = 1 Or sectionCount = 0 Then
                    e.Row.Cells(0).Text = String.Format(ctx.Translate("::LT_S0261") + " {0}", rv("disp_col")) ' LT_S0261 - Column

                    e.Row.Cells(0).CssClass = "grdField left draggable"
                    e.Row.Cells(0).Attributes.Add("draggable", "true")
                    e.Row.Cells(0).Attributes.Add("ondragstart", "dragSortStart(event)")
                    e.Row.Cells(0).Attributes.Add("data-source", "column")
                Else
                    e.Row.Cells(0).Text = ""
                End If
            End If

            If rv("disp_seq") = 999 Then
                e.Row.Cells(0).Text = ""
                If rv("disp_type") <> "09" Then e.Row.Cells(1).Text = ""
                e.Row.CssClass = "bottomSpacer"
                If e.Row.FindControl("btnDel") IsNot Nothing Then
                    e.Row.FindControl("btnDel").Visible = False
                End If
                If e.Row.FindControl("btnAmend") IsNot Nothing Then
                    e.Row.FindControl("btnAmend").Visible = False
                End If
            Else
                If rv("disp_type") = "09" Then
                    e.Row.Cells(0).CssClass = "grdField left draggable"
                    e.Row.Cells(0).Attributes.Add("draggable", "true")
                    e.Row.Cells(0).Attributes.Add("ondragstart", "dragSortStart(event)")
                    e.Row.Cells(0).Attributes.Add("data-source", "seq")
                Else
                    e.Row.Cells(1).CssClass = "grdField left draggable"
                    e.Row.Cells(1).Attributes.Add("draggable", "true")
                    e.Row.Cells(1).Attributes.Add("ondragstart", "dragSortStart(event)")
                    e.Row.Cells(1).Attributes.Add("data-source", "seq")
                End If

                'If rv("field_num") <> 0 And rv("disp_seq") <> 1 Then
                '    If e.Row.FindControl("btnAmend") IsNot Nothing Then
                '        e.Row.FindControl("btnAmend").Visible = False
                '    End If
                'End If

                If CStr(rv("disp_type")).ContainsOneOf("04", "07") Then
                    If rv("disp_seq") = 1 And sectionCount = 1 And rv("field_num") = 0 And rv("field_text") = "" Then
                        e.Row.FindControl("btnDel").Visible = False
                    End If
                End If

            End If

            ' AEAHeaders   "03"
            ' ListHeaders  "06"

            If CStr(rv("disp_type")).ContainsOneOf("03", "06") And rv("disp_seq") = 999 Then
                ' if a header type, and it's the bottom spacer don't allow drop
                e.Row.Attributes("data-target") = "sort"
            Else
                e.Row.Attributes("data-target") = "any"
            End If

        End If
    End Sub
    Sub grdFormDisplay_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdFormDisplay.RowDataBound

        If e.Row.RowType = DataControlRowType.DataRow Then
            If e.Row.DataItem Is Nothing Then Return

            e.Row.Attributes("ondrop") = "drop(event)"
            e.Row.Attributes("ondragover") = "allowDrop(event)"
            e.Row.Attributes("ondragleave") = "dragLeave(event)"

            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            Dim sectionCount = GetTypeLineColCount(rv("disp_type"), rv("disp_line"), rv("disp_col"))

            If rv("disp_line") = 1 Then
                'e.Row.Cells(0).Text = String.Format(ctx.Translate("Column") + " {0}", rv("disp_col"))
                e.Row.Cells(0).Text = String.Format("{0}", rv("disp_col"))
                e.Row.Cells(0).CssClass = "grdField left draggable"
                e.Row.Cells(0).Attributes.Add("draggable", "true")
                e.Row.Cells(0).Attributes.Add("ondragstart", "dragSortStart(event)")
            Else
                e.Row.Cells(0).Text = ""
                e.Row.Cells(0).CssClass = "grdField left"
            End If

            If rv("disp_seq") = 999 Then
                e.Row.Cells(1).Text = ""
                e.Row.Cells(2).Text = ""
                e.Row.CssClass = "bottomSpacer"
                e.Row.FindControl("btnAmend").Visible = False
                e.Row.FindControl("btnDel").Visible = False
            Else
                'e.Row.Cells(1).Text = String.Format(ctx.Translate("Line") + " {0}", rv("disp_line"))
                e.Row.Cells(1).Text = String.Format("{0}", rv("disp_line"))

                e.Row.Cells(0).Attributes.Add("data-source", "column")

                e.Row.Cells(1).CssClass = "grdField left draggable"
                e.Row.Cells(1).Attributes.Add("draggable", "true")
                e.Row.Cells(1).Attributes.Add("ondragstart", "dragSortStart(event)")
                e.Row.Cells(1).Attributes.Add("data-source", "line")

                e.Row.Cells(2).CssClass = "grdField left draggable"
                e.Row.Cells(2).Attributes.Add("draggable", "true")
                e.Row.Cells(2).Attributes.Add("ondragstart", "dragSortStart(event)")
                e.Row.Cells(2).Attributes.Add("data-source", "line")

                If rv("field_num") = 0 Then
                    e.Row.Cells(2).Text = ""
                    e.Row.FindControl("btnAmend").Visible = False
                Else
                    If rv("field_text") <> "" Then
                        e.Row.Cells(2).Text = rv("field_name") + " (" + rv("field_text") + ")"
                    End If
                End If
            End If

            If rv("disp_seq") = 999 Then
                e.Row.Attributes("data-target") = "sort"
            Else
                e.Row.Attributes("data-target") = "any"
            End If

        End If
    End Sub
    Sub grdDisp_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdDispOCA.RowCommand,
                                                                                    grdCoParent.RowCommand,
                                                                                    grdAEAHead.RowCommand,
                                                                                    grdAEAFields.RowCommand,
                                                                                    grdAEASort.RowCommand,
                                                                                    grdFormDisplay.RowCommand,
                                                                                    grdListHeaders.RowCommand,
                                                                                    grdListData.RowCommand,
                                                                                    grdListSort.RowCommand

        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing
        Dim gridArgs As DataKey = Nothing
        Dim dispLine, dispCol, dispSeq As Integer
        Dim dispType As String

        ' get the index of the clicked row
        RowIndex = Int32.Parse(e.CommandArgument.ToString())

        ' get the data keys for the gridrow
        gridArgs = DisplayGrid.DataKeys(RowIndex)

        dispType = gridArgs("disp_type")
        dispLine = gridArgs("disp_line")
        dispCol = gridArgs("disp_col")
        dispSeq = gridArgs("disp_seq")

        DR = DataViewDisplayDT.AsEnumerable().
                            FirstOrDefault(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                                        CInt(r("disp_line")) = dispLine AndAlso
                                                        CInt(r("disp_col")) = dispCol AndAlso
                                                        CInt(r("disp_seq")) = dispSeq)

        Select Case e.CommandName

            Case "AmendRow"

                If dispType = "02" Then Return

                Dim isText As Boolean = DR("field_num") = 0
                'If dispType.ContainsOneOf("01", "03", "04", "06", "07", "08") And Not isText Then Return

                hdnDispKey.Value = String.Format("{0}|{1}|{2}|{3}", DR("disp_type"), DR("disp_col"), DR("disp_line"), DR("disp_seq"))
                txtDEtext.Text = DR("field_text")
                SetViewEntryOptions(DR)
                SetModalState("DataViewEntry")

            Case "DeleteRow"
                ' OCAData      "01"
                ' AEASort      "02"
                ' AEAHeaders   "03"
                ' AEAData      "04"
                ' DetailForm   "05"
                ' ListHeaders  "06"
                ' ListData     "07"
                ' CoParent     "08"
                ' ListData     "09"

                ' for OCA and CoParent, we can just delete the line as there's no consequences
                Select Case hdnAccValue.Value
                    Case "OCA", "CoParent"
                        DR.Delete()
                                'DataViewDisplayDT.AcceptChanges()
                                'ResequenceKeys(dispType)

                    Case "AEAHead"
                        ' if we delete the header, then we delete the entries for the other two

                        DR.Delete()
                        DataViewDisplayDT.AcceptChanges()
                        ResequenceKeys(dispType)

                        ' remove the aea sort
                        removeRows("02", dispLine, dispCol)
                        ResequenceKeys("02")

                        ' remove the aea fields
                        removeRows("04", dispLine, dispCol)
                        ResequenceKeys("04")

                    Case "AEAFields", "ListData", "AEASort", "ListSort"

                        Dim rowCount As Integer = DataViewDisplayDT.AsEnumerable().
                                                                Count(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                                                                  CInt(r("disp_line")) = dispLine AndAlso
                                                                                  CInt(r("disp_col")) = dispCol AndAlso
                                                                                  CInt(r("disp_seq")) <> 999 AndAlso
                                                                                  CInt(r("disp_seq")) <> 0)
                        If rowCount = 1 Then
                            ' replace the last DR with empty 
                            DR("field_num") = 0
                            DR("field_type") = ""
                            DR("field_name") = ""
                            DR("field_text") = ""
                            DR("display_name") = ""
                        Else
                            ' remove the dr
                            DR.Delete()
                        End If
                        ' accept and resequence below

                    Case "ListHead"
                        DR.Delete()
                        DataViewDisplayDT.AcceptChanges()
                        ResequenceKeys(dispType)

                        ' remove the list data row(s)
                        removeRows("07", dispLine, dispCol)
                        ResequenceKeys("07")

                    Case "FormDisplay"
                        Dim FoundItem As Boolean = False
                        Dim rowsToRemove As List(Of DataRow)

                        ' look to see if all the lines for the col are empty
                        Dim colDRs = DataViewDisplayDT.AsEnumerable().
                                                Where(Function(row) CStr(row("disp_type")) = "05" AndAlso
                                                                    CInt(row("disp_col")) = CInt(DR("disp_col"))).ToList()
                        rowsToRemove = New List(Of DataRow)
                        For Each colDR As DataRow In colDRs
                            If colDR("field_num") <> 0 Then
                                FoundItem = True
                                Exit For
                            Else
                                rowsToRemove.Add(colDR)
                            End If
                        Next
                        If Not FoundItem Then
                            ' all lines for the col are empty so delete the col
                            For Each row As DataRow In rowsToRemove
                                DataViewDisplayDT.Rows.Remove(row)
                            Next
                        Else
                            FoundItem = False
                            Dim lineDRs = DataViewDisplayDT.AsEnumerable().
                                                Where(Function(row) CStr(row("disp_type")) = "05" AndAlso
                                                                    CInt(row("disp_line")) = CInt(DR("disp_line"))).ToList()
                            rowsToRemove = New List(Of DataRow)
                            For Each lineDR As DataRow In lineDRs
                                If lineDR("field_num") <> 0 Then
                                    FoundItem = True
                                    Exit For
                                Else
                                    rowsToRemove.Add(lineDR)
                                End If
                            Next
                            If Not FoundItem Then
                                ' all lines across the cols are empty so delete the lines
                                For Each row As DataRow In rowsToRemove
                                    DataViewDisplayDT.Rows.Remove(row)
                                Next
                            Else
                                DR("field_num") = 0
                                DR("field_type") = ""
                                DR("field_name") = ""
                                DR("display_name") = ""
                            End If
                        End If
                End Select
                DataViewDisplayDT.AcceptChanges()
                ResequenceKeys(dispType)

                RebindDisplayGrids()
        End Select
    End Sub

    Private Sub SetViewEntryOptions(DR As DataRow)
        Dim lLeft, lRight, lMid As Boolean
        Dim lFormat As String

        lFormat = DR("field_format").ToString

        ShowHideControl(trDEText, False)
        ShowHideControl(trDEtextHover, False)
        ShowHideControl(trDECase, False)
        ShowHideControl(trDEPart, False)
        ShowHideControl(trDEDateFormat, False)
        ShowHideControl(trDEcurrency, False)
        ShowHideControl(trDEgroup, False)
        ShowHideControl(trDEdecs, False)
        ShowHideControl(trDEbool, False)

        cbDEcurrency.Checked = True     ' if checked then decs and group TRs are auto hidden

        If DR("field_num") = 0 Then
            hdnFieldType.Value = ""
            ShowHideControl(trDEText, True)
            Return
        End If

        ' 03 AEAHeaders  05 DetailForm  06 ListHeaders
        If DR("disp_type").ToString.ContainsOneOf("03", "05", "06") Then
            ShowHideControl(trDEtext, True)
            ShowHideControl(trDEtextHover, True)
        End If

        hdnFieldType.Value = DR("field_type")

        Select Case DR("field_type")
            Case "01"   ' String
                ShowHideControl(trDECase, True)
                ShowHideControl(trDEPart, True)

                lLeft = lFormat.Contains("left")
                lRight = lFormat.Contains("right")
                lMid = lFormat.Contains("mid")

                cbDEUpper.Checked = lFormat.Contains("upper")
                cbDELower.Checked = lFormat.Contains("lower")
                cbDECapitalise.Checked = lFormat.Contains("capitalise")

                cbDEPartLeft.Checked = lLeft
                cbDEPartRight.Checked = lRight
                cbDEPartMid.Checked = lMid

                If lLeft Then
                    If Not GetPartStr(lFormat, "left", 2, txtDELength.Text) Then
                        txtDELength.Text = "100"
                    End If
                End If

                If lRight Then
                    If Not GetPartStr(lFormat, "right", 2, txtDELength.Text) Then
                        txtDELength.Text = "100"
                    End If
                End If

                ShowHideControl(pnlDEStart, lMid)
                If lMid Then
                    If Not GetPartStr(lFormat, "mid", 3, txtDEStart.Text) Then
                        txtDEStart.Text = "1"
                    End If
                    If Not GetPartStr(lFormat, "mid", 2, txtDELength.Text) Then
                        txtDELength.Text = "100"
                    End If
                End If

            Case "02"   ' Date
                ShowHideControl(trDEDateFormat, True)

                If String.IsNullOrEmpty(lFormat.Trim) Then lFormat = "DD/MM/YYYY"
                SetDDLBValue(ddlbDEDateFormat, lFormat)

            Case "03"   ' Group thousand settings
                trDEdecs.Visible = False                ' ctrl won't be rendered!, so js can ignore it.
                ShowHideControl(trDEcurrency, True)
                ShowHideControl(trDEgroup, True)

                cbDEcurrency.Checked = lFormat.Contains("currency")
                If cbDEcurrency.Checked Then
                    cbDEgroup.Checked = False
                Else
                    cbDEgroup.Checked = lFormat.Contains("group")
                End If

            Case "04"   ' Number of decimals
                trDEdecs.Visible = True
                ShowHideControl(trDEcurrency, True)
                ShowHideControl(trDEgroup, True)
                ShowHideControl(trDEdecs, True)

                cbDEcurrency.Checked = lFormat.Contains("currency")
                If cbDEcurrency.Checked Then
                    cbDEgroup.Checked = False
                    txtDEdecs.Text = ""
                Else
                    cbDEgroup.Checked = lFormat.Contains("group")
                    If Not GetPartStr(lFormat, "decimal", 2, txtDEdecs.Text) Then
                        txtDEdecs.Text = "2"
                    End If
                End If

            Case "05"   ' Boolean
                ShowHideControl(trDEbool, True)
                If lFormat = "" Then
                    lFormat = "Yes|No"
                End If
                SetDDLBValue(ddlbDEbool, lFormat)

        End Select

    End Sub

    Sub RebindDisplayGrids()
        Select Case hdnAccValue.Value
            Case "OCA"
                SetGrid(grdDispOCA, DataViewDisplayType("01"), False)
            Case "CoParent"
                SetGrid(grdCoParent, DataViewDisplayType("08"), False)
            Case "AEAHead"
                SetGrid(grdAEAHead, DataViewDisplayType("03"), False)
                SetGrid(grdAEAFields, DataViewDisplayType("04"), False)
                SetGrid(grdAEASort, DataViewDisplayType("02"), False)
            Case "AEAFields"
                SetGrid(grdAEAFields, DataViewDisplayType("04"), False)
            Case "AEASort"
                SetGrid(grdAEASort, DataViewDisplayType("02"), False)
            Case "FormDisplay"
                SetGrid(grdFormDisplay, DataViewDisplayTypeCols("05"), False)
            Case "ListHead"
                SetGrid(grdListHeaders, DataViewDisplayType("06"), False)
                SetGrid(grdListData, DataViewDisplayType("07"), False)
                SetGrid(grdListSort, DataViewDisplayType("08"), False)
            Case "ListData"
                SetGrid(grdListData, DataViewDisplayType("07"), False)
            Case "ListSort"
                SetGrid(grdListSort, DataViewDisplayType("09"), False)

        End Select
        ViewDetailChanged()
    End Sub

    Sub removeRows(dispType As String, dispLine As Integer, dispCol As Integer)
        Dim rowsToDelete As IEnumerable(Of DataRow)

        rowsToDelete = DataViewDisplayDT.AsEnumerable().
                                            Where(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                                              CInt(r("disp_line")) = dispLine AndAlso
                                                              CInt(r("disp_col")) = dispCol)
        For Each Row As DataRow In rowsToDelete.ToList()
            Row.Delete()
        Next
        DataViewDisplayDT.AcceptChanges()
    End Sub

    Sub SeparateKeys(TYP As String)
        ' Function to multiply the values by 10 to create insertable gaps
        For Each row As DataRow In DataViewDisplayDT.Rows
            If CStr(row("disp_type")) = TYP Then
                row("disp_line") = CInt(row("disp_line")) * 10
                row("disp_col") = CInt(row("disp_col")) * 10
                row("disp_seq") = CInt(row("disp_seq")) * 10
            End If
        Next
        DataViewDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceKeys(TYP As String)
        ' resequence based on the type
        Select Case TYP
            Case "01", "08"
                ResequenceLineSeq(TYP)
            Case "02", "03", "04", "06", "07", "09"
                ResequenceColSeq(TYP)
            Case "05"
                ResequenceAll(TYP)
        End Select
        ReSortDisplayDT()

    End Sub
    Sub ResequenceLineSeq(TYP As String)
        ' Function to resequence the values back to starting from 1 for Line and Seq
        Dim groupedData = DataViewDisplayDT.AsEnumerable().
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
        DataViewDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceColSeq(TYP As String)
        ' Function to resequence the values back to starting from 1 for Col and Seq
        Dim groupedData = DataViewDisplayDT.AsEnumerable().
                                            Where(Function(row) CStr(row("disp_type")) = TYP).
                                            OrderBy(Function(row) CInt(row("disp_col"))).
                                            ThenBy(Function(row) CInt(row("disp_seq"))).
                                            GroupBy(Function(row) New With {
                                                Key .Col = CInt(row("disp_col"))
                                            }).ToList()

        Dim newCol As Integer = 1
        For Each group As IGrouping(Of Object, DataRow) In groupedData
            Dim newSeq As Integer = 1
            For Each row As DataRow In group
                If row("disp_seq") = 999 Or row("disp_seq") = 9990 Then
                    If TYP = "03" Or TYP = "06" Then
                        row("disp_col") = 999
                    Else
                        row("disp_col") = newCol
                    End If
                    row("disp_seq") = 999
                Else
                    row("disp_col") = newCol
                    row("disp_seq") = newSeq
                    newSeq += 1
                End If
            Next
            newCol += 1
        Next
        DataViewDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceAll(TYP As String)
        ' Function to resequence the values back to starting from 1 for Line, Col and Seq

        '' Group by Col
        Dim colGroups = DataViewDisplayDT.AsEnumerable().
                                        Where(Function(row) CInt(row("disp_type")) = TYP).
                                        OrderBy(Function(row) CInt(row("disp_col"))).
                                        GroupBy(Function(row) New With {
                                            Key .Col = CInt(row("disp_col"))
                                        }).ToList()

        Dim newCol As Integer = 1
        For Each colGroup As IGrouping(Of Object, DataRow) In colGroups

            ' Now do the lines for each column
            Dim lineGroups = DataViewDisplayDT.AsEnumerable().
                                             Where(Function(row) CInt(row("disp_type")) = TYP AndAlso
                                                                 CInt(row("disp_col")) = colGroup.Key.Col).
                                             OrderBy(Function(row) CInt(row("disp_line"))).
                                             GroupBy(Function(row) New With {
                                                 Key .Line = CInt(row("disp_line"))
                                             }).ToList()

            Dim newLine As Integer = 1
            For Each lineGroup As IGrouping(Of Object, DataRow) In lineGroups
                ' update the line numbers
                If lineGroup.Count() > 0 Then
                    For Each Row As DataRow In lineGroup
                        Row("disp_line") = newLine
                        If Row("disp_Seq") >= 999 Then
                            Row("disp_seq") = 999
                        Else
                            Row("disp_seq") = 1
                        End If
                    Next
                    newLine += 1
                End If
            Next

            ' update the column numbers
            For Each Row As DataRow In colGroup
                Row("disp_col") = newCol
            Next
            newCol += 1
        Next

        DataViewDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceLine(dispType As String, dispLine As Integer)
        ' the line number we are given has been deleted, so we need to move the lower line numbers up by 1

        Dim rowsToChange = DataViewDisplayDT.AsEnumerable().
                                                Where(Function(row) CStr(row("disp_type")) = dispType AndAlso
                                                                    CInt(row("disp_line")) > dispLine)
        For Each row As DataRow In rowsToChange
            row("disp_line") = CInt(row("disp_line")) - 1
        Next
        DataViewDisplayDT.AcceptChanges()
    End Sub
    Sub ResequenceCol(dispType As String, dispCol As Integer)
        ' the line number we are given has been deleted, so we need to move the lower line numbers up by 1

        Dim rowsToChange = DataViewDisplayDT.AsEnumerable().
                                                Where(Function(row) CStr(row("disp_type")) = dispType AndAlso
                                                                    CInt(row("disp_col")) > dispCol)
        For Each row As DataRow In rowsToChange
            row("disp_col") = CInt(row("disp_col")) - 1
        Next
        DataViewDisplayDT.AcceptChanges()
    End Sub

    Sub btnDrop_Click(sender As Object, e As EventArgs) Handles btnDrop.Click
        Dim args As String() = hdnDrop.Value.Split("|"c)
        Dim dragSource As String = args(0)
        Dim rowIndexSource As Integer = Integer.Parse(args(1))
        Dim rowIndexTarget As Integer = Integer.Parse(args(2))
        Dim sourceLine, sourceCol, sourceSeq, sourceField As Integer
        Dim targetLine, targetCol, targetSeq As Integer
        Dim sourceType, targetType As String
        Dim sourceObj As DataKey = Nothing
        Dim targetObj As DataKey = Nothing
        Dim DR As DataRow
        Dim sourceDR As DataRow
        Dim newCol, newLine As Integer

        ' where we land will be the same for every drop
        targetObj = DisplayGrid.DataKeys(rowIndexTarget)

        targetType = targetObj("disp_type")
        targetLine = CInt(targetObj("disp_line")) * 10
        targetCol = CInt(targetObj("disp_col")) * 10
        targetSeq = CInt(targetObj("disp_seq")) * 10

        If dragSource = "field" Then
            ' going from the fields list to the specific grid

            sourceField = grdFields.DataKeys(rowIndexSource)("field_num")
            If targetType.ContainsOneOf("06", "07", "09") Then
                sourceDR = LinkedFieldsDR(CInt(DataViewDR("det_source_id")), sourceField)
            Else
                sourceDR = FieldsDR(sourceField)
            End If

            ' what we do now depends on the type

            ' can't drop a text field onto the AEA sort or form display
            If targetType.ContainsOneOf("02", "05") And sourceField = 0 Then Return

            ' resequence the fields so that there are gaps to drop the data in
            SeparateKeys(targetType)

            DR = DataViewDisplayDT.AsEnumerable().
                                FirstOrDefault(Function(r) CStr(r("disp_type")) = targetType AndAlso
                                                           CInt(r("disp_line")) = targetLine AndAlso
                                                           CInt(r("disp_col")) = targetCol AndAlso
                                                           CInt(r("disp_seq")) = targetSeq)

            Dim UseExisting As Boolean = False

            If targetType.ContainsOneOf("03", "05", "06") Then UseExisting = True

            If targetType.ContainsOneOf("02", "04", "07", "09") And
                        targetSeq <= 10 And
                        DR("field_num") = 0 And
                        DR("field_text") = "" Then UseExisting = True

            If Not UseExisting Then

                ' actually inserting a new row
                DR = DataViewDisplayDT.NewRow()
                SetDisplayDefaults(DR)
                DR("disp_type") = targetType

                If Not targetType.ContainsOneOf("03", "05", "06") Then
                    If targetSeq = 9990 Then
                        targetSeq = 9980
                    Else
                        targetSeq = targetSeq - 5
                    End If
                End If
                DR("disp_seq") = targetSeq

                Select Case targetType
                    Case "01",  ' OCA
                         "08"   ' CoParent
                        DR("disp_line") = targetLine

                    Case "02",  ' AEA Sort
                         "03",  ' AEA Header
                         "04",  ' AEA Data
                         "06",  ' List Header
                         "07",  ' List Data
                         "09"   ' List Sort
                        DR("disp_col") = targetCol

                    Case "05"   ' Display Form
                        DR("disp_line") = targetLine
                        DR("disp_col") = targetCol

                End Select

            End If

            DR("field_num") = sourceField
            DR("field_type") = sourceDR("field_type")
            DR("field_name") = sourceDR("field_name")

            If sourceField = 0 Then
                DR("display_name") = """ """
                DR("field_text") = " "
            Else
                DR("display_name") = sourceDR("field_name")
            End If

            If Not UseExisting Then
                DataViewDisplayDT.Rows.Add(DR)
            End If

        Else
            ' target grid is same as source grid in this case as we're just re-ordering
            sourceObj = DisplayGrid.DataKeys(rowIndexSource)
            sourceType = sourceObj("disp_type")
            sourceLine = sourceObj("disp_line") * 10
            sourceCol = sourceObj("disp_col") * 10
            sourceSeq = sourceObj("disp_seq") * 10

            If sourceType = "05" And sourceCol <> targetCol And targetSeq = 9990 Then Return

            SeparateKeys(targetType)

            sourceDR = DataViewDisplayDT.AsEnumerable().
                                        FirstOrDefault(Function(r) CStr(r("disp_type")) = sourceType AndAlso
                                                                    CInt(r("disp_line")) = sourceLine AndAlso
                                                                    CInt(r("disp_col")) = sourceCol AndAlso
                                                                    CInt(r("disp_seq")) = sourceSeq)

            ' what happens now depends on the type of data, and also which column is being dragged as you can 
            ' re-order by the column, the row, or even the seq grid column

            If dragSource = "seq" Then
                Select Case targetType
                    Case "03"
                        sourceDR("disp_col") = targetCol - 5

                    Case Else
                        ' moving a row within a specific grid
                        sourceDR("disp_line") = targetLine
                        sourceDR("disp_col") = targetCol

                        If targetSeq = 9990 Then
                            sourceDR("disp_seq") = 9985
                        Else
                            sourceDR("disp_seq") = targetSeq - 5
                        End If

                End Select

            Else

                Select Case targetType
                    Case "01",  ' OCA
                         "08"   ' CoParent

                        ' We are moving a column, so we have to set the column numbers for all moving rows
                        ' to 5 less than the target column number
                        newLine = targetLine - 5
                        Dim lineDRs = DataViewDisplayDT.AsEnumerable().
                                             Where(Function(row) CStr(row("disp_type")) = targetType AndAlso
                                                                 CInt(row("disp_line")) = sourceLine).ToList()
                        For Each colDR As DataRow In lineDRs
                            colDR("disp_line") = newLine
                        Next

                    Case "02",  ' AEA Sort
                         "03",  ' AEA Header
                         "04",  ' AEA Data
                         "06",  ' List Header
                         "07"   ' List Data

                        newCol = targetCol - 5
                        Dim colDRs = DataViewDisplayDT.AsEnumerable().
                                             Where(Function(row) CStr(row("disp_type")) = targetType AndAlso
                                                                 CInt(row("disp_col")) = sourceCol).ToList()
                        For Each colDR As DataRow In colDRs
                            colDR("disp_col") = newCol
                        Next

                    Case "05"   ' Display Form
                        Select Case dragSource
                            Case "column"
                                ' We are moving a column, so we have to set the column numbers for all moving rows
                                ' to 5 less than the target column number
                                newCol = targetCol - 5
                                Dim colDRs = DataViewDisplayDT.AsEnumerable().
                                                     Where(Function(row) CStr(row("disp_type")) = targetType AndAlso
                                                                         CInt(row("disp_col")) = sourceCol).ToList()
                                For Each colDR As DataRow In colDRs
                                    colDR("disp_col") = newCol
                                Next

                            Case "line"
                                If targetCol = sourceCol Then
                                    sourceDR("disp_line") = targetLine - 5
                                Else
                                    Dim targetDR = DataViewDisplayDT.AsEnumerable().
                                                                   FirstOrDefault(Function(row) CStr(row("disp_type")) = targetType AndAlso
                                                                                                CInt(row("disp_col")) = targetCol AndAlso
                                                                                                CInt(row("disp_line")) = targetLine)
                                    targetDR("field_num") = sourceDR("field_num")
                                    targetDR("field_type") = sourceDR("field_type")
                                    targetDR("field_name") = sourceDR("field_name")
                                    targetDR("display_name") = sourceDR("display_name")

                                    sourceDR("field_num") = 0
                                    sourceDR("field_type") = ""
                                    sourceDR("field_name") = ""
                                    sourceDR("display_name") = ""

                                End If

                        End Select

                End Select
            End If

            DataViewDisplayDT.AcceptChanges()
        End If

        ResequenceKeys(targetType)

        If targetType = "05" Then
            SetGrid(DisplayGrid, DataViewDisplayTypeCols(targetType), False)
        Else
            SetGrid(DisplayGrid, DataViewDisplayType(targetType), False)
        End If

        ViewDetailChanged()

    End Sub
    Sub btnAccSave_Click(sender As Object, e As EventArgs) Handles btnAccSave.Click
        SQL = "INSERT dbo.data_view_display
                      (view_id, disp_type, disp_col, disp_line, disp_seq, field_num, field_text, field_format, field_align) 
               VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)"

        Using DB As New IawDB
            DB.TranBegin()
            DB.NonQuery("delete from dbo.data_view_display where view_id = @p1", ViewID)

            For Each rowDR As DataRow In DataViewDisplayDT.AsEnumerable().
                                                            Where(Function(row) CInt(row("disp_seq")) <> 999 AndAlso
                                                                                CInt(row("disp_seq")) <> 0)
                If rowDR("field_num") = 0 And rowDR("field_text") = "" Then Continue For

                DB.NonQuery(SQL, ViewID,
                            rowDR("disp_type"),
                            rowDR("disp_col"),
                            rowDR("disp_line"),
                            rowDR("disp_seq"),
                            rowDR("field_num"),
                            rowDR("field_text"),
                            rowDR("field_format"),
                            rowDR("field_align"))
            Next

            DB.TranCommit()
            ViewDetailUnchanged()

            ' build the source_set_values table for the 'new' datasource
            Dim BuildViewVals As New BuildViewValues
            BuildViewVals.BuildDataViewValues(ViewID)

            RefreshGrid("DataDetails")
        End Using

    End Sub
    Sub btnAccCancel_Click(sender As Object, e As EventArgs) Handles btnAccCancel.Click
        ViewDetailUnchanged()
        RefreshGrid("DataDetails")
    End Sub
    Sub btnAccPanel_Click(sender As Object, e As EventArgs) Handles btnAccPanel.Click

        ' have to set the index for the accordian control else it is lost on the postback
        ' 0 OCA
        ' 1 Co Parent
        ' 2 AEA Header
        ' 3 AEA Display Data
        ' 4 AEA Sort
        ' 5 Form Display
        ' 5 Linked List Header
        ' 6 Linked List Data
        ' 7 Linked List Sort

        Dim PrimaryFields As Boolean = True

        Select Case hdnAccValue.Value
            Case "OCA"
                accPanel.SelectedIndex = 0
            Case "CoParent"
                accPanel.SelectedIndex = 1
            Case "AEAHead"
                accPanel.SelectedIndex = 2
            Case "AEAFields"
                accPanel.SelectedIndex = 3
            Case "AEASort"
                accPanel.SelectedIndex = 4
            Case "FormDisplay"
                accPanel.SelectedIndex = 5
            Case "ListHead"
                accPanel.SelectedIndex = 5
                PrimaryFields = False
            Case "ListData"
                accPanel.SelectedIndex = 6
                PrimaryFields = False
            Case "ListSort"
                accPanel.SelectedIndex = 7
                PrimaryFields = False
        End Select

        If PrimaryFields Then
            SetGrid(grdFields, CleanFieldsDT, False)
        Else
            Dim DR As DataRow = DataViewDR
            Dim LinkedDS As Integer = DR("det_source_id")
            SetGrid(grdFields, LinkedFieldsDT(LinkedDS, 1), False)
        End If
        FieldNum = grdFields.SelectedValue

    End Sub
    Sub btnAEAHeader_Click(sender As Object, e As EventArgs) Handles btnAEAHeader.Click
        Dim DR As DataRow

        Dim maxCol As Integer = CInt(FindLastCol(DataViewDisplayDT, "03")) + 1

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "03"
        DR("disp_col") = maxCol
        DR("disp_seq") = 1
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "02"
        DR("disp_col") = maxCol
        DR("disp_seq") = 1
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "02"
        DR("disp_col") = maxCol
        DR("disp_seq") = 999
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "04"
        DR("disp_col") = maxCol
        DR("disp_seq") = 1
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "04"
        DR("disp_col") = maxCol
        DR("disp_seq") = 999
        DataViewDisplayDT.Rows.Add(DR)

        DataViewDisplayDT.AcceptChanges()

        SetGrid(grdAEAHead, DataViewDisplayType("03"), False)
        SetGrid(grdAEAFields, DataViewDisplayType("04"), False)
        SetGrid(grdAEASort, DataViewDisplayType("02"), False)

        ViewDetailChanged()

    End Sub
    Sub btnFormDispRow_Click(sender As Object, e As EventArgs) Handles btnFormDispRow.Click
        Dim DR As DataRow

        Dim maxCol As Integer = CInt(FindLastCol(DataViewDisplayDT, "05")) * 10
        Dim maxRow As Integer = CInt(FindLastLine(DataViewDisplayDT, "05")) * 10 + 10

        SeparateKeys("05")

        For i As Integer = 10 To maxCol Step 10
            DR = DataViewDisplayDT.NewRow()
            SetDisplayDefaults(DR)
            DR("disp_type") = "05"
            DR("disp_line") = maxRow
            DR("disp_col") = i
            DR("disp_seq") = 1
            DataViewDisplayDT.Rows.Add(DR)
        Next
        DataViewDisplayDT.AcceptChanges()

        ResequenceKeys("05")

        SetGrid(grdFormDisplay, DataViewDisplayTypeCols("05"), False)

        ViewDetailChanged()
    End Sub
    Sub btnFormDispCol_Click(sender As Object, e As EventArgs) Handles btnFormDispCol.Click
        Dim DR As DataRow

        Dim maxCol As Integer = CInt(FindLastCol(DataViewDisplayDT, "05")) * 10 + 10
        Dim maxRow As Integer = CInt(FindLastLine(DataViewDisplayDT, "05")) * 10

        SeparateKeys("05")

        For i As Integer = 10 To maxRow Step 10
            DR = DataViewDisplayDT.NewRow()
            SetDisplayDefaults(DR)
            DR("disp_type") = "05"
            DR("disp_line") = i
            DR("disp_col") = maxCol
            DR("disp_seq") = 1
            DataViewDisplayDT.Rows.Add(DR)
        Next
        DataViewDisplayDT.AcceptChanges()

        ResequenceKeys("05")

        SetGrid(grdFormDisplay, DataViewDisplayTypeCols("05"), False)

        ViewDetailChanged()
    End Sub
    Sub btnListHead_Click(sender As Object, e As EventArgs) Handles btnListHead.Click
        Dim DR As DataRow
        Dim maxCol As Integer = CInt(FindLastCol(DataViewDisplayDT, "06")) + 1

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "06"
        DR("disp_col") = maxCol
        DR("disp_seq") = 1
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "07"
        DR("disp_col") = maxCol
        DR("disp_seq") = 1
        DataViewDisplayDT.Rows.Add(DR)

        DR = DataViewDisplayDT.NewRow()
        SetDisplayDefaults(DR)
        DR("disp_type") = "07"
        DR("disp_col") = maxCol
        DR("disp_seq") = 999
        DataViewDisplayDT.Rows.Add(DR)

        DataViewDisplayDT.AcceptChanges()

        SetGrid(grdListHeaders, DataViewDisplayType("06"), False)
        SetGrid(grdListData, DataViewDisplayType("07"), False)

        ViewDetailChanged()
    End Sub

    Function FindLastCol(DT As DataTable, TYP As String) As Integer
        Return DT.AsEnumerable().Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                             CInt(row("disp_seq")) <> 999).
                                         Select(Function(row) CInt(row("disp_col"))).
                                         DefaultIfEmpty(0).Max()
    End Function
    Function FindLastLine(DT As DataTable, TYP As String) As Integer
        Return DT.AsEnumerable().Where(Function(row) CStr(row("disp_type")) = TYP AndAlso
                                                             CInt(row("disp_seq")) <> 999).
                                         Select(Function(row) CInt(row("disp_line"))).
                                         DefaultIfEmpty(0).Max()
    End Function

#End Region

#Region "View Display Dialog"

    Private Sub btnDisplayEntryCancel_Click(sender As Object, e As EventArgs) Handles btnDisplayEntryCancel.Click
        SetModalState("")
    End Sub
    Private Sub btnDisplayEntrySave_Click(sender As Object, e As EventArgs) Handles btnDisplayEntrySave.Click
        'If Not Page.IsValid Then Return

        Dim DR As DataRow

        Dim dispType As String = hdnDispKey.Value.Split("|")(0)
        Dim dispCol As Integer = CInt(hdnDispKey.Value.Split("|")(1))
        Dim dispLine As Integer = CInt(hdnDispKey.Value.Split("|")(2))
        Dim dispSeq As Integer = CInt(hdnDispKey.Value.Split("|")(3))

        DR = DataViewDisplayDT.AsEnumerable().
                FirstOrDefault(Function(r) CStr(r("disp_type")) = dispType AndAlso
                                            CInt(r("disp_col")) = dispCol AndAlso
                                            CInt(r("disp_line")) = dispLine AndAlso
                                            CInt(r("disp_seq")) = dispSeq)

        If DR("field_num") = 0 Then

            If txtDEtext.Text.Trim = "" Then
                txtDEtext.Text = " "
            End If
            DR("display_name") = """" + txtDEtext.Text + """"
            DR("field_text") = txtDEtext.Text

        Else

            If txtDEtext.Text.Trim = "" Then
                DR("display_name") = DR("field_name")
                DR("field_text") = ""
            Else
                DR("display_name") = """" + txtDEtext.Text + """"
                DR("field_text") = txtDEtext.Text
            End If

            Dim lFormat As New List(Of String)
            Select Case DR("field_type")
                Case "01" ' string
                    If cbDELower.Checked Then lFormat.Add("lower")
                    If cbDEUpper.Checked Then lFormat.Add("upper")
                    If cbDECapitalise.Checked Then lFormat.Add("capitalise")
                    If cbDEPartLeft.Checked Then lFormat.Add(String.Format("left^{0}", txtDELength.Text))
                    If cbDEPartRight.Checked Then lFormat.Add(String.Format("right^{0}", txtDELength.Text))
                    If cbDEPartMid.Checked Then lFormat.Add(String.Format("mid^{0}^{1}", txtDEStart.Text, txtDELength.Text))

                Case "02" ' Date
                    lFormat.Add(ddlbDEDateFormat.SelectedValue)

                Case "03" ' Integer
                    If cbDEcurrency.Checked Then lFormat.Add("currency")
                    If cbDEgroup.Checked Then lFormat.Add("group")

                Case "04" ' Float
                    If cbDEcurrency.Checked Then lFormat.Add("currency")
                    If cbDEgroup.Checked Then lFormat.Add("group")
                    If txtDEdecs.Text = "" Then txtDEdecs.Text = 0
                    lFormat.Add(String.Format("decimal^{0}", txtDEdecs.Text))

                Case "05" ' Boolean
                    lFormat.Add(ddlbDEbool.SelectedValue)

            End Select
            If lFormat.Count > 0 Then
                DR("field_format") = String.Join("|", lFormat.ToArray)
            End If
        End If

        DataViewDisplayDT.AcceptChanges()

        RebindDisplayGrids()

        SetModalState("")
    End Sub
#End Region

#Region "Utils"
    Sub ShowHideControl(ctrl As HtmlControl, showControl As Boolean)
        ctrl.Attributes("style") = If(showControl, "display: block", "display: none")
    End Sub
    Sub ShowHideControl(ctrl As WebControl, showControl As Boolean)
        ctrl.Attributes("style") = If(showControl, "display: block", "display: none")
    End Sub
    Sub ShowHideControl(ctrl As HtmlTableRow, showcontrol As Boolean)
        ctrl.Attributes("style") = If(showcontrol, "display: table-row", "display: none")
    End Sub

    Sub ViewDetailChanged()
        StopPanelClick(pnlDataSource)
        StopPanelClick(pnlDataView)
        DisableMenu()
        divAccButtons.Visible = True
    End Sub

    Sub ViewDetailUnchanged()
        AllowPanelClick(pnlDataSource)
        AllowPanelClick(pnlDataView)
        EnableMenu()
        divAccButtons.Visible = False
    End Sub

    Sub StopPanelClick(pnl As IAWPanel)
        pnl.CssClass = "PanelClass noClick"
    End Sub

    Sub AllowPanelClick(pnl As IAWPanel)
        pnl.CssClass = "PanelClass"
    End Sub

    Sub DisableMenu()
        Dim menu As Menu = DirectCast(Page.Master.FindControl("Menu1"), Menu)
        If menu IsNot Nothing Then
            menu.CssClass = "MBItem noClick"
        End If
    End Sub
    Sub EnableMenu()
        Dim menu As Menu = DirectCast(Page.Master.FindControl("Menu1"), Menu)
        If menu IsNot Nothing Then
            menu.CssClass = "MBItem"
        End If
    End Sub

    Private Function GetPartStr(str As String, Prefix As String, N As Integer, ByRef Res As String) As Boolean
        For Each s As String In str.Split("|")
            If s.StartsWith(Prefix) Then
                Return GetPart(s, "^", N, Res)
            End If
        Next
        Return False
    End Function

    Private Function GetPartInt(str As String, Prefix As String, N As Integer, ByRef Res As Integer) As Boolean
        Dim lRes As String = ""
        For Each s As String In str.Split("|")
            If s.StartsWith(Prefix) Then
                If GetPart(s, "^", N, lRes) Then
                    Res = CInt(lRes)
                    Return True
                Else
                    Return False
                End If
            End If
        Next
        Return False
    End Function

    Private Function GetPart(str As String, Delim As String, N As Integer, ByRef Res As String) As Boolean
        If str.Trim = "" Then Return False
        If N < 1 Then Return False
        If str.Split(Delim).Length < N Then Return False
        Res = str.Split(Delim)(N - 1)
        Return True
    End Function
#End Region

End Class