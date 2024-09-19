Imports System
Imports System.Collections.Specialized
Imports System.Collections
Imports System.ComponentModel
Imports System.Security.Permissions
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports IAW.controls


Namespace IAW.boundcontrols

    Public NotInheritable Class IAWGridDateColumn
        Inherits BoundField

        Public Sub New()
            MyBase.new()
        End Sub

        ' Gets a default value for a basic design-time experience. Since
        ' it would look odd, even at design time, to have more than one
        ' radio button selected, make sure that none are selected.
        Protected Overrides Function GetDesignTimeValue() As Object
            Return False
        End Function

        ' This method is called by the ExtractRowValues methods of
        ' GridView and DetailsView. Retrieve the current value of the 
        ' cell from the Checked state of the control.
        Public Overrides Sub ExtractValuesFromCell( _
            ByVal dictionary As IOrderedDictionary, _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState, _
            ByVal includeReadOnly As Boolean)

            Dim text1 As String = Me.DataField
            Dim obj1 As Object = Nothing

            ' Determine whether the cell contain a date control 
            ' in its Controls collection.

            If cell.Controls.Count > 0 Then
                Dim d As DateDropDown = CType(cell.Controls(0), DateDropDown)

                Dim checkedValue As Object = Nothing
                If d Is Nothing Then
                    ' A checkbox is expected, but a null is encountered.
                    ' Add error handling.
                    Throw New InvalidOperationException( _
                        "DateDropDown could not extract control.")
                Else
                    'extract value
                    obj1 = d.CurrentDate
                End If

                If (Not obj1 Is Nothing) Then
                    If dictionary.Contains(text1) Then
                        dictionary.Item(text1) = obj1
                    Else
                        dictionary.Add(text1, obj1)
                    End If
                End If

            End If
        End Sub

        ' This method adds a date control and any other 
        ' content to the cell's Controls collection.
        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)
            'MyBase.InitializeDataCell(cell, rowState)

            Dim control1 As Control = Nothing
            Dim control2 As Control = Nothing
            If ((((rowState And DataControlRowState.Edit) <> DataControlRowState.Normal) AndAlso Not Me.ReadOnly) OrElse ((rowState And DataControlRowState.Insert) <> DataControlRowState.Normal)) Then
                'editable
                Dim dateCtrl As New DateDropDown
                control1 = dateCtrl
                If ((Me.DataField.Length <> 0) AndAlso ((rowState And DataControlRowState.Edit) <> DataControlRowState.Normal)) Then
                    control2 = dateCtrl
                End If
            Else
                'read only
                If (Me.DataField.Length <> 0) Then
                    control2 = cell
                End If
            End If
            If (Not control1 Is Nothing) Then
                cell.Controls.Add(control1)
            End If
            If ((Not control2 Is Nothing) AndAlso MyBase.Visible) Then
                AddHandler control2.DataBinding, New EventHandler(AddressOf Me.OnDataBindField)
            End If

        End Sub

        Protected Shadows Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim control1 As Control = CType(sender, Control)
            Dim control2 As Control = control1.NamingContainer
            Dim obj1 As Object = Me.GetValue(control2)
            Dim flag1 As Boolean = ((Me.SupportsHtmlEncode AndAlso Me.HtmlEncode) AndAlso TypeOf control1 Is TableCell)
            Dim text1 As String = Me.FormatDataValue(obj1, flag1)
            Dim d As Date
            If TypeOf control1 Is TableCell Then
                'readonly
                If (text1.Length = 0) Then
                    text1 = "&#160;"
                End If
                d = Date.Parse(text1)
                If d = New Date(1900, 1, 1) Then
                    CType(control1, TableCell).Text = "&#160;"
                Else
                    CType(control1, TableCell).Text = d.ToString(ctx.DateFormat)
                End If
            Else
                'editable
                If Not TypeOf control1 Is DateDropDown Then
                    Throw New HttpException("Expected a date control")
                End If

                If (Not obj1 Is Nothing) Then
                    CType(control1, DateDropDown).CurrentDate = Date.Parse(obj1.ToString)
                End If

                If ((Not obj1 Is Nothing) AndAlso obj1.GetType.IsPrimitive) Then
                    CType(control1, TextBox).Columns = 5
                End If
            End If
        End Sub

    End Class

End Namespace