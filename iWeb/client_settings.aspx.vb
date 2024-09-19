Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports Newtonsoft.Json


<ButtonBarRequired(False), DirtyPageHandling(False)>
Public Class client_settings
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Private SQL As String

    Public ReadOnly Property client_id As Integer
        Get
            Return ctx.session("client_id")
        End Get
    End Property

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

    Public ReadOnly Property argBrandID As Integer
        Get
            If ctx.item("brandid") Is Nothing Then
                Return -1
            Else
                Return CInt(ctx.item("brandid"))
            End If
        End Get
    End Property

#End Region
#Region "Clients Data"

    Private ReadOnly Property ClientslDR As DataRow
        Get
            Dim DT As DataTable = ViewState("ModelGlobalDT")
            If DT Is Nothing Then
                SQL = "SELECT IsNull(attributes,'') as attributes
                         FROM dbo.clients
                        WHERE client_id = @p1"
                DT = IawDB.execGetTable(SQL, client_id)
                ViewState("ModelGlobalDT") = DT
            End If
            Return DT.Rows(0)
        End Get
    End Property

    Private ReadOnly Property AttributesObj As ChartAttrib
        Get
            Return New ChartAttrib(ClientslDR("Attributes"))
        End Get
    End Property

#End Region
#Region "Fonts Data"
    Private ReadOnly Property FontsDT() As DataTable
        Get
            Dim DT As DataTable = ViewState("Fonts")
            If DT Is Nothing Then
                SQL = "SELECT F.font_name,F.font_string
                         FROM dbo.qlang L
                              JOIN dbo.font F
                                ON F.font_family = L.font_family
                        WHERE L.language_ref = @p1
                        UNION
                        SELECT C.font_name,C.font_string
                          FROM dbo.qlang L
                               JOIN dbo.client_font C
                                 ON C.font_family = L.font_family
                         WHERE L.language_ref = @p1
                           AND C.client_id = @p2"
                DT = IawDB.execGetTable(SQL, ctx.languageCode, client_id)
                ViewState("Fonts") = DT
            End If
            Return DT
        End Get
    End Property
#End Region
#Region "Brands Data"
    Private Sub EnsureBrandRecord()
        If IawDB.execScalar("SELECT count(1) FROM dbo.client_brand WHERE client_id = @p1 and default_brand = '1'", client_id) = 0 Then
            SQL = "INSERT dbo.client_brand (client_id, brand_name, branding, default_brand)
                   VALUES (@p1,@p2,@p3,'1')"
            IawDB.execNonQuery(SQL, client_id,
                                       "ViviChart",
                                       (New ChartBranding).GetData)
            ViewState("Brands") = Nothing
        End If
    End Sub

    Private Property BrandID As Integer
        Get
            Dim B As Integer = -1
            If ViewState("BrandID") Is Nothing Then
                If argBrandID <> -1 Then
                    B = argBrandID
                Else
                    EnsureBrandRecord()

                    SQL = "SELECT brand_id 
                             FROM dbo.client_brand
                            WHERE client_id = @p1
                              AND default_brand = '1'"
                    B = IawDB.execScalar(SQL, client_id)
                End If
                ViewState("BrandID") = B
            Else
                B = ViewState("BrandID")
            End If

            Return B
        End Get
        Set(value As Integer)
            ViewState("BrandID") = value
        End Set
    End Property

    Private ReadOnly Property CleanBrandsDT As DataTable
        Get
            ViewState("Brands") = Nothing
            Return BrandsDT
        End Get
    End Property

    Public ReadOnly Property BrandsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Brands")
            If DT Is Nothing Then
                ' ensure one exists
                EnsureBrandRecord()

                SQL = "SELECT brand_id,
                              brand_unique,
                              brand_name,
                              branding,
                              default_brand
                         FROM dbo.client_brand
                        WHERE client_id = @p1
                        ORDER BY brand_name"
                DT = IawDB.execGetTable(SQL, client_id)
                ViewState("Brands") = DT
                ViewState("BrandID") = Nothing
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property BrandsDR() As DataRow
        Get
            Return BrandsDT.Select("brand_id = " + BrandID.ToString)(0)
        End Get
    End Property

    Private ReadOnly Property BrandingObj As ChartBranding
        Get
            Dim o As ChartBranding = ViewState("BrandingObj")
            If o Is Nothing Then
                If BrandsDT.Rows.Count = 0 Then
                    o = New ChartBranding()
                Else
                    o = New ChartBranding(BrandsDR("branding").ToString)
                End If
                ViewState("BrandingObj") = o
            End If
            Return o
        End Get
    End Property

#End Region
#Region "Backgrounds Data"

    Public ReadOnly Property BackgroundsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Backgrounds")
            If DT Is Nothing Then
                SQL = "SELECT unique_id, content, description, image_repeat
                         FROM dbo.background
                        WHERE client_id IN (0,@p1)
                          AND background_type = '01'
                        ORDER BY client_id desc, description asc"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Backgrounds") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property BackgroundsDR(unique_id As Integer) As DataRow
        Get
            Return BackgroundsDT.Select("unique_id = " + unique_id.ToString)(0)
        End Get
    End Property

    Public ReadOnly Property GradientsDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Gradients")
            If DT Is Nothing Then
                SQL = "SELECT unique_id, content, description, structure
                         FROM dbo.background
                        WHERE client_id IN (0,@p1)
                          AND background_type = '02'
                        ORDER BY client_id desc, description asc"
                DT = IawDB.execGetTable(SQL, client_id)

                ViewState("Gradients") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property GradientsDR(unique_id As Integer) As DataRow
        Get
            Return GradientsDT.Select("unique_id = " + unique_id.ToString)(0)
        End Get
    End Property

    Private ReadOnly Property BackgroundTypesDT As DataTable
        Get
            Dim DT As New DataTable
            DT.Columns.Add("ddType")
            DT.Columns.Add("ddText")

            DT.Rows.Add("SolidColour", SawLang.Translate("::LT_S0257"))     ' Single Colour
            If GradientsDT.Rows.Count > 0 Then
                DT.Rows.Add("Gradient", SawLang.Translate("::LT_S0150"))    ' Gradient
            End If
            If BackgroundsDT.Rows.Count > 0 Then
                DT.Rows.Add("Image", SawLang.Translate("::LT_S0151"))       ' Image
            End If

            Return DT
        End Get
    End Property

    ' the following routines are maintaining the background_table
    Public ReadOnly Property CleanBgDT As DataTable
        Get
            ViewState("BgData") = Nothing
            Return BgDT
        End Get
    End Property

    Public ReadOnly Property BgDT As DataTable
        Get
            Dim DT As DataTable = ViewState("BgData")
            If DT Is Nothing Then
                SQL = "SELECT unique_id,
                              client_id,
                              background_type,
                              dbo.dbf_puptext('background', 'background_type', background_type, @p2) AS background_type_pt,
                              content,
                              description,
                              image_repeat,
                              structure,
                              image_name
                         FROM dbo.background 
                        WHERE client_id = @p1
                        ORDER BY description"
                DT = IawDB.execGetTable(SQL, client_id, ctx.languageCode)

                Dim column As DataColumn = New DataColumn()
                column.DataType = Type.GetType("System.String")
                column.ColumnName = "preview"
                column.MaxLength = -1
                DT.Columns.Add(column)

                For Each DR As DataRow In DT.Rows
                    If DR.GetValue("background_type") = "01" Then
                        Dim imgPath As String = Path.Combine(Context.Server.MapPath("~/Backgrounds/"), DR.GetValue("content"))
                        If File.Exists(imgPath) Then
                            Dim img As Image = Image.FromFile(imgPath)
                            Dim ratio As Double = Math.Min(img.Width, 30 / img.Height)
                            Dim newWidth As Integer = CInt(img.Width * ratio)
                            Dim newHeight As Integer = CInt(img.Height * ratio)

                            Dim resizedImg As Image = New Bitmap(newWidth, newHeight)
                            Using g As Graphics = Graphics.FromImage(resizedImg)
                                g.DrawImage(img, 0, 0, newWidth, newHeight)
                            End Using

                            Dim ms As New MemoryStream()
                            resizedImg.Save(ms, ImageFormat.Png)
                            Dim base64 As String = Convert.ToBase64String(ms.ToArray())
                            DR("preview") = base64
                        Else
                            DR("preview") = ""
                        End If
                    End If
                Next
                DT.AcceptChanges()
                ViewState("BgData") = DT
            End If
            Return DT
        End Get
    End Property
    ReadOnly Property BgDR(unique_id As Integer) As DataRow
        Get
            Return BgDT.Select("unique_id = " + unique_id.ToString)(0)
        End Get
    End Property

    ReadOnly Property isBackgroundInUse(backgroundID As Integer) As Boolean
        Get
            SQL = "SELECT count(1)
                     FROM (
                          SELECT attributes
                            FROM dbo.clients
                           WHERE client_id = @p1
                             AND TRY_CAST(JSON_VALUE(attributes, '$.backgroundID') AS INT) = @p2
                          UNION ALL
                          SELECT V.attributes
                            FROM dbo.data_view V 
                                 JOIN dbo.data_source S
                                   ON S.source_id = V.source_id
                           WHERE S.client_id = @p1
                             AND TRY_CAST(JSON_VALUE(V.attributes, '$.backgroundID') AS INT) = @p2
                          UNION ALL
                          SELECT H.attributes
                            FROM dbo.model_header H
                                 JOIN dbo.data_view V
                                   ON V.view_id = H.view_id
                                 JOIN dbo.data_source S
                                   ON S.source_id = V.source_id
                           WHERE S.client_id = @p1
                             AND TRY_CAST(JSON_VALUE(H.attributes, '$.backgroundID') AS INT) = @p2
                          ) AS X"
            Return IawDB.execScalar(SQL, client_id, backgroundID) > 0
        End Get
    End Property

#End Region
#Region "Theme Data"
    Private ReadOnly Property CleanThemeDT As DataTable
        Get
            ViewState("Theme") = Nothing
            Return ThemeDT
        End Get
    End Property
    Public ReadOnly Property ThemeDT As DataTable
        Get
            Dim DT As DataTable = ViewState("Theme")
            If DT Is Nothing Then
                SQL = "SELECT theme_id,
                              theme_name,
                              theme,
                              swatch
                         FROM dbo.theme
                        ORDER BY seq"
                DT = IawDB.execGetTable(SQL)

                ViewState("Theme") = DT
            End If
            Return DT
        End Get
    End Property
    Private ReadOnly Property ThemeDR(ID As Integer) As DataRow
        Get
            Return ThemeDT.Select("theme_id = " + ID.ToString)(0)
        End Get
    End Property

#End Region

#Region "Page Events"
    Private Sub client_settings_Init(sender As Object, e As EventArgs) Handles Me.Init
        useSqlViewState = True
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            If argBrandID = -1 Then
                If BrandID <> -1 Then
                    PreviewBrand()
                End If
            Else
                BrandID = argBrandID
            End If

            Populate()
            chooseFile.CssClass += " tabBodyLink"
            btnThemeSelect.CssClass += " tabBodyLink"
        End If
        'AddAttributeDefaultNames()
        grdBrand.CssClass = "allow-hover"
        grdBackgrounds.CssClass = "allow-hover"
        SetModalState()
        ControlAvailability()

        lblMenuIcon.Text = ctx.Translate("::LT_S0085") + " &#9776;" ' Menu Button &#9776;
    End Sub

    Private Sub PreviewBrand()
        Dim master As IngenWebMaster = CType(Me.Master, IngenWebMaster)
        master.enableForceBrand = True
        Session("forceBrand") = BrandID

        Dim url = SawUtil.encryptQuery("client_settings.aspx?BrandID=" + BrandID.ToString, True)
        ctx.redirect(url)
    End Sub

    Private Sub Populate()
        SetDDLB(ddlbFont, FontsDT, "font_name", "font_name")
        SetDDLB(ddlbBackgroundType, BackgroundTypesDT, "ddText", "ddType")
        SetDDLB(ddlbBodyBackgroundType, BackgroundTypesDT, "ddText", "ddType")
        SetDDLB(ddlbBackgroundImage, BackgroundsDT, "description", "unique_id")
        SetDDLB(ddlbBodyBackgroundImage, BackgroundsDT, "description", "unique_id")
        SetDDLB(ddlbBackgroundGradient, GradientsDT, "description", "unique_id")
        SetDDLB(ddlbBodyBackgroundGradient, GradientsDT, "description", "unique_id")
        SetDDLBPuptext(ddlbImageRepeatType, "background", "image_repeat")

        RefreshGrid("Brand")
        RefreshGrid("Background")

        SetModelDefaults()
        SetBrandingDefaults(BrandingObj)
        SetSliderRanges()

        hdnLogo.Value = ""
        hdnLogofilename.Value = ""
        hdnChangesPending.Value = ""
    End Sub

    Private Sub SetSliderRanges()
        slideNodeWidth.Maximum = CInt(numMaxNodeWidth.Text)
        slideNodeWidth.Steps = ((slideNodeWidth.Maximum - slideNodeWidth.Minimum) / 10) + 1

        slideNodeHeight.Maximum = CInt(numMaxNodeHeight.Text)
        slideNodeHeight.Steps = ((slideNodeHeight.Maximum - slideNodeHeight.Minimum) / 10) + 1
    End Sub

    Private Sub SetModalState(Optional ByVal Setting As String = "nothing")
        If Setting <> "nothing" Then ModalState = Setting

        mpeBrandForm.Hide()
        mpeThemeForm.Hide()
        mpeBackgroundForm.Hide()
        mpeDelete.Hide()
        mpeSimplifiedWarning.Hide()

        Select Case ModalState
            Case "Brand"
                mpeBrandForm.Show()
            Case "Theme"
                mpeThemeForm.Show()
            Case "Background"
                mpeBackgroundForm.Show()
            Case "DeleteBackground"
                mpeDelete.Show()
            Case "SimplifiedWarning"
                mpeSimplifiedWarning.Show()
        End Select
    End Sub
    Private Sub RefreshGrid(ByVal GridName As String)

        Select Case GridName
            Case "Brand"
                SetGrid(grdBrand, CleanBrandsDT, True)

                For Each row As GridViewRow In grdBrand.Rows
                    If grdBrand.DataKeys(row.RowIndex).Value.ToString() = BrandID.ToString Then
                        grdBrand.SelectedIndex = row.RowIndex
                        Exit For
                    End If
                Next
            Case "Background"
                SetGrid(grdBackgrounds, CleanBgDT, False)

        End Select
    End Sub

    Private Sub ControlAvailability()
        btnGlobalSave.Style("visibility") = "visible"
        btnGlobalCancel.Style("visibility") = "visible"

        tpBranding.Enabled = True
        tpBackgrounds.Enabled = True
        tpChart.Enabled = True
        pnlGrdBrand.CssClass = "childTabScroll"

        Dim menu As Menu = DirectCast(Page.Master.FindControl("Menu1"), Menu)
        Dim logout As HyperLink = DirectCast(Page.Master.FindControl("lnkLogout"), HyperLink)
        Select Case hdnChangesPending.Value.ToLower
            Case "branding"
                tpBackgrounds.Enabled = False
                tpChart.Enabled = False
                pnlGrdBrand.CssClass = "childTabScroll noClick"
                ctx.BannerEnabled(False)

            Case "chart"
                tpBranding.Enabled = False
                tpBackgrounds.Enabled = False
                ctx.BannerEnabled(False)

            Case Else
                btnGlobalSave.Style("visibility") = "hidden"
                btnGlobalCancel.Style("visibility") = "hidden"
                ctx.BannerEnabled(True)
        End Select
    End Sub
#End Region

#Region "Assign values to screen fields"
    Private Sub SetModelDefaults()

        Dim CA As ChartAttrib = AttributesObj

        hdnAttrib.Value = CA.GetData

        If CA.Data.backgroundType = "none" Then CA.Data.backgroundType = "SolidColour"
        If CA.Data.backgroundType <> "" Then
            ddlbBackgroundType.SelectedValue = CA.Data.backgroundType
        Else
            CA.Data.backgroundType = "SolidColour"
            ddlbBackgroundType.SelectedValue = "SolidColour"
        End If

        If CA.Data.chartDirection = Nothing Then CA.Data.chartDirection = 90

        With CA.Data
            ddlbDirection.SelectedValue = .chartDirection

            cbSettigsFixed.Checked = CA.Data.canOverride ' General Settings

            numMaxNodeHeight.Text = .maxHeight.ToString
            numMaxNodeWidth.Text = .maxWidth.ToString
            numNodeHeight.Text = .nodeHeight.ToString
            numNodeWidth.Text = .nodeWidth.ToString

            If String.IsNullOrEmpty(.backgroundType) Then
                .backgroundType = "SolidColour"
                .backgroundContent = "#FFFFFF"
            End If
            If Not .backgroundType.ContainsOneOf("SolidColour", "Gradient", "Image") Then
                .backgroundType = "SolidColour"
                .backgroundContent = "#FFFFFF"
            End If
            ddlbBackgroundType.SelectedValue = .backgroundType

            txtModelbg.Text = .backgroundContent
            Select Case .backgroundType
                Case "Image"
                    ddlbBackgroundImage.SelectedValue = .backgroundID
                Case "Gradient"
                    ddlbBackgroundGradient.SelectedValue = .backgroundID
            End Select

            ddlbDirection.SelectedValue = If(.chartDirection = Nothing, 90, .chartDirection)

            With .node
                txtNodefg.Text = .foreground
                txtNodeTxtBg.Text = .textBackground
                txtNodebg.Text = .background
                txtNodeBorder.Text = .border
                cbNodeTxtBlock.Checked = .textBackgroundBlock
                txtIconfg.Text = .iconColour
                txtIconHover.Text = .iconHover
            End With

            With .highlight
                txtHighlightfg.Text = .foreground
                txtHighlightbg.Text = .background
                txtHighlightbBorder.Text = .border
            End With

            With .tooltip
                txtTTfg.Text = .foreground
                txtTTbg.Text = .background
                txtTTBorder.Text = .border
            End With

            rbCornerRectangle.Checked = (.corners = "Rectangle")
            rbCornerRoundedRectangle.Checked = (.corners = "RoundedRectangle")
            cbShowShadow.Checked = .showShadow
            txtShadow.Text = .shadowColour

            With .link
                txtlinkColour.Text = .colour
                txtLinkHover.Text = .hover
                numLinkWidth.Text = .width.ToString
                ddlbLinkStyle.SelectedValue = .type
                With .tooltip
                    txtLinkTooltipfg.Text = .foreground
                    txtLinkTooltipbg.Text = .background
                    txtLinkTooltipBorder.Text = .border
                End With
            End With

            'cbUseImages.Checked = .imagesApplicable

            cbSettigsFixed.Checked = .canOverride
        End With

    End Sub
    Private Sub SetBrandingDefaults(BrandObj As ChartBranding)
        Dim CB As ChartBranding = BrandObj

        If CB.Data.bodyBGType = "none" Then CB.Data.bodyBGType = "SolidColour"
        If CB.Data.bodyBGType <> "" Then
            ddlbBodyBackgroundType.SelectedValue = CB.Data.bodyBGType
        Else
            CB.Data.bodyBGType = "SolidColour"
            ddlbBodyBackgroundType.SelectedValue = "SolidColour"
        End If

        cbAdvanced.Checked = CB.Data.advancedBranding
        If CB.Data.advancedBranding Then
            With CB.Data
                bodyBgColour.Text = .bodyBG
                Select Case .bodyBGType
                    Case "Image"
                        ddlbBodyBackgroundImage.SelectedValue = .bodyBGID
                    Case "Gradient"
                        ddlbBodyBackgroundGradient.SelectedValue = .bodyBGID
                End Select
                With .bannerBar
                    bannerTextColour.Text = .foreground
                    bannerBgColour.Text = .background
                End With
                bannerIconsHoverColour.Text = .bannerIconsHover
                With .legend
                    legendTextColour.Text = .foreground
                    legendBgColour.Text = .background
                End With

                legendBorderColour.Text = .legendborder

                contentAreaBgColour.Text = .contentAreaBG

                With .link
                    linkTextColour.Text = .normal.foreground
                    linkBgColour.Text = .normal.background

                    linkTextHoverColour.Text = .hover.foreground
                    linkBgHoverColour.Text = .hover.background
                End With

                textColour.Text = .textColour

                With .listheader
                    listHeaderTextColour.Text = .foreground
                    listHeaderBgColour.Text = .background
                End With

                With .listBody
                    listRowColour.Text = .foreground
                    listRowBgColour.Text = .background
                End With

                With .listAltBody
                    listAltRowColour.Text = .foreground
                    listAltRowBgColour.Text = .background
                End With

                With .inputField
                    inptFieldTextColour.Text = .foreground
                    inptFieldBgColour.Text = .background
                End With

                inptFieldBorderColour.Text = .inputFieldBorder
                inptFieldFocusBgColour.Text = .inputFieldFocusBG

                With .tab
                    tabsTextColour.Text = .normal.foreground
                    tabsBgColour.Text = .normal.background

                    tabsTextHoverColour.Text = .hover.foreground
                    tabsBgHoverColour.Text = .hover.background
                End With

                With .selectedTab
                    selectedTabsTextColour.Text = .foreground
                    selectedTabsBgColour.Text = .background
                End With

                With .selectedRow
                    selectedRowTextColour.Text = .normal.foreground
                    selectedRowBgColour.Text = .normal.background

                    selectedRowTextHoverColour.Text = .hover.foreground
                    selectedRowBgHoverColour.Text = .hover.background
                End With

                imageCharactersColour.Text = .imageCharactersColour
                imageCharactersHighlightColour.Text = .imageCharactershighlightColour
                menuButtonColour.Text = .menuButtonColour
                menuIconColour.Text = .menuIconColour

                With .menuItem
                    menuItemTextColour.Text = .foreground
                    menuItemBgColour.Text = .background
                End With

                With .draggable
                    draggablesTextColour.Text = .normal.foreground
                    draggablesBGColour.Text = .normal.background

                    draggablesTextHoverColour.Text = .hover.foreground
                    draggablesBGHoverColour.Text = .hover.background
                End With

                draggablesBorderColour.Text = .draggableBorder
                draggablesBorderHoverColour.Text = .draggableHoverBorder

                listHeaderIconsColour.Text = .listHeaderIcons
                listHeaderIconsHoverColour.Text = .listHeaderIconsHover
                listBodyIconsColour.Text = .listIcons
                listBodyIconsHoverColour.Text = .listIconsHover
                checkboxColour.Text = .CheckBox
            End With
        Else
            With CB.Data
                simpBannerBgColour.Text = .bannerBar.background
                simpPrincipal.Text = .bodyBG
                simpLightPrincipal.Text = .link.normal.background
                simpContentArea.Text = .contentAreaBG
                simpDarkContextArea.Text = .inputFieldFocusBG
                simpText.Text = .textColour
                simpLightText.Text = .listIcons
                simpExceptions.Text = .imageCharactershighlightColour
            End With
        End If

        logoNormalColour.Text = CB.Data.logoNormalColour
        logoLighterColour.Text = CB.Data.logoLighterColour
        logoDarkerColour.Text = CB.Data.logoDarkerColour

        logoPreview.ImageUrl = ctx.virtualDir + "/logos/" + CB.Data.bannerLogo
        Dim logoName = IawDB.execScalar("SELECT logo_name FROM dbo.client_brand WHERE brand_id = @p1", BrandID)
        If logoName IsNot DBNull.Value Then
            labFileName.Text = logoName
            labFileName.ToolTip = logoName
        End If

        If String.IsNullOrEmpty(CB.Data.bannerLogo) Then
            trLogoPreview.Attributes("class") = "hidden"
            trLogoColours.Attributes("class") = "hidden"
            logoPreview.Visible = False
        End If

        SetBrandingMode()
    End Sub

    Private Sub SetBrandingMode()
        ' Show / Hide the controls based on the advanced branding option
        Dim Adv As Boolean = cbAdvanced.Checked

        cbAdvanced.Checked = Adv
        tpContents.Visible = Adv
        tpLists.Visible = Adv
        pAdvanced.Visible = Adv
        pSimplified.Visible = Not Adv
    End Sub

    Private Sub cbAdvanced_CheckedChanged(sender As Object, e As EventArgs) Handles cbAdvanced.CheckedChanged
        If cbAdvanced.Checked Then
            hdnChangesPending.Value = "branding"
            BrandingObj.Data.advancedBranding = True
            SetBrandingDefaults(BrandingObj)
            ControlAvailability()
            SetBrandingMode()
        Else
            SetModalState("SimplifiedWarning")
        End If
    End Sub

    Private Sub btnSimplifiedWarningOk_Click(sender As Object, e As EventArgs) Handles btnSimplifiedWarningOk.Click
        hdnChangesPending.Value = "branding"
        BrandingObj.Data.advancedBranding = False
        If BrandingObj.Data.bodyBGType <> "SolidColour" Then
            Dim CB As ChartBranding = New ChartBranding
            BrandingObj.Data.bodyBGType = CB.Data.bodyBGType
            BrandingObj.Data.bodyBGID = CB.Data.bodyBGID
            BrandingObj.Data.bodyBG = CB.Data.bodyBG
        End If
        SetBrandingDefaults(BrandingObj)
        ControlAvailability()
        SetModalState("")
    End Sub
    Private Sub btnSimplifiedWarningCancel_Click(sender As Object, e As EventArgs) Handles btnSimplifiedWarningCancel.Click
        hdnChangesPending.Value = ""
        cbAdvanced.Checked = True
        SetModalState("")
    End Sub

    Private Sub btnGlobalSave_Click(sender As Object, e As EventArgs) Handles btnGlobalSave.Click
        hdnChangesPending.Value = ""
        SaveChartDefaults()
        SaveBranding()
        PreviewBrand()
        ControlAvailability()
    End Sub

    Private Sub btnGlobalCancel_Click(sender As Object, e As EventArgs) Handles btnGlobalCancel.Click
        hdnChangesPending.Value = ""
        ViewState("BrandingObj") = Nothing
        SetBrandingDefaults(BrandingObj)
        SetModelDefaults()
        ControlAvailability()
    End Sub

    Private Sub SaveChartDefaults()

        Dim CA As New ChartAttrib(hdnAttrib.Value)

        With CA.Data
            .maxHeight = CInt(numMaxNodeHeight.Text)
            .maxWidth = CInt(numMaxNodeWidth.Text)
            .nodeHeight = CInt(numNodeHeight.Text)
            .nodeWidth = CInt(numNodeWidth.Text)
            .chartDirection = ddlbDirection.SelectedValue
            .canOverride = cbSettigsFixed.Checked

            .backgroundType = ddlbBackgroundType.SelectedValue
            .backgroundID = 0                     ' not set

            .backgroundContent = txtModelbg.Text
            Select Case ddlbBackgroundType.SelectedValue
                Case "Image"
                    .backgroundID = ddlbBackgroundImage.SelectedValue
                Case "Gradient"
                    .backgroundID = ddlbBackgroundGradient.SelectedValue
            End Select

            With .node
                .foreground = txtNodefg.Text
                .textBackground = txtNodeTxtBg.Text
                .background = txtNodebg.Text
                .border = txtNodeBorder.Text
                .textBackgroundBlock = cbNodeTxtBlock.Checked
                .iconColour = txtIconfg.Text
                .iconHover = txtIconHover.Text
            End With

            With .highlight
                .foreground = txtHighlightfg.Text
                .background = txtHighlightbg.Text
                .border = txtHighlightbBorder.Text
            End With

            With .tooltip
                .foreground = txtTTfg.Text
                .background = txtTTbg.Text
                .border = txtTTBorder.Text
            End With
            .corners = If(rbCornerRectangle.Checked, "Rectangle", "RoundedRectangle")
            .showShadow = cbShowShadow.Checked

            .shadowColour = txtShadow.Text
            With .link
                .colour = txtlinkColour.Text
                .hover = txtLinkHover.Text
                .width = Decimal.Parse(numLinkWidth.Text, CultureInfo.InvariantCulture)
                .type = ddlbLinkStyle.SelectedValue
                With .tooltip
                    .foreground = txtLinkTooltipfg.Text
                    .background = txtLinkTooltipbg.Text
                    .border = txtLinkTooltipBorder.Text
                End With
            End With
            .imagesApplicable = True 'cbUseImages.Checked
            .canOverride = cbSettigsFixed.Checked
        End With

        SQL = "UPDATE dbo.clients
                  SET attributes = @p2
                WHERE client_id = @p1"
        IawDB.execNonQuery(SQL, client_id, CA.GetData)
    End Sub

    Private Sub SaveBranding()

        ' --------------------------------------------------------------------
        ' Branding Screen
        '
        Dim CB As ChartBranding

        If BrandingObj Is Nothing Then
            CB = New ChartBranding
        Else
            CB = BrandingObj
        End If

        If Not String.IsNullOrEmpty(hdnLogo.Value) Then
            Dim imageData As Byte() = Convert.FromBase64String(hdnLogo.Value)
            Dim img As Image = GenericFunctions.BytesToImage(imageData)
            If img.Height > 56 Then
                img = GenericFunctions.ResizeImage(img, New Size(0, 56), GenericFunctions.SpecifyAxis.YAxis)
                imageData = GenericFunctions.ImageToBytes(img, hdnLogofilename.Value)
            End If

            CB.Data.bannerLogo = saveLogo(hdnLogofilename.Value, imageData, CB.Data.bannerLogo)
            CB.Data.logoDimension.width = img.Width.ToString + "px"
            CB.Data.logoDimension.height = img.Height.ToString + "px"
        End If

        ' Branding Tab
        If CB.Data.advancedBranding Then
            With CB.Data

                .bodyBGType = ddlbBodyBackgroundType.SelectedValue
                .bodyBGID = 0                     ' not set

                .bodyBG = bodyBgColour.Text
                Select Case ddlbBodyBackgroundType.SelectedValue
                    Case "Image"
                        .bodyBGID = ddlbBodyBackgroundImage.SelectedValue
                    Case "Gradient"
                        .bodyBGID = ddlbBodyBackgroundGradient.SelectedValue
                End Select

                With .bannerBar
                    .foreground = bannerTextColour.Text
                    .background = bannerBgColour.Text
                End With
                .bannerIconsHover = bannerIconsHoverColour.Text
                With .legend
                    .foreground = legendTextColour.Text
                    .background = legendBgColour.Text
                End With
                .legendborder = legendBorderColour.Text
                .contentAreaBG = contentAreaBgColour.Text
                With .link
                    .normal.foreground = linkTextColour.Text
                    .normal.background = linkBgColour.Text
                    .hover.foreground = linkTextHoverColour.Text
                    .hover.background = linkBgHoverColour.Text
                End With
                .textColour = textColour.Text
                With .listheader
                    .foreground = listHeaderTextColour.Text
                    .background = listHeaderBgColour.Text
                End With
                With .listBody
                    .foreground = listRowColour.Text
                    .background = listRowBgColour.Text
                End With
                With .listAltBody
                    .foreground = listAltRowColour.Text
                    .background = listAltRowBgColour.Text
                End With
                With .inputField
                    .foreground = inptFieldTextColour.Text
                    .background = inptFieldBgColour.Text
                End With
                .inputFieldBorder = inptFieldBorderColour.Text
                .inputFieldFocusBG = inptFieldFocusBgColour.Text
                With .tab
                    .normal.foreground = tabsTextColour.Text
                    .normal.background = tabsBgColour.Text
                    .hover.foreground = tabsTextHoverColour.Text
                    .hover.background = tabsBgHoverColour.Text
                End With
                With .selectedTab
                    .foreground = selectedTabsTextColour.Text
                    .background = selectedTabsBgColour.Text
                End With
                With .selectedRow
                    .normal.foreground = selectedRowTextColour.Text
                    .normal.background = selectedRowBgColour.Text
                    .hover.foreground = selectedRowTextHoverColour.Text
                    .hover.background = selectedRowBgHoverColour.Text
                End With
                .imageCharactersColour = imageCharactersColour.Text
                .imageCharactershighlightColour = imageCharactersHighlightColour.Text
                .menuButtonColour = menuButtonColour.Text
                .menuIconColour = menuIconColour.Text
                With .menuItem
                    .foreground = menuItemTextColour.Text
                    .background = menuItemBgColour.Text
                End With
                With .draggable
                    .normal.foreground = draggablesTextColour.Text
                    .normal.background = draggablesBGColour.Text
                    .hover.foreground = draggablesTextHoverColour.Text
                    .hover.background = draggablesBGHoverColour.Text
                End With
                .draggableBorder = draggablesBorderColour.Text
                .draggableHoverBorder = draggablesBorderHoverColour.Text
                .listHeaderIcons = listHeaderIconsColour.Text
                .listHeaderIconsHover = listHeaderIconsHoverColour.Text
                .listIcons = listBodyIconsColour.Text
                .listIconsHover = listBodyIconsHoverColour.Text

                .listHeaderIconsFilter = hdnHeaderIconsFilter.Value
                .listHeaderIconsHoverFilter = hdnHeaderIconsHoverFilter.Value
                .listIconsFilter = hdnBodyIconsFilter.Value
                .listIconsHoverFilter = hdnBodyIconsHoverFilter.Value

                .CheckBox = checkboxColour.Text
                .CheckBoxFilter = hdnCheckboxFilter.Value
            End With
        Else
            With CB.Data
                .bodyBG = simpPrincipal.Text
                With .bannerBar
                    .foreground = simpPrincipal.Text
                    .background = simpBannerBgColour.Text
                End With
                .bannerIconsHover = simpDarkContextArea.Text
                With .legend
                    .foreground = simpPrincipal.Text
                    .background = simpContentArea.Text
                End With
                .legendborder = simpContentArea.Text
                .contentAreaBG = simpContentArea.Text
                With .link
                    .normal.foreground = simpContentArea.Text
                    .normal.background = simpLightPrincipal.Text
                    .hover.foreground = simpText.Text
                    .hover.background = simpLightPrincipal.Text
                End With
                .textColour = simpText.Text
                With .listheader
                    .foreground = simpContentArea.Text
                    .background = simpText.Text
                End With
                With .listBody
                    .foreground = simpText.Text
                    .background = simpContentArea.Text
                End With
                With .listAltBody
                    .foreground = simpText.Text
                    .background = simpContentArea.Text
                End With
                With .inputField
                    .foreground = simpText.Text
                    .background = simpContentArea.Text
                End With
                .inputFieldBorder = simpLightPrincipal.Text
                .inputFieldFocusBG = simpDarkContextArea.Text
                With .tab
                    .normal.foreground = simpContentArea.Text
                    .normal.background = simpLightPrincipal.Text
                    .hover.foreground = simpText.Text
                    .hover.background = simpLightPrincipal.Text
                End With
                With .selectedTab
                    .foreground = simpContentArea.Text
                    .background = simpPrincipal.Text
                End With
                With .selectedRow
                    .normal.foreground = simpContentArea.Text
                    .normal.background = simpPrincipal.Text
                    .hover.foreground = simpContentArea.Text
                    .hover.background = simpLightPrincipal.Text
                End With
                .imageCharactersColour = simpPrincipal.Text
                .imageCharactershighlightColour = simpExceptions.Text
                .menuButtonColour = simpPrincipal.Text
                .menuIconColour = simpPrincipal.Text
                With .menuItem
                    .foreground = simpPrincipal.Text
                    .background = simpContentArea.Text
                End With
                With .draggable
                    .normal.foreground = simpText.Text
                    .normal.background = simpContentArea.Text
                    .hover.foreground = simpContentArea.Text
                    .hover.background = simpLightPrincipal.Text
                End With
                .draggableBorder = simpContentArea.Text
                .draggableHoverBorder = simpContentArea.Text
                .listHeaderIcons = simpContentArea.Text
                .listHeaderIconsHover = simpPrincipal.Text
                .listIcons = simpLightText.Text
                .listIconsHover = simpPrincipal.Text

                .listHeaderIconsFilter = hdnHeaderIconsFilter.Value
                .listHeaderIconsHoverFilter = hdnHeaderIconsHoverFilter.Value
                .listIconsFilter = hdnBodyIconsFilter.Value
                .listIconsHoverFilter = hdnBodyIconsHoverFilter.Value

                .CheckBox = simpText.Text
                .CheckBoxFilter = hdnCheckboxFilter.Value
            End With
        End If

        CB.Data.logoNormalColour = logoNormalColour.Text
        CB.Data.logoLighterColour = logoLighterColour.Text
        CB.Data.logoDarkerColour = logoDarkerColour.Text

        CB.Data.advancedBranding = cbAdvanced.Checked

        SQL = "UPDATE dbo.client_brand
                  SET branding = @p2
                WHERE brand_id = @p1"
        IawDB.execNonQuery(SQL, BrandID, CB.GetDataNoDefault)

    End Sub

    Private Sub numMaxNodeHeight_TextChanged(sender As Object, e As EventArgs) Handles numMaxNodeHeight.TextChanged
        Try
            Dim mw As Integer = CInt(numMaxNodeHeight.Text)
            Dim w As Integer = CInt(numNodeHeight.Text)

            slideNodeHeight.Maximum = mw
            SetSliderRanges()

            If w > mw Then
                numNodeHeight.Text = mw
            End If

        Catch ex As Exception
        End Try
    End Sub

    Private Sub numMaxNodeWidth_TextChanged(sender As Object, e As EventArgs) Handles numMaxNodeWidth.TextChanged
        Try
            Dim mw As Integer = CInt(numMaxNodeWidth.Text)
            Dim w As Integer = CInt(numNodeWidth.Text)

            slideNodeWidth.Maximum = mw
            SetSliderRanges()
            If w > mw Then
                numNodeWidth.Text = mw
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Function saveLogo(filename As String, bytes As Byte(), prevLogo As String) As String

        Dim brandUnique As String
        brandUnique = IawDB.execScalar("SELECT brand_unique
                                          FROM dbo.client_brand
                                         WHERE brand_id = @p1", BrandID)

        Dim extension As String = Path.GetExtension(filename)

        Dim uploadPath As String = Context.Server.MapPath("~/logos/")

        removeLogo()

        Dim fullName As String = brandUnique + extension
        Dim filePath As String = Path.Combine(uploadPath, fullName)

        Try
            File.WriteAllBytes(filePath, bytes)
            IawDB.execScalar("UPDATE dbo.client_brand
                                 SET logo_name = @p2 
                               WHERE brand_id = @p1", BrandID, filename)
        Catch ex As Exception
            Throw New FormatException("Could not save logo to the logos folder")
        End Try

        Return fullName
    End Function

    Private Sub removeLogo()
        Dim logoName As String = IawDB.execScalar("SELECT IsNull(json_value(branding,'$.bannerLogo'),'') FROM dbo.client_brand WHERE brand_id = @p1", BrandID)
        If logoName = "" Then Return
        Dim fileName As String = Context.Server.MapPath("~/logos/") + logoName
        If File.Exists(fileName) Then
            File.Delete(fileName)
        End If
    End Sub
#End Region

#Region "Brand"
    Private Sub grdBrand_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdBrand.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow Then
            e.Row.Cells(0).Attributes("onclick") = ClientScript.GetPostBackClientHyperlink(Me.grdBrand, "Select$" & e.Row.RowIndex)
        End If
    End Sub

    Private Sub grdBrand_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdBrand.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            ' is this the vivichart record
            Dim lBrand As Integer = rv("brand_id")
            Dim HideBrand As Boolean = IawDB.execScalar("SELECT count(1)
                                                           FROM dbo.client_brand
                                                          WHERE client_id = @p1 ", client_id) = 1
            If Not HideBrand Then
                If IawDB.execScalar("SELECT count(1)
                                       FROM dbo.suser 
                                      WHERE client_id = @p1
                                        AND brand_id = @p2", client_id, lBrand) > 0 Then
                    HideBrand = True
                End If
            End If
            If rv("default_brand") = "1" Then HideBrand = True

            If HideBrand Then
                ' get a handle to the delete button and hide it
                CTRL = e.Row.FindControl("btnDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
                CTRL = e.Row.FindControl("lblNoDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = True
                End If
            End If
        End If
    End Sub

    Private Sub grdBrand_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdBrand.RowCommand
        Dim CurRowIndex As Integer
        Dim RowIndex As Integer
        Dim DR As DataRow = Nothing

        If e.CommandName = "New" Then
            hdnBrandID.Value = -1
            txtBrandName.Text = ""
            cbDefaultBrand.Checked = False

            SetModalState("Brand")
            Return
        End If

        CurRowIndex = grdBrand.SelectedIndex

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        BrandID = grdBrand.DataKeys(RowIndex).Value
        DR = BrandsDR()

        Select Case e.CommandName

            Case "Select"
                ViewState("BrandingObj") = Nothing
                PreviewBrand()
                btnThemeSelect.Visible = DR("brand_name").tolower <> "vivichart"

            Case "AmendRow"
                hdnBrandID.Value = BrandID
                txtBrandName.Text = DR("brand_name")
                cbDefaultBrand.Checked = If(DR("default_brand") = "1", True, False)
                SetModalState("Brand")

            Case "DeleteRow"
                mpeBrandDelete.Show()
        End Select
    End Sub

    Private Sub btnBrandDeleteOk_Click(sender As Object, e As EventArgs) Handles btnBrandDeleteOk.Click
        removeLogo()
        IawDB.execNonQuery("DELETE dbo.client_brand where brand_id = @p1", BrandID)
        If CleanBrandsDT.Rows.Count > 0 Then
            BrandID = BrandsDT.Rows(0)("brand_id")
            If IawDB.execScalar("SELECT count(1) FROM dbo.client_brand WHERE client_id = @p1 AND default_brand = '1'", client_id) = 0 Then
                IawDB.execNonQuery("UPDATE dbo.client_brand SET default_brand = '1' WHERE brand_id = @p1", BrandID)
            End If
        End If
        PreviewBrand()
    End Sub

    Private Sub btnBrandDeleteCancel_Click(sender As Object, e As EventArgs) Handles btnBrandDeleteCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnBrandCancel_Click(sender As Object, e As EventArgs) Handles btnBrandCancel.Click
        SetModalState("")
    End Sub

    Private Sub cvBrandName_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles cvBrandName.ServerValidate
        args.IsValid = True
        Dim hdnID As Integer
        If hdnBrandID.Value = "" Then
            hdnID = -1
        Else
            hdnID = CInt(hdnBrandID.Value)
        End If

        If IawDB.execScalar("SELECT count(1)
                               FROM dbo.client_brand
                              WHERE client_id = @p1
                                AND LOWER(brand_name) = LOWER(@p2)
                                AND brand_id <> @p3", client_id, txtBrandName.Text, hdnID) > 0 Then
            args.IsValid = False
        End If
    End Sub

    Private Sub btnBrandSave_Click(sender As Object, e As EventArgs) Handles btnBrandSave.Click
        If Not Page.IsValid Then Return

        Dim reloadScreen As Boolean = False

        ' have to have at least one 'defualt' brand
        If IawDB.execScalar("SELECT count(1) FROM dbo.client_brand WHERE default_brand = '1' AND client_id = @p1", client_id) = 0 Then
            cbDefaultBrand.Checked = True
        End If

        ' if we are setting this item as default then clear any others
        If cbDefaultBrand.Checked Then
            IawDB.execNonQuery("UPDATE dbo.client_brand
                                   SET default_brand = '0'
                                 WHERE client_id = @p1", client_id)
        End If

        If hdnBrandID.Value = -1 Then

            ' Adding new reocrd

            Dim BrandO As New ChartBranding(True)
            BrandO.Data.advancedBranding = False
            BrandO.Data.bannerLogo = Nothing

            SQL = "INSERT dbo.client_brand (client_id, brand_name, branding, default_brand)
                   VALUES (@p1,@p2,@p3,@p4);
                   SELECT SCOPE_IDENTITY()"
            BrandID = IawDB.execScalar(SQL, client_id,
                                    txtBrandName.Text.Trim,
                                    BrandO.GetDataNoDefault,
                                    If(cbDefaultBrand.Checked, "1", "0"))

            SetBrandingDefaults(BrandO)
        Else
            ' updating existing record

            SQL = "UPDATE dbo.client_brand 
                      SET brand_name = @p2,
                          default_brand = @p3
                    WHERE brand_id = @p1"
            IawDB.execNonQuery(SQL, BrandID,
                                        txtBrandName.Text.Trim,
                                        If(cbDefaultBrand.Checked, "1", "0"))
        End If

        PreviewBrand()
    End Sub
#End Region

#Region "Theme"
    Private Sub btnThemeSelect_Click(sender As Object, e As EventArgs) Handles btnThemeSelect.Click
        rblBrand.DataSource = CleanBrandsDT
        rblBrand.DataValueField = "brand_id"
        rblBrand.DataTextField = "brand_name"
        rblBrand.DataBind()

        tpCopyFromBrands.Visible = BrandsDT.Rows.Count > 0

        SetGrid(grdTheme, ThemeDT, False)

        tpCopyFromThemes.Visible = ThemeDT.Rows.Count > 0

        SetModalState("Theme")
    End Sub

    Private Sub btnThemeSave_Click(sender As Object, e As EventArgs) Handles btnThemeSave.Click
        Dim ID As Integer
        Dim script As String = "<script>change_Branding();</script>"
        If tcTheme.ActiveTabIndex = 0 Then
            If rblBrand.SelectedValue <> "" Then
                ID = BrandID
                If Integer.TryParse(rblBrand.SelectedValue, BrandID) Then
                    Dim CB As ChartBranding = New ChartBranding(BrandsDR("branding").ToString)
                    SQL = "UPDATE dbo.client_brand
                              SET branding = @p2
                            WHERE brand_id = @p1"
                    IawDB.execNonQuery(SQL, ID, CB.GetDataNoDefault)
                    'SetBrandingDefaults(New ChartBranding(BrandsDR("branding").ToString))
                    'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "change_Branding();", script, False)
                End If
                BrandID = ID
                PreviewBrand()
            End If
        Else
            ' Iterate through GridView rows to find the selected row

            For Each row As GridViewRow In grdTheme.Rows
                Dim rbSelect As RadioButton = DirectCast(row.FindControl("rbSelect"), RadioButton)
                If rbSelect IsNot Nothing AndAlso rbSelect.Checked Then
                    ' This is the selected row - process accordingly
                    ID = CInt(grdTheme.DataKeys(row.RowIndex).Value)
                    ' You can now use selectedThemeId for further processing
                    Dim CB As ChartBranding = New ChartBranding(ThemeDR(ID)("theme").ToString)
                    CB.Data.bannerLogo = Nothing
                    SQL = "UPDATE dbo.client_brand
                              SET branding = @p2
                            WHERE brand_id = @p1"
                    IawDB.execNonQuery(SQL, BrandID, CB.GetDataNoDefault)
                    PreviewBrand()
                    Exit For
                End If
            Next
        End If
        SetModalState("")
    End Sub

    Private Sub btnThemeCancel_Click(sender As Object, e As EventArgs) Handles btnThemeCancel.Click
        SetModalState("")
    End Sub

    Private Sub grdTheme_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdTheme.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)

            If rv("swatch") Is DBNull.Value Then
                e.Row.Visible = False
                Return
            End If

            ' Assuming the JSON string is in your DataRow (adjust the index or key as needed)
            Dim jsonData As String = rv("swatch")

            ' Deserialize the JSON string into a list of colors
            Dim colours As List(Of String) = JsonConvert.DeserializeObject(Of List(Of String))(jsonData)

            ' Find the Placeholder control in the current row
            Dim placeholderColors As PlaceHolder = DirectCast(e.Row.FindControl("phColours"), PlaceHolder)

            ' Create a div for each color and add it to the Placeholder
            For Each colour As String In colours
                Dim colorDiv As New LiteralControl($"<div class='swatch' style='background-color:{colour}'></div>")
                placeholderColors.Controls.Add(colorDiv)
            Next
        End If
    End Sub

#End Region
#Region "Background"
    Sub grdBackgrounds_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles grdBackgrounds.RowCommand
        Dim RowIndex As Integer
        Dim DR As DataRow

        hdnBgContent.Value = ""
        hdnBgStructure.Value = ""

        If e.CommandName = "New" Then
            hdnClientUnique.Value = ""
            txtBgName.Text = ""

            ' show the image div and hide the gradient div
            divBgGradient.Attributes("hidden") = "hidden"
            divBgImage.Attributes.Remove("hidden")

            rbBgSelectImage.Checked = True
            rbBgSelectGradient.Checked = False

            hdnBgImageName.Value = ""
            hdnBgContent.Value = ""
            hdnBgType.Value = ""

            rbBgSelectImage.Style("display") = "inline-block"
            rbBgSelectGradient.Style("display") = "inline-block"

            SetModalState("Background")
            Return
        End If

        RowIndex = Int32.Parse(e.CommandArgument.ToString())
        Dim unique_id As Integer = grdBackgrounds.DataKeys(RowIndex).Value
        DR = BgDR(unique_id)
        hdnClientUnique.Value = unique_id.ToString

        Select Case e.CommandName
            Case "AmendRow"
                txtBgName.Text = DR.GetValue("description", "")

                rbBgSelectGradient.Checked = DR.GetValue("background_type", "02") = "02"
                rbBgSelectImage.Checked = DR.GetValue("background_type", "02") = "01"

                divBgImage.Attributes("hidden") = "hidden"
                divBgGradient.Attributes("hidden") = "hidden"

                hdnBgContent.Value = DR.GetValue("content", "")
                hdnBgType.Value = DR.GetValue("background_type")

                If DR.GetValue("background_type") = "02" Then
                    'gradient
                    divBgGradient.Attributes.Remove("hidden")
                    rbBgSelectImage.Checked = False
                    rbBgSelectGradient.Checked = True
                    ddlbImageRepeatType.SelectedValue = "01"
                    hdnBgStructure.Value = DR.GetValue("structure", "")
                Else
                    ' image
                    divBgImage.Attributes.Remove("hidden")
                    hdnBgImageName.Value = DR.GetValue("image_name", "")
                    hdnPrevBgImage.Value = DR.GetValue("content", "")

                    rbBgSelectImage.Checked = True
                    rbBgSelectGradient.Checked = False
                    ddlbImageRepeatType.SelectedValue = DR.GetValue("image_repeat", "01")
                End If

                If (isBackgroundInUse(unique_id)) Then
                    If DR.GetValue("background_type") = "02" Then
                        rbBgSelectImage.Style("display") = "none"
                        rbBgSelectGradient.Style("display") = "inline-block"
                    Else
                        rbBgSelectImage.Style("display") = "inline-block"
                        rbBgSelectGradient.Style("display") = "none"
                    End If
                Else
                    rbBgSelectImage.Style("display") = "inline-block"
                    rbBgSelectGradient.Style("display") = "inline-block"
                End If

                SetModalState("Background")

            Case "DeleteRow"
                SetModalState("DeleteBackground")
        End Select
    End Sub

    Private Sub cvBgName_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles cvBgName.ServerValidate
        args.IsValid = True
        Dim hdnUnique As Integer
        If hdnClientUnique.Value = "" Then
            hdnUnique = -1
        Else
            hdnUnique = CInt(hdnClientUnique.Value)
        End If

        If IawDB.execScalar("SELECT count(1)
                               FROM dbo.background
                              WHERE client_id = @p1
                                AND LOWER(description) = LOWER(@p2)
                                AND unique_id <> @p3",
                            client_id,
                            txtBgName.Text,
                            hdnUnique) > 0 Then
            args.IsValid = False
        End If
    End Sub

    Private Sub btnBackgroundSave_Click(sender As Object, e As EventArgs) Handles btnBackgroundSave.Click
        If Not Page.IsValid Then Return
        Dim backgroundContent As String = ""
        Dim backgroundType As String
        Dim bgImageName As String = ""

        If rbBgSelectImage.Checked Then
            backgroundType = "01"       ' image
            bgImageName = hdnBgImageName.Value
        Else
            backgroundType = "02"       ' gradient
        End If

        If hdnClientUnique.Value = "" Then
            ' doing add new
            If Not String.IsNullOrEmpty(hdnBgContent.Value) Then
                'there should be something in hidden content field
                If String.Equals(backgroundType, "02") Then
                    'if background type gradient selected
                    backgroundContent = hdnBgContent.Value
                Else
                    'if background type image selected
                    backgroundContent = uploadBgImage()
                End If

                SQL = "INSERT dbo.background (client_id, background_type, content, description, image_repeat, structure, image_name) 
                       VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p7)"
                IawDB.execNonQuery(SQL, client_id,
                                       backgroundType,
                                       backgroundContent,
                                       txtBgName.Text.Trim,
                                       ddlbImageRepeatType.SelectedValue,
                                       hdnBgStructure.Value,
                                       bgImageName)
            End If
        Else
            ' doing update
            ' if an image is supplied, then be aware that you need to remove any previous one.
            ' but you'll be updating the description and image repeat items
            If Not String.IsNullOrEmpty(hdnBgContent.Value) Then

                If String.Equals(backgroundType, "02") Then
                    backgroundContent = hdnBgContent.Value
                    removeBgImage(hdnPrevBgImage.Value)
                Else
                    If FileUpload2.HasFile Then
                        removeBgImage(hdnPrevBgImage.Value)
                        backgroundContent = uploadBgImage()
                    Else
                        backgroundContent = hdnBgContent.Value
                    End If
                End If

                SQL = "UPDATE dbo.background
                          SET background_type = @p2, content = @p3, description = @p4, image_repeat = @p5, structure = @p6, image_name = @p7
                        WHERE unique_id = @p1"
                IawDB.execNonQuery(SQL, hdnClientUnique.Value,
                                       backgroundType,
                                       backgroundContent,
                                       txtBgName.Text.Trim,
                                       ddlbImageRepeatType.SelectedValue,
                                       hdnBgStructure.Value,
                                       bgImageName)
            End If
        End If

        ViewState("Backgrounds") = Nothing
        ViewState("Gradients") = Nothing

        SetDDLB(ddlbBackgroundImage, BackgroundsDT, "description", "unique_id")
        SetDDLB(ddlbBodyBackgroundImage, BackgroundsDT, "description", "unique_id")
        SetDDLB(ddlbBackgroundGradient, GradientsDT, "description", "unique_id")
        SetDDLB(ddlbBodyBackgroundGradient, GradientsDT, "description", "unique_id")

        SetModalState("")
        RefreshGrid("Background")
    End Sub

    Private Sub grdBackgrounds_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdBackgrounds.RowDataBound
        Dim CTRL As Control
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim rv As DataRowView = CType(e.Row.DataItem, DataRowView)
            If isBackgroundInUse(rv("unique_id")) Then
                CTRL = e.Row.FindControl("btnDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = False
                End If
                CTRL = e.Row.FindControl("lblNoDeleteRow")
                If CTRL IsNot Nothing Then
                    CTRL.Visible = True
                End If
            End If

            Dim label As WebControls.Label = DirectCast(e.Row.FindControl("lblgradientBGPreview"), WebControls.Label)
            If rv("background_type") = "02" Then
                label.Style("background") = rv("content")
                label.Visible = True
            Else
                If String.IsNullOrEmpty(rv("preview")) Then
                    label.CssClass = "missingImage"
                    label.ToolTip = ctx.Translate("::LT_S0014") ' LT_S0014 = No Image
                    label.Visible = True
                Else
                    Dim img As WebControls.Image = DirectCast(e.Row.FindControl("imgBGPreview"), WebControls.Image)
                    Dim extension As String = Path.GetExtension(rv("content")).Substring(1)
                    img.ImageUrl = "data:image/" + extension + ";base64," + rv("preview")
                    img.Visible = True
                End If
            End If
        End If
    End Sub

    Private Sub removeBgImage(img As String)
        Dim prevBgImage As String = Path.Combine(Context.Server.MapPath("~/Backgrounds/"), img)
        If File.Exists(prevBgImage) Then
            File.Delete(prevBgImage)
        End If
    End Sub

    Private Function uploadBgImage() As String
        Dim imageData As Byte() = Convert.FromBase64String(hdnBgContent.Value)
        Dim extension As String = Path.GetExtension(hdnBgImageName.Value)
        Dim uploadPath As String = Context.Server.MapPath("~/Backgrounds/")
        Dim guidString As String = Guid.NewGuid().ToString.Replace("-", "")
        Dim newFileName As String = guidString + extension
        Dim filePath As String = Path.Combine(uploadPath, newFileName)
        Try
            File.WriteAllBytes(filePath, imageData)
        Catch ex As Exception
            Throw New FormatException("Could not save background image in Backgrounds folder")
        End Try
        Return newFileName
    End Function

    Private Sub btnBackgroundCancel_Click(sender As Object, e As EventArgs) Handles btnBackgroundCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnDeleteCancel_Click(sender As Object, e As EventArgs) Handles btnDeleteCancel.Click
        SetModalState("")
    End Sub

    Private Sub btnDeleteOk_Click(sender As Object, e As EventArgs) Handles btnDeleteOk.Click
        Dim img As String = IawDB.execScalar("SELECT content
                                                FROM dbo.background
                                               WHERE unique_id = @p1", hdnClientUnique.Value)
        removeBgImage(img)
        IawDB.execNonQuery("DELETE FROM dbo.background WHERE unique_id = @p1", hdnClientUnique.Value)
        Dim DT As DataTable = CleanBgDT()
        SetGrid(grdBackgrounds, DT, False)
        SetModalState("")
    End Sub
#End Region

End Class
