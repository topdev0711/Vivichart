<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="Secure_Login" title="Login" Codebehind="Login.aspx.vb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server" >
    <table id="tbl-login">
        <tr class="row1">                
            <td class="row1_cell1">
                <iaw:IAWPanel runat="server" id="div_login" style="padding-top:6px;padding-bottom:6px;" ClientIDMode="Static"  >
                    <iaw:IAWLabel runat="server" ID="msgAbove" Text="::WB_B0023"></iaw:IAWLabel>
                    <iaw:IAWPanel runat="server"  ID="PNL_login" >
                        <iaw:IAWLogin runat="server" ID="Login2" />
                        <iaw:IawMessage runat="server"
                                        ID="msg_lockedout"
                                        ShowIcon="true"
                                        MessageType="information"
                                        Icon="information"
                                        VisibleOnClient="false"
                                        QMessage="WB_B0022" />                                    
                    </iaw:IAWPanel>
                    <iaw:IAWLabel runat="server" ID="msgBelow" Text="::WB_B0024"></iaw:IAWLabel>
                </iaw:IAWPanel>
                <div runat="server" class="div_agreement" id="div_agreement" style="display:none;">
                    <hr />
                    <span runat="server" id="span_agreement"></span>
                    <hr />
                    <Iaw:IAWHyperLinkButton runat="server" ID="btn_agreement"  QMessageRef="WB_B0019" OnClientClick="toggleVis(IAW.contentID + 'div_agreement', IAW.contentID + 'div_login');setFocus();return false;" />
                </div>
            </td>                                                      
        </tr>
         <tr class="row2">
            <td class="row2_cell1">
                <Iaw:IAWLabel runat="server" ID="lblMsg" CssClass="loginmesssage" />
            </td>
        </tr>                   
    </table>        

<%--    <script type="text/javascript" >      
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);      
        function EndRequestHandler(sender, args)
        {
            setFocus();
        }
        dumpControls();
    </script>  --%>
        
</asp:Content>


