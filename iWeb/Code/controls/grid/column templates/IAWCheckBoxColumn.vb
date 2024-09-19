Imports System
Imports System.Collections.Specialized
Imports System.Collections
Imports System.ComponentModel
Imports System.Security.Permissions
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data


Namespace IAW.boundcontrols

    Public NotInheritable Class IAWCheckBoxColumn
        Inherits IAWBoundColumn

        Public Property trueValue() As String
            Get
                If ViewState("trueValue") Is Nothing Then Return "1"
                Return ViewState("trueValue")
            End Get
            Set(ByVal value As String)
                ViewState("trueValue") = value
            End Set
        End Property

        Public Property falseValue() As String
            Get
                If ViewState("falseValue") Is Nothing Then Return "0"
                Return ViewState("falseValue")
            End Get
            Set(ByVal value As String)
                ViewState("falseValue") = value
            End Set
        End Property

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
            ' Determine whether the cell contain a RadioButton 
            ' in its Controls collection.
            If cell.Controls.Count > 0 Then
                Dim img As Image = CType(cell.Controls(0), Image)

                Dim checkedValue As Object = Nothing
                If img Is Nothing Then
                    ' A checkbox is expected, but a null is encountered.
                    ' Add error handling.
                    Throw New InvalidOperationException(
                        "CheckBoxField could not extract control.")
                End If

            End If
        End Sub
        ' This method adds a RadioButton control and any other 
        ' content to the cell's Controls collection.
        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)
            MyBase.InitializeDataCell(cell, rowState)
            Dim img As New Image()

            ' If bound to a DataField, add
            ' the OnDataBindingField method event handler to the
            ' DataBinding event.
            If DataField.Length <> 0 Then
                AddHandler img.DataBinding, AddressOf Me.OnDataBindField
            End If

            cell.HorizontalAlign = HorizontalAlign.Center
            cell.Controls.Add(img)
        End Sub


        Protected Shadows Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim img As Image
            img = DirectCast(sender, Image)
            Dim val As Object = DirectCast(sender.Parent.Parent.DataItem, DataRowView)(DataField)
            If TypeOf val Is DBNull Then
                img.ImageUrl = "~/graphics/1px.gif"
                Return
            End If
            Select Case val.ToString
                Case trueValue
                    'img.ImageUrl = "~/graphics/tick2.png"
                    img.ImageUrl = "~/graphics/1px.gif"
                    img.CssClass = "IconImg Icon16 IconConfirm"

                    img.AlternateText = "Yes"
                    img.ToolTip = "Yes"
                Case falseValue
                    'img.ImageUrl = "~/graphics/cross2.png"
                    img.ImageUrl = "~/graphics/1px.gif"
                    img.CssClass = "IconImg Icon16 IconCamcel"
                    img.AlternateText = "No"
                    img.ToolTip = "No"
                Case Else
                    img.ImageUrl = "~/graphics/1px.gif"
                    img.AlternateText = ""
            End Select

        End Sub

    End Class

End Namespace