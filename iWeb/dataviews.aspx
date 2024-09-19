<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master"
    CodeBehind="dataviews.aspx.vb" Inherits=".dataviews" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">

    <link rel="stylesheet" href="Library/lineAttr.css" />
    <script src="Library/lineAttr.js"></script>
    <script src="js/rgbfilter.js"></script>
    <script src="client_settings.js"></script>
    <script src="dataviews.js"></script>

    <style type="text/css">
        fieldset {
            min-width: 210px !important;
        }
            fieldset table {
                width: 100%;
            }

        .grdField {
            padding: 9px 14px;
            text-align: left;
            white-space: nowrap;
            vertical-align: middle;
        }

        .divScroll {
            overflow-y: auto;
        }

        .spacing {
            padding: 2px;
            margin-bottom: 2px;
        }

        .NoRows {
            padding: 3px 5px 3px 5px;
            text-align: left;
            white-space: nowrap;
            vertical-align: middle;
        }

        .formlabel {
            padding-right: 2px !important;
            padding-left: 2px !important;
        }

        .formrow {
            height: 30px !important;
            padding-top: 0px !important;
            padding-bottom: 0px !important;
        }

        .draggable {
            cursor: move;
            color: var(--draggable-color);
            background: var(--draggable-bg-color);
            border: 2px outset var(--draggable-border-color);
            outline: none;
        }
            .draggable:hover {
                color: var(--draggable-hover-color);
                background: var(--draggable-hover-bg-color);
                border: 2px outset var(--draggable-hover-border-color);
            }

        .dragOver {
            position: relative;
        }
            .dragOver::before {
                content: "";
                display: block;
                height: 10px;
                background: linear-gradient(to bottom, grey, transparent);
                position: absolute;
                width: 100%;
                top: 0;
                left: 0;
            }

        .topSpacer,
        .bottomSpacer {
            min-height: 10px;
            height: 10px;
        }
            .bottomSpacer td {
                border-bottom: 1px solid #c0c0c0;
            }

        .formdata td:first-child {
            width: 152px;
        }
        #tcMainContainer {
            min-height:325px;
            min-width: 625px;
        }
        #tcMainContainer_body {
            height: 325px;
            display: flex !important;
            justify-content: center;
        }

    </style>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <table border="0">
        <tr>
            <td class="top">
                <iaw:IAWPanel runat="server" ID="pnlDataSource" GroupingText="::LT_S0009" orgGroupingText="Data Source" CssClass="PanelClass">
                    <div id="divDatasources" class="divScroll grid-to-bottom">
                        <iaw:IAWGrid runat="server" ID="grdDatasource" AutoGenerateColumns="False" DataKeyNames="source_id" editState="Normal" SaveButtonId="" TranslateHeadings="True" ClientIDMode="Static">
                            <Columns>
                                <iawb:IAWBoundField DataField="short_ref" HeaderText="::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField" />
                            </Columns>
                        </iaw:IAWGrid>
                    </div>
                </iaw:IAWPanel>
            </td>
            <td class="top">
                <iaw:IAWPanel runat="server" ID="pnlDataView" GroupingText="::LT_S0221" orgGroupingText="Data View" CssClass="PanelClass">
                    <div id="divViews" class="divScroll grid-to-bottom">
                        <iaw:IAWGrid runat="server" ID="grdDataView" AutoGenerateColumns="False" DataKeyNames="view_id" ShowHeaderWhenEmpty="true" editState="Normal" SaveButtonId="" TranslateHeadings="True" ClientIDMode="Static">
                            <Columns>
                                <iawb:IAWBoundField DataField="view_ref" HeaderText="::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField" />
                                <iawb:IAWBoundField DataField="data_effective" HeaderText="::LT_S0191" orgHeaderText="Date" ReadOnly="True" ItemStyle-CssClass="grdField" DataFormatString="{0:dd/MM/yyyy HH:mm}" />

                                <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                    <HeaderTemplate>
                                        <iaw:IAWImageButton runat="server" ID="btnDataViewAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="::LT_S0039" orgTooltip="Add" />
                                    </HeaderTemplate>

                                    <ItemTemplate>
                                        <div class="ActionButtons">
                                            <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
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
                                <iaw:IAWLabel runat="server" ID="grdSetRows" Text="::LT_S0222" orgText="Please add a Data View" CssClass="NoWrap" />
                            </EmptyDataTemplate>

                            <RowStyle Wrap="False" />
                            <HeaderStyle Wrap="false" />
                        </iaw:IAWGrid>
                        <br />
                        <iaw:IAWLabel runat="server" ID="lblDataViewMsg" Visible="False" CssClass="NoRows" ForeColor="Red" />
                    </div>
                </iaw:IAWPanel>
            </td>
            <td class="top">

                <iaw:IAWPanel runat="server" ID="pnlDisplay" GroupingText="::LT_S0223" orgGroupingText="Display" ClientIDMode="Static">
                    <asp:HiddenField ID="hdnDrop" runat="server" ClientIDMode="Static" />
                    <asp:Button ID="btnDrop" runat="server" ClientIDMode="Static" Style="display: none;" />

                    <asp:Panel runat="server" ID="divAccButtons" ClientIDMode="Static" Visible="false">
                        <iaw:IAWHyperLinkButton runat="server" ID="btnAccSave" CssClass="fa-regular fa-floppy-disk banner-icon flasher" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0005" orgTooltip="Save" />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnAccCancel" CssClass="fa-solid fa-arrow-rotate-left banner-icon flasher" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0022" orgTooltip="Reset" />
                    </asp:Panel>

                    <table border="0">
                        <tr>
                            <td class="top">
                                <div id="divFields" class="divScroll grid-to-bottom">
                                    <iaw:IAWGrid runat="server" ID="grdFields" AutoGenerateColumns="False" DataKeyNames="field_num" editState="Normal" SaveButtonId="" AllowSorting="true" TranslateHeadings="True" ClientIDMode="Static">
                                        <Columns>
                                            <iawb:IAWBoundField DataField="field_num" HeaderText="::LT_S0192" orgHeaderText="#" ReadOnly="True" SortExpression="field_num" ItemStyle-CssClass="grdField center draggable" />
                                            <iawb:IAWBoundField DataField="field_name" HeaderText="::LT_S0224" orgHeaderText="Field" ReadOnly="True" SortExpression="field_name" ItemStyle-CssClass="grdField left draggable" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <iaw:IAWLabel runat="server" ID="grdNoModelDataSourceRows" Text="::LT_S0225" orgText="There are no fields defined" CssClass="NoWrap" />
                                        </EmptyDataTemplate>
                                    </iaw:IAWGrid>
                                </div>
                            </td>

                            <td class="top">
                                <asp:HiddenField runat="server" ID="hdnAccValue" ClientIDMode="Static" Value="OCA" />
                                <asp:Button ID="btnAccPanel" runat="server" Text="Hidden Button" ClientIDMode="Static" Style="display: none;" />

                                <div id="divDisplay" class="divScroll grid-to-bottom">
                                    <ajaxToolkit:Accordion ID="accPanel" runat="server" ClientIDMode="Static" HeaderCssClass="listheader left cur-pointer spacing" HeaderSelectedCssClass="listheader left cur-pointer spacing" FadeTransitions="true" TransitionDuration="100" AutoSize="None" SelectedIndex="0">
                                        <Panes>
                                            <ajaxToolkit:AccordionPane ID="accPanelOCA" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('OCA')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0227" orgText="Node Details" />
                                                            </td>
                                                        </tr>
                                                    </table>

                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdDispOCA" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="line_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>

                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelCoParent" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('CoParent')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0228" orgText="Co Parent Node Details" />
                                                            </td>
                                                        </tr>
                                                    </table>

                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdCoParent" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="line_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                                                        <iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelAEAHeader" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('AEAHead')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0229" orgText="Selection List Headers" />
                                                            </td>
                                                            <td class="right">
                                                                <iaw:IAWHyperLinkButton runat="server" ID="btnAEAHeader" CssClass="iaw-font iaw-font iaw-new-column" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0256" orgTooltip="Add Column" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdAEAHead" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>

                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelAEAFields" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('AEAFields')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0230" orgText="Selection List Fields" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdAEAFields" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelAEASort" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('AEASort')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0231" orgText="Selection List Sort" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdAEASort" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelFormDisplay" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('FormDisplay')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0232" orgText="Detail Form Display" />
                                                            </td>
                                                            <td class="right">
                                                                <iaw:IAWHyperLinkButton runat="server" ID="btnFormDispRow" CssClass="iaw-font iaw-new-row" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0226" orgTooltip="Add Line" />
                                                                <iaw:IAWHyperLinkButton runat="server" ID="btnFormDispCol" CssClass="iaw-font iaw-new-column" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0256" orgTooltip="Add Column" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdFormDisplay" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" HeaderStyle-CssClass="iaw-font iaw-column" />
                                                            <asp:BoundField DataField="line_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" HeaderStyle-CssClass="iaw-font iaw-row" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelListHeader" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('ListHead')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0233" orgText="Detail List Column Headers" />
                                                            </td>
                                                            <td class="right">
                                                                <iaw:IAWHyperLinkButton runat="server" ID="btnListHead" CssClass="iaw-font iaw-new-column" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0256" orgTooltip="Add Column" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdListHeaders" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelListData" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('ListData')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0234" orgText="Detail List Column Data" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdListData" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="col_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>

                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>

                                            <ajaxToolkit:AccordionPane ID="accPanelListSort" runat="server">
                                                <Header>
                                                    <table border="0" style="width: 100%">
                                                        <tr onclick="accPostback('ListSort')">
                                                            <td class="left">
                                                                <iaw:IAWLabel runat="server" Text="::LT_S0235" orgText="Detail List Column Sort" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </Header>
                                                <Content>
                                                    <iaw:IAWGrid runat="server" ID="grdListSort" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_col,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                                        <Columns>
                                                            <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                                <ItemTemplate>
                                                                    <div class="ActionButtons">
                                                                        <div></div>
                                                                        <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                                    </div>
                                                                    <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
                                                                    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
                                                                </ItemTemplate>
                                                                <ItemStyle HorizontalAlign="Center" />
                                                            </asp:TemplateField>
                                                        </Columns>
                                                    </iaw:IAWGrid>
                                                </Content>
                                            </ajaxToolkit:AccordionPane>
                                        </Panes>
                                    </ajaxToolkit:Accordion>
                                </div>
                            </td>
                        </tr>
                    </table>

                </iaw:IAWPanel>

            </td>
        </tr>
    </table>

    <%-- Hidden fields to contain keys --%>

    <asp:HiddenField runat="server" ID="hdnViewID" />

    <%-- Data View Popup Form --%>

    <iaw:IAWPanel runat="server" ID="popDataView" ClientIDMode="Static" GroupingText="::LT_S0221" orgGroupingText="Data View" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDataViewSave">
        <iaw:wuc_help runat="server" ID="helplink_popDataView" />

        <ajaxToolkit:TabContainer runat="server" ID="tcMainContainer" ClientIDMode="Static" CssClass="iaw" OnClientActiveTabChanged="ViewTabChanged">
            <iaw:IAWTabPanel runat="server" ID="tpGeneral" HeaderText="::LT_S0044" orgHeaderText="General">
                <ContentTemplate>

                    <table border="0" class="listform">

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel1" runat="server" Text="::LT_S0009" orgText="Data Source" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList runat="server" ID="ddlbDataSource" AutoPostBack="true" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel3" runat="server" Text="::LT_S0236" orgText="Occurrence Date" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList runat="server" ID="ddlbOccurrenceDate" AutoPostBack="true" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel2" runat="server" Text="::LT_S0010" orgText="Name" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox ID="txtReference" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="DataView" />
                                <iaw:IAWRequiredFieldValidator ControlToValidate="txtReference" ID="RequiredFieldValidator2" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="DataView" Display="Dynamic" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trTxtAEA">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel10" runat="server" Text="::LT_S0237" orgText="Selection List Title" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox ID="txtAEA" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="DataView" />
                                <iaw:IAWRequiredFieldValidator ControlToValidate="txtAEA" ID="RequiredFieldValidator1" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="DataView" Display="Dynamic" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trShowDetailsType">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel32" runat="server" Text="::LT_S0238" orgText="Show Further Details" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList ID="ddlbShowDetailsType" runat="server" AutoPostBack="true" OnClientClick="return RestringData();" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trLinkedDataSource">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel45" runat="server" Text="::LT_S0239" orgText="Related Data Source" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList ID="ddlbDetailsDataSource" runat="server" AutoPostBack="true" OnClientClick="return RestringData();" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trDetailFieldFrom">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel46" runat="server" Text="::LT_S0240" orgText="Field in this Data Source" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList ID="ddlbDetailFieldFrom" runat="server" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trDetailFieldTo">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel47" runat="server" Text="::LT_S0241" orgText="Field in related Data Source" />
                            </td>
                            <td class="formdata">
                                <asp:DropDownList ID="ddlbDetailFieldTo" runat="server" />
                            </td>
                        </tr>

                        <tr class="formrow" runat="server" id="trAllowDrilldown">
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel7" runat="server" Text="::LT_S0242" orgText="Allow Drilldown" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWCheckBox ID="cbAllowDrilldown" runat="server" />
                                <span>&nbsp;</span>
                                <iaw:IAWLabel ID="IAWLabel24" runat="server" Text="::LT_S0243" orgText="('by Display Only Users')" />
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpChart" HeaderText="::LT_S0105" orgHeaderText="Chart">
                <ContentTemplate>

                    <asp:HiddenField runat="server" ID="hdnAttrib" ClientIDMode="Static" />
                    <asp:DropDownList runat="server" ID="ddlbFont" ClientIDMode="static" CssClass="hidden" />

                    <table border="0">
                        <tbody>
                            <tr class="formrow">
                                <td colspan="2" class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel14" Text="::LT_S0106" orgText="These Settings may be overridden" />
                                    <iaw:IAWCheckBox ID="cbSettigsFixed" runat="server" />
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel34" Text="::LT_S0107" orgText="Background Type" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList ID="ddlbBackgroundType" runat="server" ClientIDMode="Static" onchange="backgroundTypeChanged()" TranslateText="false" />
                                </td>
                            </tr>

                            <tr class="formrow" id="trBackgroundColour">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel9" Text="::LT_S0056" orgText="Background Colour" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWTextbox runat="server" ID="txtModelbg" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="Background Colour" />
                                </td>
                            </tr>

                            <tr class="formrow" id="trBackgroundImage">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel35" Text="::LT_S0108" orgText="Background Image" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList ID="ddlbBackgroundImage" runat="server" ClientIDMode="Static" TranslateText="false" />
                                </td>
                            </tr>

                            <tr class="formrow" id="trBackgroundGradient">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel4" Text="::LT_S0109" orgText="Background Gradient" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList ID="ddlbBackgroundGradient" runat="server" ClientIDMode="Static" TranslateText="false" />
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel5" Text="::LT_S0110" orgText="Chart Direction" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList runat="server" ID="ddlbDirection" ClientIDMode="Static" TranslateText="true" >
                                        <%-- 
                                            LT_S0111 Top Down 
                                            LT_S0112 Left to Right 
                                            LT_S0113 Bottom Up 
                                            LT_S0114 Right to Left 
                                        --%> 
                                        <asp:ListItem Value="90" Text="::LT_S0111" title="::LT_S0111" Selected="true" />
                                        <asp:ListItem Value="0" Text="::LT_S0112" title="::LT_S0112" />
                                        <asp:ListItem Value="270" Text="::LT_S0113" title="::LT_S0113" />
                                        <asp:ListItem Value="180" Text="::LT_S0114" title="::LT_S0114" />
                                    </iaw:IAWDropDownList>
                                </td>
                            </tr>

                            <tr class="formrow" runat="server" id="trPhotosApplicable">
                                <td class="formlabel">
                                    <iaw:IAWCheckBox ID="cbPhotosApplicable" runat="server" Text="::LT_S0201" orgText="Show Images" TextAlign="Left" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList runat="server" ID="ddlbImagePosition" ClientIDMode="Static" TranslateText="true">
                                        <%-- 
                                            LT_A0117 Inline
                                            LT_A0118 Image above Left
                                            LT_A0119 Image above Centre
                                            LT_A0120 Image above Right
                                        --%> 
                                        <asp:ListItem Value="inline" Text="::LT_A0117" title="::LT_A0117" Selected="true" />
                                        <asp:ListItem Value="left" Text="::LT_A0118" title="::LT_A0118" />
                                        <asp:ListItem Value="centre" Text="::LT_A0119" title="::LT_A0119" />
                                        <asp:ListItem Value="right" Text="::LT_A0120" title="::LT_A0120" />
                                    </iaw:IAWDropDownList>
                                </td>
                            </tr>

                        </tbody>

                        <tbody><tr><td colspan="2" class="spacer-line"></td></tr></tbody>

                        <tbody class="group-box">
                            <tr class="group-box-title-row">
                                <td colspan="2">
                                    <iaw:IAWLabel runat="server" ID="lblRelationshipLinesTitle" Text="::LT_S0116" orgText="Relationship Lines" CssClass="group-box-title-text" />
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel75" Text="::LT_S0117" orgText="Colours" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWTextbox runat="server" ID="txtlinkColour" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-minus" ToolTip="::LT_S0086" orgTooltip="Colour" />
                                    <span>&nbsp;</span>
                                    <iaw:IAWTextbox runat="server" ID="txtLinkHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgTooltip="Hover Colour" />
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel13" Text="::LT_S0118" orgText="Tooltip Colours" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipfg" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                    <span>&nbsp;</span>
                                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipbg" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                    <span>&nbsp;</span>
                                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipBorder" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel76" Text="::LT_S0119" orgText="Width" />
                                </td>
                                <td class="formdata">
                                    <table border="0">
                                        <tr>
                                            <td>
                                                <iaw:IAWTextbox runat="server" ID="numLinkWidth" />
                                                <ajaxToolkit:SliderExtender ID="slideLinkWidth" runat="server" TargetControlID="numLinkWidth" Minimum="1" Maximum="5" Steps="9" Decimals="1" BoundControlID="labLinkWidth" />
                                            </td>
                                            <td class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="labLinkWidth" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <tr class="formrow">
                                <td class="formlabel">
                                    <iaw:IAWLabel runat="server" ID="IAWLabel77" Text="::LT_S0120" orgText="Style" />
                                </td>
                                <td class="formdata">
                                    <iaw:IAWDropDownList runat="server" ID="ddlbLinkStyle" Font-Names="monospace" TranslateText="false">
                                        <asp:ListItem Value="solid" Text="───────────" />
                                        <asp:ListItem Value="dotted" Text="─ ─ ─ ─ ─ ─" />
                                        <asp:ListItem Value="dashes" Text="─── ─── ───" />
                                    </iaw:IAWDropDownList>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpNodes" HeaderText="::LT_S0121" orgHeaderText="Nodes">
                <ContentTemplate>
                    <table border="0">

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel16" Text="::LT_S0117" orgText="Colours" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtNodefg" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeTxtBg" CssClass="middle SpectrumColourEmpty" ClientIDMode="Static" data-class="fa-solid fa-font white-on-black" ToolTip="::LT_S0122" orgTooltip="Text Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodebg" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeBorder" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td colspan="2" class="formlabel">
                                <iaw:IAWCheckBox ID="cbNodeTxtBlock" runat="server" onchange="change_Charts()" Text="::LT_S0123" orgText="Extend Text Background to the width of the Node" TextAlign="Left" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel31" Text="::LT_S0124" orgText="Highlight" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtHighlightfg" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtHighlightbg" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtHighlightbBorder" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel72" Text="::LT_S0125" orgText="Tooltip" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTTfg" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtTTbg" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtTTBorder" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWCheckBox ID="cbShowShadow" data-target="#txtShadow" runat="server" ClientIDMode="Static"  TextAlign="Left" Text="::LT_S0127" orgText="Show Shadow" CssClass="middle" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtShadow" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-cube" ToolTip="::LT_S0128" orgTooltip="Shadow Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel33" Text="::LT_S0088" orgText="Icon Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtIconfg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0088" orgTooltip="Icon Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtIconHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_A0066" orgToolTip="Icon Hover" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel73" Text="::LT_S0129" orgText="Corners" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbCornerRectangle" runat="server" Text="::LT_S0130" orgText="Square" GroupName="boxNodeCorners" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbCornerRoundedRectangle" runat="server" Text="::LT_S0131" orgText="Rounded" GroupName="boxNodeCorners" ClientIDMode="Static" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel8" Text="::LT_S0132" orgText="Max Width" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numMaxNodeWidth" AutoPostBack="true" />
                                            <ajaxToolkit:SliderExtender ID="slideMaxNodeWidth" runat="server" TargetControlID="numMaxNodeWidth" Minimum="100" Maximum="500" Steps="41" BoundControlID="labMaxNodeWidth" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labMaxNodeWidth" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel11" Text="::LT_S0133" orgText="Default Width" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodeWidth" />
                                            <ajaxToolkit:SliderExtender ID="slideNodeWidth" runat="server" TargetControlID="numNodeWidth" Minimum="100" Maximum="500" Steps="41" BoundControlID="labNodeWidth" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodeWidth" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel12" Text="::LT_S0134" orgText="Max Height" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numMaxNodeHeight" AutoPostBack="true" />
                                            <ajaxToolkit:SliderExtender ID="slideMaxNodeHeight" runat="server" TargetControlID="numMaxNodeHeight" Minimum="50" Maximum="200" Steps="16" BoundControlID="labMaxNodeHeight" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labMaxNodeHeight" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel19" Text="::LT_S0135" orgText="Default Height" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodeHeight" />
                                            <ajaxToolkit:SliderExtender ID="slideNodeHeight" runat="server" TargetControlID="numNodeHeight" Minimum="50" Maximum="200" Steps="16" BoundControlID="labNodeHeight" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodeHeight" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>  
                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpFont" HeaderText="::LT_S0136" orgHeaderText="Text Font">
                <ContentTemplate>
                    <table class="tblStyle">
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel17" Text="::LT_S0137" orgText="Lines" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine1"></label>
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel18" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine2"></label>
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel20" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine3" />
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel21" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine4" />
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel22" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine5" />
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel23" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblNodeLine6" />
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
        </ajaxToolkit:TabContainer>
        <table border="0">
            <tr>
                <td>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataViewSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="DataView" OnClientClick="return RestringData();" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDataViewCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>

    </iaw:IAWPanel>

    <asp:Button ID="btnDataViewFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDataViewForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDataView" TargetControlID="btnDataViewFake" />

    <%-- View Display Entry Popup --%>

    <asp:HiddenField runat="server" ID="hdnDispKey" />
    <asp:HiddenField runat="server" ID="hdnFieldType" ClientIDMode="Static" />

    <iaw:IAWPanel ID="popDisplayEntry" runat="server" GroupingText="::LT_S0221" orgGroupingText="Data View" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDisplayEntrySave">
        <iaw:wuc_help runat="server" ID="helplink_popDisplayEntry" />

        <table border="0" class="listform">
            <tr runat="server" id="trDEtext" class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel48" runat="server" Text="::LT_S0064" orgText="Text" />
                </td>
                <td class="formdata middle selDEtext">
                    <iaw:IAWTextbox ID="txtDEtext" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol seltxtDEtext" ValidationGroup="DisplayEntry" />
                </td>
            </tr>

            <tr runat="server" id="trDEtextHover" class="formrow seltrDEtextHover">
                <td>&nbsp;</td>
                <td class="formdata middle">
                    <iaw:IAWLabel ID="lblDEtextHover" runat="server" CssClass="sellblDEtextHover cur-pointer" Text="::LT_A0115" orgText="Hover here to show the spaces" TranslateText="false" />
                </td>
            </tr>

            <tr runat="server" id="trDECase" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel6" runat="server" Text="::LT_A0004" orgText="Case" />
                </td>
                <td class="formdata">
                    <iaw:IAWCheckBox runat="server" ID="cbDEUpper" data-group="DECase" Text="::LT_A0005" orgText="Upper" ClientIDMode="Static" />
                    <iaw:IAWCheckBox runat="server" ID="cbDELower" data-group="DECase" Text="::LT_A0008" orgText="Lower" ClientIDMode="Static" />
                    <iaw:IAWCheckBox runat="server" ID="cbDECapitalise" data-group="DECase" Text="::LT_A0088" orgText="Capitalise" ClientIDMode="Static" />
                </td>
            </tr>

            <tr runat="server" id="trDEPart" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel15" runat="server" Text="::LT_A0089" orgText="Use Part" />
                </td>
                <td class="formdata">
                    <iaw:IAWCheckBox runat="server" ID="cbDEPartLeft"  data-group="DEPart" Text="::LT_A0090" orgText="Left"   ClientIDMode="Static" />
                    <iaw:IAWCheckBox runat="server" ID="cbDEPartMid"   data-group="DEPart" Text="::LT_A0091" orgText="Middle" ClientIDMode="Static" />
                    <iaw:IAWCheckBox runat="server" ID="cbDEPartRight" data-group="DEPart" Text="::LT_A0092" orgText="Right"  ClientIDMode="Static" />
                    <asp:Panel runat="server" ID="pnlDEStart" style="display:inline" ClientIDMode="static">
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWLabel runat="server" ID="lblDEStart" Text="::LT_A0093" orgText="Start" />
                        <iaw:IawTextBox runat="server" ID="txtDEStart" TextMode="Number" min="1" max="100" step="1" ClientIDMode="Static" Width="20px" />
                        <ajaxToolkit:FilteredTextBoxExtender runat="server" ID="fteDEStart" TargetControlID="txtDEStart" FilterType="Numbers" />
                    </asp:Panel>
                    <asp:Panel runat="server" ID="pnlDELength" style="display:inline" ClientIDMode="static">
                        <iaw:IAWLabel runat="server" ID="lblDELength" Text="::LT_S0193" orgText="Length" />
                        <iaw:IawTextBox runat="server" ID="txtDELength" TextMode="Number" min="1" max="100" step="1" ClientIDMode="Static" Width="20px" />
                        <ajaxToolkit:FilteredTextBoxExtender runat="server" ID="fteDELength" TargetControlID="txtDELength" FilterType="Numbers" />
                    </asp:Panel>
                </td>
            </tr>

            <tr runat="server" id="trDEDateFormat" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel29" runat="server" Text="::LT_A0097" orgText="Date Format" />
                </td>
                <td class="formdata">
                    <iaw:IAWDropDownList runat="server" ID="ddlbDEDateFormat" TranslateText="True" />
<%--                    <iaw:IAWDropDownList runat="server" ID="ddlbDEDateFormat" TranslateText="false">
                        <asp:ListItem Value="dd/MM/yyyy" Text="DD/MM/YYYY" />
                        <asp:ListItem Value="MM/dd/yyyy" Text="MM/DD/YYYY" />
                        <asp:ListItem Value="yyyy/MM/dd" Text="YYYY/MM/DD" />
                        <asp:ListItem Value="dd-MM-yyyy" Text="DD-MM-YYYY" />
                        <asp:ListItem Value="MM-dd-yyyy" Text="MM-DD-YYYY" />
                        <asp:ListItem Value="yyyy-MM-dd" Text="YYYY-MM-DD" />
                    </iaw:IAWDropDownList>--%>
                </td>
            </tr>

            <tr runat="server" id="trDEcurrency" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel25" runat="server" Text="::LT_A0106" orgText="Currency" />
                </td>
                <td class="formlabel">
                    <iaw:IAWCheckBox runat="server" ID="cbDEcurrency" TextAlign="Left" ClientIDMode="Static" />
                </td>
            </tr>

            <tr runat="server" id="trDEdecs" class="formrow nonCurrency">
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel26" Text="::LT_A0102" orgText="Number of decimals" />
                </td>
                <td class="formdata">
                    <iaw:IawTextBox runat="server" ID="txtDEdecs" TextMode="Number" min="0" max="6" step="1" ClientIDMode="Static" Width="20px" />
                    <ajaxToolkit:FilteredTextBoxExtender runat="server" ID="FilteredTextBoxExtender1" TargetControlID="txtDEdecs" FilterType="Numbers" />
                </td>
            </tr>

            <tr runat="server" id="trDEgroup" class="formrow nonCurrency">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel27" runat="server" Text="::LT_A0103" orgText="Show thousands separator" />
                </td>
                <td class="formlabel" colspan="2">
                    <iaw:IAWCheckBox runat="server" ID="cbDEgroup" TextAlign="Left" ClientIDMode="Static" />
                </td>
            </tr>

            <tr runat="server" id="trDEbool" class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel28" runat="server" Text="::LT_A0114" orgText="Boolean Format" />
                </td>
                <td class="formdata">
                    <iaw:IAWDropDownList runat="server" ID="ddlbDEbool" TranslateText="false" />
                </td>
            </tr>
        </table>

        <table border="0">
            <tr>
                <td>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDisplayEntrySave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="DisplayEntry" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnDisplayEntryCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>

    </iaw:IAWPanel>

    <asp:Button ID="btnDisplayEntryFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDisplayEntryForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popDisplayEntry" TargetControlID="btnDisplayEntryFake" />

    <%-- display message --%>
    <iaw:IAWPanel ID="popMessage" runat="server" GroupingText="::LT_S0221" orgGroupingText="Data View" Style="display: none" CssClass="PopupPanel" DefaultButton="btnMessageOk">
        <table border="0">
            <tr>
                <td>
                    <iaw:IAWLabel ID="lblMessage" runat="server" />
                </td>
            </tr>
            <tr>
                <td>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnMessageOk" Text="::LT_S0255" orgText="Close" CausesValidation="true" ValidationGroup="Message" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnMessageFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeMessageForm" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popMessage" TargetControlID="btnMessageFake" />

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

</asp:Content>
