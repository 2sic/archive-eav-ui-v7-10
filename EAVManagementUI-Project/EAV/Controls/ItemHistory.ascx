<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ItemHistory.ascx.cs" Inherits="ToSic.Eav.ManagementUI.ItemHistory" %>
<asp:GridView runat="server" ID="grdItemHistory" AutoGenerateColumns="False" DataSourceID="dsrcEntityVersions" OnRowDataBound="grdItemHistory_RowDataBound" AllowSorting="True">
	<Columns>
		<asp:BoundField DataField="VersionNumber" HeaderText="Version" SortExpression="VersionNumber" />
		<asp:BoundField DataField="ChangeId" HeaderText="ChangeId" SortExpression="ChangeId" />
		<asp:BoundField DataField="Timestamp" HeaderText="Date" SortExpression="Timestamp" />
		<asp:BoundField DataField="User" HeaderText="Who" SortExpression="User" />
		<asp:TemplateField>
			<ItemTemplate>
				<asp:HyperLink runat="server" Text="Changes" ID="hlkChanges" />
			</ItemTemplate>
		</asp:TemplateField>
	</Columns>
	<EmptyDataTemplate>No History available</EmptyDataTemplate>
</asp:GridView>
<asp:ObjectDataSource ID="dsrcEntityVersions" runat="server" SelectMethod="GetEntityVersions" TypeName="ToSic.Eav.EavContext" OnObjectCreating="dsrcEntityVersions_ObjectCreating" OnSelecting="dsrcEntityVersions_Selecting">
	<SelectParameters>
		<asp:Parameter Name="entityId" Type="Int32" />
	</SelectParameters>
</asp:ObjectDataSource>
<asp:Panel runat="server" ID="pnlActions">
	<asp:Hyperlink CssClass="eav-cancel" ID="hlkBack" runat="server" Text="Back" />
</asp:Panel>