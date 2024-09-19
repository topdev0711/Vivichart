Imports Microsoft.VisualBasic
Imports System.Attribute

Namespace IAW.httpmodules

    Public Class PageModule
        Implements IHttpModule

        Public Event PreInit As PageEvent
        Public Event InitComplete As PageEvent
        Public Event Load As PageEvent
        Public Event LoadComplete As PageEvent


        Public Sub Init(ByVal context As System.Web.HttpApplication) Implements System.Web.IHttpModule.Init
            AddHandler context.PreRequestHandlerExecute, AddressOf onPreRequestHandlerExecute
        End Sub

        Private Sub onPreRequestHandlerExecute(ByVal sender As Object, ByVal e As EventArgs)
            If TypeOf HttpContext.Current.CurrentHandler Is System.Web.UI.Page Then
                Dim p As Page = DirectCast(HttpContext.Current.Handler, Page)
                AddHandler p.PreInit, AddressOf page_PreInit
                AddHandler p.InitComplete, AddressOf page_InitComplete
                AddHandler p.Load, AddressOf page_Load
                AddHandler p.LoadComplete, AddressOf page_LoadComplete
            End If
        End Sub

        Private Sub page_PreInit(ByVal sender As Object, ByVal e As EventArgs)
            Dim p As Page = DirectCast(sender, Page)
            RaiseEvent PreInit(Me, New PageEventArgs(p))
        End Sub

        Private Sub page_InitComplete(ByVal sender As Object, ByVal e As EventArgs)
            Dim p As Page = DirectCast(sender, Page)
            RaiseEvent InitComplete(Me, New PageEventArgs(p))
        End Sub

        Private Sub page_Load(ByVal sender As Object, ByVal e As EventArgs)
            Dim p As Page = DirectCast(sender, Page)
            RaiseEvent Load(Me, New PageEventArgs(p))

            HttpContext.Current.Response.Cache.SetNoStore()
        End Sub

        Private Sub page_LoadComplete(ByVal sender As Object, ByVal e As EventArgs)
            Dim p As Page = DirectCast(sender, Page)
            RaiseEvent LoadComplete(Me, New PageEventArgs(p))
        End Sub

        Public Sub Dispose() Implements System.Web.IHttpModule.Dispose
        End Sub
    End Class

End Namespace
