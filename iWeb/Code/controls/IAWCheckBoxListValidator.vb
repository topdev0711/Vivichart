Imports System.ComponentModel

Namespace IAW.controls

    Public Class IAWCheckBoxListValidator
        Inherits BaseValidator

        Private _ctrlToValidate As CheckBoxList

#Region "Properties"
        <Description("The minimum number of CheckBoxes that must be checked to be considered valid.")> _
        Public Property MinimumSelected() As Integer
            Get
                Dim o As Object = Me.ViewState.Item("MinimumSelected")
                If (o Is Nothing) Then
                    Return 1
                End If
                Return CInt(o)
            End Get
            Set(ByVal value As Integer)
                Me.ViewState.Item("MinimumSelected") = value
            End Set
        End Property
        Protected ReadOnly Property CheckBoxListToValidate() As CheckBoxList
            Get
                If (Me._ctrlToValidate Is Nothing) Then
                    Me._ctrlToValidate = TryCast(FindControl(MyBase.ControlToValidate), CheckBoxList)
                End If
                Return Me._ctrlToValidate
            End Get
        End Property
#End Region

        Protected Overrides Function ControlPropertiesValid() As Boolean
            If (MyBase.ControlToValidate.Length = 0) Then
                Throw New HttpException(String.Format("The ControlToValidate property of '{0}' cannot be blank.", Me.ID))
            End If
            If (Me.CheckBoxListToValidate Is Nothing) Then
                Throw New HttpException(String.Format("The CheckBoxListValidator can only validate controls of type CheckBoxList.", New Object(0 - 1) {}))
            End If
            If (Me.CheckBoxListToValidate.Items.Count < Me.MinimumSelected) Then
                Throw New HttpException(String.Format("MinimumSelected must be set to a value greater than or equal to the number of ListItems; MinimumSelected is set to {0}, but there are only {1} ListItems in '{2}'" _
                                                      , Me.MinimumSelected _
                                                      , Me.CheckBoxListToValidate.Items.Count _
                                                      , Me.CheckBoxListToValidate.ID))
            End If
            Return True
        End Function

        Protected Overrides Function EvaluateIsValid() As Boolean
            Dim selectedItemCount As Integer = 0
            Dim cb As ListItem
            For Each cb In Me.CheckBoxListToValidate.Items
                If cb.Selected Then
                    selectedItemCount += 1
                End If
            Next
            Return (selectedItemCount >= Me.MinimumSelected)
        End Function

        Protected Overrides Sub AddAttributesToRender(ByVal writer As HtmlTextWriter)
            MyBase.AddAttributesToRender(writer)
            If MyBase.RenderUplevel Then
                'If Helpers.EnableLegacyRendering Then
                '    writer.AddAttribute("evaluationfunction", "CheckBoxListValidatorEvaluateIsValid", False)
                '    writer.AddAttribute("minimumNumberOfSelectedCheckBoxes", Me.MinimumSelected.ToString, False)
                'Else
                Me.Page.ClientScript.RegisterExpandoAttribute(Me.ClientID, "evaluationfunction", "CheckBoxListValidatorEvaluateIsValid", False)
                Me.Page.ClientScript.RegisterExpandoAttribute(Me.ClientID, "minimumNumberOfSelectedCheckBoxes", Me.MinimumSelected.ToString, False)
                'End If
            End If
        End Sub

        Protected Overrides Sub OnPreRender(ByVal e As EventArgs)
            MyBase.OnPreRender(e)
            If Not ((Not MyBase.RenderUplevel _
                     OrElse (Me.Page Is Nothing)) _
                     OrElse Me.Page.ClientScript.IsClientScriptIncludeRegistered(MyBase.GetType, "CheckBoxListValidator.js")) Then
                Me.Page.ClientScript.RegisterClientScriptInclude(MyBase.GetType, "CheckBoxListValidator.js", _
                                                                 Me.Page.ClientScript.GetWebResourceUrl(MyBase.GetType, "CheckBoxListValidator.js"))
            End If
        End Sub


    End Class




End Namespace

