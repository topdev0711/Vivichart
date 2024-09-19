<%@ Page Title="" Async="true" Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="WooTest.aspx.vb" Inherits=".WooTest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        td {
            text-align: left;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:Panel runat="server" ID="pnlTestWebhooks" GroupingText="Test WebHooks" CssClass="PanelClass">
        <table border="0">
            <tr>
                <td>Fill Fields and press</td>
                <td>
                    <asp:Button ID="btnSend" runat="server" Text="Send" />
                    <asp:TextBox ID="txtStatus" runat="server" />
                </td>
            </tr>
            <tr>
                <td>Section</td>
                <td>
                    <asp:DropDownList ID="ddlbresource" runat="server">
                        <asp:ListItem Text="Customer" Value="customer" />
                        <asp:ListItem Text="Order" Value="order" />
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Action</td>
                <td>
                    <asp:DropDownList ID="ddlbEvent" runat="server">
                        <asp:ListItem Text="Created" Value="created" />
                        <asp:ListItem Text="Updated" Value="updated" />
                        <asp:ListItem Text="Deleted" Value="deleted" />
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Payload</td>
                <td>
                    <asp:TextBox ID="txtPayload" runat="server" TextMode="MultiLine" Style="width: 700px; height: 300px;" />
                </td>
            </tr>

        </table>
    </asp:Panel>


</asp:Content>
