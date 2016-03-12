/*
 * This is a special service which uses a field mask 
 * 
 * like "[Title] - [Subtitle]" 
 * 
 * and will then provide a list of fields which were used, as well as a resolved value if needed
 * 
 */

angular.module("eavFieldTemplates")
    .factory("fieldMask", function() {
        function createFieldMask(mask, model, overloadPreCleanValues) {
            var srv = {
                mask: mask,
                model: model,
                findFields: /\[.*?\]/ig,
                unwrapField: /[\[\]]/ig
            };

            srv.resolve = function getNewAutoValue() {
                var mask = srv.mask;
                angular.forEach(srv.fieldList(), function(e, i) {
                    var replaceValue = (srv.model.hasOwnProperty(e) && srv.model[e] && srv.model[e]._currentValue && srv.model[e]._currentValue.Value)
                        ? srv.model[e]._currentValue.Value : "";
                    var cleaned = srv.preClean(e, replaceValue);
                    mask = mask.replace(e, cleaned);
                });

                return mask;
            };

            srv.fieldList = function() {
                var result = [];
                if (!srv.mask) return result;
                var matches = srv.mask.match(srv.findFields);
                angular.forEach(matches, function(e, i) {
                    var staticName = e.replace(srv.unwrapField, "");
                    result.push(staticName);
                });
                return result;
            };

            srv.preClean = function(key, value) {
                return value;
            };

            if (overloadPreCleanValues) // got an overload...
                srv.preClean = overloadPreCleanValues;

            return srv;
        }

        return createFieldMask;
    });