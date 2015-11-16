/* global angular */
(function () {
	"use strict";

    angular.module("eavEditEntity")
        /// Standard entity commands like get one, many etc.
        .factory("entitiesSvc", function($http, appId) {
            var svc = {};

            svc.getManyForEditing = function(appId, items) {
                return $http.post("eav/entities/getmanyforediting", items, { params: { appId: appId } });
            };

            svc.saveMany = function(appId, items) {
                // first clean up unnecessary nodes - just to make sure we don't miss-read the JSONs transferred
                var removeTempValue = function(value, key) { delete value._currentValue; };
                var itmCopy = angular.copy(items);
                for (var ei = 0; ei < itmCopy.length; ei++)
                    angular.forEach(itmCopy[ei].Entity.Attributes, removeTempValue);

                return $http.post("eav/entities/savemany", itmCopy, { params: { appId: appId } }).then(function (serverKeys) {
                    alert('success function called');
                    var syncUpdatedKeys = function(value, key) {
                        // first ensure we don't break something
                        var ent = value.Entity;
                        if ((ent.Id === null || ent.Id === 0) && (ent.Guid !== null || typeof (ent.Guid) !== "undefined" || ent.Guid !== "00000000-0000-0000-0000-000000000000")) {
                            // try to find it in the return material to re-assign it
                            var newId = serverKeys.data[ent.Guid];
                            value.Entity.Id = newId;
                            value.Header.ID = newId;
                        }
                    };
                    angular.forEach(items, syncUpdatedKeys);

                    return serverKeys;
                });
            };

            svc.delete = function del(type, id) {
                return $http.get("eav/entities/delete", {
                    params: {
                        'contentType': type,
                        'id': id,
                        'appId': appId
                    }
                });
            };

            svc.newEntity = function(header) {
                return {
                    Id: null,
                    Guid: header.Guid, 
                    Type: {
                        StaticName: header.ContentTypeName // contentTypeName
                    },
                    Attributes: {},
                    IsPublished: true
                };
            };


            svc.save = function save(appId, newData) {
                return $http.post("eav/entities/save", newData, { params: { appId: appId } });
            };

            return svc;
        });


})();