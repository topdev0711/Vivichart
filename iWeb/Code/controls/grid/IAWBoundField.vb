Imports System.ComponentModel

Namespace IAW.boundcontrols

    Public Class IAWBoundField
        Inherits BoundField

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original header text.")>
        Public WriteOnly Property orgHeaderText As String
            Set(value As String)
            End Set
        End Property

    End Class

End Namespace
