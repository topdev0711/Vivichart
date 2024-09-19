Imports System.Xml
Imports System.IO
Imports System.Collections.Generic

Public Class SiteSettings

    Private _sites As List(Of Site)
    Private _filePath As String

    Public ReadOnly Property sites() As List(Of Site)
        Get
            Return _sites
        End Get
    End Property

    Public Property FilePath() As String
        Get
            Return _filePath
        End Get
        Set(ByVal value As String)
            _filePath = value
        End Set
    End Property

    Public Sub New(ByVal filePath As String)
        _sites = New List(Of Site)
        _filePath = filePath
        init(_filePath)
    End Sub

    Private Sub init(ByVal filepath As String)

        Dim doc As XmlDocument
        Dim FileName As String = filepath
        Dim configSettings As New Specialized.NameValueCollection

        Try
            'test for file first
            If File.Exists(filepath) Then
                Try
                    doc = New XmlDocument()
                    doc.Load(FileName)

                    Dim site As XmlNode
                    Dim sites As XmlNodeList
                    Dim SiteSetting As Site

                    sites = doc.SelectNodes("//site")

                    For Each site In sites
                        SiteSetting = New Site(site)
                        _sites.Add(SiteSetting)
                    Next

                    'ignore exceptions and treat as no file present
                Catch ex As IO.FileNotFoundException
                    'MessageBox.Show("There should be a siteSettings.config file in the root directory of this website, please contact support" + Environment.NewLine + ex.Message)
                Catch ex As XmlException
                    'MessageBox.Show("The file siteSettings.config file in the root directory of this website is invalid, please contact support" + Environment.NewLine + ex.Message)
                End Try
            End If
        Catch ex As IOException

        End Try



    End Sub

    Public Sub WriteXml()
        Dim settings As New XmlWriterSettings()
        settings.Indent = True
        settings.IndentChars = "    "
        Try
            Dim writer As XmlWriter = XmlWriter.Create(_filePath, settings)
            writer.WriteStartElement("siteSettings")
            For Each Site As Site In sites
                Site.WriteXml(writer)
            Next
            writer.WriteEndElement()
            writer.Flush()
            writer.Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

End Class
