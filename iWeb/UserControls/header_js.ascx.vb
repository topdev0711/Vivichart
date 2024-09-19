
Partial Class header_js
    Inherits stub_header_js

    Private _javaScriptFiles As ArrayList
    Private _javaScriptText As ArrayList
    Private _cssFiles As ArrayList
    Private _cssText As ArrayList

    Public Overrides ReadOnly Property javaScriptText() As ArrayList
        Get
            Return _javaScriptText
        End Get
    End Property

    Public Overrides ReadOnly Property javaScriptFiles() As ArrayList
        Get
            Return _javaScriptFiles
        End Get
    End Property
    Public Overrides ReadOnly Property cssFiles() As ArrayList
        Get
            Return _cssFiles
        End Get
    End Property
    Public Overrides ReadOnly Property cssText() As ArrayList
        Get
            Return _cssText
        End Get
    End Property

    Public Sub New()
        Me._javaScriptFiles = New ArrayList
        Me._javaScriptText = New ArrayList
        Me._cssFiles = New ArrayList
        Me._cssText = New ArrayList
    End Sub

    Protected Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender

        For Each s As String In Me.javaScriptFiles
            Dim js As String
            js = ("<script type='text/javascript' src='" + s + "' ></script>") + System.Environment.NewLine
            Me.phJS.Controls.Add(New LiteralControl(js))
        Next

        If Me.javaScriptText.Count > 0 Then
            Me.phJS.Controls.Add(New LiteralControl("<script>" + Environment.NewLine))
            For Each s As String In Me.javaScriptText
                Me.phJS.Controls.Add(New LiteralControl(s + Environment.NewLine))
            Next
            Me.phJS.Controls.Add(New LiteralControl("</script>" + Environment.NewLine))
        End If

        For Each s As String In Me.cssFiles
            Dim css As String
            css = ("<link type='text/css' rel='stylesheet' href='" + s + "' />") + System.Environment.NewLine
            Me.phCss.Controls.Add(New LiteralControl(css))
        Next

        If Me.cssText.Count > 0 Then
            Me.phCss.Controls.Add(New LiteralControl("<style>" + Environment.NewLine))
            For Each s As String In Me.cssText
                Me.phCss.Controls.Add(New LiteralControl(s + Environment.NewLine))
            Next
            Me.phCss.Controls.Add(New LiteralControl("</style>" + Environment.NewLine))
        End If

    End Sub


End Class
