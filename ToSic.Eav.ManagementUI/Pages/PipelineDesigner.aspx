<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.aspx.cs" Inherits="ToSic.Eav.ManagementUI.Pages.PipelineDesigner" StylesheetTheme="" %>

<!doctype html>

<html>
<head>
	<meta charset="utf-8" />
	<title>Pipeline Designer</title>
	<link rel="stylesheet" href="/dist/admin/pipeline-designer.min.css">
    


	<link rel="stylesheet" href="../CSS/bootstrap.min.css" />
	<link rel="stylesheet" href="../CSS/bootstrap-theme.min.css" />
	<link rel="stylesheet" href="../Scripts/jquery-ui-1.10.3.css" />
	<script src="../Scripts/jquery-1.9.1.min.js"></script>
	<script src="../Scripts/jquery-ui-1.10.3.min.js"></script>
	<script src="../Scripts/angular.min.js"></script>
	<script src="../Scripts/angular-resource.min.js"></script>
	<script src="../Scripts/angular-animate.min.js"></script>
    
    <%--Toaster--%>
	<link rel="stylesheet" href="/bower_components/angularjs-toaster/toaster.min.css" />
    <script src="/bower_components/angularjs-toaster/toaster.min.js"></script>
    
    <%--jsPlumb--%>
	<script src="/bower_components/jsplumb/dist/js/jquery.jsPlumb-1.7.2-min.js"></script>

    <script src="/dist/admin/tosic-eav-admin.annotated.js"></script>
   

	<script src='../EAV/AngularServices/EavGlobalConfigurationProvider.js'></script>
	<script src='../EAV/AngularServices/EavDialogService.js'></script>
    <script src='../EAV/AngularServices/eav4ng.js'></script>
</head>
<body>
	<div eav-app="PipelineDesigner" ng-include="'pipelines/pipeline-designer.html'" ></div>

</body>
</html>
