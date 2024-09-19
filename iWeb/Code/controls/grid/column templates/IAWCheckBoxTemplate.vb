
imports System
imports System.Web
imports System.Data
imports System.Web.UI
imports System.Web.UI.WebControls

Namespace IAW.templates

    Public Class IAWCheckBoxTemplate
        Inherits IAWStyleTemplate
        Implements ITemplate, INamingContainer

        Private img As Image
        Private _column As String
        Private _trueValue As String
        Private _falseValue As String

        Public Property trueValue() As String
            Get
                Return _trueValue
            End Get
            Set(ByVal value As String)
                _trueValue = value
            End Set
        End Property

        Public Property falseValue() As String
            Get
                Return _falseValue
            End Get
            Set(ByVal value As String)
                _falseValue = value
            End Set
        End Property

        Public Overloads Sub InstantiateIn(ByVal container As System.Web.UI.Control)
            MyBase.InstantiateIn(container)
            'Dim ck As New CheckBox
            'AddHandler ck.DataBinding, AddressOf Me.BindData
            'container.Controls.Add(ck)
            img = New Image
            img.ImageAlign = ImageAlign.AbsMiddle
            AddHandler img.DataBinding, AddressOf Me.BindData
            container.Controls.Add(img)

        End Sub

        Public Sub BindData(ByVal sender As Object, ByVal e As EventArgs)
            'Dim container As GridViewRow = CType(ck.NamingContainer, GridViewRow)
            Dim container As GridViewRow = DirectCast(img.NamingContainer, GridViewRow)

            Dim val As String = DirectCast(container.DataItem, DataRowView)(_column).ToString()
            Select Case val
                Case _trueValue
                    'ck.Checked = True
                    img.ImageUrl = "~/graphics/tick2.png"
                    img.AlternateText = "Yes"
                    img.ToolTip = "Yes"
                Case _falseValue
                    'ck.Checked = False
                    img.ImageUrl = "~/graphics/cross2.png"
                    img.AlternateText = "No"
                    img.ToolTip = "No"
                Case Else
                    'ck.Checked = False
                    img.ImageUrl = "~/graphics/trans.gif"
                    img.AlternateText = ""
            End Select

        End Sub

    End Class

End Namespace