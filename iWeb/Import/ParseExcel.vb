Imports System.IO
Imports NPOI.HSSF.UserModel
Imports NPOI.SS.UserModel

Public Class ParseExcel

    Dim SQL As String

    Public Proc_Imp As ProcessImport

    Public Sub New(_Proc_Imp As ProcessImport)
        Proc_Imp = _Proc_Imp
    End Sub

    Public Function ExcelProcessSegment(ByVal PassSegmentID As Integer) As DataTable

        Dim datRow As DataRow
        Dim datTable As DataTable
        Dim ImportID As Integer
        Dim ClientID As String
        Dim SegmentID As Integer
        Dim SegmentStatus As String
        Dim ShortRef As String
        Dim SourceDate As DateTime
        Dim PrevSourceDate As DateTime
        Dim SourceID As Integer
        Dim ImportField As Byte()
        Dim ImportStream As MemoryStream
        Dim SQLResult As Long
        Dim ExcelDataTable As New DataTable
        Dim ExcelSheetCols As Dictionary(Of Integer, colDef)
        Dim DBSheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' Get next row to be processed
        Using DB As New IawDB

            SQL = "SELECT CI.unique_id, CI.client_id, CI.import_data,
                          CIS.seg_status, CIS.seg_id, CIS.prev_source_date, CIS.source_date, CIS.short_ref,
                          IsNull(CIS.source_id,-1) as source_id
                     FROM dbo.client_import CI
                          JOIN dbo.client_import_seg CIS
                            ON CI.unique_id = CIS.unique_id
                    WHERE CI.import_type IN ('01','02')
                      AND CIS.seg_status IN ('01','04')
                      AND CIS.seg_id = @p1
                    ORDER BY CI.batch_ident ASC, CI.unique_id ASC, CIS.seg_id ASC"
            datTable = DB.GetTable(SQL, PassSegmentID)
            If datTable IsNot Nothing Then
                For Each datRow In datTable.Rows

                    ' If the datasource already exists, then the source_id and the previous_source_date
                    ' will be populated else they will not be set (SQL will return -1 in the source_id)

                    Proc_Imp.ErrorStatus = False
                    ImportID = datRow.GetInt("unique_id")
                    ClientID = datRow.GetValue("client_id", "")
                    SegmentID = datRow.GetInt("seg_id")
                    ShortRef = datRow.GetValue("short_ref", "")
                    SourceDate = datRow.GetDate("source_date")
                    SourceID = datRow.GetInt("source_id")
                    PrevSourceDate = datRow.GetDate("prev_source_date")
                    SegmentStatus = datRow.GetValue("seg_status", "")
                    ImportField = datRow.GetBytes("import_data")

                    ' Get the Max Sizes from the clients table
                    Proc_Imp.GetSizes(ClientID)

                    ImportStream = New MemoryStream(ImportField)

                    Select Case SegmentStatus
                        Case "01" ' Unprocessed - Evaluate the datatypes of the spreadsheet

                            If ImportID <> 0 And ClientID IsNot "" And ImportStream IsNot Nothing Then

                                ' Set Status to processing
                                SQL = "UPDATE client_import_seg
                                          SET seg_status = '02' 
                                        WHERE unique_id = @p1
                                          AND seg_id = @p2"
                                Try
                                    SQLResult = DB.NonQuery(SQL, ImportID, SegmentID)
                                Catch ex As Exception
                                    Proc_Imp.ErrorStatus = True
                                    ' Unable to set import status
                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, ex, "")
                                End Try

                                If Proc_Imp.ErrorStatus = False Then
                                    ' Update Datasource. Remember SourceID in case required later
                                    If SourceID > 0 Then
                                        ' Get column info for existing source from DB in order to compare against columns from new spreadsheet
                                        DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, PrevSourceDate, SegmentID)
                                    End If
                                End If

                                If Proc_Imp.ErrorStatus = False Then
                                    ' Get the column specification from the incoming spreadsheet
                                    ExcelSheetCols = ExcelGetColumnInfo(ImportStream, ShortRef, SegmentID)

                                    If ExcelSheetCols IsNot Nothing And Proc_Imp.ErrorStatus = False Then
                                        ' Now import the data into a datatable to run further checks
                                        'ImportStream.Position = 0
                                        ImportStream = New MemoryStream(ImportField)

                                        ExcelDataTable = ExcelReadData(ImportStream, 0, ShortRef, SegmentID, SourceDate)

                                        If Proc_Imp.ErrorStatus = False Then

                                            ' Set columns to unique if data is unique 
                                            For Each CD As colDef In ExcelSheetCols.Values

                                                If CD.coltype = "String" Or (CD.coltype = "Number" And CD.collen = 0) Then
                                                    If Proc_Imp.isUnique(ExcelDataTable, CD.colname) Then
                                                        CD.colunique = "1"
                                                    Else
                                                        CD.colunique = "0"
                                                    End If
                                                Else
                                                    CD.colunique = "0"
                                                End If
                                            Next

                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Update Datasource. Remember previous sourceID in case required later
                                                If SourceID > 0 Then
                                                    ' Compare DB and Excel Column specifications
                                                    SegmentStatus = Proc_Imp.CompareColumnSpecsVariable(SegmentID, DBSheetCols, ExcelSheetCols)
                                                Else
                                                    ' New source Assumed
                                                    SegmentStatus = "03" ' Awaiting Approval
                                                End If
                                            End If
                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Create a new source from this
                                                If SourceID > 0 Then
                                                    ' Check and change the column specifications order, incase it has changed
                                                    If Proc_Imp.ChangeColumnSpecsOrder(SegmentID, DBSheetCols, ExcelSheetCols) = 99 Then
                                                        ' Unable to evaluate column specification from XML
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, "")
                                                    End If
                                                End If

                                                If Proc_Imp.ErrorStatus = False Then

                                                    SourceID = Proc_Imp.CreateSourceFromSpec(ClientID, SourceID, ExcelSheetCols, ShortRef, SegmentID, SourceDate, SegmentStatus)

                                                    If SourceID = 0 Then
                                                        ' Unable to create new Datasource from Excel
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM005", 0, 0, "")
                                                    End If
                                                End If

                                            End If

                                        Else
                                            ' Unable to evaluate column specification from spreadsheet
                                            Proc_Imp.ErrorStatus = True
                                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM006", 0, 0, "")
                                        End If
                                    End If

                                End If


                            Else
                                ' Invalid Client ID or empty file
                                Proc_Imp.ErrorStatus = True
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM007", "ER_IM007", 0, 0, "")
                            End If

                            If Proc_Imp.ErrorStatus = True Then
                                SegmentStatus = "99" 'Error
                            End If

                            SQL = "UPDATE client_import_seg
                                      SET seg_status = @p1,
                                          source_id = @p2
                                    WHERE unique_id = @p3
                                          AND seg_id = @p4"
                            Try
                                SQLResult = DB.NonQuery(SQL, SegmentStatus, SourceID, ImportID, SegmentID)
                            Catch ex As Exception
                                ' Unable to set import status
                                Proc_Imp.ErrorStatus = True
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, ex, "")
                            End Try

                    End Select

                    Select Case SegmentStatus
                        Case "04" ' Import Data - Import the Data from the spreadsheet.

                            If SourceID > 0 Then

                                ImportStream = New MemoryStream(ImportField)

                                ' Read the Data from spreadsheet.
                                ExcelDataTable = ExcelReadData(ImportStream, SourceID, ShortRef, SegmentID, SourceDate)

                                ' Get column info for existing source from DB
                                DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, SourceDate, SegmentID)

                                ' Create New Table and Insert the Data from the Spreadsheet.
                                Proc_Imp.CreateTableInsertData(SourceID, SegmentID, DBSheetCols, ExcelDataTable, SourceDate, PrevSourceDate)

                                If Proc_Imp.ErrorStatus = False Then
                                    If PrevSourceDate.Ticks > 0 Then
                                        ' Copy Model, etc from previous source to New one
                                        Proc_Imp.CopySourceTables(SourceID, SourceDate, PrevSourceDate, SegmentID)
                                        If Proc_Imp.ErrorStatus = True Then
                                            SegmentStatus = "99" 'Error
                                        End If

                                    End If
                                Else
                                    SegmentStatus = "99" 'Error
                                End If

                            Else
                                ' Unable to match Datasource
                                Proc_Imp.ErrorStatus = True
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM009", 0, 0, "")
                                SegmentStatus = "99" 'Error
                            End If

                            If Proc_Imp.ErrorStatus = False Then
                                SegmentStatus = "10" ' Processed
                            End If

                            SQL = "UPDATE client_import_seg
                                      SET seg_status = @p1
                                    WHERE unique_id = @p2
                                      AND seg_id = @p3"
                            Try
                                SQLResult = DB.NonQuery(SQL, SegmentStatus, ImportID, SegmentID)
                            Catch ex As Exception
                                ' Unable to set import status
                                Proc_Imp.ErrorStatus = True
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, ex, "")
                                'Finally
                                '   DB.Close()
                            End Try

                    End Select

                Next
            End If
            DB.Close()
        End Using

        Return ExcelDataTable

    End Function

    Public Function ExcelReadData(ByVal ContentStream As MemoryStream, ByVal sourceID As String, ByVal ShortRef As String, ByVal SegmentID As String, ByVal SourceDate As Date) As DataTable
        Dim datTable As DataTable = Nothing
        Dim datRow As DataRow

        Dim workbook As IWorkbook
        Dim worksheet As ISheet
        Dim sheetCols As Dictionary(Of Integer, colDef)
        Dim sheetRow As IRow
        Dim sheetCell As NPOI.SS.UserModel.ICell
        Dim ImportHeadings As String() = Nothing

        Dim colIndex As Integer
        Dim colValue As Object = Nothing
        Dim colSpecIndex As Integer
        Dim foundColumn As Integer
        Dim headingRowFound As Boolean = False

        ' a spreasheet can have multiple tabs, so we could return a dataset that can contain multiple datatables
        ' For now ignore anything but the passed in Work Sheet Index.
        ' we assume that every sheet has a headings row

        Try
            workbook = WorkbookFactory.Create(ContentStream)

            For worksheetIndex As Integer = 0 To workbook.NumberOfSheets - 1

                worksheet = workbook.GetSheetAt(worksheetIndex)

                ' Ignore anything but the passed in Work Sheet Reference (SheetName).
                If worksheet.SheetName <> ShortRef Then
                    Continue For
                End If

                If sourceID > 0 Then
                    ' If source set get data from the DB.
                    sheetCols = Proc_Imp.DBGetColumnInfo(sourceID, SourceDate, SegmentID)
                Else
                    ' Still at evaluation stage Get the column details from the worksheet.
                    sheetCols = ExcelEvaluateDataTypes(worksheet, SegmentID)
                End If

                ' Now we should have a good idea of what the structure of the data should be like..
                ' so now we can create the datatable and insert the data

                datTable = New DataTable(worksheet.SheetName)   ' may need to fix the sheetname

                ' need to fix the datatypes for numbers 
                For Each CD As colDef In sheetCols.Values
                    Select Case CD.coltype
                        Case "String", "Boolean", "DateTime"
                            datTable.Columns.Add(New DataColumn(CD.colname, Type.GetType("System." + CD.coltype)))
                        Case "Number"
                            If CD.collen = 0 Then
                                datTable.Columns.Add(New DataColumn(CD.colname, Type.GetType("System.Int32")))
                            Else
                                datTable.Columns.Add(New DataColumn(CD.colname, Type.GetType("System.Decimal")))
                            End If
                    End Select
                Next

                ' now add the data into the table
                For rowIndex As Integer = 0 To worksheet.LastRowNum - 1
                    sheetRow = worksheet.GetRow(rowIndex)
                    ' Ignore empty rows
                    If sheetRow Is Nothing Then Continue For

                    ' Get headings
                    If rowIndex = 0 Or headingRowFound = False Then
                        headingRowFound = True
                        For colIndex = 0 To sheetRow.LastCellNum - 1
                            sheetCell = sheetRow.GetCell(colIndex, MissingCellPolicy.RETURN_NULL_AND_BLANK)
                            ReDim Preserve ImportHeadings(colIndex)
                            Try
                                ImportHeadings(colIndex) = sheetCell.StringCellValue
                            Catch
                                ' Missing Column Name - Error
                                Proc_Imp.ErrorStatus = True
                                ' Missing column heading in import file
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM012", "ER_IM012", rowIndex + 1, colIndex + 1, "")
                            End Try
                            ImportHeadings(colIndex) = ImportHeadings(colIndex).Replace(" ", "_").ToLower
                        Next
                        Continue For
                    End If

                    datRow = datTable.NewRow()
                    For colSpecIndex = 0 To sheetCols.Count - 1
                        ' ColumnSpec(colSpecIndex).colorg contains the original column for matching
                        foundColumn = 999 ' No Match 
                        ' Find the corresponding column in the data
                        For colIndex = 0 To ImportHeadings.Count - 1
                            If ImportHeadings(colIndex) = sheetCols(colSpecIndex).colorg Then
                                foundColumn = 1 ' Match 
                                ' Get the Cell of data. sheetCell = ImportCell
                                sheetCell = sheetRow.GetCell(colIndex, MissingCellPolicy.RETURN_NULL_AND_BLANK)

                                If sheetCell Is Nothing Then
                                    colValue = DBNull.Value
                                Else
                                    colValue = DBNull.Value
                                    Select Case sheetCols(colSpecIndex).coltype
                                        Case "Boolean"
                                            Select Case sheetCell.CellType
                                                Case CellType.Blank
                                                Case CellType.Boolean
                                                    colValue = sheetCell.BooleanCellValue
                                                Case CellType.String
                                                    If sheetCell.StringCellValue = "1" Then
                                                        colValue = True
                                                    ElseIf sheetCell.StringCellValue = "0" Then
                                                        colValue = False
                                                    End If
                                                Case CellType.Numeric
                                                    If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                        colValue = DBNull.Value
                                                    Else
                                                        If sheetCell.NumericCellValue = 1 Then
                                                            colValue = True
                                                        ElseIf sheetCell.NumericCellValue = 0 Then
                                                            colValue = False
                                                        End If
                                                    End If
                                                Case CellType.Formula
                                                    Select Case sheetCell.CachedFormulaResultType
                                                        Case CellType.Boolean
                                                            colValue = sheetCell.BooleanCellValue
                                                        Case CellType.String
                                                            If sheetCell.StringCellValue = "1" Or sheetCell.StringCellValue.ToLower = "yes" Or sheetCell.StringCellValue.ToLower = "True" Then
                                                                colValue = True
                                                            ElseIf sheetCell.StringCellValue = "0" Or sheetCell.StringCellValue.ToLower = "no" Or sheetCell.StringCellValue.ToLower = "false" Then
                                                                colValue = False
                                                            End If
                                                        Case CellType.Numeric
                                                            If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                                colValue = DBNull.Value
                                                            Else
                                                                Try
                                                                    If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                                                                        colValue = sheetCell.BooleanCellValue
                                                                    Else
                                                                        If sheetCell.NumericCellValue = 1 Then
                                                                            colValue = True
                                                                        ElseIf sheetCell.NumericCellValue = 0 Then
                                                                            colValue = False
                                                                        End If
                                                                    End If
                                                                Catch
                                                                    colValue = DBNull.Value
                                                                End Try
                                                            End If
                                                    End Select
                                                Case Else
                                                    colValue = DBNull.Value
                                            End Select

                                    'colValue = sheetCell.BooleanCellValue
                                        Case "String"
                                            Select Case sheetCell.CellType
                                                Case CellType.Blank
                                                Case CellType.Boolean
                                                    colValue = sheetCell.BooleanCellValue.ToString
                                                Case CellType.String
                                                    colValue = sheetCell.StringCellValue
                                                Case CellType.Numeric
                                                    If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                        colValue = sheetCell.DateCellValue.ToString
                                                    Else
                                                        colValue = sheetCell.NumericCellValue.ToString
                                                    End If
                                                Case CellType.Formula
                                                    Select Case sheetCell.CachedFormulaResultType
                                                        Case CellType.Boolean
                                                            colValue = sheetCell.BooleanCellValue.ToString
                                                        Case CellType.String
                                                            colValue = sheetCell.StringCellValue
                                                        Case CellType.Numeric
                                                            If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                                colValue = sheetCell.DateCellValue.ToString
                                                            Else
                                                                Try
                                                                    If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                                                                        colValue = sheetCell.BooleanCellValue.ToString
                                                                    Else
                                                                        colValue = sheetCell.NumericCellValue.ToString
                                                                    End If
                                                                Catch
                                                                    colValue = sheetCell.StringCellValue.ToString
                                                                End Try
                                                            End If
                                                    End Select
                                                Case Else
                                                    colValue = DBNull.Value
                                            End Select

                                    'colValue = sheetCell.StringCellValue
                                        Case "DateTime"
                                            Select Case sheetCell.CellType
                                                Case CellType.Blank
                                                Case CellType.Boolean
                                                Case CellType.String
                                                    If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                        colValue = sheetCell.DateCellValue
                                                    End If
                                                Case CellType.Numeric
                                                    If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                        colValue = sheetCell.DateCellValue
                                                    End If
                                                Case CellType.Formula
                                                    Select Case sheetCell.CachedFormulaResultType
                                                        Case CellType.Boolean
                                                        Case CellType.String
                                                            If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                                colValue = sheetCell.DateCellValue
                                                            End If
                                                        Case CellType.Numeric
                                                            If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                                colValue = sheetCell.DateCellValue
                                                            End If
                                                    End Select
                                                Case Else
                                                    colValue = DBNull.Value
                                            End Select

                                    'colValue = sheetCell.DateCellValue
                                        Case "Number"
                                            Select Case sheetCell.CellType
                                                Case CellType.Blank
                                                Case CellType.Boolean
                                                    If sheetCell.BooleanCellValue = True Then
                                                        colValue = 1
                                                    ElseIf sheetCell.BooleanCellValue = False Then
                                                        colValue = 0
                                                    End If
                                                Case CellType.String
                                                    If IsNumeric(sheetCell.StringCellValue) Then
                                                        colValue = CType(sheetCell.StringCellValue, Decimal)
                                                    End If

                                                Case CellType.Numeric
                                                    If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                        colValue = DBNull.Value
                                                    Else
                                                        colValue = sheetCell.NumericCellValue
                                                    End If
                                                Case CellType.Formula
                                                    Select Case sheetCell.CachedFormulaResultType
                                                        Case CellType.Boolean
                                                            If sheetCell.BooleanCellValue = True Then
                                                                colValue = 1
                                                            ElseIf sheetCell.BooleanCellValue = False Then
                                                                colValue = 0
                                                            End If
                                                        Case CellType.String
                                                            If IsNumeric(sheetCell.StringCellValue) Then
                                                                colValue = CType(sheetCell.StringCellValue, Decimal)
                                                            End If
                                                        Case CellType.Numeric
                                                            If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                                                colValue = DBNull.Value
                                                            Else
                                                                Try
                                                                    If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                                                                        If sheetCell.BooleanCellValue = False Then
                                                                            colValue = 0
                                                                        Else
                                                                            colValue = 1
                                                                        End If
                                                                    Else
                                                                        colValue = sheetCell.NumericCellValue
                                                                    End If
                                                                Catch
                                                                    If IsNumeric(sheetCell.StringCellValue) Then
                                                                        colValue = CType(sheetCell.StringCellValue, Decimal)
                                                                    End If
                                                                End Try
                                                            End If
                                                    End Select
                                                Case Else
                                                    colValue = DBNull.Value
                                            End Select

                                            'colValue = sheetCell.NumericCellValue
                                    End Select

                                    Select Case sheetCols(colSpecIndex).coltype
                                        Case "String"

                                            If colValue IsNot Nothing And colValue IsNot DBNull.Value Then
                                                If colValue.Length > Proc_Imp.MaxStringSize Then
                                                    ' Maximum String Size of {2} exceeded for data {3}. Row:{0} Col:{1}
                                                    Proc_Imp.ErrorStatus = True
                                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM029", "ER_IM029", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                                End If
                                            End If

                                            If colValue Is Nothing Then
                                                colValue = ""
                                            End If

                                        Case "Number"
                                            If colValue IsNot Nothing And colValue IsNot DBNull.Value Then

                                                If colValue > Proc_Imp.MaxNumber Then
                                                    ' Maximum Number Value of {2} exceeded for data {3}. Row:{0} Col:{1}
                                                    Proc_Imp.ErrorStatus = True
                                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM030", "ER_IM030", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                                End If
                                            End If

                                            If colValue Is Nothing Then
                                                colValue = 0
                                            End If

                                    End Select
                                End If
                                Exit For

                            End If
                        Next

                        ' No Match Found so default the data
                        If foundColumn = 999 Then
                            colValue = DBNull.Value
                        End If

                        datRow(colSpecIndex) = colValue

                        ' Original Version
                        'For colIndex = 0 To sheetRow.LastCellNum - 1

                        '    sheetCell = sheetRow.GetCell(colIndex, MissingCellPolicy.RETURN_NULL_AND_BLANK)

                        '    If sheetCell Is Nothing Then
                        '        colValue = DBNull.Value
                        '    Else
                        '        colValue = DBNull.Value
                        '        Select Case sheetCols(colIndex).coltype
                        '            Case "Boolean"
                        '                Select Case sheetCell.CellType
                        '                    Case CellType.Blank
                        '                    Case CellType.Boolean
                        '                        colValue = sheetCell.BooleanCellValue
                        '                    Case CellType.String
                        '                        If sheetCell.StringCellValue = "1" Then
                        '                            colValue = True
                        '                        ElseIf sheetCell.StringCellValue = "0" Then
                        '                            colValue = False
                        '                        End If
                        '                    Case CellType.Numeric
                        '                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                            colValue = DBNull.Value
                        '                        Else
                        '                            If sheetCell.NumericCellValue = 1 Then
                        '                                colValue = True
                        '                            ElseIf sheetCell.NumericCellValue = 0 Then
                        '                                colValue = False
                        '                            End If
                        '                        End If
                        '                    Case CellType.Formula
                        '                        Select Case sheetCell.CachedFormulaResultType
                        '                            Case CellType.Boolean
                        '                                colValue = sheetCell.BooleanCellValue
                        '                            Case CellType.String
                        '                                If sheetCell.StringCellValue = "1" Or sheetCell.StringCellValue.ToLower = "yes" Or sheetCell.StringCellValue.ToLower = "True" Then
                        '                                    colValue = True
                        '                                ElseIf sheetCell.StringCellValue = "0" Or sheetCell.StringCellValue.ToLower = "no" Or sheetCell.StringCellValue.ToLower = "false" Then
                        '                                    colValue = False
                        '                                End If
                        '                            Case CellType.Numeric
                        '                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                                    colValue = DBNull.Value
                        '                                Else
                        '                                    Try
                        '                                        If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                        '                                            colValue = sheetCell.BooleanCellValue
                        '                                        Else
                        '                                            If sheetCell.NumericCellValue = 1 Then
                        '                                                colValue = True
                        '                                            ElseIf sheetCell.NumericCellValue = 0 Then
                        '                                                colValue = False
                        '                                            End If
                        '                                        End If
                        '                                    Catch
                        '                                        colValue = DBNull.Value
                        '                                    End Try
                        '                                End If
                        '                        End Select
                        '                    Case Else
                        '                        colValue = DBNull.Value
                        '                End Select

                        '                'colValue = sheetCell.BooleanCellValue
                        '            Case "String"
                        '                Select Case sheetCell.CellType
                        '                    Case CellType.Blank
                        '                    Case CellType.Boolean
                        '                        colValue = sheetCell.BooleanCellValue.ToString
                        '                    Case CellType.String
                        '                        colValue = sheetCell.StringCellValue
                        '                    Case CellType.Numeric
                        '                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                            colValue = sheetCell.DateCellValue.ToString
                        '                        Else
                        '                            colValue = sheetCell.NumericCellValue.ToString
                        '                        End If
                        '                    Case CellType.Formula
                        '                        Select Case sheetCell.CachedFormulaResultType
                        '                            Case CellType.Boolean
                        '                                colValue = sheetCell.BooleanCellValue.ToString
                        '                            Case CellType.String
                        '                                colValue = sheetCell.StringCellValue
                        '                            Case CellType.Numeric
                        '                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                                    colValue = sheetCell.DateCellValue.ToString
                        '                                Else
                        '                                    Try
                        '                                        If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                        '                                            colValue = sheetCell.BooleanCellValue.ToString
                        '                                        Else
                        '                                            colValue = sheetCell.NumericCellValue.ToString
                        '                                        End If
                        '                                    Catch
                        '                                        colValue = sheetCell.StringCellValue.ToString
                        '                                    End Try
                        '                                End If
                        '                        End Select
                        '                    Case Else
                        '                        colValue = DBNull.Value
                        '                End Select

                        '                'colValue = sheetCell.StringCellValue
                        '            Case "DateTime"
                        '                Select Case sheetCell.CellType
                        '                    Case CellType.Blank
                        '                    Case CellType.Boolean
                        '                    Case CellType.String
                        '                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                            colValue = sheetCell.DateCellValue
                        '                        End If
                        '                    Case CellType.Numeric
                        '                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                            colValue = sheetCell.DateCellValue
                        '                        End If
                        '                    Case CellType.Formula
                        '                        Select Case sheetCell.CachedFormulaResultType
                        '                            Case CellType.Boolean
                        '                            Case CellType.String
                        '                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                                    colValue = sheetCell.DateCellValue
                        '                                End If
                        '                            Case CellType.Numeric
                        '                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                                    colValue = sheetCell.DateCellValue
                        '                                End If
                        '                        End Select
                        '                    Case Else
                        '                        colValue = DBNull.Value
                        '                End Select

                        '                'colValue = sheetCell.DateCellValue
                        '            Case "Number"
                        '                Select Case sheetCell.CellType
                        '                    Case CellType.Blank
                        '                    Case CellType.Boolean
                        '                        If sheetCell.BooleanCellValue = True Then
                        '                            colValue = 1
                        '                        ElseIf sheetCell.BooleanCellValue = False Then
                        '                            colValue = 0
                        '                        End If
                        '                    Case CellType.String
                        '                        If IsNumeric(sheetCell.StringCellValue) Then
                        '                            colValue = CType(sheetCell.StringCellValue, Decimal)
                        '                        End If

                        '                    Case CellType.Numeric
                        '                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                            colValue = DBNull.Value
                        '                        Else
                        '                            colValue = sheetCell.NumericCellValue
                        '                        End If
                        '                    Case CellType.Formula
                        '                        Select Case sheetCell.CachedFormulaResultType
                        '                            Case CellType.Boolean
                        '                                If sheetCell.BooleanCellValue = True Then
                        '                                    colValue = 1
                        '                                ElseIf sheetCell.BooleanCellValue = False Then
                        '                                    colValue = 0
                        '                                End If
                        '                            Case CellType.String
                        '                                If IsNumeric(sheetCell.StringCellValue) Then
                        '                                    colValue = CType(sheetCell.StringCellValue, Decimal)
                        '                                End If
                        '                            Case CellType.Numeric
                        '                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                        '                                    colValue = DBNull.Value
                        '                                Else
                        '                                    Try
                        '                                        If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                        '                                            If sheetCell.BooleanCellValue = False Then
                        '                                                colValue = 0
                        '                                            Else
                        '                                                colValue = 1
                        '                                            End If
                        '                                        Else
                        '                                            colValue = sheetCell.NumericCellValue
                        '                                        End If
                        '                                    Catch
                        '                                        If IsNumeric(sheetCell.StringCellValue) Then
                        '                                            colValue = CType(sheetCell.StringCellValue, Decimal)
                        '                                        End If
                        '                                    End Try
                        '                                End If
                        '                        End Select
                        '                    Case Else
                        '                        colValue = DBNull.Value
                        '                End Select

                        '                'colValue = sheetCell.NumericCellValue
                        '        End Select

                        '        Select Case sheetCols(colIndex).coltype
                        '            Case "String"

                        '                If colValue.Length > MaxStringSize Then
                        '                    ' Maximum String Size of {2} exceeded for data {3}. Row:{0} Col:{1}
                        '                    Proc_Imp.ErrorStatus = True
                        '                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM029","ER_IM029", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, MaxStringSize.ToString, colValue.ToString)

                        '                End If

                        '                If colValue Is Nothing Then
                        '                    colValue = ""
                        '                End If

                        '            Case "Number"
                        '                If colValue > MaxNumber Then
                        '                    ' Maximum Number Value of {2} exceeded for data {3}. Row:{0} Col:{1}
                        '                    Proc_Imp.ErrorStatus = True
                        '                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM030","ER_IM030", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, MaxStringSize.ToString, colValue.ToString)

                        '                End If

                        '                If colValue Is Nothing Then
                        '                    colValue = 0
                        '                End If

                        '        End Select

                        '    End If
                        '    datRow(colIndex) = colValue
                        'Next

                    Next


                    datTable.Rows.Add(datRow)

                Next
            Next

            'End Using

        Catch ex As Exception
            ' Error Accessing Data from Spreadsheet
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM010", 0, 0, ex, "")
            Return Nothing
        End Try

        Return datTable
    End Function

    Public Function ExcelGetWorksheetInfo(ByVal ContentStream As MemoryStream) As List(Of String)

        Dim workbook As IWorkbook
        Dim worksheet As ISheet
        Dim sheetNames As New List(Of String)

        ' a spreasheet can have multiple tabs, Return an array of the tab (worksheet) names

        Try
            workbook = WorkbookFactory.Create(ContentStream)

            For worksheetIndex As Integer = 0 To workbook.NumberOfSheets - 1

                worksheet = workbook.GetSheetAt(worksheetIndex)

                sheetNames.Add(worksheet.SheetName)

            Next

        Catch ex As Exception
            Proc_Imp.ErrorStatus = True
            'Errortext = "Error Getting Sheet Names from Spreadsheet"
            Return Nothing
        End Try

        Return sheetNames

    End Function

    Public Function ExcelGetColumnInfo(ByVal ContentStream As MemoryStream, ByVal ShortRef As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim workbook As IWorkbook
        Dim worksheet As ISheet
        Dim sheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' a spreasheet can have multiple tabs, so we could return a dataset that can contain multiple datatables
        ' For now we ignore anything but the passed in Work Sheet Index.
        ' we assume that every sheet has a headings row

        Try
            workbook = WorkbookFactory.Create(ContentStream)

            For worksheetIndex As Integer = 0 To workbook.NumberOfSheets - 1

                worksheet = workbook.GetSheetAt(worksheetIndex)

                ' Ignore anything but the passed in Work Sheet Reference (SheetName).
                If worksheet.SheetName <> ShortRef Then
                    Continue For
                End If

                sheetCols = ExcelEvaluateDataTypes(worksheet, SegmentID)

            Next

        Catch ex As Exception
            ' Error Evaluating Datatypes from Spreadsheet
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM011", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols

    End Function

    Public Function ExcelEvaluateDataTypes(ByVal worksheet As ISheet, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef)
        Dim sheetRow As IRow
        Dim sheetCell As NPOI.SS.UserModel.ICell

        Dim colIndex As Integer
        Dim colName As String
        Dim colHead As String
        Dim colOrg As String
        Dim colType As String
        Dim colValue As Object
        Dim colSize As Integer
        Dim i As Integer
        Dim headingRowFound As Boolean = False

        ' a spreasheet can have multiple tabs, so we return a dataset that can contain multiple datatables
        ' we assume that every sheet has a headings row

        Try
            sheetCols = New Dictionary(Of Integer, colDef)

            ' firstly, we run through the spreadsheet looking at the type of data per cell to make a best guess
            For rowIndex As Integer = 0 To worksheet.LastRowNum - 1
                sheetRow = worksheet.GetRow(rowIndex)
                If sheetRow Is Nothing Then Continue For
                If rowIndex = 0 Or headingRowFound = False Then
                    headingRowFound = True

                    ' First Row, find the headings and create the list of columns
                    For colIndex = 0 To sheetRow.LastCellNum - 1
                        colName = "Col_{0}"
                        Try
                            sheetCell = sheetRow.GetCell(colIndex, MissingCellPolicy.RETURN_NULL_AND_BLANK)

                            colName = sheetCell.StringCellValue
                        Catch
                            ' Missing Column Name - Error
                            colName = String.Format(colName, colIndex)
                            Proc_Imp.ErrorStatus = True
                            ' Missing column heading in import file
                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM012", "ER_IM012", rowIndex + 1, colIndex + 1, "")
                        End Try
                        colOrg = colName.Replace(" ", "_").ToLower ' Original Column Name
                        colName = colName.Replace(" ", "_").ToLower
                        colHead = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(colName.Replace("_", " "))

                        If colHead.Length > Proc_Imp.MaxColumnNameSize Then
                            Proc_Imp.ErrorStatus = True
                            ' Column heading length for {2} too large (Max {3}) in import file.  Row:{0} Col:{1}
                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM028", "ER_IM028", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, colHead, Proc_Imp.MaxColumnNameSize.ToString)
                        End If

                        For Each kvp As KeyValuePair(Of Integer, colDef) In sheetCols
                            If kvp.Value.colhead = colHead Then
                                ' colName = String.Format("{0}_{1}", colName, colIndex)
                                ' Duplicate Column heading {2} in import file. Row:{0} Col:{1}
                                Proc_Imp.ErrorStatus = True
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM013", "ER_IM013", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, kvp.Value.colhead)
                                Exit For
                            End If
                        Next
                        ' Now override the Column Name with Col_nn
                        colName = "Col_{0}"
                        colName = String.Format(colName, colIndex + 1)

                        sheetCols.Add(colIndex, New colDef(colName, "", 0, colHead, colOrg, "0", "01", "0", ""))
                    Next
                Else
                    ' 2nd row onwards get the datatype and check the actual data in there

                    For colIndex = 0 To sheetRow.LastCellNum - 1

                        sheetCell = sheetRow.GetCell(colIndex, MissingCellPolicy.RETURN_NULL_AND_BLANK)

                        If sheetCell Is Nothing Then Continue For

                        colType = "String"
                        colValue = DBNull.Value
                        colSize = 0

                        Select Case sheetCell.CellType
                            Case CellType.Blank
                            Case CellType.Boolean
                                colType = "Boolean"
                                colValue = sheetCell.BooleanCellValue
                            Case CellType.String
                                colType = "String"
                                colValue = sheetCell.StringCellValue
                            Case CellType.Numeric
                                If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                    colType = "DateTime"
                                    colValue = sheetCell.DateCellValue
                                Else
                                    colType = "Number"
                                    colValue = sheetCell.NumericCellValue
                                End If
                            Case CellType.Formula
                                Select Case sheetCell.CachedFormulaResultType
                                    Case CellType.Boolean
                                        colType = "Boolean"
                                        colValue = sheetCell.BooleanCellValue
                                    Case CellType.String
                                        colType = "String"
                                        colValue = sheetCell.StringCellValue
                                    Case CellType.Numeric
                                        If HSSFDateUtil.IsCellDateFormatted(sheetCell) Then
                                            colType = "DateTime"
                                            colValue = sheetCell.DateCellValue
                                        Else
                                            Try
                                                If sheetCell.CellFormula.ContainsOneOf("TRUE()", "FALSE()") Then
                                                    colType = "Boolean"
                                                    colValue = sheetCell.BooleanCellValue
                                                Else
                                                    colType = "Number"
                                                    colValue = sheetCell.NumericCellValue
                                                End If
                                            Catch
                                                colType = "String"
                                                colValue = sheetCell.StringCellValue
                                            End Try
                                        End If
                                End Select
                            Case Else
                                colType = "String"
                                colValue = sheetCell.StringCellValue
                        End Select

                        If sheetCols(colIndex).coltype = "" Then
                            sheetCols(colIndex).coltype = colType
                        End If

                        ' Check if column content has changed.
                        Select Case sheetCols(colIndex).coltype
                            Case "Number"
                                If colType.ContainsOneOf("String", "DateTime", "Boolean") Then
                                    sheetCols(colIndex).coltype = colType
                                End If
                                If colType = "Number" Then
                                    i = Proc_Imp.DecCount(colValue)
                                    If i > sheetCols(colIndex).collen Then sheetCols(colIndex).collen = i
                                End If
                            Case "DateTime"
                                If colType.ContainsOneOf("String", "Number", "Boolean") Then
                                    sheetCols(colIndex).coltype = "String"
                                End If
                            Case "Boolean"
                                If colType.ContainsOneOf("String", "DateTime", "Number") Then
                                    sheetCols(colIndex).coltype = "String"
                                End If
                            Case "String"
                                i = colValue.ToString.Length
                                ' Round to nearest (0 = variable depending on value)
                                i = Proc_Imp.RoundUpToX(i, 0)
                                If i > sheetCols(colIndex).collen Then sheetCols(colIndex).collen = i
                        End Select
                    Next
                End If
            Next

            'End Using

        Catch ex As Exception
            Proc_Imp.ErrorStatus = True
            'Error evaluating data types from spreadsheet
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM011", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols
    End Function

End Class
