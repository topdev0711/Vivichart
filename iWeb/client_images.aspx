<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="client_images.aspx.vb" Inherits=".client_images" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">

    <script src="./Library/cropper.js"></script>
    <script src="./client_images.js"></script>

    <link rel="stylesheet" href="./Library/cropper.css" />
    <link rel="stylesheet" href="./client_images.css" />

    <script type="text/javascript">
        var txtURfilterUniqueID = '<%= txtURfilter.UniqueID %>';
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField ID="hdnImage" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hdnIsNewImage" runat="server" ClientIDMode="Static" />
    <asp:Button ID="btnSavePostback" runat="server" ClientIDMode="Static" Style="display: none;" />
    <input type="file" accept="image/*" id="btnHiddenUpload" class="upload-hidden-button hidden" />

    <iaw:IAWPanel runat="server" ID="pnlNoData" GroupingText= "::LT_S0006" TranslateText="true" orgGroupingText="No Data Sources" CssClass="PanelClass" Visible="false">
        <div class="PanelPadding">
            <iaw:IAWLabel runat="server" ID="IAWLabel1" CssClass="NoRows" Text= "::LT_S0007" TranslateText="true" orgText="There are currently no Data Sources available" />
            <br />
            <br />
            <iaw:IAWLabel runat="server" ID="IAWLabel2" CssClass="NoRows" Text= "::LT_S0008" TranslateText="true" orgText="They will only be included if Show Images is enabled" />
        </div>
    </iaw:IAWPanel>

    <table runat="server" id="tblOuter">
        <tr>
            <td style="vertical-align: top">
                <iaw:IAWPanel runat="server" ID="pnlDataSource" GroupingText= "::LT_S0009" TranslateText="true" orgGroupingText="Data Source" CssClass="PanelClass">
                    <div id="divDatasources" class="divScroll grid-to-bottom">

                        <iaw:IAWGrid runat="server" ID="grdDatasource" AutoGenerateColumns="False" DataKeyNames="source_id" editState="Normal" ShowHeader="false">
                            <Columns>
                                <iawb:IAWBoundField DataField="short_ref" HeaderText= "::LT_S0010" orgHeaderText="Name" ReadOnly="True" ItemStyle-CssClass="grdField" />
                                <asp:TemplateField ItemStyle-CssClass="ActionCell">
                                    <ItemTemplate>
                                        <div class="ActionButtons">
                                            <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgToolTip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
                                        </div>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                                </asp:TemplateField>
                            </Columns>
                        </iaw:IAWGrid>

                    </div>
                </iaw:IAWPanel>
            </td>

            <td style="vertical-align: top">

                <iaw:IAWPanel runat="server" ID="pnlDatasourceImageList" GroupingText= "" TranslateText="false" CssClass="PanelClass">
                    <iaw:IAWHyperLinkButton runat="server" ID="btnSortAsc" CssClass="sort-icons content-icon fa-solid fa-arrow-up-wide-short" ToolTip= "::LT_S0012" TranslateTooltip="true" orgTooltip="Sorted Ascending" />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnSortDesc" CssClass="sort-icons content-icon fa-solid fa-arrow-down-short-wide" visible="false" ToolTip= "::LT_S0013" TranslateTooltip="true" orgTooltip="Sorted Descending" />
                    <asp:DropDownList ID="ddlbSort" runat="server" AutoPostBack="true" />
                    <br />
                    <iaw:IAWTextbox runat="server" TextMode="search" AutoPostBack="True" class="SearchBox" ID="txtURfilter" placeholder="::LT_A0039" orgPlaceholder="search" ClientIDMode="static" />
                    <div id="divList" class="divScroll grid-to-bottom">

                        <iaw:IAWGrid runat="server" ID="grdDatasourceImageList" AutoGenerateColumns="False" DataKeyNames="line_no" ShowHeader="false" editState="Normal">
                            <Columns>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Image ID="imgPhoto" runat="server" />
                                        <iaw:IAWLabel ID="lblImageMissing" runat="server" CssClass="missingImage" ToolTip= "::LT_S0014" TranslateTooltip="true" orgTooltip="No Image" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <table style="width: 100%;">
                                            <tr>
                                                <td>
                                                    <%# Eval("item_display1") %>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <%# Eval("item_display2") %>
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <iaw:IAWLabel runat="server" ID="lblModified" TranslateText="false" TranslateTooltip="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <EmptyDataTemplate>
                                <iaw:IAWLabel runat="server" ID="grdSetRows" Text= "::LT_S0015" TranslateText="true" orgText="There is nothing to display yet" CssClass="NoWrap" />
                            </EmptyDataTemplate>

                            <RowStyle Wrap="false" />
                            <HeaderStyle Wrap="false" />
                        </iaw:IAWGrid>

                    </div>
                </iaw:IAWPanel>
            </td>
            <td style="vertical-align: top">
                <iaw:IAWPanel runat="server" ID="pnlImage" GroupingText="Image" CssClass="PanelClass" TranslateText="false">

                    <div class="modal" id="model-container">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-footer cropper-controls">
                                    <iaw:IAWLabel runat="server" ID="btnCropSquare" CssClass="content-icon fa-regular fa-square cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0016" TranslateTooltip="true" orgTooltip="Square Cropping" />
                                    <iaw:IAWLabel runat="server" ID="btnCropCircle" CssClass="content-icon fa-regular fa-circle cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0017" TranslateTooltip="true" orgTooltip="Circular Cropping" />
                                    <div style="padding-right: 10px;"></div>
                                    <iaw:IAWLabel runat="server" ID="btnRotateLeft" CssClass="content-icon fa-solid fa-rotate-left cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0018" TranslateTooltip="true" orgTooltip="Rotate Left" />
                                    <iaw:IAWLabel runat="server" ID="btnRotateRight" CssClass="content-icon fa-solid fa-rotate-right cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0019" TranslateTooltip="true" orgTooltip="Rotate Right" />
                                    <div style="padding-right: 10px;"></div>
                                    <iaw:IAWLabel runat="server" ID="btnFlipHorizontal" CssClass="content-icon fa-solid fa-arrows-left-right cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0020" TranslateTooltip="true" orgTooltip="Flip Horizontally" />
                                    <iaw:IAWLabel runat="server" ID="btnFlipVertical" CssClass="content-icon fa-solid fa-arrows-up-down cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0021" TranslateTooltip="true" orgTooltip="Flip Vertically" />
                                    <asp:Panel runat="server" ID="divAccButtons" ClientIDMode="Static" Style="margin-top: unset; margin-block: unset">
                                        <iaw:IAWLabel runat="server" ID="btnImageSave" CssClass="fa-regular fa-floppy-disk banner-icon flasher" ClientIDMode="Static" ToolTip= "::LT_S0005" TranslateTooltip="true" orgTooltip="Save" />
                                        <iaw:IAWLabel runat="server" ID="btnImageCancel" CssClass="fa-solid fa-arrow-rotate-left banner-icon flasher" ClientIDMode="Static" ToolTip= "::LT_A0054" TranslateTooltip="true" orgTooltip="Undo Changes" />
                                    </asp:Panel>
                                </div>

                                <div class="modal-body" id="modal-body">
                                    <div class="image-drop-area" id="image-drop-area">
                                        <iaw:IAWLabel runat="server" ID="lblDropHere" ToolTip= "::LT_S0023" Text="::LT_S0024" TranslateText="true" TranslateTooltip="true" orgTooltip="Drop an image here" orgText="Drop Here" />
                                    </div>
                                    <!-- image display here -->
                                    <div class="cropper-area" id="image-display-area">
                                        <div class="image-display">
                                            <img src="" alt="" id="image-display" />
                                        </div>
                                    </div>
                                    <!-- image cropper here -->
                                    <div class="cropper-area" id="image-crop-area">
                                        <div id="custom-drop-area">
                                            <span>Drop Here</span>
                                        </div>
                                        <div class="image-workspace">
                                            <img src="" alt="" id="image-crop" style="width: 270px; height: 270px" />
                                        </div>
                                        <span>&nbsp;&nbsp;</span>
                                        <div id="image-preview-container">
                                            <img id="image-preview" alt="Preview" />
                                        </div>
                                    </div>
                                </div>

                                <div id="divEditMode" class="modal-footer cropper-controls">
                                    <iaw:IAWLabel runat="server" ID="btnImageUpload" CssClass="content-icon fa-solid fa-upload cropper-icon" ClientIDMode="Static" ToolTip= "::LT_S0027" TranslateTooltip="true" orgTooltip="Upload Image" />
                                    <div>
                                        <iaw:IAWLabel runat="server" ID="IAWLabel5" Text= "::LT_S0028" TranslateText="true" orgText="Upload an image or drag it above" Style="font-size: 0.8em;" />
                                        <br />
                                        <iaw:IAWLabel runat="server" ID="lblUploadedDate" Text="" Style="font-size: 0.7em; font-weight: bold" TranslateText="false" />
                                    </div>
                                </div>
                                <iaw:IAWHyperLinkButton runat="server" ID="btnRemoveImage" Text= "::LT_S0029" TranslateText="true" orgText="Remove Image" IconClass="iconButton fa-solid fa-trash" ClientIDMode="Static" ToolTip= "::LT_S0029" TranslateTooltip="true" orgTooltip="Remove Image" />
                                <iaw:IAWHyperLinkButton runat="server" ID="btnRestoreImage" Text= "::LT_S0030" TranslateText="true" orgText="Restore Original Image" IconClass="iconButton fa-solid fa-repeat" ClientIDMode="Static" ToolTip= "::LT_S0030" TranslateTooltip="true" orgTooltip="Restore Original Image" />
                            </div>
                        </div>
                    </div>
                </iaw:IAWPanel>
            </td>

        </tr>

    </table>

    <%-- Remove Image Confirm Popup --%>
    <iaw:IAWPanel ID="popRemoveImage" runat="server" GroupingText= "::LT_S0031" TranslateText="true" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnRemoveImageCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel4" runat="server" Text= "::LT_S0032" TranslateText="true" orgText="Are you sure you want to remove this image?" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRemoveImageOk" Text= "::LT_S0033" TranslateText="true" orgText="Yes" CausesValidation="false" ToolTip= "::LT_S0033" TranslateTooltip="true" orgTooltip="Yes" IconClass="iconButton fa-solid fa-circle-check" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRemoveImageCancel" Text= "::LT_S0034" TranslateText="true" orgText="No" CausesValidation="false" ToolTip= "::LT_S0034" TranslateTooltip="true" orgTooltip="No" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="popRemoveImageFake" ClientIDMode="Static" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeRemoveImage" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popRemoveImage" TargetControlID="popRemoveImageFake" />

    <%-- Restore Original Image Confirm Popup --%>
    <iaw:IAWPanel ID="popRestoreImage" runat="server" GroupingText= "::LT_S0031" TranslateText="true" orgGroupingText="Confirm" Style="display: none"
        CssClass="PopupPanel" DefaultButton="btnRestoreImageCancel">
        <table border="0" class="listform">
            <tr>
                <td>
                    <iaw:IAWLabel ID="IAWLabel46" runat="server" Text= "::LT_S0035" TranslateText="true" orgText="Are you sure you want to restore the original image?" />
                </td>
            </tr>
            <tr>
                <td align="center">
                    <br />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRestoreImageOk" Text= "::LT_S0033" TranslateText="true" orgText="Yes" CausesValidation="false" ToolTip= "::LT_S0033" TranslateTooltip="true" orgTooltip="Yes" IconClass="iconButton fa-solid fa-circle-check" />
                    <span>&nbsp;&nbsp;</span>
                    <iaw:IAWHyperLinkButton runat="server" ID="btnRestoreImageCancel" Text= "::LT_S0034" TranslateText="true" orgText="No" CausesValidation="false" ToolTip= "::LT_S0034" TranslateTooltip="true" orgTooltip="No" IconClass="iconButton fa-solid fa-circle-xmark" />
                </td>
            </tr>
        </table>
    </iaw:IAWPanel>

    <asp:Button ID="popRestoreImageFake" ClientIDMode="Static" runat="server" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeRestoreImage" ClientIDMode="Static" BackgroundCssClass="ModalBackground" PopupControlID="popRestoreImage" TargetControlID="popRestoreImageFake" />

    <script type="text/javascript">
        $(function () {
            $('#txtURfilter').on('input', function () {
                if (!this.value) {
                    __doPostBack(txtURfilterUniqueID, '');
                }
            })
        });
    </script>

</asp:Content>
