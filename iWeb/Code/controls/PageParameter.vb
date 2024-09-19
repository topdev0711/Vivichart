
Imports System.Reflection

Namespace IAW.controls

    Public Class PageParameter
        Inherits Parameter

        Public Property PropertyName() As String
            Get
                Dim obj2 As Object = MyBase.ViewState.Item("PropertyName")
                If (obj2 Is Nothing) Then
                    Return String.Empty
                End If
                Return CStr(obj2)
            End Get
            Set(ByVal value As String)
                If (Me.PropertyName <> value) Then
                    MyBase.ViewState.Item("PropertyName") = value
                    MyBase.OnParameterChanged()
                End If
            End Set
        End Property

        Protected Overrides Function Evaluate(ByVal context As System.Web.HttpContext,
                                              ByVal control As System.Web.UI.Control) As Object
            Dim value As Object
            'use reflection to grab the value of the page property
            Dim T As Type = control.Page.GetType()
            Dim P As PropertyInfo
            P = T.GetProperty(Me.PropertyName, (BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public))
            If P Is Nothing Then
                'couldn't find page property, throw toys out of pram
                Throw New HttpException("Unable to find a page property named " + Me.PropertyName)
            End If
            value = P.GetValue(control.Page, Nothing)
            Return value
        End Function

    End Class

End Namespace



