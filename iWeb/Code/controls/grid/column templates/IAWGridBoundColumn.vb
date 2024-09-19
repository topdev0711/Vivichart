Imports Microsoft.VisualBasic



Namespace IAW.boundcontrols

    Public NotInheritable Class IAWGridBoundColumn
        Inherits BoundField


#Region "Properties"

        Private ReadOnly Property regexValidate() As Boolean
            Get
                Return Not ValidationExpression = String.Empty
            End Get
        End Property

        Public Property ValidationExpression() As String
            Get
                If String.IsNullOrEmpty(ViewState("ValidationExpression")) Then Return String.Empty
                Return ViewState("ValidationExpression")
            End Get
            Set(ByVal value As String)
                ViewState("ValidationExpression") = value
            End Set
        End Property

        Public Property ValidationMessage() As String
            Get
                If String.IsNullOrEmpty(ViewState("ValidationMessage")) Then Return String.Empty
                Return ViewState("ValidationMessage")
            End Get
            Set(ByVal value As String)
                ViewState("ValidationMessage") = value
            End Set
        End Property

        Public Property MaxInputCharacters() As Integer
            Get
                If String.IsNullOrEmpty(ViewState("MaxInputCharacters")) Then Return 10
                Return ViewState("MaxInputCharacters")
            End Get
            Set(ByVal value As Integer)
                ViewState("MaxInputCharacters") = value
            End Set
        End Property

        Public Property CssClass() As String
            Get
                If String.IsNullOrEmpty(ViewState("CssClass")) Then Return String.Empty
                Return ViewState("CssClass")
            End Get
            Set(ByVal value As String)
                ViewState("CssClass") = value
            End Set
        End Property

        Public Property CssClassOnError() As String
            Get
                If String.IsNullOrEmpty(ViewState("CssClassOnError")) Then Return String.Empty
                Return ViewState("CssClassOnError")
            End Get
            Set(ByVal value As String)
                ViewState("CssClassOnError") = value
            End Set
        End Property

#End Region

        Public Sub New()
            MyBase.new()
            ValidationMessage = "*"
        End Sub

        ' This method adds a date control and any other 
        ' content to the cell's Controls collection.
        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)
            'MyBase.InitializeDataCell(cell, rowState)
            Dim custValid As CustomValidator = Nothing
            Dim control1 As Control = Nothing
            Dim control2 As Control = Nothing
            If ((((rowState And DataControlRowState.Edit) <> DataControlRowState.Normal) AndAlso Not Me.ReadOnly) OrElse ((rowState And DataControlRowState.Insert) <> DataControlRowState.Normal)) Then
                '///editable
                Dim txtBox As New TextBox
                txtBox.ID = Me.DataField
                txtBox.CssClass = Me.CssClass
                txtBox.MaxLength = Me.MaxInputCharacters
                txtBox.Columns = txtBox.MaxLength + 1
                control1 = txtBox
                If ((Me.DataField.Length <> 0) AndAlso ((rowState And DataControlRowState.Edit) <> DataControlRowState.Normal)) Then
                    control2 = txtBox
                End If

                If regexValidate Then
                    '///add regular expression validator if required
                    custValid = New CustomValidator
                    'rv.ValidationExpression = Me.ValidationExpression
                    custValid.Display = ValidatorDisplay.Dynamic
                    custValid.EnableClientScript = False
                    custValid.ControlToValidate = txtBox.ID
                    custValid.ErrorMessage = Me.ValidationMessage
                    AddHandler custValid.ServerValidate, AddressOf validateExpression
                End If

            Else
                '///read only
                If (Me.DataField.Length <> 0) Then
                    control2 = cell
                End If
            End If
            If (Not control1 Is Nothing) Then
                cell.Controls.Add(control1)
            End If
            If (Not custValid Is Nothing) Then
                cell.Controls.Add(custValid)
            End If
            If ((Not control2 Is Nothing) AndAlso MyBase.Visible) Then
                AddHandler control2.DataBinding, New EventHandler(AddressOf Me.OnDataBindField)
            End If



        End Sub

        ' This method is called by the ExtractRowValues methods of
        ' GridView and DetailsView. Retrieve the current value of the 
        ' cell from the Checked state of the control.
        Public Overrides Sub ExtractValuesFromCell( _
            ByVal dictionary As IOrderedDictionary, _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState, _
            ByVal includeReadOnly As Boolean)

            Dim text1 As String = Me.DataField
            Dim obj1 As Object = Nothing

            ' Determine whether the cell contain a date control 
            ' in its Controls collection.

            If cell.Controls.Count > 0 Then
                Dim txtBox As TextBox = CType(cell.Controls(0), TextBox)

                Dim checkedValue As Object = Nothing
                If txtBox Is Nothing Then
                    ' A textbox is expected, but a null is encountered.
                    ' Add error handling.
                    Throw New InvalidOperationException( _
                        "TextBox could not extract control.")
                Else
                    'extract value
                    obj1 = txtBox.Text
                End If

                If (Not obj1 Is Nothing) Then
                    If dictionary.Contains(text1) Then
                        dictionary.Item(text1) = obj1
                    Else
                        dictionary.Add(text1, obj1)
                    End If
                End If

            End If
        End Sub

 
        ' Gets a default value for a basic design-time experience. Since
        ' it would look odd, even at design time, to have more than one
        ' radio button selected, make sure that none are selected.
        Protected Overrides Function GetDesignTimeValue() As Object
            Return False
        End Function

        Protected Shadows Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim control1 As Control = CType(sender, Control)
            Dim control2 As Control = control1.NamingContainer
            Dim obj1 As Object = Me.GetValue(control2)
            'Dim flag1 As Boolean = ((Me.SupportsHtmlEncode AndAlso Me.HtmlEncode) AndAlso TypeOf control1 Is TableCell)
            Dim flag1 As Boolean = False
            Dim text1 As String = Me.FormatDataValue(obj1, flag1)
            If TypeOf control1 Is TableCell Then
                'readonly
                If (text1.Length = 0) Then
                    text1 = "&#160;"
                End If
                CType(control1, TableCell).Text = text1
            Else
                'editable
                If Not TypeOf control1 Is TextBox Then
                    Throw New HttpException("Expected a TextBox control")
                End If

                If (Not obj1 Is Nothing) Then
                    'CType(control1, TextBox).Text = obj1.ToString
                    CType(control1, TextBox).Text = text1
                End If

                'If ((Not obj1 Is Nothing) AndAlso obj1.GetType.IsPrimitive) Then
                '    CType(control1, TextBox).Columns = 5
                'End If

            End If
        End Sub


        Private Sub validateExpression(ByVal s As Object, ByVal e As ServerValidateEventArgs)
            Dim ls_regex As New Regex(Me.ValidationExpression)
            If Not ls_regex.IsMatch(e.Value) Then
                e.IsValid = False
                Dim cv As CustomValidator = CType(s, CustomValidator)
                Dim c As DataControlFieldCell = cv.Parent
                CType(c.FindControl(cv.ControlToValidate), TextBox).CssClass = CssClassOnError
            End If
        End Sub

    End Class

End Namespace
