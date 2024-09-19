

<Serializable()> Public Class SawChart

#Region "Variables and Properties"
    Private _CNodes As New Hashtable()  'Storage for all nodes
    Private _NodeWidth As Integer = 2 ' number of table cells per node
    Private _GapWidth As Integer = 1 ' number of table cells between nodes
    Private Rightmost As New CPosAccumulator() 'Holds position and size info on rightmost nodes
    Private HTTP As HttpContext         'HTTP session context 
    Private _TopKey As Integer = 0

    Public ReadOnly Property MaxWidth() As Integer
        Get
            Return Rightmost.MaxWidth
        End Get
    End Property

    Public ReadOnly Property MaxDepth() As Integer
        Get
            Return Rightmost.MaxDepth
        End Get
    End Property

    Public Property NodeWidth() As Integer
        Get
            Return _NodeWidth
        End Get
        Set(ByVal Value As Integer)
            _NodeWidth = Value
        End Set
    End Property

    Public Property GapWidth() As Integer
        Get
            Return _GapWidth
        End Get
        Set(ByVal Value As Integer)
            _GapWidth = Value
        End Set
    End Property

    Public ReadOnly Property GetNode(ByVal ai_ID As Integer) As CNode
        Get
            If _CNodes.ContainsKey(ai_ID) Then Return _CNodes(ai_ID)
            Return Nothing
        End Get
    End Property

    Public Sub New(ByVal aHTTP As HttpContext)
        HTTP = aHTTP
        _CNodes.Clear()
        'add the root node
        _CNodes.Add(0, New CNode(0, New Object()))
    End Sub

    Public ReadOnly Property GetNewKey() As Integer
        Get
            _TopKey += 1
            Return _TopKey
        End Get
    End Property

#End Region

    ' add a node to the tree given the parent
    Public Function AddNode(ByVal ai_ParentID As Integer, ByVal aItem As Object, ByVal ai_spacers As Integer) As Integer
        Dim li_NewID As Integer
        Dim li_key As Integer
        Dim li_parent As Integer
        Dim li_loop As Integer

        ' ensure that if specified, that the parent exists
        If ai_ParentID <> 0 And Not _CNodes.ContainsKey(ai_ParentID) Then Return 0

        ' Add the new node
        li_NewID = GetNewKey
        _CNodes.Add(li_NewID, New CNode(li_NewID, aItem))

        ' find last child node
        li_key = GetLastChild(ai_ParentID)
        If li_key = 0 Then
            ' add reference to parent node as its first child
            GetNode(ai_ParentID).ChildID = li_NewID
        Else
            GetNode(li_key).NextSib = li_NewID
            GetNode(li_NewID).PreviousSib = li_key
        End If

        li_parent = ai_ParentID

        For li_loop = 1 To ai_spacers
            GetNode(li_NewID).NodeType = 3
            GetNode(li_NewID).Width = GapWidth
            GetNode(li_NewID).ParentID = li_parent

            li_parent = li_NewID

            li_NewID = GetNewKey
            _CNodes.Add(li_NewID, New CNode(li_NewID, aItem))

            GetNode(li_parent).ChildID = li_NewID
        Next

        GetNode(li_NewID).NodeType = 1
        GetNode(li_NewID).Width = NodeWidth
        GetNode(li_NewID).ParentID = li_parent

        Return li_NewID
    End Function

    ' this will attach a node to another node (for horizontal connection)
    ' multiple attachments allowed
    Public Sub AttachNode(ByVal ai_ParentID As Integer, ByVal aItem As Object)
        Dim li_NewID As Integer
        Dim li_id As Integer

        If ai_ParentID = 0 Or Not _CNodes.ContainsKey(ai_ParentID) Then Return

        ' Add the new node
        li_NewID = GetNewKey
        _CNodes.Add(li_NewID, New CNode(li_NewID, aItem))

        GetNode(li_NewID).NodeType = 2
        GetNode(li_NewID).Width = NodeWidth
        GetNode(li_NewID).ParentID = ai_ParentID

        GetNode(ai_ParentID).Width += NodeWidth + GapWidth

        ' find where we need to attach the new node
        If GetNode(ai_ParentID).AttachmentID = 0 Then
            GetNode(ai_ParentID).AttachmentID = li_NewID
        Else
            li_id = GetNode(ai_ParentID).AttachmentID
            Do While GetNode(li_id).NextSib <> 0
                li_id = GetNode(li_id).NextSib
            Loop
            GetNode(li_id).NextSib = li_NewID
            GetNode(li_NewID).PreviousSib = li_id
        End If

    End Sub

    ' how many child nodes does a node have
    Public Function NumberOfChildren(ByVal ai_ParentID As Integer) As Integer
        Dim li_id As Integer
        Dim li_count As Integer = 0

        If ai_ParentID <> 0 And Not _CNodes.ContainsKey(ai_ParentID) Then Return 0

        li_id = GetNode(ai_ParentID).ChildID
        Do While li_id <> 0
            li_count += 1
            li_id = GetNode(li_id).NextSib
        Loop
        Return li_count
    End Function

    ' find the parent of a node
    Public Function GetParent(ByVal ai_ID As Integer) As Integer
        Return GetNode(ai_ID).ParentID
    End Function

    ' find first child of a parent node
    Public Function GetFirstChild(ByVal ai_ID As Integer) As Integer
        Return GetNode(ai_ID).ChildID
    End Function

    ' find last chid of a parent node
    Public Function GetLastChild(ByVal ai_ParentID As Integer) As Integer
        Dim li_id As Integer

        If ai_ParentID <> 0 And Not _CNodes.ContainsKey(ai_ParentID) Then Return 0

        li_id = GetNode(ai_ParentID).ChildID
        If li_id = 0 Then Return 0
        Do While GetNode(li_id).NextSib <> 0
            li_id = GetNode(li_id).NextSib
        Loop
        Return li_id
    End Function

    ' find next node from current
    Public Function GetNextSibling(ByVal ai_ID As Integer) As Integer
        Return GetNode(ai_ID).NextSib
    End Function

    ' find previous node from current
    Public Function GetPreviousSibling(ByVal ai_ID As Integer) As Integer
        Return GetNode(ai_ID).PreviousSib
    End Function

#Region "Layout Algorithm"
    'Calculate positions for optimal compact layout
    Public Sub CalcLayout()
        'Initialize Rightmost Position Data
        Rightmost = New CPosAccumulator()
        Rightmost.GapWidth = GapWidth

        'Layout recursively starting with the root node
        CalcPosition(0, 0)
    End Sub

    'This is a recursive routine to calculate node positions
    Private Sub CalcPosition(ByVal ai_ID As Integer, ByVal ai_Row As Int16)
        'Basic algorithm is:
        ' 1) Position given node to the left as far as possible
        ' 2) Call CalcPosition for each child
        ' 3) Adjust this node in the knowledge of all child positions

        Dim li_parent_width As Integer
        Dim li_left_width As Integer
        Dim li_right_width As Integer

        If ai_ID <> 0 Then
            GetNode(ai_ID).Position = Rightmost.NextAvailablePosition(ai_Row, GetNode(ai_ID).Width)
        End If

        'Position each child
        Dim li_ChildId As Integer
        li_ChildId = GetFirstChild(ai_ID)
        If li_ChildId = 0 Then Return 'Early exit when no children

        While li_ChildId <> 0
            CalcPosition(li_ChildId, ai_Row + 1)
            li_ChildId = GetNextSibling(li_ChildId)
        End While

        'Reposition parent after child placement
        If ai_ID = 0 Then Return 'Finished when parent is the dummy node

        'Find the left and right most positions of children
        Dim LPos, RPos, RequiredPos As Integer
        li_ChildId = GetFirstChild(ai_ID)

        li_parent_width = NodeWidth / 2
        li_left_width = NodeWidth / 2
        li_right_width = NodeWidth / 2

        If GetNode(ai_ID).NodeType = 3 Then li_parent_width = GapWidth / 2
        If GetNode(li_ChildId).NodeType = 3 Then li_left_width = GapWidth / 2
        If GetNode(GetLastChild(ai_ID)).NodeType = 3 Then li_right_width = GapWidth / 2

        LPos = GetNode(li_ChildId).Position
        RPos = Rightmost.Position(ai_Row + 1)

        'Desired position is midway
        'RequiredPos = (LPos + RPos) / 2
        RequiredPos = ((LPos + li_left_width + RPos + li_right_width) / 2) - li_parent_width

        '3 scenarios
        ' a) Desired pos is further right than initial guess - move it right
        ' b) Desired pos is further left than initial guess - move sub tree right (we cannot move further left)
        ' c) We could be lucky and it is correct already
        Dim li_currPos As Integer
        li_currPos = GetNode(ai_ID).Position

        If RequiredPos > li_currPos Then

            GetNode(ai_ID).Position = RequiredPos       'move right
            Rightmost.Position(ai_Row) = RequiredPos    'and don't forget to accumulate rightmost

        ElseIf RequiredPos < li_currPos Then

            MoveTree(ai_ID, li_currPos - RequiredPos, ai_Row)   'move the whole sub tree to the right
            GetNode(ai_ID).Position = li_currPos                'Reinstate after this node got shifted
            Rightmost.Position(ai_Row) = li_currPos             'and reinstate rightmost

        End If
    End Sub

    'Adjust position of sub tree by given delta
    Private Sub MoveTree(ByVal ai_ID As Integer, ByVal ai_Delta As Integer, ByVal ai_Row As Int16)
        'Move this root
        GetNode(ai_ID).Position = GetNode(ai_ID).Position + ai_Delta

        'Adjust rightmost position accumulator where necessary
        If GetNode(ai_ID).Position > Rightmost.Position(ai_Row) Then
            Rightmost.Position(ai_Row) = GetNode(ai_ID).Position
        End If

        'Move each child tree
        Dim li_ChildId As Integer
        li_ChildId = GetFirstChild(ai_ID)
        While li_ChildId <> 0
            MoveTree(li_ChildId, ai_Delta, ai_Row + 1)
            li_ChildId = GetNextSibling(li_ChildId)
        End While
    End Sub

#End Region

End Class

<Serializable()> Public Class CPosAccumulator
    'Responsible for accumulating max positions and widths for layout algoritm
    Private Const LAYOUT_ORIGIN As Integer = 1     'First column position
    Private _pos(0) As Integer
    Private _width(0) As Integer
    Private _GapWidth As Integer = 1

    Public Property Position(ByVal ai_Row As Int16) As Integer
        Get
            Return _pos(ai_Row)
        End Get
        Set(ByVal Value As Integer)
            _pos(ai_Row) = Value
        End Set
    End Property

    Public ReadOnly Property Width(ByVal ai_Row As Int16) As Integer
        Get
            Return _width(ai_Row)
        End Get
    End Property

    Public ReadOnly Property MaxWidth() As Integer
        Get
            Dim li_MaxPos As Integer = 0
            Dim li_Row As Integer
            Dim li_pos As Integer
            For li_Row = 1 To UBound(_pos)
                li_pos = _pos(li_Row) + _width(li_Row) - 1
                If li_pos > li_MaxPos Then li_MaxPos = li_pos
            Next
            Return li_MaxPos
        End Get
    End Property

    Public ReadOnly Property MaxDepth() As Integer
        Get
            Return UBound(_pos)
        End Get
    End Property

    Public Property GapWidth() As Integer
        Get
            Return _GapWidth
        End Get
        Set(ByVal Value As Integer)
            _GapWidth = Value
        End Set
    End Property

    'Calculates the next available position to the right 
    'and saves size of this entry
    Public Function NextAvailablePosition(ByVal ai_Row As Int16, ByVal ai_Width As Integer) As Integer
        If UBound(_pos) < ai_Row Then
            'Expand when we reach new depths
            ReDim Preserve _pos(ai_Row)
            ReDim Preserve _width(ai_Row)
            _pos(ai_Row) = LAYOUT_ORIGIN
        Else
            'Allow for size of current rightmost box and a gap
            _pos(ai_Row) += _width(ai_Row) + GapWidth
        End If

        _width(ai_Row) = ai_Width 'New width of this box

        Return _pos(ai_Row)
    End Function
End Class

<Serializable()> Public Class CNode

    Private _ID As Integer
    Private _ParentID As Integer
    Private _ChildID As Integer
    Private _NextSib As Integer
    Private _PreviousSib As Integer
    Private _AttachmentID As Integer
    Private _Item As Object
    Private _Position As Integer
    Private _Width As Integer
    Private _NodeType As Integer
    Private _Spacers As Integer

    Public Sub New(ByVal ai_ID As Integer, ByVal aItem As Object)
        ID = ai_ID
        ParentID = 0
        ChildID = 0
        PreviousSib = 0
        AttachmentID = 0
        NextSib = 0
        Item = aItem
        Position = 0
        Width = 2
        Nodetype = 1
        Spacers = 0
    End Sub

    Public Property ID() As Integer
        Get
            Return _ID
        End Get
        Set(ByVal Value As Integer)
            _ID = Value
        End Set
    End Property

    Public Property ParentID() As Integer
        Get
            Return _ParentID
        End Get
        Set(ByVal Value As Integer)
            _ParentID = Value
        End Set
    End Property

    Public Property ChildID() As Integer
        Get
            Return _ChildID
        End Get
        Set(ByVal Value As Integer)
            _ChildID = Value
        End Set
    End Property

    Public Property NextSib() As Integer
        Get
            Return _NextSib
        End Get
        Set(ByVal Value As Integer)
            _NextSib = Value
        End Set
    End Property

    Public Property PreviousSib() As Integer
        Get
            Return _PreviousSib
        End Get
        Set(ByVal Value As Integer)
            _PreviousSib = Value
        End Set
    End Property

    Public Property AttachmentID() As Integer
        Get
            Return _AttachmentID
        End Get
        Set(ByVal Value As Integer)
            _AttachmentID = Value
        End Set
    End Property

    Public Property Item() As Object
        Get
            Return _Item
        End Get
        Set(ByVal Value As Object)
            _Item = Value
        End Set
    End Property

    Public Property Position() As Integer
        Get
            Return _Position
        End Get
        Set(ByVal Value As Integer)
            _Position = Value
        End Set
    End Property

    Public Property Width() As Integer
        Get
            Return _Width
        End Get
        Set(ByVal Value As Integer)
            _Width = Value
        End Set
    End Property

    Public Property NodeType() As Integer
        Get
            Return _NodeType
        End Get
        Set(ByVal Value As Integer)
            _NodeType = Value
        End Set
    End Property

    Public Property Spacers() As Integer
        Get
            Return _Spacers
        End Get
        Set(ByVal Value As Integer)
            _Spacers = Value
        End Set
    End Property

End Class

