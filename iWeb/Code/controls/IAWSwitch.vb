Imports System.ComponentModel


Namespace IAW.controls

    <ToolboxData("<{0}:IAWSwitch runat=server></{0}:IAWSwitch>")>
    Public Class IAWSwitch
        Inherits CheckBox

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText() As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return ViewState("TranslateText")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            MyBase.OnPreRender(e)

            If Not Page.ClientScript.IsClientScriptBlockRegistered("toggleSwitch") Then
                Dim script As String = "
                    function toggleSwitch(element, hiddenFieldId, autoPostBack) {
                        if (element.classList.contains('iaw-switch-on')) {
                            element.classList.remove('iaw-switch-on');
                            element.classList.add('iaw-switch-off');
                            document.getElementById(hiddenFieldId).value = 'false';
                        } else {
                            element.classList.remove('iaw-switch-off');
                            element.classList.add('iaw-switch-on');
                            document.getElementById(hiddenFieldId).value = 'true';
                        }

                        if (autoPostBack) {
                            __doPostBack(element.id, 'CheckedChanged');
                        }
                    }
                "

                Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "toggleSwitch", script, True)
            End If
        End Sub

        Protected Overrides Sub Render(writer As HtmlTextWriter)
            ' Retrieve the checkbox's Text and ToolTip properties
            Dim storedText As String = Me.Text
            Dim storedTooltip As String = Me.ToolTip

            ' Set the translated storedText and storedTooltip on the checkbox
            Me.Text = If(TranslateText, ctx.Translate(storedText), storedText)
            Me.ToolTip = If(TranslateText, ctx.Translate(storedTooltip), storedTooltip)

            ' Render the hidden field for storing the state of the checkbox
            Dim hiddenFieldId = Me.ClientID & "_hidden"
            writer.WriteBeginTag("input")
            writer.WriteAttribute("type", "hidden")
            writer.WriteAttribute("id", hiddenFieldId)
            writer.WriteAttribute("value", If(Me.Checked, "true", "false"))
            writer.Write(HtmlTextWriter.SelfClosingTagEnd)

            ' Render the custom on/off switch using a font character
            writer.WriteBeginTag("span")
            writer.WriteAttribute("class", If(Me.Checked, "iaw-font iaw-switch-on", "iaw-font iaw-switch-off"))
            'writer.WriteAttribute("onclick", $"toggleSwitch(this, '{hiddenFieldId}');")
            writer.WriteAttribute("onclick", $"toggleSwitch(this, '{hiddenFieldId}', {Me.AutoPostBack.ToString().ToLower()});")
            writer.Write(HtmlTextWriter.SelfClosingTagEnd)

            ' Reset the Text and ToolTip properties to their original values
            Me.Text = storedText
            Me.ToolTip = storedTooltip
        End Sub

        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)
            If Page.IsPostBack Then
                Me.Checked = ctx.Request.Form(Me.ClientID & "_hidden") = "true"
            End If
        End Sub

    End Class
End Namespace
