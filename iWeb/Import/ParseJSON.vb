Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Text

Public Class ParseJSON

    Dim SQL As String
    Dim FieldPointer As String ' Pointer to where Field Information exists within the JSON file
    Dim RecordPointer As String ' Pointer to where Record Information exists within the JSON file

    Public Proc_Imp As ProcessImport

    Public Sub New(_Proc_Imp As ProcessImport)
        Proc_Imp = _Proc_Imp
    End Sub

    Public Function JSONProcessSegment(ByVal PassSegmentID As Integer) As DataTable

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
        Dim jsonData As String
        Dim bom As Byte() = Encoding.UTF8.GetPreamble()

        Dim SQLResult As Long
        Dim JSONDataTable As New DataTable
        Dim JSONSheetCols As Dictionary(Of Integer, colDef) = Nothing
        Dim DBSheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' Get next row to be processed
        Using DB As New IawDB

            SQL = "SELECT CI.unique_id, CI.client_id, CI.import_data,
                          CIS.seg_status, CIS.seg_id, CIS.prev_source_date, CIS.source_date, CIS.short_ref,
                          IsNull(CIS.source_id,-1) as source_id
                     FROM dbo.client_import CI
                          JOIN dbo.client_import_seg CIS
                            ON CI.unique_id = CIS.unique_id
                    WHERE CI.import_type IN ('05')
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
                    jsonData = Proc_Imp.ReadDataStream(ImportData)
                    jsonData = jsonData.Trim

                    If jsonData.StartsWith("[") Then
                        jsonData = jsonData.Remove(0, 1)
                    End If

                    If jsonData.EndsWith("]") Then
                        jsonData = jsonData.Remove(jsonData.Length - 1, 1)
                    End If

                    ' Retrieve from DB
                    ' FieldPointer = datRow.GetValue("field_pointer", "")
                    ' RecordPointer = datRow.GetValue("record_pointer", "")
                    ' For now fixed
                    'FieldPointer = "Fields"
                    FieldPointer = ""
                    RecordPointer = "Records"

                    ' This routine expects JSON in this format, with or without the Fields section.
                    ' The Field and Record Pointer (above) can be changed if the format is basically the same.
                    '[
                    '   {
                    '      "Datasource":"Employee",
                    '      "Fields":[
                    '         {
                    '            "name":"emp_ref",
                    '            "datatype":"varchar",
                    '            "length":10
                    '         },
                    '         {
                    '            "name":"forename",
                    '            "datatype":"nvarchar",
                    '            "length":50
                    '         },
                    '         {
                    '            "name":"surname",
                    '            "datatype":"nvarchar",
                    '            "length":50
                    '         },
                    '         {
                    '            "name":"date_of_birth",
                    '            "datatype":"datetime"
                    '         },
                    '         {
                    '            "name":"salary",
                    '            "datatype":"decimal",
                    '            "scale":2,
                    '            "precision":15
                    '         },
                    '         {
                    '            "name":"service",
                    '            "datatype":"int",
                    '            "scale":0,
                    '            "precision":10
                    '         }
                    '      ],
                    '      "Records":[
                    '         {
                    '            "emp_ref":"0001",
                    '            "forename":"Andy",
                    '            "surname":"Andrews",
                    '            "date_of_birth":"1997-05-14T00:00:00",
                    '            "salary":23345.76,
                    '            "service":12
                    '         },
                    '         {
                    '            "emp_ref":"0002",
                    '            "surname":"Brianson",
                    '            "date_of_birth":"1986-02-19T00:00:00",
                    '            "salary":32476.98,
                    '            "service":19
                    '         }
                    '      ]
                    '   }
                    ']

                    ' Get the Max Sizes from the clients table
                    Proc_Imp.GetSizes(ClientID)

                    Select Case SegmentStatus
                        Case "01" ' Unprocessed - Evaluate the datatypes of the spreadsheet

                            If ImportID <> 0 And ClientID IsNot "" And jsonData IsNot Nothing Then

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
                                    Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM004", 0, 0, ex, "")
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
                                    JSONSheetCols = JSONGetColumnInfo(jsonData, ShortRef, SegmentID)

                                    If JSONSheetCols IsNot Nothing And Proc_Imp.ErrorStatus = False Then
                                        ' Now import the data into a datatable to run further checks

                                        JSONDataTable = JSONReadData(jsonData, 0, ShortRef, SegmentID, SourceDate)

                                        If Proc_Imp.ErrorStatus = False Then

                                            ' Set columns to unique if data is unique 
                                            For Each CD As colDef In JSONSheetCols.Values

                                                If CD.coltype = "String" Or (CD.coltype = "Number" And CD.collen = 0) Then
                                                    If Proc_Imp.isUnique(JSONDataTable, CD.colname) Then
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
                                                    ' Compare DB and JSON Column specifications
                                                    SegmentStatus = Proc_Imp.CompareColumnSpecsVariable(SegmentID, DBSheetCols, JSONSheetCols)
                                                Else
                                                    ' New source Assumed
                                                    SegmentStatus = "03" ' Awaiting Approval
                                                End If
                                            End If
                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Create a new source from this
                                                If SourceID > 0 Then
                                                    ' Check and change the column specifications order, incase it has changed
                                                    If Proc_Imp.ChangeColumnSpecsOrder(SegmentID, DBSheetCols, JSONSheetCols) = 99 Then
                                                        ' Unable to evaluate column specification from JSON
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, "")
                                                    End If
                                                End If

                                                If Proc_Imp.ErrorStatus = False Then

                                                    SourceID = Proc_Imp.CreateSourceFromSpec(ClientID, SourceID, JSONSheetCols, ShortRef, SegmentID, SourceDate, SegmentStatus)

                                                    If SourceID = 0 Then
                                                        ' Unable to create new Datasource from JSON
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM005", 0, 0, "")
                                                    End If
                                                End If


                                            End If

                                        Else
                                            ' Unable to evaluate column specification from JSON
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
                                Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM004", 0, 0, ex, "")
                            End Try

                    End Select

                    Select Case SegmentStatus
                        Case "04" ' Import Data - Import the Data from the spreadsheet.

                            If SourceID > 0 Then

                                ' Read the Data from spreadsheet.
                                JSONDataTable = JSONReadData(jsonData, SourceID, ShortRef, SegmentID, SourceDate)

                                ' Get column info for existing Basis from DB
                                DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, SourceDate, SegmentID)

                                ' Create New Table and Insert the Data from the Spreadsheet.
                                Proc_Imp.CreateTableInsertData(SourceID, SegmentID, DBSheetCols, JSONDataTable, SourceDate, PrevSourceDate)

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
                                Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM004", 0, 0, ex, "")
                                'Finally
                                '   DB.Close()
                            End Try

                    End Select

                Next
            End If
            DB.Close()
        End Using

        Return JSONDataTable

    End Function

    Public Function JSONReadData(ByVal ContentData As String, ByVal sourceID As String, ByVal ShortRef As String, ByVal SegmentID As String, ByVal SourceDate As Date) As DataTable
        Dim datTable As DataTable = Nothing
        Dim datRow As DataRow
        Dim emptyDate As DateTime = DateTime.MinValue

        Dim sheetCols As Dictionary(Of Integer, colDef)

        Dim colIndex As Integer
        Dim colValue As Object

        Dim ImportCell As String
        Dim ImportCellType As String

        Dim jsonDoc As JObject
        Dim recordNodes As JArray
        Dim recordNode As JObject
        '      Dim childNodes As XmlNodeList


        jsonDoc = JObject.Parse(ContentData)

        ' we assume that every file has a headings row
        Try

            If sourceID > 0 Then
                ' If source set get data from the DB.
                sheetCols = Proc_Imp.DBGetColumnInfo(sourceID, SourceDate, SegmentID)
            Else
                ' Still at evaluation stage Get the column details from the worksheet.
                sheetCols = JSONEvaluateDataTypes(ContentData, SegmentID)
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

            ' Create Rows

            ' Get all the Record Nodes
            recordNodes = jsonDoc(RecordPointer)

            ' Loop round the record nodes
            For rowIndex As Integer = 0 To recordNodes.Count - 1
                ' Get an individual record node
                recordNode = recordNodes(rowIndex)

                '' Select all child nodes of the Record node
                'childNodes = recordNode.ChildNodes

                '' Iterate through each child node
                'For Each childNode As XmlNode In childNodes

                ' now add the data into the table
                datRow = datTable.NewRow()
                ' Loop round the columns to get the items in the record by index
                For colIndex = 0 To sheetCols.Count - 1

                    ' Get the Element data (by Name)
                    If recordNode(sheetCols(colIndex).colorg) IsNot Nothing Then
                        ImportCell = recordNode(sheetCols(colIndex).colorg).ToString()
                    Else
                        ImportCell = ""
                        Select Case sheetCols(colIndex).coltype
                            Case "Number", "Numeric"
                                ImportCell = "0"
                            Case "DateTime"
                                ImportCell = "1900-01-01 00:00:00"
                        End Select
                    End If

                    ' Only for information, in case required by a different format JSON.
                    If IsNumeric(ImportCell) Then
                        ImportCellType = "Number"
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
                        colValue = ImportCell

                        Select Case sheetCols(colIndex).coltype
                            Case "String"

                                If colValue.Length > Proc_Imp.MaxStringSize Then
                                    ' Maximum String Size of {2} exceeded for data {3}. Row:{0} Col:{1}
                                    Proc_Imp.ErrorStatus = True
                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM029", "ER_IM029", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                End If

                                If colValue Is Nothing Then
                                    colValue = ""
                                End If

                            Case "Number", "Numeric"
                                If colValue > Proc_Imp.MaxNumber Then
                                    ' Maximum Number Value of {2} exceeded for data {3}. Row:{0} Col:{1}
                                    Proc_Imp.ErrorStatus = True
                                    Proc_Imp.WriteImportMessage(SegmentID, "ER_IM030", "ER_IM030", rowIndex + 1, colIndex + 1, rowIndex + 1, colIndex + 1, Proc_Imp.MaxStringSize.ToString, colValue.ToString)

                                End If

                                If colValue Is Nothing Or colValue = "" Then
                                    colValue = "0"
                                End If

                            Case "DateTime"
                                If colValue Is Nothing Or colValue = "" Then
                                    colValue = "1900-01-01 00:00:00"
                                End If
                        End Select

                    End If
                    datRow(colIndex) = colValue
                Next

                ' Add row to data table.
                datTable.Rows.Add(datRow)
                'Next

            Next

        Catch ex As Exception
            ' Error Accessing Data from JSON File
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM010", 0, 0, ex, "")
            Return Nothing
        End Try

        Return datTable
    End Function

    Public Function JSONGetColumnInfo(ByVal ContentData As String, ByVal ShortRef As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' we assume that every file has a headings row
        Try

            sheetCols = JSONEvaluateDataTypes(ContentData, SegmentID)

        Catch ex As Exception
            ' Error Evaluating Datatypes from Spreadsheet
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM011", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols

    End Function

    Public Function JSONEvaluateDataTypes(ByVal ContentData As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef)

        Dim colIndex As Integer
        Dim colName As String
        Dim colHead As String
        Dim colOrg As String
        Dim colType As String
        Dim colValue As Object
        Dim colSize As Integer
        Dim i As Integer
        Dim foundColumn As Integer

        Dim ImportCellType As String

        Dim jsonDoc As JObject
        Dim recordNodes As JArray
        Dim recordNode As JObject

        Dim columnName As String
        Dim dataType As String
        Dim length As Integer
        Dim precision As Integer
        Dim scale As Integer


        jsonDoc = JObject.Parse(ContentData)

        ' We assume that every sheet has a headings row
        Try
            sheetCols = New Dictionary(Of Integer, colDef)

            ' First we look for the field data descriptors if there are any
            If FieldPointer.Length > 0 Then
                recordNodes = jsonDoc(FieldPointer)

                colIndex = 0

                For Each recordNode In recordNodes

                    '' Select all child nodes of the Record node
                    'childNodes = recordNode.ChildNodes

                    '' Iterate through each child node
                    'For Each childNode As XmlNode In childNodes

                    columnName = "col_" + colIndex.ToString
                    dataType = ""
                    length = 0
                    precision = 0
                    scale = 0

                    If recordNode("name") IsNot Nothing Then
                        columnName = recordNode("name").ToString()
                    End If
                    If recordNode("datatype") IsNot Nothing Then
                        dataType = recordNode("datatype").ToString()
                    End If
                    If recordNode("length") IsNot Nothing Then
                        length = Integer.Parse(recordNode("length").ToString())
                    End If
                    If recordNode("precision") IsNot Nothing Then
                        precision = Integer.Parse(recordNode("precision").ToString())
                    End If
                    If recordNode("scale") IsNot Nothing Then
                        scale = Integer.Parse(recordNode("scale").ToString())
                    End If

                    Select Case dataType.ToLower()

                        Case "int", "integer", "float", "number"

                            colType = "Number"

                        Case "varchar", "nvarchar", "string"

                            colType = "String"

                            'column.MaxLength = length

                        Case "datetime", "date"

                            colType = "DateTime"

                        Case "decimal", "numeric"

                            colType = "Number"

                            'column.precision = precision

                            'column.scale = scale

                        Case Else

                            ' Handle other data types as needed
                            colType = "String"

                    End Select

                    colOrg = columnName.Replace(" ", "_").ToLower ' Original Column Name
                    colName = columnName.Replace(" ", "_").ToLower
                    colHead = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(columnName.Replace("_", " "))

                    If colHead.Length > Proc_Imp.MaxColumnNameSize Then
                        Proc_Imp.ErrorStatus = True
                        ' Column heading length for {2} too large (Max {3}) in import file.  Row:{0} Col:{1}
                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM028", "ER_IM028", 0, colIndex, 0, colIndex, colHead, Proc_Imp.MaxColumnNameSize.ToString)
                    End If


                    For Each kvp As KeyValuePair(Of Integer, colDef) In sheetCols
                        If kvp.Value.colhead = colHead Then
                            ' colName = String.Format("{0}_{1}", colName, colIndex)
                            ' Duplicate Column heading {2} in import file. Row:{0} Col:{1}
                            Proc_Imp.ErrorStatus = True
                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM013", "ER_IM013", 0, colIndex, 0, colIndex, kvp.Value.colhead)
                            Exit For
                        End If
                    Next
                    ' Now override the Column Name with Col_nn
                    colName = "Col_{0}"
                    colName = String.Format(colName, colIndex + 1)

                    sheetCols.Add(colIndex, New colDef(colName, colType, length, colHead, colOrg, "0", "01", "0", ""))
                    colIndex = colIndex + 1
                    'Next

                Next
            End If
            ' Now we run through the JSON data looking at the type of data per cell to make a best guess.
            recordNodes = jsonDoc(RecordPointer)

            For Each recordNode In recordNodes

                ' Iterate through each property element of the record
                For Each propElement As JProperty In recordNode.Properties()

                    Dim elementName As String = propElement.Name
                    Dim elementValue As String = propElement.Value

                    colType = "String"
                    colValue = elementValue
                    colSize = 0

                    If IsNumeric(elementValue) Then
                        ' If number is not a decimal and has a leading zero then assume String
                        If Proc_Imp.DecCount(colValue) = 0 And Left(colValue, 1) = 0 Then
                            ImportCellType = "String"
                        Else
                            ImportCellType = "Numeric"
                        End If
                    Else
                        If IsDate(elementValue) Then
                            ImportCellType = "DateTime"
                        Else
                            ImportCellType = "String"
                        End If
                    End If

                    Select Case ImportCellType
                        Case "String"
                            colType = "String"
                            colValue = elementValue
                            colSize = Proc_Imp.RoundUpToX(elementValue.Length, 0)

                        Case "Numeric"
                            colType = "Number"
                            colValue = elementValue
                            colSize = Proc_Imp.DecCount(colValue)

                        Case "DateTime"
                            colType = "DateTime"
                            colValue = elementValue

                        Case Else
                            colType = "String"
                            colValue = elementValue
                            colSize = Proc_Imp.RoundUpToX(elementValue.Length, 0)
                    End Select

                    ' Loop round the existing columns to update the values, if column already set, else create new column
                    foundColumn = 999
                    For colIndex = 0 To sheetCols.Count - 1
                        ' Found this column 
                        If sheetCols(colIndex).colorg = elementName Then
                            foundColumn = colIndex
                            Exit For
                        End If
                    Next
                    ' Column Not Found
                    If foundColumn = 999 Then

                        colOrg = elementName.Replace(" ", "_").ToLower ' Original Column Name
                        colName = elementName.Replace(" ", "_").ToLower
                        colHead = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(elementName.Replace("_", " "))

                        If colHead.Length > Proc_Imp.MaxColumnNameSize Then
                            Proc_Imp.ErrorStatus = True
                            ' Column heading length for {2} too large (Max {3}) in import file.  Row:{0} Col:{1}
                            Proc_Imp.WriteImportMessage(SegmentID, "ER_IM028", "ER_IM028", 0, colIndex, 0, colIndex, colHead, Proc_Imp.MaxColumnNameSize.ToString)
                        End If

                        colIndex = sheetCols.Count

                        ' Now override the Column Name with Col_nn
                        colName = "Col_{0}"
                        colName = String.Format(colName, colIndex + 1)

                        sheetCols.Add(colIndex, New colDef(colName, colType, colSize, colHead, colOrg, "0", "01", "0", ""))

                    Else
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
                    End If
                Next

            Next

            'End Using

        Catch ex As Exception
            Proc_Imp.ErrorStatus = True
            'Error evaluating data types from import data
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM011", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols
    End Function

    Public Function JSONGetDatasourceName(ByVal DatasourcePointer As String, ByVal contentStream As Byte()) As String

        Dim jsonData As String
        Dim dataSource As String
        Dim jsonObj As JObject

        If DatasourcePointer = "" Then
            DatasourcePointer = "Datasource"
        End If

        jsonData = Proc_Imp.ReadDataStream(contentStream)

        jsonObj = JObject.Parse(jsonData)

        ' Get the contents of the Datasource element
        dataSource = jsonObj(DatasourcePointer).ToString()

        Return dataSource

    End Function

End Class
