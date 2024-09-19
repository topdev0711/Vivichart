<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="clients.aspx.vb" Inherits=".clients" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <iaw:IAWPanel runat="server" ID="pnlClients" GroupingText="Clients" CssClass="PanelClass" TranslateText="true">
        <iaw:IAWGrid runat="server" ID="grdClients" AutoGenerateColumns="False" DataKeyNames="client_id"
            AllowSorting="false" ShowHeaderWhenEmpty="true"
            TranslateHeadings="false" ClientIDMode="Static">
            <Columns>
                <iawb:IAWBoundField DataField="client_name" HeaderText="Company Name" ReadOnly="True" />
                <iawb:IAWBoundField DataField="client_email" HeaderText="Email Address" ReadOnly="True" />

                <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <iaw:IAWLabel runat="server" ID="lblEmailWarning" ToolTip="Email is not using your domain name" CssClass="fa-solid fa-triangle-exclamation" TranslateText="false" />
                    </ItemTemplate>
                </asp:TemplateField>

                <iawb:IAWBoundField DataField="service_type" HeaderText="Service Type" ReadOnly="True" />
                <iawb:IAWBoundField DataField="service_expiry" HeaderText="Service Expiry Date" ReadOnly="True" />

                <asp:TemplateField ItemStyle-CssClass="ActionCell">
                    <HeaderTemplate>
                        <iaw:IAWImageButton runat="server" ID="btnAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="Add Client a" TranslateTooltip="false" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <div class="ActionButtons">
                            <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="Amend" TranslateTooltip="false" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                            <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="Delete" TranslateTooltip="false" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                        </div>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Center" />
                </asp:TemplateField>
            </Columns>
            <HeaderStyle Wrap="false" />
            <RowStyle Wrap="false" />
        </iaw:IAWGrid>
    </iaw:IAWPanel>

    <%-- Add client popup --%>
    <iaw:IAWPanel ID="popClient" runat="server" GroupingText="Add New Client" Style="display: none" CssClass="PopupPanel" DefaultButton="btnClientSave" TranslateText="false">
        <table border="0" class="listform">
            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel48" runat="server" Text="Customer Id" TranslateText="false" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtCustId" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="Client" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvCustId" ControlToValidate="txtCustId" ErrorMessage="Required" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" TranslateErrorMessage="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel1" runat="server" Text="Forename" TranslateText="false" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtForename" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="Client" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvForename" ControlToValidate="txtForename" ErrorMessage="Required" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" TranslateErrorMessage="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel4" runat="server" Text="Surname" TranslateText="false" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtSurname" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="Client" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvSurname" ControlToValidate="txtSurname" ErrorMessage="Required" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" TranslateErrorMessage="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel2" runat="server" Text="Company Name" TranslateText="false" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtCompanyName" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="Client" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvCompanyName" ControlToValidate="txtCompanyName" ErrorMessage="Required" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" TranslateErrorMessage="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel3" runat="server" Text="Email Address" TranslateText="false" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtEmail" runat="server" MaxLength="250" Width="400px" CssClass="formcontrol" ValidationGroup="Client" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvEmail" ControlToValidate="txtEmail" Message="Required" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" TranslateErrorMessage="false"/>
                    <iaw:IAWCustomValidator runat="server" ID="cvEmail" ControlToValidate="txtEmail" ErrorMessage="This does not appear to be a valid email address" AddBR="true" ValidationGroup="Client" Enabled="true" Display="Dynamic" translatetext="false" />
                </td>
            </tr>
        </table>

        <table border="0">
            <tr>
                <td>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnClientSave" Text="Save" CausesValidation="true" ValidationGroup="Client" IconClass="iconButton fa-solid fa-floppy-disk" TranslateText="false" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnClientCancel" Text="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" TranslateText="false" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnClientFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeClientForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popClient" TargetControlID="btnClientFake" />

    <%-- Delete Confirm Popup --%>

    <iaw:IAWPanel ID="popDelete" runat="server" GroupingText="Confirm" Style="display: none"
        TranslateText="false" CssClass="PopupPanel" DefaultButton="btnDeleteCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="lblDeleteConfirm" runat="server" Text="Are you sure you want to delete this entry?" TranslateText="false" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteOk" Text="Yes" CausesValidation="false" ToolTip="Delete entry" IconClass="iconButton fa-solid fa-circle-check" TranslateText="false" TranslateTooltip="false" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteCancel" Text="No" CausesValidation="false" ToolTip="Do not delete entry" IconClass="iconButton fa-solid fa-circle-xmark" TranslateText="false" TranslateTooltip="false" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnDeleteFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDelete" TargetControlID="btnDeleteFake" />

</asp:Content>
