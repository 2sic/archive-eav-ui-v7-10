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
                return $http.delete("eav/entities/delete", {
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
                    Guid: header.Guid, // generateUuid(),
                    Type: {
                        StaticName: header.ContentTypeName // contentTypeName
                    },
                    Attributes: {},
                    IsPublished: true
                };
            };

            //svc.ensureGuid = function ensureGuid(item) {
            //    var ent = item.Entity;
            //    if ((ent.Id === null || ent.Id === 0) && (ent.Guid === null || typeof (ent.Guid) === "undefined" || ent.Guid === "00000000-0000-0000-0000-000000000000")) {
            //        item.Entity.Guid = generateUuid();
            //        item.Header.Guid = item.Entity.Guid;
            //    }
            //};

            svc.save = function save(appId, newData) {
                return $http.post("eav/entities/save", newData, { params: { appId: appId } });
            };

            return svc;
        });


    // Generate Guid - code from http://stackoverflow.com/a/8809472
    //function generateUuid() {
    //    var d = new Date().getTime();
    //    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
    //        var r = (d + Math.random() * 16) % 16 | 0;
    //        d = Math.floor(d / 16);
    //        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    //    });
    //    return uuid;
    //}
})();