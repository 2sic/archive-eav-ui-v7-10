<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineDesigner.PipelineDesigner" %>
<%--<div class="dnnForm scSettings dnnClear">
	<h2>Edit DataSource</h2>
	<div id="main">
		<div id="render"></div>
	</div>
	<ul class="dnnActions">
		<li>
			<select id="ddlAddPipelinePartType">
			</select>
			<input id="btnAddPipelinePart" type="button" value="Add" /></li>
		<li>
			<asp:HyperLink runat="server" ID="lbtnSavePipeline" CssClass="dnnPrimaryAction" Text="Save" NavigateUrl="javascript:pipelineDesigner.save()" /></li>
	</ul>
</div>
<script type="text/javascript">
	jsPlumb.bind("ready", function () {
		// chrome fix.
		document.onselectstart = function () { return false; };

		// render mode
		jsPlumb.setRenderMode(jsPlumb.SVG);
		pipelineDesigner.readOnly = false;
		pipelineDesigner.pipelineEntityId = 347;
		pipelineDesigner.pipelinePartAttributeSetId = 50;
		pipelineDesigner.webServicesUrl = "/PipelineDesignerServices.asmx";
		pipelineDesigner.init();
	});
</script>--%>

<div ng-app="pipelineDesinger">
	<div ng-controller="designerController">
		<div id="pipeline" class="pipelineDesigner">
			<div id="dataSource{{dataSource.id}}"
				class="dataSource w"
				style="top: {{dataSource.top}}px; left: {{dataSource.left}}px"
				ng-repeat="dataSource in pipeline.dataSources">
				{{dataSource.title}}
				<!--ng-dblclick="open()"-->
				<div class="ep"></div>
			</div>
		</div>
	</div>
</div>