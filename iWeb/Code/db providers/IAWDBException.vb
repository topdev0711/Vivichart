Imports System.Data.Common

<Serializable()>
Public Class IAWDBException
    Inherits Exception

    Private _Sql As String
    Private _Params As DbParameterCollection
    Private _IsNonQueryError As Boolean
    Private _ErrorCode As Integer
    Private _MessageRef As String
    Private _UpperStack As String

#Region "properties"

    Public ReadOnly Property Sql() As String
        Get
            Return Me._Sql
        End Get
    End Property
    Public ReadOnly Property Param() As DbParameterCollection
        Get
            Return Me._Params
        End Get
    End Property
    Public ReadOnly Property IsNonQueryError() As Boolean
        Get
            Return Me._IsNonQueryError
        End Get
    End Property
    Public ReadOnly Property ErrorCode() As Integer
        Get
            Return Me._ErrorCode
        End Get
    End Property
    Public ReadOnly Property MessageRef() As String
        Get
            Return Me._MessageRef
        End Get
    End Property
    Public Property UpperStack() As String
        Get
            Return _UpperStack
        End Get
        Set(ByVal value As String)
            _UpperStack = value
        End Set
    End Property

#End Region

#Region "constructors"

    Public Sub New()
        init()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
        init()
    End Sub
    Public Sub New(ByVal message As String, ByVal inner As Exception)
        MyBase.New(message, inner)
        init()
    End Sub
    Public Sub New(ByVal inner As Exception,
                   ByVal sql As String,
                   ByVal params As DbParameterCollection)
        MyBase.New(inner.Message, inner)
        init()
        Me._Sql = sql
        Me._Params = params
        ctx.session("ERROR-SQL") = sql
        ctx.session("ERROR-Params") = params
    End Sub
    Public Sub New(ByVal message As String, _
                   ByVal inner As Exception, _
                   ByVal sql As String, _
                   ByVal params As DbParameterCollection)
        MyBase.New(message, inner)
        init()
        Me._Sql = sql
        Me._Params = params

        ctx.session("ERROR-SQL") = sql
        ctx.session("ERROR-Params") = params
    End Sub
    Public Sub New(ByVal inner As Exception, _
                   ByVal sql As String, _
                   ByVal params As DbParameterCollection, _
                   ByVal errorCode As String, _
                   ByVal messageRef As String)
        MyBase.New(inner.Message, inner)
        init()
        Me._Sql = sql
        Me._Params = params
        Me._IsNonQueryError = True
        Me._ErrorCode = errorCode
        Me._MessageRef = messageRef
        ctx.session("ERROR-SQL") = sql
        ctx.session("ERROR-Params") = params
    End Sub

#End Region

    Private Sub init()
        Me._Sql = String.Empty
        Me._Params = Nothing
        Me._MessageRef = String.Empty
        Me._UpperStack = String.Empty
    End Sub

End Class

