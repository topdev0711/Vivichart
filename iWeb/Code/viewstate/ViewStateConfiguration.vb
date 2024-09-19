Imports System
Imports System.Xml
Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Configuration

Public Class ViewStateConfiguration
   
    Private _defaultProvider As String
    Private _providers As New Dictionary(Of String, Provider)

#Region "properties"

    Public ReadOnly Property DefaultProvider() As String
        Get
            Return _defaultProvider
        End Get
    End Property

    Public ReadOnly Property Providers() As Dictionary(Of String, Provider)
        Get
            Return _providers
        End Get
    End Property

#End Region

    Public Shared Function GetConfig() As ViewStateConfiguration
        Return CType(ConfigurationManager.GetSection("viewStateSetting/viewstate"), ViewStateConfiguration)
    End Function

    Public Sub LoadValuesFromConfigurationXml(ByVal node As XmlNode)
        Dim attributeCollection As XmlAttributeCollection = node.Attributes

        ' Read child nodes
        Dim child As XmlNode
        For Each child In node.ChildNodes
            If child.Name = "providers" Then
                GetProviders(child)
            End If
        Next child
        ' Get the default provider
        _defaultProvider = attributeCollection("defaultProvider").Value
        If Not Providers.ContainsKey(DefaultProvider) Then
            Throw New ConfigurationErrorsException(String.Format("Unable to locate the [{0}] ViewStateProvider!", DefaultProvider), node)
        End If
    End Sub

    Sub GetProviders(ByVal node As XmlNode)
        Dim provider As XmlNode
        For Each provider In node.ChildNodes
            Select Case provider.Name
                Case "add"
                    Providers.Add(provider.Attributes("name").Value, New Provider(provider.Attributes))

                Case "remove"
                    Providers.Remove(provider.Attributes("name").Value)

                Case "clear"
                    Providers.Clear()
            End Select
        Next provider
    End Sub

End Class

Public Class Provider
    
    Private _name As String
    Private _providerType As String
    Private providerAttributes As New NameValueCollection()

#Region "properties"

    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property Type() As String
        Get
            Return _providerType
        End Get
    End Property

    Public ReadOnly Property Attributes() As NameValueCollection
        Get
            Return providerAttributes
        End Get
    End Property

#End Region

    Public Sub New(ByVal attributes As XmlAttributeCollection)
        ' Set the name of the provider
        _name = attributes("name").Value

        ' Set the type of the provider
        _providerType = attributes("type").Value

        ' Store all the attributes in the attributes bucket
        Dim attribute As XmlAttribute
        For Each attribute In attributes
            If attribute.Name <> "name" And attribute.Name <> "type" Then
                providerAttributes.Add(attribute.Name, attribute.Value)
            End If
        Next attribute

    End Sub

End Class
