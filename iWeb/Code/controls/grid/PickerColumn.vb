Imports Microsoft.VisualBasic

<Serializable()> _
Public Class PickerColumn

    Private _ColumnType As DataItem.ControlTypeEnum
    Private _ColumnName As String
    Private _DisplayName As String
    Private _Export As Boolean
    Private _Display As Boolean
    Private _PupTextSQL As String
    Private _CheckBoxValues As String
    Private _Align As HorizontalAlign
    Private _FormatString As String
    Private _DeriveStyle As String
    Private _ComputeHead As Boolean
    Private _ComputeField As Boolean

    Public Sub New()
        Me.ColumnType = DataItem.ControlTypeEnum.Text
        Me.ColumnName = String.empty
        Me.DisplayName = String.empty
        Me.Export = False
        Me.Display = True
        Me.PupTextSQL = String.empty
        Me.CheckBoxValues = String.empty
        Me.Align = HorizontalAlign.Left
        Me.FormatString = String.empty
        Me.DeriveStyle = String.empty
        Me.ComputeField = False
        Me.Computehead = False
    End Sub

    Public Sub New(ByVal ColumnType As DataItem.ControlTypeEnum, ByVal ColumnName As String, ByVal DisplayName As String, ByVal Export As Boolean, Optional ByVal Display As Boolean = True, Optional ByVal PupTextSQL As String = "", Optional ByVal CheckBoxValues As String = "", Optional ByVal Align As HorizontalAlign = HorizontalAlign.Left, Optional ByVal FormatString As String = "", Optional ByVal DeriveStyle As String = "", Optional ByVal ComputeField As Boolean = False, Optional ByVal ComputeHead As Boolean = False)
        Me.ColumnType = ColumnType
        Me.ColumnName = ColumnName
        Me.DisplayName = DisplayName
        Me.Export = Export
        Me.Display = Display
        Me.PupTextSQL = PupTextSQL
        Me.CheckBoxValues = CheckBoxValues
        Me.Align = Align
        Me.FormatString = FormatString
        Me.DeriveStyle = DeriveStyle
        Me.ComputeField = ComputeField
        Me.Computehead = ComputeHead
    End Sub

    Public Property ColumnType() As DataItem.ControlTypeEnum
        Get
            Return _ColumnType
        End Get
        Set(ByVal Value As DataItem.ControlTypeEnum)
            _ColumnType = Value
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

    Public Property DisplayName() As String
        Get
            Return _DisplayName
        End Get
        Set(ByVal Value As String)
            _DisplayName = Value
        End Set
    End Property

    Public Property Export() As Boolean
        Get
            Return _Export
        End Get
        Set(ByVal Value As Boolean)
            _Export = Value
        End Set
    End Property

    Public Property Display() As Boolean
        Get
            Return _Display
        End Get
        Set(ByVal Value As Boolean)
            _Display = Value
        End Set
    End Property

    Public Property PupTextSQL() As String
        Get
            Return _PupTextSQL
        End Get
        Set(ByVal Value As String)
            _PupTextSQL = Value
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

    Public Property FormatString() As String
        Get
            Return _FormatString
        End Get
        Set(ByVal Value As String)
            _FormatString = Value
        End Set
    End Property

    Public Property DeriveStyle() As String
        Get
            Return _DeriveStyle
        End Get
        Set(ByVal Value As String)
            _DeriveStyle = Value
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

    Public Property Computehead() As Boolean
        Get
            Return _ComputeHead
        End Get
        Set(ByVal Value As Boolean)
            _ComputeHead = Value
        End Set
    End Property

End Class


<Serializable()> _
Public Class DataItemPickerStyles
    Public TableStyle As String = String.empty
    Public TableClass As String = String.empty
    Public HeaderStyle As String = String.empty
    Public HeaderClass As String = String.empty
    Public RowStyle As String = String.empty
    Public RowClass As String = String.empty
    Public AltRowStyle As String = String.Empty
    Public AltRowClass As String = String.Empty
End Class