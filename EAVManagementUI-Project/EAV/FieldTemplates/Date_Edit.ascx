<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.Date_Edit" Codebehind="Date_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register TagPrefix="Eav" TagName="DimensionMenu" Src="../Controls/DimensionMenu.ascx" %>

<asp:Label ID="FieldLabel" runat="server" />
<div class="eav-field-control">
    <asp:Calendar runat="server" ID="Calendar1" />
</div>
<Eav:DimensionMenu ID="DimensionMenu1" runat="server"></Eav:DimensionMenu>
<br />