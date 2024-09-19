Imports System
imports System.Web
imports System.Data
imports System.Web.UI
imports System.Web.UI.WebControls


Namespace IAW.templates


    Public Class IAWStyleTemplate
        Implements ITemplate, INamingContainer

        'could extend to contain the column(s), values and list of styles, ie bases the style on a column value?
        'Private _column As String
        'private _value_style as System.Collections.Generic.Dictionary(Of String,String)

        Private _style As String
        Private _derivationType As IAWDerivationType

        Public Sub InstantiateIn(ByVal container As System.Web.UI.Control) Implements System.Web.UI.ITemplate.InstantiateIn
            AddHandler container.DataBinding, AddressOf container_BindData
        End Sub

        Public Sub container_BindData(ByVal sender As Object, ByVal e As EventArgs)
            Dim cell As DataControlFieldCell = DirectCast(sender, DataControlFieldCell)
            Dim container As GridViewRow = DirectCast(cell.NamingContainer, GridViewRow)
            Dim rowData As DataRowView = DirectCast(container.DataItem, DataRowView)
            'Dim val As String = DirectCast(container.DataItem, DataRowView)(_column).ToString()
            If _style = String.Empty Or _style Is Nothing Then Return
            Select Case _derivationType
                Case IAWDerivationType.plaintext
                    populateStyle(cell, _style)
                Case IAWDerivationType.sql
                    populateStyle(cell, SawDB.ExecScalar(_style))
                Case IAWDerivationType.none

                Case Else
                    Throw New Exception("Unsupported DerivationType - IAWStyleTemplate")
            End Select
        End Sub

        Private Sub populateStyle(ByVal aCell As DataControlFieldCell, ByVal aStyle As String)
            'split on ; to get name-value pairs
            'split on : to get name & value
            For Each s As String In aStyle.Split(";")
                Dim val() As String = s.Split(":")
                If val.Length = 2 Then
                    aCell.Style.Add(val(0), val(1))
                End If
            Next
        End Sub

    End Class


End Namespace