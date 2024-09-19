Imports System.Security.Principal
Imports System.Web.Security
Imports System.Collections
Imports System.Collections.Generic

Namespace IAW

    <Serializable()> _
    Public Class IawPrincipal
        Implements IPrincipal

        Private _identity As IIdentity
        Private _currentRole As String
        Private _defaultRole As String
        Private _roles As Dictionary(Of String, role)
        Private _CompanyID As Integer

        Public ReadOnly Property Identity() As System.Security.Principal.IIdentity Implements System.Security.Principal.IPrincipal.Identity
            Get
                Return _identity
            End Get
        End Property

        Public ReadOnly Property roles() As Dictionary(Of String, role)
            Get
                Return _roles
            End Get
        End Property

        Public ReadOnly Property listOfRole() As List(Of role)
            Get
                Dim list As New List(Of role)
                For Each DE As KeyValuePair(Of String, role) In Me._roles
                    list.Add(DE.Value)
                Next
                Return list
            End Get
        End Property

        Public ReadOnly Property currentRoleAsRole() As role
            Get
                If _roles.ContainsKey(Me.currentRole) Then
                    Return _roles(Me.currentRole)
                End If
                Return Nothing
            End Get
        End Property

        Public Property currentRole() As String
            Get
                Return _currentRole
            End Get
            Set(ByVal Value As String)
                If _roles.ContainsKey(Value) Then
                    _currentRole = Value
                End If
            End Set
        End Property

        Public ReadOnly Property currentRoleName() As String
            Get
                If _roles.ContainsKey(Me.currentRole) Then
                    Return _roles(Me.currentRole).name
                End If
                Return Nothing
            End Get
        End Property

        Public Property defaultRole() As String
            Get
                Return _defaultRole
            End Get
            Set(ByVal Value As String)
                If _roles.ContainsKey(Value) Then
                    _defaultRole = Value
                End If
            End Set
        End Property

        Private ReadOnly Property hasRole() As Boolean
            Get
                If _roles.ContainsKey(_currentRole) Then Return True
                Return False
            End Get
        End Property

        Public ReadOnly Property startPage() As String
            Get
                If hasRole() Then Return _roles(_currentRole).startPage
                Return String.Empty
            End Get
        End Property

        Public ReadOnly Property CompanyID As Integer
            Get
                Return _CompanyID
            End Get
        End Property

        Public Sub New(ByVal aIdentity As IIdentity, ByVal aDefaultRole As String, ByVal aRoles As Dictionary(Of String, role), CompanyID As Integer)
            _identity = aIdentity
            _roles = aRoles
            _defaultRole = aDefaultRole
            _CompanyID = CompanyID
            'check that the object actually has the default role
            If _roles.ContainsKey(_defaultRole) Then
                _currentRole = _defaultRole
            Else
                For Each kvp As KeyValuePair(Of String, role) In _roles
                    _currentRole = kvp.Key
                    _defaultRole = _currentRole
                    Exit For
                Next
            End If

        End Sub

        Public Function IsInRole(ByVal role As String) As Boolean Implements System.Security.Principal.IPrincipal.IsInRole
            If _currentRole <> role Then Return False
            Return hasRole
        End Function

        Public Function hasRightToInsert() As Boolean
            Return Me.hasRightInProcess(ctx.process, RightTo.insert)
        End Function
        Public Function hasRightToUpdate() As Boolean
            Return Me.hasRightInProcess(ctx.process, RightTo.update)
        End Function
        Public Function hasRightToDelete() As Boolean
            Return Me.hasRightInProcess(ctx.process, RightTo.delete)
        End Function

        Public Function hasRightInProcess(ByVal aRightTo As RightTo) As Boolean
            Return Me.hasRightInProcess(ctx.process, aRightTo)
        End Function

        Public Function hasRightInProcess(ByVal aProcessRef As String) As Boolean
            If hasRole Then
                Return _roles(_currentRole).hasRightInProcess(aProcessRef, RightTo.view)
            Else
                Return False
            End If
        End Function

        Public Function hasRightInProcess(ByVal aProcessRef As String, ByVal aRightTo As RightTo) As Boolean
            If hasRole Then
                Return _roles(_currentRole).hasRightInProcess(aProcessRef, aRightTo)
            Else
                Return False
            End If
        End Function

        Public Function hasViewRightOnlyInProcess(ByVal aProcessRef As String) As Boolean
            If hasRole Then
                Return _roles(_currentRole).hasRightToViewProcessOnly(aProcessRef)
            Else
                Return True
            End If
        End Function

    End Class


End Namespace







