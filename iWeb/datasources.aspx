<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master"
    CodeBehind="datasources.aspx.vb" Inherits=".datasources" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style type="text/css">
        .grdField {
            padding: 9px 14px;
            text-align: left;
            white-space: nowrap;
            vertical-align: middle;
        }

        .headField {
            white-space: nowrap;
        }

        .NoRows {
            padding: 3px 5px 3px 5px;
            text-align: left;
            white-space: nowrap;
            vertical-align: middle;
        }

        .fieldlength {
            width: 45px !important;
            min-width: 45px !important;
        }

        #TabContainer1 .ajax__tab_body {
            min-width: 400px;
            border-radius: 0px 20px 20px 20px;
            overflow-y: auto;
        }

        #TabContainer1_tpFields_grdDates tbody tr:not(:first-child) td {
            padding: 9px;
        }

        fieldset {
            max-width: fit-content !important;
            min-width: fit-content !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table border="0">
        <tr>
            <td class="top">
                <iaw:IAWPanel runat="server" ID="panel1" GroupingText="::LT_S0009" orgGroupingText="Data Source" CssClass="PanelClass">
                    <iaw:wuc_help runat="server" ID="helplink_datasource" Reference="Datasource" />

                    <iaw:IAWGrid runat="server" ID="grdDatasource" AutoGenerateColumns="False" DataKeyNames="source_id" editState="Normal" SaveButtonId="" TranslateHeadings="True">
                        <Columns>
                            <iawb:IAWBoundField DataField="source_id" HeaderText="::LT_S0185" orgHeaderText="ID" Visible="false" />
                            <iawb:IAWBoundField DataField="short_ref" HeaderText="::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField" />
                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                <HeaderTemplate>
                                    <iaw:IAWImageButton runat="server" ID="btnDataSourceAdd" CommandName="New" ImageUrl="~/graphics/1px.gif" CssClass="IconPic Icon16 IconAddHead" ToolTip="::LT_S0039" orgTooltip="Add" />
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <div class="ActionButtons">
                                        <div>
                                            <iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                        <div>
                                            <iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                            <iaw:IAWLabel ID="lblNoDeleteRow" runat="server" CssClass="IconPic Icon16 IconBan" ToolTip="::LT_S0041" orgToolTip="In Use. Can not be deleted" Visible="false" />
                                        </div>
                                    </div>
                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />

                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <iaw:IAWLabel runat="server" ID="grdNoModelDataSourceRows" Text="::LT_S0186" orgText="Please add a Data Source" CssClass="NoWrap" />
                        </EmptyDataTemplate>

                        <RowStyle Wrap="False" />
                    </iaw:IAWGrid>
                    <br />
                    <iaw:IAWLabel runat="server" ID="lblDataSourceMsg" Visible="False" CssClass="NoRows" ForeColor="Red" />
                </iaw:IAWPanel>
            </td>
            <td class="top">
                <iaw:IAWPanel runat="server" ID="pnlDataSourceDetails" GroupingText="Set in code" CssClass="PanelClass" TranslateText="false">
                    <ajaxToolkit:TabContainer ID="TabContainer1" runat="server" CssClass="iaw" ClientIDMode="Static">
                        <iaw:IAWTabPanel runat="server" ID="tpFields" HeaderText="::LT_S0188" orgHeaderText="Fields">
                            <ContentTemplate>
                                <div runat="server" id="trActionButtons">
                                    <iaw:IAWHyperLinkButton runat="server" ID="btnFieldsAccept" Text="::LT_S0189" orgText="Accept" />
                                    <iaw:IAWHyperLinkButton runat="server" ID="btnFieldsReject" Text="::LT_S0190" orgText="Reject" />
                                </div>
                                <table border="0">
                                    <tr>
                                        <td class="top">
                                            <iaw:IAWGrid runat="server" ID="grdDates" AutoGenerateColumns="False" DataKeyNames="source_date" editState="Normal" SaveButtonId="" TranslateHeadings="True">
                                                <Columns>
                                                    <iawb:IAWBoundField DataField="source_date" HeaderText="::LT_S0191" orgHeaderText="Date" ReadOnly="True" ItemStyle-CssClass="grdField" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                                                </Columns>
                                            </iaw:IAWGrid>
                                        </td>
                                        <td class="top">

                                            <iaw:IAWGrid runat="server" ID="grdFieldList" AutoGenerateColumns="False" DataKeyNames="field_num" editState="Normal" SaveButtonId="" TranslateHeadings="True">
                                                <Columns>
                                                    <iawb:IAWBoundField DataField="field_num" HeaderText="::LT_S0192" orgHeaderText="#" ReadOnly="True" ItemStyle-CssClass="grdField center" />
                                                    <iawb:IAWBoundField DataField="field_name" HeaderText="::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                    <iawb:IAWBoundField DataField="field_type_pt" HeaderText="::LT_S0102" orgHeaderText="Type" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                    <iawb:IAWBoundField DataField="field_length" HeaderText="::LT_S0193" orgHeaderText="Length" ReadOnly="True" ItemStyle-CssClass="grdField center" />
                                                    <iawb:IAWBoundField DataField="field_format_pt" HeaderText="::LT_S0194" orgHeaderText="Format" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                    <iawb:IAWBoundField DataField="item_ref_field_pt" HeaderText="::LT_S0195" orgHeaderText="Key Field" ReadOnly="True" ItemStyle-CssClass="grdField left" HeaderStyle-Wrap="false" />
                                                    <asp:TemplateField>
                                                        <HeaderTemplate>
                                                            <iaw:IAWLabel runat="server" Text="::LT_S0196" orgText="Used as Parent" />
                                                        </HeaderTemplate>
                                                        <ItemTemplate>
                                                            <iaw:IAWCheckBox ID="cbParentRef" runat="server" Checked='<%# If(Eval("parent_ref_field") = "1", True, False) %>' />  
                                                        </ItemTemplate>
                                                        <ItemStyle CssClass="grdField center noClick" />
                                                        <HeaderStyle Wrap="false" />
                                                    </asp:TemplateField>

                                                    <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                        <ItemTemplate>
                                                            <div class="ActionButtons">
                                                                <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                            </div>
                                                        </ItemTemplate>
                                                        <ItemStyle HorizontalAlign="Center" />
                                                    </asp:TemplateField>
                                                </Columns>
                                                <EmptyDataTemplate>
                                                    <iaw:IAWLabel runat="server" ID="grdNoModelDataSourceRows" Text="::LT_S0186" orgText="Please add a Data Source" CssClass="NoWrap" />
                                                </EmptyDataTemplate>

                                            </iaw:IAWGrid>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </iaw:IAWTabPanel>
                        <iaw:IAWTabPanel runat="server" ID="tpAccessControl" HeaderText="::LT_S0197" orgHeaderText="Access Control">
                            <ContentTemplate>
                                <iaw:IAWGrid runat="server" ID="grdDataSourceAccess" AutoGenerateColumns="False" DataKeyNames="access_id" RowStyle-Wrap="false" editState="Normal" SaveButtonId="" TranslateHeadings="True" ShowHeaderWhenEmpty="true">
                                    <Columns>
                                        <asp:BoundField DataField="access_id" Visible="false" />
                                        <iawb:IAWBoundField DataField="access_type_text" HeaderText="::LT_S0102" orgHeaderText="Type" ReadOnly="True" ItemStyle-CssClass="grdField" />
                                        <iawb:IAWBoundField DataField="access_ref_text" HeaderText="::LT_S0198" orgHeaderText="User or Role" ReadOnly="True" ItemStyle-CssClass="grdField" HeaderStyle-Wrap="false" />

                                        <asp:TemplateField ItemStyle-CssClass="ActionCell" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>

                                                <div class="ActionButtons">
                                                    <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                </div>
                                                <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />

                                            </ItemTemplate>
                                            <HeaderTemplate>
                                                <iaw:IAWImageButton runat="server" ID="btnEntityAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="::LT_S0039" orgTooltip="Add" />
                                            </HeaderTemplate>

                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <iaw:IAWLabel runat="server" ID="grdNoAccessRows" Text="::LT_S0199" orgText="Data Source Access has not been assigned" CssClass="NoRows" />
                                    </EmptyDataTemplate>
                                </iaw:IAWGrid>
                            </ContentTemplate>
                        </iaw:IAWTabPanel>

                    </ajaxToolkit:TabContainer>
                </iaw:IAWPanel>

            </td>
        </tr>
    </table>

    <%-- DataSource Popup Form --%>
    <asp:HiddenField runat="server" ID="hdnDataSourceID" />
    <iaw:IAWPanel ID="popDataSource" runat="server" GroupingText="::LT_S0009" orgGroupingText="Data Source" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDataSourceSave">
        <iaw:wuc_help runat="server" ID="helplink_popDataSource" />

        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel1" runat="server" Text="::LT_S0010" orgText="Name" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox ID="txtShortRef" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="DataSource" />
                    <iaw:IAWRequiredFieldValidator ControlToValidate="txtShortRef" ID="RequiredFieldValidator6" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="DataSource" Display="Dynamic" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel10" runat="server" Text="::LT_S0200" orgText="Zip Password" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox ID="txtZipPassword" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="DataSource" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel30" runat="server" Text="::LT_S0201" orgText="Show Images" />
                </td>
                <td class="formdata">
                    <iaw:IAWCheckBox ID="cbPhotosApplicable" runat="server" />
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataSourceSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="DataSource" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp; &nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataSourceCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>

        </table>

    </iaw:IAWPanel>
    <asp:Button ID="btnDataSourceFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDataSourceForm" BackgroundCssClass="ModalBackground" PopupControlID="popDataSource" TargetControlID="btnDataSourceFake" />

    <%-- Field Popup Form --%>
    <asp:HiddenField runat="server" ID="hdnIdx" />
    <iaw:IAWPanel ID="popField" runat="server" GroupingText="::LT_S0009" orgGroupingText="Data Source" Style="display: none" CssClass="PopupPanel" DefaultButton="btnFieldSave">
        <iaw:wuc_help runat="server" ID="helplink_popField" />

        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel2" runat="server" Text="::LT_S0202" orgText="Field Number" />
                </td>
                <td class="formdata">
                    <iaw:IAWLabel ID="lblFieldNum" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel8" runat="server" Text="::LT_S0203" orgText="Field Name" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtFieldName" MaxLength="50" CssClass="formcontrol" ValidationGroup="Field" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel9" runat="server" Text="::LT_S0204" orgText="Type of Data" />
                </td>
                <td class="formdata">
                    <asp:DropDownList ID="ddlbFieldType" runat="server" AutoPostBack="true" />
                </td>
            </tr>

            <tr runat="server" id="trFieldLength" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="lblFieldLength" runat="server" Text="" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtFieldLength" type="number" CssClass="fieldlength formcontrol" ValidationGroup="Field" />
                    <ajaxToolkit:FilteredTextBoxExtender runat="server" ID="ftbeFieldLength" TargetControlID="txtFieldLength" FilterType="Custom" ValidChars="0123456789" />
                    <iaw:IAWRangeValidator runat="server" ID="rvFieldLength" ControlToValidate="txtFieldLength" MinimumValue="1" MaximumValue="1" Type="Integer" ValidationGroup="Field" Display="Dynamic" />
                </td>
            </tr>

            <tr runat="server" id="trFieldFormat" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel6" runat="server" Text="::LT_S0194" orgText="Format" />
                </td>
                <td class="formdata">
                    <iaw:IAWDropDownList runat="server" ID="ddlbFieldFormat" TranslateText="false">
                        <asp:ListItem Value="" Text="None" />
                        <asp:ListItem Value="dd/MM/yyyy" Text="DD/MM/YYYY" />
                        <asp:ListItem Value="MM/dd/yyyy" Text="MM/DD/YYYY" />
                        <asp:ListItem Value="yyyy/MM/dd" Text="YYYY/MM/DD" />
                        <asp:ListItem Value="dd-MM-yyyy" Text="DD-MM-YYYY" />
                        <asp:ListItem Value="MM-dd-yyyy" Text="MM-DD-YYYY" />
                        <asp:ListItem Value="yyyy-MM-dd" Text="YYYY-MM-DD" />
                    </iaw:IAWDropDownList>
                </td>
            </tr>

            <tr runat="server" id="trItemRefField" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel11" runat="server" Text="::LT_S0167" orgText="Unique Reference" />
                </td>
                <td class="formdata">
                    <iaw:IAWCheckBox runat="server" ID="cbUniqueRef" Checked="false" />
                </td>
            </tr>

            <tr runat="server" id="trParentRefField" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel7" runat="server" Text="::LT_S0205" orgText="Parent Reference" />
                </td>
                <td class="formdata">
                    <iaw:IAWCheckBox ID="cbParentRefField" runat="server" />
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnFieldSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="Field" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp; &nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnFieldCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>

        </table>

    </iaw:IAWPanel>
    <asp:Button ID="btnFieldFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeFieldForm" BackgroundCssClass="ModalBackground" PopupControlID="popField" TargetControlID="btnFieldFake" />

    <%-- Upload Rejection Form --%>
    <iaw:IAWPanel ID="popReject" runat="server" GroupingText="::LT_S0206" orgGroupingText="Reject Import" Style="display: none" CssClass="PopupPanel" DefaultButton="btnRejectProceed">
        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel12" runat="server" Text="::LT_S0009" orgText="Data Source" />
                </td>
                <td class="formdata">
                    <iaw:IAWLabel ID="lblRejectDataSource" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel13" runat="server" Text="::LT_S0207" orgText="Upload Date" />
                </td>
                <td class="formdata">
                    <iaw:IAWLabel ID="lblRejectUploadDate" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel14" runat="server" Text="::LT_S0208" orgText="Uploaded By" />
                </td>
                <td class="formdata">
                    <iaw:IAWLabel ID="lblRejectUploadedBy" runat="server" TranslateText="false" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel15" runat="server" Text="::LT_S0209" orgText="Reason" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtRejectReason" TextMode="MultiLine" Rows="4" MaxLength="500" CssClass="formcontrol" ValidationGroup="Reject" Width="400px" />
                </td>
            </tr>
            <tr class="formrow">
                <td class="formlabel"></td>
                <td class="formdata">
                    <iaw:IAWRequiredFieldValidator ID="rfvRejectReason" ControlToValidate="txtRejectReason" runat="server" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" Display="Dynamic" SetFocusOnError="true" ValidationGroup="Reject" AddBR="true" />
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRejectProceed" Text="::LT_S0210" orgText="Proceed" CausesValidation="true" ValidationGroup="Reject" />
                    <span>&nbsp; &nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRejectCancel" Text="::LT_S0211" orgText="Cancel Rejection" CausesValidation="false" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>
    <asp:Button ID="btnRejectFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeRejectForm" BackgroundCssClass="ModalBackground" PopupControlID="popReject" TargetControlID="btnRejectFake" />

    <%-- Group Access Popup Form --%>
    <asp:HiddenField runat="server" ID="hdnDataSourceAccessID" />
    <iaw:IAWPanel ID="popDataSourceAccess" runat="server" GroupingText="::LT_S0212" orgGroupingText="Data Source Access" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDataSourceAccessSave">
        <iaw:wuc_help runat="server" ID="helplink_popDataSourceAccess" />

        <table border="0" class="listform">
            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel3" runat="server" Text="::LT_S0213" orgText="Type of Access" />
                </td>
                <td class="formdata">
                    <asp:DropDownList ID="ddlbAccessType" runat="server" AutoPostBack="true" />
                </td>
            </tr>

            <%-- 01 Role --%>

            <tr runat="server" id="trRoles" class="formrow" visible="false">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel5" runat="server" Text="::LT_S0179" orgText="User" />
                </td>
                <td class="formdata">
                    <asp:DropDownList ID="ddlbAccessRole" runat="server" CssClass="formcontrol" ValidationGroup="Destination" />
                </td>
            </tr>

            <%-- 02 Specific User Search --%>

            <tr runat="server" id="trSearchUser" visible="false">
                <td colspan="2">
                    <br />
                    <iaw:IAWLabel ID="IAWLabel4" runat="server" Text="::LT_S0214" orgText="Enter part of their name" />
                    <br />
                    <iaw:IAWTextbox runat="server" ID="txtSearchUser" MaxLength="20" />
                    <ajaxToolkit:FilteredTextBoxExtender ID="fteSearch" runat="server" TargetControlID="txtSearchUser" FilterType="LowercaseLetters, UppercaseLetters, Numbers, Custom" ValidChars="-" />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnSearchUser" Text="::LT_S0215" orgText="Go" CausesValidation="false" />
                    <iaw:IAWLabel ID="lblUserExists" runat="server" Text="::LT_S0216" orgText="<br />User already added" ForeColor="red" Visible="false" />

                    <iaw:IAWPanel ID="pnlUserList" runat="server" Style="overflow: scroll; height: 300px;">
                        <iaw:IAWGrid runat="server" ID="grdSearchUser" AutoGenerateColumns="False" DataKeyNames="user_ref" RowStyle-Wrap="false" editState="Normal" SaveButtonId="" TranslateHeadings="True">
                            <Columns>
                                <asp:BoundField DataField="user_ref" Visible="false" />
                                <iawb:IAWBoundField DataField="name" HeaderText="::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField" />
                            </Columns>
                            <EmptyDataTemplate>
                                <iaw:IAWLabel runat="server" ID="grdSearchUserNoRows" Text="::LT_S0217" orgText="The search didn't find any people" CssClass="NoRows" />
                            </EmptyDataTemplate>
                        </iaw:IAWGrid>
                    </iaw:IAWPanel>
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataSourceAccessSave" Text="::LT_S0039" orgText="Add" CausesValidation="true" ValidationGroup="DataSourceAccess" IconClass="iconButton fa-solid fa-circle-plus" />
                    <span>&nbsp; &nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataSourceAccessCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>

        </table>

    </iaw:IAWPanel>
    <asp:Button ID="btnDataSourceAccessFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDataSourceAccessForm" BackgroundCssClass="ModalBackground" PopupControlID="popDataSourceAccess" TargetControlID="btnDataSourceAccessFake" />

    <%-- Delete Confirm Popup --%>
    <iaw:IAWPanel ID="popDelete" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDeleteCancel">
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
                    <span>&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteCancel" Text="::LT_S0034" orgText="No" CausesValidation="false" ToolTip="::LT_S0161" orgTooltip="Do not delete entry" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <%-- Ignore primary data column --%>
    <iaw:IAWPanel ID="popIgnorePrimary" runat="server" GroupingText="::LT_S0218" orgGroupingText="Ignore Primary Source" Style="display: none" CssClass="PopupPanel" DefaultButton="btnIgnorePrimarySave">
        <iaw:wuc_help runat="server" ID="helplink_popIgnorePrimary" />

        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="lblIgnore" runat="server" Text="::LT_S0219" orgText="This data source has a unique column, are you sure that you <br /> want to ignore it and make this a secondary data source ?" />
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnIgnorePrimarySave" Text="::LT_S0033" orgText="Yes" CausesValidation="true" ValidationGroup="IgnorePrimary" IconClass="iconButton fa-solid fa-circle-plus" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnIgnorePrimaryCancel" Text="::LT_S0034" orgText="No" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>

        </table>

    </iaw:IAWPanel>
    <asp:Button ID="btnIgnorePrimaryFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeIgnorePrimaryForm" BackgroundCssClass="ModalBackground" PopupControlID="popIgnorePrimary" TargetControlID="btnIgnorePrimaryFake" />

</asp:Content>
