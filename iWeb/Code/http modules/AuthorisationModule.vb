Imports System.Attribute

Namespace IAW.httpmodules

    Public Class AuthorisationModule
        Implements IHttpModule

        Private ReadOnly Property IsMobile As Boolean
            Get
                If HttpContext.Current.Request Is Nothing Then Return False
                Return HttpContext.Current.Request.FilePath.Contains("/m/")
            End Get
        End Property

        Public Sub Init(ByVal context As System.Web.HttpApplication) Implements System.Web.IHttpModule.Init
            AddHandler context.PreRequestHandlerExecute, AddressOf onPreRequestHandlerExecute
        End Sub

        Private Sub onPreRequestHandlerExecute(ByVal sender As Object, ByVal e As EventArgs)
            If TypeOf HttpContext.Current.CurrentHandler Is System.Web.UI.Page Then
                Dim p As Page = DirectCast(HttpContext.Current.Handler, Page)
                AddHandler p.PreLoad, AddressOf page_PreLoad
            End If
        End Sub

        Private Sub page_PreLoad(ByVal sender As Object, ByVal e As EventArgs)
            'If IsMobile Then Return

            Dim processRequired As Boolean = True
            Dim Attr As Attribute
            Dim processAttr As ProcessRequiredAttribute
            Dim processRef As String = ctx.item(constants.processref)

            Attr = GetCustomAttribute(sender.GetType, GetType(ProcessRequiredAttribute))
            processAttr = CType(Attr, ProcessRequiredAttribute)

            If processAttr IsNot Nothing Then processRequired = processAttr.processRequired

            'processref is required on the request for this page
            'if not supplied then logout and redirect to login page

            If processRequired And processRef IsNot Nothing Then
                If ctx.user Is Nothing Then ctx.accessDenied(ctx.rawUrl)
                'process is required and process is on the request
                'check user has rights to this process
                If Not ctx.user.hasRightInProcess(processRef) Then
                    ctx.accessDenied(ctx.rawUrl)
                End If
            ElseIf processRequired And processRef Is Nothing Then
                ctx.accessDenied(ctx.rawUrl)
            End If

        End Sub

        Public Sub Dispose() Implements System.Web.IHttpModule.Dispose
        End Sub

    End Class


End Namespace