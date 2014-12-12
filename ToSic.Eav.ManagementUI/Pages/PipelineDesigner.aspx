<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.aspx.cs" Inherits="ToSic.Eav.ManagementUI.Pages.PipelineDesigner" StylesheetTheme="" %>

<%@ Register Src="~/EAV/PipelineDesigner/PipelineDesigner.ascx" TagPrefix="eav" TagName="PipelineDesigner" %>

<!doctype html>
<html>
<head>
	<meta charset="utf-8" />
	<title>Pipeline Designer</title>
	<link rel="stylesheet" href="../EAV/PipelineDesigner/PipelineDesigner.css">
	<link rel="stylesheet" href="../Scripts/toaster.css" />
	<link rel="stylesheet" href="../CSS/bootstrap.min.css" />
	<link rel="stylesheet" href="../CSS/bootstrap-theme.min.css" />
	<link rel="stylesheet" href="../Scripts/jquery-ui-1.10.3.css" />
	<script src="../Scripts/jquery-1.9.1.min.js"></script>
	<script src="../Scripts/jquery-ui-1.10.2.min.js"></script>
	<script src="../Scripts/angular.js"></script>
	<script src="../Scripts/angular-resource.min.js"></script>
	<script src="../Scripts/angular-animate.min.js"></script>
	<script src="../Scripts/toaster.js"></script>
	<script src="../EAV/PipelineDesigner/assets/jquery.jsPlumb-1.6.4-min.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineDesigner.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineDesignerController.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineFactory.js"></script>
	<script src="../EAV/AngularServices/NotificationService.js"></script>
	<script src='../EAV/AngularServices/EavGlobalConfigurationProvider.js'></script>
	<script src='../EAV/AngularServices/EavDialogService.js'></script>
</head>
<body>
	<eav:PipelineDesigner runat="server" ID="PipelineDesigner1" />
</body>
</html>
