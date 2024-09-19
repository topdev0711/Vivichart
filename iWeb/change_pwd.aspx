<%@ Page Language="VB"
  MasterPageFile="~/IngenWebMaster.master"
  AutoEventWireup="false"
  Inherits="change_pwd"
  Title="Change Password"
  ValidateRequest="false"
  CodeBehind="change_pwd.aspx.vb" %>

<%@ Register Src="~/UserControls/ChangePassword.ascx" TagName="changePwd" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

  <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" GroupingText="::LT_S0004" TranslateText="true" orgGroupingText="Change Password">
    <iaw:IawMessage runat="server" ID="msg" ShowIcon="true" Icon="information" MessageType="information" Visible="false" />

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
      <ContentTemplate>
        <div style="padding: 10px;">
          <uc1:changePwd runat="server" ID="ChangePwd" ValidationGroup="pwd" />
          <br />
          <iaw:IAWHyperLinkButton runat="server" Text="::LT_S0005" TranslateText="true" orgText="Save" ID="btnUdatePwd" ValidationGroup="pwd" IconClass="iconButton fa-solid fa-floppy-disk" />
        </div>
      </ContentTemplate>
    </asp:UpdatePanel>
  </iaw:IAWPanel>

</asp:Content>

