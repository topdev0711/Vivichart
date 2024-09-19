Imports Microsoft.VisualBasic

Namespace IAW

    Public Enum RightType
        sql = 0
        trueFalse = 1
        derived = 2
    End Enum

    Public Enum RightTo
        view = 0
        update = 1
        insert = 2
        delete = 3
    End Enum

    <Serializable()> _
    Public Class EditRights
        Private _type As RightType
        Private _rightName As RightTo
        Private _value As String

        Public ReadOnly Property type() As RightType
            Get
                Return _type
            End Get
        End Property
        Public ReadOnly Property rightName() As RightTo
            Get
                Return _rightName
            End Get
        End Property

        Public ReadOnly Property isAllowed() As Boolean
            Get
                Return _value = "1"
            End Get
        End Property

        Public ReadOnly Property value() As String
            Get
                Return _value
            End Get
        End Property

        Public Sub New(ByVal value As Object, ByVal right As RightTo)
            Dim Val As String
            If value Is DBNull.Value OrElse String.IsNullOrEmpty(value) Then
                _type = RightType.trueFalse
                _value = "0"
                _rightName = right
                Return
            End If
            Val = value
            If (Val.TrimStart.ToLower.StartsWith("select") Or Val.TrimStart.ToLower.StartsWith("if")) And Val.TrimStart.Length > 15 Then
                _type = RightType.sql
            ElseIf Val.Trim.Equals("0") Or Val.Trim.Equals("1") Then
                _type = RightType.trueFalse
            Else
                _type = RightType.derived
            End If
            _value = Val.Trim
            _rightName = right
        End Sub

        Public Sub trace()
            ctx.trace("         right to - " + Me.rightName.ToString, Me.type.ToString + ":" + Me.value)
        End Sub

    End Class

End Namespace