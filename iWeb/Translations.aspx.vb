
Imports IAW.controls
Imports System.IO

<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class Translations
    Inherits stub_IngenWebPage

#Region "Common Properties & Variables"
    Dim SQL As String

    Public Property ModalState() As String
        Get
            Dim B As String = ViewState("ModalState")
            If B = Nothing Then
                ViewState("ModalState") = ""
                B = ""
            End If
            Return B
        End Get
        Set(ByVal value As String)
            ViewState("ModalState") = value
        End Set
    End Property
#End Region

#Region "Properties"
    Private ReadOnly Property isTranslator As Boolean
        Get
            Return ctx.role.ToLower = "translator"
            'Return rbTranslator.Checked
        End Get
    End Property

    Private ReadOnly Property isReviewer As Boolean
        Get
            Return ctx.role.ToLower = "reviewer"
            'Return rbReviewer.Checked
        End Get
    End Property

    Private ReadOnly Property isAdmin As Boolean
        Get
            Return ctx.role.ToLower = "admin"
            'Return rbPublisher.Checked
        End Get
    End Property

    Private ReadOnly Property FirstTimeIn As Boolean
        Get
            Return IawDB.execScalar("select first_time from suser where user_ref = @p1", ctx.user_ref) = "1"
        End Get
    End Property

    Private Property WorkingLanguage As String
        Get
            Return ViewState("WorkingLanguage")
        End Get
        Set(value As String)
            ViewState("WorkingLanguage") = value
        End Set
    End Property

    Private ReadOnly Property UserLanguageDT As DataTable
        Get
            Dim DT As DataTable = ViewState("UserLanguage")
            If DT Is Nothing Then
                If isAdmin Then
                    SQL = "select language_ref, language_name
                             from dbo.qlang"
                    DT = IawDB.execGetTable(SQL)
                Else
                    SQL = "select L.language_ref, L.language_name
                             from dbo.user_language UL
                                  join dbo.qlang L
                                    on L.language_ref = UL.language_ref
                            where UL.user_ref = @p1"
                    DT = IawDB.execGetTable(SQL, ctx.user_ref)
                End If
                ViewState("UserLanguage") = DT
            End If
            Return DT
        End Get
    End Property

    Private ReadOnly Property LanguageName(language_ref As String) As String
        Get
            Return UserLanguageDT.Select("language_ref='" + language_ref + "'")(0)("language_name")
        End Get
    End Property

    Private Property qlangID As Integer
        Get
            Return ViewState("qlangID")
        End Get
        Set(value As Integer)
            ViewState("qlangID") = value
        End Set
    End Property

    Private ReadOnly Property LanguageTextDT As DataTable
        Get
            Dim DT As DataTable = ViewState("LanguageText")
            If DT Is Nothing Then
                SQL = "select QT.qlang_id,
                              L.language_name,
                              GBR.text_data as base_text,
                              QT.text_data as language_text,
                              QT.text_status,
                              dbo.dbf_puptext('qlang_text','text_status',QT.text_status,'GBR') as text_status_pt,
                              IsNull(GBR.context_url,'') as context_url
                         from dbo.qlang_text QT
                              join dbo.qlang L
                                on L.language_ref = QT.language_ref
                              join dbo.qlang_text GBR
                                on GBR.text_key = QT.text_key
                        where QT.language_ref = @p1
                          and GBR.language_ref = 'GBR'
                          and GBR.text_status <> '00'   -- 00 means not to be translated
                          and IsNull(Trim(GBR.text_data),'') <> ''
                        order by GBR.text_data"
                DT = IawDB.execGetTable(SQL, WorkingLanguage)
                ViewState("LanguageText") = DT
            End If
            Return DT
        End Get
    End Property
    Private ReadOnly Property LanguageTextDR()
        Get
            Return LanguageTextDT.Select("qlang_id = " + qlangID.ToString)(0)
        End Get
    End Property

    Private ReadOnly Property FilteredLanguageTextDT() As DataTable
        Get
            If WorkingLanguage = "GBR" Then
                cbIncludeUntranslated.Visible = False
                cbIncludeTranslated.Visible = False
                cbIncludeReviewed.Visible = False
                cbIncludeReleased.Visible = False
                grdLang.Columns(1).Visible = False

                Return LanguageTextDT

            Else
                cbIncludeUntranslated.Visible = True
                cbIncludeTranslated.Visible = True
                cbIncludeReviewed.Visible = True
                cbIncludeReleased.Visible = True
                grdLang.Columns(1).Visible = True

                cbIncludeUntranslated.Text = SawUtil.GetMsg("LT_A0134") + String.Format(" ({0})", CountLanguageStatus("01") + CountLanguageStatus("04"))
                cbIncludeTranslated.Text = SawUtil.GetMsg("LT_A0135") + String.Format(" ({0})", CountLanguageStatus("02"))
                cbIncludeReviewed.Text = SawUtil.GetMsg("LT_A0136") + String.Format(" ({0})", CountLanguageStatus("03"))
                cbIncludeReleased.Text = SawUtil.GetMsg("LT_A0138") + String.Format(" ({0})", CountLanguageStatus("05"))
            End If

            Dim DV As New DataView(LanguageTextDT)

            '  text_status - 00 Do not translate
            '                01 Not Translated Yet
            '                02 Translated 
            '                03 Verified OK
            '                04 Verified Error
            '                05 Released
            ' Translator
            '   show Untranslated   ON
            '   show Translated
            '   show Reviewed
            '   show Released
            ' Reviewer
            '   show UnTranslated
            '   show Translated     ON
            '   show Reviewed
            '   show Released
            ' Publisher
            '   show UnTranslated
            '   show Translated     
            '   show Reviewed       ON
            '   show Released

            Dim Include As String
            Dim Statuses As New List(Of String)

            If cbIncludeUntranslated.Checked Then
                Statuses.Add("'01'")
                Statuses.Add("'04'")
            End If
            If cbIncludeTranslated.Checked Then
                Statuses.Add("'02'")
            End If
            If cbIncludeReviewed.Checked Then
                Statuses.Add("'03'")
            End If
            If cbIncludeReleased.Checked Then
                Statuses.Add("'05'")
            End If
            If Statuses.Count > 0 Then
                Include = "in (" + String.Join(",", Statuses.ToArray) + ")"
            Else
                Include = "in ('XX')"
            End If

            DV.RowFilter = "text_status " + Include
            Return DV.ToTable
        End Get
    End Property

    Private ReadOnly Property CountLanguageStatus(LanguageStatus) As Integer
        Get
            Return LanguageTextDT.Select("text_status='" + LanguageStatus + "'").Length
        End Get
    End Property

#End Region
#Region "Page Events"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Page.MaintainScrollPositionOnPostBack = True

        If Not Page.IsPostBack Then
            pnlMain.Visible = False
            If FirstTimeIn Then
                pnlNormal.Visible = False
                pnlFirstTime.Visible = True
            Else
                pnlNormal.Visible = True
                pnlFirstTime.Visible = False
            End If
            PopulateData()

            lblMsgFirstTime.Text = String.Format(SawUtil.GetMsg("IN_IM004"), ctx.username)
            lblMsgAfterFirst.Text = String.Format(SawUtil.GetMsg("IN_IM006"), ctx.username)

            SetRoleDefaults()

            btnHelp.Url = "contexthelp/webframe.html"
            btnHelp.Target = "HelpPage"
            btnHelp.ForeColor = System.Drawing.Color.Black

        End If
    End Sub

    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeForeignTextForm.Hide()

        Select Case ModalState

            Case "ForeignText"
                mpeForeignTextForm.Show()

        End Select
    End Sub

    Private Sub PopulateData()
        RefreshGrid("UserLanguage")

    End Sub

    Private Sub SetRoleDefaults()
        ' Translator
        '   show Untranslated   ON
        ' Reviewer
        '   show Translated     ON
        ' Publisher
        '   show Reviewed       ON

        cbIncludeUntranslated.Checked = False
        cbIncludeTranslated.Checked = False
        cbIncludeReviewed.Checked = False
        cbIncludeReleased.Checked = False

        If WorkingLanguage = "GBR" And isAdmin Then
            cbIncludeUntranslated.Visible = False
            cbIncludeTranslated.Visible = False
            cbIncludeReviewed.Visible = False
            cbIncludeReleased.Visible = False

            cbIncludeUntranslated.Checked = True
        Else
            cbIncludeUntranslated.Visible = True
            cbIncludeTranslated.Visible = True
            cbIncludeReviewed.Visible = True
            cbIncludeReleased.Visible = True

            cbIncludeUntranslated.Checked = isTranslator
            cbIncludeTranslated.Checked = isReviewer
            cbIncludeReviewed.Checked = isAdmin
        End If

        RefreshGrid("LanguageText")
    End Sub

    Private Sub cbInclude_CheckedChanged(sender As Object, e As EventArgs) Handles cbIncludeUntranslated.CheckedChanged,
                                                                                   cbIncludeTranslated.CheckedChanged,
                                                                                   cbIncludeReviewed.CheckedChanged,
                                                                                   cbIncludeReleased.CheckedChanged
        RefreshGrid("LanguageText")
    End Sub

#End Region
#Region "Grids"

    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "UserLanguage"
                ViewState("UserLanguage") = Nothing
                SetGrid(grdUserLanguageFirst, UserLanguageDT, False)
                SetGrid(grdUserLanguageNormal, UserLanguageDT, False)

            Case "LanguageText"
                ViewState("LanguageText") = Nothing
                SetGrid(grdLang, FilteredLanguageTextDT, False)
        End Select

    End Sub

    Private Sub grdUserLanguageFirst_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdUserLanguageFirst.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdUserLanguageFirst, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdUserLanguageNormal_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdUserLanguageNormal.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdUserLanguageNormal, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdUserLanguageFirst_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdUserLanguageFirst.RowCommand
        Dim RowIndex As Integer = Int32.Parse(e.CommandArgument.ToString())
        WorkingLanguage = grdUserLanguageFirst.DataKeys(RowIndex).Value

        Select Case e.CommandName

            Case "Select"
                IawDB.execNonQuery("update dbo.suser set first_time = '0' where user_ref = @p1", ctx.user_ref)
                pnlNormal.Visible = False
                pnlFirstTime.Visible = False
                pnlMain.Visible = True
                pnlMain.GroupingText = LanguageName(WorkingLanguage)

                SetRoleDefaults()

                RefreshGrid("LanguageText")

            Case "DeleteRow"
                IawDB.execNonQuery("DELETE dbo.user_language where user_ref = @p1 and language_ref = @p2", ctx.user_ref, WorkingLanguage)
                RefreshGrid("UserLanguage")

        End Select
    End Sub
    Private Sub grdUserLanguageNormal_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdUserLanguageNormal.RowCommand

        Dim RowIndex As Integer = Int32.Parse(e.CommandArgument.ToString())
        WorkingLanguage = grdUserLanguageNormal.DataKeys(RowIndex).Value

        Select Case e.CommandName
            Case "Select"
                pnlNormal.Visible = False
                pnlFirstTime.Visible = False
                pnlMain.Visible = True
                pnlMain.GroupingText = LanguageName(WorkingLanguage)

                SetRoleDefaults()
                RefreshGrid("LanguageText")
        End Select

    End Sub

    Private Sub grdLang_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdLang.RowDataBound
        Dim BLT, FLT As Label
        'Dim lblLinkCtrl As IAWLabel
        Dim linkContext As IAWHyperLink
        Dim ShowEdit As Boolean
        Dim rv As DataRowView

        'If e.Row.RowType = DataControlRowType.Header Then
        '    If (isReviewer And cbIncludeTranslated.Checked And CountLanguageStatus("02") > 0) Or
        '       (isAdmin And cbIncludeReviewed.Checked And CountLanguageStatus("03") > 0) Then
        '        ShowControl(e.Row, "btnAcceptAll")
        '    Else
        '        HideControl(e.Row, "btnAcceptAll")
        '    End If
        'End If

        If e.Row.RowType = DataControlRowType.DataRow Then
            rv = CType(e.Row.DataItem, DataRowView)

            BLT = CType(e.Row.FindControl("lblGridBaseText"), Label)
            FLT = CType(e.Row.FindControl("lblGridForeignText"), Label)

            BLT.Text = Server.HtmlEncode(BLT.Text)
            FLT.Text = Server.HtmlEncode(FLT.Text)

            HideControl(e.Row, "btnThumbsUp")
            HideControl(e.Row, "btnThumbsDown")

            Select Case rv("text_status")
                Case "01"
                    If rv("base_text") <> rv("language_text") Then
                        FLT.ForeColor = System.Drawing.Color.Purple
                    Else
                        FLT.ForeColor = System.Drawing.Color.Blue
                    End If
                    ShowEdit = isTranslator
                Case "02"
                    ShowEdit = isReviewer
                    If isReviewer Then
                        ShowControl(e.Row, "btnThumbsUp")
                        ShowControl(e.Row, "btnThumbsDown")
                    End If
                Case "03"
                    ShowEdit = isAdmin
                    If isAdmin Then
                        ShowControl(e.Row, "btnThumbsUp")
                        ShowControl(e.Row, "btnThumbsDown")
                    End If
                Case "04"
                    FLT.ForeColor = System.Drawing.Color.DarkRed
                    ShowEdit = isTranslator
                Case "05"
                    FLT.ForeColor = System.Drawing.Color.Green
                    ShowEdit = False
            End Select

            ' if admin looking at GBR then let them edit the context help
            If WorkingLanguage = "GBR" And isAdmin Then
                HideControl(e.Row, "lblForeignLabel")
                ShowControl(e.Row, "lblConextLabel")
                HideControl(e.Row, "lblGridForeignText")
                ShowControl(e.Row, "lblGridContextHelp")
            Else
                ShowControl(e.Row, "lblForeignLabel")
                HideControl(e.Row, "lblConextLabel")
                ShowControl(e.Row, "lblGridForeignText")
                HideControl(e.Row, "lblGridContextHelp")

                If Not ShowEdit Then
                    HideControl(e.Row, "btnAmend")
                End If
            End If

            linkContext = e.Row.FindControl("linkContext")
            If linkContext IsNot Nothing Then
                Dim linkurl As String = rv("context_url").ToString.Trim
                Dim haveFile As Integer

                Try
                    If Not String.IsNullOrEmpty(linkurl) AndAlso File.Exists(Server.MapPath("contexthelp/" + linkurl)) Then
                        haveFile = 1
                    Else
                        haveFile = 0
                    End If

                Catch ex As Exception
                    haveFile = 2
                End Try

                If WorkingLanguage <> "GBR" Then
                    If haveFile = 0 Or haveFile = 2 Then
                        haveFile = 3
                    End If
                End If

                linkContext.Url = ""
                Select Case haveFile
                    Case 0
                        linkContext.ForeColor = Drawing.Color.Red
                        linkContext.ToolTip = "Context help has not been provided yet"

                    Case 1
                        linkurl = "contexthelp/" + linkurl

                        linkContext.Url = linkurl
                        linkContext.Target = "HelpPage"
                        linkContext.ForeColor = Drawing.Color.Black
                        linkContext.ToolTip = "Show context"

                    Case 2
                        linkContext.ForeColor = Drawing.Color.Purple
                        linkContext.ToolTip = "Context help filename contains invalid characters"

                    Case 3
                        linkContext.Visible = False

                End Select
            End If

        End If

    End Sub

    Private Sub grdLang_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdLang.RowCommand
        Dim RowIndex As Integer = Int32.Parse(e.CommandArgument.ToString())

        qlangID = grdLang.DataKeys(RowIndex).Value

        Dim DR As DataRow = LanguageTextDR()

        Select Case e.CommandName

            Case "Accept"
                txtForeignText.Text = DR("language_text")
                DialogSaved(True)

            Case "Reject"
                txtForeignText.Text = DR("language_text")
                DialogSaved(False)

            Case "AmendRow"

                lblEnglishText.Text = DR("base_text")
                lblForeignText.Text = DR("language_name")

                If DR("base_text") = DR("language_text") Then
                    txtForeignText.Text = ""
                    txtForeignText.Attributes.Add("placeholder", DR("language_text"))
                Else
                    txtForeignText.Text = DR("language_text")
                    txtForeignText.Attributes.Remove("placeholder")
                End If

                Dim linkurl As String = DR("context_url").ToString.Trim
                If linkurl <> "" Then

                    Dim haveFile As Integer

                    Try
                        If Not String.IsNullOrEmpty(linkurl) AndAlso File.Exists(Server.MapPath("contexthelp/" + linkurl)) Then
                            haveFile = 1
                        Else
                            haveFile = 0
                        End If
                    Catch ex As Exception
                        haveFile = 2
                    End Try

                    If WorkingLanguage <> "GBR" Then
                        If haveFile = 0 Or haveFile = 2 Then
                            haveFile = 3
                        End If
                    End If

                    Select Case haveFile
                        Case 0
                            lblLinkContextForm.ForeColor = System.Drawing.Color.Red
                            lblLinkContextForm.ToolTip = "Context help has not been provided yet"

                        Case 1
                            linkurl = "contexthelp/" + linkurl
                            Dim openurl As String = "window.open('" + linkurl + "', 'Context', 'width=600,height=400,toolbar=no,location=no,status=no,menubar=no')"

                            lblLinkContextForm.Attributes.Add("onclick", openurl)
                            lblLinkContextForm.ForeColor = System.Drawing.Color.Black
                            lblLinkContextForm.ToolTip = "Show context"

                        Case 2
                            lblLinkContextForm.ForeColor = System.Drawing.Color.Purple
                            lblLinkContextForm.ToolTip = "Context help filename contains invalid characters"

                        Case 3
                            lblLinkContextForm.Visible = False

                    End Select

                Else
                    lblLinkContextForm.Visible = False
                End If

                If isTranslator Then
                    btnForeignTextSubmit.Visible = True
                    btnForeignTextSave.Text = "Save for Later"
                    btnForeignTextSubmit.Text = "Submit Translation"
                End If

                If isReviewer Then
                    btnForeignTextSubmit.Visible = True
                    btnForeignTextSave.Text = "Reject Translation"
                    btnForeignTextSubmit.Text = "Accept Translation"
                End If

                If isAdmin Then
                    btnForeignTextSubmit.Visible = False
                    If WorkingLanguage = "GBR" Then
                        lblForeignText.Text = "Context Help"
                        txtForeignText.Text = DR("context_url")
                        btnForeignTextSave.Text = "Save"
                    Else
                        btnForeignTextSave.Text = "Publish Translation"
                    End If
                End If

                SetModalState("ForeignText")
        End Select

    End Sub

#End Region

#Region "Dialogs"

    Private Sub btnForeignTextCancel_Click(sender As Object, e As EventArgs) Handles btnForeignTextCancel.Click
        SetModalState("")
    End Sub
    Private Sub btnForeignTextSave_Click(sender As Object, e As EventArgs) Handles btnForeignTextSave.Click
        DialogSaved(False)
    End Sub
    Private Sub btnForeignTextSubmit_Click(sender As Object, e As EventArgs) Handles btnForeignTextSubmit.Click
        DialogSaved(True)
    End Sub

    Private Sub DialogSaved(submit As Boolean)
        Dim DR As DataRow = LanguageTextDR()
        Dim newStatus As String = DR("text_status")

        If isTranslator Then
            ' if translator mode, and text is the same then don't do anything
            'If DR("language_text") = txtForeignText.Text Then
            '    SetModalState("")
            '    Return
            'End If

            If newStatus.ContainsOneOf("01", "04") And submit Then newStatus = "02"
        End If

        If isReviewer Then
            If newStatus = "02" And submit Then newStatus = "03"        ' Accept translation
            If newStatus = "02" And Not submit Then newStatus = "04"    ' Reject translation
        End If

        If isAdmin Then
            If newStatus = "03" And submit Then newStatus = "05"        ' Accept translation
            If newStatus = "03" And Not submit Then newStatus = "02"    ' Reject translation
        End If

        If isAdmin And WorkingLanguage = "GBR" Then
            SQL = "Update dbo.qlang_text 
                      set context_url = @p2
                    where qlang_id = @p1"
            IawDB.execNonQuery(SQL, qlangID, txtForeignText.Text)
        Else
            SQL = "Update dbo.qlang_text 
                      set text_data = @p2,
                          text_status = @p3
                    where qlang_id = @p1"
            IawDB.execNonQuery(SQL, qlangID, txtForeignText.Text, newStatus)
        End If

        SetModalState("")
        RefreshGrid("LanguageText")
    End Sub

#End Region

#Region "Misc"
    Private Sub ShowControl(row As GridViewRow, ctrlID As String)
        Dim ctrl As Control = row.FindControl(ctrlID)
        If ctrl IsNot Nothing Then
            ctrl.Visible = True
        End If
    End Sub
    Private Sub HideControl(row As GridViewRow, ctrlID As String)
        Dim ctrl As Control = row.FindControl(ctrlID)
        If ctrl IsNot Nothing Then
            ctrl.Visible = False
        End If
    End Sub

#End Region

End Class


