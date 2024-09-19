Imports Microsoft.VisualBasic
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.Collections.Generic

Namespace IAW.controls

    Public Class AnalysisCodeSelector
        Inherits System.Web.UI.WebControls.CompositeControl

#Region "properties"

        Public Property StartPosition() As Integer
            Get
                If Me.ViewState("StartPosition") Is Nothing Then
                    Me.ViewState("StartPosition") = 1
                End If
                Return Me.ViewState("StartPosition")
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("StartPosition") = value
            End Set
        End Property

        Public Property EndPosition() As Integer
            Get
                If Me.ViewState("EndPosition") Is Nothing Then
                    If String.IsNullOrEmpty(Me.AnalysisView) Then
                        Me.EndPosition = SawDB.ExecScalar("SELECT max(structure_position) FROM structure_position WHERE analysis_structure = '" + Me.AnalysisStructure + "' ")
                    Else
                        Me.EndPosition = SawDB.ExecScalar("SELECT structure_position FROM analysis_view WHERE analysis_view = '" + Me.AnalysisView + "' ")
                    End If
                End If
                Return Me.ViewState("EndPosition")
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("EndPosition") = value
            End Set
        End Property

        Public Property AnalysisView() As String
            Get
                If Me.ViewState("AnalysisView") Is Nothing Then
                    Me.ViewState("AnalysisView") = String.Empty
                End If
                Return Me.ViewState("AnalysisView")
            End Get
            Set(ByVal value As String)
                Me.ViewState("AnalysisView") = value
            End Set
        End Property

        Public Property AnalysisStructure() As String
            Get
                If Me.ViewState("AnalysisStructure") Is Nothing Then
                    Me.ViewState("AnalysisStructure") = "organisation"
                End If
                Return Me.ViewState("AnalysisStructure")
            End Get
            Set(ByVal value As String)
                Me.ViewState("AnalysisStructure") = value
            End Set
        End Property

        Public Property StructurePosition() As Integer
            Get
                If Me.ViewState("StructurePosition") Is Nothing Then
                    Me.ViewState("StructurePosition") = SawDB.ExecScalar("SELECT max(structure_position) FROM structure_position WHERE analysis_structure = '" + Me.AnalysisStructure + "' ")
                End If
                Return Me.ViewState("StructurePosition")
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("StructurePosition") = value
            End Set
        End Property

        Private Property ProcessStructurePosition() As Integer
            Get
                Return Me.ViewState("ProcessStructurePosition")
            End Get
            Set(ByVal value As Integer)
                Me.ViewState("ProcessStructurePosition") = value
            End Set
        End Property

        Public ReadOnly Property AnalysisCode() As String
            Get
                'get the selected value from the dropdown at the lowest level
                Return Me._lists(Me.EndPosition).SelectedValue
            End Get
        End Property

        Public Property Layout() As WebControls.Orientation
            Get
                If Me.ViewState("Orientation") Is Nothing Then
                    Me.ViewState("Orientation") = Orientation.Horizontal
                End If
                Return Me.ViewState("AnalysisStructure")
            End Get
            Set(ByVal value As WebControls.Orientation)
                Me.ViewState("Orientation") = value
            End Set
        End Property

        Public Property MaintainSelectedValue() As Boolean
            Get
                If Me.ViewState("MaintainSelectedValue") Is Nothing Then
                    Me.ViewState("MaintainSelectedValue") = True
                End If
                Return Me.ViewState("MaintainSelectedValue")
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState("MaintainSelectedValue") = value
            End Set
        End Property

        Public Property ShowAnalysisStructure() As Boolean
            Get
                If Me.ViewState("ShowAnalysisStructure") Is Nothing Then
                    Me.ViewState("ShowAnalysisStructure") = False
                End If
                Return Me.ViewState("ShowAnalysisStructure")
            End Get
            Set(ByVal value As Boolean)
                Me.ViewState("ShowAnalysisStructure") = value
            End Set
        End Property

#End Region

        'list of dropdowns, indexed by structure position
        Private _lists As New Dictionary(Of Integer, DropDownList)
        Private _phDropDowns As New PlaceHolder
        Public Event AnalysisCodeChanged As EventHandler

#Region "constructors"

        Public Sub New()
        End Sub

        Public Sub New(ByVal analysis_view As String)
            Me.AnalysisView = analysis_view
        End Sub

        Public Sub New(ByVal analysis_structure As String, ByVal start_position As Integer, ByVal end_position As Integer)
            Me.AnalysisStructure = analysis_structure
            Me.StartPosition = start_position
            Me.EndPosition = end_position
        End Sub

#End Region

        Public Sub SelectStructureAndCode(ByVal analysis_structure As String, ByVal structure_position As String, ByVal analysis_code As String)
            Dim ddl As DropDownList = Me.FindControl("_ddl_analysis_structure")
            If ddl IsNot Nothing Then
                ddl.SelectedValue = analysis_structure
                Me.AnalysisStructure = ddl.SelectedValue
            End If
            Me.EndPosition = SawDB.ExecScalar("SELECT max(structure_position) FROM structure_position WHERE analysis_structure = '" + Me.AnalysisStructure + "' ")
            Me.StructurePosition = Me.EndPosition
            If Not String.IsNullOrEmpty(structure_position) Then
                Me.StructurePosition = Integer.Parse(structure_position)
            End If
            CreateDropDowns()
            PopulateDropDowns(analysis_code)
        End Sub

        Public Sub DisableToStructurePosition(ByVal structure_position As String)
            'disable the analysis structure
            Dim ddlAnalysisStruc As DropDownList = Me.FindControl("_ddl_analysis_structure")
            If ddlAnalysisStruc IsNot Nothing Then
                ddlAnalysisStruc.Enabled = False
                ddlAnalysisStruc.Visible = False
            End If
            Dim sp As Integer = Integer.Parse(structure_position)
            For Each kvp As KeyValuePair(Of Integer, DropDownList) In _lists
                If sp >= kvp.Key Then
                    kvp.Value.Enabled = False
                    kvp.Value.Visible = False
                End If
            Next
        End Sub


#Region "page events"

        Protected Overrides Sub CreateChildControls()
            'create the controls here unless its a postback

            CreateAnalysisStructureDropdown()
            Me.Controls.Add(_phDropDowns)
            'If Not Page.IsPostBack Then
            CreateDropDowns()
            'End If

            If Not Page.IsPostBack And (String.IsNullOrEmpty(ctx.session("__analysis_filter")) Or Not Me.MaintainSelectedValue) Then
                PopulateDropDowns()
                'this session variable may be used in sql by the standard web forms/lists
                ctx.session("__analysis_filter") = Me.AnalysisCode
            ElseIf Not Page.IsPostBack And Not String.IsNullOrEmpty(ctx.session("__analysis_filter")) Then
                PopulateDropDowns(ctx.session("__analysis_filter"))
            End If

        End Sub

        Private Sub AnalysisCodeSelector_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

            'If Page.IsPostBack Then
            '    CreateDropDowns()
            'End If

            'If Not Page.IsPostBack And (String.IsNullOrEmpty(ctx.session("__analysis_filter")) Or Not Me.MaintainSelectedValue) Then
            '    PopulateDropDowns()
            '    'this session variable may be used in sql by the standard web forms/lists
            '    ctx.session("__analysis_filter") = Me.AnalysisCode
            'ElseIf Not Page.IsPostBack And Not String.IsNullOrEmpty(ctx.session("__analysis_filter")) Then
            '    PopulateDropDowns(ctx.session("__analysis_filter"))
            'End If
        End Sub

        'make the page use sql viewstate
        Private Sub AnalysisCodeSelector_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
            'Me._lists = New Dictionary(Of Integer, DropDownList)
            '_phDropDowns = New PlaceHolder
            'Me.Controls.Add(_phDropDowns)

            'Dim p As Page = Me.Page
            'If TypeOf p Is stub_IngenWebPage Then
            '    CType(p, stub_IngenWebPage).useSqlViewState = True
            'End If
        End Sub

#End Region

#Region "dropdowns"

        Private Sub CreateAnalysisStructureDropdown()
            If Me.ShowAnalysisStructure Then
                Dim ddl As DropDownList
                'add the analysis structure too
                ddl = New DropDownList
                ddl.ID = "_ddl_analysis_structure"
                ddl.AutoPostBack = True
                AddHandler ddl.SelectedIndexChanged, AddressOf SelectedStructureChanged
                Me.Controls.Add(ddl)

                If Not Page.IsPostBack Then
                    'do the analysis structure if required
                    'this has to be databound first because the end position depends upon the analyis structure
                    Dim ls_sql As String
                    Dim DT As New DataTable
                    ls_sql = "SELECT analysis_structure " _
                         + "  FROM analysis_structure "
                    DT = SawUtil.FillTable(ls_sql)
                    ddl.DataTextField = "analysis_structure"
                    ddl.DataValueField = "analysis_structure"
                    ddl.DataSource = DT
                    ddl.DataBind()

                    Me.AnalysisStructure = ddl.SelectedValue
                End If
            End If
        End Sub

        Private Sub CreateDropDowns(Optional ByVal viewHasChanged As Boolean = False)
            Dim ddl As DropDownList
            'clear the list of dropdowns
            Me._lists.Clear()
            Me._phDropDowns.Controls.Clear()

            For i As Integer = Me.StartPosition To Me.EndPosition
                ddl = New DropDownList
                ddl.ID = "_ddl_" + i.ToString
                ddl.AutoPostBack = True
                AddHandler ddl.SelectedIndexChanged, AddressOf SelectedIndexChanged
                _lists.Add(i, ddl)
                Me._phDropDowns.Controls.Add(ddl)
            Next

            'only populate the dropdowns if not postback or the underlying view has changed
            If viewHasChanged Then
                'if not a postback then populate all of the dropdowns
                For Each kvp As KeyValuePair(Of Integer, DropDownList) In _lists
                    'dont do analysis structure
                    If kvp.Key > -1 Then
                        PopulateDropDown(kvp.Value, kvp.Key)
                    End If
                Next
            End If

        End Sub

        Private Sub PopulateDropDowns()
            'if not a postback then populate all of the dropdowns
            For Each kvp As KeyValuePair(Of Integer, DropDownList) In _lists
                PopulateDropDown(kvp.Value, kvp.Key)
            Next
            ctx.session("__analysis_filter") = Me.AnalysisCode
        End Sub

        Private Sub PopulateDropDowns(ByVal analysis_code As String)
            'if not a postback then populate all of the dropdowns, passing in the analysis code to select
            Dim code As String = String.Empty
            For Each kvp As KeyValuePair(Of Integer, DropDownList) In _lists
                'only select the analysis code if the struture position is >= kvp.Key(structure position of dropdown)
                If Integer.Parse(Me.StructurePosition) >= kvp.Key Then
                    code = AnalysisCodeAtPosition(analysis_code, kvp.Key)
                    PopulateDropDown(kvp.Value, kvp.Key, code)
                Else
                    PopulateDropDown(kvp.Value, kvp.Key)
                End If
            Next
            ctx.session("__analysis_filter") = Me.AnalysisCode
        End Sub

        'to be called after the analysis view has been changed
        Public Sub SelectAnalysisView(ByVal analysis_view As String)
            Me.AnalysisView = analysis_view
            SetAnalysisView()
            'recreate & repopulate the dropdowns
            If Page.IsPostBack Then
                CreateDropDowns()
                PopulateDropDowns()
            End If
        End Sub

        Public Sub SelectAnalysisView(ByVal analysis_view As String, ByVal person_ref As String, ByVal emp_ref As String)
            Me.AnalysisView = analysis_view
            SetAnalysisView()
            CreateDropDowns()
            Dim code As String = SawDB.ExecScalar("SELECT ISNULL(dbo.dbf_analysis_code('" + person_ref + "','" + emp_ref + "',getdate(),'" + Me.AnalysisStructure + "'," + Me.EndPosition.ToString + "),'')")
            If String.IsNullOrEmpty(code) Then
                PopulateDropDowns()
            Else
                PopulateDropDowns(code)
            End If
        End Sub

        'used to select a particular analysis code after the dropdowns have been created and populated
        Public Sub SelectAnalysisCode(ByVal analysis_code As String)
            'find the starting structure position, split the analysis_code into it's relevant position/codes
            'go through each dropdown           
            Dim ddl As DropDownList
            Dim codeFound As Boolean

            For Each kvp As KeyValuePair(Of Integer, DropDownList) In _lists
                'kvp=structure position, doprdownlist
                'get the sp code length for this position and remove form code string
                codeFound = False
                ddl = kvp.Value
                Dim code As String = String.Empty
                code = AnalysisCodeAtPosition(analysis_code, kvp.Key)

                'try and select this analysis code in the dropdown
                For Each i As ListItem In ddl.Items
                    If i.Value = code Then
                        'go through and unselect any that are selected
                        ddl.SelectedValue = code
                        codeFound = True
                        Exit For
                    End If
                Next

                If Not codeFound Then
                    'analysis code not found in dropdown
                    Exit For
                End If

            Next

        End Sub

        'populate a dropdown based on structure position
        Private Sub PopulateDropDown(ByVal ddl As DropDownList, ByVal structure_position As Integer, Optional ByVal selected_analysis_code As String = Nothing)
            Dim ls_sql As String
            Dim DT As New DataTable
            Dim analysis_code As String = String.Empty

            'if the structure position is not the start then get the analysis code
            If structure_position > Me.StartPosition Then
                analysis_code = AnalysisCodeAtPosition(structure_position - 1)
            End If

            'the analyis structure dropdown is built in create dropdowns
            If structure_position = -1 Then Return

            ls_sql = "SELECT analysis_code, " _
                   + "       detail_name + ' (' + analysis_code + ')' as detail_name " _
                   + "  FROM analysis_code " _
                   + " WHERE analysis_structure = ?? " _
                   + "   AND structure_position = ?? " _
                   + "   AND analysis_code LIKE '" + analysis_code + "%' "

            DT = SawUtil.FillTable(ls_sql, New Object() {Me.AnalysisStructure, structure_position})
            ddl.DataTextField = "detail_name"
            ddl.DataValueField = "analysis_code"
            ddl.DataSource = DT
            ddl.DataBind()

            'set the selected value based on the analysis code passed in
            If Not String.IsNullOrEmpty(selected_analysis_code) Then
                'try and select this analysis code in the dropdown
                Dim code As String = selected_analysis_code
                For Each i As ListItem In ddl.Items
                    If i.Value = code Then
                        ddl.SelectedValue = code
                        Exit For
                    End If
                Next
            End If

        End Sub

#End Region

#Region "private helper functions"

        'used when a new instance is created or when the underlying analysis view has changed
        'sets memeber variables and recreates the dropdowns
        Private Sub SetAnalysisView()
            Dim ls_sql As String
            Dim DB As New SawDB
            ls_sql = "SELECT AV.structure_position, " _
                   + "       AV.analysis_structure, " _
                   + "       MAX(SP.structure_position) AS max_position " _
                   + "  FROM analysis_view AV, " _
                   + "       structure_position SP " _
                   + " WHERE AV.analysis_view = ?? " _
                   + "   AND SP.analysis_structure = AV.analysis_structure " _
                   + " GROUP BY AV.structure_position, " _
                   + "       AV.analysis_structure "
            DB.Query(ls_sql, New Object() {Me.AnalysisView})
            If DB.Read Then
                Me.AnalysisStructure = DB.reader("analysis_structure")
                Me.StructurePosition = DB.reader("structure_position")
                Me.StartPosition = DB.reader("structure_position")
                Me.EndPosition = DB.reader("structure_position")
            End If
            DB.Close()
            DB = Nothing

            'when using a view always show the structure position one level above the view
            If Me.StartPosition >= 3 Then
                Me.StartPosition -= 2
                Me.EndPosition -= 1
            ElseIf Me.StartPosition = 1 Or Me.StartPosition = 2 Then
                Me.StartPosition = 1
                Me.EndPosition = 1
            End If

        End Sub

        'returns the analysis code at a structure position 
        Private Function AnalysisCodeAtPosition(ByVal structure_position As Integer) As String
            Dim ddl As DropDownList = _lists(structure_position)
            If ddl IsNot Nothing AndAlso ddl.Items.Count > 0 Then
                Return ddl.SelectedValue
            End If
            Return String.Empty
        End Function

        'returns the structure position of a dropdown based on its id
        Private Function GetStructurePosition(ByVal ddl As DropDownList) As Integer
            Dim sp As String = ddl.ID.Substring(ddl.ID.LastIndexOf("_") + 1)
            Dim i As Integer
            If Integer.TryParse(sp, i) Then
                Return i
            Else
                Return -1
            End If
        End Function

        'returns the particular analysis code at a structure position froma given analysis code
        Private Function AnalysisCodeAtPosition(ByVal analysis_code As String, ByVal structure_position As Integer) As String
            Dim ls_sql As String
            Dim len As Integer
            ls_sql = "SELECT SUM(SP.length) " _
                           + "  FROM structure_position SP " _
                           + " WHERE SP.analysis_structure = ?? " _
                           + "   AND SP.structure_position <= ?? "
            len = SawDB.ExecScalar(ls_sql, New Object() {Me.AnalysisStructure, structure_position})
            Return analysis_code.Substring(0, len)
        End Function

#End Region

#Region "click and event handlers etc"

        'handle change of analysis structure
        Private Sub SelectedStructureChanged(ByVal sender As Object, ByVal e As EventArgs)
            Me.AnalysisStructure = CType(sender, DropDownList).SelectedValue
            Me.EndPosition = SawDB.ExecScalar("SELECT max(structure_position) FROM structure_position WHERE analysis_structure = '" + Me.AnalysisStructure + "' ")
            Me.StructurePosition = Me.EndPosition
            CreateDropDowns()
            PopulateDropDowns()
        End Sub

        'handle drop down change
        Private Sub SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
            'get the structure position from the control id
            'build all the dropdowns that are at a lower level
            Dim ddl As DropDownList = CType(sender, DropDownList)
            Dim structure_position As Integer = GetStructurePosition(ddl)
            If structure_position > -1 And structure_position < Me.EndPosition Then
                'no need to repopulate if tis the last dropdown
                For i As Integer = structure_position + 1 To Me.EndPosition
                    PopulateDropDown(_lists(i), i)
                Next
            End If

            ctx.session("__analysis_filter") = Me.AnalysisCode

            Raise_AnalysisCodeChanged(EventArgs.Empty)
        End Sub

        'raise AnalysisCodeChanged event
        Protected Friend Sub Raise_AnalysisCodeChanged(ByVal e As EventArgs)
            RaiseEvent AnalysisCodeChanged(Me, e)
        End Sub

#End Region

    End Class


End Namespace