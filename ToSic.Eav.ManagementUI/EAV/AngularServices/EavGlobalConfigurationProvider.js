// EavGlobalConfigurationProvider providers default global values for the EAV angular system
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.
angular.module('eavGlobalConfigurationProvider', []).factory('eavGlobalConfigurationProvider', function ($location) {

	var getItemFormUrl = function (mode, params, preventRedirect) {
		params.ManagementMode = mode + 'Item';
		if (!params.ReturnUrl)
			params.ReturnUrl = $location.url();
		if (preventRedirect)
			params.PreventRedirect = true;
		return "/Pages/EAVManagement.aspx?" + $.param(params);
	};

	return {
		api: {
			baseUrl: "/api",
			additionalHeaders: {},
			defaultParams: {}
		},
		dialogClass: "eavDialog",
		itemForm: {
			getNewItemUrl: function (attributeSetId, assignmentObjectTypeId, params, preventRedirect, prefill) {
				if (prefill)
					params.prefill = JSON.stringify(prefill);
				return getItemFormUrl('New', angular.extend({ AttributeSetId: attributeSetId, AssignmentObjectTypeId: assignmentObjectTypeId }, params), preventRedirect);
			},
			getEditItemUrl: function (entityId, params, preventRedirect) {
				return getItemFormUrl('Edit', angular.extend({ EntityId: entityId }, params), preventRedirect);
			}
		},
		pipelineDesigner: {
			getUrl: function (appId, pipelineId) {
				return '/Pages/PipelineDesigner.aspx?AppId=' + appId + '&PipelineId=' + pipelineId;
			},
			outDataSource: {
				className: 'SexyContentTemplate',
				in: ['Content', 'Presentation', 'ListContent', 'ListPresentation'],
				name: '2SexyContent Module',
				description: 'The module/template which will show this data',
				visualDesignerData: { Top: 40, Left: 400 }
			},
			defaultPipeline: {
				dataSources: [
					{
						entityGuid: 'unsaved1',
						partAssemblyAndType: 'ToSic.Eav.DataSources.App, ToSic.Eav',
						visualDesignerData: { Top: 300, Left: 440 }
					}
				],
				streamWiring: [
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'Content' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'ListContent' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'Presentation' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'ListPresentation' }
				]
			},
			testParameters: null
		},
		assignmentObjectTypeIdDataPipeline: 4
	}
});