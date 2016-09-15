/* 
 * Field: String - url-path
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-url-path",
            template: "<div><input class=\"form-control material\" only-simple-url-chars ng-pattern=\"vm.regexPattern\" ng-model=\"value.Value\" ng-blur=\"vm.finalClean()\"></div>",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-String-Url-Path-Ctrl as vm"
        });

    })
    .controller("FieldTemplate-String-Url-Path-Ctrl", function($scope, debugState, stripNonUrlCharacters, fieldMask) {
        var vm = this;

        // get configured
        var controlSettings = $scope.to.settings["string-url-path"];
        var sourceMask = (controlSettings) ? controlSettings.AutoGenerateMask || null : null;

        // todo: change to include the change-detection
        var mask = fieldMask(sourceMask, $scope, null, function preCleane(key, value) {
            return value.replace("/", "-").replace("\\", "-"); // this will remove slashes which could look like path-parts
        });

        // test values
        //sourceMask = "[Name]";
        //autoGenerateLink = true;
        var enableSlashes = true;
        $scope.enablePath = enableSlashes;

        //#region Field-Mask handling 
        vm.lastAutoCopy = "";
        vm.sourcesChangedTryToUpdate = function sourcesChangedTryToUpdate() {
            // don't do anything if the current field is not empty and doesn't have the last copy of the stripped value
            if ($scope.value && $scope.value.Value && $scope.value.Value !== vm.lastAutoCopy)
                return;

            var orig = mask.resolve(); // vm.getFieldContentBasedOnMask(sourceMask);
            //orig = orig.replace("/", "-").replace("\\", "-"); // when auto-importing, remove slashes as they shouldn't feel like paths afterwards
            var cleaned = stripNonUrlCharacters(orig, enableSlashes, true);
            if (cleaned && $scope.value) {
                vm.lastAutoCopy = cleaned;
                $scope.value.Value = cleaned;
            }
        };

        //#region enforce custom regex - copied from string-default
        var validationRegexString = ".*";
        var stringSettings = $scope.options.templateOptions.settings.merged;
        if (stringSettings && stringSettings.ValidationRegExJavaScript)
            validationRegexString = stringSettings.ValidationRegExJavaScript;
        vm.regexPattern = new RegExp(validationRegexString, "i");

        //#endregion

        //#region do final cleaning on blur / leave-field; mainly remove trailing "/"
        vm.finalClean = function() {
            var orig = $scope.value.Value;
            var cleaned = stripNonUrlCharacters(orig, enableSlashes, true);
            if (orig !== cleaned)
                $scope.value.Value = cleaned;
        };
        //#endregion


        vm.activate = function () {
            // TODO: use new functionality on the fieldMask instead!
            // add a watch for each field in the field-mask
            angular.forEach(mask.fieldList() /* vm.getFieldsOfMask(sourceMask)*/, function(e, i) {
                $scope.$watch("model." + e + "._currentValue.Value", function() {
                    if (debugState.on) console.log("url-path: " + e + " changed...");
                    vm.sourcesChangedTryToUpdate(sourceMask);
                });
            });

            $scope.debug = debugState;
            if (debugState.on) console.log($scope.options.templateOptions);
        };
        vm.activate();

    })


// this is a helper which cleans up the url and is used in various places
    .factory("stripNonUrlCharacters", function(latinizeText) {
        return function(inputValue, allowPath, trimEnd) {
            if (!inputValue) return "";
            var rexSeparators = allowPath ? /[^a-z0-9-_/]+/gi : /[^a-z0-9-_]+/gi;

            // allow only lower-case, numbers and _/- characters
            var latinized = latinizeText(inputValue.toLowerCase());
            var cleanInputValue = latinized
                .replace("'s ", "s ") // neutralize it's, daniel's etc. but only if followed by a space, to ensure we don't kill quotes
                .replace("\\", "/") // neutralize slash representation
                .replace(rexSeparators, "-") // replace everything we don't want with a -
                .replace(/-+/gi, "-") // reduce multiple "-"
                .replace(/\/+/gi, "/") // reduce multiple slashes
                .replace(/-*\/-*/gi, "/") // reduce "-/" or "/-" combinations to a simple "/"
                .replace(trimEnd ? /^-|-+$/gi : /^-/gi, ""); // trim front and maybe end "-"
            return cleanInputValue;
        };
    })


    // this monitors an input-field and ensures that only allowed characters are typed
    .directive("onlySimpleUrlChars", function(stripNonUrlCharacters) {
        return {
            require: "ngModel",
            restrict: "A",
            link: function(scope, element, attrs, modelCtrl) {
                modelCtrl.$parsers.push(function(inputValue) {
                    if (inputValue === null)
                        return "";
                    var cleanInputValue = stripNonUrlCharacters(inputValue, scope.enablePath, false);

                    if (cleanInputValue !== inputValue) {
                        modelCtrl.$setViewValue(cleanInputValue);
                        modelCtrl.$render();
                    }
                    return cleanInputValue;
                });
            }
        };
    });
