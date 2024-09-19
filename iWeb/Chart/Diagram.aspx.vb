Imports System.IO

<ButtonBarRequired(False), DirtyPageHandling(False), DisableEnterKey(False)>
Public Class Diagram
    Inherits stub_IngenWebPage

    Private SQL As String
    Public ReadOnly Property client_id As Integer
        Get
            Return ctx.session("client_id")
        End Get
    End Property

    Private ReadOnly Property ModelKey As Integer
        Get
            Return CInt(ctx.item("ModelKey"))
        End Get
    End Property

#Region "Backgrounds"

    Public ReadOnly Property BackgroundImages As ListItem()
        Get
            SQL = "select unique_id, content, description, image_repeat
                     From dbo.background
                    where client_id = @p1
                      and background_type ='01'
                    order by description asc"

            Dim dt As DataTable = IawDB.execGetTable(SQL, client_id)
            Dim listItems As New List(Of ListItem)

            For Each row As DataRow In dt.Rows
                Dim listItem As New ListItem()
                listItem.Value = row("unique_id").ToString()
                listItem.Text = row("description").ToString()
                listItem.Attributes("data-imagerepeat") = row("image_repeat").ToString()
                listItem.Attributes("data-content") = row("content").ToString()
                listItems.Add(listItem)
            Next

            Return listItems.ToArray()
        End Get
    End Property

    Public ReadOnly Property Gradients As ListItem()
        Get
            SQL = "SELECT unique_id, content, description, structure
                     FROM dbo.background
                    WHERE client_id = @p1
                      AND background_type ='02'
                    ORDER BY description asc"

            Dim dt As DataTable = IawDB.execGetTable(SQL, client_id)
            Dim listItems As New List(Of ListItem)

            For Each row As DataRow In dt.Rows
                Dim listItem As New ListItem()
                listItem.Value = row("unique_id").ToString()
                listItem.Text = row("description").ToString()
                listItem.Attributes("data-content") = row("content").ToString()
                listItem.Attributes("data-gradient") = row("structure").ToString()
                listItems.Add(listItem)
            Next

            Return listItems.ToArray()
        End Get
    End Property

    Private ReadOnly Property BackgroundTypesDT As DataTable
        Get
            Dim DT As New DataTable
            DT.Columns.Add("ddType")
            DT.Columns.Add("ddText")

            DT.Rows.Add("SolidColour", "::LT_S0257")
            If Gradients.Count > 0 Then
                DT.Rows.Add("Gradient", "::LT_S0150")
            End If
            If BackgroundImages.Count > 0 Then
                DT.Rows.Add("Image", "::LT_S0151")
            End If

            Return DT
        End Get
    End Property

#End Region

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ' store the key on the session so we don't have to pass it around everywhere
        ctx.session("ModelKey") = ModelKey

        ' store the user's role in the session so we have it available in the out-of-bounds asmx methods
        ctx.session("ChartRole") = ctx.role

        'Dim licfilename As String = Server.MapPath("~/Chart/gojs_23.lic")
        Dim licfilename As String = Server.MapPath("~/Chart/gojs_30.lic")

        If File.Exists(licfilename) Then
            Dim v As String = File.ReadAllText(licfilename)
            hdnGoJsKey.Value = SawUtil.decrypt(v)
        Else
            hdnGoJsKey.Value = ""
        End If

        ' get list of images from the icon folder and add to the label dialog icon radio button list..
        ' the number of icons per colour set determines the numebr of radio items across, so we have to set this here.

        rbLabelIcon.RepeatColumns = 10
        rbLabelIcon.Items.Clear()
        For Each FI As FileInfo In New DirectoryInfo(Server.MapPath("./Icons")).GetFiles()
            rbLabelIcon.Items.Add(New ListItem("<img src='Icons/" + FI.Name + "'/>", "Icons/" + FI.Name))
        Next

        ' for the sizing controls, set the max values from the global max values

        Dim DR As DataRow = IawDB.execGetTable("SELECT JSON_VALUE(attributes,'$.max_node_height') as max_node_height,
                                                       JSON_VALUE(attributes,'$.max_node_width') as max_node_width
                                                  FROM dbo.model_header H
                                                 WHERE model_key = @p1", ModelKey).Rows(0)
        Dim MaxWidth As Integer = CInt(DR.GetValue("max_node_width", 500))
        Dim MaxHeight As Integer = CInt(DR.GetValue("max_node_height", 200))

        sliderNodeBoxWidth.Maximum = MaxWidth
        sliderNodeBoxWidth.Steps = ((MaxWidth - 100) / 10) + 1
        sliderModelBoxWidth.Maximum = MaxWidth
        sliderModelBoxWidth.Steps = ((MaxWidth - 100) / 10) + 1

        sliderNodeBoxHeight.Maximum = MaxHeight
        sliderNodeBoxHeight.Steps = ((MaxHeight - 50) / 10) + 1
        sliderModelBoxHeight.Maximum = MaxHeight
        sliderModelBoxHeight.Steps = ((MaxHeight - 50) / 10) + 1

        ' Now set the available fonts..

        ddlbFont.Items.Clear()

        ' Now set the text for the menus

        menu_add_text.InnerText = ctx.Translate("::LT_A0010")                ' Add Note
        menu_modal_setting.InnerText = ctx.Translate("::LT_S0036")           ' Settings
        menu_make_group.InnerText = ctx.Translate("::LT_A0011")              ' Add Group
        menu_make_vacant.InnerText = ctx.Translate("::LT_A0012")             ' Make Vacant
        menu_assistant_on.InnerText = ctx.Translate("::LT_A0013")            ' Assistant
        menu_assistant_off.InnerText = ctx.Translate("::LT_A0013")           ' Assistant
        menu_node_setting.InnerText = ctx.Translate("::LT_S0025")            ' Amend
        menu_make_parent_group.InnerText = ctx.Translate("::LT_A0014")       ' Make Parent Group
        menu_make_new_parent.InnerText = ctx.Translate("::LT_A0015")         ' Make New Parent
        menu_make_child.InnerText = ctx.Translate("::LT_A0016")              ' Make Child
        menu_down_to_parent_group.InnerText = ctx.Translate("::LT_A0017")    ' Move down to Parent Group
        menu_up_to_parent_group.InnerText = ctx.Translate("::LT_C0005")      ' Move up to Parent Group
        menu_move_left.InnerText = ctx.Translate("::LT_A0018")               ' Move Left
        menu_move_right.InnerText = ctx.Translate("::LT_A0019")              ' Move Right
        menu_link_settings.InnerText = ctx.Translate("::LT_S0025")           ' Amend
        menu_expand_below.InnerText = ctx.Translate("::LT_C0003")            ' Expand Below
        menu_collapse_below.InnerText = ctx.Translate("::LT_C0004")          ' Collapse Below
        menu_delete_parent_group.InnerText = ctx.Translate("::LT_C0006")     ' Delete Parent Group
        ' add the background type, gaground images and gradient dropdowns
        SetDDLB(ddlbChartBackgroundType, BackgroundTypesDT, "ddText", "ddType")
        For Each item As ListItem In BackgroundImages
            ddlbChartBackgroundImage.Items.Add(item)
        Next
        For Each item As ListItem In Gradients
            ddlbChartBackgroundGradient.Items.Add(item)
        Next
    End Sub

    Private Sub btnModelBackToList_Click(sender As Object, e As EventArgs) Handles btnModelBackToList.Click
        ctx.redirect("m_chart_list", True)
    End Sub

    'Class Opt
    '    Public Val As String
    '    Public Str As String
    '    Public Tool As String
    '    Public Sub New(V As String, S As String, T As String)
    '        Val = V
    '        Str = S
    '        Tool = T
    '    End Sub
    'End Class

End Class
