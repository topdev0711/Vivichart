Imports Microsoft.VisualBasic

Public Class WebErrorLog

#Region "member variables"
    Private _id As Integer
    Private _message As String
    Private _date As Date
    Private _source As String
    Private _userRef As String
    Private _ipAddress As String
    Private _form As String
    Private _queryString As String
    Private _targetSite As String
    Private _stackTrace As String
    Private _referer As String
    Private _sql As String
    Private _sql_params As String
#End Region

#Region "properties"

    Public Property Id() As Integer
        Get
            Return _id
        End Get
        Set(ByVal value As Integer)
            _id = value
        End Set
    End Property
    Public Property Message() As String
        Get
            Return _message
        End Get
        Set(ByVal value As String)
            _message = value
        End Set
    End Property
    Public Property LogDate() As Date
        Get
            Return _date
        End Get
        Set(ByVal value As Date)
            _date = value
        End Set
    End Property
    Public Property Source() As String
        Get
            Return _source
        End Get
        Set(ByVal value As String)
            _source = value
        End Set
    End Property
    Public Property UserRef() As String
        Get
            Return _userRef
        End Get
        Set(ByVal value As String)
            _userRef = value
        End Set
    End Property
    Public Property IpAddress() As String
        Get
            Return _ipAddress
        End Get
        Set(ByVal value As String)
            _ipAddress = value
        End Set
    End Property
    Public Property Form() As String
        Get
            Return _form
        End Get
        Set(ByVal value As String)
            _form = value
        End Set
    End Property
    Public Property QueryString() As String
        Get
            Return _queryString
        End Get
        Set(ByVal value As String)
            _queryString = value
        End Set
    End Property
    Public Property TargetSite() As String
        Get
            Return _targetSite
        End Get
        Set(ByVal value As String)
            _targetSite = value
        End Set
    End Property
    Public Property StackTrace() As String
        Get
            Return _stackTrace
        End Get
        Set(ByVal value As String)
            _stackTrace = value
        End Set
    End Property
    Public Property Referer() As String
        Get
            Return _referer
        End Get
        Set(ByVal value As String)
            _referer = value
        End Set
    End Property
    Public Property Sql() As String
        Get
            Return _sql
        End Get
        Set(ByVal value As String)
            _sql = value
        End Set
    End Property
    Public Property SqlParams() As String
        Get
            Return _sql_params
        End Get
        Set(ByVal value As String)
            _sql_params = value
        End Set
    End Property
#End Region

#Region "constructors"
    Public Sub New()
        Id = 0
        Message = String.Empty
        LogDate = Date.Now
        Source = String.Empty
        UserRef = String.Empty
        IpAddress = String.Empty
    End Sub
#End Region

End Class
