Imports IAW.controls

<WebType(WebType.iWebCore), ProcessRequired(False), DirtyPageHandling(False)> _
Partial Class errorReport
    Inherits stub_IngenWebPage

    Private deleteSimilar As Boolean

#Region "properties"

    Public Property ModalState() As String
        Get
            Dim B As String = ViewState("ModalState")
            If B = Nothing Then
                ViewState("ModalState") = ""
                B = False
            End If
            Return B
        End Get
        Set(ByVal value As String)
            ViewState("ModalState") = value
        End Set
    End Property

    Public ReadOnly Property PageIndex() As Integer
        Get
            Return Me.pagingGrid.CurrentPage
        End Get
    End Property
    Public ReadOnly Property PageSize() As Integer
        Get
            Return Me.pagingGrid.PageSize
        End Get
    End Property
#End Region

#Region "page events"

    Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
    End Sub
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    End Sub
    Protected Overrides Sub CreateChildControls()

        ODS1.UpdateParameters.Add(New Parameter("id", TypeCode.Int32))

        'cancel the datasource update, we'll handle it in code
        AddHandler ODS1.Updating, AddressOf DSUpdate

        'change the back btn to redirect to MaintainViews.aspx
        Dim bBar As ButtonBar = ctx.buttonBar
        Dim BTN As IAWHyperLinkButton = bBar.addLinkButton(AddressOf DeleteButton_Click, "Delete")
        BTN.ID = "Delete Selected"
        BTN.TranslateText = False
        BTN.Attributes.Add("onclick", "hidebuttons(this);")

        'delete similar
        BTN = bBar.addLinkButton(AddressOf DeleteSimilar_Click, "Delete Similar")
        BTN.ID = "Delete Selected & Delete similar"
        BTN.TranslateText = False
        BTN.Attributes.Add("onclick", "hidebuttons(this);")
        BTN.CssClass = "hlink IconBtn Icon16 BtnDelete"

        grdViewErrorLog.EmptyDataText = "There are no items To display"

    End Sub

    Private Sub SetModalState()
        mpeError.Hide()

        Select Case ModalState
            Case "Error"
                mpeError.Show()

        End Select

    End Sub
#End Region

#Region "click handlers"
    Private Sub DeleteButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        deleteSimilar = False
        Save()
    End Sub
    Private Sub DeleteSimilar_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        deleteSimilar = True
        Save()
    End Sub
    Private Sub Save()
        grdViewErrorLog.Save()
        Me.pagingGrid.CurrentPage = 1
        grdViewErrorLog.DataBind()
    End Sub
#End Region

    Private Function ConvertToINT(ByVal number As Object) As Integer
        Return CInt(number)
    End Function

#Region "grid events"

    Private Sub btnErrorCancel_Click(sender As Object, e As EventArgs) Handles btnErrorCancel.Click
        ModalState = ""
        setmodalstate
    End Sub

    Private Sub grdViewErrorLog_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdViewErrorLog.RowCommand

        If e.CommandName <> "ViewError" Then Return

        Dim EventID As Integer = CInt(e.CommandArgument)

        Dim CR As String = Environment.NewLine + Environment.NewLine
        Dim SQL As String

        SQL = "Select wel_eventid," +
              "       wel_datetime," +
              "       wel_source," +
              "       wel_message," +
              "       wel_form," +
              "       wel_querystring," +
              "       wel_targetsite," +
              "       wel_stacktrace," +
              "       wel_referer," +
              "       wel_user_ref," +
              "       wel_ip_address," +
              "       IsNull(wel_sql,'') as wel_sql," +
              "       wel_sql_params " +
              "  FROM dbo.web_error_log " +
              " WHERE wel_eventid = @p1"
        Dim DT As DataTable = IawDB.execGetTable(SQL, EventID)

        Dim Txt As String = ""

        If DT.Rows.Count = 0 Then
            Txt = "Unknown Error"
        Else
            Dim DR As DataRow = DT.Rows(0)

            Txt = "EVENT ID : " + DR("wel_eventid").ToString + CR +
                  "DATETIME : " + DR("wel_datetime").ToString + CR +
                  "MESSAGE  : " + DR("wel_message").ToString + CR +
                  "USER     : " + DR("wel_user_ref").ToString + CR +
                  "URL      : " + DR("wel_querystring").ToString + CR +
                  "SOURCE   : " + DR("wel_source").ToString + CR +
                  "STACK TRACE" + CR +
                  DR("wel_stacktrace").ToString.Replace("<br>", "").Replace("<hr />", CR)
            If DR("wel_sql") <> "" Then
                Txt += CR + "SQL" + CR
                If DR("wel_sql_params").ToString.Trim <> "" Then
                    Txt += "PARMS : " + DR("wel_sql_params").ToString + CR
                End If
                Try
                    Txt += (New SqlBeautify).Beautify(DR("wel_sql").ToString)
                Catch ex As Exception
                    Txt += DR("wel_sql").ToString
                End Try
                Txt += CR
            End If
        End If

        txtErrorInfo.Text = Txt

        ModalState = "Error"
        SetModalState()

    End Sub

    Protected Sub grdViewErrorLog_RowUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewUpdateEventArgs) Handles grdViewErrorLog.RowUpdating

        Dim id As Integer = sender.DataKeys.Item(e.RowIndex)(0)

        If deleteSimilar Then
            ODS1.DeleteMethod = "DeleteSimilar"
        Else
            ODS1.DeleteMethod = "DeleteItem"
        End If

        ODS1.DeleteParameters.Clear()
        ODS1.DeleteParameters.Add("wel_eventid", TypeCode.Int32, e.Keys.Item("wel_eventid"))
        ODS1.Delete()

        'cancel update
        e.Cancel = True
    End Sub
#End Region

#Region "data source events"
    Private Sub DSUpdate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs)
        e.Cancel = True
    End Sub
    Private Sub ODS1_Selected(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.ObjectDataSourceStatusEventArgs) Handles ODS1.Selected
        Me.pagingGrid.TotalRecords = e.OutputParameters("totalRecords")
    End Sub
    Private Sub ODS1_Selecting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.ObjectDataSourceSelectingEventArgs) Handles ODS1.Selecting
        e.InputParameters.Item("totalRecords") = Me.pagingGrid.TotalRecords
    End Sub

#End Region



End Class
