Imports System.IO
Imports System.Xml

Public Class ParseXML

    Dim SQL As String
    Dim FieldPointer As String ' Pointer to where Field Information exists within the XML file
    Dim RecordPointer As String ' Pointer to where Record Information exists within the XML file

    Public Proc_Imp As ProcessImport

    Public Sub New(_Proc_Imp As ProcessImport)
        Proc_Imp = _Proc_Imp
    End Sub

    Public Function XMLProcessSegment(ByVal PassSegmentID As Integer) As DataTable

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
        Dim xmlData As String

        Dim SQLResult As Long
        Dim XMLDataTable As New DataTable
        Dim XMLSheetCols As Dictionary(Of Integer, colDef) = Nothing
        Dim DBSheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' Get next row to be processed
        Using DB As New IawDB

            SQL = "SELECT CI.unique_id, CI.client_id, CI.import_data,
                          CIS.seg_status, CIS.seg_id, CIS.prev_source_date, CIS.source_date, CIS.short_ref,
                          IsNull(CIS.source_id,-1) as source_id
                     FROM dbo.client_import CI
                          JOIN dbo.client_import_seg CIS
                            ON CI.unique_id = CIS.unique_id
                    WHERE CI.import_type IN ('04')
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
                    xmlData = Proc_Imp.ReadDataStream(ImportData)
                    '                    xmlData = Encoding.UTF8.GetString(ImportData)
                    'Dim _byteOrderMarkUtf8 As String = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble())
                    'If (xmlData.StartsWith(_byteOrderMarkUtf8)) Then
                    '    xmlData = xmlData.Remove(0, _byteOrderMarkUtf8.Length)
                    'End If

                    ' Retrieve from DB
                    ' FieldPointer = datRow.GetValue("field_pointer", "")
                    ' RecordPointer = datRow.GetValue("record_pointer", "")
                    ' For now fixed
                    'FieldPointer = "Fields"
                    FieldPointer = ""
                    RecordPointer = "Data"

                    ' This routine expects XML in this format, with or without the Fields section.
                    ' The Field and Record Pointer (above) can be changed if the format is basically the same.
                    '<Root>
                    '  <DataSource>Employee</DataSource>
                    '  <Fields>
                    '    <Field name = "person_ref" datatype="varchar" length="10" />
                    '    <Field name = "surname" datatype="varchar" length="35" />
                    '    <Field name = "forename1" datatype="varchar" length="35" />
                    '... etc
                    '  </Fields>
                    '  <Data>
                    '    <Record>
                    '      <person_ref>0000000001</person_ref>
                    '      <surname>Surname1</surname>
                    '      <forename1>Forename1</forename1>
                    '... etc
                    '    </Record>
                    '    <Record>
                    '      <person_ref>0000000002</person_ref>
                    '      <surname>Surname2</surname>
                    '      <forename1>Forename2</forename1>
                    '... etc
                    '    </Record>
                    '  </Data>
                    '</Root>

                    ' Get the Max Sizes from the clients table
                    Proc_Imp.GetSizes(ClientID)

                    Select Case SegmentStatus
                        Case "01" ' Unprocessed - Evaluate the datatypes of the spreadsheet

                            If ImportID <> 0 And ClientID IsNot "" And xmlData IsNot Nothing Then

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
                                    ' Update Datasource. Remember previous BasisID in case required later
                                    If SourceID > 0 Then
                                        ' Get column info for existing source from DB in order to compare against columns from new spreadsheet
                                        DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, PrevSourceDate, SegmentID)
                                    End If
                                End If

                                If Proc_Imp.ErrorStatus = False Then
                                    ' Get the column specification from the incoming spreadsheet
                                    XMLSheetCols = XMLGetColumnInfo(xmlData, ShortRef, SegmentID)

                                    If XMLSheetCols IsNot Nothing And Proc_Imp.ErrorStatus = False Then
                                        ' Now import the data into a datatable to run further checks

                                        XMLDataTable = XMLReadData(xmlData, 0, ShortRef, SegmentID, SourceDate)

                                        If Proc_Imp.ErrorStatus = False Then

                                            ' Set columns to unique if data is unique 
                                            For Each CD As colDef In XMLSheetCols.Values

                                                If CD.coltype = "String" Or (CD.coltype = "Number" And CD.collen = 0) Then
                                                    If Proc_Imp.isUnique(XMLDataTable, CD.colname) Then
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
                                                    ' Compare DB and XML Column specifications
                                                    SegmentStatus = Proc_Imp.CompareColumnSpecsVariable(SegmentID, DBSheetCols, XMLSheetCols)
                                                Else
                                                    ' New source Assumed
                                                    SegmentStatus = "03" ' Awaiting Approval
                                                End If
                                            End If
                                            If Proc_Imp.ErrorStatus = False Then
                                                ' Create a new source from this
                                                If SourceID > 0 Then
                                                    ' Check and change the column specifications order, incase it has changed
                                                    If Proc_Imp.ChangeColumnSpecsOrder(SegmentID, DBSheetCols, XMLSheetCols) = 99 Then
                                                        ' Unable to evaluate column specification from XML
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, "")
                                                    End If
                                                End If

                                                If Proc_Imp.ErrorStatus = False Then

                                                    SourceID = Proc_Imp.CreateSourceFromSpec(ClientID, SourceID, XMLSheetCols, ShortRef, SegmentID, SourceDate, SegmentStatus)

                                                    If SourceID = 0 Then
                                                        ' Unable to create new Datasource from XML
                                                        Proc_Imp.ErrorStatus = True
                                                        Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM005", 0, 0, "")
                                                    End If
                                                End If


                                            End If

                                        Else
                                            ' Unable to evaluate column specification from XML
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

                                ' Read the Data from spreadsheet.
                                XMLDataTable = XMLReadData(xmlData, SourceID, ShortRef, SegmentID, SourceDate)

                                ' Get column info for existing Basis from DB
                                DBSheetCols = Proc_Imp.DBGetColumnInfo(SourceID, SourceDate, SegmentID)

                                ' Create New Table and Insert the Data from the Spreadsheet.
                                Proc_Imp.CreateTableInsertData(SourceID, SegmentID, DBSheetCols, XMLDataTable, SourceDate, PrevSourceDate)

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
                                Proc_Imp.WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, ex, "")
                                'Finally
                                '   DB.Close()
                            End Try

                    End Select

                Next
            End If
            DB.Close()
        End Using

        Return XMLDataTable

    End Function

    Public Function XMLReadData(ByVal ContentData As String, ByVal sourceID As String, ByVal ShortRef As String, ByVal SegmentID As String, ByVal SourceDate As Date) As DataTable
        Dim datTable As DataTable = Nothing
        Dim datRow As DataRow
        Dim emptyDate As DateTime = DateTime.MinValue

        Dim sheetCols As Dictionary(Of Integer, colDef)

        Dim colIndex As Integer
        Dim colValue As Object

        Dim ImportCell As String
        Dim ImportCellType As String

        Dim xmlDoc As New XmlDocument()
        Dim recordNodes As XmlNodeList
        Dim recordNode As XmlNode
        Dim childNodes As XmlNodeList


        xmlDoc.LoadXml(ContentData)

        ' we assume that every file has a headings row
        Try

            If sourceID > 0 Then
                ' If source set get data from the DB.
                sheetCols = Proc_Imp.DBGetColumnInfo(sourceID, SourceDate, SegmentID)
            Else
                ' Still at evaluation stage Get the column details from the worksheet.
                sheetCols = XMLEvaluateDataTypes(ContentData, SegmentID)
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
            recordNodes = xmlDoc.SelectNodes("//" + RecordPointer)

            ' Loop round the record nodes
            For rowIndex As Integer = 0 To recordNodes.Count - 1
                ' Get an individual record node
                recordNode = recordNodes(rowIndex)

                ' Select all child nodes of the Record node
                childNodes = recordNode.ChildNodes

                ' Iterate through each child node
                For Each childNode As XmlNode In childNodes

                    ' now add the data into the table
                    datRow = datTable.NewRow()
                    ' Loop round the columns to get the items in the record by index
                    For colIndex = 0 To sheetCols.Count - 1

                        ' Access by element Number
                        'childNode = recordNode.SelectNodes("*")(colIndex)
                        'ImportCell = childNode.InnerText

                        ' Get the Element data (by Name)
                        If childNode.SelectSingleNode(sheetCols(colIndex).colorg) IsNot Nothing Then
                            ImportCell = childNode.SelectSingleNode(sheetCols(colIndex).colorg).InnerText
                        Else
                            ImportCell = ""
                            Select Case sheetCols(colIndex).coltype
                                Case "Number", "Numeric"
                                    ImportCell = "0"
                                Case "DateTime"
                                    ImportCell = "1900-01-01 00:00:00"
                            End Select
                        End If

                        ' Only for information, in case required by a different format XML.
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
                Next

            Next

        Catch ex As Exception
            ' Error Accessing Data from XML File
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM010", 0, 0, ex, "")
            Return Nothing
        End Try

        Return datTable
    End Function

    Public Function XMLGetColumnInfo(ByVal ContentData As String, ByVal ShortRef As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

        Dim sheetCols As Dictionary(Of Integer, colDef) = Nothing

        ' we assume that every file has a headings row
        Try

            sheetCols = XMLEvaluateDataTypes(ContentData, SegmentID)

        Catch ex As Exception
            ' Error Evaluating Datatypes from Spreadsheet
            Proc_Imp.ErrorStatus = True
            Proc_Imp.WriteImportMessage(CInt(SegmentID), "ER_IM041", "ER_IM011", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols

    End Function

    Public Function XMLEvaluateDataTypes(ByVal ContentData As String, ByVal SegmentID As String) As Dictionary(Of Integer, colDef)

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

        Dim xmlDoc As New XmlDocument()
        Dim recordNodes As XmlNodeList
        Dim recordNode As XmlNode
        Dim childNodes As XmlNodeList
        Dim elementNodes As XmlNodeList

        Dim columnName As String
        Dim dataType As String
        Dim length As Integer
        Dim precision As Integer
        Dim scale As Integer


        xmlDoc.LoadXml(ContentData)

        ' We assume that every sheet has a headings row
        Try
            sheetCols = New Dictionary(Of Integer, colDef)

            ' First we look for the field data descriptors if there are any
            If FieldPointer.Length > 0 Then
                recordNodes = xmlDoc.SelectNodes("//" + FieldPointer)

                colIndex = 0

                For Each recordNode In recordNodes

                    ' Select all child nodes of the Record node
                    childNodes = recordNode.ChildNodes

                    ' Iterate through each child node
                    For Each childNode As XmlNode In childNodes

                        columnName = "col_" + colIndex.ToString
                        dataType = ""
                        length = 0
                        precision = 0
                        scale = 0

                        If childNode.Attributes.GetNamedItem("name") IsNot Nothing Then
                            columnName = childNode.Attributes("name").Value
                        End If
                        If childNode.Attributes.GetNamedItem("datatype") IsNot Nothing Then
                            dataType = childNode.Attributes("datatype").Value
                        End If
                        If childNode.Attributes.GetNamedItem("length") IsNot Nothing Then
                            length = Integer.Parse(childNode.Attributes("length").Value)
                        End If
                        If childNode.Attributes.GetNamedItem("precision") IsNot Nothing Then
                            precision = Integer.Parse(childNode.Attributes("precision").Value)
                        End If
                        If childNode.Attributes.GetNamedItem("scale") IsNot Nothing Then
                            scale = Integer.Parse(childNode.Attributes("scale").Value)
                        End If

                        Select Case dataType.ToLower()

                            Case "int", "integer", "float", "number"

                                colType = "Number"

                            Case "varchar", "nvarchar"

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
                    Next

                Next
            End If
            ' Now we run through the XML data looking at the type of data per cell to make a best guess.
            recordNodes = xmlDoc.SelectNodes("//" + RecordPointer)

            For Each recordNode In recordNodes

                ' Select all child nodes of the Record node
                childNodes = recordNode.ChildNodes

                ' Iterate through each child node
                For Each childNode As XmlNode In childNodes

                    ' Select all child nodes of the Record node
                    elementNodes = childNode.ChildNodes

                    ' Iterate through each child node
                    For Each elementNode As XmlNode In elementNodes

                        ' Get the name and value of each element node
                        If elementNode.NodeType = XmlNodeType.Element Then
                            Dim elementName As String = elementNode.Name
                            Dim elementValue As String = elementNode.InnerText

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


                        End If
                    Next
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

    Public Function XMLGetDatasourceName(ByVal DatasourcePointer As String, ByVal contentStream As Byte()) As String

        Dim xmlData As String
        Dim xmlDoc As New XmlDocument()
        Dim dataSourceElement As XmlNode
        Dim dataSource As String

        If DatasourcePointer = "" Then
            DatasourcePointer = "DataSource"
        End If

        xmlData = Proc_Imp.ReadDataStream(contentStream)

        xmlDoc.LoadXml(xmlData)

        ' Find the DataSource element using "//"
        dataSourceElement = xmlDoc.SelectSingleNode("//" + DatasourcePointer)

        ' Get the contents of the DataSource element
        dataSource = dataSourceElement.InnerText

        Return dataSource

    End Function

End Class
