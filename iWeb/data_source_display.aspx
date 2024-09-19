<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="data_source_display.aspx.vb" Inherits=".data_source_display" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
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
            min-height: 325px;
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

    </style>

    <script>
        $(document).ready(function () {
            ApplyColourControls(".SpectrumColour", false, 'vcPalette');

            // enable / disable the shadow colour box based on 'show shadow checkbox'
            var isChecked = $('#cbDetailShowShadow').is(':checked');
            toggleColorPicker(isChecked);

            // On 'show shadow' checkbox change
            $('#cbDetailShowShadow').change(function () {
                var isChecked = $(this).is(':checked');
                toggleColorPicker(isChecked);
            });

            // divs originally hidden, so now we have the page drawn, show them
            $('.divScroll').show();

        });

        function toggleColorPicker(isChecked) {
            $("#txtDetailShadow").spectrum(isChecked ? 'enable' : 'disable');
        }

        function ViewTabChanged() {
            $find('mpeDataViewForm')._layout();
        }

        function accPostback(name) {
            // if going between listhead and listdata then we don't need to postback
            //  or, if we are going between a non list and another non-list then we don't need to postback

            var accTypes = ["ListHead", "ListData", "ListSort"];
            var fromList = accTypes.includes($ID("hdnAccValue").value);
            var toList = accTypes.includes(name);

            $ID("hdnAccValue").value = name;

            // unless switching to and from a related data source, don't postback'
            if ((fromList && toList) || (!fromList && !toList))
                return;

            $ID('btnAccPanel').click();
        }

        let dragged;
        let dragSource;

        function dragStart(e) {
            dragged = e.target;
            let rowIndex = e.target.rowIndex - 1;
            e.dataTransfer.setData('rowIndex', rowIndex.toString());
            //e.dataTransfer.setData("source", "field");
            dragSource = "field";
        }
        function dragSortStart(e) {
            dragged = e.target;
            let rowIndex = e.target.parentElement.rowIndex;
            e.dataTransfer.setData('rowIndex', rowIndex.toString());

            // Get the 'data-source' attribute value of the cell
            let dataSource = e.target.getAttribute("data-source");

            // Set it as the 'source' data for the drag operation
            //e.dataTransfer.setData("source", dataSource);
            dragSource = dataSource;
        }

        function allowDrop(e) {
            e.preventDefault();
            if (!dragged) return;
            let trElement = e.target;
            while (trElement.tagName.toLowerCase() !== 'tr') {
                trElement = trElement.parentElement;
            }
            // prohibit drops from field to sequence rows where the type is for headers
            let dragTarget = trElement.getAttribute("data-target");

            if (dragSource == "field" && dragTarget == "sort") return;

            // Add the dragOver class to each cell in the row
            for (let i = 0; i < trElement.cells.length - 1; i++) {
                trElement.cells[i].classList.add('dragOver');
            }
        }

        function dragLeave(e) {
            if (!dragged) return;
            let trElement = e.target;
            while (trElement.tagName.toLowerCase() !== 'tr') {
                trElement = trElement.parentElement;
            }

            // Remove the dragOver class from each cell in the row
            for (let i = 0; i < trElement.cells.length - 1; i++) {
                trElement.cells[i].classList.remove('dragOver');
            }
        }

        function drop(e) {
            e.preventDefault();
            if (dragged) {
                let trElement = e.target;
                while (trElement.tagName.toLowerCase() !== 'tr') {
                    trElement = trElement.parentElement;
                }

                // Remove the dragOver class from each cell in the row
                for (let i = 0; i < trElement.cells.length; i++) {
                    trElement.cells[i].classList.remove('dragOver');
                }

                let rowIndexSource = parseInt(e.dataTransfer.getData('rowIndex'));
                //let dragSource = e.dataTransfer.getData('source');

                let rowIndexTarget = trElement.rowIndex;

                // Populate the hidden field and trigger the hidden button
                $ID('hdnDrop').value = dragSource + '|' + rowIndexSource + '|' + rowIndexTarget;
                $ID('btnDrop').click();
                dragged = null;
            }
        }

        // this line is added to remove `detail` tab_pannel that is visible even when `chart` tab_pannel is open
        // remove this line when bug is fixed
        //setTimeout(() => { document.querySelector('.detail_tab').style.display = 'none' }, 500);
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <iaw:IAWPanel runat="server" ID="pnlDisplay" GroupingText="::LT_S0223" orgGroupingText="Display" ClientIDMode="Static">
        <iaw:IAWHyperLinkButton runat="server" ID="btnBack" ToolTip="::LT_M0025" orgToolTip="Back to list" CssClass="backButton" IconClass="iconButton fa-solid fa-arrow-left" />

        <asp:HiddenField ID="hdnDrop" runat="server" ClientIDMode="Static" />
        <asp:Button ID="btnDrop" runat="server" ClientIDMode="Static" Style="display: none;" />

        <asp:Panel runat="server" ID="divAccButtons" ClientIDMode="Static" Visible="false">
            <iaw:IAWHyperLinkButton runat="server" ID="btnAccSave" CssClass="fa-regular fa-floppy-disk banner-icon flasher" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0005" orgToolTip="Save" />
            <iaw:IAWHyperLinkButton runat="server" ID="btnAccCancel" CssClass="fa-solid fa-arrow-rotate-left banner-icon flasher" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0022" orgToolTip="Reset" />
        </asp:Panel>

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
                    <asp:HiddenField runat="server" ID="hdnAccValue" ClientIDMode="Static" Value="Display" />
                    <asp:Button ID="btnAccPanel" runat="server" Text="Hidden Button" ClientIDMode="Static" Style="display: none;" />

                    <div id="divDisplay" class="divScroll grid-to-bottom">
                        <ajaxToolkit:Accordion ID="accPanel" runat="server" ClientIDMode="Static" HeaderCssClass="listheader left cur-pointer spacing" HeaderSelectedCssClass="listheader left cur-pointer spacing" FadeTransitions="true" TransitionDuration="100" AutoSize="None" SelectedIndex="0">
                            <Panes>
                                <ajaxToolkit:AccordionPane ID="accPanelDisplay" runat="server">
                                    <Header>
                                        <table border="0" style="width: 100%">
                                            <tr onclick="accPostback('Display')">
                                                <td class="left">
                                                    <iaw:IAWLabel runat="server" Text="::LT_S0223" orgText="Display" />
                                                </td>
                                            </tr>
                                        </table>

                                    </Header>
                                    <Content>
                                        <iaw:IAWGrid runat="server" ID="grdDisplay" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                            <Columns>
                                                <asp:BoundField DataField="line_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                    <ItemTemplate>
                                                        <div class="ActionButtons">
                                                            <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                            <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
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

                                <ajaxToolkit:AccordionPane ID="accPanelSort" runat="server">
                                    <Header>
                                        <table border="0" style="width: 100%">
                                            <tr onclick="accPostback('Sort')">
                                                <td class="left">
                                                    <iaw:IAWLabel runat="server" Text="::LT_A0101" orgText="Sort" />
                                                </td>
                                            </tr>
                                        </table>
                                    </Header>
                                    <Content>
                                        <iaw:IAWGrid runat="server" ID="grdSort" AutoGenerateColumns="False" DataKeyNames="disp_type,disp_line,disp_seq,field_num" editState="Normal" SaveButtonId="" ShowHeader="false">
                                            <Columns>
                                                <asp:BoundField DataField="line_desc" ReadOnly="True" ItemStyle-CssClass="grdField left" />
                                                <asp:BoundField DataField="display_name" ReadOnly="True" ItemStyle-CssClass="grdField left" />

                                                <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                    <ItemTemplate>
                                                        <div class="ActionButtons">
                                                            <div></div>
                                                            <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
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

    <%-- View Display Entry Popup --%>

    <asp:HiddenField runat="server" ID="hdnDispKey" />
    <iaw:IAWPanel ID="popDisplayEntry" runat="server" GroupingText="::LT_S0224" orgGroupingText="Field" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDisplayEntrySave">
        <iaw:wuc_help runat="server" ID="helplink_popDisplayEntry" />

        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel middle">
                    <iaw:IAWLabel ID="IAWLabel48" runat="server" Text="::LT_S0064" orgText="Text" />
                </td>
                <td class="formdata middle">
                    <iaw:IAWTextbox ID="txtDEtext" runat="server" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="DisplayEntry" />
                    <iaw:IAWCustomValidator ID="cvDEtext" runat="server" ControlToValidate="txtDEtext" ValidationGroup="DisplayEntry" Display="Dynamic" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" />
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
