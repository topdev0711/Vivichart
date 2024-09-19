Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Data

Public Class Puptext

    Public Enum PupType
        All = 0
        System = 1
        User = 2
    End Enum

    ''' <summary>
    ''' Returns the translated pup text for a specific return value
    ''' </summary>
    ''' <param name="tableId"></param>
    ''' <param name="columnId"></param>
    ''' <param name="return_value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetPupText(ByVal tableId As String, _
                                      ByVal columnId As String, _
                                      ByVal return_value As String) As PupTextEntry
        Dim ls_sql As String

        If String.IsNullOrEmpty(tableId) _
          Or String.IsNullOrEmpty(columnId) _
          Or String.IsNullOrEmpty(return_value) Then Return Nothing

        ls_sql = "SELECT isnull(pup_text,'') " _
               + "  FROM dbo.puptext WITH (NOLOCK) " _
               + " WHERE table_id = ?? " _
               + "   AND column_id = ?? " _
               + "   AND language_ref = ?? " _
               + "   AND return_value = ?? "

        Return New PupTextEntry(SawDB.ExecScalar(ls_sql, New Object() {tableId, columnId, ctx.languageCode, return_value}), return_value)
    End Function

    Public Shared Function GetPupText(ByVal tableId As String, _
                                      ByVal columnId As String) As DataTable
        Return GetPupText(tableId, columnId, False)
    End Function

    Public Shared Function GetPupText(ByVal tableId As String, _
                                      ByVal columnId As String, _
                                      ByVal includeEmptyValue As Boolean) As DataTable
        Return GetPupText(tableId, columnId, includeEmptyValue, "", PupType.All)
    End Function

    Public Shared Function GetPupText(ByVal tableId As String, _
                                      ByVal columnId As String, _
                                      ByVal includeEmptyValue As Boolean, _
                                      ByVal type As PupType) As DataTable
        Return GetPupText(tableId, columnId, includeEmptyValue, "", type)
    End Function

    Public Shared Function GetPupText(ByVal tableId As String, _
                                      ByVal columnId As String, _
                                      ByVal includeEmptyValue As Boolean, _
                                      ByVal emptyValueText As String, _
                                      ByVal type As PupType) As DataTable
        Dim ls_sql As String
        Dim DT As DataTable
        ls_sql = "SELECT pup_text AS text, " _
               + "       return_value AS value " _
               + "  FROM dbo.puptext WITH (NOLOCK) " _
               + " WHERE table_id = ?? " _
               + "   AND column_id = ?? " _
               + "   AND language_ref = ?? "

        Select Case type
            Case PupType.System
                ls_sql += " AND pup_type = '01' "

            Case PupType.User
                ls_sql += " AND pup_type = '02' "

        End Select

        ls_sql += " ORDER BY pup_text ASC "
        DT = SawDB.ExecGetTable(ls_sql, New Object() {tableId, columnId, ctx.languageCode})

        If includeEmptyValue And DT.Rows.Count > 0 Then
            Dim dr As DataRow = DT.NewRow
            If String.IsNullOrEmpty(emptyValueText) Then
                dr(0) = " - "
            Else
                dr(0) = emptyValueText
            End If
            dr(1) = ""
            DT.Rows.InsertAt(dr, 0)
        End If

        Return DT
    End Function

End Class

<Serializable> _
Public Class PupTextEntry
    Private _text As String
    Private _value As String

    Public ReadOnly Property Text() As String
        Get
            Return Me._text
        End Get
    End Property

    Public ReadOnly Property Value() As String
        Get
            Return Me._value
        End Get
    End Property

    Public Sub New(ByVal text As String, ByVal value As String)
        Me._text = text
        Me._value=value
    End Sub

End Class



