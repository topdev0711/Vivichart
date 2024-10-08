﻿Imports System.ComponentModel

Namespace IAW.controls

    <ToolboxData("<{0}:IAWImageButton runat=server />")>
    Public Class IAWImageButton
        Inherits ImageButton

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateText() As Boolean
            Get
                If ViewState("TranslateText") Is Nothing Then Return True
                Return ViewState("TranslateText")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateText") = value
            End Set
        End Property

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal text.")>
        Public WriteOnly Property orgText As String
            Set(value As String)
            End Set
        End Property

        <Browsable(True), Category("Behavior"), DefaultValue(True), Description("Whether text should be translated")>
        Public Property TranslateTooltip() As Boolean
            Get
                If ViewState("TranslateTooltip") Is Nothing Then Return True
                Return ViewState("TranslateTooltip")
            End Get
            Set(ByVal value As Boolean)
                ViewState("TranslateTooltip") = value
            End Set
        End Property

        <Browsable(True), Category("Appearance"), DefaultValue(""), Description("The original literal tooltip.")>
        Public WriteOnly Property orgToolTip As String
            Set(value As String)
            End Set
        End Property

        Protected Overrides Sub Render(writer As HtmlTextWriter)

            ' Retrieve the image button's AlternateText and ToolTip properties
            Dim storedAltText As String = Me.AlternateText
            Dim storedTooltip As String = Me.ToolTip

            ' Set the translated storedAltText and storedTooltip on the image button
            If TranslateText Then Me.AlternateText = ctx.Translate(storedAltText)
            If TranslateTooltip Then Me.ToolTip = ctx.Translate(storedTooltip)

            ' Render the image button with the translated storedAltText and storedTooltip
            MyBase.Render(writer)

            ' Reset the AlternateText and ToolTip properties to their original values
            Me.AlternateText = storedAltText
            Me.ToolTip = storedTooltip
        End Sub

    End Class

End Namespace
