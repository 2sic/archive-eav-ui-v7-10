
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

    // this is an old service - used only in the pipeline designer. Don't reuse! just call the toastr directly
    //.factory("uiNotification", function (toastr) {
    //        "use strict";

    //        var toaster = toastr;

    //        return {
    //            clear: function () {
    //                toaster.clear();
    //            },
    //            error: function (title, bodyOrError) {
    //                var message;
    //                // test whether bodyOrError is an Error from Web API
    //                if (bodyOrError && bodyOrError.data && bodyOrError.data.Message) {
    //                    message = bodyOrError.data.Message;
    //                    if (bodyOrError.data.ExceptionMessage)
    //                        message += "\n" + bodyOrError.data.ExceptionMessage;
    //                } else
    //                    message = bodyOrError;

    //                toastr.error(title, body, { autoDismiss: false });
    //            }

    //        };
    //    }
    //)
;