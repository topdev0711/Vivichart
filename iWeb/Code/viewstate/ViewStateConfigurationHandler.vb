Imports System
Imports System.Xml
Imports System.Configuration

Namespace System.Web.UI

    Public Class ViewStateConfigurationHandler
        Implements IConfigurationSectionHandler

        Public Function Create(ByVal parent As Object, ByVal configContext As Object, ByVal section As System.Xml.XmlNode) As Object Implements System.Configuration.IConfigurationSectionHandler.Create
            Dim config As New ViewStateConfiguration()
            config.LoadValuesFromConfigurationXml(section)
            Return config
        End Function

    End Class

End Namespace