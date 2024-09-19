<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="ModelList.aspx.vb" Inherits=".ModelList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
  <style type="text/css">
    .grdField {
      padding: 3px 5px 3px 5px;
      text-align: left;
      white-space: nowrap;
      vertical-align: middle;
      cursor: pointer;
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

    .tblStyle {
      border: none;
    }
  </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <iaw:IAWPanel runat="server" ID="ErrorPanel" GroupingText="::LT_S0278" orgGroupingText="Charts" CssClass="PanelClass" Visible="false">
      <iaw:IawMessage runat="server" ID="msg" ShowIcon="true" Icon="information" MessageType="information" />
  </iaw:IAWPanel>

  <%-- -----------------------------------------------------------------------------------
         Model List
    ----------------------------------------------------------------------------------- --%>

  <iaw:IAWPanel runat="server" ID="Panel1" GroupingText="::LT_S0278" orgGroupingText="Charts" CssClass="PanelClass">

    <div runat="server" id="cbOptions">
      <iaw:IAWCheckBox ID="cbPrevious" runat="server" Text="::LT_S0279" orgText="Previous" AutoPostBack="true" />
      <iaw:IAWCheckBox ID="cbCurrent" runat="server" Text="::LT_S0280" orgText="Current" AutoPostBack="true" Checked="true" />
      <iaw:IAWCheckBox ID="cbFuture" runat="server" Text="::LT_S0281" orgText="Future" AutoPostBack="true" Checked="true" />
      <iaw:IAWCheckBox ID="cbOtherUsers" runat="server" Text="::LT_S0282" orgText="Other Users" AutoPostBack="true" Checked="true" />
      <iaw:IAWCheckBox ID="cbPublishedOnly" runat="server" Text="::LT_S0283" orgText="Published Only" AutoPostBack="true" Checked="false" />
      <hr />
    </div>

    <iaw:IAWGrid runat="server" ID="grdModels" AutoGenerateColumns="False" DataKeyNames="model_key" editState="Normal" AllowSorting="true" TranslateHeadings="True" ShowHeaderWhenEmpty="true">
      <Columns>
        <%--<asp:BoundField DataField="filter_name" HeaderText="Stricture" ReadOnly="True" ItemStyle-CssClass="grdField" SortExpression="stricture,view_ref" />--%>
        <iawb:IAWBoundField DataField="view_ref" HeaderText="::LT_S0221" orgHeaderText="Data View" ReadOnly="True" ItemStyle-CssClass="grdField" SortExpression="view_ref, effective_date DESC" />
        <iawb:IAWBoundField DataField="effective_date" HeaderText="::LT_S0284" orgHeaderText="From" ReadOnly="True" ItemStyle-CssClass="grdField" DataFormatString="{0:dd/MM/yyyy ddd}" SortExpression="effective_date DESC" />

        <asp:TemplateField ItemStyle-CssClass="ActionCell">
          <HeaderTemplate>
            <iaw:IAWImageButton runat="server" ID="btnModelAdd" CssClass="IconPic Icon16 IconAddHead" ImageUrl="~/graphics/1px.gif" CommandName="New" ToolTip="::LT_S0039" orgTooltip="Add"  />
          </HeaderTemplate>
          <ItemTemplate>
            <div class="ActionButtons">
              <div><iaw:IAWImageButton ID="btnModel" runat="server" ToolTip="::LT_S0105" orgTooltip="Chart" ImageUrl="~/graphics\1px.gif" CssClass="IconPic Icon16 IconChart" CommandName="ShowModel" CommandArgument='<%# Container.DataItemIndex %>' /></div>
              <div><iaw:IAWImageButton ID="btnAmend" runat="server" ToolTip="::LT_S0025" orgTooltip="Amend" CommandName="AmendRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
              <div><iaw:IAWImageButton ID="btnDel" runat="server" ToolTip="::LT_S0040" orgTooltip="Delete" CommandName="DeleteRow" CommandArgument='<%# Container.DataItemIndex %>' /></div>
            </div>
            <ajaxToolkit:ConfirmButtonExtender ID="cbebtnDel" runat="server" DisplayModalPopupID="mpeDelete" TargetControlID="btnDel" />

            <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeDelete" BackgroundCssClass="ModalBackground" OkControlID="btnDeleteOk" CancelControlID="btnDeleteCancel" PopupControlID="popDelete" TargetControlID="btnDel" />
          </ItemTemplate>
          <ItemStyle HorizontalAlign="Center" />
        </asp:TemplateField>

      </Columns>
      <EmptyDataTemplate>
        <iaw:IAWLabel runat="server" ID="grdNomodelRows" Text="::LT_S0285" orgText="No Charts have been defined" CssClass="NoWrap" />
      </EmptyDataTemplate>

      <RowStyle Wrap="False" />
      <HeaderStyle Wrap="false" />
    </iaw:IAWGrid>
    <br />
    <iaw:IAWLabel runat="server" ID="lblmodelMsg" Visible="False" CssClass="NoRows" ForeColor="Red" />

  </iaw:IAWPanel>

  <%-- -----------------------------------------------------------------------------------
         popup Model Form
    ----------------------------------------------------------------------------------- --%>

  <asp:HiddenField runat="server" ID="hdnModelID" />

  <iaw:IAWPanel ID="popModel" runat="server" GroupingText="::LT_S0278" orgGroupingText="Charts" Style="display: none" CssClass="PopupPanel">
    <iaw:wuc_help runat="server" ID="helplink_models" Reference="model_details" />

    <table class="tblStyle">

      <tr class="formrow">
        <td class="formlabel">
          <iaw:IAWLabel ID="IAWLabel10" runat="server" Text="::LT_S0221" orgText="Data View" />
        </td>
        <td class="formdata" colspan="4">
          <asp:DropDownList ID="ddlbView" runat="server" AutoPostBack="true" />
        </td>
      </tr>

      <tr class="formrow" runat="server" id="trReference" >
        <td class="formlabel">
          <iaw:IAWLabel runat="server" ID="IAWLabel1" Text="::LT_A0001" orgText="Reference" />
        </td>
        <td class="formdata" colspan="4">
          <asp:DropDownList ID="ddlbReference" runat="server" />
          <iaw:IAWTextbox ID="txtReference" runat="server" MaxLength="50" CssClass="formcontrol" ValidationGroup="Client" Visible="false" />
          <iaw:IAWRequiredFieldValidator ControlToValidate="txtReference" ID="IAWRequiredFieldValidator1" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="Model" Display="Dynamic" />

          <iaw:IAWImageButton runat="server" ID="btnRefAdd" CommandName="New" ImageUrl="~/graphics/1px.gif" CssClass="IconPic Icon20 IconAdd" ToolTip="::LT_S0039" orgToolTip="Add" />
          <iaw:IAWImageButton runat="server" ID="btnRefCancel" CommandName="New" ImageUrl="~/graphics/1px.gif" CssClass="IconPic Icon20 IconCancel" ToolTip="::LT_S0039" orgToolTip="Cancel" Visible="false" />
        </td>
      </tr>

      <tr class="formrow">
        <td class="formlabel">
          <iaw:IAWLabel ID="IAWLabel11" runat="server" Text="::LT_S0286" orgText="Date From" />
        </td>
        <td class="formdata" colspan="4">
          <iaw:DateDropDown runat="server" ID="dtEffectiveDate" ValidationGroup="Model" />
          <iaw:IAWRequiredFieldValidator ControlToValidate="dtEffectiveDate" ID="rfvEffectiveDate" runat="server" EnableClientScript="true" Enabled="true" ErrorMessage="::LT_S0140" orgErrorMessage="Required Field" ValidationGroup="Model" Display="Dynamic" />
          <iaw:IAWCustomValidator ID="cvEffectiveDate" runat="server" ControlToValidate="dtEffectiveDate" ErrorMessage="::LT_S0288" orgErrorMessage="Within a View, the effective dates must be unique" ValidationGroup="Model" AddBR="true" Display="Dynamic" />
        </td>
      </tr>

      <tr class="formrow" runat="server" id="trShowImages" >
        <td class="formlabel">
          <iaw:IAWLabel runat="server" ID="IAWLabel7" Text="::LT_S0201" orgText="Show Images" />
        </td>
        <td class="formdata" colspan="4">
          <iaw:IAWCheckBox ID="cbShowPhotos" runat="server" ClientIDMode="Static" />
        </td>
      </tr>

      <tr class="formrow" runat="server" id="trPublished">
        <td class="formlabel">
          <iaw:IAWLabel ID="IAWLabel12" runat="server" Text="::LT_S0287" orgText="Published" />
        </td>
        <td class="formdata" colspan="4">
          <iaw:IAWCheckBox ID="cbPublished" runat="server" />
        </td>
      </tr>

      <tr>
        <td colspan="2" class="center">
          <hr />
          <iaw:IAWHyperLinkButton runat="server" ID="btnSettingsSave" Text="::LT_S0005" orgText="Save" CausesValidation="true" ValidationGroup="Model" IconClass="iconButton fa-solid fa-floppy-disk" />
          <span>&nbsp; &nbsp;</span>
          <iaw:IAWHyperLinkButton runat="server" ID="btnSettingsCancel" Text="::LT_S0138" orgText="Cancel" CausesValidation="false" IconClass="iconButton fa-solid fa-circle-xmark" />
        </td>
      </tr>
    </table>

  </iaw:IAWPanel>

  <asp:Button ID="btnModelsFake" runat="server" Style="display: none" />
  <ajaxToolkit:ModalPopupExtender runat="server" ID="mpeModels" BackgroundCssClass="ModalBackground" PopupControlID="popModel" TargetControlID="btnModelsFake" />

  <%-- -----------------------------------------------------------------------------------
         Delete Confirm Popup 
    ----------------------------------------------------------------------------------- --%>

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


</asp:Content>
