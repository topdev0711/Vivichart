Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Web.UI
Imports System.Web.UI.WebControls
'Imports Microsoft.Security.Application

Namespace IAW.controls

    Public Class IAWGrid
        Inherits System.Web.UI.WebControls.GridView

        Private dirtyRows As New List(Of Integer)

#Region "properties"

        <IDReferenceProperty(GetType(Control))> _
        Public Property SaveButtonId() As String
            Get
                Dim val As String = ViewState("SaveButtonId")
                If val Is Nothing Then Return String.Empty
                Return val
            End Get
            Set(ByVal value As String)
                ViewState("SaveButtonId") = value
            End Set
        End Property

        Public Property editState() As DataControlRowState
            Get
                If Me.ViewState("_editState") Is Nothing Then
                    'default to edit
                    Me.ViewState("_editState") = DataControlRowState.Edit
                End If
                Return Me.ViewState("_editState")
            End Get
            Set(ByVal value As DataControlRowState)
                Me.ViewState("_editState") = value
            End Set
        End Property

        Public Property TranslateHeadings() As Boolean
            Get
                If Me.ViewState("_TranslateHeadings") Is Nothing Then
                    Me.ViewState("_TranslateHeadings") = True
                End If
                Return Me.ViewState("_TranslateHeadings")
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState("_TranslateHeadings") = value
            End Set
        End Property

#End Region

#Region "overrides"

        Protected Overrides Sub CreateChildControls()
            If Me.Page.IsPostBack Then
                Me.dirtyRows = Me.ViewState("dirtyRows")
            End If
            MyBase.CreateChildControls()
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            'attach event handler to save button
            If Not String.IsNullOrEmpty(Me.SaveButtonId) Then
                Dim btn As Control = RecursiveFindControl(Me.NamingContainer.NamingContainer, Me.SaveButtonId)
                If btn IsNot Nothing Then
                    If TypeOf btn Is Button Then
                        AddHandler CType(btn, Button).Click, AddressOf SaveClicked
                    ElseIf TypeOf btn Is IAWHyperLinkButton Then
                        AddHandler CType(btn, IAWHyperLinkButton).Click, AddressOf SaveClicked
                    End If
                End If
            End If

            Me.SelectedRowStyle.CssClass = "selected_row"

        End Sub

        Protected Overrides Function CreateRow(ByVal rowIndex As Integer, ByVal dataSourceIndex As Integer, ByVal rowType As System.Web.UI.WebControls.DataControlRowType, ByVal rowState As System.Web.UI.WebControls.DataControlRowState) As System.Web.UI.WebControls.GridViewRow
            If Me.editState = DataControlRowState.Edit Then
                Return MyBase.CreateRow(rowIndex, dataSourceIndex, rowType, Me.editState)
            Else
                Return MyBase.CreateRow(rowIndex, dataSourceIndex, rowType, rowState)
            End If
        End Function

        Protected Overrides Sub InitializeRow(ByVal row As System.Web.UI.WebControls.GridViewRow, ByVal fields() As System.Web.UI.WebControls.DataControlField)
            MyBase.InitializeRow(row, fields)

            If row.RowType = DataControlRowType.DataRow AndAlso Me.EditState = DataControlRowState.Edit Then
                'set the css row styles
                'disable the edit row style, otherwise all the rows show the selected bg colour
                If (row.RowIndex Mod 2 <> 0) Then
                    row.CssClass = Me.AlternatingRowStyle.CssClass
                Else
                    row.CssClass = Me.RowStyle.CssClass
                End If
                row.Attributes("data-class") = row.CssClass
            End If

            For Each cell As Control In row.Cells
                If cell.Controls.Count > 0 Then
                    AddChangedHandlers(cell.Controls)
                End If
            Next
        End Sub

#End Region

#Region "grid events"

        Private Sub IAWGrid_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles Me.RowDataBound
            Dim ctrl As Control
            If e.Row.RowType = DataControlRowType.DataRow Then
                If e.Row.RowIndex Mod 2 = 0 Then
                    e.Row.Attributes("data-class") = Me.RowStyle.CssClass
                Else
                    e.Row.Attributes("data-class") = Me.AlternatingRowStyle.CssClass
                End If

                ' set graphics for standard amend and delete image buttons
                Dim IMG As ImageButton

                ctrl = e.Row.FindControl("btnAmend")
                If ctrl IsNot Nothing Then
                    IMG = DirectCast(ctrl, ImageButton)
                    'IMG.ImageUrl = "~/graphics/modify.png"
                    IMG.ImageUrl = "~/graphics/1px.gif"
                    IMG.CssClass = "IconPic Icon16 IconEdit"
                End If

                ctrl = e.Row.FindControl("btnDel")
                If ctrl IsNot Nothing Then
                    IMG = DirectCast(ctrl, ImageButton)
                    'IMG.ImageUrl = "~/graphics/trash.png"
                    IMG.ImageUrl = "~/graphics/1px.gif"
                    IMG.CssClass = "IconPic Icon16 IconDelete"
                End If

                For i As Integer = 1 To 3
                    ctrl = e.Row.FindControl("btnDel" + i.ToString)
                    If ctrl IsNot Nothing Then
                        IMG = DirectCast(ctrl, ImageButton)
                        'IMG.ImageUrl = "~/graphics/trash.png"
                        IMG.ImageUrl = "~/graphics/1px.gif"
                        IMG.CssClass = "IconPic Icon16 IconDelete"
                    End If
                Next

                ctrl = e.Row.FindControl("btnAdd")
                If ctrl IsNot Nothing Then
                    IMG = DirectCast(ctrl, ImageButton)
                    'IMG.ImageUrl = "~/graphics/add.png"
                    IMG.ImageUrl = "~/graphics/1px.gif"
                    IMG.CssClass = "IconPic Icon16 IconAdd"
                End If
            End If

            If e.Row.RowType = DataControlRowType.Header And TranslateHeadings Then
                For i As Integer = 0 To e.Row.Cells.Count - 1
                    If e.Row.Cells(i).Controls.Count > 0 Then
                        Dim btn As LinkButton
                        Dim lit As LiteralControl
                        ctrl = e.Row.Cells(i).Controls(0)
                        If TypeOf ctrl Is LinkButton Then
                            btn = CType(ctrl, LinkButton)
                            btn.Text = SawLang.Translate(btn.Text)
                        End If
                        If TypeOf ctrl Is LiteralControl Then
                            lit = CType(ctrl, LiteralControl)
                            lit.Text = SawLang.Translate(lit.Text)
                        End If
                    Else
                        e.Row.Cells(i).Text = SawLang.Translate(e.Row.Cells(i).Text)
                    End If
                Next
            End If
        End Sub

        Private Sub IAWGrid_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            Me.ViewState("dirtyRows") = Me.dirtyRows
            '///add dirty page handling to the paging links
            If Me.editState = DataControlRowState.Edit And ctx.UseDirtyPageHandling And Me.BottomPagerRow IsNot Nothing Then
                AddDirtyPageHandling(Me.BottomPagerRow)
            End If
        End Sub

        Protected Overrides Sub OnPageIndexChanging(ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs)
            MyBase.OnPageIndexChanging(e)
        End Sub

        Protected Overrides Sub OnPageIndexChanged(ByVal e As System.EventArgs)
            If Me.editState = DataControlRowState.Edit And ctx.UseDirtyPageHandling Then
                '///emit the javascript that clears the dirty page flag when the user moves between the pages
                Dim js As String
                js = "<script type='text/javascript'>" & _
                         "   DirtyPage.setPageIsDirty(false);" & _
                         "</script>"
                ScriptManager.RegisterStartupScript(Me.Page, Me.GetType(), "ClearPageIsDirty", js, False)
            End If
            MyBase.OnPageIndexChanged(e)
        End Sub

#End Region

        'Protected Overrides Sub OnRowDataBound(ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs)
        '    If e.Row.RowType = DataControlRowType.DataRow AndAlso e.Row.Cells(0).Controls.Count > 0 Then
        '        'if the control is not editable then allow alternate rows
        '        If Me.AutoGenerateSelectButton Then
        '            'set the style on the select link
        '            Dim btn As LinkButton = e.Row.Cells(0).Controls(0)
        '            btn.Text = SawLang.Translate(btn.Text)
        '            Select Case e.Row.RowState
        '                Case DataControlRowState.Normal
        '                    btn.CssClass = "listLinkRow"
        '                Case DataControlRowState.Alternate
        '                    btn.CssClass = "listLinkAltRow"
        '                Case DataControlRowState.Selected
        '                    btn.CssClass = "listLinkSelectedRow"
        '                Case Else
        '                    btn.CssClass = "listLinkSelectedRow"
        '            End Select

        '        End If
        '    End If
        '    MyBase.OnRowDataBound(e)
        'End Sub


#Region "click handlers"

        Private Sub SaveClicked(ByVal sender As Object, ByVal e As EventArgs)
            Me.Save()
        End Sub

        Private Sub SaveClicked(ByVal sender As Object, ByVal e As ImageClickEventArgs)
            Me.Save()
        End Sub

#End Region

#Region "helper functions"

        Private Sub AddChangedHandlers(ByVal controls As ControlCollection)
            If controls Is Nothing Then Return
            For Each ctrl As Control In controls
                If TypeOf ctrl Is TextBox Then
                    AddHandler CType(ctrl, TextBox).TextChanged, AddressOf HandleRowChanged
                ElseIf TypeOf ctrl Is CheckBox Then
                    AddHandler CType(ctrl, CheckBox).CheckedChanged, AddressOf HandleRowChanged
                ElseIf TypeOf ctrl Is DropDownList Then
                    AddHandler CType(ctrl, DropDownList).SelectedIndexChanged, AddressOf HandleRowChanged
                ElseIf TypeOf ctrl Is DateDropDown Then
                    AddHandler CType(ctrl, DateDropDown).DateChanged, AddressOf HandleRowChanged
                ElseIf TypeOf ctrl Is Panel Then
                    '///if  div then go through the controls in the div
                    If ctrl.Controls.Count > 0 Then
                        AddChangedHandlers(ctrl.Controls)
                    End If
                End If
            Next
        End Sub

        Public Sub Delete()
            For Each row As Integer In dirtyRows
                Me.DeleteRow(row)
            Next
            dirtyRows.Clear()
            Me.DataBind()
        End Sub

        Public Sub SaveWithoutValidation()
            For Each row As Integer In dirtyRows
                Me.UpdateRow(row, False)
            Next
            dirtyRows.Clear()
            Me.DataBind()
        End Sub

        Public Sub Save()
            If Me.Page.IsValid Then
                For Each row As Integer In dirtyRows
                    Me.UpdateRow(row, False)
                Next
                dirtyRows.Clear()
                Me.DataBind()
            End If
        End Sub

        Protected Sub HandleRowChanged(ByVal sender As Object)
            HandleRowChanged(sender, New EventArgs())
        End Sub

        Protected Sub HandleRowChanged(ByVal sender As Object, ByVal e As EventArgs)
            If dirtyRows Is Nothing Then Return
            Dim row As GridViewRow = CType(sender, Control).NamingContainer
            'ignore header rows, index=-1
            If row IsNot Nothing And Not dirtyRows.Contains(row.RowIndex) And row.RowIndex >= 0 Then
                dirtyRows.Add(row.RowIndex)
            End If
        End Sub

        Protected Function RecursiveFindControl(ByVal Container As Control, ByVal Id As String) As Control
            If Container Is Nothing Then Return Nothing
            If String.IsNullOrEmpty(Id) Then Return Nothing

            Dim cntrl As Control = Container.FindControl(Id)
            If cntrl Is Nothing Then
                For Each c As Control In Container.Controls
                    'move up tree of naming containers
                    cntrl = RecursiveFindControl(c, Id)
                    If cntrl IsNot Nothing Then Return cntrl
                Next
                Return Nothing
            Else
                Return cntrl
            End If
        End Function

        Private Function AddDirtyPageHandling(ByVal aControl As WebControl) As WebControl
            Dim link As LinkButton
            If aControl Is Nothing Then Return aControl
            For Each webCtrl As WebControl In aControl.Controls
                If TypeOf webCtrl Is LinkButton Then
                    link = webCtrl
                    Dim js As New StringBuilder
                    js.AppendLine("var lnk = $get('" + link.ClientID + "');if(lnk!=null)$addHandler(lnk, 'click',DirtyPage.linkbutton_onclick)")
                    ScriptManager.RegisterStartupScript(Me, Me.GetType, link.ClientID, js.ToString, True)

                ElseIf webCtrl.Controls.Count > 0 Then
                    '///look at children
                    AddDirtyPageHandling(webCtrl)
                Else
                    Return webCtrl
                End If
            Next
            Return aControl
        End Function

#End Region

    End Class


End Namespace
