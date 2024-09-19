<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" 
         AutoEventWireup="false" 
         Inherits="access_denied" 
         title="Access Denied" Codebehind="access_denied.aspx.vb" %>
         
<%@ Register TagPrefix="uc1" TagName="wuc_text" Src="~/UserControls/wuc_text.ascx" %>
<%@ Outputcache Location="None" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
            
    <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" GroupingText="::LT_S0373" orgGroupingText="Access Denied">
        <img style="vertical-align:middle" alt="stop" src="stop.gif" />
        <iaw:IAWLabel id="lblMsg" Text="::LT_S0374" orgText="You do not have the required privilege to view this resource." runat="server"></iaw:IAWLabel>
            
        <div runat="server" id="divUrl" style="vertical-align:bottom;height:70px;width:400px;overflow:auto">
            <iaw:IAWLabel ID="lblUrl" runat="server"></iaw:IAWLabel>
        </div>
        <br />
        <iaw:IAWLabel id="lblUserMsg" runat="server"></iaw:IAWLabel>
    </iaw:IAWPanel>			
				
</asp:Content>

