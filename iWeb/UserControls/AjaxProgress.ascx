<%@ Control Language="VB" AutoEventWireup="false" Inherits="AjaxProgress" Codebehind="AjaxProgress.ascx.vb" %>


<div class="ajaxProgressDiv">
 <iaw:IAWLabel id="lblDisplayText" runat="server" Text="::LT_S0315" orgText="Please wait" />
 <asp:Image ID="imgProgress" 
            runat="server" 
            ImageAlign="AbsMiddle" 
            ImageUrl="~/graphics/activity.gif" />
 
</div> 

