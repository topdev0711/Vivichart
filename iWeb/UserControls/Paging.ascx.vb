Imports System.Reflection
Imports IAW.controls

Partial Class Paging
    Inherits System.Web.UI.UserControl
    Implements IPostBackContainer, IPostBackEventHandler

    Private Const MaxPageSize As Integer = 10000
    Private _gridToPage As GridView

    Private IndexHasChanged As Boolean
    Public Event PageIndexChanged(ByVal sender As Object, ByVal e As PageIndexChangedEventArgs)
    Public Event PageSizeChanged As EventHandler(Of EventArgs)

#Region "properties"
    Public Property TotalRecords() As Integer
        Get
            Return Me.ViewState("TotalRecords")
        End Get
        Set(ByVal value As Integer)
            ' totalPages = ((totalIssues - 1) \ PageSize) + 1
            Me.ViewState("TotalRecords") = value
            Me.TotalPages = ((value - 1) \ Me.PageSize) + 1
            Me.TotalRecordsCount.Text = "(" + value.ToString + ")"
            SetPageLinkVisibility()
            CurrentPageLabel.Text = CurrentPage.ToString()
            Update()
        End Set
    End Property
    Public Property TotalPages() As Integer
        Get
            Return Me.ViewState("totalPages")
        End Get
        Set(ByVal value As Integer)
            Me.ViewState("totalPages") = value
            TotalPagesLabel.Text = value.ToString()
        End Set
    End Property
    Public ReadOnly Property PageSettings() As AppPaging
        Get
            Dim o As New AppPaging
            With o
                .PageSize = Me.PageSize
                .CurrentPage = Me.CurrentPage
            End With
            Return o
        End Get
    End Property
    Public Property CurrentPage() As Integer
        Get
            Dim o As Object = Me.ViewState("CurrentPage")
            If o Is Nothing Then
                Return 1
            End If
            Return CInt(o)
        End Get
        Set(ByVal value As Integer)
            Me.ViewState("CurrentPage") = value
            CurrentPageLabel.Text = value.ToString()
        End Set
    End Property
    Public Property PageSize() As Integer
        Get
            Dim i As Integer
            If Integer.TryParse(ctx.Session("Pager_PageSize"), i) AndAlso i > 0 Then
                Return i
            Else
                'nothing on the session, return the default
                Return Me.ViewState("Pager_PageSize")
            End If
        End Get
        Set(ByVal value As Integer)
            Me.ViewState("Pager_PageSize") = value
        End Set
    End Property
    Private Property OriginalPageSize() As Integer
        Get
            Dim i As Integer = Me.ViewState("OriginalPageSize")
            If i = 0 Then
                i = 20
                Me.ViewState("OriginalPageSize") = i
            End If
            Return Me.ViewState("OriginalPageSize")
        End Get
        Set(ByVal value As Integer)
            Me.ViewState("OriginalPageSize") = value
        End Set
    End Property
    Public ReadOnly Property PagingRemoved() As Boolean
        Get
            Return Me.PageSize = Paging.MaxPageSize
        End Get
    End Property
    ''' <summary>
    ''' The ID of the SmartGrid that this control will page
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GridID() As String
        Get
            Return Me.ViewState("GridID")
        End Get
        Set(ByVal value As String)
            Me.ViewState("GridID") = value
        End Set
    End Property
    ''' <summary>
    ''' Internal property
    ''' The SmartGrid that this control will page (and order)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property GridToPage() As GridView
        Get
            If String.IsNullOrEmpty(Me.GridID) Then
                Return Nothing
            End If

            If Me._gridToPage IsNot Nothing Then
                Return Me._gridToPage
            End If

            'try and grab the grid
            Dim GRD As GridView = Me.NamingContainer.FindControl(Me.GridID)
            If GRD Is Nothing Then
                Throw New NotSupportedException(String.Format("Unable to find a Grid control in the same naming container with an ID of {0}", Me.GridID))
            End If

            Me._gridToPage = GRD

            Return Me._gridToPage
        End Get
    End Property
#End Region

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        If Not IsPostBack Then
            FirstButton.ImageUrl = ctx.themeGraphicsDir + "page_first.png"
            PreviousButton.ImageUrl = ctx.themeGraphicsDir + "page_previous.png"
            NextButton.ImageUrl = ctx.themeGraphicsDir + "page_next.png"
            LastButton.ImageUrl = ctx.themeGraphicsDir + "page_last.png"
        End If

        ScriptManager.GetCurrent(Me.Page).RegisterAsyncPostBackControl(Me)

    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Me.GridToPage IsNot Nothing Then
            'handler grid sorting
            AddHandler Me.GridToPage.Sorting, AddressOf GRD_Sorting
        End If
    End Sub

    Protected Sub RaisePageIndexChanged()
        CurrentPageLabel.Text = CurrentPage.ToString()
        TotalPagesLabel.Text = TotalPages.ToString()
        RaiseEvent PageIndexChanged(Me, New PageIndexChangedEventArgs(Me.CurrentPage))
        IndexHasChanged = True
        SetPageLinkVisibility()
    End Sub

#Region "private routines"
    ''' <summary>
    ''' Determines which paging controls are enabled, based on the current page.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetPageLinkVisibility()
        If Me.TotalPages <= 1 Then
            Me.Visible = False
            Return
        Else
            Me.Visible = True
        End If

        NextButton.Enabled = True
        PreviousButton.Enabled = True
        LastButton.Enabled = True
        FirstButton.Enabled = True

        '///next button
        If CurrentPage = TotalPages Then
            NextButton.Enabled = False
            LastButton.Enabled = False
        End If

        '///previous button
        If CurrentPage = 1 Then
            PreviousButton.Enabled = False
            FirstButton.Enabled = False
        End If
    End Sub
#End Region

#Region "public routines"
    '''' <summary>
    '''' Registers the paging controls as AsyncPostBackControl's
    '''' </summary>
    '''' <param name="AjaxScriptManager"></param>
    '''' <remarks>
    '''' Useful when you have a paged grid/list but when the updatePanel is ChildrenAsTriggers="false", if you don't want the control/links on the grid to be asynch
    '''' NB, in that scenario the paging control has to go into a seperate updatePanel which can be nested within the main one
    '''' </remarks>
    'Public Sub AddAsynchronousTriggers(ByVal AjaxScriptManager As ScriptManager)
    '    AjaxScriptManager.RegisterAsyncPostBackControl(Me.FirstButton)
    '    AjaxScriptManager.RegisterAsyncPostBackControl(Me.PreviousButton)
    '    AjaxScriptManager.RegisterAsyncPostBackControl(Me.NextButton)
    '    AjaxScriptManager.RegisterAsyncPostBackControl(Me.LastButton)
    'End Sub

    '''' <summary>
    '''' Removes paging by setting the page size to MaxPageSize
    '''' </summary>
    '''' <remarks>
    '''' </remarks>
    Public Sub RemovePaging()
        If Me.PageSize <> MaxPageSize Then
            Me.OriginalPageSize = Me.PageSize
            Me.PageSize = MaxPageSize
            Me.CurrentPage = 1
        End If
    End Sub

    '''' <summary>
    '''' Restores paging by setting the page size to it's original value
    '''' </summary>
    '''' <remarks></remarks>
    Public Sub RestorePaging()
        If Me.PageSize = MaxPageSize Then
            Me.PageSize = Me.OriginalPageSize
            Me.CurrentPage = 1
        End If
    End Sub
#End Region

#Region "event handlers"
    Protected Overrides Sub Render(ByVal writer As System.Web.UI.HtmlTextWriter)
        If Not IndexHasChanged  Then
            SetPageLinkVisibility()
            CurrentPageLabel.Text = CurrentPage.ToString()
        End If
        If Me.TotalPages > 1 Then
            MyBase.Render(writer)
        End If
    End Sub

    Protected Sub HandleEvent(ByVal e As CommandEventArgs)
        Me.Page_OnClick(Nothing, e)
    End Sub

    Public Sub Page_OnClick(ByVal sender As Object, ByVal e As CommandEventArgs)
        Select Case e.CommandName
            Case "SelectPage"
                CurrentPage = e.CommandArgument

            Case "LastPage"
                CurrentPage = Me.TotalPages

            Case "FirstPage"
                CurrentPage = 1

            Case "NextPage"
                CurrentPage += 1

            Case "PreviousPage"
                CurrentPage -= 1

        End Select

        OnPageIndexChanged()
    End Sub

#End Region

#Region "raise events"
    Protected Sub OnPageIndexChanged()
        CurrentPageLabel.Text = CurrentPage.ToString()
        TotalPagesLabel.Text = TotalPages.ToString()
        RaiseEvent PageIndexChanged(Me, New PageIndexChangedEventArgs(Me.CurrentPage))
        IndexHasChanged = True
        SetPageLinkVisibility()
        CurrentPageLabel.Text = CurrentPage.ToString()
    End Sub

    Protected Sub OnPageSizeChanged()
        RaiseEvent PageSizeChanged(Me, EventArgs.Empty)
    End Sub
#End Region


    Public Function GetPostBackOptions(ByVal buttonControl As System.Web.UI.WebControls.IButtonControl) As System.Web.UI.PostBackOptions Implements System.Web.UI.WebControls.IPostBackContainer.GetPostBackOptions

        If (buttonControl Is Nothing) Then
            Throw New ArgumentNullException("buttonControl")
        End If

        If buttonControl.CausesValidation Then
            Throw New InvalidOperationException(String.Format("CannotUseParentPostBackWhenValidating", New Object() {MyBase.GetType.Name, Me.ID}))
        End If

        Dim options As New PostBackOptions(Me, (buttonControl.CommandName & "$" & buttonControl.CommandArgument))
        options.RequiresJavaScriptProtocol = True

        Return options
    End Function

    '''' <summary>
    '''' IMPORTANT -  render an client id on the controls div tag
    '''' this is required for the event source to be found for dirty page handling
    '''' </summary>
    '''' <param name="writer"></param>
    '''' <remarks></remarks>
    'Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
    '    writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.ClientID, True)
    '    writer.RenderBeginTag(HtmlTextWriterTag.Div)
    '    MyBase.Render(writer)
    '    writer.RenderEndTag()
    'End Sub

    Protected Sub ValidateEvent(ByVal unique_ID As String, ByVal eventArgument As String)
        Me.Page.ClientScript.ValidateEvent(unique_ID, eventArgument)
    End Sub

    Public Sub RaisePostBackEvent(ByVal eventArgument As String) Implements System.Web.UI.IPostBackEventHandler.RaisePostBackEvent
        Me.ValidateEvent(Me.UniqueID, eventArgument)
        Dim index As Integer = eventArgument.IndexOf("$"c)
        If (index >= 0) Then
            Me.HandleEvent(New CommandEventArgs(eventArgument.Substring(0, index), eventArgument.Substring((index + 1))))
        End If
    End Sub

    ''updates the update panel that the paging control resides in
    Public Sub Update()
        Me.UPD_paging.Update()
    End Sub

    Private Sub GRD_Sorting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewSortEventArgs)
        Me.Reset()
    End Sub

    ''' <summary>
    ''' Sets the current page back to 1, and updates the paging panel
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Reset()
        Me.CurrentPage = 1
        Me.UPD_paging.Update()
    End Sub

End Class

<Serializable()> _
Public Class AppPaging

    Private _pageSize As Integer
    Private _currentPage As Integer
    Private _totalRecords As Integer

#Region "properties"
    Public Property CurrentPage() As Integer
        Get
            If Me._currentPage <= 0 Then
                Return 1
            End If
            Return Me._currentPage
        End Get
        Set(ByVal value As Integer)
            Me._currentPage = value
        End Set
    End Property
    Public Property PageSize() As Integer
        Get
            Return Me._pageSize
        End Get
        Set(ByVal value As Integer)
            Me._pageSize = value
        End Set
    End Property
    Public Property TotalRecords() As Integer
        Get
            Return Me._totalRecords
        End Get
        Set(ByVal value As Integer)
            Me._totalRecords = value
        End Set
    End Property
    Public ReadOnly Property Skip() As Integer
        Get
            Return (Me.CurrentPage - 1) * Me.PageSize
        End Get
    End Property
#End Region

    Public Sub New()
        'Me._pageSize = 20
        'Me._currentPage=1
    End Sub

End Class


Public Class PageIndexChangedEventArgs
    Inherits System.EventArgs

    Private _CurrentPage As Integer

    Public ReadOnly Property CurrentPage() As Integer
        Get
            Return _CurrentPage
        End Get
    End Property

    Public Sub New(ByVal Page As Integer)

        _CurrentPage = Page
    End Sub

End Class