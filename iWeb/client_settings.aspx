<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="client_settings.aspx.vb" Inherits=".client_settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <link rel="stylesheet" href="Library/lineAttr.css" />
    <link rel="stylesheet" href="client_settings.css" />
    <script src="Library/lineAttr.js"></script>
    <script src="js/rgbfilter.js"></script>
    <script src="client_settings.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <%-- 0|1 indicating whether something has changed and the save button has been shown --%>
    <asp:HiddenField runat="server" ID="hdnChangesPending" ClientIDMode="static" />

    <%-- The attrib is the lines attributes, font, font-size etc.  Font is the dropdown for the attribute lines --%>
    <asp:HiddenField runat="server" ID="hdnAttrib" ClientIDMode="Static" />
    <asp:DropDownList runat="server" ID="ddlbFont" ClientIDMode="static" CssClass="hidden" />

    <%-- Selected brand logo and filename --%>
    <asp:HiddenField runat="server" ID="hdnLogo" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnLogofilename" ClientIDMode="static" />

    <%-- These hidden fields hold the computed filters for the selcted colours (see RestringData() in the js file) --%>
    <asp:HiddenField runat="server" ID="hdnHeaderIconsFilter" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnHeaderIconsHoverFilter" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnBodyIconsFilter" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnBodyIconsHoverFilter" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnCheckboxFilter" ClientIDMode="static" />

    <%-- This hidden field for storing backgrounds --%>
    <asp:HiddenField runat="server" ID="hdnBgType" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnBgContent" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnBgImageName" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnPrevBgImage" ClientIDMode="static" />
    <asp:HiddenField runat="server" ID="hdnBgStructure" ClientIDMode="static" />

    <iaw:IAWPanel runat="server" ID="pnlDisplay" GroupingText= "::LT_S0036" orgGroupingText="Settings" ClientIDMode="Static" CssClass="hidden">
        <ajaxToolkit:TabContainer ID="tcBranding" runat="server" CssClass="iaw" ClientIDMode="Static">
            <iaw:IAWTabPanel runat="server" ID="tpBranding" HeaderText= "::LT_S0037" orgHeaderText="Branding" CssClass="parentTabScroll">
                <ContentTemplate>
                    <table border="0" width="100%">
                        <tr>
                            <td class="top tdGrdBrand">
                                <iaw:IAWPanel runat="server" ID="pnlGrdBrand" ClientIDMode="Static" CssClass="childTabScroll">
                                    <iaw:IAWGrid runat="server" ID="grdBrand" AutoGenerateColumns="False"
                                        ShowHeaderWhenEmpty="True" DataKeyNames="brand_id"
                                        editState="Normal" TranslateHeadings="True">
                                        <Columns>
                                            <iawb:IAWBoundField DataField="brand_name" HeaderText="::LT_S0038" orgHeaderText="Brand" Visible="true" />
                                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                                <HeaderTemplate>
                                                    <iaw:IAWImageButton runat="server" ID="btnDataSourceAdd" CommandName="New" ImageUrl="~/graphics/1px.gif" CssClass="IconPic Icon16 IconAddHead" ToolTip= "::LT_S0039" orgTooltip="Add" />
                                                </HeaderTemplate>

                                                <ItemTemplate>
                                                    <div class="ActionButtons">
                                                        <div><iaw:IAWHyperLinkButton ID="btnAmendRow" runat="server" CssClass="IconPic Icon16 IconEdit" ToolTip= "::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                                        <div>
                                                            <iaw:IAWHyperLinkButton ID="btnDeleteRow" runat="server" CssClass="IconPic Icon16 IconDelete" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                                            <iaw:IAWLabel ID="lblNoDeleteRow" runat="server" CssClass="IconPic Icon16 IconBan" ToolTip="::LT_S0041" orgToolTip="In Use. Can not be deleted" Visible="false" />
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <RowStyle Wrap="False" />
                                    </iaw:IAWGrid>
                                </iaw:IAWPanel>
                            </td>
                            <td class="top" style="width: 100%;">
                                <div class="drop-area"
                                    ondragover="allowDrop(event)"
                                    ondragenter="highlightDropArea(event)"
                                    ondragleave="unhighlightDropArea(event)"
                                    ondrop="drop(event)">
                                    <div class="drop-here">
                                        <iaw:IAWLabel runat="server" Text="::LT_S0024" orgText="Drop Here" />
                                    </div>
                                    <table width="100%" style="position: absolute; top: -8px">
                                        <tr class="formrow">
                                            <td class="formdata right" style="padding-right: 0">
                                                <iaw:IAWHyperLinkButton runat="server" ID="btnThemeSelect" Text="::LT_S0042" orgText="Apply Theme" CausesValidation="false" ToolTip= "::LT_S0043" orgTooltip="Copy Details from" Style="padding: 4px 20px;" />
                                            </td>
                                        </tr>
                                    </table>
                                    <ajaxToolkit:TabContainer ID="TabContainer1" runat="server" CssClass="iaw" style="margin-top: 5px">
                                        <iaw:IAWTabPanel runat="server" ID="tpGeneral" HeaderText= "::LT_S0044" orgHeaderText="General" CssClass="childTabScroll">
                                            <ContentTemplate>
                                                <table class="grp" width="100%">
                                                    <tr class="formrow">
                                                        <td class="formlabel" colspan="2">
                                                            <iaw:IAWHyperLinkButton runat="server" ID="chooseFile" OnClientClick="document.getElementById('FileUpload1').click(); return false;" Text="::LT_S0045" orgText="Upload Logo" Style="padding: 4px 20px;" />
                                                            <span>&nbsp;</span>
                                                            <%--<iaw:IAWLabel runat="server" ID="labFileName" ClientIDMode="Static" TranslateText="false" Text="No File Chosen" CssClass="threeDotsLabel" Style="font-size: small; vertical-align: middle" />--%>
                                                            <iaw:IAWLabel runat="server" ID="labFileName" ClientIDMode="Static" TranslateTooltip="false" TranslateText="false" Text="::LT_S0046" orgText="No File Chosen" CssClass="threeDotsLabel" Style="vertical-align: middle" />
                                                            <span>&nbsp;</span>
                                                            <asp:FileUpload runat="server" ID="FileUpload1" accept="image/*" onchange="UpChange(this)" ClientIDMode="Static" Style="display: none;" />
<%--                                                            <div style="float: inline-end; transform: translateY(4px);">--%>
                                                            <div style="float: inline-end;">
                                                                <iaw:IAWCheckBox ID="cbAdvanced" runat="server" AutoPostBack="true" Checked="false" Text="::LT_S0047" orgText="Advanced" TextAlign="Left" Style="vertical-align: middle" />
                                                            </div>
                                                        </td>
                                                    </tr>
                                                    <tr runat="server" id="trLogoPreview" class="formrow">
                                                        <td class="formlabel" colspan="2" style="padding-left: 10px">
                                                            <asp:Image runat="server" ID="logoPreview" ClientIDMode="Static" AlternateText="Logo Preview" />
                                                        </td>
                                                    </tr>
                                                    <tr runat="server" id="trLogoColours" class="formrow">
                                                        <td class="formlabel logoColourPreview" colspan="2" style="padding-left: 10px">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel37" Text="::LT_S0048" orgText="Click on image to select colour" />
                                                            <div>
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel23" Text="::LT_S0049" orgText="Selected Colour" />
                                                                <span>&nbsp;</span>
                                                                <iaw:IAWTextbox runat="server" ID="logoNormalColour" CssClass="middle SpectrumBranding SpectrumLogo" ToolTip="::LT_S0049" orgTooltip="Selected Colour" />
                                                                <span>&nbsp;&nbsp;</span>
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel33" Text="::LT_S0050" orgText="Lighter" />
                                                                <span>&nbsp;</span>
                                                                <iaw:IAWTextbox runat="server" ID="logoLighterColour" CssClass="middle SpectrumBranding SpectrumLogo" ToolTip="::LT_S0051" orgTooltip="Lighter Colour" />
                                                                <span>&nbsp;&nbsp;</span>
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel36" Text="::LT_S0052" orgText="Darker" />
                                                                <span>&nbsp;</span>
                                                                <iaw:IAWTextbox runat="server" ID="logoDarkerColour" CssClass="middle SpectrumBranding SpectrumLogo" ToolTip="::LT_S0053" orgTooltip="Darker Colour" />
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <iaw:IAWPanel runat="server" ID="pAdvanced">
                                                    <table class="grp">
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel50" Text="::LT_S0054" orgText="Banner" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="bannerTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                                <span>&nbsp;&nbsp;</span>
                                                                <iaw:IAWTextbox runat="server" ID="bannerBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel68" Text="::LT_S0057" orgText="Banner Icons (Hover)" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="bannerIconsHoverColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgTooltip="Hover Colour" />
                                                            </td>
                                                        </tr>

                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel60" Text="::LT_S0059" orgText="Body Background Type" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWDropDownList ID="ddlbBodyBackgroundType" runat="server" ClientIDMode="Static" onchange="change_Branding(); bodyBackgroundTypeChanged();" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow" id="trBodyBackgroundColour">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel16" Text="::LT_S0060" orgText="Body Colour" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="bodyBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow" id="trBodyBackgroundImage">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel62" Text="::LT_S0062" orgText="Body Image" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWDropDownList ID="ddlbBodyBackgroundImage" runat="server" ClientIDMode="Static" TranslateText="false" onchange="change_Branding();" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow" id="trBodyBackgroundGradient">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel67" Text="::LT_S0063" orgText="Body Gradient" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWDropDownList ID="ddlbBodyBackgroundGradient" runat="server" ClientIDMode="Static" TranslateText="false" onchange="change_Branding();" />
                                                            </td>
                                                        </tr>

                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel52" Text="::LT_S0064" orgText="Text" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="textColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </iaw:IAWPanel>
                                                <iaw:IAWPanel runat="server" ID="pSimplified">
                                                    <table class="grp">
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel40" Text="::LT_S0054" orgText="Banner" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="simpBannerBgColour" CssClass="middle SpectrumBranding" ToolTip="::LT_S0065" orgTooltip="Banner Colour" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel39" Text="::LT_S0066" orgText="Body" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="simpPrincipal" CssClass="middle SpectrumBranding" ToolTip="::LT_S0067" orgTooltip="Principal Colour" />
                                                                <iaw:IAWTextbox runat="server" ID="simpLightPrincipal" CssClass="middle SpectrumBranding" style="display: inline" ToolTip="::LT_S0068" orgTooltip="Lightened Body" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel41" Text="::LT_S0069" orgText="Content Area" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="simpContentArea" CssClass="middle SpectrumBranding" ToolTip="::LT_S0070" orgTooltip="Content Area Colour" />
                                                                <iaw:IAWTextbox runat="server" ID="simpDarkContextArea" CssClass="middle SpectrumBranding" style="display: inline" ToolTip="::LT_S0071" orgTooltip="Darkened Content" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel43" Text="::LT_S0064" orgText="Text" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="simpText" CssClass="middle SpectrumBranding" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                                <iaw:IAWTextbox runat="server" ID="simpLightText" CssClass="middle SpectrumBranding" style="display: inline" ToolTip="::LT_S0072" orgTooltip="Lightened Text" />
                                                            </td>
                                                        </tr>
                                                        <tr class="formrow">
                                                            <td class="formlabel">
                                                                <iaw:IAWLabel runat="server" ID="IAWLabel45" Text="::LT_S0073" orgText="Exceptions" />
                                                            </td>
                                                            <td class="formdata">
                                                                <iaw:IAWTextbox runat="server" ID="simpExceptions" CssClass="middle SpectrumBranding" ToolTip="::LT_S0074" orgTooltip="Exceptions Colour" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </iaw:IAWPanel>

                                            </ContentTemplate>
                                        </iaw:IAWTabPanel>
                                        <iaw:IAWTabPanel runat="server" ID="tpContents" HeaderText="::LT_S0165" orgHeaderText="Contents" CssClass="childTabScroll">
                                            <ContentTemplate>
                                                <table class="grp">

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel49" Text="::LT_S0069" orgText="Content Area" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="contentAreaBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel55" Text="::LT_S0075" orgText="Content Area Title" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="legendTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="legendBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="legendBorderColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel56" Text="::LT_S0077" orgText="Tabs" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="tabsTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="tabsBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel64" Text="::LT_S0078" orgText="Tabs (Hover)" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="tabsTextHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="tabsBgHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel13" Text="::LT_S0079" orgText="Tabs (Selected)" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="selectedTabsTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="selectedTabsBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0061" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel51" Text="::LT_S0080" orgText="Buttons" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="linkTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="linkBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel63" Text="::LT_S0081" orgText="Buttons (Hover)" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="linkTextHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="linkBgHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0061" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel53" Text="::LT_S0082" orgText="Input Fields" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="inptFieldTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="inptFieldBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="inptFieldBorderColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel54" Text="::LT_S0083" orgText="Input Fields (Focus)" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="inptFieldFocusBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel66" Text="::LT_S0084" orgText="Menu Items" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="menuItemTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="menuItemBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="lblMenuIcon" TranslateText="false" orgText="Menu Button &#9776;" /> <%-- This is now set in Page Load event --%>
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="menuButtonColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel42" Text="::LT_S0087" orgText="Menu Icon Colour" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="menuIconColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0088" orgTooltip="Icon Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel6" Text="::LT_S0089" orgText="Draggables" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="draggablesTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="draggablesBGColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0061" orgTooltip="Background Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="draggablesBorderColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <span>&nbsp;&nbsp;</span>
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel12" Text="::LT_S0090" orgText="Draggables (Hover)" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="draggablesTextHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="draggablesBGHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="draggablesBorderHoverColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel61" Text="::LT_S0091" orgText="Image Characters" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="imageCharactersColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" Tooltip="::LT_S0055" orgToolTip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="imageCharactersHighlightColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgToolTip="Hover Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel81" Text="::LT_S0093" orgText="Checkbox Colour" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="checkboxColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" ClientIDMode="Static" />
                                                        </td>
                                                    </tr>

                                                </table>

                                            </ContentTemplate>
                                        </iaw:IAWTabPanel>
                                        <iaw:IAWTabPanel runat="server" ID="tpLists" HeaderText="::LT_S0094" orgHeaderText="Lists" CssClass="childTabScroll">
                                            <ContentTemplate>
                                                <table class="grp">
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel57" Text="::LT_S0095" orgText="List Header" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="listHeaderTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="listHeaderBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel8" Text="::LT_S0061" orgText="List Header Icons" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="listHeaderIconsColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" ClientIDMode="Static" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="listHeaderIconsHoverColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgTooltip="Hover Colour" ClientIDMode="Static" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel11" Text="::LT_S0096" orgText="List Row" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="listRowColour" CssClass="middle SpectrumBranding" data-class="fa-solid  fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="listRowBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel28" Text="::LT_S0097" orgText="List Alt Row" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="listAltRowColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" ClientIDMode="Static" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="listAltRowBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" ClientIDMode="Static" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel58" Text="::LT_S0098" orgText="Selected Row" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="selectedRowTextColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="selectedRowBgColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>
                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel59" Text="::LT_S0099" orgText="Row Hover" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="selectedRowTextHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="selectedRowBgHoverColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                                        </td>
                                                    </tr>

                                                    <tr class="formrow">
                                                        <td class="formlabel">
                                                            <iaw:IAWLabel runat="server" ID="IAWLabel10" Text="::LT_S0100" orgText="List Icons" />
                                                        </td>
                                                        <td class="formdata">
                                                            <iaw:IAWTextbox runat="server" ID="listBodyIconsColour" CssClass="middle SpectrumBranding" data-class="fa-solid fa-font" ToolTip="::LT_S0086" orgTooltip="Colour" ClientIDMode="Static" />
                                                            <span>&nbsp;&nbsp;</span>
                                                            <iaw:IAWTextbox runat="server" ID="listBodyIconsHoverColour" CssClass="middle SpectrumBranding" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgTooltip="Hover Colour" ClientIDMode="Static" />
                                                        </td>
                                                    </tr>

                                                </table>

                                            </ContentTemplate>
                                        </iaw:IAWTabPanel>
                                    </ajaxToolkit:TabContainer>

                                </div>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpBackgrounds" HeaderText="::LT_S0101" orgHeaderText="Backgrounds" CssClass="parentTabScroll">
                <ContentTemplate>

                    <iaw:IAWGrid runat="server" ID="grdBackgrounds" AutoGenerateColumns="False"
                        DataKeyNames="unique_id" editState="Normal" SaveButtonId=""
                        ShowHeaderWhenEmpty="true"
                        TranslateHeadings="True" ClientIDMode="Static">

                        <Columns>
                            <iawb:IAWBoundField DataField="description" HeaderText="::LT_S0101" orgHeaderText="Backgrounds" ReadOnly="True" />
                            <iawb:IAWBoundField DataField="background_type_pt" HeaderText="::LT_S0102" orgHeaderText="Type" ReadOnly="True" />
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <iaw:IAWLabel runat="server" ID="IAWLabel28a" Text="::LT_S0103" orgText="Preview" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div style="display: flex; justify-content: center">
                                        <asp:Image ID="imgBGPreview" runat="server" CssClass="bgImagePreview" Visible="false" />
                                        <asp:Label ID="lblgradientBGPreview" runat="server" CssClass="bgGradientPreview" Visible="false" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                <HeaderTemplate>
                                    <iaw:IAWImageButton runat="server" ID="btnDataViewAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="::LT_S0039" orgTooltip="Add"  />
                                </HeaderTemplate>

                                <ItemTemplate>
                                    <div class="ActionButtons">
                                        <div><iaw:IAWHyperLinkButton ID="btnAmendRow" runat="server" CssClass="IconPic Icon16 IconEdit" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                        <div>
                                            <iaw:IAWHyperLinkButton ID="btnDeleteRow" runat="server" CssClass="IconPic Icon16 IconDelete" ToolTip="::LT_S0040" orgToolTip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
                                            <iaw:IAWLabel ID="lblNoDeleteRow" runat="server" CssClass="IconPic Icon16 IconBan" ToolTip="::LT_S0041" orgToolTip="In Use. Can not be deleted" Visible="false" />
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>

                        </Columns>
                    </iaw:IAWGrid>

                </ContentTemplate>
            </iaw:IAWTabPanel>

            <iaw:IAWTabPanel runat="server" ID="tpChart" HeaderText="::LT_S0104" orgHeaderText="Chart Defaults" CssClass="parentTabScroll">
                <ContentTemplate>
                    <ajaxToolkit:TabContainer ID="TabContainer3" runat="server" CssClass="iaw" style="margin-top: 5px">
                        <iaw:IAWTabPanel runat="server" ID="IAWTabPanel4" HeaderText="::LT_S0105" orgHeaderText="Chart" CssClass="childTabScroll">
                            <ContentTemplate>
                                <table class="grp">
                                    <tbody>
                                        <tr class="formrow">
                                            <td colspan="2" class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="IAWLabel5" Text="::LT_S0106" orgText="These Settings may be overridden" />
                                                <iaw:IAWCheckBox ID="cbSettigsFixed" runat="server" onchange="change_Charts()" />
                                            </td>
                                        </tr>
                                        <tr class="formrow">
                                            <td class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="IAWLabel34" Text="::LT_S0107" orgText="Background Type" />
                                            </td>
                                            <td class="formdata">
                                                <iaw:IAWDropDownList ID="ddlbBackgroundType" runat="server" ClientIDMode="Static" onchange="change_Charts(); backgroundTypeChanged()" />
                                            </td>
                                        </tr>

                                        <tr class="formrow" id="trBackgroundColour">
                                            <td class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="IAWLabel9" Text="::LT_S0056" orgText="Background Colour" />
                                            </td>
                                            <td class="formdata">
                                                <iaw:IAWTextbox runat="server" ID="txtModelbg" CssClass="middle SpectrumChart" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
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
                                                <iaw:IAWLabel runat="server" ID="IAWLabel48" Text="::LT_S0109" orgText="Background Gradient" />
                                            </td>
                                            <td class="formdata">
                                                <iaw:IAWDropDownList ID="ddlbBackgroundGradient" runat="server" ClientIDMode="Static" TranslateText="false" />
                                            </td>
                                        </tr>
                                        <tr class="formrow">
                                            <td class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="IAWLabel47" Text="::LT_S0110" orgText="Chart Direction" />
                                            </td>
                                            <td class="formdata">
                                                <iaw:IAWDropDownList runat="server" ID="ddlbDirection" ClientIDMode="Static" onchange="change_Charts()" TranslateText="true">
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
                                                <iaw:IAWTextbox runat="server" ID="txtlinkColour" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-solid fa-minus" ToolTip="::LT_S0086" orgTooltip="Colour" />
                                                <span>&nbsp;</span>
                                                <iaw:IAWTextbox runat="server" ID="txtLinkHover" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_S0058" orgTooltip="Hover Colour" />
                                            </td>
                                        </tr>

                                        <tr class="formrow">
                                            <td class="formlabel">
                                                <iaw:IAWLabel runat="server" ID="IAWLabel32" Text="::LT_S0118" orgText="Tooltip Colours" />
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
                                                            <iaw:IAWTextbox runat="server" ID="numLinkWidth" onchange="change_Charts()" />
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
                                                <iaw:IAWDropDownList runat="server" ID="ddlbLinkStyle" Font-Names="monospace" TranslateText="false" onchange="change_Charts()">
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
                        <iaw:IAWTabPanel runat="server" ID="IAWTabPanel5" HeaderText="::LT_S0121" orgHeaderText="Nodes" CssClass="childTabScroll">
                            <ContentTemplate>
                                <table class="grp">
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel2" Text="::LT_S0117" orgText="Colours" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox runat="server" ID="txtNodefg" CssClass="middle SpectrumChart" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtNodeTxtBg" CssClass="middle SpectrumChartEmpty" ClientIDMode="Static" data-class="fa-solid fa-font white-on-black" ToolTip="::LT_S0122" orgTooltip="Text Background Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtNodebg" CssClass="middle SpectrumChart" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtNodeBorder" CssClass="middle SpectrumChart" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
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
                                            <iaw:IAWTextbox runat="server" ID="txtHighlightfg" CssClass="middle SpectrumChart" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtHighlightbg" CssClass="middle SpectrumChart" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtHighlightbBorder" CssClass="middle SpectrumChart" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                        </td>
                                    </tr>

                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel72" Text="::LT_S0125" orgText="Tooltip" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox runat="server" ID="txtTTfg" CssClass="middle SpectrumChart" data-class="fa-solid fa-font" ToolTip="::LT_S0055" orgTooltip="Text Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtTTbg" CssClass="middle SpectrumChart" data-class="fa-solid fa-paint-roller" ToolTip="::LT_S0056" orgTooltip="Background Colour" />
                                            <span>&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtTTBorder" CssClass="middle SpectrumChart" data-class="fa-regular fa-square" ToolTip="::LT_S0076" orgTooltip="Border Colour" />
                                        </td>
                                    </tr>

                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWCheckBox ID="cbShowShadow" data-target="#txtShadow" runat="server" ClientIDMode="Static" ToolTip="::LT_S0126" orgTooltip="Show a shadow" Text="::LT_S0127" orgText="Show Shadow" TextAlign="Left" CssClass="middle" onchange="change_Charts()" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox runat="server" ID="txtShadow" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-solid fa-cube" ToolTip="::LT_S0076" orgTooltip="Shadow Colour" />
                                        </td>
                                    </tr>

                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel7" Text="::LT_S0088" orgText="Icon Colour" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWTextbox runat="server" ID="txtIconfg" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-solid fa-font" ToolTip="::LT_S0088" orgTooltip="Icon Colour" />
                                            <span>&nbsp;&nbsp;</span>
                                            <iaw:IAWTextbox runat="server" ID="txtIconHover" CssClass="middle SpectrumChart" ClientIDMode="Static" data-class="fa-regular fa-hand-pointer" ToolTip="::LT_A0066" orgToolTip="Icon Hover" />
                                        </td>
                                    </tr>

                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel73" Text="::LT_S0129" orgText="Corners" />
                                        </td>
                                        <td class="formdata">
                                            <iaw:IAWRadioButton ID="rbCornerRectangle" runat="server" Text="::LT_S0130" orgText="Square" GroupName="boxNodeCorners" ClientIDMode="Static" onchange="change_Charts()" />
                                            <iaw:IAWRadioButton ID="rbCornerRoundedRectangle" runat="server" Text="::LT_S0131" orgText="Rounded" GroupName="boxNodeCorners" ClientIDMode="Static" onchange="change_Charts()" />
                                        </td>
                                    </tr>
                                    <tr class="formrow">
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel3" Text="::LT_S0132" orgText="Max Width" />
                                        </td>
                                        <td class="formdata">
                                            <table border="0">
                                                <tr>
                                                    <td>
                                                        <iaw:IAWTextbox runat="server" ID="numMaxNodeWidth" AutoPostBack="true" onchange="change_Charts()" />
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
                                            <iaw:IAWLabel runat="server" ID="IAWLabel1" Text="::LT_S0133" orgText="Default Width" />
                                        </td>
                                        <td class="formdata">
                                            <table border="0">
                                                <tr>
                                                    <td>
                                                        <iaw:IAWTextbox runat="server" ID="numNodeWidth" onchange="change_Charts()" />
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
                                            <iaw:IAWLabel runat="server" ID="IAWLabel4" Text="::LT_S0134" orgText="Max Height" />
                                        </td>
                                        <td class="formdata">
                                            <table border="0">
                                                <tr>
                                                    <td>
                                                        <iaw:IAWTextbox runat="server" ID="numMaxNodeHeight" AutoPostBack="true" onchange="change_Charts()" />
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
                                                        <iaw:IAWTextbox runat="server" ID="numNodeHeight" onchange="change_Charts()" />
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
                        <iaw:IAWTabPanel runat="server" ID="IAWTabPanel6" HeaderText="::LT_S0136" orgHeaderText="Text Fonts" CssClass="childTabScroll">
                            <ContentTemplate>
                                <table border="0">
                                    <tr>
                                        <td class="formlabeldisplay">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel14" Text="::LT_S0137" orgText="Lines" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine1" onchange="change_Charts()"></label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel15" Text="" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine2"></label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel18" Text="" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine3" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel20" Text="" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine4" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel21" Text="" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine5" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="formlabel">
                                            <iaw:IAWLabel runat="server" ID="IAWLabel22" Text="" />
                                        </td>
                                        <td class="formdata">
                                            <label id="lblNodeLine6" />
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </iaw:IAWTabPanel>

                    </ajaxToolkit:TabContainer>
                </ContentTemplate>
            </iaw:IAWTabPanel>

        </ajaxToolkit:TabContainer>

        <br />
        <iaw:IAWHyperLinkButton runat="server" ID="btnGlobalSave" ClientIDMode="Static" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="DefAttr" OnClientClick="return RestringData();" IconClass="iconButton fa-solid fa-floppy-disk" />
        <iaw:IAWHyperLinkButton runat="server" ID="btnGlobalCancel" ClientIDMode="Static" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />

    </iaw:IAWPanel>


    <%-- Hidden fields to contain keys --%>

    <asp:HiddenField runat="server" ID="hdnBrandID" />

    <%-- Brand Popup Form --%>

    <iaw:IAWPanel ID="popBrand" runat="server" GroupingText="::LT_S0038" orgGroupingText="Brand" Style="display: none" CssClass="PopupPanel" DefaultButton="btnBrandSave">
        <iaw:wuc_help runat="server" ID="helplink_popBrand" Reference="SettingsBrand" />

        <table border="0" class="listform">

            <tr class="formrow">
                <td class="formlabel">
                    <iaw:IAWLabel ID="IAWLabel17" runat="server" Text="::LT_S0139" orgText="Brand Name" />
                </td>
                <td class="formdata">
                    <iaw:IAWTextbox ID="txtBrandName" runat="server" ClientIDMode="Static" MaxLength="50" Width="400px" CssClass="formcontrol" ValidationGroup="Brand" />
                    <iaw:IAWRequiredFieldValidator ControlToValidate="txtBrandName" ID="RequiredFieldValidator99" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="Brand" Display="Dynamic" AddBR="true" />
                    <iaw:IAWCustomValidator ID="cvBrandName" runat="server" ControlToValidate="txtBrandName" ErrorMessage="::LT_S0141" orgErrorMessage="Brand name already in use" ValidationGroup="Brand" Display="Dynamic" AddBR="true" />
                </td>
            </tr>

            <tr class="formrow">
                <td class="formlabel" style="padding-top: 20px">
                    <iaw:IAWLabel ID="IAWLabel30" runat="server" Text="::LT_S0142" orgText="Default Brand" />
                </td>
                <td class="formdata" style="padding-top: 20px">
                    <iaw:IAWCheckBox ID="cbDefaultBrand" runat="server" />
                </td>
            </tr>

            <tr>
                <td colspan="2" align="center">
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBrandSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="Brand" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <span>&nbsp; &nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBrandCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>

        </table>

    </iaw:IAWPanel>

    <asp:Button ID="btnBrandFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeBrandForm" BackgroundCssClass="ModalBackground" PopupControlID="popBrand" TargetControlID="btnBrandFake" />

    <%-- pop to Copy Details from --%>

    <iaw:IAWPanel ID="popTheme" runat="server" GroupingText="::LT_S0143" orgGroupingText="Copy From" CssClass="PopupPanel" Style="display: none">
        <ajaxToolkit:TabContainer ID="tcTheme" runat="server" CssClass="iaw">
            <iaw:IAWTabPanel runat="server" ID="tpCopyFromBrands" HeaderText="::LT_S0144" orgHeaderText="My Brands" CssClass="themeTabScroll">
                <ContentTemplate>
                    <iaw:IAWRadioButtonList runat="server" ID="rblBrand" TranslateText="false" />
                </ContentTemplate>
            </iaw:IAWTabPanel>
            <iaw:IAWTabPanel runat="server" ID="tpCopyFromThemes" HeaderText="::LT_S0145" orgHeaderText="Themes" CssClass="themeTabScroll">
                <ContentTemplate>

                    <iaw:IAWGrid runat="server" ID="grdTheme" AutoGenerateColumns="False"
                        DataKeyNames="theme_id" editState="Normal" SaveButtonId=""
                        ShowHeaderWhenEmpty="true"
                        TranslateHeadings="True" ClientIDMode="Static">

                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:RadioButton ID="rbSelect" runat="server" CssClass="selectThemeRadioButton" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="::LT_S0117">  <%-- LT_S0117 = "Colours" --%>
                                <ItemTemplate>
                                    <asp:Placeholder ID="phColours" runat="server"></asp:Placeholder>                                    
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </iaw:IAWGrid>

                </ContentTemplate>
            </iaw:IAWTabPanel>
        </ajaxToolkit:TabContainer>
        <iaw:IAWLabel runat="server" ID="IAWLabel44" Text="::LT_S0146" orgText="This will replace the Brand's current Settings" style="line-height: 32px; font-size: 0.8em; font-style: italic" />
        <br />
        <iaw:IAWHyperLinkButton runat="server" ID="btnThemeSave" Text="::LT_S0147" orgText="OK" CausesValidation="false" ToolTip="::LT_S0148" orgTooltip="Copy Colours" IconClass="iconButton fa-solid fa-circle-check" />
        <iaw:IAWHyperLinkButton runat="server" ID="btnThemeCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" ToolTip="::LT_S0138" orgTooltip="Cancel" IconClass="iconButton fa-solid fa-circle-xmark" />
    </iaw:IAWPanel>

    <asp:Button ID="btnThemeFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeThemeForm" BackgroundCssClass="ModalBackground" PopupControlID="popTheme" TargetControlID="btnThemeFake" />

    <%-- pop to select a background --%>
    <asp:HiddenField runat="server" ID="hdnClientUnique" />
    <iaw:IAWPanel ID="popBackground" runat="server" ClientIDMode="Static" GroupingText="::LT_S0149" orgGroupingText="Background" CssClass="PopupPanel" Style="display: none">
        <table border="0" class="listform">
            <tr>
                <td>
                    <table border="0" class="listform">
                        <tr class="formrow">
                            <td class="formlabel">
                                <iaw:IAWLabel runat="server" ID="IAWLabel24" Text="::LT_S0107" orgText="Background Type" />
                            </td>
                            <td class="formdata">
                                <iaw:IAWRadioButton ID="rbBgSelectGradient" runat="server" Text="::LT_S0150" orgText="Gradient" GroupName="backgroundTypeSelection" onchange="bgTypeSelection('gradient')" ClientIDMode="Static" Checked="true" />
                                <iaw:IAWRadioButton ID="rbBgSelectImage" runat="server" Text="::LT_S0151" orgText="Image" GroupName="backgroundTypeSelection" onchange="bgTypeSelection('image')" ClientIDMode="Static" />
                            </td>
                        </tr>
                        <tr>
                            <td class="formlabel">
                                <iaw:IAWLabel ID="IAWLabel27" runat="server" Text="::LT_S0010" orgText="Name"></iaw:IAWLabel>
                            </td>
                            <td class="formdata">
                                <iaw:IAWTextbox ID="txtBgName" runat="server" ClientIDMode="Static" CssClass="formcontrol" oninput="bgNameInput()" ValidationGroup="bgName" />
                                <iaw:IAWRequiredFieldValidator ControlToValidate="txtBgName" ID="RequiredFieldValidator2" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="bgName" Display="Dynamic" AddBR="true" />
                                <iaw:IAWCustomValidator ID="cvBgName" runat="server" ControlToValidate="txtBgName" ErrorMessage="::LT_S0152" orgErrorMessage="Background name already in use" ValidationGroup="bgName" Display="Dynamic" AddBR="true" />
                            </td>
                        </tr>
                    </table>
                    <div id="divBgGradient" class="bgTypeContainer" runat="server" clientidmode="Static">
                        <div class="list-area">
                            <div>
                                <iaw:IAWLabel runat="server" ID="IAWLabel25" Text="::LT_S0153" orgText="Colour List" />
                                <ul class="gradient-color-list"></ul>
                                <iaw:IAWHyperLinkButton runat="server" ID="btnAddNewColour" Text="::LT_S0154" orgText="Add new colour" CausesValidation="false" ClientIDMode="Static" ToolTip="::LT_S0154" orgTooltip="Add new colour" Style="padding: 6px; margin: 0" />
                            </div>
                            <div class="gradient-preview"></div>
                        </div>
                        <div class="gradient-controls">
                            <div>
                                <iaw:IAWRadioButton ID="rbLinearGradient" runat="server" Text="::LT_S0155" orgText="Linear" GroupName="gradientType" onchange="gradientTypeChange('linear')" ClientIDMode="Static" Checked="true" />
                                <iaw:IAWRadioButton ID="rbRadialGradient" runat="server" Text="::LT_S0156" orgText="Radial" GroupName="gradientType" onchange="gradientTypeChange('radial')" ClientIDMode="Static" />
                                <div class="angle-selector">
                                    <input type="range" onchange="bgChange()" id="angle" step="5" min="0" max="360" value="90" />
                                    <span>90</span>&deg;
                                </div>
                            </div>
                        </div>
                    </div>
                    <div id="divBgImage" class="bgTypeContainer" hidden="hidden" runat="server" clientidmode="Static">
                        <table style="margin-bottom: 6px;">
                            <tr>
                                <td class="formdata">
                                    <iaw:IAWLabel ID="IAWLabel29" runat="server" Text="::LT_S0157" orgText="Repeat"></iaw:IAWLabel>
                                </td>
                                <td class="formdata">
                                    <asp:DropDownList ID="ddlbImageRepeatType" runat="server" onchange="bgImageRepeatTypeChange()"></asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                        <iaw:IAWHyperLinkButton runat="server" ID="btnBgImageSelector" OnClientClick="document.getElementById('FileUpload2').click(); return false;" Text="::LT_S0158" orgText="Choose Image" Style="padding: 6px; margin: 0; margin-left: 12px;" />
                        <span>&nbsp;&nbsp;</span>
                        <iaw:IAWLabel runat="server" ID="bgImageLabel" ClientIDMode="Static" Text="::LT_S0046" orgText="No File Chosen" CssClass="threeDotsLabel" Style="font-size: small" />
                        <span>&nbsp;&nbsp;</span>
                        <asp:FileUpload runat="server" ID="FileUpload2" accept="image/*" onchange="bgImageChange(this)" ClientIDMode="Static" Style="display: none;" />
                        <div class="bgImgPreview"></div>
                    </div>
                </td>
            </tr>
            <tr class="trPopBackground">
                <td align="center">
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBackgroundSave" ClientIDMode="static" Text="::LT_S0005" orgText="Save" CausesValidation="true" OnClientClick="return RestringBackgroundData();" ToolTip="::LT_S0005" orgTooltip="Save" ValidationGroup="bgName" IconClass="iconButton fa-solid fa-floppy-disk" />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBackgroundCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" ToolTip="::LT_S0138" orgTooltip="Cancel" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnBackgroundFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeBackgroundForm" BackgroundCssClass="ModalBackground" ClientIDMode="Static" PopupControlID="popBackground" TargetControlID="btnBackgroundFake" />

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

    <%-- Delete Brand Confirm Popup --%>
    <iaw:IAWPanel ID="popBrandDelete" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnBrandDeleteCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel26" runat="server" Text="::LT_S0159" orgText="Are you sure you want to delete this entry?" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBrandDeleteOk" Text="::LT_S0033" orgText="Yes" CausesValidation="false" ToolTip="::LT_S0160" orgTooltip="Delete entry" IconClass="iconButton fa-solid fa-circle-check" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnBrandDeleteCancel" Text="::LT_S0034" orgText="No" CausesValidation="false" ToolTip="::LT_S0161" orgTooltip="Do not delete entry" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnBrandDeleteFake" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeBrandDelete" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popBrandDelete" TargetControlID="btnBrandDeleteFake" />

    <%-- simplified Confirm Popup --%>
    <iaw:IAWPanel ID="popSimplifiedWarning" runat="server" GroupingText="::LT_S0031" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnSimplifiedWarningCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel46" runat="server" Text="::LT_S0162" orgText="By switching to simplified mode, you will lose any advanced colours. Do you still wish to continue?" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnSimplifiedWarningOk" Text="::LT_S0033" orgText="Yes" CausesValidation="false" ToolTip="::LT_S0163" orgTooltip="Change to simplified mode" IconClass="iconButton fa-solid fa-circle-check" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnSimplifiedWarningCancel" Text="::LT_S0034" orgText="No" CausesValidation="false" ToolTip="::LT_S0164" orgTooltip="No, stay in advanced mode" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="btnSimplifiedWarningFake" ClientIDMode="Static" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeSimplifiedWarning" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popSimplifiedWarning" TargetControlID="btnSimplifiedWarningFake" />

</asp:Content>
