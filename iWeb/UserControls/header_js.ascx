<%@ Control Language="VB" AutoEventWireup="false" Inherits="header_js" Codebehind="header_js.ascx.vb" %>
   
<script type="text/javascript" src="<% response.Write(ctx.virtualDir()) %>/js/page_dirty.js" ></script> 
<script type="text/javascript" src="<% response.Write(ctx.virtualDir()) %>/js/master.js?n" ></script>
<script type="text/javascript">
    IAW.clientID = "<% response.Write(ctx.clientID()) %>";
    IAW.contentID = "<% response.Write(ctx.contentID()) %>_";
    IAW.buttonBarID = "<% response.Write(ctx.buttonBarID()) %>";
</script>

<asp:PlaceHolder ID="phJS" runat="server"></asp:PlaceHolder>
<asp:PlaceHolder ID="phCss" runat="server"></asp:PlaceHolder>
