/// <reference path="eavConfig.js" />
// eavConfig providers default global values for the EAV angular system
// WARNING !!!
// WARNING !!!
// WARNING !!!
// The ConfigurationProvider in 2SexyContent is not the same as in the EAV project.

// temp helper object
var $eavOnlyHelpers = {};
$eavOnlyHelpers.urlParams = {
    get: function getParameterByName(name) {
        // warning: this method is duplicated in 3 places - keep them in sync. 
        // locations are eav, 2sxc4ng and ui.html
        name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
        var searchRx = new RegExp("[\\?&]" + name + "=([^&#]*)", "i");
        var results = searchRx.exec(location.search);

        if (results === null) {
            var hashRx = new RegExp("[#&]" + name + "=([^&#]*)", "i");
            results = hashRx.exec(location.hash);
        }

        // if nothing found, try normal URL because DNN places parameters in /key/value notation
        if (results === null) {
            // Otherwise try parts of the URL
            var matches = window.location.pathname.match(new RegExp("/" + name + "/([^/]+)", "i"));

            // Check if we found anything, if we do find it, we must reverse the results so we get the "last" one in case there are multiple hits
            if (matches !== null && matches.length > 1)
                results = matches.reverse()[0];
        } else
            results = results[1];

        return results === null ? "" : decodeURIComponent(results.replace(/\+/g, " "));
    },

    require: function getRequiredParameter(name) {
        var found = $eavOnlyHelpers.urlParams.get(name);
        if (found === "") {
            var message = "Required parameter (" + name + ") missing from url - cannot continue";
            alert(message);
            throw message;
        }
        return found;
    }
};

// the following config-stuff is not in angular, because some settings are needed in dialogs which are not built with angularJS yet.
// they are included in the same file for conveniance and to motivate the remaining dialogs to get migrated to AngularJS
var $eavUIConfig = {
    languages: {
        languages: JSON.parse($eavOnlyHelpers.urlParams.require("langs")), //[{ key: "en-us", name: "English (United States)" }, { key: "de-de", name: "Deutsch (Deutschland)" }],
        defaultLanguage: $eavOnlyHelpers.urlParams.require("langpri"), // "en-us",
        // fallbackLanguage: "en", - I think not used any more...
	    currentLanguage: $eavOnlyHelpers.urlParams.require("lang"), // "en-us",
		i18nRoot: "/dist/i18n/"
	}
};

// if (angular) // always check if(angular) because this file is also included in older non-angulare dialogs
angular.module("EavConfiguration", [])
    .constant("languages", $eavUIConfig.languages)
    .factory("eavConfig", function($location) {

        return {
            api: {
                baseUrl: "/api",
                additionalHeaders: {},
                defaultParams: {}
            },
            dialogClass: "eavDialog",
            getUrlPrefix: function(area) {
                if (area === "system")
                    return "";
                if (area === "zone")
                    return "/zone-not-defined-yet-in-eav";
                if (area === "app")
                    return "/app-not-defined-yet-in-eav";
                if (area === "api")
                    return "/api";
                if (area === "dialog")
                    return "/Pages";
                if (area === "dialog-page")
                    return "/Pages/ngwrapper.cshtml";
            },
            languages: $eavUIConfig.languages,

            pipelineDesigner: {
                outDataSource: {
                    className: "SexyContentTemplate",
                    in: ["Content", "Presentation", "ListContent", "ListPresentation"],
                    name: "2SexyContent Module",
                    description: "The module/template which will show this data",
                    visualDesignerData: { Top: 40, Left: 400 }
                },
                defaultPipeline: {
                    dataSources: [
                        {
                            entityGuid: "unsaved1",
                            partAssemblyAndType: "ToSic.Eav.DataSources.App, ToSic.Eav.DataSources",
                            visualDesignerData: { Top: 300, Left: 440, Width: 400 }
                        }
                    ],
                    streamWiring: [
                        { From: "unsaved1", Out: "Default", To: "Out", In: "Content" },
                        { From: "unsaved1", Out: "Default", To: "Out", In: "ListContent" },
                        { From: "unsaved1", Out: "Default", To: "Out", In: "Presentation" },
                        { From: "unsaved1", Out: "Default", To: "Out", In: "ListPresentation" }
                    ]
                },
                testParameters: "[Demo:Demo]=true"
            },
            metadataOfAttribute: 2,
            metadataOfApp: 3,
            metadataOfEntity: 4,
            metadataOfContentType: 5,
            metadataOfZone: 6,
            metadataOfCmsObject: 10,

            versionInfo: "unknown",

            contentType: {
                defaultScope: "2SexyContent" // null 
            },

            // use this to set defaults for field types OR to provide an alternat type if one is deprecated
            // note that it's here for testing, it's really used in 2sxc for mapping the default wysiswyg
            formly: {
                inputTypeReplacementMap: {
                    //"string-wysiwyg": "string-dropdown"
                    //"string-wysiwyg": "string-wysiwyg-dnn"
                
                },

                // used to inject additional / change config if necessary
                inputTypeReconfig: function(field) {
                    var config = field.InputTypeConfig || {}; // note: can be null
                    var applyChanges = false;
                    switch (field.InputType) {
                    case "string-wysiwyg-demo":
                        config.Assets = "hello.js";
                        applyChanges = true;
                        break;
                    case "entity-default":
                        angular.extend(field.Metadata.merged, {
                            EnableEdit: true,
                            EnableCreate: true,
                            EnableAddExisting: true,
                            EnableRemove: true,
                            EnableDelete: false
                        });
                        break;
                    case "unknown": // server default if not defined
                    default: // default if not defined in this list
                        break;

                    }
                    if (applyChanges && !field.InputTypeConfig)
                        field.InputTypeConfig = config;
                }
            }

        }
    });