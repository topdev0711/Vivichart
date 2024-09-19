
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

    Public MustInherit Class IAWBoundColumn
        Inherits BoundField

#Region "Properties"

        Protected Overrides ReadOnly Property SupportsHtmlEncode() As Boolean
            Get
                Return False
            End Get
        End Property

        Protected Overridable Property style() As String
            Get
                If ViewState("style") Is Nothing Then Return String.Empty
                Return ViewState("style")
            End Get
            Set(ByVal value As String)
                ViewState("style") = value
            End Set
        End Property

        Protected Overridable Property styleType() As IAWDerivationType
            Get
                If ViewState("styleType") Is Nothing Then Return IAWDerivationType.none
                Return ViewState("styleType")
            End Get
            Set(ByVal value As IAWDerivationType)
                ViewState("styleType") = value
            End Set
        End Property

        Protected Overridable Property isVisible() As Boolean
            Get
                If ViewState("isVisible") Is Nothing Then Return String.Empty
                Return ViewState("isVisible")
            End Get
            Set(ByVal value As Boolean)
                ViewState("isVisible") = value
                MyBase.Visible = value
            End Set
        End Property

#End Region

        Protected Sub New()
        End Sub

        Public Overrides Sub InitializeCell(ByVal cell As System.Web.UI.WebControls.DataControlFieldCell, ByVal cellType As System.Web.UI.WebControls.DataControlCellType, ByVal rowState As System.Web.UI.WebControls.DataControlRowState, ByVal rowIndex As Integer)
            If Not ShowHeader And cellType = DataControlCellType.Header Then Return
            MyBase.InitializeCell(cell, cellType, rowState, rowIndex)
            Select Case cellType
                Case DataControlCellType.DataCell
                    Select Case rowState
                        Case DataControlRowState.Alternate
                            cell.CssClass = "listdataalt"
                        Case DataControlRowState.Normal
                            cell.CssClass = "listdata"
                    End Select
                Case DataControlCellType.Footer
                    cell.CssClass = "listfooter"
                Case DataControlCellType.Header
                    cell.CssClass = "listheader"
                    SortImages(cell)
            End Select
        End Sub

        Private Sub SortImages(ByVal cell As System.Web.UI.WebControls.DataControlFieldCell)
            'add sort images
            Dim sort As New Image
            Dim button As LinkButton
            Dim dcf As DataControlField = cell.ContainingField
            Dim grd As GridView = CType(Me.Control, GridView)

            sort.ImageAlign = ImageAlign.AbsMiddle

            If cell.Controls.Count > 0 Then
                If TypeOf cell.Controls.Item(0) Is LinkButton Then
                    button = CType(cell.Controls.Item(0), LinkButton)
                    If Not String.IsNullOrEmpty(button.Text) Then

                        If (grd.SortExpression = dcf.SortExpression) Then
                            If (grd.SortDirection = SortDirection.Descending) Then
                                sort.ImageUrl = "~/graphics/sort_down.gif"
                                sort.AlternateText = "Sort Down"
                            Else
                                sort.ImageUrl = "~/graphics/sort_up.gif"
                                sort.AlternateText = "Sort Up"
                            End If
                        Else
                            sort.ImageUrl = "~/graphics/alert_blank.gif"
                            sort.AlternateText = ""
                        End If
                        cell.Controls.AddAt(0, New LiteralControl("<span>"))
                        cell.Controls.Add(sort)
                        button.Controls.Add(New LiteralControl(button.Text))
                        cell.Controls.Add(New LiteralControl("</span>"))
                    End If
                End If
            End If
        End Sub

        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)

            AddHandler cell.DataBinding, AddressOf Me.OnDataBindField

        End Sub

        Protected Overrides Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim cell As DataControlFieldCell = DirectCast(sender, DataControlFieldCell)
            Dim container As GridViewRow = DirectCast(cell.NamingContainer, GridViewRow)
            Dim rowData As DataRowView = DirectCast(container.DataItem, DataRowView)
            doStyle(cell, rowData)

        End Sub

        Private Sub doStyle(ByVal aCell As DataControlFieldCell, ByVal aRowData As DataRowView)
            If style = String.Empty Or style Is Nothing Then Return
            Select Case styleType
                Case IAWDerivationType.plaintext
                    populateStyle(aCell, style)
                Case IAWDerivationType.sql
                    populateStyle(aCell, SawDB.ExecScalar(style))
                Case IAWDerivationType.none

                Case Else
                    Throw New Exception("Unsupported DerivationType - IAWBoundColumn")
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