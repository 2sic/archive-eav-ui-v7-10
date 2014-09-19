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
		<div id="pipeline" class="demo statemachine-demo">
			<div id="node{{node.id}}"
				class="node w"
				style="top: {{node.top}}px; left: {{node.left}}px"
				ng-repeat="node in pipeline.nodes">
				{{node.title}}
				<!--ng-dblclick="open()"-->
				<div class="ep"></div>
			</div>
		</div>
	</div>
</div>