Imports System.Web
Imports System.Web.Services

Public Class Downloader
    Implements System.Web.IHttpHandler

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim SQL As String
        Dim DR As DataRow
        Dim Filename As String = ""

        ' Get the parameters

        'Dim Parms As Collections.Specialized.NameValueCollection = context.Request.QueryString
        Dim Parms As Collections.Hashtable = context.Items

        Dim ParmType As String = Parms("parmtype")
        Dim ParmPersonRef As String
        Dim ParmObjectRef As String
        Dim ParmID As Integer = 0
        Dim ParmKey As String = ""
        Dim ParmUnique As Integer
        Dim ObjectDesc As String = ""
        Dim ByteArray() As Byte = Nothing
        Dim StringObj As String = ""
        Dim BinaryOutput As Boolean = True

        Using DB As New IawDB

            Select Case ParmType.ToLower

                Case "person_object"
                    ParmObjectRef = Parms("object_ref")
                    ParmPersonRef = Parms("person_ref")

                    SQL = "SELECT org_file_name, person_object " + _
                           "  FROM dbo.person_object " + _
                           " WHERE person_ref = @p1 " + _
                           "   AND object_ref = @p2 "
                    DR = DB.GetTable(SQL, ParmPersonRef, ParmObjectRef)(0)
                    Filename = DR.getvalue("org_file_name")
                    ByteArray = DR("person_object")
                    If String.IsNullOrEmpty(Filename) Then Filename = "unknown"

                Case "person_publish"
                    ParmObjectRef = Parms("object_ref")
                    ParmPersonRef = Parms("person_ref")
                    Filename = Parms("filename")

                    SQL = "SELECT publish_pdf  " + _
                          "  FROM dbo.person_publish " + _
                          " WHERE person_ref = @p1 " + _
                          "   AND publish_id = @p2 "
                    DR = DB.GetTable(SQL, ParmPersonRef, ParmObjectRef)(0)
                    ByteArray = DR("publish_pdf")

                Case "company_document"
                    ParmObjectRef = Parms("object_ref")

                    SQL = "SELECT doc_file_name, doc_object " + _
                          "  FROM dbo.company_document " + _
                          " WHERE object_ref = @p1 "
                    DR = DB.GetTable(SQL, ParmObjectRef)(0)
                    Filename = DR.GetValue("doc_file_name")
                    ByteArray = DR("doc_object")

                Case "mail_message"
                    ParmID = CInt(Parms("msg"))
                    ParmUnique = CInt(Parms("unique"))

                    SQL = "SELECT att_filename, att_file " + _
                          "  FROM dbo.msg_attach " + _
                          " WHERE msg_id = @p1 " + _
                          "   AND att_unique = @p2 "
                    DR = DB.GetTable(SQL, ParmID, ParmUnique)(0)
                    Filename = DR.GetValue("att_filename")
                    ByteArray = DR("att_file")

                Case "download_cache"
                    ParmKey = Parms("key")
                    SQL = "SELECT dc_filename,
                                  dc_string,
                                  dc_binary,
                                  case when dc_string is null then 0 else 1 end as string_data,
                                  case when dc_binary is null then 0 else 1 end as binary_data
                             FROM dbo.download_cache
                            WHERE dc_key = @p1 "
                    DR = DB.GetTable(SQL, ParmKey)(0)
                    Filename = DR.GetValue("dc_filename")
                    If DR.GetValue("string_data", "0") = 1 Then
                        StringObj = DR("dc_string")
                        BinaryOutput = False
                    End If
                    If DR.GetValue("binary_data", "0") = "1" Then
                        ByteArray = DR("dc_binary")
                    End If
                Case Else
                    Return
            End Select

            Dim FDL As New fileDownload(HttpContext.Current)
            If BinaryOutput Then
                FDL.DownloadFile(Filename, True, ByteArray)
            Else
                FDL.DownloadFile(Filename, True, StringObj)
            End If

            ' if doing download cache table, then remove the record we just used
            If ParmType.ToLower = "download_cache" Then
                SQL = "DELETE FROM dbo.download_cache
                             WHERE user_ref = @p1 "
                DB.NonQuery(SQL, Parms("user"))
            End If

        End Using

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class