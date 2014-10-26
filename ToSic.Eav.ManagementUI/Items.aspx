<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Items.aspx.cs" Inherits="ToSic.Eav.ManagementUI.Items1" StyleSheetTheme="Dialog" %>

<%@ Register Src="~/EAV/Controls/Items.ascx" TagPrefix="Eav" TagName="Items" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Items</title>
    <script type="text/javascript" src="/Scripts/jquery-1.9.1.min.js"></script>
    <script type="text/javascript" src="/Scripts/jquery-ui-1.10.2.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular.min.js"></script>
    <script type="text/javascript" src="/Scripts/angular-ui-tree.min.js"></script>
    <link rel="stylesheet" href="/Scripts/angular-ui-tree.min.css" />
    <link rel="stylesheet" href="/Scripts/jquery-ui-1.10.3.css" />
</head>
<body>
    <form id="form1" runat="server">
		<Eav:Items runat="server" ID="Items" AttributeSetId="49" AppId="1" NewItemUrl="Undefined" EditItemUrl="Undefined" ColumnNames="Edit,Delete,EntityId,Name,Description" OnEntityDeleting="Items_OnEntityDeleting" />
    </form>
</body>
</html>
