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
			<li><a href="Pages/EavManagement.aspx">EAV Management (AppId 1)</a> - <a href="/Pages/ngwrapper.cshtml?ng=content-types&appid=1"> angular </a></li>
			<li><a href="Pages/EavManagementApp2.aspx">EAV Management (AppId 2)</a> - <a href="/Pages/ngwrapper.cshtml?ng=content-types&appid=2"> angular </a></li>
            <li><a href="/Pages/ngwrapper.cshtml?ng=content-types&appid=3"> app 3</a></li>
            <li><a href="/Pages/ngwrapper.cshtml?ng=content-types&appid=4"> app 4</a></li>
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
