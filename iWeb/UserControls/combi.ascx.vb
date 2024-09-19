Imports System.Reflection
Imports IAW.controls

Partial Class combi
    Inherits System.Web.UI.UserControl


#Region "field, properties, events"
    Private Property textFieldId() As String
        Get
            If ViewState("_CtextFieldId") Is Nothing Then Return String.empty
            Return ViewState("_CtextFieldId")
        End Get
        Set(ByVal Value As String)
            ViewState("_CtextFieldId") = Value
        End Set
    End Property
    Private Property typeFieldId() As String
        Get
            If ViewState("_CtypeFieldId") Is Nothing Then Return String.empty
            Return ViewState("_CtypeFieldId")
        End Get
        Set(ByVal Value As String)
            ViewState("_CtypeFieldId") = Value
        End Set
    End Property
    Private Property autoPopulateFormFields() As Boolean
        Get
            If ViewState("_CautoPopulateFormFields") Is Nothing Then Return False
            Return ViewState("_CautoPopulateFormFields")
        End Get
        Set(ByVal Value As Boolean)
            ViewState("_CautoPopulateFormFields") = Value
        End Set
    End Property
    Public Property text() As String
        Get
            Return TextBox1.Text.Trim
        End Get
        Set(ByVal Value As String)
            TextBox1.Text = Value.Trim
        End Set
    End Property

    'declare events and delgate
    Public Event cancelClicked As EventHandler
    Public Event okClicked As EventHandler

#End Region

    Public Sub New()
        Me.Visible = False
        Me.EnableViewState = True
        textFieldId = String.empty
        typeFieldId = String.empty
        autoPopulateFormFields = False
    End Sub

    'pass in db combi value, returns the type ie SELECT, TRUE, FALSE, DERIVED
    Public Shared Function getCombiType(ByVal value As String) As String
        If value.ToUpper.StartsWith("SELECT") Then
            Return "SELECT"
        ElseIf value.StartsWith("^") Then
            Return "DERIVED"
        ElseIf value.Equals("0") Then
            Return "FALSE"
        ElseIf value.Equals("1") Then
            Return "TRUE"
        End If
        Return String.empty
    End Function

    'sets form fields ids on the parent container and autopopulate to true
    Public Sub setFormFields(ByVal displayControlId As String, ByVal valueControlId As String)
        textFieldId = valueControlId
        typeFieldId = displayControlId
        autoPopulateFormFields = True
    End Sub

    'populates form fields on the parent container, called by btnSave_Click only if autopopulate is true
    Private Sub populateFormFields(ByVal type As String)
        Dim cntrl As Control
        Dim cntrltype As Type
        Dim textProp As PropertyInfo

        cntrl = Me.Parent.FindControl(textFieldId)
        If Not cntrl Is Nothing Then
            cntrltype = cntrl.GetType()
            textProp = cntrltype.GetProperty("Text")
            If Not textProp Is Nothing Then
                textProp.SetValue(cntrl, text, Nothing)
                textProp = Nothing
            End If
            cntrl = Nothing
        End If

        cntrl = Me.Parent.FindControl(typeFieldId)
        If Not cntrl Is Nothing Then
            cntrltype = cntrl.GetType()
            textProp = cntrltype.GetProperty("Text")
            If Not textProp Is Nothing Then
                textProp.SetValue(cntrl, type, Nothing)
                textProp = Nothing
            End If
            cntrl = Nothing
        End If

    End Sub

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
    End Sub

#Region "event handlers"

    'raise cancelClicked event
    Protected Friend Sub Raise_cancelClicked(ByVal e As EventArgs)
        Me.Visible = False
        RaiseEvent cancelClicked(Me, e)
    End Sub

    'raise okClicked event
    Protected Friend Sub Raise_saveClicked(ByVal e As EventArgs)
        Me.Visible = False
        RaiseEvent okClicked(Me, e)
    End Sub

    'handles wuc cancelClicked
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Raise_cancelClicked(New combiEventArgs(text, text))
        Raise_cancelClicked(e)
    End Sub

    'handles wuc okClicked
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim type As String = combi.getCombiType(text)
        If autoPopulateFormFields Then populateFormFields(type)
        'Raise_saveClicked(New combiEventArgs(text, type))
        Raise_saveClicked(e)
    End Sub

    'handles wuc formatSql
    Private Sub btnFormatSql_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Visible = True
        Dim sql As New SqlBeautify
        text = sql.Beautify(text)
    End Sub
#End Region

    'create wuc buttons
    Protected Overrides Sub CreateChildControls()

        Dim btnSave As IAWHyperLinkButton = New IAWHyperLinkButton(AddressOf btnSave_Click)
        btnSave.ToolTip = ctx.Translate("::LT_S0147") ' LT_S0147 - OK
        btnSave.Text = ctx.Translate("::LT_S0147") ' LT_S0147 - OK

        Dim btnFormatSql As IAWHyperLinkButton = New IAWHyperLinkButton(AddressOf btnFormatSql_Click)
        btnFormatSql.ToolTip = ctx.Translate("::LT_S0316") ' LT_S316- Format Sql
        btnFormatSql.Text = ctx.Translate("::LT_S0316") ' LT_S0316 - Format Sql

        Dim btnCancel As IAWHyperLinkButton = New IAWHyperLinkButton(AddressOf btnCancel_Click)
        btnCancel.ToolTip = ctx.Translate("::LT_S0138") ' LT_S0138 - Cancel
        btnCancel.Text = ctx.Translate("::LT_S0138") ' LT_S0138 - Cancel

        phBtn.Controls.Add(btnCancel)
        phBtn.Controls.Add(btnFormatSql)
        phBtn.Controls.Add(btnSave)
    End Sub

End Class

Public Class combiEventArgs
    Inherits System.EventArgs
    Public text As String
    Public type As String

    Public Sub New(ByVal _text As String, ByVal _type As String)
        text = _text
        type = _type
    End Sub
End Class
