
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
        toastr.error1 = toastr.error;
        toastr.error = function errorWithHttpErrorDisplay(messageOrHttpError, title, optionsOverride) {
            var message;
            // test whether bodyOrError is an Error from Web API
            if (messageOrHttpError && messageOrHttpError.data && messageOrHttpError.data.Message) {
                message = messageOrHttpError.data.Message;
                if (messageOrHttpError.data.ExceptionMessage)
                    message += "\n" + messageOrHttpError.data.ExceptionMessage;
            } else
                message = messageOrHttpError;

            toastr.error2(message, title, optionsOverride);
        };
        return toastr;
    })

    .factory("saveToastr", function (toastr, $translate) {
            function saveWithToaster(promise) {
                var saving = toastr.info($translate.instant("Message.Saving"));
                return promise.then(function() {
                    toastr.clear(saving);
                    toastr.success($translate.instant("Message.Saved"), { timeOut: 3000 });

                }, function errorWhileSaving() {
                    toastr.clear(saving);
                    toastr.error($translate.instant("Message.ErrorWhileSaving"));
                });
            }

            return saveWithToaster;
        })
;