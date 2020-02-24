/*
 * This is a special service which uses a field mask
 *
 * like "[Title] - [Subtitle]"
 *
 * and will then provide a list of fields which were used, as well as a resolved value if needed
 *
 */

angular
  .module('eavFieldTemplates')
  .factory('fieldMask', function(debugState, appId, zoneId) {
    // mask: a string like "[FirstName] [LastName]"
    // model: usually the $scope.model, passed into here
    // overloadPreCleanValues: a function which will "scrub" the found field-values
    //
    // use: first create an object like mask = fieldMask.createFieldMask("[FirstName]", $scope.model);
    //      then: access result in your timer or whatever with mask.resolve();
    function createFieldMask(
      mask,
      $scope,
      changeEvent,
      overloadPreCleanValues
    ) {
      function resolveGlobalMask(mask) {
        if (mask == null || mask == undefined) return mask;
        return mask
          .replace(/\[App:AppId\]/i, appId)
          .replace(/\[App:ZoneId\]/i, zoneId);
      }

      var srv = {
        mask: resolveGlobalMask(mask),
        model: $scope.model,
        fields: [],
        value: undefined,
        findFields: /\[.*?\]/gi,
        unwrapField: /[\[\]]/gi
      };

      // resolves a mask to the final value
      srv.resolve = function getNewAutoValue() {
        var value = srv.mask;
        angular.forEach(srv.fields, function(e, i) {
          var replaceValue =
            srv.model.hasOwnProperty(e) &&
            srv.model[e] &&
            srv.model[e]._currentValue &&
            srv.model[e]._currentValue.Value
              ? srv.model[e]._currentValue.Value
              : '';
          var cleaned = srv.preClean(e, replaceValue);
          value = value.replace('[' + e + ']', cleaned);
        });

        return value;
      };

      // retrieves a list of all fields used in the mask
      srv.fieldList = function() {
        var result = [];
        if (!srv.mask) return result;
        var matches = srv.mask.match(srv.findFields);
        angular.forEach(matches, function(e, i) {
          var staticName = e.replace(srv.unwrapField, '');
          result.push(staticName);
        });
        return result;
      };

      srv.preClean = function(key, value) {
        return value;
      };

      // change-event - will only fire if it really changes
      srv.onChange = function() {
        var maybeNew = srv.resolve();
        if (srv.value !== maybeNew) changeEvent(maybeNew);
        srv.value = maybeNew;
      };

      // add watcher and execute onChange
      srv.watchAllFields = function() {
        // add a watch for each field in the field-mask
        angular.forEach(srv.fields, function(e, i) {
          // only watch fields which are real fields of this
          if (e && e.indexOf(':') === -1) {
            $scope.$watch('model.' + e + '._currentValue.Value', function() {
              if (debugState.on) console.log('url-path: ' + e + ' changed...');
              srv.onChange();
            });
          }
        });
      };

      function activate() {
        srv.fields = srv.fieldList();

        if (overloadPreCleanValues)
          // got an overload...
          srv.preClean = overloadPreCleanValues;

        // bind auto-watch only if needed...
        if ($scope && changeEvent) srv.watchAllFields();
      }

      activate();

      return srv;
    }

    return createFieldMask;
  });
