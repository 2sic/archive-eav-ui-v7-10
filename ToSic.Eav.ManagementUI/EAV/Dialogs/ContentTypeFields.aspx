<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Dialogs.ContentTypeFields" StylesheetTheme="Dialog" Codebehind="ContentTypeFields.aspx.cs" %>

<%@ Register Src="../Controls/ContentTypeFields.ascx" TagName="ContentTypeFields" TagPrefix="Eav" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Content Type Design</title>
</head>
<body>
	<form id="form1" runat="server">
		<asp:ScriptManager runat="server" ID="ScriptManager1" />
		<Eav:ContentTypeFields ID="ContentTypeFields1" runat="server" IsDialog="true" />
	</form>
</body>
</html>
