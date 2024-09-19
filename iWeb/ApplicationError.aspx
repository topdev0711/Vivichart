<%@ Page Language="VB" MasterPageFile="~/IngenWebMaster.master" AutoEventWireup="false" Inherits="ApplicationError" title="Application Error" Codebehind="ApplicationError.aspx.vb" %>

<asp:Content ID="Content2" ContentPlaceHolderID="headContent" runat="server">
    <style type="text/css">
        .Sects {
            text-align:center;
            min-width:400px;
            padding:10px 5px 10px 5px;
        }
    </style>

</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

   <iaw:IAWPanel ID="ProcPanel" runat="server" CssClass="PanelClass" 
                 GroupingText="::ER_IM000" orgGroupingText="Error" TranslateText="true">
        <div class="Sects">
            <iaw:IAWLabel ID="Label2" runat="server" TranslateText="true" 
                          Text="::LT_S0002" orgText="Sorry, an error has occurred"/>
        </div>

        <div class="Sects">
            <iaw:IAWLabel ID="msgContact" runat="server" TranslateText="true" 
                          Text="::LT_S0003" orgText="Please contact your technical support team."/>
        </div>

       <div class="Sects">
           <asp:Label runat="server" ID="labError" Visible="false" />
        </div>

   </iaw:IAWPanel>

</asp:Content>

