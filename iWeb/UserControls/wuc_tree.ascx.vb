Imports IAW.controls

Partial MustInherit Class wuc_tree
    Inherits System.Web.UI.UserControl


    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Context.Trace.Warn("control", "PageLoad")
    End Sub

    Private _TNodes As New Hashtable()
    Private _KeySize As Integer = 3

    Public Property KeySize() As Integer
        Get
            Return _KeySize
        End Get
        Set(ByVal Value As Integer)
            _KeySize = Value
        End Set
    End Property

    Public Function AddNode(ByVal aParentID As String, ByVal aText As String, ByVal aURL As String) As String
        Dim lID As String = String.Empty
        Dim li_key As Integer = 0

        Context.Trace.Warn("control", "AddNode(" + aText + ")")

        ' ensure that if specified, that the parent exists
        If aParentID <> "" And Not _TNodes.ContainsKey(aParentID) Then Return String.empty

        ' find next key 
        Do While True
            li_key += 1
            lID = aParentID + li_key.ToString.PadLeft(KeySize, "0")
            If Not _TNodes.ContainsKey(lID) Then Exit Do
        Loop

        ' add new key
        _TNodes.Add(lID, New TNode(lID, aText, aURL))

        Return lID
    End Function

    Public Function GetNode(ByVal aID As String) As TNode
        Return _TNodes(aID)
    End Function

    Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
        MyBase.OnPreRender(e)
        Context.Trace.Warn("control", "OnPreRender: savestate")

        ViewState("TNodes") = _TNodes
        ViewState("KeySize") = _KeySize

        ' set items visible or not
        ItemVisible("", 0, True)
    End Sub

    Protected Overrides Sub CreateChildControls()
        If Page.IsPostBack Then
            Context.Trace.Warn("control", "CreateChildControls: restorestate (postback)")
            _TNodes = ViewState("TNodes")
            _KeySize = ViewState("KeySize")
        Else
            Context.Trace.Warn("control", "CreateChildControls (not postback)")
        End If

        BuildTree("", 0, True)
    End Sub

    Private Sub BuildTree(ByVal as_key As String, ByVal ai_level As Integer, ByVal aShow As Boolean)
        Dim li_key As Integer = 0
        Dim ls_key As String
        '        Dim li_level As Integer
        Dim TR As TableRow
        Dim TD As TableCell
        '        Dim LNK As HyperLink
        '       Dim IMG As ImageButton
        '       Dim PIC As Image
        ' Dim LAB As Label
        Dim sBTN As IAWHyperLinkButton
        Dim cBTN As IAWHyperLink
        Dim ClassNum As String

        Context.Trace.Warn("control", "Building the tree")

        TBTree.CssClass = "TABLEMenu"
        TBTree.cellspacing = 0
        TBTree.CellPadding = 0

        If ai_level = 0 Then
            ClassNum = String.empty
        Else
            ClassNum = ai_level.ToString
        End If

        Do While True
            li_key += 1
            ls_key = as_key + li_key.ToString.PadLeft(KeySize, "0")
            If Not _TNodes.ContainsKey(ls_key) Then Exit Do

            GetNode(ls_key).Level = ClassNum

            TR = New TableRow()
            TR.ID = "r" + ls_key
            TR.CssClass = "TRMenu"
            TR.HorizontalAlign = HorizontalAlign.Left

            TD = New TableCell()
            TD.VerticalAlign = VerticalAlign.Middle

            If GetNode(ls_key).URL = String.empty Then
                TD.CssClass = "TDS" + ClassNum
                sBTN = New IAWHyperLinkButton(AddressOf Button_Click)
                'sBTN.IDent = ls_key
                sBTN.ID = ls_key
                If GetNode(ls_key).Open Then
                    sBTN.CssClass = "TDoSC" + ClassNum
                    'sBTN.HoverCssClass = "TDoSCO" + ClassNum
                Else
                    sBTN.CssClass = "TDSC" + ClassNum
                    'sBTN.HoverCssClass = "TDSCO" + ClassNum
                End If
                sBTN.Text = GetNode(ls_key).Text
                TD.Controls.Add(sBTN)
            Else
                TD.CssClass = "TDL" + ClassNum
                cBTN = New IAWHyperLink(GetNode(ls_key).URL)
                cBTN.CssClass = "TDLC" + ClassNum
                'cBTN.HoverCssClass = "TDLCO" + ClassNum
                cBTN.Text = GetNode(ls_key).Text
                TD.Controls.Add(cBTN)
            End If
            TR.Cells.Add(TD)
            TBTree.Rows.Add(TR)
            BuildTree(ls_key, ai_level + 1, GetNode(ls_key).Open)
        Loop
    End Sub

    ' ok, we parse the tree again, making items visible or invisible as necessary
    Private Sub ItemVisible(ByVal aKey As String, ByVal aLevel As Integer, ByVal aVisible As Boolean)
        Dim li_key As Integer = 0
        Dim ls_key As String
        '        Dim li_level As Integer
        '      Dim li_vis As Boolean

        Do While True
            li_key += 1
            ls_key = aKey + li_key.ToString.PadLeft(KeySize, "0")
            If Not _TNodes.ContainsKey(ls_key) Then Exit Do

            If Not aVisible Then
                FindControl("r" + ls_key).Visible = False
                ItemVisible(ls_key, aLevel + 1, False)
            Else
                FindControl("r" + ls_key).Visible = True
                If _TNodes.ContainsKey(ls_key + (1.ToString.PadLeft(KeySize, "0"))) Then
                    If GetNode(ls_key).Open Then
                        ItemVisible(ls_key, aLevel + 1, True)
                    Else
                        ItemVisible(ls_key, aLevel + 1, False)
                    End If
                End If
            End If
        Loop
    End Sub

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As EventArgs)
        If Page.IsPostBack Then
            GetNode(sender.ID).Open = Not GetNode(sender.ID).Open
            If GetNode(sender.ID).Open Then
                CType(FindControl(sender.ID), IAWHyperLinkButton).CssClass = "TDoSC" + GetNode(sender.ID).Level
                'CType(FindControl(sender.ID), IAWHyperLinkButton).HoverCssClass = "TDoSCO" + GetNode(sender.ID).Level
            Else
                CType(FindControl(sender.ID), IAWHyperLinkButton).CssClass = "TDSC" + GetNode(sender.ID).Level
                'CType(FindControl(sender.ID), IAWHyperLink).HoverCssClass = "TDSCO" + GetNode(sender.ID).Level
            End If
        End If
    End Sub

    Protected Overrides Sub OnInit(ByVal e As System.EventArgs)

    End Sub
End Class

