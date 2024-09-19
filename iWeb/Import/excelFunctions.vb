Imports System.IO
Imports NPOI.SS.UserModel

Public Class excelFunctions
    Public hasErrors As Boolean = False
    Public ErrorText As String

    Public Function ExcelGetWorksheetInfo(ByVal ContentStream As MemoryStream) As List(Of String)
        Dim workbook As IWorkbook
        Dim worksheet As ISheet
        Dim sheetNames As New List(Of String)
        Dim StreamCopy As New MemoryStream

        ' move position in stream to the start and then use a copy of the
        ' memory stream as this function will stupidly close it
        ContentStream.Position = 0
        ContentStream.CopyTo(StreamCopy)

        StreamCopy.Position = 0
        ' a spreasheet can have multiple tabs, Return an array of the tab (worksheet) names 
        Try
            hasErrors = False
            workbook = WorkbookFactory.Create(StreamCopy)
            StreamCopy.Close()

            For worksheetIndex As Integer = 0 To workbook.NumberOfSheets - 1
                worksheet = workbook.GetSheetAt(worksheetIndex)
                sheetNames.Add(worksheet.SheetName)
            Next

            workbook.Close()

        Catch ex As Exception
            hasErrors = True
            ErrorText = ctx.Translate("::LT_S0379")    '"Error Getting Sheet Names from Spreadsheet"
        End Try

        Return sheetNames

    End Function

End Class
