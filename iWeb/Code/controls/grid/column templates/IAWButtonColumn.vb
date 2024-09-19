Imports System
Imports System.Collections.Specialized
Imports System.Collections
Imports System.ComponentModel
Imports System.Security.Permissions
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports System.Collections.Generic
Imports IAW.controls


Namespace IAW.boundcontrols

    Public NotInheritable Class IAWButtonColumn
        Inherits IAWBoundColumn

#Region "properties"

        Public Property returnHere() As Boolean
            Get
                If ViewState("returnHere") Is Nothing Then Return False
                Return ViewState("returnHere")
            End Get
            Set(ByVal value As Boolean)
                ViewState("returnHere") = value
            End Set
        End Property

        Public Property exportColumns() As List(Of KeyValuePair(Of String, IAWWebListColumnType))
            Get
                If ViewState("exportColumns") Is Nothing Then Return Nothing
                Return ViewState("exportColumns")
            End Get
            Set(ByVal value As List(Of KeyValuePair(Of String, IAWWebListColumnType)))
                ViewState("exportColumns") = value
            End Set
        End Property

        Public Property destination() As String
            Get
                If ViewState("destination") Is Nothing Then Return String.empty
                Return ViewState("destination")
            End Get
            Set(ByVal value As String)
                ViewState("destination") = value
            End Set
        End Property

        Public Property buttonText() As String
            Get
                If ViewState("buttonText") Is Nothing Then Return String.empty
                Return ViewState("buttonText")
            End Get
            Set(ByVal value As String)
                ViewState("buttonText") = value
            End Set
        End Property

        Public Property buttonType() As urlType
            Get
                If ViewState("buttonType") Is Nothing Then Return String.empty
                Return ViewState("buttonType")
            End Get
            Set(ByVal value As urlType)
                ViewState("buttonType") = value
            End Set
        End Property

        Public Property processRef() As String
            Get
                If ViewState("processRef") Is Nothing Then Return String.empty
                Return ViewState("processRef")
            End Get
            Set(ByVal value As String)
                ViewState("processRef") = value
            End Set
        End Property

        Public Property taxBasis() As String
            Get
                If ViewState("taxBasis") Is Nothing Then Return String.empty
                Return ViewState("taxBasis")
            End Get
            Set(ByVal value As String)
                ViewState("taxBasis") = value
            End Set
        End Property

#End Region

        Public Sub New()
            MyBase.new()
        End Sub



        ' Gets a default value for a basic design-time experience.        
        Protected Overrides Function GetDesignTimeValue() As Object
            Return "<input type=""button"" value=""Button Column""></input>"
        End Function

        ' This method is called by the ExtractRowValues methods of
        ' GridView and DetailsView. Retrieve the current value of the 
        ' cell from the Checked state of the control.
        'Public Overrides Sub ExtractValuesFromCell( _
        '    ByVal dictionary As IOrderedDictionary, _
        '    ByVal cell As DataControlFieldCell, _
        '    ByVal rowState As DataControlRowState, _
        '    ByVal includeReadOnly As Boolean)
        '    ' Determine whether the cell contain a RadioButton 
        '    ' in its Controls collection.
        '    If cell.Controls.Count > 0 Then
        '        Dim btn As IAWHyperLink = CType(cell.Controls(0), IAWHyperLink)
        '        If btn Is Nothing Then
        '            ' A HtmlButton is expected, but a null is encountered.
        '            ' Add error handling.
        '            Throw New InvalidOperationException( _
        '                "IAWHyperLink could not extract control.")
        '        End If

        '    End If
        'End Sub

        ' This method adds a RadioButton control and any other 
        ' content to the cell's Controls collection.
        Protected Overrides Sub InitializeDataCell( _
            ByVal cell As DataControlFieldCell, _
            ByVal rowState As DataControlRowState)
            MyBase.InitializeDataCell(cell, rowState)

            Dim btn As New IAWHyperLink
            If rowState = DataControlRowState.Normal Then
                btn.SkinID = "listRow"
            ElseIf rowState = DataControlRowState.Alternate Then
                btn.SkinID = "listAltRow"
            End If

            AddHandler btn.DataBinding, AddressOf Me.OnDataBindField
            cell.Controls.Add(btn)
        End Sub


        Protected Shadows Sub OnDataBindField(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim btn As IAWHyperLink
            btn = DirectCast(sender, IAWHyperLink)
            Dim DRV As DataRowView = DirectCast(sender.Parent.Parent.DataItem, DataRowView)
            Dim row As GridViewRow = DirectCast(sender.Parent.Parent, GridViewRow)
            Dim url As String = String.Empty
            Dim txt As String

            Select Case buttonType
                Case urlType.form
                    'Throw New Exception("button type form not plumbed in yet")
                    url = generateUrl("DataPage.aspx?formid=" + destination + "&" + constants.processref + "=" + processRef, DRV)
                    'ctx.trace("button type form not plumbed in yet ", buttonText)
                Case urlType.list
                    'need listref,taxbasis,processref
                    url = generateUrl("IngenuityWebList.aspx?listref=" + destination + "&" + constants.processref + "=" + processRef, DRV)
                Case urlType.url
                    ' if SQL then derive it first
                    If destination.StartsWith("select ") Then
                        destination = SawDB.ExecScalar(SawUtil.Substitute(destination))
                    End If
                    'test for params first
                    If destination.Contains("?") And Not destination.EndsWith("?") Then
                        Dim qs As String = destination.Substring(destination.IndexOf("?") + 1)
                        url = destination.Remove(destination.IndexOf("?") + 1)
                        url = generateUrl(url + qs + "&" + constants.processref + "=" + processRef, DRV)
                    Else
                        url = generateUrl(destination + "?" + constants.processref + "=" + processRef, DRV)
                    End If
            End Select

            If returnHere Or ctx.item("returnhere") IsNot Nothing Then
                url += "&returnhere=y"
            End If

            txt = DeriveButtonText(buttonText, DRV)

            If txt.Contains("|") Then
                btn.CssClass += " IconBtn " + txt.Split("|")(0)
                btn.Text = txt.Split("|")(1).Replace(" ", "&#160;")
            Else
                btn.Text = txt.Replace(" ", "&#160;")
            End If

            btn.ToolTip = btn.Text

            btn.Url = btn.ResolveUrl(url)
            'If url = String.Empty Then
            '    btn.Attributes.Add("onclick", "alert('Form buttons are not yet active\n" + buttonText + " button');return false")
            'End If

            Try
                If DRV.Row.Table.Columns.Contains("list_link") Then
                    If DRV("list_link").ToString = "no" Then
                        btn.Visible = False
                    ElseIf DRV("list_link") = 0 Then
                        btn.Visible = False
                    Else
                        btn.Visible = True
                    End If
                End If
            Catch ex As Exception
                btn.Visible = True
            End Try

            If btn.Text.Trim = String.Empty Then btn.Visible = False

            'ctx.trace("exports cols", generateUrl(destination, dataRow))

        End Sub


        Private Function generateUrl(ByVal URL As String, ByRef aDataRow As DataRowView) As String
            Dim urlStr As String
            Dim sep As String
            If Not URL.Contains("?") Then
                sep = "?"
            ElseIf URL.EndsWith("?") Then
                sep = String.Empty
            Else
                sep = "&"
            End If
            urlStr = URL

            Dim intExports As Integer = 0

            For Each s As KeyValuePair(Of String, IAWWebListColumnType) In exportColumns
                Dim colType As IAWWebListColumnType = s.Value
                Dim field As String = s.Key
                If intExports > 0 Then sep = "&"
                intExports += 1

                urlStr += sep + "ARG"
                urlStr += intExports.ToString()
                urlStr += "="

                Select Case colType
                    Case IAWWebListColumnType.date, IAWWebListColumnType.time
                        If Not IsDBNull(aDataRow(field)) Then
                            urlStr += CDate(aDataRow(field)).ToString(HttpContext.Current.Session("DBDateFormat"))
                        End If
                    Case Else
                        'urlStr += aDataRow(field).ToString
                        urlStr += ctx.server.UrlEncode(aDataRow(field).ToString)
                End Select

            Next
            urlStr += "&argcount="
            urlStr += intExports.ToString()
            ctx.trace("export- gen url", urlStr)
            If Not urlStr.Contains("~") Then
                If Not urlStr.Trim().StartsWith("/") Then
                    urlStr = "/" + urlStr
                End If
                urlStr = "~" + urlStr
            End If
            Return urlStr

        End Function

        Private Function DeriveButtonText(ByVal aValue As String, ByVal aDataRow As DataRowView) As String
            'will either be text/sql/^ derived
            Dim val As String = aValue.TrimStart(New Char() {" "}).ToLower
            If (val.StartsWith("select ") Or val.StartsWith("if ")) And val.Length > 19 Then
                Return SawDB.ExecScalar(SawUtil.SubstituteValue(aValue, aDataRow))
            End If

            Return aValue
        End Function

    End Class
End Namespace