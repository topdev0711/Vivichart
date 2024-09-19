Imports IAW.controls

<ProcessRequired(False)> _
Partial Class WebMsgReport
    Inherits stub_IngenWebPage

    Private DeleteSimilar As Boolean
    Private btnDelete As IAWHyperLinkButton
    Private btnDeleteSim As IAWHyperLinkButton

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        InitControls()

        If Not Page.IsPostBack Then
            lstType.DataBind()
            ShowDelete(lstType.SelectedValue <> "01")
        End If
    End Sub

    Private Sub InitControls()

        ODS1.UpdateParameters.Add(New Parameter("id", TypeCode.Int32))

        'cancel the datasource update, we'll handle it in code
        AddHandler ODS1.Updating, AddressOf DSUpdate

        'change the back btn to redirect to MaintainViews.aspx
        Dim bBar As ButtonBar = ctx.buttonBar
        btnDelete = bBar.addLinkButton(AddressOf DeleteButton_Click, "Delete Selected")
        btnDelete.ID = "SaveButton"
        btnDelete.Attributes.Add("onclick", "hidebuttons(this);")

        btnDeleteSim = bBar.addLinkButton(AddressOf DeleteSimButton_Click, "Delete Similar")
        btnDeleteSim.ID = "SimButton"
        btnDeleteSim.Attributes.Add("onclick", "hidebuttons(this);")

        grdViewErrorLog.EmptyDataText = "There are no items to display"

    End Sub

    Private Sub DeleteSimButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DeleteSimilar = True
        grdViewErrorLog.Save()
        grdViewErrorLog.DataBind()
    End Sub

    Private Sub DeleteButton_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        grdViewErrorLog.Save()
        grdViewErrorLog.DataBind()
    End Sub

    Private Sub DSUpdate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.ObjectDataSourceMethodEventArgs)
        e.Cancel = True
    End Sub

    Protected Sub grdViewErrorLog_RowUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewUpdateEventArgs) Handles grdViewErrorLog.RowUpdating
        If Me.DeleteSimilar Then
            ODS1.DeleteMethod = "DeleteItems"
            ODS1.DeleteParameters.Clear()
            ODS1.DeleteParameters.Add("id", TypeCode.Int32, e.Keys.Item("Id"))
            ODS1.Delete()
        Else
            ODS1.DeleteMethod = "DeleteItem"
            ODS1.DeleteParameters.Clear()
            ODS1.DeleteParameters.Add("id", TypeCode.Int32, e.Keys.Item("Id"))
            ODS1.Delete()
        End If
    End Sub

    Protected Function getTypes() As DataTable
        Dim Db As SawDB = New SawDB()
        Dim Dt As Data.DataTable
        Dim strSql As String
        strSql = "SELECT pup_text as description ,return_value from dbo.puptext pup WITH (NOLOCK) where table_id='web_msg_log' and column_id='msg_type'"
        Dt = Db.GetTable(strSql)
        Db.Close()
        Return Dt
    End Function

    Protected Sub lstType_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstType.SelectedIndexChanged
        ODS1.FilterExpression = "type='" + lstType.SelectedValue + "'"
        ShowDelete(lstType.SelectedValue <> "01")
        grdViewErrorLog.DataBind()
    End Sub

    Private Sub ShowDelete(ByVal show As Boolean)
        grdViewErrorLog.Columns.Item(0).Visible = show
        btnDelete.Visible = show
        btnDeleteSim.Visible = show
    End Sub


End Class
