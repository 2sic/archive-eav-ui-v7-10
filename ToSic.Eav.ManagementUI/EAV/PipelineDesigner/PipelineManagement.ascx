<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineManagement.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineManagement" %>
<div ng-app="pipelineManagement" class="ng-cloak">
	<div ng-controller="pipelineManagementController">
		<button type="button" class="btn btn-default">New</button>
		<table class="table table-striped table-hover">
			<thead>
				<tr>
					<th>Name</th>
					<th>Description</th>
					<th>Actions</th>
				</tr>
			</thead>
			<tbody>
				<tr ng-repeat="pipeline in pipelines">
					<td>{{pipeline.Name}}</td>
					<td>{{pipeline.Description}}</td>
					<td>
						<button type="button" class="btn btn-xs btn-default" ng-if="pipeline.AllowEdit">Edit</button>
						<button type="button" class="btn btn-xs btn-default" ng-if="pipeline.AllowEdit">Open Designer</button>
					</td>
				</tr>
			</tbody>
		</table>
	</div>
</div>
