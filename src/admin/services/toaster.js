
angular.module("EavServices")
    // the config is important to ensure our toaster has a common setup
    .config(function(toastrConfig) {
        angular.extend(toastrConfig, {
            autoDismiss: false,
            containerId: "toast-container",
            maxOpened: 5, // def is 0    
            newestOnTop: true,
            positionClass: "toast-top-right",
            preventDuplicates: false,
            preventOpenDuplicates: false,
            target: "body"
        });
    })

    .factory("toastrWithHttpErrorHandling", function (toastr) {
        toastr.originalError = toastr.error;
        toastr.error = function errorWithHttpErrorDisplay(messageOrHttpError, title, optionsOverride) {
            var message;
            // test whether bodyOrError is an Error from Web API
            if (messageOrHttpError && messageOrHttpError.data && messageOrHttpError.data.Message) {
                message = messageOrHttpError.data.Message;
                if (messageOrHttpError.data.ExceptionMessage)
                    message += "\n" + messageOrHttpError.data.ExceptionMessage;
            } else
                message = messageOrHttpError;

            toastr.originalError(message, title, optionsOverride);
        };
        return toastr;
    })

    
    .factory("saveToastr", function (toastr, $translate) {
        function saveWithToaster(promise) {
            // todo: replace all this with a single-line calling the promise-toaster below...
            // ? return saveWithToaster(promise, "Message.Saving", "Message.Saved", "Message.ErrorWhileSaving", null, 3000, null);
                var saving = toastr.info($translate.instant("Message.Saving"));
                return promise.then(function(result) {
                    toastr.clear(saving);
                    toastr.success($translate.instant("Message.Saved"), { timeOut: 3000 });
                    return result;
                }, function errorWhileSaving(result) {
                    toastr.clear(saving);
                    toastr.error($translate.instant("Message.ErrorWhileSaving"));
                    return result;
                });
            }

            return saveWithToaster;
    })

    .factory("promiseToastr", function (toastrWithHttpErrorHandling, $translate) {
        function saveWithToaster(promise, keyInfo, keyOk, keyError, durInfo, durOk, durError) {
            var toastr = toastrWithHttpErrorHandling;
            var saving = toastr.info($translate.instant(keyInfo));
            return promise.then(function (result) {
                toastr.clear(saving);
                toastr.success($translate.instant(keyOk), { timeOut: durOk || 1000 });
                return result;
            }, function errorWhileSaving(result) {
                toastr.clear(saving);
                toastr.error(result, $translate.instant(keyError));
                return result;
            });
        }

        return saveWithToaster;
    })
;