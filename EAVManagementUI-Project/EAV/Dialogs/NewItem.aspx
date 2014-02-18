<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Dialogs.NewItem" StylesheetTheme="Dialog" Codebehind="NewItem.aspx.cs" %>

<%@ Register src="../Controls/ItemForm.ascx" tagname="ItemForm" tagprefix="Eav" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>New Item</title>
</head>
<body>
    <form id="form1" runat="server">
    <Eav:ItemForm ID="NewItemForm" runat="server" IsDialog="true" />
    </form>
</body>
</html>
