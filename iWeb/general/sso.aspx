<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/IngenWebMaster.master" CodeBehind="sso.aspx.vb" Inherits=".sso" 
    title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table id="tbl-login">
        <tr class="row1">
            <td class="row1_cell1">
                <div runat="server" class="div_agreement" id="div_agreement">
                    <hr />
                    <span runat="server" id="span_agreement"></span>
                    <hr />
                    <iaw:IAWHyperLinkButton runat="server" ID="btn_agreement" Text="Continue" />
                </div>
            </td>
        </tr>
    </table>
</asp:Content>

