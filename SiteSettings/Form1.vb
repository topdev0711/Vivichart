Imports System.Data.Sql

Imports System.Xml
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.DirectoryServices
Imports System.Drawing

Public Class Form1

#Region "Varaibles & Properties"
    Private ChangesMade As Boolean
    Private Settings As SiteSettings
    Private Path As String
    Private SettingsFile As String = "siteSettings.config"
#End Region
#Region "Page Events"
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim dir As String = Environment.CurrentDirectory

        If Not Regex.IsMatch(dir, "\\bin([\\]|$|\s*)*", RegexOptions.IgnoreCase) Then
            MessageBox.Show("The Site Settings Configuration Manager must be installed in the bin directory (or a sub directory of the bin) of the web site")
            Application.Exit()
        Else
            Path = Regex.Replace(dir, "\\bin.*", "", RegexOptions.IgnoreCase)
            Try
                Settings = New SiteSettings(Path + "\" + SettingsFile)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If

        'populate themes
        cbxThemes.DataSource = GetThemes(Path)

        'populate virtual directories
        cbxVdirs.DataSource = GetVirtualDirectories()

        'populate email errors .ddl     
        ddlEmailErrors.Items.Add("NoEmail")
        ddlEmailErrors.Items.Add("EnableEmail")
        ddlEmailErrors.Items.Add("AutoEmail")

        Dim node As TreeNode
        Me.Text = "Site Settings"
        'Clear the treeview
        tvSites.Nodes.Clear()

        For Each site As Site In Settings.sites
            node = New TreeNode(site.Name)
            node.Tag = site
            tvSites.Nodes.Add(node)
        Next

        'set up default
        If tvSites.Nodes.Count = 0 Then add("default")
    End Sub
#End Region

    Private Sub PopulateFields(ByVal node As Site)

        If node.Name.Equals("default") Then
            txtName.Enabled = False
            btnDelete.Enabled = False
            PictureBox1.Visible = False
        Else
            txtName.Enabled = True
            btnDelete.Enabled = True
            PictureBox1.Visible = True
        End If
        txtName.Text = node.Name

        cbxThemes.SelectedItem = node.Theme
        txtDbServer.Text = node.DbServer
        txtDbName.Text = node.DbName
        numMinPool.Value = node.MinPoolSize
        numMaxPool.Value = node.MaxPoolSize
        numConnectionTimeout.Value = node.ConnectionTimeout
        numCommandTimeout.Value = node.CommandTimeout
        txtProfileUser.Text = node.ProfileUser

        If Not String.IsNullOrEmpty(node.AutheticationMethod) Then
            cbxAuth.SelectedItem = node.AutheticationMethod
        Else
            cbxAuth.SelectedIndex = 0
        End If

        '// email erros enabled
        If Not String.IsNullOrEmpty(node.EmailErrorsEnabled) Then
            ddlEmailErrors.SelectedItem = node.EmailErrorsEnabled
        Else
            ddlEmailErrors.SelectedIndex = 0
        End If

        '// email errors to
        txtMailTo.Text = node.EmailErrorsTo

        Select Case node.AutheticationMethod
            Case "Active Directory"
                txtActiveDir.Text = node.ActiveDirectoryPath
                txtDomains.Text = node.ActiveDirectoryDomains
            Case "Single Sign On"
                txtProviderID.Text = node.ssoProviderID
                txtProviderURL.Text = node.SsoProviderURL
                txtProviderLogoutURL.Text = node.SsoProviderLogoutURL
                txtProviderCertFile.Text = node.SsoProviderCertFile

                txtIngenuityID.Text = node.SsoComsumerID
                txtIngenuityURL.Text = node.SsoConsumerURL
                cbResponseSigned.Checked = node.SsoWantSAMLResponseSigned
                cbAssertionSigned.Checked = node.SsoWantAssertionSigned
            Case Else
                txtActiveDir.Text = ""
                txtDomains.Text = ""
                txtProviderID.Text = ""
                txtProviderURL.Text = ""
                txtProviderCertFile.Text = ""
                txtIngenuityID.Text = ""
                txtIngenuityURL.Text = ""
        End Select

        AuthenticateVisibility(node.AutheticationMethod)

        txtBrowserTitle.Text = node.BrowserTitle
        txtErrorMessage.Text = node.ErrorMessage

        If node.ServerTimeout = 0 Then
            numServerTimeout.Value = 20
        Else
            numServerTimeout.Value = node.ServerTimeout
        End If

        If node.InnactiveTimeout = 0 Then
            numInnactiveTimeout.Value = 20
        Else
            numInnactiveTimeout.Value = node.InnactiveTimeout
        End If

        cbxUseLanguage.Checked = node.UseLanguage

        cbxProxy.Checked = node.ProxyUse
        txtProxyURI.Text = node.ProxyURI
        numProxyPort.Value = node.ProxyPort
        txtProxyUser.Text = node.ProxyUser
        txtProxyPassword.Text = node.ProxyPassword
        txtProxyDomain.Text = node.ProxyDomain

        txtProxyURI.Enabled = node.ProxyUse
        numProxyPort.Enabled = node.ProxyUse
        txtProxyUser.Enabled = node.ProxyUse
        txtProxyPassword.Enabled = node.ProxyUse
        txtProxyDomain.Enabled = node.ProxyUse

    End Sub

    Private Sub add(ByVal name As String)
        Dim site As New Site()
        Dim node As New TreeNode()
        Dim index As Integer
        node.Text = name
        node.Tag = site
        site.Name = name
        site.DbServer = "localhost"
        site.DbConnection = "sql"
        site.MinPoolSize = 0
        site.MaxPoolSize = 100
        site.ConnectionTimeout = 15
        site.CommandTimeout = 30
        site.DbName = "Ingenuity"
        site.AutheticationMethod = "Forms"
        site.ssoComsumerID = "https://iawresources.co.uk/sp"
        site.ssoConsumerURL = "https://yourdomain/iwebfolder/security/sso.aspx"
        site.BrowserTitle = "Ingenuity"
        site.ErrorMessage = "Please contact your technical support team."
        site.ServerTimeout = 20
        site.InnactiveTimeout = 20
        site.ProfileUser = ""
        site.ProxyUse = False
        site.ProxyURI = String.Empty
        site.ProxyPort = 8080
        site.ProxyUser = String.Empty
        site.ProxyPassword = String.Empty
        site.ProxyDomain = String.Empty

        index = tvSites.Nodes.Add(node)
        tvSites.SelectedNode = tvSites.Nodes(index)
        PopulateFields(node.Tag)
    End Sub

    Private Sub SaveFields(ByVal site As Site)
        site.Name = txtName.Text.Trim
        site.Theme = cbxThemes.SelectedItem
        site.DbServer = txtDbServer.Text.Trim
        site.DbName = txtDbName.Text.Trim
        site.DbConnection = "sql"
        site.MinPoolSize = numMinPool.Value
        site.MaxPoolSize = numMaxPool.Value
        site.ConnectionTimeout = numConnectionTimeout.Value
        site.CommandTimeout = numCommandTimeout.Value
        site.ProfileUser = txtProfileUser.Text.Trim
        site.AutheticationMethod = cbxAuth.SelectedItem
        site.ActiveDirectoryPath = txtActiveDir.Text.Trim
        site.ActiveDirectoryDomains = txtDomains.Text.Trim
        site.ssoProviderID = txtProviderID.Text.Trim
        site.ssoProviderURL = txtProviderURL.Text.Trim
        site.SsoProviderLogoutURL = txtProviderLogoutURL.Text.Trim
        site.SsoProviderCertFile = txtProviderCertFile.Text.Trim
        site.SsoComsumerID = txtIngenuityID.Text.Trim
        site.SsoConsumerURL = txtIngenuityURL.Text.Trim
        site.SsoWantSAMLResponseSigned = cbResponseSigned.Checked
        site.SsoWantAssertionSigned = cbAssertionSigned.Checked
        site.EmailErrorsEnabled = ddlEmailErrors.SelectedItem
        site.EmailErrorsTo = txtMailTo.Text
        site.BrowserTitle = txtBrowserTitle.Text.Trim
        site.ErrorMessage = txtErrorMessage.Text.Trim
        site.ServerTimeout = numServerTimeout.Value
        site.InnactiveTimeout = numInnactiveTimeout.Value
        site.UseLanguage = cbxUseLanguage.Checked
        site.ProxyUse = cbxProxy.Checked
        site.ProxyURI = txtProxyURI.Text.Trim
        site.ProxyPort = numProxyPort.Value
        site.ProxyUser = txtProxyUser.Text.Trim
        site.ProxyPassword = txtProxyPassword.Text.Trim
        site.ProxyDomain = txtProxyDomain.Text.Trim
    End Sub

    Private Function GetThemes(ByVal filePath As String) As IList
        Dim themes As New ArrayList
        Try
            For Each s As String In Directory.GetDirectories(filePath + "\" + "App_Themes")
                s = s.Substring(s.LastIndexOf("\") + 1)
                themes.Add(s)
            Next
        Catch ex As Exception
        End Try
        Return themes
    End Function

    Private Function GetServers() As IList
        Dim servers As New List(Of String)
        Dim DT As DataTable = SqlDataSourceEnumerator.Instance.GetDataSources
        Dim DR As DataRow
        'add an empty row to start with
        servers.Add(String.Empty)
        For Each DR In DT.Rows
            If DR(1) Is System.DBNull.Value Then
                servers.Add(DR(0))
            Else
                servers.Add(DR(0) + "\" + DR(1))
            End If
        Next
        Return servers
    End Function

    Private Function GetVirtualDirectories() As IList
        Dim iisServer As DirectoryEntry
        Dim vdirs As New List(Of String)
        Try
            Dim serverName As String = "localhost"
            Dim VirDirSchemaName As String = "IIsWebVirtualDir"
            iisServer = New DirectoryEntry("IIS://" + serverName + "/W3SVC/1")
            Dim folderRoot As DirectoryEntry = iisServer.Children.Find("Root", VirDirSchemaName)
            Dim entries As DirectoryEntries = folderRoot.Children()

            For Each d As DirectoryEntry In entries
                vdirs.Add(d.Name)
            Next

        Catch ex As Exception
            MsgBox("Please ensure that you have IIS 6 Management Compatibility features installed", MsgBoxStyle.Critical, "Error Reading Virtual Directories")
            Me.Close()
        End Try

        Return vdirs
    End Function

    Private Function Save() As Boolean
        Dim value As Boolean
        If check() Then
            Dim node As TreeNode = tvSites.SelectedNode
            SaveFields(node.Tag)
            'clear existing sites and replace with  all the ones in the treeview
            Settings.sites.Clear()
            For Each tNode As TreeNode In tvSites.Nodes
                Settings.sites.Add(CType(tNode.Tag, Site))
            Next
            Settings.WriteXml()
            ChangesMade = False
            StatusMessage("Changes Saved", True)
            SaveWebConfig()
            value = True
        Else
            StatusMessage("Please Complete all Fields", False)
        End If
        Return value
    End Function

    Private Sub StatusMessage(text As String, AllOk As Boolean)
        lblMsg.Text = text
        If AllOk Then
            lblMsg.ForeColor = Color.DarkGreen
        Else
            lblMsg.ForeColor = Color.Red
        End If
        lblMsg.Font = New Font(Label.DefaultFont, FontStyle.Bold)
        lblMsg.Visible = True
        Timer1.Interval = 4000
        Timer1.Start()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        lblMsg.Visible = False
    End Sub


    Private Sub SaveWebConfig()
        Dim doc As XmlDocument
        Dim filePath As String = "..\web.config"
        Dim authentication As XmlNode
        Dim attrMode As XmlAttribute
        Dim save As Boolean

        Try
            If System.IO.File.Exists(filePath) Then
                doc = New XmlDocument()
                doc.PreserveWhitespace = True
                doc.Load(filePath)
                authentication = doc.SelectSingleNode("//authentication")
                attrMode = authentication.Attributes("mode")

                Select Case cbxAuth.SelectedItem
                    Case "Forms", "Active Directory", "Single Sign On"
                        If attrMode.Value <> "Forms" Then
                            attrMode.Value = "Forms"
                            save = True
                        End If
                    Case "Network Logon"
                        If attrMode.Value <> "Windows" Then
                            attrMode.Value = "Windows"
                            save = True
                        End If
                    Case Else
                        If attrMode.Value <> "Forms" Then
                            attrMode.Value = "Forms"
                            save = True
                        End If
                End Select

                If save Then doc.Save(filePath)

            End If
        Catch ex As IO.FileNotFoundException
            'Throw New Exception("There should be a siteSettings.config file In the root directory Of this website, please contact support", ex)
        Catch ex As XmlException
            'Throw New Exception("The file siteSettings.config file In the root directory Of this website Is invalid, please contact support", ex)
        Catch ex As Exception
            'catch all, if no write access to dir
        Finally

        End Try

    End Sub

    Private Function check() As Boolean
        Dim val As Boolean
        If Not (String.IsNullOrEmpty(txtName.Text) Or _
                String.IsNullOrEmpty(txtDbServer.Text) Or _
                String.IsNullOrEmpty(txtDbName.Text)) Then
            val = True
        End If

        'if emailing errors is enabled then an an email address must be entered and vice versa
        If (String.IsNullOrEmpty(txtMailTo.Text) And ddlEmailErrors.SelectedItem <> "NoEmail") Then
            val = False
        End If

        If (Not String.IsNullOrEmpty(txtMailTo.Text) And ddlEmailErrors.SelectedItem = "NoEmail") Then
            'txtMailTo.Text = String.Empty
        End If

        Return val

    End Function

    Private Sub AuthenticateVisibility(ByVal AuthType As String)
        pnlActiveDirectory.Visible = False
        pnlSSO.Visible = False

        Select Case cbxAuth.SelectedItem
            Case "Active Directory"
                pnlActiveDirectory.Visible = True
            Case "Single Sign On"
                pnlSSO.Visible = True
        End Select

    End Sub

#Region "Button Handlers"
    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Dim cancel As Boolean
        If ChangesMade Then
            If MessageBox.Show("Do you want to save the changes you've made ?", "Save Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, False) = Windows.Forms.DialogResult.Yes Then
            If Not Save() Then
                    cancel = True
                End If
            End If
        End If
        e.Cancel = cancel
    End Sub

    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        If MessageBox.Show("Are you sure you want to delete this item ?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, False) = Windows.Forms.DialogResult.No Then
            Return
        End If
        Dim node As TreeNode = tvSites.SelectedNode
        Dim site As Site = CType(node.Tag, Site)
        node.Remove()
        Me.TopMost = True
    End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        SaveFields(CType(tvSites.SelectedNode.Tag, Site))
        add("new site")
    End Sub

    Private Function CheckDBFields() As Boolean
        If txtDbName.Text = String.Empty Then
            MessageBox.Show("Please enter the name of the database you wish to connect to.")
            Return False
        End If
        If txtDbServer.Text = String.Empty Then
            MessageBox.Show("Please enter the name of the database server you wish to connect to.")
            Return False
        End If
        Return True
    End Function

    Private Sub btnTestConn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestConn.Click
        If Not CheckDBFields() Then Return

        TryOpen(txtDbServer.Text, _
                txtDbName.Text, _
                numMinPool.Value, _
                numMaxPool.Value, _
                numConnectionTimeout.Value)
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Save()
    End Sub

    Private Sub btnCancelVD_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelVD.Click
        pnlVD.Visible = False
    End Sub

    Private Sub btnCancelServer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancelServer.Click
        pnlServer.Visible = False
    End Sub

    Private Sub btnTestADConn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestADConn.Click
        Dim adPath As String = txtActiveDir.Text
        Dim entry As New DirectoryEntry("LDAP://" + adPath)
        If String.IsNullOrEmpty(adPath) Then
            MessageBox.Show("Please enter the name of the Active Directory Server")
            Return
        End If
        Try
            Dim mySearcher As DirectorySearcher = New DirectorySearcher(entry)
            mySearcher.PropertiesToLoad.Add("adspath")
            Dim sr As SearchResult = mySearcher.FindOne()
            'For Each key As String In propcoll1.PropertyNames
            '    For Each val As Object In propcoll1(key)
            '        Response.Write(key + "=" + val.ToString + "<br />")
            '    Next
            'Next

            MessageBox.Show(sr.Path, "Active Directory Connection - OK", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Could not connect to Active Directory Server - " + adPath + Environment.NewLine + ex.Message, _
                            "Active Directory Connection -  Failed", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Information)


            'For Each key As String In propcoll1.PropertyNames
            '    For Each val As Object In propcoll1(key)
            '        Response.Write(key + "=" + val.ToString + "<br />")
            '    Next
            'Next
        End Try
    End Sub

#End Region
#Region "values changed"

    Private Sub txtActiveDir_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtActiveDir.Text <> site.ActiveDirectoryPath Then ChangesMade = True
    End Sub

    Private Sub txtDomains_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtDomains.Text <> site.ActiveDirectoryDomains Then ChangesMade = True
    End Sub

    Private Sub txtDbName_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDbName.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtDbName.Text <> site.DbName Then ChangesMade = True
    End Sub

    Private Sub txtDbServer_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDbServer.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtDbServer.Text <> site.DbServer Then ChangesMade = True
    End Sub

    Private Sub txtName_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtName.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtName.Text.ToLower = "default" Then
            MessageBox.Show("You may only have one default site setting.")
            txtName.Text = ""
            txtName.Focus()
        Else
            tvSites.SelectedNode.Text = txtName.Text
        End If
        If txtName.Text <> site.Name Then ChangesMade = True
    End Sub

    Private Sub cbxThemes_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxThemes.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If cbxThemes.SelectedValue <> site.Theme Then ChangesMade = True
    End Sub

    Private Sub cbxAuth_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxAuth.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If cbxAuth.SelectedValue <> site.AutheticationMethod Then ChangesMade = True
    End Sub

    Private Sub txtBrowserTitle_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtBrowserTitle.LostFocus
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtBrowserTitle.Text <> site.BrowserTitle Then ChangesMade = True
    End Sub

    Private Sub txtErrorMessage_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtErrorMessage.TextChanged
        Dim site As Site = CType(tvSites.SelectedNode.Tag, Site)
        If txtErrorMessage.Text <> site.ErrorMessage Then ChangesMade = True
    End Sub

#End Region
#Region "Test DB connection"

    Private Sub TryOpen(ByVal Server As String, _
                        ByVal Database As String, _
                        ByVal MinPool As Integer, _
                        ByVal MaxPool As Integer, _
                        ByVal Timeout As Integer)
        Dim DB As DBConn
        'Dim msg As String

        DB = New DBConn(Server, Database, MinPool, MaxPool, Timeout)
        If Not DB.TestConnection() Then
            StatusMessage("Connection Failed", False)
        Else
            StatusMessage("Connection Ok", True)
        End If
    End Sub

#End Region
#Region "other click events"
    'search virtual dir
    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click
        pnlVD.Visible = True
    End Sub

    'search server
    Private Sub PictureBox4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox4.Click
        'populate servers
        cbxServers.DataSource = GetServers()
        pnlServer.Visible = True
    End Sub

    Private Sub cbxProxy_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbxProxy.CheckedChanged
        txtProxyURI.Enabled = cbxProxy.Checked
        numProxyPort.Enabled = cbxProxy.Checked
        txtProxyUser.Enabled = cbxProxy.Checked
        txtProxyPassword.Enabled = cbxProxy.Checked
        txtProxyDomain.Enabled = cbxProxy.Checked
    End Sub

    Private Sub btnCert_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCert.Click
        fileCert.FileName = txtProviderCertFile.Text
        If fileCert.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtProviderCertFile.Text = fileCert.FileName
        End If
    End Sub
#End Region
#Region "combo change events"

    Private Sub tvSites_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles tvSites.Click
        Dim node As TreeNode = tvSites.SelectedNode
        SaveFields(node.Tag)
    End Sub

    Private Sub tvSites_AfterSelect(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvSites.AfterSelect
        Dim node As TreeNode = tvSites.SelectedNode
        PopulateFields(node.Tag)
    End Sub

    Private Sub cbxAuth_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbxAuth.SelectedIndexChanged
        AuthenticateVisibility(cbxAuth.SelectedItem)
    End Sub

    Private Sub cbxVdirs_SelectedValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxVdirs.SelectedValueChanged
        If tvSites.SelectedNode IsNot Nothing Then
            txtName.Text = cbxVdirs.SelectedItem
            tvSites.SelectedNode.Text = txtName.Text
            pnlVD.Visible = False
        End If
    End Sub

    Private Sub cbxServers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbxServers.SelectedIndexChanged
        If tvSites.SelectedNode IsNot Nothing Then
            txtDbServer.Text = cbxServers.SelectedItem
            pnlServer.Visible = False
        End If
    End Sub

#End Region
#Region "Email Test Functionality"
    Private Sub btnTestEmail_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestEmail.Click
        Dim DB As DBConn
        Dim msg As String

        Dim Expression As New System.Text.RegularExpressions.Regex("^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")

        If Not Expression.IsMatch(tbSendTo.Text) Then
            msg = "That does not seem to be a valid email address"
            MessageBox.Show(msg, "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If Not CheckDBFields() Then Return

        DB = New DBConn(txtDbServer.Text, _
                        txtDbName.Text, _
                        numMinPool.Value, _
                        numMaxPool.Value, _
                        numConnectionTimeout.Value)
        If Not DB.TestConnection() Then
            msg = "Connenction Failed - " + DB.iErrorMessage.Replace(". ", System.Environment.NewLine)
            MessageBox.Show(msg, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If Not Mailer.IsEmailConfigured Then
            msg = "Please ensure that the SMTP Server address and the From Email Address are entered"
            MessageBox.Show(msg, "eMail is not configured correctly in Setup", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        If Not Mailer.SendMail(tbSendTo.Text, "Test Email", "Test email from iWeb SiteSettings") Then
            msg = "Could not send email, please check settings" + Environment.NewLine + Mailer.iErrorMessage
            MessageBox.Show(msg, "eMail Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Else
            msg = "A test email has been sent to " + tbSendTo.Text
            MessageBox.Show(msg, "eMail Send", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

    End Sub

    Private Sub TabControl1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TabControl1.SelectedIndexChanged
        Dim Vis As Boolean = True
        If txtDbName.Text = String.Empty Then Vis = False
        If txtDbServer.Text = String.Empty Then Vis = False

        gbTestEmail.Visible = Vis
        txtSendTo.Visible = Vis
        tbSendTo.Visible = Vis
        btnTestEmail.Visible = Vis
    End Sub

#End Region

End Class
