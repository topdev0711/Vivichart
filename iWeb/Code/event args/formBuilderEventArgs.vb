Public Class formBuilderEventArgs
    Inherits System.EventArgs

    Public formItemRef As String
    Public objectType As WebObjectType

    Public Sub New(ByVal itemRef As String, ByVal type As WebObjectType)
        formItemRef = itemRef
        objectType = type
    End Sub
End Class



