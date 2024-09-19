Imports IAW.controls

Partial Public Class ChangePassword
    Inherits System.Web.UI.UserControl

#Region "properties"
    ''' <summary>
    ''' Force users to enter current password
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property EnterCurrentPwd() As Boolean
        Get
            Dim o As Object = Me.ViewState("EnterCurrentPwd")
            If o Is Nothing Then
                'check url parameter
                If Not String.IsNullOrEmpty(ctx.item("EnterCurrentPwd")) Then
                    o = CBool(ctx.item("EnterCurrentPwd"))
                    Me.ViewState("EnterCurrentPwd") = o
                Else
                    o = True
                End If
            End If
            Return CBool(o)
        End Get
        Set(ByVal value As Boolean)
            Me.ViewState("EnterCurrentPwd") = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the user ref for the emp context person ref
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property UserRef() As String
        Get
            Return ctx.session("user_ref")
        End Get
    End Property
    Public ReadOnly Property UserName() As String
        Get

            Dim o As Object = Me.ViewState("UserName")
            If o Is Nothing Then
                o = IawDB.execScalar("SELECT user_name FROM dbo.suser WHERE user_ref=@p1", Me.UserRef)
                Me.ViewState("UserName") = o
            End If
            Return CStr(o)
        End Get
    End Property
    Public Property ValidationGroup() As String
        Get
            Return Me.ViewState("ValidationGroup")
        End Get
        Set(ByVal value As String)
            Me.ViewState("ValidationGroup") = value
        End Set
    End Property

#End Region

#Region "page events"
    Private Sub Page_Init1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init

        Me.TR_orig.Visible = Me.EnterCurrentPwd
    End Sub

    Public Sub AfterLoadProcessing()
        Me.TR_orig.Visible = Me.EnterCurrentPwd
        If EnterCurrentPwd Then
            ScriptManager.GetCurrent(Me.Page).SetFocus(txtPwdOrig)
        Else
            ScriptManager.GetCurrent(Me.Page).SetFocus(txtPwdNew)
        End If
    End Sub

    Private Sub Page_Load1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Me.RPT_format.DataSource = SawPassword.PasswordFormat()
            Me.RPT_format.DataBind()
        End If
        AddJavascript()
    End Sub

#End Region

#Region "click handlers"
    Private Sub btnChangePwd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Update()
    End Sub
#End Region

    Public Function Update() As Boolean
        Dim changePwd As SawPasswordWorked

        Me.msg.Visible = False
        Me.msg_changed.VisibleOnClient = False

        If Me.EnterCurrentPwd Then
            'with current pwd
            changePwd = SawPassword.ChangePassword(Me.UserRef, Me.txtPwdOrig.Text, Me.txtPwdNew.Text, Me.txtPwdNewConfirm.Text)
        Else
            'no current pwd
            changePwd = SawPassword.ChangePassword(Me.UserRef, Me.txtPwdNew.Text, Me.txtPwdNewConfirm.Text)
        End If

        cbPasswords.Checked = False

        If Not changePwd.Worked Then
            ShowMessage(changePwd.Message)
            Return False
        End If

        Me.msg_changed.MessageText = ctx.Translate("::LT_S0308")  ' "LT_S0308 - the password has been updated

        Me.msg_changed.VisibleOnClient = True

        Return True
    End Function

    Private Sub ShowMessage(ByVal txt As String)
        Me.msg.MessageText = txt
        Me.msg.Visible = True
    End Sub

    Private Sub AddJavascript()
        Dim script As String

        If Not Page.ClientScript.IsClientScriptBlockRegistered("showhidejs") Then
            script = "function ToggleDisplay(){ " & _
                     "var cb = document.getElementById(""" + cbPasswords.ClientID + """);" & _
                     "  cb.enabled = false;" & _
                     "  var pwd1 = document.getElementById(""" + txtPwdOrig.ClientID + """);" & _
                     "  var pwd2 = document.getElementById(""" + txtPwdNew.ClientID + """);" & _
                     "  var pwd3 = document.getElementById(""" + txtPwdNewConfirm.ClientID + """);" & _
                     "  if (pwd3.type == ""password"") {" & _
                     "    if (pwd1) pwd1.type = ""text"";" & _
                     "    pwd2.type = ""text"";" & _
                     "    pwd3.type = ""text"";" & _
                     "  } else {" & _
                     "  if (pwd1) pwd1.type = ""password"";" & _
                     "    pwd2.type = ""password"";" & _
                     "    pwd3.type = ""password"";" & _
                     "  }" & _
                     "  cb.enabled = true;" & _
                     "}"
            Page.ClientScript.RegisterClientScriptBlock(Me.GetType, "showhidejs", script, True)
        End If
    End Sub

End Class