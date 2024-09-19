Imports Newtonsoft.Json.Linq

Public Class WooAction
    Inherits stub_IngenWebPage

#Region "Common Properties"
    Private SQL As String
    Public defRole As String = "client_admin"
    Public defLanguage As String = "GBR"
#End Region
#Region "Language"
    Public ReadOnly Property LanguagesDT As DataTable
        Get
            Dim DT As DataTable = ViewState("LanguagesDT")
            If DT Is Nothing Then
                SQL = "SELECT language_ref, language_name, culture
                         FROM dbo.qlang
                        ORDER BY language_name"
                DT = IawDB.execGetTable(SQL)
                ViewState("LanguagesDT") = DT
            End If
            Return DT
        End Get
    End Property

    ReadOnly Property LanguagesDR(langRef) As DataRow
        Get
            Return LanguagesDT.Select("language_ref = '" + langRef + "'")(0)
        End Get
    End Property

#End Region
    Public Sub Process(Section As String, Action As String, json As String)

        ' Parse the JSON string into a JObject
        Dim JO As JObject = JObject.Parse(json)

        Select Case Section.ToLower
            Case "customer"
                Customers(Action, JO)

            Case "orders"
                Orders(Action, JO)

        End Select

    End Sub

    Sub Customers(action As String, JO As JObject)
        Dim ClientID As Integer = CInt(JO("id"))
        Dim Email As String = JO("email").ToString
        Dim Forename As String = JO("first_name").ToString
        Dim Surname As String = JO("last_name").ToString
        Dim Fullname As String = Forename + " " + Surname
        Dim Company As String = JO("billing")("company").ToString
        If String.IsNullOrEmpty(Company) Then
            Company = Fullname
        End If

        Dim DR As DataRow = LanguagesDR(defLanguage)
        Dim client_id As Integer = 0

        If action = "created" Then

            Dim guidStr As String = Guid.NewGuid.ToString("N")
            Dim userRef As String = guidStr.Substring(guidStr.Length - 10)

            client_id = IawDB.execScalar("INSERT dbo.clients (client_cust_id, client_name, client_email, 
                                zip_password, date_format, time_format, attributes, max_string_len,
                                max_number, max_image_size, max_data)
                                VALUES (@p1,@p2,@p3,'','dd/MM/yyyy','hh:mm:ss',@p4,500,9999999999.990000,50000,50)
                                SELECT SCOPE_IDENTITY()",
                               ClientID, Company, Email, New ChartAttrib().GetData.ToString)

            SQL = "INSERT dbo.suser (user_ref,client_id, display_name, forename,surname,email_address,user_name,role_ref,language_ref,brand_id,active_user,locale_ref,
                                     prof_status,psc_status,administrator,mfa_pin_date,mfa_pin,mfa_status)
                   VALUES (@p1,@p2,@p3,@p4,@p5,@p6,@p6,@p7,@p8,@p9,@p10,@p11,
                           '01','01','0','1900-01-01','','00')"
            IawDB.execNonQuery(SQL, userRef, client_id, Fullname,
                                    Forename, Surname, Email,
                                    defRole, defLanguage, 0,
                                    "1", DR.GetValue("culture", "en-GB")
                                    )
            SawPassword.EmailNewPassword(Email)
        Else
            IawDB.execNonQuery("UPDATE dbo.clients set client_name = @p2, client_email = @p3
                                WHERE client_cust_id = @p1",
                               ClientID, Company, Email)
            client_id = IawDB.execScalar("select client_id from dbo.clients where client_cust_id = @p1", ClientID)
            IawDB.execNonQuery("UPDATE dbo.suser set display_name = @p2, forename = @p3, surname = @p4, email_address = @p5, user_name = @p5
                                WHERE client_id = @p1",
                               client_id, Fullname, Forename, Surname, Email)
        End If

    End Sub

    Sub Orders(Action As String, JO As JObject)
    End Sub
End Class
