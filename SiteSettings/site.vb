Imports System.Xml


Public Class Site

    Public Enum EnableEmailErrors
        NoEmail = 0
        EnableEmail = 1
        AutoEmail = 2
    End Enum

#Region "member variables"
    Private _name As String
    Private _theme As String
    Private _dbServer As String
    Private _dbName As String
    Private _dbConnection As String
    Private _autheticationMethod As String
    Private _activeDirectoryPath As String
    Private _activeDirectoryDomains As String
    Private _ssoProviderID As String
    Private _ssoProviderURL As String
    Private _ssoProviderLogoutURL As String
    Private _ssoProviderCertFile As String
    Private _ssoConsumerID As String
    Private _ssoConsumerURL As String
    Private _ssoWantSAMLResponseSigned As Boolean
    Private _ssoWantAssertionSigned As Boolean
    Private _emailErrors As EnableEmailErrors
    Private _emailErrorsTo As String
    Private _browserTitle As String
    Private _errorMessage As String
    Private _serverTimeout As Integer
    Private _innactiveTimeout As Integer
    Private _useLanguage As Boolean
    Private _maxPoolSize As Integer
    Private _minPoolSize As Integer
    Private _connectionTimeout As Integer
    Private _commandTimeout As Integer
    Private _ProfileUser As String
    Private _proxyuse As Boolean
    Private _proxyuri As String
    Private _proxyport As Integer
    Private _proxyuser As String
    Private _proxypassword As String
    Private _proxydomain As String
#End Region

#Region "properties"

    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
        End Set
    End Property

    Public Property Theme() As String
        Get
            Return _theme
        End Get
        Set(ByVal value As String)
            _theme = value
        End Set
    End Property

    Public Property DbServer() As String
        Get
            Return _dbServer
        End Get
        Set(ByVal value As String)
            _dbServer = value
        End Set
    End Property

    Public Property DbName() As String
        Get
            Return _dbName
        End Get
        Set(ByVal value As String)
            _dbName = value
        End Set
    End Property

    Public Property DbConnection() As String
        Get
            Return _dbConnection
        End Get
        Set(ByVal value As String)
            _dbConnection = value
        End Set
    End Property

    Public Property MaxPoolSize() As Integer
        Get
            Return _maxPoolSize
        End Get
        Set(ByVal value As Integer)
            _maxPoolSize = value
        End Set
    End Property

    Public Property MinPoolSize() As Integer
        Get
            Return _minPoolSize
        End Get
        Set(ByVal value As Integer)
            _minPoolSize = value
        End Set
    End Property

    Public Property ConnectionTimeout() As Integer
        Get
            Return _connectionTimeout
        End Get
        Set(ByVal value As Integer)
            _connectionTimeout = value
        End Set
    End Property

    Public Property CommandTimeout() As Integer
        Get
            Return _commandTimeout
        End Get
        Set(ByVal value As Integer)
            _commandTimeout = value
        End Set
    End Property

    Public Property ProfileUser() As String
        Get
            Return _ProfileUser
        End Get
        Set(ByVal value As String)
            _ProfileUser = value
        End Set
    End Property

    Public Property AutheticationMethod() As String
        Get
            Return _autheticationMethod
        End Get
        Set(ByVal value As String)
            _autheticationMethod = value
        End Set
    End Property

    Public Property ActiveDirectoryPath() As String
        Get
            Return _activeDirectoryPath
        End Get
        Set(ByVal value As String)
            _activeDirectoryPath = value
        End Set
    End Property

    Public Property ActiveDirectoryDomains() As String
        Get
            Return _activeDirectoryDomains
        End Get
        Set(ByVal value As String)
            _activeDirectoryDomains = value
        End Set
    End Property

    Public Property SsoProviderID() As String
        Get
            Return _ssoProviderID
        End Get
        Set(ByVal value As String)
            _ssoProviderID = value
        End Set
    End Property

    Public Property SsoProviderURL() As String
        Get
            Return _ssoProviderURL
        End Get
        Set(ByVal value As String)
            _ssoProviderURL = value
        End Set
    End Property

    Public Property SsoProviderLogoutURL() As String
        Get
            Return _ssoProviderLogoutURL
        End Get
        Set(ByVal value As String)
            _ssoProviderLogoutURL = value
        End Set
    End Property

    Public Property SsoProviderCertFile() As String
        Get
            Return _ssoProviderCertFile
        End Get
        Set(ByVal value As String)
            _ssoProviderCertFile = value
        End Set
    End Property

    Public Property SsoComsumerID() As String
        Get
            Return _ssoConsumerID
        End Get
        Set(ByVal value As String)
            _ssoConsumerID = value
        End Set
    End Property

    Public Property SsoConsumerURL() As String
        Get
            Return _ssoConsumerURL
        End Get
        Set(ByVal value As String)
            _ssoConsumerURL = value
        End Set
    End Property

    Public Property SsoWantSAMLResponseSigned As Boolean
        Get
            Return _ssoWantSAMLResponseSigned
        End Get
        Set(value As Boolean)
            _ssoWantSAMLResponseSigned = value
        End Set
    End Property

    Public Property SsoWantAssertionSigned As Boolean
        Get
            Return _ssoWantAssertionSigned
        End Get
        Set(value As Boolean)
            _ssoWantAssertionSigned = value
        End Set
    End Property

    Public Property EmailErrors() As Integer
        Get
            Return _emailErrors
        End Get
        Set(ByVal value As Integer)
            _emailErrors = value
        End Set
    End Property

    Public Property EmailErrorsEnabled() As String
        Get
            Return Me._emailErrors.ToString
        End Get
        Set(ByVal value As String)
            Select Case value
                Case "NoEmail"
                    Me._emailErrors = EnableEmailErrors.NoEmail
                Case "EnableEmail"
                    Me._emailErrors = EnableEmailErrors.EnableEmail
                Case "AutoEmail"
                    Me._emailErrors = EnableEmailErrors.AutoEmail
                Case Else
            End Select
        End Set
    End Property

    Public Property EmailErrorsTo() As String
        Get
            Return _emailErrorsTo
        End Get
        Set(ByVal value As String)
            _emailErrorsTo = value
        End Set
    End Property

    Public Property BrowserTitle() As String
        Get
            Return _browserTitle
        End Get
        Set(ByVal value As String)
            _browserTitle = value
        End Set
    End Property

    Public Property ErrorMessage() As String
        Get
            Return _errorMessage
        End Get
        Set(ByVal value As String)
            _errorMessage = value
        End Set
    End Property

    Public Property ServerTimeout() As Integer
        Get
            Return _serverTimeout
        End Get
        Set(ByVal value As Integer)
            _serverTimeout = value
        End Set
    End Property

    Public Property InnactiveTimeout() As Integer
        Get
            Return _innactiveTimeout
        End Get
        Set(ByVal value As Integer)
            _innactiveTimeout = value
        End Set
    End Property

    Public Property UseLanguage() As Boolean
        Get
            Return _useLanguage
        End Get
        Set(ByVal value As Boolean)
            _useLanguage = value
        End Set
    End Property

    Public Property ProxyUse() As Boolean
        Get
            Return _proxyuse
        End Get
        Set(ByVal value As Boolean)
            _proxyuse = value
        End Set
    End Property

    Public Property ProxyURI() As String
        Get
            Return _proxyuri
        End Get
        Set(ByVal value As String)
            _proxyuri = value
        End Set
    End Property

    Public Property ProxyPort() As Integer
        Get
            Return _proxyport
        End Get
        Set(ByVal value As Integer)
            _proxyport = value
        End Set
    End Property

    Public Property ProxyUser() As String
        Get
            Return _proxyuser
        End Get
        Set(ByVal value As String)
            _proxyuser = value
        End Set
    End Property

    Public Property ProxyPassword() As String
        Get
            Return _proxypassword
        End Get
        Set(ByVal value As String)
            _proxypassword = value
        End Set
    End Property

    Public Property ProxyDomain() As String
        Get
            Return _proxydomain
        End Get
        Set(ByVal value As String)
            _proxydomain = value
        End Set
    End Property

#End Region

    Public Sub New()
        init()
    End Sub

    Public Sub New(ByVal settings As XmlNode)
        init()
        populate(settings)
    End Sub

    Private Sub init()
        Name = String.Empty
        Theme = String.Empty
        DbServer = String.Empty
        DbName = String.Empty
        DbConnection = String.Empty
        MinPoolSize = 0
        MaxPoolSize = 100
        ConnectionTimeout = 15
        CommandTimeout = 30
        ServerTimeout = 20
        InnactiveTimeout = 20
        ProfileUser = String.Empty
        AutheticationMethod = "Forms"
        ActiveDirectoryPath = String.Empty
        ActiveDirectoryDomains = String.Empty
        SsoProviderID = String.Empty
        SsoProviderURL = String.Empty
        SsoProviderCertFile = "~/Certificates/provided.cer"
        SsoProviderLogoutURL = String.Empty
        SsoComsumerID = "https://iawresources.co.uk/sp"
        SsoConsumerURL = "https://yourdomain/security/sso.aspx"
        SsoWantSAMLResponseSigned = False
        SsoWantAssertionSigned = True
        EmailErrors = EnableEmailErrors.NoEmail
        EmailErrorsTo = String.Empty
        BrowserTitle = String.Empty
        ErrorMessage = "Please contact your technical support team."
        ProxyUse = False
        ProxyURI = String.Empty
        ProxyPort = 8080
        ProxyUser = String.Empty
        ProxyPassword = String.Empty
        ProxyDomain = String.Empty
    End Sub

    Private Sub populate(ByVal settings As XmlNode)
        Name = settings.Attributes("name").Value
        If settings.HasChildNodes Then
            Dim nodes As XmlNodeList = settings.ChildNodes
            Dim node As XmlNode
            Dim value As String
            For Each node In nodes
                value = node.InnerText
                Select Case node.Name
                    Case "theme"
                        Theme = value
                    Case "db_server"
                        DbServer = value
                    Case "db_name"
                        DbName = value
                    Case "db_connection"
                        DbConnection = value
                    Case "min_pool_size"
                        MinPoolSize = value
                    Case "max_pool_size"
                        MaxPoolSize = value
                    Case "connection_timeout"
                        ConnectionTimeout = value
                    Case "command_timeout"
                        CommandTimeout = value
                    Case "profileuser"
                        ProfileUser = value
                    Case "authentication_method"
                        AutheticationMethod = value
                    Case "active_dir_path"
                        ActiveDirectoryPath = value
                    Case "active_directory_domains"
                        ActiveDirectoryDomains = value
                    Case "sso_provider_id"
                        ssoProviderID = value
                    Case "sso_provider_url"
                        ssoProviderURL = value
                    Case "sso_provider_cert_file"
                        SsoProviderCertFile = value
                    Case "sso_provider_logout_url"
                        SsoProviderLogoutURL = value
                    Case "sso_consumer_id"
                        ssoComsumerID = value
                    Case "sso_consumer_url"
                        ssoConsumerURL = value
                    Case "sso_want_SAML_response_signed"
                        SsoWantSAMLResponseSigned = value = "true"
                    Case "sso_want_assertion_signed"
                        SsoWantAssertionSigned = value = "true"
                    Case "email_errors"
                        EmailErrorsEnabled = value
                    Case "email_errors_to"
                        EmailErrorsTo = value
                    Case "browser_title"
                        BrowserTitle = value
                    Case "error_message"
                        ErrorMessage = value
                    Case "timeout_interval"
                        ServerTimeout = value
                    Case "innactivity_timeout"
                        InnactiveTimeout = value
                    Case "use_language"
                        UseLanguage = value = "true"
                    Case "proxy_use"
                        ProxyUse = value = "true"
                    Case "proxy_uri"
                        ProxyURI = value
                    Case "proxy_port"
                        ProxyPort = value
                    Case "proxy_user"
                        ProxyUser = decrypt(value)
                    Case "proxy_password"
                        ProxyPassword = decrypt(value)
                    Case "proxy_domain"
                        ProxyDomain = value
                End Select
            Next
        End If
    End Sub

    Public Sub WriteXml(ByVal writer As XmlWriter)
        writer.WriteStartElement("site")
        writer.WriteAttributeString("name", Me.Name)
        writer.WriteElementString("theme", Me.Theme)
        writer.WriteElementString("db_server", Me.DbServer)
        writer.WriteElementString("db_name", Me.DbName)
        writer.WriteElementString("db_connection", Me.DbConnection)
        writer.WriteElementString("min_pool_size", Me.MinPoolSize.ToString)
        writer.WriteElementString("max_pool_size", Me.MaxPoolSize.ToString)
        writer.WriteElementString("connection_timeout", Me.ConnectionTimeout.ToString)
        writer.WriteElementString("command_timeout", Me.CommandTimeout.ToString)
        writer.WriteElementString("profileuser", Me.ProfileUser)
        writer.WriteElementString("authentication_method", Me.AutheticationMethod)
        Select Case Me.AutheticationMethod
            Case "Active Directory"
                writer.WriteElementString("active_dir_path", Me.ActiveDirectoryPath)
                writer.WriteElementString("active_directory_domains", Me.ActiveDirectoryDomains)
            Case "Single Sign On"
                writer.WriteElementString("sso_provider_id", Me.ssoProviderID)
                writer.WriteElementString("sso_provider_url", Me.ssoProviderURL)
                writer.WriteElementString("sso_provider_cert_file", Me.SsoProviderCertFile)
                writer.WriteElementString("sso_provider_logout_url", Me.SsoProviderLogoutURL)
                writer.WriteElementString("sso_consumer_id", Me.ssoComsumerID)
                writer.WriteElementString("sso_consumer_url", Me.SsoConsumerURL)
                writer.WriteElementString("sso_want_SAML_response_signed", If(Me.SsoWantSAMLResponseSigned, "true", "false"))
                writer.WriteElementString("sso_want_assertion_signed", If(Me.SsoWantAssertionSigned, "true", "false"))
        End Select
        writer.WriteElementString("email_errors", Me.EmailErrorsEnabled)
        writer.WriteElementString("email_errors_to", Me.EmailErrorsTo)
        writer.WriteElementString("browser_title", Me.BrowserTitle)
        writer.WriteElementString("error_message", Me.ErrorMessage)
        writer.WriteElementString("timeout_interval", Me.ServerTimeout.ToString)
        writer.WriteElementString("innactivity_timeout", Me.InnactiveTimeout.ToString)
        writer.WriteElementString("use_language", If(Me.UseLanguage, "true", "false"))
        writer.WriteElementString("proxy_use", If(Me.ProxyUse, "true", "false"))
        If ProxyUse Then
            writer.WriteElementString("proxy_uri", Me.ProxyURI)
            writer.WriteElementString("proxy_port", Me.ProxyPort.ToString)
            writer.WriteElementString("proxy_user", encrypt(Me.ProxyUser))
            writer.WriteElementString("proxy_password", encrypt(Me.ProxyPassword))
            writer.WriteElementString("proxy_domain", Me.ProxyDomain)
        End If
        writer.WriteEndElement()
    End Sub

    Public Shared Function encrypt(ByVal aString As String) As String
        Dim cryptographer As New Cryptography
        Return cryptographer.encrypt(aString)
    End Function

    Public Shared Function decrypt(ByVal aHexString As String) As String
        Dim cryptographer As New Cryptography
        Return cryptographer.decrypt(aHexString)
    End Function

End Class
