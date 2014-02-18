<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.Entity_Edit" Codebehind="Entity_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register src="../Controls/DimensionMenu.ascx" tagPrefix="Eav" tagName="DimensionMenu" %>

<asp:Label ID="FieldLabel" runat="server"  />
<div class="eav-field-control">
	<asp:DropDownList ID="DropDownList1" runat="server" DataTextField="Text" DataValueField="Value" AppendDataBoundItems="True" OnDataBound="DropDownList1_DataBound">
		<Items>
			<asp:ListItem Text="(none)" Value="" />
		</Items>
	</asp:DropDownList>
	<asp:HiddenField ID="hfEntityIds" runat="server" Visible="False" />
	<asp:Panel runat="server" ID="pnlMultiValues" CssClass="MultiValuesWrapper" Visible="False"/>
	<asp:PlaceHolder runat="server" ID="phAddMultiValue" Visible="False">
		<a href="javascript:void(0)" class="AddValue">Add Value</a>
	</asp:PlaceHolder>
</div>
<Eav:DimensionMenu ID="DimensionMenu1" runat="server" />
<br />