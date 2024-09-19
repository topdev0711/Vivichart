Imports Newtonsoft.Json

<Serializable>
Public Class ChartBranding

    Public Data As Branding

    Public Sub New(attr As String)
        SetData(attr)
    End Sub

    Public Sub New()
        Me.Data = GetDefault()
    End Sub

    Public Sub New(simpleBranding As Boolean)
        If simpleBranding Then
            Me.Data = GetSimpleDefault()
        Else
            Me.Data = GetDefault()
        End If
    End Sub

    Public Function GetData() As String
        If String.IsNullOrEmpty(Me.Data.bannerLogo) Or Me.Data.logoDimension Is Nothing Then
            Dim d As Branding = GetDefault()
            Me.Data.bannerLogo = d.bannerLogo
            Me.Data.logoDimension = d.logoDimension
        End If

        Return JsonConvert.SerializeObject(Me.Data, Formatting.None)
    End Function

    Public Function GetDataNoDefault() As String
        Return JsonConvert.SerializeObject(Me.Data, Formatting.None)
    End Function

    Public Sub SetData(ByVal jsonString As String)
        Dim B As Branding = GetDefault()

        If String.IsNullOrEmpty(jsonString) Then
            Me.Data = B
        Else
            Try
                Me.Data = JsonConvert.DeserializeObject(Of Branding)(jsonString)
                'If String.IsNullOrEmpty(Me.Data.bannerLogo) Then
                '    Me.Data.bannerLogo = B.bannerLogo
                '    Me.Data.logoDimension = B.logoDimension
                'End If

            Catch ex As Exception
                Me.Data = GetDefault()
            End Try
        End If
    End Sub

    Public Function GetDefault() As Branding
        ' Create the Branding object
        Dim attr As New Branding With {
                .bannerLogo = "vivichart.png",
                .logoDimension = New Dimension With {
                    .width = "241px",
                    .height = "50px"
                },
                .bodyBGType = "SolidColour",
                .bodyBGID = 0,
                .bodyBG = "#4682B4",
                .bannerBar = New Colours With {
                    .foreground = "#4682B4",
                    .background = "#FFFFFF"
                },
                .bannerIconsHover = "#079AE2",
                .contentAreaBG = "#FFFFFF",
                .link = New Hoverable With {
                    .normal = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#079AE2"
                    },
                    .hover = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#0585c5"
                    }
                },
                .textColour = "#111213",
                .inputField = New Colours With {
                    .foreground = "#111213",
                    .background = "#FFFFFF"
                },
                .inputFieldBorder = "#079AE2",
                .inputFieldFocusBG = "#E9E4EE",
                .legend = New Colours With {
                    .foreground = "#26557F",
                    .background = "#FFFFFF"
                },
                .legendborder = "#4682B4",
                .tab = New Hoverable With {
                    .normal = New Colours With {
                        .foreground = "#000000",
                        .background = "#FFFFFF"
                    },
                    .hover = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#079AE2"
                    }
                },
                .selectedTab = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#079AE2"
                },
                .listheader = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#002952"
                },
                .listBody = New Colours With {
                    .foreground = "#111213",
                    .background = "#F4F4FF"
                },
                .listAltBody = New Colours With {
                    .foreground = "#111213",
                    .background = "#F4F4FF"
                },
                .selectedRow = New Hoverable With {
                    .normal = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#079AE2"
                    },
                    .hover = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#079AE2"
                    }
                },
                .menuItem = New Colours With {
                    .foreground = "#111213",
                    .background = "#FFFFFF"
                },
                .menuButtonColour = "#079AE2",
                .imageCharactersColour = "#0000c0",
                .imageCharactershighlightColour = "#FF0000",
                .draggable = New Hoverable With {
                    .normal = New Colours With {
                        .foreground = "#111213",
                        .background = "#FFFFFF"
                    },
                    .hover = New Colours With {
                        .foreground = "#FFFFFF",
                        .background = "#079AE2"
                    }
                },
                .draggableBorder = "#FFFFFF",
                .draggableHoverBorder = "#FFFFFF",
                .listHeaderIcons = "#EBBA32",
                .listHeaderIconsHover = "#FFFFFF",
                .listIcons = "#656567",
                .listIconsHover = "#3481C6",
                .listHeaderIconsFilter = "invert(73%) sepia(58%) saturate(535%) hue-rotate(358deg) brightness(94%) contrast(97%)",
                .listHeaderIconsHoverFilter = "invert(87%) sepia(100%) saturate(3%) hue-rotate(269deg) brightness(111%) contrast(97%)",
                .listIconsFilter = "invert(39%) sepia(11%) saturate(50%) hue-rotate(201deg) brightness(98%) contrast(98%)",
                .listIconsHoverFilter = "invert(42%) sepia(65%) saturate(543%) hue-rotate(166deg) brightness(97%) contrast(90%)",
                .CheckBox = "#2B2B2B",
                .CheckBoxFilter = "invert(12%) sepia(5%) saturate(25%) hue-rotate(41deg) brightness(105%) contrast(88%)",
                .logoNormalColour = "",
                .logoLighterColour = "",
                .logoDarkerColour = "",
                .advancedBranding = True
            }

        Return attr
    End Function

    Public Function GetSimpleDefault() As Branding
        Dim attr As New Branding With {
            .bannerLogo = Nothing,
            .logoDimension = New Dimension With {
                .width = "241px",
                .height = "50px"
            },
            .bodyBGType = "SolidColour",
            .bodyBG = "#4682B4",
            .bannerBar = New Colours With {
                .foreground = "#4682B4",
                .background = "#FFFFFF"
            },
            .contentAreaBG = "#FFFFFF",
            .link = New Hoverable With {
                .normal = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#079AE2"
                },
                .hover = New Colours With {
                    .foreground = "#111213",
                    .background = "#079AE2"
                }
            },
            .textColour = "#111213",
            .inputField = New Colours With {
                .foreground = "#111213",
                .background = "#FFFFFF"
            },
            .inputFieldBorder = "#079AE2",
            .inputFieldFocusBG = "#E9E4EE",
            .legend = New Colours With {
                .foreground = "#4682B4",
                .background = "#FFFFFF"
            },
            .legendborder = "#FFFFFF",
            .tab = New Hoverable With {
                .normal = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#079AE2"
                },
                .hover = New Colours With {
                    .foreground = "#111213",
                    .background = "#079AE2"
                }
            },
            .selectedTab = New Colours With {
                .foreground = "#FFFFFF",
                .background = "#4682B4"
            },
            .listheader = New Colours With {
                .foreground = "#FFFFFF",
                .background = "#111213"
            },
            .listBody = New Colours With {
                .foreground = "#111213",
                .background = "#FFFFFF"
            },
            .listAltBody = New Colours With {
                .foreground = "#111213",
                .background = "#FFFFFF"
            },
            .selectedRow = New Hoverable With {
                .normal = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#4682B4"
                },
                .hover = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#079AE2"
                }
            },
            .menuButtonColour = "#4682B4",
            .menuItem = New Colours With {
                .foreground = "#4682B4",
                .background = "#FFFFFF"
            },
            .menuIconColour = "#000080",
            .imageCharactersColour = "#4682B4",
            .imageCharactershighlightColour = "#FF0000",
            .draggable = New Hoverable With {
                .normal = New Colours With {
                    .foreground = "#111213",
                    .background = "#FFFFFF"
                },
                .hover = New Colours With {
                    .foreground = "#FFFFFF",
                    .background = "#079AE2"
                }
            },
            .draggableBorder = "#FFFFFF",
            .draggableHoverBorder = "#FFFFFF",
            .listHeaderIcons = "#FFFFFF",
            .listHeaderIconsHover = "#4682B4",
            .listIcons = "#656567",
            .listIconsHover = "#4682B4",
            .listHeaderIconsFilter = "invert(100%) sepia(0%) saturate(0%) hue-rotate(93deg) brightness(103%) contrast(103%)",
            .listHeaderIconsHoverFilter = "invert(44%) sepia(87%) saturate(293%) hue-rotate(164deg) brightness(92%) contrast(96%)",
            .listIconsFilter = "invert(42%) sepia(0%) saturate(1705%) hue-rotate(302deg) brightness(90%) contrast(84%)",
            .listIconsHoverFilter = "invert(42%) sepia(80%) saturate(318%) hue-rotate(164deg) brightness(96%) contrast(93%)",
            .CheckBox = "#111213",
            .CheckBoxFilter = "invert(5%) sepia(9%) saturate(526%) hue-rotate(168deg) brightness(91%) contrast(94%)",
            .logoNormalColour = "",
            .logoLighterColour = "",
            .logoDarkerColour = "",
            .advancedBranding = False
        }
        Return attr
    End Function

    Public Shared Function GenerateVars(clientID As Integer, Optional URLuser As String = "") As String
        Dim SQL As String
        Dim B As String = ""
        Dim DC As ChartBranding
        Dim BrandID As Integer = -1
        Dim backRepeat As String = ""
        Dim backContent As String = ""
        Dim backString As String = ""
        Dim backSize As String = "Auto"
        Dim UserRef As String = ""

        If URLuser <> "" Then
            UserRef = URLuser
        ElseIf ctx.user_ref <> "" Then
            UserRef = ctx.user_ref
        End If

        Using DB As New IawDB

            ' ensure that a default brand record exists for the client
            If UserRef <> "" Then
                ' ensure a brand exists
                Dim TempClientID As Integer = DB.Scalar("select client_id from suser where user_ref = @p1", UserRef)
                EnsureBrand(TempClientID)
            ElseIf clientID > 0 Then
                EnsureBrand(clientID)
            End If

            ' on the client_settings screen, we can force the system to display the selected brand (as a preview)
            If Not String.IsNullOrEmpty(ctx.session("forceBrand")) Then
                BrandID = ctx.session("forceBrand")
            Else
                If UserRef <> "" Then

                    ' if we're logged in, we can use the specific brand from the suser table or
                    ' of mot set, use the default brand from the client_brand for the users' client 

                    BrandID = DB.Scalar("select IsNull(brand_id,-1) from suser where user_ref = @p1", UserRef)
                    If BrandID > 0 Then
                        ' ensure it exists
                        If DB.Scalar("select count(1) from client_brand where brand_id = @p1", BrandID) = 0 Then
                            BrandID = -1
                        End If
                    End If

                    If BrandID < 1 Then
                        ' get the default brand for the client
                        SQL = "select B.brand_id 
                                 from dbo.suser S
                                      join dbo.client_brand B
                                        on B.client_id = S.client_id
                                where S.user_ref = @p1
                                  and B.default_brand = '1'"
                        BrandID = DB.Scalar(SQL, UserRef)
                    End If

                ElseIf clientID > 0 Then
                    SQL = "select brand_id 
                             from dbo.client_brand
                            where client_id = @p1
                              and default_brand = '1'"
                    BrandID = DB.Scalar(SQL, clientID)
                Else
                    SQL = "select brand_id 
                             from dbo.client_brand
                            where client_id = 1
                              and default_brand = '1'"
                    BrandID = DB.Scalar(SQL)
                End If
            End If

            If BrandID > 0 Then
                SQL = "select branding
                         from dbo.client_brand
                        where brand_id = @p1
                          and branding is not null"
                B = DB.Scalar(SQL, BrandID)
            End If
        End Using

        If String.IsNullOrEmpty(B) Then
            DC = New ChartBranding()
        Else
            DC = New ChartBranding(B)
        End If

        ' DC should now contain the data 
        Dim str As New StringBuilder
        str.AppendLine(":root {")

        Dim Logo As String = "url('" + ctx.virtualDir + "/logos/" & DC.Data.bannerLogo & "')"

        If Not String.IsNullOrEmpty(DC.Data.bannerLogo) Then
            createVar(str, "bannerbar-logo", Logo)
        End If
        createVar(str, "logo-width", DC.Data.logoDimension.width)
        createVar(str, "logo-height", DC.Data.logoDimension.height)

        ' Page Background
        If DC.Data.bodyBGType.ToLower.ContainsOneOf("image", "gradient") Then
            SQL = "select image_repeat, content, structure from dbo.background where unique_id = @p1"
            Dim DR As DataRow = IawDB.execGetDataRow(SQL, DC.Data.bodyBGID)

            backRepeat = DR.GetValue("image_repeat", "03")
            backContent = DR.GetValue("content", "")

            Select Case DC.Data.bodyBGType.ToLower
                Case "image"
                    Select Case backRepeat
                        Case "01"   ' no repeat
                            backString += " no-repeat"
                        Case "02"   ' fit
                            backString += " no-repeat"
                            createVar(str, "body-bg-size", "cover")
                        Case "03"   ' repeat
                            backString += " repeat"
                        Case "04"   ' repeat-x (across the screen)
                            backString += " repeat-x"
                        Case "05"   ' repeat-y (down the screen)
                            backString += " repeat-y"
                        Case "06"   ' fit across
                            backString += " no-repeat"
                            createVar(str, "body-bg-size", "100% auto")
                    End Select
                    createVar(str, "body-bg", "url(" + ctx.virtualDir + "/Backgrounds/" + backContent + ")" + backString)
                Case "gradient"
                    createVar(str, "body-bg", backContent)
            End Select
        End If
        createVar(str, "body-bg-color", DC.Data.bodyBG)

        createVar(str, "bannerbar-color", DC.Data.bannerBar.foreground)
        createVar(str, "bannerbar-bg-color", DC.Data.bannerBar.background)
        createVar(str, "bannerbar-icons-hover-color", DC.Data.bannerIconsHover)
        createVar(str, "content-area-bg-color", DC.Data.contentAreaBG)
        createVar(str, "link-bg-color", DC.Data.link.normal.background)
        createVar(str, "link-color", DC.Data.link.normal.foreground)
        createVar(str, "link-bg-hover-color", DC.Data.link.hover.background)
        createVar(str, "link-hover-color", DC.Data.link.hover.foreground)
        createVar(str, "text-color", DC.Data.textColour)
        createVar(str, "input-field-bg-color", DC.Data.inputField.background)
        createVar(str, "input-field-color", DC.Data.inputField.foreground)
        createVar(str, "input-field-focus-bg-color", DC.Data.inputFieldFocusBG)
        createVar(str, "input-border", DC.Data.inputFieldBorder)
        createVar(str, "legend-bg-color", DC.Data.legend.background)
        createVar(str, "legend-color", DC.Data.legend.foreground)
        createVar(str, "legend-border-color", DC.Data.legendborder)
        createVar(str, "tab-bg-color", DC.Data.tab.normal.background)
        createVar(str, "tab-color", DC.Data.tab.normal.foreground)
        createVar(str, "tab-hover-bg-color", DC.Data.tab.hover.background)
        createVar(str, "tab-hover-color", DC.Data.tab.hover.foreground)
        createVar(str, "selected-tab-color", DC.Data.selectedTab.foreground)
        createVar(str, "selected-tab-bg-color", DC.Data.selectedTab.background)
        createVar(str, "listheader-bg-color", DC.Data.listheader.background)
        createVar(str, "listheader-color", DC.Data.listheader.foreground)
        createVar(str, "listrow-color", DC.Data.listBody.foreground)
        createVar(str, "listrow-bg-color", DC.Data.listBody.background)
        createVar(str, "listaltrow-color", DC.Data.listAltBody.foreground)
        createVar(str, "listaltrow-bg-color", DC.Data.listAltBody.background)
        createVar(str, "selected-row-bg-color", DC.Data.selectedRow.normal.background)
        createVar(str, "selected-row-color", DC.Data.selectedRow.normal.foreground)
        createVar(str, "selected-row-bg-hover-color", DC.Data.selectedRow.hover.background)
        createVar(str, "selected-row-hover-color", DC.Data.selectedRow.hover.foreground)
        createVar(str, "menu-item-bg-color", DC.Data.menuItem.background)
        createVar(str, "menu-item-color", DC.Data.menuItem.foreground)
        createVar(str, "menu-button-color", DC.Data.menuButtonColour)
        createVar(str, "menu_icon_color", DC.Data.menuIconColour)
        createVar(str, "image-characters-color", DC.Data.imageCharactersColour)
        createVar(str, "image-characters-highlight-color", DC.Data.imageCharactershighlightColour)
        createVar(str, "draggable-bg-color", DC.Data.draggable.normal.background)
        createVar(str, "draggable-color", DC.Data.draggable.normal.foreground)

        createVar(str, "draggable-border-color", DC.Data.draggableBorder)
        createVar(str, "draggable-hover-bg-color", DC.Data.draggable.hover.background)
        createVar(str, "draggable-hover-color", DC.Data.draggable.hover.foreground)
        createVar(str, "draggable-hover-border-color", DC.Data.draggableHoverBorder)

        createVar(str, "list-header-icons-color", DC.Data.listHeaderIcons)
        createVar(str, "list-header-icons-hover-color", DC.Data.listHeaderIconsHover)

        createVar(str, "list-header-icons-filter", DC.Data.listHeaderIconsFilter)
        createVar(str, "list-header-icons-hover-filter", DC.Data.listHeaderIconsHoverFilter)
        createVar(str, "list-icons-filter", DC.Data.listIconsFilter)
        createVar(str, "list-icons-hover-filter", DC.Data.listIconsHoverFilter)
        createVar(str, "list-icons", DC.Data.listIcons)
        createVar(str, "list-icons-hover", DC.Data.listIconsHover)

        createVar(str, "checkbox-color", DC.Data.CheckBox)
        createVar(str, "checkbox-color-filter", DC.Data.CheckBoxFilter)

        str.AppendLine("}")
        Return str.ToString
    End Function

    Shared Sub EnsureBrand(clientID)
        ' does the client already have a brand record ?
        If IawDB.execScalar("SELECT Count(1) FROM dbo.client_brand WHERE client_id = @p1", clientID) > 0 Then
            Return
        End If
        ' we do not have a record, so create one
        Dim ClientName As String
        ClientName = IawDB.execScalar("select client_name from dbo.clients where client_id = @p1", clientID)
        IawDB.execNonQuery("INSERT dbo.client_brand(client_id, brand_name, branding, default_brand, logo_name) 
                            VALUES (@p1, @p2, @p3, '1', 'ViviChart.png')",
                            clientID, ClientName, (New ChartBranding).GetData)
    End Sub

    Shared Sub createVar(ByRef str As StringBuilder, varname As String, varvalue As String)
        If String.IsNullOrEmpty(varvalue) Then
            str.AppendLine(String.Format("  // --{0}: Value not set;", varname))
        Else
            str.AppendLine(String.Format("  --{0}: {1};", varname, varvalue))
        End If
    End Sub

    <Serializable>
    Public Class Branding
        Public Property bannerLogo As String
        Public Property logoDimension As Dimension
        Public Property bodyBGType As String
        Public Property bodyBGID As Integer = 0
        Public Property bodyBG As String
        Public Property bannerBar As Colours
        Public Property bannerIconsHover As String
        Public Property contentAreaBG As String
        Public Property link As Hoverable
        Public Property textColour As String
        Public Property inputField As Colours
        Public Property inputFieldBorder As String
        Public Property inputFieldFocusBG As String
        Public Property legend As Colours
        Public Property legendborder As String
        Public Property tab As Hoverable
        Public Property selectedTab As Colours
        Public Property listheader As Colours
        Public Property selectedRow As Hoverable

        Public Property menuButtonColour As String
        Public Property menuItem As Colours
        Public Property menuIconColour As String
        Public Property imageCharactersColour As String
        Public Property imageCharactershighlightColour As String
        Public Property listBody As Colours
        Public Property listAltBody As Colours
        Public Property draggable As Hoverable
        Public Property draggableBorder As String
        Public Property draggableHoverBorder As String
        Public Property listHeaderIcons As String
        Public Property listHeaderIconsHover As String
        Public Property listIcons As String
        Public Property listIconsHover As String
        Public Property listHeaderIconsFilter As String
        Public Property listHeaderIconsHoverFilter As String
        Public Property listIconsFilter As String
        Public Property listIconsHoverFilter As String
        Public Property CheckBox As String
        Public Property CheckBoxFilter As String
        Public Property logoNormalColour As String
        Public Property logoLighterColour As String
        Public Property logoDarkerColour As String
        Public Property advancedBranding As Boolean
    End Class
    <Serializable>
    Public Class Hoverable
        Public Property normal As Colours
        Public Property hover As Colours
    End Class
    <Serializable>
    Public Class Colours
        Public Property foreground As String
        Public Property background As String
        'Public Property background As BackgroundStyle
    End Class
    <Serializable>
    Public Class Dimension
        Public Property width As String
        Public Property height As String
    End Class
    <Serializable>
    Public Class BackgroundStyle
        Public Property backgroundType As String
        Public Property backgroundID As Integer
        Public Property backgroundContent As String
    End Class

End Class
