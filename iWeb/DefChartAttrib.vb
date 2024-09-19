
Imports Newtonsoft.Json

<Serializable>
Public Class ChartAttrib

    Public Data As Attrib

    Public Sub New(attr As String)
        'Data = GetDefault()
        SetData(attr)
    End Sub

    Public Sub New()
        Me.Data = GetDefault()
    End Sub

    Public Function GetData() As String
        Return JsonConvert.SerializeObject(Me.Data, Formatting.None)
    End Function

    Public Sub SetData(ByVal jsonString As String)
        If String.IsNullOrEmpty(jsonString) Then
            Me.Data = GetDefault()
        Else
            Try
                Me.Data = JsonConvert.DeserializeObject(Of Attrib)(jsonString)
            Catch ex As Exception
                Me.Data = GetDefault()
            End Try
        End If
    End Sub

    Public Function GetDefault() As Attrib
        ' Create the Attrib object
        Dim attr As New Attrib With {
            .backgroundType = "SolidColour",
            .backgroundContent = "rgb(255,255,255)",
            .backgroundID = 0,
            .chartDirection = 90,
            .corners = "Rectangle",
            .showShadow = False,
            .shadowColour = "#000000",
            .maxWidth = 500,
            .nodeWidth = 200,
            .maxHeight = 200,
            .nodeHeight = 100,
            .showImages = True,
            .imagesApplicable = True,
            .imagePosition = "inline",
            .imageShape = "",
            .imageHeight = 100,
            .canOverride = True,
            .node = New NodeColours With {
                .foreground = "#000000",
                .background = "#FFFFFF",
                .border = "#000000",
                .textBackground = "",
                .textBackgroundBlock = False,
                .iconColour = "#000000",
                .iconHover = "#000080"
            },
            .highlight = New Colours With {
                .foreground = "#FFFFFF",
                .background = "#FF0000",
                .border = "#000000"
            },
            .tooltip = New Colours With {
                .foreground = "#000000",
                .background = "#D7E5F0",
                .border = "#000000"
            },
            .link = New LinkType With {
                .type = "solid",
                .colour = "#000000",
                .hover = "#8080ff",
                .width = 1.5,
                .tooltip = New Colours With {
                    .foreground = "#000000",
                    .background = "#D7E5F0",
                    .border = "#000000"
                }
            },
            .lines = New Lines With {
                .line1 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                },
                .line2 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                },
                .line3 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                },
                .line4 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                },
                .line5 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                },
                .line6 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
                }
            },
            .Buttons = New ButtonInfo With {
                .position = "left",
                .font = "8pt Arial",
                .shape = "Rectangle",
                .normal = New Colours With {
                    .foreground = "#000000",
                    .background = "#ffffff",
                    .border = "#000000"
                },
                .hover = New Colours With {
                    .foreground = "#000080",
                    .background = "#ffffff",
                    .border = "#000000"
                },
                .detailText = ctx.Translate("::LT_A0172"),  ' Details
                .noteText = ctx.Translate("::LT_A0077")     ' Note
            }
        }
        Return attr
    End Function

    <Serializable>
    Public Class Attrib
        Public Property backgroundType As String = "SolidColour"
        Public Property backgroundContent As String = "rgb(255,255,255)"
        Public Property backgroundID As Integer = 0
        Public Property chartDirection As Integer = 90
        Public Property corners As String = "Rectangle"
        Public Property showShadow As Boolean = False
        Public Property shadowColour As String = "#000000"
        Public Property maxWidth As Integer = 500
        Public Property nodeWidth As Integer = 250
        Public Property maxHeight As Integer = 200
        Public Property nodeHeight As Integer = 100
        Public Property showImages As Boolean = True
        Public Property imagesApplicable As Boolean = True
        Public Property imagePosition As String = "inline"
        Public Property imageShape As String = ""
        Public Property imageHeight As Integer = 100
        Public Property canOverride As Boolean = True
        Public Property node As New NodeColours With {
            .foreground = "#000000",
            .background = "#FFFFFF",
            .border = "#000000",
            .textBackground = "",
            .textBackgroundBlock = False,
            .iconColour = "#000000",
            .iconHover = "#000080"
        }
        Public Property highlight As New Colours With {
            .foreground = "#FFFFFF",
            .background = "#FF0000",
            .border = "#000000"
        }
        Public Property tooltip As New Colours With {
            .foreground = "#000000",
            .background = "#D7E5F0",
            .border = "#000000"
        }
        Public Property link As New LinkType With {
                .type = "solid",
                .colour = "#000000",
                .hover = "#8080ff",
                .width = 1.5,
                .tooltip = New Colours With {
                    .foreground = "#000000",
                    .background = "#D7E5F0",
                    .border = "#000000"
                }
        }
        Public Property lines As New Lines With {
            .line1 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            },
            .line2 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            },
            .line3 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            },
            .line4 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            },
            .line5 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            },
            .line6 = New Line With {
                    .font = "arial",
                    .fontSize = 12,
                    .colour = "",
                    .bg_colour = "",
                    .bold = False,
                    .italic = False,
                    .underline = False,
                    .align = "left"
            }
        }
        Public Property Buttons As New ButtonInfo With {
            .position = "left",
            .font = "8pt Arial",
            .shape = "Rectangle",
            .normal = New Colours With {
                .foreground = "#000000",
                .background = "#ffffff",
                .border = "#000000"
            },
            .hover = New Colours With {
                .foreground = "#000080",
                .background = "#ffffff",
                .border = "#000000"
            },
            .detailText = ctx.Translate("::LT_A0172"),  ' Details
            .noteText = ctx.Translate("::LT_A0077")    ' Note
        }
    End Class

    <Serializable>
    Public Class Colours
        Public Property foreground As String = "#000000"
        Public Property background As String = "#FFFFFF"
        Public Property border As String = "#000000"
    End Class

    <Serializable>
    Public Class NodeColours
        Public Property foreground As String = "#000000"
        Public Property background As String = "#FFFFFF"
        Public Property border As String = "#000000"
        Public Property textBackground As String = ""
        Public Property textBackgroundBlock As Boolean = False
        Public Property iconColour As String = "#000000"
        Public Property iconHover As String = "#000080"
    End Class

    <Serializable>
    Public Class LinkType
        Public Property type As String = "solid"
        Public Property colour As String = "#000000"
        Public Property hover As String = "#8080ff"
        Public Property width As Decimal = 1.5
        Public Property tooltip As New Colours With {
            .foreground = "#000000",
            .background = "#D7E5F0",
            .border = "#000000"
        }
    End Class

    <Serializable>
    Public Class Lines
        Public Property line1 As Line
        Public Property line2 As Line
        Public Property line3 As Line
        Public Property line4 As Line
        Public Property line5 As Line
        Public Property line6 As Line
    End Class

    <Serializable>
    Public Class Line
        Public Property font As String = "Arial"
        Public Property fontSize As String = "12"
        Public Property bold As Boolean = False
        Public Property italic As Boolean = False
        Public Property underline As Boolean = False
        Public Property colour As String = ""
        Public Property bg_colour As String = ""
        Public Property align As String = "left"
    End Class

    <Serializable>
    Public Class ButtonInfo
        Public Property position As String = "left"
        Public Property font As String = "8pt Arial"
        Public Property shape As String = "Rectangle"
        Public Property normal As New Colours With {
            .foreground = "#000000",
            .background = "#FFFFFF",
            .border = "#000000"
        }
        Public Property hover As New Colours With {
            .foreground = "#000080",
            .background = "#FFFFFF",
            .border = "#000000"
        }
        Public Property detailText As String = ctx.Translate("::LT_A0172")  ' Details
        Public Property noteText As String = ctx.Translate("::LT_A0077")    ' Note
    End Class

End Class
