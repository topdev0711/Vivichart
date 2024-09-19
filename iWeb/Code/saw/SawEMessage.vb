<Serializable()> Public Class SawEMessage
    Private _EItems As New Hashtable
    Private _TopNum As Integer

    Public Sub New()
        Clear()
    End Sub

    Private ReadOnly Property EItems(ByVal aID As String) As EItem
        Get
            If _EItems.Contains(aID) Then
                Return _EItems(aID)
            Else
                Return Nothing
            End If
        End Get
    End Property

    Private Property TopNum() As Integer
        Get
            Return _TopNum
        End Get
        Set(ByVal Value As Integer)
            _TopNum = Value
        End Set
    End Property

    Public Sub Clear()
        _EItems.Clear()
        TopNum = 0
    End Sub

    Public Sub Add(ByVal aName As String, ByVal aValue As String)
        NewEItem(aName, aValue, "s")
    End Sub

    Public Sub Add(ByVal aName As String, ByVal aValue As Short)
        NewEItem(aName, aValue.ToString, "i")
    End Sub
    Public Sub Add(ByVal aName As String, ByVal aValue As Integer)
        NewEItem(aName, aValue.ToString, "i")
    End Sub
    Public Sub Add(ByVal aName As String, ByVal aValue As Decimal)
        NewEItem(aName, aValue.ToString, "i")
    End Sub
    Public Sub Add(ByVal aName As String, ByVal aValue As Long)
        NewEItem(aName, aValue.ToString, "i")
    End Sub

    Public Sub Add(ByVal aName As String, ByVal aValue As Double)
        NewEItem(aName, aValue.ToString, "f")
    End Sub
    Public Sub Add(ByVal aName As String, ByVal aValue As Single)
        NewEItem(aName, aValue.ToString, "f")
    End Sub

    Public Sub Add(ByVal aName As String, ByVal aValue As Date, ByVal aDateOrTime As String)
        If aDateOrTime.StartsWith("d") Then
            NewEItem(aName, Format(aValue, "yyyy/MM/dd hh:mm:ss"), "d")
        Else
            NewEItem(aName, Format(aValue, "yyyy/MM/dd hh:mm:ss"), "t")
        End If
    End Sub

    Private Sub NewEItem(ByVal aName As String, ByVal aValue As String, ByVal aDatatype As String)
        TopNum += 1
        _EItems.Add(TopNum, New EItem(aName, aValue, aDatatype))
    End Sub

    Private Function Pack() As String
        Dim i As Integer
        Dim ls_packed As String = String.Empty
        Dim Item As EItem

        For i = 1 To TopNum
            Item = _EItems(i)
            ls_packed += Item.Name + "^" + Item.Val + "^" + Item.DataType + "^"
        Next
        ' name^value^datatype^name^value^datatype
        ' person_ref^0000000001^s^emp_ref^01^s^occurrence^23^i

        Return ls_packed
    End Function

    Public Function Insert(ByVal aMsgType As String) As Boolean
        Dim ls_packed As String
        Dim ls_id As String
        Dim ls_sql As String

        '///only insert if the message type is active
        ls_sql = "SELECT count(*)  " +
                 "  FROM q_e_mess WITH (NOLOCK)" +
                 " WHERE e_message_ref = '" + aMsgType + "' " +
                 "   AND active = '1' "
        If SawDB.ExecScalar(ls_sql) = 0 Then
            Return False
        End If

        ls_packed = Pack()
        If ls_packed = String.Empty Then Return False
        ls_id = SawUtil.GetKey("q_e_mess_event", "e_mess_ev_id")

        ls_sql = "INSERT INTO q_e_mess_event (" _
               + "	e_mess_ev_id," _
               + "	e_message_ref," _
               + "	e_mess_ev_arg_list," _
               + "  e_mess_ev_status " _
               + ") VALUES (??,??,??,'01')"
        If SawDB.ExecNonQuery(ls_sql, New Object() {ls_id, aMsgType, ls_packed}) = 0 Then Return False


        Return True

    End Function

End Class

<Serializable()> Public Class EItem
    Private _Name As String
    Private _Val As String
    Private _Datatype As String

    Public Sub New(ByVal aName As String, ByVal aValue As String, ByVal aDatatype As String)
        Name = aName
        Val = aValue
        DataType = aDatatype
    End Sub

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal Value As String)
            _Name = Value
        End Set
    End Property

    Public Property Val() As String
        Get
            Return _Val
        End Get
        Set(ByVal Value As String)
            _Val = Value
        End Set
    End Property

    Public Property DataType() As String
        Get
            Return _Datatype
        End Get
        Set(ByVal Value As String)
            _Datatype = Value
        End Set
    End Property
End Class