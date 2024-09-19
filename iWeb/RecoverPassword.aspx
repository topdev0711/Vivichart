<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="RecoverPassword" title="Recover Password" Codebehind="RecoverPassword.aspx.vb" %>

<%@ Register Src="~/UserControls/ChangePassword.ascx" TagName="changePwd" TagPrefix="uc1" %>

<asp:Content ID="Content2" ContentPlaceHolderID="headContent" Runat="Server">       
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

<asp:UpdatePanel runat="server" ID="UPD_panel">
    <ContentTemplate>
        <br />
        <Iaw:IAWHyperLink runat="server" ID="btnLogon" Url="~/secure/Login.aspx" Text="::LT_S0289" orgText="Back to Login Page" />
        <Iaw:IAWHyperLinkButton runat="server" ID="btnReset" CausesValidation="false" Text="::LT_S0290" orgText="Start Again" />
        <br />
        <br />
        <fieldset style="width: 600px; margin-left: auto; margin-right: auto;">
            <legend>
                <iaw:IAWLabel runat="server" ID="lblFieldset" Text="::LT_S0291" orgText="Recover Password"></iaw:IAWLabel>
            </legend>

            <iaw:IAWPanel runat="server" ID="MainPanel">
            
                <table class="dataforms" id="fieldsTable">

                    <%-- USERNAME ---------------------------------------------------------------------------------------- --%>

                    <tr>
                        <td colspan="2">
                            <iaw:IAWLabel runat="server" Font-Bold="true" ID="lblMsg" Visible="false" EnableViewState="false" />
                        </td>
                    </tr>

                    <tr>
                        <td class="formlabel">
                            <iaw:IAWLabel runat="server" ID="lblUserName" Text="::LT_S0292" orgText="Enter your Username" />
                        </td>
                        <td class="formdata">
                            <iaw:IAWTextbox ID="txtUserName" MaxLength="40" runat="server" CausesValidation="false"></iaw:IAWTextbox>
                            <iaw:IAWRequiredFieldValidator ID="RFV_txtUserName" runat="server" EnableClientScript="true" Enabled="true"
                                                        Display="Dynamic" ControlToValidate="txtUserName" AddBR="true" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_1_sep">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_1_continue">
                        <td class="formlabel">&#160;</td>
                        <td class="formdata" align="left">
                            <iaw:IAWHyperLinkButton runat="server" ID="btn_1_continue" CausesValidation="false" Text="::WB_B0019" orgText="Continue" />
                            <asp:Button runat="server" id="btn_1_hidden" style="display:none" />
                        </td>
                    </tr>

                    <%-- SECURITY QUESTION AND ADDRESSES--------------------------------------------------------------------------- --%>

                    <tr runat="server" id="TR_2_sep1">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_2_qu">
                        <td class="formlabel">
                            <iaw:IAWLabel runat="server" ID="lblSecurityQu" Text="::LT_S0294" orgText="Security Question" />
                        </td>
                        <td class="formdata">
                            <iaw:IAWLabel runat="server" ID="txtSecurityQu"></iaw:IAWLabel>
                        </td>
                    </tr>

                    <tr runat="server" id="TR_2_ans">
                        <td class="formlabel">
                            <iaw:IAWLabel runat="server" ID="lblSecurityAnswer" Text="::LT_S0295" orgText="Security Answer" />
                        </td>
                        <td class="formdata">
                            <iaw:IAWTextbox ID="txtSecurityAnswer" runat="server" TextMode="Password" CssClass="formcontrol"></iaw:IAWTextbox>
                            <iaw:IAWRequiredFieldValidator ID="RFV_txtSecurityAnswer" runat="server" EnableClientScript="true"
                                                        Display="Dynamic" Enabled="true" ControlToValidate="txtSecurityAnswer" AddBR="true" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_2_sep2">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_2_Address" valign="top">
                        <td class="formlabel" valign="top">
                            <iaw:IAWLabel runat="server" ID="IAWLabel1" Text="::LT_S0296" orgText="Send Security Code To" />
                        </td>
                        <td class="formdata" valign="top">
                            <asp:RadioButtonList ID="rblAddresses" runat="server" RepeatDirection="Vertical" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_2_sep3">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_2_Continue">
                        <td class="formlabel">&#160;</td>
                        <td class="formdata" align="left">
                            <iaw:IAWHyperLinkButton runat="server" ID="btn_2_continue" CausesValidation="true" Text="::WB_B0019" orgText="Continue" />
                            <asp:Button runat="server" id="btn_2_hidden" style="display:none" />
                        </td>
                    </tr>

                    <%-- SECURITY CODE ---------------------------------------------------------------------------------------- --%>

                    <tr runat="server" id="TR_3_MsgSent">
                        <td class="formlabel" colspan="2" align="centre">
                            <br />
                            <iaw:IAWLabel runat="server" ID="lab_3_MsgSent" Font-Bold="true" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_3_sep1">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_3_SecurityCode" valign="top">
                        <td class="formlabel">
                            <iaw:IAWLabel runat="server" ID="IAWLabel2" Text="::LT_S0297" orgText="Enter Security Code" />
                        </td>
                        <td class="formdata">
                            <iaw:IAWTextbox ID="txtSecurityCode" Width="100px" MaxLength="10" runat="server" CssClass="formcontrol"></iaw:IAWTextbox>
                            <iaw:IAWRequiredFieldValidator ID="RFV_txtSecurityCode" runat="server" EnableClientScript="true" 
                                                        Display="Dynamic" Enabled="true" ControlToValidate="txtSecurityCode" AddBR="true" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_3_labCodeFail">
                        <td class="formlabel">&#160;</td>
                        <td class="formdata" align="left">
                            <iaw:IAWLabel runat="server" ID="labCodeFail" Text="::LT_S0298" orgText="Incorrect Code" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_3_sep2">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_3_continue">
                        <td class="formlabel">&#160;</td>
                        <td class="formdata" align="left">
                            <iaw:IAWHyperLinkButton runat="server" ID="btn_3_continue" CausesValidation="true" Text="::WB_B0019" orgText="Continue" />
                            <asp:Button runat="server" id="btn_3_hidden" style="display:none" />
                        </td>
                    </tr>
                    
                    <%-- Change Password ---------------------------------------------------------------------------------------- --%>

                    <tr runat="server" id="TR_4_MsgSent">
                        <td class="formlabel" colspan="2" align="left">
                            <br />
                            <iaw:IAWLabel runat="server" ID="lab_4_MsgSent" Font-Bold="true" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_4_sep1">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_4_ChangePwd">
                        <td class="formlabel" colspan="2">
                            <uc1:changePwd runat="server" ID="ChangePwd" />
                        </td>
                    </tr>

                    <tr runat="server" id="TR_4_sep2">
                        <td colspan="2">&#160;<br /></td>
                    </tr>

                    <tr runat="server" id="TR_4_continue">
                        <td class="formlabel">&#160;</td>
                        <td class="formdata" align="left">
                            <iaw:IAWHyperLinkButton runat="server" ID="btn_4_continue" CausesValidation="true" Text="::WB_B0019" orgText="Continue" />
                            <asp:Button runat="server" id="btn_4_hidden" style="display:none" />
                        </td>
                    </tr>

                    <%-- --------------------------------------------------------------------------------------------- --%>

                </table>
            </iaw:IAWPanel>

        </fieldset>
    </ContentTemplate>
</asp:UpdatePanel>


</asp:Content>

