<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="ImagePage.aspx.vb" Inherits=".ImagePage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
   <%-- <link rel="stylesheet" href="ImagePage.css" />--%>

<%--    <style>
        .locale-grid {
            width: 100%;
            border-collapse: collapse;
        }
        .locale-grid th, .locale-grid td {
            border: 1px solid black;
            padding: 8px;
            text-align: left;
            background-color: white;
            color:black;
        }
        .locale-grid th {
            background-color: #e2e2e2;
            white-space:break-spaces;
            position: -webkit-sticky;
            position: sticky;
            top: 0;
        }
    </style>--%>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--<div class="background" />--%>

    <div>
<%--        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="locale-grid">
            <Columns>
                <asp:BoundField DataField="Locale" HeaderText="Locale" />
                <asp:BoundField DataField="LocaleName" HeaderText="Locale Name" />
                <asp:BoundField DataField="DateFormat" HeaderText="Date Format" />
                <asp:BoundField DataField="CurrencyIdentifier" HeaderText="Currency" />
                <asp:BoundField DataField="CurrencyNameEnglish" HeaderText="English" />
                <asp:BoundField DataField="CurrencyNameLocal" HeaderText="Local" />
                <asp:BoundField DataField="CurrencySymbol" HeaderText="Symbol" />
                <asp:BoundField DataField="NumberSeparator" HeaderText="Grp Sep" />
                <asp:BoundField DataField="DecimalSeparator" HeaderText="Dec Sep" />
                <asp:BoundField DataField="StandardCurrencyFormat" HeaderText="Std Fmt" />
                <asp:BoundField DataField="NegativeCurrencyFormat" HeaderText="Neg Fmt" />
            </Columns>
        </asp:GridView>--%>
        <iaw:IAWPanel runat="server" ID="pnlUsers" GroupingText="Locale Details" CssClass="PanelClass" TranslateText="false">
            <asp:Literal ID="ltLocaleInfo" runat="server" />
        </iaw:IAWPanel>
    </div>

</asp:Content>