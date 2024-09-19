Imports System.IO

Namespace System.Web.Configuration.Providers

    Public Class SqlViewStateProvider
        Inherits ViewStateProviderBase

        Private _name As String

        Public Overrides ReadOnly Property Name() As String
            Get
                Return _name
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Overrides Function LoadPageState(ByVal pControl As System.Web.UI.Control) As Object
            Dim vsKey As String = String.Empty
            Dim p As Page = pControl
            'Use the GUID stored in the hidden field
            vsKey = p.Request("__vsKeyId")


            Dim ls_sql As String
            ls_sql = "SELECT vsValue FROM web_viewstate WITH (NOLOCK) WHERE vsKey = @p1"

            Try
                Dim vs As Object = IawDB.execScalar(ls_sql, vsKey)

                '	Deserialize the view state string into object
                Dim los As New LosFormatter()
                Return los.Deserialize(vs.ToString())
            Catch ex As Exception
                ctx.trace(ex.Message)
                Throw ex
            End Try
           
        End Function

        Public Overrides Sub SavePageState(ByVal pControl As System.Web.UI.Control, ByVal viewState As Object)
            Dim vsKey As String = String.Empty
            Dim vsValue As String = String.Empty
            Dim writer As New StringWriter()

            Dim p As Page = pControl
            'Use the GUID stored in the hidden field
            vsKey = p.Request("__vsKeyId")

            If String.IsNullOrEmpty(vsKey) Then
                '	Generate new GUID
                vsKey = Guid.NewGuid().ToString()
            End If

            'Store in the hidden field
            'need to register each time !!

            p.ClientScript.RegisterHiddenField("__vsKeyId", vsKey)

            Try
                'Serialize the ViewState into String
                Dim los As New LosFormatter()
                los.Serialize(writer, viewState)
                vsValue = writer.ToString()
            Catch ex As Exception
                ctx.trace(ex.Message)
            Finally
                writer.Close()
            End Try

            '	Store the view state into database
            Dim ls_sql As String
            ls_sql = "IF EXISTS(SELECT vsKey FROM web_viewstate WITH (NOLOCK) WHERE vsKey = @p1) " +
                     "    UPDATE web_viewstate " +
                     "	     SET vsValue = @p2, " +
                     "	         vsTimestamp = getdate() " +
                     "	   WHERE vsKey = @p1 " +
                     "ELSE " +
                     "    INSERT INTO web_viewstate (vsKey, vsValue, vsTimestamp) " +
                     "    VALUES (@p1, @p2, getdate()) "
            IawDB.execNonQuery(ls_sql, vsKey, vsValue)

        End Sub

        Public Overrides Sub Initialize(ByVal name As String, ByVal configValue As NameValueCollection)
            Me._name = name
        End Sub

    End Class

End Namespace