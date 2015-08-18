<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" StylesheetTheme="Dialog" Inherits="ToSic.Eav.ManagementUI.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
		<h1>EAV</h1>
		<ul>
			<li><a href="Pages/ContentTypes.aspx">Content Types (without EavManagement-Wrapper)</a></li>
			<li><a href="Pages/EavManagement.aspx">EAV Management (AppId 1)</a></li>
			<li><a href="Pages/EavManagementApp2.aspx">EAV Management (AppId 2)</a></li>
		</ul>
		<h1>Pipelines</h1>
		<ul>
			<li><a href="Pages/PipelineManagement.aspx?AppId=1">Pipeline Management</a></li>
			<li><a href="Pages/PipelineDesigner.aspx?AppId=1&PipelineId=347">Pipeline Designer</a></li>
		</ul>
        <h1>webservice</h1>
        <ul>
            <li><a href="Pages/WebApiQuickTests.html">Test web api</a></li>
        </ul>
	</form>
</body>
</html>
