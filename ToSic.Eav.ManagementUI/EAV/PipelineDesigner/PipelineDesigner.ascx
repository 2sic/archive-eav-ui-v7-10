<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineDesigner.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineDesigner.PipelineDesigner" %>
<div ng-app="pipelineDesinger">
	<div ng-controller="pipelineDesignerController">
		<div id="pipelineContainer">
			<div datasource
				id="dataSource_{{dataSource.EntityGuid}}"
				class="dataSource"
				ng-attr-style="top: {{dataSource.VisualDesignerData.Top}}px; left: {{dataSource.VisualDesignerData.Left}}px"
				ng-repeat="dataSource in pipelineData.DataSources">
				<div class="name" ng-dblclick="editName(dataSource)">{{dataSource.Name || '(unnamed)'}}</div>
				<div class="description" ng-dblclick="editDescription(dataSource)">{{dataSource.Description || '(no description)'}}</div>
				<div class="typename" ng-attr-title="{{dataSource.PartAssemblyAndType}}">Type: {{dataSource.PartAssemblyAndType | typename: 'className'}}</div>
				<!--ng-dblclick="open()"-->
				<div class="ep" ng-if="dataSource.ReadOnly != true"></div>
				<div class="delete" ng-click="remove($index)" ng-if="dataSource.ReadOnly != true"></div>
			</div>
		</div>
		<div class="actions panel panel-default">
			<div class="panel-heading">Actions</div>
			<div class="panel-body">
				<button class="btn btn-primary btn-block" ng-click="savePipeline()">Save</button>
				<select class="form-control" ng-model="addDataSourceType" ng-change="addDataSource()" ng-options="d.ClassName for d in pipelineData.InstalledDataSources | orderBy: 'ClassName'">
					<option value="">-- Add DataSource --</option>
				</select>
				<button class="btn btn-default btn-sm" ng-click="toggleEndpointOverlays()">{{showEndpointOverlays == true ? "Hide" : "Show" }} Overlays</button>
				<button class="btn btn-default btn-sm" ng-click="repaint()">Repaint</button>
			</div>
		</div>
		<toaster-container />
		<%--<pre>{{pipelineData | json}}</pre>--%>
	</div>
</div>
