
Public Class BuildViewValues

    Private SQL As String

    Private SourceID As Integer = 0
    Private Occurrence As DateTime
    Private ViewID As Integer = 0

    Private _TableName As String = ""
    Private ReadOnly Property TableName As String
        Get
            If _TableName = "" Then
                SQL = "select DS.table_name 
                         from dbo.data_view DV
                              join dbo.data_source DS
                                on DS.source_id = DV.source_id
                       where DV.view_id = @p1"
                _TableName = IawDB.execScalar(SQL, ViewID)
            End If
            Return _TableName
        End Get
    End Property

    Private _ViewsDT As DataTable
    Private ReadOnly Property ViewsDT As DataTable
        Get
            If _ViewsDT Is Nothing Then
                SQL = "select distinct view_id 
                         from dbo.data_view
                        where source_id = @p1
                          and data_effective = @p2"
                _ViewsDT = IawDB.execGetTable(SQL, SourceID, Occurrence)
            End If
            Return _ViewsDT
        End Get
    End Property

    Private PrevViewID As Integer = -1
    Private ViewDisplay As DataTable
    Private ReadOnly Property ViewDisplayDT As DataTable
        Get
            If PrevViewID <> ViewID Then ViewDisplay = Nothing

            If ViewDisplay Is Nothing Then
                SQL = "select dispDR.disp_type, dispDR.disp_line, dispDR.disp_col, dispDR.disp_seq,
                              dispDR.field_num, dispDR.field_text, dispDR.field_format, dispDR.field_align,
                              F.field_name,F.field_column,F.field_type,F.field_length
                         from dbo.data_view S
                              join dbo.data_view_display dispDR
                                on dispDR.view_id = S.view_id
                              left outer join dbo.data_source_field F
                                on F.source_id = S.source_id
                               and F.field_num = dispDR.field_num
                        where S.view_id = @p1
                        order by disp_type, disp_line, disp_col, disp_seq "
                ViewDisplay = IawDB.execGetTable(SQL, ViewID)
                PrevViewID = ViewID
            End If
            Return ViewDisplay
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
    End Enum

    Private ReadOnly Property DisplayData(DisT As DispType) As DataTable
        Get
            Dim DType As String = "00"
            Select Case DisT
                Case DispType.OCAData
                    DType = "01"
                Case DispType.AEASort
                    DType = "02"
                Case DispType.AEAData
                    DType = "04"
            End Select

            Dim DBV As New DataView(ViewDisplayDT) With {
                        .RowFilter = "disp_type = '" + DType + "'"
                    }
            Return DBV.ToTable
        End Get
    End Property

    Private _TableData As DataTable
    Private ReadOnly Property TableData As DataTable
        Get
            If _TableData Is Nothing Then
                SQL = "select * from dbo." + TableName + " where __start_date = @p1"

                _TableData = IawDB.execGetTable(SQL, Occurrence)
            End If
            Return _TableData
        End Get
    End Property

    Public Function BuildDataSourceValues(source_id As Integer, OccurrenceDate As Date) As Boolean

        SourceID = source_id
        Occurrence = OccurrenceDate

        Using DB As New IawDB
            ' for each view for this data source, for this occurence
            For Each DR As DataRow In ViewsDT.Rows
                ViewID = DR("view_id")
                If Not BuildView() Then Return False
            Next

        End Using

        Return True
    End Function

    Public Function BuildDataViewValues(view_id As Integer) As Boolean
        ViewID = view_id
        Occurrence = IawDB.execScalar("select data_effective from dbo.data_view where view_id = @p1", ViewID)

        Return BuildView()
    End Function

    Public Function BuildView() As Boolean

        Dim C, A As List(Of String)
        Dim Align As String
        Dim LastLine As Integer
        Dim LastCol As Integer
        Dim FieldText As String
        Dim InsertSQL As String = "INSERT dbo.data_view_value (view_id, item_date, item_ref, disp_type, disp_num, disp_value, field_align)
                                   VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7)"

        Try
            Using DB As New IawDB
                DB.TranBegin()

                DB.NonQuery("DELETE FROM dbo.data_view_value
                              WHERE view_id = @p1", ViewID)

                For Each dataDR As DataRow In TableData.Rows

                    ' --------------------------------------------------------------------------------
                    ' OCA Data
                    '
                    FieldText = ""
                    Align = "01"
                    LastLine = -1
                    C = New List(Of String)
                    A = New List(Of String)
                    For Each dispDR As DataRow In DisplayData(DispType.OCAData).Rows
                        If LastLine <> -1 Then
                            If LastLine <> dispDR("disp_line") Then
                                C.Add(FieldText)
                                A.Add(Align)
                                FieldText = ""
                                Align = "01"
                                LastLine = dispDR("disp_line")
                            End If
                        End If
                        If dispDR("field_align") <> "" Then
                            Align = dispDR("field_align")
                        End If
                        If dispDR("field_num") = 0 Then
                            FieldText += dispDR("field_text")
                        Else
                            FieldText += dataDR.GetValue(dispDR("field_column"), "")
                        End If
                        LastLine = dispDR("disp_line")
                    Next
                    C.Add(FieldText)
                    A.Add(Align)
                    For i As Integer = 1 To C.Count
                        DB.NonQuery(InsertSQL, ViewID, Occurrence, dataDR("__item_ref"), "01", i, C(i - 1), A(i - 1))
                    Next

                    ' --------------------------------------------------------------------------------
                    ' AEA Sort
                    '
                    FieldText = ""
                    Align = "01"
                    LastCol = -1
                    C = New List(Of String)
                    A = New List(Of String)
                    For Each dispDR As DataRow In DisplayData(DispType.AEASort).Rows
                        If LastCol <> -1 Then
                            If LastCol <> dispDR("disp_col") Then
                                C.Add(FieldText)
                                A.Add(Align)
                                FieldText = ""
                                Align = "01"
                                LastCol = dispDR("disp_col")
                            End If
                        End If
                        If dispDR("field_align") <> "" Then
                            Align = dispDR("field_align")
                        End If

                        If FieldText <> "" Then FieldText += "-"
                        FieldText += dataDR.GetValue(dispDR("field_column"), "")
                        LastCol = dispDR("disp_col")
                    Next
                    C.Add(FieldText)
                    A.Add(Align)
                    For i As Integer = 1 To C.Count
                        DB.NonQuery(InsertSQL, ViewID, Occurrence, dataDR("__item_ref"), "02", i, C(i - 1), A(i - 1))
                    Next

                    ' --------------------------------------------------------------------------------
                    ' AEA Data
                    '
                    FieldText = ""
                    Align = "01"
                    LastCol = -1
                    C = New List(Of String)

                    For Each dispDR As DataRow In DisplayData(DispType.AEAData).Rows
                        If LastCol <> -1 Then
                            If LastCol <> dispDR("disp_col") Then
                                C.Add(FieldText)
                                A.Add(Align)
                                FieldText = ""
                                Align = "01"
                                LastCol = dispDR("disp_col")
                            End If
                        End If
                        If dispDR("field_num") = 0 Then
                            FieldText += dispDR("field_text")
                        Else
                            FieldText += dataDR.GetValue(dispDR("field_column"), "")
                        End If
                        If dispDR("field_align") <> "" Then
                            Align = dispDR("field_align")
                        End If

                        LastCol = dispDR("disp_col")
                    Next
                    C.Add(FieldText)
                    A.Add(Align)
                    For i As Integer = 1 To C.Count
                        DB.NonQuery(InsertSQL, ViewID, Occurrence, dataDR("__item_ref"), "04", i, C(i - 1), A(i - 1))
                    Next
                Next

                DB.TranCommit()
            End Using
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

End Class
