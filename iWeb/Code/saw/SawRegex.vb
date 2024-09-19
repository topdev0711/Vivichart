Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic

Public Class SawRegex

#Region "private members"
    Private _isNumber As Boolean
    Private _isDecmal As Boolean
    Private _mask As String
    Private _message As String
    Private _value As String
    Private _regex As String
    Private _isValidMask As Boolean
    Private _errorMessage As String
    Private _isIawMask As Boolean
#End Region

#Region "properties"

    Public Property IsNumber() As Boolean
        Get
            Return _isNumber
        End Get
        Set(ByVal value As Boolean)
            _isNumber = value
        End Set
    End Property

    Public Property IsDecmal() As Boolean
        Get
            Return _isDecmal
        End Get
        Set(ByVal value As Boolean)
            _isDecmal = value
        End Set
    End Property

    Public Property Mask() As String
        Get
            Return _mask
        End Get
        Set(ByVal value As String)
            _mask = value
        End Set
    End Property

    Private Property Message() As String
        Get
            Return _message
        End Get
        Set(ByVal value As String)
            _message = value
        End Set
    End Property

    Public Property Value() As String
        Get
            Return _value
        End Get
        Set(ByVal value As String)
            _value = value.Replace(Chr(13), "").Replace(Chr(10), "")
        End Set
    End Property

    Public Property RegexString() As String
        Get
            Return _regex
        End Get
        Set(ByVal value As String)
            _regex = value
        End Set
    End Property

    Public Property IsValidMask() As Boolean
        Get
            Return _isValidMask
        End Get
        Set(ByVal value As Boolean)
            _isValidMask = value
        End Set
    End Property

    Public Property IsIawMask() As Boolean
        Get
            Return _isIawMask
        End Get
        Set(ByVal value As Boolean)
            _isIawMask = value
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

#End Region

    Public Sub New(ByVal aMask As String, ByVal isIawMask As Boolean)
        init(aMask, String.Empty, isIawMask)
    End Sub

    Public Sub New(ByVal aMask As String, ByVal aValue As String, ByVal isIawMask As Boolean)
        init(aMask, aValue, isIawMask)
    End Sub

    Private Sub init(ByVal aMask As String, ByVal aValue As String, ByVal aIsIawMask As Boolean)
        _mask = aMask
        _regex = String.Empty
        _errorMessage = String.Empty
        _message = String.Empty
        _value = aValue.Replace(Chr(13), "").Replace(Chr(10), "")
        _isIawMask = aIsIawMask
        buildRegex()
    End Sub

    Private Sub buildRegex()
        If IsIawMask Then
            ' now we're dealing with a system validation we build a dynamic regex expression
            ' for each char.
            Dim lchar As Char
            Dim ls_regex, ls_msg As String

            ' convert to regex
            ls_regex = String.Empty
            ls_msg = String.Empty
            IsNumber = True
            For Each lchar In Mask.ToLower
                Select Case lchar
                    Case ","c, "."c
                        'use the decimal separator for the current culture
                        Dim sep As String = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator
                        '///if not the same as current deciaml seperator ignore
                        If lchar.ToString = sep Then
                            IsDecmal = True
                            ls_regex += "(\" + sep
                            ls_msg += sep
                        End If
                    Case "#"c
                        ls_regex += "\d{0,1}"
                        ls_msg += "n"
                    Case "0"c
                        If IsDecmal Then
                            ls_regex += "\d{0,1}"
                        Else
                            ls_regex += "\d"
                        End If
                        ls_msg += "n"
                    Case "?"c
                        IsNumber = False
                        ls_regex += ".{0,1}"
                        ls_msg += "."
                    Case "^"
                        IsNumber = False
                        ls_regex += "[A-Z]"
                        ls_msg += "X"
                    Case "!"
                        IsNumber = False
                        ls_regex += "[a-z]"
                        ls_msg += "x"
                    Case "x"
                        IsNumber = False
                        ls_regex += "[A-Za-z]{0,1}"
                        ls_msg += "x"
                    Case Else
                        ErrorMessage = Mask + " " + ctx.Translate("::LT_S0366") ' is an invalid mask
                        Return
                End Select
            Next

            If IsNumber Then
                If IsDecmal Then
                    ls_regex += ")?"
                End If
                ls_regex = ("^[-]{0,1}" + ls_regex + "$")
                ErrorMessage = ctx.Translate("::LT_S0367") ' Field is either non-numeric or too large
            Else
                ls_regex = ("^" + ls_regex + "$")
                'ErrorMessage = "Maximum of " + Me.Message.Length.ToString + " Characters"
                ErrorMessage = String.Format(ctx.Translate("::LT_S0368"), Me.Message.Length.ToString) ' Maximum of {0} Characters
            End If

            Me.Message = ls_msg
            Me.RegexString = ls_regex

        End If '///end if iaw mask
    End Sub

    Public Function isValid() As Boolean
        If String.IsNullOrEmpty(Value) Or String.IsNullOrEmpty(RegexString) Then Return True
        Dim lregex As Regex
        '///compare regex string with data 
        lregex = New Regex(RegexString)

        If Not lregex.IsMatch(Value) Then
            Return False
        End If

        Return True

    End Function

#Region "shared functions"

    Public Shared Function RegexFromMask(ByVal mask As String) As String
        Dim regex As New SawRegex(mask, True)
        Return regex.RegexString
    End Function

#End Region



End Class

