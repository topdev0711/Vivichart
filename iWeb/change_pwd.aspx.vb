Imports IAW.controls
Imports System.Reflection

<ProcessRequired(False), ButtonBarRequired(False)> _
Partial Class change_pwd
    Inherits stub_IngenWebPage

#Region "properties"
    Private ReadOnly Property MustChangePwd() As Boolean
        Get
            Return Not String.IsNullOrEmpty(ctx.item("mustchangepwd"))
        End Get
    End Property

    Private ReadOnly Property MustChangeQu() As Boolean
        Get
            Return Not String.IsNullOrEmpty(ctx.item("mustchangequ"))
        End Get
    End Property
#End Region

    Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        Me.msg.MessageText = SawLang.Translate("::LT_S0166") ' LT_S0166 - Please update your password
        Me.msg.Visible = True

    End Sub

    Private Sub btnUdatePwd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUdatePwd.Click
        If Me.ChangePwd.Update() AndAlso Me.MustChangePwd Then

            'if the user was forced to change the password then onsave redirect to their start page
            StartPage.Redirect()
        End If
    End Sub

End Class
