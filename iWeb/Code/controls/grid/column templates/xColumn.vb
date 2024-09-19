
Imports System
Imports System.Collections.Specialized
Imports System.Collections
Imports System.ComponentModel
Imports System.Security.Permissions
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data

Namespace IAW.boundcontrols

    Public Class xColumn
        Inherits BoundField


        Public Overrides Sub ExtractValuesFromCell(ByVal dictionary As IOrderedDictionary, ByVal cell As DataControlFieldCell, ByVal rowState As DataControlRowState, ByVal includeReadOnly As Boolean)
            Dim control As Control = Nothing
            Dim dataField As String = Me.DataField
            Dim text As Object = Nothing
            Dim nullDisplayText As String = Me.NullDisplayText
            If (((rowState And DataControlRowState.Insert) = DataControlRowState.Normal) OrElse Me.InsertVisible) Then
                If (cell.Controls.Count > 0) Then
                    control = cell.Controls.Item(0)
                    Dim box As TextBox = TryCast(control, TextBox)
                    If (Not box Is Nothing) Then
                        [text] = box.Text
                    End If
                ElseIf includeReadOnly Then
                    Dim s As String = cell.Text
                    If (s = "&nbsp;") Then
                        [text] = String.Empty
                    ElseIf (Me.SupportsHtmlEncode AndAlso Me.HtmlEncode) Then
                        [text] = HttpUtility.HtmlDecode(s)
                    Else
                        [text] = s
                    End If
                End If
                If (Not [text] Is Nothing) Then
                    If ((TypeOf [text] Is String AndAlso (CStr([text]).Length = 0)) AndAlso Me.ConvertEmptyStringToNull) Then
                        [text] = Nothing
                    End If
                    If ((TypeOf [text] Is String AndAlso (CStr([text]) = nullDisplayText)) AndAlso (nullDisplayText.Length > 0)) Then
                        [text] = Nothing
                    End If
                    If dictionary.Contains(dataField) Then
                        dictionary.Item(dataField) = [text]
                    Else
                        dictionary.Add(dataField, [text])
                    End If
                End If
            End If
        End Sub




    End Class


End Namespace