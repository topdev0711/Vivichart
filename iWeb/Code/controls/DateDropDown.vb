
Imports AjaxControlToolkit

Namespace IAW.controls

    <ValidationProperty("ValidationDate")>
    Public Class DateDropDown
        Inherits System.Web.UI.WebControls.CompositeControl

        Private _initialDate As Date
        Protected _textbox As New TextBox
        Protected _calendarImage As New Image
        Protected _calendarPopup As New CalendarExtender
        Protected _rfv As New RequiredFieldValidator

        Public Event DateChanged As EventHandler

#Region "properties"
        Public Property CurrentDate() As Date
            Get
                Dim d As Date
                If Date.TryParse(Me.ControlTextbox.Text, d) Then
                    Return d
                End If
                Return #1/1/1900#
            End Get
            Set(ByVal value As Date)
                If Not value = Nothing AndAlso value <> #1/1/1900# Then
                    Me.ControlTextbox.Text = value.ToString("yyyy-MM-dd")
                Else
                    Me.ControlTextbox.Text = ""
                End If
            End Set
        End Property
        ''' <summary>
        ''' If true the control will postback when the date value in the textbox has changed
        ''' </summary>
        ''' <value></value>
        ''' <remarks></remarks>
        Public WriteOnly Property AutoPostback() As Boolean
            Set(ByVal value As Boolean)
                Me.ControlTextbox.AutoPostBack = value
            End Set
        End Property
        Public WriteOnly Property Protect() As Boolean
            Set(ByVal Value As Boolean)
                Dim ls_setting As String = "false"
                If Value Then ls_setting = "true"
                _textbox.Attributes.Add("readonly", ls_setting)
            End Set
        End Property
        Public ReadOnly Property ValidationDate() As String
            Get
                If String.IsNullOrEmpty(Me._textbox.Text.Trim) Then
                    Return New Date(1900, 1, 1).ToShortDateString
                End If

                Dim d As Date
                If Not Date.TryParse(Me._textbox.Text, d) Then
                    Return "Invalid Date"
                ElseIf d <> #1/1/1900# And (d < #1/1/1901# Or d > #1/1/2100#) Then
                    Return "Invalid Date"
                Else
                    Return d.ToShortDateString()
                End If
            End Get
        End Property
        Public Overridable Property txtEnabled() As Boolean
            Get
                Dim obj2 As Object = Me.ViewState.Item("txtEnabled")
                If (Not obj2 Is Nothing) Then
                    Return CBool(obj2)
                End If
                Return True
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState.Item("txtEnabled") = value
                Me.EnsureChildControls()
                Me._textbox.Attributes.Remove("readonly")
                Me._textbox.Attributes.Add("readonly", "true")
            End Set
        End Property
        Public ReadOnly Property DBDate() As String
            Get
                Return Me.CurrentDate.ToString(ctx.DBDateFormat)
            End Get
        End Property
        Public Overrides Property Enabled() As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(ByVal value As Boolean)
                MyBase.Enabled = value
                'Me.CalendarPopup.Enabled=value
            End Set
        End Property
        ''' <summary>
        ''' The textbox that displays the date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ControlTextbox() As TextBox
            Get
                Return Me._textbox
            End Get
            Set(ByVal value As TextBox)
                Me._textbox = value
            End Set
        End Property
        ''' <summary>
        ''' The calendar image that shows the date popup
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CalendarImage() As Image
            Get
                Return Me._calendarImage
            End Get
            Set(ByVal value As Image)
                Me._calendarImage = value
            End Set
        End Property
        ''' <summary>
        ''' The popup calendar
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        'Public Property CalendarPopup() As CalendarExtender
        '    Get
        '        Return Me._calendarPopup
        '    End Get
        '    Set(ByVal value As CalendarExtender)
        '        Me._calendarPopup = value
        '    End Set
        'End Property
        Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
            Get
                Return HtmlTextWriterTag.Span
            End Get
        End Property
        ''' <summary>
        ''' The required field validator for the date
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FieldValidator() As RequiredFieldValidator
            Get
                Return Me._rfv
            End Get
            Set(ByVal value As RequiredFieldValidator)
                Me._rfv = value
            End Set
        End Property

        Public Property IsRequiredField() As Boolean
            Get
                Dim o As Object = Me.ViewState("IsRequiredField")
                If o Is Nothing Then
                    Return False
                End If
                Return DirectCast(o, Boolean)
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState("IsRequiredField") = value
            End Set
        End Property

        ''' <summary>
        ''' The validation group
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ValidationGroup() As String
            Get
                Dim o As Object = Me.ViewState("ValidationGroup")
                If o Is Nothing Then
                    Return String.Empty
                End If
                Return DirectCast(o, String)
            End Get
            Set(ByVal value As String)
                Me.ViewState("ValidationGroup") = value
                Me.FieldValidator.ValidationGroup = value
            End Set
        End Property
#End Region

#Region "initialise"
        Public Sub New(ByVal InitialDate As Date)
            Me._initialDate = InitialDate
        End Sub

        Public Sub New()
        End Sub

        Protected Overrides Sub OnInit(ByVal e As System.EventArgs)
            MyBase.OnInit(e)

            Me.Style.Add("white-space", "nowrap")

            AddHandlers()

            With Me.ControlTextbox
                .CssClass = "dateInput"
                .ID = "txtDate"
                .TextMode = TextBoxMode.Date
                .Width = New Unit(75)
            End With

            With Me.CalendarImage
                .ID = "imgDate"
                .ImageUrl = ctx.GraphicsDir + "1px.gif"
                .ImageAlign = ImageAlign.AbsMiddle
                .AlternateText = "Show Calendar"
                .CssClass = "IconPic Icon20 IconCalendar"
                .Style.Add("cursor", "pointer")
                .Attributes.Add("style", "vertical-align:top;")
                .ToolTip = "Calendar"
                .BorderStyle = WebControls.BorderStyle.None
            End With

            'Me.CalendarPopup.ID = "calExt"
            'Me.CalendarPopup.Format = ctx.DateFormat

            If Not Me._initialDate = Nothing AndAlso Not Date.TryParse(Me.ControlTextbox.Text, Me._initialDate) Then
                Me.CurrentDate = Me._initialDate
            End If

            Me.FieldValidator.ID = "txtDate_rfv"

            With Me.FieldValidator
                .EnableClientScript = True
                .Display = ValidatorDisplay.Dynamic
                .ErrorMessage = "Please enter a date"
            End With

            GenerateTextBox(Me)
            'If Enabled Then
            '    GenerateImageButton(Me)
            '    GenerateCalendarExtender(Me)
            'End If
        End Sub
#End Region

#Region "Helper Functions"
        Protected Sub AddHandlers()
            AddHandler Me.ControlTextbox.TextChanged, AddressOf OnDateChanged
        End Sub
#End Region

#Region "Create Controls"
        Protected Sub GenerateTextBox(ByVal container As Control)


            container.Controls.Add(Me.ControlTextbox)
        End Sub

        'Protected Sub GenerateImageButton(ByVal container As Control)
        '    'need to add literal otherwise textbox and image do not line up in IE8
        '    container.Controls.Add(New LiteralControl("&#160;"))
        '    container.Controls.Add(Me.CalendarImage)
        '    If Me.IsRequiredField Then
        '        Me.FieldValidator.ControlToValidate = Me.ControlTextbox.ID
        '        container.Controls.Add(Me.FieldValidator)
        '    End If
        'End Sub

        'Protected Sub GenerateCalendarExtender(ByVal container As Control)
        '    With Me.CalendarPopup
        '        .TargetControlID = Me.ControlTextbox.ID
        '        .PopupButtonID = Me.CalendarImage.ID
        '    End With
        '    container.Controls.Add(Me.CalendarPopup)
        'End Sub
#End Region

#Region "raise events"
        Protected Overridable Sub OnDateChanged(ByVal sender As Object, ByVal e As EventArgs)
            RaiseEvent DateChanged(Me, e)
        End Sub
#End Region

        Private Sub ValidateDate(ByVal sender As Object, ByVal e As ServerValidateEventArgs)
            If CurrentDate = Nothing Then
                e.IsValid = False
            End If
        End Sub

        Public Overrides Function ToString() As String
            If CurrentDate = Nothing Then
                Return String.Empty
            Else
                If Me.ControlTextbox.Text.Trim = String.Empty Or Me.CurrentDate = #1/1/1900# Then
                    Return String.Empty
                End If

                Return Me.CurrentDate.ToShortDateString()
            End If
        End Function

    End Class

End Namespace