﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PipelineManagement.ascx.cs" Inherits="ToSic.Eav.ManagementUI.EAV.PipelineManagement" %>
<div ng-app="pipelineManagement" class="ng-cloak">
	<div ng-controller="pipelineManagementController">
		<a ng-href="{{getPipelineUrl('new')}}" target="_self" class="btn btn-default">New</a>
		<button type="button" class="btn btn-default" ng-click="refresh()">
			<span class="glyphicon glyphicon-refresh"></span>refresh
		</button>
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
						<a class="btn btn-xs btn-default" ng-if="pipeline.AllowEdit" target="_self" ng-href="{{getPipelineUrl('edit', pipeline)}}">Edit</a>
						<a class="btn btn-xs btn-default" target="_blank" ng-href="{{getPipelineUrl('design', pipeline)}}">Open Designer</a>
					</td>
				</tr>
			</tbody>
		</table>
	</div>
</div>
