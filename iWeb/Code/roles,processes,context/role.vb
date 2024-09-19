
Namespace IAW

    <Serializable()> _
    Public Class role
        Private _name As String
        Private _ref As String
        Private _startPage As String
        Private _processes As Dictionary(Of String, IAWProcess)

#Region "properties"

        Public ReadOnly Property name() As String
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property ref() As String
            Get
                Return _ref
            End Get
        End Property

        Public ReadOnly Property startPage() As String
            Get
                Return _startPage
            End Get
        End Property

        Public ReadOnly Property processes() As Dictionary(Of String, IAWProcess)
            Get
                Return _processes
            End Get
        End Property

#End Region


        Public Sub New(ByVal aRoleRef As String, ByVal aRoleName As String, ByVal aStartPage As String, ByVal processRightsForRole As Dictionary(Of String, IAWProcess))
            _ref = aRoleRef
            _name = aRoleName
            _processes = processRightsForRole
            _startPage = aStartPage
        End Sub


        Public Function hasRightInProcess(ByVal aProcessRef As String, ByVal _rightTo As RightTo) As Boolean
            If Not _processes.ContainsKey(aProcessRef) Then Return False
            Dim process As IAWProcess = _processes(aProcessRef)
            Dim right As EditRights
            Dim ErrorStr As String

            For Each right In process.rights

                If right.rightName = _rightTo Then
                    Select Case right.type
                        Case RightType.trueFalse
                            If right.isAllowed Then
                                Return True
                            Else : Return False
                            End If
                        Case RightType.sql
                            Dim ls_sql As String = SawUtil.Substitute(right.value)
                            Return SawDB.ExecScalar(ls_sql).ToString = "1"
                        Case RightType.derived
                            'SawCompute.Derive(HttpContext.Current, DF, lo_reader, False, as_data, "")
                            'Return SawCompute.Derive(HttpContext.Current, Nothing, Nothing, False, right.value, "") = 1

                            ErrorStr = String.Format("Misconfigured role_process ({0}.{1}) ", ref, aProcessRef)
                            Select Case _rightTo
                                Case 0
                                    ErrorStr += "View"
                                Case 1
                                    ErrorStr += "Update"
                                Case 2
                                    ErrorStr += "Insert"
                                Case 3
                                    ErrorStr += "Delete"
                            End Select

                            LogException.HandleException(New Exception(ErrorStr), True)

                            'Throw New Exception("Misconfigured Role Process.  Please contact support")
                    End Select
                End If
            Next

            Return False
        End Function

        Public Function hasRightToViewProcess(ByVal aProcessRef As String) As Boolean
            Return hasRightInProcess(aProcessRef, RightTo.view)
        End Function

        Public Function hasRightToViewProcessOnly(ByVal aProcessRef As String) As Boolean
            If hasRightInProcess(aProcessRef, RightTo.delete) Then Return False
            If hasRightInProcess(aProcessRef, RightTo.insert) Then Return False
            If hasRightInProcess(aProcessRef, RightTo.update) Then Return False
            Return True
        End Function
    End Class

End Namespace


