<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Dialogs.Items"
	StylesheetTheme="Dialog" Codebehind="Items.aspx.cs" %>

<%@ Register Src="../Controls/Items.ascx" TagName="Items" TagPrefix="Eav" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Items</title>
</head>
<body>
	<form id="form1" runat="server">
	<asp:ScriptManager runat="server" ID="ScriptManager1" />
	<Eav:Items ID="Items1" runat="server" IsDialog="True" />
	</form>
</body>
</html>
