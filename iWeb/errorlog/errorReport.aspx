<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" ValidateRequest="false"
    Inherits="errorReport" Title="IAW Error Log" CodeBehind="errorReport.aspx.vb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style type="text/css">
        .ErrorPanel {
            min-width: 875px;
            max-width: 875px;
            width: 875px;
            min-height: 450px;
            max-height: 450px;
            height: 450px;
            overflow-y: auto;
        }
        fieldset {
          max-width:fit-content !important;
        }
    </style>
    <script>
        function copyToClipboard() {
            const textToCopy = $("#txtErrorInfo").val(); // Get the value you want to copy to clipboard

            navigator.clipboard.writeText(textToCopy).then(() => {
                $("#labErrorCopied").removeClass("hidden");
                setTimeout(() => {
                    $("#labErrorCopied").addClass("hidden");
                }, 3000);
            }).catch(err => {
                console.error("Failed to copy text: ", err);
            });
        }

    </script>
</asp:Content>

<asp:Content ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

            <iaw:IAWPanel runat="server" ID="pnlFiles" CssClass="PanelClass" GroupingText="Errors" TranslateText="false">

                <iaw:paging runat="server" GridID="grdViewErrorLog" PageSize="20" ID="pagingGrid" />

                <asp:UpdatePanel runat="server" ID="UPD_grid">
                    <ContentTemplate>
                        <iaw:IAWGrid runat="server"
                            ID="grdViewErrorLog"
                            Width="98%"
                            AutoGenerateColumns="false"
                            DataSourceID="ODS1"
                            DataKeyNames="wel_eventid"
                            SaveButtonId="SaveButton"
                            AllowPaging="false"
                            AllowSorting="true"
                            TranslateHeadings="false"
                            editState="Normal">
                            <Columns>
                                <iawb:IAWGridCheckBoxColumn AllowCheckAll="true" DataField="Wel_eventid" HeaderText="Select" ItemStyle-HorizontalAlign="center" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <iaw:IAWHyperLinkButton runat="server" ID="btnView" Text="View" ToolTip="View Error Detail" IconClass="iconButton fa-solid fa-eye"
                                            CommandName="ViewError" TranslateText="false" TranslateTooltip="false"
                                            CommandArgument='<%# Eval("wel_eventid").ToString %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="wel_eventid" SortExpression="wel_eventid" HeaderText="ID" />
                                <asp:BoundField DataField="wel_datetime" SortExpression="wel_datetime" HeaderText="Date and Time" ItemStyle-Wrap="false" HtmlEncode="False" DataFormatString="{0:dd MMM yyyy HH:mm:ss}" />
                                <asp:BoundField DataField="wel_message" SortExpression="wel_message" HtmlEncode="False" HeaderText="Error Message" ItemStyle-HorizontalAlign="Left" />
                                <asp:BoundField DataField="wel_user_ref" SortExpression="wel_user_ref" HeaderText="User Ref" ItemStyle-Wrap="false" />
                            </Columns>
                            <EmptyDataTemplate>
                                <iaw:IawMessage runat="server" ID="msg_grid" ShowIcon="true" Icon="information" MessageType="information" TranslateText="false" MessageText="No errors!" />
                            </EmptyDataTemplate>
                        </iaw:IAWGrid>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <asp:ObjectDataSource ID="ODS1"
                    runat="server"
                    TypeName="WebErrorLog_DAL"
                    DeleteMethod="DeleteItem"
                    SelectMethod="GetLogPage">
                    <SelectParameters>
                        <iaw:PageParameter PropertyName="PageIndex" Name="pageIndex" Type="Int32" DefaultValue="1" />
                        <iaw:PageParameter PropertyName="PageSize" Name="pageSize" Type="Int32" DefaultValue="25" />
                        <asp:Parameter Name="totalRecords" Type="Int32" DefaultValue="-1" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </iaw:IAWPanel>

            <iaw:IAWPanel ID="popError" runat="server" GroupingText="Error Detail" Style="display: none" CssClass="PopupPanel" DefaultButton="btnErrorCancel" TranslateText="false">
                <div>
                    <iaw:IAWTextbox runat="server" ID="txtErrorInfo" TextMode="MultiLine" CssClass="ErrorPanel" ClientIDMode="Static" />
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btnErrorCancel" Text="Close" ToolTip="Close" TranslateText="false" TranslateTooltip="false" IconClass="iconButton fa-solid fa-circle-xmark" />
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <iaw:IAWHyperLinkButton runat="server" ID="btnErrorCopy" Text="Copy to Clipboard" ToolTip="Copy to Clipboard" 
                         TranslateText="false" TranslateTooltip="false"
                         OnClientClick="copyToClipboard(); return false;"/>
                    &nbsp;&nbsp;&nbsp;
                    <asp:Label runat="server" ID="labErrorCopied" ClientIDMode="Static" Text="Copied" CssClass="hidden" />
                </div>
            </iaw:IAWPanel>

            <asp:Button ID="btnErrorFake" runat="server" Style="display: none" />
            <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeError" BackgroundCssClass="ModalBackground"
                PopupControlID="popError" TargetControlID="btnErrorFake" />

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
