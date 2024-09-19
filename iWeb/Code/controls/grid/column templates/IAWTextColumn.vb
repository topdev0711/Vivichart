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


    Public NotInheritable Class IAWTextColumn
        Inherits IAWBoundColumn

#Region "properties"
        Public Property computed() As Boolean
            Get
                If ViewState("computed") Is Nothing Then Return False
                Return ViewState("computed")
            End Get
            Set(ByVal value As Boolean)
                ViewState("computed") = value
            End Set
        End Property

        Public Property ddSql() As String
            Get
                If ViewState("ddSql") Is Nothing Then Return String.empty
                Return ViewState("ddSql")
            End Get
            Set(ByVal value As String)
                ViewState("ddSql") = value
            End Set
        End Property

        Public Property columnType() As IAWWebListColumnType
            Get
                If ViewState("columnType") Is Nothing Then Return IAWWebListColumnType.text
                Return ViewState("columnType")
            End Get
            Set(ByVal value As IAWWebListColumnType)
                ViewState("columnType") = value
            End Set
        End Property

        Public Property tableid() As String
            Get
                If ViewState("tableid") Is Nothing Then Return String.Empty
                Return ViewState("tableid")
            End Get
            Set(ByVal value As String)
                ViewState("tableid") = value
            End Set
        End Property

        Public Property columnid() As String
            Get
                If ViewState("columnid") Is Nothing Then Return String.Empty
                Return ViewState("columnid")
            End Get
            Set(ByVal value As String)
                ViewState("columnid") = value
            End Set
        End Property

        Public Property trueValue() As String
            Get
                If ViewState("trueValue") Is Nothing Then Return String.Empty
                Return ViewState("trueValue")
            End Get
            Set(ByVal value As String)
                ViewState("trueValue") = value
            End Set
        End Property

        Public Property falseValue() As String
            Get
                If ViewState("falseValue") Is Nothing Then Return String.Empty
                Return ViewState("falseValue")
            End Get
            Set(ByVal value As String)
                ViewState("falseValue") = value
            End Set
        End Property

#End Region

        Public Sub New()
            MyBase.new()
        End Sub

        ' Gets a default value for a basic design-time experience.       
        Protected Overrides Function GetDesignTimeValue() As Object
            Return "<input type=""text"" >Text Column</text>"
        End Function


        ' This method is called by the ExtractRowValues methods of
        ' GridView and DetailsView. 
        Public Overrides Sub ExtractValuesFromCell( _
                ByVal dictionary As IOrderedDictionary, _
                ByVal cell As DataControlFieldCell, _
                ByVal rowState As DataControlRowState, _
                ByVal includeReadOnly As Boolean)

            Dim selectedValue As Object = Nothing

            If cell.Controls.Count > 0 Then
                Dim lbl As Label = CType(cell.Controls(0), Label)

                If lbl Is Nothing Then
                    Throw New InvalidOperationException( _
                        "TextField could not extract control.")
                Else
                    selectedValue = lbl.Text
                End If
            End If

            ' Add the value to the dictionary
            If dictionary.Contains(DataField) Then
                dictionary(DataField) = selectedValue
            Else
                dictionary.Add(DataField, selectedValue)
            End If
        End Sub


       
        ' This method adds a label control and any other 
        ' content to the cell's Controls collection.
        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)
            MyBase.InitializeDataCell(cell, rowState)
            Dim lbl As New Label()

            ' If bound to a DataField, add
            ' the OnDataBindingField method event handler to the
            ' DataBinding event.
            If DataField.Length <> 0 Then
                AddHandler lbl.DataBinding, AddressOf Me.OnDataBindField
            End If

            cell.Controls.Add(lbl)
        End Sub


        Protected Shadows Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim lbl As Label
            lbl = DirectCast(sender, Label)
            Dim dataRow As DataRowView = DirectCast(sender.Parent.Parent.DataItem, DataRowView)
            Dim val As Object

            If Not computed Then

                val = dataRow(DataField)

                Select Case columnType
                    Case IAWWebListColumnType.text
                        lbl.Text = format(dataRow, val.ToString)
                    Case IAWWebListColumnType.date
                        Dim dformat As String

                        If DataFormatString <> "" Then
                            dformat = DataFormatString
                        Else : dformat = HttpContext.Current.Session("date_format").ToString
                        End If
                        If val IsNot System.DBNull.Value Then
                            If val <> New Date(1900, 1, 1) Then lbl.Text = CDate(val).ToString(dformat)
                        End If
                    Case IAWWebListColumnType.time
                        Dim dformat As String = "HH:mm"

                        If DataFormatString <> "" Then dformat = DataFormatString
                        If val IsNot System.DBNull.Value Then
                            lbl.Text = CDate(val).ToString(dformat)
                        End If
                    Case IAWWebListColumnType.pupText
                        If val IsNot System.DBNull.Value Then
                            lbl.Text = getPupTextValue(val)
                        End If
                    Case IAWWebListColumnType.lookup
                        lbl.Text = getLookupValue(val, dataRow)
                    Case IAWWebListColumnType.checkbox
                        'If trueValue = val Then
                        '    lbl.Text = "Yes"
                        'Else
                        '    lbl.Text = "No"
                        'End If
                        'doen in IAWCheckBoxColumn
                    Case IAWWebListColumnType.image
                End Select

            Else
                lbl.Text = ""
            End If
           

        End Sub

        Private Function format(ByVal aDataRow As DataRowView, ByVal aValue As String) As String
            If DataFormatString <> "" Then
                aValue = SawUtil.Formatter(aDataRow, DataField, DataFormatString)
            End If
            Return aValue
        End Function

        Private Function getPupTextValue(ByVal aValue As String) As String
            Return SawUtil.GetPupText(tableid, columnid, aValue)
        End Function

        Private Function getLookupValue(ByVal aValue As String, ByVal aDataRow As DataRowView) As String
            Dim obj As Object = SawDB.ExecScalar(SawUtil.SubstituteValue(ddSql, aDataRow))
            If TypeOf obj Is DBNull Then
                Return String.Empty
            Else
                Return DirectCast(obj, String)
            End If
        End Function

    End Class


End Namespace