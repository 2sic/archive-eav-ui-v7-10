/// <reference path="eavConfig.js" />
// eavConfig providers default global values for the EAV angular system
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.

// the following config-stuff is not in angular, because some settings are needed in dialogs which are not built with angularJS yet.
// they are included in the same file for conveniance and to motivate the remaining dialogs to get migrated to AngularJS
var $eavUIConfig = {
	urls: {
		exportContent: function(appId) {
			return "/todo?appId=" + appId;
		},
		importContent: function(appId) {
			return "/todo?appId=" + appId;
		},
		ngRoot: function() {
			return "/dist/";
		},
		pipelineDesigner: function (appId, pipelineId) {
		    return "/Pages/ngwrapper.cshtml?ng=pipeline-designer&AppId=" + appId + "&pipelineId=" + pipelineId;
		    //return '/Pages/ngwrapper.aspx?AppId=' + appId + '&PipelineId=' + pipelineId;
		}
	},
	languages: {
		languages: [{ key: 'en-us', name: 'English (United States)' }, { key: 'de-de', name: 'Deutsch (Deutschland)' }],
		defaultLanguage: 'en-us',
		currentLanguage: 'en-us',
		i18nRoot: "/dist/i18n/"
	}
	//{
    //    preferredLanguage: function () { return "en"; },
    //    fallbackLanguage: function () { return "en"; },
    //    i18nRoot: function () { return "/i18n/"; }
    //}
};

if (angular) // always check if(angular) because this file is also included in older non-angulare dialogs
    angular.module("EavConfiguration", [])
        .constant("languages", $eavUIConfig.languages)
        .factory('eavConfig', function($location) {

            var getItemFormUrl = function(mode, params, preventRedirect) {
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
                    getNewItemUrl: function(attributeSetId, assignmentObjectTypeId, params, preventRedirect, prefill) {
                        if (prefill)
                            params.prefill = JSON.stringify(prefill);
                        return getItemFormUrl('New', angular.extend({ AttributeSetId: attributeSetId, AssignmentObjectTypeId: assignmentObjectTypeId }, params), preventRedirect);
                    },
                    getEditItemUrl: function(entityId, params, preventRedirect) {
                        return getItemFormUrl('Edit', angular.extend({ EntityId: entityId }, params), preventRedirect);
                    }
                },
                adminUrls: $eavUIConfig.urls,
                languages: $eavUIConfig.languages,

                pipelineDesigner: {
                    //getUrl: function(appId, pipelineId) {
                    //    return '/Pages/PipelineDesigner.aspx?AppId=' + appId + '&PipelineId=' + pipelineId;
                    //},
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
                metadataOfEntity: 4,
                metadataOfAttribute: 2,
                assignmentObjectTypeIdDataPipeline: 4, // deprecated, don't use any more, will be removed in january 2016

                // new
                contentType: {
                    defaultScope: null //todo: must change this for 2sxc
                }
            }
        });