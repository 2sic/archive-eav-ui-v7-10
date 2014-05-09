<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="ToSic.Eav.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Microtests</title>
	<style type="text/css">
		body { font-family: Arial; font-size: 12px }
	</style>
</head>
<body>
    <form id="form1" runat="server">
		<div>
			<asp:Label ID="lblTimeToRender" runat="server" Text="Time to Render: {0} ms"/>
			<h1>Microtests</h1>
			<ul>
				<li><a href="?Test=All">All</a></li>
				<li><a href="?Test=TypeFilter">Type-Filter</a></li>
				<li><a href="?Test=EntityIdFilter&EntityIds=5464,5466,5467&AppId=2">EntityID Filter</a></li>
				<li><a href="?Test=ValueFilter">ValueFilter</a></li>
				<li><a href="?Test=AttributeFilter">Attribute Stripper/Filter</a></li>
				<li><a href="?Test=ValueSort">ValueSort</a></li>
				<li><a href="?Test=PublishingWithDrafts">Publishing with Drafts</a></li>
				<li><a href="?Test=PublishingPublishedOnly">Publishing - Published only</a></li>
				<li><a href="?Test=listproperty">Quick Property to access the Out["Default"].List</a></li>
				<li><a href="?Test=InMemoryEntity">In-Memory Entity</a></li>
				<li><a href="?Test=DataTableDataSource">DataTable DataSource</a></li>
				<li><a href="?Test=SqlDataSourceSimple">SQL DataSource (simple)</a></li>
				<li><a href="?Test=SqlDataSourceWithConfiguration">SQL DataSource (with configuration)</a></li>
				<li><a href="?Test=ClearCache&AppId=1">Clear Cache</a></li>
			</ul>
		</div>
		<asp:Literal runat="server" ID="litResults" />
    </form>
</body>
</html>
