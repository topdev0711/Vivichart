<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="client_details.aspx.vb" Inherits=".client_details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        .formlabel,
        .formlabel {
            height: 30px;
        }
        .trButton {
            visibility: hidden;
        }
        .backButton {
            display: inline-block;
            margin: 0;
            padding: 6px;
            background-color: var(--legend-bg-color);
            color: var(--legend-color);
            border: 3px solid var(--legend-border-color);
            position: absolute;
            top: -35px;
            left: 8px;
            width: 30px;
            text-align: center;
            border-radius: 20px;
        }
        .backButton span {
            position: relative;
            top: 1px;
        }
        .tabScroller {
            display: block;
            width: 100%;
            height: 304px;
            overflow-x: hidden;
        }
        #lblEmailWarning {
            position: absolute;
            top: 50%;
            right: -10px;
            transform: translateY(-50%);
            color: var(--text-color);
        }
        .ajax__tab_body {
            min-width: 846px;
        }
    </style>
    <script defer="defer">
        $('document').ready(() => {
            document.querySelectorAll('#tpClientDetails input').forEach(e =>
            {
                e.oninput = onAction
                e.onchange = onAction
            })
            document.querySelectorAll('#tpClientDetails select').forEach(e =>
            {
                e.onchange = onAction;
            });
        })

        function onAction() {
            const trButton = document.querySelector('.trButton')
            trButton.style.visibility = 'visible'
            BannerEnabled(false);
        }

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
    <asp:HiddenField runat="server" ID="hdnPrevEmail" />
    <asp:HiddenField runat="server" ID="hdnPrevBrand" />
    <asp:HiddenField runat="server" ID="hdnURL" ClientIDMode="Static" />

    <iaw:IAWPanel runat="server" ID="pnlClient" GroupingText="::LT_M0008" orgGroupingText="Client" CssClass="PanelClass" TranslateText="true">
        <iaw:IAWHyperLinkButton runat="server" ID="btnBack" ToolTip="::LT_M0025" orgToolTip="Back to list" CssClass="backButton" IconClass="iconButton fa-solid fa-arrow-left" Visible="false" />
        <ajaxToolkit:TabContainer ID="tcClient" runat="server" CssClass="iaw">
            <iaw:IAWTabPanel runat="server" ClientIDMode="Static" ID="tpClientDetails" HeaderText="::LT_M0009" orgHeaderText="Client Details">
                <ContentTemplate>
                    <table border="0" width="100%">
                        <tr>
                            <td style="display: block">
                                <table border="0" width="100%" class="grp">
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel7" runat="server" Text="::LT_M0002" orgText="Company Name" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox ID="txtCompanyName" runat="server" MaxLength="50" CssClass="formcontrol" ValidationGroup="Client" />
                                            <iaw:IAWRequiredFieldValidator runat="server" ID="rfvCompanyName" ControlToValidate="txtCompanyName" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" AddBR="true" />
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel9" runat="server" Text="::LT_S0180" orgText="Email Address" />
                                        </td>
                                        <td class="formdata" style="position: relative">
                                            <iaw:IAWTextbox ID="txtEmail" runat="server" MaxLength="250" CssClass="formcontrol" ValidationGroup="Client" />
                                            <iaw:IAWLabel runat="server" ID="lblEmailWarning" ToolTip="::LT_M0029" orgToolTip="Email is not using your domain name" CssClass="fa-solid fa-triangle-exclamation" />
                                            <iaw:IAWRequiredFieldValidator runat="server" ID="rfvEmail" ControlToValidate="txtEmail" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="Client" EnableClientScript="true" Enabled="true" AddBR="true" />
                                            <iaw:IAWCustomValidator runat="server" ID="cvEmail" ControlToValidate="txtEmail" ErrorMessage="::LT_S0182" orgErrorMessage="This does not appear to be a valid email address" AddBR="true" ValidationGroup="Client" Enabled="true" Display="Dynamic" />
                                            <iaw:IAWCustomValidator runat="server" ID="cvEmailDomain" ControlToValidate="txtEmail" ErrorMessage="" AddBR="true" ValidationGroup="Client" Enabled="true" Display="Dynamic" />
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel11" runat="server" Text="::LT_S0200" orgText="Zip Password" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox ID="txtZipPass" runat="server" MaxLength="50" CssClass="formcontrol" />
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
                                            <iaw:IAWLabel ID="IAWLabel2" runat="server" Text="::LT_S0038" orgText="Brand" />
                                        </td>
                                        <td class="formdata middle">
                                            <iaw:IAWDropDownList ID="ddlbBrand" runat="server" TranslateText="false" />
                                        </td>
                                    </tr>

                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel13" runat="server" Text="::LT_M0010" orgText="Maximum Image Size KB" />
                                        </td>
                                        <td class="formdata">
                                            <table border="0">
                                                <tr>
                                                    <td>
                                                        <iaw:IAWTextbox runat="server" ID="numMaxImgSize" />
                                                        <ajaxToolkit:SliderExtender ID="slideMaxImgSize" runat="server" TargetControlID="numMaxImgSize" Minimum="10" Maximum="50" Steps="5" BoundControlID="labMaxImgSize" />
                                                    </td>
                                                    <td class="formlabel">
                                                        <iaw:IAWLabel runat="server" ID="labMaxImgSize" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel15" runat="server" Text="::LT_M0011" orgText="Maximum Data Allowed MB" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWLabel ID="lblMaxData" runat="server" TranslateText="false" />
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel17" runat="server" Text="::LT_M0003" orgText="Service Type" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWLabel ID="lblServiceType" runat="server" TranslateText="false" />
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel ID="IAWLabel19" runat="server" Text="::LT_M0012" orgText="Service Expiry" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWLabel ID="lblServiceExpiry" runat="server" TranslateText="false" />
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

                                    <tr class="formrow trButton">
                                        <td colspan="2">
                                            <br />
                                            <iaw:IAWHyperLinkButton runat="server" ID="btnClientSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="Client" IconClass="iconButton fa-solid fa-floppy-disk" style="transition-duration: unset" />
                                            &nbsp;&nbsp;
                                            <iaw:IAWHyperLinkButton runat="server" ID="btnClientCancel" Text="::LT_S0138" orgText="Cancel" IconClass="iconButton fa-solid fa-circle-xmark" style="transition-duration: unset" />
                                        </td>
                                    </tr>

                                </table>
                            </td>
                            <td>
                                <asp:Chart ID="Chart1" runat="server" Width="300px" Height="300px">
                                    <Series>
                                        <asp:Series Name="Series1" ChartType="Pie" />
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1" />
                                    </ChartAreas>
                                </asp:Chart>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpAuditDetails" HeaderText="::LT_M0019" orgHeaderText="Audit Details" CssClass="tabScroller">
                <ContentTemplate>
                    <iaw:IAWGrid runat="server" ID="grdClientAudit" AutoGenerateColumns="False" DataKeyNames="audit_id"
                        AllowSorting="false" ShowHeaderWhenEmpty="true"
                        TranslateHeadings="True" ClientIDMode="Static">
                        <Columns>
                            <iawb:IAWBoundField DataField="action" HeaderText="::LT_M0013" orgHeaderText="Action" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="updated_at" HeaderText="::LT_M0014" orgHeaderText="Update At" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="updated_by" HeaderText="::LT_M0015" orgHeaderText="Update By" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="field" HeaderText="::LT_S0224" orgHeaderText="Field" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="oldValue" HeaderText="::LT_M0016" orgHeaderText="Old Value" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="newValue" HeaderText="::LT_M0017" orgHeaderText="New Value" ReadOnly="True" />
                        </Columns>
                        <HeaderStyle Wrap="false" />
                        <RowStyle Wrap="false" />
                    </iaw:IAWGrid>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpDomains" HeaderText="::LT_M0018" orgHeaderText="Domains" CssClass="tabScroller">
                <ContentTemplate>
                    <iaw:IAWGrid runat="server" ID="grdClientDomains" AutoGenerateColumns="False" DataKeyNames="domain_id"
                        AllowSorting="false" ShowHeaderWhenEmpty="true"
                        TranslateHeadings="True" ClientIDMode="Static">
                        <Columns>
                            <iawb:IAWBoundField DataField="domain_name" HeaderText="::LT_M0018" orgHeaderText="Domains" ReadOnly="True" />
                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                <HeaderTemplate>
                                    <iaw:IAWImageButton runat="server" ID="btnAddDomain" CommandName="New" ImageUrl="~/graphics/1px.gif" CssClass="IconPic Icon16 IconAddHead" ToolTip="::LT_S0039" orgToolTip="Add" />
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <div class="ActionButtons">
                                        <iaw:IAWHyperLinkButton ID="btnAmendRow" runat="server" CssClass="IconPic Icon16 IconEdit" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                        <iaw:IAWHyperLinkButton ID="btnDeleteRow" runat="server" CssClass="IconPic Icon16 IconDelete" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                        <iaw:IAWlabel ID="lblNoDeleteRow" runat="server" CssClass="IconPic Icon16 IconBan" ToolTip= "::LT_S0041" orgTooltip="In Use. Can not be deleted" Visible="false" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <HeaderStyle Wrap="false" />
                        <RowStyle Wrap="false" />
                    </iaw:IAWGrid>
                </ContentTemplate>
            </iaw:IAWTabPanel>
        </ajaxToolkit:TabContainer>
       </iaw:IAWPanel>

    <%-- add domain popup --%>
    <asp:HiddenField runat="server" ID="hdnDomainId" />
    <iaw:IAWPanel ID="popDomain" runat="server" GroupingText="::LT_M0023" orgGroupingText="Add Domain" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDomainSave">
        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel48" runat="server" Text="::LT_M0024" orgText="Domain" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtDomain" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="User" />
                    <iaw:IAWRequiredFieldValidator runat="server" ID="rfvDomain" ControlToValidate="txtDomain" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" ValidationGroup="Domain" EnableClientScript="true" Enabled="true" AddBR="true" />
                </td>
            </tr>

        </table>

        <table border="0">
            <tr>
                <td>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDomainSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="Domain" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDomainCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>

    </iaw:IAWPanel>

    <asp:Button ID="btnDomainFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDomainForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDomain" TargetControlID="btnDomainFake" />

    <%-- Add Domain Confirm Popup --%>
    <iaw:IAWPanel ID="popDomainConf" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnDomainConfOk">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel1" runat="server" Text="::LT_M0026" orgText="You have a domain now." />
                </td>
            </tr>
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel3" runat="server" Text="::LT_M0027" orgText="You need to fix your emails to use one of your domains as an email host." />
                </td>
            </tr>
            <tr>
                <td style="font-weight: bold; font-style: italic;">
                    <br />
                    <iaw:IAWLabel ID="lblDomainConf" runat="server" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDomainConfOk" Text="::LT_M0028" orgText="I'll fix emails" CausesValidation="false" ToolTip="::LT_M0028" IconClass="iconButton fa-solid fa-circle-check" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnDomainConfFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDomainConf" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDomainConf" TargetControlID="btnDomainConfFake" />

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
