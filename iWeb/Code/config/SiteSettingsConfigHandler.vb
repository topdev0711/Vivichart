Imports Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Xml

Namespace IAW

    Public Class SiteSettingsConfigHandler
        Implements IConfigurationSectionHandler


        Public Function Create(ByVal parent As Object, ByVal configContext As Object, ByVal section As System.Xml.XmlNode) As Object Implements System.Configuration.IConfigurationSectionHandler.Create
            Dim configSettings As New Specialized.NameValueCollection
            Dim c As HttpConfigurationContext = CType(configContext, HttpConfigurationContext)
            Dim path As String = c.VirtualPath.Remove(0, 1)
            Dim site As XmlNode
            Dim nodes As XmlNodeList

            site = section.SelectSingleNode("//site[@name='" + path + "']")

            If site Is Nothing Then
                'use default
                site = section.SelectSingleNode("//site[@name='default']")
                If site Is Nothing Then Throw New Exception("The siteSettings section in the web.config must contain a site tag with a name of 'default'")
            End If

            If site.HasChildNodes Then
                nodes = site.ChildNodes
                Dim node As XmlNode
                For Each node In nodes
                    configSettings.Add(node.Name, node.InnerText)
                Next
            End If

            Return configSettings
        End Function


    End Class

End Namespace
