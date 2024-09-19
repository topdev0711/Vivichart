
<Serializable()>
Public Class DataItem
    Implements IComparable

    Public Enum DataTypeEnum
        NotSet = 0
        Time = 1
        Float = 2
        [Integer] = 3
        [Char] = 4
        [Date] = 5
    End Enum

    Public Enum ControlTypeEnum
        Text = 0
        DropDown = 1
        DatePicker = 2
        TimePicker = 3
        CheckBox = 4
    End Enum

    Public Enum DateTypeEnum
        DropCalendar = 0
        DropNoCalendar = 1
        PlainCalendar = 2
        PlainNoCalendar = 3
    End Enum

    'Public Enum TimeTypeEnum
    '    Drop = 0
    '    Plain = 1
    'End Enum

#Region "private members"
    Private _ControlType As ControlTypeEnum
    Private _ColumnName As String
    Private _TableName As String
    Private _SQLDelimiter As Char
    Private _Text As String
    Private _OptionsSQL As String
    Private _Required As Boolean
    Private _Visible As Boolean
    Private _CheckBoxValues As String
    Private _LookupURL As String
    Private _LookupText As String
    Private _LookupData As String
    Private _LookupVisible As Boolean
    Private _DisplayOnly As Boolean
    Private _ControlStyle As String
    Private _ControlCssClass As String
    Private _ComputeLabel As Boolean
    Private _ComputeField As Boolean
    Private _Format As String
    Private _DefaultValue As Object
    Private _ValidationMask As String
    Private _TextMode As TextBoxMode
    Private _DateMode As DateTypeEnum
    Private _InfoPanel As Boolean
    Private _AutoPostBack As Boolean
    Private _Sequence As Integer
    Private _Protect As Boolean
    Private _Disabled As Boolean
    Private _DataType As String
    Private _DataLength As String
    Private _isPrimaryKey As Boolean
    Private _ValidateWhenEmpty As Boolean
#End Region

    Public Sub New()
        Me.ControlType = ControlTypeEnum.Text
        Me.ColumnName = String.Empty
        Me.TableName = String.Empty
        Me.SQLDelimiter = "'"
        Me.Text = String.Empty
        Me.OptionsSQL = String.Empty
        Me.Required = True
        Me.Visible = True
        Me.CheckBoxValues = String.Empty
        Me.LookupURL = String.Empty
        Me.LookupText = String.Empty
        Me.LookupData = String.Empty
        Me.LookupVisible = False
        Me.DisplayOnly = False
        Me.ControlStyle = String.Empty
        Me.ControlCssClass = String.Empty
        Me.ComputeLabel = False
        Me.ComputeField = False
        Me.Format = String.Empty
        Me.DefaultValue = Nothing
        Me.ValidationMask = String.Empty
        Me.TextMode = TextBoxMode.SingleLine
        Me.DateMode = DateTypeEnum.DropCalendar
        Me.InfoPanel = False
        Me.AutoPostBack = False
        Me.Sequence = 0
        Me._DataType = String.Empty
        Me._DataLength = String.Empty
    End Sub


    Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
        Dim Temp As DataItem = CType(obj, DataItem)
        If Me.ColumnName < Temp.ColumnName Then Return -1
        If Me.ColumnName > Temp.ColumnName Then Return 1
        Return 0
    End Function

#Region "properties"

    Public Property DataLength() As String
        Get
            Return _DataLength
        End Get
        Set(ByVal Value As String)
            _DataLength = Value
        End Set
    End Property

    Public WriteOnly Property DBDataType() As String
        Set(ByVal value As String)
            _DataType = value
        End Set
    End Property

    Public Property DataType() As DataTypeEnum
        Get
            Select Case Me._DataType
                Case String.Empty
                    Return DataTypeEnum.NotSet
                Case "t"
                    Return DataTypeEnum.Time
                Case "f"
                    Return DataTypeEnum.Float
                Case "i"
                    Return DataTypeEnum.Integer
                Case "c"
                    Return DataTypeEnum.Char
                Case "d"
                    Return DataTypeEnum.Date
                Case Else
                    Return DataTypeEnum.NotSet
            End Select
        End Get
        Set(ByVal Value As DataTypeEnum)
            Select Case Value
                Case DataTypeEnum.NotSet
                    _DataType = String.Empty
                Case DataTypeEnum.Time
                    _DataType = "t"
                Case DataTypeEnum.Float
                    _DataType = "f"
                Case DataTypeEnum.Integer
                    _DataType = "i"
                Case DataTypeEnum.Char
                    _DataType = "c"
                Case DataTypeEnum.Date
                    _DataType = "d"
            End Select
        End Set
    End Property

    Public Property ControlType() As ControlTypeEnum
        Get
            Return _ControlType
        End Get
        Set(ByVal Value As ControlTypeEnum)
            _ControlType = Value
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

    Public Property ColumnName() As String
        Get
            Return _ColumnName
        End Get
        Set(ByVal Value As String)
            _ColumnName = Value
            If Value.StartsWith("__info") Then
                InfoPanel = True
            End If
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

    Public Property Text() As String
        Get
            Return _Text
        End Get
        Set(ByVal Value As String)
            _Text = Value
        End Set
    End Property

    Public Property OptionsSQL() As String
        Get
            Return _OptionsSQL
        End Get
        Set(ByVal Value As String)
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

    Public Property Visible() As Boolean
        Get
            Return _Visible
        End Get
        Set(ByVal Value As Boolean)
            _Visible = Value
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

    Public Property LookupURL() As String
        Get
            Return _LookupURL
        End Get
        Set(ByVal Value As String)
            _LookupURL = Value
        End Set
    End Property

    Public Property LookupText() As String
        Get
            Return _LookupText
        End Get
        Set(ByVal Value As String)
            _LookupText = Value
        End Set
    End Property

    Public Property LookupData() As String
        Get
            Return _LookupData
        End Get
        Set(ByVal Value As String)
            _LookupData = Value
        End Set
    End Property

    Public Property LookupVisible() As Boolean
        Get
            Return _LookupVisible
        End Get
        Set(ByVal Value As Boolean)
            _LookupVisible = Value
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
            _ControlStyle = Value
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

    Public Property DefaultValue() As Object
        Get
            Return _DefaultValue
        End Get
        Set(ByVal Value As Object)
            _DefaultValue = Value
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

    Public Property TextMode() As TextBoxMode
        Get
            Return _TextMode
        End Get
        Set(ByVal Value As TextBoxMode)
            _TextMode = Value
        End Set
    End Property

    Public Property DateMode() As DateTypeEnum
        Get
            Return _DateMode
        End Get
        Set(ByVal Value As DateTypeEnum)
            _DateMode = Value
        End Set
    End Property

    Public Property InfoPanel() As Boolean
        Get
            Return _InfoPanel
        End Get
        Set(ByVal Value As Boolean)
            _InfoPanel = Value
        End Set
    End Property

    Public Property AutoPostBack() As Boolean
        Get
            Return _AutoPostBack
        End Get
        Set(ByVal Value As Boolean)
            _AutoPostBack = Value
        End Set
    End Property

    Public Property Sequence() As Integer
        Get
            Return _Sequence
        End Get
        Set(ByVal Value As Integer)
            _Sequence = Value
        End Set
    End Property

    Public Property Protect() As Boolean
        Get
            Return _Protect
        End Get
        Set(ByVal Value As Boolean)
            _Protect = Value
        End Set
    End Property

    '///used only in the render event of dataform
    '///only to be used if there are pending changes
    Public Property Disabled() As Boolean
        Get
            Return _Disabled
        End Get
        Set(ByVal Value As Boolean)
            _Disabled = Value
        End Set
    End Property

    Public Property isPrimaryKey() As Boolean
        Get
            Return Me._isPrimaryKey
        End Get
        Set(ByVal value As Boolean)
            Me._isPrimaryKey = value
        End Set
    End Property

    Public Property ValidateWhenEmpty() As Boolean
        Get
            Return Me._ValidateWhenEmpty
        End Get
        Set(ByVal value As Boolean)
            Me._ValidateWhenEmpty = value
        End Set
    End Property

#End Region

End Class
