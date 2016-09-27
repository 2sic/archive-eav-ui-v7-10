/* global angular */
(function () {
	"use strict";

    angular.module("eavEditEntity")
        /// Standard entity commands like get one, many etc.
        .factory("entitiesSvc", function ($http, appId, toastrWithHttpErrorHandling, promiseToastr, $q, $translate, toastr) {
            var svc = {
                toastr: toastrWithHttpErrorHandling
            };

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

            svc.tryDeleteAndAskForce = function tryDeleteAndAskForce(type, id, itemTitle) {

                var deferred = $q.defer();

                // todo: i18n
                var msg = $translate.instant("General.Questions.DeleteEntity", { title: itemTitle, id: id });
                if (!confirm(msg))
                    deferred.reject("Delete aborted by user");
                else {
                    svc.delete(type, id, false).then(function (result) {

                        if (result.status >= 200 && result.status < 300) {
                            deferred.resolve(result);
                        }
                        else {
                            // if delete failed, ask to force-delete in a toaster
                            var msg = "<div>" + $translate.instant("General.Questions.ForceDelete", { title: itemTitle, id: id }) + "<br/>"
                                + "<button type='button' id='del' class='btn btn-default' ><i class= 'icon-eav-ok'></i>" + $translate.instant("General.Buttons.ForceDelete") + "</button>"
                                + "</div>";

                            toastr.warning(msg, {
                                allowHtml: true,
                                timeOut: 5000,
                                onShown: function (toast) {
                                    // this checks for the click on the button in the toaster
                                    toast.el[0].onclick = function (event) {
                                        var target = event.target || event.srcElement;
                                        if (target.id === "del")
                                            svc.delete(type, id, true)
                                                .then(deferred.resolve);
                                    };
                                }
                            });
                        }
                    });
                }

                return deferred.promise;

            };

            svc.delete = function del(type, id, tryForce) {
                console.log("try to delete");

                var delPromise = $http.get("eav/entities/delete", {
                    ignoreErrors: true,
                    params: {
                        'contentType': type,
                        'id': id,
                        'appId': appId,
                        'force': tryForce
                    }
                });

                return promiseToastr(delPromise, "Message.Deleting", "Message.Ok", "Message.Error");
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