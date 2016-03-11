/* 
 * Field: String - url-item
 */

angular.module("eavFieldTemplates")
    .config(function(formlyConfigProvider, defaultFieldWrappers) {

        formlyConfigProvider.setType({
            name: "string-url-path",
            template: "<div><input class=\"form-control input-lg\" only-simple-url-chars ng-pattern=\"vm.regexPattern\" ng-model=\"value.Value\" ng-blur=\"vm.finalClean()\"></div>",
            wrapper: defaultFieldWrappers,
            controller: "FieldTemplate-String-Url-Item-Ctrl as vm"
        });

    }).controller("FieldTemplate-String-Url-Item-Ctrl", function($scope, debugState, stripNonUrlCharacters) {
        var vm = this;

        // get configured
        var controlSettings = $scope.to.settings["string-url-path"];
        var sourceMask = (controlSettings) ? controlSettings.AutoGenerateMask || null : null;

        // test values
        //sourceMask = "[Name]";
        //autoGenerateLink = true;

        //#region Field-Mask handling 
        vm.lastAutoCopy = "";
        vm.sourcesChangedTryToUpdate = function sourcesChangedTryToUpdate() {
            // don't do anything if the current field is not empty and doesn't have the last copy of the stripped value
            if($scope.value && $scope.value.Value && $scope.value.Value !== vm.lastAutoCopy)
                return;
            
            var orig = vm.getFieldContentBasedOnMask(sourceMask);
            var cleaned = stripNonUrlCharacters(orig, false, true);
            if (cleaned) {
                vm.lastAutoCopy = cleaned;
                $scope.value.Value = cleaned;
            }
        }

        vm.maskRegEx = /\[.*?\]/ig;
        vm.getFieldContentBasedOnMask = function getNewAutoValue(mask) {
            angular.forEach(vm.getFieldsOfMask(mask), function (e, i) {
                var replaceValue = ($scope.model.hasOwnProperty(e) && $scope.model[e] && $scope.model[e]._currentValue && $scope.model[e]._currentValue.Value)
                    ? $scope.model[e]._currentValue.Value : "";
                mask = mask.replace(e, replaceValue);
            });

            return mask;
        };

        vm.getFieldsOfMask = function (mask) {
            var result = [];
            if (!mask) return "";
            var matches = mask.match(vm.maskRegEx);
            angular.forEach(matches, function (e, i) {
                var staticName = e.replace(/[\[\]]/ig, "");
                result.push(staticName);
            });
            return result;
        };
        //#endregion 


        //#region enforce custom regex - copied from string-default
        var validationRegexString = ".*";
        var stringSettings = $scope.options.templateOptions.settings.merged;
        if (stringSettings && stringSettings.ValidationRegExJavaScript)
            validationRegexString = stringSettings.ValidationRegExJavaScript;
        vm.regexPattern = new RegExp(validationRegexString, "i");

        //#endregion

        //#region do final cleaning; mainly remove trailing "/"
        vm.finalClean = function() {
            var orig = $scope.value.Value;
            var cleaned = stripNonUrlCharacters(orig, false, true);
            if (orig !== cleaned)
                $scope.value.Value = cleaned;
        };
        //#endregion

        vm.activate = function() {
            // add a watch for each field in the field-mask
            angular.forEach(vm.getFieldsOfMask(sourceMask), function(e, i) {
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
    .factory("stripNonUrlCharacters", function() {
        return function(inputValue, allowPath, trimEnd) {
            if (inputValue == null)
                return "";
            var rexAllowed = /[^a-z0-9-_]/gi,
                rexSeparators = /[^a-z0-9-_]+/gi,
                rexMult = /-+/gi,
                rexTrim = trimEnd ? /^-|-+$/gi : /^-/gi;
            // allow only lower-case, numbers and _/- characters
            var cleanInputValue = inputValue.replace("'s ", "s ") // neutralize it's, daniel's etc.
                .toLowerCase().replace(rexSeparators, "-");
            cleanInputValue = cleanInputValue.replace(rexMult, "-");
            cleanInputValue = cleanInputValue.replace(rexAllowed, ""); ///[^\w\s]/gi, '');
            cleanInputValue = cleanInputValue.replace(rexTrim, "");
            return cleanInputValue;
        }
    })

    // this monitors an input-field and ensures that only allowed characters are typed
    .directive("onlySimpleUrlChars", function(stripNonUrlCharacters) {
        return {
            require: "ngModel",
            restrict: "A",
            link: function(scope, element, attrs, modelCtrl) {
                modelCtrl.$parsers.push(function(inputValue) {
                    if (inputValue == null)
                        return "";
                    var cleanInputValue = stripNonUrlCharacters(inputValue, false, false);

                    if (cleanInputValue !== inputValue) {
                        modelCtrl.$setViewValue(cleanInputValue);
                        modelCtrl.$render();
                    }
                    return cleanInputValue;
                });
            }
        }
    });
