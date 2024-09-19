Imports System.IO
Imports System.ComponentModel

<ToolboxData("<{0}:wuc_help runat=""server"" />")>
    <Description("Help Control")>
    Partial Class wuc_help
        Inherits System.Web.UI.UserControl

        Public Sub New()
        End Sub

        Public Property Reference As String
            Get
                If ViewState("ref") Is Nothing Then
                    ViewState("ref") = ""
                End If
                Return ViewState("ref")
            End Get
            Set(value As String)
                ViewState("ref") = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return ViewState("text")
            End Get
            Set(ByVal Value As String)
                ViewState("text") = SawLang.Translate(Value)
            End Set
        End Property

        Public Property URL() As String
            Get
                Return ViewState("url")
            End Get
            Set(ByVal Value As String)
                ViewState("url") = Value
            End Set
        End Property

        Public Property ToolTip() As String
            Get
                Return ViewState("tooltip")
            End Get
            Set(ByVal Value As String)
                ViewState("tooltip") = Value
            End Set
        End Property

        Public Property CssClass() As String
            Get
                Return ViewState("cssclass")
            End Get
            Set(ByVal Value As String)
                ViewState("cssclass") = Value
            End Set
        End Property

        Public ReadOnly Property BaseURL As String
            Get
                Return Request.Url.GetLeftPart(UriPartial.Authority) + Page.ResolveUrl("~/Help/")
            End Get
        End Property

        Private Sub wuc_help_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                URL = "#"
            End If
        'If String.IsNullOrEmpty(Text) Then Text = "?"
        If String.IsNullOrEmpty(ToolTip) Then ToolTip = SawLang.Translate("::LT_A0085")  ' Help
    End Sub

        Private Sub wuc_help_PreRender(sender As Object, e As EventArgs) Handles Me.PreRender

            ' see which help file exists starting with client versions
            '    createURL(withClient,withRole)
            Dim HtmlURL As String
            HtmlURL = createURL(True, True)
            If Not File.Exists(Server.MapPath("~/Help/") + HtmlURL) Then
                HtmlURL = createURL(True, False)
                If Not File.Exists(Server.MapPath("~/Help/") + HtmlURL) Then
                    HtmlURL = createURL(False, True)
                    If Not File.Exists(Server.MapPath("~/Help/") + HtmlURL) Then
                        HtmlURL = createURL(False, False)
                        If Not File.Exists(Server.MapPath("~/Help/") + HtmlURL) Then
                            Me.Visible = False
                            Return
                        End If
                    End If
                End If
            End If

            URL = BaseURL + HtmlURL
            Dim opn As String = "openHelp('" + URL + "');"
            labHelp.Text = SawLang.Translate(Text)
            If String.IsNullOrEmpty(CssClass) Then CssClass = "divHelpClass"
            divHelp.CssClass = CssClass
            divHelp.ToolTip = ToolTip
            divHelp.Attributes.Add("onclick", opn)

        End Sub

        ''' <summary>
        ''' Build a url
        '''   [process].html
        '''   [process]-[reference].html
        '''   [process]-[reference]-[role].html
        '''   [clientid]/[process].html
        '''   [clientid]/[process]-[reference].html
        '''   [clientid]/[process]-[reference]-[role].html
        ''' </summary>
        ''' <param name="withClient">Prefix the url with the clientid as a folder name</param>
        ''' <param name="withRole">Include the role in the url</param>
        ''' <returns></returns>
        Private Function createURL(withClient As Boolean, withRole As Boolean) As String
            Dim list As New List(Of String)
            Dim FullPath As String = String.Empty
            If withClient Then FullPath = ctx.clientID + "/"

            list.Add(ctx.process)
            If Reference.Trim <> "" Then list.Add(Reference.Trim)
            If withRole Then list.Add(ctx.role)

            FullPath += String.Join("-", list.ToArray) + ".html"

            Return FullPath
        End Function

    End Class

