Imports System.Globalization

<ButtonBarRequired(False)>
Public Class ImagePage
    Inherits stub_IngenWebPage
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            'ListLocales()
            ShowLocale()
        End If
    End Sub

    Private Sub ShowLocale()
        Dim sb As New StringBuilder()
        Dim cultureInfo As CultureInfo = CultureInfo.CurrentCulture
        Dim regionInfo As New RegionInfo(cultureInfo.Name)
        Dim uiCultureInfo As CultureInfo = CultureInfo.CurrentUICulture
        Dim numberFormat As NumberFormatInfo = cultureInfo.NumberFormat

        sb.AppendLine("<h2>Culture Information</h2>")
        sb.AppendLine($"Culture Name: {cultureInfo.Name}<br />")
        sb.AppendLine($"Display Name: {cultureInfo.DisplayName}<br />")
        sb.AppendLine($"English Name: {cultureInfo.EnglishName}<br />")
        sb.AppendLine($"Native Name: {cultureInfo.NativeName}<br />")
        sb.AppendLine($"ISO Language Name: {cultureInfo.TwoLetterISOLanguageName}<br />")
        sb.AppendLine($"ISO Language Code: {cultureInfo.ThreeLetterISOLanguageName}<br />")
        sb.AppendLine($"Calendar: {cultureInfo.Calendar.ToString()}<br />")
        sb.AppendLine($"DateTime Format: {cultureInfo.DateTimeFormat.ShortDatePattern} {cultureInfo.DateTimeFormat.ShortTimePattern}<br />")

        sb.AppendLine("<h2>Currency Format Details</h2>")
        sb.AppendLine($"Currency Symbol: {numberFormat.CurrencySymbol}<br />")
        sb.AppendLine($"Currency Name: {RegionInfo.CurrencyEnglishName}<br />")
        sb.AppendLine($"Currency Decimal Digits: {numberFormat.CurrencyDecimalDigits}<br />")
        sb.AppendLine($"Currency Group Separator: {numberFormat.CurrencyGroupSeparator}<br />")
        sb.AppendLine($"Currency Decimal Separator: {numberFormat.CurrencyDecimalSeparator}<br />")

        ' Determine positive currency format pattern
        Dim positivePattern As String = GetCurrencyPositivePattern(numberFormat)
        sb.AppendLine($"Currency Positive Format: {positivePattern}<br />")

        ' Determine negative currency format pattern
        Dim negativePattern As String = GetCurrencyNegativePattern(numberFormat)
        sb.AppendLine($"Currency Negative Format: {negativePattern}<br />")

        ltLocaleInfo.Text = sb.ToString()
    End Sub

    Private Function GetCurrencyPositivePattern(ByVal numberFormat As NumberFormatInfo) As String
        Dim Fmt As String
        Select Case numberFormat.CurrencyPositivePattern
            Case 0 : Fmt = "{0}n"
            Case 1 : Fmt = "n{0}"
            Case 2 : Fmt = "{0} n"
            Case 3 : Fmt = "n {0}"
            Case Else : Return "Unknown pattern"
        End Select
        Return String.Format(Fmt, numberFormat.CurrencySymbol)
    End Function

    Private Function GetCurrencyNegativePattern(ByVal numberFormat As NumberFormatInfo) As String
        Dim Fmt As String
        Select Case numberFormat.CurrencyNegativePattern
            Case 0 : Fmt = "({0}n)"
            Case 1 : Fmt = "-{0}n"
            Case 2 : Fmt = "{0}-n"
            Case 3 : Fmt = "{0}n-"
            Case 4 : Fmt = "(n{0})"
            Case 5 : Fmt = "-n{0}"
            Case 6 : Fmt = "n-{0}"
            Case 7 : Fmt = "n{0}-"
            Case 8 : Fmt = "-n {0}"
            Case 9 : Fmt = "-{0} n"
            Case 10 : Fmt = "n {0}-"
            Case 11 : Fmt = "{0} n-"
            Case 12 : Fmt = "{0} -n"
            Case 13 : Fmt = "n- {0}"
            Case 14 : Fmt = "({0} n)"
            Case 15 : Fmt = "(n {0})"
            Case Else : Return "Unknown pattern"
        End Select
        Return String.Format(Fmt, numberFormat.CurrencySymbol)
    End Function

    Private Sub ListLocales()
        Dim dt As New DataTable()
        dt.Columns.Add("Locale")
        dt.Columns.Add("LocaleName")
        dt.Columns.Add("DateFormat")
        dt.Columns.Add("CurrencyIdentifier")
        dt.Columns.Add("CurrencyNameEnglish")
        dt.Columns.Add("CurrencyNameLocal")
        dt.Columns.Add("CurrencySymbol")
        dt.Columns.Add("StandardCurrencyFormat")
        dt.Columns.Add("NegativeCurrencyFormat")
        dt.Columns.Add("NumberSeparator")
        dt.Columns.Add("DecimalSeparator")
        dt.Columns.Add("NegativeFormat")

        For Each ci As CultureInfo In CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            Dim row As DataRow = dt.NewRow()
            Dim region As New RegionInfo(ci.Name)
            row("Locale") = ci.Name
            row("LocaleName") = ci.DisplayName
            row("DateFormat") = ci.DateTimeFormat.ShortDatePattern
            row("CurrencyIdentifier") = region.ISOCurrencySymbol
            row("CurrencyNameEnglish") = region.CurrencyEnglishName
            row("CurrencyNameLocal") = region.CurrencyNativeName
            row("CurrencySymbol") = region.CurrencySymbol
            row("StandardCurrencyFormat") = GetCurrencyPositivePattern(ci.NumberFormat.CurrencyPositivePattern)
            row("NegativeCurrencyFormat") = GetCurrencyNegativePattern(ci.NumberFormat.CurrencyNegativePattern)
            row("NumberSeparator") = ci.NumberFormat.NumberGroupSeparator
            row("DecimalSeparator") = ci.NumberFormat.NumberDecimalSeparator
            row("NegativeFormat") = ci.NumberFormat.NegativeSign & " " & ci.NumberFormat.NumberNegativePattern
            dt.Rows.Add(row)
        Next

        'GridView1.DataSource = dt
        'GridView1.DataBind()

    End Sub

    Private Function GetCurrencyPositivePattern(pattern As Integer) As String
        Select Case pattern
            Case 0
                Return "n$"
            Case 1
                Return "$n"
            Case 2
                Return "n $"
            Case 3
                Return "$ n"
            Case Else
                Return "Unknown pattern"
        End Select
    End Function

    Private Function GetCurrencyNegativePattern(pattern As Integer) As String
        Select Case pattern
            Case 0
                Return "(n$)"
            Case 1
                Return "-n$"
            Case 2
                Return "-$n"
            Case 3
                Return "$-n"
            Case 4
                Return "n-$"
            Case 5
                Return "n$-"
            Case 6
                Return "n $-"
            Case 7
                Return "(n $)"
            Case 8
                Return "(n$)"
            Case 9
                Return "(n $)"
            Case 10
                Return "$ n-"
            Case 11
                Return "n- $"
            Case 12
                Return "(n $)"
            Case 13
                Return "(n$)"
            Case 14
                Return "n-$"
            Case 15
                Return "(n $)"
            Case Else
                Return "Unknown pattern"
        End Select
    End Function

End Class

