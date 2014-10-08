<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.aspx.cs" Inherits="ToSic.Eav.ManagementUI.PipelineDesigner" StylesheetTheme="" %>

<%@ Register Src="~/EAV/PipelineDesigner/PipelineDesigner.ascx" TagPrefix="eav" TagName="PipelineDesigner" %>

<!doctype html>
<html>
<head>
	<meta charset="utf-8" />
	<title>Pipeline Designer</title>
	<link rel="stylesheet" href="EAV/PipelineDesigner/PipelineDesigner.css">
	<link rel="stylesheet" href="Scripts/toaster.css" />
</head>
<body>
	<%--<form id="form1" runat="server">--%>
	<eav:PipelineDesigner runat="server" ID="PipelineDesigner1" />
	<%--</form>--%>

	<script src="Scripts/jquery-1.9.1.min.js"></script>
	<script src="Scripts/jquery-ui-1.10.2.min.js"></script>
	<script src="Scripts/angular.js"></script>
	<script src="Scripts/angular-resource.min.js"></script>
	<script src="Scripts/angular-animate.min.js"></script>
	<script src="Scripts/toaster.js"></script>
	<script src="EAV/PipelineDesigner/assets/jquery.jsPlumb-1.6.4.js"></script>
	<script src="EAV/PipelineDesigner/PipelineDesigner.js"></script>
	<script src="EAV/PipelineDesigner/PipelineDesignerController.js"></script>
	<script src="EAV/PipelineDesigner/PipelineFactory.js"></script>
</body>
</html>
