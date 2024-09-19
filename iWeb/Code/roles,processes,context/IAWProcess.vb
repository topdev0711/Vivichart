Imports Microsoft.VisualBasic
Imports System.Collections.Generic

Namespace IAW

    <Serializable()> _
    Public Class IAWProcess

        Public Enum Implementation
            None = 1
            Auto = 2
            Intray = 3
        End Enum

        Private _ref As String
        Private _rights As List(Of EditRights)

        Private _role_ref As String

        Public ReadOnly Property ref() As String
            Get
                Return _ref
            End Get
        End Property

        Public ReadOnly Property rights() As List(Of EditRights)
            Get
                Return _rights
            End Get
        End Property

        Public ReadOnly Property role_ref As String
            Get
                Return _role_ref
            End Get
        End Property

        Public Sub New(ByVal aProcess_ref As String,
                       ByVal aRights As List(Of EditRights),
                       ByVal aRoleRef As String)
            _ref = aProcess_ref
            _rights = aRights
            _role_ref = aRoleRef
        End Sub

    End Class
End Namespace


