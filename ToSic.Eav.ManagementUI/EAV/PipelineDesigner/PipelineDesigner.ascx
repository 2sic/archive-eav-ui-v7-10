<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineDesigner.PipelineDesigner" %>
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
				<div class="delete" ng-click="remove($index)" ng-if="dataSource.AllowDelete!=false"></div>
			</div>
		</div>
		<button ng-click="savePipeline()">Save Pipeline</button>
		<button ng-click="toggleEndpointOverlays()">{{showEndpointOverlays == true ? "Hide" : "Show" }} Overlays</button>
		<button ng-click="repaint()">Repaint</button>
		<select ng-model="addDataSourceType" ng-options="d.ClassName for d in pipelineData.InstalledDataSources | orderBy: 'ClassName'">
			<option value="">-- DataSource Type --</option>
		</select>
		<button ng-click="addDataSource()" ng-disabled="!addDataSourceType">Add DataSource</button>
		<pre>{{pipelineData | json}}</pre>
	</div>
</div>
