Imports System.IO
Imports System.Text

Public Class ProcessImport

    Dim SQL As String
    Public ErrorStatus As Boolean = False

    Dim Parce_Excel As New ParseExcel(Me)
    Dim Parce_CSV As New ParseCSV(Me)
    Dim Parce_XML As New ParseXML(Me)
    Dim Parce_JSON As New ParseJSON(Me)

    Public TableName As String = "",
           PrimarySource As String = "",
           DSName = "",
           MaxColumnNameSize As Integer = 50,
           MaxStringSize As Integer,
           MaxNumber As Decimal,
           MaxDataSize As Long,
           CurrentDataSize As Long,
           CurrentDataSetSize As Long,
           NewDataSetSize As Long,
           staticDataSource As Boolean,
           staticTempDate As Date,
           newRowCount As Long

    Public Function ProcessImportFile(ByVal PassSegmentID As Integer) As Integer
        Dim datRow As DataRow
        Dim datTable As DataTable
        Dim ImportID As Integer
        Dim ClientID As String
        Dim SegmentID As Integer
        Dim SegmentStatus As String
        Dim ImportType As String
        Dim SQLResult As Long
        Dim SetStatus As Boolean
        Dim ProcessedCount As Integer = 0
        Dim EmailAddress As String

        ' Get next row to be processed
        Using DB As New IawDB

            Do While True
                SQL = "SELECT TOP 1 CI.unique_id, CI.client_id, cis.seg_status, cis.seg_id, CI.import_type, CI.email_from
                             FROM dbo.client_import CI
                                  JOIN client_import_seg CIS
                                    ON CIS.unique_id = CI.unique_id 
                            WHERE CIS.seg_status IN ('01','04') 
                              AND (@p1 = 0
                               OR  (@p1 > 0 AND CIS.seg_id = @p1))
                            ORDER BY CI.batch_ident ASC, CI.unique_id ASC, CIS.seg_id ASC"
                datTable = DB.GetTable(SQL, PassSegmentID)

                If datTable IsNot Nothing Then
                    If datTable.Rows.Count > 0 Then
                        For Each datRow In datTable.Rows
                            ImportID = datRow.GetInt("unique_id")
                            ClientID = datRow.GetValue("client_id", "")
                            SegmentID = datRow.GetInt("seg_id")
                            SegmentStatus = datRow.GetValue("seg_status", "")
                            ImportType = datRow.GetValue("import_type", "")

                            ' Get the users default language (for any message text required)

                            EmailAddress = datRow.GetValue("email_from", "")

                            ' Count processed number
                            ProcessedCount = ProcessedCount + 1

                            SetStatus = False

                            Select Case ImportType
                                Case "01", "02" ' Excel XLS, XLSX

                                    Parce_Excel.ExcelProcessSegment(SegmentID)
                                    SetStatus = False

                                Case "03" ' CSV

                                    Parce_CSV.CSVProcessSegment(SegmentID)
                                    SetStatus = False

                                Case "04" ' XML

                                    Parce_XML.XMLProcessSegment(SegmentID)
                                    SetStatus = False

                                Case "05" ' JSON

                                    Parce_JSON.JSONProcessSegment(SegmentID)
                                    SetStatus = False

                                Case "06" ' Image
                                    ' No Processing for now just set status.
                                    SegmentStatus = "99" ' Error
                                    SetStatus = True
                                Case "99" ' Unknown
                                    ' No Processing for now just set status.
                                    SegmentStatus = "99" ' Error
                                    SetStatus = True
                            End Select

                            If SetStatus = True Then
                                WriteImportMessage(SegmentID, "ER_IM032", "ER_IM032", 0, 0, "")

                                SQL = "UPDATE client_import_seg
                                          SET seg_status = @p1
                                        WHERE unique_id = @p2
                                          AND seg_id = @p3 "
                                Try
                                    SQLResult = DB.NonQuery(SQL, SegmentStatus, ImportID, SegmentID)
                                Catch ex As Exception
                                    ' Unable to set import status
                                    WriteImportMessage(SegmentID, "ER_IM041", "ER_IM004", 0, 0, ex, "")
                                End Try

                            End If

                        Next

                    Else
                        ' No work found
                        Exit Do

                    End If

                Else
                    ' No work found
                    Exit Do

                End If

            Loop


        End Using

        Return ProcessedCount

    End Function

    Public Function DBGetColumnInfo(ByVal sourceID As Integer, ByVal SourceDate As DateTime, ByVal SegmentID As Integer) As Dictionary(Of Integer, colDef)

        Dim sheetCols As New Dictionary(Of Integer, colDef)
        Dim datRow As DataRow
        Dim datTable As DataTable

        Dim FieldCol As String
        Dim FieldType As String
        Dim FieldTypePUP As String
        Dim FieldLen As Integer  ' will be length of string or number of decimals for double type
        Dim FieldHead As String
        Dim FieldOrg As String ' Original column name
        Dim FieldUnique As String
        Dim FieldStatus As String
        Dim FieldParent As String
        Dim FieldFormat As String

        Dim colIndex As Integer = 0

        Dim outputDebugString As Boolean = False

        ' Retrieve DataType Info for source from DB

        Try

            Using DB As New IawDB
                'SQL = "select field_column, field_type, field_length, field_name, field_org, item_ref_field, field_status, parent_ref_field, field_format FROM data_source_field WHERE source_id = @p1 AND source_date = (SELECT MAX(dsf.source_date) FROM data_source_field dsf WHERE dsf.source_id = @p1 AND dsf.source_date <= @p2) ORDER BY field_num ASC"
                SQL = "select field_column, field_type, max(field_length) as field_length, field_name, field_org, item_ref_field, field_status, parent_ref_field, field_format FROM data_source_field WHERE source_id = @p1 GROUP BY field_num, field_column,field_type,field_name,field_org,item_ref_field,field_status,parent_ref_field,field_format  ORDER BY field_num ASC"
                datTable = DB.GetTable(SQL, sourceID, SourceDate)
                If datTable IsNot Nothing Then
                    For Each datRow In datTable.Rows
                        FieldCol = datRow.GetValue("field_column", "")
                        FieldTypePUP = datRow.GetValue("field_type", "")
                        Select Case FieldTypePUP
                            Case "01"
                                FieldType = "String"
                            Case "02"
                                FieldType = "DateTime"
                            Case "03"
                                FieldType = "Number"
                            Case "04"
                                FieldType = "Number"
                            Case "05"
                                FieldType = "Boolean"
                            Case Else
                                FieldType = "String"
                        End Select
                        FieldLen = datRow.GetValue("field_length", "")
                        FieldHead = datRow.GetValue("field_name", "")
                        FieldOrg = datRow.GetValue("field_org", "")
                        FieldUnique = datRow.GetValue("item_ref_field", "")
                        FieldStatus = datRow.GetValue("field_status", "")
                        FieldParent = datRow.GetValue("parent_ref_field", "")
                        FieldFormat = datRow.GetValue("field_format", "")

                        sheetCols.Add(colIndex, New colDef(FieldCol, FieldType, FieldLen, FieldHead, FieldOrg, FieldUnique, FieldStatus, FieldParent, FieldFormat))

                        colIndex = colIndex + 1

                    Next

                End If

            End Using

        Catch ex As Exception
            ErrorStatus = True
            ' Error loading datasource data types from DB
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM014", 0, 0, ex, "")
            Return Nothing
        End Try

        Return sheetCols

    End Function

    Public Function CreateSourceFromSpec(ByVal ClientID As String, ByVal IncomingSourceID As Integer, ByVal NewSpec As Dictionary(Of Integer, colDef), ByVal ShortRef As String, ByVal SegmentID As Integer, ByVal SourceDate As DateTime, ByRef SegmentStatus As String) As Integer

        Dim colIndex As Integer = 0
        Dim sourceID As Integer = 0
        Dim itemRefField As String = ""
        Dim colname As String
        Dim coltype As String
        Dim coltypePUP As String
        Dim collen As Integer  ' will be length of string or number of decimals for double type
        Dim colhead As String
        Dim colorg As String
        Dim colunique As String
        Dim fieldnum As Integer
        Dim sourceDesc As String
        Dim Primarysource As String = "0"
        Dim ZipPassword As String
        Dim DT As DataTable

        ' Create New source from the column specification

        Try

            If NewSpec.Count > 0 Then

                Using DB As New IawDB

                    If IncomingSourceID > 0 Then
                        ' Get the Description from the previous source
                        SQL = "SELECT source_desc, primary_source, zip_password 
                                 FROM dbo.data_source 
                                WHERE source_id = @p1 "
                        DT = DB.GetTable(SQL, IncomingSourceID)
                        If DT.Rows.Count > 0 Then
                            sourceDesc = DT.Rows(0).GetValue("source_desc", ShortRef)
                            Primarysource = DT.Rows(0).GetValue("primary_source", "0")
                            ZipPassword = DT.Rows(0).GetValue("zip_password", "")
                        Else
                            sourceDesc = ShortRef
                            Primarysource = "0"
                            ZipPassword = ""
                        End If

                        ' Get the Current item_ref_field (Key) column if there is one
                        SQL = "SELECT TOP 1 field_name 
                                 FROM dbo.data_source_field 
                                WHERE source_id = @p1 
                                  AND item_ref_field = '3' "
                        itemRefField = DB.Scalar(SQL, IncomingSourceID)
                        If itemRefField Is Nothing Then
                            itemRefField = ""
                        End If
                    Else
                        sourceDesc = ShortRef
                        itemRefField = ""
                        ' Check if a unique column exists. If so default source to 1 (Primary)
                        For colIndex = 0 To NewSpec.Count - 1
                            colunique = NewSpec(colIndex).colunique
                            If colunique = "1" Then ' This is the keyfield
                                Primarysource = "1"
                            End If
                        Next
                    End If

                    If IncomingSourceID = -1 Then
                        ' Only write out new data source if first time this data source has been processed (-1)
                        SQL = "INSERT INTO data_source (client_id ,source_desc, short_ref,primary_source)" + " VALUES (@p1,@p2,@p3,@p4);
                           SELECT SCOPE_IDENTITY();"
                        Try
                            sourceID = DB.Scalar(SQL, ClientID, sourceDesc, ShortRef, Primarysource)
                        Catch ex As Exception
                            ErrorStatus = True
                            ' Error inserting datasource details
                            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM015", 0, 0, ex, "")
                        End Try
                    Else
                        ' Existing data source
                        sourceID = IncomingSourceID
                    End If

                    ' Only write out new field specification if something has changed (14 internal only to indicate Length increase)
                    If sourceID > 0 And (SegmentStatus = "03" Or SegmentStatus = "14") Then

                        If SegmentStatus = "14" Then
                            SegmentStatus = "04"
                        End If

                        ' DELETE the current iteration of data_source_field in case this is a "static" datasource.
                        SQL = "DELETE FROM data_source_field WHERE source_id = @p1 AND source_date = @p2"
                        Try
                            DB.NonQuery(SQL, sourceID, SourceDate)
                        Catch ex As Exception
                            ErrorStatus = True
                            ' 'Error inserting datasource field details
                            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM016", 0, 0, ex, "")
                        End Try


                        For colIndex = 0 To NewSpec.Count - 1
                            collen = NewSpec(colIndex).collen
                            coltype = NewSpec(colIndex).coltype
                            Select Case coltype
                                Case "String"
                                    coltypePUP = "01"
                                Case "DateTime"
                                    coltypePUP = "02"
                                Case "Number"
                                    If collen > 0 Then
                                        coltypePUP = "04" ' Float
                                    Else
                                        coltypePUP = "03" ' Integer
                                    End If
                                Case "Boolean"
                                    coltypePUP = "05"
                                Case Else
                                    coltypePUP = "01"
                            End Select
                            colname = NewSpec(colIndex).colname
                            colhead = NewSpec(colIndex).colhead
                            colorg = NewSpec(colIndex).colorg
                            colunique = NewSpec(colIndex).colunique
                            If colhead = itemRefField And itemRefField <> "" Then
                                colunique = "3" ' This is the keyfield
                            End If
                            ' Extract the Integer from the DB column number (e.g. col_34) returns 34
                            If colname.Length >= 5 AndAlso Char.IsDigit(colname(4)) Then
                                fieldnum = Integer.Parse(colname.Substring(4))
                            Else
                                fieldnum = colIndex + 1
                            End If

                            SQL = "INSERT INTO data_source_field (source_id ,source_date, field_num, field_name, field_column, field_org, field_length, field_type, item_ref_field)
                                        VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9)"
                            Try
                                DB.NonQuery(SQL, sourceID, SourceDate, fieldnum, colhead, colname, colorg, collen, coltypePUP, colunique)
                            Catch ex As Exception
                                ErrorStatus = True
                                ' 'Error inserting datasource field details
                                WriteImportMessage(SegmentID, "ER_IM041", "ER_IM016", 0, 0, ex, "")
                            End Try

                        Next

                    End If

                End Using

            Else
                ' No Columns found
                sourceID = 0

            End If

        Catch ex As Exception
            ErrorStatus = True
            ' Error creating new datasource
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM027", 0, 0, ex, "")
            Return 0
        End Try

        Return sourceID

    End Function

    Public Function CompareColumnSpecs(ByVal SegmentID As Integer, ByVal OldSpec As Dictionary(Of Integer, colDef), ByRef NewSpec As Dictionary(Of Integer, colDef)) As String

        Dim retValue As String = "00"   ' 04 - All OK, 03 - Approval Required, 14 (Internal Only) - Field Length Increased, 99 - Error
        Dim colIndex As Integer = 0
        Dim colLenChanged As Boolean = False

        ' Compare old and new column specifications
        ' NOTE THIS IS NO LONGER USED AS COLUMNS MAY COME AND GO OR BE IN A DIFFERENT ORDER. See ompareColumnSpecsVariable below.

        Try

            If OldSpec.Count > NewSpec.Count Then
                ErrorStatus = True
                ' Columns are missing from the new datasource specification
                WriteImportMessage(SegmentID, "ER_IM019", "ER_IM019", 0, 0, "")
                retValue = "99" ' Error
            End If

            If ErrorStatus = False Then

                retValue = "04" ' All OK
                For colIndex = 0 To OldSpec.Count - 1

                    ' Allow new column length to be larger for the new Specification.
                    'If OldSpec(colIndex).colname <> NewSpec(colIndex).colname Or
                    If OldSpec(colIndex).colhead <> NewSpec(colIndex).colhead Then
                        ' Column details have changed for the new specification
                        ErrorStatus = True
                        WriteImportMessage(SegmentID, "ER_IM017", "ER_IM017", 0, 0, " (" + OldSpec(colIndex).colhead + ", " + OldSpec(colIndex).colname + ", " + OldSpec(colIndex).coltype + ") " +
                                                                          "-> (" + NewSpec(colIndex).colhead + ", " + NewSpec(colIndex).colname + ", " + NewSpec(colIndex).coltype + ")")
                        retValue = "99" ' Error
                    Else
                        If OldSpec(colIndex).coltype <> NewSpec(colIndex).coltype Then
                            Select Case OldSpec(colIndex).coltype
                                Case "Number"
                                    If NewSpec(colIndex).coltype.ContainsOneOf("String", "DateTime", "Boolean") Then
                                        ErrorStatus = True
                                    End If
                                Case "DateTime"
                                    If NewSpec(colIndex).coltype.ContainsOneOf("String", "Number", "Boolean") Then
                                        ErrorStatus = True
                                    End If
                                Case "Boolean" ' Allow Boolean to become a String
                                    If NewSpec(colIndex).coltype.ContainsOneOf("DateTime", "Number") Then
                                        ErrorStatus = True
                                    End If
                                Case "String"
                                    If NewSpec(colIndex).coltype.ContainsOneOf("DateTime") Then
                                        ErrorStatus = True
                                    ElseIf NewSpec(colIndex).coltype.ContainsOneOf("Boolean", "Number") Then
                                        ' Convert Boolean or Number type to a String
                                        NewSpec(colIndex).coltype = OldSpec(colIndex).coltype
                                    End If
                            End Select

                            If ErrorStatus = True Then
                                ' Column details have changed for the new specification
                                ErrorStatus = True
                                WriteImportMessage(SegmentID, "ER_IM017", "ER_IM017", 0, 0, "(" + OldSpec(colIndex).colhead + ", " + OldSpec(colIndex).colname + ", " + OldSpec(colIndex).coltype + ") " +
                                                                          "-> (" + NewSpec(colIndex).colhead + ", " + NewSpec(colIndex).colname + ", " + NewSpec(colIndex).coltype + ")")
                                retValue = "99" ' Error
                            End If
                        End If
                    End If
                    If ErrorStatus = False Then
                        If OldSpec(colIndex).collen < NewSpec(colIndex).collen Then
                            ' Column Length Increased 
                            retValue = "14" ' All OK - Approval is not required - But column length increased. Change to 04 later. 
                            NewSpec(colIndex).collenincrease = "1"
                            colLenChanged = True
                        Else
                            ' Column Length Less or Same
                            NewSpec(colIndex).collen = OldSpec(colIndex).collen
                            retValue = "04" ' All OK, but must use previous length
                        End If
                    End If
                    If ErrorStatus = False Then
                        If OldSpec(colIndex).colunique <> NewSpec(colIndex).colunique Then
                            If OldSpec(colIndex).colunique = "3" Then
                                If NewSpec(colIndex).colunique = "0" Then
                                    ' Column is no longer unique - Reject
                                    ErrorStatus = True
                                    WriteImportMessage(SegmentID, "ER_IM018", "ER_IM018", 0, 0, NewSpec(colIndex).colhead)
                                    retValue = "99" ' Error
                                Else
                                    NewSpec(colIndex).colunique = "3"
                                End If
                            End If
                        End If
                    End If
                Next
            End If

            If colLenChanged = True Then
                retValue = "14" ' All OK - Approval is not required - But column length increased. Change to 04 later. 
            End If

            ' Now check if there are any new columns to report on
            If OldSpec.Count < NewSpec.Count Then
                For colIndex = OldSpec.Count To NewSpec.Count - 1
                    NewSpec(colIndex).colnew = "1"
                    NewSpec(colIndex).colstatus = "01" ' New
                    ' New field added to datasource. {0}
                    WriteImportMessage(SegmentID, "IN_IM001", "IN_IM001", 0, 0, " (" + NewSpec(colIndex).colhead + ", " + NewSpec(colIndex).coltype + ")")
                Next
                ' Number of columns changed.
                retValue = "03" ' Approval Required
            End If

        Catch ex As Exception
            ' Error comparing column specificaions
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, ex, "")
            retValue = "99" ' Error
        End Try

        Return retValue

    End Function

    Public Function CompareColumnSpecsVariable(ByVal SegmentID As Integer, ByVal OldSpec As Dictionary(Of Integer, colDef), ByRef NewSpec As Dictionary(Of Integer, colDef)) As String

        Dim retValue As String = "00"   ' 04 - All OK, 03 - Approval Required, 14 (Internal Only) - Field Length Increased, 99 - Error
        Dim colIndex As Integer
        Dim colNewIndex As Integer
        Dim foundOldIndex As Integer
        Dim colLenChanged As Boolean = False
        Dim newColumnAdded As Boolean = False

        ' Compare old and new column specifications

        Try

            If ErrorStatus = False Then

                retValue = "04" ' All OK
                For colNewIndex = 0 To NewSpec.Count - 1
                    foundOldIndex = 999
                    For colIndex = 0 To OldSpec.Count - 1
                        If OldSpec(colIndex).colorg = NewSpec(colNewIndex).colorg Then
                            foundOldIndex = colIndex
                            Exit For
                        End If
                    Next

                    If foundOldIndex = 999 Then
                        ' New column to report on
                        NewSpec(colNewIndex).colnew = "1"
                        ' New field added to datasource. {0}
                        WriteImportMessage(SegmentID, "IN_IM001", "IN_IM001", 0, 0, " (" + NewSpec(colNewIndex).colhead + ", " + NewSpec(colNewIndex).coltype + ")")
                        ' Number of columns changed.
                        newColumnAdded = True
                        retValue = "03" ' Approval Required
                    Else
                        ' Existing Column
                        OldSpec(foundOldIndex).colmatched = "1"

                        ' Allow new column length to be larger for the new Specification.
                        If OldSpec(foundOldIndex).colhead <> NewSpec(colNewIndex).colhead Then
                            ' Column details have changed for the new specification
                            ErrorStatus = True

                            WriteImportMessage(SegmentID, "ER_IM017", "ER_IM017", 0, 0, "(" + OldSpec(foundOldIndex).colhead + ", " + OldSpec(foundOldIndex).colname + ", " + OldSpec(foundOldIndex).coltype + ") " +
                                                                              "-> (" + NewSpec(colNewIndex).colhead + ", " + NewSpec(colNewIndex).colname + ", " + NewSpec(colNewIndex).coltype + ")")
                            retValue = "99" ' Error

                        Else
                            If OldSpec(foundOldIndex).coltype <> NewSpec(colNewIndex).coltype Then
                                Select Case OldSpec(foundOldIndex).coltype
                                    Case "Number"
                                        If NewSpec(colNewIndex).coltype.ContainsOneOf("String", "DateTime", "Boolean") Then
                                            ErrorStatus = True
                                        End If
                                    Case "DateTime"
                                        If NewSpec(colNewIndex).coltype.ContainsOneOf("String", "Number", "Boolean") Then
                                            ErrorStatus = True
                                        End If
                                    Case "Boolean" ' Allow Boolean to become a String
                                        If NewSpec(colNewIndex).coltype.ContainsOneOf("DateTime", "Number") Then
                                            ErrorStatus = True
                                        End If
                                    Case "String"
                                        If NewSpec(colNewIndex).coltype.ContainsOneOf("DateTime") Then
                                            ErrorStatus = True
                                        ElseIf NewSpec(colNewIndex).coltype.ContainsOneOf("Boolean", "Number") Then
                                            ' Convert Boolean or Number type to a String
                                            NewSpec(colNewIndex).coltype = OldSpec(foundOldIndex).coltype
                                        End If
                                End Select

                                If ErrorStatus = True Then
                                    ' Column details have changed for the new specification
                                    ErrorStatus = True
                                    WriteImportMessage(SegmentID, "ER_IM017", "ER_IM017", 0, 0, "(" + OldSpec(foundOldIndex).colhead + ", " + OldSpec(foundOldIndex).colname + ", " + OldSpec(foundOldIndex).coltype + ") " +
                                                                              "-> (" + NewSpec(colNewIndex).colhead + ", " + NewSpec(colNewIndex).colname + ", " + NewSpec(colNewIndex).coltype + ")")
                                    retValue = "99" ' Error
                                End If
                            End If
                        End If
                        If ErrorStatus = False Then
                            If OldSpec(foundOldIndex).collen < NewSpec(colNewIndex).collen Then
                                ' Column Length Increased 
                                retValue = "14" ' All OK - Approval is not required - But column length increased. Change to 04 later. 
                                NewSpec(colIndex).collenincrease = "1"
                                colLenChanged = True
                            Else
                                ' Column Length Less or Same
                                NewSpec(colNewIndex).collen = OldSpec(foundOldIndex).collen
                                retValue = "04" ' All OK, but must use previous length
                            End If
                        End If
                        If ErrorStatus = False Then
                            If OldSpec(foundOldIndex).colunique <> NewSpec(colNewIndex).colunique Then
                                If OldSpec(foundOldIndex).colunique = "3" Then
                                    If NewSpec(colNewIndex).colunique = "0" Then
                                        ' Column is no longer unique - Reject
                                        ErrorStatus = True
                                        WriteImportMessage(SegmentID, "ER_IM018", "ER_IM018", 0, 0, NewSpec(colNewIndex).colhead)
                                        retValue = "99" ' Error
                                    Else
                                        NewSpec(colNewIndex).colunique = "3"
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            End If

            ' Now loop to see if old columns matched. If not write a message.
            For colIndex = 0 To OldSpec.Count - 1

                If OldSpec(colIndex).colmatched <> "1" Then
                    If OldSpec(colIndex).colunique = "3" Then
                        ' Key data column missing for Primary Data Source
                        ErrorStatus = True
                        WriteImportMessage(SegmentID, "ER_IM022", "ER_IM022", 0, 0, "")
                        retValue = "99" ' Error
                        Exit For
                    End If
                    ' Field removed from Data Source import file. {0}
                    WriteImportMessage(SegmentID, "ER_IM041", "IN_IM009", 0, 0, " (" + OldSpec(colIndex).colhead + ", " + OldSpec(colIndex).coltype + ")")

                End If
            Next

            If colLenChanged = True Then
                retValue = "14" ' All OK - Approval is not required - But column length increased. Change to 04 later. 
            End If

            If newColumnAdded = True Then
                retValue = "03" ' Approval Required
            End If

        Catch ex As Exception
            ' Error comparing column specificaions
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, ex, "")
            retValue = "99" ' Error
        End Try

        Return retValue

    End Function


    Public Function ChangeColumnSpecsOrder(ByVal SegmentID As Integer, ByVal OldSpec As Dictionary(Of Integer, colDef), ByRef NewSpec As Dictionary(Of Integer, colDef)) As String

        Dim retValue As String = "00"   ' 04 - All OK, 99 - Error
        Dim colIndex As Integer
        Dim colNewIndex As Integer
        Dim foundOldIndex As Integer
        Dim newColumnCounter As Integer
        Dim colName As String

        ' Change the order of the columns if things have changed

        Try

            newColumnCounter = OldSpec.Count

            For colNewIndex = 0 To NewSpec.Count - 1
                foundOldIndex = 999
                For colIndex = 0 To OldSpec.Count - 1
                    If OldSpec(colIndex).colorg = NewSpec(colNewIndex).colorg Then
                        foundOldIndex = colIndex
                        Exit For
                    End If
                Next

                If foundOldIndex = 999 Then
                    ' New column to report on
                    NewSpec(colNewIndex).colnew = "1"
                    ' Number of columns changed.
                    newColumnCounter = newColumnCounter + 1
                    colName = "Col_{0}"
                    colName = String.Format(colName, newColumnCounter)
                    NewSpec(colNewIndex).colname = colName

                Else
                    ' Existing Column
                    OldSpec(foundOldIndex).colmatched = "1"
                    NewSpec(colNewIndex).colname = OldSpec(foundOldIndex).colname
                    If OldSpec(foundOldIndex).colstatus = "03" Then ' Deleted 
                        NewSpec(colNewIndex).colstatus = "02" ' Set back to to accepted if back in use
                    Else
                        NewSpec(colNewIndex).colstatus = OldSpec(foundOldIndex).colstatus
                    End If
                    NewSpec(colNewIndex).colparent = OldSpec(foundOldIndex).colparent
                    NewSpec(colNewIndex).colformat = OldSpec(foundOldIndex).colformat
                End If
            Next

            ' Now loop to see if old columns matched. If not add to new specification, Mark as status deleted "03"
            For colIndex = 0 To OldSpec.Count - 1

                If OldSpec(colIndex).colmatched <> "1" Then
                    NewSpec.Add(NewSpec.Count, New colDef(OldSpec(colIndex).colname, OldSpec(colIndex).coltype, OldSpec(colIndex).collen, OldSpec(colIndex).colhead, OldSpec(colIndex).colorg, OldSpec(colIndex).colunique, "03", OldSpec(colIndex).colparent, OldSpec(colIndex).colformat))
                End If
            Next

        Catch ex As Exception
            ' Error comparing column specificaions
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM020", 0, 0, ex, "")
            retValue = "99" ' Error
        End Try

        Return retValue

    End Function



    Public Function CreateTableInsertData(ByVal SourceID As Integer, ByVal SegmentID As Integer, ByVal ColumnSpec As Dictionary(Of Integer, colDef), ByVal DatTable As DataTable, SourceDate As Date, PrevSourceDate As Date) As Integer

        Dim colIndex As Integer
        Dim colname As String
        Dim coltype As String
        Dim collen As Integer  ' will be length of string or number of decimals for double type
        Dim colhead As String
        Dim colunique As String
        Dim collenincrease As String ' Will be set to '1' if column length has been increased for existing data source
        Dim colnew As String ' Will be set to '1' if this is an additional column

        Dim DBcoltype As String
        Dim itemRefField As String = ""
        Dim itemRefType As String
        Dim itemRefSpec As String
        Dim itemRefData As String
        Dim NonUniqueIndexFound As Boolean = False
        Dim NonUniqueIndexField As String = ""
        Dim PreviousCount As Integer
        Dim NewCount As Integer
        Dim SizeofInsertData As Double = 0

        Dim outputCreate As String = ""
        Dim outputAlterAdd As String = ""
        Dim outputAlterSize As String = ""
        Dim outputInsert As String
        Dim outputInsertCols As String = ""
        Dim outputInsertData As String
        Dim outputInsertListData As New List(Of String)
        Dim outputInsertListObject As New List(Of Object)
        Dim tempString As String
        Dim colNameList As String = ""
        Dim reverseChanges As Boolean
        Dim schemaChange As Boolean = False
        Dim colNameQuoteList As String = ""
        Dim PrevDataDate As Date = Nothing

        Dim DT As DataTable

        Dim TempColumnList As String = "column_name,column_default,is_nullable,data_type,character_maximum_length, numeric_precision"        ' Create New source from the column specification

        Try

            If ColumnSpec.Count > 0 Then

                Using DB As New IawDB

                    DB.TranBegin()

                    ' Get the Table Name from the source
                    SQL = "SELECT table_name, primary_source, short_ref
                             FROM dbo.data_source 
                            WHERE source_id = @p1 "
                    DT = DB.GetTable(SQL, SourceID)
                    If DT.Rows.Count > 0 Then
                        TableName = DT.Rows(0).GetValue("table_name", "")
                        PrimarySource = DT.Rows(0).GetValue("primary_source", "01")
                        DSName = DT.Rows(0).GetValue("short_ref", "")
                    Else
                        ' Error creating datasource in DB. 
                        ErrorStatus = True
                        WriteImportMessage(SegmentID, "ER_IM041", "ER_IM025", 0, 0, "")
                    End If

                    If ErrorStatus = False Then

                        ' Default item_ref to a non-primary source. The key defaults to a guid
                        itemRefSpec = "__item_ref varchar(36) default (replace(convert(varchar(36),newid()),'-','')) "

                        For colIndex = 0 To ColumnSpec.Count - 1
                            coltype = ColumnSpec(colIndex).coltype
                            collen = ColumnSpec(colIndex).collen
                            colname = ColumnSpec(colIndex).colname
                            colhead = ColumnSpec(colIndex).colhead
                            colunique = ColumnSpec(colIndex).colunique
                            collenincrease = ColumnSpec(colIndex).collenincrease
                            colnew = ColumnSpec(colIndex).colnew

                            ' Re-get the colincrease and colnew flags if not already set if previous data set exists
                            If PrevSourceDate.Ticks > 0 Then

                                If colnew = "0" Then
                                    ' See if column previously existed
                                    SQL = "SELECT COUNT(1) as field_count
                                             FROM dbo.data_source_field 
                                            WHERE source_id = @p1 
                                              AND source_date = @p2
                                              AND field_column = @p3"
                                    DT = DB.GetTable(SQL, SourceID, PrevSourceDate, colname)
                                    If DT.Rows.Count > 0 Then
                                        If DT.Rows(0).GetValue("field_count", 0) = 0 Then
                                            colnew = "1"
                                        End If

                                    End If
                                End If

                                If colnew = "0" And collenincrease = "0" Then
                                    ' Get the previous collen to compare if not a new column
                                    SQL = "SELECT TOP 1 field_length
                                             FROM dbo.data_source_field 
                                            WHERE source_id = @p1 
                                              AND source_date = @p2
                                              AND field_column = @p3"
                                    DT = DB.GetTable(SQL, SourceID, PrevSourceDate, colname)
                                    If DT.Rows.Count > 0 Then
                                        If DT.Rows(0).GetValue("field_length", 0) < collen Then
                                            collenincrease = "1"
                                        End If
                                    End If
                                End If

                            End If

                            Select Case coltype
                                Case "Number"
                                    If collen > 0 Then
                                        DBcoltype = "Numeric(18,6)"
                                    Else
                                        DBcoltype = "Numeric(10,0)"
                                    End If
                                Case "DateTime"
                                    DBcoltype = "Datetime"
                                Case "Boolean"
                                    DBcoltype = "nVarchar"
                                Case "String"
                                    DBcoltype = "nVarchar"
                                Case Else
                                    DBcoltype = "nVarchar"
                            End Select

                            ' Check if column is marked as the item_ref (primary key)
                            If colunique = "3" Then
                                ' This is the keyfield
                                itemRefField = colname
                                itemRefType = coltype
                                'DBitemRefType = DBcoltype
                                ' Override the key column if it's a primary source
                                If PrimarySource = "1" Then
                                    itemRefSpec = "__item_ref as " + itemRefField
                                    'itemRefSpec = "__item_ref " + DBitemRefType
                                    'If DBitemRefType = "nVarchar" Then
                                    '    itemRefSpec = itemRefSpec + "(" + collen.ToString + ")"                                        'Numeric(10,0) IDENTITY(1,1) primary key
                                    'End If
                                    itemRefSpec = itemRefSpec + " PERSISTED not null "
                                End If

                            End If

                            ' Found the key field for a non-primary datasource (source). Remember it.
                            If colunique = "2" And PrimarySource = "0" Then
                                NonUniqueIndexFound = True
                                NonUniqueIndexField = colname
                            End If

                            outputCreate = outputCreate + colname + " " + DBcoltype
                            outputInsertCols = outputInsertCols + colname
                            If DBcoltype = "nVarchar" Then
                                If collen < 1 Then
                                    collen = 5
                                End If
                                outputCreate = outputCreate + "(" + collen.ToString + ")"
                            End If
                            If ColumnSpec.Count - 1 > colIndex Then

                                outputCreate = outputCreate + ", "
                                outputInsertCols = outputInsertCols + ", "

                            End If

                            If colnew = "1" Then
                                schemaChange = True
                                outputAlterAdd = outputAlterAdd + colname + " " + DBcoltype
                                If DBcoltype = "nVarchar" Then
                                    outputAlterAdd = outputAlterAdd + "(" + collen.ToString + ")"
                                End If
                                outputAlterAdd = outputAlterAdd + ", "
                            End If

                            If collenincrease = "1" Then
                                schemaChange = True
                                outputAlterSize = outputAlterSize + colname + " " + DBcoltype
                                If DBcoltype = "nVarchar" Then
                                    outputAlterSize = outputAlterSize + "(" + collen.ToString + ")"
                                End If
                                outputAlterSize = outputAlterSize + ", "
                            End If

                        Next

                        If PrimarySource = "1" And itemRefField = "" Then
                            ' Error creating datasource in DB. Key data column missing for primary datasource
                            ErrorStatus = True
                            WriteImportMessage(SegmentID, "ER_IM022", "ER_IM022", 0, 0, "")
                        End If

                        If ErrorStatus = False Then

                            outputCreate = "IF object_id('" + TableName + "') is null CREATE TABLE " + TableName + " (" + itemRefSpec + ", __start_date datetime not null, " + outputCreate + ", PRIMARY KEY (__start_date, __item_ref))"

                            SQL = outputCreate

                            Try
                                DB.NonQuery(SQL)
                            Catch ex As Exception
                                ' Error creating datasource in DB. Error creating datasource table
                                ErrorStatus = True
                                WriteImportMessage(SegmentID, "ER_IM041", "ER_IM023", 0, 0, ex, "")
                            End Try

                            If ErrorStatus = False Then
                                ' Key field for a non-primary datasource (source) has been defined. Set an index for it
                                If NonUniqueIndexFound = True Then
                                    'NonUniqueIndexField
                                    outputCreate = "IF Not Exists (select 1 from sys.indexes where name = 'ix_" + TableName + "') CREATE NONCLUSTERED INDEX ix_" + TableName + " ON dbo." + TableName + " (" + NonUniqueIndexField + ")"
                                    SQL = outputCreate

                                    Try
                                        DB.NonQuery(SQL)
                                    Catch ex As Exception
                                        ' Error creating index for non-primary datasource in DB
                                        ErrorStatus = True
                                        WriteImportMessage(SegmentID, "ER_IM041", "ER_IM031", 0, 0, ex, "")
                                    End Try

                                End If
                            End If

                            ' If Schema has changed issue the relevant Alter statements
                            If schemaChange = True Then
                                If ErrorStatus = False Then
                                    If outputAlterAdd.Length > 0 Then
                                        outputAlterAdd = Left(outputAlterAdd, outputAlterAdd.Length - 2) ' remove last comma
                                        outputAlterAdd = "ALTER TABLE " + TableName + " ADD " + outputAlterAdd
                                        SQL = outputAlterAdd

                                        Try
                                            DB.NonQuery(SQL)
                                        Catch ex As Exception
                                            ' Error creating datasource in DB. Error creating datasource table
                                            ErrorStatus = True
                                            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM023", 0, 0, ex, "")
                                        End Try
                                    End If
                                End If

                                If ErrorStatus = False Then
                                    If outputAlterSize.Length > 0 Then
                                        outputAlterSize = Left(outputAlterSize, outputAlterSize.Length - 2) ' remove last comma
                                        outputAlterSize = "ALTER TABLE " + TableName + " ALTER COLUMN " + outputAlterSize
                                        SQL = outputAlterSize

                                        Try
                                            DB.NonQuery(SQL)
                                        Catch ex As Exception
                                            ' Error creating datasource in DB. Error creating datasource table
                                            ErrorStatus = True
                                            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM023", 0, 0, ex, "")
                                        End Try
                                    End If
                                End If
                            End If

                            If ErrorStatus = False Then

                                ' If the data source is to be replaced, get the size of the old set of data.
                                staticDataSource = False

                                ' Find the previous date of dataset
                                SQL = "Select TOP 1 __start_date
                                     from " + TableName + " 
                                    where __start_date <= @p1
                                    order by __start_date DESC"

                                DT = DB.GetTable(SQL, SourceDate)
                                If DT.Rows.Count > 0 Then
                                    PrevDataDate = DT.Rows(0).GetDate("__start_date")
                                Else
                                    PrevDataDate = Nothing

                                End If

                                If SourceDate = PrevDataDate And PrevDataDate.Ticks > 0 Then
                                    staticDataSource = True
                                    staticTempDate = DateAdd("yyyy", 100, PrevDataDate) ' Previous (same) date + 100 years.
                                    CurrentDataSetSize = GetDataImportSize(TableName, PrevDataDate)

                                    ' DELETE the future iteration of the data if left lying around previously as this is a "static" datasource.
                                    SQL = "DELETE FROM " + TableName + " WHERE __start_date = @p1 "
                                    Try
                                        DB.NonQuery(SQL, staticTempDate)
                                    Catch ex As Exception
                                        ErrorStatus = True
                                        ' 'Error inserting (deleting) datasource field details
                                        WriteImportMessage(SegmentID, "ER_IM041", "ER_IM024", 0, 0, ex, "")
                                    End Try

                                    ' UPDATE the current iteration of the data with a temporary date as this is a "static" datasource.
                                    SQL = "UPDATE " + TableName + " SET __start_date = @p2 WHERE __start_date = @p1 "
                                    Try
                                        DB.NonQuery(SQL, SourceDate, staticTempDate)
                                    Catch ex As Exception
                                        ErrorStatus = True
                                        ' 'Error inserting (updating) datasource field details
                                        WriteImportMessage(SegmentID, "ER_IM041", "ER_IM024", 0, 0, ex, "")
                                    End Try
                                End If

                                newRowCount = 0

                                For Each DR As DataRow In DatTable.Rows

                                    outputInsertData = ""

                                    Dim argCounter As Integer
                                    Dim foundColumn As Integer
                                    Dim colSpecName As String
                                    Dim colDataName As String

                                    ' New Object Method of outputting data.
                                    outputInsertListData.Clear()
                                    outputInsertListObject.Clear()
                                    argCounter = 1
                                    outputInsertListData.Add("@p1")
                                    outputInsertListObject.Add(SourceDate)

                                    ' Original
                                    'For Each col As DataColumn In DatTable.Columns
                                    '    argCounter = argCounter + 1
                                    '    ' Build the argument list
                                    '    outputInsertListData.Add("@p" + argCounter.ToString)
                                    '    ' Build the object data list
                                    '    ' Only check for nothing (null) not empty string
                                    '    If DR(col.ColumnName) Is Nothing Then 'Or DR(col.ColumnName).ToString = "" Then
                                    '        outputInsertListObject.Add(Nothing)
                                    '    Else
                                    '        Select Case col.DataType.ToString
                                    '            Case "System.String"
                                    '                outputInsertListObject.Add(DR(col.ColumnName).ToString.Replace("'", "''"))
                                    '                tempString = DR(col.ColumnName).ToString.Replace("'", "''")
                                    '            Case "System.Boolean"
                                    '                outputInsertListObject.Add(DR(col.ColumnName).ToString)
                                    '                tempString = DR(col.ColumnName).ToString
                                    '            Case "System.Double"
                                    '                outputInsertListObject.Add(CDec(DR(col.ColumnName)))
                                    '                tempString = CDec(DR(col.ColumnName))
                                    '            Case "System.DateTime"
                                    '                outputInsertListObject.Add(CDate(DR(col.ColumnName)).ToString("yyyy-MM-dd HH:mm:ss"))
                                    '                tempString = CDate(DR(col.ColumnName)).ToString("yyyy-MM-dd HH:mm:ss")
                                    '            Case Else
                                    '                outputInsertListObject.Add(DR(col.ColumnName).ToString.Replace("'", "''"))
                                    '                tempString = DR(col.ColumnName).ToString.Replace("'", "''")
                                    '        End Select
                                    '        If PrimarySource = "1" And col.ColumnName = itemRefField Then
                                    '            itemRefData = tempString
                                    '        End If
                                    '    End If

                                    'Next

                                    ' Replacement for any column order - Loop round the column spec.
                                    For colIndex = 0 To ColumnSpec.Count - 1
                                        foundColumn = 999 ' Not Found
                                        argCounter = argCounter + 1
                                        ' Build the argument list
                                        outputInsertListData.Add("@p" + argCounter.ToString)
                                        colSpecName = ColumnSpec(colIndex).colname.ToLower
                                        ' Search for the relevant data
                                        For Each col As DataColumn In DatTable.Columns
                                            colDataName = col.ColumnName.ToLower
                                            If colSpecName = colDataName Then
                                                foundColumn = 1 ' Data found
                                                ' Build the object data list
                                                ' Only check for nothing (null) not empty string
                                                If DR(col.ColumnName) Is Nothing Then
                                                    outputInsertListObject.Add(Nothing)
                                                Else
                                                    Select Case col.DataType.ToString
                                                        Case "System.String"
                                                            outputInsertListObject.Add(DR(col.ColumnName).ToString.Replace("'", "''"))
                                                            tempString = DR(col.ColumnName).ToString.Replace("'", "''")
                                                        Case "System.Boolean"
                                                            outputInsertListObject.Add(DR(col.ColumnName).ToString)
                                                            tempString = DR(col.ColumnName).ToString
                                                        Case "System.Double"
                                                            outputInsertListObject.Add(CDec(DR(col.ColumnName)))
                                                            tempString = CDec(DR(col.ColumnName))
                                                        Case "System.DateTime"
                                                            outputInsertListObject.Add(CDate(DR(col.ColumnName)).ToString("yyyy-MM-dd HH:mm:ss"))
                                                            tempString = CDate(DR(col.ColumnName)).ToString("yyyy-MM-dd HH:mm:ss")
                                                        Case Else
                                                            outputInsertListObject.Add(DR(col.ColumnName).ToString.Replace("'", "''"))
                                                            tempString = DR(col.ColumnName).ToString.Replace("'", "''")
                                                    End Select
                                                    If PrimarySource = "1" And col.ColumnName = itemRefField Then
                                                        itemRefData = tempString
                                                    End If
                                                End If

                                                Exit For
                                            End If
                                        Next
                                        If foundColumn = 999 Then
                                            outputInsertListObject.Add(Nothing)
                                        End If

                                    Next


                                    ' Add the key to the Insert List
                                    'If Primarysource = "1" Then
                                    '    argCounter = argCounter + 1
                                    '    ' Build the argument list
                                    '    outputInsertListData.Add("@p" + argCounter.ToString)
                                    '    outputInsertListObject.Add(itemRefData)
                                    '    outputInsertCols = outputInsertCols + ", __item_ref"

                                    'End If

                                    newRowCount += 1
                                    outputInsertData = String.Join(",", outputInsertListData.ToArray)
                                    outputInsert = "INSERT INTO " + TableName + "(__start_date ," + outputInsertCols + ")" + " VALUES (" + outputInsertData + ")"

                                    SQL = outputInsert
                                    ' Use array of data

                                    Try
                                        DB.NonQuery(SQL, outputInsertListObject.ToArray)
                                    Catch ex As Exception
                                        ' Error creating datasource in DB. Error inserting into datasource table
                                        ErrorStatus = True
                                        WriteImportMessage(SegmentID, "ER_IM041", "ER_IM024", 0, 0, ex, "")
                                    End Try

                                    'Do an estimated size check.
                                    SizeofInsertData += GetListSizeInKB(outputInsertListObject)
                                    If (MaxDataSize * 1000) < (CurrentDataSize - CurrentDataSetSize + SizeofInsertData) Then

                                        ' Now Check that client size limit not reached on DB to make sure. MaxDataSize is held in MB, so convert to KB.
                                        NewDataSetSize = GetDataImportSize(TableName, SourceDate)
                                        If (MaxDataSize * 1000) < (CurrentDataSize - CurrentDataSetSize + NewDataSetSize) Then
                                            ' File not imported. Maximum Data Allowance of {0}MB has been reached.
                                            ErrorStatus = True
                                            WriteImportMessage(SegmentID, "ER_IM021", "ER_IM021", 0, 0, MaxDataSize.ToString)
                                            reverseChanges = True

                                        Else
                                            ' Size Allocation on DB not yet reached. Reset the INSERT total to the Actual size on DB
                                            SizeofInsertData = NewDataSetSize
                                        End If
                                    End If

                                Next
                            End If
                        End If

                        If ErrorStatus = False Then
                            reverseChanges = False
                            ' Build the list of column names to check 
                            For colIndex = 0 To ColumnSpec.Count - 1
                                colNameList = colNameList + ColumnSpec(colIndex).colname
                                colNameQuoteList = colNameQuoteList + "'" + ColumnSpec(colIndex).colname + "'"
                                If ColumnSpec.Count - 1 > colIndex Then
                                    colNameList = colNameList + ", "
                                    colNameQuoteList = colNameQuoteList + ", "
                                End If
                            Next

                            ' Now check to make sure data imported is not identical to previous data
                            If PrevDataDate.Ticks > 0 And PrevDataDate <> SourceDate Then

                                ' Next Count the rows in the previous and new tables
                                SQL = "select (Select count(*) from " + TableName + " WHERE __start_date = @p1) as prevcount, (Select count(*) from " + TableName + " WHERE __start_date = @p2) as newcount"
                                DT = DB.GetTable(SQL, PrevDataDate, SourceDate)
                                If DT.Rows.Count > 0 Then
                                    PreviousCount = DT.Rows(0).GetValue("prevcount", 0)
                                    NewCount = DT.Rows(0).GetValue("newcount", 0)
                                    If PreviousCount = NewCount Then
                                        ' The are the same. Next check whether schema has changed. 
                                        If schemaChange = False Then
                                            ' There are no schema differences. Now check the data for differences
                                            SQL = "Select Count(1) as prevcount from (" +
                                                              "Select " + colNameList + " from " + TableName + " WHERE __start_date = @p1" +
                                                              " except " +
                                                              "Select " + colNameList + " from " + TableName + " WHERE __start_date = @p2" +
                                                              ") x "
                                            PreviousCount = DB.Scalar(SQL, PrevDataDate, SourceDate)
                                            SQL = "Select Count(1) as newcount from (" +
                                                      "Select " + colNameList + " from " + TableName + " WHERE __start_date = @p2" +
                                                      " except " +
                                                      "Select " + colNameList + " from " + TableName + " WHERE __start_date = @p1" +
                                                      ") x "
                                            NewCount = DB.Scalar(SQL, PrevDataDate, SourceDate)
                                            If PreviousCount = 0 And NewCount = 0 Then
                                                ' The data is identical to previous datasource. Reject and clean up.
                                                ErrorStatus = True
                                                WriteImportMessage(SegmentID, "ER_IM034", "ER_IM034", 0, 0, "")
                                                reverseChanges = True
                                            End If

                                        End If
                                    End If
                                End If
                            End If
                        End If

                        If ErrorStatus = False Then
                            ' Check for duplicate rows in non primary data source
                            If PrimarySource = "0" Then
                                'select count(1) as newcount from (select distinct count(1) As newcount from T235744F552E541DEA26358D8A07A5768 group by Col_1,Col_2,Col_3,Col_4,Col_5,Col_6,Col_7 having count(1) > 1) x
                                SQL = "select count(1) as newcount from (select distinct count(1) As newcount from  " + TableName + " group by  " + colNameList + " having count(1) > 1) x"
                                NewCount = DB.Scalar(SQL)
                                If NewCount > 0 Then
                                    ' Duplicate data found. Reject and clean up.
                                    ErrorStatus = True
                                    WriteImportMessage(SegmentID, "ER_IM037", "ER_IM037", 0, 0, "")
                                    reverseChanges = True
                                End If
                            End If
                        End If

                        If reverseChanges = True Then
                            ' Cleanup - Delete data_source (trigger deletes related table)
                            SQL = "DELETE FROM " + TableName + " WHERE __start_date = @p1"
                            Try
                                DB.NonQuery(SQL, SourceDate)
                            Catch ex As Exception
                                ' Error deleting duplicate datasource
                                ErrorStatus = True
                                WriteImportMessage(SegmentID, "ER_IM041", "ER_IM036", 0, 0, ex, "")
                            End Try

                            If staticDataSource = True Then
                                ' UPDATE the current iteration of the data as this is a "static" datasource, with the original date.
                                SQL = "UPDATE " + TableName + " SET __start_date = @p2 WHERE __start_date = @p1 "
                                Try
                                    DB.NonQuery(SQL, staticTempDate, SourceDate)
                                Catch ex As Exception
                                    ErrorStatus = True
                                    ' 'Error inserting (updating) datasource field details
                                    WriteImportMessage(SegmentID, "ER_IM041", "ER_IM024", 0, 0, ex, "")
                                End Try
                            End If
                        Else
                            If staticDataSource = True Then

                                ' Cleanup - Delete data_source (trigger deletes related table)
                                SQL = "DELETE FROM " + TableName + " WHERE __start_date = @p1"
                                Try
                                    DB.NonQuery(SQL, staticTempDate)
                                Catch ex As Exception
                                    ' Error deleting old datasource
                                    ErrorStatus = True
                                    WriteImportMessage(SegmentID, "ER_IM041", "ER_IM036", 0, 0, ex, "")
                                End Try
                                ' Clear Old Data Source Data
                            End If

                        End If


                        DB.TranCommit()
                        End If

                End Using

            Else
                ' No Columns found
                SourceID = 0

            End If

        Catch ex As Exception
            ' Error creating datasource in DB
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM025", 0, 0, ex, "")
            Return 0
        End Try

        Return SourceID

    End Function

    Public Function CopySourceTables(SourceID As String, NewDateTime As DateTime, PrevDateTime As DateTime, SegmentID As Integer) As Boolean
        Dim retValue As Boolean = True

        Try
            Using DB As New IawDB
                DB.AddParameter("@SourceID", SourceID)
                DB.AddParameter("@NewDate", NewDateTime)
                DB.AddParameter("@OldDate", PrevDateTime)
                DB.CallStoredProc("dbo.dbp_data_source_new_occurrence")


                ' build the source_set_values table for the 'new' datasource

                Dim result As Boolean = (New BuildViewValues()).BuildDataSourceValues(SourceID, NewDateTime)
                ' Check for FALSE result if there's an error
                If result = False Then
                    ' Error copying data view data
                    ErrorStatus = True
                    WriteImportMessage(SegmentID, "ER_IM041", "ER_IM026", 0, 0, "", "")
                    Return False
                End If

                ' Write out any data compare messages (New and deleted rows) for a Primary Data source
                If PrimarySource = "1" Then
                    If staticDataSource = True Then
                        ReportDataChanges(TableName, SourceID, NewDateTime, staticTempDate, SegmentID)
                        ' Now Delete Temp data for static data source.
                        ' DELETE the future iteration of the data if left lying around previously as this is a "static" datasource.
                        SQL = "DELETE FROM " + TableName + " WHERE __start_date = @p1 "
                        Try
                            DB.NonQuery(SQL, staticTempDate)
                        Catch ex As Exception
                            ErrorStatus = True
                            ' 'Error inserting (deleting) datasource field details
                            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM024", 0, 0, ex, "")
                        End Try
                    Else
                        ReportDataChanges(TableName, SourceID, NewDateTime, PrevDateTime, SegmentID)

                    End If
                End If
            End Using
        Catch ex As Exception
            ' Error copying data view data
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM026", 0, 0, ex, "")
            Return False
        End Try

        Return retValue
    End Function

    Public Function ReportDataChanges(PassTableName As String, SourceID As String, NewDateTime As DateTime, PrevDateTime As DateTime, SegmentID As Integer) As Boolean
        Dim retValue As Boolean = True
        Dim datRow As DataRow
        Dim datTable As DataTable
        Dim keyValue As String
        Dim newCount As Long = 0
        Dim delCount As Long = 0
        Dim newFieldList As String = ""
        Dim delFieldList As String = ""

        Try
            Using DB As New IawDB

                ' Write out any data compare messages (New rows) for a Primary Data source
                SQL = "SELECT __item_ref 
                         FROM " + PassTableName + "
                        WHERE __start_date = @p1 
                          AND __item_ref NOT IN (SELECT oldd.__item_ref FROM " + PassTableName + " oldd WHERE oldd.__start_date = @p2)
                        ORDER BY __item_ref ASC"
                datTable = DB.GetTable(SQL, NewDateTime, PrevDateTime)

                If datTable IsNot Nothing Then
                    newCount = datTable.Rows.Count
                    If newCount > 0 Then
                        For Each datRow In datTable.Rows
                            keyValue = "<tr>" + datRow.GetValue("__item_ref", "").ToString + "</tr>"
                            newFieldList += keyValue ' + "/n"
                        Next
                        ' Convert /n to CRLF
                        newFieldList = newFieldList.Replace("/n", vbCrLf)
                    End If

                End If

                ' Write out any data compare messages (Deleted rows) for a Primary Data source
                SQL = "SELECT __item_ref 
                         FROM " + PassTableName + "
                        WHERE __start_date = @p2 
                          AND __item_ref NOT IN (SELECT newd.__item_ref FROM " + PassTableName + " newd WHERE newd.__start_date = @p1)
                        ORDER BY __item_ref ASC"
                datTable = DB.GetTable(SQL, NewDateTime, PrevDateTime)

                If datTable IsNot Nothing Then
                    delCount = datTable.Rows.Count
                    If delCount > 0 Then
                        For Each datRow In datTable.Rows
                            keyValue = "<tr>" + datRow.GetValue("__item_ref", "").ToString + "</tr>"
                            delFieldList += keyValue ' + "/n"
                        Next
                        ' Convert /n to CRLF
                        ' delFieldList = delFieldList.Replace("/n", vbCrLf)
                    End If

                End If

                ' <p>[Data Source] : [number of rows] imported as of [Effective Date]</p>
                ' <p>[Number of new rows] records have been added.</p>
                ' <p>[Number of rows deleted] records have been removed.</p>
                WriteImportMessage(SegmentID, "IN_IM010", "IN_IM010", 0, 0, DSName, newRowCount.ToString, NewDateTime.ToString, newCount.ToString, delCount.ToString)

                ' <p>New Records</p>
                ' <table>
                ' [Each record to show what would be displayed in each of the columns within the AEA]
                ' </table>
                WriteImportMessage(SegmentID, "IN_IM011", "IN_IM011", 0, 0, newFieldList)

                ' <p>Records that have been removed</p>
                ' <table>
                ' [Each record to show what would be displayed in each of the columns within the AEA]
                ' </table>
                WriteImportMessage(SegmentID, "IN_IM012", "IN_IM012", 0, 0, delFieldList)


            End Using
        Catch ex As Exception
            ' Error copying data view data
            ErrorStatus = True
            WriteImportMessage(SegmentID, "ER_IM041", "ER_IM026", 0, 0, ex, "")
            Return False
        End Try

        Return retValue
    End Function


    Public Function WriteImportMessage(ByVal SegID As Integer, ByVal MessageRef As String, ByVal TechMessageRef As String, ByVal RowNum As Integer, ByVal FieldNum As Integer, ByVal exp As Exception, ParamArray PA() As Object) As Boolean

        Dim retValue As Boolean = True
        Dim LogException As New LogException()

        Try

            retValue = WriteImportMessage(SegID, MessageRef, TechMessageRef, RowNum, FieldNum, PA)
            LogException.RecordError("Segment ID: " + SegID.ToString, exp)

        Catch ex As Exception
            ErrorStatus = True
            '            Errortext = "Error Inserting Import Message"
            Return False
        End Try

        Return retValue

    End Function

    Public Function WriteImportMessage(ByVal SegID As Integer, ByVal MessageRef As String, ByVal TechMessageRef As String, ByVal RowNum As Integer, ByVal FieldNum As Integer, ParamArray PA() As Object) As Boolean

        Dim retValue As Boolean = True
        Dim MessageTextOther As String = ""
        Dim MessageTextGB As String
        Dim MessageType As String = ""

        Try

            Using DB As New IawDB
                MessageType = Left(MessageRef, 2)
                Select Case MessageType.ToUpper
                    Case "IN"
                        MessageType = "02" ' Information 
                    Case "ER"
                        If MessageRef = TechMessageRef Then
                            MessageType = "01" ' User Error
                        Else
                            MessageType = "03" ' Tech Error
                        End If

                End Select

                MessageTextGB = BuildMessageGBR(TechMessageRef, PA)
                If ctx.languageCode <> "GBR" Or MessageRef <> TechMessageRef Then
                    MessageTextOther = BuildMessage(MessageRef, PA)
                End If
                If MessageTextOther = "" Then
                    MessageTextOther = MessageTextGB
                End If
                If MessageTextOther = MessageTextGB Then
                    MessageTextGB = ""
                End If

                SQL = "INSERT INTO client_import_msg (seg_id , row_num, field_num, msg_text, msg_text_gb, msg_type)
                       VALUES (@p1,@p2,@p3,@p4,@p5,@p6)"
                DB.NonQuery(SQL, SegID, RowNum, FieldNum, MessageTextOther, MessageTextGB, MessageType)

            End Using

        Catch ex As Exception
            ErrorStatus = True
            '            Errortext = "Error Inserting Import Message"
            Return False
        End Try

        Return retValue

    End Function

    Public Function ReadDataStream(ByVal data() As Byte) As String
        Dim encoding As Encoding = Encoding.Default
        Dim bomLength As Integer = 0

        Using stream As New MemoryStream(data)
            Using reader As New BinaryReader(stream)
                Dim bom() As Byte = reader.ReadBytes(4)
                If bom.Length >= 2 AndAlso bom(0) = &HFF AndAlso bom(1) = &HFE Then
                    encoding = Encoding.Unicode
                    bomLength = 2
                ElseIf bom.Length >= 2 AndAlso bom(0) = &HFE AndAlso bom(1) = &HFF Then
                    encoding = Encoding.BigEndianUnicode
                    bomLength = 2
                ElseIf bom.Length >= 3 AndAlso bom(0) = &HEF AndAlso bom(1) = &HBB AndAlso bom(2) = &HBF Then
                    encoding = Encoding.UTF8
                    bomLength = 3
                End If
                reader.BaseStream.Seek(bomLength, SeekOrigin.Begin)
            End Using
        End Using

        Using stream As New MemoryStream(data, bomLength, data.Length - bomLength)
            Using reader As New StreamReader(stream, encoding)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function

    Public Function GetSizes(ClientID As String) As Boolean
        Dim returnVal As Boolean = True
        Dim DT As DataTable
        Using DB As New IawDB

            ' Get the Max Sizes from the clients table
            SQL = "SELECT max_string_len, 
                      max_number,
                      max_data
                 FROM dbo.clients 
                WHERE client_id = @p1 "
            DT = DB.GetTable(SQL, ClientID)
            If DT.Rows.Count > 0 Then
                MaxStringSize = DT.Rows(0).GetValue("max_string_len", 500)
                MaxNumber = DT.Rows(0).GetValue("max_number", 9999999999.99)
                MaxDataSize = DT.Rows(0).GetValue("max_data", 200)
            Else
                MaxStringSize = 500
                MaxNumber = 9999999999.99
                MaxDataSize = 200
            End If

            ' Get the clients current data size
            CurrentDataSize = GetClientDataSize(ClientID)

            ' Get the size of the current data set if it's being replaced.
            'CurrentDataSetSize = Proc_Imp.GetDataImportSize()

        End Using

        Return returnVal
    End Function


    Public Function GetClientDataSize(ByVal ClientID As String) As Long

        Dim DT As DataTable
        Dim DataSizeinKB As Long

        ' Get next row to be processed
        Using DB As New IawDB

            SQL = "SELECT (SELECT Sum(S.in_row_data_page_count * 8) AS dataspace 
                            FROM SYS.dm_db_partition_stats s  
                            JOIN SYS.tables t  
                              ON S.object_id = T.object_id  
                            JOIN dbo.data_source DS  
                              ON T.name = DS.table_name  
                           WHERE DS.client_id = @p1) + 
                         (SELECT Sum(datalength(item_binary)) / 1024 
                            FROM dbo.client_document 
                           WHERE client_id = @p1) as size_kb"
            DT = DB.GetTable(SQL, ClientID)
            If DT.Rows.Count > 0 Then
                DataSizeinKB = DT.Rows(0).GetValue("size_kb", 0)
            Else
                DataSizeinKB = 0
            End If

        End Using

        Return DataSizeinKB

    End Function

    Public Function GetDataImportSize(TableName As String, SourceDate As Date) As Long

        Dim DT As DataTable = Nothing
        Dim DataSizeinKB As Long
        Dim ErrorCaused As Boolean

        ErrorCaused = False
        DataSizeinKB = 0

        ' Get next row to be processed
        Using DB As New IawDB

            'SQL = "SELECT * INTO #data_temp FROM " + TableName + " WHERE __start_date = @p1"
            'Try
            '    DB.NonQuery(SQL, SourceDate)
            'Catch ex As Exception
            '    ErrorCaused = True
            'End Try

            'If ErrorCaused = False Then
            '    SQL = "ALTER TABLE #data_temp DROP COLUMN __item_ref, __start_date "
            '    Try
            '        DB.NonQuery(SQL)
            '    Catch ex As Exception
            '        ErrorCaused = True
            '    End Try
            'End If

            'If ErrorCaused = False Then

            '    SQL = "SELECT (SELECT S.in_row_data_page_count * 8 AS dataspace 
            '       FROM tempdb.sys.dm_db_partition_stats S 
            '       JOIN tempdb.sys.tables T 
            '         ON S.object_id = T.object_id 
            '      WHERE T.name LIKE '#data_temp%') as size_kb "
            '    DT = DB.GetTable(SQL)
            '    If DT.Rows.Count > 0 Then
            '        DataSizeinKB = DT.Rows(0).GetValue("size_kb", 0)
            '    Else
            '        DataSizeinKB = 0
            '    End If
            'End If

            'If ErrorCaused = False Then
            '    SQL = "DROP TABLE #data_temp  "
            '    Try
            '        DB.NonQuery(SQL)
            '    Catch ex As Exception
            '        ErrorCaused = True
            '    End Try
            'End If

            SQL = "SELECT * INTO #data_temp FROM " + TableName + " WHERE __start_date = @p1 " +
                  "ALTER TABLE #data_temp DROP COLUMN __item_ref, __start_date " +
                  "SELECT (SELECT S.in_row_data_page_count * 8 AS dataspace 
		                 FROM tempdb.sys.dm_db_partition_stats S 
		                 JOIN tempdb.sys.tables T 
		                   ON S.object_id = T.object_id 
		                WHERE T.name LIKE '#data_temp%') as size_kb "

            Try
                DT = DB.GetTable(SQL, SourceDate)
            Catch ex As Exception
                ErrorCaused = True
            End Try

            If ErrorCaused = False Then
                If DT.Rows.Count > 0 Then
                    DataSizeinKB = DT.Rows(0).GetValue("size_kb", 0)
                Else
                    DataSizeinKB = 0
                End If
            End If

            'If ErrorCaused = False Then
            '    SQL = "DROP TABLE #data_temp  "
            '    Try
            '        DB.NonQuery(SQL)
            '    Catch ex As Exception
            '        ErrorCaused = True
            '    End Try
            'End If

        End Using

        Return DataSizeinKB

    End Function

    'Private Function GetListSizeInKB(ByVal list As List(Of Object)) As Double
    '    Dim totalSize As Double = 0.0
    '    For Each obj As Object In list
    '        Dim handle As GCHandle = GCHandle.Alloc(obj, GCHandleType.Pinned)
    '        Try
    '            totalSize += Marshal.SizeOf(obj.GetType()) / 1024.0
    '        Finally
    '            handle.Free()
    '        End Try
    '    Next
    '    Return totalSize
    'End Function

    Private Function GetListSizeInKB(ByVal list As List(Of Object)) As Double
        Dim totalSize As Double = 0.0
        For Each obj As Object In list
            If obj IsNot Nothing Then
                If TypeOf obj Is String Then
                    ' For string, we get the length of the string (characters count) and multiply by the size of Char (2 bytes = 2/1024 KB)
                    totalSize += (CType(obj, String).Length * 2) / 1024.0
                ElseIf TypeOf obj Is Date Then
                    ' For Date, we can use the Marshal.SizeOf method
                    totalSize += 8 / 1024.0
                End If
            End If
        Next
        Return totalSize
    End Function



    Public Function DecCount(ByVal ValObj As Double) As Integer
        Dim i As Integer = 0
        Try
            Dim D As Double
            D = CType(ValObj, Double)
            If D.ToString().IndexOf(".") > 0 Then
                i = D.ToString().Substring(D.ToString().IndexOf(".")).Length - 1
            End If
        Catch ex As Exception
            i = 0
        End Try
        Return i
    End Function

    Public Function isUnique(dtable As DataTable, colname As String) As Boolean
        Return dtable.DefaultView.ToTable(True, colname).Rows.Count = dtable.Rows.Count
    End Function

    Function RoundUpToX(Value As Integer, nearest As Integer) As Integer

        If nearest = 0 Then
            If Value < 5 Then
                nearest = 1
            ElseIf Value > 5 And Value < 100 Then
                nearest = 10
            ElseIf Value > 100 Then
                nearest = 50
            End If
        End If

        If nearest = 0 Or nearest = 1 Then
            Return Value
        Else
            Return CInt(Math.Ceiling(Value / nearest) * nearest)
        End If


    End Function


End Class

Public Class colDef
    Public colname As String
    Public coltype As String
    Public collen As Integer  ' will be length of string or number of decimals for double type
    Public colhead As String
    Public colorg As String
    Public colunique As String ' 0 - No, 1 - Yes, 2- Searchable, 3 - Unique Key Set
    Public colstatus As String ' 01 - New, 02 - Accepted, 03 - Deleted
    Public colparent As String
    Public colformat As String
    Public collenincrease As String ' 0 - No, 1 - Yes
    Public colnew As String ' 0 - No, 1 - Yes (This is a new column for an existing data source)
    Public colmatched As String ' 0 - No, 1 - Yes (This is a matched column for an existing data source)
    Public Sub New(_colname As String, _coltype As String, _collen As Integer, _colhead As String, _colorg As String, _colunique As String, _colstatus As String, _colparent As String, _colformat As String)
        colname = _colname
        coltype = _coltype
        collen = _collen
        colhead = _colhead
        colorg = _colorg
        colunique = _colunique
        colstatus = _colstatus
        colparent = _colparent
        colformat = _colformat
        collenincrease = "0"
        colnew = "0"
        colmatched = "0"
    End Sub
End Class
