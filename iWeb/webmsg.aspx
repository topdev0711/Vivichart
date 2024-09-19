<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="webmsgReport" Title="IAW Web Message Log" CodeBehind="webmsg.aspx.vb" %>

<asp:Content ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" GroupingText="Web Log">

    <iaw:IAWLabel ID="lblType" runat="server" Text="Type"></iaw:IAWLabel>
    <iaw:IAWDropDownList ID="lstType"
      runat="server"
      DataSource="<%# getTypes() %>"
      AppendDataBoundItems="True"
      AutoPostBack="True"
      DataTextField="description"
      DataValueField="return_value">
    </iaw:IAWDropDownList><br />
    <br />

    <iaw:IAWGrid
      runat="server"
      ID="grdViewErrorLog"
      Width="98%"
      AutoGenerateColumns="False"
      DataSourceID="ODS1"
      DataKeyNames="Id"
      SaveButtonId="SaveButton"
      AllowPaging="True"
      PageSize="30"
      EmptyDataText=" "
      AllowSorting="True"
      TranslateHeadings="false"
      editState="Normal">
      <Columns>
        <iawb:IAWGridCheckBoxColumn HeaderText="Delete" />
        <iawb:IAWBoundField DataField="Id" HeaderText="ID" Visible="False" />
        <iawb:IAWBoundField DataField="Message" SortExpression="Message" HeaderText="Error Message">
          <ItemStyle HorizontalAlign="Left" />
        </iawb:IAWBoundField>
        <iawb:IAWBoundField DataField="messagedate" SortExpression="messagedate" HeaderText="Date" HtmlEncode="False" DataFormatString="{0:dd MMM yyyy}">
          <ItemStyle Wrap="False" />
        </iawb:IAWBoundField>
        <iawb:IAWBoundField DataField="messagedate" HeaderText="Time" HtmlEncode="False" DataFormatString="{0:HH:mm:ss}">
          <ItemStyle Wrap="False" />
        </iawb:IAWBoundField>
      </Columns>
    </iaw:IAWGrid>

    <asp:ObjectDataSource ID="ODS1" runat="server" TypeName="WebMsgLog_DAL" UpdateMethod="DeleteItem" DeleteMethod="DeleteItem" SelectMethod="GetLogAsDataTable" />
  </iaw:IAWPanel>

</asp:Content>
