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

        // todo: 
        // 3. auto-pickup the address if this is still blank and a / the source-field changed

        // get configured
        var autoGenerateLink = false, sourceMask;
        var controlSettings = $scope.to.settings["string-url-path"];
        if (controlSettings) {
            sourceMask = controlSettings.AutoGenerateMask || null;
        };
        sourceMask = "[Name]";
        // we have specs...
        if (sourceMask && $scope.model.hasOwnProperty(sourceMask) && $scope.model[sourceMask]) // && $scope.model[sourceField]._currentValue) {
            autoGenerateLink = true;

        autoGenerateLink = true;
        autoGenerateLink = autoGenerateLink && !($scope.value && $scope.value.Value); // only auto-generate if the initial value was blank...

        // todo: setup watch
        if (autoGenerateLink) {
            // todo
            $scope.$watch('model.Name._currentValue.Value', function() {
                console.log('changed...');
                sourcesChangedTryToUpdate();
            });
            // 1. check the fields we rely on
            // 2. watch them
            // 3. if anything changes...call sourcesChangedTryToUpdate
            // 4. check if the current value.Value is empty - if not, return;
            // 5. otherwise use the new value, clean it, apply. 
        }

        vm.lastAutoCopy = "";
        vm.fieldMask = sourceMask;
        function sourcesChangedTryToUpdate() {
            // don't do anything if the current field is not empty and doesn't have the last copy of the stripped value
            if($scope.value && $scope.value.Value && $scope.value.Value !== vm.lastAutoCopy)
                return;
            
            var orig = vm.getFieldContentBasedOnMask();
            var cleaned = stripNonUrlCharacters(orig, false, true);
            if (cleaned) {
                vm.lastAutoCopy = cleaned;
                $scope.value.Value = cleaned;
            }
        }

        vm.maskRegEx = /\[.*?\]/ig;
        vm.getFieldContentBasedOnMask = function getNewAutoValue() {
            var mask = vm.fieldMask; // controlSettings.AddressMask;
            //if (!mask) return "";
            ////var tokenRe = /\[.*?\]/ig;
            //var matches = mask.match(vm.maskRegEx);
            //angular.forEach(matches, function(e, i) {
            //    var staticName = e.replace(/[\[\]]/ig, "");
            //    var replaceValue = ($scope.model.hasOwnProperty(staticName) && $scope.model[staticName] !== null) ? $scope.model[staticName]._currentValue.Value : "";
            //    mask = mask.replace(e, replaceValue);
            //});

            angular.forEach(vm.getFieldsOfMask(), function (e, i) {
                //var e = e.replace(/[\[\]]/ig, "");
                var replaceValue = ($scope.model.hasOwnProperty(e) && $scope.model[e] !== null) ? $scope.model[e]._currentValue.Value : "";
                mask = mask.replace(e, replaceValue);
            });

            return mask;
        };

        vm.getFieldsOfMask = function () {
            var result = [];
            var mask = vm.fieldMask; 
            if (!mask) return "";
            var matches = mask.match(vm.maskRegEx);
            angular.forEach(matches, function (e, i) {
                var staticName = e.replace(/[\[\]]/ig, "");
                result.push(staticName);
                //var replaceValue = ($scope.model.hasOwnProperty(staticName) && $scope.model[staticName] !== null) ? $scope.model[staticName]._currentValue.Value : "";
                //mask = mask.replace(e, replaceValue);
            });
            return result;
        };


        //#region enforce custom regex - copied from string-default
        var validationRegexString = ".*";
        var stringSettings = $scope.options.templateOptions.settings.merged;
        if (stringSettings && stringSettings.ValidationRegExJavaScript)
            validationRegexString = stringSettings.ValidationRegExJavaScript;
        vm.regexPattern = new RegExp(validationRegexString, 'i');

        //#endregion

        //#region do final cleaning; mainly remove trailing "/"
        vm.finalClean = function() {
            var orig = $scope.value.Value;
            var cleaned = stripNonUrlCharacters(orig, false, true);
            if (orig !== cleaned)
                $scope.value.Value = cleaned;
        };
        //#endregion

        $scope.debug = debugState;
        console.log($scope.options.templateOptions);
    })

    // this is a helper which cleans up the url and is used in various places
    .factory("stripNonUrlCharacters", function() {
        return function(inputValue, allowPath, trimEnd) {
            if (inputValue == null)
                return '';
            var rexAllowed = /[^a-z0-9-_]/gi,
                rexSeparators = /[^a-z0-9-_]+/gi,
                rexMult = /-+/gi,
                rexTrim = trimEnd ? /^-|-+$/gi : /^-/gi;
            // allow only lower-case, numbers and _/- characters
            var cleanInputValue = inputValue.replace(rexSeparators, "-");
            cleanInputValue = cleanInputValue.replace(rexMult, "-");
            cleanInputValue = cleanInputValue.replace(rexAllowed, ""); ///[^\w\s]/gi, '');
            cleanInputValue = cleanInputValue.replace(rexTrim, "");
            return cleanInputValue;
        }
    })

    // this monitors an input-field and ensures that only allowed characters are typed
    .directive('onlySimpleUrlChars', function(stripNonUrlCharacters) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function(scope, element, attrs, modelCtrl) {
                modelCtrl.$parsers.push(function(inputValue) {
                    if (inputValue == null)
                        return '';
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
