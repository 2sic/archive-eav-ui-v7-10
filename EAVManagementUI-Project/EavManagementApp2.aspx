<%@ Page Language="C#" AutoEventWireup="True" Inherits="ToSic.Eav.ManagementUI.Pages.EavManagementApp2" StylesheetTheme="Dialog" Codebehind="EavManagementApp2.aspx.cs" %>

<%@ Register src="Eav/Controls/EavManagement.ascx" tagname="EavManagement" tagprefix="Eav" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Eav Management</title>
    <script type="text/javascript" src="/Scripts/jquery-1.9.1.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
	<asp:ScriptManager runat="server" ID="ScriptManager1" />
	<%-- Optional use the BaseUrl-Property to specify a URL that this Wrapper Module will use --%>
	<Eav:EavManagement ID="EAVManagement2" runat="server" Scope="" DefaultCultureDimension="5" ZoneId="2" AppId="2" />
	<asp:Button ID="btnClearCache" runat="server" Text="Clear Cache" OnClick="btnClearCache_Click" style="position: absolute; bottom: 0; right: 0" />
    </form>
</body>
</html>
