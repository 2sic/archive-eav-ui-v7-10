<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.Text_Edit" Codebehind="String_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register src="../Controls/DimensionMenu.ascx" tagPrefix="Eav" tagName="DimensionMenu" %>

<asp:Label ID="FieldLabel" runat="server"  />
<div class="eav-field-control">
    <asp:TextBox ID="TextBox1" runat="server" Text='<%# FieldValueEditString %>' CssClass="DDTextBox" EnableViewState="true" />
</div>
<Eav:DimensionMenu ID="DimensionMenu1" runat="server"></Eav:DimensionMenu>
<br />