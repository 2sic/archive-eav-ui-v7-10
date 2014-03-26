<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemVersionDetails.ascx.cs" Inherits="ToSic.Eav.ManagementUI.ItemVersionDetails" %>
<h1><asp:Literal runat="server" Text="Version {0} of {1} (Entity {2})" ID="litControlHeading"/></h1>
<h2>Result of changes</h2>
<asp:GridView runat="server" ID="grdVersionDetails">
</asp:GridView>
