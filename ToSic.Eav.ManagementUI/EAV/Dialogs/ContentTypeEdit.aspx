<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Dialogs.ContentType" StylesheetTheme="Dialog" Codebehind="ContentTypeEdit.aspx.cs" %>

<%@ Register src="../Controls/ContentTypeEdit.ascx" tagname="ContentTypeEdit" tagprefix="Eav" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>AttributeSet</title>
	<script src="../../Scripts/jquery-1.9.1.min.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
    <Eav:ContentTypeEdit ID="ContentTypeEdit1" runat="server" IsDialog="true" />
    </form>
</body>
</html>
