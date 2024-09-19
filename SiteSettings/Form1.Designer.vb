<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnTestConn = New System.Windows.Forms.Button()
        Me.lblAuth2 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.pnlVD = New System.Windows.Forms.Panel()
        Me.btnCancelVD = New System.Windows.Forms.Button()
        Me.lblVD = New System.Windows.Forms.Label()
        Me.cbxVdirs = New System.Windows.Forms.ComboBox()
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.cbxThemes = New System.Windows.Forms.ComboBox()
        Me.lblConnType = New System.Windows.Forms.Label()
        Me.cbxAuth = New System.Windows.Forms.ComboBox()
        Me.txtDbName = New System.Windows.Forms.TextBox()
        Me.txtDbServer = New System.Windows.Forms.TextBox()
        Me.lblDbName = New System.Windows.Forms.Label()
        Me.lblDbServer = New System.Windows.Forms.Label()
        Me.lblTheme = New System.Windows.Forms.Label()
        Me.lblName = New System.Windows.Forms.Label()
        Me.lblAuth = New System.Windows.Forms.Label()
        Me.tvSites = New System.Windows.Forms.TreeView()
        Me.ttPop = New System.Windows.Forms.ToolTip(Me.components)
        Me.PictureBox4 = New System.Windows.Forms.PictureBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.PictureBox5 = New System.Windows.Forms.PictureBox()
        Me.PictureBox6 = New System.Windows.Forms.PictureBox()
        Me.PictureBox7 = New System.Windows.Forms.PictureBox()
        Me.PictureBox8 = New System.Windows.Forms.PictureBox()
        Me.PictureBox9 = New System.Windows.Forms.PictureBox()
        Me.PictureBox10 = New System.Windows.Forms.PictureBox()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.tpGeneral = New System.Windows.Forms.TabPage()
        Me.gpxGeneral = New System.Windows.Forms.GroupBox()
        Me.cbxUseLanguage = New System.Windows.Forms.CheckBox()
        Me.txtErrorMessage = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.numInnactiveTimeout = New System.Windows.Forms.NumericUpDown()
        Me.numServerTimeout = New System.Windows.Forms.NumericUpDown()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtBrowserTitle = New System.Windows.Forms.TextBox()
        Me.lblBrowserTitle = New System.Windows.Forms.Label()
        Me.tpDatabase = New System.Windows.Forms.TabPage()
        Me.gpxDB = New System.Windows.Forms.GroupBox()
        Me.txtProfileUser = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.pnlServer = New System.Windows.Forms.Panel()
        Me.btnCancelServer = New System.Windows.Forms.Button()
        Me.lblServer = New System.Windows.Forms.Label()
        Me.cbxServers = New System.Windows.Forms.ComboBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.numCommandTimeout = New System.Windows.Forms.NumericUpDown()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.numConnectionTimeout = New System.Windows.Forms.NumericUpDown()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.numMaxPool = New System.Windows.Forms.NumericUpDown()
        Me.numMinPool = New System.Windows.Forms.NumericUpDown()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.tpAuthenication = New System.Windows.Forms.TabPage()
        Me.gpxAuthentication = New System.Windows.Forms.GroupBox()
        Me.pnlSSO = New System.Windows.Forms.Panel()
        Me.txtProviderLogoutURL = New System.Windows.Forms.TextBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.cbAssertionSigned = New System.Windows.Forms.CheckBox()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.cbResponseSigned = New System.Windows.Forms.CheckBox()
        Me.btnCert = New System.Windows.Forms.Button()
        Me.txtIngenuityURL = New System.Windows.Forms.TextBox()
        Me.txtIngenuityID = New System.Windows.Forms.TextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.txtProviderCertFile = New System.Windows.Forms.TextBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.txtProviderURL = New System.Windows.Forms.TextBox()
        Me.txtProviderID = New System.Windows.Forms.TextBox()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.pnlActiveDirectory = New System.Windows.Forms.Panel()
        Me.btnTestADConn = New System.Windows.Forms.Button()
        Me.lblDomains = New System.Windows.Forms.Label()
        Me.txtDomains = New System.Windows.Forms.TextBox()
        Me.txtActiveDir = New System.Windows.Forms.TextBox()
        Me.lblActiveDir = New System.Windows.Forms.Label()
        Me.tpEmail = New System.Windows.Forms.TabPage()
        Me.gbTestEmail = New System.Windows.Forms.GroupBox()
        Me.tbSendTo = New System.Windows.Forms.TextBox()
        Me.txtSendTo = New System.Windows.Forms.Label()
        Me.btnTestEmail = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMailTo = New System.Windows.Forms.TextBox()
        Me.lblEmailErrors = New System.Windows.Forms.Label()
        Me.lblMailTo = New System.Windows.Forms.Label()
        Me.ddlEmailErrors = New System.Windows.Forms.ComboBox()
        Me.tpProxy = New System.Windows.Forms.TabPage()
        Me.numProxyPort = New System.Windows.Forms.NumericUpDown()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.txtProxyDomain = New System.Windows.Forms.TextBox()
        Me.txtProxyPassword = New System.Windows.Forms.TextBox()
        Me.txtProxyUser = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtProxyURI = New System.Windows.Forms.TextBox()
        Me.cbxProxy = New System.Windows.Forms.CheckBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.fileCert = New System.Windows.Forms.OpenFileDialog()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.lblMsg = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlVD.SuspendLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox7, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox8, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox9, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox10, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabControl1.SuspendLayout()
        Me.tpGeneral.SuspendLayout()
        Me.gpxGeneral.SuspendLayout()
        CType(Me.numInnactiveTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numServerTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tpDatabase.SuspendLayout()
        Me.gpxDB.SuspendLayout()
        Me.pnlServer.SuspendLayout()
        CType(Me.numCommandTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numConnectionTimeout, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMaxPool, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMinPool, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tpAuthenication.SuspendLayout()
        Me.gpxAuthentication.SuspendLayout()
        Me.pnlSSO.SuspendLayout()
        Me.pnlActiveDirectory.SuspendLayout()
        Me.tpEmail.SuspendLayout()
        Me.gbTestEmail.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.tpProxy.SuspendLayout()
        CType(Me.numProxyPort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.lblMsg)
        Me.Panel1.Controls.Add(Me.btnSave)
        Me.Panel1.Controls.Add(Me.btnDelete)
        Me.Panel1.Controls.Add(Me.btnAdd)
        Me.Panel1.Location = New System.Drawing.Point(249, 395)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(788, 36)
        Me.Panel1.TabIndex = 1
        '
        'btnSave
        '
        Me.btnSave.Dock = System.Windows.Forms.DockStyle.Right
        Me.btnSave.Location = New System.Drawing.Point(688, 0)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 36)
        Me.btnSave.TabIndex = 14
        Me.btnSave.Text = "Save"
        Me.ttPop.SetToolTip(Me.btnSave, "Save Details")
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        Me.btnDelete.Dock = System.Windows.Forms.DockStyle.Left
        Me.btnDelete.Location = New System.Drawing.Point(100, 0)
        Me.btnDelete.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(100, 36)
        Me.btnDelete.TabIndex = 13
        Me.btnDelete.Text = "Delete"
        Me.ttPop.SetToolTip(Me.btnDelete, "Delete Site")
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnAdd
        '
        Me.btnAdd.Dock = System.Windows.Forms.DockStyle.Left
        Me.btnAdd.Location = New System.Drawing.Point(0, 0)
        Me.btnAdd.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(100, 36)
        Me.btnAdd.TabIndex = 12
        Me.btnAdd.Text = "Add"
        Me.ttPop.SetToolTip(Me.btnAdd, "Add New Site")
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'btnTestConn
        '
        Me.btnTestConn.AutoSize = True
        Me.btnTestConn.Location = New System.Drawing.Point(316, 158)
        Me.btnTestConn.Margin = New System.Windows.Forms.Padding(4)
        Me.btnTestConn.Name = "btnTestConn"
        Me.btnTestConn.Size = New System.Drawing.Size(161, 33)
        Me.btnTestConn.TabIndex = 11
        Me.btnTestConn.Text = "Test Connection"
        Me.btnTestConn.UseVisualStyleBackColor = True
        '
        'lblAuth2
        '
        Me.lblAuth2.AutoSize = True
        Me.lblAuth2.Location = New System.Drawing.Point(12, 20)
        Me.lblAuth2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAuth2.Name = "lblAuth2"
        Me.lblAuth2.Size = New System.Drawing.Size(149, 17)
        Me.lblAuth2.TabIndex = 34
        Me.lblAuth2.Text = "Authentication Method"
        '
        'PictureBox1
        '
        Me.PictureBox1.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(124, 27)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(27, 16)
        Me.PictureBox1.TabIndex = 27
        Me.PictureBox1.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox1, "Select a Virtual Directory")
        '
        'pnlVD
        '
        Me.pnlVD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlVD.Controls.Add(Me.btnCancelVD)
        Me.pnlVD.Controls.Add(Me.lblVD)
        Me.pnlVD.Controls.Add(Me.cbxVdirs)
        Me.pnlVD.Location = New System.Drawing.Point(124, 21)
        Me.pnlVD.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlVD.Name = "pnlVD"
        Me.pnlVD.Size = New System.Drawing.Size(301, 99)
        Me.pnlVD.TabIndex = 26
        Me.pnlVD.Visible = False
        '
        'btnCancelVD
        '
        Me.btnCancelVD.Location = New System.Drawing.Point(169, 59)
        Me.btnCancelVD.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancelVD.Name = "btnCancelVD"
        Me.btnCancelVD.Size = New System.Drawing.Size(100, 28)
        Me.btnCancelVD.TabIndex = 21
        Me.btnCancelVD.Text = "Cancel"
        Me.btnCancelVD.UseVisualStyleBackColor = True
        '
        'lblVD
        '
        Me.lblVD.AutoSize = True
        Me.lblVD.Location = New System.Drawing.Point(36, 6)
        Me.lblVD.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblVD.Name = "lblVD"
        Me.lblVD.Size = New System.Drawing.Size(164, 17)
        Me.lblVD.TabIndex = 20
        Me.lblVD.Text = "Select a Virtual Directory"
        '
        'cbxVdirs
        '
        Me.cbxVdirs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxVdirs.FormattingEnabled = True
        Me.cbxVdirs.Location = New System.Drawing.Point(40, 26)
        Me.cbxVdirs.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxVdirs.Name = "cbxVdirs"
        Me.cbxVdirs.Size = New System.Drawing.Size(228, 24)
        Me.cbxVdirs.TabIndex = 19
        '
        'txtName
        '
        Me.txtName.Location = New System.Drawing.Point(159, 23)
        Me.txtName.Margin = New System.Windows.Forms.Padding(4)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(228, 22)
        Me.txtName.TabIndex = 0
        '
        'cbxThemes
        '
        Me.cbxThemes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxThemes.FormattingEnabled = True
        Me.cbxThemes.Location = New System.Drawing.Point(159, 55)
        Me.cbxThemes.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxThemes.Name = "cbxThemes"
        Me.cbxThemes.Size = New System.Drawing.Size(228, 24)
        Me.cbxThemes.TabIndex = 1
        '
        'lblConnType
        '
        Me.lblConnType.AutoSize = True
        Me.lblConnType.Location = New System.Drawing.Point(8, 97)
        Me.lblConnType.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblConnType.Name = "lblConnType"
        Me.lblConnType.Size = New System.Drawing.Size(67, 17)
        Me.lblConnType.TabIndex = 16
        Me.lblConnType.Text = "Pool Size"
        '
        'cbxAuth
        '
        Me.cbxAuth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxAuth.FormattingEnabled = True
        Me.cbxAuth.Items.AddRange(New Object() {"Forms", "Network Logon", "Active Directory", "Single Sign On"})
        Me.cbxAuth.Location = New System.Drawing.Point(216, 16)
        Me.cbxAuth.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxAuth.Name = "cbxAuth"
        Me.cbxAuth.Size = New System.Drawing.Size(228, 24)
        Me.cbxAuth.TabIndex = 5
        '
        'txtDbName
        '
        Me.txtDbName.Location = New System.Drawing.Point(133, 62)
        Me.txtDbName.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDbName.Name = "txtDbName"
        Me.txtDbName.Size = New System.Drawing.Size(308, 22)
        Me.txtDbName.TabIndex = 3
        '
        'txtDbServer
        '
        Me.txtDbServer.Location = New System.Drawing.Point(135, 30)
        Me.txtDbServer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDbServer.Name = "txtDbServer"
        Me.txtDbServer.Size = New System.Drawing.Size(272, 22)
        Me.txtDbServer.TabIndex = 2
        '
        'lblDbName
        '
        Me.lblDbName.AutoSize = True
        Me.lblDbName.Location = New System.Drawing.Point(8, 65)
        Me.lblDbName.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDbName.Name = "lblDbName"
        Me.lblDbName.Size = New System.Drawing.Size(110, 17)
        Me.lblDbName.TabIndex = 3
        Me.lblDbName.Text = "Database Name"
        '
        'lblDbServer
        '
        Me.lblDbServer.AutoSize = True
        Me.lblDbServer.Location = New System.Drawing.Point(8, 33)
        Me.lblDbServer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDbServer.Name = "lblDbServer"
        Me.lblDbServer.Size = New System.Drawing.Size(115, 17)
        Me.lblDbServer.TabIndex = 2
        Me.lblDbServer.Text = "Database Server"
        '
        'lblTheme
        '
        Me.lblTheme.AutoSize = True
        Me.lblTheme.Location = New System.Drawing.Point(8, 59)
        Me.lblTheme.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTheme.Name = "lblTheme"
        Me.lblTheme.Size = New System.Drawing.Size(52, 17)
        Me.lblTheme.TabIndex = 1
        Me.lblTheme.Text = "Theme"
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(8, 27)
        Me.lblName.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(109, 17)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Virtual Directory"
        '
        'lblAuth
        '
        Me.lblAuth.AutoSize = True
        Me.lblAuth.Location = New System.Drawing.Point(24, 168)
        Me.lblAuth.Name = "lblAuth"
        Me.lblAuth.Size = New System.Drawing.Size(114, 13)
        Me.lblAuth.TabIndex = 5
        Me.lblAuth.Text = "Authentication Method"
        '
        'tvSites
        '
        Me.tvSites.HideSelection = False
        Me.tvSites.Location = New System.Drawing.Point(16, 10)
        Me.tvSites.Margin = New System.Windows.Forms.Padding(4)
        Me.tvSites.Name = "tvSites"
        Me.tvSites.Size = New System.Drawing.Size(220, 419)
        Me.tvSites.TabIndex = 0
        '
        'PictureBox4
        '
        Me.PictureBox4.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PictureBox4.Image = CType(resources.GetObject("PictureBox4.Image"), System.Drawing.Image)
        Me.PictureBox4.Location = New System.Drawing.Point(416, 33)
        Me.PictureBox4.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox4.Name = "PictureBox4"
        Me.PictureBox4.Size = New System.Drawing.Size(27, 16)
        Me.PictureBox4.TabIndex = 28
        Me.PictureBox4.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox4, "Select a Server")
        '
        'PictureBox3
        '
        Me.PictureBox3.Image = CType(resources.GetObject("PictureBox3.Image"), System.Drawing.Image)
        Me.PictureBox3.Location = New System.Drawing.Point(161, 39)
        Me.PictureBox3.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(24, 20)
        Me.PictureBox3.TabIndex = 45
        Me.PictureBox3.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox3, "Only applicable in a multiple domain environment" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Must be a ; seperated list")
        '
        'PictureBox2
        '
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.Location = New System.Drawing.Point(161, 7)
        Me.PictureBox2.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(24, 20)
        Me.PictureBox2.TabIndex = 42
        Me.PictureBox2.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox2, "As a minimum you must enter the name of the Active Directory Server" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "optionally y" &
        "ou may specify a path within the Active Directory" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "eg; server/DC=domain,DC=com o" &
        "r server/OU=sales,DC=domain,DC=com")
        '
        'PictureBox5
        '
        Me.PictureBox5.Image = CType(resources.GetObject("PictureBox5.Image"), System.Drawing.Image)
        Me.PictureBox5.Location = New System.Drawing.Point(705, 64)
        Me.PictureBox5.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox5.Name = "PictureBox5"
        Me.PictureBox5.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox5.TabIndex = 45
        Me.PictureBox5.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox5, "The URL to the Identity Provider's authentication page")
        '
        'PictureBox6
        '
        Me.PictureBox6.Image = CType(resources.GetObject("PictureBox6.Image"), System.Drawing.Image)
        Me.PictureBox6.Location = New System.Drawing.Point(707, 37)
        Me.PictureBox6.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox6.Name = "PictureBox6"
        Me.PictureBox6.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox6.TabIndex = 42
        Me.PictureBox6.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox6, "This is the unique ID from the Identity Provider")
        '
        'PictureBox7
        '
        Me.PictureBox7.Image = CType(resources.GetObject("PictureBox7.Image"), System.Drawing.Image)
        Me.PictureBox7.Location = New System.Drawing.Point(707, 118)
        Me.PictureBox7.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox7.Name = "PictureBox7"
        Me.PictureBox7.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox7.TabIndex = 48
        Me.PictureBox7.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox7, "This is the digital certificate file supplied  by the identity provider")
        '
        'PictureBox8
        '
        Me.PictureBox8.Image = CType(resources.GetObject("PictureBox8.Image"), System.Drawing.Image)
        Me.PictureBox8.Location = New System.Drawing.Point(707, 172)
        Me.PictureBox8.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox8.Name = "PictureBox8"
        Me.PictureBox8.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox8.TabIndex = 53
        Me.PictureBox8.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox8, "<website URL>/general/sso.aspx")
        '
        'PictureBox9
        '
        Me.PictureBox9.Image = CType(resources.GetObject("PictureBox9.Image"), System.Drawing.Image)
        Me.PictureBox9.Location = New System.Drawing.Point(707, 145)
        Me.PictureBox9.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox9.Name = "PictureBox9"
        Me.PictureBox9.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox9.TabIndex = 54
        Me.PictureBox9.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox9, "Service Provider Entity ID")
        '
        'PictureBox10
        '
        Me.PictureBox10.Image = CType(resources.GetObject("PictureBox10.Image"), System.Drawing.Image)
        Me.PictureBox10.Location = New System.Drawing.Point(707, 91)
        Me.PictureBox10.Margin = New System.Windows.Forms.Padding(4)
        Me.PictureBox10.Name = "PictureBox10"
        Me.PictureBox10.Size = New System.Drawing.Size(21, 20)
        Me.PictureBox10.TabIndex = 61
        Me.PictureBox10.TabStop = False
        Me.ttPop.SetToolTip(Me.PictureBox10, "Url to call to log user out of system")
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.tpGeneral)
        Me.TabControl1.Controls.Add(Me.tpDatabase)
        Me.TabControl1.Controls.Add(Me.tpAuthenication)
        Me.TabControl1.Controls.Add(Me.tpEmail)
        Me.TabControl1.Controls.Add(Me.tpProxy)
        Me.TabControl1.Location = New System.Drawing.Point(249, 10)
        Me.TabControl1.Margin = New System.Windows.Forms.Padding(4)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(797, 378)
        Me.TabControl1.TabIndex = 5
        '
        'tpGeneral
        '
        Me.tpGeneral.BackColor = System.Drawing.Color.Gainsboro
        Me.tpGeneral.Controls.Add(Me.gpxGeneral)
        Me.tpGeneral.Location = New System.Drawing.Point(4, 25)
        Me.tpGeneral.Margin = New System.Windows.Forms.Padding(4)
        Me.tpGeneral.Name = "tpGeneral"
        Me.tpGeneral.Padding = New System.Windows.Forms.Padding(4)
        Me.tpGeneral.Size = New System.Drawing.Size(789, 349)
        Me.tpGeneral.TabIndex = 0
        Me.tpGeneral.Text = "General"
        '
        'gpxGeneral
        '
        Me.gpxGeneral.BackColor = System.Drawing.SystemColors.Control
        Me.gpxGeneral.Controls.Add(Me.cbxUseLanguage)
        Me.gpxGeneral.Controls.Add(Me.pnlVD)
        Me.gpxGeneral.Controls.Add(Me.txtErrorMessage)
        Me.gpxGeneral.Controls.Add(Me.Label4)
        Me.gpxGeneral.Controls.Add(Me.numInnactiveTimeout)
        Me.gpxGeneral.Controls.Add(Me.numServerTimeout)
        Me.gpxGeneral.Controls.Add(Me.Label3)
        Me.gpxGeneral.Controls.Add(Me.Label2)
        Me.gpxGeneral.Controls.Add(Me.txtBrowserTitle)
        Me.gpxGeneral.Controls.Add(Me.lblBrowserTitle)
        Me.gpxGeneral.Controls.Add(Me.lblName)
        Me.gpxGeneral.Controls.Add(Me.txtName)
        Me.gpxGeneral.Controls.Add(Me.PictureBox1)
        Me.gpxGeneral.Controls.Add(Me.cbxThemes)
        Me.gpxGeneral.Controls.Add(Me.lblTheme)
        Me.gpxGeneral.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gpxGeneral.Location = New System.Drawing.Point(4, 4)
        Me.gpxGeneral.Margin = New System.Windows.Forms.Padding(4)
        Me.gpxGeneral.Name = "gpxGeneral"
        Me.gpxGeneral.Padding = New System.Windows.Forms.Padding(4)
        Me.gpxGeneral.Size = New System.Drawing.Size(781, 341)
        Me.gpxGeneral.TabIndex = 0
        Me.gpxGeneral.TabStop = False
        Me.gpxGeneral.Text = "General"
        '
        'cbxUseLanguage
        '
        Me.cbxUseLanguage.AutoSize = True
        Me.cbxUseLanguage.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cbxUseLanguage.Location = New System.Drawing.Point(7, 197)
        Me.cbxUseLanguage.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxUseLanguage.Name = "cbxUseLanguage"
        Me.cbxUseLanguage.Size = New System.Drawing.Size(172, 21)
        Me.cbxUseLanguage.TabIndex = 38
        Me.cbxUseLanguage.Text = "Use browser language"
        Me.cbxUseLanguage.UseVisualStyleBackColor = True
        '
        'txtErrorMessage
        '
        Me.txtErrorMessage.Location = New System.Drawing.Point(159, 128)
        Me.txtErrorMessage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtErrorMessage.Name = "txtErrorMessage"
        Me.txtErrorMessage.Size = New System.Drawing.Size(593, 22)
        Me.txtErrorMessage.TabIndex = 34
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(8, 132)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(71, 17)
        Me.Label4.TabIndex = 37
        Me.Label4.Text = "Error Text"
        '
        'numInnactiveTimeout
        '
        Me.numInnactiveTimeout.Location = New System.Drawing.Point(340, 161)
        Me.numInnactiveTimeout.Margin = New System.Windows.Forms.Padding(4)
        Me.numInnactiveTimeout.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.numInnactiveTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numInnactiveTimeout.Name = "numInnactiveTimeout"
        Me.numInnactiveTimeout.Size = New System.Drawing.Size(48, 22)
        Me.numInnactiveTimeout.TabIndex = 36
        Me.numInnactiveTimeout.Value = New Decimal(New Integer() {20, 0, 0, 0})
        '
        'numServerTimeout
        '
        Me.numServerTimeout.Location = New System.Drawing.Point(159, 161)
        Me.numServerTimeout.Margin = New System.Windows.Forms.Padding(4)
        Me.numServerTimeout.Maximum = New Decimal(New Integer() {60, 0, 0, 0})
        Me.numServerTimeout.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numServerTimeout.Name = "numServerTimeout"
        Me.numServerTimeout.Size = New System.Drawing.Size(48, 22)
        Me.numServerTimeout.TabIndex = 35
        Me.numServerTimeout.Value = New Decimal(New Integer() {20, 0, 0, 0})
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(225, 164)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(104, 17)
        Me.Label3.TabIndex = 34
        Me.Label3.Text = "Innactive Timer"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(8, 164)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(105, 17)
        Me.Label2.TabIndex = 33
        Me.Label2.Text = "Server Timeout"
        '
        'txtBrowserTitle
        '
        Me.txtBrowserTitle.Location = New System.Drawing.Point(159, 91)
        Me.txtBrowserTitle.Margin = New System.Windows.Forms.Padding(4)
        Me.txtBrowserTitle.Name = "txtBrowserTitle"
        Me.txtBrowserTitle.Size = New System.Drawing.Size(593, 22)
        Me.txtBrowserTitle.TabIndex = 22
        '
        'lblBrowserTitle
        '
        Me.lblBrowserTitle.AutoSize = True
        Me.lblBrowserTitle.Location = New System.Drawing.Point(8, 95)
        Me.lblBrowserTitle.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblBrowserTitle.Name = "lblBrowserTitle"
        Me.lblBrowserTitle.Size = New System.Drawing.Size(90, 17)
        Me.lblBrowserTitle.TabIndex = 32
        Me.lblBrowserTitle.Text = "Browser Title"
        '
        'tpDatabase
        '
        Me.tpDatabase.Controls.Add(Me.gpxDB)
        Me.tpDatabase.Location = New System.Drawing.Point(4, 25)
        Me.tpDatabase.Margin = New System.Windows.Forms.Padding(4)
        Me.tpDatabase.Name = "tpDatabase"
        Me.tpDatabase.Padding = New System.Windows.Forms.Padding(4)
        Me.tpDatabase.Size = New System.Drawing.Size(789, 349)
        Me.tpDatabase.TabIndex = 1
        Me.tpDatabase.Text = "Database"
        Me.tpDatabase.UseVisualStyleBackColor = True
        '
        'gpxDB
        '
        Me.gpxDB.BackColor = System.Drawing.SystemColors.Control
        Me.gpxDB.Controls.Add(Me.txtProfileUser)
        Me.gpxDB.Controls.Add(Me.Label15)
        Me.gpxDB.Controls.Add(Me.pnlServer)
        Me.gpxDB.Controls.Add(Me.Label14)
        Me.gpxDB.Controls.Add(Me.numCommandTimeout)
        Me.gpxDB.Controls.Add(Me.Label13)
        Me.gpxDB.Controls.Add(Me.Label12)
        Me.gpxDB.Controls.Add(Me.numConnectionTimeout)
        Me.gpxDB.Controls.Add(Me.Label6)
        Me.gpxDB.Controls.Add(Me.numMaxPool)
        Me.gpxDB.Controls.Add(Me.numMinPool)
        Me.gpxDB.Controls.Add(Me.Label5)
        Me.gpxDB.Controls.Add(Me.PictureBox4)
        Me.gpxDB.Controls.Add(Me.btnTestConn)
        Me.gpxDB.Controls.Add(Me.lblDbServer)
        Me.gpxDB.Controls.Add(Me.txtDbServer)
        Me.gpxDB.Controls.Add(Me.txtDbName)
        Me.gpxDB.Controls.Add(Me.lblDbName)
        Me.gpxDB.Controls.Add(Me.lblConnType)
        Me.gpxDB.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gpxDB.Location = New System.Drawing.Point(4, 4)
        Me.gpxDB.Margin = New System.Windows.Forms.Padding(4)
        Me.gpxDB.Name = "gpxDB"
        Me.gpxDB.Padding = New System.Windows.Forms.Padding(4)
        Me.gpxDB.Size = New System.Drawing.Size(781, 341)
        Me.gpxDB.TabIndex = 0
        Me.gpxDB.TabStop = False
        Me.gpxDB.Text = "Database Connectivity"
        '
        'txtProfileUser
        '
        Me.txtProfileUser.Location = New System.Drawing.Point(133, 158)
        Me.txtProfileUser.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProfileUser.Name = "txtProfileUser"
        Me.txtProfileUser.Size = New System.Drawing.Size(107, 22)
        Me.txtProfileUser.TabIndex = 39
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(8, 161)
        Me.Label15.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(82, 17)
        Me.Label15.TabIndex = 38
        Me.Label15.Text = "Profile User"
        '
        'pnlServer
        '
        Me.pnlServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlServer.Controls.Add(Me.btnCancelServer)
        Me.pnlServer.Controls.Add(Me.lblServer)
        Me.pnlServer.Controls.Add(Me.cbxServers)
        Me.pnlServer.Location = New System.Drawing.Point(141, 20)
        Me.pnlServer.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlServer.Name = "pnlServer"
        Me.pnlServer.Size = New System.Drawing.Size(301, 99)
        Me.pnlServer.TabIndex = 27
        Me.pnlServer.Visible = False
        '
        'btnCancelServer
        '
        Me.btnCancelServer.Location = New System.Drawing.Point(169, 59)
        Me.btnCancelServer.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancelServer.Name = "btnCancelServer"
        Me.btnCancelServer.Size = New System.Drawing.Size(100, 28)
        Me.btnCancelServer.TabIndex = 21
        Me.btnCancelServer.Text = "Cancel"
        Me.btnCancelServer.UseVisualStyleBackColor = True
        '
        'lblServer
        '
        Me.lblServer.AutoSize = True
        Me.lblServer.Location = New System.Drawing.Point(163, 6)
        Me.lblServer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblServer.Name = "lblServer"
        Me.lblServer.Size = New System.Drawing.Size(105, 17)
        Me.lblServer.TabIndex = 20
        Me.lblServer.Text = "Select a Server"
        '
        'cbxServers
        '
        Me.cbxServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbxServers.FormattingEnabled = True
        Me.cbxServers.Location = New System.Drawing.Point(40, 26)
        Me.cbxServers.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxServers.Name = "cbxServers"
        Me.cbxServers.Size = New System.Drawing.Size(228, 24)
        Me.cbxServers.TabIndex = 19
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(300, 129)
        Me.Label14.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(71, 17)
        Me.Label14.TabIndex = 37
        Me.Label14.Text = "Command"
        '
        'numCommandTimeout
        '
        Me.numCommandTimeout.Location = New System.Drawing.Point(376, 127)
        Me.numCommandTimeout.Margin = New System.Windows.Forms.Padding(4)
        Me.numCommandTimeout.Name = "numCommandTimeout"
        Me.numCommandTimeout.Size = New System.Drawing.Size(67, 22)
        Me.numCommandTimeout.TabIndex = 36
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(141, 129)
        Me.Label13.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(79, 17)
        Me.Label13.TabIndex = 35
        Me.Label13.Text = "Connection"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(305, 97)
        Me.Label12.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(66, 17)
        Me.Label12.TabIndex = 34
        Me.Label12.Text = "Maximum"
        '
        'numConnectionTimeout
        '
        Me.numConnectionTimeout.Location = New System.Drawing.Point(224, 127)
        Me.numConnectionTimeout.Margin = New System.Windows.Forms.Padding(4)
        Me.numConnectionTimeout.Name = "numConnectionTimeout"
        Me.numConnectionTimeout.Size = New System.Drawing.Size(67, 22)
        Me.numConnectionTimeout.TabIndex = 33
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(8, 129)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(59, 17)
        Me.Label6.TabIndex = 32
        Me.Label6.Text = "Timeout"
        '
        'numMaxPool
        '
        Me.numMaxPool.Location = New System.Drawing.Point(376, 95)
        Me.numMaxPool.Margin = New System.Windows.Forms.Padding(4)
        Me.numMaxPool.Maximum = New Decimal(New Integer() {9999, 0, 0, 0})
        Me.numMaxPool.Name = "numMaxPool"
        Me.numMaxPool.Size = New System.Drawing.Size(67, 22)
        Me.numMaxPool.TabIndex = 31
        '
        'numMinPool
        '
        Me.numMinPool.Location = New System.Drawing.Point(224, 95)
        Me.numMinPool.Margin = New System.Windows.Forms.Padding(4)
        Me.numMinPool.Name = "numMinPool"
        Me.numMinPool.Size = New System.Drawing.Size(67, 22)
        Me.numMinPool.TabIndex = 30
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(159, 97)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(63, 17)
        Me.Label5.TabIndex = 29
        Me.Label5.Text = "Minimum"
        '
        'tpAuthenication
        '
        Me.tpAuthenication.Controls.Add(Me.gpxAuthentication)
        Me.tpAuthenication.Location = New System.Drawing.Point(4, 25)
        Me.tpAuthenication.Margin = New System.Windows.Forms.Padding(4)
        Me.tpAuthenication.Name = "tpAuthenication"
        Me.tpAuthenication.Padding = New System.Windows.Forms.Padding(4)
        Me.tpAuthenication.Size = New System.Drawing.Size(789, 349)
        Me.tpAuthenication.TabIndex = 2
        Me.tpAuthenication.Text = "Authenication"
        Me.tpAuthenication.UseVisualStyleBackColor = True
        '
        'gpxAuthentication
        '
        Me.gpxAuthentication.BackColor = System.Drawing.SystemColors.Control
        Me.gpxAuthentication.Controls.Add(Me.lblAuth2)
        Me.gpxAuthentication.Controls.Add(Me.cbxAuth)
        Me.gpxAuthentication.Controls.Add(Me.pnlSSO)
        Me.gpxAuthentication.Controls.Add(Me.pnlActiveDirectory)
        Me.gpxAuthentication.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gpxAuthentication.Location = New System.Drawing.Point(4, 4)
        Me.gpxAuthentication.Margin = New System.Windows.Forms.Padding(4)
        Me.gpxAuthentication.Name = "gpxAuthentication"
        Me.gpxAuthentication.Padding = New System.Windows.Forms.Padding(4)
        Me.gpxAuthentication.Size = New System.Drawing.Size(781, 341)
        Me.gpxAuthentication.TabIndex = 0
        Me.gpxAuthentication.TabStop = False
        Me.gpxAuthentication.Text = "Authentication"
        '
        'pnlSSO
        '
        Me.pnlSSO.Controls.Add(Me.PictureBox10)
        Me.pnlSSO.Controls.Add(Me.txtProviderLogoutURL)
        Me.pnlSSO.Controls.Add(Me.Label22)
        Me.pnlSSO.Controls.Add(Me.cbAssertionSigned)
        Me.pnlSSO.Controls.Add(Me.Label21)
        Me.pnlSSO.Controls.Add(Me.cbResponseSigned)
        Me.pnlSSO.Controls.Add(Me.btnCert)
        Me.pnlSSO.Controls.Add(Me.PictureBox9)
        Me.pnlSSO.Controls.Add(Me.PictureBox8)
        Me.pnlSSO.Controls.Add(Me.txtIngenuityURL)
        Me.pnlSSO.Controls.Add(Me.txtIngenuityID)
        Me.pnlSSO.Controls.Add(Me.Label20)
        Me.pnlSSO.Controls.Add(Me.Label19)
        Me.pnlSSO.Controls.Add(Me.PictureBox7)
        Me.pnlSSO.Controls.Add(Me.txtProviderCertFile)
        Me.pnlSSO.Controls.Add(Me.Label18)
        Me.pnlSSO.Controls.Add(Me.PictureBox5)
        Me.pnlSSO.Controls.Add(Me.Label16)
        Me.pnlSSO.Controls.Add(Me.txtProviderURL)
        Me.pnlSSO.Controls.Add(Me.PictureBox6)
        Me.pnlSSO.Controls.Add(Me.txtProviderID)
        Me.pnlSSO.Controls.Add(Me.Label17)
        Me.pnlSSO.Location = New System.Drawing.Point(7, 49)
        Me.pnlSSO.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlSSO.Name = "pnlSSO"
        Me.pnlSSO.Size = New System.Drawing.Size(765, 286)
        Me.pnlSSO.TabIndex = 41
        '
        'txtProviderLogoutURL
        '
        Me.txtProviderLogoutURL.Location = New System.Drawing.Point(247, 89)
        Me.txtProviderLogoutURL.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProviderLogoutURL.Name = "txtProviderLogoutURL"
        Me.txtProviderLogoutURL.Size = New System.Drawing.Size(455, 22)
        Me.txtProviderLogoutURL.TabIndex = 45
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(5, 92)
        Me.Label22.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(190, 17)
        Me.Label22.TabIndex = 59
        Me.Label22.Text = "Identity Provider Logout URL"
        '
        'cbAssertionSigned
        '
        Me.cbAssertionSigned.AutoSize = True
        Me.cbAssertionSigned.Location = New System.Drawing.Point(380, 10)
        Me.cbAssertionSigned.Margin = New System.Windows.Forms.Padding(4)
        Me.cbAssertionSigned.Name = "cbAssertionSigned"
        Me.cbAssertionSigned.Size = New System.Drawing.Size(89, 21)
        Me.cbAssertionSigned.TabIndex = 58
        Me.cbAssertionSigned.Text = "Assertion"
        Me.cbAssertionSigned.UseVisualStyleBackColor = True
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(4, 11)
        Me.Label21.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(161, 17)
        Me.Label21.TabIndex = 57
        Me.Label21.Text = "Identity Provider Signing"
        '
        'cbResponseSigned
        '
        Me.cbResponseSigned.AutoSize = True
        Me.cbResponseSigned.Location = New System.Drawing.Point(247, 10)
        Me.cbResponseSigned.Margin = New System.Windows.Forms.Padding(4)
        Me.cbResponseSigned.Name = "cbResponseSigned"
        Me.cbResponseSigned.Size = New System.Drawing.Size(94, 21)
        Me.cbResponseSigned.TabIndex = 56
        Me.cbResponseSigned.Text = "Response"
        Me.cbResponseSigned.UseVisualStyleBackColor = True
        '
        'btnCert
        '
        Me.btnCert.Location = New System.Drawing.Point(731, 114)
        Me.btnCert.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCert.Name = "btnCert"
        Me.btnCert.Size = New System.Drawing.Size(31, 25)
        Me.btnCert.TabIndex = 0
        Me.btnCert.Text = "..."
        Me.btnCert.TextAlign = System.Drawing.ContentAlignment.TopCenter
        Me.btnCert.UseVisualStyleBackColor = False
        '
        'txtIngenuityURL
        '
        Me.txtIngenuityURL.Location = New System.Drawing.Point(247, 170)
        Me.txtIngenuityURL.Margin = New System.Windows.Forms.Padding(4)
        Me.txtIngenuityURL.Name = "txtIngenuityURL"
        Me.txtIngenuityURL.Size = New System.Drawing.Size(455, 22)
        Me.txtIngenuityURL.TabIndex = 52
        '
        'txtIngenuityID
        '
        Me.txtIngenuityID.Location = New System.Drawing.Point(247, 143)
        Me.txtIngenuityID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtIngenuityID.Name = "txtIngenuityID"
        Me.txtIngenuityID.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtIngenuityID.Size = New System.Drawing.Size(455, 22)
        Me.txtIngenuityID.TabIndex = 51
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(4, 174)
        Me.Label20.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(148, 17)
        Me.Label20.TabIndex = 50
        Me.Label20.Text = "Ingenuity Service URL"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(4, 146)
        Me.Label19.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(121, 17)
        Me.Label19.TabIndex = 49
        Me.Label19.Text = "Ingenuity Entity ID"
        '
        'txtProviderCertFile
        '
        Me.txtProviderCertFile.Location = New System.Drawing.Point(247, 116)
        Me.txtProviderCertFile.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProviderCertFile.Name = "txtProviderCertFile"
        Me.txtProviderCertFile.Size = New System.Drawing.Size(455, 22)
        Me.txtProviderCertFile.TabIndex = 47
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(4, 119)
        Me.Label18.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(203, 17)
        Me.Label18.TabIndex = 46
        Me.Label18.Text = "Identity Provider Certificate File"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(4, 65)
        Me.Label16.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(236, 17)
        Me.Label16.TabIndex = 44
        Me.Label16.Text = "Identity Provider Authentication URL"
        '
        'txtProviderURL
        '
        Me.txtProviderURL.Location = New System.Drawing.Point(245, 62)
        Me.txtProviderURL.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProviderURL.Name = "txtProviderURL"
        Me.txtProviderURL.Size = New System.Drawing.Size(455, 22)
        Me.txtProviderURL.TabIndex = 43
        '
        'txtProviderID
        '
        Me.txtProviderID.Location = New System.Drawing.Point(247, 34)
        Me.txtProviderID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProviderID.Name = "txtProviderID"
        Me.txtProviderID.Size = New System.Drawing.Size(455, 22)
        Me.txtProviderID.TabIndex = 40
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(4, 38)
        Me.Label17.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(166, 17)
        Me.Label17.TabIndex = 41
        Me.Label17.Text = "Identity Provider Entity ID"
        '
        'pnlActiveDirectory
        '
        Me.pnlActiveDirectory.Controls.Add(Me.btnTestADConn)
        Me.pnlActiveDirectory.Controls.Add(Me.PictureBox3)
        Me.pnlActiveDirectory.Controls.Add(Me.lblDomains)
        Me.pnlActiveDirectory.Controls.Add(Me.txtDomains)
        Me.pnlActiveDirectory.Controls.Add(Me.PictureBox2)
        Me.pnlActiveDirectory.Controls.Add(Me.txtActiveDir)
        Me.pnlActiveDirectory.Controls.Add(Me.lblActiveDir)
        Me.pnlActiveDirectory.Location = New System.Drawing.Point(8, 66)
        Me.pnlActiveDirectory.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlActiveDirectory.Name = "pnlActiveDirectory"
        Me.pnlActiveDirectory.Size = New System.Drawing.Size(763, 126)
        Me.pnlActiveDirectory.TabIndex = 40
        '
        'btnTestADConn
        '
        Me.btnTestADConn.AutoSize = True
        Me.btnTestADConn.Location = New System.Drawing.Point(208, 75)
        Me.btnTestADConn.Margin = New System.Windows.Forms.Padding(4)
        Me.btnTestADConn.Name = "btnTestADConn"
        Me.btnTestADConn.Size = New System.Drawing.Size(161, 33)
        Me.btnTestADConn.TabIndex = 46
        Me.btnTestADConn.Text = "Test Connection"
        Me.btnTestADConn.UseVisualStyleBackColor = True
        '
        'lblDomains
        '
        Me.lblDomains.AutoSize = True
        Me.lblDomains.Location = New System.Drawing.Point(5, 43)
        Me.lblDomains.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDomains.Name = "lblDomains"
        Me.lblDomains.Size = New System.Drawing.Size(63, 17)
        Me.lblDomains.TabIndex = 44
        Me.lblDomains.Text = "Domains"
        '
        'txtDomains
        '
        Me.txtDomains.Location = New System.Drawing.Point(208, 39)
        Me.txtDomains.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDomains.Name = "txtDomains"
        Me.txtDomains.Size = New System.Drawing.Size(533, 22)
        Me.txtDomains.TabIndex = 43
        '
        'txtActiveDir
        '
        Me.txtActiveDir.Location = New System.Drawing.Point(208, 7)
        Me.txtActiveDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtActiveDir.Name = "txtActiveDir"
        Me.txtActiveDir.Size = New System.Drawing.Size(533, 22)
        Me.txtActiveDir.TabIndex = 40
        '
        'lblActiveDir
        '
        Me.lblActiveDir.AutoSize = True
        Me.lblActiveDir.Location = New System.Drawing.Point(5, 11)
        Me.lblActiveDir.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblActiveDir.Name = "lblActiveDir"
        Me.lblActiveDir.Size = New System.Drawing.Size(153, 17)
        Me.lblActiveDir.TabIndex = 41
        Me.lblActiveDir.Text = "Active Directory Server"
        '
        'tpEmail
        '
        Me.tpEmail.BackColor = System.Drawing.SystemColors.Control
        Me.tpEmail.Controls.Add(Me.gbTestEmail)
        Me.tpEmail.Controls.Add(Me.GroupBox1)
        Me.tpEmail.Location = New System.Drawing.Point(4, 25)
        Me.tpEmail.Margin = New System.Windows.Forms.Padding(4)
        Me.tpEmail.Name = "tpEmail"
        Me.tpEmail.Padding = New System.Windows.Forms.Padding(4)
        Me.tpEmail.Size = New System.Drawing.Size(789, 349)
        Me.tpEmail.TabIndex = 3
        Me.tpEmail.Text = "Email"
        '
        'gbTestEmail
        '
        Me.gbTestEmail.Controls.Add(Me.tbSendTo)
        Me.gbTestEmail.Controls.Add(Me.txtSendTo)
        Me.gbTestEmail.Controls.Add(Me.btnTestEmail)
        Me.gbTestEmail.Location = New System.Drawing.Point(19, 95)
        Me.gbTestEmail.Margin = New System.Windows.Forms.Padding(4)
        Me.gbTestEmail.Name = "gbTestEmail"
        Me.gbTestEmail.Padding = New System.Windows.Forms.Padding(4)
        Me.gbTestEmail.Size = New System.Drawing.Size(737, 130)
        Me.gbTestEmail.TabIndex = 37
        Me.gbTestEmail.TabStop = False
        Me.gbTestEmail.Text = "Test eMail Function"
        '
        'tbSendTo
        '
        Me.tbSendTo.Location = New System.Drawing.Point(139, 21)
        Me.tbSendTo.Margin = New System.Windows.Forms.Padding(4)
        Me.tbSendTo.Name = "tbSendTo"
        Me.tbSendTo.Size = New System.Drawing.Size(599, 22)
        Me.tbSendTo.TabIndex = 42
        '
        'txtSendTo
        '
        Me.txtSendTo.AutoSize = True
        Me.txtSendTo.Location = New System.Drawing.Point(5, 25)
        Me.txtSendTo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.txtSendTo.Name = "txtSendTo"
        Me.txtSendTo.Size = New System.Drawing.Size(131, 17)
        Me.txtSendTo.TabIndex = 41
        Me.txtSendTo.Text = "Send Test eMail To"
        '
        'btnTestEmail
        '
        Me.btnTestEmail.Location = New System.Drawing.Point(139, 53)
        Me.btnTestEmail.Margin = New System.Windows.Forms.Padding(4)
        Me.btnTestEmail.Name = "btnTestEmail"
        Me.btnTestEmail.Size = New System.Drawing.Size(123, 28)
        Me.btnTestEmail.TabIndex = 40
        Me.btnTestEmail.Text = "Send Test eMail"
        Me.btnTestEmail.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMailTo)
        Me.GroupBox1.Controls.Add(Me.lblEmailErrors)
        Me.GroupBox1.Controls.Add(Me.lblMailTo)
        Me.GroupBox1.Controls.Add(Me.ddlEmailErrors)
        Me.GroupBox1.Location = New System.Drawing.Point(20, 4)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(737, 87)
        Me.GroupBox1.TabIndex = 36
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "eMail Errors"
        '
        'txtMailTo
        '
        Me.txtMailTo.Location = New System.Drawing.Point(137, 52)
        Me.txtMailTo.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMailTo.Name = "txtMailTo"
        Me.txtMailTo.Size = New System.Drawing.Size(599, 22)
        Me.txtMailTo.TabIndex = 35
        '
        'lblEmailErrors
        '
        Me.lblEmailErrors.AutoSize = True
        Me.lblEmailErrors.Location = New System.Drawing.Point(5, 22)
        Me.lblEmailErrors.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEmailErrors.Name = "lblEmailErrors"
        Me.lblEmailErrors.Size = New System.Drawing.Size(80, 17)
        Me.lblEmailErrors.TabIndex = 32
        Me.lblEmailErrors.Text = "Send Mode"
        '
        'lblMailTo
        '
        Me.lblMailTo.AutoSize = True
        Me.lblMailTo.Location = New System.Drawing.Point(5, 55)
        Me.lblMailTo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMailTo.Name = "lblMailTo"
        Me.lblMailTo.Size = New System.Drawing.Size(113, 17)
        Me.lblMailTo.TabIndex = 34
        Me.lblMailTo.Text = "Send to Address"
        '
        'ddlEmailErrors
        '
        Me.ddlEmailErrors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ddlEmailErrors.FormattingEnabled = True
        Me.ddlEmailErrors.Location = New System.Drawing.Point(137, 18)
        Me.ddlEmailErrors.Margin = New System.Windows.Forms.Padding(4)
        Me.ddlEmailErrors.Name = "ddlEmailErrors"
        Me.ddlEmailErrors.Size = New System.Drawing.Size(160, 24)
        Me.ddlEmailErrors.TabIndex = 33
        '
        'tpProxy
        '
        Me.tpProxy.BackColor = System.Drawing.SystemColors.Control
        Me.tpProxy.Controls.Add(Me.numProxyPort)
        Me.tpProxy.Controls.Add(Me.Label11)
        Me.tpProxy.Controls.Add(Me.txtProxyDomain)
        Me.tpProxy.Controls.Add(Me.txtProxyPassword)
        Me.tpProxy.Controls.Add(Me.txtProxyUser)
        Me.tpProxy.Controls.Add(Me.Label10)
        Me.tpProxy.Controls.Add(Me.Label9)
        Me.tpProxy.Controls.Add(Me.Label8)
        Me.tpProxy.Controls.Add(Me.txtProxyURI)
        Me.tpProxy.Controls.Add(Me.cbxProxy)
        Me.tpProxy.Controls.Add(Me.Label7)
        Me.tpProxy.Location = New System.Drawing.Point(4, 25)
        Me.tpProxy.Margin = New System.Windows.Forms.Padding(4)
        Me.tpProxy.Name = "tpProxy"
        Me.tpProxy.Padding = New System.Windows.Forms.Padding(4)
        Me.tpProxy.Size = New System.Drawing.Size(789, 349)
        Me.tpProxy.TabIndex = 4
        Me.tpProxy.Text = "Proxy"
        '
        'numProxyPort
        '
        Me.numProxyPort.Enabled = False
        Me.numProxyPort.Location = New System.Drawing.Point(92, 78)
        Me.numProxyPort.Margin = New System.Windows.Forms.Padding(4)
        Me.numProxyPort.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.numProxyPort.Name = "numProxyPort"
        Me.numProxyPort.Size = New System.Drawing.Size(73, 22)
        Me.numProxyPort.TabIndex = 10
        Me.numProxyPort.Value = New Decimal(New Integer() {8080, 0, 0, 0})
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(15, 80)
        Me.Label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(34, 17)
        Me.Label11.TabIndex = 9
        Me.Label11.Text = "Port"
        '
        'txtProxyDomain
        '
        Me.txtProxyDomain.Enabled = False
        Me.txtProxyDomain.Location = New System.Drawing.Point(92, 169)
        Me.txtProxyDomain.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProxyDomain.Name = "txtProxyDomain"
        Me.txtProxyDomain.Size = New System.Drawing.Size(225, 22)
        Me.txtProxyDomain.TabIndex = 8
        '
        'txtProxyPassword
        '
        Me.txtProxyPassword.Enabled = False
        Me.txtProxyPassword.Location = New System.Drawing.Point(92, 138)
        Me.txtProxyPassword.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProxyPassword.Name = "txtProxyPassword"
        Me.txtProxyPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtProxyPassword.Size = New System.Drawing.Size(225, 22)
        Me.txtProxyPassword.TabIndex = 7
        '
        'txtProxyUser
        '
        Me.txtProxyUser.Enabled = False
        Me.txtProxyUser.Location = New System.Drawing.Point(92, 107)
        Me.txtProxyUser.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProxyUser.Name = "txtProxyUser"
        Me.txtProxyUser.Size = New System.Drawing.Size(225, 22)
        Me.txtProxyUser.TabIndex = 6
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(15, 172)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(56, 17)
        Me.Label10.TabIndex = 5
        Me.Label10.Text = "Domain"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(15, 142)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(69, 17)
        Me.Label9.TabIndex = 4
        Me.Label9.Text = "Password"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(15, 111)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(38, 17)
        Me.Label8.TabIndex = 3
        Me.Label8.Text = "User"
        '
        'txtProxyURI
        '
        Me.txtProxyURI.Enabled = False
        Me.txtProxyURI.Location = New System.Drawing.Point(92, 48)
        Me.txtProxyURI.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProxyURI.Name = "txtProxyURI"
        Me.txtProxyURI.Size = New System.Drawing.Size(459, 22)
        Me.txtProxyURI.TabIndex = 2
        '
        'cbxProxy
        '
        Me.cbxProxy.AutoSize = True
        Me.cbxProxy.CheckAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.cbxProxy.Location = New System.Drawing.Point(15, 14)
        Me.cbxProxy.Margin = New System.Windows.Forms.Padding(4)
        Me.cbxProxy.Name = "cbxProxy"
        Me.cbxProxy.Size = New System.Drawing.Size(293, 21)
        Me.cbxProxy.TabIndex = 1
        Me.cbxProxy.Text = "Use proxy server for external service calls" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        Me.cbxProxy.UseVisualStyleBackColor = True
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(15, 52)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(70, 17)
        Me.Label7.TabIndex = 0
        Me.Label7.Text = "Proxy URI"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(127, 48)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 21
        Me.Button1.Text = "Cancel"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(79, 5)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(123, 13)
        Me.Label1.TabIndex = 20
        Me.Label1.Text = "Select a Virtual Directory"
        '
        'ComboBox1
        '
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Location = New System.Drawing.Point(30, 21)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(172, 25)
        Me.ComboBox1.TabIndex = 19
        '
        'fileCert
        '
        Me.fileCert.Filter = "Certificate Files(*.crt;*.cer)|*.crt;*.cer|All files (*.*)|*.*"
        '
        'Timer1
        '
        '
        'lblMsg
        '
        Me.lblMsg.AutoSize = True
        Me.lblMsg.Location = New System.Drawing.Point(529, 10)
        Me.lblMsg.Name = "lblMsg"
        Me.lblMsg.Size = New System.Drawing.Size(0, 17)
        Me.lblMsg.TabIndex = 15
        Me.lblMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.lblMsg.Visible = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1049, 443)
        Me.Controls.Add(Me.tvSites)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.Panel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlVD.ResumeLayout(False)
        Me.pnlVD.PerformLayout()
        CType(Me.PictureBox4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox5, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox6, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox7, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox8, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox9, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox10, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabControl1.ResumeLayout(False)
        Me.tpGeneral.ResumeLayout(False)
        Me.gpxGeneral.ResumeLayout(False)
        Me.gpxGeneral.PerformLayout()
        CType(Me.numInnactiveTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numServerTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tpDatabase.ResumeLayout(False)
        Me.gpxDB.ResumeLayout(False)
        Me.gpxDB.PerformLayout()
        Me.pnlServer.ResumeLayout(False)
        Me.pnlServer.PerformLayout()
        CType(Me.numCommandTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numConnectionTimeout, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMaxPool, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMinPool, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tpAuthenication.ResumeLayout(False)
        Me.gpxAuthentication.ResumeLayout(False)
        Me.gpxAuthentication.PerformLayout()
        Me.pnlSSO.ResumeLayout(False)
        Me.pnlSSO.PerformLayout()
        Me.pnlActiveDirectory.ResumeLayout(False)
        Me.pnlActiveDirectory.PerformLayout()
        Me.tpEmail.ResumeLayout(False)
        Me.gbTestEmail.ResumeLayout(False)
        Me.gbTestEmail.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.tpProxy.ResumeLayout(False)
        Me.tpProxy.PerformLayout()
        CType(Me.numProxyPort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnDelete As System.Windows.Forms.Button
    Friend WithEvents btnAdd As System.Windows.Forms.Button
    Friend WithEvents lblDbName As System.Windows.Forms.Label
    Friend WithEvents lblDbServer As System.Windows.Forms.Label
    Friend WithEvents lblTheme As System.Windows.Forms.Label
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents lblAuth As System.Windows.Forms.Label
    Friend WithEvents txtDbName As System.Windows.Forms.TextBox
    Friend WithEvents txtDbServer As System.Windows.Forms.TextBox
    Friend WithEvents cbxAuth As System.Windows.Forms.ComboBox
    Friend WithEvents btnTestConn As System.Windows.Forms.Button
    Friend WithEvents lblConnType As System.Windows.Forms.Label
    Friend WithEvents tvSites As System.Windows.Forms.TreeView
    Friend WithEvents cbxThemes As System.Windows.Forms.ComboBox
    Friend WithEvents cbxVdirs As System.Windows.Forms.ComboBox
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents pnlVD As System.Windows.Forms.Panel
    Friend WithEvents lblVD As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents btnCancelVD As System.Windows.Forms.Button
    Friend WithEvents ttPop As System.Windows.Forms.ToolTip
    Friend WithEvents lblAuth2 As System.Windows.Forms.Label
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents tpGeneral As System.Windows.Forms.TabPage
    Friend WithEvents tpDatabase As System.Windows.Forms.TabPage
    Friend WithEvents tpAuthenication As System.Windows.Forms.TabPage
    Friend WithEvents gpxGeneral As System.Windows.Forms.GroupBox
    Friend WithEvents gpxDB As System.Windows.Forms.GroupBox
    Friend WithEvents gpxAuthentication As System.Windows.Forms.GroupBox
    Friend WithEvents pnlServer As System.Windows.Forms.Panel
    Friend WithEvents btnCancelServer As System.Windows.Forms.Button
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents cbxServers As System.Windows.Forms.ComboBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents PictureBox4 As System.Windows.Forms.PictureBox
    Friend WithEvents lblBrowserTitle As System.Windows.Forms.Label
    Friend WithEvents txtBrowserTitle As System.Windows.Forms.TextBox
    Friend WithEvents tpEmail As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents txtMailTo As System.Windows.Forms.TextBox
    Friend WithEvents lblMailTo As System.Windows.Forms.Label
    Friend WithEvents ddlEmailErrors As System.Windows.Forms.ComboBox
    Friend WithEvents lblEmailErrors As System.Windows.Forms.Label
    Friend WithEvents gbTestEmail As System.Windows.Forms.GroupBox
    Friend WithEvents tbSendTo As System.Windows.Forms.TextBox
    Friend WithEvents txtSendTo As System.Windows.Forms.Label
    Friend WithEvents btnTestEmail As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents numInnactiveTimeout As System.Windows.Forms.NumericUpDown
    Friend WithEvents numServerTimeout As System.Windows.Forms.NumericUpDown
    Friend WithEvents txtErrorMessage As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents numMinPool As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents numConnectionTimeout As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents numMaxPool As System.Windows.Forms.NumericUpDown
    Friend WithEvents tpProxy As System.Windows.Forms.TabPage
    Friend WithEvents cbxProxy As System.Windows.Forms.CheckBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtProxyDomain As System.Windows.Forms.TextBox
    Friend WithEvents txtProxyPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtProxyUser As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtProxyURI As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents numProxyPort As System.Windows.Forms.NumericUpDown
    Friend WithEvents numCommandTimeout As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents txtProfileUser As System.Windows.Forms.TextBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents cbxUseLanguage As System.Windows.Forms.CheckBox
    Friend WithEvents pnlActiveDirectory As System.Windows.Forms.Panel
    Friend WithEvents btnTestADConn As System.Windows.Forms.Button
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents lblDomains As System.Windows.Forms.Label
    Friend WithEvents txtDomains As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents txtActiveDir As System.Windows.Forms.TextBox
    Friend WithEvents lblActiveDir As System.Windows.Forms.Label
    Friend WithEvents pnlSSO As System.Windows.Forms.Panel
    Friend WithEvents PictureBox5 As System.Windows.Forms.PictureBox
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents txtProviderURL As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox6 As System.Windows.Forms.PictureBox
    Friend WithEvents txtProviderID As System.Windows.Forms.TextBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents txtProviderCertFile As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox7 As System.Windows.Forms.PictureBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents txtIngenuityURL As System.Windows.Forms.TextBox
    Friend WithEvents txtIngenuityID As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox8 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox9 As System.Windows.Forms.PictureBox
    Friend WithEvents btnCert As System.Windows.Forms.Button
    Friend WithEvents fileCert As System.Windows.Forms.OpenFileDialog
    Friend WithEvents cbAssertionSigned As CheckBox
    Friend WithEvents Label21 As Label
    Friend WithEvents cbResponseSigned As CheckBox
    Friend WithEvents PictureBox10 As PictureBox
    Friend WithEvents txtProviderLogoutURL As TextBox
    Friend WithEvents Label22 As Label
    Friend WithEvents Timer1 As Timer
    Friend WithEvents lblMsg As Label
End Class
