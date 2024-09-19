
Public Class IAWSiteMapNode
    Inherits SiteMapNode

    Private _process_ref As String
    Private _visibilty As String

    Public Property process_ref() As String
        Get
            Return _process_ref
        End Get
        Set(ByVal value As String)
            _process_ref = value
        End Set
    End Property

    Public Property visibilty() As String
        Get
            Return _visibilty
        End Get
        Set(ByVal value As String)
            _visibilty = value
        End Set
    End Property

    Public ReadOnly Property urlType() As IAWDerivationType
        Get
            If MyBase.Url <> "" Then
                If (MyBase.Url.ToLower.Trim.StartsWith("select") Or MyBase.Url.ToLower.Trim.StartsWith("if")) And MyBase.Url.Length > 15 Then Return IAWDerivationType.sql
                If MyBase.Url.ToLower.Trim.StartsWith("^") Then Return IAWDerivationType.derived
                If MyBase.Url.Length = 1 Then Return IAWDerivationType.plaintext
            End If
            Return IAWDerivationType.none
        End Get
    End Property

    Public Property nodeAttributes() As System.Collections.Specialized.NameValueCollection
        Get
            If MyBase.Attributes Is Nothing Then
                MyBase.Attributes = New NameValueCollection
            End If
            Return MyBase.Attributes
        End Get
        Set(ByVal value As System.Collections.Specialized.NameValueCollection)
            If MyBase.Attributes Is Nothing Then
                MyBase.Attributes = New NameValueCollection
            End If
            MyBase.Attributes = value
        End Set
    End Property

    'Public Sub New(ByVal provider As System.Web.SiteMapProvider, ByVal key As String)
    '    MyBase.new(provider, key)
    '    init()
    'End Sub

    'Public Sub New(ByVal provider As System.Web.SiteMapProvider, ByVal key As String, ByVal url As String)
    '    MyBase.new(provider, key, url)
    '    init()
    'End Sub
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="provider"></param>
    ''' <param name="key"></param>
    ''' <param name="url"></param>
    ''' <param name="title"></param>
    Public Sub New(ByVal provider As System.Web.SiteMapProvider, ByVal key As String, ByVal url As String, ByVal title As String)
        MyBase.New(provider, key, url, title)
        init()
    End Sub
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="provider"></param>
    ''' <param name="key"></param>
    ''' <param name="url"></param>
    ''' <param name="title"></param>
    ''' <param name="description"></param>
    Public Sub New(ByVal provider As System.Web.SiteMapProvider, ByVal key As String, ByVal url As String, ByVal title As String, ByVal description As String)
        MyBase.New(provider, key, url, title, description)
        init()
    End Sub

    'Public Sub New(ByVal provider As System.Web.SiteMapProvider, ByVal key As String, ByVal url As String, ByVal title As String, ByVal description As String, ByVal roles As System.Collections.IList, ByVal attributes As System.Collections.Specialized.NameValueCollection, ByVal explicitResourceKeys As System.Collections.Specialized.NameValueCollection, ByVal implicitResourceKey As String)
    '    MyBase.new(provider, key, url, title, description, roles, attributes, explicitResourceKeys, implicitResourceKey)
    '    init()
    'End Sub

    Private Sub init()
        _process_ref = String.Empty
        _visibilty = String.Empty
    End Sub




End Class
