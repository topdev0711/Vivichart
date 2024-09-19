<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="wrong_version" title="Untitled Page" Codebehind="wrong_version.aspx.vb" %>
<%@ Register TagPrefix="uc1" TagName="wuc_text" Src="~/UserControls/wuc_text.ascx" %>
<%@ Outputcache Location="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
   <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" GroupingText="::LT_S0375" orgGroupingText="Unavailable">
		<p><img style="vertical-align:middle" alt="stop" src="../general/stop.gif" /></p>
		<p>
			   
			<uc1:wuc_text id="t_error" src="WB_A0009" runat="server"></uc1:wuc_text></p>
				
		<p>
			<iaw:IAWLabel id="lblSysReq" runat="server"></iaw:IAWLabel>
			<iaw:IAWLabel id="lblSysReq_ver" runat="server"></iaw:IAWLabel>
		</p>				
		<p>
			<iaw:IAWLabel id="lblWeb" runat="server"></iaw:IAWLabel>
			<iaw:IAWLabel id="lblWeb_ver" runat="server"></iaw:IAWLabel>
		</p>
   </iaw:IAWPanel>
				
</asp:Content>

