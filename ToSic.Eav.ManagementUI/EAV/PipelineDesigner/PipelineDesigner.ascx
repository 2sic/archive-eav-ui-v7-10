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
	<div ng-controller="pipelineDesignerController">
		<div id="pipeline" class="pipelineDesigner">
			<div datasource
                id="dataSource_{{dataSource.EntityGuid}}"
				class="dataSource"
				ng-attr-style="top: {{dataSource.VisualDesignerData.Top}}px; left: {{dataSource.VisualDesignerData.Left}}px"
				ng-repeat="dataSource in pipelineData.DataSources">
				<div class="name" ng-dblclick="editName(dataSource)">{{dataSource.Name || '(unnamed)'}}</div>
				<div class="description" ng-dblclick="editDescription(dataSource)">{{dataSource.Description || '(no description)'}}</div>
				<div class="typename" ng-attr-title="{{dataSource.PartAssemblyAndType}}">Type: {{dataSource.PartAssemblyAndType | typename: 'className'}}</div>
				<!--ng-dblclick="open()"-->
				<div class="ep"></div>
				<div class="delete" ng-click="remove(pipelineData.DataSources, $index)"></div>
			</div>
		</div>
		<button ng-click="savePipeline()">Save Pipeline</button>
		<button ng-click="toggleEndpointOverlays()">{{showEndpointOverlays == true ? "Hide" : "Show" }} Overlays</button>
		<button ng-click="repaint()">Repaint Connections</button>
	    <select ng-model="addDataSourceType" ng-options="d.ClassName for d in pipelineData.DataSourcesDefinitions | orderBy: 'ClassName'">
	        <option value="">-- DataSource Type --</option>
	    </select>
        <button ng-click="addDataSource()" ng-disabled="!addDataSourceType">Add DataSource</button>
		<pre>{{pipelineData | json}}</pre>
	</div>
</div>
