Imports System.Web
Imports System.Web.Services

Public Class DL
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim SQL As String
        Dim DT As DataTable
        Dim Filename As String = ""

        ' Get the parameters

        Dim Parms As Collections.Specialized.NameValueCollection = context.Request.QueryString

        Dim ParmType As String = Parms("parmtype")
        Dim ParmPersonRef As String
        Dim ParmObjectRef As String
        Dim ObjectDesc As String = ""
        Dim ByteArray() As Byte = Nothing

        Using db As New IawDB

            Select Case ParmType.ToLower

                Case "person_object"
                    ParmObjectRef = Parms("object_ref")
                    ParmPersonRef = Parms("person_ref")

                    SQL = "SELECT org_file_name, person_object " + _
                           "  FROM dbo.person_object " + _
                           " WHERE person_ref = @p1 " + _
                           "   AND object_ref = @p2 "
                    DT = db.GetTable(SQL, ParmPersonRef, ParmObjectRef)
                    Filename = DT.Rows(0)("org_file_name")
                    ByteArray = DT.Rows(0)("person_object")
                    If String.IsNullOrEmpty(Filename) Then Filename = "unknown"

                Case "person_publish"
                    ParmObjectRef = Parms("object_ref")
                    ParmPersonRef = Parms("person_ref")
                    Filename = Parms("filename")

                    SQL = "SELECT publish_pdf  " + _
                          "  FROM dbo.person_publish " + _
                          " WHERE person_ref = @p1 " + _
                          "   AND publish_id = @p2 "
                    DT = db.GetTable(SQL, ParmPersonRef, ParmObjectRef)
                    ByteArray = DT.Rows(0)("publish_pdf")

                Case "company_document"
                    ParmObjectRef = Parms("object_ref")

                    SQL = "SELECT doc_file_name, doc_object " + _
                          "  FROM dbo.company_document " + _
                          " WHERE object_ref = @p1 "
                    DT = db.GetTable(SQL, ParmObjectRef)
                    Filename = DT.Rows(0)("doc_file_name")
                    ByteArray = DT.Rows(0)("doc_object")

                Case Else
                    Return
            End Select

            Dim FDL As New fileDownload(HttpContext.Current)
            FDL.DownloadFile(Filename, True, ByteArray)

        End Using

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class