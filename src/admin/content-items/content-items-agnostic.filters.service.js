(function () {
    'use strict';

    angular.module("ContentItemsAppAgnostic")
        .factory("agGridFilters", function() {
            return {
                get: function() {
                    if (!window.$2sxc) return {};
                    var urlFilters = window.$2sxc.urlParams.get("filters"), filters = null;
                    if (!urlFilters) return {};
                    if (urlFilters.charAt(urlFilters.length - 1) === "=") 
                        urlFilters = atob(urlFilters);
                    
                    try {
                        filters = JSON.parse(urlFilters);
                        console.log("found filters for this list:", filters);
                    } catch (e) {
                        console.log("can't parse json with filters from url: ", urlFilters);
                    }
                    if (!filters)
                        return {};

                    // check if there is a IsPublished filter, handle the special cases
                    if (filters.IsPublished === true)
                        filters.IsPublished = ["is published"];
                    else if (filters.IsPublished === false)
                        console.warn("filter ispublished = false is not implemented yet");

                    if (typeof filters.IsMetadata !== "undefined") {
                        // ensure that IsPublished is an array, in case we add Metadata-filters
                        if (!Array.isArray(filters.IsPublished))
                            filters.IsPublished = [];
                        filters.IsPublished.push(filters.IsMetadata ? "is metadata" : "is not metadata");
                        delete filters.IsMetadata;
                    }

                    // catch simple number filters, convert into ag-grid format
                    for (var field in filters)
                        if (filters.hasOwnProperty(field) && typeof filters[field] === "number")
                            filters[field] = { filter: filters[field], type: 1 };
                        
                    

                    console.log("will try to apply filter: ", filters);
                    return filters;
                    //{
                    //    // IsPublished: ["is published"],
                    //    ImageFormat: "w"
                    //}
                }
            };
        });
}());