
Imports Newtonsoft.Json

Public Class DatasourceFilter

    Public Sub Test()
        Dim DSF As New DSFilter
        'DSF.FilterName = "MyDept"

        Dim DSFC As New DSFilterColumn
        DSFC.FieldNumber = 7
        DSFC.Values.Add("Sales")
        DSFC.Values.Add("Finance")
        DSF.Columns.Add(DSFC)

        DSFC = New DSFilterColumn
        DSFC.FieldNumber = 1
        DSFC.Values.Add("1")
        DSFC.Values.Add("2")
        DSFC.Values.Add("3")
        DSF.Columns.Add(DSFC)

        Dim str As String = FilterToString(DSF)
        Dim D2 As DSFilter = StringToFilter(str)

        Dim x As DSFilterReturn = ConvertStringToSQLParams(1, str, 0)

        Dim a As Integer
        a = 1



    End Sub

    Private _BasisID As Integer
    Private Property BasisID As Integer
        Get
            Return _BasisID
        End Get
        Set(value As Integer)
            _BasisID = value
        End Set
    End Property

    Private _FieldsDT As DataTable
    Private ReadOnly Property FieldsDT As DataTable
        Get
            If _FieldsDT Is Nothing Then
                _FieldsDT = IawDB.execGetTable("select *
                                                  from dbo.data_source_field
                                                 where source_id = @p1", BasisID)
            End If
            Return _FieldsDT
        End Get
    End Property
    Private ReadOnly Property FieldDR(FieldNum As Integer) As DataRow
        Get
            Return FieldsDT.Select("field_num=" + FieldNum.ToString)(0)
        End Get
    End Property

    Public Function ConvertStringToSQLParams(source_id As Integer, jsonstr As String, CurrentParmCount As Integer) As DSFilterReturn
        BasisID = source_id

        Dim DSF As DSFilter = StringToFilter(jsonstr)
        Dim DR As DataRow

        Dim dsfRet As New DSFilterReturn
        Dim ParmNum As Integer = CurrentParmCount
        For Each COL As DSFilterColumn In DSF.Columns
            ' get the specific field from the database
            DR = FieldDR(COL.FieldNumber)

            ' if not first time in loop, we're dealing with a 2nd filter so add AND
            If ParmNum <> CurrentParmCount Then dsfRet.SQLString += " AND "

            ' now we need to glue the values together to form an IN statement
            Dim C As New List(Of String)
            For Each VAL As String In COL.Values
                ParmNum += 1
                C.Add("@p" + ParmNum.ToString)
                Select Case DR.GetValue("field_type", "01")
                    Case "01" ' String
                        dsfRet.SQLParamArray.Add(VAL)
                    Case "02" ' Date
                        dsfRet.SQLParamArray.Add(CDate(VAL))
                    Case "03"
                        dsfRet.SQLParamArray.Add(CInt(VAL))
                    Case "04"
                        dsfRet.SQLParamArray.Add(CDec(VAL))
                    Case "05"
                        dsfRet.SQLParamArray.Add(CBool(VAL))
                End Select
            Next
            dsfRet.SQLString += DR("field_column") + " IN (" + C.Glue(",") + ")"
        Next

        Return dsfRet
    End Function

    Public Function StringToFilter(Str As String) As DSFilter
        Return JsonConvert.DeserializeObject(Of DSFilter)(Str)
    End Function
    Public Function FilterToString(DSF As DSFilter) As String
        Return JsonConvert.SerializeObject(DSF)
    End Function

End Class

Public Class DSFilter
    'Public FilterName As String
    Public Columns As New List(Of DSFilterColumn)
End Class
Public Class DSFilterColumn
    Public FieldNumber As Integer
    Public Values As New List(Of String)
End Class

Public Class DSFilterReturn
    Public SQLString As String
    Public SQLParamArray As New List(Of Object)
End Class


