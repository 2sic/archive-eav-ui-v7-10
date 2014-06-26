<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Dialogs.EditItem" StylesheetTheme="Dialog" Codebehind="EditItem.aspx.cs" %>

<%@ Register src="../Controls/ItemForm.ascx" tagname="ItemForm" tagprefix="Eav" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Edit Item</title>
</head>
<body>
    <form id="form1" runat="server">
    <Eav:ItemForm ID="EditItemForm" runat="server" IsDialog="true" />
<%--
    <asp:LinkButton ID="btnUpdate" runat="server" CommandName="Update" Text="Update" OnClick="btnUpdate_Click" />
	<asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" Text="Cancel" CausesValidation="false" onclick="btnCancel_Click" />
--%>
    </form>
</body>
</html>
