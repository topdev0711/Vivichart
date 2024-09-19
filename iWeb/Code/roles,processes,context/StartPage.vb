
Imports System.Reflection
Imports IAW

Public Class StartPage

    Private Shared empChanged As Boolean

    Public Shared Function GetUrl() As String
        Return getStartPage()
    End Function

    Public Shared Function GetUrl(ByVal clearReturnUrl As Boolean) As String
        Return getStartPage(clearReturnUrl)
    End Function

    Public Shared Sub Redirect(Optional ByVal nextUrl As String = "")
        If Not String.IsNullOrEmpty(nextUrl) Then
            ctx.redirect(nextUrl)
        Else
            ctx.current.Response.Redirect(getStartPage())
        End If
    End Sub

    Public Shared Sub EmploymentChanged()
        empChanged = True
        StartPage.RetrieveRoles()
        Dim url As String = getStartPage()
        empChanged = False
        ctx.current.Response.Redirect(url)
    End Sub

    Private Shared Function getStartPage(Optional ByVal clearReturnUrl As Boolean = True) As String
        Dim ls_sql As String
        Dim ls_url As String = ""
        Dim ls_mnu As String = ""

        If ctx.user Is Nothing Then
            ls_sql = "SELECT R.start_page
                        FROM dbo.suser U
                             JOIN dbo.role R
                               On R.role_ref = U.role_ref
                       WHERE U.user_ref = @p1"
            ls_mnu = IawDB.execScalar(ls_sql, ctx.session("user_ref"))
        Else
            ls_mnu = ctx.user.startPage
        End If

        If ls_mnu Is System.DBNull.Value OrElse ls_mnu Is Nothing OrElse ls_mnu = String.Empty Then
            ls_url = getDefaultPage()
        Else
            'get the relevant menu item and build the url
            Dim page As Page = CType(HttpContext.Current.CurrentHandler, Page)
            Dim smNode As SiteMapNode
            Dim cntrl As Control = page.Master
            Dim prop As PropertyInfo

            prop = cntrl.GetType.GetProperty("siteMapNode")
            Dim getSiteMapNode As MethodInfo = prop.GetGetMethod
            smNode = getSiteMapNode.Invoke(cntrl, New Object() {ls_mnu})
            cntrl = Nothing

            If smNode IsNot Nothing Then
                ls_url = smNode.Url
            Else
                ls_url = getDefaultPage()
            End If
        End If

        If Not ls_url.StartsWith(HttpContext.Current.Request.ApplicationPath) Then
            ls_url = HttpContext.Current.Request.ApplicationPath + "/" + ls_url
        End If

        Return SawUtil.AddUserToURL(ls_url)

    End Function

    Private Shared Function getDefaultPage() As String
        Return SawUtil.encryptQuery("Chart/ModelList.aspx?" + constants.processref + "=p_chart_list", True)
    End Function

    Public Shared Sub RetrieveRoles()
        Dim currentUser As IawPrincipal
        Dim UserRef As String = ctx.session("user_ref")
        Dim cacheKey As String = "cache_" + UserRef
        Dim http As HttpContext = HttpContext.Current
        Dim DT As DataTable
        Dim role_ref As String = ""
        Dim role_name As String
        Dim start_page As String
        Dim client_id As Integer
        Dim new_role As role
        Dim pr As New Dictionary(Of String, IAWProcess)
        Dim roles As New System.Collections.Generic.Dictionary(Of String, role)

        If http.Session(cacheKey) Is Nothing Then
            'get the roles & rights for the current user
            Dim sql As String
            sql = "SELECT RP.role_ref,
                          RP.process_ref,
                          RP.view_attribute,
                          RP.update_attribute,
                          RP.insert_attribute,
                          RP.delete_attribute,
                          R.role_ref,
                          dbo.dbf_puptext('role','role_name',R.role_name,@p2) as role_name,
                          R.start_page,
                          U.client_id
                     FROM dbo.suser U WITH (NOLOCK) 
                          JOIN dbo.role R WITH (NOLOCK) 
                            ON R.role_ref = U.role_ref 
                          JOIN dbo.role_process RP WITH (NOLOCK) 
                            ON RP.role_ref = U.role_ref 
                    WHERE U.user_ref = @p1"
            DT = IawDB.execGetTable(sql, UserRef, ctx.languageCode)
            ' will only ever be one role for vivichart
            For Each DR As DataRow In DT.Rows
                role_ref = DR("role_ref")
                role_name = DR("role_name")
                start_page = DR("start_page")
                client_id = DR("client_id")
                If Not roles.ContainsKey(role_ref) Then
                    'populate process right first then add to role
                    pr = getRoleProcessRights(DT, role_ref)
                    new_role = New role(role_ref, role_name, start_page, pr)
                    roles.Add(new_role.ref, new_role)
                End If
            Next

            Dim err As String = SawUtil.GetErrMsg("WB_B0014")
            If roles.Count = 0 Then
                ctx.logout(err)
            End If

            currentUser = New IawPrincipal(http.User.Identity, role_ref, roles, client_id)
            http.Session(cacheKey) = currentUser
        Else
            'used cached version if cached
            currentUser = http.Session(cacheKey)
        End If

        ''override current role
        'If http.Session("role") IsNot Nothing Then
        '    currentUser.currentRole = http.Session("role")
        '    ctx.trace("role-mod", http.Session("role"))
        'End If

        'put the current role on the ctx
        ctx.item("role") = currentUser.currentRole
        HttpContext.Current.User = currentUser
    End Sub

    Private Shared Function getRoleProcessRights(ByVal DT As DataTable, ByVal role_ref As String) As Dictionary(Of String, IAWProcess)
        Dim processList As New Dictionary(Of String, IAWProcess)
        Dim process As IAWProcess
        Dim dr As DataRowView
        Dim i As Integer

        Dim DV As DataView = New DataView(DT)
        DV.RowFilter = " role_ref = '" + role_ref + "' "
        DV.RowStateFilter = DataViewRowState.CurrentRows

        For i = 0 To DV.Count - 1
            dr = DV(i)
            Dim rightsList As New List(Of EditRights)
            rightsList.Add(New EditRights(dr("view_attribute"), RightTo.view))
            rightsList.Add(New EditRights(dr("update_attribute"), RightTo.update))
            rightsList.Add(New EditRights(dr("insert_attribute"), RightTo.insert))
            rightsList.Add(New EditRights(dr("delete_attribute"), RightTo.delete))
            Dim req_emp_context As Boolean = False
            process = New IAWProcess(dr("process_ref"), rightsList, role_ref)
            If Not processList.ContainsKey(dr("process_ref")) Then
                processList.Add(dr("process_ref"), process)
            End If
        Next i
        Return processList
    End Function




End Class
