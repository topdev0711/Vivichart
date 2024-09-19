
Imports System.Reflection

Public Class Version

#Region "properties"

    Private Shared ReadOnly Property AssemblyInfo() As Assembly
        Get
            Return Assembly.GetExecutingAssembly
        End Get
    End Property

    Public Shared ReadOnly Property AssemblyVersion() As String
        Get
            Return AssemblyInfo.GetName.Version.ToString
        End Get
    End Property

    Public Shared ReadOnly Property Major() As Integer
        Get
            Return AssemblyInfo.GetName.Version.Major
        End Get
    End Property
    Public Shared ReadOnly Property Minor() As Integer
        Get
            Return AssemblyInfo.GetName.Version.Minor
        End Get
    End Property
    Public Shared ReadOnly Property Build() As Integer
        Get
            Return AssemblyInfo.GetName.Version.Build
        End Get
    End Property
    Public Shared ReadOnly Property Revision() As Integer
        Get
            Return AssemblyInfo.GetName.Version.Revision
        End Get
    End Property

    Public Shared ReadOnly Property DBVersion() As String
        Get
            Return IawDB.execScalar("SELECT db_revision FROM GLOBAL_PARM WITH (NOLOCK)")
        End Get
    End Property
    Public Shared ReadOnly Property TestSystem() As Boolean
        Get
            Return False
        End Get
    End Property

    Public Shared ReadOnly Property AppVer() As String
        Get
            Dim AppPart As String
            Try
                Dim APPv() As String = AssemblyVersion.Split(".")
                AppPart = APPv(0) + "." + APPv(1) + "." + APPv(2)
            Catch ex As Exception
                Return "0"
            End Try
            Return AppPart
        End Get
    End Property

    Public Shared ReadOnly Property DisplayAppVer() As String
        Get
            Dim AppPart As String
            Try
                Dim APPv() As String = AssemblyVersion.Split(".")
                AppPart = "v" + APPv(0) + "." + APPv(1) + "." + APPv(2) + "." + APPv(3)
            Catch ex As Exception
                Return "0"
            End Try
            Return AppPart
        End Get
    End Property

    Public Shared ReadOnly Property SupportedVersions() As String
        Get
            Dim Result As String = ""

            For Each s As String In Versions
                Result = If(Result = "", "", Result + ", ") + s
            Next

            Return Result
        End Get
    End Property

#End Region

    Public Shared ReadOnly Property Versions() As List(Of String)
        Get
            Dim Vers As New List(Of String)
            'Vers.Add("8.00.00")
            'Vers.Add("8.00.01")
            'Vers.Add("8.00.02")
            'Vers.Add("8.00.03")
            Vers.Add("8.01.00")
            Return Vers
        End Get
    End Property

    Public Shared Function IsValidVersion() As Boolean
        Return Versions.Contains(DBVersion)
    End Function

End Class
