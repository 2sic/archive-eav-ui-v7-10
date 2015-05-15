<%@ Page Language="C#" AutoEventWireup="true" StylesheetTheme="Dialog" %>

<%@ Register Src="~/EAV/Security/Permissions.ascx" TagPrefix="eav" TagName="Permissions" %>

<!doctype html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Permissions</title>
	<link rel="stylesheet" href="../CSS/bootstrap.min.css" />
	<link rel="stylesheet" href="../CSS/bootstrap-theme.min.css" />
	<script src="../Scripts/jquery-1.9.1.min.js"></script>
	<script src="../Scripts/angular.min.js"></script>


	<script src="../EAV/PipelineDesigner/PipelineManagement.js"></script>
	<script src="../EAV/PipelineDesigner/PipelineService.js"></script>
	<script src='../EAV/AngularServices/EavGlobalConfigurationProvider.js'></script>



<xscript src="/DesktopModules/ToSIC_SexyContent/Js/AngularJS/2sxc4ng.min.js" data-enableoptimizations="110"></xscript>
    <script src="../EAV/AngularServices/eav4Ng.js"></script>
    <script src="../EAV/AngularServices/eavNgSvcs.js"></script>
    <script src="../EAV/Security/PermissionsServices.js"></script>
    <script src="../EAV/Security/PermissionsController.js"></script>

</head>
<body>
	<form id="form1" runat="server">
		<eav:Permissions runat="server" ID="Permissions" />
	</form>
</body>
</html>
