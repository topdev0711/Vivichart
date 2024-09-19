Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class ParseCSV

    Dim SQL As String
    Dim DelimiterChar As String = ","
    Dim DelimiterChanged As Boolean = False

    Public Proc_Imp As ProcessImport

    Public Sub New(_Proc_Imp As ProcessImport)
        Proc_Imp = _Proc_Imp
    End Sub

    Public Function CSVProcessSegment(ByVal PassSegmentID As Integer) As DataTable

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
        Dim ImportData As Byte()
        Dim ImportDataAsString As String

        Dim SQLResult As Long
        Dim CSVDataTable As New DataTable
        Dim CSVSheetCols As Dictionary(Of Integer, colDef) = Nothing
        Dim DBSheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' Get next row to be processed
        Using DB As New IawDB

            SQL = "SELECT CI.unique_id, CI.client_id, CI.import_data,
                          CIS.seg_status, CIS.seg_id, CIS.prev_source_date, CIS.source_date, CIS.short_ref,
                          IsNull(CIS.source_id,-1) as source_id
                     FROM dbo.client_import CI
                          JOIN dbo.client_import_seg CIS
                            ON CI.unique_id = CIS.unique_id
                    WHERE CI.import_type IN ('03')
                      AND CIS.seg_status IN ('01','04')
                      AND CIS.seg_id = @p1
                    ORDER BY CI.batch_ident ASC, CI.unique_id ASC, CIS.seg_id ASC"
            datTable = DB.GetTable(SQL, PassSegmentID)
            If datTable IsNot Nothing Then
                For Each datRow In datTable.Rows

                    ' If the datasource already exists, then the source_id and the previous_source_date
                    ' will be populated else they will not be set (SQL will return -1 in the source_id)

                    Proc_Imp.ErrorStatus = False
                    ImportID = GetInt(datRow, "unique_id")
                    ClientID = datRow.GetValue("client_id", "")
                    SegmentID = GetInt(datRow, "seg_id")
                    ShortRef = datRow.GetValue("short_ref", "")
                    SourceDate = datRow.GetDate("source_date")
                    SourceID = datRow.GetInt("source_id")
                    PrevSourceDate = datRow.GetDate("prev_source_date")
                    SegmentStatus = datRow.GetValue("seg_status", "")
                    ImportData = datRow.GetBytes("import_data")
                    ImportDataAsString = Proc_Imp.ReadDataStream(ImportData)

                    ' Get the Delimiter Character from the seg table. IF not exists then set to ,
                    DelimiterChar = ","
                    DelimiterChanged = False
                    DelimiterChar = datRow.GetValue("delimiter_char")
                    If DelimiterChar = "" Then
                        DelimiterChar = ","
                    End If

                    'ImportDataAsString = Encoding.UTF8.GetString(ImportData)

                    'ImportRecords = ImportDataAsString.Split(New String() {Environment.NewLine},
                    '                   StringSplitOptions.None)

                    'For RecordLoop = 0 To ImportRecords.Count - 1
                    '    ImportRecordConvert = Regex.Replace(ImportRecords(RecordLoop), "(,)(?=(?:[^""]|""[^""]*"")*$)", "|").Replace("""", "")
                    '    ImportColumns = ImportRecordConvert.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)
                    '    For ColumnLoop = 0 To ImportColumns.Count - 1
                    '        ImportCell = ImportColumns(ColumnLoop)
                    '    Next
                    'Next

                    ' Get the Max Sizes from the clients table
                    Proc_Imp.GetSizes(ClientID)

                    Select Case SegmentStatus
                        Case "01" ' Unprocessed - Evaluate the datatypes of the spreadsheet

                            If ImportID <> 0 And ClientID IsNot "" And ImportDataAsString IsNot Nothing Then

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
                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, "")
                                End Try

                                If Proc_Imp.ErrorStatus = False Then
                                    ' Update Datasource. Remember previous BasisID in case required later
                                    If SourceID > 0 Then
                                        ' Get column info for existing source from DB in order to compare against columns from new spreadsheet
                                        DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, PrevSourceDate, SegmentID)
                                    End If
                                End If

                                If Proc_Imp.ErrorStatus = False Then
                                    ' Get the column specification from the incoming spreadsheet
                                    CSVSheetCols = CSVGetColumnInfo(ImportDataAsString, ShortRef, SegmentID)

                                    If CSVSheetCols IsNot Nothing And Proc_Imp.ErrorStatus = False Then
                                        ' Now import the data into a datatable to run further checks

                                        CSVDataTable = CSVReadData(ImportDataAsString, 0, ShortRef, SegmentID, SourceDate)

                                        If Proc_Imp.ErrorStatus = False Then

                                            ' Set columns to unique if data is unique 
                                            For Each CD As colDef In CSVSheetCols.Values

                                                If CD.coltype = "String" Or (CD.coltype = "Number" And CD.collen = 0) Then
                                                    If Proc_Imp.isUnique(CSVDataTable, CD.colname) Then
                                                        CD.colunique = "1"
                                                    Else
                                                        CD.colunique = "0"
                                                    End If
                                                Else
                                                    CD.colunique = "0"
                                                End If
                                            Next

                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Update Datasource. Remember previous BasisID in case required later
                                                If SourceID > 0 Then
                                                    ' Compare DB and CSV Column specifications
                                                    SegmentStatus = Proc_Imp.CompareColumnSpecsVariable(SegmentID, DBSheetCols, CSVSheetCols)
                                                Else
                                                    ' New source Assumed
                                                    SegmentStatus = "03" ' Awaiting Approval
                                                End If
                                            End If
                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Create a new source from this
                                                If SourceID > 0 Then
                                                    ' Check and change the column specifications order, incase it has changed
                                                    If Proc_Imp.ChangeColumnSpecsOrder(SegmentID, DBSheetCols, CSVSheetCols) = 99 Then
                                                        ' Unable to evaluate column specification from XML
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, "")
                                                    End If
                                                End If

                                                If Proc_Imp.ErrorStatus = False Then

                                                    SourceID = Proc_Imp.CreateSourceFromSpec(ClientID, SourceID, CSVSheetCols, ShortRef, SegmentID, SourceDate, SegmentStatus)

                                                    If SourceID = 0 Then
                                                        ' Unable to create new Datasource from CSV
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM005", 0, 0, "")
                                                    End If
                                                End If

                                            End If

                                        Else
                                            ' Unable to evaluate column specification from CSV
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
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, "")
                            End Try

                    End Select

                    Select Case SegmentStatus
                        Case "04" ' Import Data - Import the Data from the spreadsheet.

                            If SourceID > 0 Then

                                ' Read the Data from spreadsheet.
                                CSVDataTable = CSVReadData(ImportDataAsString, SourceID, ShortRef, SegmentID, SourceDate)

                                ' Get column info for existing Basis from DB
                                DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, SourceDate, SegmentID)

                                ' Create New Table and Insert the Data from the Spreadsheet.
                                Proc_Imp.CreateTableInsertData(SourceID, SegmentID, DBSheetCols, CSVDataTable, SourceDate, PrevSourceDate)


                                If Proc_Imp.ErrorStatus = False Then

                                    If PrevSourceDate.Ticks > 0 Then
                                        ' Copy Model, etc from previous Basis to New one
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
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, "")
                                'Finally
                                '   DB.Close()
                            End Try

                    End Select

                Next
            End If
            DB.Close()
        End Using

        Return CSVDataTable
    End Function

    Public Function CSVReadData(ByVal ContentData As String, ByVal sourceID As String, ByVal ShortRef As String, ByVal SegmentID As String, ByVal SourceDate As Date) As DataTable
        Dim datTable As DataTable = Nothing
        Dim datRow As DataRow

        Dim sheetCols As Dictionary(Of Integer, colDef)

        Dim colIndex As Integer
        Dim colValue As Object = Nothing
        Dim colSpecIndex As Integer
        Dim foundColumn As Integer

        Dim ImportRecords As String()
        Dim ImportRecordConvert As String
        Dim ImportColumns As String()
        Dim ImportHeadings As String() = Nothing
        Dim ImportCell As String
        Dim ImportCellType As String

        ' we assume that every file has a headings row
        Try

            ImportRecords = ContentData.Split(New String() {Environment.NewLine},
                               StringSplitOptions.None)

            If sourceID > 0 Then
                ' If source set get data from the DB.
                sheetCols = Proc_Imp.DBGetColumnInfo(sourceID, SourceDate, SegmentID)
            Else
                ' Still at evaluation stage Get the column details from the worksheet.
                sheetCols = CSVEvaluateDataTypes(ImportRecords, SegmentID)
            End If

            ' Now we should have a good idea of what the structure of the data should be like..
            ' so now we can create the datatable and insert the data

            datTable = New DataTable(ShortRef)   ' may need to fix the sheetname

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
            For rowIndex As Integer = 0 To ImportRecords.Count - 1
                ' Ignore empty rows 
                If ImportRecords(rowIndex) = "" Then Continue For

                ' Check for Seperator Character Row (i.e. first row contains one character)
                If rowIndex = 0 Then
                    DelimiterChanged = False
                    If Len(ImportRecords(rowIndex)) = 1 Then
                        DelimiterChar = ImportRecords(rowIndex)
                        DelimiterChanged = True
                    End If
                    Continue For
                End If

                ' Convert data using | as delimiter 
                ImportRecordConvert = Regex.Replace(ImportRecords(rowIndex), "(" + delimiterChar + ")(?=(?:[^""]|""[^""]*"")*$)", "|").Replace("""", "")

                ' Get headings
                If rowIndex = 0 Or (rowIndex = 1 And delimiterChanged = True) Then
                    ImportHeadings = ImportRecordConvert.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)
                    For colIndex = 0 To ImportHeadings.Count - 1
                        ImportHeadings(colIndex) = ImportHeadings(colIndex).Replace(" ", "_").ToLower
                    Next
                    Continue For
                End If

                ' Get Column Array (sheetrow = ImportColumns)
                ImportColumns = ImportRecordConvert.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)

                ' Add a new data row
                datRow = datTable.NewRow()

                For colSpecIndex = 0 To sheetCols.Count - 1
                    ' ColumnSpec(colSpecIndex).colorg contains the original column for matching
                    foundColumn = 999 ' No Match 
                    ' Find the corresponding column in the data
                    For colIndex = 0 To ImportHeadings.Count - 1
                        If ImportHeadings(colIndex) = sheetCols(colSpecIndex).colorg Then
                            foundColumn = 1 ' Match 
                            ' Get the Cell of data. sheetCell = ImportCell
                            ImportCell = ImportColumns(colIndex)

                            If IsNumeric(ImportCell) Then
                                ImportCellType = "Numeric"
                            Else
                                If IsDate(ImportCell) Then
                                    ImportCellType = "DateTime"
                                Else
                                    ImportCellType = "String"
                                End If
                            End If

                            If ImportCell Is Nothing Then
                                colValue = DBNull.Value
                            Else
                                colValue = DBNull.Value
                                Select Case sheetCols(colSpecIndex).coltype

                                    Case "String"
                                        Select Case ImportCellType
                                            Case "String"
                                                colValue = ImportCell
                                            Case "Numeric"
                                                colValue = ImportCell
                                            Case "DateTime"
                                                colValue = ImportCell
                                            Case Else
                                                colValue = DBNull.Value
                                        End Select

                                    Case "DateTime"
                                        Select Case ImportCellType
                                            Case "String"
                                                colValue = ImportCell
                                            Case "Numeric"
                                                colValue = ImportCell
                                            Case "DateTime"
                                                colValue = ImportCell
                                            Case Else
                                                colValue = DBNull.Value
                                        End Select

                                    Case "Number"
                                        Select Case ImportCellType
                                            Case "String"
                                                colValue = ImportCell
                                            Case "Numeric"
                                                colValue = ImportCell
                                            Case "DateTime"
                                                colValue = ImportCell
                                            Case Else
                                                colValue = DBNull.Value
                                        End Select
                                End Select

                                Select Case sheetCols(colSpecIndex).coltype
                                    Case "String"

                                        If colValue.Length > Proc_Imp.MaxStringSize Then
                                            ' Maximum String Size of {2} exceeded for data {3}. Row:{0} Col:{1}
                                            Proc_Imp.ErrorStatus = True
                                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM029", "ER_IM029", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                        End If

                                        If colValue Is Nothing Then
                                            colValue = ""
                                        End If

                                    Case "Number"
                                        If colValue > Proc_Imp.MaxNumber Then
                                            ' Maximum Number Value of {2} exceeded for data {3}. Row:{0} Col:{1}
                                            Proc_Imp.ErrorStatus = True
                                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM030", "ER_IM030", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                        End If

                                        If colValue Is Nothing Then
                                            colValue = "0"
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

                    '    For colIndex = 0 To ImportColumns.Count - 1

                    '        ' Get the Cell of data. sheetCell = ImportCell
                    '        ImportCell = ImportColumns(colIndex)

                    '        If IsNumeric(ImportCell) Then
                    '            ImportCellType = "Numeric"
                    '        Else
                    '            If IsDate(ImportCell) Then
                    '                ImportCellType = "DateTime"
                    '            Else
                    '                ImportCellType = "String"
                    '            End If
                    '        End If

                    '        If ImportCell Is Nothing Then
                    '            colValue = DBNull.Value
                    '        Else
                    '            colValue = DBNull.Value
                    '            Select Case sheetCols(colIndex).coltype

                    '                Case "String"
                    '                    Select Case ImportCellType
                    '                        Case "String"
                    '                            colValue = ImportCell
                    '                        Case "Numeric"
                    '                            colValue = ImportCell
                    '                        Case "DateTime"
                    '                            colValue = ImportCell
                    '                        Case Else
                    '                            colValue = DBNull.Value
                    '                    End Select

                    '                Case "DateTime"
                    '                    Select Case ImportCellType
                    '                        Case "String"
                    '                            colValue = ImportCell
                    '                        Case "Numeric"
                    '                            colValue = ImportCell
                    '                        Case "DateTime"
                    '                            colValue = ImportCell
                    '                        Case Else
                    '                            colValue = DBNull.Value
                    '                    End Select

                    '                Case "Number"
                    '                    Select Case ImportCellType
                    '                        Case "String"
                    '                            colValue = ImportCell
                    '                        Case "Numeric"
                    '                            colValue = ImportCell
                    '                        Case "DateTime"
                    '                            colValue = ImportCell
                    '                        Case Else
                    '                            colValue = DBNull.Value
                    '                    End Select
                    '            End Select

                    '            Select Case sheetCols(colIndex).coltype
                    '                Case "String"

                    '                    If colValue.Length > MaxStringSize Then
                    '                        ' Maximum String Size of {2} exceeded for data {3}. Row:{0} Col:{1}
                    '                        Proc_Imp.ErrorStatus = True
                    '                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM029","ER_IM029", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, MaxStringSize.ToString, colValue.ToString)

                    '                    End If

                    '                    If colValue Is Nothing Then
                    '                        colValue = ""
                    '                    End If

                    '                Case "Number"
                    '                    If colValue > MaxNumber Then
                    '                        ' Maximum Number Value of {2} exceeded for data {3}. Row:{0} Col:{1}
                    '                        Proc_Imp.ErrorStatus = True
                    '                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM030","ER_IM030", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, MaxStringSize.ToString, colValue.ToString)

                    '                    End If

                    '                    If colValue Is Nothing Then
                    '                        colValue = "0"
                    '                    End If

                    '            End Select

                    '        End If
                    '        datRow(colIndex) = colValue
                    '    Next
                Next


                datTable.Rows.Add(datRow)
            Next

        Catch ex As Exception
            ' Error Accessing Data from CSV File
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM010", 0, 0, "")
            Return Nothing
        End Try

        Return datTable
    End Function

    Public Function CSVGetColumnInfo(ByVal ContentData As String, ByVal ShortRef As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef) = Nothing

        Dim ImportRecords As String()

        ' we assume that every file has a headings row
        Try

            ImportRecords = ContentData.Split(New String() {Environment.NewLine},
                               StringSplitOptions.None)

            sheetCols = CSVEvaluateDataTypes(ImportRecords, SegmentID)

        Catch ex As Exception
            ' Error Evaluating Datatypes from Spreadsheet
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM011", 0, 0, "")
            Return Nothing
        End Try

        Return sheetCols

    End Function

    Public Function CSVEvaluateDataTypes(ByVal ImportRecords As String(), ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef)

        Dim colIndex As Integer
        Dim colName As String
        Dim colHead As String
        Dim colOrg As String
        Dim colType As String
        Dim colValue As Object
        Dim colSize As Integer
        Dim i As Integer

        Dim ImportRecordConvert As String
        Dim ImportColumns As String()
        Dim ImportCell As String
        Dim ImportCellType As String

        ' We assume that every sheet has a headings row
        Try
            sheetCols = New Dictionary(Of Integer, colDef)

            ' firstly, we run through the spreadsheet looking at the type of data per cell to make a best guess
            For rowIndex As Integer = 0 To ImportRecords.Count - 1

                ' Ignore empty rows
                If ImportRecords(rowIndex) = "" Then Continue For

                ' Check for Seperator Character Row (i.e. first row contains one character)
                If rowIndex = 0 Then
                    DelimiterChanged = False
                    If Len(ImportRecords(rowIndex)) = 1 Then
                        DelimiterChar = ImportRecords(rowIndex)
                        DelimiterChanged = True
                    End If
                    Continue For
                End If

                ' Convert data using | as delimiter 
                ImportRecordConvert = Regex.Replace(ImportRecords(rowIndex), "(" + DelimiterChar + ")(?=(?:[^""]|""[^""]*"")*$)", "|").Replace("""", "")

                ' Get Column Array (sheetrow = ImportColumns)
                ImportColumns = ImportRecordConvert.Split(New Char() {"|"c}, StringSplitOptions.RemoveEmptyEntries)

                If rowIndex = 0 Or (rowIndex = 1 And DelimiterChanged = True) Then
                    ' First Row, find the headings and create the list of columns
                    For colIndex = 0 To ImportColumns.Count - 1
                        ' Get the Cell of data. sheetCell = ImportCell
                        ImportCell = ImportColumns(colIndex)
                        colName = "Col_{0}"
                        If ImportCell <> "" Then
                            colName = ImportCell
                        End If
                        colOrg = colName.Replace(" ", "_").ToLower ' Original column name
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
                    For colIndex = 0 To ImportColumns.Count - 1
                        ' Get the Cell of data. sheetCell = ImportCell
                        ImportCell = ImportColumns(colIndex)

                        If ImportCell Is Nothing Then Continue For

                        colType = "String"
                        colValue = DBNull.Value
                        colSize = 0

                        If IsNumeric(ImportCell) Then
                            ' If number is not a decimal and has a leading zero then assume String
                            If Proc_Imp.DecCount(ImportCell) = 0 And Left(ImportCell, 1) = 0 Then
                                ImportCellType = "String"
                            Else
                                ImportCellType = "Numeric"
                            End If
                        Else
                            If IsDate(ImportCell) Then
                                ImportCellType = "DateTime"
                            Else
                                ImportCellType = "String"
                            End If
                        End If

                        Select Case ImportCellType
                            Case "String"
                                colType = "String"
                                colValue = ImportCell
                            Case "Numeric"
                                colType = "Number"
                                colValue = ImportCell
                            Case "DateTime"
                                colType = "DateTime"
                                colValue = ImportCell
                            Case Else
                                colType = "String"
                                colValue = ImportCell
                        End Select

                        If sheetCols(colIndex).coltype = "" Then
                            sheetCols(colIndex).coltype = colType
                        End If

                        ' Check if column content has changed.
                        Select Case sheetCols(colIndex).coltype
                            Case "Number"
                                If colType.ContainsOneOf("String", "DateTime") Then
                                    sheetCols(colIndex).coltype = colType
                                End If
                                If colType = "Number" Then
                                    i = Proc_Imp.DecCount(colValue)
                                    If i > sheetCols(colIndex).collen Then sheetCols(colIndex).collen = i
                                End If
                            Case "DateTime"
                                If colType.ContainsOneOf("String", "Number") Then
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
            'Error evaluating data types from import data
            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM011", 0, 0, "")
            Return Nothing
        End Try

        Return sheetCols
    End Function

End Class
