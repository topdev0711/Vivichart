<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master"
    CodeBehind="Diagram.aspx.vb" Inherits=".Diagram" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <link rel="stylesheet" href="ZoomSlider.css" />
    <link rel="stylesheet" href="style.css" />
    <link rel="stylesheet" href="../Library/lineAttr.css" />

    <style>
        html, body {
            overflow: hidden;
        }
    </style>

    <script src="go_238.js"></script>
<%--    <script src="go_3010_debug.js"></script>--%>
    <script src="ZoomSlider.js"></script>
    <script src="../Scripts/tinymce/tinymce.min.js"></script>

    <script src="blob-stream.js"></script>
    <script src="pdfkit.js"></script>
    <script src="require.min.js" data-main="Model"></script>
    <script src="diagram.js"></script>
    <script src="DownloadPdf.js"></script>
    <script src="../Library/lineAttr.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hdnGoJsKey" runat="server" ClientIDMode="Static" />

    <iaw:IAWPanel ID="surround_panel" runat="server" DefaultButton="hdnButton">
        <!-- default button, traps enter key so as not to cause automatic postback -->
        <asp:Button runat="server" ID="hdnButton" OnClientClick="return false;" Style="display: none;" />

        <div id="divOuter" style="display: none;">
            <div id="divLeft" class="divLeft-on">
                <div class="header-row">
                    <div id="aeaHead" class="header-box">
                        <iaw:IAWLabel runat="server" ID="btnHide" CssClass="fa-regular fa-circle-left icon-as-button" ToolTip="::LT_A0038" orgToolTip="Hide Left Pane" onclick="PanelHideLeft(true);" ClientIDMode="Static" />
                        <label id="txtaea" class="header-text middle"></label>
                        <br />
                    </div>
                </div>
                <iaw:IAWTextbox runat="server" type="search" class="AEASearch" ID="txtAEAfilter" placeholder="::LT_A0039" orgPlaceholder="search" ClientIDMode="static" />

                <div id="divUnallocated" class="peeplist allow-hover" style="border: solid 1px silver;">
                    <table class="datatable">
                        <tr id="AEAData" class="listheader nowrap" />
                        <tbody id="ulUnallocated" />
                    </table>
                </div>
            </div>
            <div class="resizer" id="dragMe">
                <div id="dragCentre"></div>
            </div>
            <div id="divRight" class="divRight-partial">
                <div class="header-row">
                    <div id="ocaHeadLeft" class="header-box left">
                        <iaw:IAWLabel runat="server" ID="btnShow" CssClass="fa-regular fa-circle-right icon-as-button" ToolTip="::LT_A0040" orgToolTip="Show Left Pane" onclick="PanelShowLeft(true);" ClientIDMode="Static" />
                        <label id="txtoca" class="header-text middle"></label>
                        <span>&nbsp;&nbsp;&nbsp;</span>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnFirstChart" CssClass="fa-solid fa-angles-left icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0041" orgToolTip="Earliest Chart" />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnEarlierChart" CssClass="fa-solid fa-angle-left icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0042" orgToolTip="Earlier Chart" />
                        <label id="txtocadate" class="header-text middle"></label>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnLaterChart" CssClass="fa-solid fa-angle-right icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0043" orgToolTip="Later Chart" />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnLastChart" CssClass="fa-solid fa-angles-right icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0044" orgToolTip="Latest Chart" />
                    </div>

                    <div id="ocaHeadSearch" class="header-box right">
                        <iaw:IAWLabel runat="server" ID="btnChangeDirection" CssClass="fa-solid fa-rotate icon-as-button" ToolTip="::LT_A0045" orgToolTip="Rotate" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnZoomFit" CssClass="fa-solid fa-arrows-up-down-left-right icon-as-button" ToolTip="::LT_A0046" orgToolTip="Zoom to Fit" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnCentreTop" CssClass="fa-solid fa-arrows-to-dot icon-as-button" ToolTip="::LT_A0047" orgToolTip="Centre at Top" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnCentreRootNode" CssClass="fa-solid fa-arrows-to-dot icon-as-button" ToolTip="::LT_A0048" orgToolTip="Centre at Root node" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnShowMagnifier" CssClass="fa-solid fa-expand icon-as-button" ToolTip="::LT_A0049" orgToolTip="Show Magnifier" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnDownloadPDF" CssClass="fa-regular fa-file-pdf icon-as-button" ToolTip="::LT_A0050" orgToolTip="Download PDF" ClientIDMode="Static" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWTextbox runat="server" TextMode="SingleLine" ID="txtSearchModel" placeholder="::LT_A0051" orgPlaceholder="search chart" Style="width: 100px;" CssClass="middle" ClientIDMode="static" />
                        <iaw:IAWLabel runat="server" ID="btnModelSearch" CssClass="fa-solid fa-magnifying-glass icon-as-button" ToolTip="::LT_A0051" orgToolTip="Search Chart" ClientIDMode="Static" />
                        <iaw:IAWLabel runat="server" ID="btnModelSearchClear" CssClass="fa-solid fa-magnifying-glass-minus icon-as-button" ToolTip="::LT_A0052" orgToolTip="Clear Search" ClientIDMode="Static" />
                    </div>

                    <div id="ocaHeadRight" class="header-box right">
                        <iaw:IAWLabel runat="server" ID="btnModelSave" CssClass="fa-regular fa-floppy-disk icon-as-button" ToolTip="::LT_A0053" orgToolTip="Save Chart" ClientIDMode="Static" />
                        <span>&nbsp;&nbsp;&nbsp;</span>
                        <iaw:IAWLabel runat="server" ID="btnModelUndo" CssClass="fa-solid fa-arrow-rotate-left icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0054" orgToolTip="Undo Changes" />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnModelBackToList" CssClass="fa-solid fa-list icon-as-button" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_A0055" orgToolTip="Back to Chart List" />
                    </div>

                </div>

                <!-- Page Content  -->
                <div id="content">
                    <div style="position: relative;">
                        <!-- Actual Diagram -->
                        <div id="myDiagramDiv" style="border: solid 1px silver;"></div>
                        <!-- Magnifier -->
                        <div id="myOverviewDiv"></div>

                        <table id="contextMenu" class="menu">
                            <tr id="make_group" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-pen-to-square" />
                                <td class="text-wrapper" runat="server" id="menu_make_group" />
                                <td />
                            </tr>
                            <tr id="modal_setting" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-gear" />
                                <td class="text-wrapper" runat="server" id="menu_modal_setting" />
                                <td />
                            </tr>
                            <tr id="make_vacant" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-regular fa-square" />
                                <td class="text-wrapper" runat="server" id="menu_make_vacant" />
                                <td />
                            </tr>
                            <tr id="node_setting" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-pencil" />
                                <td class="text-wrapper" runat="server" id="menu_node_setting" />
                                <td />
                            </tr>
                            <tr id="add_text" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper iaw-font iaw-add-note" />
                                <td class="text-wrapper" runat="server" id="menu_add_text" />
                                <td />
                            </tr>
                            <tr id="assistant_on" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-toggle-on" />
                                <td class="text-wrapper" runat="server" id="menu_assistant_on" />
                                <td />
                            </tr>
                            <tr id="assistant_off" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-toggle-off" />
                                <td class="text-wrapper" runat="server" id="menu_assistant_off" />
                                <td />
                            </tr>
                            <tr id="move_left" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_move_left" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-left"></i></td>
                            </tr>
                            <tr id="move_right" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_move_right" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-right"></i></td>
                            </tr>
                            <tr id="make_parent_group" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-regular fa-object-group" />
                                <td class="text-wrapper" runat="server" id="menu_make_parent_group" />
                                <td />
                            </tr>
                            <tr id="make_new_parent" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_make_new_parent" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-up"></i></td>
                            </tr>
                            <tr id="make_child" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_make_child" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-down"></i></td>
                            </tr>
                            <tr id="move_down_to_parent_group" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_down_to_parent_group" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-down"></i></td>
                            </tr>
                            <tr id="move_up_to_parent_group" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper" />
                                <td class="text-wrapper" runat="server" id="menu_up_to_parent_group" />
                                <td class="shortcut-wrapper">Ctrl <i class="fa-solid fa-arrow-up"></i></td>
                            </tr>
                            <tr id="link_settings" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-pencil" />
                                <td class="text-wrapper" runat="server" id="menu_link_settings" />
                                <td />
                            </tr>
                            <tr id="expand_below" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-regular fa-square-plus" />
                                <td class="text-wrapper" runat="server" id="menu_expand_below" />
                                <td />
                            </tr>
                            <tr id="collapse_below" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-regular fa-square-minus" />
                                <td class="text-wrapper" runat="server" id="menu_collapse_below" />
                                <td />
                            </tr>
                            <tr id="delete_parent_group" class="menu-item" onpointerdown="ContextMenuCommand(event)">
                                <td class="icon-wrapper fa-solid fa-trash-can" />
                                <td class="text-wrapper" runat="server" id="menu_delete_parent_group" />
                                <td class="shortcut-wrapper">DEL</td>
                            </tr>
                        </table>

                        <div id="toolTipDIV" style="position: absolute; background: white; z-index: 1000; display: none;">
                            <div id="toolTipDivPra">
<%--                                <p id="toolTipParagraph">Tooltip</p>--%>
                            </div>
                        </div>
                        <div id="LabeltoolTipDIV" style="position: absolute; background: white; z-index: 1000; display: none;">
                            <div id="LabeltoolTipDivPra">
<%--                                <p id="LabelParagraph">Tooltip</p>--%>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </iaw:IAWPanel>

    <!-- Model Settings Dialog -->

    <iaw:IAWPanel ID="popModelSettings" runat="server" ClientIDMode="Static" GroupingText="::LT_A0056" orgGroupingText="Chart Settings" Style="display: none" CssClass="PopupPanel">

        <ajaxToolkit:TabContainer ID="modelTabContainer" runat="server" CssClass="iaw" OnClientActiveTabChanged="ModelTabChanged" ClientIDMode="Static">
            <iaw:IAWTabPanel runat="server" ID="tpModelGeneral" HeaderText="::LT_S0105" orgHeaderText="Chart" ClientIDMode="Static">
                <ContentTemplate>
                    <table>
                      <tbody>
                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel34" Text="::LT_S0107" orgText="Background Type" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWDropDownList ID="ddlbChartBackgroundType" runat="server" ClientIDMode="Static" onchange="ChangedBackgroundType();" TranslateText="true" />
                            </td>
                        </tr>

                        <tr class="formrow" id="trBackgroundImage">
                            <td class="formlabel"> 
                                <iaw:IAWLabel runat="server" ID="IAWLabel35" text="::LT_S0108" orgText="Background Image" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWDropDownList ID="ddlbChartBackgroundImage" runat="server" ClientIDMode="Static" TranslateText="false" />
                            </td>
                        </tr>

                        <tr class="formrow" id="trBackgroundGradient">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel48" Text="::LT_S0109" orgText="Background Gradient" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWDropDownList ID="ddlbChartBackgroundGradient" runat="server" ClientIDMode="Static" TranslateText="false" />
                            </td>
                        </tr>

                        <tr class="formrow" id="trBackgroundColour">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel2" Text="::LT_S0056" orgText="Background Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtChartBackgroundColour" CssClass="middle SpectrumColourEmpty" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" ClientIDMode="Static" />
                            </td>
                        </tr>

                        <tr id="trModelShowPhoto">
                            <td class="formlabel">
                                <iaw:IAWCheckBox ID="cbShowPhotos" runat="server" Text="::LT_S0201" orgText="Show Images" ClientIDMode="Static" TextAlign="Left" />
                            </td>
                            <td class="formdata">
                            </td>
                        </tr>

                        <tr id="trModelImagePosition">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel19" Text="::LT_A0058" orgText="Image Position" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWDropDownList runat="server" ID="ddlbImagePosition" ClientIDMode="Static" TranslateText="true" TranslateTooltip="true">
                                    <%-- 
                                        LT_A0117 Images on Left (Inline)
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

                        <tr id="trModelImageShape">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel32" Text="::LT_A0146" orgText="Image Shape" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWDropDownList runat="server" ID="ddlbImageShape" ClientIDMode="Static" TranslateText="true" TranslateTooltip="true">
                                    <%-- 
                                        LT_A0116 Original
                                        LT_A0148 Circle
                                        LT_A0184 Rounded Rectangle
                                    --%> 
                                    <asp:ListItem Value="" Text="::LT_A0116" title="::LT_A0116" Selected="true" />
                                    <asp:ListItem Value="Circle" Text="::LT_A0148" title="::LT_A0148" />
                                    <asp:ListItem Value="RoundedRectangle" Text="::LT_A0185" title="::LT_A0185" />
                                </iaw:IAWDropDownList>
                            </td>
                        </tr>

                        <tr id="trButtonImageHeight" class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel1" Text="::LT_A0165" orgText="Image Height" />
                            </td>
                            <td class="formdata" colspan="2">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numButtonImageHeight" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="sliderButtonImageHeight" runat="server" ClientIDMode="Static" TargetControlID="numButtonImageHeight" BehaviorID="bhButtonImageHeight" Minimum="30" Maximum="200" Steps="18" Decimals="0" BoundControlID="labButtonImageHeight" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labButtonImageHeight" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
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
                                <iaw:IAWTextbox runat="server" ID="txtModelLinkColour" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-minus" ToolTip="::LT_S0086" orgToolTip="Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtModelLinkHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" Tooltip="::LT_S0058" orgToolTip="Hover Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel5" Text="::LT_S0118" orgText="Tooltip Colours" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtModelLinkTooltipFg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtModelLinkTooltipBg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtModelLinkTooltipBorder" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-square" Tooltip="::LT_S0076" orgToolTip="Border Colour" />
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
                                            <iaw:IAWTextbox runat="server" ID="numModelLinkWidth" />
                                            <ajaxToolkit:SliderExtender ID="slideLinkWidth" runat="server" TargetControlID="numModelLinkWidth" BehaviorID="bhModelLinkWidth" Minimum="1" Maximum="5" Steps="9" Decimals="1" BoundControlID="labModelLinkWidth" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labModelLinkWidth" />
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
                                <iaw:IAWDropDownList runat="server" ID="ddlbModelLinkStyle" ClientIDMode="Static" Font-Names="monospace" TranslateText="false">
                                    <asp:ListItem Value="solid" Text="───────────" />
                                    <asp:ListItem Value="dotted" Text="─ ─ ─ ─ ─ ─" />
                                    <asp:ListItem Value="dashes" Text="─── ─── ───" />
                                </iaw:IAWDropDownList>
                            </td>
                        </tr>

                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpModelButtons" HeaderText="::LT_A0057" orgHeaderText="Chart Buttons" ClientIDMode="Static">
                <ContentTemplate>
                                    
                    <table>
                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel87" Text="::LT_A0059" orgText="Position" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbButtonPositionLeft" runat="server" Text="::LT_A0060" orgText="Icons on Left" GroupName="ButtonPosition" ClientIDMode="Static" onchange="buttonPositionChanged();" />
                                <iaw:IAWRadioButton ID="rbButtonPositionBelow" runat="server" Text="::LT_A0061" orgText="Buttons Below" GroupName="ButtonPosition" ClientIDMode="Static" onchange="buttonPositionChanged();" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel38" Text="::LT_S0321" orgText="Font" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbButtonFontSmall" runat="server" Text="::LT_A0062" orgText="Small" GroupName="ButtonFont" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbButtonFontMedium" runat="server" Text="::LT_A0063" orgText="Medium" GroupName="ButtonFont" ClientIDMode="Static" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel69" Text="::LT_S0129" orgText="Corners" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbButtonShapeSquare" runat="server" Text="::LT_S0130" orgText="Square" GroupName="ButtonShape" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbButtonShapeRounded" runat="server" Text="::LT_S0131" orgText="Rounded" GroupName="ButtonShape" ClientIDMode="Static" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel79" Text="::LT_S0064" orgText="Text" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtButtonTextColour" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtButtonBackgroundColour" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtButtonBorderColour" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel83" Text="::LT_A0067" orgText="Text (Hover)" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtButtonTextHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtButtonBackgroundHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtButtonBorderHover" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="lblButtonDetailText" Text="::LT_A0064" orgText="Detail Text" ClientIDMode="Static" />
                                <iaw:IAWLabel runat="server" ID="lblButtonDetailTooltip" Text="::LT_A0163" orgText="Detail Tooltip" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtButtonDetailText" ToolTip="::LT_A0064" orgToolTip="Detail Text" MaxLength="30" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="lblButtonNoteText" Text="::LT_A0065" orgText="Note Text" ClientIDMode="Static" />
                                <iaw:IAWLabel runat="server" ID="lblButtonNoteTooltip" Text="::LT_A0164" orgText="Note Tooltip" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtButtonNoteText" ToolTip="::LT_A0065" orgToolTip="Note Text" MaxLength="30" />
                            </td>
                        </tr>

                    </table>

                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpModelNode" HeaderText="::LT_S0121" orgHeaderText="Nodes" ClientIDMode="Static">
                <ContentTemplate>

                    <table class="inline top">
                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel3" Text="::LT_S0248" orgText="Node Colours" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtNodefg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeTxtBg" CssClass="middle SpectrumColourEmpty" ClientIDMode="Static" data-class="fa-solid fa-font white-on-black" Tooltip="::LT_S0122" orgToolTip="Text Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodebg" ClientIDMode="Static" class="middle SpectrumColour" data-class="fa-solid fa-paint-roller" Tooltip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtBorderfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" Tooltip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td colspan="2" class="formlabel">
                                <iaw:IAWCheckBox ID="cbTextBgBlock" runat="server" Text="::LT_S0123" orgText="Extend Text Background to the width of the Node" TextAlign="Left" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel6" Text="::LT_S0118" orgText="Tooltip Colours" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTTfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtTTbg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtTTBorder" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel31" Text="::LT_S0124" orgText="Highlight" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtHighlightfg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtHighlightbg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel25" Text="::LT_S0088" orgText="Icon Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtModelIconfg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0088" orgTooltip="Icon Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtModelIconHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_A0066" orgToolTip="Icon Hover" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWCheckBox ID="cbModelShowShadow" data-target="#txtModelShadowColour" runat="server" ClientIDMode="Static" Text="::LT_S0127" orgText="Show Shadow" Tooltip="::LT_S0127" orgTooltip="Show Shadow" CssClass="middle" TextAlign="Left" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtModelShadowColour" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-cube" ToolTip="::LT_S0128" orgTooltip="Shadow Colour" />
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel7" Text="::LT_S0119" orgText="Width" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numModelBoxWidth" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="sliderModelBoxWidth" runat="server" ClientIDMode="Static" TargetControlID="numModelBoxWidth" BehaviorID="bhModelBoxWidth" Minimum="100" Maximum="500" Steps="41" BoundControlID="labModelBoxWidth" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labModelBoxWidth" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel4" Text="::LT_A0070" orgText="Height" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numModelBoxHeight" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="sliderModelBoxHeight" runat="server" TargetControlID="numModelBoxHeight" BehaviorID="bhModelBoxHeight" Minimum="50" Maximum="200" Steps="16" BoundControlID="labModelBoxHeight" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labModelBoxHeight" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel12" Text="::LT_S0129" orgText="Corners" />
                            </td>
                            <td class="formdata" colspan="2">
                                <iaw:IAWRadioButton ID="rbModelCornerRectangle" runat="server" Text="::LT_S0130" orgText="Square" GroupName="boxNodeCorners" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbModelCornerRoundedRectangle" runat="server" Text="::LT_S0131" orgText="Rounded" GroupName="boxNodeCorners" ClientIDMode="Static" />
                            </td>
                        </tr>

                    </table>

                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpModelFont" HeaderText="::LT_S0136" orgHeaderText="Text Fonts">
                <ContentTemplate>

                    <table class="tblStyle">
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel14" Text="::LT_S0137" orgText="Lines" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine1"></label>
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel15" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine2"></label>
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel18" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine3" />
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel20" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine4" />
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel21" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine5" />
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel right">
                                <iaw:IAWLabel runat="server" ID="IAWLabel22" Text="" />
                            </td>
                            <td class="formdata">
                                <label id="lblModelLine6" />
                            </td>
                        </tr>
                    </table>

                </ContentTemplate>
            </iaw:IAWTabPanel>
        </ajaxToolkit:TabContainer>

        <div class="areadiv">
            <table class="tblStyle">
                <tr>
                    <td class="center">
                        <iaw:IAWHyperLinkButton runat="server" ID="btnApplyModelSettings" Text="::LT_A0068" orgText="Apply" CssClass="hlink IconBtn Icon16 BtnConfirm" CausesValidation="false" ClientIDMode="Static" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnCancelModelSettings" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" ClientIDMode="Static" />
                    </td>
                </tr>
            </table>
        </div>

    </iaw:IAWPanel>

    <asp:Button ID="btnModelSettingsFake" runat="server" Style="display: none" ClientIDMode="Static" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeModelSettingsForm" BackgroundCssClass="ModalBackground" PopupControlID="popModelSettings" TargetControlID="btnModelSettingsFake" ClientIDMode="Static" />

    <!-- Node Settings Dialog -->

    <iaw:IAWPanel ID="popNodeSettings" runat="server" ClientIDMode="Static" GroupingText="::LT_A0069" orgGroupingText="Node Settings" Style="display: none" CssClass="PopupPanel">

        <ajaxToolkit:TabContainer ID="nodeTabContainer" runat="server" ClientIDMode="Static" CssClass="iaw nodeSettingsTab" OnClientActiveTabChanged="NodeTabChanged">
            <iaw:IAWTabPanel runat="server" ID="tpNodeGeneral" HeaderText="::LT_S0044" orgHeaderText="General" ClientIDMode="Static">
                <ContentTemplate>
                    <table class="tblStyle">
                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel9" Text="::LT_S0086" orgText="Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtNodeBoxfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeTextBg" ClientIDMode="Static" CssClass="middle SpectrumColourEmpty" data-class="fa-solid fa-font white-on-black" ToolTip="::LT_S0122" orgTooltip="Text Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeBoxbg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeBorderfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                            <td></td>
                        </tr>

                        <tr class="formrow">
                            <td colspan="2" class="formlabel">
                                <iaw:IAWCheckBox ID="cbNodeTextBgBlock" runat="server" Text="::LT_S0123" orgText="Extend Text Background to the width of the Node" TextAlign="Left" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWCheckBox ID="cbNodeShowShadow" data-target="#txtNodeShadowColour" runat="server" ClientIDMode="Static" ToolTip="::LT_S0127" orgToolTip="Show shadow" Text="::LT_S0127" orgText="Show shadow" CssClass="middle" TextAlign="Left" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtNodeShadowColour" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-cube" ToolTip="::LT_S0128" orgToolTip="Shadow Colour" />
                            </td>
                        </tr>

                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel28" Text="::LT_S0088" orgText="Icon Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtNodeIconfg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0088" orgTooltip="Icon Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtNodeIconHover" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_A0066" orgToolTip="Icon Hover" />
                            </td>
                        </tr>

                        <tr id="trBoxWidth">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel8" Text="::LT_S0119" orgText="Width" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodeBoxWidth" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="sliderNodeBoxWidth" runat="server" ClientIDMode="Static" TargetControlID="numNodeBoxWidth" BehaviorID="bhNodeBoxWidth" Minimum="100" Maximum="500" Steps="41" BoundControlID="labNodeBoxWidth" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodeBoxWidth" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr id="trBoxHeight">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel10" Text="::LT_A0070" orgText="Height" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodeBoxHeight" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="sliderNodeBoxHeight" runat="server" TargetControlID="numNodeBoxHeight" BehaviorID="bhNodeBoxHeight" Minimum="50" Maximum="200" Steps="16" BoundControlID="labNodeBoxHeight" ClientIDMode="Static" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodeBoxHeight" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel11" Text="::LT_S0129" orgText="Corners" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbNodeCornerRectangle" runat="server" Text="::LT_S0130" orgText="Square" GroupName="boxModelCorners" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbNodeCornerRoundedRectangle" runat="server" Text="::LT_S0131" orgText="Rounded" GroupName="boxModelCorners" ClientIDMode="Static" />
                            </td>
                        </tr>

                        <tr id="trNodesAcross">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel23" Text="::LT_A0071" orgText="Nodes Across" />
                            </td>
                            <td class="formdata">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodesAcross" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="SliderNodesAcross" runat="server" TargetControlID="numNodesAcross" BehaviorID="bhNodesAcross" Minimum="0" Maximum="10" Steps="11" BoundControlID="labNodesAcross" ClientIDMode="Static" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodesAcross" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                        <tr id="trNodesAcrossInfo">
                           <td></td>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="lblNodesAcrossInfo" Text="::LT_A0072" orgText="(0 signifies that this will be determined)" />
                            </td>
                        </tr>

                        <tr id="trShowPhoto">
                            <td class="formlabel">
                                <iaw:IAWCheckBox ID="cbNodeShowPhoto" runat="server" ClientIDMode="Static" Text="::LT_A0073" orgText="Show Image" TextAlign="Left" />
                            </td>
                            <td class="formdata" ID="sliderNodeImageHeight">
                                <table border="0">
                                    <tr>
                                        <td>
                                            <iaw:IAWTextbox runat="server" ID="numNodeImageHeight" ClientIDMode="Static" />
                                            <ajaxToolkit:SliderExtender ID="SliderNodeImageHeight" runat="server" TargetControlID="numNodeImageHeight" BehaviorID="bhNodeImageHeight" Minimum="50" Maximum="100" Steps="6" BoundControlID="labNodeImageHeight" ClientIDMode="Static" />
                                        </td>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="labNodeImageHeight" ClientIDMode="Static" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpNodeTeam" HeaderText="::LT_A0074" orgHeaderText="Displayed Text" ClientIDMode="Static">
                <ContentTemplate>
                    <asp:DropDownList runat="server" ID="ddlbFont" ClientIDMode="static" Style="display: none" />

                    <table class="tblStyle">
                        <tr id="trLine1">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="lblLines" Text="::LT_S0137" orgText="Lines" ClientIDMode="Static" />
                                <iaw:IAWLabel runat="server" ID="lblGroupTitle" Text="::LT_A0075" orgText="Title" ClientIDMode="Static" />
                            </td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine1" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine1" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine1"></label>
                            </td>
                        </tr>
                        <tr id="trLine2">
                            <td></td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine2" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine2" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine2" />
                            </td>
                        </tr>
                        <tr id="trLine3">
                            <td></td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine3" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine3" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine3" />
                            </td>
                        </tr>
                        <tr id="trLine4">
                            <td></td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine4" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine4" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine4" />
                            </td>
                        </tr>
                        <tr id="trLine5">
                            <td></td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine5" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine5" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine5" />
                            </td>
                        </tr>
                        <tr id="trLine6">
                            <td></td>
                            <td>
                                <iaw:IAWLabel runat="server" ID="iUndoLine6" CssClass="fa-solid fa-arrow-rotate-left line-icon" ToolTip="::LT_A0076" orgToolTip="Clear" ClientIDMode="Static" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtTeamLine6" MaxLength="50" TextMode="SingleLine" ClientIDMode="Static" Width="300px" />
                            </td>
                            <td class="teamAttr nowrap">
                                <label id="lblTeamLine6" />
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpNodeNote" HeaderText="::LT_A0077" orgHeaderText="Note" ClientIDMode="Static">
                <ContentTemplate>
                    <table class="tblStyle">
                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel16" Text="::LT_S0125" orgText="Tooltip" ClientIDMode="Static" />
                            </td>
                            <td class="formdata top">
                                <iaw:IAWTextbox runat="server" ID="txtTooltip" MaxLength="50" ClientIDMode="Static" CssClass="middle" Width="350px" />
                            </td>
                            <td class="formdata">
                                <div id="tooltipColours">
                                    <iaw:IAWTextbox runat="server" ID="txtNodeTooltipfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                    <span>&nbsp;&nbsp;</span>
                                    <iaw:IAWTextbox runat="server" ID="txtNodeTooltipbg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                    <span>&nbsp;&nbsp;</span>
                                    <iaw:IAWTextbox runat="server" ID="txtNodeTooltipBorder" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                                </div>
                            </td>
                        </tr>

                        <tr>
                            <td class="formdata top" colspan="3">
                                <textarea id="txtNoteArea"></textarea>
                                <%--<div id="txtNodeLabel"></div>--%>
                            </td>
                        </tr>
                        <tr>
                            <td class="formdata top" colspan="3">
                                <iaw:IAWCheckBox ID="cbPrivateLabel" runat="server" Text="::LT_A0078" orgText="Private note for me only" ClientIDMode="Static" />
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpNodeIcon" HeaderText="::LT_A0084" orgHeaderText="Icon" ClientIDMode="Static">
                <ContentTemplate>
                    <table class="tblStyle">
                        <tr>
                            <td class="formdata top">
                                <asp:RadioButtonList ID="rbLabelIcon" runat="server" class="iconlist" ClientIDMode="static" RepeatDirection="Horizontal" RepeatLayout="Table" />
                            </td>
                        </tr>
                    </table>

                    <table class="tblStyle">
                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel29" Text="::LT_S0088" orgText="Icon Colour" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox runat="server" ID="txtIconfg" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtIconbg" ClientIDMode="Static" CssClass="middle SpectrumColourChange" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                                <span>&nbsp;&nbsp;</span>
                                <iaw:IAWTextbox runat="server" ID="txtIconBorder" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                            </td>
                        </tr>
                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel30" Text="::LT_A0147" orgText="Icon Shape" />  
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbIconSquare" runat="server" Text="::LT_S0130" orgText="Square" GroupName="ButtonShape" ClientIDMode="Static" />
                                <iaw:IAWRadioButton ID="rbIconCircle" runat="server" Text="::LT_A0148" orgText="Circle" GroupName="ButtonShape" ClientIDMode="Static" />
                            </td>
                        </tr>
                    </table>

                </ContentTemplate>
            </iaw:IAWTabPanel>

        </ajaxToolkit:TabContainer>

        <div class="areadiv">
            <table class="tblStyle">
                <tr>
                    <td>
                        <br />
                        <%-- LT_A0020 "Apply to this Node"
                             LT_A0021 "Apply changes to this Node only"
                             LT_A0022 "Apply to this Node and Below"
                             LT_A0023 "Apply changes to this Node and its children"
                             LT_A0024 "CoParents in same Group"
                             LT_A0025 "Apply changes to all CoParents in the same Parent Group"
                             LT_A0026 "CoParents in same Group and Below"
                             LT_A0027 "Apply changes to all CoParents in the same Parent Group and its Children"
                             LT_A0028 "My Siblings"
                             LT_A0029 "Apply changes to all Sibling nodes of this type"
                             LT_A0030 "My Siblings and Below"
                             LT_A0031 "Apply changes to all Sibling nodes of this type and their children"
                             LT_A0032 "At this Level"
                             LT_A0033 "Apply changes to all nodes of this type at the same level"
                             LT_A0034 "At this Level and Below"
                             LT_A0035 "Apply changes to all nodes of this type at the same level and their children"
                             LT_A0036 "All Nodes"
                             LT_A0037 "Apply changes to all nodes of this type"
                             LT_A0173 'Apply to this Parent Group'
                             LT_A0174 'Apply changes to this Parent Group only'
                             LT_A0175 'Apply to this Parent Group and Below'
                             LT_A0176 'Apply changes to this Parent Group and its children'
                             LT_A0177 'Apply changes to all Sibling Parent Groups'
                             LT_A0178 'Apply changes to all Sibling Parent Groups and their children'
                             LT_A0179 'Apply changes to all Parent Groups at the same level'
                             LT_A0180 'Apply changes to all Parent Groups at the same level and their children'
                             LT_A0181 'All Parent Groups"
                             LT_A0182 'Apply changes to all Parent Groups'                            
                            --%>
                        <iaw:IAWDropDownList ID="ddlbApplyOptions" runat="server" ClientIDMode="Static" TranslateText="true" TranslateTooltip="true">
                            <asp:ListItem Value="Apply" Text="::LT_A0020" title="::LT_A0021" data-cat="node coparent" />
                            <asp:ListItem Value="Apply" Text="::LT_A0173" title="::LT_A0174" data-cat="pg" />

                            <asp:ListItem Value="ApplyBelow" Text="::LT_A0022" title="::LT_A0023" data-cat="node coparent" />
                            <asp:ListItem Value="ApplyBelow" Text="::LT_A0175" title="::LT_A0176" data-cat="pg" />

                            <asp:ListItem Value="CoParents" Text="::LT_A0024" title="::LT_A0025" data-cat="coparent" />

                            <asp:ListItem Value="CoParentsBelow" Text="::LT_A0026" title="::LT_A0027" data-cat="coparent" />

                            <asp:ListItem Value="Siblings" Text="::LT_A0028" title="::LT_A0029" data-cat="node coparent" />
                            <asp:ListItem Value="Siblings" Text="::LT_A0028" title="::LT_A0177" data-cat="pg" />

                            <asp:ListItem Value="SiblingsBelow" Text="::LT_A0030" title="::LT_A0031" data-cat="node coparent" />
                            <asp:ListItem Value="SiblingsBelow" Text="::LT_A0030" title="::LT_A0178" data-cat="pg" />

                            <asp:ListItem Value="Level" Text="::LT_A0032" title="::LT_A0033" data-cat="node coparent" />
                            <asp:ListItem Value="Level" Text="::LT_A0032" title="::LT_A0179" data-cat="pg" />

                            <asp:ListItem Value="LevelBelow" Text="::LT_A0034" title="::LT_A0035" data-cat="node coparent" />
                            <asp:ListItem Value="LevelBelow" Text="::LT_A0034" title="::LT_A0180" data-cat="pg" />

                            <asp:ListItem Value="All" Text="::LT_A0036" title="::LT_A0037" data-cat="node coparent" />
                            <asp:ListItem Value="All" Text="::LT_A0181" title="::LT_A0182" data-cat="pg" />
                        </iaw:IAWDropDownList>

                        <%-- LT_A0166 Detail Nodes Only
                             LT_A0167 Assistant Nodes Only
                             LT_A0168 Group Nodes Only
                             LT_A0169 Vacant Nodes Only
                             LT_A0170 Co Parent Nodes Only
                             LT_A0171 All Types --%>
                        <iaw:IAWDropDownList ID="ddlbApplyTypes" runat="server" ClientIDMode="Static" TranslateText="true">
                            <asp:ListItem Value="detail" Text="::LT_A0166" />
                            <asp:ListItem Value="assistant" Text="::LT_A0167" />
                            <asp:ListItem Value="team" Text="::LT_A0168" />
                            <asp:ListItem Value="vacant" Text="::LT_A0169" />
                            <asp:ListItem Value="coparent" Text="::LT_A0170" />
                            <asp:ListItem Value="alltypes" Text="::LT_A0171" Selected="true" />
                        </iaw:IAWDropDownList>
                        <iaw:IAWCheckBox runat="server" ID="cbApplySizes" ClientIDMode="Static" TextAlign="Left"
                            Text="::LT_A0183" TranslateText="true" orgText="Change Dimensions" 
                            ToolTip="::LT_A0184" TranslateTooltip="true" orgToolTip="The height and width of the current Node can be applied to other Nodes." />
                    </td>
                </tr>
                <tr>
                    <td class="center">
                        <br />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnApplyNodeSettings" Text="::LT_A0068" orgText="Apply" CausesValidation="false" ClientIDMode="Static" CssClass="hlink IconBtn Icon16 BtnConfirm" />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnApplyNewNode" Text="::LT_S0039" orgText="Add" CausesValidation="false" ClientIDMode="Static" CssClass="hlink IconBtn Icon16 BtnConfirm" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnCancelNodeSettings" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" ClientIDMode="Static" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnApplyDefaults" Text="::LT_S0022" orgText="Reset" CausesValidation="false" ClientIDMode="Static" CssClass="hlink IconBtn Icon16 BtnWithdraw" />
                    </td>
                </tr>
            </table>
        </div>
    </iaw:IAWPanel>

    <asp:Button ID="btnNodeSettingsFake" runat="server" Style="display: none" ClientIDMode="Static" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeNodeSettingsForm" BackgroundCssClass="ModalBackground" PopupControlID="popNodeSettings" TargetControlID="btnNodeSettingsFake" ClientIDMode="Static" />

    <!-- Item Node Details -->

    <iaw:IAWPanel ID="popItemDetail" runat="server" Style="display: none" CssClass="InfoPanel" ClientIDMode="Static">
        <iaw:IAWHyperLinkButton runat="server" ID="btnCancelItemDetail" CssClass="closeIcon fa-solid fa-xmark" CausesValidation="false" ClientIDMode="Static" />
        <div class="dataPanel">
            <div id="divItemDetail" class="notecontrol"></div>
        </div>
    </iaw:IAWPanel>

    <asp:Button ID="btnItemDetailFake" runat="server" Style="display: none" ClientIDMode="Static" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeItemDetailForm" BackgroundCssClass="ModalBackground" PopupControlID="popItemDetail" TargetControlID="btnItemDetailFake" ClientIDMode="Static" />

    <!-- Link Settings -->

    <iaw:IAWPanel ID="popLinkSettings" runat="server" ClientIDMode="Static" GroupingText="::LT_A0079" orgGroupingText="Relationship Line" Style="display: none" CssClass="PopupPanel">

        <table class="tblStyle">
            <tr id="trLinkColour">
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel13" Text="::LT_S0086" orgText="Colour" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtLinkColour" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-solid fa-minus" ToolTip="::LT_S0086" orgToolTip="Colour" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWTextbox runat="server" ID="txtLinkHover" ClientIDMode="Static" CssClass="middle SpectrumColour" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgToolTip="Hover Colour" />
                </td>
            </tr>

            <tr id="trLinkWidth">
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel17" Text="::LT_S0119" orgText="Width" />
                </td>
                <td class="formdata" colspan="2">
                    <table border="0">
                        <tr>
                            <td>
                                <iaw:IAWTextbox runat="server" ID="numLinkWidth" ClientIDMode="Static" />
                                <ajaxToolkit:SliderExtender ID="sliderLinkWidth" runat="server" ClientIDMode="Static" TargetControlID="numLinkWidth" BehaviorID="bhLinkWidth" Minimum="1" Maximum="5" Steps="9" Decimals="1" BoundControlID="labLinkWidth" />
                            </td>
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="labLinkWidth" ClientIDMode="Static" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>

            <tr>
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel24" Text="::LT_S0120" orgText="Style" />
                </td>
                <td class="formdata">
                    <iaw:IAWDropDownList runat="server" ID="ddlbLinkStyle" Font-Names="monospace" TranslateText="false" ClientIDMode="Static">
                        <asp:ListItem Value="solid" Text="───────────" />
                        <asp:ListItem Value="dotted" Text="─ ─ ─ ─ ─ ─" />
                        <asp:ListItem Value="dashes" Text="─── ─── ───" />
                    </iaw:IAWDropDownList>
                </td>
            </tr>

            <tr id="trTooltipColours">
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel26" Text="::LT_S0118" orgText="Tooltip Colours" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipFg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgToolTip="Text Colour" />
                    <span>&nbsp;</span>
                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipBg" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgToolTip="Background Colour" />
                    <span>&nbsp;</span>
                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltipBorder" CssClass="middle SpectrumColour" ClientIDMode="Static" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgToolTip="Border Colour" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel runat="server" ID="IAWLabel27" Text="::LT_S0125" orgText="Tooltip" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox runat="server" ID="txtLinkTooltip" MaxLength="50" ClientIDMode="Static" CssClass="middle" Width="350px" />
                </td>
            </tr>

            <tr>
                <td colspan="2">
                    <br />
                </td>
            </tr>

            <tr id="trApplyLinkOptions">
                <td colspan="2">
                    <%-- LT_A0080  Apply only to current line
                         LT_A0081  Apply the change to the Current Line only
                         LT_A0082  Apply to all lines
                         LT_A0083  Apply the change to All Lines --%>

                    <iaw:IAWDropDownList ID="ddlbApplyLinkOptions" runat="server" ClientIDMode="Static" TranslateText="true" TranslateTooltip="true">
                        <asp:ListItem Value="currentLine" Text="::LT_A0080" title="::LT_A0081" Selected="true" />
                        <asp:ListItem Value="allLine" Text="::LT_A0082" title="::LT_A0083" />
                    </iaw:IAWDropDownList>
                </td>
            </tr>

            <tr>
                <td colspan="2">
                    <br />
                </td>
            </tr>

            <tr>
                <td colspan="2">
                    <iaw:IAWHyperLinkButton runat="server" ID="btnApplyLinkSettings" Text="::LT_A0068" orgText="Apply" CausesValidation="false" ClientIDMode="Static" CssClass="hlink IconBtn Icon16 BtnConfirm" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnCancelLinkSettings" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" ClientIDMode="Static" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnLinkSettingsFake" runat="server" Style="display: none" ClientIDMode="Static" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeLinkSettingsForm" BackgroundCssClass="ModalBackground" PopupControlID="popLinkSettings" TargetControlID="btnLinkSettingsFake" ClientIDMode="Static" />

    <!-- Undo Changes -->

    <iaw:IAWPanel ID="popUndo" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none" CssClass="PopupPanel">
            <table border="0" class="listform">
                <tr>
                    <td>
                        <iaw:IAWLabel ID="lblUndoConfirm" runat="server" Text="::LT_C0001" orgText="Are you sure you want to undo the changes ?"/>
                    </td>
                </tr>
                <tr>
                    <td align="center">
                        <br />
                        <iaw:IAWHyperLinkButton runat="server" ID="btnUndoOk" ClientIDMode="Static" Text="::LT_S0033" orgText="Yes" CausesValidation="false" ToolTip="::LT_A0054" orgToolTip="Undo changes" IconClass="iconButton fa-solid fa-circle-check" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnUndoCancel" ClientIDMode="Static" Text="::LT_S0034" orgText="No" CausesValidation="false" ToolTip="::LT_C0002" orgToolTip="Keep the changes" IconClass="iconButton fa-solid fa-circle-xmark" />
                    </td>
                </tr>
            </table>
        </iaw:IAWPanel>
        <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeUndo" ClientIDMode="Static" BackgroundCssClass="ModalBackground" OkControlID="btnUndoOk" CancelControlID="btnUndoCancel" PopupControlID="popUndo" TargetControlID="btnModelUndo" />

</asp:Content>
