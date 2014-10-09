<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineDesigner.PipelineDesigner" %>
<div ng-app="pipelineDesinger">
	<div ng-controller="pipelineDesignerController">
		<div id="pipelineContainer">
			<div
				ng-repeat="dataSource in pipelineData.DataSources"
				datasource
				id="dataSource_{{dataSource.EntityGuid}}"
				class="dataSource"
				ng-attr-style="top: {{dataSource.VisualDesignerData.Top}}px; left: {{dataSource.VisualDesignerData.Left}}px"
				ng-dblclick="configureDataSource(dataSource)">
				<div class="name" ng-click="editName(dataSource)">{{dataSource.Name || '(unnamed)'}}</div>
				<div class="description" ng-click="editDescription(dataSource)">{{dataSource.Description || '(no description)'}}</div>
				<div class="typename" ng-attr-title="{{dataSource.PartAssemblyAndType}}">Type: {{dataSource.PartAssemblyAndType | typename: 'className'}}</div>
				<div class="ep" ng-if="!dataSource.ReadOnly"></div>
				<div class="delete" ng-click="remove($index)" ng-if="!dataSource.ReadOnly"></div>
			</div>
		</div>
		<div class="actions panel panel-default">
			<div class="panel-heading">Actions</div>
			<div class="panel-body">
				<button class="btn btn-primary btn-block" ng-disabled="readOnly" ng-click="savePipeline()">Save</button>
				<select class="form-control" ng-model="addDataSourceType" ng-disabled="readOnly" ng-change="addDataSource()" ng-options="d.ClassName for d in pipelineData.InstalledDataSources | orderBy: 'ClassName'">
					<option value="">-- Add DataSource --</option>
				</select>
				<button class="btn btn-default btn-sm" ng-click="toggleEndpointOverlays()">{{showEndpointOverlays ? 'Hide' : 'Show' }} Overlays</button>
				<button class="btn btn-default btn-sm" ng-click="repaint()">Repaint</button>
				<button class="btn btn-default btn-sm" ng-click="toogleDebug()">{{debug ? 'Hide' : 'Show'}} Debug Info</button>
				<button class="btn btn-default btn-sm" ng-click="queryPipeline()">Query this Pipeline</button>
			</div>
		</div>
		<toaster-container></toaster-container>
		<pre ng-if="debug">{{pipelineData | json}}</pre>
	</div>
</div>
