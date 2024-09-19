
Imports System.Runtime.CompilerServices

Module ExtensionMethods

    <Extension()>
    Public Sub SetDatasource(ByRef ddlb As DropDownList, DT As DataTable, ByVal TextField As String, ByVal ValueField As String, Optional ByVal EmptyEntryText As String = "")
        ddlb.DataSource = DT
        ddlb.DataTextField = TextField
        ddlb.DataValueField = ValueField
        ddlb.DataBind()

        If String.IsNullOrEmpty(EmptyEntryText) Then Return

        ddlb.Items.Insert(0, New ListItem(EmptyEntryText, ""))
    End Sub

    ''' <summary>
    ''' Does string contain any of the values (Case Sensitive)
    ''' </summary>
    ''' <param name="aString"></param>
    ''' <param name="Entries"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ContainsOneOf(ByVal aString As String, ByVal ParamArray Entries() As String) As Boolean
        Return Entries.Contains(aString)
    End Function

    ''' <summary>
    ''' Does string contain any of the values (Case Insensitive)
    ''' </summary>
    ''' <param name="aString"></param>
    ''' <param name="Entries"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ContainsOneOfCI(ByVal aString As String, ByVal ParamArray Entries() As String) As Boolean
        Return Entries.Contains(aString, StringComparer.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Glue a list(of string) together into a delimited string
    ''' </summary>
    ''' <param name="StrList">List(of String) to glue together</param>
    ''' <param name="Delimiter">The delimiter that you want to separate the values</param>
    ''' <returns>A concatenated string of values</returns>
    <Extension()>
    Public Function Glue(ByVal StrList As List(Of String), Delimiter As String)
        Return String.Join(Delimiter, StrList.ToArray)
    End Function

    ''' <summary>
    ''' add a class to an existing cssclass
    ''' </summary>
    ''' <param name="originalClassString"></param>
    ''' <param name="classToAdd"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ClassAdd(ByVal originalClassString As String, ByVal classToAdd As String) As String
        Dim classes As List(Of String) = originalClassString.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries).ToList()

        If Not classes.Contains(classToAdd) Then
            classes.Add(classToAdd)
        End If

        Return String.Join(" ", classes)
    End Function

    ''' <summary>
    ''' remove a class from an existing cssclass
    ''' </summary>
    ''' <param name="originalClassString"></param>
    ''' <param name="classToRemove"></param>
    ''' <returns></returns>
    <Extension()>
    Public Function ClassRemove(ByVal originalClassString As String, ByVal classToRemove As String) As String
        Dim classes As List(Of String) = originalClassString.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries).ToList()

        If classes.Contains(classToRemove) Then
            classes.Remove(classToRemove)
        End If

        Return String.Join(" ", classes)
    End Function

    <Extension()>
    Public Function SearchTheTreeView(ByVal _TV As TreeView, ByVal _Value As String) As TreeNode
        Return RecursiveSearch(_TV.Nodes, _Value)
    End Function

    Private Function RecursiveSearch(ByVal _Nodes As TreeNodeCollection, ByVal _Value As String) As TreeNode
        Dim N As TreeNode
        If _Nodes Is Nothing Then Return Nothing
        For Each TN As TreeNode In _Nodes
            If TN.Value = _Value Then
                Return TN
            End If
            N = RecursiveSearch(TN.ChildNodes, _Value)
            If N IsNot Nothing Then Return N
        Next
        Return Nothing
    End Function

    <Extension()>
    Public Function Doc(ByRef _TV As TreeView) As String
        Dim result As String = ""
        RecursiveDoc(result, _TV.Nodes)
        Return result
    End Function

    Private Sub RecursiveDoc(ByRef result As String, ByVal _Nodes As TreeNodeCollection)
        If _Nodes Is Nothing Then Return
        For Each TN As TreeNode In _Nodes
            result += Space(TN.Depth * 2) + TN.Value + " : " + TN.Text + Environment.NewLine
            RecursiveDoc(result, TN.ChildNodes)
        Next
    End Sub

    <Extension()>
    Public Function isBetween(DateVar As Date, FromDate As Date, ToDate As Date) As Boolean
        Return DateVar >= FromDate And DateVar <= ToDate
    End Function

    <Extension()>
    Public Function GetValue(DR As DataRow, Col As String) As String
        Return IawDB.GetValueDef(DR(Col), "")
    End Function
    <Extension()>
    Public Function GetValue(DR As DataRow, Col As String, Def As String) As String
        Return IawDB.GetValueDef(DR(Col), Def)
    End Function
    <Extension()>
    Public Function GetDate(DR As DataRow, Col As String) As Date
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function
    <Extension()>
    Public Function GetDate(DR As DataRow, Col As String, Def As Date) As Date
        Return IawDB.GetValueDef(DR(Col), Def)
    End Function

    <Extension()>
    Public Function GetValue(DR As DataRowView, Col As String) As String
        Return IawDB.GetValueDef(DR(Col), "")
    End Function
    <Extension()>
    Public Function GetValue(DR As DataRowView, Col As String, Def As String) As String
        Return IawDB.GetValueDef(DR(Col), Def)
    End Function
    <Extension()>
    Public Function GetDate(DR As DataRowView, Col As String) As Date
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function
    <Extension()>
    Public Function GetDate(DR As DataRowView, Col As String, Def As Date) As Date
        Return IawDB.GetValueDef(DR(Col), Def)
    End Function


    <Extension()>
    Public Function GetInt(DR As DataRow, Col As String) As Integer
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function

    <Extension()>
    Public Function GetInt(DR As DataRow, Col As String, Def As Date) As Integer
        Return IawDB.GetValueDef(DR(Col), Def)
    End Function

    <Extension()>
    Public Function GetGuid(DR As DataRow, Col As String) As Guid
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function

    <Extension()>
    Public Function GetBytes(DR As DataRow, Col As String) As Byte()
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function

    <Extension()>
    Public Function GetGuid(DR As DataRowView, Col As String) As Guid
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function

    <Extension()>
    Public Function GetBytes(DR As DataRowView, Col As String) As Byte()
        Return IawDB.GetValueDef(DR(Col), Nothing)
    End Function

    <Extension()>
    Public Function FindColumn(GRD As IAW.controls.IAWGrid, col As String) As Integer
        Dim i As Integer = 0
        For i = 0 To GRD.Columns.Count - 1
            If GRD.Columns(i).HeaderText.ToLower().Trim() = col.ToLower().Trim() Then
                Return i
            End If
        Next

        Return -1
    End Function

    <Extension()>
    Public Function Sort(DT As DataTable, SortExpression As String) As DataTable
        Dim DV As DataView = DT.DefaultView
        DV.Sort = SortExpression
        Return DV.ToTable
    End Function

    <Extension()>
    Public Sub Sorts(ByRef DT As DataTable, SortExpression As String)
        Dim DV As DataView = DT.DefaultView
        DV.Sort = SortExpression
        DT = DV.ToTable
    End Sub

    ''' <summary>
    ''' Does string contain any of the values (Case Sensitive)
    ''' </summary>
    <Extension()>
    Public Function StartsWithOneOf(ByVal aString As String, ByVal ParamArray Entries() As String) As Boolean
        For Each s As String In Entries
            If aString.StartsWith(s) Then
                Return True
            End If
        Next
        Return False
    End Function
    ''' <summary>
    ''' Does string contain any of the values (Case Sensitive)
    ''' </summary>
    <Extension()>
    Public Function StartsWithOneOfCI(ByVal aString As String, ByVal ParamArray Entries() As String) As Boolean
        For Each s As String In Entries
            If aString.ToLower.StartsWith(s.ToLower) Then
                Return True
            End If
        Next
        Return False
    End Function

End Module
