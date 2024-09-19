Imports Microsoft.VisualBasic
Imports System.Web.UI.WebControls
Imports AjaxControlToolkit

Namespace IAW.boundcontrols

    Public Class IAWGridCheckBoxColumn
        Inherits System.Web.UI.WebControls.BoundField

        Private _allowCheckAll As Boolean
        Private checkboxes As New List(Of MultiCheckBox)
        Private _preSelected As Boolean

#Region "properties"
        ''' <summary>
        ''' Returns a list of all the selected indexes
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SelectedIndexes() As List(Of Integer)
            Get
                Dim selected As New List(Of Integer)
                For Each obj As MultiCheckBox In Me.checkboxes
                    If obj.Checked Then
                        selected.Add(obj.Index)
                    End If
                Next
                Return selected
            End Get
        End Property

        ''' <summary>
        ''' Returns a list of all the selected data values
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property SelectedValues() As List(Of Object)
            Get
                Dim selected As New List(Of Object)
                For Each obj As MultiCheckBox In Me.checkboxes
                    If obj.Checked Then
                        selected.Add(obj.Value)
                    End If
                Next
                Return selected
            End Get
        End Property

        ''' <summary>
        ''' If true, inserts a checkbox into the column header whcih will check/uncheck all checkbox's in the column
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property AllowCheckAll() As Boolean
            Get
                Return _allowCheckAll
            End Get
            Set(ByVal value As Boolean)
                _allowCheckAll = value
            End Set
        End Property


#End Region

        Public Overrides Sub InitializeCell(ByVal cell As System.Web.UI.WebControls.DataControlFieldCell, _
                                            ByVal cellType As System.Web.UI.WebControls.DataControlCellType, _
                                            ByVal rowState As System.Web.UI.WebControls.DataControlRowState, _
                                            ByVal rowIndex As Integer)
            Select Case cellType
                Case DataControlCellType.DataCell
                    Me.InitializeDataCell(cell, rowState, rowIndex)

                Case DataControlCellType.Header
                    MyBase.InitializeCell(cell, cellType, rowState, rowIndex)
                    If AllowCheckAll Then
                        Dim chk As New CheckBox
                        chk.ID = "_HeaderButton"
                        chk.Attributes.Add("onclick", "CheckAll(this);")
                        If Not String.IsNullOrEmpty(cell.Text) Then
                            cell.Controls.Add(New LiteralControl(cell.Text))
                        End If
                        cell.Controls.Add(chk)
                    End If

                Case Else
                    MyBase.InitializeCell(cell, cellType, rowState, rowIndex)

            End Select
        End Sub

        Protected Shadows Sub InitializeDataCell(ByVal cell As DataControlFieldCell, _
                                                 ByVal rowState As DataControlRowState, _
                                                 ByVal rowIndex As Integer)
            Dim isInsert As Boolean = (rowState And DataControlRowState.Insert) <> DataControlRowState.Normal
            Dim chk As New MultiCheckBox
            chk.Index = rowIndex
            chk.CssClass = "grid_checkbox"
            chk.ID = "cb" + DataField
            chk.Enabled = Not Me.ReadOnly
            'chk.Checked = Me.PreSelected
            cell.Controls.Add(chk)
            'keep track of the index each checkbox is added to
            checkboxes.Add(chk)

            If (Me.DataField.Length <> 0) AndAlso Not isInsert Then
                AddHandler chk.DataBinding, New EventHandler(AddressOf Me.OnDataBindField)
            End If

        End Sub

        ''' <summary>
        ''' Handles the databind event
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Protected Shadows Sub OnDataBindField(ByVal sender As Object, _
                                              ByVal e As System.EventArgs)
            Dim control As Control = DirectCast(sender, Control)
            Dim namingContainer As Control = control.NamingContainer
            Dim dataValue As Object = Me.GetValue(namingContainer)
            Dim chk As MultiCheckBox

            If Not TypeOf control Is MultiCheckBox Then
                Throw New InvalidOperationException("MultiCheckBoxField could not extract MultiCheckBox control.")
            End If

            chk = DirectCast(control, MultiCheckBox)

            If TypeOf dataValue Is DBNull Then
                Return
            End If

            Select Case True
                Case TypeOf dataValue Is String
                    chk.Checked = dataValue.ToString.ToLower.ContainsOneOf("1", "y", "yes", "true")
                Case TypeOf dataValue Is Boolean
                    chk.Checked = CBool(dataValue)
                Case TypeOf dataValue Is Integer
                    chk.Checked = CInt(dataValue) > 0
            End Select

        End Sub

        ''' <summary>
        ''' Used to get the control value during two way databinding
        ''' </summary>
        ''' <param name="dictionary"></param>
        ''' <param name="cell"></param>
        ''' <param name="rowState"></param>
        ''' <param name="includeReadOnly"></param>
        ''' <remarks></remarks>
        Public Overrides Sub ExtractValuesFromCell(ByVal dictionary As Collections.Specialized.IOrderedDictionary, _
                                                   ByVal cell As DataControlFieldCell, _
                                                   ByVal rowState As DataControlRowState, _
                                                   ByVal includeReadOnly As Boolean)
            If String.IsNullOrEmpty(Me.DataField) OrElse (Me.ReadOnly And Not includeReadOnly) Then Return

            ' Determine whether the cell contain a MultiCheckBox 
            ' in its Controls collection.
            If cell.Controls.Count > 0 Then
                Dim ctrl As Control = cell.Controls(0)
                Dim chk As New MultiCheckBox

                If Not TypeOf ctrl Is MultiCheckBox Then
                    Throw New InvalidOperationException("MultiCheckBoxField could not extract MultiCheckBox control.")
                End If

                chk = DirectCast(ctrl, MultiCheckBox)

                ' Add the value of the SelectedDate property of the DatePicker to the dictionary.
                If dictionary.Contains(DataField) Then
                    dictionary(DataField) = chk.Value
                Else
                    dictionary.Add(DataField, chk.Value)
                End If
            End If
        End Sub
    End Class

#Region "helper class"
    ''' <summary>
    ''' Essentially a checkbox that can hold an index and a value
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MultiCheckBox
        Inherits CheckBox

#Region "properties"
        Public Property Index() As Integer
            Get
                Return Me.ViewState("Index")
            End Get
            Protected Friend Set(ByVal value As Integer)
                Me.ViewState("Index") = value
            End Set
        End Property
        Public Property Value() As Object
            Get
                Return Me.ViewState("Value")
            End Get
            Protected Friend Set(ByVal value As Object)
                Me.ViewState("Value") = value
            End Set
        End Property
#End Region
        Public Sub New()
            MyBase.New()
        End Sub
    End Class
#End Region

End Namespace
