<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="not_licensed" title="Unlicensed Notice" Codebehind="not_licensed.aspx.vb" %>
<%@ Register TagPrefix="uc1" TagName="wuc_text" Src="~/UserControls/wuc_text.ascx" %>
<%@ Outputcache Location="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" GroupingText="::LT_S0375" orgGroupingText="Unavailable">
        <iaw:iawmessage ID="errortxt" Visible="True" MessageType="error" runat="server" Icon="error" ShowIcon="true"
                        MessageText="::LT_S0376" orgMessageText="The website is currently unavailable. Please try again later"/>
    </iaw:IAWPanel>

</asp:Content>
