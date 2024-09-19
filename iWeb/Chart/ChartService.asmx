'------------------------------------------
' Filename: ChartService.asmx
'
' Description:
' The central Web Service for all Chart Methods
'
' Date       Author         Notes
' 27/05/2022 Adrian Parker  Initial Version
'
' Version: 1.0
'------------------------------------------

<%@ WebService Language="VB" Class="ChartService" %>

Imports System.Web.Services
Imports System.Web.Script.Services
Imports System.Globalization
Imports System.Threading

<WebService(Namespace:="http://iawresources.co.uk")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ScriptService()>
Public Class ChartService
    Inherits WebService

    '------------------------------------------
    ' Model Functions
    '------------------------------------------

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetModelParms() As Object
        Dim o As Object
        Try
            Starting("GetModelParms Start")
            o = (New ChartFunctions).GetModelParms()
            Ending("GetModelParms End")
        Catch ex As Exception
            ErrorIng("GetModelParms Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveModelParms(ModelParms As String) As Object
        Dim o As Object
        Try
            Starting("SaveModelParms Start")
            o = (New ChartFunctions).SaveModelParms(ModelParms)
            Ending("SaveModelParms End")
        Catch ex As Exception
            ErrorIng("SaveModelParms Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetUnallocatedItems() As Object
        Dim o As Object
        Try
            Starting("GetUnallocatedItems Start")
            o = (New ChartFunctions).GetUnallocatedItems()
            Ending("GetUnallocatedItems End")
        Catch ex As Exception
            ErrorIng("GetUnallocatedItems Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetUnallocatedDetail(ItemRef As String) As Object
        Dim o As Object
        Try
            Starting("GetUnallocatedDetail Start")
            o = (New ChartFunctions).GetUnallocatedItems(ItemRef)
            Ending("GetUnallocatedDetail End")
        Catch ex As Exception
            ErrorIng("GetUnallocatedDetail Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetModelData() As Object
        Dim o As Object
        Try
            Starting("GetModelData Start")
            o = (New ChartFunctions).GetModelData()
            Ending("GetModelData End")
        Catch ex As Exception
            ErrorIng("GetModelData Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetModelDataItem(ByVal ItemRef As String, nodeHeight As Integer) As Object
        Dim o As Object
        Try
            Starting("GetModelDataItem Start")
            o = (New ChartFunctions).GetModelDataItem(ItemRef, nodeHeight)
            Ending("GetModelDataItem End")
        Catch ex As Exception
            ErrorIng("GetModelDataItem Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetModelDataItemForType(ByVal RequiredNodeType As String, ByVal ItemRef As String, nodeHeight As Integer) As Object
        Dim o As Object
        Try
            Starting("GetModelDataItemForType Start")
            o = (New ChartFunctions).GetModelDataItemForType(RequiredNodeType, ItemRef, nodeHeight)
            Ending("GetModelDataItemForType End")
        Catch ex As Exception
            ErrorIng("GetModelDataItemForType Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SaveModelData(ModelData As String) As Object
        Dim o As Object
        Try
            Starting("SaveModelData Start")
            o = (New ChartFunctions).SaveModel(ModelData)
            Ending("SaveModelData End")
        Catch ex As Exception
            ErrorIng("SaveModelData Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GetNodePicture(ByVal ItemRef As String, nodeHeight As Integer) As Object
        Dim o As Object
        Try
            Starting("GetNodePicture Start")
            o = (New ChartFunctions).GetNodePicture(ItemRef, nodeHeight)
            Ending("GetNodePicture End")
        Catch ex As Exception
            ErrorIng("GetNodePicture Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod()>
    Public Function GetDetailInfo(ItemRef As String) As Object
        Dim o As Object
        Try
            Starting("GetDetailInfo Start")
            o = (New ChartFunctions).GetDetailInfo(ItemRef)
            Ending("GetDetailInfo End")
        Catch ex As Exception
            ErrorIng("GetDetailInfo Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return o
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod()>
    Public Function KeepAlive() As String
        Try
            Starting("KeepAlive")
        Catch ex As Exception
            ErrorIng("KeepAlive Error", ex)
            Return GenericFunctions.ConvertToJSON(ex)
        End Try
        Return "OK"
    End Function

    Private Sub Starting(section As String)
        SawDiag.LogSvcVerbose("Service : " + section)
        SetCulture()
    End Sub

    Private Sub Ending(section As String)
        SawDiag.LogSvcVerbose("Service : " + section)
    End Sub

    Private Sub ErrorIng(section As String, ex As Exception)
        SawDiag.LogSvcError("Service : " + section)
        SawDiag.LogSvcError(ex.ToString)
    End Sub

    Private Sub SetCulture()
        Dim cultureID As String = TryCast(ctx.session("locale_ref"), String)
        If Not String.IsNullOrEmpty(cultureID) Then
            ' Create the CultureInfo object
            Dim cultureInfo As CultureInfo = cultureInfo.CreateSpecificCulture(cultureID)

            ' Set the culture and UI culture for the current thread
            Thread.CurrentThread.CurrentCulture = cultureInfo
            Thread.CurrentThread.CurrentUICulture = cultureInfo
        End If
    End Sub

End Class
