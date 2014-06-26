<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.DateTime_Edit" Codebehind="DateTime_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register TagPrefix="Eav" TagName="DimensionMenu_1" Src="../Controls/DimensionMenu.ascx" %>

<asp:Label ID="FieldLabel" runat="server" />
<div class="eav-field-control">
    <asp:Calendar runat="server" ID="Calendar1" />
</div>
<Eav:DimensionMenu_1 runat="server"></Eav:DimensionMenu_1>
<br />