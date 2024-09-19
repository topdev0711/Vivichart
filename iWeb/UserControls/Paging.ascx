<%@ Control Language="VB" AutoEventWireup="false" Inherits="Paging" Codebehind="Paging.ascx.vb" %>

<asp:UpdatePanel ID="UPD_paging"                                          
                 UpdateMode="Conditional" 
                 runat="server">
    <ContentTemplate>
        <table border="0" cellpadding="1" cellspacing="1">
          <tr>
                                                          
            <td>
                <iaw:IAWImageButton id="FirstButton"                                                  
                                 CommandName="FirstPage" 
                                 TranslateText="false"
                                 OnCommand="Page_OnClick" runat="server" />                        
            </td>                                         
            <td>
                <iaw:IAWImageButton id="PreviousButton"                                                   
                                 CommandName="PreviousPage" 
                                 ToolTip="::LT_S0317" orgTooltip="Go to the previous page" 
                                 OnCommand="Page_OnClick" runat="server" />                                                                                     
            </td>
            <td style="text-align:center">Page <asp:literal id="CurrentPageLabel" runat="server" />
                of <asp:literal id="TotalPagesLabel" runat="server" />
              <iaw:IAWLabel id="TotalRecordsCount" runat="server" TranslateText="false" />
            </td>        
            <%--<td>
                <asp:PlaceHolder runat="server" ID="phPageLinks" />
            </td>--%>
            <td>
                <iaw:IAWImageButton id="NextButton"                                                  
                                 CommandName="NextPage" 
                                 ToolTip="::LT_S0318" orgTooltip="Go to the next page" 
                                 OnCommand="Page_OnClick" runat="server" />                                            
            </td>
            <td>
                <iaw:IAWImageButton id="LastButton"                                                  
                                 CommandName="LastPage" 
                                 ToolTip="::LT_S0319" orgTooltip="Go to the last page" 
                                 OnCommand="Page_OnClick" runat="server" />                                             
            </td>                                                                                               
          </tr>
        </table>    
    </ContentTemplate>    
</asp:UpdatePanel>    

