<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master"
  CodeBehind="Translations.aspx.vb" Inherits=".Translations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
  <style type="text/css">
    .grdField {
      text-align: left;
      vertical-align: middle;
      cursor: pointer;
      font-weight: bold;
      padding-left: 5px;
      padding-right: 5px;
    }

    .NoRows {
      padding: 3px 5px 3px 5px;
      text-align: left;
      white-space: nowrap;
      vertical-align: middle;
      font-weight: bold;
    }

    .Grid-Indent {
      margin-left: 100px;
    }

    .lang_icon {
      min-width: 20px;
    }

    .lang_english_label, .lang_foreign_label {
      vertical-align: top;
      text-align: left;
      font-weight: normal;
      white-space: nowrap;
      font-size: medium;
      padding-right: 15px;
    }

    .lang_english, .lang_foreign {
      text-align: left;
/*      max-width: 500px;
      width: 500px;*/
      white-space: normal;
      font-weight: normal;
      font-size: medium;
    }

    .lang_foreign_label {
      color: var(--translations-lang-foreign-color);
    }

    .lang_foreign {
      color: var(--translations-lang-foreign-color);
    }

    .multiline_input {
      width: 500px;
      color: var(--translations-multiline-input-color);
      font-size: medium;
    }

    .context_link {
      vertical-align: top;
      cursor: pointer;
      text-decoration: none;
    }

    .large {
      font-size: large;
    }

    .context_link:hover {
      color: var(--translations-context-link-hover-color);
      text-decoration: none;
    }

    .divScroll {
      overflow-y: auto;
    }
  </style>

  <script type="text/javascript">
    // if the user changes the size of the edit foreign text box, then relayout the modal
    $(document).ready(function () {
      var resizing = false;

      $('#txtForeignText').on('mousedown', function (e) {
        if ($(e.target).is('#txtForeignText')) {
          resizing = true;
        }
      });

      $(document).on('mouseup', function () {
        if (resizing) {
          resizing = false;
          setTimeout(function () {
            var modalPopupBehavior = $find('mpeForeignTextForm');
            if (modalPopupBehavior) {
              modalPopupBehavior._layout();
            }
          }, 50);
        }
      });

    });
  </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <asp:HiddenField ID="hdnLangY" runat="server" ClientIDMode="Static" Value="0" />

  <iaw:IAWPanel runat="server" ID="pnlFirstTime" GroupingText="Translations" TranslateText="false">
    <asp:Label runat="server" ID="lblMsgFirstTime" />

    <div class="Grid-Indent">
      <iaw:IAWGrid runat="server" ID="grdUserLanguageFirst" AutoGenerateColumns="False" DataKeyNames="language_ref" editState="Normal" SaveButtonId="" TranslateHeadings="false" ShowHeader="false" Style="width: auto !important; table-layout: auto;" RowStyle-BackColor="Transparent" AlternatingRowStyle-BackColor="Transparent">
        <Columns>
          <asp:BoundField DataField="language_name" ReadOnly="True" ItemStyle-CssClass="grdField stdlink" />
          <asp:TemplateField ItemStyle-CssClass="ActionCell">
            <ItemTemplate>
              <div>
                <iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="I do not want to translate this language" TranslateText="false" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' />
              </div>
              <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />
              <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
            </ItemTemplate>
            <ItemStyle HorizontalAlign="Center" />
          </asp:TemplateField>
        </Columns>
        <EmptyDataTemplate>
          Currently, there are no languages assigned for translation, please contact IAW
        </EmptyDataTemplate>

        <RowStyle Wrap="False" />

      </iaw:IAWGrid>

    </div>

    <p>Click on the language to begin entering the translated text.</p>
    <p>If you no longer wish to translate the <b><span style="color: DodgerBlue">vivi</span><span style="color: black;">chart</span></b> service into another language, please click the delete button.</p>

  </iaw:IAWPanel>
  <iaw:IAWPanel runat="server" ID="pnlNormal" GroupingText="Translations" TranslateText="false">
    <asp:Label runat="server" ID="lblMsgAfterFirst" />

    <div class="Grid-Indent">
      <iaw:IAWGrid runat="server" ID="grdUserLanguageNormal" AutoGenerateColumns="False" DataKeyNames="language_ref" editState="Normal" SaveButtonId="" TranslateHeadings="false" ShowHeader="false" Style="width: auto !important; table-layout: auto;" RowStyle-BackColor="Transparent" AlternatingRowStyle-BackColor="Transparent">
        <Columns>
          <asp:BoundField DataField="language_name" HeaderText="Language" ReadOnly="True" ItemStyle-CssClass="grdField stdlink" />
        </Columns>
        <EmptyDataTemplate>
            Currently, there are no languages assigned for translation, please contact IAW
        </EmptyDataTemplate>

        <RowStyle Wrap="False" />
      </iaw:IAWGrid>
    </div>

    <p>Click on the language to see what you have already done and what remains outstanding.</p>
    <p>The scope of the <b><span style="color: DodgerBlue">vivi</span><span style="color: black;">chart</span></b> service is summarised  <span class="stdlink"> <a href="browserHelp/Overview.html" target="HelpPage"> Here </a> </span> </p>
  </iaw:IAWPanel>

  <iaw:IAWPanel runat="server" ID="pnlMain" TranslateText="false">

    <table border="0" width="100%">
      <tr>
        <td>
          <iaw:IAWTextbox runat="server" TextMode="search" AutoPostBack="True" class="SearchBox" ID="txtURfilter" placeholder="::LT_A0039" orgPlaceholder="search" ClientIDMode="static" />

          <iaw:IAWCheckBox runat="server" ID="cbIncludeUntranslated" ToolTip="Include Untranslated" Checked="true" AutoPostBack="true" TranslateText="false" TranslateTooltip="false" />
          <iaw:IAWCheckBox runat="server" ID="cbIncludeTranslated" ToolTip="Include Translated" Checked="false" AutoPostBack="true" TranslateText="false" TranslateTooltip="false" />
          <iaw:IAWCheckBox runat="server" ID="cbIncludeReviewed" ToolTip="Include Reviewed" Checked="false" AutoPostBack="true" TranslateText="false" TranslateTooltip="false" />
          <iaw:IAWCheckBox runat="server" ID="cbIncludeReleased" ToolTip="Include Released" Checked="false" AutoPostBack="true" TranslateText="false" TranslateTooltip="false" />
        </td>
        <td style="min-width: 30px">
          <iaw:IAWHyperLink runat="server" ID="btnHelp" CssClass="context_link large fa-regular fa-circle-question" ToolTip="Show Help" addCmdParameter="false" TranslateText="false" />
        </td>
      </tr>
    </table>
    <div id="divLang" class="divScroll grid-to-bottom">
      <iaw:IAWGrid runat="server" ID="grdLang" AutoGenerateColumns="False" DataKeyNames="qlang_id" editState="Normal" SaveButtonId="" TranslateHeadings="false">
        <Columns>
          <asp:TemplateField HeaderText="Text to translate">
            <ItemTemplate>
              <table border="0">
                <tr>
                  <td class="lang_english_label">English</td>
                  <td class="lang_icon">
                    <iaw:IAWHyperLink runat="server" ID="linkContext" CssClass="context_link fa-regular fa-circle-question" ToolTip="Show context" addCmdParameter="false" TranslateText="false" TranslateTooltip="false" />
                  </td>
                  <td class="lang_english">
                    <asp:Label ID="lblGridBaseText" runat="server" Text='<%# Bind("base_text") %>' />
                  </td>
                </tr>
                <tr>
                  <td class="lang_foreign_label">
                    <asp:Label ID="lblForeignLabel" runat="server" Text='<%# Bind("language_name") %>' />
                    <asp:Label ID="lblConextLabel" runat="server" Text='Context Help' />
                  </td>
                  <td class="lang_icon">
                    <iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="Amend Text" TranslateTooltip="false" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' />
                  </td>
                  <td class="lang_foreign">
                    <asp:Label ID="lblGridForeignText" runat="server" Text='<%# Bind("language_text") %>' />
                    <asp:Label ID="lblGridContextHelp" runat="server" Text='<%# Bind("context_url") %>' />
                  </td>
                </tr>
              </table>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:BoundField DataField="text_status_pt" HeaderText="Status" ReadOnly="True" ItemStyle-CssClass="grdField" />

          <asp:TemplateField>

            <ItemTemplate>
              <iaw:IAWHyperLinkButton runat="server" ID="btnThumbsUp" CssClass="context_link" TranslateText="false" ToolTip="Accept" CommandName="Accept" CommandArgument='<%# Container.DataItemIndex %>'>
                <i class="fa-regular fa-thumbs-up"></i>
              </iaw:IAWHyperLinkButton>
              <span>&nbsp;</span>
              <iaw:IAWHyperLinkButton runat="server" ID="btnThumbsDown" CssClass="context_link" TranslateText="false" ToolTip="Reject" CommandName="Reject" CommandArgument='<%# Container.DataItemIndex %>'>
                <i class="fa-regular fa-thumbs-down"></i>
              </iaw:IAWHyperLinkButton>
            </ItemTemplate>
          </asp:TemplateField>

        </Columns>
        <EmptyDataTemplate>
          <iaw:IAWLabel runat="server" ID="lblUserLangNoRows" Text="No information has been found" CssClass="NoRows" TranslateText="false" />
        </EmptyDataTemplate>

        <RowStyle Wrap="False" />
      </iaw:IAWGrid>
    </div>


  </iaw:IAWPanel>

  <%-- Foreign Text Popup Form --%>

  <iaw:IAWPanel ID="popForeignText" runat="server" GroupingText="Translate Text" TranslateText="false" Style="display: none" CssClass="PopupPanel" DefaultButton="btnForeignTextSave">
    <iaw:wuc_help runat="server" ID="helplink_popForeignText" Reference="TranslateForeignText" />

    <table border="0" class="listform">
      <tr>
        <td class="lang_english_label">English <span>&nbsp;&nbsp;&nbsp;</span>
          <iaw:IAWLabel runat="server" ID="lblLinkContextForm" CssClass="context_link fa-regular fa-circle-question" ToolTip="Show context" TranslateText="false" TranslateTooltip="false" />
        </td>
        <td class="lang_english">
          <asp:Label ID="lblEnglishText" runat="server" />
        </td>
      </tr>
      <tr>
        <td class="lang_foreign_label">
          <asp:Label ID="lblForeignText" runat="server" />
        </td>
        <td>
          <asp:TextBox ID="txtForeignText" runat="server" class="multiline_input" ClientIDMode="static" TextMode="MultiLine" Rows="4" />
        </td>
      </tr>
      <tr>
        <td colspan="2" align="center">
          <hr />
          <iaw:IAWHyperLinkButton runat="server" ID="btnForeignTextSubmit" Text="Submit Translation" CausesValidation="true" ValidationGroup="ForeignText" TranslateText="false" />
          <span>&nbsp; &nbsp;</span>
          <iaw:IAWHyperLinkButton runat="server" ID="btnForeignTextSave" Text="Save for Later" CausesValidation="true" ValidationGroup="ForeignText" TranslateText="false" />
          <span>&nbsp; &nbsp;</span>
          <iaw:IAWHyperLinkButton runat="server" ID="btnForeignTextCancel" Text="Cancel" CausesValidation="false" TranslateText="false" IconClass="iconButton fa-solid fa-circle-xmark" />
        </td>
      </tr>

    </table>

  </iaw:IAWPanel>

  <asp:Button ID="btnForeignTextFake" runat="server" Style="display: none" />
  <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeForeignTextForm" BackgroundCssClass="ModalBackground" PopupControlID="popForeignText" TargetControlID="btnForeignTextFake" ClientIDMode="static" />

  <%-- Delete Confirm Popup --%>

  <iaw:IAWPanel ID="popDelete" runat="server" GroupingText="Confirm" Style="display: none" CssClass="PopupPanel" DefaultButton="btnDeleteCancel" TranslateText="false">
    <table border="0" class="listform">
      <tr>
        <td>
          <iaw:IAWLabel ID="lblDeleteConfirm" runat="server" Text="Are you sure you want to delete this entry?" TranslateText="false" />
        </td>
      </tr>
      <tr>
        <td align="center">
          <br />
          <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteOk" Text="Yes" CausesValidation="false" ToolTip="Delete entry" TranslateText="false" TranslateTooltip="false" IconClass="iconButton fa-solid fa-circle-check" />
          <span>&nbsp;</span>
          <iaw:IAWHyperLinkButton runat="server" ID="btnDeleteCancel" Text="No" CausesValidation="false" ToolTip="Do not delete entry" TranslateText="false" TranslateTooltip="false" IconClass="iconButton fa-solid fa-circle-xmark" />
        </td>
      </tr>
    </table>
  </iaw:IAWPanel>

</asp:Content>
