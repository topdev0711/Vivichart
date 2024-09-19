<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="ChangePassword.ascx.vb" Inherits=".ChangePassword" %>

<asp:UpdatePanel runat="server" ID="UPD_panel">
    <ContentTemplate>                
        
        <%-- PASSWORD SUCCESS --%>   
        <iaw:IawMessage runat="server"
                        ID="msg_changed"
                        ShowIcon="true"
                        Icon="information"
                        MessageType="information"
                        VisibleOnClient="false"
                        EnableViewState="false"
                        MessageText="The Password has been updated" />  <%-- "The Password has been updated" is overridden in the VB for translation --%>   
                        
        <%-- PASSWORD ERROR --%>   
        <iaw:IawMessage runat="server"
                        ID="msg"
                        ShowIcon="true"
                        Icon="error"
                        MessageType="warning"
                        Visible="false"
                        EnableViewState="false" />
        
         <%-- ENTER PASSWORD --%>
         <div style="margin-left:auto;margin-right:auto;">
            <table class="dataforms" id="Table1" cellspacing="1" cellpadding="1" border="0">				      
	            <tr runat="server" id="TR_orig">
		            <td class="formlabel"><iaw:IAWLabel id="lab_org" runat="server" text="::LT_S0303" orgText="Current Password"></iaw:IAWLabel></td>
		            <td class="formdata">
		                <iaw:IAWTextbox id="txtPwdOrig" runat="server" CssClass="formcontrol" TextMode="Password"></iaw:IAWTextbox>
		            </td>
	            </tr>
	            <tr>
		            <td class="formlabel"><iaw:IAWLabel id="lab_new1" runat="server" text="::LT_S0304" orgText="New Password"></iaw:IAWLabel></td>
		            <td class="formdata">
		                <iaw:IAWTextbox id="txtPwdNew" runat="server" CssClass="formcontrol" TextMode="Password"></iaw:IAWTextbox>
		            </td>
	            </tr>
	            <tr>
		            <td class="formlabel"><iaw:IAWLabel id="lab_new2" runat="server" text="::LT_S0305" orgText="Confirm New Password"></iaw:IAWLabel></td>
		            <td class="formdata">
		                <iaw:IAWTextbox id="txtPwdNewConfirm" runat="server" CssClass="formcontrol" TextMode="Password"></iaw:IAWTextbox>
		            </td>
	            </tr>
	            <tr>
	                <td />
	                <td align="left">
                        &nbsp;&nbsp;
                        <iaw:IAWCheckbox ID="cbPasswords" runat="server" Text="::LT_S0306" orgText="Show Passwords" OnClick="ToggleDisplay();"/>
	                </td>
	            </tr>
            </table>		    	       		         
         </div>

         <%-- PASSWORD FORMAT --%>
         <table class="tblIawMessage" style="text-align:left;margin-left:auto;margin-right:auto;width:550px">
            <tr>
                <td class="informationIcon"></td>
                <td class="infoMessage">		                        
                      <iaw:iawLabel runat=server text="::LT_S0307" orgText="The Password" />
                      <asp:Repeater ID="RPT_format" runat="server">
                        <HeaderTemplate>
                            <ul style="margin-top:6px;margin-bottom:0px;">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li><%#Container.DataItem()%></li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                      </asp:Repeater>			                                        
                </td>
            </tr>
         </table>

    </ContentTemplate>
</asp:UpdatePanel>