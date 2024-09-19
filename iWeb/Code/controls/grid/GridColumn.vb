<Serializable()> Public Class DataGridFormStyles
    Public TableStyle As String = String.empty
    Public TableClass As String = String.empty
    Public HeaderStyle As String = String.empty
    Public HeaderClass As String = String.empty
    Public RowStyle As String = String.empty
    Public RowClass As String = String.empty
    Public AltRowStyle As String = String.empty
    Public AltRowClass As String = String.empty
    Public DataEntryStyle As String = String.empty
    Public DataEntryClass As String = String.empty
    Public DataEntryControlStyle As String = String.empty
    Public DataEntryControlClass As String = String.empty
    Public DataDisplayStyle As String = String.empty
    Public DataDisplayClass As String = String.empty
End Class

<Serializable()> Public Class GridColumn

    Private _ControlType As DataItem.ControlTypeEnum
    Private _ColumnName As String = String.empty
    Private _TableName As String = String.empty
    Private _SQLDelimiter As Char
    Private _Text As String = String.empty
    Private _OptionsSQL As String = String.empty
    Private _Required As Boolean = False
    Private _CheckBoxValues As String = String.empty
    Private _Align As HorizontalAlign = HorizontalAlign.Left
    Private _LookupURL As String = String.empty
    Private _LookupText As String = "..."
    Private _DisplayOnly As Boolean = False
    Private _ControlStyle As String = String.empty
    Private _ControlCssClass As String = String.empty
    Private _DeriveStyle As String = String.empty
    Private _ComputeLabel As Boolean = False
    Private _ComputeField As Boolean = False
    Private _Format As String = String.empty
    Private _ValidationMask As String = String.empty
    Private _Visible As Boolean = True
    Private _TextBoxType As TextBoxMode = TextBoxMode.SingleLine
    Private _DefaultValue As Object = Nothing
    Private _DeriveValidationMask As Boolean = False
    Private _DateMode As DataItem.DateTypeEnum = DataItem.DateTypeEnum.PlainNoCalendar
    Private _ColNum As Integer
    Public Property ControlType() As DataItem.ControlTypeEnum
        Get
            Return _ControlType
        End Get
        Set(ByVal Value As DataItem.ControlTypeEnum)
            _ControlType = Value
        End Set
    End Property
    Public Property ColumnName() As String
        Get
            Return _ColumnName
        End Get
        Set(ByVal Value As String)
            _ColumnName = Value
        End Set
    End Property
    Public Property TableName() As String
        Get
            Return _TableName
        End Get
        Set(ByVal Value As String)
            _TableName = Value
        End Set
    End Property
    Public Property SQLDelimiter() As Char
        Get
            Return _SQLDelimiter
        End Get
        Set(ByVal Value As Char)
            _SQLDelimiter = Value
        End Set
    End Property
    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.empty
            _Text = Value
        End Set
    End Property
    Public Property OptionsSQL() As String
        Get
            Return _OptionsSQL
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.empty
            _OptionsSQL = Value
        End Set
    End Property
    Public Property Required() As Boolean
        Get
            Return _Required
        End Get
        Set(ByVal Value As Boolean)
            _Required = Value
        End Set
    End Property
    Public Property CheckBoxValues() As String
        Get
            Return _CheckBoxValues
        End Get
        Set(ByVal Value As String)
            _CheckBoxValues = Value
        End Set
    End Property
    Public Property Align() As HorizontalAlign
        Get
            Return _Align
        End Get
        Set(ByVal Value As HorizontalAlign)
            _Align = Value
        End Set
    End Property
    Public Property LookupURL() As String
        Get
            Return _LookupURL
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.empty
            _LookupURL = Value
        End Set
    End Property
    Public Property LookupText() As String
        Get
            Return _LookupText
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.empty
            _LookupText = Value
        End Set
    End Property
    Public Property DisplayOnly() As Boolean
        Get
            Return _DisplayOnly
        End Get
        Set(ByVal Value As Boolean)
            _DisplayOnly = Value
        End Set
    End Property
    Public Property ControlStyle() As String
        Get
            Return _ControlStyle
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.Empty
            _ControlStyle = Value
        End Set
    End Property
    Public Property DeriveStyle() As String
        Get
            Return _DeriveStyle
        End Get
        Set(ByVal Value As String)
            If Value Is Nothing Then Value = String.Empty
            _DeriveStyle = Value
        End Set
    End Property
    Public Property ControlCssClass() As String
        Get
            Return _ControlCssClass
        End Get
        Set(ByVal Value As String)
            _ControlCssClass = Value
        End Set
    End Property
    Public Property ComputeLabel() As Boolean
        Get
            Return _ComputeLabel
        End Get
        Set(ByVal Value As Boolean)
            _ComputeLabel = Value
        End Set
    End Property
    Public Property ComputeField() As Boolean
        Get
            Return _ComputeField
        End Get
        Set(ByVal Value As Boolean)
            _ComputeField = Value
        End Set
    End Property
    Public Property Format() As String
        Get
            Return _Format
        End Get
        Set(ByVal Value As String)
            _Format = Value
        End Set
    End Property
    Public Property ValidationMask() As String
        Get
            Return _ValidationMask
        End Get
        Set(ByVal Value As String)
            _ValidationMask = Value
        End Set
    End Property
    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get
        Set(ByVal Value As Boolean)
            _Visible = Value
        End Set
    End Property
    Public Property TextBoxType() As TextBoxMode
        Get
            Return _TextBoxType
        End Get
        Set(ByVal Value As TextBoxMode)
            _TextBoxType = Value
        End Set
    End Property
    Public Property DefaultValue() As Object
        Get
            Return _DefaultValue
        End Get
        Set(ByVal Value As Object)
            _DefaultValue = Value
        End Set
    End Property
    Public Property DeriveValidationMask() As Boolean
        Get
            Return _DeriveValidationMask
        End Get
        Set(ByVal Value As Boolean)
            _DeriveValidationMask = Value
        End Set
    End Property
    Public Property DateMode() As DataItem.DateTypeEnum
        Get
            Return _DateMode
        End Get
        Set(ByVal Value As DataItem.DateTypeEnum)
            _DateMode = Value
        End Set
    End Property

    Public Property ColNum() As Integer
        Get
            Return _ColNum
        End Get
        Set(ByVal Value As Integer)
            _ColNum = Value
        End Set
    End Property
End Class
