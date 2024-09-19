Imports Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Xml
Imports System.Xml.XPath
Imports System.IO

Public Class SiteSettings

    Private Shared c As HttpContext = HttpContext.Current
    Private Shared cacheKey As String = "siteSettings"

    Public Shared ReadOnly Property Settings() As Specialized.NameValueCollection
        Get
            If c.Cache(cacheKey) Is Nothing Then
                'call init and add to cache
                c.Cache.Add(cacheKey, init(), Nothing, System.Web.Caching.Cache.NoAbsoluteExpiration, New TimeSpan(30, 23, 59, 59), CacheItemPriority.NotRemovable, Nothing)
            Else
                'if its already cached then check the date
                Try
                    'converted to string then back to date, otherwise the file compare doesn't work, when put in the cache some datetime data is lost
                    Dim FileDate As Date = CType(File.GetLastWriteTime(filePath).ToString, Date)
                    Dim cacheFileDate As Date = CType(c.Cache(cacheKey), Specialized.NameValueCollection).Item("LastWriteTime")
                    If cacheFileDate < FileDate Then
                        c.Cache(cacheKey) = init()
                        'remove connection string from cache
                        ctx.cache.Remove("connection_string")
                    End If
                Catch ex As Exception

                End Try
            End If
            Return c.Cache(cacheKey)
        End Get
    End Property

    Private Shared ReadOnly Property filePath() As String
        Get
            Dim filename As String = "siteSettings.config"
            Return c.Server.MapPath(c.Request.ApplicationPath + "/" + filename)
        End Get
    End Property

    Public Shared ReadOnly Property Item(ByVal key As String) As String
        Get
            If Settings(key) IsNot Nothing Then
                Return Settings(key)
            Else
                Return String.Empty
            End If
        End Get
    End Property

    Public Shared Sub Initialise()
        init()
    End Sub

    Private Shared Function init() As Specialized.NameValueCollection
        Dim doc As XmlDocument

        Dim configSettings As Specialized.NameValueCollection = Nothing

        Try
            If File.Exists(filePath) Then
                doc = New XmlDocument()
                doc.Load(filePath)

                configSettings = New Specialized.NameValueCollection
                configSettings.Add("LastWriteTime", File.GetLastWriteTime(filePath))

                Dim path As String
                path = c.Request.ApplicationPath.Remove(0, 1)
                Dim site As XmlNode
                Dim nodes As XmlNodeList

                'site = doc.SelectSingleNode("//site[@name='" + path + "']")
                site = doc.SelectSingleNode("//site[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='" + path.ToLower + "']")

                If site Is Nothing Then
                    'use default
                    site = doc.SelectSingleNode("//site[@name='default']")
                    If site Is Nothing Then Throw New Exception("The siteSettings section in the web.config must contain a site tag with a name of 'default'")
                End If

                If site.HasChildNodes Then
                    nodes = site.ChildNodes
                    Dim node As XmlNode
                    For Each node In nodes
                        configSettings.Add(node.Name, node.InnerText)
                    Next
                End If
            Else
                c.Response.Write("There should be a siteSettings.config file in the root directory of this website, please contact support")
                c.Response.End()
            End If
        Catch ex As IO.FileNotFoundException
            Throw New Exception("There should be a siteSettings.config file in the root directory of this website, please contact support", ex)
        Catch ex As XmlException
            Throw New Exception("The file siteSettings.config file in the root directory of this website is invalid, please contact support", ex)
        End Try

        Return configSettings

    End Function

End Class
