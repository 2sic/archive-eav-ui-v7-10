<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PipelineManagement.aspx.cs" Inherits="ToSic.Eav.ManagementUI.Pages.PipelineManagement" StylesheetTheme="Dialog" %>

<%@ Register Src="~/EAV/PipelineDesigner/PipelineManagement.ascx" TagPrefix="eav" TagName="PipelineManagement" %>

<!doctype html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Pipeline Management</title>
	<base href="/" />
	<link rel="stylesheet" href="../CSS/bootstrap.min.css" />
	<link rel="stylesheet" href="../CSS/bootstrap-theme.min.css" />
	<script src="../Scripts/jquery-1.9.1.min.js"></script>
	<script src="../Scripts/angular.min.js"></script>
	<script src="../Scripts/angular-resource.min.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineManagement.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineService.js"></script>
	<script src='../EAV/AngularServices/EavGlobalConfigurationProvider.js'></script>
</head>
<body>
	<form id="form1" runat="server">
		<eav:PipelineManagement runat="server" ID="PipelineManagement1" />
	</form>
</body>
</html>
