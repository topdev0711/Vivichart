<%@ Page Title="" Async="true" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="client_users.aspx.vb" Inherits=".client_users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <script defer="defer">
        function copyToClipboard()
        {
            const textToCopy = $ID("hdnURL").value;
            navigator.clipboard.writeText(textToCopy).then(() =>
            {
                $("#labInfo").removeClass("hidden");
                setTimeout(() =>
                {
                    $("#labInfo").addClass("hidden");
                }, 3000);
            }).catch(err =>
            {
                iawError("Failed to copy text: ", err);
            });
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <iaw:IAWPanel runat="server" ID="pnlUsers" GroupingText="::LT_S0172" orgGroupingText="Users" CssClass="PanelClass">
        <div id="divUsers" class="divScroll">
            <iaw:IAWGrid runat="server" ID="grdUsers" AutoGenerateColumns="False" DataKeyNames="user_ref"
                         AllowSorting="false"
                         TranslateHeadings="True" ClientIDMode="Static">
                <Columns>
                    <iawb:IAWBoundField DataField="forename" HeaderText="::LT_S0173" orgHeaderText="Forename" ReadOnly="True" SortExpression="forename,surname" />
                    <iawb:IAWBoundField DataField="surname" HeaderText="::LT_S0174" orgHeaderText="Surname" ReadOnly="True" SortExpression="surname,forename" />
                    <iawb:IAWBoundField DataField="email_address" HeaderText="::LT_S0175" orgHeaderText="Email" ReadOnly="True" SortExpression="email_address" />
                    
                    <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <iaw:IAWLabel runat="server" ID="lblEmailWarning" ToolTip="::LT_M0029" orgToolTip="Email is not using your domain name" CssClass="fa-solid fa-triangle-exclamation" TranslateText="false" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    
                    <iawb:IAWBoundField DataField="role_name" HeaderText="::LT_S0176" orgHeaderText="User Type" ReadOnly="True" SortExpression="role,surname,forename"  />
                    <iaw:IAWCheckBoxField DataField="active_user_bool" HeaderText="::LT_S0177" orgHeaderText="Active" ReadOnly="true" SortExpression="active,surname,forename" ItemStyle-HorizontalAlign="Center" />
                    <asp:TemplateField ItemStyle-CssClass="ActionCell">
                        <HeaderTemplate>
                            <iaw:IAWImageButton runat="server" ID="btnAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="::LT_S0039" orgTooltip="Add"/>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <div class="ActionButtons">
                                <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                <div>
                                    <iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                    <iaw:IAWLabel ID="lblNoDeleteRow" runat="server" CssClass="IconPic Icon16 IconBan" ToolTip="::LT_S0041" orgToolTip="In Use. Can not be deleted" Visible="false" />
                                </div>
                            </div>
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>

                </Columns>
                <HeaderStyle Wrap="false" />
                <RowStyle Wrap="false" />
            </iaw:IAWGrid>
        </div>
    </iaw:IAWPanel>

    <asp:HiddenField runat="server" ID="hdnUserRef" />
    <iaw:IAWPanel ID="popUser" runat="server" GroupingText="::LT_S0179" orgGroupingText="User" Style="display: none" CssClass="PopupPanel" DefaultButton="btnUserSave">
        <iaw:wuc_help runat="server" ID="helplink_popUser" Reference="ClientUsers"/>
        <asp:HiddenField runat="server" ID="hdnURL" ClientIDMode="Static" />

        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel48" runat="server" Text="::LT_S0173" orgText="Forename" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtForename" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="User" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvForename" ControlToValidate="txtForename" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="User" EnableClientScript="true" Enabled="true" AddBR="true" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel2" runat="server" Text="::LT_S0174" orgText="Surname" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtSurname" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="User" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvSurname" ControlToValidate="txtSurname" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="User" EnableClientScript="true" Enabled="true" AddBR="true" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel3" runat="server" Text="::LT_S0180" orgText="Email Address" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtEmail" runat="server" MaxLength="250" Width="400px" CssClass="formcontrol" ValidationGroup="User" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvEmail" ControlToValidate="txtEmail" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="User" EnableClientScript="true" Enabled="true" AddBR="true" />
                    <iaw:IAWCustomValidator runat="server" ID="cvEmail" ControlToValidate="txtEmail" ErrorMessage="::LT_S0182" orgErrorMessage="This does not appear to be a valid email address" AddBR="true" ValidationGroup="User" Enabled="true" Display="Dynamic" />
                    <iaw:IAWCustomValidator runat="server" ID="cvEmailDomain" ControlToValidate="txtEmail" ErrorMessage="" AddBR="true" ValidationGroup="User" Enabled="true" Display="Dynamic" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel4" runat="server" Text="::LT_S0176" orgText="User Type" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWDropDownList ID="ddlbRole" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel5" runat="server" Text="::LT_S0178" orgText="Language" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWDropDownList ID="ddlbLanguage" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel1" runat="server" Text="::LT_S0038" orgText="Brand" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWDropDownList ID="ddlbBrand" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel6" runat="server" Text="::LT_S0177" orgText="Active" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWCheckBox runat="server" ID="cbActive" />
                </td>
            </tr>

            <tr class="formrow">
                <td colspan="2">
                    <iaw:IAWHyperLinkButton runat="server" ID="btnCopyLoginURL" 
                        Text="::LT_A0186" orgText="Copy Login URL" 
                        IconClass="iconButton fa-regular fa-copy"
                        OnClientClick="copyToClipboard(); return false;"/>
                    <span>&nbsp;</span>
                    <asp:Label runat="server" ID="labInfo" ClientIDMode="Static" CssClass="fa-solid fa-check hidden" />
                </td>
            </tr>

        </table>

        <table border="0">
            <tr>
                <td>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnUserSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="User" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnUserCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
            <tr>
                <td>
                    <iaw:IAWLabel ID="txtErrorMessage" runat="server" ForeColor="red" Visible="false" />
                </td>

            </tr>

        </table>

    </iaw:IAWPanel>

    <asp:Button ID="btnUserFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeUserForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popUser" TargetControlID="btnUserFake" />

    <%-- Delete Confirm Popup --%>

    <iaw:IAWPanel ID="popDelete" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnDeleteCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="lblDeleteConfirm" runat="server" Text="::LT_S0159" orgText="Are you sure you want to delete this entry?" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteOk" Text="::LT_S0033" orgText="Yes" CausesValidation="false" ToolTip="::LT_S0160" orgTooltip="Delete entry" IconClass="iconButton fa-solid fa-circle-check" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteCancel" Text="::LT_S0034" orgText="No" CausesValidation="false" ToolTip="::LT_S0161" orgTooltip="Do not delete entry" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnDeleteFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDelete" TargetControlID="btnDeleteFake" />

</asp:Content>
