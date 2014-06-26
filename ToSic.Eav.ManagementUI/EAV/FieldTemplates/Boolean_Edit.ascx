<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.Boolean_Edit" Codebehind="Boolean_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register TagPrefix="Eav" TagName="DimensionMenu" Src="../Controls/DimensionMenu.ascx" %>

<asp:Label ID="FieldLabel" runat="server" />
<div class="eav-field-control">
    <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Convert.ToBoolean(FieldValueEditString) %>' CssClass="DDCheckBox" EnableViewState="true" />
</div>
<Eav:DimensionMenu ID="DimensionMenu1" runat="server"></Eav:DimensionMenu>
<br />