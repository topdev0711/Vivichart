Imports System
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Collections
Imports System.Web.Caching
Imports System.Web.UI.WebControls
Imports System.Configuration
Imports System.Collections.Specialized
Imports System.Web.UI.Design
Imports System.ComponentModel
Imports System.Reflection

Namespace IAW.controls
    'control displays a tooltip(onmouseover) based on the innerHTML of a hidden panel
    'controls can either be added to ToolTipControls
    'or text can be added to the [text] property which will displayed as the tooltip
    'text can be used as the rendered item on which the mouseover will show the tooltip
    'or an image may be used, EnableImage is the image shown when there is a tooltip to show
    'DisableImage is the image used when there is nothing (text or controls) to be shown
    'the images must reside in the relevant theme graphics directory

    <Designer(GetType(ToolTipDesigner)), DefaultProperty("Text"), ToolboxData("<{0}:IAWToolTip runat=server></{0}:IAWToolTip>")> _
    Public Class ToolTip
        Inherits System.Web.UI.WebControls.CompositeControl

        Public Enum icon
            information = 0
            validation_error = 1
            help = 2
        End Enum

#Region "private members"
        Private _hyper As HyperLink
        Private _text As String
        Private _ToolTipControls As Panel
        Private _displayText As String
        Private _enableImage As String
        Private _disableImage As String
        Private _sticky As Boolean
        Private _notesWidth As String
        Private _above As Boolean
        Private _bgColor As String
        Private _bgImg As String
        Private _borderWidth As Integer
        Private _borderColor As String
        Private _delay As Integer
        Private _fix As String
        Private _fontColor As String
        Private _fontFace As String
        Private _fontSize As String
        Private _fontWeight As String
        Private _left As Boolean
        Private _offSetX As Integer
        Private _offSetY As Integer
        Private _opaCity As Integer
        Private _padding As Integer
        Private _shadowColor As String
        Private _shadowWidth As Integer
        Private _tstatic As Boolean
        Private _temp As Integer
        Private _textAlign As String
        Private _title As String
        Private _titleColor As String
        Private _icon As ToolTip.icon
#End Region

#Region "boolean attributes - if set then only render them, otherwise let pick default from wz_tooltip.js"
        Private _isStickySet As Boolean
        Private _isNotesWidthSet As Boolean
        Private _isAboveSet As Boolean
        Private _isBgColorSet As Boolean
        Private _isBgImgSet As Boolean
        Private _isBorderWidthSet As Boolean
        Private _isBorderColorSet As Boolean
        Private _isDelaySet As Boolean
        Private _isFixSet As Boolean
        Private _isFontColorSet As Boolean
        Private _isFontFaceSet As Boolean
        Private _isFontSizeSet As Boolean
        Private _isFontWeightSet As Boolean
        Private _isLeftSet As Boolean
        Private _isOffSetXSet As Boolean
        Private _isOffSetYSet As Boolean
        Private _isOpaCitySet As Boolean
        Private _isPaddingSet As Boolean
        Private _isShadowColorSet As Boolean
        Private _isShadowWidthSet As Boolean
        Private _isTstaticSet As Boolean
        Private _isTempSet As Boolean
        Private _isTextAlignSet As Boolean
        Private _isTitleSet As Boolean
        Private _isTitleColorSet As Boolean
#End Region

        Public Sub New()
            _hyper = New HyperLink
            _icon = icon.information
            _displayText = ""
            _enableImage = "about.gif"
            _disableImage = "about_disabled.gif"
            _ToolTipControls = New Panel()
            _sticky = False
            _notesWidth = "300"
            _above = False
            _bgColor = "#FFFFE1"
            _bgImg = ""
            _borderWidth = 1
            _borderColor = "black"
            _delay = 500
            _fix = ""
            _fontColor = "black"
            _fontFace = "arial,helvetica,sans-serif"
            _fontSize = "x-small"
            _fontWeight = "normal"
            _left = False
            _offSetX = 12
            _offSetY = 15
            _opaCity = 100
            _padding = 3
            _shadowColor = "black"
            _shadowWidth = 2
            _tstatic = True
            _temp = 0
            _textAlign = "left"
            _title = ""
            _titleColor = "#ffffff"
            Me.CssClass = "ToolTip"
        End Sub

#Region "properties"
        Public Property imageType() As ToolTip.icon
            Get
                Return Me._icon
            End Get
            Set(ByVal value As ToolTip.icon)
                Select Case value
                    Case icon.information
                        EnableImage = "about.gif"
                        DisableImage = "about_disabled.gif"
                    Case icon.validation_error
                        EnableImage = "tooltip_warning.gif"
                        DisableImage = "tooltip_warning_disabled.gif"
                    Case icon.help
                        EnableImage = "tooltip_help.gif"
                        DisableImage = "tooltip_help_disabled.gif"
                End Select
                Me._icon = value
            End Set
        End Property
        ''' <summary>
        ''' Controls the style for the display text
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DisplayTextStyle() As CssStyleCollection
            Get
                Return Me._hyper.Style
            End Get
        End Property

        ''' <summary>
        ''' The text that appears in the tooltip.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [Text]() As String
            Get
                Return _text
            End Get
            Set(ByVal value As String)
                _text = value
            End Set
        End Property

        Public ReadOnly Property ToolTipControls() As Panel
            Get
                Return Me._ToolTipControls
            End Get
        End Property

        ''' <summary>
        ''' Display Text instead of image icon.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DisplayText() As String
            Get
                Return _displayText
            End Get
            Set(ByVal value As String)
                _displayText = value
            End Set
        End Property
        '/ <summary>
        '/ Image to display when tooltip text has some data.
        '/ 
        '/ If DisplayText is not empty, this is ignored.
        '/ 
        '/ Default: notes.gif
        '/ </summary>
        Public Property EnableImage() As String
            Get
                Return _enableImage
            End Get
            Set(ByVal value As String)
                _enableImage = value
            End Set
        End Property

        '/ <summary>
        '/ Image to display when tooltip text is empty.
        '/ 
        '/ If DisplayText is not empty, this is ignored.
        '/ 
        '/ Default: notesblank.gif
        '/ </summary>
        Public Property DisableImage() As String
            Get
                Return _disableImage
            End Get
            Set(ByVal value As String)
                _disableImage = value
            End Set
        End Property '/ <summary>
        '/ The tooltip stays fixed on its initial position until another tooltip is activated, or the user clicks on the document. Value: true. To enforce the tooltip to disappear after a certain time span, however, you might additionally apply the this.Temp command. 
        '/ 
        '/ Default: false
        '/ </summary>
        Public Property Sticky() As Boolean
            Get
                Return _sticky
            End Get
            Set(ByVal value As Boolean)
                _sticky = value
                _isStickySet = True
            End Set
        End Property

        '/ <summary>
        '/ Notes Tooltip Width
        '/ 
        '/ Default: 300
        '/ </summary>
        Public Property NotesWidth() As String
            Get
                Return _notesWidth
            End Get
            Set(ByVal value As String)
                _notesWidth = value
                _isNotesWidthSet = True
            End Set
        End Property

        '/ <summary>
        '/ Places the tooltip above the mousepointer
        '/ 
        '/ Additionally applying the this.OffSetY command allows to set the vertical distance from the mousepointer
        '/ 
        '/ Default: false
        '/ </summary>
        Public Property Above() As Boolean
            Get
                Return _above
            End Get
            Set(ByVal value As Boolean)
                _above = value
                _isAboveSet = True
            End Set
        End Property

        '/ <summary>
        '/ Background color of the tooltip.
        '/ 
        '/ Default: #e6ecff
        '/ </summary>
        Public Property BgColor() As String
            Get
                Return _bgColor
            End Get
            Set(ByVal value As String)
                _bgColor = value
                _isBgColorSet = True
            End Set
        End Property

        '/ <summary>
        '/ Background image.
        '/ 
        '/ Default: Empty
        '/ </summary>
        Public Property BgImg() As String
            Get
                Return _bgImg
            End Get
            Set(ByVal value As String)
                _bgImg = value
                _isBgImgSet = True
            End Set
        End Property

        '/ <summary>
        '/ Width of tooltip border.
        '/ 
        '/ Default: 1
        '/ </summary>
        Public Property TBorderWidth() As Integer
            Get
                Return _borderWidth
            End Get
            Set(ByVal value As Integer)
                _borderWidth = value
                _isBorderWidthSet = True
            End Set
        End Property

        '/ <summary>
        '/ Border color.
        '/ 
        '/ Default: #003399
        '/ </summary>
        Public Property TBorderColor() As String
            Get
                Return _borderColor
            End Get
            Set(ByVal value As String)
                _borderColor = value
                _isBorderColorSet = True
            End Set
        End Property

        '/ <summary>
        '/ Tooltip shows up after the specified timeout (milliseconds). A behavior similar to that of OS based tooltips. 
        '/ 
        '/ Default: 500
        '/ </summary>
        Public Property Delay() As Integer
            Get
                Return _delay
            End Get
            Set(ByVal value As Integer)
                _delay = value
                _isDelaySet = True
            End Set
        End Property

        '/ <summary>
        '/ Fixes the tooltip to the co-ordinates specified within the square brackets. Useful, for example, if combined with the this.Sticky command. 
        '/ 
        '/ E.g. [200, 400]
        '/ Default: Empty
        '/ </summary>
        Public Property Fix() As String
            Get
                Return _fix
            End Get
            Set(ByVal value As String)
                _fix = value
                _isFixSet = True
            End Set
        End Property

        '/ <summary>
        '/ Font Color.
        '/ 
        '/ Default: #000066
        '/ </summary>

        Public Property FontColor() As String
            Get
                Return _fontColor
            End Get
            Set(ByVal value As String)
                _fontColor = value
                _isFontColorSet = True
            End Set
        End Property

        '/ <summary>
        '/ Font face/family.
        '/ 
        '/ Default: arial,helvetica,sans-serif
        '/ </summary>
        Public Property FontFace() As String
            Get
                Return _fontFace
            End Get
            Set(ByVal value As String)
                _fontFace = value
                _isFontFaceSet = True
            End Set
        End Property

        '/ <summary>
        '/ Font size + unit.
        '/ 
        '/ Default: 11px
        '/ </summary>
        Public Property FontSize() As String
            Get
                Return _fontSize
            End Get
            Set(ByVal value As String)
                _fontSize = value
                _isFontSizeSet = True
            End Set
        End Property

        '/ <summary>
        '/ Font Weight. normal or bold.
        '/ 
        '/ Default: normal
        '/ </summary>
        Public Property FontWeight() As String
            Get
                Return _fontWeight
            End Get
            Set(ByVal value As String)
                _fontWeight = value
                _isFontWeightSet = True
            End Set
        End Property

        '/ <summary>
        '/ Tooltip positioned on the left side of the mousepointer.
        '/ 
        '/ Default: false
        '/ </summary>
        Public Property Left() As Boolean
            Get
                Return _left
            End Get
            Set(ByVal value As Boolean)
                _left = value
                _isLeftSet = True
            End Set
        End Property

        '/ <summary>
        '/ Horizontal offset from mouse-pointer
        '/ 
        '/ Default: 12
        '/ </summary>
        Public Property OffSetX() As Integer
            Get
                Return _offSetX
            End Get
            Set(ByVal value As Integer)
                _offSetX = value
                _isOffSetXSet = True
            End Set
        End Property

        '/ <summary>
        '/ Vertical offset from mouse-pointer
        '/ 
        '/ Default: 15
        '/ </summary>
        Public Property OffSetY() As Integer
            Get
                Return _offSetY
            End Get
            Set(ByVal value As Integer)
                _offSetY = value
                _isOffSetYSet = True
            End Set
        End Property

        '/ <summary>
        '/ Transparency of tooltip. Opacity is the opposite of transparency. Value must be a number between 0 (fully transparent) and 100 (opaque, no transparency). Not (yet) supported by Opera.
        '/ 
        '/ Default: 100
        '/ </summary>
        Public Property OpaCity() As Integer
            Get
                Return _opaCity
            End Get
            Set(ByVal value As Integer)
                _opaCity = value
                _isOpaCitySet = True
            End Set
        End Property

        '/ <summary>
        '/ Inner spacing, i.e. the spacing between border and content, for instance text or image(s). 
        '/ 
        '/ Default: 3
        '/ </summary>
        Public Property Padding() As Integer
            Get
                Return _padding
            End Get
            Set(ByVal value As Integer)
                _padding = value
                _isPaddingSet = True
            End Set
        End Property

        '/ <summary>
        '/ Creates shadow with the specified color.
        '/ 
        '/ Default: Empty
        '/ </summary>
        Public Property ShadowColor() As String
            Get
                Return _shadowColor
            End Get
            Set(ByVal value As String)
                _shadowColor = value
                _isShadowColorSet = True
            End Set
        End Property

        '/ <summary>
        '/ Creates shadow with the specified width (offset). Shadow color is automatically set to '#cccccc' (light grey)
        '/ 
        '/ Default: 0
        '/ </summary>
        Public Property ShadowWidth() As Integer
            Get
                Return _shadowWidth
            End Get
            Set(ByVal value As Integer)
                _shadowWidth = value
                _isShadowWidthSet = True
            End Set
        End Property

        '/ <summary>
        '/ Like OS-based tooltips, the tooltip doesn't follow the movements of the mouse-pointer.
        '/ 
        '/ Default: true
        '/ </summary>
        Public Property Tstatic() As Boolean
            Get
                Return _tstatic
            End Get
            Set(ByVal value As Boolean)
                _tstatic = value
                _isTstaticSet = True
            End Set
        End Property

        '/ <summary>
        '/ Specifies a time span in milliseconds after which the tooltip disappears, even if the mousepointer is still on the concerned HTML element, or if the this.T_STICKY command has been applied. Values less than or equal to 0 make the tooltip behave normally as if no time span had been specified. 
        '/ 
        '/ Default: 0
        '/ </summary>
        Public Property Temp() As Integer
            Get
                Return _temp
            End Get
            Set(ByVal value As Integer)
                _temp = value
                _isTempSet = True
            End Set
        End Property

        '/ <summary>
        '/ Aligns the text of both the title and the body of the tooltip. Values must be included in single quotes and can be either 'right', 'justify' or 'left', the latter being unnecessary since it is the preset default value.
        '/ 
        '/ Default: left
        '/ </summary>
        Public Property TextAlign() As String
            Get
                Return _textAlign
            End Get
            Set(ByVal value As String)
                _textAlign = value
                _isTextAlignSet = True
            End Set
        End Property

        ''' <summary>
        ''' Title. Text in single quotes. Background color is automatically the same as the border color. 
        ''' 
        ''' Default: Empty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title() As String
            Get
                Return _title
            End Get
            Set(ByVal value As String)
                _title = value
                _isTitleSet = True
            End Set
        End Property

        '/ <summary>
        '/ Color of title text. 
        '/ 
        '/ Default: #ffffff
        '/ </summary>
        Public Property TitleColor() As String
            Get
                Return _titleColor
            End Get
            Set(ByVal value As String)
                _titleColor = value
                _isTitleColorSet = True
            End Set
        End Property
#End Region

        Public Overrides Sub RenderBeginTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            'MyBase.RenderBeginTag(writer)
        End Sub

        Public Overrides Sub RenderEndTag(ByVal writer As System.Web.UI.HtmlTextWriter)
            'MyBase.RenderEndTag(writer)
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As EventArgs)
            MyBase.OnLoad(e)

            If Not (Page Is Nothing) Then
            End If

            ' Register the control with the page's postback mechanism, or
            ' the control will not update on Postback
            ' Page.RegisterRequiresPostBack(this);				 
            If Not Page.IsPostBack Then
                'Create child controls
                EnsureChildControls()
            End If

        End Sub 'OnLoad


        Protected Overrides Sub CreateChildControls()

            Me._ToolTipControls.ID = "txtNotes"
            Me._ToolTipControls.Style.Add("display", "none")

            _hyper.NavigateUrl = "javascript:void(0)"
            If [Text] <> "" Or Me.ToolTipControls.Controls.Count > 0 Then

                Dim onOverScript As String = ""
                onOverScript += "this.T_BGCOLOR='" + Me.BgColor + "';"
                onOverScript += "this.T_BORDERWIDTH=" + Me._borderWidth.ToString() + ";"
                onOverScript += "this.T_BORDERCOLOR='" + Me._borderColor + "';"
                onOverScript += "this.T_SHADOWCOLOR='" + Me.ShadowColor + "';"
                onOverScript += "this.T_SHADOWWIDTH=" + Me.ShadowWidth.ToString() + ";"
                onOverScript += "this.T_FONTCOLOR='" + Me.FontColor + "';"
                onOverScript += "this.T_FONTSIZE='" + Me.FontSize + "';"

                If _isStickySet Then onOverScript += "this.T_STICKY=1;"
                If _isNotesWidthSet Then onOverScript += "this.T_WIDTH=" + Me.NotesWidth + ";"
                If _isAboveSet Then onOverScript += "this.T_ABOVE=" + Me.Above.ToString().ToLower() + ";"
                If _isBgImgSet Then onOverScript += "this.T_BGIMG='" + Me.BgImg + "';"
                If _isDelaySet Then onOverScript += "this.T_DELAY=" + Me.Delay.ToString() + ";"
                If _isFixSet Then onOverScript += "this.T_FIX=" + Me.Fix + ";"


                If _isFontFaceSet Then onOverScript += "this.T_FONTFACE='" + Me.FontFace + "';"

                If _isFontWeightSet Then onOverScript += "this.T_FONTWEIGHT='" + Me.FontWeight + "';"
                If _isLeftSet Then onOverScript += "this.T_LEFT=" + Me.Left.ToString().ToLower() + ";"
                If _isOffSetXSet Then onOverScript += "this.T_OFFSETX=" + Me.OffSetX.ToString() + ";"
                If _isOffSetYSet Then onOverScript += "this.T_OFFSETY=" + Me.OffSetY.ToString() + ";"
                If _isOpaCitySet Then onOverScript += "this.T_OPACITY=" + Me.OpaCity.ToString() + ";"
                If _isPaddingSet Then onOverScript += "this.T_PADDING=" + Me.Padding.ToString() + ";"
                If _isTstaticSet Then onOverScript += "this.T_STATIC=" + Me.Tstatic.ToString().ToLower() + ";"
                If _isTempSet Then onOverScript += "this.T_TEMP=" + Me.Temp.ToString() + ";"
                If _isTextAlignSet Then onOverScript += "this.T_TEXTALIGN='" + Me.TextAlign + "';"
                If _isTitleSet Then onOverScript += "this.T_TITLE='" + Me.Title + "';"
                If _isTitleColorSet Then onOverScript += "this.T_TITLECOLOR='" + Me.TitleColor + "';"

                _hyper.Attributes.Add("onmouseover", onOverScript + "return escape(this.previousSibling.innerHTML)")

            End If '
            Me._ToolTipControls.TabIndex = -1
            _hyper.TabIndex = -1
            _hyper.CssClass = Me.CssClass
            Me.Controls.Add(Me._ToolTipControls)
            Me.Controls.Add(_hyper)
        End Sub 'CreateChildControls


        '/ <summary>
        '/ Print tooltip .JS file here.
        '/ </summary>
        '/ <param name="e"></param>
        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            MyBase.OnPreRender(e)

            If DisplayText <> "" Then
                Dim lit As New Literal()
                lit.Text = DisplayText
                _hyper.Controls.Add(lit)
            Else
                Dim img As New Image()
                img.ID = "imgNotes"
                img.TabIndex = -1

                If [Text] <> "" Or Me.ToolTipControls.Controls.Count > 0 Then
                    img.ImageUrl = ctx.themeGraphicsDir + EnableImage
                Else
                    img.ImageUrl = ctx.themeGraphicsDir + DisableImage
                End If
                _hyper.Controls.Add(img)
            End If

            Dim p As Panel = CType(Me.FindControl("txtNotes"), Panel)
            p.Controls.Add(New LiteralControl(Me.Text))

            'As per the guidelines given by wztooltip.js, js file must be included after all the tooltip used.
            'If Not Me.Page.ClientScript.IsStartupScriptRegistered("wztooltip") Then
            ScriptManager.RegisterStartupScript(Me.Page, Me.GetType, "wztooltip", "<script src='" + ctx.virtualDir + "/js/wz_tooltip.js' type='text/javascript'></script>", False)
            'End If

            'makes the tooltip work when part of an asych postback        
            Dim SM As ScriptManager = ScriptManager.GetCurrent(Me.Page)
            If SM.IsInAsyncPostBack Then
                ScriptManager.RegisterStartupScript(Me.Page, Me.GetType(), "wztooltipASYCH", "tt_Init();", True)
            End If

        End Sub 'OnPreRender
    End Class 'Notes

    NotInheritable Class ToolTipDesigner
        Inherits System.Web.UI.Design.ControlDesigner

        Public Overrides Function GetDesignTimeHtml() As String
            Try
                Dim notes As ToolTip = CType(Component, ToolTip)
                Return "<img src='" + ctx.themeGraphicsDir + notes.EnableImage + "' />"
            Catch ex As Exception
                Return GetErrorDesignTimeHtml(ex)
            End Try
        End Function 'GetDesignTimeHtml

        Protected Overrides Function GetErrorDesignTimeHtml(ByVal e As Exception) As String
            Return CreatePlaceHolderDesignTimeHtml(e.Message)
        End Function 'GetErrorDesignTimeHtml

    End Class 'NotesDesigner

End Namespace