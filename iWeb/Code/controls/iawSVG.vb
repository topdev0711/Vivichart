
Imports System.ComponentModel
Imports System.IO

Namespace IAW.controls

    <ToolboxData("<{0}:iawSVG runat=server></{0}:SvgControl>")>
    Public Class iawSVG
        Inherits WebControl

        Private _svgFileName As String

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The name of the SVG file to display.")>
        Public Property SvgFileName As String
            Get
                Return _svgFileName
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) AndAlso Not value.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) Then
                    _svgFileName = value & ".svg"
                Else
                    _svgFileName = value
                End If
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether tooltip should be translated")>
        Public Property TranslateTooltip As Boolean
            Get
                If ViewState("TranslateTooltip") Is Nothing Then
                    Return True ' default to True if ViewState("TranslateTooltip") is Nothing
                End If
                Return Convert.ToBoolean(ViewState("TranslateTooltip"))
            End Get
            Set(value As Boolean)
                ViewState("TranslateTooltip") = value
            End Set
        End Property
        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub OnInit(ByVal e As EventArgs)
            MyBase.OnInit(e)

            If String.IsNullOrEmpty(Me.CssClass) Then
                Me.CssClass = "iawSVG"
            End If
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
            Dim svgOutput As String = String.Empty
            Dim originalTooltip As String = Me.ToolTip

            ' Read the SVG content from the file
            Dim svgFolderPath As String = HttpContext.Current.Server.MapPath("~/graphics\svg/")
            Dim svgFilePath As String = Path.Combine(svgFolderPath, SvgFileName)

            If File.Exists(svgFilePath) Then
                svgOutput = File.ReadAllText(svgFilePath)
            End If

            ' Insert the title (tooltip) into the SVG if it's in use
            If Not String.IsNullOrEmpty(Me.ToolTip) Then
                Dim tooltip As String = If(TranslateTooltip, ctx.Translate(Me.ToolTip), Me.ToolTip)
                svgOutput = svgOutput.Insert(svgOutput.IndexOf(">") + 1, $"<title>{tooltip}</title>")
            End If

            writer.Write(svgOutput)

            ' Restore the original ToolTip value after rendering
            Me.ToolTip = originalTooltip
        End Sub

    End Class
End Namespace
