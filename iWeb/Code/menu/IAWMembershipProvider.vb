Imports Microsoft.VisualBasic

Public Class IAWMembershipProvider
    Inherits MembershipProvider

    Private _strName As String
    Private _strApplicationName As String
    Private _boolEnablePasswordReset As Boolean
    Private _boolEnablePasswordRetrieval As Boolean
    Private _boolRequiresQuestionAndAnswer As Boolean
    Private _boolRequiresUniqueEMail As Boolean
    Private _iPasswordAttemptThreshold As Integer
    Private _oPasswordFormat As MembershipPasswordFormat

    Public Sub New()
        _strName = String.Empty
        _strApplicationName = String.Empty
        _boolRequiresQuestionAndAnswer = False
        _boolEnablePasswordReset = False
        _boolEnablePasswordRetrieval = False
        _boolRequiresQuestionAndAnswer = False
        _boolRequiresUniqueEMail = False
    End Sub

    'DFB: reads entries from web.config and initializes this class from those values
    '  Once the provider is loaded, the 
    '  runtime calls Initialize and passes the settings as name-value 
    '  pairs in an instance of the NameValueCollection class.
    '
    Public Overrides Sub Initialize(ByVal strName As String, ByVal config As System.Collections.Specialized.NameValueCollection)
        _strName = strName
        _strApplicationName = "/"
        _boolRequiresQuestionAndAnswer = False
        _boolEnablePasswordReset = True
        _boolEnablePasswordRetrieval = True
        _boolRequiresQuestionAndAnswer = False
        _boolRequiresUniqueEMail = True
    End Sub

    Public Overrides Function ValidateUser(ByVal username As String, ByVal password As String) As Boolean
        'have overriden authenticate method for login control ibnstead!!!
        Dim loginSuccess As Boolean

        Select Case SawAuth.ValidateUser(username, password)
            Case SawAuth.Validate.failed, SawAuth.Validate.locked_out
                loginSuccess = False
            Case else
                loginSuccess = True
        End Select

        Return loginSuccess

    End Function

#Region "non-implemented methods"
    Public Overrides Function ChangePassword(ByVal username As String, ByVal oldPassword As String, ByVal newPassword As String) As Boolean
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function ChangePasswordQuestionAndAnswer(ByVal username As String, ByVal password As String, ByVal newPasswordQuestion As String, ByVal newPasswordAnswer As String) As Boolean
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function CreateUser(ByVal username As String, ByVal password As String, ByVal email As String, ByVal passwordQuestion As String, ByVal passwordAnswer As String, ByVal isApproved As Boolean, ByVal providerUserKey As Object, ByRef status As System.Web.Security.MembershipCreateStatus) As System.Web.Security.MembershipUser
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function DeleteUser(ByVal username As String, ByVal deleteAllRelatedData As Boolean) As Boolean
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function FindUsersByEmail(ByVal emailToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As System.Web.Security.MembershipUserCollection
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function FindUsersByName(ByVal usernameToMatch As String, ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As System.Web.Security.MembershipUserCollection
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function GetAllUsers(ByVal pageIndex As Integer, ByVal pageSize As Integer, ByRef totalRecords As Integer) As System.Web.Security.MembershipUserCollection
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function GetNumberOfUsersOnline() As Integer
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function GetPassword(ByVal username As String, ByVal answer As String) As String
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overloads Overrides Function GetUser(ByVal username As String, ByVal userIsOnline As Boolean) As System.Web.Security.MembershipUser
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overloads Overrides Function GetUser(ByVal providerUserKey As Object, ByVal userIsOnline As Boolean) As System.Web.Security.MembershipUser
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function GetUserNameByEmail(ByVal email As String) As String
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function ResetPassword(ByVal username As String, ByVal answer As String) As String
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Function UnlockUser(ByVal userName As String) As Boolean
        Throw New Exception("The method or operation is not implemented.")
    End Function

    Public Overrides Sub UpdateUser(ByVal user As System.Web.Security.MembershipUser)
        Throw New Exception("The method or operation is not implemented.")
    End Sub

#End Region

#Region "non-implemented properties"
    Public Overrides ReadOnly Property MaxInvalidPasswordAttempts() As Integer
        Get
            Throw New Exception("The method or operation is not implemented.")
        End Get
    End Property

    Public Overrides ReadOnly Property MinRequiredNonAlphanumericCharacters() As Integer
        Get
            Throw New Exception("The method or operation is not implemented.")
        End Get
    End Property

    Public Overrides ReadOnly Property MinRequiredPasswordLength() As Integer
        Get
            Throw New Exception("The method or operation is not implemented.")
        End Get
    End Property

    Public Overrides ReadOnly Property PasswordAttemptWindow() As Integer
        Get
            Throw New Exception("The method or operation is not implemented.")
        End Get
    End Property

    Public Overrides ReadOnly Property PasswordStrengthRegularExpression() As String
        Get
            Throw New Exception("The method or operation is not implemented.")
        End Get
    End Property
#End Region

    Public Overrides ReadOnly Property Name() As String
        Get
            Return _strName
        End Get
    End Property

    Public Overrides Property ApplicationName() As String
        Get
            Return _strApplicationName
        End Get
        Set(ByVal value As String)
            _strApplicationName = value
        End Set
    End Property

    Public Overrides ReadOnly Property EnablePasswordReset() As Boolean
        Get
            Return _boolEnablePasswordReset
        End Get
    End Property

    Public Overrides ReadOnly Property EnablePasswordRetrieval() As Boolean
        Get
            Return _boolEnablePasswordRetrieval
        End Get
    End Property

    Public Overrides ReadOnly Property PasswordFormat() As System.Web.Security.MembershipPasswordFormat
        Get
            Return _oPasswordFormat
        End Get
    End Property

    Public Overrides ReadOnly Property RequiresQuestionAndAnswer() As Boolean
        Get
            Return _boolRequiresQuestionAndAnswer
        End Get
    End Property

    Public Overrides ReadOnly Property RequiresUniqueEmail() As Boolean
        Get
            Return _boolRequiresUniqueEMail
        End Get
    End Property


End Class
