Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Configuration.Provider
Imports System.Security.Permissions
Imports System.Web
Imports System.Data
Imports System.Configuration

Public Class IAWSiteMapProvider
    Inherits SiteMapProvider

    Private parentSiteMapProvider As SiteMapProvider = Nothing
    Private IAWProviderName As String = Nothing
    Private aRootNode As SiteMapNode = Nothing
    Private siteMapNodes As ArrayList = Nothing
    Private childParentRelationship As ArrayList = Nothing
    Private language As String = String.Empty

    ' A default constructor. The Name property is initialized in the
    ' Initialize method.
    Public Sub New()

    End Sub

    ' Implement the ProviderBase.Initialize method.
    ' Initialize is used to initialize the state that the Provider holds, but
    ' not actually build the site map.
    Public Overrides Sub Initialize(ByVal name As String, ByVal attributes As NameValueCollection)
        SyncLock Me
            MyBase.Initialize(name, attributes)
            IAWProviderName = name
            'arraylists now initialised in LoadSiteMapFromStore
            ' siteMapNodes = New ArrayList()
            ' childParentRelationship = New ArrayList()
            ' Build the site map in memory.

            LoadSiteMapFromStore()
            language = ctx.languageCode

        End SyncLock
    End Sub 'Initialize

    ' Implement the CurrentNode property.
    Public Overrides ReadOnly Property CurrentNode() As SiteMapNode
        Get
            Dim currentUrl As String = FindCurrentUrl()
            ' Find the SiteMapNode that represents the current page.

            Dim i As Integer
            For i = 0 To siteMapNodes.Count - 1
                Dim item As DictionaryEntry = CType(siteMapNodes(i), DictionaryEntry)
                If CType(item.Value, SiteMapNode).Url = currentUrl Then
                    Return CType(item.Value, SiteMapNode)
                End If
            Next i

            Return Nothing
        End Get
    End Property

    ' Implement the RootNode property.
    Public Overrides ReadOnly Property RootNode() As SiteMapNode
        Get
            Return aRootNode
        End Get
    End Property

    ' Implement the ParentProvider property.
    Public Overrides Property ParentProvider() As SiteMapProvider
        Get
            Return parentSiteMapProvider
        End Get
        Set(ByVal value As SiteMapProvider)
            parentSiteMapProvider = value
        End Set
    End Property

    ' Implement the RootProvider property.
    Public Overrides ReadOnly Property RootProvider() As SiteMapProvider
        Get
            ' If the current instance belongs to a provider hierarchy, it
            ' cannot be the RootProvider. Rely on the ParentProvider.
            If Not (Me.ParentProvider Is Nothing) Then
                Return ParentProvider.RootProvider
                ' If the current instance does not have a ParentProvider, it is
                ' not a child in a hierarchy, and can be the RootProvider.
            Else
                Return Me
            End If
        End Get
    End Property

    ' Implement the FindSiteMapNode method.
    Public Overrides Function FindSiteMapNode(ByVal key As String) As SiteMapNode
        ' Does the root node match the URL?
        If RootNode.Key = key Then
            Return RootNode
        Else
            Dim candidate As SiteMapNode = Nothing
            ' Retrieve the SiteMapNode that matches the URL.
            SyncLock Me
                candidate = GetNode(siteMapNodes, key)
            End SyncLock
            Return candidate
        End If
    End Function 'FindSiteMapNode

    ' Implement the GetChildNodes method.
    Public Overrides Function GetChildNodes(ByVal node As SiteMapNode) As SiteMapNodeCollection
        Dim children As New SiteMapNodeCollection()
        ' Iterate through the ArrayList and find all nodes that have the specified node as a parent.
        SyncLock Me
            Dim i As Integer
            For i = 0 To childParentRelationship.Count - 1

                Dim de As DictionaryEntry = CType(childParentRelationship(i), DictionaryEntry)
                Dim nodeKey As String = CType(de.Key, String)

                Dim parent As SiteMapNode = GetNode(childParentRelationship, nodeKey)

                If Not (parent Is Nothing) AndAlso node.Key = parent.Key Then
                    ' The SiteMapNode with the Url that corresponds to nodeUrl
                    ' is a child of the specified node. Get the SiteMapNode for
                    ' the nodeUrl.
                    Dim child As SiteMapNode = FindSiteMapNode(nodeKey)
                    If Not (child Is Nothing) Then
                        Dim ctx As HttpContext = HttpContext.Current
                        If IsAccessibleToUser(ctx, child) Then
                            children.Add(CType(child, SiteMapNode))
                        End If

                    Else
                        Throw New Exception("ArrayLists not in sync.")
                    End If
                End If
            Next i
        End SyncLock
        Return children
    End Function 'GetChildNodes

    Protected Overrides Function GetRootNodeCore() As SiteMapNode
        Return RootNode
    End Function ' GetRootNodeCore()

    ' Implement the GetParentNode method.
    Public Overrides Function GetParentNode(ByVal node As SiteMapNode) As SiteMapNode
        ' Check the childParentRelationship table and find the parent of the current node.
        ' If there is no parent, the current node is the RootNode.
        Dim parent As SiteMapNode = Nothing
        SyncLock Me
            ' Get the Value of the node in childParentRelationship
            parent = GetNode(childParentRelationship, node.Key)
        End SyncLock
        Return parent
    End Function 'GetParentNode

    Public Overrides Function IsAccessibleToUser(ByVal aContext As System.Web.HttpContext, ByVal aNode As System.Web.SiteMapNode) As Boolean
        If (aNode Is Nothing) Then
            Throw New ArgumentNullException("node reference cannot be nothing")
        End If
        If (aContext Is Nothing) Then
            Throw New ArgumentNullException("context reference cannot be nothing")
        End If

        Dim node As IAWSiteMapNode = CType(aNode, IAWSiteMapNode)
        Dim hasRight As Boolean = True

        'used by the ctx object to substitute the current process ref for the one on the menu object, used for analysis codes etc - role/process table
        ctx.session("USE_PROCESS") = node.process_ref

        'check visibilty property
        hasRight = node.visibilty = "1"

        If node.Key = "m_menu" Then
            Return True
        End If

        If ctx.user Is Nothing Then Return False
        If hasRight And Not ctx.user.hasRightInProcess(node.process_ref) Then
            hasRight = False
        End If

        ctx.session("USE_PROCESS") = Nothing
        Return hasRight

    End Function

    ' Private helper methods
    Private Function GetNode(ByVal list As ArrayList, ByVal key As String) As SiteMapNode
        Dim i As Integer

        For i = 0 To list.Count - 1
            Dim item As DictionaryEntry = CType(list(i), DictionaryEntry)
            If CStr(item.Key) = key Then
                Return CType(item.Value, SiteMapNode)
            End If
        Next i
        Return Nothing
    End Function 'GetNode

    ' Get the URL of the currently displayed page.
    Private Function FindCurrentUrl() As String
        Try
            ' The current HttpContext.
            Dim currentContext As HttpContext = HttpContext.Current
            If Not (currentContext Is Nothing) Then
                Return currentContext.Request.RawUrl
            Else
                Throw New Exception("HttpContext.Current is Invalid")
            End If
        Catch e As Exception
            Throw New NotSupportedException("This provider requires a valid context.", e)
        End Try
    End Function 'FindCurrentUrl

    Public Sub rebuildMenu()
        LoadSiteMapFromStore()
    End Sub

    Public Function GetMenuText(process_ref As String) As String
        Dim node As IAWSiteMapNode
        If String.IsNullOrEmpty(process_ref) Then Return ""
        For Each entry As DictionaryEntry In siteMapNodes
            node = DirectCast(entry.Value, SiteMapNode)
            If node.process_ref = process_ref Then
                Return node.Title
            End If
        Next
        Return ""
    End Function

    Protected Overridable Sub LoadSiteMapFromStore()
        SyncLock Me
            siteMapNodes = New ArrayList()
            childParentRelationship = New ArrayList()

            Dim DB As New SawDB()
            'build sql string
            Dim ls_sql As String
            ls_sql = "SELECT mnu_ref, 
                             mnu_seq, 
                             mnu_parent_ref, 
                             mnu_text, 
                             mnu_type, 
                             mnu_process, 
                             process_name, 
                             mnu_url, 
                             mnu_visible,
                             isNull(icon_char,'') as icon_char
                        FROM dbo.web_menu WITH (NOLOCK)
                             JOIN dbo.process P WITH (NOLOCK)
                               ON P.process_ref = mnu_process
                       ORDER BY mnu_parent_ref, mnu_seq "

            'execute sql and populate datatable
            Dim dt As New DataTable
            Dim dr As DataRow

            dt = DB.GetTable(ls_sql)

            'create root node - this is not displayed
            Dim root As New IAWSiteMapNode(Me, "root", "", "root") ', "root")
            aRootNode = root

            'For Each dr In dt.Rows
            For Each dr In dt.Select("mnu_parent_ref is NULL or mnu_parent_ref=''", "mnu_seq asc")
                'top level item
                Dim rootNode As SiteMapNode = AddMenuNode(dr)
                If DirectCast(dr.Item("mnu_type"), String) = "3" Then
                    'get the child nodes of this parent(rootNode)
                    childSiteMapNodes(dt, dr.Item("mnu_ref"), rootNode)
                End If
            Next
            DB.Close()
            DB = Nothing
        End SyncLock
    End Sub

    'recursive function
    'selects all the rows, in sequence order, from the datatable where the current_parent_ref = mnu_parent_ref
    'if it's a menu item then a new node is added and the function recurses using this new node as the current_node
    'if not a menu item then a new node is added as a child of the current_node
    Private Sub childSiteMapNodes(ByVal dt As DataTable, ByVal current_parent_ref As String, ByVal current_node As SiteMapNode)
        Dim dRow As DataRow
        For Each dRow In dt.Select("mnu_parent_ref='" & current_parent_ref & "'", "mnu_seq asc")
            Dim node As SiteMapNode = AddMenuNode(dRow, current_node)
            If DirectCast(dRow.Item("mnu_type"), String) = "3" Then
                childSiteMapNodes(dt, dRow.Item("mnu_ref"), node)
            End If
        Next
    End Sub

    'appends a new node to either the root(no ParentNode passed in) or to the  ParentNode that is passed in
    'returns the newly created node
    Private Function AddMenuNode(ByVal dRow As DataRow, Optional ByVal ParentNode As SiteMapNode = Nothing) As SiteMapNode
        'Dim smNode As SiteMapNode
        Dim smNode As IAWSiteMapNode
        Dim key As String = dRow("mnu_ref")
        Dim url As String = String.Empty
        Dim process As String = constants.processref + "=" + dRow("mnu_process")
        Dim icon As String = ""

        Select Case DirectCast(dRow("mnu_type"), String)
            Case "3" 'menu 
            Case "4" 'special
                Dim mnu_url As String = GetValue("mnu_url", dRow)
                Dim delimiter As String = String.Empty
                If mnu_url.Contains("?") And Not mnu_url.EndsWith("?") Then
                    delimiter = "&"
                ElseIf Not mnu_url.Contains("?") Then
                    delimiter = "?"
                End If
                url = HttpRuntime.AppDomainAppVirtualPath & "/" & mnu_url + delimiter + process
        End Select

        If dRow("mnu_type") <> "3" Then url = SawUtil.encryptQuery(url, True, True)

        Dim displayText As String = dRow("mnu_text")
        If displayText <> "Menu" Then
            icon = "<span class='menuIcon " + dRow("icon_char") + "'></span>"
            displayText = icon + SawLang.Translate(dRow("mnu_text"))
        End If

        smNode = New IAWSiteMapNode(Me, key, url, displayText)  ', displayText)
        smNode.process_ref = dRow("mnu_process").ToString
        smNode.visibilty = dRow("mnu_visible").ToString

        ' Is this a root node yet?
        If dRow("mnu_parent_ref") Is System.DBNull.Value OrElse _
           dRow("mnu_parent_ref") = String.Empty Then

            ' aRootNode = smNode
            siteMapNodes.Add(New DictionaryEntry(smNode.Key, smNode))
            childParentRelationship.Add(New DictionaryEntry(smNode.Key, aRootNode))

            ' If not the root node, add the node to the various collections.
        Else
            Dim parentKey As String = dRow("mnu_parent_ref")
            siteMapNodes.Add(New DictionaryEntry(smNode.Key, smNode))

            ' The parent node has already been added to the collection.
            Dim smNodeParent As SiteMapNode = FindSiteMapNode(parentKey)

            If Not (smNodeParent Is Nothing) Then
                childParentRelationship.Add(New DictionaryEntry(smNode.Key, smNodeParent))
            Else
                Throw New Exception("Parent node not found for current node." + dRow("mnu_ref"))
            End If
        End If

        ctx.trace("IAWSiteProvider", url)

        Return smNode
    End Function


    Public Function GetValue(ByVal aColumnName As String, ByVal DR As DataRow) As Object
        Try
            If DR(aColumnName) Is System.DBNull.Value Then
                Return String.Empty
            ElseIf DR(aColumnName) Is Nothing Then
                Return String.Empty
            Else
                Return DR(aColumnName)
            End If
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function


End Class

