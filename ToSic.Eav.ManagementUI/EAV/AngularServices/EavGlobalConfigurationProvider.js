// EavGlobalConfigurationProvider providers default global values for the EAV angular system
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.
angular.module('eavGlobalConfigurationProvider', []).factory('eavGlobalConfigurationProvider', function ($location) {

	var itemFormBaseUrl = "/Pages/EAVManagement.aspx?";
	var newItemUrl = itemFormBaseUrl + "ManagementMode=NewItem&AttributeSetId=[AttributeSetId]&CultureDimension=[CultureDimension]&KeyNumber=[KeyNumber]&KeyGuid=[KeyGuid]&AssignmentObjectTypeId=[AssignmentObjectTypeId]";
	var editItemUrl = itemFormBaseUrl + "ManagementMode=EditItem&EntityId=[EntityId]&CultureDimension=[CultureDimension]";

	return {
		api: {
			baseUrl: "/api",
			additionalHeaders: {},
			defaultParams: {}
		},
		dialogClass: "eavDialog",
		itemForm: {
			newItemUrl: newItemUrl,
			editItemUrl: editItemUrl,
			getUrl: function (mode, params) {
				angular.extend(params, { ManagementMode: mode + 'Item' });
				if (!params.ReturnUrl)
					params.ReturnUrl = $location.url();
				return itemFormBaseUrl + $.param(params);
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
				visualDesignerData: { Top: 50, Left: 410 }
			},
			defaultPipeline: {
				dataSources: [
					{
						partAssemblyAndType: 'ToSic.Eav.DataSources.App, ToSic.Eav',
						visualDesignerData: { Top: 400, Left: 450 }
					}
				],
				streamWiring: [
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'Content' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'ListContent' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'Presentation' },
					{ From: 'unsaved1', Out: 'Default', To: 'Out', In: 'ListPresentation' }
				]
			}
		}
	}
});