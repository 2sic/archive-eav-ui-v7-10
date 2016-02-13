/* 
 * 
 * Simple service which takes care of ctrl+S keyboard shortcuts. 
 * use it as a service for your controller, then add a line like 
         function activate() {
            // add ctrl+s to save
            ctrlS.bind(function() { vm.save(false); });
        }

 */

angular.module("EavServices")
    .factory("ctrlS", function ($window) {

        // Create a capture Ctrl+S and execute action-object
        function createSave(action) {
            var save = {
                _event: null,
                _action: null,
                _isbound: false,

                // this will be called on each keydown, will check if it was a ctrl+S
                detectCtrlSAndExcecute: function(e) {
                    if (e.keyCode === 83 && (navigator.platform.match("Mac") ? e.metaKey : e.ctrlKey)) {
                        if (save._action === null)
                            return console.log("can't do anything on ctrl+S, no action registered");
                        e.preventDefault();
                        save._action();
                    }
                },

                bind: function bind(action) {
                    save._action = action;
                    save._isbound = true;
                    save._event = $window.addEventListener("keydown", save.detectCtrlSAndExcecute, false);

                },

                unbind: function unbind() {
                    $window.removeEventListener("keydown", save.detectCtrlSAndExcecute);
                    save._isbound = false;
                },

                // re-attach Ctrl+S if it had already been attached previously
                rebind: function rebind() {
                    if (save._action === null)
                        throw "can't rebind, as it was never initially bound";
                    if (!save._isbound)
                        throw "can't rebind, as it's still bound";
                    save.bind(save._action);
                }
            };

            save.bind(action);

            return save;
        }

        return createSave;
    });