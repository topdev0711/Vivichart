
Namespace IAW.controls

    <ToolboxData("<{0}:message runat=server></{0}:message>")> _
    Public Class IawMessage
        Inherits CompositeControl

        Public Enum MessageStyle
            warning = 1
            information = 2
            [error] = 3
        End Enum

        Public Enum IconStyle
            warning = 1
            information = 2
            [error] = 3
        End Enum

        Private _tbl As New Table
        Private _tr As New TableRow
        Private _tdMessage As New TableCell
        Private _tdIcon As New TableCell
        Private _message_label As New Label
        Private _tblCssClass As String
        Private _tdCssClass As String
        Private _labelCssClass As String
        Private _messageType As IawMessage.MessageStyle
        Private _showIcon As Boolean
        Private _iconStyle As IawMessage.IconStyle

#Region "properties"

        Public Property TranslateText As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return Convert.ToBoolean(ViewState("TranslateText"))
            End Get
            Set(value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        Public WriteOnly Property orgMessageText As String
            Set(value As String)
            End Set
        End Property

        Public Property VisibleOnClient() As Boolean
            Get
                If ViewState("_visibleOnClient") Is Nothing Then Return True
                Return ViewState("_visibleOnClient")
            End Get
            Set(ByVal value As Boolean)
                ViewState("_visibleOnClient") = value
            End Set
        End Property

        Public Property tdCssClass() As String
            Get
                Return Me._tdCssClass
            End Get
            Set(ByVal value As String)
                Me._tdCssClass = value
            End Set
        End Property

        Public Property labelCssClass() As String
            Get
                Return Me._labelCssClass
            End Get
            Set(ByVal value As String)
                Me._labelCssClass = value
            End Set
        End Property

        Public Property ShowIcon() As Boolean
            Get
                Return Me._showIcon
            End Get
            Set(ByVal value As Boolean)
                Me._showIcon = value
            End Set
        End Property

        Public Property Icon() As IawMessage.IconStyle
            Get
                Return Me._iconStyle
            End Get
            Set(ByVal value As IawMessage.IconStyle)
                Me._iconStyle = value
                Select Case Me._iconStyle
                    Case IconStyle.error
                        _tdIcon.CssClass = "errorIcon"
                    Case IconStyle.information
                        _tdIcon.CssClass = "informationIcon"
                    Case IconStyle.warning
                        _tdIcon.CssClass = "warningIcon"
                End Select
            End Set
        End Property

        Public Property MessageType() As IawMessage.MessageStyle
            Get
                Return Me._messageType
            End Get
            Set(ByVal value As IawMessage.MessageStyle)
                'set the styles when the message type is changed
                Select Case value
                    Case MessageStyle.error
                        _tblCssClass = "tblIawMessage"
                        _tdCssClass = "errorMessage"
                        _labelCssClass = "errorMessage"
                        _iconStyle = IconStyle.error
                        _tdIcon.CssClass = "errorIcon"

                    Case MessageStyle.information
                        _tblCssClass = "tblIawMessage"
                        _tdCssClass = "infoMessage"
                        _labelCssClass = "infoMessage"
                        _iconStyle = IconStyle.information
                        _tdIcon.CssClass = "informationIcon"

                    Case MessageStyle.warning
                        _tblCssClass = "tblIawMessage"
                        _tdCssClass = "warnMessage"
                        _labelCssClass = "warnMessage"
                        _iconStyle = IconStyle.warning
                        _tdIcon.CssClass = "warningIcon"

                End Select
            End Set
        End Property

        Public ReadOnly Property message() As String
            Get
                Return Me._message
            End Get
        End Property

        Protected Property _message() As String
            Get
                Dim txt As String = Me.ViewState("_message")
                If String.IsNullOrEmpty(txt) Then
                    Me.ViewState("_message") = String.Empty
                End If
                Return Me.ViewState("_message")
            End Get
            Set(ByVal value As String)
                Me.ViewState("_message") = value
            End Set
        End Property

        Protected Overrides ReadOnly Property TagKey() As System.Web.UI.HtmlTextWriterTag
            Get
                Return HtmlTextWriterTag.Div
            End Get
        End Property

        Public WriteOnly Property QMessage() As String
            Set(ByVal value As String)
                addQMessage(value)
            End Set
        End Property

        ''' <summary>
        ''' Adds a message to the control, message IS TRANSLATED
        ''' If no translation is required use addMessage()
        ''' </summary>
        ''' <value></value>
        ''' <remarks></remarks>
        Public WriteOnly Property MessageText() As String
            Set(ByVal value As String)
                addMessage(value)
            End Set
        End Property

#End Region

        Public Sub New()
            _message = String.Empty
            'default to error message
            Me.MessageType = MessageStyle.error
        End Sub

        Protected Overrides Sub CreateChildControls()
            If Not VisibleOnClient Then MyBase.Style("display") = "none"
            MyBase.CreateChildControls()
            Me.Controls.Add(_tbl)
            'add the icon cell
            _tr.Cells.Add(_tdIcon)

            'add the message cell
            _tdMessage.Controls.Add(_message_label)
            _tr.Cells.Add(_tdMessage)

            _tbl.Rows.Add(_tr)
        End Sub

        Public Sub addQMessage(ByVal messageID As String)
            'remove ::, if exists 
            messageID = messageID.Replace("::", "")
            Me._message = SawUtil.GetErrMsg(messageID)
        End Sub

        Public Sub addMessage(ByVal aMessage As String)
            If TranslateText Then
                Me._message = SawLang.Translate(aMessage)
            Else
                Me._message = aMessage
            End If
        End Sub

        Private Sub IawMessage_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.PreRender
            'set styles
            _tbl.CssClass = Me._tblCssClass
            _tdMessage.CssClass = Me._tdCssClass
            _message_label.CssClass = Me._labelCssClass
            _message_label.Text = Me._message

            If Me.ShowIcon Then
                _tdIcon.Visible = True
            Else
                _tdIcon.Visible = False
            End If

            If VisibleOnClient Then
                MyBase.Style.Remove("display")
            Else
                MyBase.Style("display") = "none"
            End If

        End Sub

    End Class


End Namespace